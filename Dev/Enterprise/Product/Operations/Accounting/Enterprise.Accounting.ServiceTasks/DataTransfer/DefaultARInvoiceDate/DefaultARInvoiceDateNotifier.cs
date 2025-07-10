using System;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ServiceTasks.DataTransfer.DefaultARInvoiceDate
{
	internal class DefaultARInvoiceDateNotifier : IProcessor
	{
		public DefaultARInvoiceDateNotifier(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		readonly BusinessObjectFactory Factory;

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			GlbCompany[] companies = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_IsActive, true));

			foreach (GlbCompany currentCompany in companies)
			{
				token.ThrowIfCancellationRequested();
				if (currentCompany.Branches.Any() && AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.GetValueWithoutFallback(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty) == AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code)
				{
					var branchWithEarliestLocalTime = currentCompany.Branches.OrderBy(branch => branch.HomePort.LocationDateTime).First();
					var earliestLocalTime = branchWithEarliestLocalTime.HomePort.LocationDateTime;

					if (earliestLocalTime.Hour == 0)
					{
						if (ARDefaultInvoiceAndPostDateCalculator.CheckSuspensionNeedsToBeLifted(earliestLocalTime, currentCompany))
						{
							using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser, branchWithEarliestLocalTime.PK.ToGuid(), Env.CurrentDepartment != null ? Env.CurrentDepartment.PK : Guid.Empty)))
							{
								notifications.Notify(new InfoNotification(Res.GetString("333b6454-7951-4c74-869f-622a91469df7", "Sending email notification to Company {0}", currentCompany.GC_Code)));
								(new InvoiceDateIncrementingSuspensionEmail(InvoiceDateIncrementingSuspensionEmail.EmailType.SuspensionNeedsToBeLifted, currentCompany.GC_Code)).Send();
								notifications.Notify(new InfoNotification(Res.GetString("a615ac2f-7e02-4581-aa17-ad9628ed782c", "Email notification sent to Company {0}", currentCompany.GC_Code)));
							}
						}
					}
				}
			}
		}
	}
}
