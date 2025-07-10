using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(HarmonisedCodeWrapperCollection))]
	sealed class HarmonisedCodeWrapperCollectionTest : Base.Testing.GenericWrapperCollectionTest<HarmonisedCodeWrapperCollection>
	{
		public void TestLoadFromPackLine()
		{
			var packLine = Factory.New<PackLine>();
			var collection = new HarmonisedCodeWrapperCollection(packLine.HarmonisedCodes, Factory);

			AssertEquals("collection.Count", 0, collection.Count);

			var hc1 = packLine.HarmonisedCodes.AddNew();
			hc1.JLH_RN_NKCountry = "DE";
			hc1.JLH_Code = "1234";
			collection = new HarmonisedCodeWrapperCollection(packLine.HarmonisedCodes, Factory);
			AssertEquals("collection.Count", 1, collection.Count);

			var hc2 = packLine.HarmonisedCodes.AddNew();
			hc2.JLH_RN_NKCountry = "FR";
			hc2.JLH_Code = "8888";
			collection = new HarmonisedCodeWrapperCollection(packLine.HarmonisedCodes, Factory);
			AssertEquals("collection.Count", 2, collection.Count);
		}

		protected override HarmonisedCodeWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new HarmonisedCodeWrapperCollection(Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			var packLine = Factory.New<PackLine>();
			var hc = packLine.HarmonisedCodes.AddNew();
			return new HarmonisedCodeWrapper(hc, Factory);
		}
	}
}
