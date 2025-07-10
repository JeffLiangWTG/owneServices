using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CusAttachmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Code()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var attachments = instruction.Attachments;
			var attachment = attachments.CreateNewCusAttachment();
			attachment.CY_Code = ZString.Empty;
			AssertNoNotifications(attachment.CY_CodeInfo);
			attachment.CY_Code = "@#$";
			AssertNoNotifications(attachment.CY_CodeInfo);
		}
	}
}
