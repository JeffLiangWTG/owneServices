using System.Collections.Generic;

namespace Enterprise.Integration.DocumentEngine
{
	public interface IDocument
	{
		string DocumentDeliveryMethod { get; }
		string DocumentName { get; }
		bool IncludeInPrint { get; }
		bool CanIncludeInPrint { get; set; }

		bool SupportsDeliveryMethod(string deliveryMethod);

		IEnumerable<string> GetSupportedDeliveryMethods();
		IEnumerable<string> GetSupportedDeliveryMethodDespiteOfPrintCopyType();
	}
}
