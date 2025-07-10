using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Forwarding.Business
{
	public abstract class ForwardingShipmentCustomsStatusProvider
	{
		protected ForwardingShipmentCustomsStatusProvider(ForwardingShipment shipment)
		{
			if (shipment == null)
			{
				throw new ArgumentNullException(nameof(shipment));
			}

			this.Shipment = shipment;
			Factory = this.Shipment.Factory;
		}

		protected readonly ForwardingShipment Shipment;
		protected readonly BusinessObjectFactory Factory;

		static ZBool CurrentCompanyIsAustralian
		{
			get { return Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) == Core.Constants.CountryCodes.Australia; }
		}

		static ZBool CurrentCompanyIsBasedInNZ
		{
			get { return Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) == Core.Constants.CountryCodes.NewZealand; }
		}

		static ZBool CurrentCompanyIsUSCompany
		{
			get { return Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) == Core.Constants.CountryCodes.UnitedStates; }
		}

		static ZBool CurrentCompanyIsInEuropeanUnion
		{
			get { return GlbCompany.CurrentCompany.Country.IsPartOfEuropeanUnion; }
		}

		static ZBool CurrentCompanyIsUK
		{
			get { return Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) == Core.Constants.CountryCodes.UnitedKingdom; }
		}

		public static ForwardingShipmentCustomsStatusProvider New(ForwardingShipment shipment)
		{
			Type statusProviderType = null;
			if (CurrentCompanyIsAustralian && shipment.IsAir)
			{
				statusProviderType = ObjectFactory.GetType<Integration.Customs.AU.ICusHAWBForwardingShipmentCustomsStatusProvider>();
			}
			else if (CurrentCompanyIsAustralian && shipment.IsSea)
			{
				statusProviderType = ObjectFactory.GetType<Integration.Customs.AU.ICusSCAHouseForwardingShipmentCustomsStatusProvider>();
			}
			else if (CurrentCompanyIsBasedInNZ)
			{
				statusProviderType = ObjectFactory.GetType<Integration.Customs.NZ.IForwardingShipmentCustomsStatusProvider>();
			}
			else if (CurrentCompanyIsUSCompany)
			{
				statusProviderType = ObjectFactory.GetType<Integration.Customs.US.IForwardingShipmentCustomsStatusProvider>();
			}
			else if (CurrentCompanyIsUK)
			{
				statusProviderType = ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ICcsukForwardingShipmentCustomsStatusProvider>();
			}
			else if (CurrentCompanyIsInEuropeanUnion)
			{
				statusProviderType = ObjectFactory.GetType<Integration.Customs.EU.IForwardingShipmentCustomsStatusProvider>();
			}
			else
			{
				statusProviderType = typeof(EmptyForwardingShipmentCustomsStatusProvider);
			}

			return (ForwardingShipmentCustomsStatusProvider)Activator.CreateInstance(statusProviderType, new object[] { shipment });
		}

		internal class EmptyForwardingShipmentCustomsStatusProvider : ForwardingShipmentCustomsStatusProvider
		{
			public EmptyForwardingShipmentCustomsStatusProvider(ForwardingShipment shipment)
				: base(shipment)
			{
			}

			public override ZString CustomsCargoStatus()
			{
				return ZString.Empty;
			}

			public override ZString CustomsMessageStatus()
			{
				return ZString.Empty;
			}
		}

		public abstract ZString CustomsCargoStatus();
		public abstract ZString CustomsMessageStatus();

		public virtual ZString ACICargoStatus()
		{
			return ZString.Empty;
		}

		public virtual ZString ACIMessageStatus()
		{
			return ZString.Empty;
		}

		#region CA
		public virtual ZString EManifestCargoStatus()
		{
			return ZString.Empty;
		}
		public virtual ZString EManifestMessageStatus()
		{
			return ZString.Empty;
		}
		#endregion

		#region US Status Fields

		public virtual ZString CRLStatus()
		{
			return ZString.Empty;
		}

		public virtual ZString SEBillStatus()
		{
			return ZString.Empty;
		}

		public virtual ZString HLDOrEXMStatus()
		{
			return ZString.Empty;
		}

		public virtual ZString ENSStatus()
		{
			return ZString.Empty;
		}

		public virtual ZString EXPStatus()
		{
			return ZString.Empty;
		}

		public virtual ZString ITStatus()
		{
			return ZString.Empty;
		}

		#endregion
	}
}
