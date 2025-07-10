using CargoWise.Common;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Business.Testing
{
	[TestedType(typeof(HelpDataString))]
	sealed class HelpDataStringTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		public void TestClone()
		{
			HelpDataString helpDataStringExisting = (HelpDataString)GetNewBusinessObject();
			helpDataStringExisting.HD_Code = "code";
			helpDataStringExisting.HD_Language = "abc";
			helpDataStringExisting.HD_Caption = "caption";
			helpDataStringExisting.HD_ShortCaption = "short";
			helpDataStringExisting.HD_MidCaption = "mid";
			helpDataStringExisting.HD_FullDescription = "full";

			HelpDataString clone = helpDataStringExisting.Clone();
			AssertEquals("code", clone.HD_Code);
			AssertEquals("abc", clone.HD_Language);
			AssertEquals("caption", clone.HD_Caption);
			AssertEquals("short", clone.HD_ShortCaption);
			AssertEquals("mid", clone.HD_MidCaption);
			AssertEquals("full", clone.HD_FullDescription);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(MasterFiles.Business.GlbCompany.CurrentCompany.OrgProxy.OH_Language, HelpDataString.HD_Language);
		}

		public void TestHD_IsCheckedOut_ReadOnly()
		{
			AssertEquals(true, HelpDataString.HD_IsCheckedOutInfo.ReadOnly);
		}

		public void TestIsEmpty()
		{
			AssertEquals(true, HelpDataString.IsEmpty);
			HelpDataString.HD_Caption = "something";
			AssertEquals(false, HelpDataString.IsEmpty);
			HelpDataString.HD_Caption = "";
			AssertEquals(true, HelpDataString.IsEmpty);

			HelpDataString.HD_ShortCaption = "something";
			AssertEquals(false, HelpDataString.IsEmpty);
			HelpDataString.HD_ShortCaption = "";
			AssertEquals(true, HelpDataString.IsEmpty);

			HelpDataString.HD_FullDescription = "something";
			AssertEquals(false, HelpDataString.IsEmpty);
			HelpDataString.HD_FullDescription = "";
			AssertEquals(true, HelpDataString.IsEmpty);
		}

		public void TestCurrentLanguageRefersToCurrentStaffWorkingLanguage()
		{
			AssertEquals(GlbStaff.CurrentUser.GS_WorkingLanguage, ResourceStrings.Instance.CurrentLanguage);
		}

		public void TestToResourceStringData()
		{
			HelpDataString.HD_Code = "AAA";
			HelpDataString.HD_Caption = "BBB";
			HelpDataString.HD_ShortCaption = "CCC";
			HelpDataString.HD_FullDescription = "DDD";

			ResourceStringData res = HelpDataString.ToResourceStringData();
			AssertEquals("AAA", res.Key);
			AssertEquals("BBB", res.Caption);
			AssertEquals("CCC", res.ShortCaption);
			AssertEquals("DDD", res.FullDescription);
		}

		public void TestNoMaxLengthOnShortCaption()
		{
			HelpDataString.HD_ShortCaption = new string('x', 500);
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
		}

		public void TestNoMaxLengthOnMidCaption()
		{
			HelpDataString.HD_MidCaption = new string('x', 500);
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
		}

		HelpDataString HelpDataString
		{
			get { return helpDataString ?? (helpDataString = new HelpDataString()); }
		}
		HelpDataString helpDataString;
	}
}
