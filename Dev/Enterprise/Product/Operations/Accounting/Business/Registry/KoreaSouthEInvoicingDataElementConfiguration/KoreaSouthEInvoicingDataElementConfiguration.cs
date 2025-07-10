using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class KoreaSouthEInvoicingDataElementConfiguration : RegistryBusinessObjectTemplate
	{
		public KoreaSouthEInvoicingDataElementConfiguration()
		{
		}

		public KoreaSouthEInvoicingDataElementConfiguration(ZString invoiceType, ZString dataElement, string configuration = "")
		{
			InvoiceType = invoiceType;
			DataElement = dataElement;
			Configuration = configuration;
		}

		#region Schema

		public abstract class Schema
		{
			public const string InvoiceType = "InvoiceType";
			public const string DataElement = "DataElement";
			public const string Configuration = "Configuration";
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new KoreaSouthEInvoicingDataElementConfiguration();
		}

		#region Properties

		#region InvoiceType

		[ReadOnlyMember(nameof(InvoiceType_ReadOnly))]
		[ResourceStringData("KoreaSouthEInvoicingDataElementConfiguration|InvoiceType", Caption = "Invoice Type")]
		public ZString InvoiceType
		{
			get { return invoiceType; }
			set
			{
				CheckMaximumLength(InvoiceTypeInfo, value);
				if (invoiceType != value)
				{
					SetNonPersistentPropertyValue(InvoiceTypeInfo, ref invoiceType, value);
					Configuration = ZString.Empty;
				}
			}
		}
		ZString invoiceType;

		public ZPropertyInfo InvoiceTypeInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceType); }
		}

		bool InvoiceType_ReadOnly => true;

		#endregion

		#region DataElement

		[ReadOnlyMember(nameof(DataElement_ReadOnly))]
		[ResourceStringData("KoreaSouthEInvoicingDataElementConfiguration|DataElement", Caption = "Data Element")]
		public ZString DataElement
		{
			get { return dataElement; }
			set
			{
				CheckMaximumLength(DataElementInfo, value);
				SetNonPersistentPropertyValue(DataElementInfo, ref dataElement, value);
			}
		}
		ZString dataElement;

		public ZPropertyInfo DataElementInfo
		{
			get { return GetZPropertyInfo(Schema.DataElement); }
		}

		bool DataElement_ReadOnly => true;

		#endregion

		#region Configuration

		[MaxLength(1024)]
		[ResourceStringData("KoreaSouthEInvoicingDataElementConfiguration|Configuration", Caption = "Configuration")]
		public ZString Configuration
		{
			get { return configuration; }
			set
			{
				CheckMaximumLength(ConfigurationInfo, value);
				SetNonPersistentPropertyValue(ConfigurationInfo, ref configuration, value);
			}
		}
		ZString configuration;

		public ZPropertyInfo ConfigurationInfo
		{
			get { return GetZPropertyInfo(Schema.Configuration); }
		}

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.InvoiceType, InvoiceType);
			writer.WriteElementString(Schema.DataElement, DataElement);
			writer.WriteElementString(Schema.Configuration, Configuration);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			InvoiceType = reader.ReadElementString(Schema.InvoiceType);
			DataElement = reader.ReadElementString(Schema.DataElement);
			Configuration = reader.ReadElementString(Schema.Configuration);
		}

		#endregion
	}
}
