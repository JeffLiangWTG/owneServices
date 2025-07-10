using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CUSPRLCusTempStorageLineProvider : CusTempStorageLineProvider, ICUSPRLTempStorageLine
	{
		public CUSPRLCusTempStorageLineProvider(CusTempStorageLine storageLine)
			: base(storageLine)
		{
		}

		public bool HasPreliminaryChanges => storageLine.TSL_IsModified;

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

		public string TransportNumberType => storageLine.TSL_TransportNumberType;

		public string TransportReferenceNumber => storageLine.TSL_TransportNumber;

		public string CarrierEoriNumber
		{
			get
			{
				var carrierDocAddress = ((CUSPRLCusTempStorageLine)storageLine).CarrierDocAddress;
				var carrierEORI = carrierDocAddress?.Organisation?.GetEORI();
				return (PreviousReferenceTypeIsN355 && !string.IsNullOrEmpty(carrierEORI)) ? carrierEORI : null;
			}
		}

		public string TransportDocumentMasterLevelType
		{
			get
			{
				var documentType = ((CUSPRLCusTempStorageLine)storageLine).TransportDocumentMaster?.CSI_Code;
				return (PreviousReferenceTypeIsN355 && !string.IsNullOrEmpty(documentType)) ? documentType : null;
			}
		}

		public string TransportDocumentMasterLevelNumber
		{
			get
			{
				var documentNumber = ((CUSPRLCusTempStorageLine)storageLine).TransportDocumentMaster?.CSI_ReferenceNumber;
				return (PreviousReferenceTypeIsN355 && !string.IsNullOrEmpty(documentNumber)) ? documentNumber : null;
			}
		}

		public string ReceptacleIdentificationNumber
		{
			get
			{
				var receptacle = ((CUSPRLCusTempStorageLine)storageLine).Receptacle.ToString();
				return (PreviousReferenceTypeIsN355 && !string.IsNullOrEmpty(receptacle) && string.IsNullOrEmpty(storageLine.TSL_TransportNumberType)) ? receptacle : null;
			}
		}

		public string TransportEquipmentIdentificationNumber
		{
			get
			{
				var containerNumber = ((CUSPRLCusTempStorageLine)storageLine).ContainerNumber.ToString();
				return (PreviousReferenceTypeIsN355 && !string.IsNullOrEmpty(containerNumber) && !storageLine.TSL_ReferenceNumber.IsEmpty) ? containerNumber : null;
			}
		}

		public bool PreviousReferenceTypeIsN355 => CachedValueHelper.GetValue(ref previousReferenceTypeIsN355, () => storageLine.StorageHeader.SJH_PreviousReferenceType == PreviousReferenceType.Codes._N355);
		CachedValue<bool> previousReferenceTypeIsN355;
	}
}
