using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.AU.Declaration.Business.AUJobDeclarationLookups;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUJobDeclarationLookupsTest : TestCaseWithFactory
	{
		public void TestInvoicesToAttach()
		{
			AssertType(typeof(AUAttachInvoiceCollection), declaration.Lookups.InvoicesToAttach);
		}

		public void TestAllOrganisations()
		{
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(OrgHeaderCollection), declaration.Lookups.AllOrganisations.GetType());
		}

		public void TestAQISLoadingEstablishmentLocationOrganisations()
		{
			var organisations = declaration.Lookups.AQISLoadingEstablishmentLocationOrganisations;
			var filters = organisations.FilterBusinessObjectDefaults.ToList<FilterBusinessObjectDefault>();
			var expected = new[]
			{
				"Registration Country/Type.Property1:" + Core.Constants.CountryCodes.Australia,
				"Registration Country/Type.Property2:" + OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber
			};
			AssertContainsExactElementsInExactOrder(expected, filters.Select(d => $"{d.FilterName}.{d.PropertyName}:{d.Value}"));
		}

		public void TestPaymentPartyList()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CodeDescriptionPairList paymentList = declaration.Lookups.PaymentPartyList;
			AssertEquals("Contains Second Broker", true, paymentList.ContainsCode(JobDeclaration.PaymentMethods.SecondBroker));
			AssertEquals("Contains Cash", true, paymentList.ContainsCode(JobDeclaration.PaymentMethods.Cash));
		}

		public void TestPaymentPartyListForDreawback()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			CodeDescriptionPairList paymentList = declaration.Lookups.PaymentPartyList;
			AssertEquals("Contains Drawback Claimant", true, paymentList.ContainsCode(JobDeclaration.PaymentMethods.DrawbackClaimant));
			AssertEquals("Contains Broker", true, paymentList.ContainsCode(JobDeclaration.PaymentMethods.Broker));
			AssertEquals("Contains Second Broker", true, paymentList.ContainsCode(JobDeclaration.PaymentMethods.SecondBroker));
			AssertEquals("Does not contain Cash", false, paymentList.ContainsCode(JobDeclaration.PaymentMethods.Cash));
			AssertEquals("Does not contains ImporterCash", false, paymentList.ContainsCode(JobDeclaration.PaymentMethods.Importer));
		}

		public void TestMessageTypeList()
		{
			var messageTypeList = declaration.Lookups.MessageTypeList;
			AssertEquals("Contains Drawback", true, messageTypeList.ContainsCode(JobMessageTypeList.Codes.Drawback));
			AssertEquals("Doesn't contain Refund", false, messageTypeList.ContainsCode(JobMessageTypeList.Codes.Refund));

			AssertEquals("Contains Import", true, messageTypeList.ContainsCode(JobMessageTypeList.Codes.Import));
			AssertEquals("Contains Export", true, messageTypeList.ContainsCode(JobMessageTypeList.Codes.Export));

			AssertEquals("Contains ImportByExternalBroker", true, messageTypeList.ContainsCode(JobMessageTypeList.Codes.ImportDeclarationByExternalBroker));
			AssertEquals("Contains ExportByExternalBroker", true, messageTypeList.ContainsCode(JobMessageTypeList.Codes.ExportDeclarationByExternalBroker));
		}

		public void TestMessageTypeListForWEA()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Should not contain WEA because no importer is selected", false, declaration.Lookups.MessageTypeList.ContainsCode(JobMessageTypeList.Codes.WarehousedByExternalAgent));

			OrgHeader importerWithWEASupport = OrgHeader.New(Factory);
			importerWithWEASupport.FillWithValidTestData();
			importerWithWEASupport.CompanyData.OB_IMUsedBondedWhs = true;

			declaration.JE_OH_Importer = importerWithWEASupport.PK;
			AssertEquals("Should contain WEA because it is licenced and importer is setup", true, declaration.Lookups.MessageTypeList.ContainsCode(JobMessageTypeList.Codes.WarehousedByExternalAgent));

			OrgHeader importerWithoutWEASupport = OrgHeader.New(Factory);
			importerWithoutWEASupport.FillWithValidTestData();
			importerWithoutWEASupport.CompanyData.OB_IMUsedBondedWhs = false;

			declaration.JE_OH_Importer = importerWithoutWEASupport.PK;
			AssertEquals("Should not contain WEA because importer is not setup", false, declaration.Lookups.MessageTypeList.ContainsCode(JobMessageTypeList.Codes.WarehousedByExternalAgent));
		}

		public void TestMessageSubTypeListForCMRAndEdifice()
		{
			declaration.JE_DateOfFirstArrival = new ZDateTime(2004, 01, 01);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CodeDescriptionPairList list = declaration.Lookups.MessageSubTypeList;
			AssertEquals("!IsImportCMR", false, declaration.IsImportCMR);
			AssertEquals("Edifice list", 5, list.Count);
			AssertEquals("Contains SimplifiedEntry", "Simplified Entry", list.GetDescriptionFromCode(JobDeclaration.MessageSubType.SimplifiedEntry));

			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("IsImportCMR", true, declaration.IsImportCMR);
			list = declaration.Lookups.MessageSubTypeList;
			AssertEquals("CMR list", 6, list.Count);
			AssertEquals("Contains SelfAssessedClearance", "Self Assessed Clearance", list.GetDescriptionFromCode(JobDeclaration.MessageSubType.SelfAssessedClearance));
			AssertEquals("Contains SelfAssessedClearance", "Self assessed clearance with lines (for alcohol and tobacco)", list.GetDescriptionFromCode(JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Export list", 3, declaration.Lookups.MessageSubTypeList.Count);
		}

		public void TestMessageSubTypeListForCMRImportAndPost()
		{
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Mail;
			AssertEquals(5, declaration.Lookups.MessageSubTypeList.Count);
		}

		public void TestMessageSubTypeListForDrawback()
		{
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals(1, declaration.Lookups.MessageSubTypeList.Count);
			AssertEquals("Formal Entry", "Formal Entry", declaration.Lookups.MessageSubTypeList.GetDescriptionFromCode(JobDeclaration.MessageSubType.FormalEntry));
		}

		public void TestCustomsEntryList()
		{
			AssertEquals("Count", 44, declaration.Lookups.EntryStatusList.Count);
		}

		public void TestJE_ExportGoodsType_List()
		{
			AssertEquals("JE_ExportGoodsType_List.Count", 6, declaration.Lookups.JE_ExportGoodsType_List.Count);
		}

		public void TestEntryStatusList()
		{
			var consolidatedEntryStatusList = new ConsolidatedEntryStatusList().CodesAsString;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			AssertEquals("Entry status list", $"{new LegacyCustomsEntryStatusList().CodesAsString}, {consolidatedEntryStatusList}", declaration.Lookups.EntryStatusList.CodesAsString);

			declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("Entry status list", $"{new CMRImportEntryAdviceList().CodesAsString}, {consolidatedEntryStatusList}", declaration.Lookups.EntryStatusList.CodesAsString);
		}

		public void TestMessageStatusList()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			AssertEquals("Message list is empty", true, declaration.Lookups.MessageStatusList.Count == 0);

			declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			Assert("PreLodge message status should be in the list", declaration.Lookups.MessageStatusList.ContainsCode(CustomsEntryStatus.AwaitingPreLodge.Code));
			Assert("PreLodge message status should be in the list", declaration.Lookups.MessageStatusList.ContainsCode(CustomsEntryStatus.FailPreLodge.Code));
			Assert("PreLodge message status should be in the list", declaration.Lookups.MessageStatusList.ContainsCode(CustomsEntryStatus.ClearPreLodge.Code));
			Assert("WARREL message status should not be in the list", !declaration.Lookups.MessageStatusList.ContainsCode(CustomsEntryStatus.ClearWARRELOriginal.Code));

			declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			Assert("PreLodge message status should not be in the list", !declaration.Lookups.MessageStatusList.ContainsCode(CustomsEntryStatus.AwaitingPreLodge.Code));
			Assert("WARREL message status should be in the list", declaration.Lookups.MessageStatusList.ContainsCode(CustomsEntryStatus.ClearWARRELOriginal.Code));
			Assert("WARREL message status should be in the list", declaration.Lookups.MessageStatusList.ContainsCode(CustomsEntryStatus.FailWARRELOriginal.Code));
			Assert("WARREL message status should be in the list", declaration.Lookups.MessageStatusList.ContainsCode(CustomsEntryStatus.AwaitingWARRELOriginal.Code));

			declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			Assert("PreLodge message status should not be in the list", !declaration.Lookups.MessageStatusList.ContainsCode(CustomsEntryStatus.AwaitingPreLodge.Code));
			Assert("WARREL message status should not be in the list", !declaration.Lookups.MessageStatusList.ContainsCode(CustomsEntryStatus.ClearWARRELOriginal.Code));
			Assert("Clear original status should be in the list", declaration.Lookups.MessageStatusList.ContainsCode(CustomsEntryStatus.ClearOriginal.Code));
			Assert("Awaiting Replacement status should be in the list", declaration.Lookups.MessageStatusList.ContainsCode(CustomsEntryStatus.AwaitingReplacement.Code));
			Assert("Failed withdraw status should be in the list", declaration.Lookups.MessageStatusList.ContainsCode(CustomsEntryStatus.FailWithdrawal.Code));
			Assert("Lodged status should be in the list", declaration.Lookups.MessageStatusList.ContainsCode(CustomsEntryStatus.Lodged.Code));
		}

		public void TestCargoIdTypeList()
		{
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("One code for cargo id", 1, declaration.Lookups.CargoIdTypeList.Count);
			Assert("Contains AIR", declaration.Lookups.CargoIdTypeList.ContainsCode(Core.Constants.ContainerModes.AIR));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Four codes for cargo id", 5, declaration.Lookups.CargoIdTypeList.Count);
			Assert("Contains NCT", declaration.Lookups.CargoIdTypeList.ContainsCode(Core.Constants.ContainerModes.NonContainerised));
			Assert("Contains Combination", declaration.Lookups.CargoIdTypeList.ContainsCode(Core.Constants.ContainerModes.Combination));
			Assert("Contains BBK", declaration.Lookups.CargoIdTypeList.ContainsCode(Core.Constants.ContainerModes.Containerised));
			Assert("Contains BLK", declaration.Lookups.CargoIdTypeList.ContainsCode(Enterprise.Core.Constants.ContainerModes.Bulk));
			Assert("Contains LQD", declaration.Lookups.CargoIdTypeList.ContainsCode(Enterprise.Core.Constants.ContainerModes.Liquid));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_DateAtFinalDestination = new ZDateTime(2008, 12, 31);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			AssertEquals("Three codes for cargo id", 4, declaration.Lookups.CargoIdTypeList.Count);
			Assert("Contains Containerised", declaration.Lookups.CargoIdTypeList.ContainsCode(Core.Constants.ContainerModes.Containerised));
			Assert("Contains BBK", declaration.Lookups.CargoIdTypeList.ContainsCode(Core.Constants.ContainerModes.BreakBulk));
			Assert("Contains BLK", declaration.Lookups.CargoIdTypeList.ContainsCode(Enterprise.Core.Constants.ContainerModes.Bulk));
			Assert("Contains LQD", declaration.Lookups.CargoIdTypeList.ContainsCode(Enterprise.Core.Constants.ContainerModes.Liquid));

			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("Five codes for cargo id", 6, declaration.Lookups.CargoIdTypeList.Count);
			Assert("Contains FCL", declaration.Lookups.CargoIdTypeList.ContainsCode(Core.Constants.ContainerModes.FCL));
			Assert("Contains LCL", declaration.Lookups.CargoIdTypeList.ContainsCode(Enterprise.Core.Constants.ContainerModes.LCL));
			Assert("Contains FCX", declaration.Lookups.CargoIdTypeList.ContainsCode(Enterprise.Core.Constants.ContainerModes.FCLMixedShipper));
			Assert("Contains BBK", declaration.Lookups.CargoIdTypeList.ContainsCode(Core.Constants.ContainerModes.BreakBulk));
			Assert("Contains BLK", declaration.Lookups.CargoIdTypeList.ContainsCode(Enterprise.Core.Constants.ContainerModes.Bulk));
			Assert("Contains LQD", declaration.Lookups.CargoIdTypeList.ContainsCode(Enterprise.Core.Constants.ContainerModes.Liquid));

			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			AssertEquals("ond codes for cargo id", 1, declaration.Lookups.CargoIdTypeList.Count);
			Assert("Not containerised", declaration.Lookups.CargoIdTypeList.ContainsCode(Core.Constants.ContainerModes.NonContainerised));

			declaration.JE_TransportMode = Core.Constants.TransportModes.Other;
			AssertEquals("One code for cargo id of OTH", 1, declaration.Lookups.CargoIdTypeList.Count);
			Assert("The one code in the list is 'OTH'", declaration.Lookups.CargoIdTypeList.ContainsCode(Core.Constants.ContainerModes.Other));
		}

		public void TestApplicationCodeList() => CombineAssertions(() =>
		{
			var list = declaration.Lookups.ApplicationCodeList;
			AssertEquals("Elements", "CMR - Send CMR Message\r\nITF - Submit entry through designated Service Provider Interface", list.ElementsAsString);
			AssertSame("Cached", list, declaration.Lookups.ApplicationCodeList);
		});

		public void TestSeaDischargeList()
		{
			RefUNLOCO testLoco = Factory.New<RefUNLOCO>();
			testLoco.RL_Code = "AUXXX";
			testLoco.RL_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Factory.Save();

			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKPortOfArrival = testLoco.RL_Code;
			AssertEquals("This port is not valid as RefLocoMap doesnt have a local port code", true, declaration.JE_RL_NKPortOfArrivalInfo.HasMessageErrors());

			declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_RL_NKPortOfArrival = testLoco.RL_Code;
			AssertEquals("This port is not valid as RefLocoMap doesnt have a local port code", false, declaration.JE_RL_NKPortOfArrivalInfo.HasMessageErrors());

			declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			RefLocoMap testMap = Factory.New<RefLocoMap>();
			testMap.RY_LocalPortCode = "1W";
			testMap.RY_RL_NKLocoPort = testLoco.RL_Code;
			testMap.RY_RN = GlbCompany.CurrentCompany.Country.PK;
			testMap.RY_SystemUsage = Enterprise.Core.Constants.TransportModes.Sea;
			Factory.Save();

			declaration.JE_RL_NKPortOfArrival = testLoco.RL_Code;
			AssertEquals("This port is valid as RefLocoMap has a local port code now", false, declaration.JE_RL_NKPortOfArrivalInfo.HasMessageErrors());
		}

		public void TestAirDischargeList()
		{
			RefUNLOCO testLoco = Factory.New<RefUNLOCO>();
			testLoco.RL_Code = "AUXXX";
			testLoco.RL_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Factory.Save();

			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKPortOfArrival = testLoco.RL_Code;
			AssertEquals("This port is not valid as RefLocoMap doesnt have a local port code", true, declaration.JE_RL_NKPortOfArrivalInfo.HasMessageErrors());

			declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_RL_NKPortOfArrival = testLoco.RL_Code;
			AssertEquals("This port is not valid as RefLocoMap doesnt have a local port code", false, declaration.JE_RL_NKPortOfArrivalInfo.HasMessageErrors());

			declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			RefLocoMap testMap = Factory.New<RefLocoMap>();
			testMap.RY_LocalPortCode = "1W";
			testMap.RY_RL_NKLocoPort = testLoco.RL_Code;
			testMap.RY_RN = GlbCompany.CurrentCompany.Country.PK;
			testMap.RY_SystemUsage = Enterprise.Core.Constants.TransportModes.Air;
			Factory.Save();

			declaration.JE_RL_NKPortOfArrival = testLoco.RL_Code;
			AssertEquals("This port is valid as RefLocoMap has a local port code now", false, declaration.JE_RL_NKPortOfArrivalInfo.HasMessageErrors());
		}

		public void TestPostDischargeList()
		{
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Mail;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKPortOfArrival = "AUXXX";
			declaration.JE_RL_NKPortOfFirstArrival = "AUXXX";
			AssertHasMessageErrors("This port is not valid", declaration.JE_RL_NKPortOfArrivalInfo);
			AssertHasMessageErrors("This port is not valid", declaration.JE_RL_NKPortOfFirstArrivalInfo);

			declaration.JE_RL_NKPortOfArrival = "AUMAS";
			declaration.JE_RL_NKPortOfFirstArrival = "AUMAS";
			AssertNoMessageErrors("This port is valid", declaration.JE_RL_NKPortOfArrivalInfo);
			AssertNoMessageErrors("This port is valid", declaration.JE_RL_NKPortOfFirstArrivalInfo);

			declaration.JE_RL_NKPortOfArrival = "AUMEL";
			declaration.JE_RL_NKPortOfFirstArrival = "AUMEL";
			AssertNoMessageErrors("This port is valid", declaration.JE_RL_NKPortOfArrivalInfo);
			AssertNoMessageErrors("This port is valid", declaration.JE_RL_NKPortOfFirstArrivalInfo);

			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_RL_NKPortOfFirstArrival = "AUSYD";
			AssertNoMessageErrors("This port is valid", declaration.JE_RL_NKPortOfArrivalInfo);
			AssertNoMessageErrors("This port is valid", declaration.JE_RL_NKPortOfFirstArrivalInfo);

			declaration.JE_RL_NKPortOfArrival = "AUBNE";
			declaration.JE_RL_NKPortOfFirstArrival = "AUBNE";
			AssertNoMessageErrors("This port is valid", declaration.JE_RL_NKPortOfArrivalInfo);
			AssertNoMessageErrors("This port is valid", declaration.JE_RL_NKPortOfFirstArrivalInfo);

			declaration.JE_RL_NKPortOfArrival = "AUADL";
			declaration.JE_RL_NKPortOfFirstArrival = "AUADL";
			AssertNoMessageErrors("This port is valid", declaration.JE_RL_NKPortOfArrivalInfo);
			AssertNoMessageErrors("This port is valid", declaration.JE_RL_NKPortOfFirstArrivalInfo);

			declaration.JE_RL_NKPortOfArrival = "AUPER";
			declaration.JE_RL_NKPortOfFirstArrival = "AUPER";
			AssertNoMessageErrors("This port is valid", declaration.JE_RL_NKPortOfArrivalInfo);
			AssertNoMessageErrors("This port is valid", declaration.JE_RL_NKPortOfFirstArrivalInfo);
		}

		#region Transport Type List

		public void TestTransportTypeListForEdificeImport()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			AssertEquals("Transport Type List", 3, declaration.Lookups.TransportTypeList.Count);
		}

		public void TestTransportTypeListForCMR()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("Transport Type List for Import", 4, declaration.Lookups.TransportTypeList.Count);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Transport Type List for Export", 3, declaration.Lookups.TransportTypeList.Count);
		}

		#endregion

		#region Implementation

		JobDeclaration declaration;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = JobDeclaration.New(Factory);
		}

		#endregion
	}
}
