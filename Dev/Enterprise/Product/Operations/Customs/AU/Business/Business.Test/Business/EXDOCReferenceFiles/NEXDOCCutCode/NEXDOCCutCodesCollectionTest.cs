using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(NEXDOCCutCodesCollection))]
	sealed class NEXDOCCutCodesCollectionTest : EXDOCRefCodeCollectionTest<NEXDOCCutCodesCollection>
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			helper.Header1.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			return new NEXDOCCutCodesCollection(helper.Header1.QuarantineExDocHeader);
		}
	}
}
