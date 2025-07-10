using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.DocumentScanning.GUI
{
	sealed class ReviewParsedResultsMenuItemTestCase : eDocMenuItemTestCase
	{
		protected override eDocMenuItem GetMenuItem()
		{
			return new ReviewParsedResultsMenuItem();
		}

		public override void TestGetEnabledStatusWhenMultipleElementsSelected()
		{
			var document1 = StorageDocs.NewWithParentWithoutFK_DEBUG(MasterFactory);
			var document2 = StorageDocs.NewWithParentWithoutFK_DEBUG(MasterFactory);

			var menuItem = GetMenuItem();
			AssertEquals("Case: Multiple rows selected", false, menuItem.GetEnabledStatusWhenMultipleElementsSelected(new StorageDocs[] { document1, document2 }));
		}

		protected override void RunAssertionsForGetEnabledStatus(eDocMenuItem menuItem, StorageDocsBase bizO)
		{
			AssertEquals("Case: no BizO selected under mouse, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(null, false));
			AssertEquals("Case: no BizO selected under mouse, Multiple rows selected", false, menuItem.GetEnabledStatus(null, true));

			AssertEquals("Case: BizO selected under mouse, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse, Multiple rows selected", false, menuItem.GetEnabledStatus(bizO, true));

			bizO.SC_IsDeleted = true;
			AssertEquals("Case: BizO selected under mouse is deleted, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse is deleted, Multiple rows selected", false, menuItem.GetEnabledStatus(bizO, true));
		}

		public void TestReviewParsedResultsEnabledStatus()
		{
			var menuItem = GetMenuItem();
			var numberedFactory = MasterFactory.GetFactory(1);

			var document = StorageDocs.NewWithParentWithoutFK_DEBUG(numberedFactory);
			document.SC_DataType = "JPG";
			document.ParentMain.SM_ParentFK = Factory.NewWithValidTestData<OrgHeader>().PK;
			RunAssertionsForReviewParsedResultsEnabledStatus(menuItem, document);

			var file = StorageFile.NewWithParent_DEBUG(numberedFactory);
			file.SC_DataType = "PDF";
			file.ParentMain.SM_ParentFK = Factory.NewWithValidTestData<OrgHeader>().PK;
			RunAssertionsForReviewParsedResultsEnabledStatus(menuItem, file);
		}

		public void RunAssertionsForReviewParsedResultsEnabledStatus(eDocMenuItem menuItem, StorageDocsBase bizO)
		{
			MockSetupShipamaxParseStatus(bizO, ZString.Empty, true);
			AssertEquals("Case: BizO selected under mouse is allocated, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse is allocated, Multiple rows selected", false, menuItem.GetEnabledStatus(bizO, true));

			MockSetupShipamaxParseStatus(bizO, EDIMessageStatusList.Codes.Sent);
			AssertEquals("Case: BizO selected under mouse is allocated, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse is allocated, Multiple rows selected", false, menuItem.GetEnabledStatus(bizO, true));

			MockSetupShipamaxParseStatus(bizO, EDIMessageStatusList.Codes.Queued);
			AssertEquals("Case: BizO selected under mouse is allocated, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse is allocated, Multiple rows selected", false, menuItem.GetEnabledStatus(bizO, true));

			MockSetupShipamaxParseStatus(bizO, EDIMessageStatusList.Codes.Failed);
			AssertEquals("Case: BizO selected under mouse is allocated, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse is allocated, Multiple rows selected", false, menuItem.GetEnabledStatus(bizO, true));

			MockSetupShipamaxParseStatus(bizO, EDIMessageStatusList.Codes.Discarded);
			AssertEquals("Case: BizO selected under mouse is allocated, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse is allocated, Multiple rows selected", false, menuItem.GetEnabledStatus(bizO, true));

			MockSetupShipamaxParseStatus(bizO, EDIMessageStatusList.Codes.ProcessedOK);
			AssertEquals("Case: BizO selected under mouse is allocated, Multiple rows NOT selected", true, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse is allocated, Multiple rows selected", false, menuItem.GetEnabledStatus(bizO, true));

			MockSetupShipamaxParseStatus(bizO, EDIMessageStatusList.Codes.PreProcessedOK);
			AssertEquals("Case: BizO selected under mouse is allocated, Multiple rows NOT selected", true, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse is allocated, Multiple rows selected", false, menuItem.GetEnabledStatus(bizO, true));
		}

		void MockSetupShipamaxParseStatus(StorageDocsBase bizO, ZString status, bool initialSetup = false)
		{
			if (initialSetup)
			{
				using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					bizO.SC_DocType = "CIV";
					bizO.SC_IsDeleted = false;

					var shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
					bizO.ParentMain.SM_ParentFK = shipment.PK;
					bizO.ParentMain.SM_Type = "SHP";

					MasterFactory.Save();

					AssertNotNull("EDocsShipamaxMessage should be created when a new eDoc is saved", bizO.ActiveShipamaxMessage);
					AssertEquals("Unparsed", bizO.ParseStatus);
				}
			}

			bizO.ActiveShipamaxMessage.EM_Status = status;
		}
	}
}
