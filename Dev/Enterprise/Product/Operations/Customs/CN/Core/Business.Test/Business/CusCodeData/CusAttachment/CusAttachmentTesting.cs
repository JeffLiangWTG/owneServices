using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CusAttachment))]
	class CusAttachmentTesting : CusCodeDataTest<CusAttachment>
	{
		public void TestSupportsNotes()
		{
			AssertEquals("SupportsNotes should be false", false, ((CusAttachment)BusinessObject).SupportsNotes);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var attachments = instruction.Attachments;
			var attachment = attachments.CreateNewCusAttachment();
			attachment.CY_Code = CSDDocTypeList.Codes._10000002;
			attachment.CY_Data = "123456789012";
			return attachment;
		}

		public void TestDefaultValues()
		{
			var attachment = (CusAttachment)GetNewBusinessObject();
			AssertEquals(Constants.CusCodeDataTypes.Codes.CusAttachment, attachment.CY_Type);
		}

		public void TestGetNewValidation()
		{
			var attachment = (CusAttachment)GetNewBusinessObject();
			AssertType<CusAttachmentValidation>(attachment.Validation);
		}

		public void TestAddNewAndExists()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			CusAttachment.AddNew(instruction, CSDDocTypeList.Codes._10000001, "code1");
			Assert(CusAttachment.Exists(instruction, CSDDocTypeList.Codes._10000001, "code1"));
			Assert(!CusAttachment.Exists(instruction, CSDDocTypeList.Codes._10000001, "code2"));
			Assert(!CusAttachment.Exists(instruction, CSDDocTypeList.Codes._10000002, "code1"));
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			CusAttachment.AddNew(anotherFactory.Load<CusEntryInstruction>(instruction.PK), CSDDocTypeList.Codes._10000002, "code2");
			anotherFactory.Save();

			Assert(CusAttachment.Exists(instruction, CSDDocTypeList.Codes._10000002, "code2"));
		}
	}
}
