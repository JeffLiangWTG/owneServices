using CargoWise.Customs.BR.MessageDefinitions.TTCE;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	class OptionalLegalBasisProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertNull(OptionalLegalBasisProvider.New(null));
			AssertType<OptionalLegalBasisProvider>(OptionalLegalBasisProvider.New(new FundamentoLegalOpcionalDisponivelDTO()));
		}

		public void TestProperties()
		{
			var optionalLegalBasis = new FundamentoLegalOpcionalDisponivelDTO
			{
				tributo = new CodigoNomeTributoDTO { codigo = "10" },
				regime  = new CodigoNomeRegimeDTO { codigo = "01", nome = "Regime" },
				fundamentoLegal = new CodigoNomeTipoFundamentoOpcionalDTO { codigo = "02", nome = "Tipo Fundamento", tipo = "I" }
			};

			var dataProvider = OptionalLegalBasisProvider.New(optionalLegalBasis);

			AssertEquals("TributeCode", 10, dataProvider.TributeCode);
			AssertEquals("RegimeCode", 1, dataProvider.RegimeCode);
			AssertEquals("LegalBasiCode", 2, dataProvider.LegalBasiCode);
			AssertEquals("AlternativaCode", string.Empty, dataProvider.AlternativaCode);
		}
	}
}
