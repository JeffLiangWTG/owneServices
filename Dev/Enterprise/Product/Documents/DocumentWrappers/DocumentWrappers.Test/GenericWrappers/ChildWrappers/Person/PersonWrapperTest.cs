using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PersonWrapper))]
	sealed class PersonWrapperTest : GenericWrapperTest
	{
		public void TestPicture()
		{
			GlbPerson person = Factory.New<GlbPerson>();

			using (MemoryStream stream = new MemoryStream())
			using (Bitmap image = new Bitmap(20, 10))
			{
				image.Save(stream, ImageFormat.Bmp);
				stream.Flush();
				person.PER_Picture = stream.ToArray();
			}

			PersonWrapper wrapper = new PersonWrapper(person, Factory);

			AssertNotNull("wrapper.Signature", wrapper.Picture);
			AssertEquals("wrapper.Signature.Height", 10, wrapper.Picture.Height);
			AssertEquals("wrapper.Signature.Width", 20, wrapper.Picture.Width);
		}

		public void TestPhoneNumber()
		{
			GlbPerson staff = Factory.New<GlbPerson>();

			staff.PER_MobilePhone = "+61 2 5555 9999";
			staff.PER_HomePhone = "+61 2 8888 9999";
			staff.PER_FaxNumber = "+61 2 7777 9999";

			var wrapper = new PersonWrapper(staff, Factory);
			AssertEquals("Mobile phone should be formatted", "+61 2 5555 9999", wrapper.MobilePhone);
			AssertEquals("Home phone should be formatted", "+61 2 8888 9999", wrapper.HomePhone);
			AssertEquals("Home phone should be formatted", "+61 2 7777 9999", wrapper.FaxNum);
		}

		[TestDate(2018, 5, 23)]
		public void TestBirthDate()
		{
			GlbPerson person = Factory.New<GlbPerson>();

			person.PER_BirthDate = new ZDate(2018, 05, 14);

			var wrapper = new PersonWrapper(person, Factory);
			AssertEquals("14-May-18 (0 yrs)", wrapper.BirthDate);
		}

		public void TestGender()
		{
			GlbPerson staff = Factory.New<GlbPerson>();

			staff.PER_Gender = Core.Constants.Genders.NotSpecified;

			var wrapper = new PersonWrapper(staff, Factory);
			AssertEquals("Gender should be blank", ZString.Empty, wrapper.Gender);

			staff.PER_Gender = Core.Constants.Genders.Custom;
			wrapper = new PersonWrapper(staff, Factory);
			AssertEquals("Gender should be blank", ZString.Empty, wrapper.Gender);

			staff.PER_Gender = Core.Constants.Genders.Woman;
			wrapper = new PersonWrapper(staff, Factory);
			AssertEquals("Gender should be copied normally", Core.Constants.Genders.Woman, wrapper.Gender);
		}

		public override void TestWrapperMappingsEmpty()
		{
			PersonWrapper wrapper = (PersonWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapper.FirstName", "", wrapper.FirstName);
			AssertEquals("wrapper.Title", "", wrapper.Title);
			AssertEquals("wrapper.Signature", null, wrapper.Picture);
			AssertEquals("wrapper.MobilePhone", "", wrapper.MobilePhone);
			AssertEquals("wrapper.HomePhone", "", wrapper.HomePhone);
			AssertEquals("wrapper.FaxNum", "", wrapper.FaxNum);
			AssertEquals("wrapper.EmailAddress", "", wrapper.EmailAddress);
		}

		#region Implementation

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return "Registry : (No Default Field Value Available on Registry)";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			GlbPerson person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "Test Liang";
			person.PER_EmailAddress = "jfl@blaticus.com.au";
			person.PER_FaxNumber = "02 5555 9999";
			person.PER_HomePhone = "02 8888 9999";
			person.PER_MobilePhone = "02 7777 9999";
			person.PER_BirthDate = new ZDate(2018, 05, 14);
			return new PersonWrapper(person, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Person                                       (Default Field: FullName)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Age                                     Int
BirthDate                               String
City                                    String
EmailAddress                            String
FaxNum                                  String
FirstName                               String
FullName                                String
Gender                                  String
HomePhone                               String
MobilePhone                             String
Title                                   String
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			GlbPerson person = Factory.NewWithValidTestData<GlbPerson>();
			return new PersonWrapper(person, Factory);
		}

		#endregion Implementation
	}
}
