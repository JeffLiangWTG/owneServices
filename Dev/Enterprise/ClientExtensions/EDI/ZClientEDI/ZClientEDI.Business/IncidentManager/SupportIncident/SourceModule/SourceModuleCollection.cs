using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Modules;
using Enterprise.CustomerService.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class SourceModuleCollection : NonPersistentBusinessObjectCollection<SourceModule>
	{
		public SourceModule GetSourceModule(string code, string product)
		{
			foreach (SourceModule sourceModule in this)
			{
				if (sourceModule.Code.EqualsIgnoringCase(code) && (string.IsNullOrEmpty(sourceModule.Product) || sourceModule.Product.EqualsIgnoringCase(product)))
				{
					return sourceModule;
				}
			}

			return null;
		}

		public SourceModule GetSourceModule(string code, ModuleListType moduleListType, string product)
		{
			foreach (SourceModule sourceModule in this)
			{
				if (sourceModule.Code.EqualsIgnoringCase(code)
					&& SourceModule.IsModuleTypeCompatible(moduleListType, sourceModule.ModuleListType)
					&& (string.IsNullOrEmpty(sourceModule.Product) || sourceModule.Product.EqualsIgnoringCase(product)))
				{
					return sourceModule;
				}
			}
			return null;
		}

		public SourceModule GetSourceModule(IMainFormModule mainFormModule)
		{
			var code = mainFormModule.ModuleTreeID;
			var path = SourceModule.GetPath(mainFormModule);

			foreach (SourceModule sourceModule in this)
			{
				if (sourceModule.Code.EqualsIgnoringCase(code) && sourceModule.Path.EqualsIgnoringCase(path))
				{
					return sourceModule;
				}
			}
			return null;
		}

		public ZBool ContainsCodeAndPath(IMainFormModule mainFormModule)
		{
			return ContainsCodeAndPath(mainFormModule.ModuleTreeID, SourceModule.GetPath(mainFormModule));
		}

		public ZBool ContainsCodeAndPath(string code, string path)
		{
			foreach (SourceModule sourceModule in this)
			{
				if (sourceModule.Code.EqualsIgnoringCase(code) && sourceModule.Path.EqualsIgnoringCase(path))
				{
					return true;
				}
			}

			return false;
		}

		public SourceModule AddNew(IMainFormModule mainFormModule, bool isSelectableForOverride, bool isSearchable, string product = "")
		{
			var code = mainFormModule.ModuleTreeID;
			var description = mainFormModule.Description.GetUnresolvedString();
			var path = SourceModule.GetPath(mainFormModule);

			return AddNew(code, description, path, ModuleListType.DetectedMenuItem, mainFormModule.ParentSection.CustomerServiceMenuSectionCode, isSelectableForOverride, isSearchable, product);
		}

		public SourceModule AddNew(string code, string description, string path, string defaultMenuSection, bool isSelectableForOverride, bool isSearchable, string product = "")
		{
			return AddNew(code, description, path, ModuleListType.MenuSection, defaultMenuSection, isSelectableForOverride, isSearchable, product);
		}

		public SourceModule AddNew(string code, string description, string path, ModuleListType moduleListType, string defaultModule, bool isSelectableForOverride, bool isSearchable, string product = "")
		{
			var sourceModule = AddNew();
			using (sourceModule.GetValidationSuspender())
			{
				sourceModule.Code = code;
				sourceModule.Description = description;
				sourceModule.Path = path;
				sourceModule.ModuleListType = moduleListType;
				sourceModule.DefaultModule = defaultModule;
				sourceModule.IsSelectableForOverride = isSelectableForOverride;
				sourceModule.IsSearchable = isSearchable;
				sourceModule.Product = product;
			}

			return sourceModule;
		}

		public SourceModuleCollection GetCopy()
		{
			var result = new SourceModuleCollection();
			foreach (SourceModule sourceModule in this)
			{
				result.AddNew(sourceModule.Code, sourceModule.Description, sourceModule.Path, sourceModule.ModuleListType, sourceModule.DefaultModule, sourceModule.IsSelectableForOverride, sourceModule.IsSearchable, sourceModule.Product);
			}

			return result;
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SourceModule();
		}

		#endregion
	}
}

