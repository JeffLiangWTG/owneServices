using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class AsycudaPack : ASYCUDA.Business.AsycudaPack
		, Integration.Customs.ASYCUDA.CLManifest.IAsycudaPack
	{
		public AsycudaPack(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaBill Bill => (AsycudaBill)base.Bill;

		public new AsycudaPackLookups Lookups => (AsycudaPackLookups)base.Lookups;

		protected override ManifestBase.AsycudaPackLookups GetNewLookups() => new AsycudaPackLookups(this);

		public override void Delete()
		{
			this.DeleteChildren<AsycudaArrivalLine>(AsycudaArrivalLineSchema.ATL_APA_AsycudaPack);
			base.Delete();
		}

		#region Overrided properties

		public override bool CanDelete => Bill.CanDelete;

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				switch (Bill.ABL_BillStatus)
				{
					case MessageStatusCodeList.Codes.Accepted:
						return ResString.GetMultilingualString("33D8C9AC-B3DC-4D58-BABF-71EFDE32CCC9", "This Bill is already sent and accepted.");
					case MessageStatusCodeList.Codes.Sent when Bill.ABL_MessageStatus == MessageStatusCodeList.Codes.Awaiting:
						return ResString.GetMultilingualString("F898FC5E-E47E-42E3-913F-1BA3ECA74C91", "This Bill is already sent and it is awaiting for a response.");
					case MessageStatusCodeList.Codes.Cancel:
						return ResString.GetMultilingualString("9DE2398A-CF21-44D7-92CC-41B2561C54AB", "This bill is canceled. Packs must be kept for history purposes.");
					default:
						return base.ReasonForNotAbleToDelete;
				}
			}
		}

		#endregion

		protected override bool IsPackUQNeedToConvertCore => false;
	}
}
