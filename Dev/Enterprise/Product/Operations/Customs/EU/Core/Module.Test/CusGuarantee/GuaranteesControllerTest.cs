using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Guarantees;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(GuaranteesController))]
	class GuaranteesControllerTest : Customs.Module.Testing.GuaranteesControllerBasherTest
	{
		public void TestGetForm_ReturnType()
		{
			var guarantee = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			Factory.Save();
			using (var form = Controller.ShowEditForm(guarantee))
			{
				CombineAssertions(() =>
				{
					AssertType<GuaranteeForm>(form);

					var guaranteeForm = form as GuaranteeForm;

					var subForm = guaranteeForm.FindSingle<GuaranteeTransactionFilterControl>("GuaranteeTransactionFilterControl");
					var filterBusiness = subForm.FilterBusinessObject;

					AssertNotNull("The filter object for EU exist", filterBusiness);
					AssertType<GUI.GuaranteeTransactionFilterStripBusinessObject>(filterBusiness);
				});
			}
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.Guarantees;
	}
}
