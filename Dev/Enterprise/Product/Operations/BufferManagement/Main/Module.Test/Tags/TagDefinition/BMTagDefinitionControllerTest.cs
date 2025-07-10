using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMTagDefinitionController))]
	class BMTagDefinitionControllerTest : BMControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.BMTagDefinition;
		}

		public void TestGetForm()
		{
			using (var form = new BMTagDefinitionController().ShowFormForNewEntity(Factory.NewWithValidTestData<TagDefinition>()))
			{
				AssertNotNull(form);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}
	}
}
