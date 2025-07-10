using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(StaffWrapper))]
	sealed class StaffWrapperTest : GenericWrapperTest
	{
		public void TestSignature()
		{
			var staff = Factory.New<GlbStaff>();
			var documentUsageReporter = new DocumentUsageReporter();
			Factory.ServiceContainer.AddService(new DocumentUsageDetailsCollector(documentUsageReporter));

			var wrapper = new StaffWrapper(staff, Factory);
			AssertNull("wrapper.Signature", wrapper.Signature);
			AssertEquals("IsUserSignatureUsed", false, documentUsageReporter.IsUserSignatureUsed);

			using (var stream = new MemoryStream())
			using (var image = new Bitmap(20, 10))
			{
				image.Save(stream, ImageFormat.Bmp);
				stream.Flush();
				staff.GS_UserSignature = stream.ToArray();
			}

			wrapper = new StaffWrapper(staff, Factory);
			AssertNotNull("wrapper.Signature", wrapper.Signature);
			Assert("IsUserSignatureUsed", documentUsageReporter.IsUserSignatureUsed);

			AssertEquals("wrapper.Signature.Height", 10, wrapper.Signature.Height);
			AssertEquals("wrapper.Signature.Width", 20, wrapper.Signature.Width);
		}

		public void TestWorkPhone()
		{
			StaffWrapper wrapper;
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			staff.GS_WorkPhone = "02 5555 9999";
			staff.GS_PublishWorkPhone = true;

			wrapper = new StaffWrapper(staff, Factory);
			AssertEquals("+61 2 5555 9999", wrapper.WorkPhone.Internal);
			AssertEquals(true, wrapper.WorkPhone.IsPublished);

			staff.GS_PublishWorkPhone = false;
			wrapper = new StaffWrapper(staff, Factory);
			AssertEquals("+61 2 5555 9999", wrapper.WorkPhone.Internal);
			AssertEquals(false, wrapper.WorkPhone.IsPublished);
		}

		public void TestWorkExtension()
		{
			StaffWrapper wrapper;
			GlbStaff staff = Factory.New<GlbStaff>();

			staff.GS_WorkExtension = "WorkExt";
			staff.GS_PublishWorkExtension = true;

			wrapper = new StaffWrapper(staff, Factory);
			AssertEquals("WorkExt", wrapper.WorkExtension.Internal);
			AssertEquals(true, wrapper.WorkExtension.IsPublished);

			staff.GS_PublishWorkExtension = false;
			wrapper = new StaffWrapper(staff, Factory);
			AssertEquals("WorkExt", wrapper.WorkExtension.Internal);
			AssertEquals(false, wrapper.WorkExtension.IsPublished);
		}

		public void TestMobilePhone()
		{
			StaffWrapper wrapper;
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			staff.GS_MobilePhone = "0420 019 999";
			staff.GS_PublishMobilePhone = true;

			wrapper = new StaffWrapper(staff, Factory);
			AssertEquals("+61 420 019 999", wrapper.MobilePhone.Internal);
			AssertEquals(true, wrapper.MobilePhone.IsPublished);

			staff.GS_PublishMobilePhone = false;
			wrapper = new StaffWrapper(staff, Factory);
			AssertEquals("+61 420 019 999", wrapper.MobilePhone.Internal);
			AssertEquals(false, wrapper.MobilePhone.IsPublished);
		}

		public void TestHomePhone()
		{
			StaffWrapper wrapper;
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			staff.GS_HomePhone = "02 5555 9999";
			staff.GS_PublishHomePhone = true;

			wrapper = new StaffWrapper(staff, Factory);
			AssertEquals("+61 2 5555 9999", wrapper.HomePhone.Internal);
			AssertEquals(true, wrapper.HomePhone.IsPublished);

			staff.GS_PublishHomePhone = false;
			wrapper = new StaffWrapper(staff, Factory);
			AssertEquals("+61 2 5555 9999", wrapper.HomePhone.Internal);
			AssertEquals(false, wrapper.HomePhone.IsPublished);
		}

		public void TestFaxNum()
		{
			StaffWrapper wrapper;
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			staff.GS_FaxNum = "02 5555 9999";
			staff.GS_PublishFaxNum = true;

			wrapper = new StaffWrapper(staff, Factory);
			AssertEquals("+61 2 5555 9999", wrapper.FaxNum.Internal);
			AssertEquals(true, wrapper.FaxNum.IsPublished);

			staff.GS_PublishFaxNum = false;
			wrapper = new StaffWrapper(staff, Factory);
			AssertEquals("+61 2 5555 9999", wrapper.FaxNum.Internal);
			AssertEquals(false, wrapper.FaxNum.IsPublished);
		}

		public void TestEmailAddress()
		{
			StaffWrapper wrapper;
			GlbStaff staff = Factory.New<GlbStaff>();

			staff.GS_EmailAddress = "EmailAddress";
			staff.GS_PublishEmailAddress = true;

			wrapper = new StaffWrapper(staff, Factory);
			AssertEquals("EmailAddress", wrapper.EmailAddress.Internal);
			AssertEquals(true, wrapper.EmailAddress.IsPublished);

			staff.GS_PublishEmailAddress = false;
			wrapper = new StaffWrapper(staff, Factory);
			AssertEquals("EmailAddress", wrapper.EmailAddress.Internal);
			AssertEquals(false, wrapper.EmailAddress.IsPublished);
		}

		public override void TestWrapperMappingsEmpty()
		{
			StaffWrapper wrapper = (StaffWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapper.FullName", "", wrapper.FullName);
			AssertEquals("wrapper.FirstName", "", wrapper.FirstName);
			AssertEquals("wrapper.Title", "", wrapper.Title);
			AssertEquals("wrapper.Signature", null, wrapper.Signature);
			AssertEquals("wrapper.SignatureForQuoteDocuments", null, wrapper.Signature);
			AssertEquals("wrapper.WorkPhone.Internal", "", wrapper.WorkPhone.Internal);
			AssertEquals("wrapper.WorkPhone.IsPublished", false, wrapper.WorkPhone.IsPublished);
			AssertEquals("wrapper.WorkExtension.Internal", "", wrapper.WorkExtension.Internal);
			AssertEquals("wrapper.WorkExtension.IsPublished", false, wrapper.WorkExtension.IsPublished);
			AssertEquals("wrapper.MobilePhone.Internal", "", wrapper.MobilePhone.Internal);
			AssertEquals("wrapper.MobilePhone.IsPublished", false, wrapper.MobilePhone.IsPublished);
			AssertEquals("wrapper.HomePhone.Internal", "", wrapper.HomePhone.Internal);
			AssertEquals("wrapper.HomePhone.IsPublished", false, wrapper.HomePhone.IsPublished);
			AssertEquals("wrapper.FaxNum.Internal", "", wrapper.FaxNum.Internal);
			AssertEquals("wrapper.FaxNum.IsPublished", false, wrapper.FaxNum.IsPublished);
			AssertEquals("wrapper.EmailAddress.Internal", "", wrapper.EmailAddress.Internal);
			AssertEquals("wrapper.EmailAddress.IsPublished", true, wrapper.EmailAddress.IsPublished);
		}

		#region Implementation

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
EmailAddress : bob@blaticus.org
FaxNum : 4567
HomePhone : 1234
MobilePhone : 3456
Registry : (No Default Field Value Available on Registry)
WorkExtension : 9
WorkPhone : 2345
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Bob Blaticus";
			staff.GS_EmailAddress = "bob@blaticus.org";
			staff.GS_PublishEmailAddress = true;
			staff.GS_FaxNum = "4567";
			staff.GS_PublishFaxNum = true;
			staff.GS_HomePhone = "1234";
			staff.GS_PublishHomePhone = true;
			staff.GS_MobilePhone = "3456";
			staff.GS_PublishMobilePhone = true;
			staff.GS_WorkPhone = "2345";
			staff.GS_PublishWorkPhone = true;
			staff.GS_WorkExtension = "9";
			staff.GS_PublishWorkExtension = true;
			return new StaffWrapper(staff, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
StaffMember                                  (Default Field: FullName)
======================================================================
Name                                    Type
----------------------------------------------------------------------
EmailAddress                            SecureDetail
FaxNum                                  SecureDetail
HomePhone                               SecureDetail
MobilePhone                             SecureDetail
WorkExtension                           SecureDetail
WorkPhone                               SecureDetail
FirstName                               String
FullName                                String
Title                                   String
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			return new StaffWrapper(staff, Factory);
		}

		#endregion
	}
}
