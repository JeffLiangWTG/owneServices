using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.Common;
using Enterprise.Accounting.ElectronicMessaging.Common;

namespace Enterprise.Accounting.ElectronicMessaging.Hungary
{
	/// <summary>
	/// Builds a Hungary GEN ManageInvoiceRequest XML document.
	/// IMPORTANT: no CW1 database access is permitted; all data must be in ManageInvoiceRequestModel.
	/// </summary>
	public class ManageInvoiceRequestXmlBuilder
	{
		public ManageInvoiceRequestXmlBuilder(ManageInvoiceRequestModel model)
		{
			Model = Argument.NotNull(model, nameof(model));
		}

		ManageInvoiceRequestModel Model { get; }

		internal IXmlElement GetXML()
		{
			var xml = new ComplexXmlElement(DocumentXmlns
										, Tag_ManageInvoiceRequest
										, BuildHeaderXml()
										, BuildUserXml()
										, BuildSoftwareXml()
										, BuildExchangeTokenXml()
										, BuildInvoiceOperationsXml());
			xml.Attributes.AddRange(BuildNameSpaceAttributes());
			return xml;
		}

		public ComplexXmlElement BuildHeaderXml()
		{
			var header = new ComplexXmlElement(HeaderXmlns
									, Tag_Header
									, new SimpleXmlElement(HeaderXmlns, Tag_Header_RequestId, Val_Header_RequestId)
									, new SimpleXmlElement(HeaderXmlns, Tag_Header_Timestamp, Val_Header_Timestamp)
									, new SimpleXmlElement(HeaderXmlns, Tag_Header_RequestVersion, Model.RequestVersion)
									, new SimpleXmlElement(HeaderXmlns, Tag_Header_HeaderVersion, Val_Header_HeaderVersion));
			return header;
		}

		IEnumerable<XmlElementAttribute> BuildNameSpaceAttributes() => new[] { new XmlElementAttribute(XNamespace.Xmlns + nameof(common), common) };

		public ComplexXmlElement BuildUserXml()
		{
			return new ComplexXmlElement(UserXmlns
										, Tag_User
										, new SimpleXmlElement(UserXmlns, Tag_User_Login, Model.LoginId)
										, BuildPasswordXml()
										, new SimpleXmlElement(UserXmlns, Tag_User_TaxNumber, Model.TaxPayerRegistrationNumber)
										, BuildRequestSignatureElement());
		}

		IXmlElement BuildPasswordXml()
		{
			var element = new SimpleXmlElement(UserXmlns, Tag_User_PasswordHash, Model.PasswordHash);
			element.Attributes.Add(new XmlElementAttribute(Attribute_CryptoType, Val_User_Password_CryptoType));
			return element;
		}

		IXmlElement BuildRequestSignatureElement()
		{
			var element = new SimpleXmlElement(UserXmlns, Tag_User_RequestSignature, Val_User_RequestSignature);
			element.Attributes.Add(new XmlElementAttribute(Attribute_CryptoType, Val_ElectronicInvoiceHash_CryptoType));
			return element;
		}

		public ComplexXmlElement BuildSoftwareXml()
		{
			return new ComplexXmlElement(DocumentXmlns
									, Tag_Software
									, new SimpleXmlElement(DocumentXmlns, Tag_Software_SoftwareId, Val_Software_SoftwareId)
									, new SimpleXmlElement(DocumentXmlns, Tag_Software_SoftwareName, Val_Software_SoftwareName)
									, new SimpleXmlElement(DocumentXmlns, Tag_Software_SoftwareOperation, Val_Software_SoftwareOperation)
									, new SimpleXmlElement(DocumentXmlns, Tag_Software_SoftwareMainVersion, Model.SoftwareVersion)
									, new SimpleXmlElement(DocumentXmlns, Tag_Software_SoftwareDevName, Val_Software_SoftwareDevName)
									, new SimpleXmlElement(DocumentXmlns, Tag_Software_SoftwareDevContact, Val_Software_SoftwareDevContact)
									, new SimpleXmlElement(DocumentXmlns, Tag_Software_SoftwareDevCountryCode, Val_Software_SoftwareDevCountryCode)
									, new SimpleXmlElement(DocumentXmlns, Tag_Software_SoftwareDevTaxNumber, Val_Software_SoftwareDevTaxNumber));
		}

		public SimpleXmlElement BuildExchangeTokenXml()
			=> new SimpleXmlElement(DocumentXmlns, Tag_ExchangeToken, Val_ExchangeToken);

		public ComplexXmlElement BuildInvoiceOperationsXml()
		{
			return new ComplexXmlElement(DocumentXmlns
										, Tag_InvoiceOperations
										, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceOperations_CompressedContent, Val_InvoiceOperations_CompressedContent)
										, BuildInvoiceOperationXml());
		}

