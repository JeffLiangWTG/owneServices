using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Forwarding.Business
{
	public sealed class ForwardingShipmentCustomsInformation : NonPersistentBusinessObject, IShipmentCustomsInformation
	{
		public ForwardingShipmentCustomsInformation(ForwardingShipment shipment)
		{
			this.Shipment = Argument.NotNull(shipment, "shipment");
		}

		public ForwardingShipment Shipment { get; }

		public static class Schema
		{
			public const string CustomsCargoStatus = "CustomsCargoStatus";
			public const string CustomsMessageStatus = "CustomsMessageStatus";

			public const string CRLStatus = "CRLStatus";
			public const string SEBillStatus = "SEBillStatus";
			public const string HLDOrEXMStatus = "HLDOrEXMStatus";
			public const string ENSStatus = "ENSStatus";
			public const string EXPStatus = "EXPStatus";
			public const string ITStatus = "ITStatus";

			public const string ISFBillNumber = "ISFBillNumber";
			public const string ISFBillStatus = "ISFBillStatus";
			public const string ISFBillStatusDescription = "ISFBillStatusDescription";

			public const string AFRBillStatus = "AFRBillStatus";
			public const string AFRBillStatusDescription = "AFRBillStatusDescription";

			public const string EntryStatusDescription = "EntryStatusDescription";

			public const string ACICargoStatus = "ACICargoStatus";
			public const string ACIMessageStatus = "ACIMessageStatus";

			public const string EManifestCargoStatus = "EManifestCargoStatus";
			public const string EManifestMessageStatus = "EManifestMessageStatus";
		}

		#region CustomsStatus

		ForwardingShipmentCustomsStatusProvider CustomsStatusProvider
		{
			get { return customsStatusProvider ?? (customsStatusProvider = ForwardingShipmentCustomsStatusProvider.New(Shipment)); }
		}
		ForwardingShipmentCustomsStatusProvider customsStatusProvider;

		public ZString CustomsCargoStatus
		{
			get
			{
				return CustomsStatusProvider.CustomsCargoStatus();
			}
		}

		public ZString CustomsMessageStatus
		{
			get
			{
				return CustomsStatusProvider.CustomsMessageStatus();
			}
		}

		public ZString CRLStatus
		{
			get
			{
				return CustomsStatusProvider.CRLStatus();
			}
		}

		public ZString SEBillStatus
		{
			get
			{
				return CustomsStatusProvider.SEBillStatus();
			}
		}

		public ZString HLDOrEXMStatus
		{
			get
			{
				return CustomsStatusProvider.HLDOrEXMStatus();
			}
		}

		public ZString ENSStatus
		{
			get
			{
				return CustomsStatusProvider.ENSStatus();
			}
		}

		public ZString EXPStatus
		{
			get
			{
				return CustomsStatusProvider.EXPStatus();
			}
		}

		public ZString ITStatus
		{
			get
			{
				return CustomsStatusProvider.ITStatus();
			}
		}

		#endregion

		#region ISF

		ISFInformationProvider ISFInformationProvider
		{
			get { return isfInformationProvider ?? (isfInformationProvider = new ISFInformationProvider(Shipment)); }
		}
		ISFInformationProvider isfInformationProvider;

		public ZString ISFBillNumber
		{
			get
			{
				return ISFInformationProvider.ISFBillNumber;
			}
		}

		public ZString ISFBillStatus
		{
			get
			{
				return ISFInformationProvider.ISFBillStatus;
			}
		}

		public ZString ISFBillStatusDescription
		{
			get
			{
				return ISFInformationProvider.ISFBillStatusDescription;
			}
		}

		#endregion

		#region ACI

		ForwardingShipmentCustomsStatusProvider ACIStatusProvider
		{
			get { return aciCustomsStatusProvider ?? (aciCustomsStatusProvider = (ForwardingShipmentCustomsStatusProvider)Activator.CreateInstance(ObjectFactory.GetType("CAForwardingShipmentCustomsStatusProvider"), new object[] { Shipment })); }
		}
		ForwardingShipmentCustomsStatusProvider aciCustomsStatusProvider;

		public ZString ACICargoStatus
		{
			get { return ACIStatusProvider.ACICargoStatus(); }
		}

		public ZString ACIMessageStatus
		{
			get { return ACIStatusProvider.ACIMessageStatus(); }
		}

		#endregion

		#region CA
		public ZString EManifestCargoStatus
		{
			get { return ACIStatusProvider.EManifestCargoStatus(); }
		}

		public ZString EManifestMessageStatus
		{
			get { return ACIStatusProvider.EManifestMessageStatus(); }
		}
		#endregion

		#region JP AFR Bill Status

		public ZString AFRBillStatus
		{
			get
			{
				if (afrBillStatus == null)
				{
					afrBillStatus = new CachedProperty<ZString>(Shipment.Factory, () => AFRStatusHelper.GetAFRBillStatus(Shipment));
				}
				return afrBillStatus.Value;
			}
		}
		CachedProperty<ZString> afrBillStatus;

		public ZString AFRBillStatusDescription
		{
			get
			{
				if (afrBillStatusDescription == null)
				{
					afrBillStatusDescription = new CachedProperty<ZString>(Shipment.Factory, () => AFRStatusHelper.GetAFRBillStatusDescription(Shipment.Factory, AFRBillStatus));
				}
				return afrBillStatusDescription.Value;
			}
		}
		CachedProperty<ZString> afrBillStatusDescription;

		JP.AFR.IAFRStatusHelper AFRStatusHelper
		{
			get { return afrStatusHelper ?? (afrStatusHelper = ObjectFactory.Get<JP.AFR.IAFRStatusHelper>()); }
		}
		JP.AFR.IAFRStatusHelper afrStatusHelper;

		#endregion

		#region EntryStatusDescription

		public ZString EntryStatusDescription
		{
			get
			{
				if (fEntryStatusDescription.IsEmpty)
				{
					fEntryStatusDescription = "#";
					var declaration = Shipment.DeclarationForDocuments;
					if (declaration != null)
					{
						fEntryStatusDescription += (ZString)declaration["JE_EntryStatusDescription"];// JE_EntryStatusDescription is a calculated property not available in schema
					}
				}
				return fEntryStatusDescription.Substring(1);
			}
		}
		ZString fEntryStatusDescription;

		#endregion

		#region Destination Goods Value

		public ZDecimal DestinationGoodsValue => CachedValueHelper.GetValue(Shipment.Factory, ref destinationGoodsValue, () => ((ZDecimal)(Shipment.JS_GoodsValue * DestinationExchangeRate)).Round(2));
		CachedProperty<ZDecimal> destinationGoodsValue;

		public ZString DestinationCurrencyCode => CachedValueHelper.GetValue(Shipment.Factory, ref destinationCurrencyCode, () =>
		{
			var result = ZString.Empty;
			var destinationCountryCode = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Shipment.JS_RL_NKDestination.Left(2));
			if (!string.IsNullOrEmpty(destinationCountryCode))
			{
				var destinationCountry = new RefCountry.Loader(Shipment.Factory).LoadForCountry(destinationCountryCode);
				if (destinationCountry != null)
				{
					result = destinationCountry.RN_RX_NKLocalCurrency;
				}
			}

			return result;
		});
		CachedProperty<ZString> destinationCurrencyCode;

		public ZDateTime DateForDestinationExchangeRate => CachedValueHelper.GetValue(Shipment.Factory, ref dateForDestinationExchangeRate, () =>
		{
			var result = ZDateTime.Today;

			if (Shipment.ArrivalConsol is ForwardingConsol consol && consol.JK_JX_JA_E_DEP.IsValid)
			{
				result = consol.JK_JX_JA_E_DEP;
			}
			else if (Shipment.JS_E_DEP.IsValid)
			{
				result = Shipment.JS_E_DEP;
			}

			return result;
		});
		CachedProperty<ZDateTime> dateForDestinationExchangeRate;

		public ZDecimal DestinationExchangeRate => CachedValueHelper.GetValue(Shipment.Factory, ref destinationExchangeRate, () =>
		{
			var result = ZDecimal.Zero;
			if (!DestinationCurrencyCode.IsEmpty)
			{
				var destCurrency = RefCurrency.LoadFromCurrencyCode(Shipment.Factory, DestinationCurrencyCode);
				result = CurrencyConverter.GetExchangeRate(Shipment.GoodsValueCurr, destCurrency).Round(6);
			}

			return result;
		});
		CachedProperty<ZDecimal> destinationExchangeRate;

		public ZPropertyInfo DestinationExchangeRateInfo => GetZPropertyInfo(nameof(DestinationExchangeRate));

		internal ForwardingShipmentDestinationCurrencyConverterDataProvider CurrencyConverter => CachedValueHelper.GetValue(Shipment.Factory, ref currencyConverter, () => new ForwardingShipmentDestinationCurrencyConverterDataProvider(Shipment.Factory, DateForDestinationExchangeRate, Shipment.JS_RX_NKGoodsValueCurr));
		CachedProperty<ForwardingShipmentDestinationCurrencyConverterDataProvider> currencyConverter;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public ForwardingShipmentCustomsInformationValidation Validation => new ForwardingShipmentCustomsInformationValidation(this);

		#endregion
	}
}
