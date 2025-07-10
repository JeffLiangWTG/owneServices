using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.Client.UPE.Business.BISI.Testing;
using Enterprise.Client.UPE.Business.DataImport.Level1FileFormat;
using Enterprise.Client.UPE.Business.Testing;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.MasterData.Business.Tests;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business
{
	[TestedType(typeof(UPECusHAWB))]
	sealed class UPECusHAWB01Test : EnterpriseBusinessObjectTestCase
	{
		#region New Properties
		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.UPECusHAWB);
			}
		}

		public void TestLevel1RecordStoredInNote()
		{
			Level1Record level1Record = new Level1Record();
			level1Record.AddRecordLines(new ZString[] { "US2795AU9639040422              D3824ARFY9JH2000003824ARFY9JH           N 1 19   LBS US          USDNNNN           USDAKE32156QF    09642             USD         USD5100      USDN0 N   NEDI  19APR200419 LBS         USDD1    NNN NN  NN  N USD           USD    T1                      19APR20040000           5100       P/PNRE       19   LBSNNC0000051901QF12            N N 1   N ", "US2795AU9639040422              D3824ARFY9JH2020001Z3824AR6640806327                 19     19     2    N                                                            7709705048E                AU09639  S1AU9639TF.113B2004-04-21                              Y                                                                             AUDNNNNNBI                                  ", "US2795AU9639040422              D3824ARFY9JH300000073706003824AR    BIOTECH CORP                       107 OAKWOOD DRIVE                                                     GLASTONBURY                                            CT060332481US 18606338111                                                                               11644                                         ", "US2795AU9639040422              D3824ARFY9JH400000170520138AU0495926SNOWSILL, SONYA                                             15 LEANDER ST                                                         FALCON                                                   6210     AU 0895343637                  SNOWSILL    SNOWSILL                                                 237           ", "US2795AU9639040422              DA4W82133K3G401000        8AU0596227WENDY PATERSON                     ContactName              GPO BOX 1609                       ABC                                SYDNEY                                                 NS2001     AU 123456789                                                                                        237           ", "US2795AU9639040422              D3824ARFY9JH5000006   EA SHEN MIN EXTRA STRENGTH                                                                                 600       USD11644               US                                   AU                                                                                                                                                 ", "US2795AU9639040422              D3947808NNSR60000099999999999    11 LBS         USD         USD          USD                             C0000092779UPS6901         NY1ZAT27736792092649                 10     AAY89758UPS                                                                                                                                                               ", });
			UPECusHAWB.Level1Record = level1Record;
			AssertNotNull(UPECusHAWB.Level1Record._200000);
			AssertNotNull(UPECusHAWB.Level1Record._202000);
			AssertNotNull(UPECusHAWB.Level1Record._300000);
			AssertNotNull(UPECusHAWB.Level1Record._400000);
			AssertNotNull(UPECusHAWB.Level1Record._401000);
			AssertEquals(1, UPECusHAWB.Level1Record._500000Lines.Count);
			AssertEquals(1, UPECusHAWB.Level1Record._600000Lines.Count);
			AssertEquals("Ensure that no level1 notes exist at this point", 0, UPECusHAWB.Notes.FindByDescription(UPEPredefinedNoteTypes.Instance.Level1Record.Description).Length);
			Factory.Save();
			string expected = "US2795AU9639040422              D3824ARFY9JH2000003824ARFY9JH           N 1 19   LBS US          USDNNNN           USDAKE32156QF    09642             USD         USD5100      USDN0 N   NEDI  19APR200419 LBS         USDD1    NNN NN  NN  N USD           USD    T1                      19APR20040000           5100       P/PNRE       19   LBSNNC0000051901QF12            N N 1   N \n" + "US2795AU9639040422              D3824ARFY9JH2020001Z3824AR6640806327                 19     19     2    N                                                            7709705048E                AU09639  S1AU9639TF.113B2004-04-21                              Y                                                                             AUDNNNNNBI                                  \n" + "US2795AU9639040422              D3824ARFY9JH300000073706003824AR    BIOTECH CORP                       107 OAKWOOD DRIVE                                                     GLASTONBURY                                            CT060332481US 18606338111                                                                               11644                                         \n" + "US2795AU9639040422              D3824ARFY9JH400000170520138AU0495926SNOWSILL, SONYA                                             15 LEANDER ST                                                         FALCON                                                   6210     AU 0895343637                  SNOWSILL    SNOWSILL                                                 237           \n" + "US2795AU9639040422              DA4W82133K3G401000        8AU0596227WENDY PATERSON                     ContactName              GPO BOX 1609                       ABC                                SYDNEY                                                 NS2001     AU 123456789                                                                                        237           \n" + "US2795AU9639040422              D3824ARFY9JH5000006   EA SHEN MIN EXTRA STRENGTH                                                                                 600       USD11644               US                                   AU                                                                                                                                                 \n" + "US2795AU9639040422              D3947808NNSR60000099999999999    11 LBS         USD         USD          USD                             C0000092779UPS6901         NY1ZAT27736792092649                 10     AAY89758UPS                                                                                                                                                               \n";
			AssertMultilineASCIIEquals(string.Empty, expected, UPECusHAWB.Level1RecordNote.Text);
			UPECusHAWB.Level1Record._600000Lines.Add(new _600000Line("US2795AU9639040422              D3947808NNSR60000199999999999    11 LBS         USD         USD          USD                             C0000092779UPS6901         NY1ZAT27736792092649                 10     AAY89758UPS                                                                                                                                                               "));
			expected += "US2795AU9639040422              D3947808NNSR60000199999999999    11 LBS         USD         USD          USD                             C0000092779UPS6901         NY1ZAT27736792092649                 10     AAY89758UPS                                                                                                                                                               \n";
			Factory.Save();
			AssertMultilineASCIIEquals(string.Empty, expected, UPECusHAWB.Level1RecordNote.Text);
			UPECusHAWB.Level1Record = null;
			AssertNotNull(UPECusHAWB.Level1Record._200000);
			AssertNotNull(UPECusHAWB.Level1Record._202000);
			AssertNotNull(UPECusHAWB.Level1Record._300000);
			AssertNotNull(UPECusHAWB.Level1Record._400000);
			AssertNotNull(UPECusHAWB.Level1Record._401000);
			AssertEquals(1, UPECusHAWB.Level1Record._500000Lines.Count);
			AssertEquals(2, UPECusHAWB.Level1Record._600000Lines.Count);
		}

		public void TestHasFormalDec()
		{
			UPECusHAWB hAWB = Factory.New<UPECusHAWB>();
			hAWB.CS_JE_CustomsFormalEntry = ZGuid.Empty;
			AssertEquals("When a formal declaration is NOT attached to the HAWB", false, hAWB.HasFormalDec);
			hAWB.CS_JE_CustomsFormalEntry = Factory.NewWithValidTestData(typeof(Customs.Business.BaseJobDeclaration)).PK;
			AssertEquals("When a formal declaration is attached to the HAWB", true, hAWB.HasFormalDec);
		}

		public void TestLogMatchEvent()
		{
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.LogMatchEvent(false, "Test");
			uPECusHAWB.LogMatchEvent(false, "Test");
			uPECusHAWB.LogMatchEvent(true, "Test");
			uPECusHAWB.LogMatchEvent(true, "Test");
			uPECusHAWB.LogMatchEvent(true, "Test");
			ZQuery autoLogsFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AutoMatchDone.Code);
			AssertEquals(3, uPECusHAWB.Logs.GetAllLogs().Find(autoLogsFilter).Length);
			ZQuery manualLogsFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ManualMatchDone.Code);
			AssertEquals(2, uPECusHAWB.Logs.GetAllLogs().Find(manualLogsFilter).Length);
		}

		public void TestTotalAutoMatches()
		{
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.LogMatchEvent(true, "Test");
			uPECusHAWB.LogMatchEvent(true, "Test");
			uPECusHAWB.LogMatchEvent(true, "Test");
			AssertEquals(3, uPECusHAWB.TotalAutoMatches);
		}

		public void TestTotalManualMatches()
		{
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.LogMatchEvent(false, "Test");
			uPECusHAWB.LogMatchEvent(false, "Test");
			uPECusHAWB.LogMatchEvent(false, "Test");
			AssertEquals(3, uPECusHAWB.TotalManualMatches);
		}

		public void TestTryMatchConsignee_AccountNumberMatching()
		{
			UPEOrgHeader orgForMatching = Factory.New<UPEOrgHeader>();
			orgForMatching.OH_Code = "CODE";
			orgForMatching.MainAddress.OA_Address1 = "ADDRESS 1";
			orgForMatching.AccountNumber = "ACCT_NO";
			Factory.Save();
			int orgCount = Factory.GetDatabaseCount(typeof(OrgHeader));
			Level1Record level1Record = new Level1Record();
			level1Record._400000 = new _400000Line("US4196AU9639000626              DA15V04FXKC7400000        ACCT_NO   DAVID TASKER                       DAVID TASKER             18 WENDY CRESENT                   XYZ                                QUEENSLAND                                             VI4019     AU 0738831842    000                                                                                000           ");
			UPECusHAWB.Level1Record = level1Record;
			UPECusHAWB.TryMatchConsignee();
			Assert(!UPECusHAWB.UPEConsigneeAddress.P3_OH_MatchOrg.IsEmpty);
			Factory.Save();
			AssertEquals("Ensure that no extra temporary organisation is saved", orgCount, Factory.GetDatabaseCount(typeof(OrgHeader)));
		}

		public void TestTryMatchConsignee_NoMatchUseTempOrg()
		{
			int orgCount = Factory.GetDatabaseCount(typeof(OrgHeader));
			UPECusHAWB.CS_ConsigneeName = "CONSIGNEE NAME";
			UPECusHAWB.CS_ConsigneeStreet = "TEST";
			UPECusHAWB.CS_RL_NKDestination = "AUSYD";
			UPECusHAWB.TryMatchConsignee();
			Factory.Save();
			OrgHeader matchedOrg = Factory.Load<OrgHeader>(UPECusHAWB.UPEConsigneeAddress.P3_OH_MatchOrg);
			AssertEquals("CONSIGNEE NAME", matchedOrg.OH_FullName);
			AssertEquals(false, matchedOrg.OH_IsTempAccount);
			Factory.Save();
			AssertEquals("Ensure that extra organisation is saved", ++orgCount, Factory.GetDatabaseCount(typeof(OrgHeader)));
		}

		public void TestTryMatchConsignee_EnsureThatPhoneAndFaxAreSetOnNewOrg()
		{
			int orgCount = Factory.GetDatabaseCount(typeof(OrgHeader));
			UPECusHAWB.CS_ConsigneeName = "CONSIGNEE NAME";
			UPECusHAWB.CS_ConsigneeStreet = "TEST";
			UPECusHAWB.CS_ConsigneePhone = "12125649997";
			UPECusHAWB.CS_RL_NKDestination = "AUSYD";
			Level1Record level1Record = new Level1Record();
			level1Record._400000 = new _400000Line("US2795AU9639040422              D3824ARFY9JH400000        ACCT_NO   DAVID TASKER                       DAVID TASKER             18 WENDY CRESENT                   XYZ                                QUEENSLAND                                             VI4019     AU 0738831842    999                                                                             000              ");
			UPECusHAWB.Level1Record = level1Record;
			UPECusHAWB.TryMatchConsignee();
			Factory.Save();
			OrgHeader matchedOrg = Factory.Load<OrgHeader>(UPECusHAWB.UPEConsigneeAddress.P3_OH_MatchOrg);
			AssertEquals("CONSIGNEE NAME", matchedOrg.OH_FullName);
			AssertEquals("12125649997", matchedOrg.MainAddress.OA_Phone);
			AssertEquals("999", matchedOrg.MainAddress.OA_Fax);
			AssertEquals(false, matchedOrg.OH_IsTempAccount);
			Factory.Save();
			AssertEquals("Ensure that extra organisation is saved", ++orgCount, Factory.GetDatabaseCount(typeof(OrgHeader)));
		}

		public void TestTryMatchConsignee_EnsureThatPhoneAndFaxAreSetOnExistingOrg()
		{
			UPEOrgHeader orgForMatching = Factory.New<UPEOrgHeader>();
			orgForMatching.OH_Code = "CODE";
			orgForMatching.MainAddress.OA_Address1 = "ADDRESS 1";
			orgForMatching.AccountNumber = "ACCT_NO";
			Factory.Save();
			int orgCount = Factory.GetDatabaseCount(typeof(OrgHeader));
			Level1Record level1Record = new Level1Record();
			level1Record._400000 = new _400000Line("US2795AU9639040422              D3824ARFY9JH400000        ACCT_NO   DAVID TASKER                       DAVID TASKER             18 WENDY CRESENT                   XYZ                                QUEENSLAND                                             VI4019     AU 0738831842    999                                                                             000              ");
			UPECusHAWB.Level1Record = level1Record;
			UPECusHAWB.CS_ConsigneePhone = "12125649997";
			UPECusHAWB.TryMatchConsignee();
			OrgHeader matchedOrg = Factory.Load<OrgHeader>(UPECusHAWB.UPEConsigneeAddress.P3_OH_MatchOrg);
			AssertEquals("12125649997", matchedOrg.MainAddress.OA_Phone);
			AssertEquals("999", matchedOrg.MainAddress.OA_Fax);
		}

		public void TestTryMatchConsignee_EnsureThatPhoneAndFaxAreNoSetOnExistingOrgWhereTheOrgAlreadyHasTheseDetails()
		{
			UPEOrgHeader orgForMatching = Factory.New<UPEOrgHeader>();
			orgForMatching.OH_Code = "CODE";
			orgForMatching.MainAddress.OA_Address1 = "ADDRESS 1";
			orgForMatching.MainAddress.OA_Phone = "11111";
			orgForMatching.MainAddress.OA_Fax = "22222";
			orgForMatching.AccountNumber = "ACCT_NO";
			Factory.Save();
			int orgCount = Factory.GetDatabaseCount(typeof(OrgHeader));
			Level1Record level1Record = new Level1Record();
			level1Record._400000 = new _400000Line("US2795AU9639040422              D3824ARFY9JH400000        ACCT_NO   DAVID TASKER                       DAVID TASKER             18 WENDY CRESENT                   XYZ                                QUEENSLAND                                             VI4019     AU 0738831842    999                                                                             000              ");
			UPECusHAWB.Level1Record = level1Record;
			UPECusHAWB.TryMatchConsignee();
			OrgHeader matchedOrg = Factory.Load<OrgHeader>(UPECusHAWB.UPEConsigneeAddress.P3_OH_MatchOrg);
			AssertEquals("11111", matchedOrg.MainAddress.OA_Phone);
			AssertEquals("22222", matchedOrg.MainAddress.OA_Fax);
		}

		public void TestTryMatchConsignor_NoMatchUseTempOrg()
		{
			int orgCount = Factory.GetDatabaseCount(typeof(OrgHeader));
			UPECusHAWB.CS_ConsignorName = "CONSIGNOR NAME";
			UPECusHAWB.CS_ConsignorStreet = "TEST";
			UPECusHAWB.CS_RL_NKOrigin = "SGSIN";
			UPECusHAWB.TryMatchConsignor();
			Factory.Save();
			OrgHeader matchedOrg = Factory.Load<OrgHeader>(UPECusHAWB.UPEConsignorAddress.P3_OH_MatchOrg);
			AssertEquals("CONSIGNOR NAME", matchedOrg.OH_FullName);
			AssertEquals(false, matchedOrg.OH_IsTempAccount);
			Factory.Save();
			AssertEquals("Ensure that extra organisation is saved", ++orgCount, Factory.GetDatabaseCount(typeof(OrgHeader)));
		}

		public void TestTryMatchImporter_AccountNumberMatching()
		{
			UPEOrgHeader orgForMatching = Factory.New<UPEOrgHeader>();
			orgForMatching.OH_Code = "CODE";
			orgForMatching.MainAddress.OA_Address1 = "ADDRESS 1";
			orgForMatching.AccountNumber = "ACCT_NO";
			Factory.Save();
			int orgCount = Factory.GetDatabaseCount(typeof(OrgHeader));
			Level1Record level1Record = new Level1Record();
			level1Record._401000 = new _401000Line("US2795AU9639040422              DA4W82133K3G401000        ACCT_NO   WENDY PATERSON                     ContactName              GPO BOX 1609                       ABC                                SYDNEY                                                 NS2001     AU 123456789                   09576XP334CAU09639169   173    LBS466                                237           ");
			UPECusHAWB.Level1Record = level1Record;
			OrgPatternMatchAddress importer = Factory.New<OrgPatternMatchAddress>();
			importer.P3_AddressType = OrgPatternMatchAddress.Constants.AddressType.AirCargoImporter;
			importer.P3_ParentID = UPECusHAWB.PK;
			UPECusHAWB.TryMatchImporter();
			Assert(!UPECusHAWB.UPEImporterAddress.P3_OH_MatchOrg.IsEmpty);
			Factory.Save();
			AssertEquals("Ensure that no extra temporary organisation is saved", orgCount, Factory.GetDatabaseCount(typeof(OrgHeader)));
		}

		public void TestTryMatchImporter_NoMatchUseTempOrg()
		{
			OrgPatternMatchAddress importer = Factory.New<OrgPatternMatchAddress>();
			importer.P3_AddressType = OrgPatternMatchAddress.Constants.AddressType.AirCargoImporter;
			importer.P3_ParentID = UPECusHAWB.PK;
			importer.P3_CompanyName = "IMPORTER NAME";
			importer.P3_Address1 = "ADDRESS";
			UPECusHAWB.CS_RL_NKDestination = "AUPER";
			int orgCount = Factory.GetDatabaseCount(typeof(OrgHeader));
			UPECusHAWB.TryMatchImporter();
			OrgHeader matchedOrg = Factory.Load<OrgHeader>(UPECusHAWB.UPEImporterAddress.P3_OH_MatchOrg);
			AssertEquals("IMPORTER NAME", matchedOrg.OH_FullName);
			AssertEquals(false, matchedOrg.OH_IsTempAccount);
			Factory.Save();
			AssertEquals("Ensure that extra temporary organisation is saved", ++orgCount, Factory.GetDatabaseCount(typeof(OrgHeader)));
		}

		public void TestTryMatchAutomatically()
		{
			OrgHeader orgForMatching = GetOrgForMatching();
			UPECusHAWB airCargo = Factory.NewWithValidTestData<UPECusHAWB>();
			airCargo.CS_ConsignorCity = orgForMatching.MainAddress.OA_City;
			airCargo.CS_ConsignorName = orgForMatching.OH_FullName;
			airCargo.CS_ConsignorPhone = orgForMatching.MainAddress.OA_Phone;
			airCargo.CS_ConsignorPostcode = orgForMatching.MainAddress.OA_PostCode;
			airCargo.CS_ConsignorState = orgForMatching.MainAddress.OA_State;
			airCargo.CS_ConsignorStreet = orgForMatching.MainAddress.OA_Address1;
			airCargo.CS_ConsignorStreet2 = orgForMatching.MainAddress.OA_Address2;
			UPEOrgHeader orgForAccountCodeMatching = Factory.New<UPEOrgHeader>();
			orgForAccountCodeMatching.OH_Code = "CODE";
			orgForAccountCodeMatching.MainAddress.OA_Address1 = "ADDRESS 1";
			orgForAccountCodeMatching.AccountNumber = "ACCT_NO";
			Level1Record level1Record = new Level1Record();
			level1Record._400000 = new _400000Line("US4196AU9639000626              DA15V04FXKC7400000        ACCT_NO   DAVID TASKER                       DAVID TASKER             18 WENDY CRESENT                   XYZ                                QUEENSLAND                                             VI4019     AU 0738831842    000                                                                                000           ");
			level1Record._401000 = new _401000Line("US2795AU9639040422              DA4W82133K3G401000        ACCT_NO   WENDY PATERSON                     ContactName              GPO BOX 1609                       ABC                                SYDNEY                                                 NS2001     AU 123456789                   09576XP334CAU09639169   173    LBS466                                237           ");
			airCargo.Level1Record = level1Record;
			OrgPatternMatchAddress importer = Factory.New<OrgPatternMatchAddress>();
			importer.P3_AddressType = OrgPatternMatchAddress.Constants.AddressType.AirCargoImporter;
			importer.P3_ParentID = airCargo.PK;
			airCargo.TryMatchAutomatically();
			Factory.Save();
			Assert(!airCargo.UPEConsigneeAddress.P3_OH_MatchOrg.IsEmpty);
			Assert(!airCargo.UPEConsignorAddress.P3_OH_MatchOrg.IsEmpty);
		}

		public void TestWayBillShort()
		{
			UPECusHAWB.CS_HAWB = "TEST101";
			AssertEquals("When record does not exist, default should be the HAWB number", "TEST101", UPECusHAWB.WayBillShort);
			UPECusHAWB.CS_HAWB = "MORETHAN11CHARACTERLONG";
			AssertEquals("HAWB Number is more than 11 character long, should be an empty string", string.Empty, UPECusHAWB.WayBillShort);
			InsertJobRelatedWayBillRecord(UPECusHAWB.PK, JobRelatedWayBill.Constants.RelatedWayBillType.Child, string.Empty, "ABC", CusHAWBSchema.Constants.Prefix); // Not included in the filter
			InsertJobRelatedWayBillRecord(UPECusHAWB.PK, JobRelatedWayBill.Constants.RelatedWayBillType.Parent, string.Empty, "BCD", CusHAWBSchema.Constants.Prefix); // Included
			AssertEquals("BCD", UPECusHAWB.WayBillShort);
			UPECusHAWB.WayBillShort = "TESTWB101";
			AssertEquals("Should be assigned to the new value", "TESTWB101", UPECusHAWB.WayBillShort);
			Factory.Save();
			UPECusHAWB newUPECusHAWB = Factory.Load<UPECusHAWB>(UPECusHAWB.PK);
			AssertEquals("Should be loaded from the database", "TESTWB101", newUPECusHAWB.WayBillShort);
		}

		public void TestDutyType()
		{
			AssertEquals("Pre-condition", string.Empty, UPECusHAWB.DutyType);
			UPECusHAWB.CurrentQueue.P4_CustomAttrib6 = "_'";
			AssertEquals("Value should change", "_'", UPECusHAWB.DutyType);
			UPECusHAWB.DutyType = "*|";
			AssertEquals("Value should change", "*|", UPECusHAWB.DutyType);
			AssertEquals("Value should change", "*|", UPECusHAWB.CurrentQueue.P4_CustomAttrib6);
			AssertEquals("Incorrect max length", 3, UPECusHAWB.DutyTypeInfo.MaxLength);
			Assert("Invalid duty type, should have error", UPECusHAWB.DutyTypeInfo.HasErrors());
			UPECusHAWB.DutyType = DutyTypeCodeDescriptionPairList.Codes.Dutiable;
			Assert("Valid duty type, should not have error", !UPECusHAWB.DutyTypeInfo.HasErrors());
		}

		public void TestIsAlternateBrokerSplitShipment()
		{
			UPECusHAWB subsequentSplitCusHAWB = Factory.NewWithValidTestData<UPECusHAWB>();
			subsequentSplitCusHAWB.CS_CM = Factory.NewWithValidTestData<CusMAWB>().PK;
			UPECusHAWB.MAWB.CM_MAWB = "11111";
			subsequentSplitCusHAWB.MAWB.CM_MAWB = "22222";
			UPECusHAWB.CS_HAWB = "SPLITHBL";
			subsequentSplitCusHAWB.CS_HAWB = "SPLITHBL";
			AssertEquals("Is NOT a split shipment with an alternate broker", false, UPECusHAWB.IsAlternateBrokerSplitShipment);
			UPECusHAWB.CS_JE_CustomsFormalEntry = Factory.New(typeof(Customs.Business.BaseJobDeclaration)).PK;
			AssertEquals("Is NOT a split shipment with an alternate broker", false, UPECusHAWB.IsAlternateBrokerSplitShipment);
			UPECusHAWB.Declaration.JE_OH_Importer = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			OrgHeader airCusBroker = Factory.NewWithValidTestData<OrgHeader>();
			UPECusHAWB.Declaration.Importer.SetRelatedParty(airCusBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);
			AssertEquals("Is a split shipment with an alternate broker", true, UPECusHAWB.IsAlternateBrokerSplitShipment);
			subsequentSplitCusHAWB.Delete();
			AssertEquals("Is NOT a split shipment without the subsequent split", false, UPECusHAWB.IsAlternateBrokerSplitShipment);
		}

		public void TestBisiUploadDate()
		{
			AssertEquals(ZDateTime.Empty, UPECusHAWB.BisiUploadDate);
			ZDateTime expectedDate = new ZDateTime(2005, 12, 12);
			UPECusHAWB.CurrentQueue.P4_CustomDate1 = expectedDate;
			AssertEquals(expectedDate, UPECusHAWB.BisiUploadDate);
			expectedDate = new ZDateTime(2004, 10, 10);
			UPECusHAWB.BisiUploadDate = expectedDate;
			AssertEquals(expectedDate, UPECusHAWB.BisiUploadDate);
			AssertEquals(expectedDate, UPECusHAWB.CurrentQueue.P4_CustomDate1);
			AssertEquals("InnerInfo should be " + UPECusHAWB.CurrentQueue.P4_CustomDate1Info.Name, UPECusHAWB.CurrentQueue.P4_CustomDate1Info, ((ZWrappedPropertyInfo)UPECusHAWB.BisiUploadDateInfo).InnerInfo);
			Assert("Should be read-only", UPECusHAWB.BisiUploadDateInfo.ReadOnly);
		}

		public void TestDeliveryDate()
		{
			AssertEquals(ZDateTime.Empty, UPECusHAWB.DeliveryDate);
			ZDateTime expectedDate = new ZDateTime(2005, 12, 12);
			UPECusHAWB.CurrentQueue.P4_CustomDate3 = expectedDate;
			AssertEquals(expectedDate, UPECusHAWB.DeliveryDate);
			expectedDate = new ZDateTime(2004, 10, 10);
			UPECusHAWB.DeliveryDate = expectedDate;
			AssertEquals(expectedDate, UPECusHAWB.DeliveryDate);
			AssertEquals(expectedDate, UPECusHAWB.CurrentQueue.P4_CustomDate3);
			AssertEquals("InnerInfo should be " + UPECusHAWB.CurrentQueue.P4_CustomDate3Info.Name, UPECusHAWB.CurrentQueue.P4_CustomDate3Info, ((ZWrappedPropertyInfo)UPECusHAWB.DeliveryDateInfo).InnerInfo);
			Assert("Should be read-only", UPECusHAWB.DeliveryDateInfo.ReadOnly);
		}

		public void TestWeightInKg()
		{
			UPECusHAWB.CS_Weight = 20.5m;
			UPECusHAWB.CS_WeightUQ = Core.Constants.Weight.Tonnes;
			AssertEquals(20500m, UPECusHAWB.WeightInKg);
		}

		public void TestInvoiceNumber()
		{
			AssertEquals("Schema column correctly mapped", ProcessQueueSchema.P4_CustomAttrib1.Name, Callout.InvoiceNumberProcessQueueColumn.Name);
			AssertEquals(string.Empty, UPECusHAWB.InvoiceNumber);
			UPECusHAWB.CurrentQueue.P4_CustomAttrib1 = "INV101";
			AssertEquals("INV101", UPECusHAWB.InvoiceNumber);
			UPECusHAWB.InvoiceNumber = "INV102";
			AssertEquals("INV102", UPECusHAWB.InvoiceNumber);
			AssertEquals("INV102", UPECusHAWB.CurrentQueue.P4_CustomAttrib1);
			AssertEquals("InnerInfo should be " + UPECusHAWB.CurrentQueue.P4_CustomAttrib1Info.Name, UPECusHAWB.CurrentQueue.P4_CustomAttrib1Info, ((ZWrappedPropertyInfo)UPECusHAWB.InvoiceNumberInfo).InnerInfo);
			Assert("Should be read-only", UPECusHAWB.InvoiceNumberInfo.ReadOnly);
		}

		public void TestConsignorAccountNum()
		{
			AssertEquals("Empty when Consignor not set", ZString.Empty, UPECusHAWB.ConsignorAccountNum);
			UPECusHAWB.Level1Record = new Level1Record();
			UPECusHAWB.Level1Record._300000 = new _300000Line("US4196AU9639000626              DA15V04FXKC730000007201004A15V04    AVM SOFTWARE                       213WEST 35TH STREET,               402                                NEW YORK                                               NY10001    US 12125649997   12125630422                                                                                                    NALENI MCGA");
			AssertEquals("From level 1 record if no matched Consignor", "A15V04", UPECusHAWB.ConsignorAccountNum);
			UPECusHAWB.CS_OA_ConsignorAddress = Factory.NewWithValidTestData<UPEOrgHeader>().MainAddress.PK;
			AssertEquals("Empty when matched Consignor Account Number is empty", string.Empty, UPECusHAWB.ConsignorAccountNum);
			UPECusHAWB.Consignor.AccountNumber = "Account";
			AssertEquals("Consignor Account Number", "Account", UPECusHAWB.ConsignorAccountNum);
			AssertEquals("Should be ReadOnly so the user can't edit it without opening the org", true, UPECusHAWB.ConsignorAccountNumInfo.ReadOnly);
		}

		public void TestConsigneeAccountNum()
		{
			AssertEquals("Empty when Consignee not set", ZString.Empty, UPECusHAWB.ConsigneeAccountNum);
			UPECusHAWB.Level1Record = new Level1Record();
			UPECusHAWB.Level1Record._400000 = new _400000Line("US4196AU9639000626              DA15V04FXKC7400000        0000A15V04DAVID TASKER                       DAVID TASKER             18 WENDY CRESENT                   XYZ                                QUEENSLAND                                             VI4019     AU 0738831842    12125630422                                                                        000           ");
			AssertEquals("From level 1 record if no matched consignee", "A15V04", UPECusHAWB.ConsigneeAccountNum);
			UPECusHAWB.CS_OA_ConsigneeAddress = Factory.NewWithValidTestData<UPEOrgHeader>().MainAddress.PK;
			AssertEquals("Empty when matched consignee Account Number is empty", string.Empty, UPECusHAWB.ConsigneeAccountNum);
			UPECusHAWB.Consignee.AccountNumber = "Account";
			AssertEquals("Consignee Account Number", "Account", UPECusHAWB.ConsigneeAccountNum);
			AssertEquals("Should be ReadOnly so the user can't edit it without opening the org", true, UPECusHAWB.ConsigneeAccountNumInfo.ReadOnly);
		}

		public void TestLevel1RecordConsignorAccountNum()
		{
			AssertEquals("Empty when no Level1Record", ZString.Empty, UPECusHAWB.Level1RecordConsignorAccountNum);
			UPECusHAWB.Level1Record = new Level1Record();
			AssertEquals("Empty when no Level1Record._300000", ZString.Empty, UPECusHAWB.Level1RecordConsignorAccountNum);
			UPECusHAWB.Level1Record._300000 = new _300000Line("US4196AU9639000626              DA15V04FXKC730000007201004A15V04    AVM SOFTWARE                       213WEST 35TH STREET,               402                                NEW YORK                                               NY10001    US 12125649997   12125630422                                                                                                    NALENI MCGA");
			AssertEquals("Level1RecordConsignorAccountNum", "A15V04", UPECusHAWB.Level1RecordConsignorAccountNum);
		}

		public void TestLevel1RecordConsigneeAccountNum()
		{
			AssertEquals("Empty when no Level1Record", ZString.Empty, UPECusHAWB.Level1RecordConsigneeAccountNum);
			UPECusHAWB.Level1Record = new Level1Record();
			AssertEquals("Empty when no Level1Record._400000", ZString.Empty, UPECusHAWB.Level1RecordConsigneeAccountNum);
			UPECusHAWB.Level1Record._400000 = new _400000Line("US4196AU9639000626              DA15V04FXKC7400000        0000A15V04DAVID TASKER                       DAVID TASKER             18 WENDY CRESENT                   XYZ                                QUEENSLAND                                             VI4019     AU 0738831842    12125630422                                                                        000           ");
			AssertEquals("Level1RecordConsigneeAccountNum", "A15V04", UPECusHAWB.Level1RecordConsigneeAccountNum);
		}

		public void TestBillToAccountNumber()
		{
			AssertEquals("Empty initially", ZString.Empty, UPECusHAWB.BillToAccountNumber);
			AssertEquals("Should be ReadOnly so the user can't edit it without opening the org", true, UPECusHAWB.BillToAccountNumberInfo.ReadOnly);
			AssertEquals("Schema column should be mapped correctly", ProcessQueueSchema.P4_CustomAttrib2.Name, UPECusHAWB.BillToAccountNumberProcessQueueColumn.Name);
			UPECusHAWB.CurrentQueue.P4_CustomAttrib2 = "X123";
			AssertEquals("X123", UPECusHAWB.BillToAccountNumber);
			UPECusHAWB.BillToAccountNumber = "X124";
			AssertEquals("X124", UPECusHAWB.BillToAccountNumber);
			AssertEquals("X124", UPECusHAWB.CurrentQueue.P4_CustomAttrib2);
			AssertEquals("InnerInfo should be " + UPECusHAWB.CurrentQueue.P4_CustomAttrib2Info.Name, UPECusHAWB.CurrentQueue.P4_CustomAttrib2Info, ((ZWrappedPropertyInfo)UPECusHAWB.BillToAccountNumberInfo).InnerInfo);
			Assert("Should be read-only", UPECusHAWB.BillToAccountNumberInfo.ReadOnly);
		}

		public void TestDeclarationQueueStatus()
		{
			AssertNull("Should be no declaration", UPECusHAWB.Declaration);
			AssertEquals("no declaration, should be 'NO DEC'", "NO DEC", UPECusHAWB.DeclarationQueueStatus);
			fUPECusHAWB = AirCargoThatCausesDecToBeCreated();
			UPECusHAWB.CreateFormalDecAndMatchIfRequired();
			UPECusHAWB.Declaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Lodgement;
			AssertEquals("status should be " + DeclarationQueueCodeDescriptionPairList.Codes.Lodgement, DeclarationQueueCodeDescriptionPairList.Descriptions.Lodgement, UPECusHAWB.DeclarationQueueStatus);
		}

		#region Billing Terms
		public void TestBillingTerms()
		{
			AssertEquals("Pre-condition", ZString.Empty, UPECusHAWB.BillingTerms);
			UPECusHAWB.CurrentQueue.P4_CustomAttrib3 = "A";
			AssertEquals("A", UPECusHAWB.BillingTerms);
			UPECusHAWB.BillingTerms = "B";
			AssertEquals("B", UPECusHAWB.BillingTerms);
			AssertEquals("B", UPECusHAWB.CurrentQueue.P4_CustomAttrib3);
			AssertEquals("Incorrect max length", 3, UPECusHAWB.BillingTermsInfo.MaxLength);
			Assert("Invalid billing terms, should have error", UPECusHAWB.BillingTermsInfo.HasErrors());
			UPECusHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.CostAndFreight;
			Assert("Valid billing terms, should not have error", !UPECusHAWB.BillingTermsInfo.HasErrors());
		}

		public void TestBillingTerms_SettingFreightPrepaidCollect_PostCMR()
		{
			UPECusHAWB.CS_FreightPrepaidCollect = string.Empty;
			AssertFreightPrepaidCollectAfterSettingBillingTerms(BillingTermsCodeDescriptionPairList.Codes.CostAndFreight, CMRMethodsOfPayment.Codes.PrepaidOnly);
			AssertFreightPrepaidCollectAfterSettingBillingTerms(BillingTermsCodeDescriptionPairList.Codes.FreeDomicile, CMRMethodsOfPayment.Codes.PrepaidOnly);
			AssertFreightPrepaidCollectAfterSettingBillingTerms(BillingTermsCodeDescriptionPairList.Codes.SplitDutyAndVat, CMRMethodsOfPayment.Codes.PrepaidOnly);
			AssertFreightPrepaidCollectAfterSettingBillingTerms(BillingTermsCodeDescriptionPairList.Codes.Prepaid, CMRMethodsOfPayment.Codes.PrepaidOnly);
			AssertFreightPrepaidCollectAfterSettingBillingTerms(BillingTermsCodeDescriptionPairList.Codes.FreeBorder, CMRMethodsOfPayment.Codes.Collect);
			AssertFreightPrepaidCollectAfterSettingBillingTerms(BillingTermsCodeDescriptionPairList.Codes.FreightCollect, CMRMethodsOfPayment.Codes.Collect);
			AssertFreightPrepaidCollectAfterSettingBillingTerms(BillingTermsCodeDescriptionPairList.Codes.FreeOnBoard, CMRMethodsOfPayment.Codes.FobPortOfCall);
		}

		public void TestIsBillingTermsFreeDomicile()
		{
			UPECusHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreeBorder;
			AssertEquals(false, UPECusHAWB.IsBillingTermsFreeDomicile);
			UPECusHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreeDomicile;
			AssertEquals(true, UPECusHAWB.IsBillingTermsFreeDomicile);
		}

		void AssertFreightPrepaidCollectAfterSettingBillingTerms(string billingTerms, string expectedFreightPrepaidCollect)
		{
			UPECusHAWB.BillingTerms = billingTerms;
			AssertEquals("FreightPrepaidCollect should be set when billing terms is changed", expectedFreightPrepaidCollect, UPECusHAWB.CS_FreightPrepaidCollect);
		}

		#endregion
		#region Shipment Type
		public void TestShipmentType()
		{
			AssertEquals("Pre-condition", ZString.Empty, UPECusHAWB.ShipmentType);
			UPECusHAWB.CurrentQueue.P4_CustomAttrib4 = "b";
			AssertEquals("b", UPECusHAWB.ShipmentType);
			UPECusHAWB.ShipmentType = "c";
			AssertEquals("c", UPECusHAWB.ShipmentType);
			AssertEquals("c", UPECusHAWB.CurrentQueue.P4_CustomAttrib4);
			AssertEquals("Incorrect max length", 3, UPECusHAWB.ShipmentTypeInfo.MaxLength);
			Assert("Invalid shipment type, should have error", UPECusHAWB.ShipmentTypeInfo.HasErrors());
			UPECusHAWB.ShipmentType = ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments;
			Assert("Valid shipment type, should not have error", !UPECusHAWB.ShipmentTypeInfo.HasErrors());
		}

		public void TestShipmentType_SettingBaseShipmentType()
		{
			AssertBaseShipmentTypeAfterSettingShipmentType(ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments, "STD");
			AssertBaseShipmentTypeAfterSettingShipmentType(ShipmentTypeCodeDescriptionPairList.Codes.Documents, "DOC");
			AssertBaseShipmentTypeAfterSettingShipmentType(ShipmentTypeCodeDescriptionPairList.Codes.Letter, "DOC");
		}

		void AssertBaseShipmentTypeAfterSettingShipmentType(string shipmentType, string expectedBaseShipmentType)
		{
			UPECusHAWB.ShipmentType = shipmentType;
			AssertEquals("base shipmentType should be set when ShipmentType is changed", expectedBaseShipmentType, UPECusHAWB.CS_ShipmentType);
		}

		#endregion
		#region Rebill Flags
		public void TestRebillFlag()
		{
			UPECusHAWB.RebillFlagChanging += new RebillFlagChangingEventHandler(UPECusHAWB_RebillFlagChanging);
			AssertEquals("Default value should be 0", 0m, UPECusHAWB.CurrentQueue.P4_CustomDecimal1);
			AssertEquals("Default value should be Unflagged", RebillFlags.Unflagged, UPECusHAWB.RebillFlag);
			UPECusHAWB.RebillFlag = RebillFlags.IsChangedToFreeDomicile;
			AssertEquals(1m, UPECusHAWB.CurrentQueue.P4_CustomDecimal1);
			AssertEquals(RebillFlags.IsChangedToFreeDomicile, UPECusHAWB.RebillFlag);
			AssertEquals(1, RebillFlagChangedCount);
			UPECusHAWB.RebillFlag = RebillFlags.IsAbandoned;
			AssertEquals(3m, UPECusHAWB.CurrentQueue.P4_CustomDecimal1);
			AssertEquals(RebillFlags.IsAbandoned, UPECusHAWB.RebillFlag);
			AssertEquals(2, RebillFlagChangedCount);
			UPECusHAWB.RebillFlag = RebillFlags.IsRTS;
			AssertEquals(4m, UPECusHAWB.CurrentQueue.P4_CustomDecimal1);
			AssertEquals(RebillFlags.IsRTS, UPECusHAWB.RebillFlag);
			AssertEquals(3, RebillFlagChangedCount);
			UPECusHAWB.RebillFlag = RebillFlags.IsTranshipment;
			AssertEquals(2m, UPECusHAWB.CurrentQueue.P4_CustomDecimal1);
			AssertEquals(RebillFlags.IsTranshipment, UPECusHAWB.RebillFlag);
			AssertEquals(4, RebillFlagChangedCount);
			RebillFlagChangingShouldBeCancelled = true;
			UPECusHAWB.RebillFlag = RebillFlags.IsRTS;
			AssertEquals("Cancelled, should not change", RebillFlags.IsTranshipment, UPECusHAWB.RebillFlag);
		}

		public void TestRebillFlag_ReadOnly()
		{
			UPECusHAWB.RebillFlag = RebillFlags.IsChangedToFreeDomicile;
			AssertRebillFlagsReadOnly(false, true, true, true);
			UPECusHAWB.RebillFlag = RebillFlags.IsAbandoned;
			AssertRebillFlagsReadOnly(true, true, false, true);
			UPECusHAWB.RebillFlag = RebillFlags.IsTranshipment;
			AssertRebillFlagsReadOnly(true, false, true, true);
			UPECusHAWB.RebillFlag = RebillFlags.IsRTS;
			AssertRebillFlagsReadOnly(true, true, true, false);
			UPECusHAWB.RebillFlag = RebillFlags.Unflagged;
			AssertRebillFlagsReadOnly(false, false, false, false);
		}

		public void TestIsFreeDomicile()
		{
			UPECusHAWB.IsFreeDomicile = true;
			AssertEquals(RebillFlags.IsChangedToFreeDomicile, UPECusHAWB.RebillFlag);
			UPECusHAWB.IsFreeDomicile = false;
			AssertEquals("Should be unflagged", RebillFlags.Unflagged, UPECusHAWB.RebillFlag);
		}

		public void TestIsTranshipment()
		{
			UPECusHAWB.IsTranshipment = true;
			AssertEquals(RebillFlags.IsTranshipment, UPECusHAWB.RebillFlag);
			UPECusHAWB.IsTranshipment = false;
			AssertEquals("Should be unflagged", RebillFlags.Unflagged, UPECusHAWB.RebillFlag);
		}

		public void TestIsAbandoned()
		{
			UPECusHAWB.IsAbandoned = true;
			AssertEquals(RebillFlags.IsAbandoned, UPECusHAWB.RebillFlag);
			UPECusHAWB.IsAbandoned = false;
			AssertEquals("Should be unflagged", RebillFlags.Unflagged, UPECusHAWB.RebillFlag);
		}

		public void TestIsRTS()
		{
			UPECusHAWB.IsRTS = true;
			AssertEquals(RebillFlags.IsRTS, UPECusHAWB.RebillFlag);
			UPECusHAWB.IsRTS = false;
			AssertEquals("Should be unflagged", RebillFlags.Unflagged, UPECusHAWB.RebillFlag);
		}

		void AssertRebillFlagsReadOnly(bool isFreeDomicileReadOnly, bool isTranshipmentReadOnly, bool isAbandonedReadOnly, bool isRTSReadOnly)
		{
			AssertEquals(isFreeDomicileReadOnly, UPECusHAWB.IsFreeDomicileInfo.ReadOnly);
			AssertEquals(isTranshipmentReadOnly, UPECusHAWB.IsTranshipmentInfo.ReadOnly);
			AssertEquals(isAbandonedReadOnly, UPECusHAWB.IsAbandonedInfo.ReadOnly);
			AssertEquals(isRTSReadOnly, UPECusHAWB.IsRTSInfo.ReadOnly);
		}

		void UPECusHAWB_RebillFlagChanging(RebillFlagChangingEventArgs args)
		{
			args.Cancel = RebillFlagChangingShouldBeCancelled;
			RebillFlagChangedCount++;
		}

		int RebillFlagChangedCount;
		bool RebillFlagChangingShouldBeCancelled;
		#endregion
		#region Status Flags
		public void TestDoesStatusIndicateTranshipment()
		{
			UPECusHAWB.CS_CustomsStatus = string.Empty;
			AssertEquals(false, UPECusHAWB.DoesStatusIndicateTranshipment);
			UPECusHAWB.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus;
			AssertEquals(true, UPECusHAWB.DoesStatusIndicateTranshipment);
			UPECusHAWB.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.TranshphrmTranshipmentCargoIsClearButIsIdentifiedAsHighRiskMovement;
			AssertEquals(true, UPECusHAWB.DoesStatusIndicateTranshipment);
		}

		#endregion
		#region COD and Brown PostCodes
		public void TestIsConsigneeOrDeliveryAddressPostcodeCOD()
		{
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			var zone = CODPostcodeTransportProvider.Zones.AddNew();
			var item = zone.Items.AddNew();
			var postCode1 = Factory.New<RefPostCode>();
			postCode1.RK_CityTownPostCode = "1";
			item.TQ_FromPostCode = postCode1.RK_CityTownPostCode;
			var postCode2 = Factory.New<RefPostCode>();
			postCode2.RK_CityTownPostCode = "2";
			item.TQ_ToPostCode = postCode2.RK_CityTownPostCode;
			uPECusHAWB.CS_OA_ConsigneeAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			uPECusHAWB.Consignee.MainAddress.OA_PostCode = "99";
			AssertEquals("Non-COD postcode", false, uPECusHAWB.IsConsigneeOrDeliveryAddressPostcodeCOD);
			uPECusHAWB.Consignee.MainAddress.OA_PostCode = "1";
			AssertEquals("COD postcode", true, uPECusHAWB.IsConsigneeOrDeliveryAddressPostcodeCOD);
			uPECusHAWB.Consignee.Delete();
			uPECusHAWB.IsRedirected = true;
			uPECusHAWB.DeliveryAddressOverride.P3_PostCode = "2";
			AssertEquals("COD postcode", true, uPECusHAWB.IsConsigneeOrDeliveryAddressPostcodeCOD);
		}

		public void TestIsConsigneeOrDeliveryAddressPostcodeBrown()
		{
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			var zone = BrownPostcodeTransportProvider.Zones.AddNew();
			var item = zone.Items.AddNew();
			var postCode1 = Factory.New<RefPostCode>();
			postCode1.RK_CityTownPostCode = "1";
			item.TQ_FromPostCode = postCode1.RK_CityTownPostCode;
			var postCode2 = Factory.New<RefPostCode>();
			postCode2.RK_CityTownPostCode = "2";
			item.TQ_ToPostCode = postCode2.RK_CityTownPostCode;
			uPECusHAWB.CS_OA_ConsigneeAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			uPECusHAWB.Consignee.MainAddress.OA_PostCode = "99";
			AssertEquals("Non-Brown postcode", false, uPECusHAWB.IsConsigneeOrDeliveryAddressPostcodeBrown);
			AssertEquals(false, uPECusHAWB.IsCOD);
			uPECusHAWB.Consignee.MainAddress.OA_PostCode = "1";
			AssertEquals("Brown postcode", true, uPECusHAWB.IsConsigneeOrDeliveryAddressPostcodeBrown);
			uPECusHAWB.Consignee.Delete();
			uPECusHAWB.IsRedirected = true;
			uPECusHAWB.DeliveryAddressOverride.P3_PostCode = "2";
			AssertEquals("Brown postcode", true, uPECusHAWB.IsConsigneeOrDeliveryAddressPostcodeBrown);
			uPECusHAWB.DeliveryAddressOverride.P3_PostCode = "09";
			AssertEquals("Non-brown postcode", false, uPECusHAWB.IsCOD);
			uPECusHAWB.PaymentMethod = UPECargoPaymentMethod.Cheque;
			AssertEquals("Non-brown postcode but payment method is cheque", true, uPECusHAWB.IsCOD);
		}

		public void TestIsCOD()
		{
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			AssertEquals("Pre-Condition", false, uPECusHAWB.IsCOD);
			uPECusHAWB.PaymentMethod = UPECargoPaymentMethod.Cheque;
			AssertEquals("Cheque payment type", true, uPECusHAWB.IsCOD);
			uPECusHAWB.PaymentMethod = UPECargoPaymentMethod.Other;
			AssertEquals("Other payment type", false, uPECusHAWB.IsCOD);
			var zone = BrownPostcodeTransportProvider.Zones.AddNew();
			var item = zone.Items.AddNew();
			var postCode1 = Factory.New<RefPostCode>();
			postCode1.RK_CityTownPostCode = "1";
			item.TQ_FromPostCode = postCode1.RK_CityTownPostCode;
			var postCode2 = Factory.New<RefPostCode>();
			postCode2.RK_CityTownPostCode = "2";
			item.TQ_ToPostCode = postCode2.RK_CityTownPostCode;
			uPECusHAWB.CS_OA_ConsigneeAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			uPECusHAWB.Consignee.MainAddress.OA_PostCode = "99";
			AssertEquals("Non-COD postcode", false, uPECusHAWB.IsCOD);
			uPECusHAWB.Consignee.MainAddress.OA_PostCode = "1";
			AssertEquals("COD postcode", true, uPECusHAWB.IsCOD);
		}

		public void TestPaymentMethod()
		{
			AssertEquals("Default value should be None", UPECargoPaymentMethod.None, UPECusHAWB.PaymentMethod);
			UPECusHAWB.PaymentMethod = UPECargoPaymentMethod.Account;
			AssertEquals((ZDecimal)((int)UPECargoPaymentMethod.Account), UPECusHAWB.CurrentQueue.P4_CustomDecimal2);
			UPECusHAWB.PaymentMethod = UPECargoPaymentMethod.Nett7Day;
			AssertEquals((ZDecimal)((int)UPECargoPaymentMethod.Nett7Day), UPECusHAWB.CurrentQueue.P4_CustomDecimal2);
		}

		public void TestPaymentMethodOriginalValue()
		{
			UPECusHAWB.PaymentMethod = UPECargoPaymentMethod.EFT;
			Factory.Save();
			UPECusHAWB.PaymentMethod = UPECargoPaymentMethod.CreditCard;
			AssertEquals(UPECargoPaymentMethod.EFT, UPECusHAWB.PaymentMethodOriginalValue);
		}

		public void TestIsSubsequentSplitShipmentAndUPEProcessQueueOnSaving()
		{
			var melbourneTime = ZDateTime.Now;

			using (DisposableEnvironment.ForBranch(PerBranch.GB_Code))
			{
				AssertEquals("Commercial Queue Log count", 0, UPECusHAWB.CurrentQueue.CommercialQueueLogs.Count);
				AssertEquals("Custom Queue Log count", 0, UPECusHAWB.CurrentQueue.CustomsQueueLogs.Count);

				UPECusHAWB.MAWB.CM_GB = PerBranch.PK;
				Factory.Save();
			}

			using (DisposableEnvironment.ForBranch(MelBranch.GB_Code))
			{
				AssertEquals("Commercial Queue Log count", 0, UPECusHAWB.CurrentQueue.CommercialQueueLogs.Count);
				AssertEquals("Custom Queue Log count", 1, UPECusHAWB.CurrentQueue.CustomsQueueLogs.Count);
				AssertGreaterThan("Customs Queue log should not be melbourneTime; it should be in Perth time zone", melbourneTime, UPECusHAWB.CurrentQueue.CustomsQueuedDate);

				UPECusHAWB.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Hold;
				UPECusHAWB.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment;
				AssertEquals(false, UPECusHAWB.IsSubsequentSplitShipment);
				AssertEquals("CusHawb does not have any log related to split shipment", 0, UPECusHAWB.Logs.Find(s => s.SL_SE_NKEvent == Events.EditedARecordCode && s.SL_Reference.Contains("Moved to CPL: SET TO SUBSEQUENT SPLIT SHIPMENT")).Count());

				UPECusHAWB.IsSubsequentSplitShipment = true;
				Factory.Save();
			}

			AssertEquals(true, UPECusHAWB.IsSubsequentSplitShipment);
			AssertEquals(CommercialQueueCodeDescriptionPairList.Codes.Completed, UPECusHAWB.CurrentQueue.P4_QueueName);
			AssertEquals(ReasonCodeDescriptionPairList.Codes._C1_SubsequentSplitShipment, UPECusHAWB.CurrentQueue.P4_Status);
			AssertEquals("Commercial Queue Log count", 1, UPECusHAWB.CurrentQueue.CommercialQueueLogs.Count);
			AssertEquals("Custom Queue Log count", 1, UPECusHAWB.CurrentQueue.CustomsQueueLogs.Count);
			var log = UPECusHAWB.Logs.Find(s => s.SL_SE_NKEvent == Events.EditedARecordCode && s.SL_Reference.Contains("Moved to CPL: SET TO SUBSEQUENT SPLIT SHIPMENT")).FirstOrDefault();
			AssertNotNull("Split shipment log is found", log);
			AssertEquals("Event 's Branch", perBranch.GB_Code, log.SL_GB_NKBranch);
			AssertGreaterThan("Event Time should be on Perth 's timezone", melbourneTime, log.SL_EventTime);
			AssertGreaterThan("Commercial Queue log should not be melbourneTime; it should be in Perth time zone", melbourneTime, UPECusHAWB.CurrentQueue.CommercialQueuedDate);
			AssertGreaterThan("Customs Queue log should not be melbourneTime; it should be in Perth time zone", melbourneTime, UPECusHAWB.CurrentQueue.CustomsQueuedDate);
		}

		GlbBranch MelBranch
		{
			get
			{
				if (melBranch == null)
				{
					melBranch = Factory.New<GlbBranch>();
					melBranch.GB_GC = GlbCompany.CurrentCompany.PK;
					melBranch.GB_Code = "TML";
					melBranch.GB_RL_NKHomePort = "AUMEL";
					Factory.Save();
				}

				return melBranch;
			}
		}

		GlbBranch melBranch;
		GlbBranch PerBranch
		{
			get
			{
				if (perBranch == null)
				{
					perBranch = Factory.New<GlbBranch>();
					perBranch.GB_GC = GlbCompany.CurrentCompany.PK;
					perBranch.GB_Code = "TPR";
					perBranch.GB_RL_NKHomePort = "AUPER";
					Factory.Save();
				}

				return perBranch;
			}
		}

		GlbBranch perBranch;
		UPERateTransportProvider CODPostcodeTransportProvider
		{
			get
			{
				UPERateTransportProvider result = UPERateTransportProvider.LoadCODPostcodeTransportProvider(Factory);
				if (result == null)
				{
					result = Factory.New<UPERateTransportProvider>();
					result.TP_OH_RelatedParty = CODPostcodeTransportOrg.PK;
				}

				return result;
			}
		}

		UPERateTransportProvider BrownPostcodeTransportProvider
		{
			get
			{
				UPERateTransportProvider result = UPERateTransportProvider.LoadBrownPostcodeTransportProvider(Factory);
				if (result == null)
				{
					result = Factory.New<UPERateTransportProvider>();
					result.TP_OH_RelatedParty = BrownPostcodeTransportOrg.PK;
				}

				return result;
			}
		}

		OrgHeader CODPostcodeTransportOrg
		{
			get
			{
				OrgHeader result = OrgHeader.LoadFromCode(Factory, UPERateTransportProvider.CODPostcodeTransportOrgCode);
				if (result == null)
				{
					result = Factory.NewWithValidTestData<OrgHeader>();
					result.OH_Code = UPERateTransportProvider.CODPostcodeTransportOrgCode;
				}

				return result;
			}
		}

		OrgHeader BrownPostcodeTransportOrg
		{
			get
			{
				OrgHeader result = OrgHeader.LoadFromCode(Factory, UPERateTransportProvider.BrownPostcodeTransportOrgCode);
				if (result == null)
				{
					result = Factory.NewWithValidTestData<OrgHeader>();
					result.OH_Code = UPERateTransportProvider.BrownPostcodeTransportOrgCode;
				}

				return result;
			}
		}

		#endregion
		#endregion
		#region Property Overrides
		public void TestCurrentQueue()
		{
			AssertEquals("Incorrect type", typeof(UPECargoReportQueue), UPECusHAWB.CurrentQueue.GetType());
		}

		public void TestCS_HAWBInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, UPECusHAWB.CS_HAWBInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, UPECusHAWB.CS_HAWBInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, UPECusHAWB.CS_HAWBInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, UPECusHAWB.CS_HAWBInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestCS_MasterHouseBillInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, UPECusHAWB.CS_MasterHouseBillInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, UPECusHAWB.CS_MasterHouseBillInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, UPECusHAWB.CS_MasterHouseBillInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, UPECusHAWB.CS_MasterHouseBillInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestCS_WarehouseLocationInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, UPECusHAWB.CS_WarehouseLocationInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, UPECusHAWB.CS_WarehouseLocationInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, UPECusHAWB.CS_WarehouseLocationInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, UPECusHAWB.CS_WarehouseLocationInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestCS_RS_NK_ServiceLevelInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, UPECusHAWB.CS_RS_NK_ServiceLevelInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, UPECusHAWB.CS_RS_NK_ServiceLevelInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, UPECusHAWB.CS_RS_NK_ServiceLevelInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, UPECusHAWB.CS_RS_NK_ServiceLevelInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestCS_PiecesManifestedInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, UPECusHAWB.CS_PiecesManifestedInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, UPECusHAWB.CS_PiecesManifestedInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, UPECusHAWB.CS_PiecesManifestedInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, UPECusHAWB.CS_PiecesManifestedInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestCS_PiecesLandedInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, UPECusHAWB.CS_PiecesLandedInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, UPECusHAWB.CS_PiecesLandedInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, UPECusHAWB.CS_PiecesLandedInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, UPECusHAWB.CS_PiecesLandedInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestOnCreateAutoAdminLogDoesntThrowExceptionWithoutAutoAdminLog()
		{
			UPECusHAWB.Logs.GetType().GetMethod("SuspendCreateAutoAdminLog", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(UPECusHAWB.Logs, null);
			UPECusHAWB.Factory.Save();
			UPECusHAWB.CS_ConsigneeContactName = "abc";
			AssertNoExceptionThrown(() => UPECusHAWB.Factory.Save());
		}

		#endregion
		#region Overridden Captions
		public void TestWarehouseLocationCaption()
		{
			AssertEquals("Bond Location:", UPECusHAWB.WarehouseLocationCaption);
		}

		public void TestChargableWeightCaption()
		{
			AssertEquals("Dimensional Weight:", UPECusHAWB.ChargableWeightCaption);
		}

		#endregion
		#region Related Business Objects
		public void TestCusDecImporter()
		{
			AssertNull("No job declaration, should be null", UPECusHAWB.CusDecImporter);
			UPEJobDeclaration declaration = Factory.New<UPEJobDeclaration>();
			UPECusHAWB.CS_JE_CustomsFormalEntry = declaration.PK;
			AssertNull("No importer specified in the declaration, should be null", UPECusHAWB.CusDecImporter);
			OrgHeader testOrg = Factory.New<OrgHeader>();
			UPECusHAWB.Declaration.JE_OH_Importer = testOrg.PK;
			AssertEquals("Should be the Declaration's Importer", testOrg, UPECusHAWB.CusDecImporter);
			Assert("Should be read-only", UPECusHAWB.CusDecImporter.ReadOnly);
		}

		public void TestDeclaration()
		{
			AssertNull("Pre-condition", UPECusHAWB.Declaration);
			UPECusHAWB.CS_JE_CustomsFormalEntry = Factory.New(typeof(OrgHeader)).PK;
			AssertNull("Should still be null as the CS_JE_CustomsFormalEntry is not a PK for JobDeclaration", UPECusHAWB.Declaration);
			UPEJobDeclaration declaration = Factory.New<UPEJobDeclaration>();
			UPECusHAWB.CS_JE_CustomsFormalEntry = declaration.PK;
			AssertEquals(declaration.PK, UPECusHAWB.Declaration.PK);
		}

		public void TestConsigneePostcodeZone()
		{
			RateTransportProvider transportProvider = Factory.New<RateTransportProvider>();
			transportProvider.TP_OH_RelatedParty = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var zone1 = transportProvider.Zones.AddNew();
			var item1 = zone1.Items.AddNew();
			var postCode11 = Factory.New<RefPostCode>();
			postCode11.RK_CityTownPostCode = "1000";
			item1.TQ_FromPostCode = postCode11.RK_CityTownPostCode;
			var postCode12 = Factory.New<RefPostCode>();
			postCode12.RK_CityTownPostCode = "1002";
			item1.TQ_ToPostCode = postCode12.RK_CityTownPostCode;
			RateTransportProvider decoyTransportProvider = Factory.New<RateTransportProvider>();
			decoyTransportProvider.TP_OH_RelatedParty = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			var zone2 = transportProvider.Zones.AddNew();
			var item2 = zone2.Items.AddNew();
			var postCode21 = Factory.New<RefPostCode>();
			postCode21.RK_CityTownPostCode = "1000";
			item2.TQ_FromPostCode = postCode21.RK_CityTownPostCode;
			var postCode22 = Factory.New<RefPostCode>();
			postCode22.RK_CityTownPostCode = "1002";
			item2.TQ_ToPostCode = postCode22.RK_CityTownPostCode;
			UPECusHAWB hAWB = Factory.New<UPECusHAWB>();
			AssertEquals("ConsigneePostcodeZone when no match to any known postcode range", null, hAWB.ConsigneePostcodeZone);
			hAWB.CS_ConsigneePostcode = "1001";
			AssertEquals("Correctly matched ConsigneePostcodeZone", zone1.PK, hAWB.ConsigneePostcodeZone.PK);
		}

		public void TestBISIUploadedShipmentHeader()
		{
			AssertEquals("No BISIUploadedShipmentHeader object initially for the test", null, UPECusHAWB.BISIUploadedShipmentHeader);
			new BISIShipmentDataAccessor(Factory).UpdateUploadData(UPECusHAWB);
			Factory.ClearQueryCache();
			AssertNotNull("BISIUploadedShipmentHeader not null after upload completed", UPECusHAWB.BISIUploadedShipmentHeader);
		}

		#region UPEImporterAddress / ImporterMatchApproval
		public void TestUPEImporterAddress()
		{
			UPECusHAWB airCargo = Factory.New<UPECusHAWB>();
			AssertEquals("UPEImporterAddress should not be available by default", null, airCargo.UPEImporterAddress);
			OrgPatternMatchAddress importerAddress = Factory.New<OrgPatternMatchAddress>();
			importerAddress.P3_ParentID = airCargo.PK;
			importerAddress.P3_AddressType = OrgPatternMatchAddress.Constants.AddressType.AirCargoImporter;
			AssertEquals("Importer should be available once it is created", importerAddress.PK, airCargo.UPEImporterAddress.PK);
		}

		public void TestImporterMatchApproval()
		{
			UPECusHAWB airCargo = Factory.New<UPECusHAWB>();
			CreateNewAirCargoImporterAddress(airCargo);
			AssertEquals("Should not require a Importer match by default", null, airCargo.ImporterMatchApproval);
			AssertEquals("Should not require a Importer match by default", false, airCargo.RequiresImporterMatch);
			airCargo.RequiresImporterMatch = true;
			OrgPatternMatchAddress importerMatchApprovalAddress = airCargo.ImporterMatchApproval.AddressToBeMatched;
			AssertEquals("The address on the OrgMatchApproval record should correspond to the importer address returned from the air cargo biz obj", airCargo.UPEImporterAddress.PK, importerMatchApprovalAddress.PK);
			OrgMatchApproval importerMatchApproval = airCargo.ImporterMatchApproval;
			AssertEquals("Should require a Importer match when specified to", true, airCargo.RequiresImporterMatch);
			AssertNotNull("Should require a Importer match when specified to", importerMatchApproval);
			airCargo.RequiresImporterMatch = false;
			AssertEquals("Should not require a Importer match when specified not to", false, airCargo.RequiresImporterMatch);
			AssertEquals("Should not require a Importer match when specified not to", null, airCargo.ImporterMatchApproval);
			AssertEquals("The Importer address should NOT be deleted as it is the only source of information of the importer, unlike consignee/consignor which also exist on the air cargo record", false, importerMatchApprovalAddress.IsDeleted);
			AssertEquals("Match approval record should be deleted once it is no longer required", true, importerMatchApproval.IsDeleted);
		}

		public void TestSetRequiresImporterMatchToTrueWithoutAnImporterThrowsSilentException()
		{
			UPECusHAWB airCargo = Factory.New<UPECusHAWB>();
			AssertEquals("Should not require a Importer match by default", false, airCargo.RequiresImporterMatch);
			ErrorReporter.Clear();
			airCargo.RequiresImporterMatch = true;
			AssertEquals("Silent exception should be raised due to no importer being available", 1, ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count);
			ErrorReporter.Clear();
		}

		#endregion
		#region UPEConsigneeAddress / ConsigneeMatchApproval
		public void TestUPEConsigneeAddress()
		{
			UPECusHAWB airCargo = Factory.New<UPECusHAWB>();
			AssertEquals("UPEConsigneeAddress should not be available by default", null, airCargo.UPEConsigneeAddress);
			OrgPatternMatchAddress consigneeAddress = Factory.New<OrgPatternMatchAddress>();
			consigneeAddress.P3_ParentID = airCargo.PK;
			consigneeAddress.P3_AddressType = OrgPatternMatchAddress.Constants.AddressType.AirCargoConsignee;
			AssertEquals("Importer should be available once it is created", consigneeAddress.PK, airCargo.UPEConsigneeAddress.PK);
		}

		public void TestConsigneeMatchApproval()
		{
			UPECusHAWB airCargo = Factory.New<UPECusHAWB>();
			AssertEquals("Should not require a consignee match by default", null, airCargo.ConsigneeMatchApproval);
			AssertEquals("Should not require a consignee match by default", false, airCargo.RequiresConsigneeMatch);
			airCargo.RequiresConsigneeMatch = true;
			OrgPatternMatchAddress consigneeMatchApprovalAddress = airCargo.ConsigneeMatchApproval.AddressToBeMatched;
			OrgMatchApproval consigneeMatchApproval = airCargo.ConsigneeMatchApproval;
			AssertEquals("Should require a consignee match when specified to", true, airCargo.RequiresConsigneeMatch);
			AssertNotNull("Should require a consignee match when specified to", consigneeMatchApproval);
			airCargo.RequiresConsigneeMatch = false;
			AssertEquals("Should not require a consignee match when specified not to", false, airCargo.RequiresConsigneeMatch);
			AssertEquals("Should not require a consignee match when specified not to", null, airCargo.ConsigneeMatchApproval);
			AssertEquals("Match approval address record should be deleted once it is no longer required", true, consigneeMatchApprovalAddress.IsDeleted);
			AssertEquals("Match approval record should be deleted once it is no longer required", true, consigneeMatchApproval.IsDeleted);
		}

		#endregion
		#region UPEConsignorAddress / ConsignorMatchApproval
		public void TestUPEConsignorAddress()
		{
			UPECusHAWB airCargo = Factory.New<UPECusHAWB>();
			AssertEquals("UPEConsignorAddress should not be available by default", null, airCargo.UPEConsignorAddress);
			OrgPatternMatchAddress consignorAddress = Factory.New<OrgPatternMatchAddress>();
			consignorAddress.P3_ParentID = airCargo.PK;
			consignorAddress.P3_AddressType = OrgPatternMatchAddress.Constants.AddressType.AirCargoConsignor;
			AssertEquals("Importer should be available once it is created", consignorAddress.PK, airCargo.UPEConsignorAddress.PK);
		}

		public void TestConsignorMatchApproval()
		{
			UPECusHAWB airCargo = Factory.New<UPECusHAWB>();
			AssertEquals("Should not require a consignor match by default", null, airCargo.ConsignorMatchApproval);
			AssertEquals("Should not require a consignor match by default", false, airCargo.RequiresConsignorMatch);
			airCargo.RequiresConsignorMatch = true;
			OrgPatternMatchAddress consignorMatchApprovalAddress = airCargo.ConsignorMatchApproval.AddressToBeMatched;
			OrgMatchApproval consignorMatchApproval = airCargo.ConsignorMatchApproval;
			AssertEquals("Should require a consignor match when specified to", true, airCargo.RequiresConsignorMatch);
			AssertNotNull("Should require a consignor match when specified to", consignorMatchApproval);
			airCargo.RequiresConsignorMatch = false;
			AssertEquals("Should not require a consignor match when specified not to", false, airCargo.RequiresConsignorMatch);
			AssertEquals("Should not require a consignor match when specified not to", null, airCargo.ConsignorMatchApproval);
			AssertEquals("Match approval address record should be deleted once it is no longer required", true, consignorMatchApprovalAddress.IsDeleted);
			AssertEquals("Match approval record should be deleted once it is no longer required", true, consignorMatchApproval.IsDeleted);
		}

		#endregion
		#region ImporterOrConsigneeMatchApproval
		public void TestRequiresImporterOrConsigneeMatchApproval()
		{
			UPECusHAWB airCargo = Factory.New<UPECusHAWB>();
			airCargo.RequiresImporterOrConsigneeMatchApproval = true;
			AssertEquals("RequiresImporterOrConsigneeMatchApproval should be true after set to be true", true, airCargo.RequiresImporterOrConsigneeMatchApproval);
			AssertEquals("RequiresConsigneeMatchApproval true because we have no importer so must use consignee", true, airCargo.RequiresConsigneeMatch);
			AssertEquals("RequiresImporterMatchApproval false because we have no importer so must use consignee", false, airCargo.RequiresImporterMatch);
			airCargo.RequiresImporterOrConsigneeMatchApproval = false;
			AssertEquals("RequiresImporterOrConsigneeMatchApproval should be false after set to be false", false, airCargo.RequiresImporterOrConsigneeMatchApproval);
			AssertEquals("RequiresConsigneeMatchApproval should be false", false, airCargo.RequiresConsigneeMatch);
			AssertEquals("RequiresImporterMatchApproval should be false", false, airCargo.RequiresImporterMatch);
			CreateNewAirCargoImporterAddress(airCargo);
			airCargo.RequiresImporterMatch = true;
			airCargo.RequiresImporterOrConsigneeMatchApproval = true;
			AssertEquals("RequiresImporterOrConsigneeMatchApproval should be true after set to be true", true, airCargo.RequiresImporterOrConsigneeMatchApproval);
			AssertEquals("RequiresConsigneeMatchApproval false because we have an importer", false, airCargo.RequiresConsigneeMatch);
			AssertEquals("RequiresImporterMatchApproval true because we have an importer", true, airCargo.RequiresImporterMatch);
			airCargo.RequiresImporterOrConsigneeMatchApproval = false;
			AssertEquals("RequiresImporterOrConsigneeMatchApproval should be false after set to be false", false, airCargo.RequiresImporterOrConsigneeMatchApproval);
			AssertEquals("RequiresConsigneeMatchApproval should be false", false, airCargo.RequiresConsigneeMatch);
			AssertEquals("RequiresImporterMatchApproval should be false", false, airCargo.RequiresImporterMatch);
		}

		public void TestImporterOrConsigneeMatchApproval()
		{
			UPECusHAWB airCargo = Factory.New<UPECusHAWB>();
			AssertEquals("No match approval is available initially when there is no consignee or importer for the test", null, airCargo.ImporterOrConsigneeMatchApproval);
			airCargo.RequiresConsigneeMatch = true;
			AssertEquals("Should use the consignee match approval if there is no importer", airCargo.ConsigneeMatchApproval.PK, airCargo.ImporterOrConsigneeMatchApproval.PK);
			CreateNewAirCargoImporterAddress(airCargo);
			airCargo.RequiresImporterMatch = true;
			AssertEquals("Should use the importer match approval if one is specified", airCargo.ImporterMatchApproval.PK, airCargo.ImporterOrConsigneeMatchApproval.PK);
		}

		public void TestImporterOrConsigneeMatchedOrgPK_ForImporter()
		{
			UPECusHAWB airCargo = Factory.New<UPECusHAWB>();
			OrgPatternMatchAddress importerAddress = Factory.New<OrgPatternMatchAddress>();
			importerAddress.P3_AddressType = OrgPatternMatchAddress.Constants.AddressType.AirCargoImporter;
			importerAddress.P3_ParentID = airCargo.PK;
			OrgPatternMatchAddress consigneeAddress = Factory.New<OrgPatternMatchAddress>();
			consigneeAddress.P3_AddressType = OrgPatternMatchAddress.Constants.AddressType.AirCargoConsignee;
			consigneeAddress.P3_ParentID = airCargo.PK;
			AssertEquals("ImporterOrConsigneeMatchedOrgPK with a matched consignee, and with importer that requires a match but isnt yet matched", true, airCargo.ImporterOrConsigneeMatchedOrgPK.IsEmpty);
			consigneeAddress.P3_OH_MatchOrg = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			AssertEquals("ImporterOrConsigneeMatchedOrgPK with consignee and importer, with a matched importer", airCargo.UPEConsigneeAddress.P3_OH_MatchOrg, airCargo.ImporterOrConsigneeMatchedOrgPK);
		}

		public void TestImporterOrConsigneeMatchedOrgPK_ForConsignee()
		{
			UPECusHAWB airCargo = Factory.NewWithValidTestData<UPECusHAWB>();
			airCargo.CS_OA_ConsigneeAddress = ZGuid.Empty;
			AssertEquals("ImporterOrConsigneeMatchedOrgPK with consignee, without an org match", true, airCargo.ImporterOrConsigneeMatchedOrgPK.IsEmpty);
			airCargo.RequiresConsigneeMatch = true;
			airCargo.UPEConsigneeAddress.P3_OH_MatchOrg = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			AssertEquals("ImporterOrConsigneeMatchedOrgPK with consignee on OrgPatternMatchAddress", airCargo.UPEConsigneeAddress.P3_OH_MatchOrg, airCargo.ImporterOrConsigneeMatchedOrgPK);
			airCargo.CS_OA_ConsigneeAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			Factory.Save();
			AssertEquals("ImporterOrConsigneeMatchedOrgPK with consignee set on CusHAWB (which cascades to OrgPatternMatchAddress)", airCargo.Consignee.PK, airCargo.ImporterOrConsigneeMatchedOrgPK);
		}

		public void TestSendAirCargoMessage()
		{
			UPECusHAWB uPECusHAWB = GetNewCusHAWBValidForACA();
			uPECusHAWB.CS_HAWB = string.Empty;
			uPECusHAWB.DutyType = DutyTypeCodeDescriptionPairList.Codes.NonDutiable;
			uPECusHAWB.SendAirCargoMessage();
			AssertEquals(0, uPECusHAWB.Messages.Count);
			uPECusHAWB.CS_HAWB = "TEST";
			uPECusHAWB.CS_ConsigneeName = "consignee";
			uPECusHAWB.CS_ConsignorName = "consignor";
			uPECusHAWB.CS_ConsigneeStreet = "consignee street";
			uPECusHAWB.CS_ConsignorStreet = "consignor street";
			uPECusHAWB.SendAirCargoMessage();
			AssertEquals(1, uPECusHAWB.Messages.Count);
		}

		UPECusHAWB GetNewCusHAWBValidForACA()
		{
			CusMAWB cusMAWB = Factory.New<CusMAWB>();
			cusMAWB.CM_ArrivalDate = new ZDateTime(2005, 10, 10);
			cusMAWB.CM_FlightNo = "QF123";
			cusMAWB.CM_MAWB = "08133333333";
			cusMAWB.CM_RL_NKDischargePort = "AUSYD";
			cusMAWB.CM_RL_NKFirstArrivalPort = "AUBNE";
			cusMAWB.CM_RL_NKLoadPort = "SGSIN";
			UPECusHAWB result = (UPECusHAWB)cusMAWB.ChildBills.AddNew();
			result.CS_HAWB = "UPECusHAWB";
			result.CS_RL_NKOrigin = "SGSIN";
			result.CS_RL_NKDestination = "AUSYD";
			result.CS_MasterHouseBill = "08144444444";
			result.CS_Weight = 10m;
			result.CS_WeightUQ = "KG";
			result.CS_GoodsValue = 5230.12m;
			result.CS_RX_NKGoodsCurrency = "ZAR";
			result.CS_GoodsDescription = "GoodsDescription";
			result.CS_IsSurplus = false;
			result.CS_FreightPrepaidCollect = CMRMethodsOfPayment.Codes.PrepaidOnly;
			result.ShipmentType = ShipmentTypeCodeDescriptionPairList.Codes.Documents;
			result.CS_ShipmentType = "DOC";
			result.CS_PiecesManifested = 10;
			result.CS_RS_NK_ServiceLevel = "STD";
			result.CS_ConsigneeCity = "Sydney";
			result.CS_ConsigneeContactName = "ConsigneeContactName";
			result.CS_ConsigneeName = "ConsigneeName";
			result.CS_ConsigneePhone = "9637660";
			result.CS_ConsigneePostcode = "7551";
			result.CS_ConsigneeState = "NSW";
			result.CS_ConsigneeStreet = "Street1";
			result.CS_ConsigneeStreet2 = "Street2";
			result.CS_RN_NKConsigneeCountry = "AU";
			result.CS_ConsignorCity = "Singapore";
			result.CS_ConsignorContactName = "ConsignorContactName";
			result.CS_ConsignorName = "ConsignorName";
			result.CS_ConsignorPhone = "963766";
			result.CS_ConsignorPostcode = "7550";
			result.CS_ConsignorState = "Jurong";
			result.CS_ConsignorStreet = "Pasir";
			result.CS_ConsignorStreet2 = "Gudang";
			result.CS_RN_NKConsignorCountry = "SG";
			return result;
		}

		OrgHeader GetOrgForMatching()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "AUSTRALIAN FILM & PIPE MANUFACTURERS";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.MainAddress.OA_City = "SYDNEY";
			orgHeader.MainAddress.OA_Phone = "+61297255045";
			orgHeader.MainAddress.OA_PostCode = "2164";
			orgHeader.MainAddress.OA_State = "NSW";
			orgHeader.MainAddress.OA_Address1 = "150 WOODPARK RD";
			orgHeader.MainAddress.OA_Address2 = "SMITHFIELD";
			orgHeader.CreatePatternMatchingAddressFromMainAddress(Factory);
			orgHeader.CreatePatternMatchingName(Factory);
			Factory.Save();
			return orgHeader;
		}

		public void TestPutOnOrgMatchingQueueIfRequired_WithImporter()
		{
			UPECusHAWB airCargo = Factory.New<UPECusHAWB>();
			CreateNewAirCargoImporterAddress(airCargo);
			airCargo.PutOnOrgMatchingQueueOrCreateFormalDec();
			AssertEquals("Importer match required", true, airCargo.RequiresImporterMatch);
			AssertEquals("Consignor match required", true, airCargo.RequiresConsignorMatch);
			airCargo.RequiresImporterMatch = false;
			airCargo.RequiresConsignorMatch = false;
			airCargo.UPEImporterAddress.P3_OH_MatchOrg = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			airCargo.CS_OA_ConsignorAddress = ZGuid.Empty;
			airCargo.PutOnOrgMatchingQueueOrCreateFormalDec();
			AssertEquals("Importer match not required because it is already done", false, airCargo.RequiresConsigneeMatch);
			AssertEquals("Consignor match required", true, airCargo.RequiresConsignorMatch);
			airCargo.RequiresImporterMatch = false;
			airCargo.RequiresConsignorMatch = false;
			airCargo.UPEImporterAddress.P3_OH_MatchOrg = ZGuid.Empty;
			airCargo.CS_OA_ConsignorAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			airCargo.PutOnOrgMatchingQueueOrCreateFormalDec();
			AssertEquals("Importer match required", true, airCargo.RequiresImporterMatch);
			AssertEquals("Consignor match not required because it is already done", false, airCargo.RequiresConsignorMatch);
		}

		public void TestPutOnOrgMatchingQueueIfRequired_WithConsignor()
		{
			UPECusHAWB airCargo = Factory.New<UPECusHAWB>();
			airCargo.PutOnOrgMatchingQueueOrCreateFormalDec();
			AssertEquals("Consignee match required", true, airCargo.RequiresConsigneeMatch);
			AssertEquals("Consignor match required", true, airCargo.RequiresConsignorMatch);
			airCargo.RequiresConsigneeMatch = false;
			airCargo.RequiresConsignorMatch = false;
			airCargo.CS_OA_ConsigneeAddress = ZGuid.Empty;
			airCargo.CS_OA_ConsignorAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			airCargo.PutOnOrgMatchingQueueOrCreateFormalDec();
			AssertEquals("Consignee match required", true, airCargo.RequiresConsigneeMatch);
			AssertEquals("Consignor match not required because it is already done", false, airCargo.RequiresConsignorMatch);
		}

		public void TestPutOnOrgMatchingQueueOrCreateFormalDec_CreateFormalDecWhenNoMatchingRequired()
		{
			UPECusHAWB airCargo = Factory.New<UPECusHAWB>();
			airCargo.CS_GoodsDescription = "test air cargo goods";
			airCargo.RequiresConsigneeMatch = true;
			airCargo.UPEConsigneeAddress.P3_OH_MatchOrg = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			airCargo.CS_OA_ConsignorAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			airCargo.PutOnOrgMatchingQueueOrCreateFormalDec();
			AssertFormalDecCreatedWithGoodsDescription(airCargo.CS_GoodsDescription);
		}

		public void TestIsOnOrgMatchingQueueOrMatchingCompleted_WhenQueuedForMatching()
		{
			UPECusHAWB airCargo = Factory.New<UPECusHAWB>();
			airCargo.RequiresConsigneeMatch = true;
			AssertEquals("Only on matching queue if both consignee/importer and consignor are on queue", false, airCargo.IsOnOrgMatchingQueueOrMatchingCompleted);
			airCargo.RequiresConsignorMatch = true;
			AssertEquals("Only on matching queue if both consignee/importer and consignor are on queue", true, airCargo.IsOnOrgMatchingQueueOrMatchingCompleted);
		}

		public void TestIsOnOrgMatchingQueueOrMatchingCompleted_WhenCompleted()
		{
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			UPECusHAWB airCargo = Factory.NewWithValidTestData<UPECusHAWB>();
			airCargo.CS_OA_ConsigneeAddress = organisation.MainAddress.PK;
			Factory.Save();
			AssertEquals("Only completed if both consignee/importer and consignor are completed", false, airCargo.IsOnOrgMatchingQueueOrMatchingCompleted);
			airCargo.CS_OA_ConsignorAddress = organisation.MainAddress.PK;
			Factory.Save();
			AssertEquals("Only completed if both consignee/importer and consignor are completed", true, airCargo.IsOnOrgMatchingQueueOrMatchingCompleted);
		}

		public void TestIsOnOrgMatchingQueueOrMatchingCompleted_WhenConsigneeQueuedAndConsignorCompleted()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			UPECusHAWB airCargo = Factory.New<UPECusHAWB>();
			airCargo.RequiresConsigneeMatch = true;
			AssertEquals("Only completed if both consignee/importer and consignor are queued/completed", false, airCargo.IsOnOrgMatchingQueueOrMatchingCompleted);
			airCargo.CS_OA_ConsignorAddress = organisation.MainAddress.PK;
			AssertEquals("Only completed if both consignee/importer and consignor are queued/completed", true, airCargo.IsOnOrgMatchingQueueOrMatchingCompleted);
		}

		public void TestIsOnOrgMatchingQueueOrMatchingCompleted_WhenConsigneeCompletedAndConsignorQueued()
		{
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			UPECusHAWB airCargo = Factory.NewWithValidTestData<UPECusHAWB>();
			airCargo.CS_OA_ConsigneeAddress = organisation.MainAddress.PK;
			Factory.Save();
			AssertEquals("Only completed if both consignee/importer and consignor are queued/completed", false, airCargo.IsOnOrgMatchingQueueOrMatchingCompleted);
			airCargo.RequiresConsignorMatch = true;
			Factory.Save();
			AssertEquals("Only completed if both consignee/importer and consignor are queued/completed", true, airCargo.IsOnOrgMatchingQueueOrMatchingCompleted);
		}

		void AssertFormalDecCreatedWithGoodsDescription(string goodsDescription)
		{
			AssertNotNull("Declaration should be created from the air cargo because we had all the information we needed; no org matching was required", FindDec(goodsDescription));
		}

		JobDeclaration FindDec(string goodsDescription)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JobDeclarationSchema.JE_GoodsDescription, goodsDescription);
			return Factory.LoadTop1<JobDeclaration>(filter);
		}

		#endregion
		#region ChildRelatedWayBills
		public void TestChildRelatedWayBills()
		{
			InsertJobRelatedWayBillRecord(ZGuid.NewZGuid(), "123", "123", string.Empty, CusHAWBSchema.Constants.Prefix); // Not included
			InsertJobRelatedWayBillRecord(UPECusHAWB.PK, "ABC", "ABC", string.Empty, CusHAWBSchema.Constants.Prefix); // Not included
			InsertJobRelatedWayBillRecord(UPECusHAWB.PK, JobRelatedWayBill.Constants.RelatedWayBillType.Child, "ABC101", string.Empty, CusHAWBSchema.Constants.Prefix); // Included
			AssertEquals(1, UPECusHAWB.ChildRelatedWayBills.Count);
			AssertEquals("ABC101", UPECusHAWB.ChildRelatedWayBills[0].EB_WaybillNumber);
			Assert("ChildRelatedWayBill has been added to the collection, UPECusHAWB.HasChanges has to be true if ChildRelatedWayBills is registered as editable children", UPECusHAWB.HasChanges);
			InsertJobRelatedWayBillRecord(UPECusHAWB.PK, JobRelatedWayBill.Constants.RelatedWayBillType.Child, "BCD", string.Empty, "XX"); // Not included
			InsertJobRelatedWayBillRecord(UPECusHAWB.PK, JobRelatedWayBill.Constants.RelatedWayBillType.Child, "BCD102", string.Empty, CusHAWBSchema.Constants.Prefix); // Included
			InsertJobRelatedWayBillRecord(UPECusHAWB.PK, JobRelatedWayBill.Constants.RelatedWayBillType.Child, "BCD103", string.Empty, CusHAWBSchema.Constants.Prefix); // Included
			AssertEquals("Before reloading, should still be 1", 1, UPECusHAWB.ChildRelatedWayBills.Count);
			UPECusHAWB.ChildRelatedWayBills.Load();
			AssertEquals("After reloading, should be 3", 3, UPECusHAWB.ChildRelatedWayBills.Count);
		}

		void InsertJobRelatedWayBillRecord(ZGuid parentID, ZString wayBillType, ZString wayBillNumber, ZString wayBillShortNumber, ZString tablePrefix)
		{
			JobRelatedWayBill wayBill = Factory.New<JobRelatedWayBill>();
			wayBill.EB_ParentID = parentID;
			wayBill.EB_WaybillType = wayBillType;
			wayBill.EB_WaybillNumber = wayBillNumber;
			wayBill.EB_WaybillShortNumber = wayBillShortNumber;
			wayBill.EB_ParentTableCode = tablePrefix;
		}

		#endregion
		#region Delivery Address Redirection
		public void TestIsRedirected()
		{
			Assert("By default should be false", !UPECusHAWB.IsRedirected);
			AssertNull("Should not exist", UPECusHAWB.DeliveryAddressOverride);
			UPECusHAWB.CS_ConsigneeName = "AAA";
			UPECusHAWB.CS_ConsigneeContactName = "Kramer";
			UPECusHAWB.CS_ConsigneeStreet = "Elizabeth Street";
			UPECusHAWB.CS_ConsigneeStreet2 = "Down the Alley";
			UPECusHAWB.CS_ConsigneeCity = "Gladstone";
			UPECusHAWB.CS_ConsigneeState = "QLD";
			UPECusHAWB.CS_ConsigneePostcode = "00098";
			UPECusHAWB.CS_ConsigneePhone = "9992838";
			UPECusHAWB.IsRedirected = true;
			AssertNotNull("Should exist", UPECusHAWB.DeliveryAddressOverride);
			AssertEquals("AAA", UPECusHAWB.DeliveryAddressOverride.P3_CompanyName);
			AssertEquals("Kramer", UPECusHAWB.DeliveryAddressOverride.P3_ContactName);
			AssertEquals("Elizabeth Street", UPECusHAWB.DeliveryAddressOverride.P3_Address1);
			AssertEquals("Down the Alley", UPECusHAWB.DeliveryAddressOverride.P3_Address2);
			AssertEquals("Gladstone", UPECusHAWB.DeliveryAddressOverride.P3_City);
			AssertEquals("QLD", UPECusHAWB.DeliveryAddressOverride.P3_State);
			AssertEquals("00098", UPECusHAWB.DeliveryAddressOverride.P3_PostCode);
			AssertEquals("9992838", UPECusHAWB.DeliveryAddressOverride.P3_Phone);
			UPECusHAWB.IsRedirected = false;
			AssertNull("Should be deleted", UPECusHAWB.DeliveryAddressOverride);
		}

		public void TestIsRedirected_IsRedirectedChangedEvent()
		{
			UPECusHAWB.IsRedirectedChanging += new IsRedirectedChangingEventHandler(UPECusHAWB_IsRedirectedChanging);
			UPECusHAWB.IsRedirected = true;
			AssertEquals(true, IsRedirectedChangingEventArgsIsRedirected);
			AssertEquals(1, IsRedirectedChangingCount);
			AssertNotNull(UPECusHAWB.DeliveryAddressOverride);
			AssertEquals("Has to be a registered editable children", true, UPECusHAWB.IsRegisteredEditableChildObject(UPECusHAWB.DeliveryAddressOverride));
			UPECusHAWB.IsRedirected = false;
			AssertEquals(false, IsRedirectedChangingEventArgsIsRedirected);
			AssertEquals(2, IsRedirectedChangingCount);
			AssertNull(UPECusHAWB.DeliveryAddressOverride);
			ShouldCancelIsRedirectedChange = true;
			UPECusHAWB.IsRedirected = true;
			AssertEquals("Event should be fired, but canceled", 3, IsRedirectedChangingCount);
			AssertNull("Should still be null as the event is canceled", UPECusHAWB.DeliveryAddressOverride);
		}

		public void TestDeliveryAddressOverride()
		{
			AssertNull("By default should not exist in Factory", UPECusHAWB.DeliveryAddressOverride);
			OrgPatternMatchAddress newAddress = Factory.New<OrgPatternMatchAddress>();
			newAddress.P3_ParentID = UPECusHAWB.PK;
			AssertNull("Should not be found, AddressType is not specified", UPECusHAWB.DeliveryAddressOverride);
			newAddress.P3_AddressType = OrgPatternMatchAddress.Constants.AddressType.AirCargoConsigneeOverride;
			AssertEquals("Should be found", newAddress.PK, UPECusHAWB.DeliveryAddressOverride.PK);
			AssertEquals("Has to be a registered editable children", true, UPECusHAWB.IsRegisteredEditableChildObject(UPECusHAWB.DeliveryAddressOverride));
			Factory.Save();
			BusinessObjectFactory cleanFactory = new BusinessObjectFactory();
			UPECusHAWB newHAWB = cleanFactory.Load<UPECusHAWB>(UPECusHAWB.PK);
			AssertEquals("Should be found", newAddress.PK, newHAWB.DeliveryAddressOverride.PK);
			AssertEquals(true, newHAWB.IsRedirected);
		}

		public void TestDeliveryAddressOverrideNotDuplicated()
		{
			UPECusHAWB.CreateDeliveryAddressOverrideIfNotExist();
			AssertNotNull("Sanity check", UPECusHAWB.DeliveryAddressOverride);
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			UPECusHAWB newUPECusHAWB = newFactory.Load<UPECusHAWB>(UPECusHAWB.PK);
			newUPECusHAWB.CreateDeliveryAddressOverrideIfNotExist();
			AssertEquals("Should not be creating anoter one if already exist", UPECusHAWB.DeliveryAddressOverride.PK, newUPECusHAWB.DeliveryAddressOverride.PK);
		}

		void UPECusHAWB_IsRedirectedChanging(IsRedirectedChangingEventArgs eventArgs)
		{
			eventArgs.Cancel = ShouldCancelIsRedirectedChange;
			IsRedirectedChangingEventArgsIsRedirected = eventArgs.IsRedirected;
			IsRedirectedChangingCount++;
		}

		bool IsRedirectedChangingEventArgsIsRedirected;
		bool ShouldCancelIsRedirectedChange;
		int IsRedirectedChangingCount;
		#endregion
		#endregion
		#region Alerts
		public void TestAlertsList()
		{
			AssertNotNull(UPECusHAWB.AlertsList);
			UPECusHAWB.AlertsList.Add("TEST");
			AssertEquals("TEST", UPECusHAWB.AlertsList[0]);
		}

		public void TestAlertsList_ForUnreadRelatedDocuments()
		{
			AssertEquals("No alerts initially for the test", 0, UPECusHAWB.AlertsList.Count);
			UPECusHAWB.ResetAlertsList();
			DocTypeWithForceUserToRead.Factory.Save();
			using (CurrentUserInitialsChanger.ChangeCurrentUserInitials("XXX"))
			{
				UPECusHAWB.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 1, 1, 1, 1 }, "Test.pdf", DocTypeWithForceUserToRead.RT_DocType);
				UPECusHAWB.DocManagerInfo.Save();
			}

			AssertEquals("There are unread eDocs attached to this job", UPECusHAWB.AlertsList[0]);
		}

		public void TestHasAlerts()
		{
			AssertEquals(false, UPECusHAWB.HasAlerts);
			UPECusHAWB.AlertsList.Add("TEST");
			AssertEquals(true, UPECusHAWB.HasAlerts);
		}

		public void TestResetAlertsList()
		{
			UPECusHAWB.AlertsList.Add("TEST");
			AssertEquals("TEST", UPECusHAWB.AlertsList[0]);
			UPECusHAWB.ResetAlertsList();
			AssertEquals(false, UPECusHAWB.HasAlerts);
		}

		RefDocType DocTypeWithForceUserToRead
		{
			get
			{
				if (fDocTypeWithForceUserToRead == null)
				{
					fDocTypeWithForceUserToRead = Factory.New<RefDocType>();
					fDocTypeWithForceUserToRead.RT_ForceUserToRead = true;
					fDocTypeWithForceUserToRead.RT_ReferenceType = DocumentImaging.DocManagerReferenceTypes.All;
					fDocTypeWithForceUserToRead.RT_DocType = "FUR";
				}

				return fDocTypeWithForceUserToRead;
			}
		}

		RefDocType fDocTypeWithForceUserToRead;
		#endregion
		#region Declaration Queue Summary
		[TestDate(2004, 1, 1)]
		public void TestDeclarationQueuedDate()
		{
			AssertEquals("No declaration, should be empty", ZDateTime.Empty, UPECusHAWB.DeclarationQueuedDate);
			TaxOrFeeTestHelper.SetDeminimus(Factory, 100m);
			UPECusHAWB.CS_JE_CustomsFormalEntry = Factory.New(typeof(UPEJobDeclaration)).PK;
			UPECusHAWB.CS_GoodsValue = 300;
			UPECusHAWB.CS_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Aruba;
			AssertEquals("Sanity check", false, UPECusHAWB.IsFormalDecRequired);
			AssertEquals("Formal Dec not required, should be empty", ZDateTime.Empty, UPECusHAWB.DeclarationQueuedDate);
			UPECusHAWB.CS_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Australia;
			AssertEquals("No queue changed events, should be empty", ZDateTime.Empty, UPECusHAWB.DeclarationQueuedDate);
			UPECusHAWB.Declaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Completed;
			Factory.Save();
			AssertEquals(new ZDateTime(2004, 1, 1), UPECusHAWB.DeclarationQueuedDate);
		}

		public void TestDeclarationQueueHeldOrCompleted()
		{
			TaxOrFeeTestHelper.SetDeminimus(Factory, 100m);
			UPECusHAWB.CS_GoodsValue = 300;
			UPECusHAWB.CS_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Aruba;
			AssertEquals("Sanity check", false, UPECusHAWB.IsFormalDecRequired);
			AssertEquals("COMPLETED", UPECusHAWB.DeclarationQueueHeldOrCompleted);
			UPECusHAWB.CS_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Australia;
			AssertEquals("Sanity check", true, UPECusHAWB.IsFormalDecRequired);
			AssertEquals("HELD", UPECusHAWB.DeclarationQueueHeldOrCompleted);
			UPECusHAWB.CS_JE_CustomsFormalEntry = Factory.New(typeof(UPEJobDeclaration)).PK;
			UPECusHAWB.Declaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.EIR;
			AssertEquals("HELD", UPECusHAWB.DeclarationQueueHeldOrCompleted);
			UPECusHAWB.CS_GoodsValue = 50;
			AssertEquals("HELD", UPECusHAWB.DeclarationQueueHeldOrCompleted);
			UPECusHAWB.Declaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Completed;
			Factory.Save();
			AssertEquals("COMPLETED", UPECusHAWB.DeclarationQueueHeldOrCompleted);
		}

		public void TestDeclarationQueueSummary()
		{
			TaxOrFeeTestHelper.SetDeminimus(Factory, 100m);
			UPECusHAWB.CS_GoodsValue = 300;
			UPECusHAWB.CS_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Aruba;
			AssertEquals("Sanity check", false, UPECusHAWB.IsFormalDecRequired);
			AssertEquals("NOT REQUIRED", UPECusHAWB.DeclarationQueueSummary);
			UPECusHAWB.CS_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Australia;
			AssertEquals("Sanity check", true, UPECusHAWB.IsFormalDecRequired);
			AssertEquals(string.Empty, UPECusHAWB.DeclarationQueueSummary);
			UPECusHAWB.CS_JE_CustomsFormalEntry = Factory.New(typeof(UPEJobDeclaration)).PK;
			UPECusHAWB.Declaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.EIR;
			UPECusHAWB.Declaration.CurrentQueue.P4_CustomsStatus = string.Empty;
			Factory.Save();
			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.EIR, UPECusHAWB.DeclarationQueueSummary);
			UPECusHAWB.CS_GoodsValue = 50;
			AssertEquals("Should show summary if there is declaration attached", DeclarationQueueCodeDescriptionPairList.Codes.EIR, UPECusHAWB.DeclarationQueueSummary);
			UPECusHAWB.Declaration.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.DN_SplitShipment;
			Factory.Save();
			string expected = DeclarationQueueCodeDescriptionPairList.Codes.EIR + " / " + ReasonCodeDescriptionPairList.Descriptions.DN_SplitShipment;
			AssertEquals(expected, UPECusHAWB.DeclarationQueueSummary);
		}

		#endregion
		#region Queue Moving
		public void TestQueueIsMovedDependingOnCMRCustomsStatus()
		{
			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				fUPECusHAWB = AirCargoThatCausesDecToBeCreated();
				MasterBill.ChildBills.Add(UPECusHAWB);
				AssertQueueMoving(CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, CargoReportQueueCodeDescriptionPairList.Codes.Completed, string.Empty);
				AssertQueueMoving(CMRConsolidatedCargoStatuses.Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed, CargoReportQueueCodeDescriptionPairList.Codes.Completed, string.Empty);
				AssertQueueMoving(CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus, CargoReportQueueCodeDescriptionPairList.Codes.Completed, string.Empty);
				AssertQueueMoving(CMRConsolidatedCargoStatuses.Codes.WithdrawnCargoReportHadBeenWithdrawn, CargoReportQueueCodeDescriptionPairList.Codes.Completed, string.Empty);
				AssertQueueMoving(CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, CargoReportQueueCodeDescriptionPairList.Codes.Completed, string.Empty, DeclarationQueueCodeDescriptionPairList.Codes.Completed, string.Empty, true);
				AssertQueueMoving(CMRConsolidatedCargoStatuses.Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed, CargoReportQueueCodeDescriptionPairList.Codes.Completed, string.Empty, DeclarationQueueCodeDescriptionPairList.Codes.Completed, string.Empty, true);
				AssertQueueMoving(CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus, CargoReportQueueCodeDescriptionPairList.Codes.Completed, string.Empty, DeclarationQueueCodeDescriptionPairList.Codes.Completed, string.Empty, true);
				AssertQueueMoving(CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms, CargoReportQueueCodeDescriptionPairList.Codes.Completed, string.Empty, DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding, string.Empty);
				AssertQueueMoving(CMRConsolidatedCargoStatuses.Codes.AqisseizedCargoIsSeizedByQuarantine, CargoReportQueueCodeDescriptionPairList.Codes.Completed, string.Empty, DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding, string.Empty);
				AssertQueueMoving(CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement, CargoReportQueueCodeDescriptionPairList.Codes.Completed, string.Empty, DeclarationQueueCodeDescriptionPairList.Codes.BCA, ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold);
				AssertQueueMoving(CMRConsolidatedCargoStatuses.Codes.TranshphrmTranshipmentCargoIsClearButIsIdentifiedAsHighRiskMovement, CargoReportQueueCodeDescriptionPairList.Codes.Completed, string.Empty, DeclarationQueueCodeDescriptionPairList.Codes.BCA, ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold);
				UPECusHAWB.CS_JE_CustomsFormalEntry = Factory.New(typeof(UPEJobDeclaration)).PK;
				AssertQueueMoving("CRP", CargoReportQueueCodeDescriptionPairList.Codes.AwaitingDeclaration, string.Empty, false);
				UPECusHAWB.CS_JE_CustomsFormalEntry = ZGuid.Empty;
				AssertQueueMoving("CRP", CargoReportQueueCodeDescriptionPairList.Codes.Unknown, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, false);
			}
		}

		public void TestQueueIsMovedDependingOnCMRCustomsStatus_FinanceCompletedWhenCargoReportCompletedWithAlternateBroker()
		{
			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				fUPECusHAWB = AirCargoThatCausesDecToBeCreated();
				MasterBill.ChildBills.Add(UPECusHAWB);
				UPECusHAWB.CS_JE_CustomsFormalEntry = Factory.New<UPEJobDeclaration>().PK;
				UPECusHAWB.Declaration.JE_OH_Importer = Factory.New<OrgHeader>().PK;
				UPECusHAWB.Declaration.Importer.SetRelatedParty(Factory.New<OrgHeader>(), RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);
				UPECusHAWB.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
				AssertEquals("Commercial queue should be completed when the cargo report is cleared", CommercialQueueCodeDescriptionPairList.Codes.Completed, UPECusHAWB.CurrentQueue.P4_QueueName);
			}
		}

		public void TestQueueIsMovedDependingOnCMRMessageStatus()
		{
			fUPECusHAWB = AirCargoThatCausesDecToBeCreated();
			MasterBill.ChildBills.Add(UPECusHAWB);
			AssertQueueMoving_MsgStatus(CMRBaseStatuses.Codes.AwaitingResponseToOriginal, CargoReportQueueCodeDescriptionPairList.Codes.Pending);
			AssertQueueMoving_MsgStatus(CMRBaseStatuses.Codes.AwaitingResponseToAmendment, CargoReportQueueCodeDescriptionPairList.Codes.Pending);
			AssertQueueMoving_MsgStatus(CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal, CargoReportQueueCodeDescriptionPairList.Codes.Pending);
			AssertQueueMoving_MsgStatus(CMRBaseStatuses.Codes.AmendmentRejected, CargoReportQueueCodeDescriptionPairList.Codes.Intervention, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease);
			AssertQueueMoving_MsgStatus(CMRBaseStatuses.Codes.OriginalRejected, CargoReportQueueCodeDescriptionPairList.Codes.Intervention, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease);
			AssertQueueMoving_MsgStatus(CMRBaseStatuses.Codes.WithdrawalRejected, CargoReportQueueCodeDescriptionPairList.Codes.Intervention, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease);
		}

		public void TestQueueShouldNotBeMovedDependingOnCMRMessageStatusIfQueueHasBeenCompleted()
		{
			fUPECusHAWB = AirCargoThatCausesDecToBeCreated();
			MasterBill.ChildBills.Add(UPECusHAWB);
			UPECusHAWB.CurrentQueue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.Completed;
			AssertQueueMoving_MsgStatus(CMRBaseStatuses.Codes.AwaitingResponseToOriginal, CargoReportQueueCodeDescriptionPairList.Codes.Completed);
			AssertQueueMoving_MsgStatus(CMRBaseStatuses.Codes.AwaitingResponseToAmendment, CargoReportQueueCodeDescriptionPairList.Codes.Completed);
			AssertQueueMoving_MsgStatus(CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal, CargoReportQueueCodeDescriptionPairList.Codes.Completed);
			AssertQueueMoving_MsgStatus(CMRBaseStatuses.Codes.AmendmentRejected, CargoReportQueueCodeDescriptionPairList.Codes.Completed);
			AssertQueueMoving_MsgStatus(CMRBaseStatuses.Codes.OriginalRejected, CargoReportQueueCodeDescriptionPairList.Codes.Completed);
			AssertQueueMoving_MsgStatus(CMRBaseStatuses.Codes.WithdrawalRejected, CargoReportQueueCodeDescriptionPairList.Codes.Completed);
		}

		public void TestQueueIsMovedToPendingWhenResponsePendingFlagIsSet()
		{
			object toBeSaved = UPECusHAWB.CurrentQueue;
			Factory.Save();
			UPECusHAWB.CurrentQueue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.Intervention;
			UPECusHAWB.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease;
			UPECusHAWB.CS_IsResponsePending = false;
			AssertCustomsQueue(UPECusHAWB.CurrentQueue, CargoReportQueueCodeDescriptionPairList.Codes.Intervention, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, string.Empty);
			UPECusHAWB.CS_IsResponsePending = true;
			AssertCustomsQueue(UPECusHAWB.CurrentQueue, CargoReportQueueCodeDescriptionPairList.Codes.Pending, string.Empty, string.Empty);
		}

		public void TestQueueMovement()
		{
			UPECusHAWB.CurrentQueue.P4_CustomsQueue = string.Empty;
			UPECusHAWB.CurrentQueue.P4_CustomsStatus = string.Empty;
			UPECusHAWB.CurrentQueue.P4_CustomsSubStatus = string.Empty;
			int numberOfLogsCreated = UPECusHAWB.Logs.LogsNotInDB.Length;
			UPECusHAWB.MoveCustomsQueueTo(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			Assert(UPECusHAWB.CurrentQueue.P4_CustomsQueue == ZString.Empty);
			Assert(UPECusHAWB.CurrentQueue.P4_CustomsStatus == ZString.Empty);
			Assert(UPECusHAWB.CurrentQueue.P4_CustomsSubStatus == ZString.Empty);
			Assert(UPECusHAWB.Logs.LogsNotInDB.Length == numberOfLogsCreated);
			UPECusHAWB.MoveCustomsQueueTo(CargoReportQueueCodeDescriptionPairList.Codes.Hold, ZString.Empty, ZString.Empty, ZString.Empty);
			Assert(CargoReportQueueCodeDescriptionPairList.Codes.Hold == UPECusHAWB.CurrentQueue.P4_CustomsQueue);
			UPECusHAWB.MoveCustomsQueueTo(CargoReportQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, ZString.Empty, ZString.Empty);
			Assert(ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease == UPECusHAWB.CurrentQueue.P4_CustomsStatus);
			UPECusHAWB.MoveCustomsQueueTo(CargoReportQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, "bla", ZString.Empty);
			Assert("bla" == UPECusHAWB.CurrentQueue.P4_CustomsSubStatus);
			UPECusHAWB.MoveCustomsQueueTo(CargoReportQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, ZString.Empty, "log ref");
			Assert(UPECusHAWB.Logs.LogsNotInDB.Length == numberOfLogsCreated + 1);
			Assert(UPECusHAWB.Logs.MostRecentLog.SL_Reference.Contains("log ref"));
		}

		public void TestMoveToCommercialQueue()
		{
			UPECusHAWB.CurrentQueue.P4_QueueName = string.Empty;
			UPECusHAWB.CurrentQueue.P4_Status = string.Empty;
			UPECusHAWB.CurrentQueue.P4_SubStatus = string.Empty;
			int numberOfLogsCreated = UPECusHAWB.Logs.LogsNotInDB.Length;
			UPECusHAWB.MoveToQueue(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			Assert(UPECusHAWB.CurrentQueue.P4_QueueName == ZString.Empty);
			Assert(UPECusHAWB.CurrentQueue.P4_Status == ZString.Empty);
			Assert(UPECusHAWB.CurrentQueue.P4_SubStatus == ZString.Empty);
			Assert(UPECusHAWB.Logs.LogsNotInDB.Length == numberOfLogsCreated);
			UPECusHAWB.MoveToQueue(CargoReportQueueCodeDescriptionPairList.Codes.Hold, ZString.Empty, ZString.Empty, ZString.Empty);
			Assert(CargoReportQueueCodeDescriptionPairList.Codes.Hold == UPECusHAWB.CurrentQueue.P4_QueueName);
			UPECusHAWB.MoveToQueue(CargoReportQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, ZString.Empty, ZString.Empty);
			Assert(ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease == UPECusHAWB.CurrentQueue.P4_Status);
			UPECusHAWB.MoveToQueue(CargoReportQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, "bla", ZString.Empty);
			Assert("bla" == UPECusHAWB.CurrentQueue.P4_SubStatus);
			UPECusHAWB.MoveToQueue(CargoReportQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, ZString.Empty, "log ref");
			Assert(UPECusHAWB.Logs.LogsNotInDB.Length == numberOfLogsCreated + 1);
			Assert(UPECusHAWB.Logs.MostRecentLog.SL_Reference.Contains("log ref"));
		}

		public void TestMoveDeclarationToQueue()
		{
			fUPECusHAWB = AirCargoThatCausesDecToBeCreated();
			AssertNull("Pre-condition", UPECusHAWB.Declaration);
			UPECusHAWB.MoveDeclarationToQueue(DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding, ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, ZString.Empty, ZString.Empty);
			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding, UPECusHAWB.Declaration.CurrentQueue.P4_CustomsQueue);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, UPECusHAWB.Declaration.CurrentQueue.P4_CustomsStatus);
		}

		[ExpectNoExceptions]
		public void TestMoveDeclarationToQueue_OrganisationsCannotBeMatched()
		{
			AssertNull("Pre-condition", UPECusHAWB.Declaration);
			UPECusHAWB.MoveDeclarationToQueue(DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding, ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, ZString.Empty, ZString.Empty);
		}

		public void TestMoveDeclarationToQueue_OnlyIfDeclarationIsNotYetCreated()
		{
			UPECusHAWB.CS_JE_CustomsFormalEntry = Factory.New(typeof(UPEJobDeclaration)).PK;
			UPECusHAWB.Declaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Classification;
			UPECusHAWB.Declaration.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration;
			UPECusHAWB.MoveDeclarationToQueue(DeclarationQueueCodeDescriptionPairList.Codes.EIR, ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, ZString.Empty, ZString.Empty);
			AssertEquals("Should not be moved when Declaration has already been created", DeclarationQueueCodeDescriptionPairList.Codes.Classification, UPECusHAWB.Declaration.CurrentQueue.P4_CustomsQueue);
			AssertEquals("Should not be moved when Declaration has already been created", ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration, UPECusHAWB.Declaration.CurrentQueue.P4_CustomsStatus);
		}

		public void TestDeclarationQueueIsMovedWhenLinkedDependingOnCMRCustomsStatus()
		{
			AssertDeclarationQueueMovingWhenLinked(CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, DeclarationQueueCodeDescriptionPairList.Codes.Completed, string.Empty);
			AssertDeclarationQueueMovingWhenLinked(CMRConsolidatedCargoStatuses.Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed, DeclarationQueueCodeDescriptionPairList.Codes.Completed, string.Empty);
			AssertDeclarationQueueMovingWhenLinked(CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus, DeclarationQueueCodeDescriptionPairList.Codes.Completed, string.Empty);
			AssertDeclarationQueueMovingWhenLinked(CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms, DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding, string.Empty);
			AssertDeclarationQueueMovingWhenLinked(CMRConsolidatedCargoStatuses.Codes.AqisseizedCargoIsSeizedByQuarantine, DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding, string.Empty);
			AssertDeclarationQueueMovingWhenLinked(CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement, DeclarationQueueCodeDescriptionPairList.Codes.BCA, ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold);
			AssertDeclarationQueueMovingWhenLinked(CMRConsolidatedCargoStatuses.Codes.TranshphrmTranshipmentCargoIsClearButIsIdentifiedAsHighRiskMovement, DeclarationQueueCodeDescriptionPairList.Codes.BCA, ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold);
		}

		public void TestReasonCodeShouldBeEmptyForQueuesWhichDoNotNeedReasonCode()
		{
			MasterBill.ChildBills.Add(UPECusHAWB);
			UPECusHAWB.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			UPECusHAWB.CS_MsgStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			AssertEquals(CargoReportQueueCodeDescriptionPairList.Codes.Pending, UPECusHAWB.CurrentQueue.P4_CustomsQueue);
			AssertEquals(string.Empty, UPECusHAWB.CurrentQueue.P4_CustomsStatus);
		}

		public void TestCusHAWBShouldBePlacedInPendingQueueIfAirCargoSent()
		{
			UPECusHAWB uPECusHAWB = GetNewCusHAWBValidForACA();
			uPECusHAWB.DutyType = DutyTypeCodeDescriptionPairList.Codes.NonDutiable;
			AssertEquals("Pre condition. Should be in intervention before sending", CargoReportQueueCodeDescriptionPairList.Codes.Intervention, uPECusHAWB.CurrentQueue.P4_CustomsQueue);
			AssertEquals("Pre condtion. Status code is Awaiting Release", ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, uPECusHAWB.CurrentQueue.P4_CustomsStatus);
			uPECusHAWB.SendAirCargoMessage();
			AssertEquals("Should be in Pending queue now", CargoReportQueueCodeDescriptionPairList.Codes.Pending, uPECusHAWB.CurrentQueue.P4_CustomsQueue);
			AssertEquals("Status code should be empty", string.Empty, uPECusHAWB.CurrentQueue.P4_CustomsStatus);
		}

		public void TestCusHAWBShouldStayInInterventionQueueIfIsIdentifiedForScreening()
		{
			UPEDataRegistry.Instance.StopPhrasesForConsigneeAccountNum = new string[] { "CNAN" };
			UPEDataRegistry.Instance.StopPhrasesForConsigneeAddress = new string[] { "CNAD" };
			UPEDataRegistry.Instance.StopPhrasesForConsigneeName = new string[] { "CNNM" };
			UPEDataRegistry.Instance.StopPhrasesForConsignorAccountNum = new string[] { "SHAN" };
			UPEDataRegistry.Instance.StopPhrasesForConsignorAddress = new string[] { "SHAD" };
			UPEDataRegistry.Instance.StopPhrasesForConsignorName = new string[] { "SHNM" };
			UPEDataRegistry.Instance.StopPhrasesForGoodsDescription = new string[] { "UPS Stop" };
			UPEDataRegistry.Instance.QuarantineStopPhrasesForGoodsDescription = new string[] { "UPS Quarantine" };
			UPEDataRegistry.Instance.NonDocumentScreeningValueRangesItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "20.0 - 30.0");
			UPECusHAWB uPECusHAWB = GetNewCusHAWBValidForACA();
			uPECusHAWB.CS_ShipmentTypeForBinding = ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments;
			uPECusHAWB.DutyType = DutyTypeCodeDescriptionPairList.Codes.NonDutiable;
			uPECusHAWB.CS_RX_NKGoodsCurrency = "AUD";
			AssertEquals("Pre condition. Should be in intervention before sending", CargoReportQueueCodeDescriptionPairList.Codes.Intervention, uPECusHAWB.CurrentQueue.P4_CustomsQueue);
			AssertEquals("Pre condtion. Status code is Awaiting Release", ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, uPECusHAWB.CurrentQueue.P4_CustomsStatus);
			uPECusHAWB.Level1RecordNote.Text = "BLAH";
			AssertEquals(false, uPECusHAWB.IsIdentifiedForScreening);
			uPECusHAWB.Level1Record._400000 = new _400000Line("                                                          CNAN");
			AssertEquals(true, uPECusHAWB.IsIdentifiedForScreening);
			uPECusHAWB.SendAirCargoMessage();
			AssertEquals("Should still be in Intervention queue", CargoReportQueueCodeDescriptionPairList.Codes.Intervention, uPECusHAWB.CurrentQueue.P4_CustomsQueue);
			AssertEquals("Status code should be Awaiting Release", ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, uPECusHAWB.CurrentQueue.P4_CustomsStatus);
			uPECusHAWB.Level1Record._400000 = null;
			AssertEquals(false, uPECusHAWB.IsIdentifiedForScreening);
			uPECusHAWB.CS_ConsigneeStreet = "This is CNAD ST";
			AssertEquals(true, uPECusHAWB.IsIdentifiedForScreening);
			uPECusHAWB.SendAirCargoMessage();
			AssertEquals("Should still be in Intervention queue", CargoReportQueueCodeDescriptionPairList.Codes.Intervention, uPECusHAWB.CurrentQueue.P4_CustomsQueue);
			AssertEquals("Status code should be Awaiting Release", ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, uPECusHAWB.CurrentQueue.P4_CustomsStatus);
			uPECusHAWB.CS_ConsigneeStreet = ZString.Empty;
			AssertEquals(false, uPECusHAWB.IsIdentifiedForScreening);
			uPECusHAWB.CS_ConsigneeStreet2 = "This is 2nd CNAD ST";
			AssertEquals(true, uPECusHAWB.IsIdentifiedForScreening);
			uPECusHAWB.SendAirCargoMessage();
			AssertEquals("Should still be in Intervention queue", CargoReportQueueCodeDescriptionPairList.Codes.Intervention, uPECusHAWB.CurrentQueue.P4_CustomsQueue);
			AssertEquals("Status code should be Awaiting Release", ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, uPECusHAWB.CurrentQueue.P4_CustomsStatus);
			uPECusHAWB.CS_ConsigneeStreet2 = ZString.Empty;
			AssertEquals(false, uPECusHAWB.IsIdentifiedForScreening);
			uPECusHAWB.CS_ConsigneeName = "ABC CNNM ABC";
			AssertEquals(true, uPECusHAWB.IsIdentifiedForScreening);
			uPECusHAWB.SendAirCargoMessage();
			AssertEquals("Should still be in Intervention queue", CargoReportQueueCodeDescriptionPairList.Codes.Intervention, uPECusHAWB.CurrentQueue.P4_CustomsQueue);
			AssertEquals("Status code should be Awaiting Release", ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, uPECusHAWB.CurrentQueue.P4_CustomsStatus);
			uPECusHAWB.CS_ConsigneeName = ZString.Empty;
			AssertEquals(false, uPECusHAWB.IsIdentifiedForScreening);
			uPECusHAWB.Level1Record._300000 = new _300000Line("                                                          SHAN");
			AssertEquals(true, uPECusHAWB.IsIdentifiedForScreening);
			uPECusHAWB.SendAirCargoMessage();
			AssertEquals("Should still be in Intervention queue", CargoReportQueueCodeDescriptionPairList.Codes.Intervention, uPECusHAWB.CurrentQueue.P4_CustomsQueue);
			AssertEquals("Status code should be Awaiting Release", ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, uPECusHAWB.CurrentQueue.P4_CustomsStatus);
			uPECusHAWB.Level1Record._300000 = null;
			AssertEquals(false, uPECusHAWB.IsIdentifiedForScreening);
			uPECusHAWB.CS_ConsignorStreet = "This is 1st SHAD ST";
			AssertEquals(true, uPECusHAWB.IsIdentifiedForScreening);
			uPECusHAWB.SendAirCargoMessage();
			AssertEquals("Should still be in Intervention queue", CargoReportQueueCodeDescriptionPairList.Codes.Intervention, uPECusHAWB.CurrentQueue.P4_CustomsQueue);
			AssertEquals("Status code should be Awaiting Release", ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, uPECusHAWB.CurrentQueue.P4_CustomsStatus);
			uPECusHAWB.CS_ConsignorStreet = ZString.Empty;
			AssertEquals(false, uPECusHAWB.IsIdentifiedForScreening);
			uPECusHAWB.CS_ConsignorStreet2 = "This is 2nd SHAD ST";
			AssertEquals(true, uPECusHAWB.IsIdentifiedForScreening);
			uPECusHAWB.SendAirCargoMessage();
			AssertEquals("Should still be in Intervention queue", CargoReportQueueCodeDescriptionPairList.Codes.Intervention, uPECusHAWB.CurrentQueue.P4_CustomsQueue);
			AssertEquals("Status code should be Awaiting Release", ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, uPECusHAWB.CurrentQueue.P4_CustomsStatus);
			uPECusHAWB.CS_ConsignorStreet2 = ZString.Empty;
			AssertEquals(false, uPECusHAWB.IsIdentifiedForScreening);
			uPECusHAWB.CS_ConsignorName = "ABC SHNM ABC";
			AssertEquals(true, uPECusHAWB.IsIdentifiedForScreening);
			uPECusHAWB.SendAirCargoMessage();
			AssertEquals("Should still be in Intervention queue", CargoReportQueueCodeDescriptionPairList.Codes.Intervention, uPECusHAWB.CurrentQueue.P4_CustomsQueue);
			AssertEquals("Status code should be Awaiting Release", ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, uPECusHAWB.CurrentQueue.P4_CustomsStatus);
			uPECusHAWB.CS_ConsignorName = ZString.Empty;
			AssertEquals(false, uPECusHAWB.IsIdentifiedForScreening);
			uPECusHAWB.CS_GoodsDescription = "UPS Stop";
			AssertEquals(true, uPECusHAWB.IsIdentifiedForScreening);
			uPECusHAWB.SendAirCargoMessage();
			AssertEquals("Should still be in Intervention queue", CargoReportQueueCodeDescriptionPairList.Codes.Intervention, uPECusHAWB.CurrentQueue.P4_CustomsQueue);
			AssertEquals("Status code should be Awaiting Release", ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, uPECusHAWB.CurrentQueue.P4_CustomsStatus);
			uPECusHAWB.CS_GoodsDescription = "UPS Quarantine";
			AssertEquals(false, uPECusHAWB.IsIdentifiedForScreening);
			AssertEquals(true, uPECusHAWB.IsIdentifiedForQuarantine);
			uPECusHAWB.SendAirCargoMessage();
			AssertEquals("Should still be in Intervention queue", CargoReportQueueCodeDescriptionPairList.Codes.Intervention, uPECusHAWB.CurrentQueue.P4_CustomsQueue);
			AssertEquals("Status code should be Awaiting Release", ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, uPECusHAWB.CurrentQueue.P4_CustomsStatus);
			uPECusHAWB.CS_GoodsDescription = string.Empty;
			AssertEquals(false, uPECusHAWB.IsIdentifiedForQuarantine);
			AssertEquals(false, uPECusHAWB.IsIdentifiedForScreening);
			uPECusHAWB.CS_GoodsValue = 25m;
			AssertEquals(true, uPECusHAWB.IsIdentifiedForScreening);
			uPECusHAWB.SendAirCargoMessage();
			AssertEquals("Should still be in Intervention queue", CargoReportQueueCodeDescriptionPairList.Codes.Intervention, uPECusHAWB.CurrentQueue.P4_CustomsQueue);
			AssertEquals("Status code should be Awaiting Release", ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, uPECusHAWB.CurrentQueue.P4_CustomsStatus);
		}

		public void TestCusHAWBShouldStayInInterventionQueueIfAirCargoIsNotSent()
		{
			UPECusHAWB uPECusHAWB = GetNewCusHAWBValidForACA();
			AssertEquals("Pre condition. Should be in intervention before saving", CargoReportQueueCodeDescriptionPairList.Codes.Intervention, uPECusHAWB.CurrentQueue.P4_CustomsQueue);
			AssertEquals("Pre condtion. Status code is Awaiting Release", ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, uPECusHAWB.CurrentQueue.P4_CustomsStatus);
			Factory.Save();
			AssertEquals("Should still be in Intervention queue", CargoReportQueueCodeDescriptionPairList.Codes.Intervention, uPECusHAWB.CurrentQueue.P4_CustomsQueue);
			AssertEquals("Status code should be Awaiting Release", ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, uPECusHAWB.CurrentQueue.P4_CustomsStatus);
		}

		public void TestDeclarationQueueShouldNotBeAutomaticallyCompletedIfThereAreEntries()
		{
			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				UPECusHAWB.CS_JE_CustomsFormalEntry = Factory.New(typeof(UPEJobDeclaration)).PK;
				UPECusHAWB.Declaration.CustomsEntryHeaders.AddNew();
				AssertQueueMoving(CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, CargoReportQueueCodeDescriptionPairList.Codes.Completed, string.Empty, false);
				AssertCustomsQueue(UPECusHAWB.Declaration.CurrentQueue, DeclarationQueueCodeDescriptionPairList.Codes.Compiling, string.Empty, string.Empty);
			}
		}

		public void TestDeclarationQueueIsNotCompletedWhenLinkedIfThereAreEntries()
		{
			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				UPECusHAWB.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
				UPEJobDeclaration declaration = Factory.New<UPEJobDeclaration>();
				declaration.CustomsEntryHeaders.AddNew();
				UPECusHAWB.CS_JE_CustomsFormalEntry = declaration.PK;
				AssertCustomsQueue(UPECusHAWB.Declaration.CurrentQueue, DeclarationQueueCodeDescriptionPairList.Codes.Compiling, string.Empty, string.Empty);
			}
		}

		#region Try Complete Commercial Queue on Upload
		public void TestCompleteCommercialQueueOnUpload_OnlyInInterfaceMember()
		{
			UPECusHAWBWithDummyLocalCharges localHAWB = GetNewCusHAWBForBISIUploadQueueMovingTest();
			localHAWB.SetTotalLocalCharges(0);
			localHAWB.BisiUploadDate = ZDateTime.Now;
			AssertEquals("Finance Queue should not be completed", true, localHAWB.CurrentQueue.P4_QueueName.IsEmpty);
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertEquals("Finance Queue should be completed", false, localHAWB.CurrentQueue.P4_QueueName.IsEmpty);
		}

		public void TestMoveCommercialQueueOnUpload_MoveToAlternateBrokerHold()
		{
			UPECusHAWB localHAWB = Factory.New<UPECusHAWB>();
			localHAWB.CS_JE_CustomsFormalEntry = Factory.New<UPEJobDeclaration>().PK;
			localHAWB.Declaration.JE_OH_Importer = Factory.New<OrgHeader>().PK;
			localHAWB.Declaration.Importer.SetRelatedParty(Factory.New<OrgHeader>(), RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue(localHAWB.CurrentQueue, CommercialQueueCodeDescriptionPairList.Codes.AlternateBroker, string.Empty, string.Empty);
		}

		public void TestMoveCommercialQueueOnUpload_MoveToCompletedIfHasAlternateBrokerAndCustomsIsComplete()
		{
			UPECusHAWB localHAWB = Factory.New<UPECusHAWB>();
			localHAWB.CS_JE_CustomsFormalEntry = Factory.New<UPEJobDeclaration>().PK;
			localHAWB.Declaration.JE_OH_Importer = Factory.New<OrgHeader>().PK;
			localHAWB.Declaration.Importer.SetRelatedParty(Factory.New<OrgHeader>(), RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);
			localHAWB.CurrentQueue.P4_CustomsQueue = CustomsQueueCodeDescriptionPairList.Codes.Completed;
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue(localHAWB.CurrentQueue, CommercialQueueCodeDescriptionPairList.Codes.Completed, string.Empty, string.Empty);
		}

		public void TestCompleteCommercialQueueOnUpload_NoLocalCharges()
		{
			UPECusHAWBWithDummyLocalCharges localHAWB = GetNewCusHAWBForBISIUploadQueueMovingTest();
			localHAWB.SetTotalLocalCharges(0);
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue(localHAWB.CurrentQueue, CommercialQueueCodeDescriptionPairList.Codes.Completed, string.Empty, string.Empty);
		}

		public void TestCompleteCommercialQueueOnUpload_FreeDomicile()
		{
			UPECusHAWBWithDummyLocalCharges localHAWB = GetNewCusHAWBForBISIUploadQueueMovingTest();
			localHAWB.SetTotalLocalCharges(10000);
			localHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreeDomicile;
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue(localHAWB.CurrentQueue, CommercialQueueCodeDescriptionPairList.Codes.Completed, string.Empty, string.Empty);
			localHAWB.CurrentQueue.P4_QueueName = string.Empty;
			localHAWB.IsAbandoned = true;
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue("Should be moved to the Rebill queue when any of the rebill flags is set.", localHAWB.CurrentQueue, CommercialQueueCodeDescriptionPairList.Codes.Rebill, string.Empty, string.Empty);
			localHAWB.CurrentQueue.P4_QueueName = string.Empty;
			localHAWB.IsRTS = true;
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue("Should be moved to the Rebill queue when any of the rebill flags is set.", localHAWB.CurrentQueue, CommercialQueueCodeDescriptionPairList.Codes.Rebill, string.Empty, string.Empty);
			localHAWB.CurrentQueue.P4_QueueName = string.Empty;
			localHAWB.IsFreeDomicile = true;
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue("Should be moved to the Rebill queue when any of the rebill flags is set.", localHAWB.CurrentQueue, CommercialQueueCodeDescriptionPairList.Codes.Rebill, string.Empty, string.Empty);
			localHAWB.CurrentQueue.P4_QueueName = string.Empty;
			localHAWB.IsTranshipment = true;
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue("Should be moved to the Rebill queue when any of the rebill flags is set.", localHAWB.CurrentQueue, CommercialQueueCodeDescriptionPairList.Codes.Rebill, string.Empty, string.Empty);
			localHAWB.CurrentQueue.P4_QueueName = string.Empty;
			localHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreightCollect;
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue("Should not be completed. Billing terms is not Free Domicile.", localHAWB.CurrentQueue, string.Empty, string.Empty, string.Empty);
		}

		public void TestCompleteCommercialQueueOnUpload_ThirdPartyIndicator()
		{
			UPECusHAWBWithDummyLocalCharges localHAWB = GetNewCusHAWBForBISIUploadQueueMovingTest();
			localHAWB.SetTotalLocalCharges(10000);
			localHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreightCollect;
			localHAWB.Level1Record = CreateLevel1RecordWithDummyThirdPartyIndicator("5");
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue(localHAWB.CurrentQueue, CommercialQueueCodeDescriptionPairList.Codes.Completed, string.Empty, string.Empty);
			localHAWB.CurrentQueue.P4_QueueName = string.Empty;
			localHAWB.Level1Record = CreateLevel1RecordWithDummyThirdPartyIndicator("8");
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue(localHAWB.CurrentQueue, CommercialQueueCodeDescriptionPairList.Codes.Completed, string.Empty, string.Empty);
			localHAWB.CurrentQueue.P4_QueueName = string.Empty;
			localHAWB.Level1Record = CreateLevel1RecordWithDummyThirdPartyIndicator("7");
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue("Only shipments with third party indicator 5 or 8 can be completed on upload", localHAWB.CurrentQueue, string.Empty, string.Empty, string.Empty);
		}

		public void TestCompleteCommercialQueueOnUpload_AlternateBroker()
		{
			UPECusHAWBWithDummyLocalCharges localHAWB = GetNewCusHAWBForBISIUploadQueueMovingTest();
			localHAWB.SetTotalLocalCharges(10);
			localHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreightCollect;
			localHAWB.Level1Record = null;
			localHAWB.CS_JE_CustomsFormalEntry = Factory.New(typeof(UPEJobDeclaration)).PK;
			localHAWB.Declaration.JE_OH_Importer = Factory.New(typeof(UPEOrgHeader)).PK;
			localHAWB.Declaration.Importer.SetRelatedParty(Factory.New<UPEOrgHeader>(), RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);
			AssertEquals("Sanity check", true, localHAWB.HasAlternateBroker);
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue("Should not complete commercial queue on upload if the shipment is handled by Alternate Broker", localHAWB.CurrentQueue, CommercialQueueCodeDescriptionPairList.Codes.AlternateBroker, string.Empty, string.Empty);
		}

		public void TestCompleteCommercialQueueOnUpload_PreferredAccountImporter()
		{
			UPECusHAWBWithDummyLocalCharges localHAWB = GetNewCusHAWBForBISIUploadQueueMovingTest();
			localHAWB.SetTotalLocalCharges(10);
			localHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreightCollect;
			localHAWB.Level1Record = null;
			localHAWB.CS_JE_CustomsFormalEntry = Factory.New(typeof(UPEJobDeclaration)).PK;
			AssertEquals("Sanity check", false, localHAWB.HasAlternateBroker);
			localHAWB.Declaration.JE_OH_Importer = CreateOrgHeaderWithDummyAccountClass("2").PK;
			AssertEquals("Sanity check", true, localHAWB.Declaration.Importer.IsPreferredAccount);
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue("Should be completed if the Importer is a Preferred Account", localHAWB.CurrentQueue, CommercialQueueCodeDescriptionPairList.Codes.Completed, string.Empty, string.Empty);
		}

		public void TestCompleteCommercialQueueOnUpload_TotalLocalChargesAboveOrEqualCODConfirmPayment()
		{
			UPEDataRegistry.Instance.CODConfirmPaymentThreshold = 100;
			UPECusHAWBWithDummyLocalCharges localHAWB = GetNewCusHAWBForBISIUploadQueueMovingTest();
			localHAWB.SetTotalLocalCharges(100);
			localHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreightCollect;
			localHAWB.Level1Record = null;
			localHAWB.CS_JE_CustomsFormalEntry = Factory.New(typeof(UPEJobDeclaration)).PK;
			localHAWB.Declaration.JE_OH_Importer = CreateOrgHeaderWithDummyAccountClass("1").PK;
			AssertEquals("Sanity check", false, localHAWB.HasAlternateBroker);
			AssertEquals("Sanity check", true, localHAWB.Declaration.Importer.IsStandardAccount);
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue("Should not be moved, total local charges is equal to the COD Confirm Payment threshold", localHAWB.CurrentQueue, string.Empty, string.Empty, string.Empty);
			localHAWB.SetTotalLocalCharges(200);
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue("Should not be moved, total local charges is above the COD Confirm Payment threshold", localHAWB.CurrentQueue, string.Empty, string.Empty, string.Empty);
		}

		public void TestCompleteCommercialQueueOnUpload_TotalLocalChargesBelowCODConfirmPayment_StandardAccountImporter()
		{
			UPEDataRegistry.Instance.CODConfirmPaymentThreshold = 100;
			UPECusHAWBWithDummyLocalCharges localHAWB = GetNewCusHAWBForBISIUploadQueueMovingTest();
			localHAWB.SetTotalLocalCharges(10);
			localHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreightCollect;
			localHAWB.Level1Record = null;
			localHAWB.CS_JE_CustomsFormalEntry = Factory.New(typeof(UPEJobDeclaration)).PK;
			AssertEquals("Sanity check", false, localHAWB.HasAlternateBroker);
			localHAWB.Declaration.JE_OH_Importer = CreateOrgHeaderWithDummyAccountClass("1").PK;
			AssertEquals("Sanity check", true, localHAWB.Declaration.Importer.IsStandardAccount);
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			string errorMessage = "Should be completed if the Importer is a Standard Account and Total Local Charges is below COD Confirm Payment threshold";
			AssertCommercialQueue(errorMessage, localHAWB.CurrentQueue, CommercialQueueCodeDescriptionPairList.Codes.Completed, string.Empty, string.Empty);
		}

		public void TestCompleteCommercialQueueOnUpload_TotalLocalChargesBelowCODConfirmPayment_CreditCardOrOnFileAccountImporter()
		{
			UPEDataRegistry.Instance.CODConfirmPaymentThreshold = 100;
			UPECusHAWBWithDummyLocalCharges localHAWB = GetNewCusHAWBForBISIUploadQueueMovingTest();
			localHAWB.SetTotalLocalCharges(10);
			localHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreightCollect;
			localHAWB.Level1Record = null;
			localHAWB.CS_JE_CustomsFormalEntry = Factory.New(typeof(UPEJobDeclaration)).PK;
			AssertEquals("Sanity check", false, localHAWB.HasAlternateBroker);
			localHAWB.Declaration.JE_OH_Importer = CreateOrgHeaderWithDummyAccountClass("5").PK;
			AssertEquals("Sanity check", true, localHAWB.Declaration.Importer.IsCreditCardAccount);
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			string errorMessage = "Should be moved to OnFile if the Importer is a Credit Card/On File Account and Total Local Charges is below COD Confirm Payment threshold";
			AssertCommercialQueue(errorMessage, localHAWB.CurrentQueue, CommercialQueueCodeDescriptionPairList.Codes.OnFile, string.Empty, string.Empty);
			localHAWB.CurrentQueue.P4_QueueName = string.Empty;
			localHAWB.Declaration.JE_OH_Importer = CreateOrgHeaderWithDummyAccountClass("14").PK;
			AssertEquals("Sanity check", true, localHAWB.Declaration.Importer.IsOnFileCODAccount);
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue(errorMessage, localHAWB.CurrentQueue, CommercialQueueCodeDescriptionPairList.Codes.OnFile, string.Empty, string.Empty);
		}

		public void TestCompleteCommercialQueueOnUpload_TotalLocalChargesBelowCODConfirmPaymentAndCODAutoReleaseThreshold()
		{
			UPEDataRegistry.Instance.CODConfirmPaymentThreshold = 100;
			UPEDataRegistry.Instance.CODAutoReleaseAtUploadThreshold = 20;
			UPEDataRegistry.Instance.CODAutoReleaseAndChaseThreshold = 24;
			UPECusHAWBWithDummyLocalCharges localHAWB = GetNewCusHAWBForBISIUploadQueueMovingTest();
			localHAWB.SetTotalLocalCharges(25);
			localHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreightCollect;
			localHAWB.Level1Record = null;
			localHAWB.CS_JE_CustomsFormalEntry = Factory.New(typeof(UPEJobDeclaration)).PK;
			AssertEquals("Sanity check", false, localHAWB.HasAlternateBroker);
			localHAWB.Declaration.JE_OH_Importer = CreateOrgHeaderWithDummyAccountClass("10").PK;
			AssertEquals("Sanity check", true, localHAWB.Declaration.Importer.IsCODAccount);
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue(localHAWB.CurrentQueue, string.Empty, string.Empty, string.Empty);
			localHAWB.SetTotalLocalCharges(20);
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue("COD Debtor Group but total charges is below/equal to auto release threshold. Should be completed", localHAWB.CurrentQueue, CommercialQueueCodeDescriptionPairList.Codes.Completed, string.Empty, string.Empty);
			localHAWB.CurrentQueue.P4_QueueName = string.Empty;
			localHAWB.SetTotalLocalCharges(19);
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue("COD Debtor Group but total charges is below/equal to auto release threshold. Should be completed", localHAWB.CurrentQueue, CommercialQueueCodeDescriptionPairList.Codes.Completed, string.Empty, string.Empty);
		}

		public void TestCompleteCommercialQueueOnUpload_TotalLocalChargesBelowCODConfirmPaymentAndCODAutoReleaseAndChaseThreshold()
		{
			UPEDataRegistry.Instance.CODConfirmPaymentThreshold = 100;
			UPEDataRegistry.Instance.CODAutoReleaseAtUploadThreshold = 20;
			UPEDataRegistry.Instance.CODAutoReleaseAndChaseThreshold = 24;
			UPECusHAWBWithDummyLocalCharges localHAWB = GetNewCusHAWBForBISIUploadQueueMovingTest();
			localHAWB.SetTotalLocalCharges(25);
			localHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreightCollect;
			localHAWB.Level1Record = null;
			localHAWB.CS_JE_CustomsFormalEntry = Factory.New(typeof(UPEJobDeclaration)).PK;
			AssertEquals("Sanity check", false, localHAWB.HasAlternateBroker);
			localHAWB.Declaration.JE_OH_Importer = CreateOrgHeaderWithDummyAccountClass("10").PK;
			AssertEquals("Sanity check", true, localHAWB.Declaration.Importer.IsCODAccount);
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue(localHAWB.CurrentQueue, string.Empty, string.Empty, string.Empty);
			localHAWB.SetTotalLocalCharges(24);
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue("Above auto release at upload but below Chase Threshold. Should be completed", localHAWB.CurrentQueue, CommercialQueueCodeDescriptionPairList.Codes.Chase, string.Empty, string.Empty);
			localHAWB.CurrentQueue.P4_QueueName = string.Empty;
			localHAWB.SetTotalLocalCharges(21);
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertCommercialQueue("Above auto release at upload but below Chase Threshold. Should be completed", localHAWB.CurrentQueue, CommercialQueueCodeDescriptionPairList.Codes.Chase, string.Empty, string.Empty);
		}

		public void TestDontCreateHeldForPaymentLogToSendToBISI_WhenNotCompleteCommercialQueueOnUploadAndIsAlternateBroker()
		{
			UPECusHAWBWithDummyLocalCharges localHAWB = GetNewCusHAWBForBISIUploadQueueMovingTest();
			localHAWB.SetTotalLocalCharges(1000);
			localHAWB.CS_JE_CustomsFormalEntry = Factory.New<UPEJobDeclaration>().PK;
			localHAWB.Declaration.JE_OH_Importer = Factory.New<OrgHeader>().PK;
			localHAWB.Declaration.Importer.SetRelatedParty(Factory.New<OrgHeader>(), RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);
			AssertEquals("PreCondition", true, localHAWB.HasAlternateBroker);
			((IBisiUpload)localHAWB).OnBeforeBisiUpload();
			AssertEquals(0, localHAWB.CurrentQueue.CommercialQueueLogs.Count);
		}

		public void TestSpecialQueue_MessageNotTruncatedAndWithCorrectEventTime()
		{
			using (DisposableEnvironment.ForBranch(MelBranch.GB_Code))
			{
				UPEDataRegistry.Instance.CODConfirmPaymentThreshold = 100;
				UPEDataRegistry.Instance.CODAutoReleaseAtUploadThreshold = 20;
				UPEDataRegistry.Instance.CODAutoReleaseAndChaseThreshold = 24;
				UPECusHAWBWithDummyLocalCharges localHAWB = GetNewCusHAWBForBISIUploadQueueMovingTest();
				var localMAWB = Factory.NewWithValidTestData<CusMAWB>(TestBusinessObjectKind.MinimumRequiredToSave);
				localMAWB.CM_GB = PerBranch.PK;
				localHAWB.CS_CM = localMAWB.PK;
				ZDateTime melbourneTime = ZDateTime.Empty;
				localHAWB.MAWB.CM_GB = PerBranch.PK;
				localHAWB.SetTotalLocalCharges(25);
				localHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreightCollect;
				localHAWB.Level1Record = null;
				localHAWB.CS_JE_CustomsFormalEntry = Factory.New(typeof(UPEJobDeclaration)).PK;
				AssertEquals("Sanity check", false, localHAWB.HasAlternateBroker);
				melbourneTime = ZDateTime.Now;
				((IBisiUpload)localHAWB).OnBeforeBisiUpload();
				Factory.Save();
				UPECusHAWB loadedCustHAWB = Factory.Load<UPECusHAWB>(localHAWB.PK);
				AssertCommercialQueue(localHAWB.CurrentQueue, string.Empty, string.Empty, string.Empty);
				AssertEquals(1, localHAWB.CurrentQueue.CommercialQueueLogs.Count);
				AssertEquals("SL_Reference should not be truncated", "COM\"\",\"OQ\",\"\",\"auto upload Local Charges below COD Threshold\",\"\"", loadedCustHAWB.CurrentQueue.CommercialQueueLogs[0].SL_Reference);
				AssertGreaterThan("SL_EventTime should not be melboune time; it should be in Perth time zone", melbourneTime, loadedCustHAWB.CurrentQueue.CommercialQueueLogs[0].SL_EventTime);
			}
		}

		UPEOrgHeader CreateOrgHeaderWithDummyAccountClass(string accountClass)
		{
			UPEOrgHeader result = Factory.New<UPEOrgHeader>();
			result.CompanyData.OB_OJ_ARDebtorGroup = Factory.New(typeof(OrgDebtorGroup)).PK;
			result.CompanyData.ARDebtorGroup.OJ_Code = accountClass;
			return result;
		}

		UPECusHAWBWithDummyLocalCharges GetNewCusHAWBForBISIUploadQueueMovingTest()
		{
			UPECusHAWBWithDummyLocalCharges result = Factory.New<UPECusHAWBWithDummyLocalCharges>();
			result.CurrentQueue.P4_QueueName = string.Empty;
			result.CurrentQueue.P4_Status = string.Empty;
			result.CurrentQueue.P4_SubStatus = string.Empty;
			return result;
		}

		#endregion
		#region Queue movement based on new inbound CARST message
		public void TestCusHAWBShouldBeMovedToQuarantineQueueWhenKeywordsMatched()
		{
			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				AssertCusHAWBQuarantineQueueMovement("QUARANTINE ACTION HOLD", "YES");
				AssertCusHAWBQuarantineQueueMovement("DOCO ASSESSMENT - GOODS INSPECT REQUIRED", "YES");
				AssertCusHAWBQuarantineQueueMovement("PENDING QUARANTINE ACTION", "YES");
				AssertCusHAWBQuarantineQueueMovement("CONDITIONAL RELEASE", "YES");
				AssertCusHAWBQuarantineQueueMovement("INSPECTION", "YES");
				AssertCusHAWBQuarantineQueueMovement(string.Empty, "NO");
			}
		}

		public void TestCusHAWBShouldBeMovedToShipperConsigneeDetailsHoldWhenKeywordsMatched()
		{
			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				AssertCusHAWBShipperConsigneeDetailsHoldQueueMovement("DEFICIENT CONSIGNEE ADDRESS");
				AssertCusHAWBShipperConsigneeDetailsHoldQueueMovement("DEFICIENT CONSIGNEE NAME");
				AssertCusHAWBShipperConsigneeDetailsHoldQueueMovement("DEFICIENT CONSIGNOR ADDRESS");
				AssertCusHAWBShipperConsigneeDetailsHoldQueueMovement("DEFICIENT CONSIGNOR NAME");
				AssertCusHAWBShipperConsigneeDetailsHoldQueueMovement("FULL CONSIGNEE NAME AND ADDRESS DETAILS ARE REQUIRED");
				AssertCusHAWBShipperConsigneeDetailsHoldQueueMovement("FULL CONSIGNOR NAME AND ADDRESS DETAILS ARE REQUIRED");
			}
		}

		public void TestCusHAWBShouldBeMovedToUnknownQueueWhenThereIsNoMatchedKeywordsInFTXSegments()
		{
			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				AssertCusHAWBQueueMovementBasedOnNewCARSTMessage("No Matched keywords. Should be moved to the Unknown queue", CARSTMessageWithHoldConsolidatedStatusButNoMatchedKeywords, CargoReportQueueCodeDescriptionPairList.Codes.Unknown, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease);
			}
		}

		public void TestCusHAWBShouldBeMovedOnlyBasedOnTheLastCARSTMessage()
		{
			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				AssertCusHAWBQuarantineQueueMovement("QUARANTINE ACTION HOLD", "YES");
				string failureMessage = "Should still be in the Quarantine queue as the newly added message is not the last CARST message";
				AssertCusHAWBQueueMovementBasedOnNewCARSTMessage(failureMessage, CARSTMessageWithHoldConsolidatedStatusButNoMatchedKeywords, ZDateTime.Now.AddYears(-1), CargoReportQueueCodeDescriptionPairList.Codes.Quarantine, ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold, StatusCodeDescriptionPairList.EmptyStatus);
			}
		}

		public void TestCusHAWBShouldBeMovedOnlyBasedOnTheLastCARSTMessage_CompareByMessageNumWhenMessagesReceivedAtSameTime()
		{
			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				ZDateTime commonDateTime = ZDateTime.Now;
				AssertCusHAWBQuarantineQueueMovement("QUARANTINE ACTION HOLD", "YES", "2", commonDateTime);
				string failureMessage = "Should still be in the Quarantine queue as the newly added message is not the last CARST message, by message number (the SystemCreateTimes are the same)";
				AssertCusHAWBQueueMovementBasedOnNewCARSTMessage(failureMessage, CARSTMessageWithHoldConsolidatedStatusButNoMatchedKeywords, "1", commonDateTime, CargoReportQueueCodeDescriptionPairList.Codes.Quarantine, ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold, StatusCodeDescriptionPairList.EmptyStatus);
			}
		}

		public void TestCusHAWBShouldBeMovedWhenNewFreeTextSegmentKeywordsMatched()
		{
			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				CusHAWBAutoQueueMovementCollection defaultValue = UPEDataRegistry.Instance.CusHAWBAutoQueueMovementRegistryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				try
				{
					CusHAWBAutoQueueMovementCollection collection = new CusHAWBAutoQueueMovementCollection();
					collection.Add(CreateCusHAWBAutoQueueMovement("MEH", "HIGH PRIORITY", CargoReportQueueCodeDescriptionPairList.Codes.EIR, ReasonCodeDescriptionPairList.Codes.NY_MissingInvoice, StatusCodeDescriptionPairList.Codes.BQ_NoAnswer, 1));
					UPEDataRegistry.Instance.CusHAWBAutoQueueMovementRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
					string messageText = ConstructCARSTMessageForShipperConsigneeDetailsHoldQueueMovementTest("DEFICIENT CONSIGNEE ADDRESS");
					AssertCusHAWBQueueMovementBasedOnNewCARSTMessage("ShipperConsigneeDetailsHold queue movement is not set in the registry. Should not be moved to ShipperConsigneeDetailsHold queue.", messageText, CargoReportQueueCodeDescriptionPairList.Codes.Unknown, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease);
					messageText = ConstructCARSTMessage("FTX+AHN+++MEH:HIGH PRIORITY'");
					AssertCusHAWBQueueMovementBasedOnNewCARSTMessage("ShipperConsigneeDetailsHold queue movement is not set in the registry. Should not be moved to ShipperConsigneeDetailsHold queue.", messageText, CargoReportQueueCodeDescriptionPairList.Codes.EIR, ReasonCodeDescriptionPairList.Codes.NY_MissingInvoice, StatusCodeDescriptionPairList.Codes.BQ_NoAnswer);
				}
				finally
				{
					UPEDataRegistry.Instance.CusHAWBAutoQueueMovementRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue);
				}
			}
		}

		public void TestHighPriorityQueueMovementIsPerformedFirst()
		{
			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				CusHAWBAutoQueueMovementCollection defaultValue = UPEDataRegistry.Instance.CusHAWBAutoQueueMovementRegistryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				try
				{
					CusHAWBAutoQueueMovementCollection collection = new CusHAWBAutoQueueMovementCollection();
					CusHAWBAutoQueueMovement highPriority = CreateCusHAWBAutoQueueMovement("MEH", "HIGH PRIORITY", CargoReportQueueCodeDescriptionPairList.Codes.EIR, ReasonCodeDescriptionPairList.Codes.NY_MissingInvoice, StatusCodeDescriptionPairList.Codes.BQ_NoAnswer, 7);
					collection.Add(highPriority);
					CusHAWBAutoQueueMovement medPriority = CreateCusHAWBAutoQueueMovement("MEHMEH", "MEDIUM PRIORITY", CargoReportQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient, StatusCodeDescriptionPairList.Codes.BR_MessageLeft, 15);
					collection.Add(medPriority);
					CusHAWBAutoQueueMovement lowPriority = CreateCusHAWBAutoQueueMovement("HAHA", "LOW PRIORITY", CargoReportQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.BA_InadequateDescription, StatusCodeDescriptionPairList.Codes.KO_ReceiverContacted, 21);
					collection.Add(lowPriority);
					CusHAWBAutoQueueMovement sLowPriority = CreateCusHAWBAutoQueueMovement("MEH", "SUPER LOW PRIORITY", CargoReportQueueCodeDescriptionPairList.Codes.Pending, string.Empty, string.Empty, 22);
					collection.Add(sLowPriority);
					UPEDataRegistry.Instance.CusHAWBAutoQueueMovementRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
					string messageText = ConstructCARSTMessage("FTX+AHN+++MEH:HIGH PRIORITY'FTX+AHN+++MEHMEH:MEDIUM PRIORITY'FTX+AHN+++HAHA:LOW PRIORITY'FTX+AHN+++MEH:SUPER LOW PRIORITY'");
					AssertCusHAWBQueueMovementBasedOnNewCARSTMessage("Higher priority queue movement should be performed first", messageText, CargoReportQueueCodeDescriptionPairList.Codes.EIR, ReasonCodeDescriptionPairList.Codes.NY_MissingInvoice, StatusCodeDescriptionPairList.Codes.BQ_NoAnswer);
					collection.Remove(highPriority);
					UPEDataRegistry.Instance.CusHAWBAutoQueueMovementRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
					AssertCusHAWBQueueMovementBasedOnNewCARSTMessage("Higher priority queue movement should be performed first", messageText, CargoReportQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient, StatusCodeDescriptionPairList.Codes.BR_MessageLeft);
					collection.Remove(medPriority);
					UPEDataRegistry.Instance.CusHAWBAutoQueueMovementRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
					AssertCusHAWBQueueMovementBasedOnNewCARSTMessage("Higher priority queue movement should be performed first", messageText, CargoReportQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.BA_InadequateDescription, StatusCodeDescriptionPairList.Codes.KO_ReceiverContacted);
					collection.Remove(lowPriority);
					UPEDataRegistry.Instance.CusHAWBAutoQueueMovementRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
					AssertCusHAWBQueueMovementBasedOnNewCARSTMessage("Higher priority queue movement should be performed first", messageText, CargoReportQueueCodeDescriptionPairList.Codes.Pending, string.Empty, string.Empty);
					collection.Remove(sLowPriority);
					UPEDataRegistry.Instance.CusHAWBAutoQueueMovementRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
					AssertCusHAWBQueueMovementBasedOnNewCARSTMessage("Nothing is matched. Should be moved to the unknown queue.", messageText, CargoReportQueueCodeDescriptionPairList.Codes.Unknown, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, string.Empty);
				}
				finally
				{
					UPEDataRegistry.Instance.CusHAWBAutoQueueMovementRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue);
				}
			}
		}

		void AssertCusHAWBQuarantineQueueMovement(string aCSAQISImpedimentDetails, string aQISCargoReportEvaluationComplete)
		{
			AssertCusHAWBQuarantineQueueMovement(aCSAQISImpedimentDetails, aQISCargoReportEvaluationComplete, "1", ZDateTime.Now);
		}

		void AssertCusHAWBQuarantineQueueMovement(string aCSAQISImpedimentDetails, string aQISCargoReportEvaluationComplete, ZString messageNum, ZDateTime systemCreateTime)
		{
			string messageText = ConstructCARSTMessageForQuarantineQueueMovementTest(aCSAQISImpedimentDetails, aQISCargoReportEvaluationComplete);
			AssertCusHAWBQueueMovementBasedOnNewCARSTMessage("Moved to the wrong queue", messageText, messageNum, systemCreateTime, CargoReportQueueCodeDescriptionPairList.Codes.Quarantine, ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold, StatusCodeDescriptionPairList.EmptyStatus);
		}

		void AssertCusHAWBShipperConsigneeDetailsHoldQueueMovement(string aCSAQISImpedimentDetails)
		{
			string messageText = ConstructCARSTMessageForShipperConsigneeDetailsHoldQueueMovementTest(aCSAQISImpedimentDetails);
			AssertCusHAWBQueueMovementBasedOnNewCARSTMessage("Moved to the wrong queue", messageText, CargoReportQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient, StatusCodeDescriptionPairList.Codes.TF_IncorrectAddressOrNeedPhoneNumber);
		}

		void AssertCusHAWBQueueMovementBasedOnNewCARSTMessage(string failureMessage, ZString messageText, ZString expectedQueue, ZString expectedReasonCode)
		{
			AssertCusHAWBQueueMovementBasedOnNewCARSTMessage(failureMessage, messageText, ZDateTime.Now, expectedQueue, expectedReasonCode, string.Empty);
		}

		void AssertCusHAWBQueueMovementBasedOnNewCARSTMessage(string failureMessage, ZString messageText, ZString expectedQueue, ZString expectedReasonCode, ZString expectedStatusCode)
		{
			AssertCusHAWBQueueMovementBasedOnNewCARSTMessage(failureMessage, messageText, ZDateTime.Now, expectedQueue, expectedReasonCode, expectedStatusCode);
		}

		void AssertCusHAWBQueueMovementBasedOnNewCARSTMessage(string failureMessage, string messageText, ZDateTime systemCreateTime, ZString expectedQueue, ZString expectedReasonCode, ZString expectedStatusCode)
		{
			AssertCusHAWBQueueMovementBasedOnNewCARSTMessage(failureMessage, messageText, "1", systemCreateTime, expectedQueue, expectedReasonCode, expectedStatusCode);
		}

		void AssertCusHAWBQueueMovementBasedOnNewCARSTMessage(string failureMessage, string messageText, ZString messageNum, ZDateTime systemCreateTime, ZString expectedQueue, ZString expectedReasonCode, ZString expectedStatusCode)
		{
			CMRCARSTMessage message = (CMRCARSTMessage)UPECusHAWB.Messages.AddNew(typeof(CMRCARSTMessage));
			message.EM_MessageText = messageText;
			message.EM_SystemCreateTimeUtc = systemCreateTime;
			message.EM_MessageNum = messageNum;
			UPECusHAWB.CurrentQueue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.Pending;
			UPECusHAWB.CurrentQueue.P4_CustomsStatus = string.Empty;
			Factory.Save();
			AssertEquals(failureMessage, expectedQueue, UPECusHAWB.CurrentQueue.P4_CustomsQueue);
			AssertEquals("Incorrect reason code", expectedReasonCode, UPECusHAWB.CurrentQueue.P4_CustomsStatus);
			AssertEquals("Incorrect status code", expectedStatusCode, UPECusHAWB.CurrentQueue.P4_CustomsSubStatus);
		}

		CusHAWBAutoQueueMovement CreateCusHAWBAutoQueueMovement(ZString segmentName, ZString segmentValue, ZString queueName, ZString reasonCode, ZString statusCode, ZInt priority)
		{
			CusHAWBAutoQueueMovement result = new CusHAWBAutoQueueMovement();
			result.FreeTextSegmentName = segmentName;
			result.FreeTextSegmentValue = segmentValue;
			result.Queue.QueueName = queueName;
			result.Queue.Status = reasonCode;
			result.Queue.SubStatus = statusCode;
			result.Priority = priority;
			return result;
		}

		#region CARST Message Text
		string ConstructCARSTMessageForQuarantineQueueMovementTest(string aCSAQISImpedimentDetails, string aQISCargoReportEvaluationComplete)
		{
			return string.Format(CARSTMessageForQueueMovementTest, aCSAQISImpedimentDetails, aQISCargoReportEvaluationComplete);
		}

		string ConstructCARSTMessageForShipperConsigneeDetailsHoldQueueMovementTest(string aCSAQISImpedimentDetails)
		{
			return string.Format(CARSTMessageForQueueMovementTest, aCSAQISImpedimentDetails, "YES");
		}

		string ConstructCARSTMessage(string freeTextSegments)
		{
			return string.Format(CARSTMessageForQueueMovementTestWithFreeText, freeTextSegments);
		}

		const string CARSTMessageForQueueMovementTest = "UNH+000001+CUSRES:D:99B:UN'" + "BGM+34:::CARST+2I9C 2C3F C215:1+8'" + "DTM+9:20051013090045839311:ZZZ'" + "DTM+132:20051013:102'" + "FTX+AHN+++CONSOLIDATED STATUS:HELD'" + "FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'" + "FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'" + "FTX+AHN+++IAR ACS CLEARED:YES'" + "FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'" + "FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'" + "FTX+AHN+++DECONSOLIDATION UNDERBOND SATISFIED:NO'" + "FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'" + "FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'" + "FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'" + "FTX+AHN+++IAR QUARANTINE CLEARED:YES'" + "FTX+AHN+++CARGO REPORT QUARANTINE EVALUATED:NO'" + "FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'" + "FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'" + "FTX+AHN+++IMPORT DECLARATION QUARANTINE EVALUATED:N/A'" + "FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'" + "FTX+AHN+++ACS/QUARANTINE IMPEDIMENT DETAILS:{0}'" + "FTX+AHN+++ACS EVALUATION COMPLETE:YES'" + "FTX+AHN+++QUARANTINE CARGO REPORT EVALUATION COMPLETE:{1}'" + "FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'" + "FTX+AHN+++QUARANTINE IMPORT DECLARATION EVALUATION COMPLETE:N/A'" + "FTX+AHN+++IMPORT DECLARATION PAID:N/A'" + "FTX+AHN+++CARGO REPORT SAC:YES'" + "TDT+20+108++6+QF::3'" + "LOC+12+AUSYD::6'" + "LOC+4+9532M::95'" + "NAD+MR+FGH939C::95'" + "NAD+UD+83003926181::95'" + "RFF+ABO:A00046918/PRD1::1'" + "RFF+MWB:08143619354'";
		const string CARSTMessageWithHoldConsolidatedStatusButNoMatchedKeywords = "UNH+000001+CUSRES:D:99B:UN'" + "BGM+34:::CARST+AHJI 94C2 15:1+8'" + "DTM+9:20051013235107085958:ZZZ'" + "DTM+132:20051014:102'" + "FTX+AHN+++CONSOLIDATED STATUS:HELD'" + "FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'" + "FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'" + "FTX+AHN+++IAR ACS CLEARED:YES'" + "FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:NO'" + "FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'" + "FTX+AHN+++DECONSOLIDATION UNDERBOND SATISFIED:NO'" + "FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'" + "FTX+AHN+++RELEASE PREMISE IN DESTINATION:NO'" + "FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'" + "FTX+AHN+++IAR QUARANTINE CLEARED:YES'" + "FTX+AHN+++CARGO REPORT QUARANTINE EVALUATED:YES'" + "FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'" + "FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'" + "FTX+AHN+++IMPORT DECLARATION QUARANTINE EVALUATED:N/A'" + "FTX+AHN+++ACS EVALUATION COMPLETE:YES'" + "FTX+AHN+++QUARANTINE CARGO REPORT EVALUATION COMPLETE:YES'" + "FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'" + "FTX+AHN+++QUARANTINE IMPORT DECLARATION EVALUATION COMPLETE:N/A'" + "FTX+AHN+++IMPORT DECLARATION PAID:N/A'" + "FTX+AHN+++CARGO REPORT SAC:YES'" + "TDT+20+135++6+CX::3'" + "LOC+12+AUMEL::6'" + "LOC+4+DJ72A::95'" + "NAD+MR+FGH939C::95'" + "NAD+UD+83003926181::95'" + "RFF+ABO:A00052605/PRD1::1'" + "RFF+MWB:16039947810'" + "RFF+HWB:M1658933517'" + "UNT+34+000001'" + "UNZ+1+00000000037838'";
		const string CARSTMessageForQueueMovementTestWithFreeText = "UNH+000001+CUSRES:D:99B:UN'" + "BGM+34:::CARST+2I9C 2C3F C215:1+8'" + "DTM+9:20051013090045839311:ZZZ'" + "DTM+132:20051013:102'" + "FTX+AHN+++CONSOLIDATED STATUS:HELD'" + "FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'" + "FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'" + "FTX+AHN+++IAR ACS CLEARED:YES'" + "FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'" + "FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'" + "FTX+AHN+++DECONSOLIDATION UNDERBOND SATISFIED:NO'" + "FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'" + "FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'" + "FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'" + "FTX+AHN+++IAR QUARANTINE CLEARED:YES'" + "FTX+AHN+++CARGO REPORT QUARANTINE EVALUATED:NO'" + "FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'" + "FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'" + "FTX+AHN+++IMPORT DECLARATION QUARANTINE EVALUATED:N/A'" + "FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'{0}" + "FTX+AHN+++ACS EVALUATION COMPLETE:YES'" + "FTX+AHN+++QUARANTINE CARGO REPORT EVALUATION COMPLETE:YES'" + "FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'" + "FTX+AHN+++QUARANTINE IMPORT DECLARATION EVALUATION COMPLETE:N/A'" + "FTX+AHN+++IMPORT DECLARATION PAID:N/A'" + "FTX+AHN+++CARGO REPORT SAC:YES'" + "TDT+20+108++6+QF::3'" + "LOC+12+AUSYD::6'" + "LOC+4+9532M::95'" + "NAD+MR+FGH939C::95'" + "NAD+UD+83003926181::95'" + "RFF+ABO:A00046918/PRD1::1'" + "RFF+MWB:08143619354'";
		#endregion
		#endregion
		void AssertCommercialQueue(string errorMessage, ProcessQueue queue, ZString expectedQueueName, ZString expectedReasonCode, ZString expectedStatusCode)
		{
			AssertEquals("Has been moved to the wrong Queue." + errorMessage, expectedQueueName, queue.P4_QueueName);
			AssertEquals("Queue has been moved with the wrong reason code." + errorMessage, expectedReasonCode, queue.P4_Status);
			AssertEquals("Queue has been moved with the wrong status code." + errorMessage, expectedStatusCode, queue.P4_SubStatus);
		}

		void AssertCommercialQueue(ProcessQueue queue, ZString expectedQueueName, ZString expectedReasonCode, ZString expectedStatusCode)
		{
			AssertCommercialQueue(string.Empty, queue, expectedQueueName, expectedReasonCode, expectedStatusCode);
		}

		void AssertCustomsQueue(ProcessQueue queue, ZString expectedQueueName, ZString expectedReasonCode, ZString expectedStatusCode)
		{
			AssertEquals("Has been moved to the wrong Queue", expectedQueueName, queue.P4_CustomsQueue);
			AssertEquals("Queue has been moved with the wrong reason code", expectedReasonCode, queue.P4_CustomsStatus);
			AssertEquals("Queue has been moved with the wrong status code", expectedStatusCode, queue.P4_CustomsSubStatus);
		}

		void AssertQueueMoving(ZString airCargoStatus, ZString expectedQueueName, ZString expectedReasonCode)
		{
			AssertQueueMoving(airCargoStatus, expectedQueueName, expectedReasonCode, true);
		}

		void AssertQueueMoving(ZString airCargoStatus, ZString expectedQueueName, ZString expectedReasonCode, bool ensureDecDoNotExist)
		{
			if (ensureDecDoNotExist)
			{
				UPECusHAWB.CS_JE_CustomsFormalEntry = ZGuid.Empty;
			}

			UPECusHAWB.CS_CustomsStatus = airCargoStatus;
			AssertCustomsQueue(UPECusHAWB.CurrentQueue, expectedQueueName, expectedReasonCode, string.Empty);
			if (ensureDecDoNotExist)
			{
				AssertEquals("Declaration should not be created as a result of CustomsStatus change", true, UPECusHAWB.CS_JE_CustomsFormalEntry.IsEmpty);
			}
		}

		void AssertQueueMoving(ZString airCargoStatus, ZString expectedQueueName, ZString expectedReasonCode, ZString expectedDecQueueName, ZString expectedDecStatus)
		{
			AssertQueueMoving(airCargoStatus, expectedQueueName, expectedReasonCode, expectedDecQueueName, expectedDecStatus, false);
		}

		void AssertQueueMoving(ZString airCargoStatus, ZString expectedQueueName, ZString expectedReasonCode, ZString expectedDecQueueName, ZString expectedDecStatus, bool createDeclarationFirst)
		{
			UPECusHAWB.CS_JE_CustomsFormalEntry = (createDeclarationFirst) ? Factory.New(typeof(UPEJobDeclaration)).PK : ZGuid.Empty;
			UPECusHAWB.CurrentQueue.CustomsQueueLogs.RemoveAll();
			AssertQueueMoving(airCargoStatus, expectedQueueName, expectedReasonCode, false);
			AssertCustomsQueue(UPECusHAWB.Declaration.CurrentQueue, expectedDecQueueName, expectedDecStatus, string.Empty);
		}

		void AssertQueueMoving_MsgStatus(ZString messageStatus, ZString expectedQueueName, ZString expectedStatusCode)
		{
			UPECusHAWB.CS_MsgStatus = messageStatus;
			AssertEquals("Has been moved to the wrong Queue", expectedQueueName, UPECusHAWB.CurrentQueue.P4_CustomsQueue);
			string errorMessage = (expectedStatusCode.IsEmpty) ? "Status Code should not be populated" : "Has been moved with the wrong Status Code";
			AssertEquals(errorMessage, expectedStatusCode, UPECusHAWB.CurrentQueue.P4_CustomsStatus);
		}

		void AssertQueueMoving_MsgStatus(ZString messageStatus, ZString expectedQueueName)
		{
			AssertQueueMoving_MsgStatus(messageStatus, expectedQueueName, string.Empty);
		}

		void AssertDeclarationQueueMovingWhenLinked(ZString airCargoStatus, ZString expectedQueueName, ZString expectedReasonCode)
		{
			UPECusHAWB.CS_CustomsStatus = airCargoStatus;
			UPECusHAWB.CS_JE_CustomsFormalEntry = Factory.New(typeof(UPEJobDeclaration)).PK;
			AssertCustomsQueue(UPECusHAWB.Declaration.CurrentQueue, expectedQueueName, expectedReasonCode, string.Empty);
		}

		#endregion
		#region Cascade Updating Consignee/Consignor Address Record
		public void TestCS_OA_ConsigneeAddress_CascadesToOrgPatternMatchAddress()
		{
			OrgHeader organsisation = Factory.NewWithValidTestData<OrgHeader>();
			UPECusHAWB.CS_OA_ConsigneeAddress = ZGuid.Empty;
			Factory.Save();
			AssertEquals("Consignee OrgPatternMatchAddress should not be lazy created when consignee set to empty", null, UPECusHAWB.UPEConsigneeAddress);
			UPECusHAWB.CS_OA_ConsigneeAddress = organsisation.MainAddress.PK;
			Factory.Save();
			AssertEquals("OrgPatternMatchAddress.P3_OH_MatchOrg should not be cascade updated until save", organsisation.PK, UPECusHAWB.UPEConsigneeAddress.P3_OH_MatchOrg);
			AssertEquals("CS_OA_ConsigneeAddress FK should cascade update OrgPatternMatchAddress.P3_OH_MatchOrg", organsisation.PK, UPECusHAWB.UPEConsigneeAddress.P3_OH_MatchOrg);
			UPECusHAWB.CS_OA_ConsigneeAddress = ZGuid.Empty;
			Factory.Save();
			AssertEquals("CS_OA_ConsigneeAddress FK should cascade update OrgPatternMatchAddress.P3_OH_MatchOrg", ZGuid.Empty, UPECusHAWB.UPEConsigneeAddress.P3_OH_MatchOrg);
		}

		public void TestCS_OA_ConsignorAddress_CascadesToOrgPatternMatchAddress()
		{
			OrgHeader organsisation = Factory.NewWithValidTestData<OrgHeader>();
			UPECusHAWB.CS_OA_ConsignorAddress = ZGuid.Empty;
			Factory.Save();
			AssertEquals("Consignor OrgPatternMatchAddress should not be lazy created when consignor set to empty", null, UPECusHAWB.UPEConsignorAddress);
			UPECusHAWB.CS_OA_ConsignorAddress = organsisation.MainAddress.PK;
			Factory.Save();
			AssertEquals("OrgPatternMatchAddress.P3_OH_MatchOrg should not be cascade updated until save", organsisation.PK, UPECusHAWB.UPEConsignorAddress.P3_OH_MatchOrg);
			AssertEquals("CS_OA_ConsignorAddress FK should cascade update OrgPatternMatchAddress.P3_OH_MatchOrg", organsisation.PK, UPECusHAWB.UPEConsignorAddress.P3_OH_MatchOrg);
			UPECusHAWB.CS_OA_ConsignorAddress = ZGuid.Empty;
			Factory.Save();
			AssertEquals("CS_OA_ConsignorAddress FK should cascade update OrgPatternMatchAddress.P3_OH_MatchOrg", ZGuid.Empty, UPECusHAWB.UPEConsignorAddress.P3_OH_MatchOrg);
		}

		#endregion
		#region CreateFormalDecAndMatch
		public void TestCreateFormalDecAndMatchIfRequired()
		{
			TaxOrFeeTestHelper.SetDeminimus(Factory, 250m);
			OrgHeader orgForMatching = GetOrgForMatching();
			UPECusHAWB airCargo = Factory.New<UPECusHAWB>();
			airCargo.CS_CM = MasterBill.PK;
			airCargo.CS_GoodsValue = 100m;
			airCargo.CS_GoodsDescription = "test air cargo goods";
			airCargo.CS_RL_NKOrigin = "SGSIN";
			airCargo.CS_RL_NKDestination = "AUSYD";
			airCargo.CS_ConsigneeCity = orgForMatching.MainAddress.OA_City;
			airCargo.CS_ConsigneeName = orgForMatching.OH_FullName;
			airCargo.CS_ConsigneePhone = orgForMatching.MainAddress.OA_Phone;
			airCargo.CS_ConsigneePostcode = orgForMatching.MainAddress.OA_PostCode;
			airCargo.CS_ConsigneeState = orgForMatching.MainAddress.OA_State;
			airCargo.CS_ConsigneeStreet = orgForMatching.MainAddress.OA_Address1;
			airCargo.CS_ConsigneeStreet2 = orgForMatching.MainAddress.OA_Address2;
			airCargo.CS_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Australia;
			airCargo.CS_ConsignorCity = orgForMatching.MainAddress.OA_City;
			airCargo.CS_ConsignorName = orgForMatching.OH_FullName;
			airCargo.CS_ConsignorPhone = orgForMatching.MainAddress.OA_Phone;
			airCargo.CS_ConsignorPostcode = orgForMatching.MainAddress.OA_PostCode;
			airCargo.CS_ConsignorState = orgForMatching.MainAddress.OA_State;
			airCargo.CS_ConsignorStreet = orgForMatching.MainAddress.OA_Address1;
			airCargo.CS_ConsignorStreet2 = orgForMatching.MainAddress.OA_Address2;
			airCargo.CreateFormalDecAndMatchIfRequired();
			AssertNull(FindDec(airCargo.CS_GoodsDescription));
			airCargo.CS_GoodsValue = 300m;
			airCargo.CS_JE_CustomsFormalEntry = ZGuid.NewZGuid();
			airCargo.CreateFormalDecAndMatchIfRequired();
			AssertNull(FindDec(airCargo.CS_GoodsDescription));
			airCargo.CS_JE_CustomsFormalEntry = ZGuid.Empty;
			airCargo.CreateFormalDecAndMatchIfRequired();
			AssertNotNull(FindDec(airCargo.CS_GoodsDescription));
			airCargo.CS_GoodsDescription = "test air cargo goods1";
			airCargo.CS_GoodsValue = 300m;
			OrgPatternMatchAddress importer = Factory.New<OrgPatternMatchAddress>();
			importer.P3_ParentID = airCargo.PK;
			importer.P3_ParentTableCode = airCargo.TablePrefix;
			importer.P3_AddressType = OrgPatternMatchAddress.Constants.AddressType.AirCargoImporter;
			airCargo.UPEImporterAddress.P3_City = orgForMatching.MainAddress.OA_City;
			airCargo.UPEImporterAddress.P3_CompanyName = orgForMatching.OH_FullName;
			airCargo.UPEImporterAddress.P3_Phone = orgForMatching.MainAddress.OA_Phone;
			airCargo.UPEImporterAddress.P3_PostCode = orgForMatching.MainAddress.OA_PostCode;
			airCargo.UPEImporterAddress.P3_State = orgForMatching.MainAddress.OA_State;
			airCargo.UPEImporterAddress.P3_Address1 = orgForMatching.MainAddress.OA_Address1;
			airCargo.UPEImporterAddress.P3_Address2 = orgForMatching.MainAddress.OA_Address2;
			airCargo.CreateFormalDecAndMatchIfRequired();
			Factory.Save();
			BusinessObjectFactory cleanFactory = new BusinessObjectFactory();
			AssertNotNull(Factory.Load(typeof(Customs.Business.BaseJobDeclaration), airCargo.CS_JE_CustomsFormalEntry));
		}

		public void TestCreateFormalDecAndMatchIfRequired_RelevantProcessQueueLogCreated()
		{
			TaxOrFeeTestHelper.SetDeminimus(Factory, 250m);
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_GoodsValue = 300;
			uPECusHAWB.CS_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Australia;
			AssertEquals("Pre-condition", 0, uPECusHAWB.CurrentQueue.CustomsQueueLogs.Count);
			Assert("Sanity check", uPECusHAWB.IsFormalDecRequired);
			uPECusHAWB.CreateFormalDecAndMatchIfRequired();
			AssertEquals(1, uPECusHAWB.CurrentQueue.CustomsQueueLogs.Count);
			AssertProcessQueueLog(uPECusHAWB.CurrentQueue.CustomsQueueLogs[0], string.Empty, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, StatusCodeDescriptionPairList.Codes.Y1_DocumentsToCustomsAQIS, string.Empty, string.Empty);
			uPECusHAWB.CreateFormalDecAndMatchIfRequired();
			AssertEquals("Log with empty Queue name already exists, should not create another log", 1, uPECusHAWB.CurrentQueue.CustomsQueueLogs.Count);
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.RemoveAll();
			uPECusHAWB.CS_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Congo;
			Assert("Sanity check", !uPECusHAWB.IsFormalDecRequired);
			uPECusHAWB.CS_FreightPrepaidCollect = Core.Constants.PaymentType.Prepaid;
			uPECusHAWB.CreateFormalDecAndMatchIfRequired();
			AssertEquals("Should not create log, Payment type is prepaid", 0, uPECusHAWB.CurrentQueue.CustomsQueueLogs.Count);
			uPECusHAWB.CS_JE_CustomsFormalEntry = ZGuid.Empty;
			uPECusHAWB.CS_FreightPrepaidCollect = Core.Constants.PaymentType.Collect;
			uPECusHAWB.CreateFormalDecAndMatchIfRequired();
			AssertEquals(1, uPECusHAWB.CurrentQueue.CustomsQueueLogs.Count);
			AssertProcessQueueLog(uPECusHAWB.CurrentQueue.CustomsQueueLogs[0], string.Empty, ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, string.Empty, string.Empty, string.Empty);
			uPECusHAWB.CreateFormalDecAndMatchIfRequired();
			AssertEquals("Log with empty Queue name already exists, should not create another log", 1, uPECusHAWB.CurrentQueue.CustomsQueueLogs.Count);
		}

		public void TestCreateFormalDecAndMatch_CreateDecImmediately()
		{
			TaxOrFeeTestHelper.SetDeminimus(Factory, 250m);
			OrgHeader orgForMatching = GetOrgForMatching();
			UPECusHAWB airCargo = Factory.New<UPECusHAWB>();
			airCargo.CS_RL_NKOrigin = "SGSIN";
			airCargo.CS_RL_NKDestination = "AUSYD";
			airCargo.CS_CM = MasterBill.PK;
			airCargo.CS_GoodsValue = 100m;
			airCargo.CS_GoodsDescription = "test air cargo goods";
			airCargo.CS_ConsigneeCity = orgForMatching.MainAddress.OA_City;
			airCargo.CS_ConsigneeName = orgForMatching.OH_FullName;
			airCargo.CS_ConsigneePhone = orgForMatching.MainAddress.OA_Phone;
			airCargo.CS_ConsigneePostcode = orgForMatching.MainAddress.OA_PostCode;
			airCargo.CS_ConsigneeState = orgForMatching.MainAddress.OA_State;
			airCargo.CS_ConsigneeStreet = orgForMatching.MainAddress.OA_Address1;
			airCargo.CS_ConsigneeStreet2 = orgForMatching.MainAddress.OA_Address2;
			airCargo.CS_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Australia;
			airCargo.CS_ConsignorCity = orgForMatching.MainAddress.OA_City;
			airCargo.CS_ConsignorName = orgForMatching.OH_FullName;
			airCargo.CS_ConsignorPhone = orgForMatching.MainAddress.OA_Phone;
			airCargo.CS_ConsignorPostcode = orgForMatching.MainAddress.OA_PostCode;
			airCargo.CS_ConsignorState = orgForMatching.MainAddress.OA_State;
			airCargo.CS_ConsignorStreet = orgForMatching.MainAddress.OA_Address1;
			airCargo.CS_ConsignorStreet2 = orgForMatching.MainAddress.OA_Address2;
			airCargo.CreateFormalDecAndMatch();
			AssertNotNull("Should auto-match and create formal dec", FindDec(airCargo.CS_GoodsDescription));
			AssertEquals(1, airCargo.CurrentQueue.CustomsQueueLogs.Count);
			AssertProcessQueueLog(airCargo.CurrentQueue.CustomsQueueLogs[0], string.Empty, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, StatusCodeDescriptionPairList.Codes.Y1_DocumentsToCustomsAQIS, string.Empty, string.Empty);
		}

		void AssertProcessQueueLog(ProcessQueueLog log, ZString queue, ZString status, ZString subStatus, ZString reason, ZString assignedTo)
		{
			AssertEquals(queue, log.Queue);
			AssertEquals(status, log.Status);
			AssertEquals(subStatus, log.SubStatus);
			AssertEquals(reason, log.Reason);
			AssertEquals(assignedTo, log.AssignedTo);
		}

		#endregion
		#region Is Formal Dec Required
		public void TestIsFormalDecRequired()
		{
			TaxOrFeeTestHelper.SetDeminimus(Factory, 10m);
			UPECusHAWB.CS_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Australia;
			UPECusHAWB.CS_JE_CustomsFormalEntry = ZGuid.Empty;
			UPECusHAWB.CS_GoodsValue = 11;
			UPECusHAWB.CS_RX_NKGoodsCurrency = Core.Constants.CurrencyCodes.Australia;
			AssertEquals(true, UPECusHAWB.IsFormalDecRequired);
		}

		public void TestIsFormalDecRequired_BasedOnDestinationCountry()
		{
			TaxOrFeeTestHelper.SetDeminimus(Factory, 10m);
			UPECusHAWB.CS_JE_CustomsFormalEntry = ZGuid.Empty;
			UPECusHAWB.CS_GoodsValue = 100m;
			AssertEquals("Destination Country not Australia", false, UPECusHAWB.IsFormalDecRequired);
			UPECusHAWB.CS_RN_NKConsigneeCountry = Core.Constants.CountryCodes.NewZealand;
			AssertEquals("Destination Country not Australia", false, UPECusHAWB.IsFormalDecRequired);
			UPECusHAWB.CS_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Australia;
			AssertEquals("Destination Country is Australia", true, UPECusHAWB.IsFormalDecRequired);
		}

		public void TestIsFormalDecRequired_IfGoodsValueLessThanScreenFreeValue()
		{
			TaxOrFeeTestHelper.SetDeminimus(Factory, 1000m);
			UPECusHAWB.CS_JE_CustomsFormalEntry = ZGuid.Empty;
			UPECusHAWB.CS_GoodsValue = 10m;
			UPECusHAWB.CS_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Australia;
			UPECusHAWB childUPECusHAWB = Factory.New<UPECusHAWB>();
			childUPECusHAWB.CS_HAWB = "Child1";
			InsertJobRelatedWayBillRecord(UPECusHAWB.PK, UPEJobRelatedWayBill.Constants.RelatedWayBillType.Child, "Child1", string.Empty, CusHAWBSchema.Constants.Prefix);
			AssertEquals(false, UPECusHAWB.IsFormalDecRequired);
		}

		public void TestGoodsValueInAUD()
		{
			UPECusHAWB.CS_GoodsValue = 100.25m;
			UPECusHAWB.CS_RX_NKGoodsCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertEquals(72.12m, UPECusHAWB.GoodsValueInAUD);
		}

		public void TestCurrencyConverter()
		{
			AssertEquals(ZArchitecture.Core.ExchangeRateType.Customs, UPECusHAWB.CurrencyConverter.RateType);
		}

		public void TestAUD()
		{
			AssertEquals(Core.Constants.CurrencyCodes.Australia, UPECusHAWB.AUD.RX_Code);
		}

		#endregion
		#region Related Notes
		public void TestBusinessObjectsWithRelatedNotes()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			UPECusHAWB.CS_JE_CustomsFormalEntry = declaration.PK;
			declaration.JE_DeclarationReference = "DECREF";
			AssertEquals("DECREF", ((JobDeclaration)UPECusHAWB.BusinessObjectsWithRelatedNotes[0]).JE_DeclarationReference);
		}

		#endregion
		#region Shipment Held Letter
		public void TestShipmentHeldLetterDetails()
		{
			AssertNotNull(UPECusHAWB.ShipmentHeldLetterDetails);
		}

		public void TestShipmentHeldLetterDetails_IsSaved()
		{
			UPECusHAWB.ShipmentHeldLetterDetails.ReasonText = "Reason to save";
			Factory.Save();
			BusinessObjectFactory separateFactory = new BusinessObjectFactory();
			UPECusHAWB loadedCusHAWB = separateFactory.Load<UPECusHAWB>(UPECusHAWB.PK);
			AssertEquals("Details should be saved along with the CusHAWB", "Reason to save", loadedCusHAWB.ShipmentHeldLetterDetails.ReasonText);
		}

		public void TestQueryForShipmentHeldLetterDetails()
		{
			AssertEquals("If no event hooked, 'operation cancelled' is the default result (true)", true, UPECusHAWB.QueryForShipmentHeldLetterDetails(ShipmentHeldLetterRecipient.Consignee));
			UPECusHAWB.QueryShipmentHeldLetterDetails += new QueryShipmentHeldLetterDetailsEventHandler(OnUPECusHAWB_QueryShipmentHeldLetterDetails);
			AssertEquals("The user did not cancel the query", false, UPECusHAWB.QueryForShipmentHeldLetterDetails(ShipmentHeldLetterRecipient.Consignee));
			AssertEquals("Business object should be populated", "I want to", UPECusHAWB.ShipmentHeldLetterDetails.ReasonText);
			QueryShipmentHeldLetterDetails_Cancel = true;
			AssertEquals("The query was cancelled", true, UPECusHAWB.QueryForShipmentHeldLetterDetails(ShipmentHeldLetterRecipient.Consignee));
		}

		void OnUPECusHAWB_QueryShipmentHeldLetterDetails(object sender, QueryShipmentHeldLetterDetailsEventArgs e)
		{
			e.BizObj.ReasonText = "I want to";
			if (QueryShipmentHeldLetterDetails_Cancel)
			{
				e.Cancel = true;
			}
		}

		bool QueryShipmentHeldLetterDetails_Cancel;
		#endregion
		#region Shipment Held Letter Auto-delivery
		public void TestDeliverShipmentHeldLetter_ReasonDefaultedWhenAutoDeliveredByBatchProcessor()
		{
			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				UPECusHAWB.CurrentQueue.P4_CustomsQueue = DefaultQueueCodeDescriptionPairList.Codes.EIR;
				UPECusHAWB.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.BP_PhoneNumberMissing;
				UPECusHAWB.ShipmentHeldLetterDetails.ReasonCode = ReasonCodeDescriptionPairList.Codes.B5_ClientRegistration;
				Factory.Save();
			}

			AssertEquals("Reason defaulted if auto-delivered in a batch processor", ReasonCodeDescriptionPairList.Codes.BP_PhoneNumberMissing, UPECusHAWB.ShipmentHeldLetterDetails.ReasonCode);
		}

		public void TestDeliverShipmentHeldLetter_ReasonNotDefaultedWhenAutoDeliveredByUser()
		{
			UPECusHAWB.CurrentQueue.P4_CustomsQueue = DefaultQueueCodeDescriptionPairList.Codes.EIR;
			UPECusHAWB.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.BP_PhoneNumberMissing;
			UPECusHAWB.ShipmentHeldLetterDetails.ReasonCode = ReasonCodeDescriptionPairList.Codes.B5_ClientRegistration;
			Factory.Save();
			AssertEquals("Reason not defaulted if auto-delivered by a user's save; the user has picked the reason code in a pre-save dialog", ReasonCodeDescriptionPairList.Codes.B5_ClientRegistration, UPECusHAWB.ShipmentHeldLetterDetails.ReasonCode);
		}

		#endregion
		#region Profiling and Screening
		public void TestIsValidForSAC()
		{
			var customsStopPhrase = "Fish";
			CMRReferenceFilesTestHelper.InsertThesaurusData(Factory, customsStopPhrase);
			UPEDataRegistry.Instance.StopPhrasesForGoodsDescription = new string[] { "MEH" };
			TaxOrFeeTestHelper.SetDeminimus(Factory, 200m);
			UPECusHAWB.CS_GoodsDescription = "Valid Description";
			UPECusHAWB.CS_GoodsValue = 200;
			UPECusHAWB.CS_RX_NKGoodsCurrency = Core.Constants.CurrencyCodes.Australia;
			AssertEquals(true, UPECusHAWB.IsValidForSAC);
			UPECusHAWB.CS_GoodsDescription = "Laughing is a good exercise MEH MEH";
			AssertEquals("MEH is one of the UPS Stop Words", false, UPECusHAWB.IsValidForSAC);
			UPECusHAWB.CS_GoodsDescription = customsStopPhrase + " DEVICE";	
			AssertEquals(customsStopPhrase + " is one of the Customs Stop Words", false, UPECusHAWB.IsValidForSAC);
			UPECusHAWB.CS_GoodsDescription = "valid description";
			UPECusHAWB.CS_GoodsValue = 201;
			AssertEquals("Goods description is above the screen free threshold", false, UPECusHAWB.IsValidForSAC);
		}

		public void TestIsIdentifiedForProfiling()
		{
			var customsStopPhrase = "Biological";
			CMRReferenceFilesTestHelper.InsertThesaurusData(Factory, customsStopPhrase);
			UPEDataRegistry.Instance.StopPhrasesForGoodsDescription = new string[] { "UPS Stop Phrase" };
			TaxOrFeeTestHelper.SetDeminimus(Factory, 10m);
			UPECusHAWB.CS_GoodsValue = 5m;
			UPECusHAWB.CS_GoodsDescription = customsStopPhrase;
			AssertEquals("Goods description has a customs stop phrase", true, UPECusHAWB.IsIdentifiedForProfiling);
			UPECusHAWB.CS_GoodsValue = 5m;
			UPECusHAWB.CS_GoodsDescription = "UPS Stop Phrase";
			AssertEquals("Goods description has a UPS stop phrase", false, UPECusHAWB.IsIdentifiedForProfiling);
			UPECusHAWB.CS_GoodsValue = 15m; // over the screen free threshold
			UPECusHAWB.CS_GoodsDescription = string.Empty;
			AssertEquals("Goods value is over the threshold", false, UPECusHAWB.IsIdentifiedForProfiling);
			UPECusHAWB.CS_GoodsDescription = customsStopPhrase;
			AssertEquals("Goods value is over the threshold", true, UPECusHAWB.IsIdentifiedForProfiling);
			UPECusHAWB.CS_GoodsValue = 5m;
			UPECusHAWB.CS_GoodsDescription = string.Empty;
			AssertEquals("Goods value under threshold and description has no stop phrases", false, UPECusHAWB.IsIdentifiedForProfiling);
		}

		public void TestIsIdentifiedForScreening()
		{
			var customsStopPhrase = "Biological";
			CMRReferenceFilesTestHelper.InsertThesaurusData(Factory, customsStopPhrase);
			UPEDataRegistry.Instance.NonDocumentScreeningValueRangesItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "20.0 - 30.0");
			UPEDataRegistry.Instance.StopPhrasesForGoodsDescription = new string[] { "UPS Stop" };
			UPECusHAWB.CS_ShipmentTypeForBinding = ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments;
			UPECusHAWB.CS_GoodsValue = 10m;
			UPECusHAWB.CS_GoodsDescription = "UPS Stop";
			AssertEquals("Goods description with a UPS stop phrase", true, UPECusHAWB.IsIdentifiedForScreening);
			UPECusHAWB.CS_GoodsValue = 10m;
			UPECusHAWB.CS_GoodsDescription = customsStopPhrase;
			AssertEquals("Goods description has a customs stop phrase only", false, UPECusHAWB.IsIdentifiedForScreening);
			UPECusHAWB.CS_GoodsValue = 25m; // within the UPS screening goods value range
			UPECusHAWB.CS_GoodsDescription = string.Empty;
			AssertEquals("Goods value within screening range", true, UPECusHAWB.IsIdentifiedForScreening);
			UPECusHAWB.CS_GoodsValue = 30.01m;
			UPECusHAWB.CS_GoodsDescription = string.Empty;
			AssertEquals("Goods value outside of screening range and description with no UPS stop phrases", false, UPECusHAWB.IsIdentifiedForScreening);
		}

		public void TestIsIdentifiedForQuarantine()
		{
			UPEDataRegistry.Instance.QuarantineStopPhrasesForGoodsDescription = new string[] { "fruit" };
			UPECusHAWB.CS_GoodsDescription = string.Empty;
			Assert("no quarantine stop words are in goods description", !UPECusHAWB.IsIdentifiedForQuarantine);
			UPECusHAWB.CS_GoodsDescription = "blah";
			Assert("no quarantine stop words are in goods description", !UPECusHAWB.IsIdentifiedForQuarantine);
			UPECusHAWB.CS_GoodsDescription = "blah fruit stuff";
			Assert("quarantine stop word fruit is in goods description", UPECusHAWB.IsIdentifiedForQuarantine);
		}

		public void TestIdentifiedForAction()
		{
			CMRReferenceFilesTestHelper.InsertThesaurusData(Factory, "Drug", "Biological");
			UPEDataRegistry.Instance.StopPhrasesForGoodsDescription = new string[] { "UPS Stop" };
			UPEDataRegistry.Instance.QuarantineStopPhrasesForGoodsDescription = new string[] { "fruit" };
			UPECusHAWB.CS_GoodsDescription = string.Empty;
			AssertEquals("Neither", string.Empty, UPECusHAWB.IdentifiedForAction);
			UPECusHAWB.CS_GoodsDescription = "UPS Stop";
			AssertEquals("IsIdentifiedForScreening", "Screening", UPECusHAWB.IdentifiedForAction);
			string screening = UPECusHAWB.IdentifiedForAction;
			UPECusHAWB.CS_GoodsDescription = "Biological";
			AssertEquals("IsIdentifiedForProfiling", "Profiling", UPECusHAWB.IdentifiedForAction);
			string profiling = UPECusHAWB.IdentifiedForAction;
			UPECusHAWB.CS_GoodsDescription = "fruit";
			AssertEquals("IsIdentifiedForQuarantine", "Quarantine", UPECusHAWB.IdentifiedForAction);
			UPECusHAWB.CS_GoodsDescription = "UPS Stop Biological";
			AssertEquals("IsIdentifiedForScreening & IsIdentifiedForProfiling", "Any", UPECusHAWB.IdentifiedForAction);
			UPECusHAWB.CS_GoodsDescription = "UPS Stop fruit";
			AssertEquals("IsIdentifiedForScreening & IsIdentifiedForQuarantine", "Any", UPECusHAWB.IdentifiedForAction);
			UPECusHAWB.CS_GoodsDescription = "fruit Biological";
			AssertEquals("IsIdentifiedForQuarantine & IsIdentifiedForProfiling", "Any", UPECusHAWB.IdentifiedForAction);
			UPECusHAWB.CS_GoodsDescription = "fruit Biological UPS Stop";
			AssertEquals("IsIdentifiedForQuarantine & IsIdentifiedForProfiling & IsIdentifiedForScreening", "Any", UPECusHAWB.IdentifiedForAction);
			UPECusHAWB.CS_GoodsDescription = "Jessica Alba Clones";
			UPECusHAWB.CS_GoodsValue = 200;
			UPECusHAWB.CS_RX_NKGoodsCurrency = Core.Constants.CurrencyCodes.Australia;
			UPECusHAWB childUPECusHAWB = Factory.New<UPECusHAWB>();
			childUPECusHAWB.CS_HAWB = "Child1";
			InsertJobRelatedWayBillRecord(UPECusHAWB.PK, UPEJobRelatedWayBill.Constants.RelatedWayBillType.Child, "Child1", string.Empty, CusHAWBSchema.Constants.Prefix);
			UPECusHAWB.CS_GoodsDescription = "UPS Stop Biological";
			AssertEquals("IsIdentifiedForMerging & IsIdentifiedForScreening & IsIdentifiedForProfiling", "Any", UPECusHAWB.IdentifiedForAction);
			UPECusHAWB.CS_GoodsDescription = "UPS Stop fruit";
			AssertEquals("IsIdentifiedForMerging & IsIdentifiedForScreening & IsIdentifiedForQuarantine", "Any", UPECusHAWB.IdentifiedForAction);
			UPECusHAWB.CS_GoodsDescription = "fruit Biological";
			AssertEquals("IsIdentifiedForMerging & IsIdentifiedForQuarantine & IsIdentifiedForProfiling", "Any", UPECusHAWB.IdentifiedForAction);
			UPECusHAWB.CS_GoodsDescription = "fruit Biological UPS Stop";
			AssertEquals("IsIdentifiedForMerging & IsIdentifiedForQuarantine & IsIdentifiedForProfiling & IsIdentifiedForScreening", "Any", UPECusHAWB.IdentifiedForAction);
		}

		public void TestRunScreeningValidation()
		{
			UPEDataRegistry.Instance.NonDocumentScreeningValueRangesItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1-20");
			UPEDataRegistry.Instance.StopPhrasesForGoodsDescription = new string[] { "x" };
			UPEDataRegistry.Instance.StopPhrasesForConsignorName = new string[] { "x" };
			UPEDataRegistry.Instance.StopPhrasesForConsignorAddress = new string[] { "x" };
			UPEDataRegistry.Instance.StopPhrasesForConsignorAccountNum = new string[] { "x" };
			UPEDataRegistry.Instance.StopPhrasesForConsigneeName = new string[] { "x" };
			UPEDataRegistry.Instance.StopPhrasesForConsigneeAddress = new string[] { "x" };
			UPEDataRegistry.Instance.StopPhrasesForConsigneeAccountNum = new string[] { "x" };
			UPECusHAWB.CS_ShipmentTypeForBinding = ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments;
			UPECusHAWB.CS_RX_NKGoodsCurrency = Core.Constants.CurrencyCodes.Australia;
			UPECusHAWB.CS_GoodsValue = 5;
			UPECusHAWB.CS_GoodsDescription = "x";
			UPECusHAWB.CS_ConsigneeName = "x";
			UPECusHAWB.CS_ConsigneeStreet = "x";
			UPECusHAWB.CS_ConsigneeStreet2 = "x";
			UPECusHAWB.CS_ConsignorName = "x";
			UPECusHAWB.CS_ConsignorStreet = "x";
			UPECusHAWB.CS_ConsignorStreet2 = "x";
			UPECusHAWB.Level1Record = new Level1Record();
			UPECusHAWB.Level1Record._400000 = new _400000Line("US4196AU9639000626              DA15V04FXKC7400000        x         DAVID TASKER                       DAVID TASKER             18 WENDY CRESENT                   XYZ                                QUEENSLAND                                             VI4019     AU 0738831842    000                                                                                000           ");
			UPECusHAWB.Level1Record._300000 = new _300000Line("US4196AU9639000626              DA15V04FXKC730000007201004x         AVM SOFTWARE                       213WEST 35TH STREET,               402                                NEW YORK                                               NY10001    US 12125649997   12125630422                                                                                                    NALENI MCGA");
			UPECusHAWB.Level1RecordNote.Text = UPECusHAWB.Level1Record.ToString();
			Factory.Save();
			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();
			UPECusHAWB loadedCusHAWB = loadingFactory.Load<UPECusHAWB>(UPECusHAWB.PK);
			loadedCusHAWB.RunScreeningValidation();
			AssertEquals("CS_GoodsValue", true, loadedCusHAWB.CS_GoodsValueInfo.HasWarnings());
			AssertEquals("CS_GoodsDescription", true, loadedCusHAWB.CS_GoodsDescriptionInfo.HasWarnings());
			AssertEquals("CS_ConsignorName", true, loadedCusHAWB.CS_ConsignorNameInfo.HasWarnings());
			AssertEquals("CS_ConsignorStreet", true, loadedCusHAWB.CS_ConsignorStreetInfo.HasWarnings());
			AssertEquals("CS_ConsignorStreet2", true, loadedCusHAWB.CS_ConsignorStreet2Info.HasWarnings());
			AssertEquals("Level1RecordConsignorAccountNum", true, loadedCusHAWB.Level1RecordConsignorAccountNumInfo.HasWarnings());
			AssertEquals("CS_ConsigneeName", true, loadedCusHAWB.CS_ConsigneeNameInfo.HasWarnings());
			AssertEquals("CS_ConsigneeStreet", true, loadedCusHAWB.CS_ConsigneeStreetInfo.HasWarnings());
			AssertEquals("CS_ConsigneeStreet2", true, loadedCusHAWB.CS_ConsigneeStreet2Info.HasWarnings());
			AssertEquals("Level1RecordConsigneeAccountNum", true, loadedCusHAWB.Level1RecordConsigneeAccountNumInfo.HasWarnings());
		}

		public void TestSetRemarksIfThisIsANewHAWBAndInInterventionQueue()
		{
			CMRReferenceFilesTestHelper.InsertThesaurusData(Factory, "Drug", "Biological");
			UPEDataRegistry.Instance.StopPhrasesForGoodsDescription = new string[] { "UPS", "MOBILE" };
			UPEDataRegistry.Instance.QuarantineStopPhrasesForGoodsDescription = new string[] { "FLY", "FRUIT" };
			UPECusHAWB.CS_GoodsDescription = string.Empty;
			UPECusHAWB.SetRemarksIfThisIsANewHAWBAndInInterventionQueue();
			AssertEquals("should be nothing in remarks because there is no warning", string.Empty, UPECusHAWB.CurrentQueue.P4_CustomsReason);
			UPECusHAWB.fIdentifiedForAction = null;
			UPECusHAWB.CurrentQueue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.Hold;
			UPECusHAWB.CS_GoodsDescription = "blah";
			UPECusHAWB.SetRemarksIfThisIsANewHAWBAndInInterventionQueue();
			AssertEquals("should be nothing in remarks because HAWB is not in intervention queue", string.Empty, UPECusHAWB.CurrentQueue.P4_CustomsReason);
			UPECusHAWB.fIdentifiedForAction = null;
			UPECusHAWB.CurrentQueue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.Intervention;
			UPECusHAWB.CS_GoodsDescription = "FRUIT FLY";
			UPECusHAWB.SetRemarksIfThisIsANewHAWBAndInInterventionQueue();
			AssertEquals("abbreviated version of GoodsDescriptionStopPhraseWarning should be in the remarks field of queue", "QUA:FLY,FRUIT", UPECusHAWB.CurrentQueue.P4_CustomsReason);
			UPECusHAWB.fIdentifiedForAction = null;
			UPECusHAWB.CS_GoodsDescription = "mobile phone";
			UPECusHAWB.SetRemarksIfThisIsANewHAWBAndInInterventionQueue();
			AssertEquals("abbreviated version of GoodsDescriptionStopPhraseWarning", "SCR:MOBILE", UPECusHAWB.CurrentQueue.P4_CustomsReason);
			UPECusHAWB.fIdentifiedForAction = null;
			UPECusHAWB.CS_GoodsDescription = "drugs";
			UPECusHAWB.SetRemarksIfThisIsANewHAWBAndInInterventionQueue();
			AssertEquals("abbreviated version of GoodsDescriptionStopPhraseWarning", "PRF:Drug", UPECusHAWB.CurrentQueue.P4_CustomsReason);
			UPECusHAWB.fIdentifiedForAction = null;
			UPECusHAWB.CS_GoodsDescription = "UPS drugs";
			UPECusHAWB.SetRemarksIfThisIsANewHAWBAndInInterventionQueue();
			AssertEquals("abbreviated version of GoodsDescriptionStopPhraseWarning", "ANY:Drug,UPS", UPECusHAWB.CurrentQueue.P4_CustomsReason);
			UPECusHAWB.fIdentifiedForAction = null;
			UPECusHAWB.CS_GoodsDescription = "mobile fly";
			UPECusHAWB.SetRemarksIfThisIsANewHAWBAndInInterventionQueue();
			AssertEquals("abbreviated version of GoodsDescriptionStopPhraseWarning", "ANY:MOBILE,FLY", UPECusHAWB.CurrentQueue.P4_CustomsReason);
			UPECusHAWB.fIdentifiedForAction = null;
			UPECusHAWB.CS_GoodsDescription = "drug induced fruit fly";
			UPECusHAWB.SetRemarksIfThisIsANewHAWBAndInInterventionQueue();
			AssertEquals("abbreviated version of GoodsDescriptionStopPhraseWarning", "ANY:Drug,FLY,FRUIT", UPECusHAWB.CurrentQueue.P4_CustomsReason);
			UPECusHAWB.fIdentifiedForAction = null;
			UPECusHAWB.CS_GoodsDescription = "drug induced biological fruit fly";
			UPECusHAWB.SetRemarksIfThisIsANewHAWBAndInInterventionQueue();
			var expectedSubstrings = new[] { "ANY:", "Drug", "Biological", "FLY", "FRUIT" };
			AssertEquals("abbreviated version of GoodsDescriptionStopPhraseWarning", expectedSubstrings.All(x => UPECusHAWB.CurrentQueue.P4_CustomsReason.Contains(x)), true);
			Factory.Save();
			UPECusHAWB.CS_GoodsDescription = "fruit fly";
			UPECusHAWB.SetRemarksIfThisIsANewHAWBAndInInterventionQueue();
			AssertEquals("after HAWB is saved, stop phrase in remarks should not change", expectedSubstrings.All(x => UPECusHAWB.CurrentQueue.P4_CustomsReason.Contains(x)), true);
		}

		#endregion
		#region IUPEDocumentSupporter
		public void TestDocumentSupporter()
		{
			UPECusHAWB hAWB = Factory.New<UPECusHAWB>();
			AssertEquals("Should be the correct document supporter type", true, hAWB.GetDocumentSupporter() is UPECusHAWBDocumentSupporter);
		}

		public void TestPrintBatchItemQueuedEvent()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			UPECusHAWB.PrintBatchItemQueued += new PrintBatchItemQueuedEventHandler(OnCusHAWB_PrintBatchItemQueued);
			UPEPrintBatch currentPrintBatch = new UPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
			UPEPrintBatchItem printBatchItem = currentPrintBatch.QueueForBatchPrintAndSave(UPECusHAWB, new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice().PK);
			AssertEquals("Factory should be saved after print item queued", false, UPECusHAWB.HasChanges);
			AssertEquals("Factory should be saved after print item queued", false, printBatchItem.HasChanges);
			AssertEquals("PrintBatchItemQueued should be fired with the correct UPEPrintBatchItem in the event args", printBatchItem, LastPrintBatchItemQueuedEventArgs.PrintItem);
			AssertEquals("PrintBatchItemQueued should be fired with the NotifyUser=true", true, LastPrintBatchItemQueuedEventArgs.NotifyUser);
			currentPrintBatch.QueueForBatchPrintAndSave(UPECusHAWB, new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice().PK, false);
			AssertEquals("PrintBatchItemQueued should be fired with the NotifyUser=false", false, LastPrintBatchItemQueuedEventArgs.NotifyUser);
		}

		PrintBatchItemQueuedEventArgs LastPrintBatchItemQueuedEventArgs;
		void OnCusHAWB_PrintBatchItemQueued(object sender, PrintBatchItemQueuedEventArgs e)
		{
			LastPrintBatchItemQueuedEventArgs = e;
		}

		#endregion
		#region IShipmentData
		public void TestShouldBeUploaded()
		{
			UPECusHAWBWithDummyLocalCharges localHAWB = Factory.New<UPECusHAWBWithDummyLocalCharges>();
			IShipmentData shipmentData = localHAWB;
			localHAWB.SetTotalLocalCharges(10);
			AssertEquals("There are local charges, should be uploaded", true, shipmentData.ShouldBeUploaded);
			localHAWB.SetTotalLocalCharges(0);
			localHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.Prepaid;
			localHAWB.DutyType = DutyTypeCodeDescriptionPairList.Codes.NonDutiable;
			localHAWB.Level1Record = CreateLevel1RecordWithDummyThirdPartyIndicator("0");
			AssertEquals("No charges and not GCC, Freight Collect, or Third Party AU. Should not be uploaded", false, shipmentData.ShouldBeUploaded);
			localHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreightCollect;
			AssertEquals("Billing terms is freight collect. Should be uploaded", true, shipmentData.ShouldBeUploaded);
			localHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreeBorder;
			localHAWB.DutyType = DutyTypeCodeDescriptionPairList.Codes.GCC;
			AssertEquals("Duty type is GCC. Should be uploaded", true, shipmentData.ShouldBeUploaded);
		}

		public void TestShouldBeUploaded_ThirdPartyIndicator()
		{
			UPECusHAWBWithDummyLocalCharges localHAWB = Factory.New<UPECusHAWBWithDummyLocalCharges>();
			IShipmentData shipmentData = localHAWB;
			localHAWB.SetTotalLocalCharges(0);
			localHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.Prepaid;
			localHAWB.DutyType = DutyTypeCodeDescriptionPairList.Codes.NonDutiable;
			localHAWB.Level1Record = null;
			AssertEquals("Level 1 record is null, cannot get ThirdPartyIndicator. Should be false", false, shipmentData.ShouldBeUploaded);
			localHAWB.Level1Record = new Level1Record();
			AssertEquals("_200000 line is null, cannot get ThirdPartyIndicator. Should be false", false, shipmentData.ShouldBeUploaded);
			localHAWB.Level1Record = CreateLevel1RecordWithDummyThirdPartyIndicator("1");
			AssertEquals("ThirdPartyIndicator is 1. Should be uploaded", true, shipmentData.ShouldBeUploaded);
			localHAWB.Level1Record = CreateLevel1RecordWithDummyThirdPartyIndicator("2");
			AssertEquals("ThirdPartyIndicator is 2. Should be uploaded", true, shipmentData.ShouldBeUploaded);
			localHAWB.Level1Record = CreateLevel1RecordWithDummyThirdPartyIndicator("4");
			AssertEquals("ThirdPartyIndicator is 4. Should be uploaded", true, shipmentData.ShouldBeUploaded);
			localHAWB.Level1Record = CreateLevel1RecordWithDummyThirdPartyIndicator("6");
			AssertEquals("ThirdPartyIndicator is 6. Should be uploaded", true, shipmentData.ShouldBeUploaded);
			localHAWB.Level1Record = CreateLevel1RecordWithDummyThirdPartyIndicator("0");
			AssertEquals("ThirdPartyIndicator is 0. Should not be uploaded", false, shipmentData.ShouldBeUploaded);
			localHAWB.Level1Record = CreateLevel1RecordWithDummyThirdPartyIndicator("7");
			AssertEquals("ThirdPartyIndicator is 7. Should be uploaded", true, shipmentData.ShouldBeUploaded);
			localHAWB.Level1Record = CreateLevel1RecordWithDummyThirdPartyIndicator("8");
			AssertEquals("ThirdPartyIndicator is 8. Should not be uploaded", false, shipmentData.ShouldBeUploaded);
		}

		public void TestIsAlreadyUploadedCondition()
		{
			UPECusHAWB localHAWB = Factory.New<UPECusHAWB>();
			IShipmentData shipmentData = localHAWB;
			AssertEquals("Precondition", false, shipmentData.IsAlreadyUploaded);
			AssertNull("Has no declaration", localHAWB.Declaration);
			((IShipmentData)localHAWB).BisiDeclarationUploadDate = ZDateTime.UtcNow;
			AssertEquals("No declaration upload date", false, shipmentData.IsAlreadyUploaded);
			((IBisiUpload)localHAWB).TransferredDateTime = ZDateTime.Now;
			AssertEquals(true, shipmentData.IsAlreadyUploaded);
		}

		public void TestIsAlreadyUploadedCondition_Declaration()
		{
			UPECusHAWB localHAWB = Factory.New<UPECusHAWB>();
			IShipmentData shipmentData = localHAWB;
			localHAWB.CS_JE_CustomsFormalEntry = Factory.New(typeof(UPEJobDeclaration)).PK;
			UPECusHAWB childUPECusHAWB = Factory.New<UPECusHAWB>();
			childUPECusHAWB.CS_HAWB = "Child1";
			InsertJobRelatedWayBillRecord(UPECusHAWB.PK, UPEJobRelatedWayBill.Constants.RelatedWayBillType.Child, "Child1", string.Empty, CusHAWBSchema.Constants.Prefix);
			((IShipmentData)localHAWB).BisiDeclarationUploadDate = ZDateTime.UtcNow;
			AssertEquals(true, shipmentData.IsAlreadyUploaded);
		}

		public void TestShouldBeDownloaded()
		{
			UPECusHAWB hAWB = Factory.New<UPECusHAWB>();
			IShipmentData shipmentData = hAWB;
			hAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreightCollect;
			AssertEquals("Shipment should be uploaded for the test", true, shipmentData.ShouldBeUploaded);
			AssertEquals("Shipment should be downloaded because it is uploaded", true, shipmentData.ShouldBeDownloaded);
			hAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.Prepaid;
			AssertEquals("Shipment should NOT be uploaded for the test", false, shipmentData.ShouldBeUploaded);
			AssertEquals("Shipment should NOT be downloaded because it is not uploaded", false, shipmentData.ShouldBeDownloaded);
			hAWB.DutyType = DutyTypeCodeDescriptionPairList.Codes.GCC;
			AssertEquals("Shipment should be uploaded for the test", true, shipmentData.ShouldBeUploaded);
			AssertEquals("Shipment should be downloaded because it is uploaded", true, shipmentData.ShouldBeDownloaded);
			hAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreeDomicile;
			AssertEquals("Shipment should be uploaded for the test", true, shipmentData.ShouldBeUploaded);
			AssertEquals("Shipment is not expected to be downloaded because it is free domicile", false, shipmentData.ShouldBeDownloaded);
		}

		public void TestShipmentRef()
		{
			InsertJobRelatedWayBillRecord(UPECusHAWB.PK, JobRelatedWayBill.Constants.RelatedWayBillType.Parent, string.Empty, "MEH", CusHAWBSchema.Constants.Prefix);
			AssertEquals("MEH", BrokerageShipmentData.ShipmentRef);
		}

		public void TestImportDate()
		{
			ZDateTime expectedDate = new ZDateTime(2005, 12, 8);
			MasterBill.CM_ArrivalDate = expectedDate;
			AssertEquals(expectedDate, BrokerageShipmentData.ImportDate);
		}

		public void TestConsigneePostCode()
		{
			UPECusHAWB.CS_ConsigneePostcode = "TEST";
			AssertEquals("TEST", ((ILineKey)UPECusHAWB).ConsigneePostCode);
			UPECusHAWB.CS_OA_ConsigneeAddress = Factory.New<OrgHeader>().MainAddress.PK;
			UPECusHAWB.Consignee.MainAddress.OA_PostCode = "NEWPC";
			AssertEquals("NEWPC", ((ILineKey)UPECusHAWB).ConsigneePostCode);
		}

		public void TestImporterAccountNo()
		{
			AssertEquals("Should always be empty", ZString.Empty, BrokerageShipmentData.ImporterAccountNumber);
			UPECusHAWB.CS_JE_CustomsFormalEntry = Factory.New(typeof(UPEJobDeclaration)).PK;
			UPECusHAWB.Declaration.JE_OH_Importer = Factory.New(typeof(OrgHeader)).PK;
			OrgCusCode customsCode2 = UPECusHAWB.Declaration.Importer.CustomsCodes.AddNew();
			customsCode2.OK_CustomsRegNo = "MEHHAHA";
			customsCode2.OK_CodeType = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber;
			customsCode2.OK_RN_NKCodeCountry = "AU";
			AssertEquals("Should always be empty", ZString.Empty, BrokerageShipmentData.ImporterAccountNumber);
		}

		public void TestBrokerageShipmentDutyType()
		{
			UPECusHAWB.DutyType = "meh";
			AssertEquals("meh", BrokerageShipmentData.DutyType);
		}

		public void TestMasterBillNumber()
		{
			AssertEquals("Incorrect MAWB number", "MAWB101", BrokerageShipmentData.MasterBillNumber);
			UPECusHAWB.CS_CM = ZGuid.Empty;
			AssertEquals("Should be empty when HouseBill is not attached to a Master", string.Empty, BrokerageShipmentData.MasterBillNumber);
		}

		public void TestDestinationPort()
		{
			UPECusHAWB.MAWB.CM_RL_NKDischargePort = "AUPER";
			AssertEquals("IShipmentData.DestinationPort from level 1 file", "AUPER", BrokerageShipmentData.DischargePort);
		}

		public void TestCustomsValue()
		{
			UPECusHAWB.Level1Record = null;
			AssertEquals("Level1Record is null, cannot get CustomsValue. Should be 0", 0m, BrokerageShipmentData.CustomsValue);
			UPECusHAWB.Level1Record = new Level1Record();
			AssertEquals("_200000 line is null, cannot get CustomsValue. Should be 0", 0m, BrokerageShipmentData.CustomsValue);
			UPECusHAWB.Level1Record._200000 = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US0000001234USDNNNN Y987654321USDAAY71267UPS   09651    123456789USD223456789USD4000      USDN0NL   NEDI  23JUN20001  LBS         USDD1    NNY NN  NN  N USD12345678901USD    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals(12.34m, BrokerageShipmentData.CustomsValue);
		}

		public void TestDVCCurrencyCode()
		{
			UPECusHAWB.Level1Record = null;
			AssertEquals("Level1Record is null, cannot get DeclaredValueCurrency. Should be empty", string.Empty, BrokerageShipmentData.DVCCurrencyCode);
			UPECusHAWB.Level1Record = new Level1Record();
			AssertEquals("_200000 line is null, cannot get DeclaredValueCurrecy. Should be empty", string.Empty, BrokerageShipmentData.DVCCurrencyCode);
			UPECusHAWB.Level1Record._200000 = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US0000001234USDNNNN Y987654321USDAAY71267UPS   09651    123456789USD223456789USD4000      USDN0NL   NEDI  23JUN20001  LBS         USDD1    NNY NN  NN  N USD12345678901USD    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals("USD", BrokerageShipmentData.DVCCurrencyCode);
		}

		public void TestCustomsExchangeRate()
		{
			AssertEquals("Should always be 0 since we're uploading the DeclaredValue in original currency", 0m, BrokerageShipmentData.CustomsExchangeRate);
		}

		public void TestCustomsEntryStatus()
		{
			UPECusHAWBWithDummyLocalCharges localHAWB = Factory.New<UPECusHAWBWithDummyLocalCharges>();
			IShipmentData shipmentData = localHAWB;
			localHAWB.SetTotalLocalCharges(0);
			AssertEquals("Should be empty if there are no local charges", string.Empty, shipmentData.BISICustomsEntryStatus);
			localHAWB.SetTotalLocalCharges(20);
			AssertEquals("Should be 02 when there are local charges", "02", shipmentData.BISICustomsEntryStatus);
		}

		public void TestCustomsEntryNumber()
		{
			UPECusHAWB.CS_JE_CustomsFormalEntry = ZGuid.Empty;
			AssertEquals("Should be empty when there is no declaration attached", string.Empty, BrokerageShipmentData.CustomsEntryNumber);
			UPEJobDeclaration declaration = Factory.New<UPEJobDeclaration>();
			declaration.JE_DeclarationReference = "MEHNUMBER";
			UPECusHAWB.CS_JE_CustomsFormalEntry = declaration.PK;
			AssertEquals("MEHNUMBER", BrokerageShipmentData.CustomsEntryNumber);
		}

		[TestDate(2002, 04, 28)]
		public void TestCustomsEntryDate()
		{
			UPECusHAWB.CS_JE_CustomsFormalEntry = ZGuid.Empty;
			AssertEquals("Should be the current date if Declaration does not exist", ZDateTime.Now, BrokerageShipmentData.CustomsEntryDate);
			UPEJobDeclaration declaration = Factory.New<UPEJobDeclaration>();
			ZDateTime expectedDate = new ZDateTime(2005, 11, 4);
			declaration.CustomsEntryDate = expectedDate;
			UPECusHAWB.CS_JE_CustomsFormalEntry = declaration.PK;
			AssertEquals(expectedDate, BrokerageShipmentData.CustomsEntryDate);
			declaration.CustomsEntryDate = ZDateTime.Empty;
			AssertEquals("Should be the current date if Declaration.CustomsEntryDate is empty", ZDateTime.Now, BrokerageShipmentData.CustomsEntryDate);
		}

		public void TestBrokerageShipmentBisiUploadDate_NoDeclaration()
		{
			ZDateTime expectedDate1 = new ZDateTime(2005, 3, 4);
			ZDateTime expectedDate2 = new ZDateTime(2005, 3, 5);
			UPECusHAWB.BisiUploadDate = expectedDate1;
			UPECusHAWB.CS_JE_CustomsFormalEntry = ZGuid.Empty;
			AssertEquals(expectedDate1, BrokerageShipmentData.BisiDeclarationUploadDate);
			((IBisiUpload)BrokerageShipmentData).TransferredDateTime = expectedDate2;
			BrokerageShipmentData.BisiDeclarationUploadDate = expectedDate2;
			AssertEquals(expectedDate2, BrokerageShipmentData.BisiDeclarationUploadDate);
			AssertEquals(expectedDate2, UPECusHAWB.BisiUploadDate);
		}

		public void TestBrokerageShipmentBisiUploadDate_WithDeclaration()
		{
			ZDateTime expectedDate1 = new ZDateTime(2005, 3, 4);
			ZDateTime expectedDate2 = new ZDateTime(2005, 3, 5);
			ZDateTime expectedDate3 = new ZDateTime(2005, 3, 6);
			UPECusHAWB.BisiUploadDate = expectedDate1;
			UPECusHAWB.CS_JE_CustomsFormalEntry = Factory.New(typeof(UPEJobDeclaration)).PK;
			UPECusHAWB.Declaration.BisiUploadDate = expectedDate2;
			AssertEquals("Should be getting the date from the Declaration", expectedDate2, BrokerageShipmentData.BisiDeclarationUploadDate);
			((IBisiUpload)BrokerageShipmentData).TransferredDateTime = expectedDate3;
			BrokerageShipmentData.BisiDeclarationUploadDate = expectedDate3;
			AssertEquals(expectedDate3, BrokerageShipmentData.BisiDeclarationUploadDate);
			AssertEquals(expectedDate3, UPECusHAWB.BisiUploadDate);
			AssertEquals("Should be setting the declaration's upload date as well", expectedDate3, UPECusHAWB.Declaration.BisiUploadDate);
		}

		#region TestCommoditiesData
		public void TestCommoditiesData()
		{
			CusHAWBWithDummyDeclarationChargesForTest localHAWB = Factory.New<CusHAWBWithDummyDeclarationChargesForTest>();
			JobComInvoiceHeader invHeader = localHAWB.Declaration.Invoices.AddNew();
			invHeader.JZ_InvoiceNumber = "1111";
			invHeader.ZA_ORG = Core.Constants.CountryCodes.UnitedStates;
			JobComInvoiceLine invLine1 = invHeader.JobComInvoiceLines.AddNew();
			invLine1.JI_Description = "Goods1";
			invLine1.JI_Tariff = "1";
			invLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invLine1.JI_LinePrice = 100.10;
			JobComInvoiceLine invLine2 = invHeader.JobComInvoiceLines.AddNew();
			invLine2.JI_Description = "Goods2";
			invLine2.JI_Tariff = "2";
			invLine2.JI_CountryOfOrigin = string.Empty;
			invLine2.JI_LinePrice = 200.20;
			var commoditiesData = ((IShipmentData)localHAWB).CommoditiesData;
			AssertEquals("CommoditiesData.Length", 2, commoditiesData.Count);
			AssertCommodityDetailData(commoditiesData[0], "Goods1", "1", Core.Constants.CountryCodes.Australia, 100.10m);
			AssertCommodityDetailData(commoditiesData[1], "Goods2", "2", Core.Constants.CountryCodes.UnitedStates, 200.20m);
		}

		void AssertCommodityDetailData(CommodityDetailData commoditiesData, ZString expectedDescription, ZString expectedTariff, ZString expectedCountry, ZDecimal expectedItemPrice)
		{
			AssertEquals(expectedDescription, commoditiesData.GoodsDescription);
			AssertEquals(expectedTariff, commoditiesData.TariffNumber);
			AssertEquals(expectedCountry, commoditiesData.CountryOfOrigin);
			AssertEquals(expectedItemPrice, commoditiesData.ItemPrice);
		}

		#endregion
		#region TestChargesData
		public void TestTotalLocalCharges()
		{
			CusHAWBWithDummyDeclarationChargesForTest localHAWB = Factory.New<CusHAWBWithDummyDeclarationChargesForTest>();
			localHAWB.ShipmentType = ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments;
			localHAWB.Declaration.JE_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			UPEDataRegistry.Instance.SecurityFeeAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 9.95m);
			localHAWB.Declaration.QuarantineFee = 100m;
			localHAWB.Declaration.QuarantineProcessingFee = 54m;
			localHAWB.RequiresConsigneeMatch = true;
			localHAWB.UPEConsigneeAddress.P3_OH_MatchOrg = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertEquals("Sanity check", false, localHAWB.Declaration.HasAlternateBroker);
			AssertEquals(290.55m, localHAWB.TotalLocalCharges);
		}

		public void TestChargesData()
		{
			CusHAWBWithDummyDeclarationChargesForTest localHAWB = Factory.New<CusHAWBWithDummyDeclarationChargesForTest>();
			localHAWB.ShipmentType = ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments;
			UPEDataRegistry.Instance.SecurityFeeAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 9.95m);
			localHAWB.Declaration.JE_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			localHAWB.Declaration.QuarantineFee = 100m;
			localHAWB.Declaration.QuarantineProcessingFee = 54m;
			localHAWB.RequiresConsigneeMatch = true;
			localHAWB.UPEConsigneeAddress.P3_OH_MatchOrg = Factory.NewWithValidTestData<OrgHeader>().PK;
			var chargesData = ShipmentChargeDataComparer.SortChargesByTypeCode(((IShipmentData)localHAWB).ChargesData);
			AssertEquals("Sanity check", false, localHAWB.Declaration.HasAlternateBroker);
			AssertEquals(7, chargesData.Count);
			AssertShipmentChargeData(chargesData[0], "201", 10m);
			AssertShipmentChargeData(chargesData[1], "206", 20m);
			AssertShipmentChargeData(chargesData[2], "216", 60m);
			AssertShipmentChargeData(chargesData[3], "224", 100m);
			AssertShipmentChargeData(chargesData[4], "231", 36.60m);
			AssertShipmentChargeData(chargesData[5], "309", 54m);
			AssertShipmentChargeData(chargesData[6], "348", 9.95m);
		}

		public void TestSecurityFeeInclusion()
		{
			UPECusHAWB.ShipmentType = ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments;
			UPECusHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreightCollect;
			UPECusHAWB.Level1Record = CreateLevel1RecordWithDummyThirdPartyIndicator("1");
			UPEDataRegistry.Instance.SecurityFeeAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0m);
			AssertEquals("Empty security fee amount, should not be included", false, ContainsSecurityFee(((IShipmentData)UPECusHAWB).ChargesData));
			UPEDataRegistry.Instance.SecurityFeeAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10m);
			AssertEquals("Security Fee should be included on All Freight Collect Shipments", true, ContainsSecurityFee(((IShipmentData)UPECusHAWB).ChargesData));
			UPECusHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.CostAndFreight;
			AssertEquals("No local charges, should not be included", false, ContainsSecurityFee(((IShipmentData)UPECusHAWB).ChargesData));
			UPECusHAWB.CS_JE_CustomsFormalEntry = Factory.New(typeof(UPEJobDeclarationWithDummyCharges)).PK;
			UPECusHAWB.Declaration.QuarantineProcessingFee = 20m;
			UPECusHAWB.RequiresConsigneeMatch = true;
			UPECusHAWB.UPEConsigneeAddress.P3_OH_MatchOrg = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertEquals("should be included now", true, ContainsSecurityFee(((IShipmentData)UPECusHAWB).ChargesData));
			UPECusHAWB.Level1Record = CreateLevel1RecordWithDummyThirdPartyIndicator("2");
			AssertEquals("ThirdParyIndicator is Exempt from security fee", false, ContainsSecurityFee(((IShipmentData)UPECusHAWB).ChargesData));
			UPECusHAWB.Level1Record = CreateLevel1RecordWithDummyThirdPartyIndicator("5");
			AssertEquals("ThirdParyIndicator is Exempt from security fee", false, ContainsSecurityFee(((IShipmentData)UPECusHAWB).ChargesData));
			UPECusHAWB.Level1Record = CreateLevel1RecordWithDummyThirdPartyIndicator("8");
			AssertEquals("ThirdParyIndicator is Exempt from security fee", false, ContainsSecurityFee(((IShipmentData)UPECusHAWB).ChargesData));
			UPECusHAWB.Level1Record = CreateLevel1RecordWithDummyThirdPartyIndicator("7");
			AssertEquals("Not exempt, should be included", true, ContainsSecurityFee(((IShipmentData)UPECusHAWB).ChargesData));
			UPECusHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreeDomicile;
			AssertEquals("Free domicile, should be exempted", false, ContainsSecurityFee(((IShipmentData)UPECusHAWB).ChargesData));
			UPECusHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreightCollect;
			AssertEquals("Not exempt, should be included", true, ContainsSecurityFee(((IShipmentData)UPECusHAWB).ChargesData));
			UPECusHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreeBorder;
			((UPEOrgHeader)UPECusHAWB.UPEConsigneeAddress.MatchOrg).IsITFChargableForThisImporter = false;
			AssertEquals("ITF not chargable, should be exempted", false, ContainsSecurityFee(((IShipmentData)UPECusHAWB).ChargesData));
		}

		public void TestIncludeSecurityFeeIfFreightCollectAndNonDocument()
		{
			UPECusHAWB.ShipmentType = ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments;
			UPECusHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreightCollect;
			UPECusHAWB.Level1Record = CreateLevel1RecordWithDummyThirdPartyIndicator("1");
			UPEDataRegistry.Instance.SecurityFeeAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 9.95m);
			Assert("Empty security fee amount, should be included", ContainsSecurityFee(((IShipmentData)UPECusHAWB).ChargesData));
			UPECusHAWB.ShipmentType = ShipmentTypeCodeDescriptionPairList.Codes.Letter;
			Assert("Empty security fee amount, should be included", !ContainsSecurityFee(((IShipmentData)UPECusHAWB).ChargesData));
			UPECusHAWB.ShipmentType = ShipmentTypeCodeDescriptionPairList.Codes.Documents;
			Assert("Empty security fee amount, should be included", !ContainsSecurityFee(((IShipmentData)UPECusHAWB).ChargesData));
			UPECusHAWB.ShipmentType = ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments;
			UPECusHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.CostAndFreight;
			Assert("Empty security fee amount, should be included", !ContainsSecurityFee(((IShipmentData)UPECusHAWB).ChargesData));
		}

		public void TestIncludeSecurityFeeIfDirectDebitShipment()
		{
			var importer = Factory.LoadTop1<UPEOrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, ZBool.True));
			importer.MiscServ.OM_IMEFTBankBSB = "1343";
			importer.MiscServ.OM_IMEFTBankAccount = "101846574";
			importer.MiscServ.OM_IMEftCustomsFromImport = false;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			UPECusHAWB.CS_JE_CustomsFormalEntry = declaration.PK;
			UPECusHAWB.CS_ShipmentType = ShipmentTypeCodeDescriptionPairList.Codes.Documents;
			UPECusHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreightCollect;
			Assert("There are no charges, Security fee should not be added", !ContainsSecurityFee(((IShipmentData)UPECusHAWB).ChargesData));
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var charges = entryHeader.Charges.AddNew();
			charges.C1_ChargeAmount = 34.43m;
			charges.C1_ChargeType = Enterprise.Customs.Business.CustomsChargeTypeList.Codes.AdditionCharge;
			AssertEquals("CustomsCharges available", 1, ServiceLocator.GetService<ICustomsCharges>(UPECusHAWB.Declaration).GetCustomsCharges(null).Length);
			Assert("should Not Direct debit", !UPECusHAWB.Declaration.IsCustomsEFTActive);
			Assert("Upload charges should be true", UPECusHAWB.Declaration.UploadCustomsCharges);
			Assert("UploadCustomsCharges is true, Security fee should not be added", !ContainsSecurityFee(((IShipmentData)UPECusHAWB).ChargesData));
			importer.MiscServ.OM_IMEftCustomsFromImport = true;
			Assert("Upload charges should be false", !UPECusHAWB.Declaration.UploadCustomsCharges);
			Assert("should be Direct debit", UPECusHAWB.Declaration.IsCustomsEFTActive);
			Assert("Security fee should be added when shipment is direct debit", ContainsSecurityFee(((IShipmentData)UPECusHAWB).ChargesData));
		}

		public void TestThirdPartyIndicator()
		{
			UPECusHAWB.Level1Record = CreateLevel1RecordWithDummyThirdPartyIndicator("1");
			UPECusHAWB.Level1Record = CreateLevel1RecordWithDummyThirdPartyIndicator("2");
			UPECusHAWB.Level1Record = CreateLevel1RecordWithDummyThirdPartyIndicator("3");
			UPECusHAWB.Level1Record = CreateLevel1RecordWithDummyThirdPartyIndicator("4");
		}

		public void TestOnLoaded()
		{
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_CM = Factory.New<UPECusMAWB>().PK;
			Factory.Save();
			BusinessObjectFactory clearFactory = new BusinessObjectFactory();
			uPECusHAWB = clearFactory.Load<UPECusHAWB>(uPECusHAWB.PK);
			AssertEquals(false, uPECusHAWB.HasChanges);
		}

		void AssertShipmentChargeData(ShipmentChargeData chargeData, ZString expectedTypeCode, ZDecimal expectedAmount)
		{
			AssertEquals(expectedTypeCode, chargeData.TypeCode);
			AssertEquals(expectedAmount, chargeData.GrossAmount);
		}

		bool ContainsSecurityFee(IReadOnlyList<ShipmentChargeData> chargesData)
		{
			foreach (ShipmentChargeData chargeData in chargesData)
			{
				if (chargeData.TypeCode == ((int)ShipmentChargeTypeCode.Security).ToString())
				{
					return true;
				}
			}

			return false;
		}

		#endregion
		public void TestMarkShipmentAsSplitShipmentIfApplicable()
		{
			UPECusHAWB.CurrentQueue.P4_QueueName = string.Empty;
			UPECusHAWB.CurrentQueue.P4_Status = string.Empty;
			UPECusHAWB.CS_JE_CustomsFormalEntry = ZGuid.Empty;
			AssertEquals("Sanity check", false, UPECusHAWB.IsSubsequentSplitShipment);
			BrokerageShipmentData.MarkShipmentAsSplitShipmentIfApplicable();
			AssertEquals("Should not be marked as split shipment, no declaration attached", false, UPECusHAWB.IsSubsequentSplitShipment);
			UPECusHAWB.CS_JE_CustomsFormalEntry = Factory.New(typeof(UPEJobDeclaration)).PK;
			BrokerageShipmentData.MarkShipmentAsSplitShipmentIfApplicable();
			AssertEquals("Should not be marked as split shipment, no upload date is set in the declaration", false, UPECusHAWB.IsSubsequentSplitShipment);
			UPECusHAWB.BisiUploadDate = ZDateTime.Now;
			BrokerageShipmentData.MarkShipmentAsSplitShipmentIfApplicable();
			AssertEquals("Not a split shipment, the CusHAWB has the upload date set", false, UPECusHAWB.IsSubsequentSplitShipment);
			UPECusHAWB.BisiUploadDate = ZDateTime.Empty;
			UPECusHAWB.Declaration.BisiUploadDate = ZDateTime.Now;
			BrokerageShipmentData.MarkShipmentAsSplitShipmentIfApplicable();
			AssertEquals("Should be marked as split shipment if the Declaration has the upload date set but not on the CusHAWB", true, UPECusHAWB.IsSubsequentSplitShipment);
		}

		public void TestNonRequiredFields()
		{
			AssertEquals(0m, BrokerageShipmentData.StatisticalValue);
			AssertEquals(string.Empty, BrokerageShipmentData.EntryType);
			AssertEquals(string.Empty, BrokerageShipmentData.CustomsOfficeNumber);
			AssertEquals(string.Empty, BrokerageShipmentData.VATNumber);
			AssertEquals(string.Empty, BrokerageShipmentData.ImporterVATDefermentNumber);
			AssertEquals(string.Empty, BrokerageShipmentData.SplitDutyDefermentNumber);
		}

		IShipmentData BrokerageShipmentData
		{
			get
			{
				return UPECusHAWB;
			}
		}

		Level1Record CreateLevel1RecordWithDummyThirdPartyIndicator(string thirdPartyIndicator)
		{
			Level1Record result = new Level1Record();
			result._200000 = new _200000Line(string.Format("US2795AU9639040422              D4A14T9J3YYD2000004A14T9J3YYD           N 1 6    LBS US          USDNNNN           USDAKE32632QF    08695             USD4700     USD21089     USDN{0} N   YEDI  18APR20046  LBS         USDD1    NNN NN  NN  N USD           USD    T1                      18APR20040000           56089      P/PNTF       6    LBSNNC0000051894QF12            N N 1   N ", thirdPartyIndicator));
			AssertEquals("Sanity check", thirdPartyIndicator, result._200000.ThirdPartyIndicator);
			return result;
		}

		#region UPECusHAWBWithDummyLocalCharges
		sealed class UPECusHAWBWithDummyLocalCharges : UPECusHAWB, IShipmentData
		{
			public UPECusHAWBWithDummyLocalCharges(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public void SetTotalLocalCharges(ZDecimal totalLocalCharges)
			{
				fChargesData = new ShipmentChargeData[] { new ShipmentChargeData(ShipmentChargeTypeCode.GST, totalLocalCharges, Core.Constants.CurrencyCodes.Australia) };
				AssertEquals("Sanity check", totalLocalCharges, this.TotalLocalCharges);
			}

			IReadOnlyList<CommodityDetailData> IShipmentData.CommoditiesData
			{
				get
				{
					return Array.Empty<CommodityDetailData>();
				}
			}

			IReadOnlyList<ShipmentChargeData> IShipmentData.ChargesData
			{
				get
				{
					return fChargesData;
				}
			}

			ShipmentChargeData[] fChargesData;
		}

		#endregion
		#endregion
		#region EIR Raised
		public void TestMovedToEIRQueue_NoAction()
		{
			object o = UPECusHAWB.PK; //get the lazy loading to work
			Factory.Save();
			BusinessObjectFactory factoryForTest = new BusinessObjectFactory();
			UPECusHAWB uPECusHAWBReLoaded = factoryForTest.Load<UPECusHAWB>(UPECusHAWB.PK);
			EIRBeenRaisedAnswer = false;
			try
			{
				uPECusHAWBReLoaded.CurrentQueue.AskHasEIRBeenRaised += new CancelEventHandler(OnCusHAWB_AskHasEIRBeenRaised);
				uPECusHAWBReLoaded.CurrentQueue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.EIR;
				uPECusHAWBReLoaded.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.AEIR.Codes.S1_ShipperConsigneeDetailsInsufficient;
				uPECusHAWBReLoaded.CurrentQueue.P4_CustomsSubStatus = StatusCodeDescriptionPairList.EmptyStatus;
				uPECusHAWBReLoaded.CS_GoodsDescription = "TEST";
				factoryForTest.Save();
				AssertEquals(Events.EditedARecord.Code, uPECusHAWBReLoaded.Logs.AutoCreatedLog.SL_SE_NKEvent);
				AssertEquals(string.Empty, uPECusHAWBReLoaded.Logs.AutoCreatedLog.SL_Reference);
				AssertEquals(ZDateTime.Empty, uPECusHAWBReLoaded.CurrentQueue.P4_CustomDate4);
			}
			finally
			{
				uPECusHAWBReLoaded.CurrentQueue.AskHasEIRBeenRaised -= new CancelEventHandler(OnCusHAWB_AskHasEIRBeenRaised);
			}
		}

		public void TestOpenInEIRQueueAndSaveInEIRQueue_EIRNotRaisedSelected()
		{
			UPECusHAWB.CurrentQueue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.EIR;
			UPECusHAWB.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.AEIR.Codes.S1_ShipperConsigneeDetailsInsufficient;
			UPECusHAWB.CurrentQueue.P4_CustomsSubStatus = StatusCodeDescriptionPairList.EmptyStatus;
			Factory.Save();
			BusinessObjectFactory factoryForTest = new BusinessObjectFactory();
			UPECusHAWB uPECusHAWBReLoaded = factoryForTest.Load<UPECusHAWB>(UPECusHAWB.PK);
			EIRBeenRaisedAnswer = false;
			try
			{
				uPECusHAWBReLoaded.CurrentQueue.AskHasEIRBeenRaised += new CancelEventHandler(OnCusHAWB_AskHasEIRBeenRaised);
				uPECusHAWBReLoaded.CurrentQueue.P4_Reason = "Test";
				uPECusHAWBReLoaded.CS_GoodsDescription = "TEST";
				factoryForTest.Save();
				AssertEquals(Events.EditedARecord.Code, uPECusHAWBReLoaded.Logs.AutoCreatedLog.SL_SE_NKEvent);
				AssertEquals("EIR Not Raised", uPECusHAWBReLoaded.Logs.AutoCreatedLog.SL_Reference);
				AssertEquals(ZDateTime.Empty, uPECusHAWBReLoaded.CurrentQueue.P4_CustomDate4);
			}
			finally
			{
				uPECusHAWBReLoaded.CurrentQueue.AskHasEIRBeenRaised -= new CancelEventHandler(OnCusHAWB_AskHasEIRBeenRaised);
			}
		}

		[TestDate(2006, 5, 5)]
		public void TestOpenInEIRQueueAndSaveInEIRQueue_EIRRaisedSelected()
		{
			UPECusHAWB.CurrentQueue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.EIR;
			UPECusHAWB.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.AEIR.Codes.S1_ShipperConsigneeDetailsInsufficient;
			UPECusHAWB.CurrentQueue.P4_CustomsSubStatus = StatusCodeDescriptionPairList.EmptyStatus;
			Factory.Save();
			BusinessObjectFactory factoryForTest = new BusinessObjectFactory();
			UPECusHAWB uPECusHAWBReLoaded = factoryForTest.Load<UPECusHAWB>(UPECusHAWB.PK);
			EIRBeenRaisedAnswer = true;
			try
			{
				uPECusHAWBReLoaded.CurrentQueue.AskHasEIRBeenRaised += new CancelEventHandler(OnCusHAWB_AskHasEIRBeenRaised);
				uPECusHAWBReLoaded.CurrentQueue.P4_Reason = "Test";
				uPECusHAWBReLoaded.CS_GoodsDescription = "TEST";
				factoryForTest.Save();
				AssertEquals(Events.EditedARecord.Code, uPECusHAWBReLoaded.Logs.AutoCreatedLog.SL_SE_NKEvent);
				AssertEquals("EIR Raised", uPECusHAWBReLoaded.Logs.AutoCreatedLog.SL_Reference);
				AssertEquals(new ZDateTime(2006, 5, 5), uPECusHAWBReLoaded.CurrentQueue.P4_CustomDate4);
			}
			finally
			{
				uPECusHAWBReLoaded.CurrentQueue.AskHasEIRBeenRaised -= new CancelEventHandler(OnCusHAWB_AskHasEIRBeenRaised);
			}
		}

		public void TestOpenInEIRQueueAndSaveInAnotherQueue()
		{
			UPECusHAWB.CurrentQueue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.EIR;
			UPECusHAWB.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.AEIR.Codes.S1_ShipperConsigneeDetailsInsufficient;
			UPECusHAWB.CurrentQueue.P4_CustomsSubStatus = StatusCodeDescriptionPairList.EmptyStatus;
			Factory.Save();
			BusinessObjectFactory factoryForTest = new BusinessObjectFactory();
			UPECusHAWB uPECusHAWBReLoaded = factoryForTest.Load<UPECusHAWB>(UPECusHAWB.PK);
			EIRBeenRaisedAnswer = true;
			try
			{
				uPECusHAWBReLoaded.CurrentQueue.AskHasEIRBeenRaised += new CancelEventHandler(OnCusHAWB_AskHasEIRBeenRaised);
				uPECusHAWBReLoaded.CurrentQueue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.Intervention;
				uPECusHAWBReLoaded.CS_GoodsDescription = "TEST";
				factoryForTest.Save();
				AssertEquals(Events.EditedARecord.Code, uPECusHAWBReLoaded.Logs.AutoCreatedLog.SL_SE_NKEvent);
				AssertEquals(string.Empty, uPECusHAWBReLoaded.Logs.AutoCreatedLog.SL_Reference);
				AssertEquals(ZDateTime.Empty, uPECusHAWBReLoaded.CurrentQueue.P4_CustomDate4);
			}
			finally
			{
				uPECusHAWBReLoaded.CurrentQueue.AskHasEIRBeenRaised -= new CancelEventHandler(OnCusHAWB_AskHasEIRBeenRaised);
			}
		}

		[TestDate(2006, 5, 5)]
		public void TestOpenInEIRQueueAndMultipleSaves()
		{
			UPECusHAWB.CurrentQueue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.EIR;
			UPECusHAWB.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.AEIR.Codes.S1_ShipperConsigneeDetailsInsufficient;
			UPECusHAWB.CurrentQueue.P4_CustomsSubStatus = StatusCodeDescriptionPairList.EmptyStatus;
			Factory.Save();
			BusinessObjectFactory factoryForTest = new BusinessObjectFactory();
			UPECusHAWB uPECusHAWBReLoaded = factoryForTest.Load<UPECusHAWB>(UPECusHAWB.PK);
			EIRBeenRaisedAnswer = false;
			try
			{
				uPECusHAWBReLoaded.CurrentQueue.AskHasEIRBeenRaised += new CancelEventHandler(OnCusHAWB_AskHasEIRBeenRaised);
				uPECusHAWBReLoaded.CS_GoodsDescription = "TEST";
				factoryForTest.Save();
				AssertEquals(Events.EditedARecord.Code, uPECusHAWBReLoaded.Logs.AutoCreatedLog.SL_SE_NKEvent);
				AssertEquals("EIR Not Raised", uPECusHAWBReLoaded.Logs.AutoCreatedLog.SL_Reference);
				AssertEquals(ZDateTime.Empty, uPECusHAWBReLoaded.CurrentQueue.P4_CustomDate4);
				EIRBeenRaisedAnswer = true;
				uPECusHAWBReLoaded.CS_GoodsDescription = "TEST1";
				factoryForTest.Save();
				AssertEquals(Events.EditedARecord.Code, uPECusHAWBReLoaded.Logs.AutoCreatedLog.SL_SE_NKEvent);
				AssertEquals("EIR Raised", uPECusHAWBReLoaded.Logs.AutoCreatedLog.SL_Reference);
				AssertEquals(new ZDateTime(2006, 5, 5), uPECusHAWBReLoaded.CurrentQueue.P4_CustomDate4);
			}
			finally
			{
				uPECusHAWBReLoaded.CurrentQueue.AskHasEIRBeenRaised -= new CancelEventHandler(OnCusHAWB_AskHasEIRBeenRaised);
			}
		}

		void OnCusHAWB_AskHasEIRBeenRaised(object sender, CancelEventArgs eventArgs)
		{
			eventArgs.Cancel = !EIRBeenRaisedAnswer;
		}

		bool EIRBeenRaisedAnswer;
		#endregion
		#region Implementation
		protected override Dictionary<string, IZType> CachedValueForSettingValueCallsRefreshBindingTestCore
		{
			get
			{
				Dictionary<string, IZType> result = base.CachedValueForSettingValueCallsRefreshBindingTestCore;
				result["HoldForCollectDepot"] = (ZString)HFCDepotCodeDescriptionPairList.Codes.Perth;
				return result;
			}
		}

		UPECusHAWB AirCargoThatCausesDecToBeCreated()
		{
			TaxOrFeeTestHelper.SetDeminimus(Factory, 250m);
			OrgHeader orgForMatching = GetOrgForMatching();
			UPECusHAWB result = Factory.New<UPECusHAWB>();
			result.CS_GoodsValue = 500m;
			result.CS_GoodsDescription = "test air cargo goods";
			result.CS_ConsigneeCity = orgForMatching.MainAddress.OA_City;
			result.CS_ConsigneeName = orgForMatching.OH_FullName;
			result.CS_ConsigneePhone = orgForMatching.MainAddress.OA_Phone;
			result.CS_ConsigneePostcode = orgForMatching.MainAddress.OA_PostCode;
			result.CS_ConsigneeState = orgForMatching.MainAddress.OA_State;
			result.CS_ConsigneeStreet = orgForMatching.MainAddress.OA_Address1;
			result.CS_ConsigneeStreet2 = orgForMatching.MainAddress.OA_Address2;
			result.CS_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Australia;
			result.CS_ConsignorCity = orgForMatching.MainAddress.OA_City;
			result.CS_ConsignorName = orgForMatching.OH_FullName;
			result.CS_ConsignorPhone = orgForMatching.MainAddress.OA_Phone;
			result.CS_ConsignorPostcode = orgForMatching.MainAddress.OA_PostCode;
			result.CS_ConsignorState = orgForMatching.MainAddress.OA_State;
			result.CS_ConsignorStreet = orgForMatching.MainAddress.OA_Address1;
			result.CS_ConsignorStreet2 = orgForMatching.MainAddress.OA_Address2;
			return result;
		}

		OrgPatternMatchAddress CreateNewAirCargoImporterAddress(UPECusHAWB airCargo)
		{
			OrgPatternMatchAddress result = Factory.New<OrgPatternMatchAddress>();
			result.P3_ParentID = airCargo.PK;
			result.P3_AddressType = OrgPatternMatchAddress.Constants.AddressType.AirCargoImporter;
			return result;
		}

		public UPECusHAWB UPECusHAWB
		{
			get
			{
				if (fUPECusHAWB == null)
				{
					fUPECusHAWB = (UPECusHAWB)MasterBill.ChildBills.AddNew();
					fUPECusHAWB.CS_RL_NKOrigin = "SGSIN";
					fUPECusHAWB.CS_RL_NKDestination = "AUSYD";
				}

				return fUPECusHAWB;
			}
		}

		UPECusHAWB fUPECusHAWB;
		CusMAWB MasterBill
		{
			get
			{
				if (fMasterBill == null)
				{
					fMasterBill = Factory.New<CusMAWB>();
					fMasterBill.CM_MAWB = "MAWB101";
					fMasterBill.CM_RL_NKDischargePort = "AUSYD";
				}

				return fMasterBill;
			}
		}

		CusMAWB fMasterBill;
		protected override BusinessObject GetNewBusinessObject()
		{
			CreateNewAirCargoImporterAddress(UPECusHAWB);
			return UPECusHAWB;
		}

		ZString currentRegNo;
		OrgHeader currentCompany;
		BusinessObjectFactory currentCompanyFactory;
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			currentCompanyFactory = new BusinessObjectFactory();
			currentRegNo = GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number;
			currentCompany = currentCompanyFactory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			currentCompany.PrimaryRegistrationNumber.Number = "21 003 980 130 123";
			currentCompanyFactory.Save();
			TaxOrFeeTestHelper.SetUp();
		}

		protected override void TearDown()
		{
			currentCompany.PrimaryRegistrationNumber.Number = currentRegNo;
			currentCompanyFactory.Save();
			base.TearDown();
		}
		#endregion
	}
}
