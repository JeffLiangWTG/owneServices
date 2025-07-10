using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

[TestedType(typeof(NctsBill))]
sealed class NctsBillTest : EnterpriseBusinessObjectTestCase
{
	public void TestGoodsItemsType()
	{
		AssertType<EU.NCTS.Business.NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>>(nctsBill.GoodsItems);
	}

	public void TestArrivalGoodsItemsType()
	{
		AssertType<NctsArrivalCargoDescCollection>(nctsBill.ArrivalGoodsItems);
	}

	public void TestCusSupplyChainActors()
	{
		AssertType<CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>>(nctsBill.CusSupplyChainActorReferences);
	}

	public void TestArrivalTransportInfos()
	{
		AssertType<EU.NCTS.Business.ArrivalCusTransportMeansCollection<ArrivalCusTransportMeans>>(nctsBill.ArrivalTransportInfos);
	}

	public void TestSupportingDocuments()
	{
		AssertType<EU.NCTS.Business.NctsSupportingDocumentCollection<NctsSupportingDocument>>(nctsBill.SupportingDocuments);
	}

	public void TestPreviousDocuments()
	{
		AssertType<EU.NCTS.Business.CommonPreviousDocumentCollection<CommonPreviousDocument>>(nctsBill.PreviousDocuments);
	}

	public void TestAdditionalInfos()
	{
		AssertType<EU.NCTS.Business.NctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument>>(nctsBill.AdditionalDocuments);
	}

