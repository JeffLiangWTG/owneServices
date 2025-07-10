using System;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class TIRHeaderWrapperTest : NctsSADHeaderCommonWrapperTest<TIRHeaderWrapper>
{
	protected override Type ExpectedMeansOfTransportCrossingBorderType => typeof(TIRHeaderEmptyMeansOfTransportCrossingBorderWrapper);

	protected override Type ExpectedPrincipalTraderType => typeof(TIRHeaderPrincipalTraderWrapper);

	public override void TestAuthorizationCIN()
	{
		AssertEquals(nameof(headerWrapper.AuthorizationCIN), ZString.Empty, headerWrapper.AuthorizationCIN);
	}

	public override void TestAuthorizationNo()
	{
		AssertEquals(nameof(headerWrapper.AuthorizationNo), ZString.Empty, headerWrapper.AuthorizationNo);
	}

	protected override Type ExpectedDeclarationType => typeof(TIRHeaderDeclarationWrapper);

	public override void TestGuarantees()
	{
		var guaranteees = headerWrapper.Guarantees;
		AssertNotNull(nameof(guaranteees), guaranteees);
		AssertEquals($"Number of {nameof(guaranteees)}", 0, guaranteees.Count());
	}

	protected override Type ExpectedTransactionDataType => typeof(TIRHeaderEmptyTransactionDataWrapper);

	public override void TestTransitCustomsOffices()
	{
		var transitCustomsOffices = headerWrapper.TransitCustomsOffices;
		AssertNotNull(nameof(transitCustomsOffices), transitCustomsOffices);
		AssertEquals($"Number of {nameof(transitCustomsOffices)}", 0, transitCustomsOffices.Count());
	}

	protected override TIRHeaderWrapper GetHeaderWrapper(NctsHeader nctsHeader) => new TIRHeaderWrapper(nctsHeader);
}
