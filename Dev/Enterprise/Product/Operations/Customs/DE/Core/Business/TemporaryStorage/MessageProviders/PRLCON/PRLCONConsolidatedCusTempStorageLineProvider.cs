using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class PRLCONConsolidatedCusTempStorageLineProvider : CusTempStorageLineProvider, IPRLCONConsolidatedTempStorageLine
	{
		public PRLCONConsolidatedCusTempStorageLineProvider(CusTempStorageLine storageLine)
			: base(storageLine)
		{
		}

		public string CustomsAuthorisationNumber
		{
			get
			{
				var result = string.Empty;
				var custodianPk = storageLine.Custodian?.Header.PK ?? ZGuid.Empty;
				if (!custodianPk.IsEmpty)
				{
					result = CusAuthorisationHeader.Loader.GetAuthorisationNumber(storageLine.Factory,
						Core.Constants.CountryCodes.Germany,
						CusAuthorizationHeaderTypeList.Codes.TemporaryStorage,
						storageLine.StorageHeader.SJH_PresentationDate,
						custodianPk);
				}
				return result.IsEmpty() ? PreviousReferenceType.Codes._OHNE : result;
			}
		}
	}
}
