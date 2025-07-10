using System.Collections.Generic;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using IAddress = CargoWise.Customs.EU.MessageContracts.ICS2.IAddress;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class HrcmScreeningResultsProvider : IHrcmScreeningResults
	{
		public HrcmScreeningResultsProvider(AsycudaBillScreening billScreening)
		{
			asycudaBillScreening = Argument.NotNull(billScreening, nameof(billScreening));
		}

		public HrcmScreeningResultsProvider(AsycudaBillScreening billScreening, string referralRequestReference)
		{
			asycudaBillScreening = Argument.NotNull(billScreening, nameof(billScreening));
			this.referralRequestReference = referralRequestReference;
		}

		readonly AsycudaBillScreening asycudaBillScreening;
		readonly string referralRequestReference;

		public string ReferralRequestReference => referralRequestReference ?? asycudaBillScreening.ASR_TransportNumber;

		public string Result => asycudaBillScreening.ASR_Result;

		public IReadOnlyCollection<string> ScreeningMethods => screeningMethods ?? (screeningMethods = GetScreeningMethod());
		IReadOnlyCollection<string> screeningMethods;

		IReadOnlyCollection<string> GetScreeningMethod()
		{
			return MessageProviderHelper.ToArray<ScreeningMethod, string>(asycudaBillScreening.Bill.ScreeningMethods, (screeningMethod) => screeningMethod.CSI_Code.ToString());
		}

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformationCollection => additionalInformationCollection ?? (additionalInformationCollection = MessageProviderHelper.GetAdditionalInformationCollection(asycudaBillScreening.AdditionalInfos));
		IReadOnlyCollection<IAdditionalInformation> additionalInformationCollection;

		public string AuthorizedPersonName => asycudaBillScreening.ASR_AuthorizedPersonName;

		public string AuthorizedPersonIdentificationNumber => asycudaBillScreening.ASR_AuthorizedPersonIdentifier;

		public string AuthorizedPersonType => asycudaBillScreening.ASR_AuthorizedPersonType;

		public IReadOnlyCollection<IBinaryFile> BinaryFiles => binaryFiles ?? (binaryFiles = GetBinaryFiles());
		IReadOnlyCollection<IBinaryFile> binaryFiles;

		IReadOnlyCollection<IBinaryFile> GetBinaryFiles()
		{
			return MessageProviderHelper.ToArray<CusStorageDocPivot, IBinaryFile>(asycudaBillScreening.Bill.EDocPivotCollection, (attachment) => new BinaryFileProvider(attachment));
		}

		public IAddress FacilityPlace => CachedValueHelper.GetValue(ref facilityPlaceCached, () => ICS2JobDocAddressProvider.NewOrNull(asycudaBillScreening.FacilityPlace));
		CachedValue<IAddress> facilityPlaceCached;

		public IIdentifierTypePair TransportDocumentHouseLevel => CachedValueHelper.GetValue(ref transportDocumentHouseLevelCached, () =>
		{
			var bill = asycudaBillScreening.Bill;
			return bill.IsChildMasterBill ? null : TransportDocumentProvider.NewOrNull(bill.ABL_BillNumber, bill.TransportDocumentType);
		});

		CachedValue<IIdentifierTypePair> transportDocumentHouseLevelCached;
	}
}
