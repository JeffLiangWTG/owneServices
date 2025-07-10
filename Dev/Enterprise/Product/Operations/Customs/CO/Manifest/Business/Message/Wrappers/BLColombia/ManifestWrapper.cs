using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.CO.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CO.Manifest.Business
{
	public class ManifestWrapper : IManifest
	{
		public ManifestWrapper(AsycudaManifestHeader header, ZString action, List<CusTransactionNumber> documentIDs)
		{
			this.header = Argument.NotNull(header, "AsycudaManifestHeader cannot be null");
			this.action = action;
			this.documentIDs = documentIDs;
		}
		readonly AsycudaManifestHeader header;
		readonly ZString action;
		readonly List<CusTransactionNumber> documentIDs;

		IHeader IManifest.Header => iHeader ?? (iHeader = new HeaderWrapper(header, action));
		IHeader iHeader;

		IMaster IManifest.Master => master ?? (master = new MasterBillWrapper(header, documentIDs));
		IMaster master;
	}
}
