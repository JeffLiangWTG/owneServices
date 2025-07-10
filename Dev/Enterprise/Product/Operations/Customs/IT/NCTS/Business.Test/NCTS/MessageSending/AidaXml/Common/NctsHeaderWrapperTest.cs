using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

[TestedType(typeof(NctsHeaderWrapper))]
sealed class NctsHeaderWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new NctsHeaderWrapper(null));
	}

	public void TestGetBindingItinerary()
	{
		AssertEquals("When CountiesOfRouting is empty", 0, wrapper.GetBindingItinerary());

		header.CountriesOfRouting.AddNew();
		AssertEquals("When CountiesOfRouting is not empty", 1, wrapper.GetBindingItinerary());
	}

	public void TestCarrier()
	{
		AssertEquals("When Carrier is not set", null, wrapper.GetCarrier());

		var carrierHeader = Factory.New<OrgHeader>();
		carrierHeader.CustomsCodes.AddNew("EOR", "385040449", "IT");
		header.MovementHeader.Carrier.E2_OA_Address = carrierHeader.MainAddress.PK;

		var carrier = wrapper.GetCarrier();
		AssertNotNull("Carrier", carrier);
		AssertEquals("IT385040449", carrier.IdentificationNumber);
	}

	public void TestGetContainerIndicator()
	{
		AssertEquals("Pre Condition", 0, CreateWrapper().GetContainerIndicator());

		var headerContainers = header.DepartureHeaderContainers;
		var cnt2 = headerContainers.AddNew();
		cnt2.BC_Mode = "NCT";
		cnt2.BC_ContainerNum = "CURE123456";
		cnt2.Seal1 = "Seal4";
		AssertEquals("when no container exists", 0, CreateWrapper().GetContainerIndicator());

		var cnt1 = headerContainers.AddNew();
		cnt1.BC_Mode = "CNT";
		cnt1.BC_ContainerNum = "TURE123456";
		cnt1.Seal1 = "Seal1";
		cnt1.Seal2 = "Seal2";
		cnt1.AdditionalSeals.AddNew().BK_SealNumber = "Seal3";
		AssertEquals("when container exists", 1, CreateWrapper().GetContainerIndicator());
	}

	public void TestGetGoodsReferences()
	{
		var headerContainer = header.DepartureHeaderContainers.AddNew();
		headerContainer.BC_ContainerNum = "TURE123456";

		AssertEquals("GetGoodsReference", 0, wrapper.GetGoodsReference(headerContainer).Count);

		var houseConsignment1 = header.Bills.AddNew();
		var goodsItem1 = houseConsignment1.GoodsItems.AddNew();
		goodsItem1.BY_DeclarationGoodsItemNumber = 1;
		var package1 = goodsItem1.Packages.AddNew();
		package1.ContainersPivotsForBindingOnly[0].ContainerSelected = true;
		var goodsItem2 = houseConsignment1.GoodsItems.AddNew();
		goodsItem2.BY_DeclarationGoodsItemNumber = 2;
		var package2 = goodsItem2.Packages.AddNew();
		package2.ContainersPivotsForBindingOnly[0].ContainerSelected = false;

		var houseConsignment2 = header.Bills.AddNew();
		var goodsItem3 = houseConsignment2.GoodsItems.AddNew();
		goodsItem3.BY_DeclarationGoodsItemNumber = 3;
		var package3 = goodsItem3.Packages.AddNew();
		package3.ContainersPivotsForBindingOnly[0].ContainerSelected = false;
		var goodsItem4 = houseConsignment2.GoodsItems.AddNew();
		goodsItem4.BY_DeclarationGoodsItemNumber = 4;
		var package4 = goodsItem4.Packages.AddNew();
		package4.ContainersPivotsForBindingOnly[0].ContainerSelected = true;

		var houseConsignment3 = header.Bills.AddNew();
		var goodsItem5 = houseConsignment3.GoodsItems.AddNew();
		goodsItem5.BY_DeclarationGoodsItemNumber = 5;

		var goodsReferences = wrapper.GetGoodsReference(headerContainer);
		AssertContainsExactElementsInAnyOrder("GetGoodsReference", new[] { 1, 4 }, goodsReferences.Select(g => g.DeclarationGoodsItemNumber).ToArray());
	}

	public void TestConsignee()
	{
		using (SetTransitionPeriod(true))
		{
			var wrapper = CreateWrapper();
			var consignee = wrapper.GetConsignee();
			AssertNull("Consignee", consignee);

			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew("EOR", "385040449", "IT");
			var address = org.Addresses.AddNew();
			header.Consignee.E2_OA_Address = address.PK;
			wrapper = CreateWrapper();
			consignee = wrapper.GetConsignee();

			AssertNotNull("Consignee", consignee);
			AssertType<EoriOrTcuTraderWrapper>(consignee);
			AssertEquals("IT385040449", consignee.IdentificationNumber);
		}
	}

	public void TestConsigneeDuringNonTransitionPeriodWithPortIsNotC0009AndSecurityBTH()
	{
		SetupC0009List();
		using (SetTransitionPeriod(false))
		{
			var wrapper = CreateWrapper();
			var consignee = wrapper.GetConsignee();
			AssertNull("Consignee", consignee);

			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew("EOR", "385040449", "IT");
			var consigneeAddress = org.Addresses.AddNew();
			header.Consignee.E2_OA_Address = consigneeAddress.PK;

			movementHeader.BM_RL_NKDestinationPort = "IN";
			movementHeader.BM_TypeOfSecurity = "BTH";
			Add30600AdditionalInfo(header.AdditionalDocuments);

			wrapper = CreateWrapper();
			consignee = wrapper.GetConsignee();

			AssertNull("Consignee", consignee);
		}
	}

	public void TestConsigneeDuringNonTransitionPeriodWithPortIsC0009()
	{
		SetupC0009List();
		using (SetTransitionPeriod(false))
		{
			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew("EOR", "385040449", "IT");
			var consigneeAddress = org.Addresses.AddNew();
			header.Consignee.E2_OA_Address = consigneeAddress.PK;

			movementHeader.BM_RL_NKDestinationPort = "IT";
			var bill1 = header.Bills.AddNew();
			bill1.Consignee.E2_OA_Address = consigneeAddress.PK;

			var wrapper = CreateWrapper();
			var consignee = wrapper.GetConsignee();

			AssertNotNull("Consignee", consignee);
			AssertType<EoriOrTcuTraderWrapper>(consignee);
			AssertEquals("IT385040449", consignee.IdentificationNumber);
		}
	}

	public void TestConsigneeWithHouseConsignmentInstancesHavingSameConsignee()
	{
		SetupC0009List();
		using (SetTransitionPeriod(false))
		{
			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew("EOR", "385040449", "IT");

			movementHeader.BM_RL_NKDestinationPort = "IN";
			movementHeader.BM_TypeOfSecurity = "BTH";
			header.AdditionalDocuments.Clear();

			var houseConsignment1 = header.Bills.AddNew();
			var commonAddress = org.Addresses.AddNew();
			houseConsignment1.Consignee.E2_OA_Address = commonAddress.PK;

			Add30600AdditionalInfo(houseConsignment1.AdditionalDocuments);

			var wrapper = CreateWrapper();
			var consignee = wrapper.GetConsignee();

			AssertNull("Consignee", consignee);

			movementHeader.BM_TypeOfSecurity = "NON";
			movementHeader.BM_RL_NKDestinationPort = "IT";

			wrapper = CreateWrapper();
			consignee = wrapper.GetConsignee();

			AssertNotNull("Consignee", consignee);
			AssertType<EoriOrTcuTraderWrapper>(consignee);
			AssertEquals("IT385040449", consignee.IdentificationNumber);

			var houseConsignment2 = header.Bills.AddNew();
			houseConsignment2.Consignee.E2_OA_Address = commonAddress.PK;

			var houseConsignment3 = header.Bills.AddNew();
			houseConsignment3.Consignee.E2_OA_Address = commonAddress.PK;

			wrapper = CreateWrapper();
			consignee = wrapper.GetConsignee();

			AssertNotNull("Consignee", consignee);
			AssertEquals("IT385040449", consignee.IdentificationNumber);
		}
	}

	public void TestConsigneeWithHouseConsignmentInstancesHavingDifferentConsignee()
	{
		SetupC0009List();
		using (SetTransitionPeriod(false))
		{
			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew("EOR", "385040449", "IT");

			movementHeader.BM_RL_NKDestinationPort = "IN";
			movementHeader.BM_TypeOfSecurity = "BTH";
			header.AdditionalDocuments.Clear();

			var houseConsignment1 = header.Bills.AddNew();
			var commonAddress = org.Addresses.AddNew();
			houseConsignment1.Consignee.E2_OA_Address = commonAddress.PK;

			Add30600AdditionalInfo(houseConsignment1.AdditionalDocuments);

			var wrapper = CreateWrapper();
			var consignee = wrapper.GetConsignee();

			AssertNull("Consignee", consignee);

			movementHeader.BM_TypeOfSecurity = "NON";
			movementHeader.BM_RL_NKDestinationPort = "IT";

			wrapper = CreateWrapper();
			consignee = wrapper.GetConsignee();

			AssertNotNull("Consignee", consignee);
			AssertType<EoriOrTcuTraderWrapper>(consignee);
			AssertEquals("IT385040449", consignee.IdentificationNumber);

			var houseConsignment3 = header.Bills.AddNew();
			houseConsignment3.Consignee.E2_OA_Address = Factory.New<OrgHeader>().Addresses.AddNew().PK;

			wrapper = CreateWrapper();
			consignee = wrapper.GetConsignee();
			AssertNull("Consignee", consignee);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		movementHeader = header.MovementHeader;
		wrapper = new NctsHeaderWrapper(header);
	}

	AdditionalInfo Add30600AdditionalInfo(ICusSupportingInfoCollection<AdditionalInfo> additionalInfoCollection)
	{
		var additionalInfo = additionalInfoCollection.AddNew();
		additionalInfo.CSI_Code = AdditionalDocumentTypes._30600;
		additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;

		return additionalInfo;
	}

	void SetupC0009List()
	{
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, "C0009 Desc");
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy,
			EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009,
			Core.Constants.CountryCodes.Italy,
			ZDateTime.MinSmallDateTimeValue,
			ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();
	}

	IDisposable SetTransitionPeriod(bool isActive)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isActive);

	INctsHeaderWrapper CreateWrapper() => new NctsHeaderWrapper(header);

	NctsHeader header;
	NctsDepartureMovementHeader movementHeader;
	INctsHeaderWrapper wrapper;
}
