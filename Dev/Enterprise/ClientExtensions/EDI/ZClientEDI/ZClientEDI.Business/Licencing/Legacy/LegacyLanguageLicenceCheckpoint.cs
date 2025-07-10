
using Enterprise.Licensing;
namespace Enterprise.Client.EDI.Licencing.Business
{
	internal class LegacyLanguageLicenceCheckpoint : LegacyLicenceCheckpoint
	{
		internal LegacyLanguageLicenceCheckpoint(string name, string displayName, LegacyLicence parentLicences, string parentName)
			: base(name, displayName, parentLicences, parentName, isManuallyEnabled: false)
		{
			var prefix = name.Substring(1);
			DocBuilderLanguageCheckpoint = new LegacyLanguageLicenceChildCheckpoint(prefix + "7", displayName + " - DocBuilder", parentLicences, name);
			GUILanguageCheckpoint = new LegacyLanguageLicenceChildCheckpoint(prefix + "8", displayName + " - GUI", parentLicences, name);
			WebTrackerLanguageCheckpoint = new LegacyLanguageLicenceChildCheckpoint(prefix + "9", displayName + " - WebTracker", parentLicences, name);
		}

		public override string GetDefaultLicenceValue()
		{
			return LicenceTypes.Codes.ODM;
		}

		public override string GetDefaultEnabledLicenceValue()
		{
			return LicenceTypes.Codes.ODM;
		}

		public LegacyLanguageLicenceChildCheckpoint DocBuilderLanguageCheckpoint { get; private set; }
		public LegacyLanguageLicenceChildCheckpoint GUILanguageCheckpoint { get; private set; }
		public LegacyLanguageLicenceChildCheckpoint WebTrackerLanguageCheckpoint { get; private set; }
	}

	public class LegacyLanguageLicenceChildCheckpoint : LegacyLicenceCheckpoint
	{
		internal LegacyLanguageLicenceChildCheckpoint(string name, string displayName, LegacyLicence parentLicences, string parentName)
			: base(name, displayName, parentLicences, parentName)
		{ }

		public override string GetDefaultLicenceValue()
		{
			return LicenceTypes.Codes.ODM;
		}

		public override string GetDefaultEnabledLicenceValue()
		{
			return LicenceTypes.Codes.ODM;
		}
	}
}
