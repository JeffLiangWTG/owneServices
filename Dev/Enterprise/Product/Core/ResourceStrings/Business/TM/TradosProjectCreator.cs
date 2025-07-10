#if DEBUG
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ResourceStrings.Business
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	public class TradosProjectCreator : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ZBoolDescriptionPairList LanguagesList
		{
			get
			{
				if (languagesList == null)
				{
					languagesList = new ZBoolDescriptionPairList();
					foreach (var language in DataFile.GetAvailableLanguages())
					{
						if (!Res.IsEnglish(language))
						{
							languagesList.AddNew(LanguagesLookupList.GetDescriptionFromCode(language), false);
						}
					}
					languagesList.OnPairChanged += new ZBoolDescriptionPairChangedEventHandler(languagesList_OnPairChanged);
				}
				return languagesList;
			}
		}

		ZBoolDescriptionPairList languagesList;

		void languagesList_OnPairChanged(ZBoolDescriptionPairChangedEventArgs e)
		{
			UpdatePathWithDefault();
		}

		public List<string> SelectedLanguages
		{
			get
			{
				return LanguagesList.Where(item => item.Value).ToList().ConvertAll(item => LanguagesLookupList.GetCodeFromDescription(item.Description));
			}
		}

		public CodeDescriptionPairList LanguagesLookupList
		{
			get { return languagesLookupList ?? (languagesLookupList = new CodeDescriptionPairList(OLookUpEditType.Language)); }
		}
		CodeDescriptionPairList languagesLookupList;

		[List("ProjectTypesList")]
		[MaxLength(3)]
		public ZString ProjectType
		{
			get { return projectType; }
			set
			{
				SetNonPersistentPropertyValue(ProjectTypeInfo, ref projectType, value);
				UpdatePathWithDefault();
				ValidateProjectType();
			}
		}
		ZString projectType;

		public ZPropertyInfo ProjectTypeInfo
		{
			get { return this.GetZPropertyInfo(nameof(ProjectType)); }
		}

		public CodeDescriptionPairList ProjectTypesList
		{
			get
			{
				if (projectTypesList == null)
				{
					projectTypesList = new CodeDescriptionPairList();
					projectTypesList.AddPair(ProjectTypeCodes.ALL, ProjectTypeDescription.ALL);
					projectTypesList.AddPair(ProjectTypeCodes.DocBuilder, ProjectTypeDescription.DocBuilder);
					projectTypesList.AddPair(ProjectTypeCodes.Report, ProjectTypeDescription.Report);
					projectTypesList.AddPair(ProjectTypeCodes.GUI, ProjectTypeDescription.GUI);
					projectTypesList.AddPair(ProjectTypeCodes.WebTracker, ProjectTypeDescription.WebTracker);
					projectTypesList.AddPair(ProjectTypeCodes.Billing, ProjectTypeDescription.Billing);
					projectTypesList.AddPair(ProjectTypeCodes.PAVE, ProjectTypeDescription.PAVE);
					projectTypesList.AddPair(ProjectTypeCodes.Customs, ProjectTypeDescription.Customs);
					projectTypesList.AddPair(ProjectTypeCodes.UpdateNotes, ProjectTypeDescription.UpdateNotes);
				}
				return projectTypesList;
			}
		}
		CodeDescriptionPairList projectTypesList;

		public static class ProjectTypeCodes
		{
			public const string ALL = "ALL";
			public const string DocBuilder = "DOC";
			public const string GUI = "GUI";
			public const string WebTracker = "WEB";
			public const string Report = "REP";
			public const string Billing = "BIL";
			public const string PAVE = "PAV";
			public const string Customs = "CUS";
			public const string UpdateNotes = "UPN";
		}

		public static class ProjectTypeDescription
		{
			public const string ALL = "All";
			public const string DocBuilder = "DocBuilder";
			public const string GUI = "GUI";
			public const string WebTracker = "WebTracker";
			public const string Report = "Reports";
			public const string Billing = "Billing";
			public const string PAVE = "PAVE";
			public const string Customs = "Customs";
			public const string UpdateNotes = "UpdateNotes";
		}

		public ZBool UntranslatedOnly
		{
			get { return untranslatedOnly; }
			set { SetNonPersistentPropertyValue(UntranslatedOnlyInfo, ref untranslatedOnly, value); }
		}
		ZBool untranslatedOnly;

		public ZPropertyInfo UntranslatedOnlyInfo
		{
			get { return this.GetZPropertyInfo(nameof(UntranslatedOnly)); }
		}

		public ZBool ExportGapList
		{
			get { return exportGapList; }
			set { SetNonPersistentPropertyValue(ExportGapListInfo, ref exportGapList, value); }
		}
		ZBool exportGapList;

		public ZPropertyInfo ExportGapListInfo
		{
			get { return this.GetZPropertyInfo(nameof(ExportGapList)); }
		}

		public ZBool ImportNewDuplicateTranslations
		{
			get { return importNewDuplicateTranslations; }
			set { SetNonPersistentPropertyValue(ImportNewDuplicateTranslationsInfo, ref importNewDuplicateTranslations, value); }
		}
		ZBool importNewDuplicateTranslations;

		public ZPropertyInfo ImportNewDuplicateTranslationsInfo
		{
			get { return this.GetZPropertyInfo(nameof(ImportNewDuplicateTranslations)); }
		}

		public ZBool Pretranslate
		{
			get { return pretranslate; }
			set { SetNonPersistentPropertyValue(PretranslateInfo, ref pretranslate, value); }
		}
		ZBool pretranslate = true;

		public ZPropertyInfo PretranslateInfo
		{
			get { return this.GetZPropertyInfo(nameof(Pretranslate)); }
		}

		[MaxLength(255)]
		public ZString ProjectPath
		{
			get { return projectPath; }
			set
			{
				SetNonPersistentPropertyValue(ProjectPathInfo, ref projectPath, value);
				if (!settingDefault)
				{
					isDefault = false;
					ValidateProjectType();
				}
			}
		}
		ZString projectPath;
		bool isDefault = true;
		bool settingDefault;

		public ZPropertyInfo ProjectPathInfo
		{
			get { return this.GetZPropertyInfo(nameof(ProjectPath)); }
		}

		void UpdatePathWithDefault()
		{
			if (isDefault)
			{
				settingDefault = true;
				ProjectPath = Path.Combine(DefaultProjectDirectory, ProjectTypesList.GetDescriptionFromCode(ProjectType) + (SelectedLanguages.Count == 1 ? " " + Culture.GetCultureForLanguage(SelectedLanguages[0]).ToString() : "") + " " + ZDateTime.Now.ToString("yyyy-MM-dd"));
				settingDefault = false;
			}
		}

		string DefaultProjectDirectory
		{
			get { return @"\\sdl\Exchange\Projects\"; }
		}

		protected override void RunPreSaveValidationCore()
		{
			ValidateLanguages();
			ValidateProjectType();
			ValidateProjectPath();
			base.RunPreSaveValidationCore();
		}

		void ValidateLanguages()
		{
			this.ClearRowNotifications();
			if (SelectedLanguages.Count == 0)
			{
				this.AddRowError("You must select at least one language");
			}
		}

		void ValidateProjectType()
		{
			ProjectTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ProjectTypeInfo);
		}

		void ValidateProjectPath()
		{
			ProjectPathInfo.ClearAllNotifications();
		}

		public bool Successful
		{
			get;
			set;
		}

		public string ErrorDetails
		{
			get;
			set;
		}
	}
}

#endif
