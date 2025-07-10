using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.ZArchitecture.Business.Res;
using ResString = Enterprise.ZArchitecture.Business.ResString;

namespace Enterprise.ZArchitecture.DataMapping
{
	public abstract class ImportExportWizard : NonPersistentBusinessObject, IObsoleteValidation
	{
		protected static readonly int MaxPreviewLines = 50;

		protected ImportExportWizard(ISettingsStorage settingsStorage, IFileMapper fileMapper)
			: this(null, settingsStorage, fileMapper)
		{
		}

		protected ImportExportWizard(BusinessObjectFactory factory, ISettingsStorage settingsStorage, IFileMapper fileMapper)
			: base(factory)
		{
			this.settingsStorage = settingsStorage;
			this.fileMapper = fileMapper;
		}

		public abstract IEnumerable<IImportPropertyInfo> CollectionInfoProperties { get; }

		#region Properties

		#region Setting

		[MaxLength(50)]
		public ZString Setting
		{
			get { return setting; }
			set
			{
				if (setting != value)
				{
					SetNonPersistentPropertyValue(SettingInfo, ref setting, value);

					if (!IsValidationSuspended)
					{
						ValidateSetting();
						LoadSettings();
					}
				}
			}
		}

		ZString setting;

		public bool IsSettingSystemSetting
		{
			get { return Setting.Length > 2 && Setting.Left(1) == "[" && Setting.Right(1) == "]"; }
		}

		public ZPropertyInfo SettingInfo
		{
			get { return GetZPropertyInfo(nameof(Setting)); }
		}

		void ValidateSetting()
		{
			SettingInfo.ClearAllNotifications();
		}

		#endregion

		#region Delimiter

		[MaxLength(5)]
		public ZString Delimiter
		{
			get { return delimiter; }
			set
			{
				if (delimiter != value)
				{
					SetNonPersistentPropertyValue(DelimiterInfo, ref delimiter, value);

					if (!IsValidationSuspended)
					{
						ValidateDelimiter();
						DelimiterWasSet = true;
						RefreshFileContent(false);
					}
				}
			}
		}

		ZString delimiter = ",";
		protected bool DelimiterWasSet { get; private set; }

		public ZPropertyInfo DelimiterInfo
		{
			get { return GetZPropertyInfo(nameof(Delimiter)); }
		}

		void ValidateDelimiter()
		{
			DelimiterInfo.ClearAllNotifications();
		}

		protected virtual bool Delimiter_ReadOnly
		{
			get { return false; }
		}

		#endregion

		#region TextQualifier

		[MaxLength(1)]
		public ZString TextQualifier
		{
			get { return textQualifier; }
			set
			{
				if (textQualifier != value)
				{
					SetNonPersistentPropertyValue(TextQualifierInfo, ref textQualifier, value);

					if (!IsValidationSuspended)
					{
						ValidateTextQualifier();
						RefreshFileContent(false);
					}
				}
			}
		}

		ZString textQualifier = "\"";

		public ZPropertyInfo TextQualifierInfo
		{
			get { return GetZPropertyInfo(nameof(TextQualifier)); }
		}

		void ValidateTextQualifier()
		{
			TextQualifierInfo.ClearAllNotifications();
		}

		#endregion

		#region FileName

		[MaxLength(MAX_PATH)]
		public ZString FileName
		{
			get { return fileName; }
			set
			{
				if (fileName != value)
				{
					SetNonPersistentPropertyValue(FileNameInfo, ref fileName, value);

					if (!IsValidationSuspended)
					{
						ValidateFileName();
						RefreshFileContent(true);
					}
				}
			}
		}

		ZString fileName;

		public ZPropertyInfo FileNameInfo
		{
			get { return GetZPropertyInfo(nameof(FileName)); }
		}

		protected virtual void ValidateFileName()
		{
			FileNameInfo.ClearAllNotifications();
			if (ShouldValidateFileName)
			{
				MandatoryValidation.CheckEntered(FileNameInfo);
			}
		}

		protected virtual bool ShouldValidateFileName
		{
			get { return true; }
		}

		const int MAX_PATH = 260;

		public int FilterIndex { get; set; }

		#endregion

		#region CustomMapLists

		public CustomMapPairListWrapperCollection CustomMapLists
		{
			get
			{
				if (customMapLists == null)
				{
					customMapLists = new CustomMapPairListWrapperCollection();
				}

				return customMapLists;
			}
		}

