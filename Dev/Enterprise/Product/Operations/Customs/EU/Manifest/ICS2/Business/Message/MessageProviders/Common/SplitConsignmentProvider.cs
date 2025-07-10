using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SplitConsignmentProvider : ISplitConsignment
	{
		readonly AsycudaManifestHeader manifestHeader;

		public static SplitConsignmentProvider NewOrNull(AsycudaManifestHeader manifestHeader) => manifestHeader == null ? null : new SplitConsignmentProvider(manifestHeader);

		SplitConsignmentProvider(AsycudaManifestHeader manifestHeader)
		{
			this.manifestHeader = Argument.NotNull(manifestHeader, nameof(manifestHeader));
		}

		public string SplitConsignmentIndicator => manifestHeader.SplitConsignmentIndicator ? "1" : "0";

		public string PreviousMRN => manifestHeader.PreviousMRN;
	}
}
