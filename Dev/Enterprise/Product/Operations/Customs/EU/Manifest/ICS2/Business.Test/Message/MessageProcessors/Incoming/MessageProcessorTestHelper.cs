using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	public static class MessageProcessorTestHelper
	{
		public static IEnumerable<string> GetAllRequestInformationText(AsycudaManifestHeader manifestHeader)
		{
			foreach (var requestInformation in manifestHeader.RequestHeaders.Cast<RequestHeader>().SelectMany(x => x.RequestInformations.Cast<RequestInformation>()))
			{
				yield return requestInformation.CSI_Description;
			}
		}

		public static IEnumerable<(string, string)> GetAllSupportingDocuments(AsycudaManifestHeader manifestHeader)
		{
			foreach (var supportingDocument in manifestHeader.RequestHeaders.Cast<RequestHeader>().SelectMany(x => x.SupportingDocuments.Cast<SupportingDocument>()))
			{
				yield return (supportingDocument.CSI_Code, supportingDocument.CSI_ReferenceNumber);
			}
		}
	}
}
