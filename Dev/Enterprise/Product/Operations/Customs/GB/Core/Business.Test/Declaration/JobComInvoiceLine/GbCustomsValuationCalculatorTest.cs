using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Business.Testing
{
	class GbCustomsValuationCalculatorTest : EU.Business.Declaration.Testing.EuCustomsValuationCalculatorTest
	{
		protected override EU.Business.Declaration.JobDeclaration GetNewJobDeclaration() => Factory.New<JobDeclaration>();
	}
}