		CustomMapPairListWrapperCollection customMapLists;

		#endregion

		#region FileContent

		protected virtual void RefreshFileContent(bool force)
		{
		}

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateSetting();
			ValidateDelimiter();
			ValidateTextQualifier();
			ValidateFileName();
		}

		#endregion

		#region Lookups

		#region Delimiters

		public CodeDescriptionPairList Delimiters
		{
			get
			{
				if (delimiters == null)
				{
					delimiters = new CodeDescriptionPairList();
					AddSupportedDelimiters(delimiters);
				}

				return delimiters;
			}
		}

		protected virtual void AddSupportedDelimiters(CodeDescriptionPairList delimiters)
		{
			delimiters.AddPair(SupportedDelimiter.Comma, ResString.GetMultilingualString("98e54087-728f-4e7a-bbc1-9b1792fa1d50", "Comma"));
			delimiters.AddPair(SupportedDelimiter.Tilde, ResString.GetMultilingualString("eec73253-6b59-477a-b5d0-5f00eb51dfa2", "Tilde"));
			delimiters.AddPair(SupportedDelimiter.Pipe, ResString.GetMultilingualString("7cfbc537-dfd3-4f7e-93e2-40adcf5d7942", "Pipe"));
			delimiters.AddPair(SupportedDelimiter.Space, ResString.GetMultilingualString("0d98c892-4469-4272-abf3-cdebe92ffd26", "Space")); // Code should not be translated
			delimiters.AddPair(SupportedDelimiter.Tab, ResString.GetMultilingualString("19f2dd9f-5482-452d-813d-cb862c04397d", "Tab")); // Code should not be translated
			delimiters.AddPair(SupportedDelimiter.Colon, ResString.GetMultilingualString("e1cbd552-fcc4-4445-9137-1f38a09ff783", "Colon"));
			delimiters.AddPair(SupportedDelimiter.Semicolon, ResString.GetMultilingualString("8c8e107b-625e-4617-8110-ef61f1a357b1", "Semicolon"));
		}

		CodeDescriptionPairList delimiters;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Delimiters")]
		public static class SupportedDelimiter
		{
			public const string Comma = ",";
			public const string Tilde = "~";
			public const string Pipe = "|";
			public const string Space = "space";
			public const string Tab = "tab";
			public const string Colon = ":";
			public const string Semicolon = ";";

			//Just for field mapping
			public const string Newline = "newline";
		}

		#endregion

		#region Settings

		public CodeDescriptionPairList Settings
		{
			get
			{
				if (savedSettings == null)
				{
					savedSettings = new CodeDescriptionPairList();
					foreach (string savedSetting in settingsStorage.GetSavedSettings())
					{
						savedSettings.AddPair(savedSetting);
					}
				}

				return savedSettings;
			}
		}

		CodeDescriptionPairList savedSettings;

		#endregion

		#endregion

		#region Serialization

		public abstract Type SettingsType { get; }
		protected abstract void GetSettingsCore(ImportExportWizardSettings wizardSettings);
		protected abstract void SetSettingsCore(ImportExportWizardSettings wizardSettings);

		public ImportExportWizardSettings GetSettings()
		{
			var wizardSettings = (ImportExportWizardSettings)Activator.CreateInstance(SettingsType);

			wizardSettings.Delimiter = Delimiter;
			wizardSettings.TextQualifier = TextQualifier;
			wizardSettings.FilterIndex = FilterIndex;

			var mapList = new List<CustomMapPairListSetting>();
			for (var i = 0; i < CustomMapLists.Count; i++)
			{
				var pairList = new List<CustomMapPairSetting>();
				for (var j = 0; j < CustomMapLists[i].List.Count; j++)
				{
					pairList.Add(new CustomMapPairSetting()
					{
						Input = CustomMapLists[i].List[j].Input,
						Output = CustomMapLists[i].List[j].Output,
					});
				}

				mapList.Add(new CustomMapPairListSetting()
				{
					Name = CustomMapLists[i].Name,
					Pairs = pairList
				});
			}
			wizardSettings.CustomMapLists = mapList;

			GetSettingsCore(wizardSettings);

			return wizardSettings;
		}

