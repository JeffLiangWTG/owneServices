using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using WTG.RtfConverter;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class CustomDefaultBranchConfiguration : RegistryBusinessObjectTemplate
	{
		public CustomDefaultBranchConfiguration(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public CustomDefaultBranchConfiguration()
			: base()
		{
		}

		public ZString ConfigAsString
		{
			get
			{
				return Encoding.UTF8.GetString(Config);
			}
			set
			{
				Config = Encoding.UTF8.GetBytes(value);
			}
		}

		public ZBlob Config_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(Config);
			}

			set
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				Config = ZBlob.FromUTF8(htmlToRtfConverter.Convert(value.ToUTF8()));
			}
		}

		public ZBlob Config
		{
			get
			{
				return config;
			}
			set
			{
				SetNonPersistentPropertyValue(ConfigInfo, ref config, value);
			}
		}

		ZBlob config;

		public ZPropertyInfo ConfigInfo
		{
			get { return GetZPropertyInfo(nameof(Config)); }
		}

		[List("JobHeaderCollection")]
		public ZGuid JobHeaderPK
		{
			get
			{
				return jobHeaderPK;
			}
			set
			{
				SetNonPersistentPropertyValue(JobHeaderPKInfo, ref jobHeaderPK, value);
			}
		}
		ZGuid jobHeaderPK;

		public ZPropertyInfo JobHeaderPKInfo
		{
			get { return GetZPropertyInfo(nameof(JobHeaderPK)); }
		}

		public JobHeaderCollection JobHeaderCollection
		{
			get { return new JobHeaderCollection(CurrentFactory); }
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CustomDefaultBranchConfiguration(fallbackLevel, factory);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var castedClone = (CustomDefaultBranchConfiguration)clone;
			castedClone.ConfigAsString = ConfigAsString;
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ConfigAsString = reader.ReadElementString(nameof(Config));
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(nameof(Config), ConfigAsString);
		}
	}
}
