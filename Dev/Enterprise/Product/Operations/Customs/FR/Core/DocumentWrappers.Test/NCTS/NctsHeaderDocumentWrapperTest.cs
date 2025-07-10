using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.FR.Registry;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.DocumentWrappers.NCTS.Testing;

[TestedType(typeof(NctsHeaderDocumentWrapper))]
sealed class NctsHeaderDocumentWrapperTest : NonPersistentBusinessObjectTestCase
{
	public void TestNew()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

		AssertExceptionThrown<ArgumentNullException>("Expected exception when NctsHeader parameter is null", () => NctsHeaderDocumentWrapper.New(null, Factory));
		AssertExceptionThrown<ArgumentNullException>("Expected exception when Factory parameter is null", () => NctsHeaderDocumentWrapper.New(header, null));

		AssertNotNull("Instance of NctsHeaderDocumentWrapper expected", NctsHeaderDocumentWrapper.New(header, Factory));
	}

	public void TestDocumentWrapperTypeDependingOnIsSecurityDeclaration()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		header.BH_FTZMove = true;
		NCTSTestHelper.CreateJobDocAddressForTest(Factory, "COS", header.SecurityConsignor);

		CombineAssertions(() =>
		{
			AssertType<SecurityNctsHeaderDocumentWrapper>("When IsSecurityDeclaration is true then returned type is SecurityNctsHeaderDocumentWrapper", NctsHeaderDocumentWrapper.New(header, header.Factory));

			header.BH_FTZMove = false;
			AssertType<NctsHeaderDocumentWrapper>("When IsSecurityDeclaration is false then returned type is NctsHeaderDocumentWrapper", NctsHeaderDocumentWrapper.New(header, header.Factory));
		});
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		nctsHeader.MovementHeader.GoodsItems.AddNew();
		return NctsHeaderDocumentWrapper.New(nctsHeader, Factory);
	}

	public void TestLinesCollectionType()
	{
		var header = Factory.New<NctsHeader>();

		var wrapper = new NctsHeaderDocumentWrapperForTest(header);
		AssertType<NctsDepartureCargoDescWrapperCollection>("The returned lines collection is ESNctsDepartureCargoDescWrapperCollection", wrapper.GetLinesCore_Exposed());
	}

	public void TestShowStampOnBoxC()
	{
		var fallbackSetting = new FallbackSettings();
		fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
		fallbackSetting.End = ZDateTime.Today.AddDays(1);
		FRCustomsDataRegistry.Instance.DeltaTMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var moveHeader = header.MovementHeader;
		AssertEquals(true, header.FallBackIsActive);

		moveHeader.IsSimplifiedNctsProcedure = false;
		moveHeader.BM_LocationOfGoodsCode = "";
		var wrapper = NctsHeaderDocumentWrapper.New(header, Factory);
		AssertEquals(nameof(wrapper.SHOWSTAMPONBOXC), false, wrapper.SHOWSTAMPONBOXC);

		moveHeader.IsSimplifiedNctsProcedure = true;
		moveHeader.BM_LocationOfGoodsCode = "";
		wrapper = NctsHeaderDocumentWrapper.New(header, Factory);
		AssertEquals(nameof(wrapper.SHOWSTAMPONBOXC), false, wrapper.SHOWSTAMPONBOXC);

		moveHeader.IsSimplifiedNctsProcedure = false;
		moveHeader.BM_LocationOfGoodsCode = "FR7758258";
		wrapper = NctsHeaderDocumentWrapper.New(header, Factory);
		AssertEquals(nameof(wrapper.SHOWSTAMPONBOXC), false, wrapper.SHOWSTAMPONBOXC);

		moveHeader.IsSimplifiedNctsProcedure = true;
		moveHeader.BM_LocationOfGoodsCode = "FR7758258";
		wrapper = NctsHeaderDocumentWrapper.New(header, Factory);
		AssertEquals(nameof(wrapper.SHOWSTAMPONBOXC), true, wrapper.SHOWSTAMPONBOXC);

		fallbackSetting.Start = ZDateTime.Today.AddDays(1);
		FRCustomsDataRegistry.Instance.DeltaTMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);
		AssertEquals(false, header.FallBackIsActive);

		wrapper = NctsHeaderDocumentWrapper.New(header, Factory);
		AssertEquals(nameof(wrapper.SHOWSTAMPONBOXC), false, wrapper.SHOWSTAMPONBOXC);
	}

	public void TestBoxDSignature()
	{
		var fallbackSetting = new FallbackSettings();
		fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
		fallbackSetting.End = ZDateTime.Today.AddDays(1);
		FRCustomsDataRegistry.Instance.DeltaTMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var moveHeader = header.MovementHeader;
		AssertEquals(true, header.FallBackIsActive);

		moveHeader.IsSimplifiedNctsProcedure = false;
		var wrapper = NctsHeaderDocumentWrapper.New(header, Factory);
		AssertEquals(nameof(wrapper.BOXDSIGNATURE), ZString.Empty, wrapper.BOXDSIGNATURE);

		moveHeader.IsSimplifiedNctsProcedure = true;
		wrapper = NctsHeaderDocumentWrapper.New(header, Factory);
		AssertEquals(nameof(wrapper.BOXDSIGNATURE), "Authorized consignor — 99206", wrapper.BOXDSIGNATURE);

		fallbackSetting.Start = ZDateTime.Today.AddDays(1);
		FRCustomsDataRegistry.Instance.DeltaTMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);
		AssertEquals(false, header.FallBackIsActive);

		wrapper = NctsHeaderDocumentWrapper.New(header, Factory);
		AssertEquals(nameof(wrapper.BOXDSIGNATURE), ZString.Empty, wrapper.BOXDSIGNATURE);
	}

	public void TestBox50Signature()
	{
		var fallbackSetting = new FallbackSettings();
		fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
		fallbackSetting.End = ZDateTime.Today.AddDays(1);
		FRCustomsDataRegistry.Instance.DeltaTMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var moveHeader = header.MovementHeader;
		AssertEquals(true, header.FallBackIsActive);

		moveHeader.IsSimplifiedNctsProcedure = false;
		var wrapper = NctsHeaderDocumentWrapper.New(header, Factory);
		AssertEquals(nameof(wrapper.BOX50SIGNATURE), ZString.Empty, wrapper.BOX50SIGNATURE);

		moveHeader.IsSimplifiedNctsProcedure = true;
		wrapper = NctsHeaderDocumentWrapper.New(header, Factory);
		AssertEquals(nameof(wrapper.BOX50SIGNATURE), "Signature waived — 99207", wrapper.BOX50SIGNATURE);

		fallbackSetting.Start = ZDateTime.Today.AddDays(1);
		FRCustomsDataRegistry.Instance.DeltaTMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);
		AssertEquals(false, header.FallBackIsActive);

		wrapper = NctsHeaderDocumentWrapper.New(header, Factory);
		AssertEquals(nameof(wrapper.BOX50SIGNATURE), ZString.Empty, wrapper.BOX50SIGNATURE);
	}

	public void TestBoxCAuthorizedConsignorName()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "Name";
		var auth1 = orgHeader.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit);
		auth1.CPH_Number = "FR7758258";
		auth1.CPH_OH_PermitHolder = orgHeader.PK;
		Factory.Save();

		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var moveHeader = header.MovementHeader;

		moveHeader.BM_LocationOfGoodsCode = "";
		var wrapper = NctsHeaderDocumentWrapper.New(header, Factory);
		AssertEquals(nameof(wrapper.BOXCAUTHORIZEDCONSIGNORNAME), ZString.Empty, wrapper.BOXCAUTHORIZEDCONSIGNORNAME);

		moveHeader.BM_LocationOfGoodsCode = "123456";
		wrapper = NctsHeaderDocumentWrapper.New(header, Factory);
		AssertEquals(nameof(wrapper.BOXCAUTHORIZEDCONSIGNORNAME), ZString.Empty, wrapper.BOXCAUTHORIZEDCONSIGNORNAME);

		moveHeader.BM_LocationOfGoodsCode = "FR7758258";
		wrapper = NctsHeaderDocumentWrapper.New(header, Factory);
		AssertEquals(nameof(wrapper.BOXCAUTHORIZEDCONSIGNORNAME), "Name", wrapper.BOXCAUTHORIZEDCONSIGNORNAME);
	}

	class NctsHeaderDocumentWrapperForTest : NctsHeaderDocumentWrapper
	{
		public NctsHeaderDocumentWrapperForTest(NctsHeader nctsHeader) : base(nctsHeader, nctsHeader.Factory)
		{
		}

		public DocBaseWrapperCollection<Enterprise.DocumentWrappers.Customs.EU.NCTS.NctsDepartureCargoDescWrapper> GetLinesCore_Exposed() => base.GetLinesCore();
	}
}
