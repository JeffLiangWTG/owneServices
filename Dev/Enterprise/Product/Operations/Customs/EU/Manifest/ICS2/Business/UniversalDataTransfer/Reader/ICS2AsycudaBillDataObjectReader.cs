using System;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class ICS2AsycudaBillDataObjectReader : AsycudaBillDataObjectReader
	{
		public ICS2AsycudaBillDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ASYCUDA.Business.AsycudaManifestHeader header, AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled)
			: base(dataObject, logger, factory, header, helper, isUpdateEnabled)
		{
		}

		protected override void PopulateBillForSpecificRules(ASYCUDA.Business.AsycudaBill bill)
		{
			base.PopulateBillForSpecificRules(bill);

			if (bill is AsycudaBill ics2Bill)
			{
				FillMasterBillScreens(ics2Bill);
				FillTransportDocumentType(ics2Bill);
			}
		}

		public void FillMasterBillScreens(AsycudaBill bill)
		{
			if (bill != null && dataObject.BillScreeningCollection != null)
			{
				var supportedTypes = helper.GetBillSupportedCusSupportingInfoCSI_Types(bill);
				bill.BillScreenings.RemoveAndDeleteAll();

				foreach (var dataObject in dataObject.BillScreeningCollection)
				{
					var billScreening = bill.BillScreenings.AddNew();

					SetValue(billScreening, AsycudaBillScreeningSchema.ASR_Result, dataObject.Result);
					SetValue(billScreening, AsycudaBillScreeningSchema.ASR_AuthorizedPersonName, dataObject.AuthorisedPerson);
					SetValue(billScreening, AsycudaBillScreeningSchema.ASR_AuthorizedPersonType, dataObject.PersonType);
					SetValue(billScreening, AsycudaBillScreeningSchema.ASR_AuthorizedPersonIdentifier, dataObject.PersonIdentifier);

					var organizationAddress = dataObject.FacilityPlace;
					if (organizationAddress != null)
					{
						var facilityPlace = billScreening.FacilityPlace;

						var orgReader = new OrganisationDataObjectReader(organizationAddress, logger, factory);
						var orgAddress = orgReader.GetMatched();
						orgReader.PopulateJobDocAddress(orgAddress, facilityPlace);

						SetValueIfNotNull(dataObject.FacilityPlaceSubDivision, (value) => facilityPlace.SubDivision = value);
						SetValueIfNotNull(dataObject.FacilityPlaceNumber, (value) => facilityPlace.Number = value);
						SetValueIfNotNull(dataObject.FacilityPlacePOBox, (value) => facilityPlace.POBox = value);
					}

					if (supportedTypes.Length > 0)
					{
						var reader = new CustomsSupportingInformationCollectionDataObjectReader(logger, new UniversalDataObjectReaderHelper(factory, header.AMA_RN_NKCountry, header.AMA_RN_NKCountry));
						reader.ReadIntoDataRows(billScreening.PK, billScreening.TablePrefix, billScreening.IsInDatabase, dataObject, supportedTypes);
					}
				}
			}

			void SetValueIfNotNull(ZString? text, Action<ZString> setter)
			{
				if (text.HasValue)
				{
					setter(text.Value);
				}
			}
		}

		void FillTransportDocumentType(AsycudaBill bill)
		{
			var transportDocumentType = dataObject.AddInfoCollection.GetZStringValue(AsycudaBill.Schema.TransportDocumentType, logger);
			if (transportDocumentType.HasValue)
			{
				var transportDocumentTypeDetail = new GenAddOnDetail()
				{
					GenAddOnColumnName = AsycudaBill.Schema.TransportDocumentType,
					PropertyName = AsycudaBill.Schema.TransportDocumentType,
					Value = transportDocumentType.Value
				};
				transportDocumentTypeDetail.ReadIntoBusinessObject(IsDefaultingEnabled, bill);
			}
		}
	}
}
