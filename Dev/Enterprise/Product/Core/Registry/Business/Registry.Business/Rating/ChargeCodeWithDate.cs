using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ChargeCodeWithDate : RegistryBusinessObjectTemplate
	{
		public ChargeCodeWithDate()
		{ }

		public ChargeCodeWithDate(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{ }

		#region Schema

		public static class Schema
		{
			public const string ChargeCode = "ChargeCode";
			public const string ActiveTimeUtc = "ActiveTimeUtc";
		}

		#endregion

		#region Properties

		public ZGuid ChargeCode
		{
			get => fChargeCode;
			set
			{
				if (fChargeCode != value)
				{
					SetNonPersistentPropertyValue(ChargeCodeInfo, ref fChargeCode, value);
					ChargeCodeInfo.RefreshBinding();
				}
			}
		}
		ZGuid fChargeCode;

		public ZPropertyInfo ChargeCodeInfo => GetZPropertyInfo(Schema.ChargeCode);

		#region ActiveTimeUtc
		[ReadOnly(true)]
		public ZDateTime ActiveTimeUtc
		{
			get { return fActiveTime; }
			set
			{
				SetNonPersistentPropertyValue<ZDateTime>(ActiveTimeUtcInfo, ref fActiveTime, value);
			}
		}
		ZDateTime fActiveTime;

		public ZPropertyInfo ActiveTimeUtcInfo
		{
			get { return GetZPropertyInfo(Schema.ActiveTimeUtc); }
		}

		#endregion

		#endregion

		#region Override

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ChargeCodeWithDate(fallbackLevel, factory);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "string internal used only ")]
		const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ChargeCode = new ZGuid(reader.ReadElementString(Schema.ChargeCode));
			ActiveTimeUtc = reader.ReadElementStringAsZDateTime(Schema.ActiveTimeUtc, DateTimeFormat);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ChargeCode, ChargeCode.ToString());
			writer.WriteElementString(Schema.ActiveTimeUtc, (ActiveTimeUtc.IsValid ? ActiveTimeUtc : ZDateTime.UtcNow).ToString(DateTimeFormat));
		}
		#endregion
	}
}
