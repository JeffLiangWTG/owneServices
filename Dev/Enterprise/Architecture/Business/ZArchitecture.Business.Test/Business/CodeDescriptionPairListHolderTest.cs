using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class CodeDescriptionPairListHolderTest : TestCase
	{
		public void TestSameListReturnedBySameKey()
		{
			CodeDescriptionPairListHolder holder = new CodeDescriptionPairListHolder();
			CodeDescriptionPairList x = new CodeDescriptionPairList();
			holder["key"] = x;
			AssertEquals(x, holder["key"]);
		}

		public void TestIndexByZQuery()
		{
			CodeDescriptionPairListHolder holder = new CodeDescriptionPairListHolder();
			CodeDescriptionPairList x = new CodeDescriptionPairList();
			ZQuery filter1 = new ZQuery(DummyBizoSchema.Z0_Code, "Code1");
			holder[filter1] = x;
			AssertEquals(x, holder[filter1]);

			CodeDescriptionPairList y = new CodeDescriptionPairList();
			ZQuery filter2 = new ZQuery(DummyBizoSchema.Z0_Code, "Code2");
			holder[filter2] = y;
			AssertEquals(y, holder[filter2]);
		}
	}
}
