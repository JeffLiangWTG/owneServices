using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Interfaces.TTCE.Outgoing;
using CargoWise.Customs.BR.MessageDefinitions.TTCE;

namespace Enterprise.Customs.BR.Business
{
	public class OptionalLegalBasisProvider : IOptionalLegalBasis
	{
		OptionalLegalBasisProvider(FundamentoLegalOpcionalDisponivelDTO optionalLegalBasis)
		{
			this.optionalLegalBasis = Argument.NotNull(optionalLegalBasis, nameof(optionalLegalBasis));
		}
		readonly FundamentoLegalOpcionalDisponivelDTO optionalLegalBasis;

		public static OptionalLegalBasisProvider New(FundamentoLegalOpcionalDisponivelDTO optionalLegalBasis) => optionalLegalBasis == null ? null : new OptionalLegalBasisProvider(optionalLegalBasis);

		public int TributeCode => int.TryParse(optionalLegalBasis.tributo?.codigo, out var result) ? result : 0;

		public int RegimeCode => int.TryParse(optionalLegalBasis.regime?.codigo, out var result) ? result : 0;

		public int LegalBasiCode => int.TryParse(optionalLegalBasis.fundamentoLegal?.codigo, out var result) ? result : 0;

		public string AlternativaCode => string.Empty;
	}
}

