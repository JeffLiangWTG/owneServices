using System.Collections.Generic;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC917C;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC917CProvider
	{
		public CC917CProvider(Cc917CType xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Cc917CType xmlObject;

		public ZString LRN => xmlObject.Header?.Lrn;

		public ZString MRN => xmlObject.Header?.Mrn;

		public IReadOnlyCollection<(ZString ErrorLineNumber, ZString ErrorColumnNumber, ZString ErrorPointer, ZString ErrorCode, ZString ErrorText)> XMLErrors
		{
			get
			{
				if (errors == null)
				{
					var list = new List<(ZString ErrorLineNumber, ZString ErrorColumnNumber, ZString ErrorPointer, ZString ErrorCode, ZString ErrorText)>();
					var functionalError = xmlObject.XmlError;
					if (functionalError != null)
					{
						foreach (var error in functionalError)
						{
							list.Add((error.ErrorLineNumber, error.ErrorColumnNumber, error.ErrorPointer, error.ErrorCode.GetXmlEnumAttributeValue(), error.ErrorText));
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
