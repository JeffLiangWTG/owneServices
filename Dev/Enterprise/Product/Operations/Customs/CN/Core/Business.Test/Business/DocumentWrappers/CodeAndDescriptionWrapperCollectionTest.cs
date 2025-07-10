using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CodeAndDescriptionWrapperCollection))]
	class CodeAndDescriptionWrapperCollectionTest : GenericWrapperCollectionTest<CodeAndDescriptionWrapperCollection>
	{
		public void TestStringIndex()
		{
			var codes = new[] { new ZString("B01"), new ZString("B02") };
			var collection = CodeAndDescriptionWrapperCollection.New(codes, Factory.GetCachedValue<SpecialBusinessList>(), Factory);
			AssertEquals("B01 - 国际赛事", collection["B01"].CodeAndDescription);
			AssertEquals("B02 - 特殊进出军工物资", collection[1].CodeAndDescription);
			AssertNull("Does not contain B03", collection["B03"]);
		}

		public void TestCreateCodeAndDescriptionWrapperCollection()
		{
			var codes = new[] { new ZString("1"), new ZString("2"), new ZString("3") };
			var collection = CodeAndDescriptionWrapperCollection.New(codes, Factory.GetCachedValue<FeeMarkTypeList>(), Factory);
			AssertEquals("1", collection[0].Code);
			AssertEquals("2", collection[1].Code);
			AssertEquals("3", collection[2].Code);
			AssertEquals("费率", collection[0].Description);
			AssertEquals("单价", collection[1].Description);
			AssertEquals("总价", collection[2].Description);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => DefaultCollection.AddNew();

		protected override GenericWrapper GetNewWrapperToAddToTheCollection() => CodeAndDescriptionWrapper.New("", "", Factory);

		protected override CodeAndDescriptionWrapperCollection GetNewDocumentWrapperCollection() => DefaultCollection;

		CodeAndDescriptionWrapperCollection DefaultCollection => fDefaultCollection ?? (fDefaultCollection = CodeAndDescriptionWrapperCollection.New(Enumerable.Empty<ZString>(), Factory.GetCachedValue<FeeMarkTypeList>(), Factory));
		CodeAndDescriptionWrapperCollection fDefaultCollection;
	}
}
