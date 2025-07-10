using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobDeclarationEventParentFinder : DataTransfer.Universal.JobDeclarationEventParentFinder
	{
		public JobDeclarationEventParentFinder(BusinessObjectFactory factory, JobDeclarationDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContextCore(UniversalEvent eventData)
		{
			var result = base.GetLogParentsForEventUsingContextCore(eventData);

			if ((result == null || result.Length == 0) && eventData.IsNEXDOCS())
			{
				BusinessObject parent = null;

				if (eventData.IsNEXDOCSNotification())
				{
					parent = FindNotificationParent(eventData);
				}
				else if (eventData.IsNEXDOCSMessageReceived())
				{
					parent = FindParentByRexNumber(eventData);
				}
				else if (eventData.IsNEXDOCSCertificatePrint())
				{
					parent = FindOrCreateCertificateParent(eventData);
				}

				if (parent != null)
				{
					result = new[] { parent };
				}
			}

			return result;
		}

		#region ParentFinders

		BusinessObject FindNotificationParent(IXmlEventValueObject eventData)
		{
			const string TenDigitRexNumberPattern = @"\sREX[0-9]{10}";

			var decoder = new NEXDOCEventDecoder(eventData, logger);
			BusinessObject parent = new QuarantineNexDocNotificationCreator(factory).Create(decoder.NotificationType, decoder.NotificationTitle, decoder.NotificationText);
			if (parent == null)
			{
				var matchedRex = Regex.Match(decoder.NotificationTitle, TenDigitRexNumberPattern);
				if (matchedRex.Success)
				{
					parent = FindDeclarationByRexNumber(matchedRex.Value.Trim());
				}
			}

			return parent;
		}

		JobDeclaration FindParentByRexNumber(IXmlEventValueObject eventData)
		{
			var decoder = new NEXDOCEventDecoder(eventData, logger);
			return FindDeclarationByRexNumber(decoder.RexNumber);
		}

		BusinessObject FindOrCreateCertificateParent(IXmlEventValueObject eventData)
		{
			var decoder = new NEXDOCEventDecoder(eventData, logger);
			var declaration = FindDeclarationByRexNumber(decoder.RexNumber)
				?? FindDeclarationByExporterReference(decoder.ExporterReference)
				?? CreateDeclarationForCertificate(decoder);

			return declaration;
		}

		JobDeclaration FindDeclarationByRexNumber(ZString rexNumber)
		{
			JobDeclaration result = null;

			if (factory != null && !rexNumber.IsEmpty)
			{
				var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, QuarantineExDocHeaderSchema.PK);
				cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, QuarantineExDocHeaderSchema.Constants.TableName);
				cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Australia);
				cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumber.EntryType.RequestForPermitStatus);
				cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, rexNumber);

				var quarantineExDocHeaderQuery = new ZDBOnlySubQuery(typeof(QuarantineExDocHeader), QuarantineExDocHeaderSchema.QH_JZ);
				quarantineExDocHeaderQuery.AddSubQuery(cusEntryNumQuery, JoinCondition.And);

				var jobComInvoiceHeaderQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
				jobComInvoiceHeaderQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, false);
				jobComInvoiceHeaderQuery.AddSubQuery(quarantineExDocHeaderQuery, JoinCondition.And);

				var query = new ZDBOnlyQuery(typeof(JobDeclaration));
				query.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Quarantine);
				query.AddSubQuery(jobComInvoiceHeaderQuery, JoinCondition.And);

				var declarations = factory.Load<JobDeclaration>(query);
				if (declarations.Length == 1)
				{
					result = declarations[0];
				}
			}

			return result;
		}

		JobDeclaration FindDeclarationByExporterReference(ZString exporterReference)
		{
			JobDeclaration result = null;

			if (factory != null && !exporterReference.IsEmpty)
			{
				var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, QuarantineExDocHeaderSchema.PK);
				cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, QuarantineExDocHeaderSchema.Constants.TableName);
				cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Australia);
				cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumber.EntryType.RequestForPermitStatus);
				cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.IsNotBlank, ZString.Empty);

				var quarantineExDocHeaderQuery = new ZDBOnlySubQuery(typeof(QuarantineExDocHeader), QuarantineExDocHeaderSchema.QH_JZ);
				quarantineExDocHeaderQuery.AddSubQuery(cusEntryNumQuery, JoinCondition.And);

				var jobComInvoiceHeaderQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE, notIn: true);
				jobComInvoiceHeaderQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, false);
				jobComInvoiceHeaderQuery.AddSubQuery(quarantineExDocHeaderQuery, JoinCondition.And);

				var query = new ZDBOnlyQuery(typeof(JobDeclaration));
				query.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Quarantine);
				query.AddToFilter(JobDeclarationSchema.JE_OwnerRef, exporterReference);
				query.AddSubQuery(jobComInvoiceHeaderQuery, JoinCondition.And);

				result = factory.LoadTop1<JobDeclaration>(query);
			}

			return result;
		}

		JobDeclaration CreateDeclarationForCertificate(NEXDOCEventDecoder decoder)
		{
			var defaultBranch = factory.Load<GlbBranch>(AUCustomsDataRegistry.Instance.DefaultBranchForTransferIn.Value);
			var useDefaultBranch = (defaultBranch?.Country?.Code ?? ZString.Empty) == Core.Constants.CountryCodes.Australia;
			using (useDefaultBranch ? DisposableEnvironment.ForBranch(defaultBranch.PK.ToGuid()) : null)
			{
				var tempFactory = new BusinessObjectFactory();
				var declaration = tempFactory.New<JobDeclaration>();
				declaration.JE_GB = GlbBranch.CurrentBranch.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
				declaration.JE_OwnerRef = decoder.ExporterReference;
				declaration.Invoices.AddNew();
				declaration.QuarantineInvoice.QuarantineExDocHeader.QH_RequestForPermitNumber = decoder.RexNumber;

				if (ZDateTime.TryParseISO8601Date(decoder.DepartureDate, out var departureDate))
				{
					declaration.JE_ExportDate = departureDate;
				}

				tempFactory.Save();

				return factory.Load<JobDeclaration>(declaration.PK);
			}
		}

		#endregion
	}
}
