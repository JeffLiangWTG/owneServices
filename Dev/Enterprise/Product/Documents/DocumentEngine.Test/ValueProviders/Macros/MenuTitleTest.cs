using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(MenuTitle))]
	sealed class MenuTitleTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <Reprot Name>", !ValueProviderToTest.IsResponsibleForReplacing("<Meuu Title>", Passes.FirstPass));
			Assert("should match <MenuTitle(T)>", ValueProviderToTest.IsResponsibleForReplacing("<MenuTitle(T)>", Passes.FirstPass));
			Assert("should match <MenuTitle(TY)>", ValueProviderToTest.IsResponsibleForReplacing("<MenuTitle(TY)>", Passes.FirstPass));
			Assert("should match < report      name       >", ValueProviderToTest.IsResponsibleForReplacing("< menu          title       >", Passes.FirstPass));
			Assert("should match <MenuTitle>", ValueProviderToTest.IsResponsibleForReplacing("<MenuTitle>", Passes.FirstPass));
			Assert("should match <MenuTitle  (Y)  >", ValueProviderToTest.IsResponsibleForReplacing("<MenuTitle  (Y)  >", Passes.FirstPass));
			Assert("should match <MenuTitle(   Y   )>>", ValueProviderToTest.IsResponsibleForReplacing("<MenuTitle(   Y   )>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			SetupDocumentName();
			PrepareRenderer();
			AssertEquals("Pre Alert", ValueProviderToTest.GetReplacement("<MenuTitle>", Report));
			AssertEquals("货况预报", ValueProviderToTest.GetReplacement("<MenuTitle(Y)>", Report));
			AssertEquals("Pre Alert", ValueProviderToTest.GetReplacement("<MenuTitle(N)>", Report));
			AssertEquals("Pre Alert", ValueProviderToTest.GetReplacement("<MenuTitle(T)>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new MenuTitle();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			SetupDocumentName();
		}

		void SetupDocumentName()
		{
			Report.Parent.StmMenuCommand.SU_MenuName = "Pre Alert";
			var resKey = "SU_MenuName$UHJlIEFsZXJ0";
			mockData = Res.UseMockData();
			mockData.Put(resKey, new ResourceStringData(resKey, "货况预报"));
		}

		protected override void TearDown()
		{
			base.TearDown();
			mockData?.Dispose();
		}

		IMockResourceStringCache mockData;
	}
}
