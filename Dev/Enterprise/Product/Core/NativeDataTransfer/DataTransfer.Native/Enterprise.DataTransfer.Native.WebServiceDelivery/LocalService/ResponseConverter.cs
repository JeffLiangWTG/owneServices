using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Integration;

namespace Enterprise.DataTransfer.Native.WebServiceDelivery.LocalService
{
	public class ResponseConverter : IConverter<XElement, IResponseMessage>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Namespace Identity, Keyword for Content of XML")]
		public IResponseMessage Convert(XElement source)
		{
			var natigator = new XmlNamespaceManager(new NameTable());
			natigator.AddNamespace("cw", ReferenceDataXMLForDeSerialize.NameSpace_Universal);
			var statusElement = source.XPathSelectElement("./cw:Status", natigator);

			if (statusElement == null)
			{
				return new ResponseMessageData
				{
					HasError = true,
					ErrorMessage = ErrorMessageMissingStatusTag,
					ResponseMessage = source.ToString(),
				};
			}
			if (statusElement.Value == "Rejected")
			{
				return new ResponseMessageData
				{
					HasError = true,
					ErrorMessage = source.ToString(),
					ResponseMessage = source.ToString()
				};
			}
			return new ResponseMessageData
			{
				HasError = false,
				ResponseMessage = source.ToString()
			};
		}

		internal static string ErrorMessageMissingStatusTag
		{
			get { return Res.GetString("0b68ed1e-f396-461d-8005-d05e9f6343d9", @"Error Parsing 'Response' tag. Opening Tag should take the form: <Response xmlns=""http://www.cargowise.com/Schemas/Universal"">"); }
		}
	}
}
