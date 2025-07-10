using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Customs.JP.AFR.Business.Testing;
using Enterprise.Customs.JP.AFR.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	partial class JPAFRBillsDataObjectWriterTest : OrganizationAddressTestHelper
	{
		public void TestJPAFRHeaderMappings_NVOCC()
		{
			var header = SetupJPAFRHeader(Factory.New<JPAFRHeader>(), "MB324242");
			header.JPH_IsShippingLineEntry = false;
			header.JPH_VesselDetailsChanged = true;
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "HB24";
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "HB89";
			var bill3 = header.Bills.AddNew();
			bill3.JPB_BillNumber = "HB43";
			Factory.SaveForTesting();
			var writer = new JPAFRHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)));
			var headerData = writer.GetDataObject(header);
			AssertJPManifestContents(headerData, "MB324242", false);
			AssertEquals("headerData.AddInfoCollection.GetZStringValue(Constants.Header.VesselDetailsChanged)", "Y", headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.VesselDetailsChanged));
			AssertEquals("headerData.SubShipmentCollection.Count", 3, headerData.SubShipmentCollection.Count);
			AssertNotNull("HB24", headerData.SubShipmentCollection.FirstOrDefault(x => x.WayBillNumber.GetValueOrDefault() == "HB24"));
			AssertNotNull("HB89", headerData.SubShipmentCollection.FirstOrDefault(x => x.WayBillNumber.GetValueOrDefault() == "HB89"));
			AssertNotNull("HB43", headerData.SubShipmentCollection.FirstOrDefault(x => x.WayBillNumber.GetValueOrDefault() == "HB43"));
		}

		public void TestJPAFRHeaderMappings_VOCC()
		{
			var header = SetupJPAFRHeader(Factory.New<JPAFRHeader>(), "MB324242");
			header.JPH_IsShippingLineEntry = true;
			header.JPH_OperationalCarrierVoyageNo = "TESTNO.";
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "HB24";
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "HB89";
			var bill3 = header.Bills.AddNew();
			bill3.JPB_BillNumber = "HB43";
			Factory.SaveForTesting();
			var writer = new JPAFRHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)));
			var headerData = writer.GetDataObject(header);
			AssertJPManifestContents(headerData, "MB324242", true);
			AssertEquals("headerData.AddInfoCollection.GetZStringValue(Constants.Header.OperationalCarrierVoyageNo)", "TESTNO.", headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.OperationalCarrierVoyageNo));
			AssertEquals("headerData.SubShipmentCollection.Count", 3, headerData.SubShipmentCollection.Count);
			AssertNotNull("HB24", headerData.SubShipmentCollection.FirstOrDefault(x => x.WayBillNumber.GetValueOrDefault() == "HB24"));
			AssertNotNull("HB89", headerData.SubShipmentCollection.FirstOrDefault(x => x.WayBillNumber.GetValueOrDefault() == "HB89"));
			AssertNotNull("HB43", headerData.SubShipmentCollection.FirstOrDefault(x => x.WayBillNumber.GetValueOrDefault() == "HB43"));
		}

		public void TestJPAFRHeaderMappingsWithInvalidVessel()
		{
			var header = SetupJPAFRHeader(Factory.New<JPAFRHeader>(), "MB324242");
			header.JPH_RadioCallSign = "TEST012345";
			header.JPH_RN_NKCountryOfReg = Core.Constants.CountryCodes.Jamaica;

			AssertNull("Precondition", header.Vessel);

			Factory.SaveForTesting();

			var writer = new JPAFRHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)));
			var headerData = writer.GetDataObject(header);

			CombineAssertions(() =>
			{
				AssertEquals("headerData.VesselName", APLVessel.RV_Name, headerData.VesselName);
				AssertNotNull("headerData.VesselCountryOfRegistration", headerData.VesselCountryOfRegistration);
				AssertEquals("headerData.VesselCountryOfRegistration.Code", Core.Constants.CountryCodes.Jamaica, headerData.VesselCountryOfRegistration.Code);
				AssertEquals("headerData.VesselCountryOfRegistration.Name", "Jamaica", headerData.VesselCountryOfRegistration.Name);
				AssertNull("headerData.LloydsIMO", headerData.LloydsIMO);
				AssertEquals("headerData.VoyageFlightNo", "V324", headerData.VoyageFlightNo);
			});
		}

		#region Implementation

		const string CarrierCode1 = "SD23";
		const string CarrierCode2 = "KJ65";

		InBondDetailInitiatorTestHelper InBondDetailInitiatorTestHelper
		{
			get { return inBondDetailInitiatorTestHelper ?? (inBondDetailInitiatorTestHelper = new InBondDetailInitiatorTestHelper()); }
		}
		InBondDetailInitiatorTestHelper inBondDetailInitiatorTestHelper;

		protected override void TearDown()
		{
			if (inBondDetailInitiatorTestHelper != null)
			{
				inBondDetailInitiatorTestHelper.Dispose();
				inBondDetailInitiatorTestHelper = null;
			}
			base.TearDown();
		}

		JPAFRHeader SetupJPAFRHeader(JPAFRHeader header, ZString wayBillNumber)
		{
			var carrier = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			carrier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, CarrierCode2, Core.Constants.CountryCodes.Japan);
			header.JPH_GB_Branch = JPBranch.PK;
			header.JPH_MasterBillNumber = wayBillNumber;
			header.JPH_VesselName = APLVessel.RV_Code;
			header.JPH_Voyage = "V324";
			header.JPH_ETD = new ZDateTime(2013, 9, 4, 10, 50, 45, 350);
			header.JPH_RL_NKLoading = SeaForeignPort1.RL_Code;
			header.JPH_LoadingPortSuffix = "1";
			header.JPH_RelaxedAppId = ZBool.True;
			header.JPH_RL_NKDischarge = SeaLocalPort1.RL_Code;
			header.JPH_ETA = new ZDateTime(2013, 9, 10);
			header.Carrier.E2_AddressOverride = false;
			header.Carrier.E2_OA_Address = carrier.MainAddress.PK;
			header.JPH_CarrierCode = CarrierCode1;
			header.Notes.AddNew(ZBool.True, "DUMMY NOTE", "HELLO WORLD");
			header.Notes.AddNew(ZBool.False, "DUMMY NOTE 2", "GOODBYE WORLD");
			return header;
		}

		RefVessel APLVessel
		{
			get
			{
				if (aplVessel == null)
				{
					aplVessel = Factory.New<RefVessel>();
					aplVessel.RV_Code = "APL VESSEL";
					aplVessel.RV_LloydsNumber = "9832343";
					aplVessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Jamaica;
					aplVessel.RV_RadioCallSign = "CALLME";
				}
				return aplVessel;
			}
		}
		RefVessel aplVessel;

		GlbCompany JPCompany
		{
			get
			{
				if (jpCompany == null)
				{
					jpCompany = Factory.New<GlbCompany>();
					jpCompany.GC_Code = "JP@";
					jpCompany.GC_Name = "JP Company Test";
					jpCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
					jpCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
				}
				return jpCompany;
			}
		}
		GlbCompany jpCompany;

		GlbBranch JPBranch
		{
			get
			{
				if (jpBranch == null)
				{
					jpBranch = JPCompany.Branches.AddNew();
					jpBranch.GB_Code = "JP@";
					jpBranch.GB_BranchName = "JP Branch Test";
					jpBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				}
				return jpBranch;
			}
		}
		GlbBranch jpBranch;

		RefUNLOCO SeaLocalPort1
		{
			get
			{
				if (seaLocalPort1 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					query.AddToFilter(RefUNLOCOSchema.RL_HasSeaport, true);
					seaLocalPort1 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return seaLocalPort1;
			}
		}
		RefUNLOCO seaLocalPort1;

		RefUNLOCO SeaLocalPort2
		{
			get
			{
				if (seaLocalPort2 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, SeaLocalPort1.PK);
					query.AddToFilter(RefUNLOCOSchema.RL_HasSeaport, true);
					seaLocalPort2 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return seaLocalPort2;
			}
		}
		RefUNLOCO seaLocalPort2;

		RefUNLOCO SeaLocalPort3
		{
			get
			{
				if (seaLocalPort3 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, new[] { SeaLocalPort1.PK, SeaLocalPort2.PK });
					query.AddToFilter(RefUNLOCOSchema.RL_HasSeaport, true);
					seaLocalPort3 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return seaLocalPort3;
			}
		}
		RefUNLOCO seaLocalPort3;

		RefUNLOCO SeaForeignPort1
		{
			get
			{
				if (seaForeignPort1 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					query.AddToFilter(RefUNLOCOSchema.RL_HasSeaport, true);
					seaForeignPort1 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return seaForeignPort1;
			}
		}
		RefUNLOCO seaForeignPort1;

		RefUNLOCO SeaForeignPort2
		{
			get
			{
				if (seaForeignPort2 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, SeaForeignPort1.PK);
					query.AddToFilter(RefUNLOCOSchema.RL_HasSeaport, true);
					seaForeignPort2 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return seaForeignPort2;
			}
		}
		RefUNLOCO seaForeignPort2;

		void AssertContents(Date dateDataObject, DateType type, ZDateTime dateTime, ZBool isEstimate)
		{
			AssertNotNull("Precondition: dateDataObject", dateDataObject);
			CombineAssertions(delegate
			{
				AssertEquals("dateDataObject.Type", type, dateDataObject.Type);
				AssertEquals("dateDataObject.Value", dateTime, dateDataObject.Value);
				AssertEquals("dateDataObject.IsEstimate", isEstimate, dateDataObject.IsEstimate);
			});
		}

		void AssertContents(Note noteData, ZBool isCustomDescription, ZString description, ZString noteText)
		{
			AssertNotNull("Precondition: noteData", noteData);
			CombineAssertions(delegate
			{
				AssertEquals("noteData.IsCustomDescription", isCustomDescription, noteData.IsCustomDescription);
				AssertEquals("noteData.Description", description, noteData.Description);
				AssertEquals("noteData.NoteText", noteText, noteData.NoteText);
			});
		}

		void AssertJPManifestContents(Shipment headerData, ZString? wayBillNumber, bool isShippingLineEntry)
		{
			ZString? dischargePortSuffix = null;
			dischargePortSuffix = isShippingLineEntry ? ZString.Empty : dischargePortSuffix;
			AssertAFRHeaderContents(headerData, wayBillNumber, CarrierCode1, APLVessel.RV_Code, APLVessel.RV_LloydsNumber, CodeDescriptionPairForTesting.New(APLVessel.CountryOfReg.RN_Code, APLVessel.CountryOfReg.RN_Desc), "V324", CodeDescriptionPairForTesting.New(SeaForeignPort1.RL_Code, SeaForeignPort1.RL_PortName), "1", ZBool.True, CodeDescriptionPairForTesting.New(SeaLocalPort1.RL_Code, SeaLocalPort1.RL_PortName), dischargePortSuffix, CodeDescriptionPairForTesting.New(JPBranch.GB_Code, JPBranch.GB_BranchName), "CALLME");
			AssertNotNull("headerData.DateCollection", headerData.DateCollection);
			AssertEquals("headerData.DateCollection.Count", 2, headerData.DateCollection.Count);
			AssertContents(headerData.DateCollection[0], DateType.Departure, new ZDateTime(2013, 9, 4, 10, 50, 45, 350), ZBool.False);
			AssertContents(headerData.DateCollection[1], DateType.Arrival, new ZDateTime(2013, 9, 10), ZBool.False);
			AssertEquals("headerData.OrganizationAddressCollection.Count", 1, headerData.OrganizationAddressCollection.Count);
			AssertOrganizationBO_WUFSHIJNB("Carrier", headerData.OrganizationAddressCollection[0], nameof(DocAddressType.Carrier));
			AssertEquals("headerData.NoteCollection.Count", 2, headerData.NoteCollection.Count);
			AssertContents(headerData.NoteCollection[0], ZBool.True, "DUMMY NOTE", "HELLO WORLD");
			AssertContents(headerData.NoteCollection[1], ZBool.False, "DUMMY NOTE 2", "GOODBYE WORLD");
		}

		void AssertAFRHeaderContents(Shipment headerData, ZString? wayBillNumber, ZString? carrierCode, ZString? vessel, ZString? lloyds, ICodeDescription vesselCountryOfRegistration, ZString? voyage, ICodeDescription portOfLoading, ZString? loadingPortSuffix, ZBool? isDepartureFromRelaxedArea, ICodeDescription portOfDischarge, ZString? dischargePortSuffix, ICodeDescription branch, ZString callSign)
		{
			AssertNotNull("Precondition: headerData", headerData);

			CombineAssertions(delegate
			{
				AssertEquals("headerData.WayBillNumber", wayBillNumber, headerData.WayBillNumber);
				AssertNotNull("headerData.WayBillType", headerData.WayBillType);
				AssertEquals("headerData.WayBillType.Code", WayBillTypeList.Codes.Master, headerData.WayBillType.Code);
				AssertEquals("headerData.WayBillType.Description", WayBillTypeList.Descriptions.Master, headerData.WayBillType.Description);
				AssertNotNull("headerData.TransportMode", headerData.TransportMode);
				AssertEquals("headerData.TransportMode.Code", Core.Constants.TransportModes.Sea, headerData.TransportMode.Code);
				AssertEquals("headerData.TransportMode.Description", "Sea Freight", headerData.TransportMode.Description);
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(Constants.Header.CarrierCode)", carrierCode, headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.CarrierCode));
				AssertEquals("headerData.VesselName", vessel, headerData.VesselName);
				AssertNotNull("headerData.VesselCountryOfRegistration", headerData.VesselCountryOfRegistration);
				AssertEquals("headerData.VesselCountryOfRegistration.Code", vesselCountryOfRegistration.Code, headerData.VesselCountryOfRegistration.Code);
				AssertEquals("headerData.VesselCountryOfRegistration.Name", vesselCountryOfRegistration.Description, headerData.VesselCountryOfRegistration.Name);
				AssertEquals("headerData.LloydsIMO", lloyds, headerData.LloydsIMO);
				AssertEquals("headerData.VoyageFlightNo", voyage, headerData.VoyageFlightNo);
				AssertNotNull("headerData.PortOfLoading", headerData.PortOfLoading);
				AssertEquals("headerData.PortOfLoading.Code", portOfLoading.Code, headerData.PortOfLoading.Code);
				AssertEquals("headerData.PortOfLoading.Name", portOfLoading.Description, headerData.PortOfLoading.Name);
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(Constants.Header.PortOfLoadingSuffix)", loadingPortSuffix, headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.PortOfLoadingSuffix));
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(Constants.Header.PortOfDischargeSuffix)", dischargePortSuffix, headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.PortOfDischargeSuffix));
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(Constants.Header.VesselCallSign)", callSign, headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.VesselCallSign));
				AssertEquals("headerData.AddInfoCollection.GetZBoolValue(Constants.Header.IsDepartureFromRelaxedArea)", isDepartureFromRelaxedArea, headerData.AddInfoCollection.GetZBoolValue(AddInfoConstants.Header.IsDepartureFromRelaxedArea));
				AssertNotNull("headerData.PortOfDischarge", headerData.PortOfDischarge);
				AssertEquals("headerData.PortOfDischarge.Code", portOfDischarge.Code, headerData.PortOfDischarge.Code);
				AssertEquals("headerData.PortOfDischarge.Name", portOfDischarge.Description, headerData.PortOfDischarge.Name);
				AssertNotNull("headerData.Branch", headerData.Branch);
				AssertEquals("headerData.Branch.Code", branch.Code, headerData.Branch.Code);
				AssertEquals("headerData.Branch.Name", branch.Description, headerData.Branch.Name);
			});
		}

		#endregion
	}
}
