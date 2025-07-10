using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class TransitHeaderWrapperTest : NctsSADHeaderCommonWrapperTest<TransitHeaderWrapper>
{
	protected override Type ExpectedMeansOfTransportCrossingBorderType => typeof(TransitHeaderMeansOfTransportCrossingBorderWrapper);

	public void TestMeansOfTransportCrossingBorderForTransit()
	{
		nctsMovementHeader.BM_TOLCarrierCode = "";
		nctsMovementHeader.BM_TOLCarrierID = "";
		CombineAssertions("When input value are empty", () => AssertMeansOfTransportCrossingBorder(ZString.Empty, ZString.Empty));

		nctsMovementHeader.BM_TOLCarrierCode = "KR";
		nctsMovementHeader.BM_TOLCarrierID = "RX2839A";
		CombineAssertions("When input value are filled", () => AssertMeansOfTransportCrossingBorder("KR", "RX2839A"));

		void AssertMeansOfTransportCrossingBorder(ZString nationality, ZString identity)
		{
			var meansOfTransportCrossingBorder = headerWrapper.MeansOfTransportCrossingBorder;
			AssertEquals(nameof(meansOfTransportCrossingBorder.Nationality), nationality, meansOfTransportCrossingBorder.Nationality);
			AssertEquals(nameof(meansOfTransportCrossingBorder.Identity), identity, meansOfTransportCrossingBorder.Identity);
			AssertEquals(nameof(meansOfTransportCrossingBorder.Type), ZString.Empty, meansOfTransportCrossingBorder.Type);
		}
	}

	protected override Type ExpectedPrincipalTraderType => typeof(TransitHeaderPrincipalTraderWrapper);

	public override void TestAuthorizationCIN()
	{
		nctsHeader.Authorization = ZString.Empty;
		AssertEquals(nameof(headerWrapper.AuthorizationCIN), ZString.Empty, headerWrapper.AuthorizationCIN);

		nctsHeader.Authorization = "123456D";
		AssertEquals(nameof(headerWrapper.AuthorizationCIN), "D", headerWrapper.AuthorizationCIN);
	}

	public override void TestAuthorizationNo()
	{
		nctsHeader.Authorization = ZString.Empty;
		AssertEquals(nameof(headerWrapper.AuthorizationNo), ZString.Empty, headerWrapper.AuthorizationNo);

		nctsHeader.Authorization = "123456D";
		AssertEquals(nameof(headerWrapper.AuthorizationNo), "123456", headerWrapper.AuthorizationNo);
	}

	protected override Type ExpectedDeclarationType => typeof(TransitHeaderDeclarationWrapper);

	public override void TestGuarantees()
	{
		AssertEquals($"When {nctsHeader.Guarantees} collection is empty, {nameof(headerWrapper.Guarantees)}", 0, headerWrapper.Guarantees.Count());

		nctsHeader.Guarantees.AddNew();
		nctsHeader.Guarantees.AddNew();
		AssertEquals($"When {nctsHeader.Guarantees} collection is not empty, {nameof(headerWrapper.Guarantees)}", 2, headerWrapper.Guarantees.Count());
	}

	protected override Type ExpectedTransactionDataType => typeof(TransitHeaderTransactionDataWrapper);

	public override void TestTransitCustomsOffices()
	{
		AssertEquals($"When {nctsHeader.CustomsOffices} collection is empty, {nameof(headerWrapper.TransitCustomsOffices)}", 0, headerWrapper.TransitCustomsOffices.Count());

		nctsHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "1");
		nctsHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "2");
		nctsHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "3");
		AssertEquals($"When {nctsHeader.CustomsOffices} collection is not empty, {nameof(headerWrapper.TransitCustomsOffices)}", 2, headerWrapper.TransitCustomsOffices.Count());
	}

	protected override TransitHeaderWrapper GetHeaderWrapper(NctsHeader nctsHeader) => new TransitHeaderWrapper(nctsHeader);
}
