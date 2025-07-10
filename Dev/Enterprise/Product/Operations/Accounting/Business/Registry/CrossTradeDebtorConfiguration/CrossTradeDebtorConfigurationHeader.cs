using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Accounting.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class CrossTradeDebtorConfigurationHeader : RegistryBusinessObjectTemplate
	{
		public CrossTradeDebtorConfigurationHeader()
		{
		}

		public CrossTradeDebtorConfigurationHeader(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CrossTradeDebtorConfigurationHeader(fallbackLevel);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var castedClone = (CrossTradeDebtorConfigurationHeader)clone;
			if (crossTradeDebtorConfigurationCollection != null)
			{
				castedClone.crossTradeDebtorConfigurationCollection = (CrossTradeDebtorConfigurationCollection)Configurations.Clone(castedClone.CurrentFallbackLevel, castedClone.Factory);
				castedClone.RegisterEditableChildObject(castedClone.Configurations);
			}
		}

		#region CrossTradeDebtorConfigurationCollection

		public CrossTradeDebtorConfigurationCollection Configurations
		{
			get
			{
				if (crossTradeDebtorConfigurationCollection == null)
				{
					crossTradeDebtorConfigurationCollection = new CrossTradeDebtorConfigurationCollection();
					RegisterEditableChildObject(crossTradeDebtorConfigurationCollection);
				}
				return crossTradeDebtorConfigurationCollection;
			}
		}
		CrossTradeDebtorConfigurationCollection crossTradeDebtorConfigurationCollection;

		ZXmlSerializer fCrossTradeDebtorConfigurationCollectionSerialiser;
		ZXmlSerializer CrossTradeDebtorConfigurationCollectionSerialiser
		{
			get
			{
				return fCrossTradeDebtorConfigurationCollectionSerialiser ?? (fCrossTradeDebtorConfigurationCollectionSerialiser = ZXmlSerializer.New(typeof(CrossTradeDebtorConfigurationCollection)));
			}
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			CrossTradeDebtorConfigurationCollectionSerialiser.Serialize(writer, Configurations);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			crossTradeDebtorConfigurationCollection = (CrossTradeDebtorConfigurationCollection)CrossTradeDebtorConfigurationCollectionSerialiser.Deserialize(reader);
			RegisterEditableChildObject(crossTradeDebtorConfigurationCollection);
		}

		#endregion
	}
}
