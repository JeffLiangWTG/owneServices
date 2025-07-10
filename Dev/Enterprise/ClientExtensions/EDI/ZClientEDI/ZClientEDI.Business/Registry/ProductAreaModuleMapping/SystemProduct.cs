using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.CustomerService.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Client.EDI.Registry.Business
{
	[CodeProperty("Code"), DescriptionProperty("Description")]
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class SystemProduct : AutoSystemProduct, ICanDelete
	{
		public SystemProduct()
		{
		}

		public SystemProduct(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new SystemProduct(fallbackLevel, factory);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			var product = (SystemProduct)clone;
			product.IsProductReadOnly = IsProductReadOnly;
			product.moduleMappings = (ProductAreaModuleMappingCollection)ModuleMappings.Clone(product.CurrentFallbackLevel, product.Factory);
			product.serviceTypeModuleMappings = (ServiceTypeProductAreaModuleMappingCollection)ServiceTypeModuleMappings.Clone(product.CurrentFallbackLevel, product.Factory);
			product.serviceTypeModuleMappings.ProductCode = Code;
			product.serviceTypeModuleMappings.ProductCriticality = ProductCriticality;
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			Enabled = true;
		}

		public override ZString Code
		{
			get { return base.Code; }
			set
			{
				base.Code = value;
				if (serviceTypeModuleMappings != null && serviceTypeModuleMappings.ProductCode != value)
				{
					ServiceTypeModuleMappings.ProductCode = value;
				}
				codeExisting = value;
			}
		}

		public bool Code_ReadOnly
		{
			get { return IsProductReadOnly; }
		}

		public bool CodeExisting_ReadOnly
		{
			get { return IsProductReadOnly; }
		}

		public bool Description_ReadOnly
		{
			get { return IsProductReadOnly; }
		}

		public bool Enabled_ReadOnly
		{
			get { return IsProductReadOnly; }
		}

		ZString codeExisting;

		[List("ProductList")]
		[MaxLength(Schema.CodeMaxLength)]
		public ZString CodeExisting
		{
			get { return codeExisting; }
			set
			{
				if (!IsCopying && (!Code.EqualsIgnoringCase(value) || (string.IsNullOrEmpty(value))) && codeExisting != value)
				{
					CheckMaximumLength(CodeExistingInfo, value);
					SetNonPersistentPropertyValue(CodeExistingInfo, ref codeExisting, value);

					if (ProductList.ContainsCode(value))
					{
						Code = value;
						Description = ProductList.GetDescriptionFromCode(value);
					}

					ValidateCodeExisting();
				}
			}
		}

		public virtual ZPropertyInfo CodeExistingInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(CodeExisting));
			}
		}

		#region ProductList

		CodeDescriptionPairList productList;
		public ICodeDescriptionPairList ProductList
		{
			get
			{
				if (productList == null)
				{
					productList = new CodeDescriptionPairList();
					foreach (SystemProduct product in EDIDataRegistry.Instance.SystemProductMappings.Value)
					{
						productList.Add(new CodeDescriptionPair(product.Code.ToString(), product.Description));
					}
				}

				return productList;
			}
		}

		public ModuleListType ProductCriticality { get; set; }

		#endregion

		#region Module Mappings

		ProductAreaModuleMappingCollection moduleMappings;

		[BusinessObjectTestExclude] // These properties implement setters on collections, which is generally not advised.
		public ProductAreaModuleMappingCollection ModuleMappings
		{
			get
			{
				if (moduleMappings == null)
				{
					moduleMappings = new ProductAreaModuleMappingCollection();
					RegisterEditableChildObject(moduleMappings);
				}

				return moduleMappings;
			}
			private set
			{
				UnRegisterEditableChildObject(moduleMappings);
				moduleMappings = value;
				RegisterEditableChildObject(moduleMappings);
			}
		}

		#endregion

		#region Service Type Module Mappings

		ServiceTypeProductAreaModuleMappingCollection serviceTypeModuleMappings;

		[BusinessObjectTestExclude] // These properties implement setters on collections, which is generally not advised.
		public ServiceTypeProductAreaModuleMappingCollection ServiceTypeModuleMappings
		{
			get
			{
				if (serviceTypeModuleMappings == null)
				{
					serviceTypeModuleMappings = new ServiceTypeProductAreaModuleMappingCollection(Code, ProductCriticality);
					RegisterEditableChildObject(serviceTypeModuleMappings);
				}

				return serviceTypeModuleMappings;
			}
			private set
			{
				UnRegisterEditableChildObject(serviceTypeModuleMappings);
				serviceTypeModuleMappings = value;
				RegisterEditableChildObject(serviceTypeModuleMappings);
			}
		}

		#endregion

		#region XML Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			ModuleMappingCollectionSerialiser.Serialize(writer, ModuleMappings);
			ServiceTypeModuleMappingsSerialiser.Serialize(writer, ServiceTypeModuleMappings);
		}

		protected override void ReadElements(XmlReaderWrapper readerWrapper)
		{
			base.ReadElements(readerWrapper);
			ModuleMappings = (ProductAreaModuleMappingCollection)ModuleMappingCollectionSerialiser.Deserialize(readerWrapper);
			ServiceTypeModuleMappings = (ServiceTypeProductAreaModuleMappingCollection)ServiceTypeModuleMappingsSerialiser.Deserialize(readerWrapper);
			ServiceTypeModuleMappings.ProductCode = Code;
			ServiceTypeModuleMappings.ProductCriticality = ProductCriticality;
		}

		ZXmlSerializer ModuleMappingCollectionSerialiser
		{
			get
			{
				if (sourceModuleMappingCollectionSerialiser == null)
				{
					sourceModuleMappingCollectionSerialiser = ZXmlSerializer.New(typeof(ProductAreaModuleMappingCollection));
				}
				return sourceModuleMappingCollectionSerialiser;
			}
		}
		ZXmlSerializer sourceModuleMappingCollectionSerialiser;

		ZXmlSerializer ServiceTypeModuleMappingsSerialiser
							=> serviceTypeModuleMappingsSerialiser ?? (serviceTypeModuleMappingsSerialiser = ZXmlSerializer.New(typeof(ServiceTypeProductAreaModuleMappingCollection)));
		ZXmlSerializer serviceTypeModuleMappingsSerialiser;

		#endregion

		public bool IsProductReadOnly { get; set; }

		#region ICanDelete

		public override bool CanDelete
		{
			get { return !IsProductReadOnly; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return (NoResString)"Product is hard-coded and can not be removed via registry."; }
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCodeExisting();

			ServiceTypeModuleMappings.RunPreSaveValidation();
			ModuleMappings.RunPreSaveValidation();

			if (ServiceTypeModuleMappings.HasErrors())
			{
				foreach (var item in ServiceTypeModuleMappings.GetErrors())
				{
					if (!this.GetErrors().Contains(item.Message))
					{
						this.AddRowError(item.Message);
					}
				}
			}
		}

		public override void ValidateCode()
		{
			base.ValidateCode();
			MandatoryValidation.CheckEntered(CodeInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(CodeInfo);
		}

		public void ValidateCodeExisting()
		{
			if (CodeExisting.IsEmpty || CodeExisting.EqualsIgnoringCase(Code))
			{
				CodeExistingInfo.ClearAllNotifications();
				return;
			}

			ListValidation.ErrorIfInvalidCode(CodeExistingInfo, ProductList);
		}

		public override void ValidateDescription()
		{
			base.ValidateDescription();
			if (!IsProductReadOnly)
			{
				MandatoryValidation.CheckEntered(DescriptionInfo);
			}
		}

		#endregion
	}
}

