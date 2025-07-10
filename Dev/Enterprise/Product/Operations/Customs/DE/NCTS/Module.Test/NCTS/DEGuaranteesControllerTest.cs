using Enterprise.Core;
using Enterprise.Customs.DE.NCTS.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Module.Testing
{
	[TestedType(typeof(DEGuaranteesController))]
	sealed class DEGuaranteesControllerTest : ZControllerBasherTest
	{
		public void TestFormType()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.Guarantees);
			
			using (var form = controller.ShowNewForm())
			{
				AssertType<DEGuaranteeForm>(form);
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.Guarantees;
		}

		protected override string CountryCode => Constants.CountryCodes.Germany;
	}
}
