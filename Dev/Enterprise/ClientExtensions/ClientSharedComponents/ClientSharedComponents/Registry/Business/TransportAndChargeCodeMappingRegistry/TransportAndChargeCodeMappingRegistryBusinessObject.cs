using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ClientSharedComponents.Registry
{
	[XmlSerializerAssembly("Enterprise.ClientSharedComponents.XmlSerializers")]
	public class TransportAndChargeCodeMappingRegistryBusinessObject : RegistryBusinessObjectTemplate
	{
		public TransportAndChargeCodeMappingRegistryBusinessObject()
			: base()
		{
		}

		public TransportAndChargeCodeMappingRegistryBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public TransportAndChargeCodeMappingRegistryBusinessObject(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public TransportAndChargeCodeMappingRegistryBusinessObject(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Schema
		public static class Schema
		{
			public const string TransportModeCode = "TransportModeCode";
			public const string ChargeCodePK = "ChargeCodePK";
			public const string NominalCostCode = "NominalCostCode";
			public const string NominalRevenueCode = "NominalRevenueCode";
		}
		#endregion

		#region TransportModeCode
		[MaxLength(3)]
		public ZString TransportModeCode
		{
			get { return transportModeCode; }
			set
			{
				SetNonPersistentPropertyValue(TransportModeCodeInfo, ref transportModeCode, value);
				if (!IsValidationSuspended)
				{
					ValidateTransportModeCode();
				}
				TransportModeCodeInfo.RefreshBinding();
			}
		}
		ZString transportModeCode;

		public ZPropertyInfo TransportModeCodeInfo
		{
			get { return GetZPropertyInfo(Schema.TransportModeCode); }
		}

		public void ValidateTransportModeCode()
		{
			TransportModeCodeInfo.ClearAllNotifications();
			if (TransportModeCode.Trim().IsEmpty)
			{
				MandatoryValidation.CheckEntered(TransportModeCodeInfo);
			}
			if (!TransportModeCodeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(TransportModeCodeInfo, TransportModes);
			}
			if (!TransportModeCodeInfo.HasErrors())
			{
				UniquePropertiesValidation.CheckPropertiesAreUniqueInCollection(TransportModeCodeInfo, ChargeCodePKInfo);
			}
		}
		#endregion

		#region ChargeCodePK
		public ZGuid ChargeCodePK
		{
			get { return chargeCodePK; }
			set
			{
				SetNonPersistentPropertyValue(ChargeCodePKInfo, ref chargeCodePK, value);
				if (!IsValidationSuspended)
				{
					ValidateChargeCodePK();
				}
				ChargeCodePKInfo.RefreshBinding();
			}
		}
		ZGuid chargeCodePK;

		public ZPropertyInfo ChargeCodePKInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeCodePK, "ChargeCode"); }
		}

		public void ValidateChargeCodePK()
		{
			ChargeCodePKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ChargeCodePKInfo);
			if (!ChargeCodePKInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidPK(ChargeCodePKInfo, ChargeCodes);
			}
			if (!ChargeCodePKInfo.HasErrors())
			{
				UniquePropertiesValidation.CheckPropertiesAreUniqueInCollection(TransportModeCodeInfo, ChargeCodePKInfo);
			}
		}
		#endregion

		#region NominalCostCode
		[MaxLength(10)]
		public ZString NominalCostCode
		{
			get { return nominalCostCode; }
			set
			{
				CheckMaximumLength(NominalCostCodeInfo, value);
				SetNonPersistentPropertyValue(NominalCostCodeInfo, ref nominalCostCode, value);
				if (!IsValidationSuspended)
				{
					ValidateNominalCostCode();
				}
				NominalCostCodeInfo.RefreshBinding();
			}
		}
		ZString nominalCostCode;

		public ZPropertyInfo NominalCostCodeInfo
		{
			get { return GetZPropertyInfo(Schema.NominalCostCode); }
		}

		void ValidateNominalCostCode()
		{
			NominalCostCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(NominalCostCodeInfo);
		}
		#endregion

		#region NominalRevenueCode
		[MaxLength(10)]
		public ZString NominalRevenueCode
		{
			get { return nominalRevenueCode; }
			set
			{
				CheckMaximumLength(NominalRevenueCodeInfo, value);
				SetNonPersistentPropertyValue(NominalRevenueCodeInfo, ref nominalRevenueCode, value);
				if (!IsValidationSuspended)
				{
					ValidateNominalRevenueCode();
				}
				NominalRevenueCodeInfo.RefreshBinding();
			}
		}
		ZString nominalRevenueCode;

		public ZPropertyInfo NominalRevenueCodeInfo
		{
			get { return GetZPropertyInfo(Schema.NominalRevenueCode); }
		}

		void ValidateNominalRevenueCode()
		{
			NominalRevenueCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(NominalRevenueCodeInfo);
		}
		#endregion

		#region Lookups
		public CodeDescriptionPairList TransportModes
		{
			get { return FreightCodePairLists.JS_TransportModeList(); }
		}

		public AccChargeCode CurrentCharge
		{
			get { return CurrentFactory.Load<AccChargeCode>(ChargeCodePK); }
		}

		public AccChargeCodeCollection ChargeCodes
		{
			get
			{
				return chargeCodes ?? (chargeCodes = CurrentFallbackLevel != null ? new AccChargeCodeCollection(CurrentFactory, new ZQuery(),
						CurrentFallbackLevel.CompanyPK(false)) : new AccChargeCodeCollection(CurrentFactory));
			}
		}
		AccChargeCodeCollection chargeCodes;
		#endregion

		#region Write/Read XML
		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.TransportModeCode, TransportModeCode);
			writer.WriteElementString(Schema.ChargeCodePK, ChargeCodePK.ToString());
			writer.WriteElementString(Schema.NominalCostCode, NominalCostCode);
			writer.WriteElementString(Schema.NominalRevenueCode, NominalRevenueCode);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			TransportModeCode = reader.ReadElementString(Schema.TransportModeCode);
			ChargeCodePK = new ZGuid(reader.ReadElementString(Schema.ChargeCodePK));
			NominalCostCode = reader.ReadElementString(Schema.NominalCostCode);
			NominalRevenueCode = reader.ReadElementString(Schema.NominalRevenueCode);
		}
		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TransportAndChargeCodeMappingRegistryBusinessObject(fallbackLevel, factory);
		}
	}
}
