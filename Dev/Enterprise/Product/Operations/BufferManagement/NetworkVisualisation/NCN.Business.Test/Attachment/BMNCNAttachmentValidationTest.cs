using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class BMNCNAttachmentValidationTest : BusinessObjectValidationTestCase
	{
		#region Light Validation

		public void TestShouldSupportLightValidation()
		{
			var attachment = Factory.New<BMNCNAttachment>();
			Assert(attachment is ILightValidationInternals);
		}

		#endregion
	}
}
