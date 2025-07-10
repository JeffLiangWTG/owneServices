using Enterprise.Customs.Universal;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CUS001
	{
		internal void Method(ZZRefCusCodeListAttributeCombined codeListAttribute)
		{
			//CUS001:Some RefZZ Fields Are Case Insensitive Rule
			_ = codeListAttribute.ZZE_Value == "string";

			//CUS001:Some RefZZ Fields Are Case Insensitive Rule
			_ = codeListAttribute.ZZE_ZXE_NKName == "string";
		}
	}
}
