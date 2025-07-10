using System.Collections.Generic;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC917C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class CC917CProvider
	{
		public CC917CProvider(Cc917C xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Cc917C xmlObject;

		public ZString LocalReferenceNumber => xmlObject.Header?.Lrn;

		public ZString MovementReferenceNumber => xmlObject.Header?.Mrn;

		public IReadOnlyCollection<(ZString ErrorLineNumber, ZString ErrorColumnNumber, ZString ErrorPointer, ZString ErrorCode, ZString ErrorText)> Errors
		{
			get
			{
				if (errors == null)
				{
					var list = new List<(ZString ErrorLineNumber, ZString ErrorColumnNumber, ZString ErrorPointer, ZString ErrorCode, ZString ErrorText)>();
					var xmlError = xmlObject.XmlError;
					if (xmlError != null)
					{
						foreach (var error in xmlError)
						{
							var isValidErrorCode = error.ErrorCode != XmlErrorCodes.Empty;
							list.Add((error.ErrorLineNumber, error.ErrorColumnNumber, isValidErrorCode ? error.ErrorPointer : string.Empty, isValidErrorCode ? error.ErrorCode.GetXmlEnumAttributeValue() : string.Empty, error.ErrorText));
						}
					}
					errors = list;
				}

				return errors;
			}
		}

		IReadOnlyCollection<(ZString ErrorLineNumber, ZString ErrorColumnNumber, ZString ErrorPointer, ZString ErrorCode, ZString ErrorText)> errors;
	}
}
