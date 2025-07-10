using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusCAeMHItem))]
	sealed class CusCAeMHItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIHouseBillLineMembers()
		{
			CusCAeMHItem item = (CusCAeMHItem)GetNewBusinessObject();
			item.BX_LineNumber = 1;
			item.BX_Quantity = 23;
			item.BX_QuantityUQ = "BOX";
			item.BX_Marks = "MARKS";
			item.BX_Description = "GOODS";
			item.BX_HSCode = "12345678";
			item.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			item.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "1066", "", "IMO").First().PK;

			AssertEquals(1, ((IHouseBillLine)item).LineNumber);
			AssertEquals(23m, ((IHouseBillLine)item).Packs);
			AssertEquals("BOX", ((IHouseBillLine)item).PacksUOM);
			AssertEquals("MARKS", ((IHouseBillLine)item).Marks.First());
			AssertEquals("GOODS", ((IHouseBillLine)item).GoodsDescription);
			AssertEquals("12345678", ((IHouseBillLine)item).HSCode);
			AssertEquals(2, ((IHouseBillLine)item).DGCodes.Count());
			AssertEquals("0004", ((IHouseBillLine)item).DGCodes.First());
			AssertEquals("1066", ((IHouseBillLine)item).DGCodes.Last());

			item.HouseBill.MasterBill.BP_ModeOfTransport = TransportTypeList.Codes.Sea;
			item.BX_IsDangerousInBulk = true;
			AssertEquals(1, ((IHouseBillLine)item).DGCodes.Count());
			AssertEquals("MHB", ((IHouseBillLine)item).DGCodes.First());
		}

		public void TestFirstUNDG()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "AS";
			subs.DG_UNNO = "AS";
			subs.DG_MP = "M";

			CusCAeMHItem item = (CusCAeMHItem)GetNewBusinessObject();
			item.FirstUNDG = subs.PK;
			Assert("UNDG should be registered as an EditableChildObject", item.IsRegisteredEditableChildObject(item.UNDG));
			Assert("HasChanges should be set", item.HasChanges);
			AssertNotNull(item.UNDG);
		}

		public void TestBX_QuantityUQDescription()
		{
			var item = House.Items.AddNew();

			item.BX_QuantityUQ = ACROSSPackageTypes.Codes.AMMOPACK;
			AssertEquals(ACROSSPackageTypes.Descriptions.AMMOPACK, item.BX_QuantityUQDescription);

			item.BX_QuantityUQ = ACROSSPackageTypes.Codes.ATTACHMENT;
			AssertEquals(ACROSSPackageTypes.Descriptions.ATTACHMENT, item.BX_QuantityUQDescription);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var item = House.Items.AddNew();
			item.BX_LineNumber = 1;
			return item;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		CusCAeMHHouse House
		{
			get { return fHouse ?? (fHouse = Factory.NewWithValidTestData<CusCAeMHMaster>().HouseBills.AddNew()); }
		}
		CusCAeMHHouse fHouse;
	}
}
