using System;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Customs.DE.Business.Res;

namespace Enterprise.Customs.DE.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.DE.Business.XmlSerializers")]
	public class ExportStatusRequestRecipientRegistry : RegistryBusinessObjectTemplate
	{
		public ExportStatusRequestRecipientRegistry()
			: base()
		{
		}

		public ExportStatusRequestRecipientRegistry(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public static class Schema
		{
			public const string SystemCode = "SystemCode";
			public const string MessageRecipient = "MessageRecipient";
		}

		[ReadOnly(true)]
		[ResourceStringData("448462DA-10F7-45CE-BF73-2826C9C72645", Caption = "System")]
		public ZString SystemCode
		{
			get => systemCode;
			set
			{
				SetNonPersistentPropertyValue(SystemCodeInfo, ref systemCode, value);
				SystemCodeInfo.RefreshBinding();
			}
		}
		ZString systemCode;

		public ZPropertyInfo SystemCodeInfo => GetZPropertyInfo(nameof(SystemCode));

		[MaxLength(8)]
		[ResourceStringData("E6D8EB4B-F0CE-4550-9C5E-1D1E98AEA2F3", Caption = "Message Recipient")]
		public ZString MessageRecipient
		{
			get => messageRecipient;
			set
			{
				SetNonPersistentPropertyValue(MessageRecipientInfo, ref messageRecipient, value);
				if (!IsValidationSuspended)
				{
					ValidateMessageRecipient();
				}
				MessageRecipientInfo.RefreshBinding();
			}
		}
		ZString messageRecipient;

		public ZPropertyInfo MessageRecipientInfo => GetZPropertyInfo(nameof(MessageRecipient));

		public void ValidateMessageRecipient()
		{
			MessageRecipientInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(MessageRecipientInfo);
			if (MessageRecipient.Length != 8 || !MessageRecipient.StartsWith(Core.Constants.CountryCodes.Germany, StringComparison.OrdinalIgnoreCase))
			{
				MessageRecipientInfo.AddWarning(Res.GetString("58991CA3-7A81-4C33-A7E5-A04C6C132086", "The length should be 8 characters for Recipient Code and start with 'DE'."));
			}
		}

		public static ZString CurrentAtlasMessageRecipient => DECustomsDataRegistry.Instance.ExportStatusRequestRecipient.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)
														.Cast<ExportStatusRequestRecipientRegistry>()
														.Single(x => x.SystemCode == ExportStatusRequestRecipientRegistry.AtlasSystemCode)
														.MessageRecipient;
		public static ZString CurrentAESMessageRecipient => DECustomsDataRegistry.Instance.ExportStatusRequestRecipient.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)
														.Cast<ExportStatusRequestRecipientRegistry>()
														.Single(x => x.SystemCode == ExportStatusRequestRecipientRegistry.AESSystemCode)
														.MessageRecipient;

		public const string AtlasSystemCode = "ATLAS";
		public const string AESSystemCode = "AES";

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			SystemCode = reader.ReadElementString(Schema.SystemCode);
			MessageRecipient = reader.ReadElementString(Schema.MessageRecipient);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.SystemCode, SystemCode);
			writer.WriteElementString(Schema.MessageRecipient, MessageRecipient);
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new ExportStatusRequestRecipientRegistry(fallbackLevel, factory);
	}
}

