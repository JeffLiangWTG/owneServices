using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Customs.JP.AFR.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Module.Testing
{
	[TestedType(typeof(JPAFRController))]
	class JPAFRControllerTest : ZControllerBasherTest
	{
		public void TestGetForm()
		{
			CargoWise.Common.Testing.DisposableLeakListener.Instance.StackTraceEnabled = true;
			var header = Factory.New<JPAFRHeader>();
			Factory.Save();
			var controller = new JPAFRController();
			using (var form = controller.ShowEditForm(header))
			{
				AssertEquals(typeof(JPAFRForm), form.GetType());
			}
			var consol = Factory.New<ForwardingConsol>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			Factory.Save();
			using (var form = controller.ShowEditForm(header))
			{
				AssertEquals(typeof(ConsolForm), form.GetType());
			}
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(JPAFRHeader), new JPAFRController().TypeOfTopLevelBusinessObject);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.JP.AFR;
		}

		public void TestGetNewBusinessEntityInLocalFactoryWithRightJobType()
		{
			var controller = new JPAFRController();
			var controllerInternal = (new JPAFRController()) as ZControllerInternals;

			controller.CreateVOCCAFR = true;
			using (var testForm = controller.ShowNewForm() as ZForm)
			{
				Assert((testForm.BusinessEntity as JPAFRHeader).JPH_IsShippingLineEntry);
			}
			var testHeader1 = controllerInternal.GetNewBusinessEntityInLocalFactory() as JPAFRHeader;
			AssertNotNull(testHeader1);
			Assert(!testHeader1.JPH_IsShippingLineEntry);

			controller.CreateVOCCAFR = false;
			using (var testForm = controller.ShowNewForm() as ZForm)
			{
				Assert(!(testForm.BusinessEntity as JPAFRHeader).JPH_IsShippingLineEntry);
			}
			var testHeader2 = controllerInternal.GetNewBusinessEntityInLocalFactory() as JPAFRHeader;
			AssertNotNull(testHeader2);
			Assert(!testHeader2.JPH_IsShippingLineEntry);
		}
	}
}
