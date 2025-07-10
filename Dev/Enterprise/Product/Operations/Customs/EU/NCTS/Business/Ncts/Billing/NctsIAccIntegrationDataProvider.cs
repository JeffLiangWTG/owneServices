using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsIAccIntegrationDataProvider : IAccIntegrationDataProvider
	{
		public NctsIAccIntegrationDataProvider(ChargePosterBehaviours actions, ZGuid nctsHeaderPk, BusinessObjectFactory factory)
		{
			this.actions = actions;
			this.billingFactory = factory;
			this.nctsHeaderInBillingFactory = billingFactory.Load<NctsHeader>(nctsHeaderPk);
		}

		protected readonly ChargePosterBehaviours actions;
		protected readonly BusinessObjectFactory billingFactory;
		protected readonly NctsHeader nctsHeaderInBillingFactory;

		#region IControllerIDProvider Members

		IControllerIDProvider ControllerIDProvider
		{
			get { return nctsHeaderInBillingFactory; }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get
			{
				var controllerIDProvider = ControllerIDProvider;
				return controllerIDProvider != null ? controllerIDProvider.ControllerID : null;
			}
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get
			{
				var controllerIDProvider = ControllerIDProvider;
				return controllerIDProvider != null ? controllerIDProvider.BusinessObjectPK : Guid.Empty;
			}
		}

		#endregion

		BusinessObjectFactory IAccIntegrationDataProvider.Factory
		{
			get { return billingFactory; }
		}

		ZGuid IAccIntegrationDataProvider.AutoPostingEmailRecipient => ZGuid.Empty;  // nothing to notify

		ChargePosterBehaviours IAccIntegrationDataProvider.Action
		{
			get { return actions; }
		}

		ZGuid[] IAccIntegrationDataProvider.DisbursementChargeCodes
		{
			get { return Array.Empty<ZGuid>(); }
		}

		ZString IAccIntegrationDataProvider.ReferenceID
		{
			get { return nctsHeaderInBillingFactory?.BH_JobReference ?? ZString.Empty; }
		}

		ZString IAccIntegrationDataProvider.JobType
		{
			get { return NctsHeaderAssemblyData.HumanReadableNameShared; }
		}

		IAccInvoiceDataProvider[] IAccIntegrationDataProvider.InvDataProviders
		{
			get
			{
				return Array.Empty<IAccInvoiceDataProvider>();
			}
		}

		bool IAccIntegrationDataProvider.SupportIntegration
		{
			get
			{
				return nctsHeaderInBillingFactory != null
					&& (nctsHeaderInBillingFactory.IsDepartureTabReadOnly // i.e. is departure
						|| nctsHeaderInBillingFactory.IsArrivalTabReadOnly);   // i.e. is arrival beyond unloading
			}
		}

		void IAccIntegrationDataProvider.LogPostingResult(string message)
		{
		}

		Action IAccIntegrationDataProvider.OnIntegrated
		{
			get { return OnIntegratedWithAccountingSuccessfully; }
		}

		void OnIntegratedWithAccountingSuccessfully()
		{
		}

		GlbCompany IAccIntegrationDataProvider.Company
		{
			get { return nctsHeaderInBillingFactory?.Company; }
		}
	}
}

