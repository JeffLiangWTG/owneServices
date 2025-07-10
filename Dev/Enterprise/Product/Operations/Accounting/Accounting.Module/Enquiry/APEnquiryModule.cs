using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class APEnquiryModule : APTransactionModuleStrip
	{
		public APEnquiryModule() : base() { }

		public APEnquiryModule
			(
				(IWithholdingJournalCreatorForMultipleInvoices withholdingJournalCreatorForMultipleInvoices, IWithholdingJournalRealizerForMultipleInvoices withholdingJournalRealizerForMultipleInvoices) withholdingJournaldependecies
			)
			: base (withholdingJournaldependecies)
		{ }

		#region Default Collection

		public ITransactionCollection<TransactionHeader> CollectionForDefault
		{
			get
			{
				if (fCollectionForDefault == null)
				{
					fCollectionForDefault = new ITransactionCollection<TransactionHeader>(Factory);
				}
				return fCollectionForDefault;
			}
		}

		ITransactionCollection<TransactionHeader> fCollectionForDefault;

		#endregion

		#region Implementation

		protected override bool CanAddAuditAndCashMenus => false;

		protected override bool CanCreateComplianceDocuemntsMenus => false;

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.APEnquiry; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.PayablesAccountEnquiry; }
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new APEnquiryFilterControl(GridCollection, (APEnquiryFilterBusinessObject)FilterBusinessObject);
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}

		#endregion

		#region Grid / Filter Strips

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			var aPTransactionHeaders = new APTransactionHeaderCollection(Factory);
			aPTransactionHeaders.IsManagedForDataRefresh = true;
			aPTransactionHeaders.CountChanged += new CollectionCountChangedEventHandler(GridCollection_CountChanged);
			return new FilteredTransactionHeaderCollectionView(aPTransactionHeaders);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			APEnquiryFilterBusinessObject filterBizO = new APEnquiryFilterBusinessObject();
			filterBizO.ModuleFiltersCreated += new ModuleFiltersCreatedHandler(FilterBizO_ModuleFiltersCreated);
			CollectionForDefault.OrganizationGuid = filterBizO.OrganisationPK;
			return filterBizO;
		}

		void FilterBizO_ModuleFiltersCreated(IFilterStripBusinessObject filterStripBizO)
		{
			((APEnquiryFilterBusinessObject)filterStripBizO).OrganisationFilter.PropertyInfo.ValueChanged += new EventHandler(FilterOrganisationChanged);
		}

		protected override void HandleNewCore(string transactionType)
		{
			ZController controller = ControllerFromTransactionType(transactionType);
			controller.SetCollectionForDefaultsAndValidation(CollectionForDefault);
			controller.ShowNewForm();
		}

		OrgHeader CurrentOrg
		{
			get { return Factory.Load<OrgHeader>(CollectionForDefault.OrganizationGuid); }
		}

		void DisableCopyAndReverseFunction()
		{
			FormActionMenu.ToString(); // force lazy property initialization
			DeleteMenuItem.Enabled = false;
			CopyMenuItem.Enabled = false;
		}

		protected override ResultCountMessage GetNewResultCountMessage()
		{
			return new EnquireyResultCountMessage((IFilterControl)EmbeddedControl, MaxRowsToLoad, MaxRecommendedRowsToLoad);
		}

		void FilterOrganisationChanged(object sender, EventArgs e)
		{
			CollectionForDefault.OrganizationGuid = ((APEnquiryFilterBusinessObject)FilterBusinessObject).OrganisationPK;
			if (CurrentOrg != null && !CurrentOrg.OH_IsActive)
			{
				DisableCopyAndReverseFunction();
			}
		}

		void GridCollection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			BusinessObjectCollection gridcollection = GridCollection is FilteredTransactionHeaderCollectionView ? ((FilteredTransactionHeaderCollectionView)GridCollection).CollectionToFilter : (BusinessObjectCollection)GridCollection;
			if (!gridcollection.IsLoading && !IsPerformingSearch)
			{
				((APEnquiryFilterBusinessObject)FilterBusinessObject).SetInfoValues();
			}
		}

		#endregion
	}
}
