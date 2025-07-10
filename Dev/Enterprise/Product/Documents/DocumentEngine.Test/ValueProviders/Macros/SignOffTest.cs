using System;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(SignOff))]
	sealed class SignOffTest : ValueProviderTest
	{
		public void TestMacroWorksWithNullEnvironment()
		{
			RunInNullEnvironment(() =>
				AssertEquals("Replaced Result when User is Null", BrandingFactory.Instance.ProductName, ValueProviderToTest.GetReplacement("<SignOff>", Report))
			);
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < SignOff >", ValueProviderToTest.IsResponsibleForReplacing("< SignOff >", Passes.FirstPass));
			Assert("should not match < SignOffText >", !ValueProviderToTest.IsResponsibleForReplacing("< SignOffText >", Passes.FirstPass));
			Assert("should not match < Salu tation>", !ValueProviderToTest.IsResponsibleForReplacing("< Salu tation>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			Assert(ValueProviderToTest.GetReplacement("<SignOff>", Report).GetType() == typeof(string));
		}

		public void TestSignOff()
		{
			var extraFactory = new BusinessObjectFactory();
			var staff = extraFactory.New<GlbStaff>();

			staff.GS_LoginName = "testname";

			staff.GS_FullName = "John Doe";
			staff.GS_Title = "Manager";

			staff.GS_EmailAddress = "user@edi.com.au";
			staff.GS_PublishEmailAddress = true;

			staff.GS_WorkPhone = "111";
			staff.GS_PublishWorkPhone = true;

			staff.GS_WorkExtension = "222";
			staff.GS_PublishWorkExtension = true;

			staff.GS_FaxNum = "333";
			staff.GS_PublishFaxNum = true;

			staff.GS_HomePhone = "444";
			staff.GS_PublishHomePhone = true;

			staff.GS_MobilePhone = "555";
			staff.GS_PublishMobilePhone = true;

			DocumentsDataRegistry.Instance.ShowUserTitleOnDocuments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			DocumentsDataRegistry.Instance.ShowPublishedStaffDetailsOnDocuments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			extraFactory.Save();

			using (ZArchitecture.Core.Testing.CurrentUserChanger.SwitchToNewUserTemporarily(staff.GS_LoginName))
			{
				string expectedSignOff = @"Yours Sincerely,

John Doe
Manager
 Email: user@edi.com.au Work: 111 Work Extension: 222 Fax: 333 Home: 444 Mobile: 555";

				AssertEquals("Should be 'John Doe'", expectedSignOff, ValueProviderToTest.GetReplacement("<SignOff>", Report));

				GlbStaff.CurrentUser.GS_EmailAddress = "";
				extraFactory.Save();

				string expectedSignOff2 = @"Yours Sincerely,

John Doe
Manager
 Work: 111 Work Extension: 222 Fax: 333 Home: 444 Mobile: 555";

				AssertEquals("Should be 'John Doe'", expectedSignOff2, ValueProviderToTest.GetReplacement("<SignOff>", Report));

				GlbStaff.CurrentUser.GS_PublishHomePhone = false;
				extraFactory.Save();

				string expectedSignOff3 = @"Yours Sincerely,

John Doe
Manager
 Work: 111 Work Extension: 222 Fax: 333 Mobile: 555";

				AssertEquals("Should be 'John Doe'", expectedSignOff3, ValueProviderToTest.GetReplacement("<SignOff>", Report));
			}
		}

		public void TestAllPublishedStaffDetailsAreIncluded()
		{
			string[] includedPropertyNames =
				  {
						GlbStaffSchema.Constants.GS_EmailAddress,
						GlbStaffSchema.Constants.GS_FaxNum,
						GlbStaffSchema.Constants.GS_HomePhone,
						GlbStaffSchema.Constants.GS_MobilePhone,
						GlbStaffSchema.Constants.GS_WorkPhone,
						GlbStaffSchema.Constants.GS_WorkExtension
				  };

			foreach (ZPropertyInfo propertyInfo in GlbStaff.CurrentUser.ZPropertyInfoHash)
			{
				if (propertyInfo.IsPersistent && propertyInfo.Name.StartsWith("GS_Publish") && (propertyInfo.PropertyType == typeof(ZBool)))
				{
					string expectedPropertyName = "GS_" + propertyInfo.Name.Substring(10);
					if (GlbStaff.CurrentUser.ZPropertyInfoHash.ContainsKey(expectedPropertyName))
					{
						AssertCollectionContains(expectedPropertyName + " should be included.", expectedPropertyName, includedPropertyNames);
					}
				}
			}
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new SignOff();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			var registry = RawDataRegistry.Instance.FindByName("SignOffText");
			registry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Best Regards,");
			DocumentsDataRegistry.Instance.ShowUserTitleOnDocuments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			DocumentsDataRegistry.Instance.ShowPublishedStaffDetailsOnDocuments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlbStaff.CurrentUser.GS_LoginName = "JHD";
			GlbStaff.CurrentUser.GS_FullName = "John Doe";
			GlbStaff.CurrentUser.GS_Title = "Manager";
			GlbStaff.CurrentUser.GS_EmailAddress = "user@wisetechglobal.com";
			GlbStaff.CurrentUser.GS_PublishEmailAddress = true;
			GlbStaff.CurrentUser.GS_WorkPhone = "123456789";
			GlbStaff.CurrentUser.GS_PublishWorkPhone = true;
			GlbStaff.CurrentUser.GS_WorkExtension = "789";
			GlbStaff.CurrentUser.GS_PublishWorkExtension = true;
			GlbStaff.CurrentUser.GS_FaxNum = "+61 2 9025 1199";
			GlbStaff.CurrentUser.GS_PublishFaxNum = true;
			GlbStaff.CurrentUser.GS_HomePhone = "+61 2 9025 1100";
			GlbStaff.CurrentUser.GS_PublishHomePhone = true;
			GlbStaff.CurrentUser.GS_MobilePhone = "123456789";
			GlbStaff.CurrentUser.GS_PublishMobilePhone = true;
		}
	}
}
