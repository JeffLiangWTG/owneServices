using NUnit.Framework;

namespace Enterprise.Customs.MY.Business.Testing
{
	[TestedType(typeof(CusEntryHeader))]
	class CusEntryHeaderTest : Customs.Business.Testing.CusEntryHeaderTest
	{
		protected override bool RatesAreReciprocal => true;
	}
}
