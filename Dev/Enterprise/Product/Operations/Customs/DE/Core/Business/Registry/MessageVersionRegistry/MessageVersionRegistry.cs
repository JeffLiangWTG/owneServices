using System;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.DE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.DE.Business.XmlSerializers")]
	public class MessageVersionRegistry : RegistryBusinessObjectTemplate
	{
		public MessageVersionRegistry()
			: base()
		{
		}

		public MessageVersionRegistry(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public static class Schema
		{
			public const string SystemCode = "SystemCode";
			public const string VersionNumber = "VersionNumber";
		}

		[ReadOnly(true)]
		[ResourceStringData("B87CB187-6DD2-4E44-8C5A-9A47D05CDF15|SystemCode", Caption = "System")]
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

		public ZPropertyInfo SystemCodeInfo => GetZPropertyInfo(Schema.SystemCode);

		[ResourceStringData("E8AC2BDC-317E-4B7C-928C-733FF90D51FC|VersionNumber", Caption = "Version")]
		[List(nameof(VersionNumbers))]
		public ZString VersionNumber
		{
			get => versionNumber;
			set
			{
				SetNonPersistentPropertyValue(VersionNumberInfo, ref versionNumber, value);
				if (!IsValidationSuspended)
				{
					ValidateVersionNumber();
				}
				VersionNumberInfo.RefreshBinding();
			}
		}
		ZString versionNumber;

		public ZPropertyInfo VersionNumberInfo => GetZPropertyInfo(Schema.VersionNumber);

		public void ValidateVersionNumber()
		{
			VersionNumberInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(VersionNumberInfo);
			ListValidation.ErrorIfInvalidCode(VersionNumberInfo);
		}

		public CodeDescriptionPairList VersionNumbers
		{
			get
			{
				var system = SystemCode;
				return CurrentFactory.GetCachedValue("dc00621f-c0fe-4534-a060-6ce85e2b8d29|" + system, () =>
				{
					switch (system)
					{
						case AtlasSystemCode:
							var atlasVersionNumberList = new ATLASVersionNumberList();
							atlasVersionNumberList.RemoveCode(ATLASVersionNumberList.Codes._102);
							return atlasVersionNumberList;
						case AESSystemCode:
							var aesVersionNumberList = new AESVersionNumberList();
							aesVersionNumberList.RemoveCode(AESVersionNumberList.Codes._40);
							return aesVersionNumberList;
						case EmcsSystemCode:
							return new EmcsVersionNumberList();
						default:
							return new CodeDescriptionPairList();
					}
				});
			}
		}

		public static ZString CurrentAtlasVersion => DECustomsDataRegistry.Instance.CustomsMessageVersion.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)
														.Cast<MessageVersionRegistry>()
														.Single(x => x.SystemCode == MessageVersionRegistry.AtlasSystemCode)
														.VersionNumber;
		public static ZString CurrentAESVersion => DECustomsDataRegistry.Instance.CustomsMessageVersion.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)
														.Cast<MessageVersionRegistry>()
														.Single(x => x.SystemCode == MessageVersionRegistry.AESSystemCode)
														.VersionNumber;
		public static ZString CurrentEMCSVersion => DECustomsDataRegistry.Instance.CustomsMessageVersion.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)
														.Cast<MessageVersionRegistry>()
														.Single(x => x.SystemCode == MessageVersionRegistry.EmcsSystemCode)
														.VersionNumber;

		public const string AtlasSystemCode = "ATLAS";
		public const string AtlasDefaultVersionNumber = ATLASVersionNumberList.Codes._101;
		public const string AESSystemCode = "AES";
		public const string AESDefaultVersionNumber = AESVersionNumberList.Codes._30;
		public const string EmcsSystemCode = "EMCS";
		public const string EmcsDefaultVersionNumber = EmcsVersionNumberList.Codes._24;

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			SystemCode = reader.ReadElementString(Schema.SystemCode);
			VersionNumber = reader.ReadElementString(Schema.VersionNumber);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.SystemCode, SystemCode);
			writer.WriteElementString(Schema.VersionNumber, VersionNumber);
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new MessageVersionRegistry(fallbackLevel, factory);
	}
}

