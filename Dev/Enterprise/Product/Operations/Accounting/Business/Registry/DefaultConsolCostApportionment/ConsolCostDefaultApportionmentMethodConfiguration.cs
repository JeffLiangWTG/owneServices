using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class ConsolCostDefaultApportionmentMethodConfiguration : RegistryBusinessObjectTemplate
	{
		public ConsolCostDefaultApportionmentMethodConfiguration()
		{
		}

		public ConsolCostDefaultApportionmentMethodConfiguration(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

#if DEBUG
		public static ConsolCostDefaultApportionmentMethodConfiguration Create_ForTestOnly(string apportionment, string transportMode = ApportionmentMethod.AllCode,
			string containerMode = ApportionmentMethod.AllCode, string consolType = ApportionmentMethod.AllCode, string module = ApportionmentMethod.AllCode, string direction = ApportionmentMethod.AllCode)
		{
			var config = new ConsolCostDefaultApportionmentMethodConfiguration();
			var newMethod = new ConsolCostDefaultApportionmentMethod
			{
				Apportionment = apportionment,
				TransportMode = transportMode,
				ContainerMode = containerMode,
				ConsolType = consolType,
				Module = module,
				Direction = direction,
			};
			config.ConsolCostDefaultApportionmentMethodCollection.RemoveAll();
			config.ConsolCostDefaultApportionmentMethodCollection.Add(newMethod);
			return config;
		}
#endif

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ConsolCostDefaultApportionmentMethodConfiguration(fallbackLevel);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			ConsolCostDefaultApportionmentMethodConfiguration castedClone = (ConsolCostDefaultApportionmentMethodConfiguration)clone;
			if (ConsolCostDefaultApportionmentMethodCollection != null)
			{
				castedClone.consolCostDefaultApportionmentMethodCollection = (ConsolCostDefaultApportionmentMethodCollection)ConsolCostDefaultApportionmentMethodCollection.Clone(castedClone.CurrentFallbackLevel, castedClone.Factory);
				castedClone.RegisterEditableChildObject(castedClone.ConsolCostDefaultApportionmentMethodCollection);
			}
		}

		#region Bound Properties

		#region ConsolCostDefaultApportionmentMethodCollection

		public ConsolCostDefaultApportionmentMethodCollection ConsolCostDefaultApportionmentMethodCollection
		{
			get
			{
				if (consolCostDefaultApportionmentMethodCollection == null)
				{
					consolCostDefaultApportionmentMethodCollection = new ConsolCostDefaultApportionmentMethodCollection();
					var defaultMethod = new ConsolCostDefaultApportionmentMethod();
					defaultMethod.ParentCollection = consolCostDefaultApportionmentMethodCollection;
					defaultMethod.Apportionment = AllocationMethod.ChargeableUnits;
					consolCostDefaultApportionmentMethodCollection.Add(defaultMethod);
					RegisterEditableChildObject(ConsolCostDefaultApportionmentMethodCollection);
				}
				return consolCostDefaultApportionmentMethodCollection;
			}
		}
		ConsolCostDefaultApportionmentMethodCollection consolCostDefaultApportionmentMethodCollection;

		ZXmlSerializer fConsolCostDefaultApportionmentMethodCollection;
		ZXmlSerializer ConsolCostDefaultApportionmentMethodCollectionSerialiser
		{
			get
			{
				return fConsolCostDefaultApportionmentMethodCollection ?? (fConsolCostDefaultApportionmentMethodCollection = ZXmlSerializer.New(typeof(ConsolCostDefaultApportionmentMethodCollection)));
			}
		}

		#endregion

		#endregion

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			ConsolCostDefaultApportionmentMethodCollectionSerialiser.Serialize(writer, ConsolCostDefaultApportionmentMethodCollection);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			consolCostDefaultApportionmentMethodCollection = (ConsolCostDefaultApportionmentMethodCollection)ConsolCostDefaultApportionmentMethodCollectionSerialiser.Deserialize(reader);
			RegisterEditableChildObject(ConsolCostDefaultApportionmentMethodCollection);
		}
	}
}
