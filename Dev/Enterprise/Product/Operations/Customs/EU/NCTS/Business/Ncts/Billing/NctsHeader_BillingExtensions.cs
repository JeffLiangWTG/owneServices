using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public partial class NctsHeader
		: IControllerIDProvider
		, IJobInvoicingPlugIn
	{
		public IAutoBillingResult IntegrateWithAccountingIfRequired(bool useNewFactory = true) => IntegrateWithAccountingIfRequiredCore(useNewFactory);
		protected virtual IAutoBillingResult IntegrateWithAccountingIfRequiredCore(bool useNewFactory)
		{
			return new NctsInvoicePostingAccountingIntegrator().IntegrateIfNecessary(GetJobDeclarationIAccIntegrationDataProvider(useNewFactory));
		}

		public NctsIAccIntegrationDataProvider GetJobDeclarationIAccIntegrationDataProvider(bool useNewFactory)
		{
			var factory = useNewFactory ? new BusinessObjectFactory() : Factory;
			return new NctsIAccIntegrationDataProvider(this.GetChargePosterBehaviours(), this.PK, factory);
		}

		public void SetJobNumberFieldOnSaving()
		{
			PopulateNumberPropertyIfRequired(BH_JobReferenceInfo, GetNewLrnReference);
		}

		public void OnJobCreating(JobHeader job)
		{
		}

		public void OnJobCreated(JobHeader job)
		{
		}

		public void OnJobDeleting(JobHeader job)
		{
		}

		public void OnJobDeleted(JobHeader job)
		{
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get
			{
				var controllerId = ControllerIDs.Customs.EU.NctsMovementController;
				if (IsPluggedIntoShipment)
				{
					controllerId = ControllerIDs.Customs.EU.NctsMovementInShipmentController;
				}
				else
				{
					if (IsPluggedIntoConsol)
					{
						controllerId = ControllerIDs.Customs.EU.NctsMovementInConsolController;
					}
				}
				return controllerId;
			}
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return invoicingSupporter ?? (invoicingSupporter = GetNewInvoicingSupporter()); }
		}
		NctsInvoicingSupporter invoicingSupporter;

		protected virtual NctsInvoicingSupporter GetNewInvoicingSupporter()
		{
			return new NctsInvoicingSupporter(this);
		}

		public bool AllowInvoiceDeletion => false;
	}
}
