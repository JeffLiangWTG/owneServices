using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CHGTSTCusTempStorageLineProvider : CusTempStorageLineProvider, ICHGTSTTempStorageLine
	{
		public CHGTSTCusTempStorageLineProvider(CusTempStorageLine storageLine)
			: base(storageLine)
		{
		}

		public string CustomsAuthorisationNumber
		{
			get
			{
				var result = string.Empty;
				var branchPK = GlbBranch.CurrentBranch.OrgProxy?.PK ?? ZGuid.Empty;
				if (!branchPK.IsEmpty)
				{
					result = CusAuthorisationHeader.Loader.GetAuthorisationNumber(storageLine.Factory,
						Core.Constants.CountryCodes.Germany,
						CusAuthorizationHeaderTypeList.Codes.TemporaryStorage,
						storageLine.StorageHeader.SJH_PresentationDate,
						branchPK);
				}
				return result.IsEmpty() ? PreviousReferenceType.Codes._OHNE : result;
			}
		}
	}
}
