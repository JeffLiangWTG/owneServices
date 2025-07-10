using System;
using System.Linq;
using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class SystemUsageDocumentSupporter : DocumentSupporter
	{
		public SystemUsageDocumentSupporter(SystemUsage systemUsage)
			: base(systemUsage)
		{
			//This is needed because SystemUsage is Nonpersistent. See DocEngine or ask Ben for more details.
			this.HasChanges = false;
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CargoWiseBilling; }
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Core.Constants.DataContext[] { Enterprise.Core.Constants.DataContext.CargoWiseBilling };
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Core.Constants.DataContext.CargoWiseBilling && commandBeingRun.SU_MenuName.StartsWith("Billing Usage", StringComparison.OrdinalIgnoreCase))
			{
				BillingSystem billingSystem = new BillingSystemList().FirstOrDefault(x => x.SystemCode == BusinessObject.SystemCode);
				if (billingSystem != null)
				{
					var user = BusinessObject.User;
					var context = new BillingLoadRawUsageContext(Factory, BusinessObject.PeriodStart, BusinessObject.OrganisationPK, user.ClientCompanyPK, user.LicenceCompanyPK, user.DatabasePK);
					SystemRawUsage rawUsage = billingSystem.LoadOdplRawUsage(context);
					if (rawUsage != null)
					{
						return new DocumentWrapper[] { DocSystemRawUsage.New(rawUsage, Factory) };
					}
				}
			}

			return Array.Empty<DocumentWrapper>();
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		public new SystemUsage BusinessObject
		{
			get { return (SystemUsage)base.BusinessObject; }
		}
	}
}

