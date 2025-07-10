using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsDataRetrieveMethodsTest : TestCaseWithFactory
	{
		public void TestGetJobDocAddress()
		{
			var item = CreateNctsDepartureCargoDesc();
			var address = Factory.New<JobDocAddress>();
			address.E2_ParentID = item.PK;
			address.E2_ParentTableCode = item.TablePrefix;
			address.E2_AddressType = "CEA";
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertNull("wrong type", NctsDataRetrieveMethods.GetJobDocAddress(item, "XXX"));
				AssertNull("wrong parent", NctsDataRetrieveMethods.GetJobDocAddress(CreateNctsDepartureCargoDesc(), "CEA"));
				AssertEquals("correct type", address, NctsDataRetrieveMethods.GetJobDocAddress(item, "CEA"));
			});
		}

		public void TestGetUNDGDataItems()
		{
			var item = CreateNctsDepartureCargoDesc();
			var undg = Factory.New<UNDGDataItem>();
			undg.DI_ParentID = item.PK;
			undg.DI_ParentTableCode = item.TablePrefix;
			Factory.Save();
			AssertEquals(1, NctsDataRetrieveMethods.GetUNDGDataItems(item).Count);
		}

		NctsDepartureCargoDesc CreateNctsDepartureCargoDesc()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			return bill.GoodsItems.AddNew();
		}

		public void TestGetCusReferences()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			bill.CusSupplyChainActorReferences.AddNew();
			bill.CusSupplyChainActorReferences.AddNew();
			bill2.CusSupplyChainActorReferences.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("correct PK", 2, NctsDataRetrieveMethods.GetCusReferences(Factory, bill.PK).Count);
				AssertEquals("correct PK, with type filter", 2, NctsDataRetrieveMethods.GetCusReferences(Factory, bill.PK, "SCA").Count);
				AssertEquals("empty PK", 0, NctsDataRetrieveMethods.GetCusReferences(Factory, ZGuid.Empty).Count);
				AssertEquals("invalid PK", 0, NctsDataRetrieveMethods.GetCusReferences(Factory, ZGuid.Invalid).Count);
				AssertEquals("missing PK", 0, NctsDataRetrieveMethods.GetCusReferences(Factory, ZGuid.Missing).Count);
			});
		}

		public void TestGetCusSupportingInfo()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = Factory.New<NctsBill>();
			bill.B0_BH = header.PK;
			var bill2 = Factory.New<NctsBill>();
			bill2.B0_BH = header.PK;
			var cusSupportingInfo1 = Factory.New<CusSupportingInfo>();
			cusSupportingInfo1.CSI_ParentID = bill.PK;
			cusSupportingInfo1.CSI_Type = "OTH";
			cusSupportingInfo1.CSI_SubType = "REF";
			var cusSupportingInfo2 = Factory.New<CusSupportingInfo>();
			cusSupportingInfo2.CSI_ParentID = bill.PK;
			cusSupportingInfo2.CSI_Type = "OTH";
			cusSupportingInfo2.CSI_SubType = "REF";
			var cusSupportingInfo3 = Factory.New<CusSupportingInfo>();
			cusSupportingInfo3.CSI_ParentID = bill.PK;
			cusSupportingInfo3.CSI_Type = "OTH";
			cusSupportingInfo3.CSI_SubType = "TRA";
			var cusSupportingInfo4 = Factory.New<CusSupportingInfo>();
			cusSupportingInfo4.CSI_ParentID = bill.PK;
			cusSupportingInfo4.CSI_Type = "PRE";
			cusSupportingInfo4.CSI_SubType = null;
			var cusSupportingInfo5 = Factory.New<CusSupportingInfo>();
			cusSupportingInfo5.CSI_ParentID = bill2.PK;
			cusSupportingInfo5.CSI_Type = "OTH";
			cusSupportingInfo5.CSI_SubType = "REF";

			AssertEquals(3, NctsDataRetrieveMethods.GetCusSupportingInfo(Factory, bill.PK, "OTH").Count);
			AssertEquals(2, NctsDataRetrieveMethods.GetCusSupportingInfo(Factory, bill.PK, "OTH", "REF").Count);
			AssertEquals(0, NctsDataRetrieveMethods.GetCusSupportingInfo(Factory, bill.PK, ZString.Empty).Count);
			AssertEquals(0, NctsDataRetrieveMethods.GetCusSupportingInfo(Factory, ZGuid.Empty, "OTH").Count);
			AssertEquals(0, NctsDataRetrieveMethods.GetCusSupportingInfo(Factory, ZGuid.Invalid, "OTH").Count);
			AssertEquals(0, NctsDataRetrieveMethods.GetCusSupportingInfo(Factory, ZGuid.Missing, "OTH").Count);
		}
	}
}
