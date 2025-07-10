using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[XmlRoot("Statements")]
	public class ExportStatementSettingCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ExportStatementSettingCollection()
			: this(null)
		{
		}

		public ExportStatementSettingCollection(CountryExportStatementSetting parent)
		{
			this.Parent = parent;
		}

		readonly CountryExportStatementSetting Parent;

		public override void Add(BusinessObject businessObject)
		{
			ExportStatementSetting statement = businessObject as ExportStatementSetting;
			if (statement != null)
			{
				statement.Parent = Parent;
			}
			base.Add(businessObject);
		}

		public new ExportStatementSetting this[int index]
		{
			get { return (ExportStatementSetting)Elements[index]; }
		}

		public ExportStatementSetting this[string name]
		{
			get
			{
				foreach (ExportStatementSetting setting in this)
				{
					if (setting.Code == name)
					{
						return setting;
					}
				}
				return null;
			}
		}

		public new ExportStatementSetting AddNew()
		{
			return (ExportStatementSetting)base.AddNew();
		}

		public bool IsDuplicateSetting(ExportStatementSetting settingToCheck)
		{
			bool result = false;

			foreach (ExportStatementSetting setting in this)
			{
				if (setting != settingToCheck && setting.Code == settingToCheck.Code)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ExportStatementSettingCollection(Parent);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ExportStatementSetting(Parent);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			HasChanges = true; //This is a work-around for the bug causes HasChanges not to get set when removing an entry from this collection
		}

		public override bool ReadOnly => IsUsaOrTerritory && UsExportStatementSettings.ReferenceSet.SetEquals(this.Cast<ExportStatementSetting>());

		bool IsUsaOrTerritory => Parent != null && Constants.CountryCodes.IsUsaOrTerritory(Parent.CountryCode);
	}
}
