using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Messaging.SAD;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public abstract class NctsSADHeaderPrincipalTraderWrapper : SADTraderWrapper, IETHeaderPrincipalTrader
{
	protected NctsSADHeaderPrincipalTraderWrapper(JobDocAddress principalTraderAddress) : base(principalTraderAddress)
	{
	}

	public ZString TraderGuaranteeTaxIdentificationNumber => ZString.Empty;

	public ZString TIRHolderIdentification => TIRHolderIdentificationCore;
	protected abstract ZString TIRHolderIdentificationCore { get; }

	public ZString RepresentativeGuaranteeTaxIdentificationNumber => RepresentativeGuaranteeTaxIdentificationNumberCore;
	protected virtual ZString RepresentativeGuaranteeTaxIdentificationNumberCore => ZString.Empty;

	public ZString RepresentativeName => RepresentativeNameCore;
	protected virtual ZString RepresentativeNameCore => ZString.Empty;

	public ZString RepresentativeType => ZString.Empty;

	protected override ZBool ShouldUseZeroCustomsCodePlaceholderIfNoneFound => ZBool.False;
}
