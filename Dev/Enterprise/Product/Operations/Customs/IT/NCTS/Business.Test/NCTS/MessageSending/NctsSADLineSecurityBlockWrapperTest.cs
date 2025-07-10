using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Messaging.SAD;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsSADLineSecurityBlockWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new NctsSADLineSecurityBlockWrapper(goodsItem: null));
	}

	public void TestConsignor()
	{
		var organization = GetOrganizationForTest();
		goodsItem.SecurityConsignor.E2_OA_Address = organization.MainAddress.PK;
		AssertTraderWrapper(lineSecurityBlockWrapper.Consignor);
	}

	public void TestConsignee()
	{
		var organization = GetOrganizationForTest();
		goodsItem.SecurityConsignee.E2_OA_Address = organization.MainAddress.PK;
		AssertTraderWrapper(lineSecurityBlockWrapper.Consignee);
	}

	public void TestTransportChargesMethodOfPayment()
	{
		AssertEquals(nameof(lineSecurityBlockWrapper.TransportChargesMethodOfPayment), ZString.Empty, lineSecurityBlockWrapper.TransportChargesMethodOfPayment);

		goodsItem.BY_TransportChargesMethodOfPayment = "X";
		AssertEquals(nameof(lineSecurityBlockWrapper.TransportChargesMethodOfPayment), "X", lineSecurityBlockWrapper.TransportChargesMethodOfPayment);
	}

	public void TestCommercialReferenceNumber()
	{
		AssertEquals(nameof(lineSecurityBlockWrapper.CommercialReferenceNumber), ZString.Empty, lineSecurityBlockWrapper.CommercialReferenceNumber);

		goodsItem.BY_CommercialReferenceNumber = "A";
		AssertEquals(nameof(lineSecurityBlockWrapper.CommercialReferenceNumber), "A", lineSecurityBlockWrapper.CommercialReferenceNumber);
	}

	public void TestUNDangerousGoodsCode()
	{
		AssertEquals(nameof(lineSecurityBlockWrapper.UNDangerousGoodsCode), ZString.Empty, lineSecurityBlockWrapper.UNDangerousGoodsCode);

		goodsItem.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
		goodsItem.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0005", "", "IMO").First().PK;
		AssertEquals(nameof(lineSecurityBlockWrapper.UNDangerousGoodsCode), "0004", lineSecurityBlockWrapper.UNDangerousGoodsCode);
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.NewDepartureNctsHeader();
		nctsMovementHeader = nctsHeader.MovementHeader;
		goodsItem = nctsMovementHeader.GoodsItems.AddNew();
		lineSecurityBlockWrapper = new NctsSADLineSecurityBlockWrapper(goodsItem);
	}
	NctsHeader nctsHeader;
	NctsDepartureMovementHeader nctsMovementHeader;
	NctsDepartureCargoDesc goodsItem;
	NctsSADLineSecurityBlockWrapper lineSecurityBlockWrapper;

	#region Implementation

	void AssertTraderWrapper(ITrader trader)
	{
		AssertNotNull(nameof(trader), trader);
		CombineAssertions(() =>
		{
			AssertType<SADTraderWrapper>($"{nameof(trader)} type", trader);
			AssertEquals(nameof(trader.IdCountryCode), "IT", trader.IdCountryCode);
			AssertEquals(nameof(trader.ID), "385040449", trader.ID);
			AssertEquals(nameof(trader.Name), "IKEA", trader.Name);
			AssertEquals(nameof(trader.Address), "MAIN ADDRESS", trader.Address);
			AssertEquals(nameof(trader.Postcode), "4000", trader.Postcode);
			AssertEquals(nameof(trader.City), "ABCEXPMEL", trader.City);
			AssertEquals(nameof(trader.CountryCode), "ZA", trader.CountryCode);
		});
	}

	OrgHeader GetOrganizationForTest()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.CustomsCodes.AddNew("EOR", "385040449", "IT");
		orgHeader.MainAddress.CompanyName = "IKEA";
		orgHeader.MainAddress.Address1 = "MAIN";
		orgHeader.MainAddress.Address2 = "ADDRESS";
		orgHeader.MainAddress.Postcode = "4000";
		orgHeader.MainAddress.City = "ABCEXPMEL";
		orgHeader.MainAddress.OA_RN_NKCountryCode = "ZA";
		return orgHeader;
	}

	#endregion
}
