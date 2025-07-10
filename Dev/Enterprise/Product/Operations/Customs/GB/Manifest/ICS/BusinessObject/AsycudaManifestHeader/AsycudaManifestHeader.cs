using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.ICS.Business
{
	public class AsycudaManifestHeader : AsycudaManifestHeaderBase, Integration.Customs.GB.GBICS.IAsycudaManifestHeader
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public override ASYCUDA.Business.BaseMessageSendingNotificationHelper GetMessageSendingNotificationHelper()
		{
			return new GBMessageSendingNotificationHelper(this);
		}

		protected override string HumanReadableNamePrefixCore => Constants.MessageSubTypePreFixes.ICS;
	}
}
