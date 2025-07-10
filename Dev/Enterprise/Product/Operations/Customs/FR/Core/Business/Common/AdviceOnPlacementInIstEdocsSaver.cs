using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business
{
	public class AdviceOnPlacementInIstEDocsSaver : BaseEDocsSaver<CusTempStorageJobHeader>
	{
		public AdviceOnPlacementInIstEDocsSaver(CusTempStorageJobHeader storageHeader) : base(storageHeader)
		{
		}

		protected override ZGuid BranchPKCore => target.SJH_GB;

		protected override ZString DocTitleCore => new ZString((NoResString)"Advice On Placement In IST");

		protected override ZString DocNameCore => new ZString($"IST - {target.CusTempStorageDec.STH_OwnerReferenceNumber}");

		protected override ZGuid DocumentMenuItemPKCore => new ZGuid("38141500-b70e-461a-8231-5561c0819362"); // PK for Advice Of Placement In IST
	}
}
