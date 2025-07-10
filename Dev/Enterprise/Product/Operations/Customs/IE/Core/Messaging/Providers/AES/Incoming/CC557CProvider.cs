using System.Collections.Generic;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC557C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class CC557CProvider
	{
		public CC557CProvider(Cc557C xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Cc557C xmlObject;

		public ZString LocalReferenceNumber => xmlObject.ExportOperation?.Lrn;

		public ZString MovementReferenceNumber => xmlObject.ExportOperation?.Mrn;

		public ZString BusinessRejectionType => xmlObject.ExportOperation?.BusinessRejectionType;

		public ZDateTime RejectionDateAndTime => new ZDateTime(xmlObject.ExportOperation?.RejectionDateAndTime);

		public ZString RejectionCode => xmlObject.ExportOperation?.RejectionCode;

		public ZString RejectionReason => xmlObject.ExportOperation?.RejectionReason;

		public IReadOnlyCollection<(ZString ErrorPointer, ZString ErrorCode, ZString ErrorReason)> FunctionalErrors
		{
			get
			{
				if (errors == null)
				{
					var list = new List<(ZString ErrorPointer, ZString ErrorCode, ZString ErrorReason)>();
					var functionalError = xmlObject.FunctionalError;
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
