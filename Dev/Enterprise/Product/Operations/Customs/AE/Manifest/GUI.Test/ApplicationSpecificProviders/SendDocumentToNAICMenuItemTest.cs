using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.GUI;

namespace Enterprise.Customs.AE.Manifest.GUI.Testing;

sealed class SendDocumentToNAICMenuItemTest : eDocMenuItemTestCase
{
	protected override void RunAssertionsForGetEnabledStatus(eDocMenuItem menuItem, StorageDocsBase bizO)
	{
		AssertEquals("Case: no BizO selected under mouse, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(null, false));
		AssertEquals("Case: no BizO selected under mouse, Multiple rows selected", false, menuItem.GetEnabledStatus(null, true));

		AssertEquals("Case: BizO selected under mouse, Multiple rows NOT selected", true, menuItem.GetEnabledStatus(bizO, false));
		AssertEquals("Case: BizO selected under mouse, Multiple rows selected", false, menuItem.GetEnabledStatus(bizO, true));

		bizO.SC_IsDeleted = true;
		AssertEquals("Case: BizO selected under mouse is deleted, Multiple rows NOT selected", false, menuItem.GetEnabledStatus(bizO, false));
		AssertEquals("Case: BizO selected under mouse is deleted, Multiple rows selected", false, menuItem.GetEnabledStatus(bizO, true));
	}

	protected override eDocMenuItem GetMenuItem() => new SendDocumentToNAICMenuItem(null);
}
