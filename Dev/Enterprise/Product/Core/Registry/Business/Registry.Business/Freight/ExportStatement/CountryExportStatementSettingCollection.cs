using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CountryExportStatementSettingCollection : RegistryBusinessObjectCollectionTemplate
	{
		public CountryExportStatementSettingCollection()
		{
		}

		public new CountryExportStatementSetting this[int index]
		{
			get { return (CountryExportStatementSetting)(Elements[index]); }
		}

		public CountryExportStatementSetting this[string countryCode]
		{
			get
			{
				foreach (CountryExportStatementSetting countrySetting in this)
				{
					if (countrySetting.CountryCode == countryCode)
					{
						return countrySetting;
					}
				}
				return null;
			}
		}

		public new CountryExportStatementSetting AddNew()
		{
			return (CountryExportStatementSetting)base.AddNew();
		}

		public CodeDescriptionPairList GetExportStatementDescriptionPairListForCountry(string countryCode)
		{
			var result = new CodeDescriptionPairList();
			var countrySetting = this[countryCode];
			if (countrySetting != null)
			{
				foreach (ExportStatementSetting statement in countrySetting.Statements)
				{
					if (statement.Visibility == ExportStatementSetting.VisibilityList.UserDefined)
					{
						result.AddPair(statement.Code, statement.StatementDescription);
					}
				}
			}

			return result;
		}

		public CountryExportStatementSettingCollection GetMandatoryStatements(string countryCode)
		{
			var result = new CountryExportStatementSettingCollection();
			var countrySetting = this[countryCode];
			if (countrySetting != null)
			{
				foreach (ExportStatementSetting statement in countrySetting.Statements)
				{
					if (statement.Visibility == ExportStatementSetting.VisibilityList.Mandatory)
					{
						result.Add(statement);
					}
				}
			}

			return result;
		}

		public void AddDefaultValues(string countryCode)
		{
			switch (countryCode)
			{
				case Core.Constants.CountryCodes.UnitedStates:
				case Core.Constants.CountryCodes.PuertoRico:
				case Core.Constants.CountryCodes.Guam:
				case Core.Constants.CountryCodes.AmericanSamoa:
				case Core.Constants.CountryCodes.VirginIslands:
				case Core.Constants.CountryCodes.NorthernMarianaIslands:
					var setting = AddNew();
					setting.CountryCode = countryCode;
					setting.Statements.AddRange(UsExportStatementSettings.BuildReferenceSet(setting));
					break;

				default:
					return;
			}
		}

		public bool IsDuplicateSetting(CountryExportStatementSetting settingToCheck)
		{
			var result = false;
			foreach (CountryExportStatementSetting countrySetting in this)
			{
				if (countrySetting != settingToCheck && countrySetting.CountryCode == settingToCheck.CountryCode)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CountryExportStatementSetting();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CountryExportStatementSettingCollection();
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			HasChanges = true; //This is a work-around for the bug causes HasChanges not to get set when removing an CountrySetting from this collection
		}

		#endregion
	}
}
