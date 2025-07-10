using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Environment;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class SumACusTempStorageJobHeaderProvider : ITempStorageHeader
	{
		public SumACusTempStorageJobHeaderProvider(CusTempStorageJobHeader tempStorageHeader)
		{
			TempStorageHeader = Argument.NotNull(tempStorageHeader, nameof(tempStorageHeader));
		}
		protected readonly CusTempStorageJobHeader TempStorageHeader;

		public IDateAndTime PreparationDateAndTimeCET => preparationDateTimeCET ?? (preparationDateTimeCET = new CentralEuropeanStandardDateAndTimeProvider(true));
		IDateAndTime preparationDateTimeCET;

		public string InterchangeSenderEoriNumber
		{
			get
			{
				var result = TempStorageHeader.RepresentativeEoriNumber;
				if (result.IsEmpty)
				{
					result = TempStorageHeader.PresenterEoriNumber;
				}
				return result;
			}
		}

		public string InterchangeSenderEoriBranch
		{
			get
			{
				var result = TempStorageHeader.RepresentativeEoriBranch;
				if (result.IsEmpty)
				{
					result = TempStorageHeader.PresenterEoriBranch;
				}
				return result;
			}
		}

		public string InterchangeRecipientReferenceNumber => TempStorageHeader.SJH_CustomsOffice;

		public string LocalReferenceNumber => TempStorageHeader.SJH_ReferenceNumber.ValueOrNullIfEmpty() ?? TempStorageHeader.SJH_JobReference;

		public DateTime? ArrivalDate => TempStorageHeader.SJH_ArrivalDate.IsValid ? TempStorageHeader.SJH_ArrivalDate.ToDateTime() : null;

		public DateTime? PresentationDate => TempStorageHeader.SJH_PresentationDate.IsValid ? TempStorageHeader.SJH_PresentationDate.ToDateTime() : null;

		public bool NCTSFlag => TempStorageHeader.SJH_NCTSFlag && !TempStorageHeader.NCTSFlagNotSupported;

		public bool MaritimeTransportFlag => TempStorageHeader.IsSea && (TempStorageHeader.CustomsOffice?.Attributes?.Cast<ZZRefCusCodeListAttributeCombined>().Any(x => x.ZZE_IsSea) ?? false);

		public string LoadingPlace => TempStorageHeader.Loading?.RL_IATA;

		public string UnloadingPlace => TempStorageHeader.Loading?.RL_PortName;

		public string AdditionalInformation => TempStorageHeader.SJH_AdditionalInformation;

		public string AuthorisationNumber
		{
			get
			{
				var result = TempStorageHeader.Representative?.GetATLASParticipantIdentificationNumber();
				if (string.IsNullOrEmpty(result))
				{
					result = TempStorageHeader.Presenter?.GetATLASParticipantIdentificationNumber();
				}
				return result;
			}
		}

		public string PresenterEoriNumber => TempStorageHeader.PresenterEoriNumber;

		public string PresenterEoriBranch => TempStorageHeader.PresenterEoriBranch;

		public string PresenterName => TempStorageHeader.Presenter?.Header.OH_FullName;

		public string PresenterAddress => TempStorageHeader.Presenter?.OA_Address1;

		public string PresenterCountry => TempStorageHeader.Presenter?.OA_RN_NKCountryCode;

		public string PresenterPostcode => TempStorageHeader.Presenter?.OA_PostCode;

		public string PresenterCity => TempStorageHeader.Presenter?.OA_City;

		public string PresenterDistrict => TempStorageHeader.Presenter?.OA_Address2;

		public string RepresentativeEoriNumber => TempStorageHeader.RepresentativeEoriNumber;

		public string RepresentativeEoriBranch => TempStorageHeader.RepresentativeEoriBranch;

		public string ContactName => Env.CurrentUser.FullName;

		public string ContactPosition => Env.CurrentUser.Title;

		public string ContactPhoneNumber => Env.CurrentUser.WorkPhone;

		public string ContactEmailAddress => Env.CurrentUser.EmailAddress;

		public string FirstEntryCustomsOfficeReferenceNumber => TempStorageHeader.SJH_CustomsOfficeOfEntryIntoEU;

		public string TransportMeansCode => TempStorageHeader.SJH_TransportMeansCode;

		public string BorderTransportDescription => TempStorageHeader.SJH_TransportMeansDescription;

		public string TransportRegistrationNumber => TempStorageHeader.SJH_TransportRegNo;

		public string PreviousReferenceType => TempStorageHeader.SJH_PreviousReferenceType;

		public string PreviousReferenceNumber => TempStorageHeader.SJH_PreviousReferenceNumber;

		public int ContainerQuantity => TempStorageHeader.SJH_ContainerCount;
	}
}
