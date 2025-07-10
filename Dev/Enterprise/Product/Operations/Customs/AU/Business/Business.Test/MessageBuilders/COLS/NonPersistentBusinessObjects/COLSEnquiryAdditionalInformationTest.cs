using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(COLSEnquiryAdditionalInformation))]
	sealed class COLSEnquiryAdditionalInformationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultContactDetails()
		{
			colsHeader.ResponsibleParty.E2_Contact = "Contact name";
			colsHeader.ResponsibleParty.Address.OA_Phone = "+61 2 9744 8000";
			colsHeader.ResponsibleParty.Address.OA_Mobile = "+61 415 923 947";
			colsHeader.ResponsibleParty.Address.OA_Email = "user@testcompany.com";
			var additionalInfo = new COLSEnquiryAdditionalInformation(colsHeader);
			additionalInfo.DefaultContactDetails = true;
			CombineAssertions("Contact Details is default to Responsible Party", () =>
			{
				AssertEquals("Name", "Contact name", additionalInfo.ContactName);
				AssertEquals("Phone", "+61297448000", additionalInfo.ContactPhone);
				AssertEquals("Email", "user@testcompany.com", additionalInfo.ContactEmail);
			});

			additionalInfo.DefaultContactDetails = false;
			CombineAssertions("Contact Details is default to blank", () =>
			{
				AssertEquals("Name", string.Empty, additionalInfo.ContactName);
				AssertEquals("Phone", string.Empty, additionalInfo.ContactPhone);
				AssertEquals("Email", string.Empty, additionalInfo.ContactEmail);
			});

			colsHeader.ResponsibleParty.Address.OA_Phone = ZString.Empty;
			additionalInfo.DefaultContactDetails = true;
			AssertEquals("Should get value from mobile number if phone number is empty", "+61415923947", additionalInfo.ContactPhone);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new COLSEnquiryAdditionalInformation(colsHeader);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
		}
		QuarantineColsHeader colsHeader;
	}
}
