using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ResourceStrings.Business
{
	public class HelpDataStringValidation : AutoHelpDataStringValidation
	{
		public HelpDataStringValidation(AutoHelpDataString parent)
			: base(parent)
		{ }

		new HelpDataString Parent
		{
			get { return (HelpDataString)base.Parent; }
		}

		bool reinforce;

		protected override void CheckHD_Caption()
		{
			base.CheckHD_Caption();

			if (!reinforce)
			{
				reinforce = true;
				ValidateHD_MidCaption();
				ValidateHD_ShortCaption();
				ValidateHD_FullDescription();
				reinforce = false;
			}
		}

		protected override void CheckHD_MidCaption()
		{
			base.CheckHD_MidCaption();

			if (!Parent.HD_MidCaption.IsEmpty && !Parent.HD_Caption.IsEmpty &&
				Parent.HD_MidCaption.Length > Parent.HD_Caption.Length)
			{
				Parent.HD_MidCaptionInfo.AddError("Medium size captions must be shorter than long size captions.");
			}

			if (!reinforce)
			{
				reinforce = true;
				ValidateHD_Caption();
				ValidateHD_ShortCaption();
				ValidateHD_FullDescription();
				reinforce = false;
			}
		}

		protected override void CheckHD_ShortCaption()
		{
			base.CheckHD_ShortCaption();

			if (!Parent.HD_ShortCaption.IsEmpty && !Parent.HD_MidCaption.IsEmpty &&
				Parent.HD_ShortCaption.Length > Parent.HD_MidCaption.Length)
			{
				Parent.HD_ShortCaptionInfo.AddError("Short captions must be shorter than medium size captions.");
			}

			if (!Parent.HD_ShortCaption.IsEmpty && !Parent.HD_Caption.IsEmpty &&
				Parent.HD_ShortCaption.Length > Parent.HD_Caption.Length)
			{
				Parent.HD_ShortCaptionInfo.AddError("Short captions must be shorter than long size captions.");
			}

			if (!reinforce)
			{
				reinforce = true;
				ValidateHD_MidCaption();
				ValidateHD_Caption();
				ValidateHD_FullDescription();
				reinforce = false;
			}
		}

		protected override void CheckHD_FullDescription()
		{
			base.CheckHD_FullDescription();

			if (!Parent.HD_FullDescription.IsEmpty && !Parent.HD_Caption.IsEmpty && (Parent.HD_FullDescription.EqualsIgnoringCase(Parent.HD_Caption) || Parent.HD_FullDescription.EqualsIgnoringCase(Parent.HD_Caption + ".")))
			{
				Parent.HD_FullDescriptionInfo.AddError("Description is the same as the Caption. Enter a valuable Description or none at all.");
			}

			if (!Parent.HD_FullDescription.IsEmpty && !Parent.HD_FullDescription.EndsWith(".") && !Parent.HD_FullDescription.EndsWith("?"))
			{
				Parent.HD_FullDescriptionInfo.AddWarning("Description should be a full sentence.");
			}

			if (Parent.HD_FullDescription.Length > 0 && char.GetUnicodeCategory(Parent.HD_FullDescription[0]) == System.Globalization.UnicodeCategory.LowercaseLetter)
			{
				Parent.HD_FullDescriptionInfo.AddWarning("Description should start with a capital.");
			}

			if (!Parent.HD_FullDescription.IsEmpty)
			{
				CheckSlashRSlashN();

				string text = Parent.HD_FullDescription;
				if (text != text.Trim())
				{
					Parent.HD_FullDescriptionInfo.AddWarning("Description should not have leading and/or trailing spaces.");
				}
			}

			if (!reinforce)
			{
				reinforce = true;
				ValidateHD_MidCaption();
				ValidateHD_ShortCaption();
				ValidateHD_Caption();
				reinforce = false;
			}
		}

		void CheckSlashRSlashN()
		{
			string text = Parent.HD_FullDescription;
			for (int i = 0; i < text.Length; i++)
			{
				if ((text[i] == '\n' && (i == 0 || text[i - 1] != '\r')) ||
					(text[i] == '\r' && (i == text.Length - 1 || text[i + 1] != '\n')))
				{
					Parent.HD_FullDescriptionInfo.AddError("Description has wrong New Line marks.");
				}
			}
		}

		protected override void CheckHD_Language()
		{
			base.CheckHD_Language();
			MandatoryValidation.CheckEntered(Parent.HD_LanguageInfo, "language");
			ListValidation.ErrorIfInvalidCode(Parent.HD_LanguageInfo, Parent.Lookups.Languages, (NoResString)"language");
		}

		protected override void CheckHD_Code()
		{
			base.CheckHD_Code();
			MandatoryValidation.CheckEntered(Parent.HD_CodeInfo, "code");
		}

		protected override void CheckHD_EditReason()
		{
			base.CheckHD_EditReason();
			if (Parent.HasChanges)
			{
				MandatoryValidation.CheckEntered(Parent.HD_EditReasonInfo);
				ListValidation.ErrorIfInvalidCode(Parent.HD_EditReasonInfo);
				if (Parent.HD_EditReason == EditReasons.Codes.TranslationFeedback)
				{
					Parent.HD_EditReasonInfo.AddError("Cannot directly save translation feedback resource strings");
				}
			}
		}
	}
}
