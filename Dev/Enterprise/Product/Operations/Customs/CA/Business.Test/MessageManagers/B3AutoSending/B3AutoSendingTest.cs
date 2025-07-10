using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	sealed class B3AutoSendingTest : TestCaseWithFactory
	{
		[TestDate(2015, 04, 23, 12, 0, 0)]
		public void TestAutoB3SendingOfHV()
		{
			expectedSelectedDeclarations = new List<ZGuid>();

			var delayFactorRegistryBO = new DelayFactorRegistryBusinessObject();
			delayFactorRegistryBO.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.DAR;
			delayFactorRegistryBO.HVSDelayInterval = 3;
			CACustomsDataRegistry.Instance.EntryAutomaticSendingDelayThresholds.SetTemporaryValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, delayFactorRegistryBO);

			CreateDeclaration(true, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0));// selected: all criteria match for this HV shipment
			CreateDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).CA_B3AutoSend = false;// not selected: CA_B3AutoSend is false
			CreateDeclaration(false, branch2, new ZDateTime(2015, 04, 20, 12, 0, 0));// not selected: branch setting is default NON
			CreateDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).JE_MessageType = "EXP";// not selected: message type
			CreateDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).JE_MessageSubType = "NO";// not selected: not electronic type
			CreateDeclaration(true, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).JE_MessageSubType = "10";// selected: is electronic type
			CreateDeclaration(true, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).JE_MessageSubType = "13";// selected: is electronic type
			CreateDeclaration(true, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).JE_MessageSubType = "20";// selected: is electronic type
			CreateDeclaration(true, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).JE_MessageSubType = "21";// selected: is electronic type
			CreateDeclaration(true, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).JE_MessageSubType = "22";// selected: is electronic type
			CreateDeclaration(true, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).JE_MessageSubType = "30";// selected: is electronic type
			CreateDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).JE_EntryAuthorisationDate = ZDateTime.Empty;// not selected: not released
			CreateDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC).CH_MessageType = MessageTypeList.Codes.EDIRelease;// not selected: no B3 header
			CreateDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0), messageStatus: MessageStatusList.Codes.AwaitingOriginal);// not selected: waiting for response
			CreateDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0), entryStatus: B3EntryStatusList.Codes.Accepted);// not selected: already clear
			CreateDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).CA_K84AccountingDate = ZDateTime.Now;// not selected: reported on K84
			CreateDeclaration(true, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC).MergedLines[0].CL_CustomsValue--;// selected: LV shipment same with HVS
			CreateDeclaration(false, branch1, new ZDateTime(2015, 04, 21, 12, 0, 0));// not selected: threshold date
			CreateDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).JE_MessageStatus = MessageStatusList.Codes.Sent;// not selected: has been sent

			CreateDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).ImporterAddInfo.ZO_HVSDelayIntervalTypeAutoSend = DelayIntervalTypeCodes.Codes.None;// not selected: importer override NON
			var addInfo = CreateDeclaration(false, branch1, new ZDateTime(2015, 04, 22, 12, 0, 0)).ImporterAddInfo;// not selected: importer override DAR, 2 days only
			addInfo.ZO_HVSDelayIntervalTypeAutoSend = DelayIntervalTypeCodes.Codes.DAR;
			addInfo.ZO_HVSDelayIntervalAutoSend = 2;
			addInfo = CreateDeclaration(true, branch1, new ZDateTime(2015, 04, 21, 12, 0, 0)).ImporterAddInfo;// selected: importer override DAR, 2 days only, and meet threshold
			addInfo.ZO_HVSDelayIntervalTypeAutoSend = DelayIntervalTypeCodes.Codes.DAR;
			addInfo.ZO_HVSDelayIntervalAutoSend = 2;
			addInfo = CreateDeclaration(true, branch2, new ZDateTime(2015, 04, 21, 12, 0, 0)).ImporterAddInfo;// selected: same as previous with branch 2 with no registry override
			addInfo.ZO_HVSDelayIntervalTypeAutoSend = DelayIntervalTypeCodes.Codes.DAR;
			addInfo.ZO_HVSDelayIntervalAutoSend = 2;

			AssertSelectedResults("High Value selection test");
		}

		[TestDate(2015, 04, 23, 12, 0, 0)]
		public void TestAutoB3SendingOfLVDAR()
		{
			expectedSelectedDeclarations = new List<ZGuid>();

			var delayFactorRegistryBO = new DelayFactorRegistryBusinessObject();
			delayFactorRegistryBO.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.DAR;
			delayFactorRegistryBO.HVSDelayInterval = 3;
			CACustomsDataRegistry.Instance.EntryAutomaticSendingDelayThresholds.SetTemporaryValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, delayFactorRegistryBO);

			CreateLVDeclaration(true, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0));// selected: all criteria match for this LV shipment
			CreateLVDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).CA_B3AutoSend = false;// not selected: CA_B3AutoSend is false
			CreateLVDeclaration(false, branch2, new ZDateTime(2015, 04, 20, 12, 0, 0));// not selected: branch setting is default NON
			CreateLVDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).JE_MessageType = "EXP";// not selected: message type
			CreateLVDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).JE_MessageSubType = "NO";// not selected: not electronic type
			CreateLVDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).JE_EntryAuthorisationDate = ZDateTime.Empty;// not selected: not released
			CreateLVDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC).CH_MessageType = MessageTypeList.Codes.EDIRelease;// not selected: no B3 header
			CreateLVDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0), messageStatus: MessageStatusList.Codes.AwaitingOriginal);// not selected: waiting for response
			CreateLVDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0), entryStatus: B3EntryStatusList.Codes.Accepted);// not selected: already clear
			CreateLVDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).CA_K84AccountingDate = ZDateTime.Now;// not selected: reported on K84
			CreateLVDeclaration(true, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC).MergedLines[0].CL_CustomsValue++;// selected: HV LV shipment use HVSDelayInterval factor.
			CreateLVDeclaration(false, branch1, new ZDateTime(2015, 04, 21, 12, 0, 0));// not selected: threshold date
			CreateLVDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).JE_MessageStatus = MessageStatusList.Codes.Sent;// not selected: has been sent

			CreateLVDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).ImporterAddInfo.ZO_HVSDelayIntervalTypeAutoSend = DelayIntervalTypeCodes.Codes.None;// not selected: importer override NON
			var addInfo = CreateLVDeclaration(false, branch1, new ZDateTime(2015, 04, 22, 12, 0, 0)).ImporterAddInfo;// not selected: importer override DAR, 2 days only
			addInfo.ZO_HVSDelayIntervalTypeAutoSend = DelayIntervalTypeCodes.Codes.DAR;
			addInfo.ZO_HVSDelayIntervalAutoSend = 2;
			addInfo = CreateLVDeclaration(true, branch1, new ZDateTime(2015, 04, 21, 12, 0, 0)).ImporterAddInfo;// selected: importer override DAR, 2 days only, and meet threshold
			addInfo.ZO_HVSDelayIntervalTypeAutoSend = DelayIntervalTypeCodes.Codes.DAR;
			addInfo.ZO_HVSDelayIntervalAutoSend = 2;
			addInfo = CreateLVDeclaration(true, branch2, new ZDateTime(2015, 04, 21, 12, 0, 0)).ImporterAddInfo;// selected: same as previous with branch 2 with no registry override
			addInfo.ZO_HVSDelayIntervalTypeAutoSend = DelayIntervalTypeCodes.Codes.DAR;
			addInfo.ZO_HVSDelayIntervalAutoSend = 2;

			AssertSelectedResults("Low Value selection test, DAR method");
		}

		[TestDate(2015, 04, 23, 12, 0, 0)]
		public void TestAutoB3SendingOfCARCONLVSDAR()
		{
			expectedSelectedDeclarations = new List<ZGuid>();

			var delayFactorRegistryBO = new DelayFactorRegistryBusinessObject();
			delayFactorRegistryBO.CONDelayIntervalType = DelayIntervalTypeCodes.Codes.DAY;
			delayFactorRegistryBO.CONDelayInterval = 3;
			CACustomsDataRegistry.Instance.EntryAutomaticSendingDelayThresholds.SetTemporaryValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, delayFactorRegistryBO);

			CreateConVARLVSDeclaration(true, branch1, new ZDateTime(2015, 03, 20, 12, 0, 0));// selected: all criteria match for this HV shipment
			CreateConVARLVSDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).CA_B3AutoSend = false;// not selected: CA_B3AutoSend is false
			CreateConVARLVSDeclaration(false, branch2, new ZDateTime(2015, 04, 20, 12, 0, 0));// not selected: branch setting is default NON
			CreateConVARLVSDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).CA_LVSCloseDate = ZDateTime.Empty;// not selected: not closed
			CreateConVARLVSDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC).CH_MessageType = MessageTypeList.Codes.EDIRelease;// not selected: no B3 header
			CreateConVARLVSDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0), messageStatus: MessageStatusList.Codes.AwaitingOriginal);// not selected: waiting for response
			CreateConVARLVSDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0), entryStatus: B3EntryStatusList.Codes.Accepted);// not selected: already clear
			CreateConVARLVSDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).CA_K84AccountingDate = ZDateTime.Now;// not selected: reported on K84
			CreateConVARLVSDeclaration(true, branch1, new ZDateTime(2015, 03, 20, 12, 0, 0)).GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC).MergedLines[0].CL_CustomsValue--;// selected: value is irrelavant
			CreateConVARLVSDeclaration(false, branch1, new ZDateTime(2015, 04, 21, 12, 0, 0));// not selected: threshold date
			CreateConVARLVSDeclaration(false, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0)).JE_MessageStatus = MessageStatusList.Codes.Sent;// not selected: has been sent

			CreateConVARLVSDeclaration(true, branch1, new ZDateTime(2015, 03, 20, 12, 0, 0)).ImporterAddInfo.ZO_CONDelayIntervalTypeAutoSend = DelayIntervalTypeCodes.Codes.None;// selected: importer is ignored
			var addInfo = CreateConVARLVSDeclaration(true, branch1, new ZDateTime(2015, 03, 20, 12, 0, 0)).ImporterAddInfo;// selected: importer is ignored
			addInfo.ZO_CONDelayIntervalTypeAutoSend = DelayIntervalTypeCodes.Codes.DAY;
			addInfo.ZO_CONDelayIntervalAutoSend = 4;

			AssertSelectedResults("Consolidated LVS/VAR selection test, DAC method");
		}

		[TestDate(2015, 04, 23, 12, 0, 0)]
		public void TestAutoB3SendingMixture()
		{
			expectedSelectedDeclarations = new List<ZGuid>();

			var delayFactorRegistryBO = new DelayFactorRegistryBusinessObject();
			delayFactorRegistryBO.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.DAR;
			delayFactorRegistryBO.HVSDelayInterval = 3;
			CACustomsDataRegistry.Instance.EntryAutomaticSendingDelayThresholds.SetTemporaryValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, delayFactorRegistryBO);

			var delayFactorRegistryBO2 = new DelayFactorRegistryBusinessObject();
			delayFactorRegistryBO2.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.DAR;
			delayFactorRegistryBO2.HVSDelayInterval = 4;
			delayFactorRegistryBO2.CONDelayIntervalType = DelayIntervalTypeCodes.Codes.DAY;
			delayFactorRegistryBO2.CONDelayInterval = 2;
			CACustomsDataRegistry.Instance.EntryAutomaticSendingDelayThresholds.SetTemporaryValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, delayFactorRegistryBO2);
			CreateDeclaration(true, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0));// selected: all criteria match for this HV shipment
			CreateDeclaration(false, branch2, new ZDateTime(2015, 04, 20, 12, 0, 0));// not selected: threshold date
			CreateDeclaration(true, branch2, new ZDateTime(2015, 04, 17, 12, 0, 0));// selected: all criteria match for this HV shipment

			CreateLVDeclaration(true, branch1, new ZDateTime(2015, 04, 20, 12, 0, 0));// selected: all criteria match for this LV shipment
			CreateLVDeclaration(false, branch2, new ZDateTime(2015, 04, 20, 12, 0, 0));// not selected: branch setting is default NON
			CreateLVDeclaration(false, branch1, new ZDateTime(2015, 04, 21, 12, 0, 0));// not selected: threshold date

			CreateConMSILVSDeclaration(true, branch2, new ZDateTime(2015, 02, 20, 12, 0, 0));// selected: all criteria match for this Con LVS shipment
			CreateConMSILVSDeclaration(false, branch1, new ZDateTime(2015, 02, 20, 12, 0, 0));// not selected: branch setting is default NON
			CreateConMSILVSDeclaration(true, branch2, new ZDateTime(2015, 02, 20, 12, 0, 0)).GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC).MergedLines[0].CL_CustomsValue++;// selected: value is irrelavant
			CreateConMSILVSDeclaration(true, branch2, new ZDateTime(2015, 02, 21, 12, 0, 0));// selected: all criteria match for this Con LVS shipment
			CreateConMSILVSDeclaration(false, branch2, new ZDateTime(2015, 02, 20, 12, 0, 0)).ImporterAddInfo.ZO_CONDelayIntervalTypeAutoSend = DelayIntervalTypeCodes.Codes.None;// not selected: importer override NON
			var addInfo = CreateConMSILVSDeclaration(false, branch2, new ZDateTime(2015, 04, 20, 12, 0, 0)).ImporterAddInfo;// not selected: importer override DAS, 5 days only
			addInfo.ZO_CONDelayIntervalTypeAutoSend = DelayIntervalTypeCodes.Codes.None;
			addInfo.ZO_CONDelayIntervalAutoSend = 4;
			addInfo = CreateConMSILVSDeclaration(true, branch2, new ZDateTime(2015, 02, 17, 12, 0, 0)).ImporterAddInfo;// selected
			addInfo.ZO_CONDelayIntervalTypeAutoSend = DelayIntervalTypeCodes.Codes.DAY;
			addInfo.ZO_CONDelayIntervalAutoSend = 4;
			AssertSelectedResults("Mixture of declarations");
		}

		[TestDate(2015, 02, 18, 12, 0, 0)]
		public void TestAutoB3SendingPublicHoliday()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0497", "0497", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "ON");
			var officeCode2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0396", "0396", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "QC");
			Factory.Save();

			// Feb 16,2015 is a public holiday in ON, but not in QC
			expectedSelectedDeclarations = new List<ZGuid>();

			var delayFactorRegistryBO = new DelayFactorRegistryBusinessObject();
			delayFactorRegistryBO.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.DAR;
			delayFactorRegistryBO.HVSDelayInterval = 3;
			CACustomsDataRegistry.Instance.EntryAutomaticSendingDelayThresholds.SetTemporaryValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, delayFactorRegistryBO);

			CreateDeclaration(false, branch1, new ZDateTime(2015, 02, 13, 12, 0, 0));// not selected: because of the holoiday in ON
			CreateDeclaration(true, branch1, new ZDateTime(2015, 02, 13, 12, 0, 0)).JE_CustomsOffice = "0396";// selected: no holiday in QC

			AssertSelectedResults("Public holiday test");
		}

		List<ZGuid> expectedSelectedDeclarations;
		readonly List<ZGuid> allDeclarations = new List<ZGuid>();

		void AssertSelectedResults(string comment)
		{
			Factory.Save();

			// run query here and check that selected declarations are contained in expectedSelectedDeclarations, and that expectedSelectedDeclarations does not contain any not selected declarations.
			List<ZGuid> actualSelectedDeclarations = new List<ZGuid>();

			foreach (var branch in new GlbBranch.Loader(new BusinessObjectFactory()).LoadAllBranchesInThisCountryActiveOnly(Core.Constants.CountryCodes.Canada))
			{
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					new B3AutoSenderForTesting(actualSelectedDeclarations).Process();
				}
			}

			expectedSelectedDeclarations.Sort();
			actualSelectedDeclarations.Sort();
			AssertArrayEqualsByElements(comment, expectedSelectedDeclarations.ToArray(), actualSelectedDeclarations.ToArray());
		}

		JobDeclaration CreateDeclaration(bool expectedSelection, GlbBranch branch, ZDateTime releaseDate, string messageStatus = "", string entryStatus = "")
		{
			var result = Factory.New<JobDeclaration>();
			result.JE_MessageType = JobMessageTypeList.Codes.Import;
			result.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
			result.JE_MessageStatus = MessageStatusList.Codes.NotSent;
			result.JE_GB = branch.PK;
			result.JE_EntryAuthorisationDate = releaseDate;
			result.JE_CustomsOffice = "0497"; // toronto airport (ontario for provincial holiday purposes)
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			result.JE_OH_Importer = importer.PK;
			var entryHeader = result.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader.CH_EntryStatus = entryStatus;
			entryHeader.CH_Status = messageStatus;
			entryHeader.MergedLines.AddNew().CL_CustomsValue = 1501m;
			entryHeader.MergedLines.AddNew().CL_CustomsValue = 1000m;

			if (expectedSelection)
			{
				expectedSelectedDeclarations.Add(result.PK);
			}

			allDeclarations.Add(result.PK);
			result.Invoices.AddNew().InvoiceLines.AddNew();
			return result;
		}

		JobDeclaration CreateLVDeclaration(bool expectedSelection, GlbBranch branch, ZDateTime releaseDate, string messageStatus = "", string entryStatus = "")
		{
			var result = CreateDeclaration(expectedSelection, branch, releaseDate, messageStatus: messageStatus, entryStatus: entryStatus);
			result.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC).MergedLines[0].CL_CustomsValue--;
			return result;
		}

		JobDeclaration CreateConMSILVSDeclaration(bool expectedSelection, GlbBranch branch, ZDateTime releaseDate, string messageStatus = "", string entryStatus = "")
		{
			var result = CreateDeclaration(expectedSelection, branch, releaseDate, messageStatus: messageStatus, entryStatus: entryStatus);
			result.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			result.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			result.JE_EntryAuthorisationDate = releaseDate;
			return result;
		}

		JobDeclaration CreateConVARLVSDeclaration(bool expectedSelection, GlbBranch branch, ZDateTime releaseDate, string messageStatus = "", string entryStatus = "")
		{
			var result = CreateDeclaration(expectedSelection, branch, releaseDate, messageStatus: messageStatus, entryStatus: entryStatus);
			result.CA_LVSCloseDate = releaseDate;
			result.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			result.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			result.JE_EntryAuthorisationDate = releaseDate;
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();

			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);

			var caCompany1 = Factory.New<GlbCompany>();
			caCompany1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			caCompany1.GC_Code = "__A";
			caCompany1.GC_IsActive = true;
			branch1 = caCompany1.Branches.AddNew();
			branch1.GB_Code = "__A";
			branch1.GB_IsActive = true;
			branch2 = caCompany1.Branches.AddNew();
			branch2.GB_Code = "__B";
			branch2.GB_IsActive = true;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var taxOrFee = helper.CreateTaxOrFee(Enterprise.Customs.Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, 2500, Core.Constants.CountryCodes.Canada, new ZDateTime(2013, 01, 07), new ZDateTime(2079, 06, 06));
			CACustomsDataRegistry.Instance.ActivateAutoB3Sending.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Factory.Save();
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		IDisposable asecSetup;
		GlbBranch branch1;
		GlbBranch branch2;

		class B3AutoSenderForTesting : B3AutoSender
		{
			public B3AutoSenderForTesting(List<ZGuid> actualSelectedDeclarations)
				: base(new Integration.DummyLogger())
			{
				this.actualSelectedDeclarations = actualSelectedDeclarations;
			}

			readonly List<ZGuid> actualSelectedDeclarations;

			protected override bool SendMessage(JobDeclaration declaration, CusEntryHeader cusEntryheader)
			{
				base.SendMessage(declaration, cusEntryheader);
				actualSelectedDeclarations.Add(declaration.PK);
				return true;
			}
		}
	}
}
