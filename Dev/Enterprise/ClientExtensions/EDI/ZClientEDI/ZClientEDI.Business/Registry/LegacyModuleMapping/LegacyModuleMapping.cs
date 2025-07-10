using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.CustomerService.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class LegacyModuleMapping : AutoLegacyModuleMapping
	{
		public LegacyModuleMapping()
			: this(ModuleListType.MenuSection)
		{
		}

		public LegacyModuleMapping(ModuleListType legacyModuleType)
		{
			LegacyModuleType = legacyModuleType;
			CachedModuleMappingList = new Lazy<CodeDescriptionPairList>(() => Lookups.ModuleMappingList);
		}

		public ModuleListType LegacyModuleType
		{
			get; set;
		}

		#region Properties

		[List("Lookups.CriticalityList")]
		public override ZString CriticalityMapping
		{
			get { return base.CriticalityMapping; }
			set { base.CriticalityMapping = value; }
		}

		[List("Lookups.ModuleMappingList")]
		public override ZString ModuleMapping
		{
			get { return base.ModuleMapping; }
			set { base.ModuleMapping = value; }
		}

		public ModuleListType ModuleTypeMapping
		{
			get
			{
				return string.IsNullOrEmpty(CriticalityMapping) ? LegacyModuleType : IncidentApprovalLookups.GetModuleListType(CriticalityMapping);
			}
		}

		[ThreadSafe] // Property used only to display a value in the grid
		public ZString ModuleMappingDescription
		{
			get { return CachedModuleMappingList.Value.GetDescriptionFromCode(ModuleMapping); }
		}

		readonly Lazy<CodeDescriptionPairList> CachedModuleMappingList;

		[List("Lookups.CountryList")]
		public override ZString CountryMapping
		{
			get { return base.CountryMapping; }
			set { base.CountryMapping = value; }
		}

		#endregion

		#region Validation

		public override void ValidateCode()
		{
			base.ValidateCode();
			MandatoryValidation.CheckEntered(CodeInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(CodeInfo);

			var productAreaModuleMappingsRegistryItem = EDIDataRegistry.GetProductAreaModuleMappingsRegistryItem(LegacyModuleType);
			if (productAreaModuleMappingsRegistryItem != null)
			{
				var activeModulesList = productAreaModuleMappingsRegistryItem.Value;
				var descriptionOfConflictingModule = activeModulesList.GetDescriptionFromCode(ProductTypes.Codes.Enterprise, Code);
				if (!string.IsNullOrEmpty(descriptionOfConflictingModule))
				{
					CodeInfo.AddError(string.Format(CultureInfo.CurrentCulture, "Code [{0}] is currently being used by Module [{1}]", Code, descriptionOfConflictingModule));
				}
			}
		}

		public override void ValidateCriticalityMapping()
		{
			base.ValidateCriticalityMapping();
			ListValidation.ErrorIfInvalidCode(CriticalityMappingInfo);
		}

		public override void ValidateModuleMapping()
		{
			base.ValidateModuleMapping();
			MandatoryValidation.CheckEntered(ModuleMappingInfo);
			ListValidation.ErrorIfInvalidCode(ModuleMappingInfo);
		}

		#endregion

		#region Implementation

		public LegacyModuleMappingLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new LegacyModuleMappingLookups(this);
					if (Globals.IsTest && DummyLookupsForTests != null)
					{
						lookups = DummyLookupsForTests(this);
					}
				}
				return lookups;
			}
			internal set { lookups = value; }
		}
		LegacyModuleMappingLookups lookups;

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		internal static Func<LegacyModuleMapping, LegacyModuleMappingLookups> DummyLookupsForTests;

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new LegacyModuleMapping(LegacyModuleType);
		}

		#endregion
	}
}
