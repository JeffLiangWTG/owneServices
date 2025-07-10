using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;
using static Enterprise.Freight.Integration.Forwarding;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ShipmentInspectionTypes : RegistryBusinessObjectTemplate
	{
		#region Schema

		protected abstract class Schema
		{
			public const string CountryCode = "CountryCode";
		}

		#endregion

		public ShipmentInspectionTypes()
			: base()
		{
		}

		public ShipmentInspectionTypes(ZString countryCode)
		{
			CountryCode = countryCode;
		}

		public ShipmentInspectionTypes(ZString countryCode, ShipmentInspectionTypeCollection typesList)
		{
			CountryCode = countryCode;
			Types = typesList;
		}

		#region Cloning

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			ShipmentInspectionTypes result = new ShipmentInspectionTypes(CountryCode);
			result.Types = (ShipmentInspectionTypeCollection)Types.Clone(fallbackLevel, factory);
			return result;
		}

		#endregion

		#region Types

		[BusinessObjectTestExclude]
		public ShipmentInspectionTypeCollection Types
		{
			get
			{
				return types ?? (Types = new ShipmentInspectionTypeCollection());
			}
			private set
			{
				types = value;
				types.Parent = this;
				RegisterEditableChildObject(types);
			}
		}
		ShipmentInspectionTypeCollection types;

		#endregion

		#region SystemDefinedTypes

		public ShipmentInspectionTypeCollection SystemDefinedTypes
		{
			get { return SystemDefinedLists.GetCountrySpecificSystemDefinedList(CountryCode); }
		}

		ShipmentInspectionTypeLists SystemDefinedLists
		{
			get { return systemDefinedLists ?? (systemDefinedLists = new ShipmentInspectionTypeLists(CurrentFactory)); }
		}
		ShipmentInspectionTypeLists systemDefinedLists;

		#endregion

		#region CountryCode

		public ZString CountryCode { get; private set; }

		#endregion

		#region AllowedOnPassengerFlights

		public bool AllowedOnPassengerFlightsApplies
		{
			get { return SupplyChainSecurityConfiguration.PassengerFlightValidationApplies; }
		}

		#endregion

		#region SupplyChainSecurityConfiguration

		ISupplyChainSecurityConfiguration SupplyChainSecurityConfiguration
		{
			get
			{
				if (supplyChainSecurityConfigurationHelper == null)
				{
					supplyChainSecurityConfigurationHelper = ObjectFactory.Get<ISupplyChainSecurityConfigurationHelper>();
				}

				return supplyChainSecurityConfigurationHelper.GetConfigurationForCountry(CountryCode);
			}
		}
		ISupplyChainSecurityConfigurationHelper supplyChainSecurityConfigurationHelper;

		#endregion

		#region XML Serialisation

		ZXmlSerializer KnownShipperTypeCollectionSerialiser
		{
			get { return knownShipperTypeCollectionSerialiser ?? (knownShipperTypeCollectionSerialiser = ZXmlSerializer.New(typeof(ShipmentInspectionTypeCollection))); }
		}
		ZXmlSerializer knownShipperTypeCollectionSerialiser;

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.CountryCode, CountryCode);
			KnownShipperTypeCollectionSerialiser.Serialize(writer, Types);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			CountryCode = reader.ReadElementString(Schema.CountryCode);
			Types = (ShipmentInspectionTypeCollection)KnownShipperTypeCollectionSerialiser.Deserialize(reader);
			Types.CountryCode = CountryCode;
		}

		#endregion
	}
}
