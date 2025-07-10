using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class ICS2AsycudaManifestHeaderDataObjectWriterTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestExportAsycudaManifestHeaderAdditionalInfos()
		{
			var header = ICS2DataObjectWriterTestHelper.SetupManifestHeader(Factory);

			var additionalInfo = header.AdditionalInfos.AddNew();
			additionalInfo.CSI_Code = "10600";
			additionalInfo.CSI_Type = "OTH";

			Factory.SaveForTesting();

			var headerData = UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, header) as UniversalShipment;

			var customsSupportingInformationCollection = headerData.CustomsSupportingInformationCollection;
			CombineAssertions("CustomsSupportInformationCollection", () =>
			{
				AssertNotNull(customsSupportingInformationCollection);
				AssertEquals(1, customsSupportingInformationCollection.Count);
				AssertEquals("10600", customsSupportingInformationCollection[0].Type.Code);
				AssertEquals("Consignee Unknown", customsSupportingInformationCollection[0].Type.Description);
				AssertEquals("OTH", customsSupportingInformationCollection[0].Category.Code);
			});
		}

		public void TestExportAsycudaBillAdditionalInfos()
		{
			var header = ICS2DataObjectWriterTestHelper.SetupManifestHeader(Factory);

			var bill = header.Bills.AddNew();

			var additionalInfo = bill.AdditionalInfos.AddNew();
			additionalInfo.CSI_Code = "10600";
			additionalInfo.CSI_Type = "OTH";

			Factory.SaveForTesting();

			var headerData = UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, header) as UniversalShipment;

			var billDataObject = headerData.SubShipmentCollection.Single();
			var customsSupportingInformationCollection = billDataObject.CustomsSupportingInformationCollection;
			CombineAssertions("CustomsSupportInformationCollection", () =>
			{
				AssertNotNull(customsSupportingInformationCollection);
				AssertEquals(1, customsSupportingInformationCollection.Count);
				AssertEquals("10600", customsSupportingInformationCollection[0].Type.Code);
				AssertEquals("Consignee Unknown", customsSupportingInformationCollection[0].Type.Description);
				AssertEquals("OTH", customsSupportingInformationCollection[0].Category.Code);
			});
		}

		public void TestExportAsycudaManifestHeaderCusSupplyChainActorReferences()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var orgHeader = orgAddress.Header;
			orgHeader.OH_Code = "EU_ICS2";

			var header = ICS2DataObjectWriterTestHelper.SetupManifestHeader(Factory);

			var cusSupplyChainActorReference = header.CusSupplyChainActorReferences.AddNew();
			cusSupplyChainActorReference.CFR_Code = "CS";
			cusSupplyChainActorReference.CFR_Type = "SCA";
			cusSupplyChainActorReference.CFR_Reference = "1223334444";
			cusSupplyChainActorReference.CFR_OA_Owner = orgAddress.PK;

			Factory.SaveForTesting();

			var headerData = UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, header) as UniversalShipment;

			var customsReferencesCollection = headerData.CustomsReferenceCollection;
			CombineAssertions("CustomsReferenceCollection", () =>
			{
				AssertNotNull(customsReferencesCollection);
				AssertEquals(1, customsReferencesCollection.Count);
				AssertEquals("SCA", customsReferencesCollection[0].Type.Code);
				AssertEquals("CS", customsReferencesCollection[0].SubType.Code);
				AssertEquals("1223334444", customsReferencesCollection[0].Reference);
				AssertEquals("EU_ICS2", customsReferencesCollection[0].Owner.OrganizationCode);
			});
		}

		public void TestExportAsycudaManifestHeaderFields()
		{
			var header = ICS2DataObjectWriterTestHelper.SetupManifestHeader(Factory);

			header.SpecificCircumstanceIndicator = "F22";
			header.ReEntryIndicator = true;
			header.SplitConsignmentIndicator = true;
			header.MasterBill.TransportDocumentType = "N741";
			header.AMA_A_ARV = ZDateTime.BrettsBirthday;

			Factory.SaveForTesting();

			var headerData = UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, header) as UniversalShipment;

			var addInfoCollection = headerData.AddInfoCollection;
			var specificCircumstanceIndicatorDataObject = addInfoCollection.Single(x => x.Key.Value == "SpecificCircumstanceIndicator");
			var reEntryIndicatorDataObject = addInfoCollection.Single(x => x.Key.Value == "ReEntryIndicator");
			var splitConsignmentIndicatorDataObject = addInfoCollection.Single(x => x.Key.Value == "SplitConsignmentIndicator");
			var transportDocumentTypeDataObject = addInfoCollection.Single(x => x.Key.Value == "TransportDocumentType");
			var actualArrivalDate = headerData.DateCollection.FirstOrDefault(x => x.Type == DateType.ActualArrival).Value;

			CombineAssertions(() =>
			{
				AssertEquals("F22", specificCircumstanceIndicatorDataObject.Value.Value);
				AssertEquals("Y", reEntryIndicatorDataObject.Value.Value);
				AssertEquals("Y", splitConsignmentIndicatorDataObject.Value.Value);
				AssertEquals("N741", transportDocumentTypeDataObject.Value.Value);
				AssertEquals(ZDateTime.BrettsBirthday, actualArrivalDate);
			});
		}

		public void TestExportBillScreenings()
		{
			var header1 = Factory.NewWithValidTestData<OrgHeader>();
			var header2 = Factory.NewWithValidTestData<OrgHeader>();

			var header = ICS2DataObjectWriterTestHelper.SetupManifestHeader(Factory);
			header.AMA_MasterBill = "MB2023001";

			var masterBillScreen = header.MasterBill.BillScreenings.AddNew();
			masterBillScreen.ASR_Result = "1";
			masterBillScreen.ASR_AuthorizedPersonName = "Rick";
			masterBillScreen.ASR_AuthorizedPersonType = "1";
			masterBillScreen.ASR_AuthorizedPersonIdentifier = "C137";
			masterBillScreen.FacilityPlace.OrganisationPK = header1.PK;
			masterBillScreen.FacilityPlace.SubDivision = "TST";
			masterBillScreen.FacilityPlace.Number = "117777777";
			masterBillScreen.FacilityPlace.POBox = "20230101";

			var masterAdditionalInfo = masterBillScreen.AdditionalInfos.AddNew();
			masterAdditionalInfo.CSI_Code = "C1";
			masterAdditionalInfo.CSI_SubType = "A00";
			masterAdditionalInfo.CSI_Description = "TEST Master Bill AdditionalInfo";

			var houseBill = header.Bills.AddNew();
			houseBill.FillWithValidTestData();

			var houseBillScreen = houseBill.BillScreenings.AddNew();
			houseBillScreen.ASR_Result = "2";
			houseBillScreen.ASR_AuthorizedPersonName = "Morty";
			houseBillScreen.ASR_AuthorizedPersonType = "2";
			houseBillScreen.ASR_AuthorizedPersonIdentifier = "H137";
			houseBillScreen.FacilityPlace.OrganisationPK = header2.PK;
			houseBillScreen.FacilityPlace.SubDivision = "AXA";
			houseBillScreen.FacilityPlace.Number = "228888888";
			houseBillScreen.FacilityPlace.POBox = "20230707";

			var houseAdditionalInfo = houseBillScreen.AdditionalInfos.AddNew();
			houseAdditionalInfo.CSI_Code = "C2";
			houseAdditionalInfo.CSI_SubType = "A20";
			houseAdditionalInfo.CSI_Description = "TEST House Bill AdditionalInfo";

			Factory.SaveForTesting();

			var headerData = UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, header) as UniversalShipment;

			AssertBillScreening("MasterBill.BillScreeningCollection", masterBillScreen, headerData.BillScreeningCollection);
			AssertBillScreening("HouseBill.BillScreeningCollection", houseBillScreen, headerData.SubShipmentCollection[0].BillScreeningCollection);

			void AssertBillScreening(string message, AsycudaBillScreening asycudaBillScreening, List<BillScreening> billScreeningCollection)
			{
				AssertEquals("Should create a BillScreeningCollection from the BillScreen.", 1, billScreeningCollection.Count);

				var billScreening = billScreeningCollection[0];

				CombineAssertions(message, () =>
				{
					AssertEquals("Result", asycudaBillScreening.ASR_Result, billScreening.Result);
					AssertEquals("AuthorisedPerson", asycudaBillScreening.ASR_AuthorizedPersonName, billScreening.AuthorisedPerson);
					AssertEquals("PersonType", asycudaBillScreening.ASR_AuthorizedPersonType, billScreening.PersonType);
					AssertEquals("PersonIdentifier", asycudaBillScreening.ASR_AuthorizedPersonIdentifier, billScreening.PersonIdentifier);

					AssertEquals("FacilityPlaceSubDivision", asycudaBillScreening.FacilityPlace.SubDivision, billScreening.FacilityPlaceSubDivision);
					AssertEquals("FacilityPlaceNumber", asycudaBillScreening.FacilityPlace.Number, billScreening.FacilityPlaceNumber);
					AssertEquals("FacilityPlacePOBox", asycudaBillScreening.FacilityPlace.POBox, billScreening.FacilityPlacePOBox);
				});

				var supportingInformationCollection = billScreening.CustomsSupportingInformationCollection;
				AssertEquals("Should create a CustomsSupportingInformation from the AdditionalInfos of BillScreen.", 1, supportingInformationCollection.Count);

				CombineAssertions(message, () =>
				{
					var additionalInfo = asycudaBillScreening.AdditionalInfos[0];
					var supportingInformation = supportingInformationCollection[0];

					AssertEquals("SupportingInformation.Category", CusSupportingInfoTypeList.Codes.AdditionalInfo, supportingInformation.Category.Code);
					AssertEquals("SupportingInformation.Type", additionalInfo.CSI_Code, supportingInformation.Type.Code);
					AssertEquals("SupportingInformation.SubType", additionalInfo.CSI_SubType, supportingInformation.SubType.Code);
					AssertEquals("SupportingInformation.Description", additionalInfo.CSI_Description, supportingInformation.Description);
				});
			}
		}
	}
}
