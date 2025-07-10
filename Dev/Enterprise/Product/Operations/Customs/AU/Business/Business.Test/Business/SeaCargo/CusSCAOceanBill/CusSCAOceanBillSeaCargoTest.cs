using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAOceanBillSeaCargoTest : SeaCargoTestCase
	{
		const string ABN1 = "83058313205";
		const string ABN2 = "63004280871";
		public void TestPrincipalID()
		{
			OrgHeader shippingLine1 = CreateShippingLine("Shipping Line 1");
			shippingLine1.LocalBusinessRegNo = ABN1;
			OrgHeader shippingLine2 = CreateShippingLine("Shipping Line 2");
			shippingLine2.LocalBusinessRegNo = ABN2;
			OrgHeader forwarderSharingABN = CreateConsignee("Forwarder 1");
			forwarderSharingABN.OH_IsForwarder = true;
			forwarderSharingABN.LocalBusinessRegNo = ABN2;

			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OH_ShippingLine = shippingLine1.PK;
			AssertEquals("OceanBill Principal ID Default from Shipping Line", ABN1, oceanBill.CB_PrincipalID);
			oceanBill.CB_PrincipalID = ABN2;
			AssertEquals("OceanBill Shipping Line Defaulting From Principal", shippingLine2.PK, oceanBill.CB_OH_ShippingLine);
		}

		const string ABNWithSpaces1 = "83 058 313 205";
		const string ABNWithSpaces2 = "63 004 280 871";

		public void TestSpacesStrippedFromResponsiblePartyIDAndPrincipalDuringDefaulting()
		{
			OrgHeader shippingLine = CreateShippingLine("Shipping Line");
			shippingLine.LocalBusinessRegNo = ABNWithSpaces1;
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = ABNWithSpaces2;

			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			AssertEquals("Responsibgle Part ID Default", ABN2, oceanBill.CB_ResponsiblePartyID);
			oceanBill.CB_OH_ShippingLine = shippingLine.PK;
			AssertEquals("Principal ID default", ABN1, oceanBill.CB_PrincipalID);
		}

		public void TestQuestionMarksReplaceInvalidPrincipalID()
		{
			OrgHeader shippingLine = CreateShippingLine("Shipping Line");
			shippingLine.LocalBusinessRegNo = InvalidABN;

			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OH_ShippingLine = shippingLine.PK;
			AssertEquals("Invalid Principal ID", "???", oceanBill.CB_PrincipalID);
		}

		const string InvalidABN = "8305831320583058313205";

		public void TestCanBeDeleted()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			AssertEquals(true, oceanBill.CanDelete);

			var house = oceanBill.HouseBills.AddNew();
			AssertEquals(true, oceanBill.CanDelete);

			house.CA_MessageStatus = ZString.Empty;
			AssertEquals(true, oceanBill.CanDelete);

			house.CA_MessageStatus = CMRBaseStatuses.Codes.NotSent;
			AssertEquals(true, oceanBill.CanDelete);

			house.CA_MessageStatus = CMRBaseStatuses.Codes.AmendmentRejected;
			AssertEquals(false, oceanBill.CanDelete);

			oceanBill = Factory.New<CusSCAOceanBill>();
			house = oceanBill.HouseBills.AddNew();
			var pivot = house.Pivot.AddNew();
			var underbond = pivot.Underbonds.AddNew();

			underbond.UnderbondStatus.Code = ZString.Empty;
			AssertEquals(true, oceanBill.CanDelete);

			underbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.NotSent;
			AssertEquals(true, oceanBill.CanDelete);

			underbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			AssertEquals(false, oceanBill.CanDelete);

			oceanBill = Factory.New<CusSCAOceanBill>();
			var container = oceanBill.Containers.AddNew();
			var underbond2 = container.Underbonds.AddNew();
			AssertEquals(true, oceanBill.CanDelete);

			underbond2.UnderbondStatus.Code = ZString.Empty;
			AssertEquals(true, oceanBill.CanDelete);

			underbond2.UnderbondStatus.Code = CMRBaseStatuses.Codes.NotSent;
			AssertEquals(true, oceanBill.CanDelete);

			underbond2.UnderbondStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			AssertEquals(false, oceanBill.CanDelete);
		}

		public void TestCB_IsBureauDefaults()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_IsBureau = true;
			CusSCAContainer container = oceanBill.Containers.AddNew();
			container.CN_ContainerNumber = ContainerNumber1;
			CusSCAHouse house = oceanBill.HouseBills.AddNew();
			house.Pivot.Add(container.Pivots.AddNew());
			house.CA_HouseBill = HouseBillNumber1;
			CusUnderbond containerUnderbond = container.Underbonds.AddNew();
			CusUnderbond houseUnderbond = house.Pivot[0].Underbonds.AddNew();
			oceanBill.AllUnderbonds.Load();
			AssertEquals("Container Underbond Default Is Bureau", true, containerUnderbond.C4_IsBureau);
			AssertEquals("House Underbond Default Is Bureau", true, houseUnderbond.C4_IsBureau);
			oceanBill.CB_IsBureau = false;
			AssertEquals("Container Underbond Ocean Bill Changed Is Bureau", false, containerUnderbond.C4_IsBureau);
			AssertEquals("House Underbond Ocean Bill Changed Is Bureau", false, houseUnderbond.C4_IsBureau);
			oceanBill.CB_IsBureau = true;
			AssertEquals("Container Underbond Ocean Bill Changed Is Bureau", true, containerUnderbond.C4_IsBureau);
			AssertEquals("House Underbond Ocean Bill Changed Is Bureau", true, houseUnderbond.C4_IsBureau);

			containerUnderbond.C4_MessageStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			houseUnderbond.C4_MessageStatus = CMRBaseStatuses.Codes.OriginalRejected;

			oceanBill.CB_IsBureau = false;
			AssertEquals("Container Underbond Ocean Bill Changed Is Bureau", true, containerUnderbond.C4_IsBureau);
			AssertEquals("House Underbond Ocean Bill Changed Is Bureau", false, houseUnderbond.C4_IsBureau);
		}
	}
}
