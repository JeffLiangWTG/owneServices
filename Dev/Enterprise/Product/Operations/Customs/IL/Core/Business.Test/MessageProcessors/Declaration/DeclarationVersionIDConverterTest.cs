using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationVersionIDConverterTest : TestCaseWithFactory
	{
		public void TestToEntryHeaderVersionID()
		{
			CombineAssertions("Not valid string", () =>
			{
				AssertEquals("When null", (ZShort)0, DeclarationVersionIDConverter.ToEntryHeaderVersionID(null));
				AssertEquals("When number is not positive", (ZShort)0, DeclarationVersionIDConverter.ToEntryHeaderVersionID("-3.0"));
				AssertEquals("When number is not positive", (ZShort)0, DeclarationVersionIDConverter.ToEntryHeaderVersionID("0.-1"));
				AssertEquals("When bigger than 33.00", (ZShort)0, DeclarationVersionIDConverter.ToEntryHeaderVersionID("33.0"));
			});

			CombineAssertions("valid string", () =>
			{
				AssertEquals("When 0.0", (ZShort)0, DeclarationVersionIDConverter.ToEntryHeaderVersionID("0.0"));
				AssertEquals("When 0.1", (ZShort)100, DeclarationVersionIDConverter.ToEntryHeaderVersionID("0.1"));
				AssertEquals("When 0.11", (ZShort)110, DeclarationVersionIDConverter.ToEntryHeaderVersionID("0.11"));
				AssertEquals("When 0.999", (ZShort)999, DeclarationVersionIDConverter.ToEntryHeaderVersionID("0.999"));
				AssertEquals("When 0.200", (ZShort)200, DeclarationVersionIDConverter.ToEntryHeaderVersionID("0.200"));
				AssertEquals("When 2.0", (ZShort)2000, DeclarationVersionIDConverter.ToEntryHeaderVersionID("2.0"));
				AssertEquals("When 4.202", (ZShort)4202, DeclarationVersionIDConverter.ToEntryHeaderVersionID("4.202"));
				AssertEquals("When 31.999", (ZShort)31999, DeclarationVersionIDConverter.ToEntryHeaderVersionID("31.999"));
				AssertEquals("When 32.767", (ZShort)32767, DeclarationVersionIDConverter.ToEntryHeaderVersionID("32.767"));
			});
		}

		public void TestToDeclarationVersionID()
		{
			CombineAssertions("valid ZShort", () =>
			{
				AssertEquals("When 0", "0.0", DeclarationVersionIDConverter.ToDeclarationVersionID((ZShort)0));
				AssertEquals("When 100", "0.1", DeclarationVersionIDConverter.ToDeclarationVersionID((ZShort)100));
				AssertEquals("When 110", "0.11", DeclarationVersionIDConverter.ToDeclarationVersionID((ZShort)110));
				AssertEquals("When 999", "0.999", DeclarationVersionIDConverter.ToDeclarationVersionID((ZShort)999));
				AssertEquals("When 200", "0.2", DeclarationVersionIDConverter.ToDeclarationVersionID((ZShort)200));
				AssertEquals("When 2000", "2.0", DeclarationVersionIDConverter.ToDeclarationVersionID((ZShort)2000));
				AssertEquals("When 4202", "4.202", DeclarationVersionIDConverter.ToDeclarationVersionID((ZShort)4202));
				AssertEquals("When 31999", "31.999", DeclarationVersionIDConverter.ToDeclarationVersionID((ZShort)31999));
				AssertEquals("When 32767", "32.767", DeclarationVersionIDConverter.ToDeclarationVersionID((ZShort)32767));
			});
		}
	}
}
