using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class TIRHeaderPrincipalTraderWrapper : NctsSADHeaderPrincipalTraderWrapper
{
	public TIRHeaderPrincipalTraderWrapper(JobDocAddress jobDocAddress) : base(jobDocAddress)
	{
	}

	protected override ZString TIRHolderIdentificationCore => Organisation?.GetCusCode(TirCusCodeType)?.OK_CustomsRegNo ?? ZString.Empty;

	const string TirCusCodeType = "TIR";
}
