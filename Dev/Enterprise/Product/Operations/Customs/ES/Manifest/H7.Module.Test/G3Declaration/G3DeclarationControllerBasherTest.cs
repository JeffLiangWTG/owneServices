using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Module.Testing
{
	[TestedType(typeof(G3DeclarationController))]
	public class G3DeclarationControllerBasherTest : ZControllerBasherTest
	{
		public override void TestNewForm()
		{
			Assert(true);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.ES.G3Declaration;

		protected override string CountryCode => Core.Constants.CountryCodes.Spain;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var g3Declaration = Factory.NewWithValidTestData<G3EDIMessage>();
			Factory.Save();
			return g3Declaration;
		}
	}
}
