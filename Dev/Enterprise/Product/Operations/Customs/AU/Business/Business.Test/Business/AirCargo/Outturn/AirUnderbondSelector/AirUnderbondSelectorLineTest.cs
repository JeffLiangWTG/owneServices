using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AirUnderbondSelectorLine))]
	sealed class AirUnderbondSelectorLineTest : UnderbondSelectorLineTest
	{
		protected override UnderbondSelectorLine GetUnderbondSelectorLine(CusUnderbond cusUnderbond) => new AirUnderbondSelectorLine(cusUnderbond);
	}
}
