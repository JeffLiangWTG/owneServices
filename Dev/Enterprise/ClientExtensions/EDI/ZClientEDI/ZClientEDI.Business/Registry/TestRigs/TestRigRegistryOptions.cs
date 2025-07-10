using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class TestRigRegistryOptions : RegistryBusinessObjectTemplate, IWorkItemLookupsParent
	{
		public TestRigRegistryOptions()
		{
		}

		public TestRigRegistryOptions(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Properties

		[List("Lookups.ActiveTypes")]
		[MaxLength(3)]
		[ResourceStringData("TestRigRegistryOptions|Product", Caption = "Product")]
		public ZString Product
		{
			get { return product; }
			set
			{
				SetNonPersistentPropertyValue(ProductInfo, ref product, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateProduct();
				}
			}
		}

		ZString product;

		public ZPropertyInfo ProductInfo => GetZPropertyInfo(nameof(Product));

		[List("Lookups.ActiveAreas")]
		[MaxLength(3)]
		[ResourceStringData("TestRigRegistryOptions|ProductArea", Caption = "Product Area")]
		public ZString ProductArea
		{
			get { return productArea; }
			set
			{
				SetNonPersistentPropertyValue(ProductAreaInfo, ref productArea, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateProductArea();
				}
			}
		}

		ZString productArea;

		public ZPropertyInfo ProductAreaInfo => GetZPropertyInfo(nameof(ProductArea));

		[List("Lookups.ActiveActivityTypes")]
		[MaxLength(3)]
		[ResourceStringData("TestRigRegistryOptions|Module", Caption = "Module")]
		public ZString Module
		{
			get { return module; }
			set
			{
				SetNonPersistentPropertyValue(ModuleInfo, ref module, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateModule();
				}
			}
		}

		ZString module;

		public ZPropertyInfo ModuleInfo => GetZPropertyInfo(nameof(Module));

		[List("Lookups.ActiveActivitySubtypes")]
		[MaxLength(3)]
		[ResourceStringData("TestRigRegistryOptions|ChangeType", Caption = "Change Type")]
		public ZString ChangeType
		{
			get { return changeType; }
			set
			{
				SetNonPersistentPropertyValue(ChangeTypeInfo, ref changeType, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateChangeType();
				}
			}
		}

		ZString changeType;

		public ZPropertyInfo ChangeTypeInfo => GetZPropertyInfo(nameof(ChangeType));

		[ResourceStringData("TestRigRegistryOptions|BackupFile", Caption = "Backup File")]
		[MaxLength(260)]
		public ZString BackupFile
		{
			get { return backupFile; }
			set
			{
				SetNonPersistentPropertyValue(BackupFileInfo, ref backupFile, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateBackupFile();
				}
			}
		}

		ZString backupFile;

		public ZPropertyInfo BackupFileInfo => GetZPropertyInfo(nameof(BackupFile));

		[ResourceStringData("TestRigRegistryOptions|AdditionalOptions", Caption = "Additional Options")]
		[MaxLength(1000)]
		public ZString AdditionalOptions
		{
			get { return additionalOptions; }
			set
			{
				SetNonPersistentPropertyValue(AdditionalOptionsInfo, ref additionalOptions, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateAdditionalOptions();
				}
			}
		}

		ZString additionalOptions;

		public ZPropertyInfo AdditionalOptionsInfo => GetZPropertyInfo(nameof(AdditionalOptions));

		#endregion

		#region Validation

		public TestRigRegistryOptionsValidation Validation => validation ?? (validation = new TestRigRegistryOptionsValidation(this));
		TestRigRegistryOptionsValidation validation;

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TestRigRegistryOptions(fallbackLevel, factory)
			{
				Product = Product,
				ProductArea = ProductArea,
				Module = Module,
				ChangeType = ChangeType,
				BackupFile = BackupFile,
				AdditionalOptions = AdditionalOptions
			};
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ReadElement(reader, ProductInfo);
			ReadElement(reader, ProductAreaInfo);
			ReadElement(reader, ModuleInfo);
			ReadElement(reader, ChangeTypeInfo);
			ReadElement(reader, BackupFileInfo);
			ReadElement(reader, AdditionalOptionsInfo);
		}

		static void ReadElement(XmlReaderWrapper reader, ZPropertyInfo property)
		{
			property.Value = new ZString(reader.ReadElementString(property.Name));
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			WriteElement(writer, ProductInfo);
			WriteElement(writer, ProductAreaInfo);
			WriteElement(writer, ModuleInfo);
			WriteElement(writer, ChangeTypeInfo);
			WriteElement(writer, BackupFileInfo);
			WriteElement(writer, AdditionalOptionsInfo);
		}

		static void WriteElement(XmlWriter writer, ZPropertyInfo property)
		{
			writer.WriteStartElement(property.Name);
			writer.WriteValue(property.Value.ToString());
			writer.WriteEndElement();
		}

		#region Lookups

		public WorkItemActualLookups Lookups => lookups ?? (lookups = new WorkItemActualLookups(this, CurrentFactory));
		WorkItemActualLookups lookups;

		ZString IWorkItemLookupsParent.WKI_WorkItemType => Product;
		ZString IWorkItemLookupsParent.WKI_WorkItemArea => ProductArea;
		ZString IWorkItemLookupsParent.WKI_ActivityType => Module;
		ZString IWorkItemLookupsParent.WKI_ActivitySubtype => ChangeType;

		#endregion
	}
}

