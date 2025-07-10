using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.WFN.Testing
{
	[TestedType(typeof(WFNDataRegistry))]
	class WFNDataRegistryTest : RegistryItemSetTestCaseWithFactory<WFNDataRegistry>
	{
		public void TestUserVisibleRegistryItems()
		{
			AssertEquals("Should be 2 Items in the Registry", 2, AllItems.Count);
			AssertVisible(ItemSet.DimercoXMLImportDirectoryRaw);
			AssertVisible(ItemSet.DimercoXMLImportNotificationGroup);
		}

		public void TestFlatTextFileDirectory()
		{
			AssertEquals("DimercoXMLImportDirectory empty", ZString.Empty, ItemSet.DimercoXMLImportDirectory);
			var tempPath = TempForTest.TempPath;
			ItemSet.DimercoXMLImportDirectory = tempPath;
			AssertEquals("DimercoXMLImportDirectory populated", tempPath, ItemSet.DimercoXMLImportDirectory);
		}

		public void TestCodeMapMasterOrganisationRaw()
		{
			GlbGroup group = Factory.LoadTop1<GlbGroup>(new ZQuery());
			ItemSet.DimercoXMLImportNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			AssertEquals("DimercoXMLImportNotificationGroup", group.PK, ItemSet.DimercoXMLImportNotificationGroup.Value);
		}
	}
}
