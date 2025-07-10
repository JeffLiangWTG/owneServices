using Enterprise.Core.Environment;
using Enterprise.Integration.Licensing;

namespace Enterprise.Licensing
{
	public class LanguageLicenceCheckpoint : LicenceCheckpoint
	{
		internal LanguageLicenceCheckpoint(string name, string displayName, Licences parentLicences, LicenceCheckpoint parentCheckpoint)
			: base(name, displayName, parentLicences, parentCheckpoint, LicenceModuleCategories.Codes.None)
		{
			DocBuilderLanguageCheckpoint = new LanguageLicenceChildCheckpoint(name.Substring(1) + "7", displayName + " - DocBuilder", parentLicences, this);
			GUILanguageCheckpoint = new LanguageLicenceChildCheckpoint(name.Substring(1) + "8", displayName + " - GUI", parentLicences, this);
			WebTrackerLanguageCheckpoint = new LanguageLicenceChildCheckpoint(name.Substring(1) + "9", displayName + " - WebTracker", parentLicences, this);
		}

		public override ModuleLicenceType GetDefaultLicenceValue()
		{
			return ModuleLicenceType.ODM;
		}

		public override ModuleLicenceType GetDefaultEnabledLicenceValue()
		{
			return ModuleLicenceType.ODM;
		}

		public LanguageLicenceChildCheckpoint DocBuilderLanguageCheckpoint
		{
			get;
			private set;
		}

		public LanguageLicenceChildCheckpoint GUILanguageCheckpoint
		{
			get;
			private set;
		}

		public LanguageLicenceChildCheckpoint WebTrackerLanguageCheckpoint
		{
			get;
			private set;
		}
	}

	public class LanguageLicenceChildCheckpoint : LicenceCheckpoint
	{
		internal LanguageLicenceChildCheckpoint(string name, string displayName, Licences parentLicences, LicenceCheckpoint parentCheckpoint)
			: base(name, displayName, parentLicences, parentCheckpoint, LicenceModuleCategories.Codes.None)
		{ }

		public override ModuleLicenceType GetDefaultLicenceValue()
		{
			return ModuleLicenceType.ODM;
		}

		public override ModuleLicenceType GetDefaultEnabledLicenceValue()
		{
			return ModuleLicenceType.ODM;
		}
	}
}
