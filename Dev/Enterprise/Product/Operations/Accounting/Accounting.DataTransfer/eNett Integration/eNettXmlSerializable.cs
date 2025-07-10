using System.Xml.Serialization;

namespace Enterprise.Accounting.DataTransfer.com.enett991
{
	[XmlSerializerAssembly("Enterprise.Accounting.DataTransfer.XmlSerializers")]
	[XmlRoot(Namespace = "https://enettlogistics.com/")]
	partial class Response_ProcessDirectDebit
	{ }

	[XmlSerializerAssembly("Enterprise.Accounting.DataTransfer.XmlSerializers")]
	[XmlRoot(Namespace = "https://enettlogistics.com/")]
	partial class Response_OfflinePaymentNotification
	{ }

	[XmlSerializerAssembly("Enterprise.Accounting.DataTransfer.XmlSerializers")]
	[XmlRoot(Namespace = "https://enettlogistics.com/")]
	partial class Response_ProcessCreditCard
	{
	}
}
