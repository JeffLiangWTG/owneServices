using System.Collections.Generic;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC057C;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC057CProvider
	{
		Cc057CType XmlObject { get; }

		public CC057CProvider(Cc057CType xmlObject)
		{
			XmlObject = xmlObject;
		}

		public ZString MRN => XmlObject.TransitOperation?.Mrn ?? ZString.Empty;
		public ZString RejectionCode => XmlObject.TransitOperation?.RejectionCode ?? ZString.Empty;
		public ZString RejectionReason => XmlObject.TransitOperation?.RejectionReason ?? ZString.Empty;
		public IReadOnlyCollection<(ZString ErrorPointer, ZString ErrorCode, ZString ErrorReason, ZString OriginalAttributeValue)> FunctionalErrors
		{
			get
			{
				if (errors == null)
				{
					var list = new List<(ZString ErrorPointer, ZString ErrorCode, ZString ErrorReason, ZString OriginalAttributeValue)>();
					var functionalError = XmlObject.FunctionalError;
					if (functionalError != null)
					{
						foreach (var error in functionalError)
						{
							list.Add((error.ErrorPointer, error.ErrorCode.GetXmlEnumAttributeValue(), error.ErrorReason, error.OriginalAttributeValue));
						}
					}
					errors = list;
				}

				return errors;
			}
		}

		IReadOnlyCollection<(ZString ErrorPointer, ZString ErrorCode, ZString ErrorReason, ZString OriginalAttributeValue)> errors;
	}
}
