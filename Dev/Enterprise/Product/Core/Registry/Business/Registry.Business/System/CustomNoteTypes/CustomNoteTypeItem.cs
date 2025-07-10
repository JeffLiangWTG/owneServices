using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CustomNoteTypeItem : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string NoteName = "NoteName";
			public const string IsTextOnly = "IsTextOnly";
			public const string IsAppendingNote = "IsAppendingNote";
			public const string IsReadOnlyAfterAdd = "IsReadOnlyAfterAdd";
			public const string DefaultVisibility = "DefaultVisibility";
			public const string ForceRead = "ForceRead";
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CustomNoteTypeItem();
		}

		#region Bound Properties

		#region NoteName

		[MaxLength(StmNote.Schema.ST_DescriptionMaxLength)]
		[CustomNoteNameTranslatableDataField(Schema.NoteName, MaxLength = StmNote.Schema.ST_DescriptionMaxLength, Type = typeof(CustomNoteTypeItem), Asmid = ResString.AssemblyId)]
		[ResourceStringData("9fe6d1b2-4b36-4504-b678-dbb77f3ea95a", Caption = "Note Type Description")]
		public ZString NoteName
		{
			get { return noteName; }
			set
			{
				value = value.Trim();
				CheckMaximumLength(NoteNameInfo, value);
				SetNonPersistentPropertyValue<ZString>(NoteNameInfo, ref noteName, value);
				if (!IsValidationSuspended)
				{
					ValidateNoteName();
				}
			}
		}
		ZString noteName;

		public ZPropertyInfo NoteNameInfo
		{
			get { return GetZPropertyInfo(Schema.NoteName); }
		}

		public MultilingualString NoteNameMultilingual
		{
			get { return GetMultilingual(NoteNameInfo); }
		}

		#endregion

		#region IsTextOnly

		public ZBool IsTextOnly
		{
			get { return isTextOnly; }
			set { SetNonPersistentPropertyValue<ZBool>(IsTextOnlyInfo, ref isTextOnly, value); }
		}
		ZBool isTextOnly;

		public ZPropertyInfo IsTextOnlyInfo
		{
			get { return GetZPropertyInfo(Schema.IsTextOnly); }
		}

		#endregion

		#region IsAppendingNote

		public ZBool IsAppendingNote
		{
			get { return isAppendingNote; }
			set { SetNonPersistentPropertyValue<ZBool>(IsAppendingNoteInfo, ref isAppendingNote, value); }
		}
		ZBool isAppendingNote;

		public ZPropertyInfo IsAppendingNoteInfo
		{
			get { return GetZPropertyInfo(Schema.IsAppendingNote); }
		}

		#endregion

		#region IsReadOnlyAfterAdd

		public ZBool IsReadOnlyAfterAdd
		{
			get { return isReadOnlyAfterAdd; }
			set { SetNonPersistentPropertyValue<ZBool>(IsReadOnlyAfterAddInfo, ref isReadOnlyAfterAdd, value); }
		}
		ZBool isReadOnlyAfterAdd;

		public ZPropertyInfo IsReadOnlyAfterAddInfo
		{
			get { return GetZPropertyInfo(Schema.IsReadOnlyAfterAdd); }
		}

		#endregion

		#region ForceRead

		public ZBool ForceRead
		{
			get { return forceRead; }
			set { SetNonPersistentPropertyValue<ZBool>(ForceReadInfo, ref forceRead, value); }
		}
		ZBool forceRead;

		public ZPropertyInfo ForceReadInfo
		{
			get { return GetZPropertyInfo(Schema.ForceRead); }
		}

		#endregion

		#region DefaultVisibility

		[MaxLength(3)]
		public ZString DefaultVisibility
		{
			get { return defaultVisibility; }
			set
			{
				CheckMaximumLength(DefaultVisibilityInfo, value);
				SetNonPersistentPropertyValue<ZString>(DefaultVisibilityInfo, ref defaultVisibility, value);
				if (!IsValidationSuspended)
				{
					ValidateDefaultVisibility();
				}
			}
		}
		ZString defaultVisibility;

		public ZPropertyInfo DefaultVisibilityInfo
		{
			get { return GetZPropertyInfo(Schema.DefaultVisibility); }
		}

		#endregion

		#endregion

		#region Lookups

		public CodeDescriptionPairList VisibilityList
		{
			get
			{
				if (visibilityList == null)
				{
					visibilityList = new CodeDescriptionPairList();
					visibilityList.AddPair(StmNoteDescription.Pub, StmNoteDescription.PubDescriptive);
					visibilityList.AddPair(StmNoteDescription.Int, StmNoteDescription.IntDescriptive);
					visibilityList.AddPair(StmNoteDescription.Prv, StmNoteDescription.PrvDescriptive);
					visibilityList.AddPair(StmNoteDescription.Agv, StmNoteDescription.AgvDescriptive);
				}

				return visibilityList;
			}
		}
		CodeDescriptionPairList visibilityList;

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.IsTextOnly, IsTextOnly.ToString());
			writer.WriteElementString(Schema.IsAppendingNote, IsAppendingNote.ToString());
			writer.WriteElementString(Schema.IsReadOnlyAfterAdd, IsReadOnlyAfterAdd.ToString());
			writer.WriteElementString(Schema.ForceRead, ForceRead.ToString());
			writer.WriteElementString(Schema.DefaultVisibility, DefaultVisibility);
			writer.WriteElementString(Schema.NoteName, NoteName);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			IsTextOnly = new ZBool(reader.ReadElementString(Schema.IsTextOnly));
			IsAppendingNote = new ZBool(reader.ReadElementString(Schema.IsAppendingNote));
			IsReadOnlyAfterAdd = new ZBool(reader.ReadElementString(Schema.IsReadOnlyAfterAdd));
			ForceRead = new ZBool(reader.ReadElementString(Schema.ForceRead));
			DefaultVisibility = reader.ReadElementString(Schema.DefaultVisibility);
			NoteName = reader.ReadElementString(Schema.NoteName);
		}

		#endregion

		#region Validation

		public void ValidateDefaultVisibility()
		{
			DefaultVisibilityInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(DefaultVisibilityInfo, VisibilityList);
			MandatoryValidation.CheckEntered(DefaultVisibilityInfo);
		}

		public void ValidateNoteName()
		{
			NoteNameInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(NoteNameInfo);

			EnglishCharactersValidation.ErrorIfNotWesternEuropean(NoteNameInfo);

			if (PredefinedNoteTypes.Instance.NoteTypeByDescription(NoteName, false) != null)
			{
				NoteNameInfo.AddError(Res.GetString("560d08f6-bd52-47d6-809d-d4178ab88769", "A system-provided note type called \"{0}\" already exists. You cannot create a custom note with the same name as a system-defined note.", NoteName));
			}

			if (!NoteNameInfo.HasErrors() && ModuleAndCountry != null && ModuleAndCountry.NoteTypeParent != null && ModuleAndCountry.CountryCode != CustomNoteModuleAndCountry.ModuleAndCountryCodes.CountryALL)
			{
				CustomNoteModuleAndCountry moduleForAll = ModuleAndCountry.NoteTypeParent.NoteModuleAndCountryList.FindOrCreateEntryFromCode(ModuleAndCountry.ModuleIDName, CustomNoteModuleAndCountry.ModuleAndCountryCodes.CountryALL, ZBool.False);
				if (moduleForAll != null)
				{
					foreach (CustomNoteTypeItem item in moduleForAll.CustomNoteTypesList)
					{
						if (item.NoteName == NoteName)
						{
							NoteNameInfo.AddError(Res.GetString("1b528ec7-1175-4cf8-af10-99933cbb056f", "A note by the same name has already been defined for the module {0} under the \"All Countries/Regions\" category. You cannot have two modules with the same description listed in both the All Countries/Regions category and a specific country/region category.", ModuleAndCountry.ModuleIDName));
							break;
						}
					}
				}
			}

			TranslatableDataFieldAttribute.Validate(NoteNameInfo);
		}

		public void ValidateIsAppendingNote()
		{
			IsAppendingNoteInfo.ClearAllNotifications();
			if (IsAppendingNote && !IsTextOnly)
			{
				IsAppendingNoteInfo.AddError(Res.GetString("3ce1f800-1d57-42a2-bd7c-fef6b4e41509", "The Is Appending Note feature is only available for text-only notes. A note marked as having Appending Note behavior must also be flagged as a Text-Only note."));
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateDefaultVisibility();
			ValidateNoteName();
			ValidateIsAppendingNote();
		}

		internal CustomNoteModuleAndCountry ModuleAndCountry
		{
			get { return ParentCollections.Count > 0 ? ((CustomNoteTypeItemCollection)ParentCollections.First()).ModuleAndCountry : null; }
		}

		#endregion
	}

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[XmlRoot("CustomNoteTypesList")]
	public class CustomNoteTypeItemCollection : RegistryBusinessObjectCollectionTemplate
	{
		public CustomNoteTypeItemCollection()
		{
		}

		public CustomNoteTypeItemCollection(CustomNoteModuleAndCountry moduleAndCountry)
		{
			ModuleAndCountry = moduleAndCountry;
		}

		public CustomNoteModuleAndCountry ModuleAndCountry
		{
			get { return moduleAndCountry; }
			set { moduleAndCountry = value; }
		}
		CustomNoteModuleAndCountry moduleAndCountry;

		public new CustomNoteTypeItem this[int i]
		{
			get { return (CustomNoteTypeItem)Elements[i]; }
		}

		public new CustomNoteTypeItem AddNew()
		{
			return (CustomNoteTypeItem)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CustomNoteTypeItemCollection(ModuleAndCountry);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CustomNoteTypeItem();
		}

		public NoteTypeCollection ToNoteTypeCollection()
		{
			NoteTypeCollection result = new NoteTypeCollection();
			foreach (CustomNoteTypeItem item in this)
			{
				StmNoteVisibility visiblity = (StmNoteVisibility)Enum.Parse(typeof(StmNoteVisibility), item.DefaultVisibility);
				PredefinedNoteType newNoteType = new PredefinedNoteType(item.NoteNameMultilingual, visiblity, true, item.IsReadOnlyAfterAdd, item.IsTextOnly, item.IsAppendingNote, true);
				result.Add(newNoteType);
			}
			return result;
		}
	}
}
