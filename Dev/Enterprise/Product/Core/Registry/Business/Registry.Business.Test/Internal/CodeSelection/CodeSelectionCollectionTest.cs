using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeSelectionCollection))]
	sealed class CodeSelectionCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CodeSelectionCollection>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override CodeSelectionCollection GetCollectionToTest()
		{
			return new CodeSelectionCollection(CodeSelectionTest.GetCodesProviderForTesting());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CodeSelection();
		}

		public void TestVeryLargeHashCode()
		{
			var codesProvider = new CodeDescriptionPairListProvider(() =>
			{
				var codes = new CodeDescriptionPairList();
				for (int i = 0; i < 100; ++i)
				{
					codes.AddPair(new string(new char[] { (char)('A' + i / 10), (char)('A' + i % 10) }), "test");
				}
				return codes;
			});
			var a = new CodeSelectionCollection(codesProvider);
			for (int i = 0; i < 100; ++i)
			{
				a.AddNew().Code = new string(new char[] { (char)('A' + i / 10), (char)('A' + i % 10) });
			}
			AssertNoExceptionThrown(() => a.GetHashCode());
		}

		public void TestEquals()
		{
			var a = new CodeSelectionCollection(CodeSelectionTest.GetCodesProviderForTesting());
			var b = new CodeSelectionCollection(CodeSelectionTest.GetCodesProviderForTesting());
			Assert(a.Equals(a));
			Assert(b.Equals(b));
			Assert(a.Equals(b));
			Assert(b.Equals(a));
			Assert(a == b);
			Assert(b == a);

			a.AddNew().Code = "GRE";
			Assert(a.Equals(a));
			Assert(b.Equals(b));
			Assert(!a.Equals(b));
			Assert(!b.Equals(a));
			Assert(a != b);
			Assert(b != a);

			var item = b.AddNew();
			item.Code = "RIF";
			Assert(a.Equals(a));
			Assert(b.Equals(b));
			Assert(!a.Equals(b));
			Assert(!b.Equals(a));
			Assert(a != b);
			Assert(b != a);

			item.Code = "GRE";
			Assert(a.Equals(a));
			Assert(b.Equals(b));
			Assert(a.Equals(b));
			Assert(b.Equals(a));
			Assert(a == b);
			Assert(b == a);

			b.AddNew().Code = "GRE";
			Assert(a.Equals(a));
			Assert(b.Equals(b));
			Assert(!a.Equals(b));
			Assert(!b.Equals(a));
			Assert(a != b);
			Assert(b != a);

			CodeSelectionCollection c = null;
			CodeSelectionCollection d = null;
			Assert(c == d);
			Assert(c == null);
			Assert(null == c);
			Assert(a != c);
			Assert(a != null);
			Assert(null != a);

			Assert(!a.Equals(c));
			Assert(!a.Equals(null));
		}
	}
}
