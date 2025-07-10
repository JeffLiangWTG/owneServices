using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.CustomerService.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class ServiceTypeProductAreaModuleMapping : AutoProductAreaModuleMapping, IProductAreaMapping
	{
		public ServiceTypeProductAreaModuleMapping() { }

		public ServiceTypeProductAreaModuleMapping(FallbackLevel fallbackLevel, BusinessObjectFactory factory, string productCode = "", ModuleListType productCriticality = ModuleListType.Unspecified)
			: base(fallbackLevel, factory)
		{
			this.productCode = productCode;
			this.productCriticality = productCriticality;
		}

		public ServiceTypeProductAreaModuleMappingLookups Lookups => lookups ?? (lookups = new ServiceTypeProductAreaModuleMappingLookups(this));
		ServiceTypeProductAreaModuleMappingLookups lookups;

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ServiceTypeProductAreaModuleMapping(fallbackLevel, factory);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			var mapping = (ServiceTypeProductAreaModuleMapping)clone;
			mapping.IsModuleReadOnly = IsModuleReadOnly;
			mapping.serviceTypeMappings = (ServiceTypeModuleMappingCollection)ServiceTypeMappings.Clone(mapping.CurrentFallbackLevel, mapping.Factory);
			mapping.ProductCode = ProductCode;
			mapping.ProductCriticality = ProductCriticality;
		}

		#endregion

		#region Properties

		[List("Lookups.Modules")]
		public override ZString ModuleCode
		{
			get => base.ModuleCode;
			set
			{
				if (base.ModuleCode != value)
				{
					base.ModuleCode = value;
					SetInternalEnabledFromModule();
				}
			}
		}

		void SetInternalEnabledFromModule()
		{
			if (!IsInDatabase && !ModuleCode.IsEmpty && !ProductCode.IsEmpty)
			{
				var module = Lookups.GetModule(ModuleCode);
				if (module != null)
				{
					IsEnabled = module.IsEnabled;
					IsInternal = module.IsInternal;
				}
			}
		}

		public ZString CalculatedModuleDescription
		{
			get
			{
				var sourceModule = GetSourceModule();
				return sourceModule != null ? (ZString)sourceModule.Description : ZString.Empty;
			}
		}

		CodeDescriptionPair GetSourceModule()
								=> Lookups.Modules.Cast<CodeDescriptionPair>().FirstOrDefault(s => s.Code.Equals(ModuleCode, System.StringComparison.InvariantCultureIgnoreCase));

		[List("Lookups.ProductAreaList")]
		public override ZString ProductArea
		{
			get { return base.ProductArea; }
			set { base.ProductArea = value; }
		}

		public bool IsModuleReadOnly { get; set; }

		public ZString ProductCode
		{
			get { return productCode; }
			set { productCode = value; }
		}
		ZString productCode;

		public ModuleListType ProductCriticality
		{
			get { return productCriticality; }
			set { productCriticality = value; }
		}
		ModuleListType productCriticality;

		#endregion

		#region Service Type Mappings

		[BusinessObjectTestExclude] // These properties implement setters on collections, which is generally not advised.
		public ServiceTypeModuleMappingCollection ServiceTypeMappings
		{
			get
			{
				if (serviceTypeMappings == null)
				{
					serviceTypeMappings = new ServiceTypeModuleMappingCollection();
					RegisterEditableChildObject(serviceTypeMappings);
				}

				return serviceTypeMappings;
			}
			private set
			{
				UnRegisterEditableChildObject(serviceTypeMappings);
				serviceTypeMappings = value;
				RegisterEditableChildObject(serviceTypeMappings);
			}
		}
		ServiceTypeModuleMappingCollection serviceTypeMappings;

		#endregion

		#region Validation

		public override void ValidateModuleCode()
		{
			base.ValidateModuleCode();
			MandatoryValidation.CheckEntered(ModuleCodeInfo, Res.GetString("7c83a5de-7ded-4b2f-b35d-507dfcbcfeef", "module code. Enter Product Area first to see the modules."));
			ListValidation.ErrorIfInvalidCode(ModuleCodeInfo);
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
			ServiceTypeMappingCollectionSerialiser.Serialize(writer, ServiceTypeMappings);
		}

		protected override void ReadElements(XmlReaderWrapper readerWrapper)
		{
			base.ReadElements(readerWrapper);
			ServiceTypeMappings = (ServiceTypeModuleMappingCollection)ServiceTypeMappingCollectionSerialiser.Deserialize(readerWrapper);
		}

		ZXmlSerializer ServiceTypeMappingCollectionSerialiser
		{
			get
			{
				if (serviceTypeMappingCollectionSerialiser == null)
				{
					serviceTypeMappingCollectionSerialiser = ZXmlSerializer.New(typeof(ServiceTypeModuleMappingCollection));
				}
				return serviceTypeMappingCollectionSerialiser;
			}
		}
		ZXmlSerializer serviceTypeMappingCollectionSerialiser;

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

