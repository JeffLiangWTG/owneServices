using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.Business
{
	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class ModuleGuidModuleSpecifiedFilter : ModuleGuidFilter
	{
		static class Schema
		{
			public const string SelectedModule = "SelectedModule";
		}

		#region Constructors

		protected ModuleGuidModuleSpecifiedFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public ModuleGuidModuleSpecifiedFilter(ZString description, SchemaGuidColumn filterColumn, IEnumerable<ModuleIdentifier> moduleOptionsList = null)
			: base(description, ModuleIDs.NotAssigned, filterColumn, GetBusinessObjectList)
		{
			moduleOptionsListDelegate = () => { return moduleOptionsList ?? AllModules; };
		}

		public ModuleGuidModuleSpecifiedFilter(ZString description, SchemaGuidColumn filterColumn, GetModuleOptionsDelegate moduleOptionsListDelegate)
			: base(description, ModuleIDs.NotAssigned, filterColumn, GetBusinessObjectList)
		{
			this.moduleOptionsListDelegate = moduleOptionsListDelegate;
		}

		public ModuleGuidModuleSpecifiedFilter(ZString description, SchemaGuidColumn filterColumn, GetCodeDescriptionModuleFilterPairOptionsDelegate codeDescriptionModuleFilterPairOptionsDelegate)
			: base(description, ModuleIDs.NotAssigned, filterColumn, GetBusinessObjectList)
		{
			CodeDescriptionModuleFilterPairOptionsListDelegate = codeDescriptionModuleFilterPairOptionsDelegate;
		}

		#endregion

		#region ModuleFilter Overrides

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			selectedFilterPair = ((ModuleGuidModuleSpecifiedFilter)filterToCopyFrom).selectedFilterPair;
			base.CopyPersistantValuesFromFilter(filterToCopyFrom);
		}

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new ModuleGuidModuleSpecifiedFilter(category, parentCollection);
		}

		protected override void ClearCore()
		{
			base.ClearCore();
			SetModuleId(DefaultCodeDescriptionModuleFilterPair);
		}

		protected override void OnComparisonOperatorChanged()
		{
			base.OnComparisonOperatorChanged();

			if (Property_ReadOnly)
			{
				Property = DefaultProperty;
			}
			if (SelectedFilters_ReadOnly)
			{
				ClearSelectedFilters();
			}
			if (SelectedModule_ReadOnly)
			{
				SetModuleId(DefaultCodeDescriptionModuleFilterPair);
			}
		}

		protected override bool Property_ReadOnly => base.Property_ReadOnly || ModuleId == DefaultModule;

		protected override bool SelectedFilters_ReadOnly => base.SelectedFilters_ReadOnly || ModuleId == DefaultModule;

		protected override FilterStripBusinessObject GetNewSelectedFilters(StmModuleFilter layoutToLoad)
		{
			return HasValidModuleInCurrentContext ? base.GetNewSelectedFilters(layoutToLoad) : null;
		}

		#endregion

		#region Persisted Values

		public ZPropertyInfo SelectedModuleInfo => GetZPropertyInfo(Schema.SelectedModule);

		[BusinessObjectMaxLengthTestExclude]
		public ZString SelectedModule
		{
			get
			{
				return selectedFilterPair.Code;
			}
			set
			{
				var selected = GetCodeDescriptionModuleFilterPair(value);

				SetModuleId(selected);
			}
		}

		CodeDescriptionModuleFilterPair selectedFilterPair = DefaultCodeDescriptionModuleFilterPair;

		internal readonly ModuleIdentifier DefaultModule = DefaultCodeDescriptionModuleFilterPair.Module;
		static CodeDescriptionModuleFilterPair DefaultCodeDescriptionModuleFilterPair => CodeDescriptionModuleFilterPair.DefaultCodeDescriptionModule;

		void SetModuleId(CodeDescriptionModuleFilterPair filterPair)
		{
			var oldModule = selectedFilterPair.Module;
			selectedFilterPair = filterPair;

			if (!IsValidationSuspended)
			{
				Validation.ValidateSelectedModule();
			}

			SelectedModuleInfo.RefreshBinding();
			OnModuleSelected(oldModule, filterPair.Module);
		}

		void OnModuleSelected(ModuleIdentifier oldModule, ModuleIdentifier newModule)
		{
			if (oldModule != newModule)
			{
				Property = DefaultProperty;
				ClearSelectedFilters();
				InvalidateCachedQuery();
			}

			ModuleSelected?.Invoke(this, new ModuleSelectedEventArgs(oldModule, newModule));
		}

		public event EventHandler<ModuleSelectedEventArgs> ModuleSelected;

		public class ModuleSelectedEventArgs : EventArgs
		{
			public ModuleIdentifier OldModule { get; private set; }
			public ModuleIdentifier NewModule { get; private set; }

			public ModuleSelectedEventArgs(ModuleIdentifier oldModule, ModuleIdentifier newModule)
			{
				OldModule = oldModule;
				NewModule = newModule;
			}
		}

		protected override ModuleIdentifier GetModuleIdCore()
		{
			return selectedFilterPair.Module;
		}

		protected virtual bool SelectedModule_ReadOnly => ShouldComparisonOperatorCauseReadOnly();

		protected override bool HasValidModuleInCurrentContextCore => base.HasValidModuleInCurrentContextCore && ModuleOptions.ContainsCode(SelectedModule);

		#endregion

		#region Lists

		static IBusinessObjectCollection GetBusinessObjectList(ModuleFilterWithList currentModuleFilter)
		{
			var filter = currentModuleFilter as ModuleGuidModuleSpecifiedFilter;

			if (filter == null || !filter.HasValidModuleInCurrentContext)
			{
				return null;
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(filter.ModuleId))
			{
				return module.GridCollection;
			}
		}

		readonly GetModuleOptionsDelegate moduleOptionsListDelegate;
		public delegate IEnumerable<ModuleIdentifier> GetModuleOptionsDelegate();
		readonly GetCodeDescriptionModuleFilterPairOptionsDelegate CodeDescriptionModuleFilterPairOptionsListDelegate;
		public delegate IEnumerable<CodeDescriptionModuleFilterPair> GetCodeDescriptionModuleFilterPairOptionsDelegate();

		public CodeDescriptionPairList ModuleOptions
		{
			get
			{
				if (moduleOptions == null && CodeDescriptionModuleFilterPairOptionsList != null)
				{
					moduleOptions = new CodeDescriptionPairList();
					var result = CodeDescriptionModuleFilterPairOptionsList.Select(x => new CodeDescriptionPair(x.Code, x.Description));
					result.ForEach(x => moduleOptions.Add(x));
				}
				return moduleOptions;
			}
		}
		CodeDescriptionPairList moduleOptions;

		IEnumerable<CodeDescriptionModuleFilterPair> CodeDescriptionModuleFilterPairOptionsList
		{
			get
			{
				if (codeDescriptionModuleFilterPairOptionsList == null)
				{
					if (CodeDescriptionModuleFilterPairOptionsListDelegate != null)
					{
						codeDescriptionModuleFilterPairOptionsList = GetValidCodeDescriptionModuleFilterPairList(CodeDescriptionModuleFilterPairOptionsListDelegate.Invoke());
					}
					else if (moduleOptionsListDelegate != null)
					{
						codeDescriptionModuleFilterPairOptionsList = GetValidCodeDescriptionModuleFilterPairList(moduleOptionsListDelegate.Invoke());
					}
				}
				return codeDescriptionModuleFilterPairOptionsList;
			}
		}
		IEnumerable<CodeDescriptionModuleFilterPair> codeDescriptionModuleFilterPairOptionsList;

		IEnumerable<CodeDescriptionModuleFilterPair> GetValidCodeDescriptionModuleFilterPairList(IEnumerable<ModuleIdentifier> modules)
		{
			return GetValidCodeDescriptionModuleFilterPairList(modules.Select(x => new CodeDescriptionModuleFilterPair(x.Name, x.Description, x)));
		}

		IEnumerable<CodeDescriptionModuleFilterPair> GetValidCodeDescriptionModuleFilterPairList(IEnumerable<CodeDescriptionModuleFilterPair> filterPairs)
		{
			var validFilterPairList = new List<CodeDescriptionModuleFilterPair>();
			var moduleArray = filterPairs.Select(x => x.Module);
			var validModules = GetValidModulesForFilter(moduleArray);

			foreach (var filterPair in filterPairs.Where(x => validModules.Contains(x.Module.Name)).OrderBy(m => m.Module.Name))
			{
				validFilterPairList.Add(filterPair);
			}

			return validFilterPairList;
		}

		IEnumerable<string> GetValidModulesForFilter(IEnumerable<ModuleIdentifier> modules)
		{
			if (validModulesForFilter == null)
			{
				validModulesForFilter = new List<string>();

				foreach (var id in modules.Where(id => id != ModuleIDs.NotAssigned && ZModuleFactory.Instance.IsZFilterGridModule(id)))
				{
					validModulesForFilter.Add(id.Name);
				}
			}

			return validModulesForFilter;
		}
		List<string> validModulesForFilter;

		CodeDescriptionModuleFilterPair GetCodeDescriptionModuleFilterPair(string filterPairCode)
		{
			CodeDescriptionModuleFilterPair filterPair;
			string code;
			string description;
			string moduleName;

			var result = CodeDescriptionModuleFilterPairOptionsList?.Where(x => x.Code == filterPairCode || x.Module.Name == filterPairCode)?.FirstOrDefault();
			if (result == null)
			{
				code = filterPairCode;
				description = filterPairCode;
				moduleName = filterPairCode;
			}
			else
			{
				code = result.Code;
				description = result.Description;
				moduleName = result.Module.Name;
			}

			var module = AllModules.FirstOrDefault(x => string.Equals(x.Name, moduleName, StringComparison.OrdinalIgnoreCase));
			filterPair = module == null ? DefaultCodeDescriptionModuleFilterPair : new CodeDescriptionModuleFilterPair(code, description, module);
			return filterPair;
		}

		IEnumerable<ModuleIdentifier> AllModules => allModules ?? (allModules = ModuleIDs.AllIncludingClientModules);
		IEnumerable<ModuleIdentifier> allModules;

		#endregion

		#region Serialisation

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString(Schema.SelectedModule, ModuleId.Name);

			base.SerializePropertiesToXml(writer);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			if (reader.Name == Schema.SelectedModule)
			{
				selectedFilterPair = GetCodeDescriptionModuleFilterPair(reader.ReadElementString(Schema.SelectedModule));

				if (!IsValidationSuspended)
				{
					Validation.ValidateSelectedModule();
				}
			}

			base.DeserializePropertiesFromXml(reader);
		}

		#endregion

		#region Validation

		public Validation SelectedModuleValidation
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return selectedModuleValidation; }
			[System.Diagnostics.DebuggerStepThrough]
			set { selectedModuleValidation = value; }
		}
		Validation selectedModuleValidation;

		public new ModuleGuidModuleSpecifiedFilterValidation Validation => (ModuleGuidModuleSpecifiedFilterValidation)GetNewValidation();

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ModuleGuidModuleSpecifiedFilterValidation(this);
		}

		public class ModuleGuidModuleSpecifiedFilterValidation : ModuleGuidFilterValidation
		{
			public ModuleGuidModuleSpecifiedFilterValidation(ModuleGuidModuleSpecifiedFilter parent)
				: base(parent)
			{
				this.parent = parent;
			}

			public void ValidateSelectedModule()
			{
				ValidateCalculatedProperty(parent.SelectedModuleInfo);
			}

			protected void CheckSelectedModule()
			{
				if (!parent.SelectedModule_ReadOnly)
				{
					var selectedCode = parent.selectedFilterPair.Code;
					if (parent.selectedFilterPair.Module == parent.DefaultModule)
					{
						parent.SelectedModuleInfo.AddError(Res.GetString("f3912072-f749-4b0e-9638-1b6aed1a1c31", "Please select a module."));
					}
					else if (!parent.ModuleOptions.ContainsCode(selectedCode))
					{
						parent.SelectedModuleInfo.AddError(Res.GetString("34d16b6d-d838-43b7-b189-d5a2df5a7107", "Please select a supported module."));
					}
					else
					{
						if (string.Equals(parent.ComparisonOperator, ModuleTextFilter.ComparisonConstants.FiltersMatch, StringComparison.OrdinalIgnoreCase)
							&& ModuleIDsNotAllowedToAddCompanyRelatedFilters.Any(module => string.Equals(selectedCode, module.Name, StringComparison.OrdinalIgnoreCase))
							&& !ObjectFactory.Get<ITagRulePolicy>().ShouldAddCompanyRelatedFilters)
						{
							parent.SelectedModuleInfo.AddError(Res.GetString("67771dc3-c92c-4f6f-b774-6d5de10d6dbf", "Company filters are not allowed for this module."));
						}
					}
				}
			}

			IEnumerable<ModuleIdentifier> ModuleIDsNotAllowedToAddCompanyRelatedFilters
			{
				get
				{
					yield return ModuleIDs.Customs.AU.HouseSeaCargo;
				}
			}

			public override void ValidateAll()
			{
				if (parent.ModuleId != parent.DefaultModule)
				{
					base.ValidateAll();
				}

				ValidateSelectedModule();
			}

			public override Type AutoValidationType => GetType();

			readonly ModuleGuidModuleSpecifiedFilter parent;
		}

		#endregion

		#region For Test
#if DEBUG

		public void SetModuleId_ForTest(ModuleIdentifier moduleId)
		{
			SetModuleId_ForTestCore(moduleId);
		}

		protected virtual void SetModuleId_ForTestCore(ModuleIdentifier moduleId)
		{
			SelectedModule = moduleId.Name; // At least one subclass uses something other than the module name for codes, so they need to override this for testing.
		}

#endif
		#endregion
	}
}
