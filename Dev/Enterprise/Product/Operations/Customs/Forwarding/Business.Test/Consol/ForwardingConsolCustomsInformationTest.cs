using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingConsolCustomsInformation))]
	class ForwardingConsolCustomsInformationTest : NonPersistentBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestOutwardReportEntryNumberAndStatus()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_AgentType = Constants.AgentType.Agent;
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
			entryNumber.CE_EntryNum = "123456789";
			entryNumber.CE_ParentID = consol.PK;
			entryNumber.CE_EntryStatus = OutwardReportStatusList.Codes.Cleared;
			var information = new ForwardingConsolCustomsInformation(consol);
			NUnit.Framework.Assert.That(information.OutwardReportEntryNumber, Is.EqualTo("123456789").Using(CustomComparers.TypeComparison), "OutwardReportEntryNumber");
			NUnit.Framework.Assert.That(information.OutwardReportStatus, Is.EqualTo("CLR").Using(CustomComparers.TypeComparison), "OutwardReportStatus");
			NUnit.Framework.Assert.That(information.OutwardReportStatusDescription, Is.EqualTo("Cleared").Using(CustomComparers.TypeComparison), "OutwardReportStatusDescription");
		}

		[ExpectNoExceptions]
		public void TestOutwardReportAcknowledmentStatus()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_AgentType = Constants.AgentType.Agent;
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
			entryNumber.CE_EntryNum = "123456789";
			entryNumber.CE_ParentID = consol.PK;
			entryNumber.CE_EntryStatus = "";
			var information = new ForwardingConsolCustomsInformation(consol);
			NUnit.Framework.Assert.That(information.OutwardReportEntryNumber, Is.EqualTo("123456789").Using(CustomComparers.TypeComparison), "OutwardReportEntryNumber");
			NUnit.Framework.Assert.That(information.OutwardReportStatus, Is.EqualTo(OutwardReportStatusList.Codes.Acknowledgement).Using(CustomComparers.TypeComparison), "OutwardReportStatus should show ACK when OutwardReportEntry object exists but status is blank");
			NUnit.Framework.Assert.That(information.OutwardReportStatusDescription, Is.EqualTo(OutwardReportStatusList.Descriptions.Acknowledgement).Using(CustomComparers.TypeComparison), "OutwardReportStatusDescription");
		}

		[ExpectNoExceptions]
		public void TestORNStatusList()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			var information = new ForwardingConsolCustomsInformation(consol);
			NUnit.Framework.Assert.That(information.ORNStatusList.GetType(), Is.EqualTo(typeof(OutwardReportStatusList)), "ORNStatusList");
		}

		[ExpectNoExceptions]
		public void TestAMSBillStatus()
		{
			var carrier = Factory.New<US.IUSCarrierCombined>();
			carrier.UI_Code = "OTTA";
			carrier.UI_ModeOfTransportation = "10";
			var carrier2 = Factory.New<US.IUSCarrierCombined>();
			carrier2.UI_Code = "SCAC";
			carrier2.UI_ModeOfTransportation = "10";
			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTTB", Constants.CountryCodes.UnitedStates);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTTA", Constants.CountryCodes.UnitedStates);
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_OA_SendingForwarderAddress = org.MainAddress.PK;
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HB1";
			shipment1.HouseBillIssuingPartyDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
			var information = new ForwardingConsolCustomsInformation(consol);
			NUnit.Framework.Assert.That(information.AMSBillStatus, Is.EqualTo(AMSConsolBillCustomsStatusList.Codes.OutOfSync).Using(CustomComparers.TypeComparison), "There is no AMS done for this consol");
			NUnit.Framework.Assert.That(information.AMSBillStatusDescription, Is.EqualTo(AMSConsolBillCustomsStatusList.Descriptions.OutOfSync).Using(CustomComparers.TypeComparison), "There is no AMS done for this consol");
			var header = Factory.New<US.USAMS.ICusInBondHeader>();
			header.BH_OverrideFreightDefaults = true; // stop synchronisation
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var bill1 = Factory.New<US.USAMS.ICusInBondBill>();
			bill1.B0_BH = header.PK;
			bill1.B0_IssuerCode = "OTTA";
			bill1.B0_MasterBillNumber = "HB1";
			NUnit.Framework.Assert.That(information.AMSBillStatus, Is.EqualTo(AMSBillCustomsStatusList.Codes.NotOnFile).Using(CustomComparers.TypeComparison), "Should match to AMS Bill");
			NUnit.Framework.Assert.That(information.AMSBillStatusDescription, Is.EqualTo(AMSBillCustomsStatusList.Descriptions.NotOnFile).Using(CustomComparers.TypeComparison), "Should match to AMS Bill");
			consol.JK_TransportMode = Constants.TransportModes.Air;
			NUnit.Framework.Assert.That(information.AMSBillStatus, Is.EqualTo(ZString.Empty), "AMS currently only support Sea");
			NUnit.Framework.Assert.That(information.AMSBillStatusDescription, Is.EqualTo(ZString.Empty), "AMS currently only support Sea");
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			NUnit.Framework.Assert.That(information.AMSBillStatus, Is.EqualTo(AMSBillCustomsStatusList.Codes.NotOnFile).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(information.AMSBillStatusDescription, Is.EqualTo(AMSBillCustomsStatusList.Descriptions.NotOnFile).Using(CustomComparers.TypeComparison));
			consol.JK_RL_NKDischargePort = "AUSYD";
			NUnit.Framework.Assert.That(information.AMSBillStatus, Is.EqualTo(ZString.Empty), "Consol is not AMS consol as it doesn't go via/to US");
			NUnit.Framework.Assert.That(information.AMSBillStatusDescription, Is.EqualTo(ZString.Empty), "Consol is not AMS consol as it doesn't go via/to US");
			consol.JK_RL_NKDischargePort = "USLAX";
			NUnit.Framework.Assert.That(information.AMSBillStatus, Is.EqualTo(AMSBillCustomsStatusList.Codes.NotOnFile).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(information.AMSBillStatusDescription, Is.EqualTo(AMSBillCustomsStatusList.Descriptions.NotOnFile).Using(CustomComparers.TypeComparison));
			var bill2 = Factory.New<US.USAMS.ICusInBondBill>();
			bill2.B0_IssuerCode = "OTTA";
			bill2.B0_MasterBillNumber = "HB2";
			bill2.B0_BH = header.PK;
			NUnit.Framework.Assert.That(information.AMSBillStatus, Is.EqualTo(AMSConsolBillCustomsStatusList.Codes.OutOfSync).Using(CustomComparers.TypeComparison), "Number of AMS Bill Of Ladings do not match Consol shipments");
			NUnit.Framework.Assert.That(information.AMSBillStatusDescription, Is.EqualTo(AMSConsolBillCustomsStatusList.Descriptions.OutOfSync).Using(CustomComparers.TypeComparison), "Number of AMS Bill Of Ladings do not match Consol shipments");
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HB2";
			shipment2.HouseBillIssuingPartyDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
			NUnit.Framework.Assert.That(information.AMSBillStatus, Is.EqualTo(AMSBillCustomsStatusList.Codes.NotOnFile).Using(CustomComparers.TypeComparison), "All AMS Bill of Ladings are not on file");
			NUnit.Framework.Assert.That(information.AMSBillStatusDescription, Is.EqualTo(AMSBillCustomsStatusList.Descriptions.NotOnFile).Using(CustomComparers.TypeComparison), "All AMS Bill of Ladings are not on file");
			var bill2MovementDetail = Factory.LoadTop1<US.USAMS.ICusInBondMoveDetail>(new ZQuery(CusInBondMoveDetailSchema.B9_B0, bill2.PK));
			bill2MovementDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.OnFile;
			NUnit.Framework.Assert.That(information.AMSBillStatus, Is.EqualTo(AMSConsolBillCustomsStatusList.Codes.Multiple).Using(CustomComparers.TypeComparison), "1 AMS Bill of Lading is not on file while the other is");
			NUnit.Framework.Assert.That(information.AMSBillStatusDescription, Is.EqualTo(AMSConsolBillCustomsStatusList.Descriptions.Multiple).Using(CustomComparers.TypeComparison), "1 AMS Bill of Lading is not on file while the other is");
			var bill1MovementDetail = Factory.LoadTop1<US.USAMS.ICusInBondMoveDetail>(new ZQuery(CusInBondMoveDetailSchema.B9_B0, bill1.PK));
			bill1MovementDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.OnFile;
			NUnit.Framework.Assert.That(information.AMSBillStatus, Is.EqualTo(AMSBillCustomsStatusList.Codes.OnFile).Using(CustomComparers.TypeComparison), "All AMS Bill of Ladings are on file");
			NUnit.Framework.Assert.That(information.AMSBillStatusDescription, Is.EqualTo(AMSBillCustomsStatusList.Descriptions.OnFile).Using(CustomComparers.TypeComparison), "All AMS Bill of Ladings are on file");
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_OA_SendingForwarderAddress = org.MainAddress.PK;
			NUnit.Framework.Assert.That(information.AMSBillStatus, Is.EqualTo(AMSBillCustomsStatusList.Codes.OnFile).Using(CustomComparers.TypeComparison), "All AMS Bill of Ladings are on file and should match Consol Shipments based on Sending Agent details");
			NUnit.Framework.Assert.That(information.AMSBillStatusDescription, Is.EqualTo(AMSBillCustomsStatusList.Descriptions.OnFile).Using(CustomComparers.TypeComparison), "All AMS Bill of Ladings are on file and should match Consol Shipments based on Sending Agent details");
			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_RL_NKDischargePort = "USLAX";
			consol2.JK_AgentType = Constants.AgentType.Agent;
			consol2.JK_OA_SendingForwarderAddress = org.MainAddress.PK;
			var shipment21 = consol2.Shipments.AddNew();
			shipment21.JS_HouseBill = "OTTAHB1234567"; // "OTTA" is valid carrier code.
			var information2 = new ForwardingConsolCustomsInformation(consol2);
			var header2 = Factory.New<US.USAMS.ICusInBondHeader>();
			header2.BH_OverrideFreightDefaults = true; // stop synchronisation
			header2.BH_ParentID = consol2.PK;
			header2.BH_ParentTableCode = consol2.TablePrefix;
			var bill21 = Factory.New<US.USAMS.ICusInBondBill>();
			bill21.B0_BH = header2.PK;
			bill21.B0_IssuerCode = "OTTA";
			bill21.B0_MasterBillNumber = "HB1234567";
			Factory.InvalidateCachedProperties();
			NUnit.Framework.Assert.That(information2.AMSBillStatus, Is.EqualTo(AMSBillCustomsStatusList.Codes.NotOnFile).Using(CustomComparers.TypeComparison), "Should match to AMS Bill");
			NUnit.Framework.Assert.That(information2.AMSBillStatusDescription, Is.EqualTo(AMSBillCustomsStatusList.Descriptions.NotOnFile).Using(CustomComparers.TypeComparison), "Should match to AMS Bill");
			shipment21.JS_HouseBill = "BBBBHB123456";
			bill21.B0_IssuerCode = "BBBC";
			bill21.B0_MasterBillNumber = "HB123456";
			Factory.InvalidateCachedProperties();
			NUnit.Framework.Assert.That(information2.AMSBillStatus, Is.EqualTo(AMSConsolBillCustomsStatusList.Codes.OutOfSync).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(information2.AMSBillStatusDescription, Is.EqualTo(AMSConsolBillCustomsStatusList.Descriptions.OutOfSync).Using(CustomComparers.TypeComparison));
			bill21.B0_IssuerCode = "BBBB";
			Factory.InvalidateCachedProperties();
			NUnit.Framework.Assert.That(information2.AMSBillStatus, Is.EqualTo(AMSBillCustomsStatusList.Codes.NotOnFile).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(information2.AMSBillStatusDescription, Is.EqualTo(AMSBillCustomsStatusList.Descriptions.NotOnFile).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLatestAMSDispositionCodeAndDesc()
		{
			var helper = ObjectFactory.Get<Shared.Universal.IUniversalReferenceTestDataHelper>("Universal.IUniversalReferenceTestDataHelper", Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode, "AMSDD", "US");
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList("US", "AMSDD", "3U", "3U DESC", startDate, endDate);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var consol = newFactory.New<ForwardingConsol>();
			var header = newFactory.New<US.USAMS.ICusInBondHeader>();
			header.BH_ParentID = consol.PK;
			var bill = newFactory.New<US.USAMS.ICusInBondBill>();
			bill.B0_BH = header.PK;
			var dispositionCode = newFactory.New<US.IDispositionData>();
			dispositionCode.B7_ParentID = bill.PK;
			dispositionCode.B7_ParentTableCode = "B0";
			dispositionCode.US_Code = "3U";
			dispositionCode.US_DispositionDate = ZDateTime.Today;
			var information = new ForwardingConsolCustomsInformation(consol);
			NUnit.Framework.Assert.That(information.LatestAMSDispositionCode, Is.EqualTo("3U").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(information.LatestAMSDispositionCode, Is.EqualTo(header.BH_LatestDispositionCode));
			NUnit.Framework.Assert.That(information.LatestAMSDispositionDesc, Is.EqualTo("3U DESC").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(information.LatestAMSDispositionDesc, Is.EqualTo(header.BH_LatestDispositionCodeDescription));
		}

		[ExpectNoExceptions]
		public void TestAFRBillStatus()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "OTTAHB1";
			var information = new ForwardingConsolCustomsInformation(consol);
			NUnit.Framework.Assert.That(information.AFRBillStatus, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "There is no AFR done for this consol");
			NUnit.Framework.Assert.That(information.AFRBillStatusDescription, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "There is no AFR done for this consol");
			var header = Factory.New<JP.AFR.IJPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill1 = Factory.New<JP.AFR.IJPAFRBills>();
			bill1.JPB_JPH_Header = header.PK;
			bill1.JPB_BillNumber = "HB1";
			NUnit.Framework.Assert.That(information.AFRBillStatus, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "There is no AFR done for this consol");
			NUnit.Framework.Assert.That(information.AFRBillStatusDescription, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "There is no AFR done for this consol");
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "JPTKY";
			NUnit.Framework.Assert.That(information.AFRBillStatus, Is.EqualTo(AFRBillCustomsStatusList.Codes.NotRegistered).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(information.AFRBillStatusDescription, Is.EqualTo(AFRBillCustomsStatusList.Descriptions.NotRegistered).Using(CustomComparers.TypeComparison));
			var consol2 = Factory.New<ForwardingConsol>();
			var shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_HouseBill = "OTTAHB2";
			var information2 = new ForwardingConsolCustomsInformation(consol2);
			NUnit.Framework.Assert.That(information2.AFRBillStatus, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "There is no AFR done for this consol");
			NUnit.Framework.Assert.That(information2.AFRBillStatusDescription, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "There is no AFR done for this consol");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
			NUnit.Framework.Assert.That(information.AFRBillStatus, Is.EqualTo(AFRBillCustomsStatusList.Codes.NotRegistered).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(information.AFRBillStatusDescription, Is.EqualTo(AFRBillCustomsStatusList.Descriptions.NotRegistered).Using(CustomComparers.TypeComparison));
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			NUnit.Framework.Assert.That(information.AFRBillStatus, Is.EqualTo(AFRBillCustomsStatusList.Codes.Registered).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(information.AFRBillStatusDescription, Is.EqualTo(AFRBillCustomsStatusList.Descriptions.Registered).Using(CustomComparers.TypeComparison));
			var bill2 = Factory.New<JP.AFR.IJPAFRBills>();
			bill2.JPB_JPH_Header = header.PK;
			bill2.JPB_BillNumber = "HB2";
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			NUnit.Framework.Assert.That(information.AFRBillStatus, Is.EqualTo(AFRBillCustomsStatusList.Codes.Registered).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(information.AFRBillStatusDescription, Is.EqualTo(AFRBillCustomsStatusList.Descriptions.Registered).Using(CustomComparers.TypeComparison));
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
			NUnit.Framework.Assert.That(information.AFRBillStatus, Is.EqualTo("Multiple").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(information.AFRBillStatusDescription, Is.EqualTo("There are multiple bills with different statuses").Using(CustomComparers.TypeComparison));
			bill2.JPB_ReleaseStatus = ZString.Empty;
			NUnit.Framework.Assert.That(information.AFRBillStatus, Is.EqualTo("Multiple").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(information.AFRBillStatusDescription, Is.EqualTo("There are multiple bills with different statuses").Using(CustomComparers.TypeComparison));
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.HLD;
			NUnit.Framework.Assert.That(information.AFRBillStatus, Is.EqualTo("Multiple").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(information.AFRBillStatusDescription, Is.EqualTo("There are multiple bills with different statuses").Using(CustomComparers.TypeComparison));
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.HLD;
			NUnit.Framework.Assert.That(information.AFRBillStatus, Is.EqualTo(AFRBillCustomsStatusList.Codes.HLD).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(information.AFRBillStatusDescription, Is.EqualTo(AFRBillCustomsStatusList.Descriptions.HLD).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAsycudaRegistrationStatus()
		{
			var consol = Factory.New<ForwardingConsol>();
			var manifest1 = (BusinessObject)Factory.New<ASYCUDA.IAsycudaManifestHeader>();
			manifest1[AsycudaManifestHeaderSchema.AMA_ParentId] = consol.PK;
			manifest1[AsycudaManifestHeaderSchema.AMA_ParentTableCode] = JobConsolSchema.Constants.Prefix;
			manifest1.FillWithValidTestData();
			manifest1[AsycudaManifestHeaderSchema.AMA_JobReference] = "1";
			var asycudaRegistration1 = CusEntryNumber.LoadOrCreate(manifest1, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.Eritrea);
			asycudaRegistration1.CE_EntryStatus = Common.Shared.AsycudaRegistrationStatuses.Codes.Registered;
			var manifest2 = (BusinessObject)Factory.New<ASYCUDA.IAsycudaManifestHeader>();
			manifest2[AsycudaManifestHeaderSchema.AMA_ParentId] = consol.PK;
			manifest2[AsycudaManifestHeaderSchema.AMA_ParentTableCode] = JobConsolSchema.Constants.Prefix;
			manifest2.FillWithValidTestData();
			manifest2[AsycudaManifestHeaderSchema.AMA_JobReference] = "2";
			var asycudaRegistration2 = CusEntryNumber.LoadOrCreate(manifest2, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.Eritrea);
			asycudaRegistration2.CE_EntryStatus = Common.Shared.AsycudaRegistrationStatuses.Codes.Stored;
			var manifest3 = (BusinessObject)Factory.New<ASYCUDA.IAsycudaManifestHeader>();
			manifest3[AsycudaManifestHeaderSchema.AMA_ParentId] = consol.PK;
			manifest3[AsycudaManifestHeaderSchema.AMA_ParentTableCode] = JobConsolSchema.Constants.Prefix;
			manifest3.FillWithValidTestData();
			manifest3[AsycudaManifestHeaderSchema.AMA_JobReference] = "3";
			var asycudaRegistration3 = CusEntryNumber.LoadOrCreate(manifest3, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.Eritrea);
			asycudaRegistration3.CE_EntryStatus = string.Empty;
			Factory.Save();

			var information = new ForwardingConsolCustomsInformation(consol);
			NUnit.Framework.Assert.That(information.AsycudaRegistrationStatus, Is.EqualTo((ZString)"REG; STO"));
		}

		#region Implement
		protected override BusinessObject GetNewBusinessObject()
		{
			var consol = Factory.New<ForwardingConsol>();
			return new ForwardingConsolCustomsInformation(consol);
		}
		#endregion
	}
}
