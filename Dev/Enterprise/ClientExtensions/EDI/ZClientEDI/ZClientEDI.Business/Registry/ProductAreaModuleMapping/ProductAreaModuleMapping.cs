using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class ProductAreaModuleMapping : AutoProductAreaModuleMapping, IProductAreaMapping
	{
		public ProductAreaModuleMapping() { }

		public ProductAreaModuleMapping(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		public ProductAreaModuleMappingLookups Lookups
		{
			get { return lookups ?? (lookups = new ProductAreaModuleMappingLookups(this)); }
		}
		ProductAreaModuleMappingLookups lookups;

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ProductAreaModuleMapping(fallbackLevel, factory);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			var mapping = (ProductAreaModuleMapping)clone;
			mapping.IsModuleReadOnly = IsModuleReadOnly;
			mapping.sourceModuleMappings = (ProductAreaSourceModuleMappingCollection)SourceModuleMappings.Clone(mapping.CurrentFallbackLevel, mapping.Factory);
		}

		#endregion

		#region Properties

		public bool ModuleCode_ReadOnly
		{
			get { return IsModuleReadOnly; }
		}

		public override ZString ModuleDescription
		{
			get => ModuleDescriptionMultilingual.GetUnresolvedString();
			set => ModuleDescriptionMultilingual = (NoResString)value;
		}

		[MaxLength(Schema.ModuleDescriptionMaxLength)]
		public MultilingualString ModuleDescriptionMultilingual
		{
			get => moduleDescriptionMultilingual ?? (NoResString)"";
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}

				CheckMaximumLength(ModuleDescriptionMultilingualInfo, value.GetUnresolvedString());
				SetNonPersistentPropertyValue(ModuleDescriptionMultilingualInfo, ref moduleDescriptionMultilingual, value, false);
				ModuleDescriptionMultilingualInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					ValidateModuleDescription();
				}
			}
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		MultilingualString moduleDescriptionMultilingual;

		public ZPropertyInfo ModuleDescriptionMultilingualInfo => GetZPropertyInfo(Schema.ModuleDescription);

		public bool ModuleDescription_ReadOnly
		{
			get { return IsModuleReadOnly; }
		}

		[List("Lookups.ProductAreaList")]
		public override ZString ProductArea
		{
			get { return base.ProductArea; }
			set { base.ProductArea = value; }
		}

		public ZString GetProductAreaWithSourceModule(string sourceModule)
		{
			var sourceModuleMapping = SourceModuleMappings.GetSourceModuleMapping(sourceModule);
			if (sourceModuleMapping != null)
			{
				return sourceModuleMapping.ProductArea;
			}

			return ProductArea;
		}

		public bool IsModuleReadOnly { get; set; }

		#endregion

		#region Source Module Mappings

		ProductAreaSourceModuleMappingCollection sourceModuleMappings;

		[BusinessObjectTestExclude] // These properties implement setters on collections, which is generally not advised.
		public ProductAreaSourceModuleMappingCollection SourceModuleMappings
		{
			get
			{
				if (sourceModuleMappings == null)
				{
					sourceModuleMappings = new ProductAreaSourceModuleMappingCollection();
					RegisterEditableChildObject(sourceModuleMappings);
				}

				return sourceModuleMappings;
			}
			private set
			{
				UnRegisterEditableChildObject(sourceModuleMappings);
				sourceModuleMappings = value;
				RegisterEditableChildObject(sourceModuleMappings);
			}
		}

		#endregion

		#region Validation

		public override void ValidateModuleCode()
		{
			base.ValidateModuleCode();
			MandatoryValidation.CheckEntered(ModuleCodeInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(ModuleCodeInfo);
		}

		public override void ValidateModuleDescription()
		{
			base.ValidateModuleDescription();
			if (!IsModuleReadOnly)
			{
				MandatoryValidation.CheckEntered(ModuleDescriptionInfo);
			}
		}

		public override void ValidateProductArea()
		{
			base.ValidateProductArea();
			ListValidation.ErrorIfInvalidCode(ProductAreaInfo);
		}

		#endregion

		#region XML Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			SourceModuleMappingCollectionSerialiser.Serialize(writer, SourceModuleMappings);
		}

		protected override void ReadElements(XmlReaderWrapper readerWrapper)
		{
			base.ReadElements(readerWrapper);
			SourceModuleMappings = (ProductAreaSourceModuleMappingCollection)SourceModuleMappingCollectionSerialiser.Deserialize(readerWrapper);
		}

		ZXmlSerializer sourceModuleMappingCollectionSerialiser;
		ZXmlSerializer SourceModuleMappingCollectionSerialiser
		{
			get
			{
				if (sourceModuleMappingCollectionSerialiser == null)
				{
					sourceModuleMappingCollectionSerialiser = ZXmlSerializer.New(typeof(ProductAreaSourceModuleMappingCollection));
				}
				return sourceModuleMappingCollectionSerialiser;
			}
		}

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get { return !IsModuleReadOnly; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return (NoResString)"Module is hard-coded and can not be removed via registry."; }
		}

		#endregion
	}
}

