using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina
{
	public interface IClsFEXAuthRequestBuilder
	{
		XStreamingElement BuildXML(TransactionInfo transaction, XNamespace fexv1, ZString registrationNumber);
	}

	class ClsFEXAuthRequestBuilder : IClsFEXAuthRequestBuilder
	{
		XStreamingElement IClsFEXAuthRequestBuilder.BuildXML(TransactionInfo transaction, XNamespace fexv1, ZString registrationNumber)
		{
			#region SuppressResourceStringsCheckRegion

			return new XStreamingElement(fexv1 + "Auth",
							new XElement(fexv1 + "Token", ""),
							new XElement(fexv1 + "Sign", ""),
							new XElement(fexv1 + "Cuit", registrationNumber.RemoveNonNumericCharacters()));

			#endregion
		}
	}
}
