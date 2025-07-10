using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class TIRHeaderPrincipalTraderWrapperTest : NctsSADHeaderPrincipalTraderWrapperTest
{
	public override void TestTIRHolderIdentification()
	{
		AssertEquals(nameof(PrincipalTraderWrapper.TIRHolderIdentification), ZString.Empty, PrincipalTraderWrapper.TIRHolderIdentification);

		orgHeader.CustomsCodes.AddNew("TIR", "385040449", "IT");
		AssertEquals(nameof(PrincipalTraderWrapper.TIRHolderIdentification), "385040449", PrincipalTraderWrapper.TIRHolderIdentification);
	}

	protected override SADTraderWrapper GetNewSADTraderWrapper() => new TIRHeaderPrincipalTraderWrapper(jobDocAddress);

	protected override NctsSADHeaderPrincipalTraderWrapper GetPrincipalTraderWrapper(JobDocAddress principalTraderAddress, JobDocAddress representativeAddress) => new TIRHeaderPrincipalTraderWrapper(principalTraderAddress);
}
