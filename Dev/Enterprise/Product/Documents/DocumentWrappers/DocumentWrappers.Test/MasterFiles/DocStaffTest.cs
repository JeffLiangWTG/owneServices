using System;
using System.Drawing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocStaff))]
	public class DocStaffTest : DocumentWrapperTestCase
	{
		#region Signature

		public void TestSignature()
		{
			var documentUsageReporter = new DocumentUsageReporter();
			Factory.ServiceContainer.AddService(new DocumentUsageDetailsCollector(documentUsageReporter));
			AssertNull(GlbStaffWrapper.Signature);
			AssertEquals("IsUserSignatureUsed", false, documentUsageReporter.IsUserSignatureUsed);

			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var memStream = resourceRetriever.GetBytes("Enterprise.DocumentWrappers.Testing.AWBMasterTitle.png");
			Staff.GS_UserSignature = memStream;

			AssertNotNull(GlbStaffWrapper.Signature);
			Assert("IsUserSignatureUsed", documentUsageReporter.IsUserSignatureUsed);
		}

		public void TestIsSignatureSpecified()
		{
			AssertNull(GlbStaffWrapper.Signature);
			Assert(!GlbStaffWrapper.IsSignatureSpecified);

			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var memStream = resourceRetriever.GetBytes("Enterprise.DocumentWrappers.Testing.AWBMasterTitle.png");
			Staff.GS_UserSignature = memStream;
			AssertNotNull(GlbStaffWrapper.Signature);
			Assert(GlbStaffWrapper.IsSignatureSpecified);
		}

		public void TestPrintSignatureOnQuotationDocuments()
		{
			AssertNull(GlbStaffWrapper.PrintSignatureOnQuoteDocuments);

			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var memStream = resourceRetriever.GetBytes("Enterprise.DocumentWrappers.Testing.AWBMasterTitle.png");
			Staff.GS_UserSignature = memStream;
			if (Env.Registry.Rating.PrintScannedUserSignatureOnQuotationDocuments)
			{
				AssertNotNull(GlbStaffWrapper.PrintSignatureOnQuoteDocuments);
			}
			else
			{
				AssertNull(GlbStaffWrapper.PrintSignatureOnQuoteDocuments);
			}
		}

		#endregion

		#region Properties

		public void TestPhoneNumbersAreFormatted()
		{
			Staff.GS_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			Staff.GS_EmergencyHomePhone = "0201010101";
			AssertEquals("Emergency Home Phone should be formatted", "+61 2 0101 0101", GlbStaffWrapper.EmergencyHomePhone);

			Staff.GS_EmergencyWorkPhone = "0202020202";
			AssertEquals("Emergency Work Phone should be formatted", "+61 2 0202 0202", GlbStaffWrapper.EmergencyWorkPhone);

			Staff.GS_FaxNum = "0203030303";
			AssertEquals("Fax Num should be formatted", "+61 2 0303 0303", GlbStaffWrapper.FaxNum);

			Staff.GS_HomePhone = "0204040404";
			AssertEquals("Home Phone should be formatted", "+61 2 0404 0404", GlbStaffWrapper.HomePhone);

			Staff.GS_MobilePhone = "0205050505";
			AssertEquals("Mobile Phone should be formatted", "+61 2 0505 0505", GlbStaffWrapper.MobilePhone);

			Staff.GS_NextOfKinHomePhone = "0206060606";
			AssertEquals("Next Of Kin Home Phone should be formatted", "+61 2 0606 0606", GlbStaffWrapper.NextOfKinHomePhone);

			Staff.GS_NextOfKinWorkPhone = "0207070707";
			AssertEquals("Next Of Kin Work Phone should be formatted", "+61 2 0707 0707", GlbStaffWrapper.NextOfKinWorkPhone);

			Staff.GS_WorkPhone = "0208080808";
			AssertEquals("Work Phone should be formatted", "+61 2 0808 0808", GlbStaffWrapper.WorkPhone);
		}

		public void TestPublishStaffDetails()
		{
			Staff.GS_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			Staff.GS_PublishHomePhone = true;
			Staff.GS_HomePhone = "0223456789";
			AssertEquals("Home Phone should be published", "+61 2 2345 6789", GlbStaffWrapper.PublishHomePhone);
			Staff.GS_PublishHomePhone = false;
			AssertEquals("Home Phone should not be published", ZString.Empty, GlbStaffWrapper.PublishHomePhone);

			Staff.GS_PublishMobilePhone = true;
			Staff.GS_MobilePhone = "0233456789";
			AssertEquals("Mobile Phone should be published", "+61 2 3345 6789", GlbStaffWrapper.PublishMobilePhone);
			Staff.GS_PublishMobilePhone = false;
			AssertEquals("Mobile Phone should not be published", ZString.Empty, GlbStaffWrapper.PublishMobilePhone);

			Staff.GS_PublishWorkPhone = true;
			Staff.GS_WorkPhone = "0243456789";
			AssertEquals("Work Phone should be published", "+61 2 4345 6789", GlbStaffWrapper.PublishWorkPhone);
			Staff.GS_PublishWorkPhone = false;
			AssertEquals("Work Phone should be not published", ZString.Empty, GlbStaffWrapper.PublishWorkPhone);

			Staff.GS_PublishWorkExtension = true;
			Staff.GS_WorkExtension = "123";
			AssertEquals("Work Extension should be published", "123", GlbStaffWrapper.PublishWorkExtension);
			Staff.GS_PublishWorkExtension = false;
			AssertEquals("Work Extension should be not published", ZString.Empty, GlbStaffWrapper.PublishWorkExtension);

			Staff.GS_PublishFaxNum = true;
			Staff.GS_FaxNum = "0253456789";
			AssertEquals("Fax num should be published", "+61 2 5345 6789", GlbStaffWrapper.PublishFaxNum);
			Staff.GS_PublishFaxNum = false;
			AssertEquals("Fax num should be not published", ZString.Empty, GlbStaffWrapper.PublishFaxNum);

			Staff.GS_PublishEmailAddress = true;
			Staff.GS_EmailAddress = "email@domain.com";
			AssertEquals("Email Address should be published", "email@domain.com", GlbStaffWrapper.PublishEmailAddress);
			Staff.GS_PublishEmailAddress = false;
			AssertEquals("Email Address should be not published", ZString.Empty, GlbStaffWrapper.PublishEmailAddress);
		}

		public void TestCity()
		{
			Staff.GS_City = "DUCKSVILLE";
			AssertEquals("DUCKSVILLE", GlbStaffWrapper.City);
		}

		public void TestFullName()
		{
			Staff.GS_FullName = "Mister Duckey";
			AssertEquals("Mister Duckey", GlbStaffWrapper.FullName);
		}

		public void TestChangePasswordAtNextLogin()
		{
			Staff.GS_ChangePasswordAtNextLogin = ZBool.True;
			Assert(GlbStaffWrapper.ChangePasswordAtNextLogin);
			Staff.GS_ChangePasswordAtNextLogin = ZBool.False;
			AssertEquals(ZBool.False, GlbStaffWrapper.ChangePasswordAtNextLogin);
		}

		public void TestSignatureEncoded()
		{
			Staff.SignatureImage = null;
			AssertEquals("", GlbStaffWrapper.SignatureEncoded);

			Staff.SignatureImage = new Bitmap(1, 1);
			var base64String = Convert.ToBase64String(Staff.GS_UserSignature);
			AssertEquals(base64String, GlbStaffWrapper.SignatureEncoded);
		}

		public void TestProfilePhotoEncoded()
		{
			Staff.ProfileImage = null;
			AssertEquals("", GlbStaffWrapper.ProfilePhotoEncoded);

			Staff.ProfileImage = new Bitmap(2, 2);
			var base64String = Convert.ToBase64String(Staff.GS_ProfilePhoto);
			AssertEquals(base64String, GlbStaffWrapper.ProfilePhotoEncoded);
		}

		#endregion

		#region Implementation

		GlbStaff Staff;
		DocStaff GlbStaffWrapper;

		protected override void SetUp()
		{
			Staff = Factory.New<GlbStaff>();
			GlbStaffWrapper = DocStaff.New(Staff, Factory);
			base.SetUp();
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocStaff.New(Staff, Factory) };
		}

		#endregion
	}
}
