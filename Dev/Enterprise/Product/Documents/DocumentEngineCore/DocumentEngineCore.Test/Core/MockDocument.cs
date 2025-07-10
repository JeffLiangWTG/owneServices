using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngineCore.Testing
{
	sealed class MockDocument : IDocument
	{
		public MockDocument(string documentName, string documentDeliveryMethod)
		{
			DocumentName = documentName;
			DocumentDeliveryMethod = documentDeliveryMethod;
		}

		public string DocumentDeliveryMethod { get; set; }
		public string DocumentName { get; set; }
		public bool IncludeInPrint { get; set; }
		public bool CanIncludeInPrint { get; set; }

		public IEnumerable<string> GetSupportedDeliveryMethods()
		{
			if (DocumentDeliveryMethod.ToUpper() == nameof(PrintCopyType.ALL))
			{
				return Core.Constants.ContactNotifyModes.All;
			}
			else if (DocumentDeliveryMethod.ToUpper() == nameof(PrintCopyType.PRN))
			{
				return new string[] { Core.Constants.ContactNotifyModes.Print, Core.Constants.ContactNotifyModes.EPrint };
			}
			else if (DocumentDeliveryMethod.ToUpper() == nameof(PrintCopyType.EML))
			{
				return new string[] { Core.Constants.ContactNotifyModes.Email, Core.Constants.ContactNotifyModes.EPrint };
			}
			else if (DocumentDeliveryMethod.ToUpper() == nameof(PrintCopyType.FAX))
			{
				return new string[] { Core.Constants.ContactNotifyModes.Fax };
			}

			return System.Array.Empty<string>();
		}

		public IEnumerable<string> GetSupportedDeliveryMethodDespiteOfPrintCopyType()
		{
			return GetSupportedDeliveryMethods();
		}

		public bool SupportsDeliveryMethod(string deliveryMethod)
		{
			return GetSupportedDeliveryMethods().Any(method => method == deliveryMethod?.ToUpper());
		}
	}
}
