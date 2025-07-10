using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusOutturnHeaderDataObjectReaderTest : DataTransfer.Universal.Outturn.Testing.OutturnDataObjectReaderTestHelper<CusOutturnHeader, DepotCusOutturn>
	{
		public void TestGetReasonForNotAbleToUpdateFromDataSourceOrTargetBO()
		{
			var expectedReason = "Vessel Lloyds and Voyage Number and Premise ID must not be empty.";
			var reader = new CusOutturnHeaderDataObjectReader(shipment, logger, Factory);
			var outturnHeader = reader.ReadIntoBusinessObject();

			AssertEquals("Get exist OutturnHeader", existOutturnHeader.PK, outturnHeader.PK);
			Assert("No Log error", !logger.HasErrors);

			logger.ClearLogs();
			shipment.DataContext.DataTargetCollection.Single().Key = existOutturnHeader.C6_SendersMessageReference;
			shipment.VoyageFlightNo = "";

			reader = new CusOutturnHeaderDataObjectReader(shipment, logger, Factory);
			reader.ReadIntoBusinessObject();

			AssertEquals("OutturnHeader cannot be updated", "Q123", existOutturnHeader.C6_VoyageNum);
			Assert("Has Log error", logger.HasErrors);
			Assert("Log: Stop import when LloydsIMO is empty", logger.GetErrors().Contains(expectedReason));

			logger.ClearLogs();
			shipment.DataContext.DataTargetCollection.Single().Key = "";

			reader = new CusOutturnHeaderDataObjectReader(shipment, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();

			AssertNull("Stop import and can't new outturnHeader when LloydsIMO is empty", outturnHeader);
			Assert("Has Log error", logger.HasErrors);
			Assert("Log: Stop import when LloydsIMO is empty", logger.GetErrors().Contains(expectedReason));
		}

		public void TestGetOutturnHeaderPropertiesToSuspendSetting()
		{
			var getOutturnHeaderPropertiesToSuspendSetting = typeof(CusOutturnHeaderDataObjectReader).GetMethod("GetOutturnHeaderPropertiesToSuspendSetting", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
			var reader = new CusOutturnHeaderDataObjectReader(shipment, logger, Factory);
			var suspenderList = (IEnumerable<ZString>)getOutturnHeaderPropertiesToSuspendSetting.Invoke(reader, System.Array.Empty<object>());
			AssertEquals("suspend C6_LloydsIMO", true, suspenderList.Contains(CusOutturnHeader.Schema.C6_LloydsIMO));
			AssertEquals("suspender C6_VesselName", true, suspenderList.Contains(CusOutturnHeader.Schema.C6_VesselName));
			AssertEquals("suspender C6_OutturningPremiseID", true, suspenderList.Contains(CusOutturnHeader.Schema.C6_OutturningPremiseID));
			AssertEquals("suspender C6_OA_OutturningPremise", true, suspenderList.Contains(CusOutturnHeader.Schema.C6_OA_OutturningPremise));

			shipment.VoyageFlightNo = "901S";
			shipment.VesselName = "";
			shipment.LloydsIMO = "";
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>()
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = AddressTypes.ArrivalCFSAddress,
					CompanyName = "ZZZ",
				}
			});
			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>
			{
				new AdditionalReference
				{
					Type = new UniversalDataBuss.DataObjects.Universal.EntryType
					{
						Code = DataTransfer.Universal.Constants.AdditionalReference.EntryType.Codes.ControlledPremiseID,
						Description = ""
					},
					ContextInformation = "AU",
					ReferenceNumber = ""
				}
			});
			reader = new CusOutturnHeaderDataObjectReader(shipment, logger, Factory);
			suspenderList = (IEnumerable<ZString>)getOutturnHeaderPropertiesToSuspendSetting.Invoke(reader, System.Array.Empty<object>());
			AssertEquals("suspend C6_LloydsIMO", true, suspenderList.Contains(CusOutturnHeader.Schema.C6_LloydsIMO));
			AssertEquals("suspender C6_VesselName", true, suspenderList.Contains(CusOutturnHeader.Schema.C6_VesselName));
			AssertEquals("suspender C6_OutturningPremiseID", true, suspenderList.Contains(CusOutturnHeader.Schema.C6_OutturningPremiseID));
			AssertEquals("suspender C6_OA_OutturningPremise", true, suspenderList.Contains(CusOutturnHeader.Schema.C6_OA_OutturningPremise));

			Shipment testShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				VoyageFlightNo = "901S"
			};
			testShipment.SetOrganizationAddressCollection(() => null);
			testShipment.SetAdditionalReferenceCollection(() => null);
			reader = new CusOutturnHeaderDataObjectReader(testShipment, logger, Factory);
			suspenderList = (IEnumerable<ZString>)getOutturnHeaderPropertiesToSuspendSetting.Invoke(reader, System.Array.Empty<object>());
			AssertEquals("suspend C6_LloydsIMO", false, suspenderList.Contains(CusOutturnHeader.Schema.C6_LloydsIMO));
			AssertEquals("suspender C6_VesselName", false, suspenderList.Contains(CusOutturnHeader.Schema.C6_VesselName));
			AssertEquals("suspender C6_OutturningPremiseID", false, suspenderList.Contains(CusOutturnHeader.Schema.C6_OutturningPremiseID));
			AssertEquals("suspender C6_OA_OutturningPremise", false, suspenderList.Contains(CusOutturnHeader.Schema.C6_OA_OutturningPremise));
		}

		public void TestImportingLloydsIMOForNewOutturnHeader()
		{
			var testShipment = CreateMinimalOutturnHeaderShipment();
			var reader = new CusOutturnHeaderDataObjectReader(testShipment, logger, Factory);
			var outturnHeader = reader.ReadIntoBusinessObject();

			AssertNull("Stop import hen XML has no LloydsIMO can no vessel name", outturnHeader);

			testShipment.VesselName = "VESSEL";
			reader = new CusOutturnHeaderDataObjectReader(testShipment, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();

			AssertNotEquals("Add new outturn header", existOutturnHeader.PK, outturnHeader.PK);
			AssertEquals("C6_VesselName: Set xml value when XML has VesselName", "VESSEL", outturnHeader.C6_VesselName);
			AssertEquals("C6_LloydsIMO: Set calculated value based on vessel name when XML has NO LloydsIMO", "9832343", outturnHeader.C6_LloydsIMO);

			testShipment.VesselName = "VESSEL";
			testShipment.LloydsIMO = "";
			reader = new CusOutturnHeaderDataObjectReader(testShipment, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();

			AssertNull("Stop import when XML has empty LloydsIMO", outturnHeader);

			testShipment.VesselName = "";
			testShipment.LloydsIMO = "9832343";
			reader = new CusOutturnHeaderDataObjectReader(testShipment, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();

			AssertNotEquals("Add new outturn header", existOutturnHeader.PK, outturnHeader.PK);
			AssertEquals("C6_VesselName: Set empty when XML has empty VesselName", "", outturnHeader.C6_VesselName);
			AssertEquals("C6_LloydsIMO: Set xml value when has LloydsIMO in xml", "9832343", outturnHeader.C6_LloydsIMO);

			testShipment.VesselName = "ZZZ";
			testShipment.LloydsIMO = "ZZZ";
			reader = new CusOutturnHeaderDataObjectReader(testShipment, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();

			AssertNotEquals("Add new outturn header", existOutturnHeader.PK, outturnHeader.PK);
			AssertEquals("C6_VesselName: Set xml value when XML has VesselName", "ZZZ", outturnHeader.C6_VesselName);
			AssertEquals("C6_LloydsIMO: Set xml value when XML has LloydsIMO", "ZZZ", outturnHeader.C6_LloydsIMO);

			testShipment.VesselName = "VESSEL";
			testShipment.LloydsIMO = "ZZZ";
			reader = new CusOutturnHeaderDataObjectReader(testShipment, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();

			AssertNotEquals("Add new outturn header", existOutturnHeader.PK, outturnHeader.PK);
			AssertEquals("C6_VesselName: Set xml value when XML has VesselName", "VESSEL", outturnHeader.C6_VesselName);
			AssertEquals("C6_LloydsIMO: Set xml value when XML has LloydsIMO", "ZZZ", outturnHeader.C6_LloydsIMO);
		}

		public void TestImportingLloydsIMOForForExistOutturnHeader()
		{
			var testShipment = CreateMinimalOutturnHeaderShipment(existOutturnHeader.C6_SendersMessageReference);
			var reader = new CusOutturnHeaderDataObjectReader(testShipment, logger, Factory);
			var outturnHeader = reader.ReadIntoBusinessObject();

			AssertEquals("Get exist outturn header", existOutturnHeader.PK, outturnHeader.PK);
			AssertNotEquals("Stop import and keep current voyage number when XML has no LloydsIMO can no vessel name", "904S", outturnHeader.C6_VoyageNum);
			AssertEquals("Stop import and keep current LloydesIMO when XML has no LloydsIMO can no vessel name", "9832343", outturnHeader.C6_LloydsIMO);

			testShipment.VesselName = "VESSEL";
			reader = new CusOutturnHeaderDataObjectReader(testShipment, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();

			AssertEquals("Get exist outturn header", existOutturnHeader.PK, outturnHeader.PK);
			AssertEquals("C6_LloydsIMO: Set calculated value when XML has NO LloydsIMO and can generated by vessel name", "9832343", outturnHeader.C6_LloydsIMO);

			testShipment.VesselName = "ZZZ";
			reader = new CusOutturnHeaderDataObjectReader(testShipment, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();

			AssertEquals("Get exist outturn header", existOutturnHeader.PK, outturnHeader.PK);
			AssertNotEquals("Stop import when XML has no LloydsIMO and and cannot generated by vessel name", "ZZZ", outturnHeader.C6_VesselName);

			testShipment.VesselName = "ZZZ";
			testShipment.LloydsIMO = "";
			reader = new CusOutturnHeaderDataObjectReader(testShipment, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();

			AssertNotEquals("Stop import when XML has empty LloydsIMO", "ZZZ", outturnHeader.C6_VesselName);

			testShipment.VesselName = "";
			testShipment.LloydsIMO = "ZZZ";
			reader = new CusOutturnHeaderDataObjectReader(testShipment, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();

			AssertEquals("Get exist outturn header", existOutturnHeader.PK, outturnHeader.PK);
			AssertEquals("C6_VesselName: Set empty when XML has empty VesselName", "", outturnHeader.C6_VesselName);
			AssertEquals("C6_LloydsIMO: Set xml value when XML has LloydsIMO", "ZZZ", outturnHeader.C6_LloydsIMO);

			testShipment.VesselName = "ZZZ";
			testShipment.LloydsIMO = "ZZZ";
			reader = new CusOutturnHeaderDataObjectReader(testShipment, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();

			AssertEquals("Get exist outturn header", existOutturnHeader.PK, outturnHeader.PK);
			AssertEquals("C6_VesselName: Set xml value when XML has VesselName", "ZZZ", outturnHeader.C6_VesselName);
			AssertEquals("C6_LloydsIMO: Set xml value when XML has LloydsIMO", "ZZZ", outturnHeader.C6_LloydsIMO);

			testShipment.VesselName = "VESSEL";
			testShipment.LloydsIMO = "ZZZ";
			reader = new CusOutturnHeaderDataObjectReader(testShipment, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();

			AssertEquals("Get exist outturn header", existOutturnHeader.PK, outturnHeader.PK);
			AssertEquals("C6_VesselName: Set xml value when XML has VesselName", "VESSEL", outturnHeader.C6_VesselName);
			AssertEquals("C6_LloydsIMO: Set xml value when XML has LloydsIMO", "ZZZ", outturnHeader.C6_LloydsIMO);
		}

		public void TestImportingLloydsOnlyIfSingleMatch()
		{
			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Code = "VSL";
			vessel1.RV_LloydsNumber = "IMO1";

			var testShipment = CreateMinimalOutturnHeaderShipment(existOutturnHeader.C6_SendersMessageReference);
			testShipment.VesselName = "VSL";
			var reader = new CusOutturnHeaderDataObjectReader(testShipment, logger, Factory);
			var outturnHeader = reader.ReadIntoBusinessObject();
			AssertEquals(existOutturnHeader.PK, outturnHeader.PK);
			AssertEquals("If there is only one Lloyds matched with the vessel name in database, we populate it.", "IMO1", outturnHeader.C6_LloydsIMO);

			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Code = "VSL";
			vessel2.RV_LloydsNumber = "IMO0";

			var vessel3 = Factory.New<RefVessel>();
			vessel3.RV_Code = "VSL";
			vessel3.RV_LloydsNumber = "IMO2";

			outturnHeader.C6_LloydsIMOInfo.ClearValue();
			reader = new CusOutturnHeaderDataObjectReader(testShipment, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();
			AssertEquals(existOutturnHeader.PK, outturnHeader.PK);
			AssertEquals("If there are multiple Lloyds matched with the vessel name in database, we populate it by the first one.", "IMO0", outturnHeader.C6_LloydsIMO);

			outturnHeader.C6_LloydsIMOInfo.ClearValue();
			testShipment.LloydsIMO = "NEW IMO";
			reader = new CusOutturnHeaderDataObjectReader(testShipment, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();
			AssertEquals(existOutturnHeader.PK, outturnHeader.PK);
			AssertEquals("If there is Lloyds defined in the XML, use it.", "NEW IMO", outturnHeader.C6_LloydsIMO);
		}

		public void TestImportingPremiseIDForNewOutturnHeader()
		{
			shipment.VoyageFlightNo = "TEST0";
			Factory.SaveForTesting();

			var reader = new CusOutturnHeaderDataObjectReader(shipment, logger, Factory);
			var outturnHeader = reader.ReadIntoBusinessObject();

			AssertNotEquals("Add new outturn header", existOutturnHeader.PK, outturnHeader.PK);
			AssertEquals("C6_OA_OutturningPremise: Set xml value when xml has valid premise address", premise.MainAddress.PK, outturnHeader.C6_OA_OutturningPremise);
			AssertEquals("C6_OutturningPremiseID: Set xml value when xml has premise ID", "CP123", outturnHeader.C6_OutturningPremiseID);

			shipment.VoyageFlightNo = "TEST1";
			shipment.SetAdditionalReferenceCollection(() => null);
			Factory.SaveForTesting();

			reader = new CusOutturnHeaderDataObjectReader(shipment, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();

			AssertNotEquals("Add new outturn header", existOutturnHeader.PK, outturnHeader.PK);
			AssertEquals("C6_OA_OutturningPremise: Set xml value when xml has valid premise address", premise.MainAddress.PK, outturnHeader.C6_OA_OutturningPremise);
			AssertEquals("C6_OutturningPremiseID: Set calculated value when xml has NO premise ID and can generated by premise address", "CP123", outturnHeader.C6_OutturningPremiseID);

			shipment.VoyageFlightNo = "TEST2";
			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>
				{
					new AdditionalReference
					{
						Type = new UniversalDataBuss.DataObjects.Universal.EntryType
						{
							Code = DataTransfer.Universal.Constants.AdditionalReference.EntryType.Codes.ControlledPremiseID,
							Description = DataTransfer.Universal.Constants.AdditionalReference.EntryType.Descriptions.ControlledPremiseID
						},
						ContextInformation = "AU",
						ReferenceNumber = ""
					},
				});
			Factory.SaveForTesting();

			reader = new CusOutturnHeaderDataObjectReader(shipment, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();

			AssertNull("Stop import when XML has empty premise ID", outturnHeader);

			shipment.VoyageFlightNo = "TEST3";
			shipment.SetOrganizationAddressCollection(() => null);
			Factory.SaveForTesting();
			reader = new CusOutturnHeaderDataObjectReader(shipment, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();
			AssertNull("Stop import and cannot new outturn header when xml has NO premise ID and cannot generated by premise address", outturnHeader);
		}

		public void TestImportingPremiseIDForExistOutturnHeader()
		{
			shipment.DataContext.DataTargetCollection.Single().Key = existOutturnHeader.C6_SendersMessageReference;
			var reader = new CusOutturnHeaderDataObjectReader(shipment, logger, Factory);
			var outturnHeader = reader.ReadIntoBusinessObject();

			AssertEquals("Update exist outturn header", existOutturnHeader.PK, outturnHeader.PK);
			AssertEquals("C6_OA_OutturningPremise: Set xml value when xml has valid premise address", premise.MainAddress.PK, outturnHeader.C6_OA_OutturningPremise);
			AssertEquals("C6_OutturningPremiseID: Set xml value when xml has premise ID", "CP123", outturnHeader.C6_OutturningPremiseID);

			shipment.VoyageFlightNo = "TEST2";
			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>
				{
					new AdditionalReference
					{
						Type = new UniversalDataBuss.DataObjects.Universal.EntryType
						{
							Code = DataTransfer.Universal.Constants.AdditionalReference.EntryType.Codes.ControlledPremiseID,
							Description = DataTransfer.Universal.Constants.AdditionalReference.EntryType.Descriptions.ControlledPremiseID
						},
						ContextInformation = "AU",
						ReferenceNumber = ""
					},
				});

			reader = new CusOutturnHeaderDataObjectReader(shipment, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();

			AssertEquals("Update exist outturn header", existOutturnHeader.PK, outturnHeader.PK);
			AssertNotEquals("Stop import when XML has empty premise ID", "TEST2", outturnHeader.C6_VoyageNum);

			shipment.VoyageFlightNo = "TEST2";
			shipment.SetAdditionalReferenceCollection(() => null);

			reader = new CusOutturnHeaderDataObjectReader(shipment, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();
			AssertEquals("Update exist outturn header", existOutturnHeader.PK, outturnHeader.PK);
			AssertEquals("C6_OA_OutturningPremise: Set xml value when xml has valid premise address", premise.MainAddress.PK, outturnHeader.C6_OA_OutturningPremise);
			AssertEquals("C6_OutturningPremiseID: Set calculated value when xml has NO premise ID and can generated by premise address", premise.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID), outturnHeader.C6_OutturningPremiseID);

			shipment.VoyageFlightNo = "TEST3";
			shipment.SetOrganizationAddressCollection(() => null);

			reader = new CusOutturnHeaderDataObjectReader(shipment, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();

			AssertEquals("Update exist outturn header", existOutturnHeader.PK, outturnHeader.PK);
			AssertEquals("Stop import and cannot update outturn header when xml has NO premise ID and cannot generated by premise address", "CP123", outturnHeader.C6_OutturningPremiseID);
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			refVessel = CreateRefVesselForTest();
			premise = CreatePremiseAddressForTest();
			Factory.SaveForTesting();

			existOutturnHeader = CreateOuttrunHeader(refVessel, premise, "Q123", "O00000123");
			shipment = CreateTestOutturnHeaderShipment(refVessel, premise, "Q123");
			Factory.SaveForTesting();
		}

		OrgHeader premise;
		RefVessel refVessel;
		CusOutturnHeader existOutturnHeader;
		Shipment shipment;

		Shipment CreateMinimalOutturnHeaderShipment(ZString? reference = null)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.SeaCargoOutturn, reference);
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				TransportMode = new CodeDescriptionPair
				{
					Code = Core.Constants.TransportModes.Sea,
					Description = Core.Constants.TransportModeDescriptions.Sea
				},
				VoyageFlightNo = "904S",
			};
			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>
			{
				new AdditionalReference
				{
				Type = new UniversalDataBuss.DataObjects.Universal.EntryType
				{
					Code = DataTransfer.Universal.Constants.AdditionalReference.EntryType.Codes.ControlledPremiseID,
					Description = DataTransfer.Universal.Constants.AdditionalReference.EntryType.Descriptions.ControlledPremiseID
				},
				ContextInformation = "AU",
				ReferenceNumber = "ZZZ"
				}
			});

			return shipment;
		}
	}
}