		public ComplexXmlElement BuildInvoiceOperationXml()
		{
			var invoiceOperationElement = new ComplexXmlElement(DocumentXmlns
								, Tag_InvoiceOperations_InvoiceOperation
								, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceOperations_InvoiceOperation_Index, Val_InvoiceOperations_InvoiceOperation_Index)
								, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceOperations_InvoiceOperation_InvoiceOperation, Model.InvoiceOperation)
								, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceOperations_InvoiceOperation_InvoiceData, GetInvoiceDataXmlBuilder().GetInvoiceDataAsBase64()));

			if (invoiceOperationElement != null)
			{
				var electronicInvoiceHashElement = new SimpleXmlElement(DocumentXmlns, Tag_ElectronicInvoiceHash, Val_ElectronicInvoiceHash);
				electronicInvoiceHashElement.Attributes.Add(new XmlElementAttribute(Attribute_CryptoType, Val_ElectronicInvoiceHash_CryptoType));
				invoiceOperationElement.Children.Add(electronicInvoiceHashElement);
			}
			return invoiceOperationElement;
		}

		XNamespace DocumentXmlns => "http://schemas.nav.gov.hu/OSA/3.0/api";

		XNamespace HeaderXmlns => common;

		XNamespace UserXmlns => common;

		InvoiceDataXmlBuilder GetInvoiceDataXmlBuilder() => new InvoiceDataXmlBuilder(Model.GetInvoiceDataModel());

		#region XML Constants
		#region SuppressResourceStringsCheckRegion

		const string Tag_ManageInvoiceRequest = "ManageInvoiceRequest";

		const string common = "http://schemas.nav.gov.hu/NTCA/1.0/common";

		#region <Shared>

		const string Attribute_CryptoType = "cryptoType";

		#endregion

		#region <header>

		const string Tag_Header = "header";
		const string Tag_Header_RequestId = "requestId";
		const string Tag_Header_Timestamp = "timestamp";
		const string Tag_Header_RequestVersion = "requestVersion";
		const string Tag_Header_HeaderVersion = "headerVersion";

		const string Val_Header_RequestId = "$$requestIdReplaceMe$$";
		const string Val_Header_Timestamp = "$$timestampReplaceMe$$";
		const string Val_Header_HeaderVersion = "1.0";

		#endregion

		#region <user>

		const string Tag_User = "user";
		const string Tag_User_Login = "login";
		const string Tag_User_PasswordHash = "passwordHash";
		const string Tag_User_TaxNumber = "taxNumber";
		const string Tag_User_RequestSignature = "requestSignature";

		const string Val_User_RequestSignature = "$$requestSignatureReplaceMe$$";
		const string Val_User_Password_CryptoType = "SHA-512";
		const string Val_ElectronicInvoiceHash_CryptoType = "SHA3-512";

		#endregion

		#region <software>

		const string Tag_Software = "software";
		const string Tag_Software_SoftwareId = "softwareId";
		const string Tag_Software_SoftwareName = "softwareName";
		const string Tag_Software_SoftwareOperation = "softwareOperation";
		const string Tag_Software_SoftwareMainVersion = "softwareMainVersion";
		const string Tag_Software_SoftwareDevName = "softwareDevName";
		const string Tag_Software_SoftwareDevContact = "softwareDevContact";
		const string Tag_Software_SoftwareDevCountryCode = "softwareDevCountryCode";
		const string Tag_Software_SoftwareDevTaxNumber = "softwareDevTaxNumber";

		const string Val_Software_SoftwareId = "AU658947CARGOWISE1";
		const string Val_Software_SoftwareName = "CargoWise";
		const string Val_Software_SoftwareOperation = "ONLINE_SERVICE";
		const string Val_Software_SoftwareDevName = "Wisetech Global Limited";
		const string Val_Software_SoftwareDevContact = "eInvoice.Hungary@WisetechGlobal.com";
		const string Val_Software_SoftwareDevCountryCode = "AU";
		const string Val_Software_SoftwareDevTaxNumber = "41065894724";

		#endregion

		#region <exchangeToken>

		const string Tag_ExchangeToken = "exchangeToken";

		const string Val_ExchangeToken = "$$exchangeTokenReplaceMe$$";

		#endregion

		#region <invoiceOperations>

		const string Tag_InvoiceOperations = "invoiceOperations";
		const string Tag_InvoiceOperations_CompressedContent = "compressedContent";
		const string Tag_InvoiceOperations_InvoiceOperation = "invoiceOperation";
		const string Tag_InvoiceOperations_InvoiceOperation_Index = "index";
		const string Tag_InvoiceOperations_InvoiceOperation_InvoiceOperation = "invoiceOperation";
		const string Tag_InvoiceOperations_InvoiceOperation_InvoiceData = "invoiceData";
		const string Tag_ElectronicInvoiceHash = "electronicInvoiceHash";
		const string Val_ElectronicInvoiceHash = "$$invoiceHashReplaceMe$$";

		const string Val_InvoiceOperations_CompressedContent = "false";
		const string Val_InvoiceOperations_InvoiceOperation_Index = "1";

		#endregion

		#endregion
		#endregion
	}
}