		public void SetSettings(ImportExportWizardSettings wizardSettings)
		{
			Delimiter = wizardSettings.Delimiter;
			TextQualifier = wizardSettings.TextQualifier;
			FilterIndex = wizardSettings.FilterIndex;

			bool hasTruncatedLines = false;
			ZStringBuilder warningMessages = new ZStringBuilder();
			foreach (CustomMapPairListSetting pairListSetting in wizardSettings.CustomMapLists)
			{
				CustomMapPairListWrapper pairListWrapper = CustomMapLists.Find(pairListSetting.Name);
				if (pairListWrapper != null)
				{
					pairListWrapper.List.RemoveAndDeleteAll();
				}
				else
				{
					pairListWrapper = CustomMapLists.AddNew();
					pairListWrapper.Name = pairListSetting.Name;
				}

				foreach (CustomMapPairSetting pairSetting in pairListSetting.Pairs)
				{
					CustomMapPair pair = pairListWrapper.List.AddNew();
					string inputValue = pairSetting.Input;
					string outputValue = pairSetting.Output;

					if (inputValue.Length > pair.InputInfo.MaxLength)
					{
						hasTruncatedLines = true;
						inputValue = inputValue.Substring(0, pair.InputInfo.MaxLength);
					}
					if (outputValue.Length > pair.OutputInfo.MaxLength)
					{
						hasTruncatedLines = true;
						outputValue = outputValue.Substring(0, pair.OutputInfo.MaxLength);
					}
					pair.Input = inputValue;
					pair.Output = outputValue;
				}
				if (hasTruncatedLines)
				{
					warningMessages.AppendLine(Res.GetString("142b36bd-0871-4566-afd5-390408512a89", "Values for custom mapping \"{0}\" were truncated", pairListSetting.Name));
					hasTruncatedLines = false;
				}
			}

			if (!warningMessages.IsEmpty)
			{
				Globals.Message.ShowWarning(warningMessages.ToString());
			}

			SetSettingsCore(wizardSettings);
			RefreshFileContent(false);
		}

		public void SaveSettings()
		{
			if (!Setting.IsEmpty && (!Settings.ContainsCode(Setting) || HasSettingsChanges()))
			{
				if (settingsStorage.HasSecurityRight())
				{
					settingsStorage.SaveSettings(Setting, GetSettings().AsXml());
				}
				else
				{
					settingsStorage.ShowSecurityError();
				}
			}
		}

		public void LoadSettings()
		{
			if (Setting.IsEmpty)
			{
				return;
			}

			int index = Settings.IndexOfCode(Setting);
			if (index >= 0)
			{
				string xml = settingsStorage.LoadSettings(Setting);
				if (xml != null)
				{
					ImportExportWizardSettings wizardSettings = (ImportExportWizardSettings)XmlSerializableSetting.FromXml(xml, SettingsType, false);
					if (wizardSettings != null)
					{
						SetSettings(wizardSettings);
					}
				}
			}
		}

		public void RemoveSettings()
		{
			if (Setting.IsEmpty)
			{
				return;
			}

			int index = Settings.IndexOfCode(Setting);
			if (index >= 0)
			{
				if (settingsStorage.HasSecurityRight())
				{
					settingsStorage.RemoveSettings(Setting);
					Settings.RemoveAt(index);
					Setting = ZString.Empty;
				}
				else
				{
					settingsStorage.ShowSecurityError();
				}
			}
		}

		public bool HasSettingsChanges()
		{
			if (!Setting.IsEmpty)
			{
				int index = Settings.IndexOfCode(Setting);
				if (index >= 0)
				{
					string xml = settingsStorage.LoadSettings(Setting);
					if (xml != null)
					{
						return xml != GetSettings().AsXml();
					}
				}
			}

			return false;
		}

		#endregion

		#region Progress

		public delegate bool ProgressChangedEventHandler(int percentComplete, string status);
		public event ProgressChangedEventHandler ProgressChanged;
		public
#if DEBUG
			virtual
#endif
			bool OnProgressChanged(int percentComplete, string status)
		{
			if (ProgressChanged != null)
			{
				return ProgressChanged(percentComplete, status);
			}

			return true;
		}

		#endregion

		#region DlrProxy

		public IDlrProxy DlrProxy
		{
			get
			{
				if (dlrProxy == null)
				{
					dlrProxy = ObjectFactory.Get<IDlrProxy>();
				}

				return dlrProxy;
			}
		}

		IDlrProxy dlrProxy;

		#endregion

		readonly ISettingsStorage settingsStorage;
		protected IFileMapper fileMapper;
	}
}
