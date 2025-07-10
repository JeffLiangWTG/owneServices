using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(ConsolidatedDeclarationController))]
	sealed class ConsolidatedDeclarationControllerTest : Customs.Module.Testing.ConsolidatedDeclarationControllerTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
			Factory.Save();
			return consolidatedDeclaration;
		}

		public override void TestEditForm()
		{
			using (var myForm = Controller.ShowEditForm(GetBusinessObjectThatIsInTheDatabase()) as ConsolidatedDeclarationForm)
			{
				AssertNotEquals("New form should be of type ConsolidatedDeclarationForm", null, myForm);
			}
		}
	}
}
