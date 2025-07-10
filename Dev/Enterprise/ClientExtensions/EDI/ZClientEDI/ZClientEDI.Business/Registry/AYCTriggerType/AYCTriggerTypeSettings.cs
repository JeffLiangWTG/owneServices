using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class AYCTriggerTypeSettings : RegistryBusinessObjectTemplate
	{
		public AYCTriggerTypeSettings()
		: base()
		{
		}

		public AYCTriggerTypeSettings(FallbackLevel fallbackLevel)
		: base(fallbackLevel)
		{
		}

		public AYCTriggerTypeSettings(BusinessObjectFactory factory)
		: base(factory)
		{
		}

		public AYCTriggerTypeSettings(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		: base(fallbackLevel, factory)
		{
		}

		public static class Schema
		{
			public const string PrimaryChargeCode = "PrimaryChargeCode";
			public const string SecondaryChargeCode = "SecondaryChargeCode";
		}
		public ZString PrimaryChargeCode
		{
			get => primaryChargeCode;
			set
			{
				SetNonPersistentPropertyValue(PrimaryChargeCodeInfo, ref primaryChargeCode, value);

				if (!IsValidationSuspended)
				{
					ValidatePrimaryChargeCode();
				}
			}
		}

		ZString primaryChargeCode;

		public ZPropertyInfo PrimaryChargeCodeInfo => GetZPropertyInfo(Schema.PrimaryChargeCode);

		public void ValidatePrimaryChargeCode()
		{
			PrimaryChargeCodeInfo.ClearAllNotifications();
			CheckChargeCode(PrimaryChargeCodeInfo);
		}

		public ZString SecondaryChargeCode
		{
			get => secondaryChargeCode;
			set
			{
				SetNonPersistentPropertyValue(SecondaryChargeCodeInfo, ref secondaryChargeCode, value);

				if (!IsValidationSuspended)
				{
					ValidateSecondaryChargeCode();
				}
			}
		}

		ZString secondaryChargeCode;

		public ZPropertyInfo SecondaryChargeCodeInfo => GetZPropertyInfo(Schema.SecondaryChargeCode);

		public void ValidateSecondaryChargeCode()
		{
			SecondaryChargeCodeInfo.ClearAllNotifications();
			CheckChargeCode(SecondaryChargeCodeInfo);
		}

		protected void CheckChargeCode(ZPropertyInfo info)
		{
			if (!info.Value.IsEmpty)
			{
				var validCodes = ClientLicenceFeeValidation.GetValidChargeCodes(CurrentFactory);

				if (!validCodes.ContainsCode((ZString)info.Value))
				{
					info.AddError("ChargeCode must be active, of Charge Type NON or REV, and with Charge Group NGC or NJR in at least one company.");
				}
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AYCTriggerTypeSettings(null, null) { PrimaryChargeCode = primaryChargeCode, SecondaryChargeCode = secondaryChargeCode };
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			PrimaryChargeCode = reader.ReadElementString(Schema.PrimaryChargeCode);
			SecondaryChargeCode = reader.ReadElementString(Schema.SecondaryChargeCode);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.PrimaryChargeCode, PrimaryChargeCode);
			writer.WriteElementString(Schema.SecondaryChargeCode, SecondaryChargeCode);
		}
	}
}
