using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentScanning.GUI
{
	sealed class SplitDocumentMenuItemTest : eDocMenuItemTestCase
	{
		protected override void RunAssertionsForGetEnabledStatus(eDocMenuItem menuItem, StorageDocsBase bizO)
		{
			if (bizO.ParentMain != null)
			{
				OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
				bizO.ParentMain.SM_ParentFK = header.PK;
			}
			bizO.SC_DataType = Core.Constants.FileFormats.PDF;
			AssertEquals("Case: BizO selected under mouse, BizO not in database, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(bizO, false));
			bizO.Factory.Save();

			AssertEquals("Case: BizO selected under mouse, Multiple rows NOT selected", true, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse, Multiple rows selected", false, menuItem.GetEnabledStatus(bizO, true));

			AssertEquals("Case: no BizO selected under mouse, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(null, false));
			AssertEquals("Case: no BizO selected under mouse, Multiple rows selected", false, menuItem.GetEnabledStatus(null, true));
			bizO.SC_IsDeleted = true;
			AssertEquals("Case: BizO selected under mouse is deleted, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(bizO, false));
			AssertEquals("Case: BizO selected under mouse is deleted, Multiple rows selected", false, menuItem.GetEnabledStatus(bizO, true));

			bizO.SC_DataType = Core.Constants.FileFormats.GIF;
			AssertEquals("Case: File type of BizO selected under mouse is not supported, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(bizO, false));
		}

		protected override eDocMenuItem GetMenuItem()
		{
			return new SplitDocumentMenuItem();
		}
	}
}
