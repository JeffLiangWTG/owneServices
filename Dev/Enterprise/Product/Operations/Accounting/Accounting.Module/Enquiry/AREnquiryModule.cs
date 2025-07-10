using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI.ARAP.Statements;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class AREnquiryModule : ARTransactionModuleStrip
	{
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

		MultilingualString PrintStatementMenuText
		{
			get { return ResString.GetMultilingualString("f2cba5c9-8914-4269-9c66-35fba96acd07", "Print Stat&ement"); }
		}
		MultilingualString PrintStatementPackMenuText
		{
			get { return ResString.GetMultilingualString("2ba6f718-da79-4182-97eb-e237846f98ab", "Print Statement &Pack"); }
		}
		string OrganisationIsInvalidMessageText
		{
			get { return Res.GetString("43c928b0-cb3c-4886-afc6-cb9a0a83b514", "Please select the organization first"); }
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AREnquiry; }
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AREnquiryFilterControl(GridCollection, (AREnquiryFilterBusinessObject)FilterBusinessObject);
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ReceivablesAccountEnquiry; }
		}

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewAdditionalMenuItems());
			PrintMenuItem.MenuItems.Add(new ZMenuItem(PrintStatementMenuText, new EventHandler(HandlePrintStatement)));
			PrintMenuItem.MenuItems.Add(new ZMenuItem(PrintStatementPackMenuText, new EventHandler(HandlePrintStatementPack)));
			return result.ToArray();
		}

		protected virtual StatementPrintForm GetStatementPrintForm(Statement statementObject)
		{
			return new StatementPrintForm(statementObject);
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}

		#endregion

		#region Grid / Filter Strips

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			var aRTransactionHeaders = new ARTransactionHeaderCollection(Factory);
			aRTransactionHeaders.IsManagedForDataRefresh = true;
			aRTransactionHeaders.CountChanged += new CollectionCountChangedEventHandler(GridCollection_CountChanged);
			return new FilteredTransactionHeaderCollectionView(aRTransactionHeaders);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			AREnquiryFilterBusinessObject filterBizO = new AREnquiryFilterBusinessObject();
			filterBizO.ModuleFiltersCreated += new ModuleFiltersCreatedHandler(FilterBizO_ModuleFiltersCreated);

			CollectionForDefault.OrganizationGuid = filterBizO.OrganisationPK;
			return filterBizO;
		}

		void FilterBizO_ModuleFiltersCreated(IFilterStripBusinessObject filterStripBizO)
		{
			((APEnquiryFilterBusinessObject)filterStripBizO).OrganisationFilter.PropertyInfo.ValueChanged += new EventHandler(FilterOrganisationChanged);
		}

		protected override ResultCountMessage GetNewResultCountMessage()
		{
			return new EnquireyResultCountMessage((IFilterControl)EmbeddedControl, MaxRowsToLoad, MaxRecommendedRowsToLoad);
		}

		protected override void HandleNewCore(string transactionType)
		{
			if (!IsCurrentOrgInactiveAndValid)
			{
				ZController controller = ControllerFromTransactionType(transactionType);
				controller.SetCollectionForDefaultsAndValidation(CollectionForDefault);
				controller.ShowNewForm();
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("a55f4e56-7518-4cc5-a854-44808a4c618f", "New Transactions cannot be created because the current organization is inactive"), Res.GetString("4e619e69-ded7-413b-863f-bc9e371879bd", "New Transactions"));
			}
		}

		void FilterOrganisationChanged(object sender, EventArgs e)
		{
			CollectionForDefault.OrganizationGuid = ((AREnquiryFilterBusinessObject)FilterBusinessObject).OrganisationPK;
		}

		ZBool IsCurrentOrgInactiveAndValid
		{
			get
			{
				ZBool result = false;
				if (CollectionForDefault.OrganizationGuid.IsValid)
				{
					OrgHeader currentOrg = Factory.Load<OrgHeader>(CollectionForDefault.OrganizationGuid);
					if (currentOrg != null && !currentOrg.OH_IsActive)
					{
						result = true;
					}
				}
				return result;
			}
		}

		void GridCollection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			BusinessObjectCollection gridcollection = GridCollection is FilteredTransactionHeaderCollectionView ? ((FilteredTransactionHeaderCollectionView)GridCollection).CollectionToFilter : (BusinessObjectCollection)GridCollection;
			if (!gridcollection.IsLoading && !IsPerformingSearch)
			{
				((AREnquiryFilterBusinessObject)FilterBusinessObject).SetInfoValues();
			}
		}

		void HandlePrintStatement(object sender, EventArgs e)
		{
			ShowStatementForm(false);
		}

		void HandlePrintStatementPack(object sender, EventArgs e)
		{
			ShowStatementForm(true);
		}

		void ShowStatementForm(ZBool statementPack)
		{
			ZGuid currentOrganisationPK = ((AREnquiryFilterBusinessObject)FilterBusinessObject).OrganisationPK;

			if (!Env.Security.ReceivablesCollectionDocuments.IsAllowed)
			{
				Env.Security.ReceivablesCollectionDocuments.ShowError();
			}
			else if (!currentOrganisationPK.IsValid)
			{
				Globals.Message.ShowInformation(OrganisationIsInvalidMessageText, Res.GetString("70769750-38a6-4abe-b75a-3a81d3b4ca6a", "Print Statement") + ((statementPack) ? " " + Res.GetString("7f47663a-275c-4e10-a2ce-91b9eac6eb78", "Pack") : ""));
			}
			else
			{
				Statement statementObject = GetNewStatementBusinessObject(statementPack, currentOrganisationPK);
				ZFormModaliser.ShowDialogAndDispose(GetStatementPrintForm(statementObject));
			}
		}

		protected virtual Statement GetNewStatementBusinessObject(ZBool statementPack, ZGuid currentOrganisationPK)
		{
			Statement statementObject = Statement.New(GlbBranch.CurrentBranch);
			statementObject.OH_PK = currentOrganisationPK;
			statementObject.CreditStatements = Statement.CreditOptions.AllDocuments;

			if (statementPack)
			{
				statementObject.IssueStatementPack = AccountingConstants.IssueStatementPackType.StatementAndInvoices;
			}

			return statementObject;
		}

		#endregion
	}
}
