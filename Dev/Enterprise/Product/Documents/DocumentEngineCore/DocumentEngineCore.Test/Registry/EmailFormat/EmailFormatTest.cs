using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(EmailFormat))]
	public class EmailFormatTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestFields()
		{
			AssertNotNull(BizObj.EmailSubjectFields);
			AssertNotNull(BizObj.EmailSignatureFields);

			//default values
			EmailFieldCollection collection = new EmailFieldCollection();
			collection.Add(new EmailSubjectField("1", Core.Constants.EmailFormat.EmailFieldCodes.CompanyBrandName));
			collection.Add(new EmailSubjectField("2", Core.Constants.EmailFormat.EmailFieldCodes.BranchName));
			collection.Add(new EmailSubjectField("3", Core.Constants.EmailFormat.EmailFieldCodes.DocumentName));
			AssertDefaultCollection(collection, BizObj.EmailSubjectFields);

			collection = new EmailFieldCollection();
			collection.Add(new EmailSignatureField("1", Core.Constants.EmailFormat.EmailFieldCodes.CompanyBrandName));
			collection.Add(new EmailSignatureField("2", Core.Constants.EmailFormat.EmailFieldCodes.BranchName));
			collection.Add(new EmailSignatureField("3", Core.Constants.EmailFormat.EmailFieldCodes.BranchAddress));
			collection.Add(new EmailSignatureField("4", Core.Constants.EmailFormat.EmailFieldCodes.BranchPhone));
			collection.Add(new EmailSignatureField("5", Core.Constants.EmailFormat.EmailFieldCodes.BranchFax));
			AssertDefaultCollection(collection, BizObj.EmailSignatureFields);
		}

		public void TestEmailSubjectFieldsCannotBeEmpty()
		{
			var emailSubject = new EmailFormat();
			emailSubject.Separator = "#";
			emailSubject.RunPreSaveValidation();
			AssertEquals(3, emailSubject.EmailSubjectFields.Count);
			Assert(!emailSubject.HasErrors);

			emailSubject.EmailSubjectFields.RemoveAll();
			emailSubject.RunPreSaveValidation();
			Assert(emailSubject.HasErrors);

			var element = emailSubject.EmailSubjectFields.AddNew();
			element.Code = "";
			emailSubject.RunPreSaveValidation();
			Assert(emailSubject.HasErrors);

			element.Code = "Company Name";
			emailSubject.RunPreSaveValidation();
			Assert(emailSubject.HasErrors);
		}

		public void TestRunPreSaveValidation()
		{
			BizObj.Separator = "";

			BizObj.ClearAllNotifications();
			AssertNoErrors("Precondition: There should not be any errors.", BizObj);

			BizObj.RunPreSaveValidation();
			AssertHasErrors("RunPreSaveValidation() should have validated Separator.", BizObj.SeparatorInfo);
		}

		public void TestDisclaimerMaxLength()
		{
			var email = new EmailFormat();
			AssertEquals("Email Disclaimer MaxLength should be 2000.", 2000, email.DisclaimerInfo.MaxLength);
		}

		#region Implementation

		void AssertDefaultCollection(EmailFieldCollection collection1, EmailFieldCollection collection2)
		{
			AssertEquals(collection1.Count, collection2.Count);

			for (int i = 0; i < collection1.Count; i++)
			{
				AssertEquals(collection1[i].Code, collection2[i].Code);
			}
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			BizObj.Separator = "#";
			BizObj.Disclaimer = "blah blah";

			EmailSubjectField subjectField = new EmailSubjectField();
			subjectField.Index = "1";
			subjectField.Code = Core.Constants.EmailFormat.EmailFieldCodes.CompanyBrandName;
			BizObj.EmailSubjectFields.Add(subjectField);

			EmailSignatureField signatureField = new EmailSignatureField();
			signatureField.Index = "2";
			signatureField.Code = Core.Constants.EmailFormat.EmailFieldCodes.BranchName;
			BizObj.EmailSignatureFields.Add(signatureField);

			return BizObj;
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new EmailFormat BizObj
		{
			get { return (EmailFormat)base.BizObj; }
		}

		#endregion
	}
}
