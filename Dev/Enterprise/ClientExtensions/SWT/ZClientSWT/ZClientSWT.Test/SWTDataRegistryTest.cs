using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.SWT.Testing
{
	[TestedType(typeof(SWTDataRegistry))]
	class SWTDataRegistryTest : RegistryItemSetTestCaseWithFactory<SWTDataRegistry>
	{
		public void TestVisibleRegistryItem()
		{
			AssertEquals("AllItems.Count", 4, AllItems.Count);
			AssertVisible(ItemSet.ConsignorsList);
			AssertVisible(ItemSet.NotYetArrivedAutomaticSettings, true);
			AssertVisible(ItemSet.NotYetArrivedReportOpeningTextItem);
			AssertVisible(ItemSet.NotYetArrivedReportClosingTextItem);
		}

		public void TestConsignorsList()
		{
			AssertEquals("No items in the Consignors List", 0, ItemSet.ConsignorsList.Value.Length);
			OrgHeader consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true));
			List<Guid> orgsPKs = new List<Guid>();
			orgsPKs.Add(consignor.PK.ToGuid());
			ItemSet.ConsignorsList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, orgsPKs.ToArray());
			AssertEquals("There should be a consignor in the list", 1, ItemSet.ConsignorsList.Value.Length);
			OrgHeader newOrg = Factory.New<OrgHeader>();
			newOrg.OH_Code = "NewTestOrg";
			newOrg.OH_FullName = "New test organisation";
			Factory.Save();
			orgsPKs.Add(newOrg.PK.ToGuid());
			ItemSet.ConsignorsList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, orgsPKs.ToArray());
			OrgHeaderCodeListCollection loadedOrgsFromRegistry = new OrgHeaderCodeListCollection(ItemSet.ConsignorsList.Value);
			bool result = false;
			foreach (OrgHeaderCodeListElement element in loadedOrgsFromRegistry)
			{
				if (element.ClientGuid == newOrg.PK)
				{
					result = true;
					break;
				}
			}

			AssertEquals("NewOrg should be contained in ConsignorsList registry item", true, result);
		}
	}
}
