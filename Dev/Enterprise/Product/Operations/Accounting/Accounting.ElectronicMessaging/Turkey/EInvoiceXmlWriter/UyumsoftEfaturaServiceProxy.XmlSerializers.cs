///To update auto generated part of these classes please run following command -
/// "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\SvcUtil.exe" https://efatura.uyumsoft.com.tr/Services/Integration?singleWsdl /t:code /l:c# /o:"C:\DEV\Enterprise\Product\Operations\Accounting\Accounting.ElectronicMessaging\Turkey\UyumsoftEfaturaServiceProxy.cs" /n:*,Enterprise.Accounting.ElectronicMessaging.efatura.uyumsoft.com.tr --enableDataBinding

using System.Xml.Serialization;

namespace Enterprise.Accounting.ElectronicMessaging.efatura.uyumsoft.com.tr
{
	[XmlSerializerAssembly("Enterprise.Accounting.ElectronicMessaging.XmlSerializers")]
	public partial class CustomerPartyType
	{
	}

	[XmlSerializerAssembly("Enterprise.Accounting.ElectronicMessaging.XmlSerializers")]
	public partial class InvoiceInfo
	{
	}

	[XmlSerializerAssembly("Enterprise.Accounting.ElectronicMessaging.XmlSerializers")]
	public partial class MonetaryTotalType
	{
	}

	[XmlSerializerAssembly("Enterprise.Accounting.ElectronicMessaging.XmlSerializers")]
	public partial class InvoiceLineType
	{
	}

	[XmlSerializerAssembly("Enterprise.Accounting.ElectronicMessaging.XmlSerializers")]
	public partial class InvoiceType
	{
	}

	[XmlSerializerAssembly("Enterprise.Accounting.ElectronicMessaging.XmlSerializers")]
	public partial class NoteType
	{
	}

	[XmlSerializerAssembly("Enterprise.Accounting.ElectronicMessaging.XmlSerializers")]
	public partial class PaymentMeansType
	{
	}

	[XmlSerializerAssembly("Enterprise.Accounting.ElectronicMessaging.XmlSerializers")]
	public partial class SupplierPartyType
	{
	}

	[XmlSerializerAssembly("Enterprise.Accounting.ElectronicMessaging.XmlSerializers")]
	public partial class TaxTotalType
	{
	}

	[XmlSerializerAssembly("Enterprise.Accounting.ElectronicMessaging.XmlSerializers")]
	public partial class EArchiveCancelInvoiceContext
	{
	}

	[XmlSerializerAssembly("Enterprise.Accounting.ElectronicMessaging.XmlSerializers")]
	[System.Serializable()]
	[System.Diagnostics.DebuggerStepThrough()]
	[System.ComponentModel.DesignerCategory("code")]
	[XmlRoot(Namespace = "http://tempuri.org/")]
	public partial class CancelEArchiveInvoice
	{
		[XmlElement(ElementName = "request", Namespace = null)]
		public EArchiveCancelInvoiceContext request;
	}

	[XmlSerializerAssembly("Enterprise.Accounting.ElectronicMessaging.XmlSerializers")]
	public partial class ItemType
	{
		protected IDType idField;

		[XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2", Order = 12)]
		public IDType ID
		{
			get
			{
				return this.idField;
			}
			set
			{
				this.idField = value;
				this.RaisePropertyChanged(nameof(ID));
			}
		}
	}
}
