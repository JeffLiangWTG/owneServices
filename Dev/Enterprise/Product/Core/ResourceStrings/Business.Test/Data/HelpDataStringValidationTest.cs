using CargoWise.EntityFramework.Testing;

namespace Enterprise.ResourceStrings.Business.Testing
{
	sealed class HelpDataStringValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCode()
		{
			Data.HD_Code = "";
			AssertHasError(Data.HD_CodeInfo, "Please enter a code.");
			Data.HD_Code = "abc";
			AssertNoError(Data.HD_CodeInfo, "Please enter a code.");
		}

		public void TestLanguage()
		{
			Data.HD_Language = "";
			Data.Validation.ValidateHD_Language();
			AssertHasError(Data.HD_LanguageInfo, "Please enter a language.");
			Data.HD_Language = Res.DefaultLanguage;
			AssertNoError(Data.HD_LanguageInfo, "Please enter a language.");
			AssertNoError(Data.HD_LanguageInfo, "Enter a valid language.");
			Data.HD_Language = "Z1Z";
			AssertHasError(Data.HD_LanguageInfo, "Enter a valid language.");
		}

		public void TestMinimalFieldsPassValidation()
		{
			Data.HD_Code = "code";
			Data.HD_EditReason = EditReasons.Codes.TradosImport;
			Data.RunPreSaveValidation();
			AssertNoErrors(Data);
		}

		public void TestHD_MidCaption_ShorterThanLongCaptions()
		{
			Data.HD_FullDescription = "Fully describe each field.";
			Data.HD_Caption = "Caption";
			Data.HD_MidCaption = "Too long medium caption";
			AssertHasErrorContaining(Data.HD_MidCaptionInfo, "must be shorter than long size captions");

			Data.HD_Caption = "The full caption";
			Data.HD_MidCaption = "Medium caption";
			Data.HD_ShortCaption = "Short caption";
			AssertNoErrors(Data.HD_MidCaptionInfo);
			AssertNoWarnings(Data.HD_MidCaptionInfo);
		}

		public void TestHD_ShortCaption_ShorterThanMediumCaptions()
		{
			Data.HD_FullDescription = "Fully describe each field.";
			Data.HD_Caption = "The full caption";
			Data.HD_MidCaption = "Medium caption";
			Data.HD_ShortCaption = "Too long short caption";
			AssertHasErrorContaining(Data.HD_ShortCaptionInfo, "must be shorter than medium size captions");

			Data.HD_MidCaption = "The medium caption";
			Data.HD_ShortCaption = "Short caption";
			AssertNoErrors(Data.HD_ShortCaptionInfo);
			AssertNoWarnings(Data.HD_ShortCaptionInfo);
		}

		public void TestHD_ShortCaption_ShorterThanLongCaptions()
		{
			Data.HD_FullDescription = "Fully describe each field.";
			Data.HD_Caption = "Long caption";
			Data.HD_ShortCaption = "Too long short caption caption";
			AssertHasErrorContaining(Data.HD_ShortCaptionInfo, "must be shorter than long size captions");

			Data.HD_Caption = "The full caption";
			Data.HD_ShortCaption = "Short Caption";
			AssertNoErrors(Data.HD_ShortCaptionInfo);
			AssertNoWarnings(Data.HD_ShortCaptionInfo);
		}

		public void TestHD_ShortCaption_EnteredOnlyWhenCaptionEntered()
		{
			Data.HD_FullDescription = "The full description is a description.";
			Data.HD_Caption = "The full caption";
			Data.HD_ShortCaption = "Short caption";
			AssertNoErrors(Data.HD_ShortCaptionInfo);

			Data.HD_Caption = "";
			Data.HD_ShortCaption = "ShoCaption";
			AssertNoErrors(Data.HD_ShortCaptionInfo);
		}

		public void TestHD_FullDescription_MustStartWithACapital()
		{
			Data.HD_Caption = "Caption";
			Data.HD_FullDescription = "A description.";
			AssertNoErrors(Data.HD_FullDescriptionInfo);
			AssertNoWarnings(Data.HD_FullDescriptionInfo);
			Data.HD_FullDescription = "a description.";
			Data.Validation.ValidateHD_FullDescription();
			AssertHasWarningContaining(Data.HD_FullDescriptionInfo, "capital");
			Data.HD_FullDescription = "֤nter a description.";
			AssertNoErrors(Data.HD_FullDescriptionInfo);
			AssertNoWarnings(Data.HD_FullDescriptionInfo);
		}

		public void TestFullDescriptionSameAsCaption()
		{
			Data.HD_Caption = "Caption";
			Data.HD_FullDescription = "Caption";
			AssertHasErrorContaining(Data.HD_FullDescriptionInfo, "Enter a valuable Description");
			Data.HD_FullDescription = "";
			AssertNoErrors(Data.HD_FullDescriptionInfo);
			Data.HD_FullDescription = "Caption.";
			AssertHasErrorContaining(Data.HD_FullDescriptionInfo, "Enter a valuable Description");
			Data.HD_FullDescription = "";
			AssertNoErrors(Data.HD_FullDescriptionInfo);
			Data.HD_FullDescription = "caption.";
			AssertHasErrorContaining(Data.HD_FullDescriptionInfo, "Enter a valuable Description");
		}

		public void TestEditReasonRequired()
		{
			Data.HD_EditReason = "";
			AssertNoErrors(Data.HD_EditReasonInfo);
			Data.HD_Caption = "Somthing";
			Data.RunPreSaveValidation();
			AssertHasErrorContaining(Data.HD_EditReasonInfo, "Edit Reason");
			Data.HD_EditReason = "XXX";
			AssertHasErrorContaining(Data.HD_EditReasonInfo, "Edit Reason");
			Data.HD_EditReason = EditReasons.Codes.TradosImport;
			AssertNoErrors(Data.HD_EditReasonInfo);
			Data.HD_EditReason = EditReasons.Codes.TranslationFeedback;
			AssertHasErrorContaining(Data.HD_EditReasonInfo, "Cannot directly save");
		}

		#region Implementation

		HelpDataString Data
		{
			get { return data ?? (data = new HelpDataString()); }
		}
		HelpDataString data;

		#endregion
	}
}