	public void TestGetCusSupportingInfoTypes_SupportingDocument()
	{
		AssertEquals(typeof(NctsSupportingDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)nctsBill).GetCusSupportingInfoTypes()[CusSupportingInfoTypeList.Codes.SupportingDocument]);
	}

	public void TestGetCusSupportingInfoTypes_PreviousDocument()
	{
		AssertEquals(typeof(CommonPreviousDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)nctsBill).GetCusSupportingInfoTypes()[CusSupportingInfoTypeList.Codes.PreviousDocument]);
	}

	public void TestGetCusSupportingInfoTypes_AdditionalInfo()
	{
		AssertEquals(typeof(NctsBillAdditionalDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)nctsBill).GetCusSupportingInfoTypes()[CusSupportingInfoTypeList.Codes.AdditionalInfo]);
	}

	public void TestResetBY_DeclarationGoodsItemNumberOnDelete()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

		var bill1 = nctsHeader.Bills.AddNew();
		var goodItem11 = bill1.GoodsItems.AddNew();
		goodItem11.BY_DeclarationGoodsItemNumber = 1;
		var goodItem12 = bill1.GoodsItems.AddNew();
		goodItem12.BY_DeclarationGoodsItemNumber = 2;

		var bill2 = nctsHeader.Bills.AddNew();
		var goodItem21 = bill2.GoodsItems.AddNew();
		goodItem21.BY_DeclarationGoodsItemNumber = 3;
		var goodItem22 = bill2.GoodsItems.AddNew();
		goodItem22.BY_DeclarationGoodsItemNumber = 4;

		var bill3 = nctsHeader.Bills.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("goodItem11.BY_DeclarationGoodsItemNumber is not 0 before deleting a bill", 1, goodItem11.BY_DeclarationGoodsItemNumber);
			AssertEquals("goodItem12.BY_DeclarationGoodsItemNumber is not 0 before deleting a bill", 2, goodItem12.BY_DeclarationGoodsItemNumber);
			AssertEquals("goodItem21.BY_DeclarationGoodsItemNumber is not 0 before deleting a bill", 3, goodItem21.BY_DeclarationGoodsItemNumber);
			AssertEquals("goodItem22.BY_DeclarationGoodsItemNumber is not 0 before deleting a bill", 4, goodItem22.BY_DeclarationGoodsItemNumber);

			bill1.Delete();
			AssertEquals("goodItem21.BY_DeclarationGoodsItemNumber is not 0 after deleting a bill with goodsItems when it is not in the database, bill", 3, goodItem21.BY_DeclarationGoodsItemNumber);
			AssertEquals("goodItem22.BY_DeclarationGoodsItemNumber is not 0 after deleting a bill with goodsItems when it is not in the database, bill", 4, goodItem22.BY_DeclarationGoodsItemNumber);

			bill1 = nctsHeader.Bills.AddNew();
			goodItem11 = bill1.GoodsItems.AddNew();
			goodItem11.BY_DeclarationGoodsItemNumber = 1;
			goodItem12 = bill1.GoodsItems.AddNew();
			goodItem12.BY_DeclarationGoodsItemNumber = 2;

			Factory.Save();

			bill3.Delete();
			AssertEquals("goodItem11.BY_DeclarationGoodsItemNumber is not 0 after deleting a bill without goodsItems when data is in database, bill3", 1, goodItem11.BY_DeclarationGoodsItemNumber);
			AssertEquals("goodItem12.BY_DeclarationGoodsItemNumber is not 0 after deleting a bill without goodsItems when data is in database, bill3", 2, goodItem12.BY_DeclarationGoodsItemNumber);
			AssertEquals("goodItem21.BY_DeclarationGoodsItemNumber is not 0 after deleting a bill without goodsItems when data is in database, bill3", 3, goodItem21.BY_DeclarationGoodsItemNumber);
			AssertEquals("goodItem22.BY_DeclarationGoodsItemNumber is not 0 after deleting a bill without goodsItems when data is in database, bill3", 4, goodItem22.BY_DeclarationGoodsItemNumber);

			bill1.Delete();
			AssertEquals("goodItem21.BY_DeclarationGoodsItemNumber is 0 after deleting a bill with goodsItems when data is in database, bill1", 0, goodItem21.BY_DeclarationGoodsItemNumber);
			AssertEquals("goodItem22.BY_DeclarationGoodsItemNumber is 0 after deleting a bill with goodsItems when data is in database, bill1", 0, goodItem22.BY_DeclarationGoodsItemNumber);

			bill1 = nctsHeader.Bills.AddNew();
			goodItem11 = bill1.GoodsItems.AddNew();
			goodItem11.BY_DeclarationGoodsItemNumber = 1;
			goodItem12 = bill1.GoodsItems.AddNew();
			goodItem12.BY_DeclarationGoodsItemNumber = 2;
			goodItem21.BY_DeclarationGoodsItemNumber = 3;
			goodItem22.BY_DeclarationGoodsItemNumber = 4;
			nctsHeader.UpdatePreDeclaration = true;

			Factory.Save();
			AssertEquals("goodItem11.BY_DeclarationGoodsItemNumber is not 0 before deleting a bill with goodsItems when data is in database with UpdatePreDeclaration true", 1, goodItem11.BY_DeclarationGoodsItemNumber);
			AssertEquals("goodItem12.BY_DeclarationGoodsItemNumber is not 0 before deleting a bill with goodsItems when data is in database with UpdatePreDeclaration true", 2, goodItem12.BY_DeclarationGoodsItemNumber);
			AssertEquals("goodItem21.BY_DeclarationGoodsItemNumber is not 0 before deleting a bill with goodsItems when data is in database with UpdatePreDeclaration true", 3, goodItem21.BY_DeclarationGoodsItemNumber);
			AssertEquals("goodItem22.BY_DeclarationGoodsItemNumber is not 0 before deleting a bill with goodsItems when data is in database with UpdatePreDeclaration true", 4, goodItem22.BY_DeclarationGoodsItemNumber);

			bill1.Delete();
			AssertEquals("goodItem21.BY_DeclarationGoodsItemNumber is not 0 after deleting a bill with goodsItems when data is in database with UpdatePreDeclaration true, bill1", 3, goodItem21.BY_DeclarationGoodsItemNumber);
			AssertEquals("goodItem22.BY_DeclarationGoodsItemNumber is not 0 after deleting a bill with goodsItems when data is in database with UpdatePreDeclaration true, bill1", 4, goodItem22.BY_DeclarationGoodsItemNumber);
		});
	}

	public void TestJB0_ReferenceID_Caption()
	{
		CombineAssertions(() =>
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(nctsBill.B0_ReferenceIDInfo);
			AssertEquals("Caption", "Reference Number / UCR", captionResourceString.Caption);
			AssertEquals("MediumCaption", "Ref. No. / UCR", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "Ref. No. / UCR", captionResourceString.ShortCaption);
			AssertEquals("FullDescription", "Indicate the Reference Number / Unique Consignment Reference (UCR)", captionResourceString.FullDescription);
		});
	}

	public void TestB0_Weight()
	{
		CombineAssertions(() =>
		{
			nctsBill.B0_Weight = 2;
			AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
			nctsBill.B0_Weight = 3;
			AssertEquals("When CustomsStatus is PRE and field changes", "1", nctsHeader.BH_ReleaseStatus);
		});
	}

	public void TestCustomsEntryIntegrator()
	{
		AssertType<NctsBillCustomsEntryIntegrator>("CustomsEntryIntegrator", nctsBill.GetCustomsEntryIntegrator());
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => nctsBill;

	protected override BusinessObject GetNewBusinessObject() => nctsBill;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => nctsBill;

	protected override void SetUp()
	{
		base.SetUp();
		nctsBill = CreateBill(Factory);
	}
	NctsBill nctsBill;
	NctsHeader nctsHeader;

	NctsBill CreateBill(BusinessObjectFactory factory)
	{
		nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		return nctsHeader.Bills.AddNew();
	}
}
