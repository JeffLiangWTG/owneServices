using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Integration.TransportBooking;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ChargeableFactor : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string MetricFactor = "MetricFactor";
			public const string ImperialFactor = "ImperialFactor";
		}

		#endregion

		public ChargeableFactor()
		{
			MetricFactorForBinding = new ConversionFactorViewModel(
				p => new UnitSystemChargeableFactorLookups(p, UnitsSystem.Metric),
				p => new UnitSystemChargeableFactorValidation(p, UnitsSystem.Metric));

			ImperialFactorForBinding = new ConversionFactorViewModel(
				p => new UnitSystemChargeableFactorLookups(p, UnitsSystem.Imperial),
				p => new UnitSystemChargeableFactorValidation(p, UnitsSystem.Imperial));
		}

		public ChargeableFactor(ConversionFactor metricFactor, ConversionFactor imperialFactor)
			: this()
		{
			MetricFactorForBinding.ConversionFactor = metricFactor;
			ImperialFactorForBinding.ConversionFactor = imperialFactor;
		}

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ChargeableFactor(MetricFactor, ImperialFactor);
		}

		#endregion

		#region Properties

		#region MetricFactor

		public ConversionFactorViewModel MetricFactorForBinding
		{
			get;
			private set;
		}

		public ConversionFactor MetricFactor
		{
			get { return MetricFactorForBinding.ConversionFactor; }
		}

		#endregion

		#region ImperialFactor

		public ConversionFactorViewModel ImperialFactorForBinding
		{
			get;
			private set;
		}

		public ConversionFactor ImperialFactor
		{
			get { return ImperialFactorForBinding.ConversionFactor; }
		}

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			MetricFactorForBinding.Validation.ValidateAll();
			ImperialFactorForBinding.Validation.ValidateAll();
		}

		#endregion

		#region XML Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.MetricFactor, MetricFactorForBinding.ConversionFactor.ToShortString());
			writer.WriteElementString(Schema.ImperialFactor, ImperialFactorForBinding.ConversionFactor.ToShortString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ConversionFactor e;

			if (ConversionFactor.TryParse(reader.ReadElementString(Schema.MetricFactor), out e))
			{
				MetricFactorForBinding.ConversionFactor = e;
			}

			if (ConversionFactor.TryParse(reader.ReadElementString(Schema.ImperialFactor), out e))
			{
				ImperialFactorForBinding.ConversionFactor = e;
			}
		}

		#endregion

		#region Overrides

		public override bool Equals(object obj)
		{
			ChargeableFactor other = obj as ChargeableFactor;

			if (other == null)
			{
				return false;
			}

			return MetricFactor.Equals(other.MetricFactor) && ImperialFactor.Equals(other.ImperialFactor);
		}

		public override int GetHashCode()
		{
			return MetricFactor.GetHashCode() ^ ImperialFactor.GetHashCode();
		}

		#endregion

		#region GetDefault ChargeableFactor

		public static ChargeableFactor GetDefault(ChargeableFactorSource chargeableFactorSource, string transportMode)
		{
			switch (chargeableFactorSource)
			{
				case ChargeableFactorSource.International:
					return GetFreightChargeableFactor(false, transportMode);
				case ChargeableFactorSource.Domestic:
					return GetFreightChargeableFactor(true, transportMode);
				case ChargeableFactorSource.Warehouse:
					return GetWarehouseChargeableFactor(transportMode);
				case ChargeableFactorSource.TransitWarehouse:
					return GetTransitWarehouseChargeableFactor(transportMode);
				case ChargeableFactorSource.TransportBooking:
					var transportRegistry = ObjectFactory.Get<ITransportBookingRegistryProvider>();
					var chargeableFactorRegistryItem = (ChargeableFactorRegistryItem)transportRegistry.TransportBookingChargeableFactor;
					return chargeableFactorRegistryItem.Value;
				default:
					return null;
			}
		}

		static ChargeableFactor GetFreightChargeableFactor(bool isDomestic, string transportMode)
		{
			switch (transportMode)
			{
				case Core.Constants.TransportModes.Air:
					return isDomestic ? FreightDataRegistry.Instance.DomesticChargeableFactorAir.Value : FreightDataRegistry.Instance.InternationalChargeableFactorAir.Value;
				case Core.Constants.TransportModes.AirSea:
					return isDomestic ? FreightDataRegistry.Instance.DomesticChargeableFactorAir.Value : FreightDataRegistry.Instance.InternationalChargeableFactorAir.Value;
				case Core.Constants.TransportModes.Road:
					return isDomestic ? FreightDataRegistry.Instance.DomesticChargeableFactorRoad.Value : FreightDataRegistry.Instance.InternationalChargeableFactorRoad.Value;
				case Core.Constants.TransportModes.Courier:
				case Core.Constants.TransportModes.Mail:
					return isDomestic ? FreightDataRegistry.Instance.DomesticChargeableFactorCourier.Value : FreightDataRegistry.Instance.InternationalChargeableFactorCourier.Value;
				case Core.Constants.TransportModes.Rail:
					return isDomestic ? FreightDataRegistry.Instance.DomesticChargeableFactorRail.Value : FreightDataRegistry.Instance.InternationalChargeableFactorRail.Value;
				case Core.Constants.TransportModes.Sea:
					return isDomestic ? FreightDataRegistry.Instance.DomesticChargeableFactorSea.Value : FreightDataRegistry.Instance.InternationalChargeableFactorSea.Value;
				default:
					return null;
			}
		}

		static ChargeableFactor GetTransitWarehouseChargeableFactor(string transportMode)
		{
			switch (transportMode)
			{
				case Core.Constants.TransportModes.Air:
					return WarehouseDataRegistry.Instance.TransitChargeableFactorForAir.Value;
				case Core.Constants.TransportModes.Road:
					return WarehouseDataRegistry.Instance.TransitChargeableFactorForRoad.Value;
				case Core.Constants.TransportModes.Sea:
					return WarehouseDataRegistry.Instance.TransitChargeableFactorForSea.Value;
				default:
					return null;
			}
		}

		static ChargeableFactor GetWarehouseChargeableFactor(string transportMode)
		{
			switch (transportMode)
			{
				case Core.Constants.TransportModes.Storage:
					return WarehouseDataRegistry.Instance.WarehouseChargeableFactorStorage.Value;
				case Core.Constants.TransportModes.WarehouseHandling:
					return WarehouseDataRegistry.Instance.WarehouseChargeableFactorHandling.Value;
				default:
					return null;
			}
		}

		#endregion
	}

	public enum ChargeableFactorSource
	{
		International,
		Domestic,
		Warehouse,
		TransitWarehouse,
		TransportBooking
	}
}
