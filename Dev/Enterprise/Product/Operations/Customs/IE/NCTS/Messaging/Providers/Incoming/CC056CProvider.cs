using System.Collections.Generic;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC056C;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC056CProvider
	{
		Cc056CType XmlObject { get; }

		public CC056CProvider(Cc056CType xmlObject)
		{
			XmlObject = xmlObject;
		}

		public ZString MRN => XmlObject.TransitOperation?.Mrn ?? ZString.Empty;
		public ZDateTime RejectionDateAndTime => (XmlObject.TransitOperation?.RejectionDateAndTime).ConvertToZDateTime();
		public ZString RejectionCode => XmlObject.TransitOperation?.RejectionCode ?? ZString.Empty;
		public ZString RejectionReason => XmlObject.TransitOperation?.RejectionReason ?? ZString.Empty;
		public ZString BusinessRejectionType => XmlObject.TransitOperation?.BusinessRejectionType ?? ZString.Empty;

		public IReadOnlyCollection<(ZString ErrorPointer, ZString ErrorCode, ZString ErrorReason)> FunctionalErrors
		{
			get
			{
				if (errors == null)
				{
					var list = new List<(ZString ErrorPointer, ZString ErrorCode, ZString ErrorReason)>();
					var functionalError = XmlObject.FunctionalError;
					if (functionalError != null)
					{
						foreach (var error in functionalError)
						{
							list.Add((error.ErrorPointer, error.ErrorCode.GetXmlEnumAttributeValue(), error.ErrorReason));
						}
					}
					errors = list;
				}

				return errors;
			}
		}
		IReadOnlyCollection<(ZString ErrorPointer, ZString ErrorCode, ZString ErrorReason)> errors;
	}
}
