using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ShipmentInspectionTypeCollection : RegistryBusinessObjectCollection
	{
		public ShipmentInspectionTypeCollection()
			: base()
		{
		}

		public ShipmentInspectionTypeCollection(FallbackLevel currentFallbackLevel)
			: this(currentFallbackLevel, ZString.Empty)
		{
		}

		public ShipmentInspectionTypeCollection(ZString countryCode)
			: this(null, countryCode)
		{
		}

		public ShipmentInspectionTypeCollection(FallbackLevel currentFallbackLevel, ZString countryCode)
			: base(currentFallbackLevel)
		{
			CountryCode = countryCode;
		}

		#region Parent

		public ShipmentInspectionTypes Parent { get; set; }

		#endregion

		#region Allow New

		protected override bool AllowNewCore
		{
			get { return CountryCode.IsEmpty || CountryCode == Core.Constants.CountryCodes.EuropeanUnion; }
		}

		#endregion

		#region GetClone

		protected sealed override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			ShipmentInspectionTypeCollection clone = GetNewCollection();
			clone.CodeMaxLength = CodeMaxLength;
			clone.CurrentFallbackLevel = fallbackLevel;
			return clone;
		}

		#endregion

		protected virtual ShipmentInspectionTypeCollection GetNewCollection()
		{
			return new ShipmentInspectionTypeCollection(CurrentFallbackLevel, CountryCode);
		}

		public new ShipmentInspectionType this[int i]
		{
			get { return (ShipmentInspectionType)base[i]; }
		}

		public new ShipmentInspectionType AddNew()
		{
			return (ShipmentInspectionType)base.AddNew();
		}

		public ShipmentInspectionType Add(ZString code, MultilingualString description, bool showInList, bool allowedOnPassengerFlights)
		{
			ShipmentInspectionType result = AddNew();
			result.Code = code;
			result.Description = description;
			result.ShowInList = showInList;
			result.AllowedOnPassengerFlights = allowedOnPassengerFlights;

			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ShipmentInspectionType(CurrentFallbackLevel);
		}

		public ZString CountryCode { get; set; }
	}
}
