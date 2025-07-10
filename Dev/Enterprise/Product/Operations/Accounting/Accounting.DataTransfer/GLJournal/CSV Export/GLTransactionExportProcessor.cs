using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.DataTransfer.GLJournals
{
	public class GLTransactionExportProcessor : IProcessor
	{
		#region Constructor

		protected GLTransactionExportProcessor()
		{
		}

		public static GLTransactionExportProcessor New()
		{
			GLTransactionExportProcessor result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden();
			}
			else
			{
				result = new GLTransactionExportProcessor();
			}

			return result;
		}

		protected delegate GLTransactionExportProcessor NewDelegate();
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

#if DEBUG
		internal
#endif
		protected virtual GLTransactionExporter GetNewGLTransactionExporter(GLTransactionBusinessObject glBizObj, NotificationBuffer notification)
		{
			return new GLTransactionExporter(glBizObj, notification, true);
		}

		protected virtual GLTransactionBusinessObject GetNewGLTransactionBusinessObject()
		{
			return new GLTransactionBusinessObject(new BusinessObjectFactory());
		}

		protected virtual IAggregateRunner GetIAggregateRunner()
		{
			return Activator.CreateInstance(ObjectFactory.GetType<IAggregateRunner>()) as IAggregateRunner;
		}

		#region Implementation

		enum ProcessCompanyResult
		{
			Success,
			Failed,
			Locked,
			DatabaseDeadlock,
		}

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			var companiesToProcess = GetActiveCompanies();
			var companiesToTryAgain = new List<GlbCompany>();

			foreach (GlbCompany company in companiesToProcess)
			{
				token.ThrowIfCancellationRequested();
				var result = ProcessCompany(notifications, company);

				if (result == ProcessCompanyResult.Locked || result == ProcessCompanyResult.DatabaseDeadlock)
				{
					companiesToTryAgain.Add(company);
				}
			}

			foreach (GlbCompany company in companiesToTryAgain)
			{
				token.ThrowIfCancellationRequested();
				ProcessCompany(notifications, company);
			}
		}

		ProcessCompanyResult ProcessCompany(INotifications notifications, GlbCompany company)
		{
			var result = ProcessCompanyResult.Failed;
			var branch = AccountingUtils.GetTopOneActiveBranchOfCompany(company.PK, Factory);
			var inner = new NotificationBuffer(notifications);
			GLTransactionBusinessObject glBizObj = null;

			try
			{
				if (branch == null)
				{
					inner.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("923E3CA4-E1D2-46e9-B549-C3C172E55F2A", "Cannot process company {0} because it does not have active branch.", company.GC_Code)));
				}
				else
				{
					using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
					{
						try
						{
							if (IsValidToRunExport(company, inner))
							{
								var aggregateRunner = GetIAggregateRunner();

								if (aggregateRunner != null)
								{
									if (aggregateRunner.Aggregate())
									{
										glBizObj = GetNewGLTransactionBusinessObject();
										glBizObj.ExportDirectory = SystemDataRegistry.Instance.GLTransactionsCSVExportDirectory.Value;
										glBizObj.CreateAndExportBatch = true;

										notifications.Notify(new InfoNotification(Res.GetString("9971dde5-5fbc-434e-a942-0cfe0e0852ee", "Export GL Transactions Process of Company {0} Started.", company.GC_Code)));

										GLTransactionExporter exporter = GetNewGLTransactionExporter(glBizObj, inner);
										glBizObj.Exporter = exporter;
										try
										{
											glBizObj.Factory.Save();
										}
										catch (ZCannotSaveException) { }

										notifications.Notify(new InfoNotification(Res.GetString("81501de1-76f8-4944-896f-e6fbd49c7f80", "Export GL Transactions Process of Company {0} Finished.", company.GC_Code)));
										result = ProcessCompanyResult.Success;
									}
									else
									{
										var message = aggregateRunner.AggregateResult;

										if (aggregateRunner.FailedToAquireMutex)
										{
											message = aggregateRunner.FailedToAquireMutexReason;
											result = ProcessCompanyResult.Locked;
										}
										else if (aggregateRunner.FailedWithDeadlock)
										{
											result = ProcessCompanyResult.DatabaseDeadlock;
										}

										message = Res.GetString("3a1f5f6e-7820-4ab1-824a-d95bdc404bfa", "Export GL Transactions Process of Company {0} cannot run now because: {1}", company.GC_Code, message);

										if (result == ProcessCompanyResult.Failed)
										{
											inner.Notify(new ErrorNotification(ErrorType.Error, message));
										}
										else    // Locked || DatabaseDeadlock
										{
											inner.Notify(new InfoNotification(message));
										}
									}
								}
							}
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							inner.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("4264b2de-36e9-495b-9575-e93cb2e22ffb", "Error Exporting GL Transactions of Company {0} : {1}.", company.GC_Code,
								string.Format("{0}{1}{2}", ex.Message, System.Environment.NewLine, ex.StackTrace))));
						}
					}
				}
			}
			finally
			{
				SendEmailToNotificationGroup(inner, company, glBizObj);
			}

			return result;
		}

		void SendEmailToNotificationGroup(NotificationBuffer notification, GlbCompany company, GLTransactionBusinessObject glTransactions)
		{
			bool hasErrors = notification.HasErrors;
			Guid emailGrp = SystemDataRegistry.Instance.GLTransCSVExportNotificationGroup.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty);

			if (emailGrp != Guid.Empty)
			{
				EmailDef email = new EmailDef();
				email.Subject = (hasErrors ? Res.GetString("c50d8789-ccc0-4cb1-8628-b1f76b5773a5", "GL Transactions of Company {0} Export Failure", company.GC_Code)
											: Res.GetString("a75a4faf-8094-4d0e-b7da-002a496c5398", "GL Transactions of Company {0} Export Successfully", company.GC_Code)) +
								(glTransactions != null && !glTransactions.BatchNumber.IsEmpty ? " - " + Res.GetString("ca2e1cac-5657-4cd3-bdeb-05fb6eac1ca0", "Batch Number: {0}", glTransactions.BatchNumber) : "");

				INotificationType notificationType = (hasErrors) ? ErrorType.Error : NotificationSubscriberType.Info;
				email.Body = GetNotifications(notification, notificationType);

				try
				{
					Env.OutgoingMailManager.CreateAndSave(email, emailGrp, GroupSourceLocator.GetFromRegistryItem(SystemDataRegistry.Instance.GLTransCSVExportNotificationGroup));
				}
				catch (EmailHasNoRecipientsException ex)
				{
					notification.Notify(new ErrorNotification(ErrorType.Error, ex.Message));
				}
			}
		}

		ZString GetNotifications(NotificationBuffer notifications, INotificationType notificationType)
		{
			ZStringBuilder builder = new ZStringBuilder();
			foreach (INotification current in notifications.GetEventsByType(notificationType))
			{
				builder.Append(current.Message);
			}
			return builder.ToStringWithNewLineBetweenAppends();
		}

		bool IsValidToRunExport(GlbCompany company, INotifications notifications)
		{
			Guid companyPK = company.PK.ToGuid();
			ZString exportDir = SystemDataRegistry.Instance.GLTransactionsCSVExportDirectory.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);

			bool isValidToRunExport = !exportDir.IsEmpty &&
					Directory.Exists(exportDir) &&
					SystemDataRegistry.Instance.GLTransCSVExportNotificationGroup.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty) != Guid.Empty;

			if (!isValidToRunExport)
			{
				notifications.Notify(new ErrorNotification(ErrorType.Error,
					Res.GetString("25151300-cbd6-45d9-81c5-798e171061c1", "Export GL Transactions to CSV registry items of Company {0} are not set or invalid. Please verify the values in Admin->Registry->System->Data Export Settings->Export GL Transactions to CSV",
					company.GC_Code)));
			}

			return isValidToRunExport;
		}

		List<GlbCompany> GetActiveCompanies()
		{
			List<GlbCompany> result = new List<GlbCompany>();

			GlbCompany[] companies = AccountingUtils.GetAllActiveCompanies(Factory);

			if (companies != null)
			{
				foreach (GlbCompany current in companies)
				{
					if (SystemDataRegistry.Instance.EnableAutomaticGLTransactionsCSVExport.GetValueWithoutFallback(current.PK.ToGuid(), Guid.Empty, Guid.Empty))
					{
						result.Add(current);
					}
				}
			}

			return result;
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		#endregion
	}
}
