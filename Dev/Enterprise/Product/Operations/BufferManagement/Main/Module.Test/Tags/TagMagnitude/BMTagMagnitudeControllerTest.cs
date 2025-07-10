using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMTagMagnitudeController))]
	class BMTagMagnitudeControllerTest : BMControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.BMTagMagnitude;
		}

		public void TestGetForm()
		{
			using (var form = new BMTagMagnitudeController().ShowFormForNewEntity(Factory.NewWithValidTestData<TagMagnitude>()))
			{
				AssertNotNull(form);
			}
		}

		public void TestGetFormShouldThrow_WhenBusinessEntityIsNotTagMagnitude()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			var controller = new BMTagMagnitudeController();

			AssertExceptionThrown("Should throw", typeof(ModuleGuiNotSupportedException), "Trying to open TagDefinitionForm for a business object which is not a tag. Object type: CargoWise.EntityFramework.Testing.DummyBusinessObject.", () => controller.ShowFormForNewEntity(dummy));
		}

		public void TestGetFormShouldThrow_WhenThereIsNoTagGroup()
		{
			var magnitude = Factory.NewWithValidTestData<TagMagnitude>();
			magnitude.TGM_TGD_Tag = ZGuid.ParseSafe("30c646a3-dc7a-4d32-b044-42336b082af6");
			var controller = new BMTagMagnitudeController();

			AssertExceptionThrown("Should throw", typeof(ModuleGuiNotSupportedException), "We failed to find the Tag Group for this Tag Magnitude. Tag group PK: 30c646a3-dc7a-4d32-b044-42336b082af6.", () => controller.ShowFormForNewEntity(magnitude));
		}

		public override void TestNewForm()
		{
			AssertControllerNotNull();
			var controller = new BMTagMagnitudeController();
			AssertNoExceptionThrown(() => controller.ShowNewForm());
			AssertEquals("New tags cannot be created here. Please use the Tag Groups module to create new tags.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}
	}
}
