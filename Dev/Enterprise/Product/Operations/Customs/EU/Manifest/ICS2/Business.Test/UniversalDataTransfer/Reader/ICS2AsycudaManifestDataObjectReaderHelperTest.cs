using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class ICS2AsycudaManifestDataObjectReaderHelperTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestImportAsycudaManifestHeaderCusSupplyChainActorReferences()
		{
			var headerDataObject = CreateUniversalShipment().Shipment;

			headerDataObject.SetCustomsReferenceCollection(() => new List<CustomsReference>());

			var customsReferenceDataObject = new CustomsReference()
			{
				Type = new CodeDescriptionPair { Code = "SCA" },
				SubType = new CodeDescriptionPair35Char { Code = "CS" },
				Reference = "122333444",
			};

			headerDataObject.CustomsReferenceCollection.Add(customsReferenceDataObject);

			var headerBO = ProcessAndFindHeader(headerDataObject);
			var customsReference = headerBO.CusSupplyChainActorReferences.Cast<CusSupplyChainActorReference>().Single();

			CombineAssertions(() =>
			{
				AssertEquals("SCA", customsReference.CFR_Type);
				AssertEquals("CS", customsReference.CFR_Code);
				AssertEquals("122333444", customsReference.CFR_Reference);
			});
		}

		public void TestImportAsycudaManifestHeaderAdditionalInfos()
		{
			var headerDataObject = CreateUniversalShipment().Shipment;

			headerDataObject.SetCustomsSupportingInformationCollection(() => new List<CustomsSupportingInformation>());

			var customsSupportInfo1 = new CustomsSupportingInformation()
			{
				Category = new CodeDescriptionPair { Code = "OTH" },
				Type = new CodeDescriptionPair6Char { Code = "10600" },
			};

			var customsSupportInfo2 = new CustomsSupportingInformation()
			{
				Category = new CodeDescriptionPair { Code = "OTH" },
				Type = new CodeDescriptionPair6Char { Code = "10900" },
			};

			headerDataObject.CustomsSupportingInformationCollection.Add(customsSupportInfo1);
			headerDataObject.CustomsSupportingInformationCollection.Add(customsSupportInfo2);

			var headerBO = ProcessAndFindHeader(headerDataObject);

			CombineAssertions(() =>
			{
				AssertEquals(2, headerBO.AdditionalInfos.Count);
				AssertEquals(1, headerBO.AdditionalInfos.Where(x => x.CSI_Code == "10600").Count());
				AssertEquals(1, headerBO.AdditionalInfos.Where(x => x.CSI_Code == "10900").Count());
			});
		}

		public void TestImportAsycudaBillAdditionalInfos()
		{
			(var headerDataObject, var billDataObject) = CreateUniversalShipment();

			billDataObject.SetCustomsSupportingInformationCollection(() => new List<CustomsSupportingInformation>());

			var customsSupportInfo1 = new CustomsSupportingInformation()
			{
				Category = new CodeDescriptionPair { Code = "OTH" },
				Type = new CodeDescriptionPair6Char { Code = "10600" },
			};

			var customsSupportInfo2 = new CustomsSupportingInformation()
			{
				Category = new CodeDescriptionPair { Code = "OTH" },
				Type = new CodeDescriptionPair6Char { Code = "10900" },
			};

			billDataObject.CustomsSupportingInformationCollection.Add(customsSupportInfo1);
			billDataObject.CustomsSupportingInformationCollection.Add(customsSupportInfo2);
			headerDataObject.SubShipmentCollection.Add(billDataObject);

			var headerBO = ProcessAndFindHeader(headerDataObject);
			var billBO = headerBO.Bills.AsEnumerable().Single();

			CombineAssertions(() =>
			{
				AssertEquals(2, billBO.AdditionalInfos.Count);
				AssertEquals(1, billBO.AdditionalInfos.Where(x => x.CSI_Code == "10600").Count());
				AssertEquals(1, billBO.AdditionalInfos.Where(x => x.CSI_Code == "10900").Count());
			});
		}

		public void TestImportAsycudaManifestHeaderFields()
		{
			var headerDataObject = CreateUniversalShipment().Shipment;

			headerDataObject.SetAddInfoCollection(() => new List<AddInfo>());
			headerDataObject.AddInfoCollection.Add(new AddInfo { Key = AsycudaManifestHeader.Schema.SpecificCircumstanceIndicator, Value = "F24" });
			headerDataObject.AddInfoCollection.Add(new AddInfo { Key = AsycudaManifestHeader.Schema.ReEntryIndicator, Value = "Y" });
			headerDataObject.AddInfoCollection.Add(new AddInfo { Key = AsycudaManifestHeader.Schema.SplitConsignmentIndicator, Value = "Y" });
			headerDataObject.AddInfoCollection.Add(new AddInfo { Key = AsycudaBill.Schema.TransportDocumentType, Value = "N741" });

			headerDataObject.SetDateCollection(() => new List<Date>());
			headerDataObject.DateCollection.Add(Date.New(DateType.ActualArrival, ZBool.False, ZDateTime.BrettsBirthday));

			var headerBO = ProcessAndFindHeader(headerDataObject);

			CombineAssertions(() =>
			{
				AssertEquals("headerBO.AMA_A_ARV", ZDateTime.BrettsBirthday, headerBO.AMA_A_ARV);
				AssertEquals("headerBO.SpecificCircumstanceIndicator", "F24", headerBO.SpecificCircumstanceIndicator);
				Assert("headerBO.ReEntryIndicator", headerBO.ReEntryIndicator);
				Assert("headerBO.SplitConsignmentIndicator", headerBO.SplitConsignmentIndicator);
				AssertEquals("headerBO.MasterBill.TransportDocumentType", "N741", headerBO.MasterBill.TransportDocumentType);
			});
		}

		public void TestImportBillScreenings()
		{
			var header1 = Factory.NewWithValidTestData<OrgHeader>();
			var header2 = Factory.NewWithValidTestData<OrgHeader>();

			(var headerDataObject, var billDataObject) = CreateUniversalShipment();

			BillScreening masterBillScreening = null;
			BillScreening houseBillScreening = null;

			headerDataObject.SetBillScreeningCollection(() =>
			{
				var collection = new List<BillScreening>();

				var dataObject = new BillScreening(DefaultDataObjectWriterStrategy.TestInstance);
				dataObject.Result = "1";
				dataObject.AuthorisedPerson = "Rick";
				dataObject.PersonType = "2";
				dataObject.PersonIdentifier = "C137";

				dataObject.FacilityPlaceSubDivision = "TST";
				dataObject.FacilityPlaceNumber = "117777777";
				dataObject.FacilityPlacePOBox = "20230101";
				dataObject.FacilityPlace = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
				dataObject.FacilityPlace.AddressType = DocAddressTypes.Codes.ICS2FacilityPlace;
				dataObject.FacilityPlace.OrganizationCode = new ZCodeMappedZString(header1.OH_Code);
				dataObject.FacilityPlace.Address1 = header1.MainAddress.Address1;

				dataObject.SetCustomsSupportingInformationCollection(() =>
				{
					return new List<CustomsSupportingInformation>
					{
						new CustomsSupportingInformation
						{
							Category = new CodeDescriptionPair() { Code = CusSupportingInfoTypeList.Codes.AdditionalInfo },
							Type = new CodeDescriptionPair6Char() { Code = "C1" },
							SubType = new CodeDescriptionPair5Char() { Code = "A00" },
							Description = "TEST Master Bill AdditionalInfo"
						}
					};
				});

				masterBillScreening = dataObject;

				collection.Add(dataObject);
				return collection;
			});

			billDataObject.SetBillScreeningCollection(() =>
			{
				var collection = new List<BillScreening>();

				var dataObject = new BillScreening(DefaultDataObjectWriterStrategy.TestInstance);
				dataObject.Result = "2";
				dataObject.AuthorisedPerson = "Morty";
				dataObject.PersonType = "2";
				dataObject.PersonIdentifier = "H137";

				dataObject.FacilityPlaceSubDivision = "AXA";
				dataObject.FacilityPlaceNumber = "228888888";
				dataObject.FacilityPlacePOBox = "20230707";
				dataObject.FacilityPlace = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
				dataObject.FacilityPlace.AddressType = DocAddressTypes.Codes.ICS2FacilityPlace;
				dataObject.FacilityPlace.OrganizationCode = new ZCodeMappedZString(header2.OH_Code);
				dataObject.FacilityPlace.Address1 = header1.MainAddress.Address1;

				dataObject.SetCustomsSupportingInformationCollection(() =>
				{
					return new List<CustomsSupportingInformation>
					{
						new CustomsSupportingInformation
						{
							Category = new CodeDescriptionPair() { Code = CusSupportingInfoTypeList.Codes.AdditionalInfo },
							Type = new CodeDescriptionPair6Char() { Code = "C2" },
							SubType = new CodeDescriptionPair5Char() { Code = "A20" },
							Description = "TEST House Bill AdditionalInfo"
						}
					};
				});

				houseBillScreening = dataObject;

				collection.Add(dataObject);
				return collection;
			});

			var headerBO = ProcessAndFindHeader(headerDataObject);
			var billBO = headerBO.Bills.AsEnumerable().Single();

			AssertEquals(1, headerBO.BillScreenings.Count);
			AssertEquals(1, billBO.BillScreenings.Count);

			AssertBillScreening(headerBO.BillScreenings[0], masterBillScreening);
			AssertBillScreening(billBO.BillScreenings[0], houseBillScreening);

			void AssertBillScreening(AsycudaBillScreening billScreening, BillScreening dataObject)
			{
				AssertEquals("ASR_Result", dataObject.Result, billScreening.ASR_Result);
				AssertEquals("ASR_AuthorizedPersonName", dataObject.AuthorisedPerson, billScreening.ASR_AuthorizedPersonName);
				AssertEquals("ASR_AuthorizedPersonType", dataObject.PersonType, billScreening.ASR_AuthorizedPersonType);
				AssertEquals("ASR_AuthorizedPersonIdentifier", dataObject.PersonIdentifier, billScreening.ASR_AuthorizedPersonIdentifier);
				AssertEquals("FacilityPlace.Organisation", dataObject.FacilityPlace.OrganizationCode, billScreening.FacilityPlace.Organisation.OH_Code);
				AssertEquals("FacilityPlace.SubDivision", dataObject.FacilityPlaceSubDivision, billScreening.FacilityPlace.SubDivision);
				AssertEquals("FacilityPlace.Number", dataObject.FacilityPlaceNumber, billScreening.FacilityPlace.Number);
				AssertEquals("FacilityPlace.POBox", dataObject.FacilityPlacePOBox, billScreening.FacilityPlace.POBox);

				var customsSupportingInformation = dataObject.CustomsSupportingInformationCollection[0];
				var additionalInfo = billScreening.AdditionalInfos[0];

				AssertEquals("AdditionalInfo.CSI_Code", customsSupportingInformation.Type.Code, additionalInfo.CSI_Code);
				AssertEquals("AdditionalInfo.CSI_SubType", customsSupportingInformation.SubType.Code, additionalInfo.CSI_SubType);
				AssertEquals("AdditionalInfo.CSI_Description", customsSupportingInformation.Description, additionalInfo.CSI_Description);
			}
		}

		(Shipment Shipment, Shipment SubShipment) CreateUniversalShipment()
		{
			ZZDataTestHelper.SetupZZ(new BusinessObjectFactory(), Core.Constants.CountryCodes.EuropeanUnion);
			var helper = new AsycudaManifestDataObjectReaderTestHelper();

			var lodingPort = new UNLOCO { Code = helper.GetAirLocalPort1("AU").RL_Code };
			var dischargePort = new UNLOCO { Code = helper.GetAirLocalPort1("FR").RL_Code };

			var shipment = helper.SetupManifestHeader("EUICS0001", lodingPort, dischargePort, new ZDateTime(2018, 2, 10), new ZDateTime(2018, 2, 1), "", EUICS2ManifestTypes.Codes.ENS);
			var entryHeader = helper.SetupCountryHeaderEntryHeader(Core.Constants.CountryCodes.EuropeanUnion, 1);

			var headerEntryInstruction = helper.SetupCountryHeaderEntryInstruction(1, lodingPort, "OTT1", Core.Constants.CountryCodes.EuropeanUnion, EUICS2ManifestTypes.Codes.ENS, "IMP");
			shipment.SetEntryHeaderCollection(() => new List<EntryHeader>());
			shipment.EntryHeaderCollection.Add(entryHeader);
			shipment.SetEntryInstructionCollection(() => new List<EntryInstruction>());
			shipment.EntryInstructionCollection.Add(headerEntryInstruction);

			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>());

			var subShipment = helper.SetupBill("BIL00001", lodingPort, dischargePort, 300m, "Goods Desc", 3m, "Carrier Reference", "STD", "PRE");
			shipment.SubShipmentCollection.Add(subShipment);

			var billCountryEntryHeader = helper.SetupCountryBillEntryHeader("FR", 1, "CLR", "BIL00001");
			subShipment.SetEntryHeaderCollection(() => new List<EntryHeader> { billCountryEntryHeader });

			return (shipment, subShipment);
		}

		AsycudaManifestHeader ProcessAndFindHeader(Shipment shipment)
		{
			var message = GetQueuedUniversalShipmentMessage(shipment);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			Factory.SaveForTesting();

			var query = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
			subQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);
			subQuery.AddToFilter(AsycudaBillSchema.ABL_BillNumber, "EUICS0001");
			query.AddSubQuery(subQuery, JoinCondition.And);

			return Factory.LoadTop1<AsycudaManifestHeader>(query);
		}
	}
}
