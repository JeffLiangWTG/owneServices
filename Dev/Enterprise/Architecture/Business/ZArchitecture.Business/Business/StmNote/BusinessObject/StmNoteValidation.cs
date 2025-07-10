using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business
{
	public class StmNoteValidation : AutoStmNoteValidation
	{
		public StmNoteValidation(AutoStmNote parent)
			: base(parent)
		{
		}

		public new StmNote Parent
		{
			get { return parent ?? (parent = (StmNote)base.Parent); }
		}

		StmNote parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCalculatedProperty(Parent.ST_DescriptionInDatabaseInfo);
		}

		#region ST_Description

		protected override void CheckST_Description()
		{
			base.CheckST_Description();

			MandatoryValidation.CheckEntered(Parent.ST_DescriptionInfo);

			if (Parent.HasMaster && !Parent.ST_IsCustomDescription)
			{
				if (Parent.ST_Description == PredefinedNoteTypes.Instance.OrderUpdateHistory.Description && Parent.Master is ICommonShipment)
				{
					var message = Res.GetString("050bd8ca-7521-46e1-8317-3f8526c8f5c6", "A note type with the description \"{0}\" is not a valid note type.", Parent.ST_Description);
					Parent.ST_DescriptionInfo.AddError(message);
				}

				if (!CustomNotesProvider.Instance.NoteTypeExistsByName(Parent.ST_DescriptionInDatabase) && PredefinedNoteTypes.Instance.NoteTypeByDescription(Parent.ST_DescriptionInDatabase) == null)
				{
					if (!Parent.ST_DescriptionInDatabase.IsEmpty && !Parent.ST_Description_List.List.ContainsCode(Parent.ST_DescriptionInDatabase))
					{
						var message = Res.GetString("ec92f3d9-bf54-4b89-b509-5748a580e103", "A note type with the description \"{0}\" is not a pre-defined note type in the list of available types.\r\n\r\nIf you would like to enter a note with this description, select the \"Custom Description\" flag, or have your system administrator set up a new Custom Note Type in the Registry.", Parent.ST_Description);
						Parent.ST_DescriptionInfo.AddError(message);
					}
				}

				if (!Parent.ST_DescriptionInfo.HasErrors() && Parent.ST_Description_List.IsOnlyOneAllowedForDescription(Parent.ST_DescriptionInDatabase))
				{
					ValidatePredefinedDescriptionIsUnique();
				}

				if (!Parent.ST_DescriptionInfo.HasErrors() && !Parent.ST_DescriptionInfo.ReadOnly && Parent.ST_Description == PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description)
				{
					var message = Res.GetString("45902a03-1793-4ca3-87a1-36ac6fa93117", "It is not recommended to manually enter AutoRating Log Notes because it will be recreated and overridden on next AutoRating session.");
					Parent.ST_DescriptionInfo.AddWarning(message);
				}
			}
		}

		void ValidatePredefinedDescriptionIsUnique()
		{
			var collectionForCheck = new StmNoteCollection(Parent.Master, Parent.Factory);
			collectionForCheck.Load();

			var reloadedElements = new BusinessObjectFactory().Load<StmNote>(new ZQuery(Parent.Master.Notes.ElementsInternal.CompleteFilter));
			var reloadedNotes = reloadedElements.Where(note => !note.IsDeleted && collectionForCheck.All(n => n.PK != note.PK));

			collectionForCheck.AddRange(reloadedNotes);

			foreach (StmNote note in collectionForCheck)
			{
				if (IsParentNoteNotUnique(note))
				{
					var matchingNote = Parent.Factory.GetBizOsForPK(note.PK.ToGuid()).FirstOrDefault(o => o is StmNote);
					if (matchingNote != null && matchingNote.IsDeleted)
					{
						continue;
					}

					Parent.ST_DescriptionInfo.AddError(Res.GetString("e4d3090a-e3e9-48c5-a5a3-7a45e8336cab", "There is already another note with the description '{0}' for Context Module '", Parent.ST_Description) + Parent.ST_NoteContextModuleCaption.Substring(3) +
						Res.GetString("49510c72-93bc-4217-acca-0db9242e89e6", "', Direction '") + Parent.ST_NoteContextDirectionCaption.Substring(3) + Res.GetString("0b9740b6-c5c8-462a-b0fd-ec81b28b74b8", "', Freight Mode '") + Parent.ST_NoteContextFreightModeCaption.Substring(3) + Res.GetString("d6437223-b91b-4826-9a35-f40b5b0f11fc", "'. Please select a different description, context module, direction or freight mode. If the duplicate note is not shown, it could belong to another company or recently added by another user - please reload the form. "));
					break;
				}
			}
		}

		bool IsParentNoteNotUnique(StmNote note)
		{
			var result = note != null
						 && note.PK != Parent.PK
						 && string.Equals(note.ST_Description, Parent.ST_Description, StringComparison.OrdinalIgnoreCase)
						 && string.Equals(note.ST_NoteContext, Parent.ST_NoteContext, StringComparison.OrdinalIgnoreCase)
						 && !ShouldSkipUniquenessValidationForReadonlyNoteAfterAdd(note);

			if (result && note.ST_Description == PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description)
			{
				result = note.ST_GC_RelatedCompany == Parent.ST_GC_RelatedCompany;
			}

			return result;
		}

		bool ShouldSkipUniquenessValidationForReadonlyNoteAfterAdd(StmNote note)
		{
			var noteType = PredefinedNoteTypes.Instance.NoteTypeByDescription(note.ST_DescriptionInDatabase);
			return noteType != null && noteType.IsReadOnlyAfterAdd && note.ReadOnly && parent.ReadOnly;
		}

		#endregion

		#region ST_IsCustomDescription

		protected override void CheckST_IsCustomDescription()
		{
			base.CheckST_IsCustomDescription();

			if (Parent.ST_IsCustomDescription && Parent.HasMaster && Parent.ST_Description_List.NoteTypeByDescription(Parent.ST_Description) != null)
			{
				if (!Parent.IsInDatabase || Parent.HasChanges)
				{
					Parent.ST_IsCustomDescriptionInfo.AddError(Res.GetString("89f88c9e-ab09-4dc4-aba4-8f9a548eb611", "{0} is not a custom note. Please uncheck 'Custom Desc.' for the {0} note.", Parent.ST_Description));
				}
				else
				{
					Parent.ST_IsCustomDescriptionInfo.AddWarning(Res.GetString("89f88c9e-ab09-4dc4-aba4-8f9a548eb611", "{0} is not a custom note. Please uncheck 'Custom Desc.' for the {0} note.", Parent.ST_Description));
				}
			}

			if (!Parent.ST_IsCustomDescriptionInfo.HasErrors() && Parent.Master != null && ((BusinessObject)Parent.Master).IsInDatabase)
			{
				ISecurityProxy security = EnvProxy.Instance.Security;
				if (!security.NotesNewCustomNote.IsAllowed && security.NotesEdit.IsAllowed && (ZBool)Parent.ST_IsCustomDescriptionInfo.OriginalValue == ZBool.False && (ZBool)Parent.ST_IsCustomDescriptionInfo.Value == ZBool.True)
				{
					Parent.ST_IsCustomDescriptionInfo.AddError(Res.GetString("c26e7160-ac18-4df7-b01e-a235a7771690", "You do not have security to convert this existing note from a non-custom note to a custom note. If you require this ability, please ask your system administrator to grant access to Notes --> New --> New Custom Note."));
				}
			}
		}

		#endregion

		#region ST_NoteContext

		protected override void CheckST_NoteContext()
		{
			base.CheckST_NoteContext();
			ValidateST_Description();

			ValidateCalculatedProperty(Parent.ST_NoteContextModuleInfo);
			ValidateCalculatedProperty(Parent.ST_NoteContextDirectionInfo);
			ValidateCalculatedProperty(Parent.ST_NoteContextFreightModeInfo);

			if (Parent.ShouldNoteContextBeChecked && (Parent.ST_NoteContextInfo.HasChanges || !Parent.IsInDatabase) && !EnvProxy.Instance.Security.NotesNewWithAllMDFForOrg.IsAllowed)
			{
				if (Parent.ST_NoteContext == "AAA")
				{
					Parent.ST_NoteContextInfo.AddError(Res.GetString("AEF6DABD-63E9-407D-84CF-93559DA6C95C", "You do not have permission to create an organization note with a visibility of All/All/All. Please select a more specific visibility."));
				}
			}
		}

		#endregion

		#region ST_GC_RelatedCompany

		protected override void CheckST_GC_RelatedCompany()
		{
			base.CheckST_GC_RelatedCompany();

			if (Parent.ShouldNoteContextBeChecked && (Parent.ST_GC_RelatedCompanyInfo.HasChanges || !Parent.IsInDatabase) && !EnvProxy.Instance.Security.NotesNewWithAllCompanyForOrg.IsAllowed)
			{
				if (Parent.ST_GC_RelatedCompany.IsEmpty)
				{
					Parent.ST_GC_RelatedCompanyInfo.AddError(Res.GetString("10C73012-2E82-407F-8BCD-2CC321585F6F", "You do not have permission to create an organization note without a company. Please select a company."));
				}
			}
		}

		#endregion

		#region ST_NoteContextModuleCaption

		protected void CheckST_NoteContextModuleCaption()
		{
			MandatoryValidation.CheckEntered(Parent.ST_NoteContextModuleCaptionInfo);
			if (!Parent.ST_NoteContextModule_List.ToArray().Select(x => ((CodeDescriptionPair)x).MultilingualCode.ToString()).Any(x => x == Parent.ST_NoteContextModuleCaption))
			{
				ListValidation.ErrorIfInvalidCode(Parent.ST_NoteContextModuleCaptionInfo, Parent.ST_NoteContextModule_List);
			}
		}

		#endregion

		#region ST_NoteContextDirectionCaption

		protected void CheckST_NoteContextDirectionCaption()
		{
			MandatoryValidation.CheckEntered(Parent.ST_NoteContextDirectionCaptionInfo);
			if (!Parent.ST_NoteContextDirection_List.ToArray().Select(x => ((CodeDescriptionPair)x).MultilingualCode.ToString()).Any(x => x == Parent.ST_NoteContextDirectionCaption))
			{
				ListValidation.ErrorIfInvalidCode(Parent.ST_NoteContextDirectionCaptionInfo, Parent.ST_NoteContextDirection_List);
			}
		}

		#endregion

		#region ST_NoteContextFreightModeCaption

		protected void CheckST_NoteContextFreightModeCaption()
		{
			MandatoryValidation.CheckEntered(Parent.ST_NoteContextFreightModeCaptionInfo);
			if (!Parent.ST_NoteContextFreightMode_List.ToArray().Select(x => ((CodeDescriptionPair)x).MultilingualCode.ToString()).Any(x => x == Parent.ST_NoteContextFreightModeCaption))
			{
				ListValidation.ErrorIfInvalidCode(Parent.ST_NoteContextFreightModeCaptionInfo, Parent.ST_NoteContextFreightMode_List);
			}
		}

		#endregion

		#region ST_NoteType

		protected override void CheckST_NoteType()
		{
			base.CheckST_NoteType();
			ValidateST_NoteType_DescriptiveText(); // validate the calculated property we are bound to
		}

		public void ValidateST_NoteType_DescriptiveText()
		{
			ValidateCalculatedProperty(Parent.ST_NoteType_DescriptiveTextInfo);
		}

		protected void CheckST_NoteType_DescriptiveText()
		{
			MandatoryValidation.CheckEntered(Parent.ST_NoteType_DescriptiveTextInfo);

			if (!Parent.ST_NoteType_DescriptiveTextInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.ST_NoteType_DescriptiveTextInfo, Parent.ST_NoteType_List);
			}
		}

		#endregion

		#region ST_NoteData & ST_NoteText

		protected override void CheckST_NoteData()
		{
			base.CheckST_NoteData();

			if (!Parent.ST_IsTextOnly && IsEmptyValue(Parent.ST_NoteData))
			{
				var notificationType = Parent.IsInDatabase && IsEmptyValue((ZBlob)Parent.ST_NoteDataInfo.OriginalValue) ?
					CargoWise.ComponentModel.NotificationType.Warning :
					CargoWise.ComponentModel.NotificationType.Error;

				Parent.ST_NoteDataInfo.AddNotification(notificationType, NoNoteDetailsErrorMessage);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "need to check this as perviously we saved a file in rtf")]
		bool IsEmptyValue(ZBlob b)
		{
			if (b.IsEmpty)
			{
				return true;
			}

			var asAsciiString = b.ToAscii();
			return asAsciiString.StartsWith(@"{\rtf1", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(ORtfTextUtil.RtfToText(asAsciiString)) && !asAsciiString.Contains(@"{\object\objemb", StringComparison.OrdinalIgnoreCase);
		}

		protected override void CheckST_NoteText()
		{
			base.CheckST_NoteText();

			if (Parent.ST_IsTextOnly && Parent.ST_NoteText.IsEmpty)
			{
				Parent.ST_NoteTextInfo.AddError(NoNoteDetailsErrorMessage);
			}

			if (Parent.ST_Description == PredefinedNoteTypes.Instance.CountryRulesValidation.Description)
			{
				ObjectFactory.Get<IRefCountryRulesValidator>().ValidateRule(Parent.Master as BusinessObject, Parent.ST_NoteTextInfo, Parent.ST_NoteText);
			}
		}

		string NoNoteDetailsErrorMessage
		{
			get { return Parent.ST_Description.IsEmpty ? Res.GetString("3e5dcfe8-d458-4be6-bab5-80c6fbbf7421", "A note without a description also has no note details.") : Res.GetString("22fe00f4-b329-4424-ad92-fc615c817bc6", "Enter some note details for {0}.", Parent.ST_Description); }
		}

		protected override void CheckST_NoteDataIsValidZBlobSize()
		{
			if (!Parent.IsInDatabase || !Parent.ST_NoteDataInfo.ReadOnly)
			{
				if (Parent.ST_Description != PredefinedNoteTypes.Instance.MessageInterpretation.Description)
				{
					base.CheckST_NoteDataIsValidZBlobSize();
				}
			}
		}

		#endregion
	}
}
