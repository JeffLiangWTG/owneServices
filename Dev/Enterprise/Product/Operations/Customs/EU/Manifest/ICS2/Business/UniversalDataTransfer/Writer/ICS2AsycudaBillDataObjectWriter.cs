using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class ICS2AsycudaBillDataObjectWriter : AsycudaBillDataObjectWriter<AsycudaBill>
	{
		public ICS2AsycudaBillDataObjectWriter(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
			: base(manager, helper)
		{
		}

		protected override void PopulateSpecificData(AsycudaBill sourceBill, Shipment uxml)
		{
			base.PopulateSpecificData(sourceBill, uxml);
			PopulateBillScreens(sourceBill, uxml);
		}

		public void PopulateBillScreens(AsycudaBill sourceBill, Shipment shipment)
		{
			shipment.SetBillScreeningCollection(() =>
			{
				var collection = new List<BillScreening>();

				foreach (AsycudaBillScreening billScreening in sourceBill.BillScreenings)
				{
					var dataObject = new BillScreening(writeManager.WriterStrategy);
					dataObject.Result = billScreening.ASR_Result;
					dataObject.AuthorisedPerson = billScreening.ASR_AuthorizedPersonName;
					dataObject.PersonType = billScreening.ASR_AuthorizedPersonType;
					dataObject.PersonIdentifier = billScreening.ASR_AuthorizedPersonIdentifier;

					var facilityPlace = billScreening.FacilityPlace;
					if (facilityPlace != null)
					{
						dataObject.FacilityPlaceSubDivision = facilityPlace.SubDivision;
						dataObject.FacilityPlaceNumber = facilityPlace.Number;
						dataObject.FacilityPlacePOBox = facilityPlace.POBox;

						dataObject.FacilityPlace = new JobDocAddressDataObjectWriter(writeManager).GetDataObject(facilityPlace);
					}

					var supportingInfoList = new List<CustomsSupportingInformation>();

					foreach (var additionalInfo in billScreening.AdditionalInfos)
					{
						var supportingInfo = CustomsSupportingInformationCollectionCreator.Create(additionalInfo, additionalInfo.Lookups.TypeList, writeManager);
						supportingInfoList.Add(supportingInfo);
					}

					if (supportingInfoList.Count > 0)
					{
						dataObject.SetCustomsSupportingInformationCollection(() => supportingInfoList);
					}

					collection.Add(dataObject);
				}

				return collection;
			});
		}

		protected override void PopulateAddInfos(AsycudaBill billBO, Shipment billData)
		{
			base.PopulateAddInfos(billBO, billData);
			billData.AddAddInfo(AsycudaBill.Schema.TransportDocumentType, billBO.TransportDocumentType);
		}
	}
}
