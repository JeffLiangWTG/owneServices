using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(JobDeclaration))]
	public sealed class JobDeclarationTest : Customs.Business.Testing.BaseJobDeclarationAbstractTest
	{
		public void TestIsQueuedEntriesFunctionEnabled()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.PQENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Queued Entry Lodgements is not enabled when both FUNCS and PFUNC are not enabled", false, declaration.IsQueuedEntryLodgementsEnabled);
				AssertEquals("Queued Entry Payments is not enabled when both FUNCS and PFUNC are not enabled", false, declaration.IsQueuedEntryPaymentsEnabled);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.PQENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Queued Entry Lodgements is enabled when either FUNCS or PFUNC is enabled and it's import declaration", true, declaration.IsQueuedEntryLodgementsEnabled);
				AssertEquals("Queued Entry Payments is enabled when either FUNCS or PFUNC is enabled and it's import declaration", true, declaration.IsQueuedEntryPaymentsEnabled);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Queued Entry Lodgements is not enabled when either FUNCS or PFUNC is enabled but it's not import declaration", false, declaration.IsQueuedEntryLodgementsEnabled);
				AssertEquals("Queued Entry Payments is not enabled when either FUNCS or PFUNC is enabled but it's not import declaration", false, declaration.IsQueuedEntryPaymentsEnabled);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.PQENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Queued Entry Lodgements is enabled when either FUNCS or PFUNC is enabled and it's import declaration", true, declaration.IsQueuedEntryLodgementsEnabled);
				AssertEquals("Queued Entry Payments is enabled when either FUNCS or PFUNC is enabled and it's import declaration", true, declaration.IsQueuedEntryPaymentsEnabled);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Queued Entry Lodgements is not enabled when either FUNCS or PFUNC is enabled but it's not import declaration", false, declaration.IsQueuedEntryLodgementsEnabled);
				AssertEquals("Queued Entry Payments is not enabled when either FUNCS or PFUNC is enabled but it's not import declaration", false, declaration.IsQueuedEntryPaymentsEnabled);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.PQENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Queued Entry Lodgements is enabled when both FUNCS and PFUNC are enabled and it's import declaration", true, declaration.IsQueuedEntryLodgementsEnabled);
				AssertEquals("Queued Entry Payments is enabled when both FUNCS and PFUNC are enabled and it's import declaration", true, declaration.IsQueuedEntryPaymentsEnabled);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Queued Entry Lodgements is not enabled when both FUNCS and PFUNC are enabled but it's not import declaration", false, declaration.IsQueuedEntryLodgementsEnabled);
				AssertEquals("Queued Entry Payments is not enabled when both FUNCS and PFUNC are enabled but it's not import declaration", false, declaration.IsQueuedEntryPaymentsEnabled);
			}
		}

		public void TestJE_ApplicationCode_DefaultValue_NotOverrideLocalCountryCustomsInterfaceRegistryItem()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(ZString.Empty, declaration.JE_ApplicationCode);
		}

		public void TestJE_ApplicationCode_DefaultValue_OverrideLocalCountryCustomsInterfaceRegistryItem()
		{
			CombineAssertions(() =>
			{
				var customsInterface = new LocalCountryCustomsInterface();
				customsInterface.RecipientID = "RecipientID";
				customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Builtin;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					AssertEquals("BuiltIn", ZString.Empty, Factory.New<BaseJobDeclaration>().JE_ApplicationCode);
				}

				customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					AssertEquals("BothBuiltInDefaulted", ZString.Empty, Factory.New<BaseJobDeclaration>().JE_ApplicationCode);
				}

				customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					AssertEquals("Interfaced", DeclarationApplicationCodeList.Codes.Interfaced, Factory.New<BaseJobDeclaration>().JE_ApplicationCode);
				}

				customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					AssertEquals("BothInterfaceDefaulted", DeclarationApplicationCodeList.Codes.Interfaced, Factory.New<BaseJobDeclaration>().JE_ApplicationCode);
				}
			});
		}

		public void TestJE_ApplicationCode_JE_MessageTypeChanges_NotOverrideLocalCountryCustomsInterfaceRegistryItem()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals(ApplicationCodeList.Codes.AUCMR, declaration.JE_ApplicationCode);
		}

		public void TestJE_ApplicationCode_JE_MessageTypeChanges_OverrideLocalCountryCustomsInterfaceRegistryItem()
		{
			CombineAssertions(() =>
			{
				var customsInterface = new LocalCountryCustomsInterface();
				customsInterface.RecipientID = "RecipientID";
				customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Builtin;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
					AssertEquals("BuiltIn", ApplicationCodeList.Codes.AUCMR, declaration.JE_ApplicationCode);
				}

				customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
					AssertEquals("BothBuiltInDefaulted", ApplicationCodeList.Codes.AUCMR, declaration.JE_ApplicationCode);
				}

				customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
					AssertEquals("Interfaced", DeclarationApplicationCodeList.Codes.Interfaced, declaration.JE_ApplicationCode);
				}

				customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
					AssertEquals("BothInterfaceDefaulted", DeclarationApplicationCodeList.Codes.Interfaced, declaration.JE_ApplicationCode);
				}
			});
		}

		public void TestJE_ApplicationCode_ResetDeclaration_NotOverrideLocalCountryCustomsInterfaceRegistryItem()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = "XXX";
				declaration.ResetDeclaration();
				AssertEquals("EXP", ZString.Empty, declaration.JE_ApplicationCode);

				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = "XXX";
				declaration.ResetDeclaration();
				AssertEquals("IMP", ApplicationCodeList.Codes.AUCMR, declaration.JE_ApplicationCode);
			});
		}

		public void TestJE_ApplicationCode_ResetDeclaration_OverrideLocalCountryCustomsInterfaceRegistryItem()
		{
			CombineAssertions(() =>
			{
				var customsInterface = new LocalCountryCustomsInterface();
				customsInterface.RecipientID = "RecipientID";
				customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Builtin;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_ApplicationCode = "XXX";
					declaration.ResetDeclaration();
					AssertEquals("Builtin, EXP", ZString.Empty, declaration.JE_ApplicationCode);

					declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = "XXX";
					declaration.ResetDeclaration();
					AssertEquals("Builtin, IMP", ApplicationCodeList.Codes.AUCMR, declaration.JE_ApplicationCode);
				}

				customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_ApplicationCode = "XXX";
					declaration.ResetDeclaration();
					AssertEquals("BothBuiltInDefaulted, EXP", ZString.Empty, declaration.JE_ApplicationCode);

					declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = "XXX";
					declaration.ResetDeclaration();
					AssertEquals("BothBuiltInDefaulted, IMP", ApplicationCodeList.Codes.AUCMR, declaration.JE_ApplicationCode);
				}

				customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_ApplicationCode = "XXX";
					declaration.ResetDeclaration();
					AssertEquals("Interfaced, EXP", DeclarationApplicationCodeList.Codes.Interfaced, declaration.JE_ApplicationCode);

					declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = "XXX";
					declaration.ResetDeclaration();
					AssertEquals("Interfaced, IMP", DeclarationApplicationCodeList.Codes.Interfaced, declaration.JE_ApplicationCode);
				}

				customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_ApplicationCode = "XXX";
					declaration.ResetDeclaration();
					AssertEquals("BothInterfaceDefaulted, EXP", DeclarationApplicationCodeList.Codes.Interfaced, declaration.JE_ApplicationCode);

					declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = "XXX";
					declaration.ResetDeclaration();
					AssertEquals("BothInterfaceDefaulted, IMP", DeclarationApplicationCodeList.Codes.Interfaced, declaration.JE_ApplicationCode);
				}
			});
		}

		public void TestShowSubmitMenuItem()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = ZString.Empty;
				AssertEquals("Empty", false, declaration.ShowSubmitMenuItem);

				declaration.JE_ApplicationCode = ApplicationCodeList.Codes.AUCMR;
				AssertEquals("CMR", false, declaration.ShowSubmitMenuItem);

				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
				AssertEquals("ITF", true, declaration.ShowSubmitMenuItem);
			});
		}

		public void TestIsDeclarationIntegrated()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = ZString.Empty;
				AssertEquals("Empty", false, declaration.IsDeclarationIntegrated);

				declaration.JE_ApplicationCode = ApplicationCodeList.Codes.AUCMR;
				AssertEquals("CMR", false, declaration.IsDeclarationIntegrated);

				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
				AssertEquals("ITF", true, declaration.IsDeclarationIntegrated);
			});
		}

		protected override Type ExpectedMetadataType => typeof(Metadata.Business.AUJobDeclaration);

		public void TestDefaultJE_MergeByForExport()
		{
			var org = Factory.New<OrgHeader>();
			org.MiscServ.OM_EXMergeCustomsInvoiceLinesBy = OrgConstants.MergeInvoiceLines.Classification;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = org.PK;
			AssertNotEquals("JE_MergeBy should not default from OM_EXMergeCustomsInvoiceLinesBy when declaration is not EXP", OrgConstants.MergeInvoiceLines.Classification, declaration.JE_MergeBy);

			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Supplier = org.PK;
			AssertEquals("JE_MergeBy should default from OM_EXMergeCustomsInvoiceLinesBy when declaration is EXP", OrgConstants.MergeInvoiceLines.Classification, declaration.JE_MergeBy);
		}

		[DeveloperOnlyTest]
		public void TestIsNature10Performance()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;

			var invoiceLines = declaration.InvoiceLines;
			for (var i = 0; i < 5000; i++)
			{
				invoiceLines.AddNew();
			}

			var stopWatch = new Stopwatch();
			stopWatch.Start();
			for (var i = 0; i < invoiceLines.Count; i++)
			{
				_ = declaration.IsNature10;
			}
			AssertLessThan(stopWatch.ElapsedMilliseconds, 100);
		}

		public void TestIsECNNumberTransferred()
		{
			var declaration = CreateSendableDeclaration();
			declaration.JE_MessageType = AUJobMessageTypeList.Codes.Quarantine;

			declaration.JE_EntryStatus = CustomsEntryStatus.CargoCleared.Code;
			AssertEquals($"Should be false as the entry status is not {CustomsEntryStatus.Transferred.Code}.", false, declaration.IsECNNumberTransferred);

			declaration.JE_EntryStatus = CustomsEntryStatus.Transferred.Code;
			AssertEquals($"Should be true as the entry status is {CustomsEntryStatus.Transferred.Code}.", true, declaration.IsECNNumberTransferred);

			declaration.JE_MessageType = AUJobMessageTypeList.Codes.Import;
			AssertEquals($"Should be false as the message type is not {AUJobMessageTypeList.Codes.Quarantine}.", false, declaration.IsECNNumberTransferred);
		}

		public void TestIsDestinedOrTransitingThroughEU()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert("Default to false.", !declaration.IsDestinedOrTransitingThroughEU);

			declaration.JE_RL_NKFinalDestination = "DEHAM";
			Factory.InvalidateCachedProperties();

			Assert("Should be true as the final discharge port is in EU.", declaration.IsDestinedOrTransitingThroughEU);

			declaration.JE_RL_NKFinalDestination = "AUSYD";
			Factory.InvalidateCachedProperties();

			Assert("Should be false as the final discharge port is not in EU.", !declaration.IsDestinedOrTransitingThroughEU);

			var transport = declaration.Transports.AddNew("SGSIN", "DEHAM");
			Factory.InvalidateCachedProperties();

			Assert("Should be true as the discharge port of transport is in EU.", declaration.IsDestinedOrTransitingThroughEU);

			transport.JW_RL_NKLoadPort = "DEHAM";
			transport.JW_RL_NKDiscPort = "USCHI";
			Factory.InvalidateCachedProperties();

			Assert("Should be true as the load port of transport is in EU.", declaration.IsDestinedOrTransitingThroughEU);

			transport.JW_RL_NKLoadPort = "SGSIN";
			Factory.InvalidateCachedProperties();

			Assert("Should be false as there is no related port which is in EU.", !declaration.IsDestinedOrTransitingThroughEU);
		}

		public void TestIsNEXDOCSActive()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			Assert(!testDec.IsNEXDOCSActive);
			var invoice = testDec.Invoices.AddNew();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
				Assert(testDec.IsNEXDOCSActive);
				testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
				Assert(!testDec.IsNEXDOCSActive);
				testDec.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
				invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
				Assert(!testDec.IsNEXDOCSActive);
			}
		}

		public void TestJE_UseOwnerRefAsQuarantineRef()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_UseOwnerRefAsQuarantineRef = true;
			testDec.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			Assert(testDec.JE_UseOwnerRefAsQuarantineRef);
			var invoice = testDec.Invoices.AddNew();
			Assert(testDec.JE_UseOwnerRefAsQuarantineRef);
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			Assert(testDec.JE_UseOwnerRefAsQuarantineRef);
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			Assert(testDec.JE_UseOwnerRefAsQuarantineRef);
		}

		public void TestNotifyUserEmails()
		{
			var company = GlbCompany.CurrentCompany;
			var branchSyd = Factory.NewWithValidTestData<GlbBranch>();
			branchSyd.GB_Code = "SSS";
			branchSyd.GB_BranchName = "Sydney Test Branch";
			var branchMel = Factory.NewWithValidTestData<GlbBranch>();
			branchMel.GB_Code = "MMM";
			branchMel.GB_BranchName = "Melbourne Test Branch";
			company.Branches.Add(branchSyd);
			company.Branches.Add(branchMel);

			var sydGroup = Factory.NewWithValidTestData<GlbGroup>();
			sydGroup.GG_Code = "S_G";
			sydGroup.GG_Desc = "Sydney notification group";

			var melGroup = Factory.NewWithValidTestData<GlbGroup>();
			melGroup.GG_Code = "M_G";
			melGroup.GG_Desc = "Melbourne notification group";

			var sydStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			sydStaff1.GS_Code = "SS1";
			sydStaff1.GS_FullName = "John Anderson";
			sydStaff1.GS_GB_HomeBranch = branchSyd.PK;
			sydStaff1.GS_EmailAddress = "john.anderson@testcompany.org";
			sydGroup.Staff.Add(sydStaff1);

			var sydStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			sydStaff2.GS_Code = "SS2";
			sydStaff2.GS_FullName = "Bill Smith";
			sydStaff2.GS_GB_HomeBranch = branchSyd.PK;
			sydStaff2.GS_EmailAddress = "bill.smith@testcompany.org";
			sydGroup.Staff.Add(sydStaff2);
			Env.Registry.AUCustoms.EdificeSendAcknowledgementsToGroup = sydGroup.PK.ToGuid();

			var melStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			melStaff1.GS_Code = "MM1";
			melStaff1.GS_FullName = "Greg Johnson";
			melStaff1.GS_GB_HomeBranch = branchMel.PK;
			melStaff1.GS_EmailAddress = "greg.johnson@testcompany.org";
			melGroup.Staff.Add(melStaff1);
			Factory.Save();

			var sydDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			sydDeclaration.JE_GB = branchSyd.PK;
			AssertEquals("RegistryBranchPK by default will be current branch", branchSyd.PK, sydDeclaration.RegistryBranchPK);
			AssertEquals("There should be multiple mail recipients", true, sydDeclaration.MailRecipentsWhenDeliveryAddressChangedByFreight.Length > 1);
			foreach (string staffAddress in sydDeclaration.MailRecipentsWhenDeliveryAddressChangedByFreight)
			{
				AssertEquals("Should have sydney group emails", true, (staffAddress == "john.anderson@testcompany.org" || staffAddress == "bill.smith@testcompany.org"));
				AssertEquals("Should not have melbourne group email adresses", false, staffAddress == "greg.johnson@testcompany.org");
			}

			Env.Registry.AUCustoms.EdificeSendAcknowledgementsToGroup = melGroup.PK.ToGuid();
			var melDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			melDeclaration.JE_GB = branchMel.PK;
			Factory.Save();
			AssertEquals("RegistryBranchPK by default will be current branch", branchMel.PK, melDeclaration.RegistryBranchPK);
			foreach (string staffAddress in melDeclaration.MailRecipentsWhenDeliveryAddressChangedByFreight)
			{
				AssertEquals("Should not have sydney group emails", false, (staffAddress == "john.anderson@testcompany.org" || staffAddress == "bill.smith@testcompany.org"));
				AssertEquals("Should have melbourne group email adresses", true, staffAddress == "greg.johnson@testcompany.org");
			}
		}

		public void TestNotesNotReadOnly_WhenMessageStatusIsHoldAwaiting()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_EntryStatus = CustomsEntryStatus.FailCreate.Code;
			testDec.PlaceHold("Waiting for bill to put in the correct tariffs");
			Assert("ReadOnly", testDec.ReadOnly);
			AssertEquals("EntryStatus", CustomsEntryStatus.HoldAwaiting.Code, testDec.JE_EntryStatus);
			AssertEquals("HoldLogsCount", 1, testDec.HoldAwaitingLogs.Count);
			AssertEquals("HoldLogsDescription", "Reason: 'Waiting for bill to put in the correct tariffs'", testDec.HoldAwaitingLogs[0].SL_Reference);
			AssertEquals("ReadOnly", false, testDec.Notes.GetAllNotes().ReadOnly);

			Factory.Save();

			testDec = Factory.Load<JobDeclaration>(testDec.PK);
			Assert("ReadOnly", testDec.ReadOnly);
			AssertEquals("EntryStatus", CustomsEntryStatus.HoldAwaiting.Code, testDec.JE_EntryStatus);
			AssertEquals("HoldLogsCount", 1, testDec.HoldAwaitingLogs.Count);
			AssertEquals("HoldLogsDescription", "Reason: 'Waiting for bill to put in the correct tariffs'", testDec.HoldAwaitingLogs[0].SL_Reference);
			AssertEquals("ReadOnly", false, testDec.Notes.GetAllNotes().ReadOnly);

			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			testDec.RemoveHold();
			Assert(!testDec.IsHolding);
			Assert(!testDec.ReadOnly);
			AssertEquals(ZString.Empty, testDec.JE_MessageStatus);
			AssertEquals("ReadOnly", false, testDec.Notes.GetAllNotes().ReadOnly);

			testDec.JE_EntryStatus = CustomsEntryStatus.FailCreate.Code;
			testDec.PlaceHold("Waiting for bill to put in the correct tariffs");
			Assert("ReadOnly", testDec.ReadOnly);
			AssertEquals("EntryStatus", ZString.Empty, testDec.JE_EntryStatus);
			AssertEquals("MessageStatus", CustomsEntryStatus.HoldAwaiting.Code, testDec.JE_MessageStatus);
			AssertEquals("HoldLogsCount", 1, testDec.HoldAwaitingLogs.Count);
			AssertEquals("HoldLogsDescription", "Reason: 'Waiting for bill to put in the correct tariffs'", testDec.HoldAwaitingLogs[0].SL_Reference);
			AssertEquals("ReadOnly", false, testDec.Notes.GetAllNotes().ReadOnly);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var dec = Factory.New<JobDeclaration>();
			dec.JE_JS = shipment.PK;
			dec.JE_EntryStatus = CustomsEntryStatus.FailCreate.Code;
			dec.PlaceHold("Waiting for bill to put in the correct tariffs");
			Assert("ReadOnly", dec.ReadOnly);
			Assert("ReadOnly", !shipment.ReadOnly);
			AssertEquals("EntryStatus", CustomsEntryStatus.HoldAwaiting.Code, dec.JE_EntryStatus);
			AssertEquals("HoldLogsCount", 1, dec.HoldAwaitingLogs.Count);
			AssertEquals("HoldLogsDescription", "Reason: 'Waiting for bill to put in the correct tariffs'", dec.HoldAwaitingLogs[0].SL_Reference);
			AssertEquals("ReadOnly", false, dec.NotesOfDeclarationOrShipment.GetAllNotes().ReadOnly);
			AssertEquals("ReadOnly", false, shipment.Notes.GetAllNotes().ReadOnly);

			Factory.Save();
			dec = Factory.Load<JobDeclaration>(dec.PK);
			Assert("ReadOnly", dec.ReadOnly);
			Assert("ReadOnly", !shipment.ReadOnly);
			AssertEquals("EntryStatus", CustomsEntryStatus.HoldAwaiting.Code, dec.JE_EntryStatus);
			AssertEquals("HoldLogsCount", 1, dec.HoldAwaitingLogs.Count);
			AssertEquals("HoldLogsDescription", "Reason: 'Waiting for bill to put in the correct tariffs'", dec.HoldAwaitingLogs[0].SL_Reference);
			AssertEquals("ReadOnly", false, dec.NotesOfDeclarationOrShipment.GetAllNotes().ReadOnly);
			AssertEquals("ReadOnly", false, shipment.Notes.GetAllNotes().ReadOnly);

			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			dec.RemoveHold();
			Assert(!dec.IsHolding);
			Assert(!dec.ReadOnly);
			AssertEquals(ZString.Empty, dec.JE_MessageStatus);
			AssertEquals("ReadOnly", false, dec.NotesOfDeclarationOrShipment.GetAllNotes().ReadOnly);

			dec.JE_EntryStatus = CustomsEntryStatus.FailCreate.Code;
			dec.PlaceHold("Waiting for bill to put in the correct tariffs");
			Assert("ReadOnly", dec.ReadOnly);
			Assert("ReadOnly", !shipment.ReadOnly);
			AssertEquals("EntryStatus", ZString.Empty, dec.JE_EntryStatus);
			AssertEquals("MessageStatus", CustomsEntryStatus.HoldAwaiting.Code, dec.JE_MessageStatus);
			AssertEquals("HoldLogsCount", 1, dec.HoldAwaitingLogs.Count);
			AssertEquals("HoldLogsDescription", "Reason: 'Waiting for bill to put in the correct tariffs'", dec.HoldAwaitingLogs[0].SL_Reference);
			AssertEquals("ReadOnly", false, dec.NotesOfDeclarationOrShipment.GetAllNotes().ReadOnly);
			AssertEquals("ReadOnly", false, shipment.Notes.GetAllNotes().ReadOnly);
		}

		public void TestHasAtLeastOneLineFeeOfType()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert(!declaration.HasAtLeastOneLineFeeOfType(CusEntryChargeTypeList.Codes.GSTDeferred));

			var header1 = declaration.ActiveEntryHeaders.AddNew();
			var header2 = declaration.ActiveEntryHeaders.AddNew();
			Assert(!declaration.HasAtLeastOneLineFeeOfType(CusEntryChargeTypeList.Codes.GSTDeferred));
			var line1 = header1.MergedLines.AddNew();
			var line2 = header2.MergedLines.AddNew();
			var line3 = header2.MergedLines.AddNew();
			Assert(!declaration.HasAtLeastOneLineFeeOfType(CusEntryChargeTypeList.Codes.GSTDeferred));
			var fee1 = line1.Fees.AddNew();
			var fee2 = line3.Fees.AddNew();
			var fee3 = line3.Fees.AddNew();
			fee1.CF_ChargeType = CusEntryChargeTypeList.Codes.GSTAmount;
			fee1.CF_ChargeAmount = 1m;
			fee2.CF_ChargeType = CusEntryChargeTypeList.Codes.GSTAmount;
			fee2.CF_ChargeAmount = 1m;
			fee3.CF_ChargeType = CusEntryChargeTypeList.Codes.GSTAmount;
			fee3.CF_ChargeAmount = 1m;
			Assert(declaration.HasAtLeastOneLineFeeOfType(CusEntryChargeTypeList.Codes.GSTAmount));
			Assert(!declaration.HasAtLeastOneLineFeeOfType(CusEntryChargeTypeList.Codes.GSTDeferred));
			fee2.CF_ChargeType = CusEntryChargeTypeList.Codes.GSTDeferred;
			Assert(declaration.HasAtLeastOneLineFeeOfType(CusEntryChargeTypeList.Codes.GSTDeferred));
		}

		public void TesteDocAbleToBeAddedToReadOnlyDec()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.PlaceHold("TEST");
			Assert("Pre-Condition", declaration.IsHolding);
			Assert("Pre-Condition", declaration.ReadOnly);
			Factory.Save();

			var docManagerInfo = ((IDocManagerSupport)declaration).DocManagerInfo;
			AssertEquals("Pre-condition", 0, docManagerInfo.Files.Count);

			docManagerInfo.AddFileOrDocument(new byte[] { 5, 6, 7 }, "Formal Import Declaration for I00329.pdf", "CAU", false);
			AssertEquals("Document should be able to be dropped onto eDocs tab even if declaration is ReadOnly", 1, docManagerInfo.Files.Count);
			Factory.Save();

			var reLoadedDeclaration = Factory.Load<JobDeclaration>(declaration.PK);
			docManagerInfo = ((IDocManagerSupport)reLoadedDeclaration).DocManagerInfo;
			AssertEquals("Document should be able to be saved even if declaration is ReadOnly", 1, docManagerInfo.Files.Count);
			var eDocItem = docManagerInfo.AllEDocs[0];
			AssertEquals("DocType", "CAU", eDocItem.DocType);
			AssertEquals("Formal Import Declaration for I00329.pdf", eDocItem.FileName);
		}

		public void TestEDocsForSelection()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			var shipmentDocManagerInfo = ((IDocManagerSupport)shipment).DocManagerInfo;
			AssertEquals("Pre-condition", 0, shipmentDocManagerInfo.Files.Count);
			shipmentDocManagerInfo.AddFileOrDocument(new byte[] { 5, 6, 7 }, "Ship1111.pdf", Core.Constants.RefDocTypes.CustomsAuthority);
			AssertEquals(1, shipmentDocManagerInfo.Files.Count);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			var declarationDocManagerInfo = ((IDocManagerSupport)declaration).DocManagerInfo;
			AssertEquals("Pre-condition", 0, declarationDocManagerInfo.Files.Count);
			declarationDocManagerInfo.AddFileOrDocument(new byte[] { 5, 6, 7 }, "Dec2222.pdf", Core.Constants.RefDocTypes.QuarantineRemotePrint);
			AssertEquals(1, declarationDocManagerInfo.Files.Count);
			Factory.Save();

			var reLoadedDeclaration = Factory.Load<JobDeclaration>(declaration.PK);
			var eDocsForSelection = reLoadedDeclaration.EDocsForSelection.ToArray();
			AssertEquals(2, eDocsForSelection.Length);

			var decEDocs = eDocsForSelection[0];
			AssertEquals(1, decEDocs.Count);
			var decEDocItem = decEDocs[0];
			AssertEquals("DocType", "QRP", decEDocItem.DocType);
			AssertEquals("Dec2222.pdf", decEDocItem.FileName);

			var shipmentEDocs = eDocsForSelection[1];
			AssertEquals(1, shipmentEDocs.Count);
			var shipmentEDocItem = shipmentEDocs[0];
			AssertEquals("DocType", "CAU", shipmentEDocItem.DocType);
			AssertEquals("Ship1111.pdf", shipmentEDocItem.FileName);
		}

		[TestDate(2021, 06, 16, 11, 35, 24)]
		public void TestQuarantineCertificateNumbersForSelection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.Invoices.AddNew();
			var quarantineHeader = invoice.QuarantineExDocHeader;

			var cert1 = Factory.New<CusEntryNumber>();
			cert1.CE_ParentID = quarantineHeader.PK;
			cert1.CE_ParentTable = QuarantineExDocHeader.Schema.TableName;
			cert1.CE_EntryType = CusEntryNumberTypes.Australia.QuarantineCertificateNumber;
			cert1.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			cert1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			cert1.CE_EntryNum = "DEC1111";
			cert1.CE_IssueDate = ZDateTime.UtcNow;
			cert1.CE_SystemCreateTimeUtc = ZDateTime.UtcNow;

			var cert2 = Factory.New<CusEntryNumber>();
			cert2.CE_ParentID = quarantineHeader.PK;
			cert2.CE_ParentTable = QuarantineExDocHeader.Schema.TableName;
			cert2.CE_EntryType = CusEntryNumberTypes.Australia.QuarantineCertificateNumber;
			cert2.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			cert2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			cert2.CE_EntryNum = "DEC2222";
			cert2.CE_IssueDate = ZDateTime.UtcNow.AddMinutes(10);
			cert2.CE_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(10);

			var cert3 = Factory.New<CusEntryNumber>();
			cert3.CE_ParentID = quarantineHeader.PK;
			cert3.CE_ParentTable = QuarantineExDocHeader.Schema.TableName;
			cert3.CE_EntryType = CusEntryNumberTypes.Australia.QuarantineCertificateNumber;
			cert3.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			cert3.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			cert3.CE_EntryNum = "DEC2222";
			cert3.CE_IssueDate = ZDateTime.UtcNow.AddMinutes(15);
			cert3.CE_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(15);

			var cert4 = Factory.New<CusEntryNumber>();
			cert4.CE_ParentID = quarantineHeader.PK;
			cert4.CE_ParentTable = QuarantineExDocHeader.Schema.TableName;
			cert4.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			cert4.CE_EntryNum = "DEC4444";
			cert4.CE_IssueDate = ZDateTime.UtcNow.AddMinutes(20);
			cert4.CE_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(20);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var declarationIOF = otherFactory.Load<JobDeclaration>(declaration.PK);
			var certificates = declarationIOF.QuarantineCertificateNumbersForSelection;
			AssertEquals(3, certificates.Count);
			AssertEquals("DEC1111", certificates[0].Code);
			AssertEquals("16-Jun-21 21:35:00", certificates[0].Description);
			AssertEquals("DEC2222", certificates[1].Code);
			AssertEquals("16-Jun-21 21:45:00", certificates[1].Description);
			AssertEquals("DEC2222", certificates[2].Code);
			AssertEquals("16-Jun-21 21:50:00", certificates[2].Description);
		}

		public void TestLogsAreNotLoadedWhenAddInfoInstantiated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.PlaceHold("TEST");
			Assert("PreCondition", declaration.IsHolding);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var decLoaded = factory2.Load<JobDeclaration>(declaration.PK);
			var addInfoAccessed = decLoaded.AddInfo;

			var stmALogTableSelect = factory2.GetTableHitCount(StmALogSchema.Constants.TableName);
			AssertEquals(0, stmALogTableSelect);
			AssertEquals("AddInfo.Readonly should reflect JobDeclaration.ReadOnly without being set specifically", true, decLoaded.AddInfo.ReadOnly);
		}

		public void TestAutoApportionOfWeightIsStoppedForExWarehouse()
		{
			using (CustomsDataRegistry.Instance.EnableAutoApportionWeight.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, GlbDepartment.CurrentDepartment.PK.ToGuid(), true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				AssertEquals(false, declaration.IsWeightApportionmentSupported);

				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 1000m;
				invoice.JZ_RX_NKInvoice_Currency = "AUD";

				invoice.JZ_Weight = 100m;//not visible on the form, but imported via some methods of data transfer
				invoice.JZ_WeightUQ = "KG";

				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 1000m;
				invoiceLine.JI_Weight = 101m;//entered by users
				invoiceLine.JI_WeightUQ = "KG";

				var invoiceLine2 = declaration.InvoiceLines.AddNew();
				invoiceLine2.JI_LinePrice = 1000m;
				invoiceLine2.JI_Weight = 102m;//entered by users
				invoiceLine2.JI_WeightUQ = "KG";

				AssertEquals("Apportion is not run because we should not override what users have entered with values that are not visible on the form", 101m, invoiceLine.JI_Weight);
				AssertEquals("Apportion is not run because we should not override what users have entered with values that are not visible on the form", 102m, invoiceLine2.JI_Weight);
			}
		}

		public void TestSubmitWeeklyNilReturnN30()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SettlementPeriodType = "SW";
			declaration.NilReturnInd = true;
			Assert(declaration.SubmitWeeklyNilReturnN30);
		}

		public void TestDoMergeWithoutInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			declaration.JE_SettlementPeriodType = "SW";
			declaration.NilReturnInd = true;
			Assert(declaration.DoMerge());
		}

		public void TestNotThrowAwayMergeWhenDeclaraitonIsExport()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_DeclarationReference = "B99999999";
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			testDec.JE_MessageStatus = "AAA";
			var invoice = testDec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var merger = new LineMerger(testDec);
			merger.DoMerge();
			AssertEquals("Pre-condition", true, testDec.IsMergeDone);

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("Still keep merge", "AAA", testDec.JE_MessageStatus);
		}

		public void TestIsWHSUniversalXMLActive()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			var declaration = Factory.New<JobDeclaration>();
			Assert(declaration.IsWHSUniversalXMLActive);

			declaration.JE_SettlementPeriodType = "SW";
			declaration.NilReturnInd = true;
			Assert(!declaration.IsWHSUniversalXMLActive);
		}

		public void TestPartShipConsignmentReferenceNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = "AIR";
			declaration.JE_PartShipConsignmentReference = "X1";

			declaration.JE_TransportMode = "SEA";
			AssertEquals(ZString.Empty, declaration.JE_PartShipConsignmentReference);

			declaration.JE_TransportMode = "AIR";
			declaration.JE_PartShipConsignmentReference = "X1";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(ZString.Empty, declaration.JE_PartShipConsignmentReference);
		}

		public void TestWarehouseAddressRequirement()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_IsPackToBondForLine = true;
			declaration.WarehouseDocAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(declaration.WarehouseDocAddress.OrganisationPKInfo, JobDeclaration.EnterAWarehouseMessageError);

			var org = Factory.New<OrgHeader>();
			var address1 = org.MainAddress;
			var address2 = org.Addresses.AddNew();
			address2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "4323", Core.Constants.CountryCodes.Australia);
			declaration.WarehouseDocAddress.E2_OA_Address = address1.PK;
			declaration.WarehouseDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(declaration.WarehouseDocAddress.OrganisationPKInfo, JobDeclaration.EnterAWarehouseMessageError);
			AssertHasMessageError(declaration.WarehouseDocAddress.OrganisationPKInfo, JobDeclaration.NoCCPMessageError);

			declaration.WarehouseDocAddress.E2_OA_Address = address2.PK;
			declaration.WarehouseDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(declaration.WarehouseDocAddress.OrganisationPKInfo, JobDeclaration.EnterAWarehouseMessageError);
			AssertNoMessageError(declaration.WarehouseDocAddress.OrganisationPKInfo, JobDeclaration.NoCCPMessageError);
		}

		public void TestJE_PartShipConsignmentReference()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = "AIR";
			declaration.JE_PartShipConsignmentReference = "X1";
			AssertNotNull(declaration.PrimaryHouseBill);

			declaration.JE_HouseBill = "V1";
			AssertEquals(1, declaration.Bills.Count);
			AssertEquals("V1", declaration.PrimaryHouseBill.CU_BillNum);

			declaration.JE_PartShipConsignmentReference = "X2";
			AssertEquals("X2", declaration.PrimaryHouseBill.CU_fPartShipConsignmentReference);
		}

		public void TestIDataExportCSVFileNameProviderMembers()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var dec = Factory.New<JobDeclaration>();
			dec.JE_JS = shipment.PK;
			dec.JE_DeclarationReference = "B111";
			dec.JE_HouseBill = "HS001";

			var fileNameProvider = dec as IDataExportCSVFileNameProvider;
			AssertEquals("File name suffix", "B111", fileNameProvider.FileNameSuffix);

			dec.JE_DeclarationReference = string.Empty;
			AssertEquals("File name suffix", "HS001", fileNameProvider.FileNameSuffix);
		}

		public void TestApportionmentInbalance_CS00149967()
		{
			new ZTestHelper(Factory).SetExchangeRate(ZDateTime.Today, ZDateTime.Today, 1.0757m, Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD")));

			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_ValuationDateOverride = ZDateTime.Today;

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.Charges.AddNew("OFT", 0.05m, "USD");

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.Charges.AddNew("OFT", 1.33m, "USD");
			invoiceLine2.Charges.AddNew("OFT", 0.05m, "AUD");
			invoiceLine2.Charges.AddNew("OFT", 0.08m, "AUD");

			declaration.ResumeApportionment();

			AssertEquals("Invoice has an aggregated amount", 1, invoice.GroupCharges.GetCharge("OFT").Length);

			string errorMessage;
			Assert("Balanced", declaration.Invoices.AreChargesBalancedForInvoices(out errorMessage));
		}

		public void TestIMessageManageableBizObj()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Factory.Save();
			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.MessageInitiator = sender;

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;

			IBackDoorSavingSupportableBizObj bizObj = declaration;
			AssertEquals(typeof(CMRAmendmentWithdrawalReason), bizObj.GetAmendmentWithdrawalReason().GetType());
			AssertEquals(typeof(IMDMultiMessageManager), bizObj.GetMessageManagerForAmendmentDetection().GetType());

			IMDMultiMessageManager manager = (IMDMultiMessageManager)bizObj.GetMessageManagerForAmendmentDetection();
			AssertEquals(CMRMessageTypes.OriginalForAmendmentDetection, manager.MessageType);

			AssertEquals(false, bizObj.SupportBackDoorForSavingWhenAmendmentDetected);
			AssertEquals("ShouldCheckAmendment for export", false, bizObj.IsInAStatusAmendmentSendable);
			AssertEquals("Nothing to process for export and wants to keep going", ContinueWithDetection.Yes, bizObj.ProcessBeforeDetectingAmendmentAndContinue());

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			AssertEquals(true, bizObj.SupportBackDoorForSavingWhenAmendmentDetected);
			AssertEquals("ShouldCheckAmendment for import MessageStatus not right", false, bizObj.IsInAStatusAmendmentSendable);
			AssertEquals(ContinueWithDetection.Yes, bizObj.ProcessBeforeDetectingAmendmentAndContinue());

			declaration.JE_MessageStatus = "AAA";
			declaration.JE_EntryStatus = CMRImportEntryAdvice.Withdrawn.Code;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("ShouldCheckAmendment for import CustomsStatus not right", false, bizObj.IsInAStatusAmendmentSendable);

			declaration.JE_EntryStatus = ZString.Empty;

			AssertEquals("ShouldCheckAmendment for import", true, bizObj.IsInAStatusAmendmentSendable);
			AssertEquals("No invoices exist to merge", ContinueWithDetection.No, bizObj.ProcessBeforeDetectingAmendmentAndContinue());
			AssertEquals(Core.Constants.Customs.MergeErrors.ReasonCannotMergeNoInvoiceHeaders, sender.InvalidOperationText);

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			sender.InvalidOperationText = "";
			AssertEquals(ContinueWithDetection.Yes, bizObj.ProcessBeforeDetectingAmendmentAndContinue());
			AssertEquals("", sender.InvalidOperationText);

			AssertEquals("invoice line is merged", true, declaration.InvoiceLines[0].JI_CL.IsValid);
		}

		public void TestSupportJE_PaymentMethodUsage()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(true, declaration.SupportJE_PaymentMethodUsage);
		}

		public void TestDefaultingNumberOfPacksToPivot()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_HouseBill = "HB1";
			declaration.JE_TotalNoOfPacks = 123;
			AssertEquals("House Bill COntainers", 0, declaration.PackingGroups.Count);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_HouseBill = "HB1";
			declaration.JE_TotalNoOfPacks = 123;
			AssertEquals("House Bill COntainers", 1, declaration.PackingGroups.Count);
			AssertEquals("Packs defaulted", 123, declaration.PackingGroups[0].TotalNumberOfPackages);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.JE_HouseBill = "HB1";
			declaration.JE_TotalNoOfPacks = 123;
			AssertEquals("House Bill COntainers", 0, declaration.PackingGroups.Count);
		}

		public void TestDefaultingNumberOfPacksWithNoPivot()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TotalNoOfPacks = 123;
			AssertEquals("House Bill containers", 0, declaration.PackingGroups.Count);

			declaration.JE_HouseBill = "House Bill";
			AssertEquals(declaration.JE_HouseBill, declaration.PrimaryHouseBill.CU_HouseBill);
			AssertEquals("House Bill containers", 1, declaration.PackingGroups.Count);
			var packingGroup = declaration.PackingGroups[0];
			AssertEquals("Packs defaulted", declaration.PrimaryHouseBill, packingGroup.Bill);
			AssertEquals("Packs defaulted", 1, packingGroup.Packages.Count);
			var package = packingGroup.Packages[0];
			AssertEquals(123, package.CW_PackQty);
		}

		public void TestDontDefaultNoOfPacksIfWarehouseFilledIn()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_HouseBill = "House Bill";
			declaration.JE_TotalNoOfPacks = 123;

			AssertEquals("Count of pivot rows", 1, declaration.PackingGroups.Count);

			Package pack = declaration.Packages[0];
			PackingGroup packGroup = declaration.PackingGroups[0];
			AssertEquals("Packs defaulted", 123, packGroup.TotalNumberOfPackages);

			pack.CW_InBondPackQty = 123;
			pack.CW_PackQty = 0;
			Factory.Save();
			AssertEquals("Packs not defaulted", 0, packGroup.TotalNumberOfPackages);
			AssertEquals("Warehouse Packs", 123, packGroup.WarehouseNumberOfPackages);

			declaration.JE_TotalNoOfPacks = 150;
			Factory.Save();
			AssertEquals("Packs not defaulted", 0, packGroup.TotalNumberOfPackages);
			AssertEquals("Warehouse Packs", 123, packGroup.WarehouseNumberOfPackages);
		}

		public void TestDontDefaultNoOfPacksIfWarehouseFilledInOnMerge()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_HouseBill = "House Bill";
			declaration.JE_TotalNoOfPacks = 123;

			AssertEquals("Count of pivot rows", 1, declaration.PackingGroups.Count);

			Package pack = declaration.Packages[0];
			PackingGroup packGroup = declaration.PackingGroups[0];
			AssertEquals("Packs defaulted", 123, packGroup.TotalNumberOfPackages);

			pack.CW_InBondPackQty = 123;
			pack.CW_PackQty = 0;
			declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			AssertEquals("Packs not defaulted", 0, packGroup.TotalNumberOfPackages);
			AssertEquals("Warehouse Packs", 123, packGroup.WarehouseNumberOfPackages);

			declaration.JE_TotalNoOfPacks = 150;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			AssertEquals("Packs not defaulted", 0, packGroup.TotalNumberOfPackages);
			AssertEquals("Warehouse Packs", 123, packGroup.WarehouseNumberOfPackages);
		}

		public void TestBBKPackLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			declaration.JE_MasterBill = "FOO100";
			declaration.JE_TotalNoOfPacks = 1;
			AssertEquals("precondition", 1, declaration.Bills.Count);
			AssertEquals("precondition", 1, declaration.PackingGroups.Count);
			AssertEquals("precondition", 0, declaration.CusContainers.Count);

			declaration.CusContainers.AddNew();
			declaration.CusContainers.RemoveAndDeleteAll();

			AssertEquals(1, declaration.Bills.Count);
			AssertEquals(1, declaration.PackingGroups.Count);
			AssertEquals(0, declaration.CusContainers.Count);
			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });
		}

		public void TestBulkPackLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
			declaration.JE_MasterBill = "FOO100";
			declaration.JE_TotalNoOfPacks = 1;

			AssertEquals("precondition", 1, declaration.Bills.Count);
			AssertEquals("precondition", 1, declaration.PackingGroups.Count);
			AssertEquals("precondition", 0, declaration.CusContainers.Count);

			declaration.CusContainers.AddNew();
			declaration.CusContainers.RemoveAndDeleteAll();

			AssertEquals(1, declaration.Bills.Count);
			AssertEquals(1, declaration.PackingGroups.Count);
			AssertEquals(0, declaration.CusContainers.Count);
			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });
		}

		public void TestLiquidPackLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
			declaration.JE_MasterBill = "FOO100";
			declaration.JE_TotalNoOfPacks = 1;

			AssertEquals("precondition", 1, declaration.Bills.Count);
			AssertEquals("precondition", 1, declaration.PackingGroups.Count);
			AssertEquals("precondition", 0, declaration.CusContainers.Count);

			declaration.CusContainers.AddNew();
			declaration.CusContainers.RemoveAndDeleteAll();

			AssertEquals(1, declaration.Bills.Count);
			AssertEquals(1, declaration.PackingGroups.Count);
			AssertEquals(0, declaration.CusContainers.Count);
			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });
		}

		public void TestTransportModeEnum()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Transport mode", TransportModeEnum.Sea, declaration.TransportModeEnum);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Transport mode", TransportModeEnum.Air, declaration.TransportModeEnum);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			AssertEquals("Transport mode", TransportModeEnum.Post, declaration.TransportModeEnum);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Other;
			AssertEquals("Transport mode", TransportModeEnum.Other, declaration.TransportModeEnum);

			declaration.JE_TransportMode = "";
			AssertEquals("Transport mode", TransportModeEnum.Undefined, declaration.TransportModeEnum);
		}

		public void TestIMessageAttacheeHolder()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();

			AssertEquals("IsImportCMR", true, testDec.IsImportCMR);
			AssertEquals("MessageAttachees", 1, ((IMessageAttacheeParent)testDec).MessageAttachees.Length);
			AssertEquals("EntryHeader is", entryHeader, ((IMessageAttacheeParent)testDec).MessageAttachees[0]);

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("IsImportCMR", false, testDec.IsImportCMR);
			AssertEquals("MessageAttachees", 0, ((IMessageAttacheeParent)testDec).MessageAttachees.Length);
		}

		public void TestIsImportCMR_Interface()
		{
			var decalaration = Factory.New<JobDeclaration>();
			decalaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			decalaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			AssertEquals(true, decalaration.IsImportCMR);
		}

		public void TestIsImportEdifice()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("IsImport Edifice", false, testDec.IsImportEdifice);

			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			AssertEquals("IsImport Edifice", false, testDec.IsImportEdifice);

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("IsImportEdifice", true, testDec.IsImportEdifice);

			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("IsImportEdifice", false, testDec.IsImportEdifice);
		}

		public void TestJE_PaidUnderProtestStatement()
		{
			testJobDeclaration.JE_ApplicationCode = "CMR";
			AssertEquals("Paid Under Protest Statement", ZString.Empty, testJobDeclaration.JE_PaidUnderProtestStatement);

			testJobDeclaration.JE_PaidUnderProtestStatement = "Test";
			AssertEquals("Paid Under Protest Statement", "Test", testJobDeclaration.JE_PaidUnderProtestStatement);
			AssertEquals("Has one note", 1, testJobDeclaration.Notes.GetAllNotes().Count);
			StmNote note = ((StmNoteCollection)testJobDeclaration.Notes.GetAllNotes())[0];
			AssertEquals("Note Text", "Test", note.ST_NoteDataAsText);

			testJobDeclaration.JE_PaidUnderProtestStatement = "Test notes change";
			AssertEquals("Paid Under Protest Statement", "Test notes change", testJobDeclaration.JE_PaidUnderProtestStatement);
			AssertEquals("Has one note", 1, testJobDeclaration.Notes.GetAllNotes().Count);
			note = ((StmNoteCollection)testJobDeclaration.Notes.GetAllNotes())[0];
			AssertEquals("Note Text", "Test notes change", note.ST_NoteDataAsText);

			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration reloadedDeclaration = factory2.Load<JobDeclaration>(testJobDeclaration.PK);
			AssertEquals("Paid Under Protest", "Test notes change", reloadedDeclaration.JE_PaidUnderProtestStatement);
			note = ((StmNoteCollection)testJobDeclaration.Notes.GetAllNotes())[0];

			reloadedDeclaration.JE_PaidUnderProtestStatement = ZString.Empty;
			AssertEquals("Paid Under Protest", ZString.Empty, reloadedDeclaration.JE_PaidUnderProtestStatement);
			AssertEquals("No notes", 0, reloadedDeclaration.Notes.GetAllNotes().Count);

			reloadedDeclaration.JE_PaidUnderProtestStatement = "Test again";
			AssertEquals("Paid Under Protest", "Test again", reloadedDeclaration.JE_PaidUnderProtestStatement);
			AssertEquals("No notes", 1, reloadedDeclaration.Notes.GetAllNotes().Count);
		}

		public void TestJE_AmberStatement()
		{
			testJobDeclaration.JE_ApplicationCode = "CMR";
			AssertEquals("Amber Statement", ZString.Empty, testJobDeclaration.JE_AmberStatement);

			testJobDeclaration.JE_AmberStatement = "Test";
			AssertEquals("Amber Statement", "Test", testJobDeclaration.JE_AmberStatement);
			AssertEquals("Has one note", 1, testJobDeclaration.Notes.GetAllNotes().Count);
			StmNote note = ((StmNoteCollection)testJobDeclaration.Notes.GetAllNotes())[0];
			AssertEquals("Note Text", "Test", note.ST_NoteDataAsText);

			testJobDeclaration.JE_AmberStatement = "Test notes change";
			AssertEquals("Amber Statement", "Test notes change", testJobDeclaration.JE_AmberStatement);
			AssertEquals("Has one note", 1, testJobDeclaration.Notes.GetAllNotes().Count);
			note = ((StmNoteCollection)testJobDeclaration.Notes.GetAllNotes())[0];
			AssertEquals("Note Text", "Test notes change", note.ST_NoteDataAsText);

			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration reloadedDeclaration = factory2.Load<JobDeclaration>(testJobDeclaration.PK);
			AssertEquals("Amber", "Test notes change", reloadedDeclaration.JE_AmberStatement);
			note = ((StmNoteCollection)testJobDeclaration.Notes.GetAllNotes())[0];

			reloadedDeclaration.JE_AmberStatement = ZString.Empty;
			AssertEquals("Amber", ZString.Empty, reloadedDeclaration.JE_AmberStatement);
			AssertEquals("No notes", 0, reloadedDeclaration.Notes.GetAllNotes().Count);

			reloadedDeclaration.JE_AmberStatement = "Test again";
			AssertEquals("Amber", "Test again", reloadedDeclaration.JE_AmberStatement);
			AssertEquals("No notes", 1, reloadedDeclaration.Notes.GetAllNotes().Count);
		}

		public void TestIsUPEDeclaration()
		{
			var declaration = (JobDeclaration)GetNewBusinessObject();
			declaration.AddInfo.ZA_UPEIndicator_Hidden = true;
			Assert(declaration.IsUPEDeclaration);
			declaration.AddInfo.ZA_UPEIndicator_Hidden = false;
			Assert(!declaration.IsUPEDeclaration);
		}

		public void TestImplementUPE()
		{
			AUCustomsDataRegistry.Instance.UPEImplementationDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			var declaration = (JobDeclaration)GetNewBusinessObject();
			Assert("Do not implement Unaccompanied Personal Effects", !declaration.ImplementUPE);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			Assert("Do not implement Unaccompanied Personal Effects", !declaration.ImplementUPE);
			declaration.ManualClearanceDate = ZDateTime.Today;
			Assert("Do not implement Unaccompanied Personal Effects", !declaration.ImplementUPE);
			AUCustomsDataRegistry.Instance.UPEImplementationDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
			Assert("Implement Unaccompanied Personal Effects", declaration.ImplementUPE);
		}

		public void TestIsSOFADeclaration()
		{
			var declaration = (JobDeclaration)GetNewBusinessObject();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.AddInfo.ZA_SOFAIndicator_Hidden = true;
			Assert(!declaration.IsSOFADeclaration);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			Assert("Declaration is Import Dec & Status of Forces Agreement indicator has bee set", declaration.IsSOFADeclaration);
			declaration.AddInfo.ZA_SOFAIndicator_Hidden = false;
			Assert(!declaration.IsSOFADeclaration);
		}

		public void TestIsNature10()
		{
			var declaration = (JobDeclaration)GetNewBusinessObject();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			var header = declaration.Invoices.AddNew();
			var line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "1";
			AssertEquals("IsNature10", true, declaration.IsNature10);

			var line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "2";
			AssertEquals("IsNature10", true, declaration.IsNature10);

			var line3 = header.JobComInvoiceLines.AddNew();
			line3.JI_Tariff = "3";
			line3.JI_IsPackToBondForLine = true;
			AssertEquals("IsNature10 - not any more", false, declaration.IsNature10);
		}

		public void TestIsNature10_MessageTypeChanged()
		{
			var declaration = (JobDeclaration)GetNewBusinessObject();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			var header = declaration.Invoices.AddNew();
			var line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "1";
			AssertEquals("IsNature10", true, declaration.IsNature10);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("IsNature10=false when JE_MessageType <> IMP", false, declaration.IsNature10);
		}

		public void TestIsNature10_MessageSubTypeChanged()
		{
			var declaration = (JobDeclaration)GetNewBusinessObject();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			var header = declaration.Invoices.AddNew();
			var line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "1";
			AssertEquals("IsNature10", true, declaration.IsNature10);

			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.PeriodDeclarationType1;
			AssertEquals("IsNature10=false when JE_MessageSubType <> FormalEntry", false, declaration.IsNature10);
		}

		public void TestIsNature10_IsPackToBondForLineChanged()
		{
			var declaration = (JobDeclaration)GetNewBusinessObject();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			var header = declaration.Invoices.AddNew();
			var line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "1";
			AssertEquals("IsNature10", true, declaration.IsNature10);

			line1.JI_IsPackToBondForLine = true;
			AssertEquals("IsNature10=false when JI_IsPackToBondForLine = true", false, declaration.IsNature10);
		}

		public void TestIsSOFAVisible()
		{
			var declaration = (JobDeclaration)GetNewBusinessObject();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			AssertEquals(false, declaration.IsSOFAVisible);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			AssertEquals("IsSOFAVisible", true, declaration.IsSOFAVisible);

			var header = declaration.Invoices.AddNew();
			var line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "1";
			line1.JI_IsPackToBondForLine = true;
			AssertEquals(false, declaration.IsSOFAVisible);
		}

		public void TestHasMultipleWarehouses()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			var declaration = (JobDeclaration)GetNewBusinessObject();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			AssertEquals("no lines", 0, declaration.FilteredInvoiceLines.Count);
			AssertEquals("HasMultipleWarehouses", false, declaration.HasMultipleWarehouses);
			var line1 = declaration.FilteredInvoiceLines.AddNew();
			line1.AddInfo.ZA_OA_WarehouseAddress_Hidden = OrgHeader.New(Factory).MainAddress.PK;
			line1.AddInfo.WarehouseAddress.LocalControlledPremisesID = "abc";
			line1.JI_IsPackToBondForLine = true;
			line1.SetDeclarationForTesting(declaration);
			AssertEquals("HasMultipleWarehouses", false, declaration.HasMultipleWarehouses);
			declaration.WarehouseDocAddress.E2_OA_Address = OrgHeader.New(Factory).MainAddress.PK;
			declaration.WarehouseAddress.LocalControlledPremisesID = "def";
			AssertEquals("HasMultipleWarehouses", false, declaration.HasMultipleWarehouses);
			var line2 = declaration.FilteredInvoiceLines.AddNew();
			line2.SetDeclarationForTesting(declaration);
			AssertEquals("HasMultipleWarehouses", false, declaration.HasMultipleWarehouses);
			line2.JI_IsPackToBondForLine = true;
			AssertEquals("HasMultipleWarehouses", true, declaration.HasMultipleWarehouses);
			declaration.WarehouseDocAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("HasMultipleWarehouses", false, declaration.HasMultipleWarehouses);
			line2.AddInfo.ZA_OA_WarehouseAddress_Hidden = OrgHeader.New(Factory).MainAddress.PK;
			line2.AddInfo.WarehouseAddress.LocalControlledPremisesID = "zzz";
			AssertEquals("HasMultipleWarehouses", true, declaration.HasMultipleWarehouses);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			line2.AddInfo.WarehouseAddress.LocalControlledPremisesID = "";
			AssertEquals("HasMultipleWarehouses", false, declaration.HasMultipleWarehouses);

			declaration.JE_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(2);
			declaration.Factory.InvalidateCachedProperties();
			AssertEquals("HasMultipleWarehouses", true, declaration.HasMultipleWarehouses);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("HasMultipleWarehouses", false, declaration.HasMultipleWarehouses);
		}

		public void TestJE_RS_NKServiceLevelNotDefaultedToJE_RS_NKServiceLevelOfImporterWhenImporterSetAndJE_RS_NKServiceLevelIsNotSTD()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			organisation.MiscServ.OM_RS_NKIMDefaultServiceLevel = "D2D";

			BaseJobDeclaration jobDeclaration = Factory.New<BaseJobDeclaration>();
			jobDeclaration.JE_RS_NKServiceLevel = "BLH";
			jobDeclaration.JE_OH_Importer = organisation.PK;
			AssertEquals("BLH", jobDeclaration.JE_RS_NKServiceLevel);
		}

		public void TestJE_RS_NKServiceLevelDefaultedToJE_RS_NKServiceLevelOfImporterWhenImporterSet()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			organisation.MiscServ.OM_RS_NKIMDefaultServiceLevel = "D2D";

			BaseJobDeclaration jobDeclaration = Factory.New<BaseJobDeclaration>();
			jobDeclaration.JE_OH_Importer = organisation.PK;
			AssertEquals("D2D", jobDeclaration.JE_RS_NKServiceLevel);
		}

		#region Agent Reference

		public void TestAgentReferenceForPAR()
		{
			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.PAR;

			JobDeclaration declaration = JobDeclaration.New(Factory);
			AssertEquals("Agent Reference is empty", "", declaration.JE_AgentsReference);

			Factory.Save();
			AssertEquals("Agent Reference is not empty", declaration.JE_DeclarationReference, declaration.JE_AgentsReference);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration reLoadedDec = factory2.Load<JobDeclaration>(declaration.PK);
			AssertEquals("Agent Reference is not empty", declaration.JE_DeclarationReference, declaration.JE_AgentsReference);

			declaration.JE_AgentsReference = "Test";
			Factory.Save();
			AssertEquals("Agent Reference is not empty", "Test", declaration.JE_AgentsReference);
		}

		public void TestAgentReferenceForFAR()
		{
			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.FAR;

			JobDeclaration declaration = JobDeclaration.New(Factory);
			AssertEquals("Agent Reference is empty", ZString.Empty, declaration.JE_AgentsReference);

			Factory.Save();
			AssertEquals("Agent Reference is empty", ZString.Empty, declaration.JE_AgentsReference);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration reLoadedDec = factory2.Load<JobDeclaration>(declaration.PK);
			AssertEquals("Agent Reference is empty", ZString.Empty, declaration.JE_AgentsReference);

			declaration.JE_AgentsReference = "Test";
			Factory.Save();
			AssertEquals("Agent Reference is not empty", "Test", declaration.JE_AgentsReference);
		}

		public void TestAgentReferenceForNSR()
		{
			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.NSR;

			JobDeclaration declaration = JobDeclaration.New(Factory);
			AssertEquals("Agent Reference is empty", ZString.Empty, declaration.JE_AgentsReference);

			Factory.Save();
			AssertEquals("Agent Reference is empty", ZString.Empty, declaration.JE_AgentsReference);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration reLoadedDec = factory2.Load<JobDeclaration>(declaration.PK);
			AssertEquals("Agent Reference is empty", ZString.Empty, declaration.JE_AgentsReference);

			declaration.JE_AgentsReference = "Test";
			Factory.Save();
			AssertEquals("Agent Reference is not empty", "Test", declaration.JE_AgentsReference);
		}

		public void TestAgentReferenceForDEF()
		{
			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.DEF;

			JobDeclaration declaration = JobDeclaration.New(Factory);
			AssertEquals("Agent Reference is empty", ZString.Empty, declaration.JE_AgentsReference);

			Factory.Save();
			AssertEquals("Agent Reference is empty", ZString.Empty, declaration.JE_AgentsReference);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration reLoadedDec = factory2.Load<JobDeclaration>(declaration.PK);
			AssertEquals("Agent Reference is empty", ZString.Empty, declaration.JE_AgentsReference);

			declaration.JE_AgentsReference = "Test";
			Factory.Save();
			AssertEquals("Agent Reference is not empty", "Test", declaration.JE_AgentsReference);
		}

		#endregion

		public void TestILandedCostHeaderForAU()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = "IMP";
			testDec.JE_MessageSubType = "SAC";

			AssertEquals("PreCondition:IsSAC", true, testDec.IsSAC);
			AssertEquals("LC is not supported for SAC", false, ((ILandedCostHeader)testDec).IsLCSupported);
			AssertEquals("LC is not supported for SAC", JobDeclaration.SACMessageShownWhenLCIsNotSupported, ((ILandedCostHeader)testDec).MessageShownWhenLCIsNotSupported);

			testDec.JE_MessageSubType = "FRM";
			AssertEquals("PreCondition:Is formal entry", false, testDec.IsSAC);
			AssertEquals("LC is supported for Formal entry", true, ((ILandedCostHeader)testDec).IsLCSupported);
			AssertEquals("LC is supported for formal entry", "", ((ILandedCostHeader)testDec).MessageShownWhenLCIsNotSupported);
		}

		public void TestFirstWarehouseCCP()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("", declaration.FirstWarehouseCCP);
			JobComInvoiceLine invoiceLine1 = declaration.FilteredInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = declaration.FilteredInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine3 = declaration.FilteredInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine4 = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 1;
			invoiceLine1.JI_LineNo = 2;
			invoiceLine3.JI_LineNo = 3;
			invoiceLine4.JI_LineNo = 4;
			AssertEquals("", declaration.FirstWarehouseCCP);
			invoiceLine1.AddInfo.ZA_OA_WarehouseAddress_Hidden = OrgHeader.New(Factory).MainAddress.PK;
			invoiceLine1.AddInfo.WarehouseAddress.LocalControlledPremisesID = "w007";
			invoiceLine1.JI_IsPackToBondForLine = true;
			invoiceLine2.AddInfo.ZA_OA_WarehouseAddress_Hidden = OrgHeader.New(Factory).MainAddress.PK;
			invoiceLine2.AddInfo.WarehouseAddress.LocalControlledPremisesID = "w666";
			invoiceLine2.JI_IsPackToBondForLine = true;
			invoiceLine4.AddInfo.ZA_OA_WarehouseAddress_Hidden = OrgHeader.New(Factory).MainAddress.PK;
			invoiceLine4.AddInfo.WarehouseAddress.LocalControlledPremisesID = "w999";
			invoiceLine4.JI_IsPackToBondForLine = true;
			AssertEquals("W666", declaration.FirstWarehouseCCP.ToUpper());

			invoiceLine1.JI_LineNo = 10;
			invoiceLine4.JI_LineNo = 1;
			invoiceLine2.JI_LineNo = 2;
			AssertEquals("W999", declaration.FirstWarehouseCCP.ToUpper());
		}

		public void TestApplicationCodeDefaultsToCMR()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("Application code is cmr", Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages, declaration.JE_ApplicationCode);

			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			AssertEquals("Application code is legacy", Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages, declaration.JE_ApplicationCode);

			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("Application code is legacy", Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages, declaration.JE_ApplicationCode);
		}

		public void TestGetImporterEquipment()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			OrgHeader header = OrgHeader.New(Factory);
			header.MainAddress.OA_AIREquipmentNeeded = "AIR";
			header.MainAddress.OA_LCLEquipmentNeeded = "LCL";
			header.MainAddress.OA_FCLEquipmentNeeded = "FCL";

			declaration.JE_OH_Importer = header.PK;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Equipment", "AIR", declaration.JE_FCLDeliveryOrPickupEquipmentNeeded);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Importer = header.PK;
			AssertEquals("Equipment", "FCL", declaration.JE_FCLDeliveryOrPickupEquipmentNeeded);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Importer = header.PK;
			declaration.CusContainers.AddNew().CO_FCL_LCL_AIR = "LCL";
			AssertEquals("Equipment", "LCL", declaration.JE_FCLDeliveryOrPickupEquipmentNeeded);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Other;
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Importer = header.PK;
			AssertEquals("Equipment is empty", true, declaration.JE_FCLDeliveryOrPickupEquipmentNeeded.IsEmpty);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Importer = header.PK;
			AssertEquals("Equipment is empty", true, declaration.JE_FCLDeliveryOrPickupEquipmentNeeded.IsEmpty);
		}

		#region AQIS Concern Type

		public void TestAQISConcernTypes()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			AssertEquals("No values in collection", 0, declaration.AQISConcernTypes.Count);

			AQISConcernType concernType = declaration.AQISConcernTypes.AddNew();
			concernType.Code = "1234";
			AssertEquals("One value in the collection", 1, declaration.AQISConcernTypes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", "1234", declaration.AddInfo.ZA_AQISConcern_Hidden);
		}

		public void TestAQISConcernTypesCallsValidateON_AnswerCode()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CMRCusEntryCPDec cpDec = entryHeader.Questions.AddNew();
			bool validationCalled = false;
			cpDec.ON_AnswerCodeInfo.AdditionalValidation += () =>
			{ validationCalled = true; };
			declaration.AQISConcernTypes.AddNew();
			Assert(validationCalled);
		}

		public void TestAQISConcernTypesWithOneValueInAddInfo()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.AddInfo.ZA_AQISConcern_Hidden = "1234";
			AssertEquals("One value in the collection", 1, declaration.AQISConcernTypes.Count);

			AQISConcernType concernType = declaration.AQISConcernTypes.AddNew();
			concernType.Code = "5678";
			AssertEquals("Two values in the collection", 2, declaration.AQISConcernTypes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISConcern_Hidden.Contains("1234"));
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISConcern_Hidden.Contains("5678"));
		}

		public void TestAQISConcernTypesWithMultipleValuesInAddInfo()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.AddInfo.ZA_AQISConcern_Hidden = "1234,5678,9012";
			AssertEquals("One value in the collection", 3, declaration.AQISConcernTypes.Count);

			AQISConcernType concernType = declaration.AQISConcernTypes.AddNew();
			concernType.Code = "3456";
			AssertEquals("Two values in the collection", 4, declaration.AQISConcernTypes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISConcern_Hidden.Contains("1234"));
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISConcern_Hidden.Contains("5678"));
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISConcern_Hidden.Contains("9012"));
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISConcern_Hidden.Contains("3456"));
		}

		#endregion

		public void TestAuthorityToPayLog()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			AssertNull("Authority to pay", testDec.LiveAuthorityToPayLog);

			testDec.AddAuthorityToPayLog();
			AssertNotNull("Authority to pay added", testDec.LiveAuthorityToPayLog);

			testDec.AddAuthorityToPayLog();
			AssertEquals("A new Authority to pay is not added", 1, testDec.AuthorityToPayLogs.Count);

			testDec.AuthorityToPayLogs.CancelAll();
			AssertNull("There is no alive authority to pay", testDec.LiveAuthorityToPayLog);

			testDec.AddAuthorityToPayLog();
			AssertNotNull("Authority to pay added", testDec.LiveAuthorityToPayLog);

			AssertEquals("Reference added", "EFP Payment Authority given by the importer", testDec.LiveAuthorityToPayLog.SL_Reference);
		}

		public void TestOwnerCode()
		{
			var importer = Factory.New<OrgHeader>();
			var edificeCode = importer.CustomsCodes.AddNew();
			edificeCode.OK_CustomsRegNo = "EdificeCode";
			edificeCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			importer.CustomsClientID = "CMRCode";
			var cusCode = importer.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia);

			var testDec = JobDeclaration.New(Factory);
			testDec.JE_OH_Importer = ZGuid.Empty;
			AssertEquals("Owner code empty when importer not set", ZString.Empty, testDec.OwnerCode);

			testDec.JE_OH_Importer = importer.PK;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("IsImport CMR", true, testDec.IsImportCMR);
			AssertEquals("Owner code is CustomsClientID when CMR", importer.CustomsClientID, testDec.OwnerCode);
			cusCode.OK_OA_PremisesAddress = Factory.New<OrgAddress>().PK;
			AssertEquals("Owner code is empty when CID code Premises Address not matching Importer Main Address", ZString.Empty, testDec.OwnerCode);

			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			AssertEquals("IsImport CMR", false, testDec.IsImportCMR);
			AssertEquals("Owner code is LocalCustomsClientCode when Legacy", importer.LocalCustomsClientCode, testDec.OwnerCode);
		}

		public void TestIsWaitingForExportResponse()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			declaration.JE_EntryStatus = CustomsEntryStatus.AwaitingReplacement.Code;
			AssertEquals("Export", true, declaration.IsWaitingForExportResponse);

			declaration.JE_EntryStatus = CustomsEntryStatus.AwaitingOriginal.Code;
			AssertEquals("Export", true, declaration.IsWaitingForExportResponse);
		}

		public void TestIsCustomsChargesActive()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			ICustomsCharges decAsICustomsCharges = ServiceLocator.GetService<ICustomsCharges>(declaration);
			AssertEquals(false, decAsICustomsCharges.IsActive);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(true, decAsICustomsCharges.IsActive);
		}

		public void TestApplicationCodeSetBasedOnMessageType()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			Assert("App code is empty", declaration.JE_ApplicationCode.IsEmpty);

			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 11);
			Assert("App code is still empty as it's an export", declaration.JE_ApplicationCode.IsEmpty);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("App code is now CMR", Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages, declaration.JE_ApplicationCode);
		}

		public void TestGetDefaultContainerisedContainerMode()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			AssertEquals(ZString.Empty, declaration.GetDefaultContainerisedContainerMode());

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(ZString.Empty, declaration.GetDefaultContainerisedContainerMode());

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(Core.Constants.ContainerModes.Containerised, declaration.GetDefaultContainerisedContainerMode());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals(Core.Constants.ContainerModes.FCL, declaration.GetDefaultContainerisedContainerMode());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(Core.Constants.TransportModes.Air, declaration.GetDefaultContainerisedContainerMode());
		}

		public void TestJE_RN_NKTransportNationality()
		{
			var declaration = Factory.New<JobDeclaration>();
			RefVessel vessel = RefVessel.New(Factory);
			vessel.RV_Code = "Vessel";
			vessel.RV_RN_NKCountryOfReg = "US";
			declaration.JE_VesselName = vessel.RV_Code;
			AssertEquals("US", declaration.JE_RN_NKTransportNationality);
		}

		#region Entry and Cargo Status

		public void TestJE_EntryStatusForCMR()
		{
			testJobDeclaration.JE_EntryStatus = "TTT";
			testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			AssertEquals("TTT", "TTT", testJobDeclaration.JE_EntryStatus);

			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testJobDeclaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testJobDeclaration.JE_EntryStatus = "HLD";
			AssertEquals("HLD", "HLD", testJobDeclaration.JE_EntryStatus);
		}

		public void TestJE_EntryStatusDescriptionForCMR()
		{
			testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CusEntryHeader entryHeader = testJobDeclaration.CustomsEntryHeaders.AddNew();
			testJobDeclaration.JE_EntryStatus = CMRImportEntryAdvice.Held.Code;
			AssertEquals("Empty for non cmr", "Unknown", testJobDeclaration.JE_EntryStatusDescription);

			testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testJobDeclaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			testJobDeclaration.JE_EntryStatus = CMRImportEntryAdvice.Held.Code;
			AssertEquals("Not Empty", CMRImportEntryAdvice.Held.Description, testJobDeclaration.JE_EntryStatusDescription);
		}

		public void TestConsolidatedCargoStatusDescription()
		{
			testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testJobDeclaration.JE_ConsolidatedCargoStatus = CMRConsolidatedCargoStatuses.Codes.AqisseizedCargoIsSeizedByQuarantine;
			AssertEquals("Cargo status", CMRConsolidatedCargoStatuses.ShortDescriptions.Aqisseized, testJobDeclaration.ConsolidatedCargoStatusDescription);
			testJobDeclaration.JE_ConsolidatedCargoStatus = CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation;
			AssertEquals("Cargo status", CMRConsolidatedCargoStatuses.ShortDescriptions.Condclear, testJobDeclaration.ConsolidatedCargoStatusDescription);
		}

		public void TestSettingJE_ConsolidatedCargoStatusWillCreateCSHLogForHVLVStandAloneDeclaration()
		{
			testJobDeclaration.JE_ConsolidatedCargoStatus = "AAA";
			Assert("Should not create CSH log for regular declaration", !testJobDeclaration.Logs.HasLogWith(log => log.SL_SE_NKEvent == AutoEvents.ClearanceStatusChangedCode));

			testJobDeclaration.Logs.AddNew(AutoEvents.Transferred, new KeyValuePair<string, string>("TYP", "HVL"));
			testJobDeclaration.JE_ConsolidatedCargoStatus = "BBB";
			Assert("Should create CSH log for HVLV standalone declaration", testJobDeclaration.Logs.HasLogWith(log => log.SL_SE_NKEvent == AutoEvents.ClearanceStatusChangedCode));
		}

		public void TestCustomsClearanceStatus()
		{
			testJobDeclaration.JE_ConsolidatedCargoStatus = "AAA";
			AssertEquals("AAA", testJobDeclaration.CustomsClearanceStatus);
		}

		public void TestRemoveLodgementQAOnAppliedToConsolidatedDeclaration()
		{
			var consolidatedDeclaration = Factory.NewWithValidTestData<ConsolidatedDeclaration>();
			testJobDeclaration.ActiveEntryHeaders.AddNew();
			testJobDeclaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			foreach (int questionID in new int[] { 12, 13, 14, 15 })
			{
				var question = testJobDeclaration.EntryHeader.Questions.AddNew();
				question.ON_CPDecNum = questionID;
			}
			AssertEquals("Question count", 4, testJobDeclaration.EntryHeader.Questions.Count);
			consolidatedDeclaration.JobDeclarations.Add(testJobDeclaration);
			AssertEquals("Entry status has set for added declaration", ConsolidatedEntryStatusList.Codes.AppliedToConsolidation, testJobDeclaration.JE_EntryStatus);
			AssertEquals("Question count", 0, testJobDeclaration.EntryHeader.Questions.Count);
		}

		public void TestIsCargoClear()
		{
			testJobDeclaration.JE_ConsolidatedCargoStatus = ZString.Empty;
			Assert(!testJobDeclaration.isCargoStatusAvailableAndCargoClear);
			Assert(!testJobDeclaration.isCargoStatusAvailableAndCargoNotClear);
			testJobDeclaration.JE_ConsolidatedCargoStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			Assert(testJobDeclaration.isCargoStatusAvailableAndCargoClear);
			Assert(!testJobDeclaration.isCargoStatusAvailableAndCargoNotClear);
			testJobDeclaration.JE_ConsolidatedCargoStatus = CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement;
			Assert(testJobDeclaration.isCargoStatusAvailableAndCargoClear);
			Assert(!testJobDeclaration.isCargoStatusAvailableAndCargoNotClear);
			testJobDeclaration.JE_ConsolidatedCargoStatus = CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation;
			Assert(testJobDeclaration.isCargoStatusAvailableAndCargoClear);
			Assert(!testJobDeclaration.isCargoStatusAvailableAndCargoNotClear);
			testJobDeclaration.JE_ConsolidatedCargoStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			Assert(!testJobDeclaration.isCargoStatusAvailableAndCargoClear);
			Assert(testJobDeclaration.isCargoStatusAvailableAndCargoNotClear);
		}

		public void TestCargoStatusExtraDetails()
		{
			JobDeclaration testJobDeclaration = Factory.New<JobDeclaration>();
			testJobDeclaration.JE_ConsolidatedCargoStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			CusEntryHeader entryHeader = testJobDeclaration.CustomsEntryHeaders.AddNew();
			AssertEquals("No CargoStatusExtraDetails", "No Consolidated Cargo Status Information is available.", testJobDeclaration.CombinedConsolidatedCargoStatusDetails);

			CMRCARSTMessage message1 = Factory.New<CMRCARSTMessage>();
			message1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+2AI7 GEA9 4G65:1+8'
DTM+9:20050915174906614576:ZZZ'
DTM+132:20050908:102'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'
FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'
FTX+AHN+++DECONSOLIDATION UNDERBOND SATISFIED:N/A'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:N/A'
FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'
FTX+AHN+++IAR AQIS CLEARED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'
FTX+AHN+++TRANSHIPMENT NUMBER:AAAA4XMFL'
FTX+AHN+++ACS EVALUATION COMPLETE:NO'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++IMPORT DECLARATION PAID:N/A'
FTX+AHN+++CARGO REPORT SAC:N/A'
TDT+20+9980++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+9938N::95'
NAD+MR+AAA447Y::95'
NAD+UD+87003014042::95'
RFF+ABO:L3020041H0001::1'
RFF+MWB:08122220052'
UNT+34+000001'".Replace("\r\n", "");
			CMRCARSTMessage message2 = Factory.New<CMRCARSTMessage>();
			message2.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+2AI7 GEA9 4G65:1+8'
DTM+9:20050915174906614576:ZZZ'
DTM+132:20050908:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:N/A'
TDT+20+9980++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+9938N::95'
NAD+MR+AAA447Y::95'
NAD+UD+87003014042::95'
RFF+ABO:L3020041H0001::1'
RFF+MWB:08122220053'
UNT+34+000001'".Replace("\r\n", "");
			CMRDSAMessage message3 = Factory.New<CMRDSAMessage>();
			message3.EM_MessageText = CMRImportDeclarationTestData.DSA;
			message3.EM_LinkedObject = entryHeader;
			CMRDSAMessage message4 = Factory.New<CMRDSAMessage>();
			message4.EM_MessageText = CMRImportDeclarationTestData.DSA;
			message4.EM_LinkedObject = entryHeader;

			var mockPackGroup1 = Factory.NewMoq<PackingGroup>();
			PackingGroup packGroup1 = mockPackGroup1.Object;
			packGroup1.CR_CargoStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			mockPackGroup1.Setup(m => m.MostRecentCARSTorDSAMessage).Returns(message1);
			var mockPackGroup2 = Factory.NewMoq<PackingGroup>();
			PackingGroup packGroup2 = mockPackGroup2.Object;
			packGroup2.CR_CargoStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			mockPackGroup2.Setup(m => m.MostRecentCARSTorDSAMessage).Returns(message2);
			var mockPackGroup3 = Factory.NewMoq<PackingGroup>();
			PackingGroup packGroup3 = mockPackGroup3.Object;
			packGroup3.CR_CargoStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			mockPackGroup3.Setup(m => m.MostRecentCARSTorDSAMessage).Returns(message3);
			packGroup3.CR_HouseContainerNumber = 1;
			var mockPackGroup4 = Factory.NewMoq<PackingGroup>();
			PackingGroup packGroup4 = mockPackGroup4.Object;
			packGroup4.CR_CargoStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			mockPackGroup4.Setup(m => m.MostRecentCARSTorDSAMessage).Returns(message4);
			packGroup4.CR_HouseContainerNumber = 2;

			testJobDeclaration.PackingGroups.Add(packGroup3);
			testJobDeclaration.PackingGroups.Add(packGroup4);
			testJobDeclaration.PackingGroups.Add(packGroup1);
			testJobDeclaration.PackingGroups.Add(packGroup2);
			Package pack1 = packGroup1.Packages.AddNew();
			Package pack2 = packGroup2.Packages.AddNew();
			Package pack3 = packGroup3.Packages.AddNew();
			Package pack4 = packGroup4.Packages.AddNew();

			string expectedResult =
			"Impediments:\r\n" +
			"Agency / Program: QUARANTINE\r\n" +
			"Impediment Document Version Number: 4\r\n" +
			"Impediment Type: Matched a community protection profile\r\n" +
			"Risk Identifier: 190: QUARANTINE: HIGH RISK\r\n" +
			"Risk Line Number: 2\r\n" +
			"\r\n" +
			"Agency / Program: ACS\r\n" +
			"Impediment Document Version Number: 4\r\n" +
			"Impediment Type: Matched a community protection profile\r\n" +
			"Risk Identifier: 215: ARE THESE GOODS MADE OF, OR CONTAIN, CITES LISTED SPECIES?\r\n" +
			"Risk Line Number: 2\r\n" +
			"\r\n" +
			"Agency / Program: ACS\r\n" +
			"Impediment Document Version Number: 3\r\n" +
			"\r\n" +
			"Agency / Program: QUARANTINE\r\n" +
			"Impediment Document Version Number: 3\r\n" +
			"Impediment Type: Conditional release\r\n" +
			"Advice Note: PENDING AQIS ACTION (QUARANTINE)\r\n" +
			"\r\n" +
			"Status for Packing (Transport) Line: 1\r\n" +
			"***CONSOLIDATED CARGO STATUS: ***HELD***\r\n" +
			"ACSDec/ACSCR/AQISDec/AQISCR: Y/N/Y/Y\r\n" +
			"Screening Period Expiry: 10Jul2008 03:16 \r\n" +
			"Customs Indicator: Linked to Cargo Report Line \r\n" +
			"CONSOLIDATED STATUS: HELD\r\n" +
			"CARGO REPORT ACS EVALUATED: NO\r\n" +
			"LCL UNDERBOND SATISFIED: NO\r\n" +
			"DECONSOL UNDERBOND SATISFIED: NO\r\n" +
			"\r\n\r\n" +
			"Status for Packing (Transport) Line: 2\r\n" +
			"***CONSOLIDATED CARGO STATUS: ***CLEAR***\r\n" +
			"ACSDec/ACSCR/AQISDec/AQISCR: Y/Y/Y/Y\r\n" +
			"Screening Period Expiry: 10Jul2008 03:16 \r\n" +
			"Customs Indicator: Linked to Cargo Report Line \r\n" +
			"CONSOLIDATED STATUS: CLEAR\r\n" +
			"CARGO REPORT ACS EVALUATED: YES\r\n" +
			"LCL UNDERBOND SATISFIED: NO\r\n" +
			"DECONSOL UNDERBOND SATISFIED: NO\r\n" +
			"\r\n\r\n" +
			"***CONSOLIDATED CARGO STATUS: ***HELD***\r\n" +
			"MAWB: 08122220052\r\n" +
			"ACSDec/ACSCR/AQISDec/AQISCR: N/N/N/Y\r\n" +
			"Detailed Status Description:\r\n" +
			"\tCONSOLIDATED STATUS: HELD\r\n" +
			"\tDEPARTURE FROM LAST OVERSEAS PORT: YES\r\n" +
			"\tQUOTED MASTER / OCEAN BILL EXISTS: YES\r\n" +
			"\tIAR ACS CLEARED: YES\r\n" +
			"\tCOMPLETE UNDERBOND SERIES APPROVED: N/A\r\n" +
			"\tLCL UNDERBOND SATISFIED: N/A\r\n" +
			"\tDECONSOLIDATION UNDERBOND SATISFIED: N/A\r\n" +
			"\tCARGO NOT A CONSOLIDATION: YES\r\n" +
			"\tRELEASE PREMISE IN DESTINATION: N/A\r\n" +
			"\tCARGO REPORT ACS EVALUATED: NO\r\n" +
			"\tIAR AQIS CLEARED: YES\r\n" +
			"\tCARGO REPORT AQIS EVALUATED: YES\r\n" +
			"\tIMPORT DECLARATIONS MATCHED: N/A\r\n" +
			"\tIMPORT DECLARATION ACS EVALUATED: N/A\r\n" +
			"\tIMPORT DECLARATION AQIS EVALUATED: N/A\r\n" +
			"\tTRANSHIPMENT NUMBER: AAAA4XMFL\r\n" +
			"\tACS EVALUATION COMPLETE: NO\r\n" +
			"\tAQIS CARGO REPORT EVALUATION COMPLETE: YES\r\n" +
			"\tACS IMPORT DECLARATION EVALUATION COMPLETE: N/A\r\n" +
			"\tAQIS IMPORT DECLARATION EVALUATION COMPLETE: N/A\r\n" +
			"\tIMPORT DECLARATION PAID: N/A\r\n" +
			"\tCARGO REPORT SAC: N/A\r\n" +
			"\r\n" +
			"***CONSOLIDATED CARGO STATUS: ***CLEAR***\r\n" +
			"MAWB: 08122220053\r\n" +
			"ACSDec/ACSCR/AQISDec/AQISCR: Y/Y/Y/Y\r\n" +
			"Detailed Status Description:\r\n" +
			"\tCONSOLIDATED STATUS: CLEAR\r\n" +
			"\tCARGO REPORT SAC: N/A\r\n" +
			"\r\n";
			testJobDeclaration.fCombinedConsolidatedCargoStatusDetails = ZString.Empty;
			AssertMultilineASCIIEquals("CargoStatusExtraDetails", expectedResult, testJobDeclaration.CombinedConsolidatedCargoStatusDetails);
			AssertEquals("Cargo status", "HELD  Y/N/Y/Y", testJobDeclaration.ConsolidatedCargoStatusDescription);
		}

		#endregion

		#region Message Status

		public void TestJE_MessageStatusForCMR()
		{
			testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testJobDeclaration.JE_MessageStatus = "TTT";
			AssertEquals("TTT", "TTT", testJobDeclaration.JE_MessageStatus);

			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testJobDeclaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testJobDeclaration.JE_MessageStatus = "CFP";
			AssertEquals("CFP", "CFP", testJobDeclaration.JE_MessageStatus);
		}

		public void TestJE_MessageStatusDescriptionForCMR()
		{
			testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testJobDeclaration.JE_MessageStatus = CustomsEntryStatus.FailAmendment.Code;
			AssertEquals("Empty for non cmr", "Not Sent", testJobDeclaration.JE_EntryStatusDescription);

			testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testJobDeclaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			testJobDeclaration.JE_MessageStatus = CustomsEntryStatus.ClearSAC.Code;
			AssertEquals("Not Empty", CustomsEntryStatus.ClearSAC.Description, testJobDeclaration.JE_MessageStatusDescription);
		}

		#endregion

		#region Is Company Importer

		public void TestIsEntryForAnImporter()
		{
			ZString previousABN = GlbCompany.CurrentCompany.GC_BusinessRegNo;
			AssertEquals("ABN not the same", false, testJobDeclaration.IsEntryForAnImporter);

			OrgHeader header = OrgHeader.New(Factory);
			header.LocalBusinessRegNo = "1234";
			testJobDeclaration.JE_OH_Importer = header.PK;
			AssertEquals("ABN not the same", false, testJobDeclaration.IsEntryForAnImporter);

			header.LocalBusinessRegNo = GlbCompany.CurrentCompany.GC_BusinessRegNo;
			AssertEquals("ABN the same", true, testJobDeclaration.IsEntryForAnImporter);

			GlbCompany.CurrentCompany.GC_BusinessRegNo = ZString.Empty;
			AssertEquals("Company ABN empty", false, testJobDeclaration.IsEntryForAnImporter);

			header.LocalBusinessRegNo = ZString.Empty;
			AssertEquals("ABN the same but Company ABN empty", false, testJobDeclaration.IsEntryForAnImporter);

			GlbCompany.CurrentCompany.GC_BusinessRegNo = previousABN;
			header.LocalBusinessRegNo = GlbCompany.CurrentCompany.GC_BusinessRegNo;
			AssertEquals("ABN same", true, testJobDeclaration.IsEntryForAnImporter);

			header.LocalBusinessRegNo = ZString.Empty;
			AssertEquals("ABN not the same", false, testJobDeclaration.IsEntryForAnImporter);
		}

		public void TestCompanyABN()
		{
			ZString previousABN = GlbCompany.CurrentCompany.GC_BusinessRegNo;

			AssertEquals("Company ABN", GlbCompany.CurrentCompany.GC_BusinessRegNo.Replace(" ", "").Trim(), testJobDeclaration.CompanyABN);

			GlbCompany.CurrentCompany.GC_BusinessRegNo = " 1   2     34 ";
			AssertEquals("Company ABN", "1234", testJobDeclaration.CompanyABN);

			GlbCompany.CurrentCompany.GC_BusinessRegNo = previousABN;
		}

		public void TestImporterABN()
		{
			OrgHeader header = OrgHeader.New(Factory);
			header.LocalBusinessRegNo = "12345678901";
			testJobDeclaration.JE_OH_Importer = header.PK;
			AssertEquals("ImporterABN", "12345678901", testJobDeclaration.ImporterABN);

			header.LocalBusinessRegNo = " 1    2       34   5678901 ";
			AssertEquals("ImporterABN", "12345678901", testJobDeclaration.ImporterABN);

			header.LocalBusinessRegNo = " 1    2       34   5678901 /234";
			AssertEquals("ImporterABN", "12345678901", testJobDeclaration.ImporterABN);
		}

		#endregion

		#region Merge

		public void TestThrowAwayMerge()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_DeclarationReference = "B00001000";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var bill = declaration.Bills.AddNew();
			var packingGroup = declaration.PackingGroups.AddNew();
			packingGroup.CR_HouseContainerNumber = 1;
			packingGroup.CR_CU_HouseBill = bill.PK;

			AssertEquals("Pack number", (ZShort)1, declaration.PackingGroups[0].CR_HouseContainerNumber);
			declaration.ThrowAwayMerge();
			AssertEquals("Pack number", ZShort.Zero, declaration.PackingGroups[0].CR_HouseContainerNumber);
		}

		public void TestThrowAwayMergeNotDeleteCOLSEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.Load();
			declaration.ThrowAwayMerge();
			AssertEquals("Entry header not linked with COLS header is deleted", true, entryHeader1.IsDeleted);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader2 = declaration2.ActiveEntryHeaders.AddNew();
			var impNumber = CusEntryNumber.New(entryHeader2, CusEntryNumberTypes.Australia.IMP, Core.Constants.CountryCodes.Australia);
			impNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader2.PK;

			declaration.CustomsEntryHeaders.Load();
			declaration.ThrowAwayMerge();
			AssertEquals("Entry header linked with COLS header is not deleted", false, entryHeader2.IsDeleted);
		}

		[ExpectNoExceptions]
		public void TestResetToOriginal()
		{
			ZTestHelper helper = new ZTestHelper(Factory);
			helper.PopulateSimpleImportDeclaration();
			JobDeclaration testDec = helper.Declaration;
			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();
			Customs.Business.Testing.MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec.MergeManager);
			Factory.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			JobDeclaration testDecLoaded = JobDeclaration.New(anotherFactory);
			testDecLoaded.ThrowAwayMerge();
		}

		public void TestBeforeMergeItCreatesInvoiceLineForEachEntryHeaderOfSAC()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);

			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			JobComInvoiceHeader header = testDec.Invoices.AddNew();
			header.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			header.JZ_InvoiceAmount = 100m;
			AssertEquals(0, testDec.Invoices[0].JobComInvoiceLines.Count);

			testDec.DoMerge();

			AssertEquals(1, testDec.Invoices[0].JobComInvoiceLines.Count);
			JobComInvoiceLine invoiceLine = testDec.Invoices[0].JobComInvoiceLines[0];
			AssertEquals(100m, invoiceLine.JI_LinePrice);
		}

		public void TestMergeDoesntHavePreviousMergingResult()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader header = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			JobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();

			testJobDeclaration.DoMerge();
			AssertEquals("One Customs Entry Header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("One Customs Entry Line", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);

			line1.JI_Tariff = "0000.00.00";
			testJobDeclaration.ThrowAwayMerge();
			testJobDeclaration.DoMerge();
			AssertEquals("One Customs Entry Header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("One Customs Entry Line", 2, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestRemoveMerge()
		{
			OrgHeader supplier = OrgHeader.LoadFromCode(Factory, "ABIGAS");
			OrgHeader importer = OrgHeader.LoadFromCode(Factory, "ABABEU");

			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.GSTCode, "90093519530");
			supplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.SupplierCode, "955411R");
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_OH_Importer = importer.PK;
			testDec.JE_OH_Supplier = supplier.PK;
			testDec.AddInfo.ZA_MergeBy_Hidden = OrgConstants.MergeInvoiceLines.Tariff;
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testDec.JE_AgentsReference = "198810 -MP";
			testDec.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_DateOfArrival = new ZDateTime(2003, 12, 1, 10, 2, 0);
			testDec.JE_DateOfFirstArrival = new ZDateTime(2003, 12, 1, 10, 2, 0);
			testDec.JE_DeclarationReference = "B00103428";
			testDec.JE_EntryStatus = CustomsEntryStatus.ClearCreate.Code;
			testDec.JE_ExportDate = new ZDateTime(2003, 11, 28, 10, 2, 0);
			testDec.JE_ExportGoodsType = "OT";
			testDec.JE_GB = GlbBranch.CurrentBranch.PK;
			testDec.JE_MasterBill = "PONLCPH22002959";
			testDec.JE_MessageSubType = "FRM";
			testDec.JE_OwnerRef = "1200994221";
			testDec.JE_RL_NKFinalDestination = "AUMEL";
			testDec.JE_RL_NKOrigin = "DEFRA";
			testDec.JE_RL_NKPortOfArrival = "AUMEL";
			testDec.JE_RL_NKPortOfFirstArrival = "AUSYD";
			testDec.JE_RL_NKPortOfLoading = "DEHAM";
			testDec.JE_VesselName = "ADMIRALENGRACHT";
			testDec.JE_TotalNoOfPacks = 1;
			testDec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			testDec.JE_VoyageFlightNo = "2038";

			testDec.JobComInvoiceGroupHeaders[0].JZ_GroupInvoice = true;
			testDec.JobComInvoiceGroupHeaders[0].JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testDec.JobComInvoiceGroupHeaders[0].JZ_InvoiceCurrExRate = 1.000000000m;
			testDec.JobComInvoiceGroupHeaders[0].JZ_InvoiceDate = new ZDateTime(2003, 11, 28);

			JobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 2300, "USD");
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 310.7300m, "AUD");

			JobComInvoiceHeader testHeader1 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testHeader1.JZ_AddInfo = "ORG=DK*PackCountForNature10_Hidden=1*ValuationBasis_Hidden=UT";
			testHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			testHeader1.JZ_InvoiceAmount = 124295.5000m;
			testHeader1.JZ_InvoiceCurrExRate = 1.000000000m;
			testHeader1.JZ_InvoiceDate = new ZDateTime(2003, 11, 28);
			testHeader1.JZ_InvoiceNumber = "1";
			testHeader1.JZ_OH_Supplier = supplier.PK;
			testHeader1.JZ_RX_NKInvoice_Currency = "AUD";
			testHeader1.JZ_VolumeUQ = "M3";
			testHeader1.JZ_Weight = 25500.000m;
			testHeader1.JZ_WeightUQ = "KG";

			JobComInvoiceLine testLine1 = testHeader1.JobComInvoiceLines.AddNew();
			testLine1.JI_AddInfo = "GSTE=FOOD";
			testLine1.JI_CustomsQuantity = 24859.1000m;
			testLine1.JI_CustomsUnitQty = "KG";
			testLine1.JI_Description = "FROZDANPORK";
			testLine1.JI_InvoiceQuantity = 24859.10000m;
			testLine1.JI_InvoiceUQ = "KG";
			testLine1.JI_LineNo = (short)1;
			testLine1.JI_LinePrice = 124295.5000m;
			testLine1.JI_Tariff = "0203.29.00 41";
			testLine1.JI_Weight = 24859.100m;
			testLine1.JI_WeightUQ = "KG";

			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			testDec.DoMerge();
			AssertEquals("Number Of Customs Entries", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("Number Of Customs Lines", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);

			testDec.ThrowAwayMerge();
			AssertEquals("Number Of Customs Entries", 0, testDec.CustomsEntryHeaders.Count);
		}

		public void TestIsExportDeclarationClear()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			Assert("IsExportDeclarationClear", declaration.IsExportDeclarationClear);
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearReplacement.Code;
			Assert("IsExportDeclarationClear", declaration.IsExportDeclarationClear);
			declaration.JE_EntryStatus = CustomsEntryStatus.ReleasedFromEmbargo.Code;
			Assert("IsExportDeclarationClear", declaration.IsExportDeclarationClear);
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearWithdrawal.Code;
			Assert("IsExportDeclarationClear, Not", !declaration.IsExportDeclarationClear);
		}

		public void TestThrowAwayMergeOnClone()
		{
			var testDec = Factory.Load<JobDeclaration>(JobDeclarationTest.SetUpAndSaveImportDecWithCPDecsAnswered(""));
			AssertEquals("PreCondition - CusHeaders", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("PreCondition - CusLines", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("PreCondition - CusLineInvoiceLines", 2, testDec.CustomsEntryHeaders[0].MergedLines[0].InvoiceLines.Count);
			JobDeclaration clone = (JobDeclaration)testDec.TemplateCopy();
			clone.ThrowAwayMerge();

			AssertEquals("CusHeaders", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("CusLines", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("CusLineInvoiceLines", 2, testDec.CustomsEntryHeaders[0].MergedLines[0].InvoiceLines.Count);
			clone.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(true);
			clone.DoMerge();

			AssertEquals("CusHeaders", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("CusLines", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("CusLineInvoiceLines", 2, testDec.CustomsEntryHeaders[0].MergedLines[0].InvoiceLines.Count);
			clone.Delete();

			AssertEquals("CusHeaders", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("CusLines", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("CusLineInvoiceLines", 2, testDec.CustomsEntryHeaders[0].MergedLines[0].InvoiceLines.Count);
		}

		public void TestThrowAwayMergeWithShipment()
		{
			var testDec = Factory.Load<JobDeclaration>(JobDeclarationTest.SetUpAndSaveImportDecWithCPDecsAnswered(""));
			AssertEquals("PreCondition - CusHeaders", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("PreCondition - CusLines", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("PreCondition - CusLineInvoiceLines", 2, testDec.CustomsEntryHeaders[0].MergedLines[0].InvoiceLines.Count);
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			testDec.JE_JS = shipment.PK;
			Factory.Save();
			shipment.ResetCusEntryNumbers();
			AssertEquals("CusEntryNumberCount", 1, shipment.CusEntryNumbers.Count);
			AssertEquals("PreCondition - Shipment CustomsEntryNum", testDec.CustomsEntryHeaders[0].EntryNumber, shipment.CustomsEntryNumber);
			AssertEquals("PreCondition - Shipment CustomsEntryNumType", CustomsEntryStatus.LodgeImpediment.Code, shipment.CustomsEntryNumberType);

			testDec.ThrowAwayMerge();
			AssertEquals("CustomsEntryNum - Should be blank and not throw exception", "", shipment.CustomsEntryNumber);
			AssertEquals("CustomsEntryNumType not throw exception", "CAN", shipment.CustomsEntryNumberType);
		}

		#endregion

		public void TestCPQAManagerAndCachedQuestions()
		{
			AssertNotNull(testJobDeclaration.CPQAManager);
			AssertNotNull(testJobDeclaration.CachedQuestions);
		}

		[TestDate(2006, 5, 12)]
		public void TestEffectiveDutyDate()
		{
			testJobDeclaration.JE_DateOfFirstArrival = ZDateTime.Today.AddDays(1);
			AssertEquals("Effective Duty Date is today, if not lodged, regardless of First Arrival", ZDateTime.Today, testJobDeclaration.EffectiveDutyDate);
			testJobDeclaration.JE_DateOfFirstArrival = ZDateTime.Today.AddDays(-1);
			AssertEquals("Effective Duty Date is today, if not lodged, regardless of First Arrival", ZDateTime.Today, testJobDeclaration.EffectiveDutyDate);

			testJobDeclaration.JE_DateOfFirstArrival = new ZDateTime(2006, 05, 05);
			testJobDeclaration.JE_EntrySubmittedDate = new ZDateTime(2006, 05, 06);
			CusEntryHeader entry = testJobDeclaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("Effective Duty Date is Lodged Date if > First Arrival", new ZDateTime(2006, 05, 06), testJobDeclaration.EffectiveDutyDate);

			testJobDeclaration.JE_DateOfFirstArrival = new ZDateTime(2006, 05, 07);
			AssertEquals("Effective Duty Date is First Arrival Date if > Lodged", new ZDateTime(2006, 05, 07), testJobDeclaration.EffectiveDutyDate);

			testJobDeclaration.JE_DateOfFirstArrival = ZDateTime.Today.AddDays(1);
			AssertEquals("PreCondition:DateOfFirstArrival is in the future", true, testJobDeclaration.JE_DateOfFirstArrival.IsInTheFutureDatePartOnly);
			AssertEquals("Effective duty date is today when lodged and first arrival date is in the future", ZDateTime.Today, testJobDeclaration.EffectiveDutyDate);
		}

		public void TestICPQAAttacheeHolder()
		{
			JobDeclaration testDec = (JobDeclaration)GetNewBusinessObject();
			testDec.CustomsEntryHeaders.AddNew();
			testDec.CustomsEntryHeaders.AddNew();
			testDec.CustomsEntryHeaders[0].MergedLines.AddNew();
			testDec.CustomsEntryHeaders[1].MergedLines.AddNew();
			testDec.CustomsEntryHeaders[1].MergedLines.AddNew();

			AssertEquals("ICPQAAttacheeHolder.Headers", 2, ((ICPQAAttacheeHolder)testDec).Headers.Length);
			AssertEquals("Headers has CusEntryHeader", testDec.CustomsEntryHeaders[0], ((ICPQAAttacheeHolder)testDec).Headers[0]);
			AssertEquals("ICPQAAttacheeHolder.Lines", 3, ((ICPQAAttacheeHolder)testDec).Lines.Length);
			AssertEquals("Lines has CusEntryLine", testDec.CustomsEntryHeaders[0].MergedLines[0], ((ICPQAAttacheeHolder)testDec).Lines[0]);
		}

		//HACK: Done until namespaces are cleaned up a bit
		class CustomsChargeTypeList : Customs.Business.CustomsChargeTypeList
		{
		}

		public void TestWarehouseInvoiceLink()
		{
			IInvoiceLink link = ((IInvoiceLinkProvider)GetNewBusinessObject()).Link;
			AssertEquals("Correct type", typeof(WarehouseInvoiceLink), link.GetType());
		}

		public void TestDefaultCommodityCodeForInvoiceLines()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.OH_RL_NKClosestPort = "";//If valid, it sets Message Type depending on this data.
			var commodity1 = Factory.LoadTop1<RefCommodityCode>(new ZQuery());
			var commodity2 = Factory.LoadTop1<RefCommodityCode>(new ZQuery(RefCommodityCodeSchema.PK, SQLComparisonOperator.NotEqual, commodity1.PK));

			importer.MiscServ.OM_RH_NKCMMainImportCmdty = commodity1.RH_Code;
			importer.MiscServ.OM_RH_NKCMMainExportCmdty = commodity2.RH_Code;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			testDec.JE_OH_Importer = importer.PK;
			AssertEquals("Message type is still EXP", true, testDec.IsExport);
			AssertEquals("InvoiceLine's commodity code remains as empty as this is Export Job", ZString.Empty, invoiceLine.JI_RH_NKCommodity_Code);

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_OH_Importer = ZGuid.Empty;
			testDec.JE_OH_Importer = importer.PK;
			AssertEquals("Message type is still IMP", true, testDec.IsImport);
			AssertEquals("InvoiceLine's commodity code should default from Importer's Import Code", commodity1.RH_Code, invoiceLine.JI_RH_NKCommodity_Code);
		}

		public void TestCMROverride()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ZString.Empty;
			AssertEquals("IsImport CMR", false, declaration.IsImportCMR);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("IsImport CMR", true, declaration.IsImportCMR);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			AssertEquals("IsImport CMR", false, declaration.IsImportCMR);
		}

		public void TestJE_ApplicationCode()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("Application Code", Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages, declaration.JE_ApplicationCode);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("Application Code", ZString.Empty, declaration.JE_ApplicationCode);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			AssertEquals("Application Code", Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages, declaration.JE_ApplicationCode);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("Application Code", ZString.Empty, declaration.JE_ApplicationCode);
		}

		public void TestApportionBeforeRunningValidationWhenMerging()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100m, "AUD");
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 10m, "AUD");

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			AssertEquals("PreCondition: Apportionment is dirty", true, declaration.ApportionmentDirty);
			AssertEquals("The invoice has not been apportioned OFT & ONS", false, invoice.GroupCharges.HasChargeWithThisKey(groupHeader.Charges[0].ChargeKey));
			AssertEquals("The invoice has not been apportioned OFT & ONS", false, invoice.GroupCharges.HasChargeWithThisKey(groupHeader.Charges[1].ChargeKey));
			AssertEquals("Invoice Incoterm CIF does not have the mandatory charges", true, invoice.JZ_IncoTermInfo.HasMessageErrors());

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Apportionment is resumed before validation runs", false, declaration.ApportionmentDirty);
		}

		public void TestSaveN30ClearsNotUsedFields()
		{
			JobDeclaration n30Declaration = JobDeclaration.New(Factory);
			ZDateTime date = ZDateTime.Now;

			n30Declaration.JE_MasterBill = "MASTER";
			n30Declaration.JE_RL_NKOrigin = "X1";
			n30Declaration.JE_RL_NKPortOfArrival = "X2";
			n30Declaration.JE_RL_NKPortOfFirstArrival = "X3";
			n30Declaration.JE_RL_NKPortOfLoading = "X4";
			n30Declaration.JE_DateAtFinalDestination = date;
			n30Declaration.JE_DateAtOrigin = date;
			n30Declaration.JE_DateOfArrival = date;
			n30Declaration.JE_DateOfFirstArrival = date;
			n30Declaration.JE_ExportDate = date;
			n30Declaration.Factory.Save();

			var declaration2 = Factory.Load<BaseJobDeclaration>(n30Declaration.PK);
			AssertEquals("Value should be set", "MASTER", declaration2.JE_MasterBill);
			AssertEquals("Value should be set", "X1", declaration2.JE_RL_NKOrigin);
			AssertEquals("Value should be set", "X2", declaration2.JE_RL_NKPortOfArrival);
			AssertEquals("Value should be set", "X3", declaration2.JE_RL_NKPortOfFirstArrival);
			AssertEquals("Value should be set", "X4", declaration2.JE_RL_NKPortOfLoading);
			AssertEquals("Value should be set", date, declaration2.JE_DateAtFinalDestination);
			AssertEquals("Value should be set", date, declaration2.JE_DateAtOrigin);
			AssertEquals("Value should be set", date, declaration2.JE_DateOfArrival);
			AssertEquals("Value should be set", date, declaration2.JE_DateOfFirstArrival);
			AssertEquals("Value should be set", date, declaration2.JE_ExportDate);

			n30Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			n30Declaration.Factory.Save();

			declaration2 = Factory.Load<BaseJobDeclaration>(n30Declaration.PK);
			AssertEquals("Value should be set", "MASTER", declaration2.JE_MasterBill);
			AssertEquals("Value should be cleared", ZString.Empty, declaration2.JE_RL_NKOrigin);
			AssertEquals("Value should be cleared", ZString.Empty, declaration2.JE_RL_NKPortOfArrival);
			AssertEquals("Value should be cleared", ZString.Empty, declaration2.JE_RL_NKPortOfFirstArrival);
			AssertEquals("Value should be cleared", ZString.Empty, declaration2.JE_RL_NKPortOfLoading);
			AssertEquals("Value should be cleared", ZDateTime.Empty, declaration2.JE_DateAtFinalDestination);
			AssertEquals("Value should be cleared", ZDateTime.Empty, declaration2.JE_DateAtOrigin);
			AssertEquals("Value should be cleared", ZDateTime.Empty, declaration2.JE_DateOfArrival);
			AssertEquals("Value should be cleared", ZDateTime.Empty, declaration2.JE_DateOfFirstArrival);
			AssertEquals("Value should be cleared", ZDateTime.Empty, declaration2.JE_ExportDate);
		}

		[ExpectNoExceptions]
		public override void TestSaveAndDeleteBusinessObject()
		{
			ZTestHelper helper = new ZTestHelper(Factory);
			helper.PopulateSimpleImportDeclaration();
			JobDeclaration testDec = helper.Declaration;
			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();//All related BO's generated
			Customs.Business.Testing.MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec.MergeManager);
			Factory.Save();
			testDec.Delete();
		}

		public void TestSortedInvoiceLines()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "11";
			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line3 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line4 = invoice.JobComInvoiceLines.AddNew();

			line1.JI_LineNo = 3;
			line1.JI_LinePrefix = "P";
			line2.JI_LineNo = 4;
			line2.JI_LinePrefix = "T";
			line3.JI_LineNo = 1;
			line4.JI_LineNo = 2;

			Factory.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			JobDeclaration decLoaded = anotherFactory.Load<JobDeclaration>(declaration.PK);
			JobComInvoiceLine[] result = decLoaded.SortedInvoiceLines;
			AssertEquals("First", line3.JI_LineNo, result[0].JI_LineNo);
			AssertEquals("Second", line4.JI_LineNo, result[1].JI_LineNo);
			AssertEquals("Third", line1.JI_LineNo, result[2].JI_LineNo);
			AssertEquals("4th", line2.JI_LineNo, result[3].JI_LineNo);
		}

		public void TestExportManualSubTypeOpensDeclarationNumber()
		{
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			testJobDeclaration.JE_MessageSubType = JobDeclaration.MessageSubType.Manual;
			AssertEquals("Declaration number is opened up for editing", false, testJobDeclaration.DeclarationNumberInfo.ReadOnly);
			Factory.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			JobDeclaration jobDecLoaded = anotherFactory.Load<JobDeclaration>(testJobDeclaration.PK);
			AssertEquals("Declaration number is not readonly", false, jobDecLoaded.DeclarationNumberInfo.ReadOnly);
		}

		public void TestExportDeclarationByExternalBrokerOpensDeclarationNumber()
		{
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			testJobDeclaration.JE_MessageSubType = JobDeclaration.MessageSubType.NonConfirming;
			testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("Declaration number is not opened up for editing", true, testJobDeclaration.DeclarationNumberInfo.ReadOnly);
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExportDeclarationByExternalBroker;
			AssertEquals("Declaration number is opened up for editing", false, testJobDeclaration.DeclarationNumberInfo.ReadOnly);
		}

		public void TestImportDeclarationByExternalBrokerDoesNotOpenDeclarationNumber()
		{
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testJobDeclaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("Declaration number is not opened up for editing", true, testJobDeclaration.DeclarationNumberInfo.ReadOnly);
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ImportDeclarationByExternalBroker;
			AssertEquals("Declaration number is not opened up for editing", true, testJobDeclaration.DeclarationNumberInfo.ReadOnly);
		}

		public void TestCurrencyConverterForExportUseMostRecentExchangeRate()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				//USD is a valid EX1 currency without exchange rate to declare, however there are situations where system needs to have them to calculate FOB
				ZQuery filter = new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, uSDCurrency.RX_Code);
				filter.AddToFilter(RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.Today);
				filter.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThan, ZDateTime.Today);

				var rate = Factory.LoadTop1<RefExchangeRate>(filter);
				if (rate != null)
				{
					rate.RE_StartDate = ZDateTime.Today.AddDays(-2);
					rate.RE_ExpiryDate = ZDateTime.Today.AddDays(-1);
					rate.RE_SellRate = 0.5m;
				}

				testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				testJobDeclaration.JE_ExportDate = ZDateTime.Today.AddDays(7);//in the future

				JobComInvoiceGroupHeader groupHeader = testJobDeclaration.JobComInvoiceGroupHeaders[0];
				JobComInvoiceHeader invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
				invoiceHeader.JZ_InvoiceAmount = 1500m;
				invoiceHeader.JZ_RX_NKInvoice_Currency = "AUD";
				invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;

				groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 225, uSDCurrency.RX_Code);
				groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 25, uSDCurrency.RX_Code);

				testJobDeclaration.ResumeApportionment();
				Money overseasFreight = invoiceHeader.GroupCharges.GetCharge(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OverseasFreight, false, true));
				Money overseasInsurance = invoiceHeader.GroupCharges.GetCharge(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OverseasInsurance, false, true));

				AssertEquals("Freight Currency", uSDCurrency.PK, overseasFreight.Currency.PK);
				AssertEquals("Insurance Currency", uSDCurrency.PK, overseasInsurance.Currency.PK);
				AssertEquals("Freight incalculable", 225m, overseasFreight.Amount);
				AssertEquals("Insurance incalculable", 25m, overseasInsurance.Amount);
			}
		}

		public void TestShipmentDeclarationReferenceNumber()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			testJobDeclaration.JE_JS = shipment.PK;

			Factory.Save();
			Assert("Shipment has been assigned reference number", !shipment.JS_UniqueConsignRef.IsEmpty);
			AssertEquals("Declaration should have shipment reference", shipment.JS_UniqueConsignRef, testJobDeclaration.JE_DeclarationReference);
		}

		public void TestJobDeclarationReferenceNumber()
		{
			Assert("PreCondition:DeclarationReference number is empty", testJobDeclaration.JE_DeclarationReference.IsEmpty);
			Factory.Save();
			Assert("DeclarationReference number has been assigned", !testJobDeclaration.JE_DeclarationReference.IsEmpty);
			Assert("Declaration Reference number", testJobDeclaration.JE_DeclarationReference.StartsWith("B"));
		}

		public void TestISupportDataImporting()
		{
			testJobDeclaration.IsImportingData = true;
			AssertEquals("IsImporting", true, testJobDeclaration.IsImportingData);
		}

		public void TestImportEntryNumer()
		{
			JobDeclaration declaration = SetUpImportDec(Factory);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			Assert("PreCondition:CusEntryHeader should be created", declaration.CustomsEntryHeaders.Count > 0);

			string expected = SetUpImportEntryNumbers(declaration.CustomsEntryHeaders);
			AssertEquals("ImportDeclarationNumber", expected, declaration.ImportEntryNumbers);
		}

		string SetUpImportEntryNumbers(ICusEntryHeaderCollection<CusEntryHeader> collection)
		{
			string result = "";
			foreach (CusEntryHeader entryHeader in collection)
			{
				CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
				entryNumber.CE_ParentID = entryHeader.PK;
				entryNumber.CE_ParentTable = entryHeader.TableName;
				entryNumber.CE_EntryType = "IMP";
				entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				entryNumber.CE_EntryNum = "ZZ";
				result += entryNumber.CE_EntryNum + ",";
			}
			return result.Trim(',');
		}

		public void TestIsWithdrawn()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals("IsWithdrawn", false, declaration.IsWithdrawn);
			declaration.JE_EntryStatus = CustomsEntryStatus.ErrorWithdrawal.Code;
			AssertEquals("IsWithdrawn", true, declaration.IsWithdrawn);
		}

		public void TestMasterBillNumberValidated()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_MasterBill = "08155555555";
			Assert("This master bill is valid", !declaration.JE_MasterBillInfo.HasNotifications());

			declaration.JE_MasterBill = "08155555551";
			Assert("This master bill not valid", declaration.JE_MasterBillInfo.HasNotifications());
		}

		[TestDate(2004, 10, 05)]
		public void TestChangesOnMessageTypeUpdatesCurrencyConverterRateType()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_ExportDate = new ZDateTime(2003, 12, 31);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("Currency Converter can take Customs rate type", ZArchitecture.Core.ExchangeRateType.Customs, invoiceHeader.CurrencyConverter.RateType);
		}

		[TestDate(2004, 10, 05)]
		public void TestChangesOnExportDateUpdatesCurrencyConverterRateType()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_ExportDate = new ZDateTime(2003, 12, 31);

			declaration.JE_ExportDate = ZDateTime.Today;
			AssertEquals("Currency Converter can take Customs Rate Type", ZArchitecture.Core.ExchangeRateType.Customs, invoiceHeader.CurrencyConverter.RateType);
		}

		public void TestCorrectCurrencyConverterForEdifice()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ExportDate = new ZDateTime(2003, 12, 31);

			AssertEquals("Currency Converter can only take Customs of Rate Type", ZArchitecture.Core.ExchangeRateType.Customs, invoiceHeader.CurrencyConverter.RateType);
		}

		public void TestTemplateCopy()
		{
			JobDeclaration declaration = CreateTestDeclaration();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "TESTTEST";

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			ErrorReporter.Clear();
			JobDeclaration copiedDeclaration = (JobDeclaration)declaration.TemplateCopy();

			AssertEquals("Foreign Key to Shipment Cleared on TemplateCopy", ZGuid.Empty, copiedDeclaration.JE_JS);
			AssertEquals("Ref ID Cleared on TemplateCopy", ZString.Empty, copiedDeclaration.JE_DeclarationReference);
			Factory.Save();
			AssertEquals("Date Cleared on TemplateCopy", ZDateTime.Empty, copiedDeclaration.JE_ExportDate);
			AssertEquals("Containers on TemplateCopy", (short)0, copiedDeclaration.JE_ContainerCount);
			AssertEquals("Invoices on TemplateCopy", 1, copiedDeclaration.Invoices.Count);
			AssertEquals("InvoiceLines on TemplateCopy", 1, copiedDeclaration.InvoiceLines.Count);
			AssertEquals("FilteredInvoiceLines on TemplateCopy", 1, copiedDeclaration.FilteredInvoiceLines.Count);
		}

		public void TestValidateJE_MessageType()
		{
			Assert(!testJobDeclaration.HasNotifications());
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			Assert(!testJobDeclaration.JE_MessageTypeInfo.HasMessageErrors());

			Assert(!testJobDeclaration.JE_MessageTypeInfo.HasNotifications());
			testJobDeclaration.JE_MessageType = "";
			Assert(testJobDeclaration.JE_MessageTypeInfo.HasErrors());

			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			Assert(!testJobDeclaration.JE_MessageTypeInfo.HasMessageErrors());

			Assert(!testJobDeclaration.HasErrors);
			testJobDeclaration.JE_MessageType = "JNK";
			Assert(testJobDeclaration.HasMessageErrors);
		}

		public void TestSettingImporterChangesMessageType()
		{
			OrgHeader importerInAustralia = Factory.New<OrgHeader>();
			importerInAustralia.OH_IsConsignee = true;
			importerInAustralia.OH_RL_NKClosestPort = "AUSYD";
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			testJobDeclaration.JE_OH_Importer = importerInAustralia.PK;
			AssertEquals(Customs.Business.JobMessageTypeList.Codes.Import, testJobDeclaration.JE_MessageType);
		}

		public void TestSettingSupplierChangesMessageType()
		{
			OrgHeader supplierInAustralia = Factory.New<OrgHeader>();
			supplierInAustralia.OH_IsConsignor = true;
			supplierInAustralia.OH_RL_NKClosestPort = "AUSYD";
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testJobDeclaration.JE_OH_Supplier = supplierInAustralia.PK;
			AssertEquals(Customs.Business.JobMessageTypeList.Codes.Export, testJobDeclaration.JE_MessageType);
		}

		public void TestValidateEstablishmentCode()
		{
			// Conditional on Customable Excisable Indicator

			OrgHeader headerWithRego = Factory.New<OrgHeader>();
			headerWithRego.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "W123N");
			OrgAddress addressWithRego = headerWithRego.Addresses.AddNew();

			OrgHeader headerWithoutRego = Factory.New<OrgHeader>();
			OrgAddress addressWithoutRego = headerWithoutRego.Addresses.AddNew();

			testJobDeclaration.WarehouseDocAddress.E2_OA_Address = addressWithoutRego.PK;
			AssertEquals("Customs Registration Number Required Message", true, testJobDeclaration.WarehouseDocAddress.E2_OA_AddressInfo.HasMessageErrors());
			// Validate when Warehouse is selected, the Customs Excisable Indicator is true, and Establishment is required
			testJobDeclaration.WarehouseDocAddress.E2_OA_Address = addressWithRego.PK;
			AssertEquals("Customs Registration Number should be found on this record", false, testJobDeclaration.WarehouseDocAddress.E2_OA_AddressInfo.HasMessageErrors());
		}

		public void TestOriginCalculatedFromSupplier()
		{
			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true));
			testJobDeclaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("JE_RL_NKOrigin should default to Suppliers Home Port", supplier.OH_RL_NKClosestPort, testJobDeclaration.JE_RL_NKOrigin);
		}

		public void TestDestinationCalculatedFromImporter()
		{
			var importer = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true));
			testJobDeclaration.JE_OH_Importer = importer.PK;
			AssertEquals("JE_RL_NKOrigin should default to Suppliers Home Port", importer.OH_RL_NKClosestPort, testJobDeclaration.JE_RL_NKFinalDestination);
		}

		public void TestDelete()
		{
			ZGuid decPK = CreateAndSaveTestRecord();

			BusinessObjectFactory factory = new BusinessObjectFactory();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			var declarationToDelete = factory.Load<JobDeclaration>(decPK);
			JobDeclaration declarationToDelete2 = factory2.Load<JobDeclaration>(decPK);
			declarationToDelete.Delete();
			factory.Save();
			BusinessObjectFactory tempFactory = new BusinessObjectFactory();
			JobDeclaration deletedDecCheck = tempFactory.Load<JobDeclaration>(decPK);
			AssertNull("Declaration should not exist in DB", deletedDecCheck);
		}

		public void TestLoadMessages()
		{
			ZGuid decPK = SetUpAndSaveDeclaration();

			BusinessObjectFactory factory = new BusinessObjectFactory();
			var jobDec = factory.Load<JobDeclaration>(decPK);
			AssertEquals("No Message sent", 0, jobDec.Messages.Count);

			var sendStatus = jobDec.SendDeclarationOriginal(ExportDeclarationType.Replacement);
			AssertEquals("1 message sent", 1, jobDec.Messages.Count);
			AssertEquals("Success", sendStatus.ToString());
		}

		public void TestSendMessageConcurrencyErrorStatus()
		{
			ZGuid decPK = SetUpAndSaveDeclaration();

			var factory = new BusinessObjectFactory();
			var jobDec = factory.Load<JobDeclarationToTestException>(decPK);
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			jobDec.MessageInitiator = messageInitiator;
			jobDec.ExceptionToThrow = new ZSaveConcurrencyException(new ZDataConcurrencyException(new InvalidOperationException(), null, null), factory);
			jobDec.SendExportDeclaration();
			AssertNull("Failed message not saved in database", jobDec.Messages.LastMessage);
			AssertEquals("A system error has occured, declaration message not sent. Please reload the form and try again.", messageInitiator.InvalidOperationText);
			AssertEquals(null, messageInitiator.SuccessfulSendText);
		}

		public void TestSendMessageExceptionErrorStatus()
		{
			ZGuid decPK = SetUpAndSaveDeclaration();

			var factory = new BusinessObjectFactory();
			var jobDec = factory.Load<JobDeclarationToTestException>(decPK);
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			jobDec.MessageInitiator = messageInitiator;
			jobDec.ExceptionToThrow = new Exception("Fail for testing");

			try
			{
				jobDec.SendExportDeclaration();
				AssertEquals("An error should be reported", 1, Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count);
				AssertEquals("Fail for testing", Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance[0].Message);
			}
			finally
			{
				ErrorReporter.Clear();
			}

			AssertEquals("Failed message not saved in database", false, jobDec.Messages.LastMessage.IsInDatabase);
			AssertEquals(null, messageInitiator.InvalidOperationText);  // The exception should be caught by the catch in SendDeclarationMessage and be displayed by Globals.Message.ShowDeveloperException(ex);
			AssertEquals(null, messageInitiator.SuccessfulSendText);
		}

		public class JobDeclarationToTestException : JobDeclaration
		{
			public Exception ExceptionToThrow { get; set; }

			public JobDeclarationToTestException(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override void PopulateEntrySubmittedDate(ZDateTime value)
			{
				if (ExceptionToThrow != null)
				{
					throw ExceptionToThrow;
				}
				else
				{
					base.PopulateEntrySubmittedDate(value);
				}
			}
		}

		public void TestExportDeclarationNumber()
		{
			Assert("Pre-Condition", testJobDeclaration.IsExport);
			Assert("Pre-Condition", testJobDeclaration.CustomsAuthorityNumber.IsEmpty);

			testJobDeclaration.JE_ApplicationCode = "";
			testJobDeclaration.DeclarationNumber = "123";
			AssertEquals("TestJobDeclaration.ExportDeclarationNumber", "123", testJobDeclaration.DeclarationNumber);
			Assert("Should be true as the declaration has a valid Customs Authority Number", !testJobDeclaration.CustomsAuthorityNumber.IsEmpty);

			testJobDeclaration.DeclarationNumber = "456";
			AssertEquals("TestJobDeclaration.ExportDeclarationNumber", "456", testJobDeclaration.DeclarationNumber);
			Assert("Should be true as the declaration has a valid Customs Authority Number", !testJobDeclaration.CustomsAuthorityNumber.IsEmpty);
		}

		public void TestDrawbackClaimID()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			declaration.DeclarationNumber = "123";
			AssertEquals("DrawbackClaimID", "123", declaration.DeclarationNumber);
			Assert(declaration.DrawbackClaimID != null);
			AssertEquals(CusEntryNumberTypes.Standard.DrawbackClaim, declaration.DrawbackClaimID.CE_EntryType);
			AssertEquals("123", declaration.DrawbackClaimID.CE_EntryNum);
			declaration.DeclarationNumber = "456";
			AssertEquals("DrawbackClaimID", "456", declaration.DeclarationNumber);
			AssertEquals("456", declaration.DrawbackClaimID.CE_EntryNum);
		}

		public void TestExportDeclarationNumberSetToEmpty()
		{
			var sQLFilter = new ZQuery();
			sQLFilter.AddToFilter(CusEntryNumSchema.CE_ParentID, testJobDeclaration.PK);
			sQLFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Australia.CAN);
			sQLFilter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, "AU");

			var permits = Factory.Load(typeof(AUCusEntryNumber), sQLFilter);
			AssertEquals("PermitCount", 0, permits.Length);
			Assert("Should be false as there is no Customs Authority Number.", testJobDeclaration.CustomsAuthorityNumber.IsEmpty);

			testJobDeclaration.JE_ApplicationCode = "";
			testJobDeclaration.DeclarationNumber = "123";
			permits = Factory.Load(typeof(AUCusEntryNumber), sQLFilter);
			AssertEquals("PermitCount", 1, permits.Length);
			Assert("Should be true as there is a Customs Authority Number.", !testJobDeclaration.CustomsAuthorityNumber.IsEmpty);

			testJobDeclaration.DeclarationNumber = "124";
			permits = Factory.Load(typeof(AUCusEntryNumber), sQLFilter);
			AssertEquals("PermitCount", 1, permits.Length);
			Assert("Should be true as there is a Customs Authority Number.", !testJobDeclaration.CustomsAuthorityNumber.IsEmpty);

			AssertEquals("pre-condition", 0, new LogsForNominatedEvent(testJobDeclaration.Logs, Events.DeletedARecordInTheSystem).Count);
			testJobDeclaration.DeclarationNumber = ZString.Empty;
			permits = Factory.Load(typeof(AUCusEntryNumber), sQLFilter);

			AssertEquals("PermitCount", 0, permits.Length);
			AssertEquals("log posted", 1, new LogsForNominatedEvent(testJobDeclaration.Logs, Events.DeletedARecordInTheSystem).Count);
			Assert("Should be false as there is no Customs Authority Number.", testJobDeclaration.CustomsAuthorityNumber.IsEmpty);

			testJobDeclaration.DeclarationNumber = "125";
			permits = Factory.Load(typeof(AUCusEntryNumber), sQLFilter);
			AssertEquals("PermitCount", 1, permits.Length);
			Assert("Should be true as there is a Customs Authority Number.", !testJobDeclaration.CustomsAuthorityNumber.IsEmpty);
		}

		public void TestOnSavingClearsContainer()
		{
			testJobDeclaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			testJobDeclaration.JE_ContainerCount = 10;
			testJobDeclaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			testJobDeclaration.Factory.Save();
			//TestJobDeclaration.OnSaving();
			AssertEquals("Container count cleared", (short)0, testJobDeclaration.JE_ContainerCount);
		}

		public void TestOnSavingUpdateCusHeaderBGMReferences()
		{
			CusEntryHeader header1 = testJobDeclaration.CustomsEntryHeaders.AddNew();
			CusEntryHeader header2 = testJobDeclaration.CustomsEntryHeaders.AddNew();
			CusEntryHeader header3 = testJobDeclaration.CustomsEntryHeaders.AddNew();
			testJobDeclaration.Factory.Save();
			AssertEquals("BGMREference", testJobDeclaration.JE_DeclarationReference + "/1", header1.CH_BGMReference);
			AssertEquals("BGMREference", testJobDeclaration.JE_DeclarationReference + "/2", header2.CH_BGMReference);
			AssertEquals("BGMREference", testJobDeclaration.JE_DeclarationReference + "/3", header3.CH_BGMReference);
		}

		public void TestOnSavingUpdateInvoiceInsuranceGroupCharges() => CombineAssertions(() =>
		{
			var insurance = Factory.NewWithValidTestData<CusCalculationRule>();
			insurance.CCR_StartDate = new ZDateTime(2024, 05, 01).ToOffset();
			insurance.CCR_EndDate = new ZDateTime(2024, 05, 31).ToOffset();
			insurance.CCR_Formula = "IF(VFD >= 3000, 1000, IF(VFD >= 2000, 500, 10))";
			insurance.CCR_BasedOn = Core.Constants.IncoTerms.FreeOnBoard;
			insurance.CCR_RX_NKCurrency = "AUD";
			insurance.CCR_TransportMode = ZString.Empty;
			Factory.Save();

			testJobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var groupHeader = testJobDeclaration.JobComInvoiceGroupHeaders[0];
			var invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_ValuationDateOverride = new ZDateTime(2024, 5, 7);
			invoice1.JZ_RX_NKInvoice_Currency = "AUD";
			invoice1.JZ_InvoiceAmount = 2100;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			var oftCharge1 = invoice1.Charges[CustomsChargeTypeList.Codes.OverseasFreight];
			oftCharge1.J7_Amount = 100m;
			AssertEquals("invoice1 EffectiveFOBAmount", 2000m, invoice1.EffectiveFOBAmount);

			var onsCharge = groupHeader.Charges[CustomsChargeTypeList.Codes.OverseasInsurance];
			AssertEquals("ValueForDuty", 2000m, ((IUniversalRateCalcData)groupHeader).ValueForDuty);
			AssertEquals("Insurance amount", 500m, onsCharge.J7_Amount);
			AssertEquals("J7_IsCalculated", true, onsCharge.J7_IsCalculated);

			var invoice2 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_ValuationDateOverride = new ZDateTime(2024, 5, 7);
			invoice2.JZ_RX_NKInvoice_Currency = "AUD";
			invoice2.JZ_InvoiceAmount = 1150;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			var oftCharge2 = invoice2.Charges[CustomsChargeTypeList.Codes.OverseasFreight];
			oftCharge2.J7_Amount = 150m;
			AssertEquals("invoice2 EffectiveFOBAmount", 1000m, invoice2.EffectiveFOBAmount);

			AssertEquals("Before saved insurance amount is still 500", 500m, onsCharge.J7_Amount);
			testJobDeclaration.OnSaving();
			AssertEquals("ValueForDuty with 2nd invoice", 3000m, ((IUniversalRateCalcData)groupHeader).ValueForDuty);
			AssertEquals("Insurance amount with 2nd invoice", 1000m, onsCharge.J7_Amount);
			AssertEquals("J7_IsCalculated", true, onsCharge.J7_IsCalculated);

			onsCharge.J7_IsCalculated = false;
			onsCharge.J7_Amount = 750m;
			testJobDeclaration.OnSaving();
			AssertEquals("Insurance amount is not updated when J7_IsCalculated = False", 750m, onsCharge.J7_Amount);
			AssertEquals("J7_IsCalculated", false, onsCharge.J7_IsCalculated);

			onsCharge.J7_IsCalculated = true;
			testJobDeclaration.OnSaving();
			AssertEquals("Insurance amount updated when J7_IsCalculated = True", 1000m, onsCharge.J7_Amount);
			AssertEquals("J7_IsCalculated", true, onsCharge.J7_IsCalculated);
		});

		public void TestCurrencyConverter()
		{
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader header = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			AssertEquals("Default CurrencyConverter RateType for Edifice", Enterprise.ZArchitecture.Core.ExchangeRateType.Customs, header.CurrencyConverter.RateType);
			ZDateTime testDate = new ZDateTime(2000, 1, 2);
			testJobDeclaration.JE_ExportDate = testDate;
			AssertEquals("CurrencyConverter Date after setting declaration date", testDate, header.CurrencyConverter.DateForRate);
		}

		[ExpectNoExceptions]
		public void TestSuccessfulDeletionCustomsEntries()
		{
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader header = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			RefCurrency aUDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			header.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			JobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();

			testJobDeclaration.DoMerge();

			CusEntryHeaderCollection headers = new CusEntryHeaderCollection(testJobDeclaration, Factory);
			headers.Load();
			AssertEquals("One Customs Entry header", 1, headers.Count);

			testJobDeclaration.DoMerge(); //Delete the previous merging result
		}

		public void TestUpdateMessageTypeInInvoiceLine()
		{
			JobComInvoiceHeader header = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("Invoice Line Message Type", Customs.Business.JobMessageTypeList.Codes.Export, line.MessageType);

			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("Invoice Line Message Type", Customs.Business.JobMessageTypeList.Codes.Import, line.MessageType);
		}

		public void TestMakeContainerModeReadOnlyWhenAir()
		{
			testJobDeclaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			AssertEquals("Container Mode ReadOnly", true, testJobDeclaration.JE_ContainerModeInfo.ReadOnly);
		}

		public void TestDoChangesResultInADifferentMessageEX1OrgFalse()
		{
			ZGuid savedDecPK = SetUpAndSaveDeclaration();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var declaration = factory.Load<JobDeclaration>(savedDecPK);
			declaration.JE_HouseBill = "sdfSDFsdf";
			AssertEquals("DeclarationChangesResultInMessageChanges", false, declaration.DoChangesResultInADifferentMessage(ExportDeclarationType.Original));
		}

		public void TestAddInfo()
		{
			string testAddInfo = "DTY=23.24*AMB=DVTQPOC*LCTE=404*ICN=C23000H";
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_AddInfo = testAddInfo;
			AssertEquals("Declaration AddInfo Object Field - Duty", 23.24m, declaration.AddInfo.ZA_DTY);
			AssertEquals("Declaration AddInfo Object Field - Amber Processing", "DVTQPOC", declaration.AddInfo.ZA_AMB);
			AssertEquals("Declaration AddInfo Object Field - Luxury Car Tax Exemption", "404", declaration.AddInfo.ZA_LCTE);
			AssertEquals("Declaration AddInfo Object Field - Import Credit Number", "C23000H", declaration.AddInfo.ZA_ICN);
			AssertEquals("Declaration AddInfo Object Field - Origin", "", declaration.AddInfo.ZA_ORG);
			AssertEquals("Declaration AddInfo Object Field - Invoice Spirit Strength", 0m, declaration.AddInfo.ZA_ISS);
		}

		public void TestGetGoodsValueWithSameValidCurrency()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceHeader invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			invoiceHeader1.JZ_InvoiceAmount = 1000m;
			invoiceHeader2.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			invoiceHeader2.JZ_InvoiceAmount = 2000m;

			AssertEquals("Goods value", 3000m, declaration.GoodsValue.Amount);
			AssertEquals("Goods value currency", aUDCurrency.PK, declaration.GoodsValue.Currency.PK);
		}

		public void TestGetGoodsValueWithMultipleCurrencies()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceHeader invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			invoiceHeader1.JZ_InvoiceAmount = 1000m;
			invoiceHeader2.JZ_RX_NKInvoice_Currency = uSDCurrency.RX_Code;
			invoiceHeader2.JZ_InvoiceAmount = 2000m;

			ZDecimal expected = invoiceHeader1.CurrencyConverter.ConvertRounded(new Money(2000m, invoiceHeader2.Invoice_Currency), JobDeclaration.GetLocalCurrency()).Amount + 1000m;
			AssertEquals("Goods value", expected, declaration.GoodsValue.Amount);
			AssertEquals("Goods value currency", JobDeclaration.LocalCurrencyConstantCode, declaration.GoodsValue.Currency.Code);
		}

		public void TestInvoiceLinesLoadFromHierarchicalInvoices()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeaderG01 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceHeader invoiceHeaderG02 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeaderG01.JobComInvoiceLines.AddNew();
			invoiceHeaderG01.JobComInvoiceLines.AddNew();
			invoiceHeaderG02.JobComInvoiceLines.AddNew();
			invoiceHeaderG02.JobComInvoiceLines.AddNew();

			AssertEquals("Invoice Lines on Declaration", 4, declaration.FilteredInvoiceLines.Count);

			invoiceHeaderG02.JobComInvoiceLines.AddNew();
			AssertEquals("Invoice Lines on Declaration", 5, declaration.FilteredInvoiceLines.Count);

			Factory.Save();

			BusinessObjectFactory databaseFactory = new BusinessObjectFactory();
			JobDeclaration loadedDeclaration = databaseFactory.Load<JobDeclaration>(declaration.PK);

			AssertEquals("Declaration saved to database invoice line count", 5, loadedDeclaration.FilteredInvoiceLines.Count);
		}

		public void TestAddedDeclarationIsNotReadOnlyOnRecall()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration declaration2 = factory2.Load<JobDeclaration>(declaration.PK);
			Assert("New declaration should be editable immediately after save", !declaration2.ReadOnly);
		}

		public void TestHasNature20()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();
			invoiceLine1.JI_IsPackToBondForLine = true;
			invoiceLine2.JI_IsPackToBondForLine = false;

			AssertEquals("Has Nature20", true, declaration.HasNature20Entry);
		}

		public void TestHasNature20False()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();

			AssertEquals("Has Nature20", false, declaration.HasNature20Entry);
		}

		public void TestGSTExemptLine()
		{
			JobDeclaration testDec = CreateSendableDeclaration();
			testDec.DoMerge();
			AssertEquals("Number Of Customs Entries", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("Number Of Customs Lines", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("GST Owed", 0m, testDec.CustomsEntryHeaders[0].MergedLines[0].GSTVATAmount);
		}

		public void TestMessageTypeAndSubType()
		{
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("Message Sub Type for import", JobDeclaration.MessageSubType.FormalEntry, testJobDeclaration.JE_MessageSubType);

			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("Message Sub type for export", JobDeclaration.MessageSubType.NonConfirming, testJobDeclaration.JE_MessageSubType);
		}

		public void TestJE_AdditionalStatusInformation1()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_EntryStatus = CustomsEntryStatus.ClearCreate.Code;
			AssertEquals("Additional Status Info", "", testDec.JE_AdditionalStatusInformation);
			testDec.PlaceHold("Waiting for clients OK");
			AssertEquals("Additional Status Info", "\tReason: 'Waiting for clients OK'\r\n", testDec.JE_AdditionalStatusInformation);
		}

		public void TestJE_EntryStatusDescription()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.PlaceDeclarationWorkComplete("Withdrawn");
			AssertEquals("EntryStatusDescription", "Declaration Work Complete: Reason: 'Withdrawn'", testDec.JE_EntryStatusDescription);
		}

		public void TestJE_AdditionalStatusInformation2()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_EntryStatus = CustomsEntryStatus.ClearCreate.Code;
			AssertEquals("Additional Status Info", "", testDec.JE_AdditionalStatusInformation);
			testDec.PlaceDeclarationWorkComplete("Had to go manual");
			AssertEquals("Additional Status Info", "\tReason: 'Had to go manual'\r\n", testDec.JE_AdditionalStatusInformation);
		}

		public void TestJE_AdditionalStatusInformation3()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_EntryStatus = CustomsEntryStatus.ClearLodge.Code;
			AssertEquals("Additional Status Info", "", testDec.JE_AdditionalStatusInformation);
			CusEntryHeader entryHeader1 = testDec.CustomsEntryHeaders.AddNew();
			entryHeader1.EntryNumber = "12345";
			entryHeader1.LogImpediment("Your entry was red-lined.");
			testDec.PlaceDeclarationWorkComplete("Had to go manual");
			AssertEquals("Additional Status Info", "\tReason: 'Had to go manual'\r\n" +
				"Entry 12345 impediment(s):\r\n" +
				"Your entry was red-lined.\r\n", testDec.JE_AdditionalStatusInformation);
		}

		public void TestIncorrectGST()
		{
			OrgHeader supplier = OrgHeader.LoadFromCode(Factory, "ABIGAS");
			OrgHeader importer = OrgHeader.LoadFromCode(Factory, "ABABEU");

			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.GSTCode, "90093519530");
			supplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.SupplierCode, "955411R");
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			JobComInvoiceHeader testHeader1 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			using (testDec.GetValidationSuspender())
			{
				testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

				testDec.JE_OH_Importer = importer.PK;
				testDec.JE_OH_Supplier = supplier.PK;
				testDec.JE_AgentsReference = "195158 -";
				testDec.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.AIR;
				testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				testDec.JE_DateOfArrival = new ZDateTime(2003, 11, 30, 6, 56, 0);
				testDec.JE_DateOfFirstArrival = new ZDateTime(2003, 11, 30, 6, 56, 0);
				testDec.JE_DeclarationReference = "B00102668";
				testDec.JE_EntryStatus = "FLL";
				testDec.JE_ExportDate = new ZDateTime(2003, 11, 27, 6, 56, 0);
				testDec.JE_ExportGoodsType = "OT";
				testDec.JE_GB = GlbBranch.CurrentBranch.PK;
				testDec.JE_HouseBill = "903085";
				testDec.JE_MasterBill = "08654297913";
				testDec.JE_MessageSubType = "FRM";
				testDec.AddInfo.ZA_MergeBy_Hidden = OrgConstants.MergeInvoiceLines.Tariff;
				testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
				testDec.JE_OwnerRef = "INV 623365/8,70";
				testDec.JE_RL_NKFinalDestination = "AUBNE";
				testDec.JE_RL_NKOrigin = "DEFRA";
				testDec.JE_RL_NKPortOfArrival = "AUBNE";
				testDec.JE_RL_NKPortOfFirstArrival = "AUBNE";
				testDec.JE_RL_NKPortOfLoading = "NZCHC";
				testDec.JE_TotalNoOfPacks = 1;
				testDec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
				testDec.JE_VoyageFlightNo = "NZ555";

				testDec.JobComInvoiceGroupHeaders[0].JZ_GroupInvoice = true;
				testDec.JobComInvoiceGroupHeaders[0].JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				testDec.JobComInvoiceGroupHeaders[0].JZ_InvoiceCurrExRate = 1.000000000m;
				testDec.JobComInvoiceGroupHeaders[0].JZ_InvoiceDate = new ZDateTime(2003, 11, 27);

				testHeader1.JZ_AddInfo = "PRF=S*ORG=NZ*PackCountForNature10_Hidden=1*ValuationBasis_Hidden=RT";
				testHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				testHeader1.JZ_InvoiceAmount = 65688.1600m;
				testHeader1.JZ_InvoiceCurrExRate = 1.000000000m;
				testHeader1.JZ_InvoiceDate = new ZDateTime(2003, 11, 27);
				testHeader1.JZ_InvoiceNumber = "1";
				testHeader1.JZ_OH_Supplier = supplier.PK;
				testHeader1.JZ_RX_NKInvoice_Currency = "AUD";
				testHeader1.JZ_VolumeUQ = "M3";
				testHeader1.JZ_Weight = 356.000m;
				testHeader1.JZ_WeightUQ = "KG";

				JobComInvoiceLine testLine1 = CreateInvoiceLine(testHeader1, "PRF=X*ORG=MLAY", 2.0000m, "NO", "000-10029-55 XSTR",
					2.00000m, "NO", 1, 4.2000m, "MY", "8541.29.00 39", 2.000m, "KG");
				JobComInvoiceLine testLine2 = CreateInvoiceLine(testHeader1, "ORG=USA", 2.0000m, "NO", "002-00010-75",
					2.00000m, "NO", 2, 6.8600m, "US", "8542.21.00 62", 2.000m, "KG");
				JobComInvoiceLine testLine3 = CreateInvoiceLine(testHeader1, "ORG=USA", "For goods of 8525 NSA, 8526 or 8527.90.90",
					3, 88.6200m, "US", "8529.90.30 80", "KG");
				JobComInvoiceLine testLine4 = CreateInvoiceLine(testHeader1, "ORG=USA", "240-04021-63 CONNECTOR", 4, 48.600m, "US", "8536.69.90 63", "KG");
				JobComInvoiceLine testLine5 = CreateInvoiceLine(testHeader1, "ORG=USA", 20.0000m, "NO", "356-01070-00",
					20.00000m, "NO", 5, 98.0000m, "US", "7320.90.00 23", 20.000m, "KG");
				JobComInvoiceLine testLine6 = CreateInvoiceLine(testHeader1, "PRF=X*ORG=TAIW", 10.0000m, "NO", "001-00011-70 DIODE",
					10.00000m, "NO", 6, 8.1000m, "TW", "8541.10.00 15", 10.000m, "KG");
				JobComInvoiceLine testLine7 = CreateInvoiceLine(testHeader1, "ORG=USA", 13.0000m, "NO", "002-00010-75",
					13.00000m, "NO", 7, 44.5900m, "US", "8542.21.00 62", 13.000m, "KG");
				JobComInvoiceLine testLine8 = CreateInvoiceLine(testHeader1, "ORG=USA", "240-04030-10 CONNECTOR", 8, 18.3500m, "US", "8536.69.90 63", "KG");
				JobComInvoiceLine testLine9 = CreateInvoiceLine(testHeader1, "ORG=USA", 1.0000m, "NO", "002-00010-75",
					1.00000m, "NO", 9, 12.0400m, "US", "8542.21.00 62", 1.000m, "KG");
				JobComInvoiceLine testLine10 = CreateInvoiceLine(testHeader1, "ORG=JAP", 5.0000m, "NO", "232-00010-19 SWITCH",
					5.00000m, "NO", 10, 4.0500m, "JP", "8536.50.99 61", 5.000m, "KG");
				JobComInvoiceLine testLine11 = CreateInvoiceLine(testHeader1, "ORG=JAP", "240-00026-20 PLUG", 11, 0.0500m, "JP", "8536.69.90 63", "KG");
				JobComInvoiceLine testLine12 = CreateInvoiceLine(testHeader1, "ORG=USA", 5.0000m, "NO", "000-00023-14 XSTR",
					5.00000m, "NO", 12, 27.2000m, "US", "8541.29.00 39", 5.000m, "KG");
				JobComInvoiceLine testLine13 = CreateInvoiceLine(testHeader1, "ORG=USA", 2.0000m, "NO", "002-00018-30 IC",
					2.00000m, "NO", 13, 18.7200m, "US", "8542.21.00 62", 2.000m, "KG");
				JobComInvoiceLine testLine14 = CreateInvoiceLine(testHeader1, "PRF=X*ORG=INIA", 10.0000m, "240-04020-53 SOCKET",
					10.00000m, "NO", 14, 8.1000m, "IN", "8536.69.90 63", 10.000m, "KG");
				JobComInvoiceLine testLine15 = CreateInvoiceLine(testHeader1, "PRF=X*ORG=HONG", "T952-012 PLUG PACK", 15, 67.2000m, "HK", "8536.69.90 63", "KG");
				JobComInvoiceLine testLine16 = CreateInvoiceLine(testHeader1, "ORG=USA", 10.0000m, "NO", "MICROPHONE",
					10.00000m, "NO", 16, 1100.0000m, "US", "8518.10.90 89", 10.000m, "KG");
				JobComInvoiceLine testLine17 = CreateInvoiceLine(testHeader1, "ORG=USA", "ANTENNA", 17, 457.0000m, "US", "8529.10.20 77", "KG");
				JobComInvoiceLine testLine18 = CreateInvoiceLine(testHeader1, "ORG=USA", 4.0000m, "NO", "MICROPHONE",
					4.00000m, "NO", 18, 336.0000m, "US", "8518.10.90 89", 4.000m, "KG");
				JobComInvoiceLine testLine19 = CreateInvoiceLine(testHeader1, "ORG=USA", "ANTENNA", 19, 145.0000m, "US", "8529.10.20 77", "KG");

				JobComInvoiceLine testLine20 = CreateInvoiceLine(testHeader1, 3.0000m, "NO", "T2000-05 KIT", 3.00000m, "NO",
					20, 69.0000m, "8518.21.00 22", 3.000m, "KG");

				JobComInvoiceLine testLine21 = CreateInvoiceLine(testHeader1, 15.0000m, "NO", "MICROPHONE", 15.00000m, "NO", 21, 420.0000m, "8518.10.90 89", 15.000m, "KG");
				JobComInvoiceLine testLine22 = CreateInvoiceLine(testHeader1, 1.0000m, "KG", "PROGRAMMING LEAD", 1.00000m, "KG", 22, 56.0000m, "8544.41.90 31", 1.000m, "KG");
				JobComInvoiceLine testLine23 = CreateInvoiceLine(testHeader1, 1.0000m, "KG", "T2000-15 POWER LEAD", 1.00000m, "KG", 23, 76.0000m, "8544.51.90 53", 1.000m, "KG");
				JobComInvoiceLine testLine24 = CreateInvoiceLine(testHeader1, 6.0000m, "NO", "MICROPHONE", 6.00000m, "NO", 24, 168.0000m, "8518.10.90 89", 6.000m, "KG");
				JobComInvoiceLine testLine25 = CreateInvoiceLine(testHeader1, 1.0000m, "KG", "PROGRAMMING LEAD", 1.00000m, "KG", 25, 84.0000m, "8544.41.90 31", 1.000m, "KG");
				JobComInvoiceLine testLine26 = CreateInvoiceLine(testHeader1, 1.0000m, "NO", "MANUALS", 1.00000m, "NO", 26, 38.0000m, "4901.99.90 05", 1.000m, "KG");
				JobComInvoiceLine testLine27 = CreateInvoiceLine(testHeader1, 5.0000m, "KG", "410-01066-00 CRTN", 5.00000m, "KG", 27, 258.5000m, "4819.20.00 10", 5.000m, "KG");
				JobComInvoiceLine testLine28 = CreateInvoiceLine(testHeader1, "", "BROCHURE", 28, 25.0000m, "", "4911.10.90 36", "KG");
				JobComInvoiceLine testLine29 = CreateInvoiceLine(testHeader1, 28.0000m, "", "TRANSCEIVER", 28.00000m, "NO", 29, 10220.0000m, "8525.20.00 89", 28.000m, "KG");
				JobComInvoiceLine testLine30 = CreateInvoiceLine(testHeader1, 1.0000m, "", "HEADSET", 1.00000m, "NO", 30, 19.0000m, "8518.30.90 10", 1.000m, "KG");
				JobComInvoiceLine testLine31 = CreateInvoiceLine(testHeader1, 17.0000m, "", "TRANSCEIVER", 17.00000m, "NO", 31, 4931.0000m, "8525.20.00 89", 17.000m, "KG");
				JobComInvoiceLine testLine32 = CreateInvoiceLine(testHeader1, 43.0000m, "", "TRANSCEIVER", 43.00000m, "NO", 32, 11830.0000m, "8525.20.00 89", 43.000m, "KG");
				JobComInvoiceLine testLine33 = CreateInvoiceLine(testHeader1, 1.0000m, "KG", "PROGRAMMING LEAD", 1.00000m, "KG", 33, 94.0000m, "8544.41.90 31", 1.000m, "KG");
				JobComInvoiceLine testLine34 = CreateInvoiceLine(testHeader1, 1.0000m, "NO", "CD SOFTWARE", 1.00000m, "NO", 34, 84.0000m, "8524.31.00 01", 1.000m, "KG");
				JobComInvoiceLine testLine35 = CreateInvoiceLine(testHeader1, 1.0000m, "KG", "PROGRAMMING LEAD", 1.00000m, "KG", 35, 47.0000m, "8544.41.90 31", 1.000m, "KG");
				JobComInvoiceLine testLine36 = CreateInvoiceLine(testHeader1, "", "T952-112 LEATHER CASE", 36, 90.0000m, "", "4202.91.90 22", "KG");
				JobComInvoiceLine testLine37 = CreateInvoiceLine(testHeader1, 67.0000m, "NO", "Nickel-cadmium", 67.00000m, "NO", 37, 3170.0000m, "8507.30.00 82", 67.000m, "KG");
				JobComInvoiceLine testLine38 = CreateInvoiceLine(testHeader1, 8.0000m, "NO", "Other", 8.00000m, "NO", 38, 67.2000m, "8504.40.90 80", 8.000m, "KG");
				JobComInvoiceLine testLine39 = CreateInvoiceLine(testHeader1, 41.0000m, "NO", "Other", 41.00000m, "NO", 39, 2076.0000m, "8504.40.90 80", 41.000m, "KG");
				JobComInvoiceLine testLine40 = CreateInvoiceLine(testHeader1, 5.0000m, "", "TRANSCEIVER", 5.00000m, "NO", 40, 1650.0000m, "8525.20.00 89", 5.000m, "KG");
				JobComInvoiceLine testLine41 = CreateInvoiceLine(testHeader1, 5.0000m, "NO", "T800-80-0000", 5.00000m, "NO", 41, 110.0000m, "8518.10.90 90", 5.000m, "KG");
				JobComInvoiceLine testLine42 = CreateInvoiceLine(testHeader1, 1.0000m, "NO", "MANUALS", 1.00000m, "NO", 42, 38.6000m, "4901.99.90 05", 1.000m, "KG");
				JobComInvoiceLine testLine43 = CreateInvoiceLine(testHeader1, 1.0000m, "KG", "T2000-15 POWER LEAD", 1.00000m, "KG", 43, 19.0000m, "8544.51.90 53", 1.000m, "KG");
				JobComInvoiceLine testLine44 = CreateInvoiceLine(testHeader1, "", "TRANSMITTER", 44, 7582.0000m, "", "8525.10.10 01", "KG");
				JobComInvoiceLine testLine45 = CreateInvoiceLine(testHeader1, "", "Other", 45, 3132.0000m, "", "8527.90.90 14", "KG");
				JobComInvoiceLine testLine46 = CreateInvoiceLine(testHeader1, 5.0000m, "NO", "Other", 5.00000m, "NO", 46, 910.0000m, "8504.40.90 80", 5.000m, "KG");
				JobComInvoiceLine testLine47 = CreateInvoiceLine(testHeader1, 26.0000m, "", "TRANSCEIVER", 26.00000m, "NO", 47, 10025.0000m, "8525.20.00 89", 26.000m, "KG");
				JobComInvoiceLine testLine48 = CreateInvoiceLine(testHeader1, "", "For goods of 8525 NSA, 8526 or 8527.90.90", 48, 4906.1800m, "", "8529.90.30 80", "KG");
				JobComInvoiceLine testLine49 = CreateInvoiceLine(testHeader1, "", "For goods of 8525 NSA, 8526 or 8527.90.90", 49, 1000.0000m, "", "8529.90.30 80", "KG");
			}

			testDec.DoMerge();
			AssertEquals("number of merged headers", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("number of merged lines", 30, testDec.CustomsEntryHeaders[0].MergedLines.Count);
		}

		JobDeclaration CreateTestDeclaration()
		{
			OrgHeader supplier = OrgHeader.LoadFromCode(Factory, "ABIGAS");
			OrgHeader importer = OrgHeader.LoadFromCode(Factory, "ABABEU");

			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.GSTCode, "90093519530");
			supplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.SupplierCode, "955411R");
			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, "2297108A");
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			using (testDec.GetValidationSuspender())
			{
				testDec.JE_OH_Importer = importer.PK;
				testDec.JE_OH_Supplier = supplier.PK;
				testDec.AddInfo.ZA_MergeBy_Hidden = OrgConstants.MergeInvoiceLines.Tariff;
				testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
				testDec.JE_AgentsReference = "198810 -MP";
				testDec.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
				testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				testDec.JE_DateOfArrival = new ZDateTime(2003, 12, 1, 10, 2, 0);
				testDec.JE_DateOfFirstArrival = new ZDateTime(2003, 12, 1, 10, 2, 0);
				testDec.JE_DeclarationReference = "B00103428";
				testDec.JE_EntryStatus = CustomsEntryStatus.ClearCreate.Code;
				testDec.JE_ExportDate = new ZDateTime(2003, 11, 28, 10, 2, 0);
				testDec.JE_ExportGoodsType = "OT";
				testDec.JE_GB = GlbBranch.CurrentBranch.PK;
				testDec.JE_MasterBill = "PONLCPH22002959";
				testDec.JE_MessageSubType = "FRM";
				testDec.JE_OwnerRef = "1200994221";
				testDec.JE_RL_NKFinalDestination = "AUMEL";
				testDec.JE_RL_NKOrigin = "DEFRA";
				testDec.JE_RL_NKPortOfArrival = "AUMEL";
				testDec.JE_RL_NKPortOfFirstArrival = "AUSYD";
				testDec.JE_RL_NKPortOfLoading = "DEHAM";
				testDec.JE_VesselName = "ADMIRALENGRACHT";
				testDec.JE_TotalNoOfPacks = 20;
				testDec.JE_TotalNoOfPacksPackType = "CT";
				testDec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				testDec.JE_VoyageFlightNo = "2038";
			}

			JobComInvoiceHeader testHeader1 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			using (testHeader1.GetValidationSuspender())
			{
				testHeader1.JZ_AddInfo = "ORG=DK*PackCountForNature10_Hidden=1*ValuationBasis_Hidden=UT";
				testHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
				testHeader1.JZ_InvoiceAmount = 124295.5000m;
				testHeader1.JZ_InvoiceCurrExRate = 1.000000000m;
				testHeader1.JZ_InvoiceDate = new ZDateTime(2003, 11, 28);
				testHeader1.JZ_InvoiceNumber = "1";
				testHeader1.JZ_OH_Supplier = supplier.PK;
				testHeader1.JZ_RX_NKInvoice_Currency = "AUD";
				testHeader1.JZ_VolumeUQ = "M3";
				testHeader1.JZ_Weight = 24859.100m;
				testHeader1.JZ_WeightUQ = "KG";
				testHeader1.JZ_Nature10PackCount = 20;
			}

			testDec.JobComInvoiceGroupHeaders[0].JZ_GroupInvoice = true;
			testDec.JobComInvoiceGroupHeaders[0].JZ_InvoiceDate = new ZDateTime(2003, 11, 28);

			JobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 2300, "USD");
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 310.73m, "AUD");

			CreateInvoiceLine(testHeader1, "GSTE=FOOD", 24859.1000m, "KG", "FROZDANPORK", 24859.10000m, "KG", 1, 124295.5000m, "", "0203.29.00 41", 24859.100m, "KG");
			return testDec;
		}

		public void TestHasChangesIsFalseAfterLoad()
		{
			ZGuid myGuid = SetUpAndSaveDeclarationWithAddInfoSet();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var loadedDeclaration = factory.Load<JobDeclaration>(myGuid);
			AssertNotNull(loadedDeclaration.AddInfo);
			JobComInvoiceLine line1 = loadedDeclaration.InvoiceLines[0];
			AssertNotNull(line1.AddInfo);
			AQISProducerCodeCollection aQISProducerCodes = line1.AQISProducerCodes;
			AssertEquals("FRED", aQISProducerCodes[0].Code);
			AssertEquals("HasChanges", false, loadedDeclaration.HasChanges);
		}

		public void TestImportBankAccountDetailsRefresh()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			ZQuery firstOrgFilter = new ZQuery();
			firstOrgFilter.OrderBy = OrgHeader.Schema.OH_Code;
			var factory1OrgHeader = factory1.LoadTop1<OrgHeader>(firstOrgFilter);

			var factory2OrgHeader = factory2.LoadTop1<OrgHeader>(firstOrgFilter);

			AssertEquals("OriginalBSB", "", factory2OrgHeader.MiscServ.OM_IMEFTBankBSB);

			factory1OrgHeader.MiscServ.OM_IMEFTBankBSB = "24680";

			AssertEquals("OriginalBSB", "", factory2OrgHeader.MiscServ.OM_IMEFTBankBSB);

			factory1.Save();

			AssertEquals("BSB", "24680", factory2OrgHeader.MiscServ.OM_IMEFTBankBSB);
		}

		public void TestLineZA_PRFListIsEmptyForExports()
		{
			OrgHeader supplier = OrgHeader.LoadFromCode(Factory, "ABIGAS");
			OrgHeader importer = OrgHeader.LoadFromCode(Factory, "ABABEU");

			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;

			JobComInvoiceHeader testHeader54785496 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			JobComInvoiceLine testLine1 = testHeader54785496.JobComInvoiceLines.AddNew();
			testLine1.JI_CountryOfOrigin = "AG";
			Assert("NoPreferencesInList", testLine1.AddInfo.Lookups.ZA_PRFList.Count == 0);
		}

		public void TestHaveAllEntriesBeenLodgedEdifice()
		{
			JobDeclaration testDec = GetJobDeclarationWithTwoDummyCusEntryHeaders();
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;

			AssertEquals(false, testDec.HaveAllEntriesBeenLodged);
			testDec.CustomsEntryHeaders[0].CH_Status = EntryHeaderStatus.LodgeMessageCleared.Code;
			AssertEquals(false, testDec.HaveAllEntriesBeenLodged);
			testDec.CustomsEntryHeaders[1].CH_Status = EntryHeaderStatus.LodgeMessageCleared.Code;
			AssertEquals(true, testDec.HaveAllEntriesBeenLodged);
			testDec.CustomsEntryHeaders[1].CH_Status = EntryHeaderStatus.LodgeImpediment.Code;
			AssertEquals(true, testDec.HaveAllEntriesBeenLodged);
			testDec.CustomsEntryHeaders[1].CH_Status = EntryHeaderStatus.ReadyForPayment.Code;
			AssertEquals(true, testDec.HaveAllEntriesBeenLodged);
			testDec.CustomsEntryHeaders[0].CH_Status = EntryHeaderStatus.GoodsReleased.Code;
			testDec.CustomsEntryHeaders[1].CH_Status = EntryHeaderStatus.PayMessageCleared.Code;
			AssertEquals(true, testDec.HaveAllEntriesBeenLodged);
		}

		public void TestHaveAllEntriesBeenLodgedCMR()
		{
			JobDeclaration testDec = GetJobDeclarationWithTwoDummyCusEntryHeaders();
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals(false, testDec.HaveAllEntriesBeenLodged);
			testDec.CustomsEntryHeaders[0].CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals(false, testDec.HaveAllEntriesBeenLodged);
			testDec.CustomsEntryHeaders[1].CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals(true, testDec.HaveAllEntriesBeenLodged);
			testDec.CustomsEntryHeaders.RemoveAndDeleteAll();
			AssertEquals(true, testDec.HaveAllEntriesBeenLodged);
		}

		public void TestSettingVesselSetsShippingLine()
		{
			using (CustomsDataRegistry.Instance.RelatedPartyDefaultingCarrier.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RelatedPartyDefaultingTypeList.Codes.Both))
			{
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				RefVessel vessel = RefVessel.New(Factory);
				vessel.RV_Code = "Vessel";
				OrgHeader org = Factory.New<OrgHeader>();
				vessel.RV_OH = org.PK;
				declaration.JE_VesselName = vessel.RV_Code;
				AssertEquals(vessel.RV_OH, declaration.JE_OH_ShippingLine);
			}
		}

		public void TestDutyCodeProblem()
		{
			OrgHeader supplier = OrgHeader.LoadFromCode(Factory, "ABIGAS");
			OrgHeader importer = OrgHeader.LoadFromCode(Factory, "ABABEU");

			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.GSTCode, "90093519530");
			supplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.SupplierCode, "955411R");
			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, "0070111J");
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			testDec.JE_OH_Importer = importer.PK;
			testDec.JE_OH_Supplier = supplier.PK;
			testDec.JE_AddInfo = "MergeBy_Hidden=TRF*NumberOfEntryPrints_Hidden=1";
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testDec.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.AIR;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_DateOfArrival = new ZDateTime(2004, 1, 30);
			testDec.JE_DateOfFirstArrival = new ZDateTime(2004, 1, 30);
			testDec.JE_DeclarationReference = "B00110373";
			testDec.JE_ExportDate = new ZDateTime(2004, 1, 15);
			testDec.JE_ExportGoodsType = "OT";
			testDec.JE_GB = GlbBranch.CurrentBranch.PK;
			testDec.JE_HouseBill = "ASDASDSDFSDF2223";
			testDec.JE_MessageSubType = "FRM";
			testDec.JE_RL_NKFinalDestination = "AUBNE";
			testDec.JE_RL_NKOrigin = "DEFRA";
			testDec.JE_RL_NKPortOfArrival = "AUBNE";
			testDec.JE_RL_NKPortOfFirstArrival = "AUBNE";
			testDec.JE_RL_NKPortOfLoading = "DEFRA";
			testDec.JE_TotalNoOfPacks = 50;
			testDec.JE_TotalNoOfPacksPackType = "CTN";
			testDec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			testDec.JE_VoyageFlightNo = "QF666";
			testDec.JE_OwnerRef = ".";

			testDec.JobComInvoiceGroupHeaders[0].JZ_GroupInvoice = true;
			testDec.JobComInvoiceGroupHeaders[0].JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testDec.JobComInvoiceGroupHeaders[0].JZ_InvoiceCurrExRate = 1.000000000m;
			testDec.JobComInvoiceGroupHeaders[0].JZ_InvoiceDate = new ZDateTime(2004, 1, 8);

			JobComInvoiceHeader testHeader1 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testHeader1.JZ_AddInfo = "TILV=400.00*PackCountForNature10_Hidden=50*ValuationBasis_Hidden=RT";
			testHeader1.JZ_InvoiceAmount = 1000.0000m;
			testHeader1.JZ_InvoiceCurrExRate = 1.000000000m;
			testHeader1.JZ_InvoiceDate = new ZDateTime(2004, 1, 8);
			testHeader1.JZ_InvoiceNumber = "1";
			testHeader1.JZ_OH_Supplier = supplier.PK;
			testHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			testHeader1.JZ_RX_NKInvoice_Currency = "AUD";
			testHeader1.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 500);
			testHeader1.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 120);
			testHeader1.JZ_Weight = 50.000m;
			testHeader1.JZ_WeightUQ = "KG";

			JobComInvoiceLine testLine1 = testHeader1.JobComInvoiceLines.AddNew();
			testLine1.JI_AddInfo = "ORG=DE*TreatmentCode_Hidden=215";
			testLine1.JI_Description = "Spades and shovels";
			testLine1.JI_InvoiceQuantity = 50.00000m;
			testLine1.JI_InvoiceUQ = "PCE";
			testLine1.JI_LineNo = (short)1;
			testLine1.JI_LinePrice = 380.0000m;
			testLine1.JI_CountryOfOrigin = "DE";
			testLine1.JI_Tariff = "8201.10.00 01";
			testLine1.JI_WeightUQ = "KG";

			bool result = testDec.DoMerge();
			Assert("Merge Worked", result);
		}

		public void TestGetDeclarationReferenceFromShipmentIfShipmentExists()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			Factory.Save();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			Factory.Save();
			AssertEquals("DeclarationReference", shipment.JS_UniqueConsignRef, declaration.JE_DeclarationReference);
		}

		public void TestPlaceHold()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_EntryStatus = CustomsEntryStatus.FailCreate.Code;
			testDec.PlaceHold("Waiting for bill to put in the correct tariffs");
			Assert("ReadOnly", testDec.ReadOnly);
			AssertEquals("EntryStatus", CustomsEntryStatus.HoldAwaiting.Code, testDec.JE_EntryStatus);
			AssertEquals("HoldLogsCount", 1, testDec.HoldAwaitingLogs.Count);
			AssertEquals("HoldLogsDescription", "Reason: 'Waiting for bill to put in the correct tariffs'", testDec.HoldAwaitingLogs[0].SL_Reference);
		}

		public void TestReadOnlyForIsHolding()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B00001018";
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B00001018/1";
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			Assert(!dec.IsHolding);
			Assert(!dec.ReadOnly);

			dec.PlaceHold("TEST WITHOUT SAVING");
			Assert(dec.IsHolding);
			Assert(dec.ReadOnly);
			AssertEquals(CustomsEntryStatus.HoldAwaiting.Code, dec.JE_MessageStatus);

			dec.RemoveHold();
			Assert(!dec.IsHolding);
			Assert(!dec.ReadOnly);
			AssertEquals(CustomsEntryStatus.ClearFormalLodge.Code, dec.JE_MessageStatus);

			dec.PlaceHold("TEST WITH SAVING");
			Assert(dec.IsHolding);
			Assert(dec.ReadOnly);
			AssertEquals(CustomsEntryStatus.HoldAwaiting.Code, dec.JE_MessageStatus);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var dec2 = JobDeclaration.LoadFirstMatchingInCurrentCompanyIncludingInActive(factory2, "B00001018");
			AssertNotNull("Declaration exists", dec2);
			var entryHeader2 = CusEntryHeader.LoadForBGMReference(factory2, dec.CustomsEntryHeaders[0].CH_BGMReference);
			AssertNotNull("Entry header exists", entryHeader2);
			dec2.CustomsEntryHeaders.Add(entryHeader2);
			AssertEquals(1, dec2.CustomsEntryHeaders.Count);
			Assert(dec2.IsHolding);
			Assert(dec2.ReadOnly);
			AssertEquals(CustomsEntryStatus.HoldAwaiting.Code, dec2.JE_MessageStatus);

			dec2.RemoveHold();
			Assert(!dec2.IsHolding);
			Assert(!dec2.ReadOnly);
			AssertEquals(CustomsEntryStatus.ClearFormalLodge.Code, dec2.JE_MessageStatus);
		}

		public void TestReadOnlyForIsDeclarationWorkFinished()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B00001019";
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B00001019/1";
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			Assert(!dec.IsDeclarationWorkFinished);
			Assert(!dec.ReadOnly);

			dec.PlaceDeclarationWorkComplete("TEST WITHOUT SAVING");
			Assert(dec.IsDeclarationWorkFinished);
			Assert(dec.ReadOnly);
			AssertEquals(CustomsEntryStatus.DeclarationWorkComplete.Code, dec.JE_MessageStatus);

			dec.RemoveDeclarationWorkComplete();
			Assert(!dec.IsDeclarationWorkFinished);
			Assert(!dec.ReadOnly);
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;

			dec.PlaceDeclarationWorkComplete("TEST WITH SAVING");
			Assert(dec.IsDeclarationWorkFinished);
			Assert(dec.ReadOnly);
			AssertEquals(CustomsEntryStatus.DeclarationWorkComplete.Code, dec.JE_MessageStatus);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var dec2 = JobDeclaration.LoadFirstMatchingInCurrentCompanyIncludingInActive(factory2, "B00001019");
			AssertNotNull("Declaration exists", dec2);
			var entryHeader2 = CusEntryHeader.LoadForBGMReference(factory2, dec.CustomsEntryHeaders[0].CH_BGMReference);
			AssertNotNull("Entry header exists", entryHeader2);
			dec2.CustomsEntryHeaders.Add(entryHeader2);
			AssertEquals(1, dec2.CustomsEntryHeaders.Count);
			Assert(dec2.IsDeclarationWorkFinished);
			Assert(dec2.ReadOnly);
			AssertEquals(CustomsEntryStatus.DeclarationWorkComplete.Code, dec2.JE_MessageStatus);

			dec2.RemoveDeclarationWorkComplete();
			Assert(!dec2.IsDeclarationWorkFinished);
			Assert(!dec2.ReadOnly);
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;

			dec2.Logs.AddNew(Events.DeclarationWorkComplete, "Event with estimate time", ZDateTimeOffset.Now, true);
			Assert(!dec2.IsDeclarationWorkFinished);
			Assert(!dec2.ReadOnly);
		}

		public void TestDoNotRefreshReadOnlyWhenTheCurrentEnvIsWebOrWebService()
		{
			var originalWebValue = Globals.IsWeb;
			var originalWebServiceValue = Globals.IsWebService;

			void AssertRefreshReadOnly(bool isWeb, bool isWebService, bool expectedReadOnly)
			{
				using (new DisposableAction(() => { Globals.IsWeb = isWeb; }, () => { Globals.IsWeb = originalWebValue; }))
				using (new DisposableAction(() => { Globals.IsWebService = isWebService; }, () => { Globals.IsWebService = originalWebServiceValue; }))
				{
					var newFactory = NewFactory();
					newFactory.RefreshEnabled = false;

					var dec = newFactory.New<JobDeclaration>();
					dec.JE_MessageType = JobMessageTypeList.Codes.Import;

					var entryHeader = dec.CustomsEntryHeaders.AddNew();
					entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;

					Assert("Default to false.", !dec.ReadOnly);

					dec.PlaceDeclarationWorkComplete("TEST WITH SAVING IN WEB ENV");
					Assert("Precondition.", dec.IsDeclarationWorkFinished);

					Assert("Should be true as the declaration work is finished.", dec.ReadOnly);

					dec.ReadOnly = false;
					Assert("Precondition.", dec.IsDeclarationWorkFinished);

					newFactory.Save();

					AssertEquals("Should only refresh readonly when the current program is not running in Web or WebService mode.", expectedReadOnly, dec.ReadOnly);
				}
			}

			AssertRefreshReadOnly(true, true, false);
			AssertRefreshReadOnly(true, false, false);
			AssertRefreshReadOnly(false, true, false);
			AssertRefreshReadOnly(false, false, true);
		}

		public void TestPlaceHoldWithLongReason()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_EntryStatus = CustomsEntryStatus.FailCreate.Code;
			testDec.PlaceHold(@"This is a really long description for why the declaration should be in stat status hold awaiting that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference
This is a really long description for why the declaration should be in stat status hold awaiting that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference
This is a really long description for why the declaration should be in stat status hold awaiting that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference
This is a really long description for why the declaration should be in stat status hold awaiting that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference
This is a really long description for why the declaration should be in stat status hold awaiting that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference
This is a really long description for why the declaration should be in stat status hold awaiting that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference
This is a really long description for why the declaration should be in stat status hold awaiting that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference
This is a really long description for why the declaration should be in stat status hold awaiting that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference
This is a really long description for why the declaration should be in stat status hold awaiting that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference
This is a really long description for why the declaration should be in stat status hold awaiting that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference");
			AssertEquals("HoldLogsDescription", @"Reason: 'This is a really long description for why the declaration should be in stat status hold awaiting that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference
This is a really long description for why the declaration should be in stat status hold awaiting that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference
This is a really long description for why the declaration should be in stat status hold awaiting that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference
This is a really long description for why the declaration should be in stat status hold awaiting that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference
This is a really long description for why the declaration should be in stat status hold awaiting that is going to be more characters than the amount that can be fit into the StmALog fiel'", testDec.HoldAwaitingLogs[0].SL_Reference);
		}

		public void TestRemoveHold()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.PlaceHold("Waiting for bill to put in the correct tariffs");
			Assert("ReadOnly", testDec.ReadOnly);
			testDec.RemoveHold();
			Assert("NotReadOnly", !testDec.ReadOnly);
			AssertEquals("EntryStatus", ZString.Empty, testDec.JE_EntryStatus);//TODO: replace the following line when 'NOT' is the code for not sent
																			   //AssertEquals("EntryStatus", CustomsEntryStatus.NotSent.Code, TestDec.JE_EntryStatus);
		}

		public void TestPlaceDeclarationWorkComplete()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_EntryStatus = CustomsEntryStatus.FailCreate.Code;
			testDec.PlaceDeclarationWorkComplete("Had to go manual because the SMTP server was down");
			Assert("ReadOnly", testDec.ReadOnly);
			AssertEquals("EntryStatus", CustomsEntryStatus.DeclarationWorkComplete.Code, testDec.JE_EntryStatus);
			AssertEquals("DeclarationWorkCompleteLogsCount", 1, testDec.DeclarationWorkCompleteLogs.Count);
			AssertEquals("DeclarationWorkCompleteLogsDescription", "Reason: 'Had to go manual because the SMTP server was down'", testDec.DeclarationWorkCompleteLogs[0].SL_Reference);
		}

		public void TestPlaceDeclarationWorkCompleteWithLongReason()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_EntryStatus = CustomsEntryStatus.FailCreate.Code;
			testDec.PlaceHold(@"This is a really long description for why the declaration should be in stat status declaration work complete that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference
This is a really long description for why the declaration should be in stat status declaration work complete that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference
This is a really long description for why the declaration should be in stat status declaration work complete that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference
This is a really long description for why the declaration should be in stat status declaration work complete that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference
This is a really long description for why the declaration should be in stat status declaration work complete that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference
This is a really long description for why the declaration should be in stat status declaration work complete that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference
This is a really long description for why the declaration should be in stat status declaration work complete that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference
This is a really long description for why the declaration should be in stat status declaration work complete that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference
This is a really long description for why the declaration should be in stat status declaration work complete that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference
This is a really long description for why the declaration should be in stat status declaration work complete that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference");
			AssertEquals("HoldLogsDescription", @"Reason: 'This is a really long description for why the declaration should be in stat status declaration work complete that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference
This is a really long description for why the declaration should be in stat status declaration work complete that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference
This is a really long description for why the declaration should be in stat status declaration work complete that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference
This is a really long description for why the declaration should be in stat status declaration work complete that is going to be more characters than the amount that can be fit into the StmALog field SL_Reference
This is a really long description for why the declaration should be in stat status declaration work complete that is going to be more char'", testDec.HoldAwaitingLogs[0].SL_Reference);
		}

		public void TestRemoveDeclarationWorkComplete()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.PlaceDeclarationWorkComplete("Waiting for bill to put in the correct tariffs");
			Assert("ReadOnly", testDec.ReadOnly);
			testDec.RemoveDeclarationWorkComplete();
			Assert("NotReadOnly", !testDec.ReadOnly);
			AssertEquals("EntryStatus", ZString.Empty, testDec.JE_EntryStatus);//TODO: replace the following line when 'NOT' is the code for not sent
																			   //AssertEquals("EntryStatus", CustomsEntryStatus.NotSent.Code, TestDec.JE_EntryStatus);
			AssertEquals("DeclarationWorkCompleteLogsCount", 0, testDec.DeclarationWorkCompleteLogs.Count);
		}

		public void TestResettingDeclarationDeletedEntryNumbers()
		{
			JobDeclaration declaration = CreateSendableDeclaration();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AUCusEntryNumber entryNumber = Factory.New<AUCusEntryNumber>();
			entryNumber.CE_ParentID = declaration.PK;
			entryNumber.CE_ParentTable = declaration.TableName;
			entryNumber.CE_EntryType = CusEntryNumberTypes.Australia.ECN;
			entryNumber.CE_RN_NKCountryCode = "AU";
			entryNumber.CE_EntryNum = "1S828371912";
			AssertNotNull("Failed to add Entry Number", declaration.ExportEntryNumber);
			Assert("Declaration Status", declaration.JE_EntryStatus != "");
			EDIMessage message1 = declaration.Messages.AddNew();
			EDIMessage message2 = declaration.Messages.AddNew();

			declaration.ResetDeclaration();
			AssertEquals("Declaration Status", "", declaration.JE_EntryStatus);
			AssertNull("Declaration.AUCusEntryNumber should be null now", declaration.ExportEntryNumber);
			AssertEquals("Messages are discarded", EDIMessage.Status.Discarded, message1.EM_Status);
			AssertEquals("Messages are discarded", EDIMessage.Status.Discarded, message2.EM_Status);
		}

		public void TestDefaultMessageSubType()
		{
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("MessageSubType", JobDeclaration.MessageSubType.NonConfirming, testJobDeclaration.JE_MessageSubType);
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("MessageSubType", JobDeclaration.MessageSubType.FormalEntry, testJobDeclaration.JE_MessageSubType);
		}

		public void TestCRN()
		{
			AssertEquals("CRN", ZString.Empty, testJobDeclaration.CRN);
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();
			testJobDeclaration.JE_JS = shipment.PK;
			FreightConsolWrapper consolWrapper = new FreightConsolWrapper(consol);
			AUCusEntryNumber entryNumber = consolWrapper.CreateCusEntryNumber();
			entryNumber.CE_EntryNum = "2468013579";
			AssertEquals("CRN", "2468013579", testJobDeclaration.CRN);
		}

		public void TestJobDeclarationCanOnlyBeCreatedInCountryCodeAU()
		{
			bool exceptionCaught = false;
			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.HongKong;
				JobDeclaration testInvalidExceptionThrowByDec = Factory.New<JobDeclaration>();
			}
			catch (ApplicationException e)
			{
				if (e.InnerException.InnerException.Message.StartsWith("Attempted to create Australian Job Declaration in country code "))
				{
					exceptionCaught = true;
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Australia;
			}
			Assert("Incorrect Country Exception Not Thrown", exceptionCaught);
		}

		[ExpectNoExceptions()]
		public void TestPreSaveValidationWhenCusEntryLineHasNoInvoiceLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			JobComInvoiceHeader header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();
			line1.JI_CL = entryLine1.PK;
			line2.JI_CL = entryLine2.PK;

			Customs.Business.Testing.MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();

			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			JobDeclaration loadedDeclaration = secondFactory.Load<JobDeclaration>(declaration.PK);
			loadedDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.RemoveAndDeleteAll();
			loadedDeclaration.RunPreSaveValidation();
			loadedDeclaration.CustomsEntryHeaders[0].RunPreSaveValidation();
		}

		public void TestDateOfValuation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(5);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("ValuationDate", declaration.JE_ExportDate, declaration.DateOfValuation);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("ValuationDate", ZDateTime.Today, declaration.DateOfValuation);
		}

		public void TestDepotOrCTOIDAndWarehouseID()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			OrgAddress address1 = testDec.Factory.New<OrgAddress>();
			OrgHeader header1 = testDec.Factory.New<OrgHeader>();
			address1.OA_OH = header1.PK;
			header1.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "1111Z");
			testDec.ContainerTerminalOperatorDocAddress.E2_OA_Address = address1.PK;
			OrgAddress address2 = testDec.Factory.New<OrgAddress>();
			OrgHeader header2 = testDec.Factory.New<OrgHeader>();
			address2.OA_OH = header2.PK;
			testDec.WarehouseDocAddress.E2_OA_Address = address2.PK;
			header2.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "2222Z");
			AssertEquals("DepotOrCTOID is CTO", "1111Z", testDec.DepotOrCTOID);
			AssertEquals("DepotID", "", testDec.DepotID);
			AssertEquals("WarehouseID", "2222Z", testDec.WarehouseID);
			OrgAddress address3 = testDec.Factory.New<OrgAddress>();
			OrgHeader header3 = testDec.Factory.New<OrgHeader>();
			address3.OA_OH = header3.PK;
			header3.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "3333Z");
			testDec.DepotDocAddress.E2_OA_Address = address3.PK;
			AssertEquals("DepotOrCTOID is depot", "3333Z", testDec.DepotOrCTOID);
			AssertEquals("CTOID", "1111Z", testDec.CTOID);
			AssertEquals("DepotID", "3333Z", testDec.DepotID);
		}

		public void TestCleanVoyageNumber()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_VoyageFlightNo = "123";
			AssertEquals("ClearVoyageNumber", "123", testDec.CleanVoyageNumber);
			testDec.JE_VoyageFlightNo = "NB000123";
			AssertEquals("ClearVoyageNumber", "123", testDec.CleanVoyageNumber);
			testDec.JE_VoyageFlightNo = "NB000";
			AssertEquals("ClearVoyageNumber", "", testDec.CleanVoyageNumber);
		}

		[ExpectNoExceptions]
		public void TestDeleteJobDeclarationWithDataRefreshBusRunning()
		{
			JobDeclaration dec = Factory.New<JobDeclaration>();
			dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			Factory.Save();

			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			JobDeclaration secondFactoryDec = secondFactory.Load<JobDeclaration>(dec.PK);

			secondFactoryDec.Delete();
			secondFactory.Save();
			AssertEquals("IsDeleted", true, dec.IsDeleted);
		}

		public void TestSupplierWrapper()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertNull("SupplierWrapper", declaration.SupplierWrapper);
			declaration.JE_OH_Supplier = Factory.New(typeof(OrgHeader)).PK;
			AssertNotNull("SupplierWrapper", declaration.SupplierWrapper);
		}

		public void TestEntryType()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("CAN", declaration.EntryType);
		}

		public void TestDeclarationDetailsContainsCANForEXD()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.DeclarationNumber = "12345";
			Assert(declaration.Details.Contains("CAN: 12345"));
		}

		public void TestDeclarationDetailsContainsDrawbackID()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			declaration.DeclarationNumber = "12345";
			Assert(declaration.Details.Contains("Drawback Claim Identifier: 12345"));
		}

		public void TestSettingDeclarationByExternalBroker()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_DeclarationReference = "B99999999";
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			AssertEquals("Pre-condition", OrgConstants.MergeInvoiceLines.Classification, testDec.JE_MergeBy);
			AssertEquals("Pre-condition", false, testDec.JE_MergeByInfo.ReadOnly);
			AssertEquals("Pre-condition", true, entryHeader.EntryNumberInfo.ReadOnly);
			AssertEquals("Pre-condition", true, testDec.ManualClearanceDateInfo.ReadOnly);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ImportDeclarationByExternalBroker;
			AssertEquals("DeclarationByExternalBroker", false, entryHeader.EntryNumberInfo.ReadOnly);
			AssertEquals("DeclarationByExternalBroker", true, testDec.DeclarationNumberInfo.ReadOnly);
			AssertEquals("DeclarationByExternalBroker", false, testDec.ManualClearanceDateInfo.ReadOnly);
		}

		public void TestClearingDeclarationByExternalBroker()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_DeclarationReference = "B99999999";
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ImportDeclarationByExternalBroker;
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			entryHeader.EntryNumber = "AAA";
			testDec.ManualClearanceDate = ZDateTime.Today;
			AssertEquals("Pre-condition", "AAA", entryHeader.EntryNumber);
			AssertEquals("Pre-condition", false, entryHeader.EntryNumberInfo.ReadOnly);
			Assert("Pre-condition", !testDec.ManualClearanceDate.IsEmpty);
			AssertEquals("DeclarationByExternalBroker", true, testDec.DeclarationNumberInfo.ReadOnly);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("DeclarationByExternalBroker", "", entryHeader.EntryNumber);
			AssertEquals("DeclarationByExternalBroker", true, entryHeader.EntryNumberInfo.ReadOnly);
			AssertEquals("DeclarationByExternalBroker", true, testDec.DeclarationNumberInfo.ReadOnly);
			Assert("DeclarationByExternalBroker", testDec.ManualClearanceDate.IsEmpty);
		}

		public void TestDrawbackSetsMergeByToNoAndReadOnly()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			AssertEquals("Pre-condition", false, testDec.JE_MergeByInfo.ReadOnly);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			AssertEquals("Drawback set merge by to No", OrgConstants.MergeInvoiceLines.NotMerge, testDec.JE_MergeBy);
			AssertEquals("Pre-condition", true, testDec.JE_MergeByInfo.ReadOnly);
		}

		public void TestSettingExportDeclarationByExternalBroker()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_DeclarationReference = "B99999999";
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("Pre-condition", true, testDec.DeclarationNumberInfo.ReadOnly);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExportDeclarationByExternalBroker;
			AssertEquals("DeclarationByExternalBroker", false, testDec.DeclarationNumberInfo.ReadOnly);
		}

		public void TestClearingExportDeclarationByExternalBroker()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExportDeclarationByExternalBroker;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_DeclarationReference = "B99999999";
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			testDec.DeclarationNumber = "AAA";
			AssertEquals("Pre-condition", "AAA", testDec.DeclarationNumber);
			AssertEquals("Pre-condition", false, testDec.DeclarationNumberInfo.ReadOnly);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("DeclarationByExternalBroker", "", testDec.DeclarationNumber);
			AssertEquals("DeclarationByExternalBroker", true, testDec.DeclarationNumberInfo.ReadOnly);
		}

		public void TestDrawbackQuestions()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_DeclarationReference = "B99999999";
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			CMRDeclarationQuestionsCollection drawbackQuestions = testDec.DrawbackQuestions;
			testDec.GenerateDrawbackQuestionsIfNecessary();
			AssertEquals("DrawbackQuestions count", 6, drawbackQuestions.Count);
			ZString questionNumbers = "";
			foreach (CMRCusEntryCPDec entryDec in drawbackQuestions)
			{
				questionNumbers += entryDec.ON_CPDecNum.ToString() + "*";
				if (entryDec.ON_CPDecNum == 999)
				{
					AssertEquals("Drawback Payee Declaration Question", CMRCusEntryCPDec.DrawbackPayeeDec, entryDec.Question);
				}
			}
			AssertEquals("DrawbackQuestions contains", true, questionNumbers.Contains("999*"));
			AssertEquals("DrawbackQuestions contains", true, questionNumbers.Contains("283*"));
			AssertEquals("DrawbackQuestions contains", true, questionNumbers.Contains("284*"));
			AssertEquals("DrawbackQuestions contains", true, questionNumbers.Contains("285*"));
			AssertEquals("DrawbackQuestions contains", true, questionNumbers.Contains("286*"));
			AssertEquals("DrawbackQuestions contains", true, questionNumbers.Contains("287*"));
		}

		public void TestGetEntryNumberFromDeclarationReference()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_DeclarationReference = "B99999999";
			testDec.DeclarationNumber = "EDNNO";
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("GetEntryNumberFromDeclarationReference", "EDNNO", testDec.GetEntryNumberFromDeclarationReference("B99999999"));
		}

		public void TestGetQuarantineCOLSHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("Related QuarantineCOLSHeader is null", declaration.QuarantineCOLSHeader == null);

			var colsHeader = declaration.CreateCOLSHeaderIfRequired();
			Assert("QuarantineCOLSHeader is not created", declaration.QuarantineCOLSHeader == null);

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			colsHeader = declaration.CreateCOLSHeaderIfRequired();
			Assert("QuarantineCOLSHeader is not created", declaration.QuarantineCOLSHeader == null);

			var impNumber = CusEntryNumber.New(entryHeader, CusEntryNumberTypes.Australia.IMP, Core.Constants.CountryCodes.Australia);
			impNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			impNumber.CE_EntryNum = "IMP1234";

			colsHeader = declaration.CreateCOLSHeaderIfRequired();
			Assert("QuarantineCOLSHeader is created", declaration.QuarantineCOLSHeader != null);
			AssertEquals("QuarantineCOLSHeader Type", typeof(QuarantineColsHeader), declaration.QuarantineCOLSHeader.GetType());
			AssertEquals("QuarantineCOLSHeader should be registered", true, declaration.IsRegisteredEditableChildObject(colsHeader));
			AssertEquals("New QuarantineCOLSHeader should have HasChanges=true", true, declaration.QuarantineCOLSHeader.HasChanges);

			colsHeader = declaration.CreateCOLSHeaderIfRequired();
			AssertEquals("QuarantineCOLSHeader should not created new object", colsHeader.PK, declaration.QuarantineCOLSHeader.PK);
		}

		public void TestAttachExporDeclaration()
		{
			var class1 = Factory.New<Classification>();
			class1.CC_LookupCode = "CODE";
			class1.CC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			class1.CC_ClassificationType = Classification.ClassificationType.IMP;
			class1.CC_Description = "CLASSDESC";
			class1.CC_TariffNum = "2001.10.00 90";
			var part4 = Factory.New<AUOrgSupplierPart>();
			part4.AddNewImportPivotWithClassification(class1.PK);
			part4.OP_PartNum = "PART4";
			part4.OP_StockKeepingUnit = "NO";
			var importer1 = Factory.New<OrgHeader>();
			importer1.OH_Code = "ORG1";
			var relation1 = part4.RelatedOrganisations.AddNew();
			relation1.OU_OH = importer1.PK;
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var class2 = Factory.New<Classification>();
			class2.CC_LookupCode = "CODE2";
			class2.CC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			class2.CC_ClassificationType = Classification.ClassificationType.IMP;
			class2.CC_Description = "CLASSDESC2";
			class2.CC_TariffNum = "6506.10.00 19";
			var part5 = Factory.New<AUOrgSupplierPart>();
			part5.AddNewImportPivotWithClassification(class2.PK);
			part5.OP_PartNum = "PART5";
			part5.OP_StockKeepingUnit = "NO";
			var relation2 = part5.RelatedOrganisations.AddNew();
			relation2.OU_OH = importer1.PK;
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			importDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			importDeclaration.JE_DeclarationReference = "B12345678";
			importDeclaration.JE_OH_Importer = importer1.PK;
			importDeclaration.ManualClearanceDate = new ZDateTime(2007, 6, 30);
			var importInvoice = importDeclaration.Invoices.AddNew();
			var importInvoiceLine = importInvoice.JobComInvoiceLines.AddNew();
			importInvoiceLine.JI_PartNo = "PART4";
			importInvoiceLine.JI_InvoiceQuantity = 1000m;
			importInvoiceLine.JI_InvoiceUQ = "UNT";
			importInvoiceLine.JI_CustomsQuantity = 500m;
			importInvoiceLine.JI_CustomsUnitQty = "KG";
			var importInvoiceLine2 = importInvoice.JobComInvoiceLines.AddNew();
			importInvoiceLine2.JI_PartNo = "PART3";
			importInvoiceLine2.JI_InvoiceQuantity = 100m;
			importInvoiceLine2.JI_InvoiceUQ = "UNT";
			importInvoiceLine2.JI_CustomsQuantity = 1000m;
			importInvoiceLine2.JI_CustomsUnitQty = "KG";
			var importInvoiceLine3 = importInvoice.JobComInvoiceLines.AddNew();
			importInvoiceLine3.JI_PartNo = "PART5";
			importInvoiceLine3.JI_InvoiceQuantity = 12m;
			importInvoiceLine3.JI_InvoiceUQ = "T";
			importInvoiceLine3.JI_CustomsQuantity = 50000m;
			importInvoiceLine3.JI_CustomsUnitQty = "NO";
			var entryHeader = importDeclaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;
			entryLine.CL_LineNumber = 1;
			importInvoiceLine.JI_CL = entryLine.PK;
			importInvoiceLine2.JI_CL = entryLine.PK;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;
			entryLine2.CL_LineNumber = 2;
			importInvoiceLine3.JI_CL = entryLine2.PK;
			entryHeader.EntryNumber = "AAABBBCCC";
			Factory.Save();

			JobDeclaration exportDec = Factory.New<JobDeclaration>();
			exportDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			exportDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			exportDec.JE_DeclarationReference = "B00000001";
			exportDec.DeclarationNumber = "EDNNO";
			JobComInvoiceHeader exportInvoice = exportDec.Invoices.AddNew();
			exportInvoice.JZ_InvoiceNumber = "EXINVNO";
			exportInvoice.JZ_InvoiceAmount = 3000m;
			exportInvoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			exportInvoice.JZ_IncoTerm = "CIF";
			exportInvoice.JZ_InvoiceDate = new ZDateTime(2007, 9, 3);
			JobComInvoiceLine exportInvoiceLine1 = exportInvoice.JobComInvoiceLines.AddNew();
			exportInvoiceLine1.JI_PartNo = "PART1";
			exportInvoiceLine1.JI_Description = "DESCRIPTION1";
			exportInvoiceLine1.JI_LinePrice = 1000m;
			exportInvoiceLine1.JI_InvoiceQuantity = 1m;
			JobComInvoiceLine exportInvoiceLine2 = exportInvoice.JobComInvoiceLines.AddNew();
			exportInvoiceLine2.JI_PartNo = "PART2";
			exportInvoiceLine2.JI_Description = "DESCRIPTION2";
			exportInvoiceLine2.JI_LinePrice = 1000m;
			exportInvoiceLine2.JI_InvoiceQuantity = 2m;
			exportInvoiceLine2.JI_Drawback = false;
			JobComInvoiceLine exportInvoiceLine3 = exportInvoice.JobComInvoiceLines.AddNew();
			exportInvoiceLine3.JI_PartNo = "PART3";
			exportInvoiceLine3.JI_Description = "DESCRIPTION3";
			exportInvoiceLine3.JI_LinePrice = 1000m;
			exportInvoiceLine3.JI_InvoiceQuantity = 3m;

			// set customs qty directly from export line customs qty
			var exportInvoiceLine = exportInvoice.JobComInvoiceLines.AddNew();
			exportInvoiceLine.JI_PartNo = "PART4";
			exportInvoiceLine.JI_Description = "DESCRIPTION4";
			exportInvoiceLine.JI_LinePrice = 1000m;
			exportInvoiceLine.JI_InvoiceQuantity = 3m;
			exportInvoiceLine.JI_InvoiceUQ = "NO";
			exportInvoiceLine.JI_CustomsUnitQty = "KG";
			exportInvoiceLine.JI_CustomsQuantity = 250m;

			// set customs qty from export line customs qty with diverse weight units
			exportInvoiceLine = exportInvoice.JobComInvoiceLines.AddNew();
			exportInvoiceLine.JI_PartNo = "PART4";
			exportInvoiceLine.JI_Description = "DESCRIPTION5";
			exportInvoiceLine.JI_LinePrice = 1000m;
			exportInvoiceLine.JI_InvoiceQuantity = 3m;
			exportInvoiceLine.JI_InvoiceUQ = "NO";
			exportInvoiceLine.JI_CustomsUnitQty = "LB";
			exportInvoiceLine.JI_CustomsQuantity = 250m;

			// set customs qty from matching invoice line invoice quantity
			exportInvoiceLine = exportInvoice.JobComInvoiceLines.AddNew();
			exportInvoiceLine.JI_PartNo = "PART4";
			exportInvoiceLine.JI_Description = "DESCRIPTION6";
			exportInvoiceLine.JI_LinePrice = 1000m;
			exportInvoiceLine.JI_InvoiceQuantity = 300m;
			exportInvoiceLine.JI_InvoiceUQ = "KG";
			exportInvoiceLine.JI_CustomsUnitQty = "";
			exportInvoiceLine.JI_CustomsQuantity = 0m;

			// set customs qty from matching invoice line invoice quantity with diverse weight units
			exportInvoiceLine = exportInvoice.JobComInvoiceLines.AddNew();
			exportInvoiceLine.JI_PartNo = "PART4";
			exportInvoiceLine.JI_Description = "DESCRIPTION7";
			exportInvoiceLine.JI_LinePrice = 1000m;
			exportInvoiceLine.JI_InvoiceQuantity = 300m;
			exportInvoiceLine.JI_InvoiceUQ = "LB";
			exportInvoiceLine.JI_CustomsUnitQty = "";
			exportInvoiceLine.JI_CustomsQuantity = 0m;

			// set customs qty from linked import declaration line
			exportInvoiceLine = exportInvoice.JobComInvoiceLines.AddNew();
			exportInvoiceLine.JI_PartNo = "PART4";
			exportInvoiceLine.JI_Description = "DESCRIPTION8";
			exportInvoiceLine.JI_LinePrice = 1000m;
			exportInvoiceLine.JI_InvoiceQuantity = 300m;
			exportInvoiceLine.JI_InvoiceUQ = "UNT";
			exportInvoiceLine.JI_CustomsUnitQty = "";
			exportInvoiceLine.JI_CustomsQuantity = 0m;

			// set customs qty from linked import declaration line with diverse weight units
			exportInvoiceLine = exportInvoice.JobComInvoiceLines.AddNew();
			exportInvoiceLine.JI_PartNo = "PART5";
			exportInvoiceLine.JI_Description = "DESCRIPTION9";
			exportInvoiceLine.JI_LinePrice = 1000m;
			exportInvoiceLine.JI_InvoiceQuantity = 3000m;
			exportInvoiceLine.JI_InvoiceUQ = "LB";
			exportInvoiceLine.JI_CustomsUnitQty = "";
			exportInvoiceLine.JI_CustomsQuantity = 0m;

			// set customs qty from export line net weight
			exportInvoiceLine = exportInvoice.JobComInvoiceLines.AddNew();
			exportInvoiceLine.JI_PartNo = "PART4";
			exportInvoiceLine.JI_Description = "DESCRIPTION10";
			exportInvoiceLine.JI_LinePrice = 1000m;
			exportInvoiceLine.JI_NetWeight = 200m;
			exportInvoiceLine.JI_NetWeightUQ = "KG";
			exportInvoiceLine.JI_InvoiceQuantity = 300m;
			exportInvoiceLine.JI_InvoiceUQ = "NO";
			exportInvoiceLine.JI_CustomsUnitQty = "";
			exportInvoiceLine.JI_CustomsQuantity = 0m;

			// set customs qty from export line net weight with diverse weigt units
			exportInvoiceLine = exportInvoice.JobComInvoiceLines.AddNew();
			exportInvoiceLine.JI_PartNo = "PART4";
			exportInvoiceLine.JI_Description = "DESCRIPTION11";
			exportInvoiceLine.JI_LinePrice = 1000m;
			exportInvoiceLine.JI_NetWeight = 200m;
			exportInvoiceLine.JI_NetWeightUQ = "LB";
			exportInvoiceLine.JI_InvoiceQuantity = 300m;
			exportInvoiceLine.JI_InvoiceUQ = "NO";
			exportInvoiceLine.JI_CustomsUnitQty = "";
			exportInvoiceLine.JI_CustomsQuantity = 0m;

			JobDeclaration drawbackDec = Factory.New<JobDeclaration>();
			drawbackDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			drawbackDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			drawbackDec.JE_DeclarationReference = "B00000002";
			drawbackDec.JE_OH_Importer = importer1.PK;
			drawbackDec.AddInfo.ZA_DAM_Hidden = JobDeclaration.DrawbackAssessmentMethods.RepresentativeShipment;

			drawbackDec.AttachExportDeclaration(exportDec.PK);

			AssertEquals("Should be 1 invoice", 1, drawbackDec.Invoices.Count);
			JobComInvoiceHeader drawbackDecInvoice = drawbackDec.Invoices[0];
			AssertEquals("JZ_InvoiceNumber", "EXINVNO", drawbackDecInvoice.JZ_InvoiceNumber);
			AssertEquals("JZ_InvoiceAmount", 10000m, drawbackDecInvoice.JZ_InvoiceAmount);
			AssertEquals("Invoice_Currency", "AUD", drawbackDecInvoice.Invoice_Currency.RX_Code);
			AssertEquals("JZ_IncoTerm", "CIF", drawbackDecInvoice.JZ_IncoTerm);
			AssertEquals("JZ_InvoiceDate", new ZDateTime(2007, 9, 3), drawbackDecInvoice.JZ_InvoiceDate);
			AssertEquals("EDN", "EDNNO", drawbackDecInvoice.AddInfo.ZA_EDN_Hidden);
			AssertEquals("Drawback Method", JobDeclaration.DrawbackAssessmentMethods.RepresentativeShipment, drawbackDecInvoice.AddInfo.ZA_DAM_Hidden);
			AssertEquals("Should be 6 lines", 10, drawbackDecInvoice.JobComInvoiceLines.Count);
			JobComInvoiceLine drawbackInvoiceLine1 = drawbackDecInvoice.JobComInvoiceLines[0];
			AssertEquals("JI_PartNo", "PART1", drawbackInvoiceLine1.JI_PartNo);
			AssertEquals("JI_Description", "DESCRIPTION1", drawbackInvoiceLine1.JI_Description);
			AssertEquals("JI_LinePrice", 0m, drawbackInvoiceLine1.JI_LinePrice);
			AssertEquals("JI_InvoiceQuantity", 1m, drawbackInvoiceLine1.JI_InvoiceQuantity);
			AssertEquals("EDN", "EDNNO", drawbackInvoiceLine1.AddInfo.ZA_EDN_Hidden);
			JobComInvoiceLine drawbackInvoiceLine2 = drawbackDecInvoice.JobComInvoiceLines[1];
			AssertEquals("JI_PartNo", "PART3", drawbackInvoiceLine2.JI_PartNo);
			AssertEquals("JI_Description", "DESCRIPTION3", drawbackInvoiceLine2.JI_Description);
			AssertEquals("JI_LinePrice", 0m, drawbackInvoiceLine2.JI_LinePrice);
			AssertEquals("JI_InvoiceQuantity", 3m, drawbackInvoiceLine2.JI_InvoiceQuantity);
			AssertEquals("EDN", "EDNNO", drawbackInvoiceLine2.AddInfo.ZA_EDN_Hidden);

			// set customs qty directly from export line customs qty
			var drawbackInvoiceLine = drawbackDecInvoice.JobComInvoiceLines[2];
			AssertEquals("JI_PartNo", "PART4", drawbackInvoiceLine.JI_PartNo);
			AssertEquals("JI_Description", "DESCRIPTION4", drawbackInvoiceLine.JI_Description);
			AssertEquals("JI_LinePrice", 0m, drawbackInvoiceLine.JI_LinePrice);
			AssertEquals("JI_InvoiceQuantity", 3m, drawbackInvoiceLine.JI_InvoiceQuantity);
			AssertEquals("EDN", "EDNNO", drawbackInvoiceLine.AddInfo.ZA_EDN_Hidden);
			AssertEquals("JI_CustomsQuantity", 250m, drawbackInvoiceLine.JI_CustomsQuantity);
			AssertEquals("JI_CustomsUnitQty", "KG", drawbackInvoiceLine.JI_CustomsUnitQty);

			// set customs qty from export line customs qty with diverse weight units
			drawbackInvoiceLine = drawbackDecInvoice.JobComInvoiceLines[3];
			AssertEquals("JI_PartNo", "PART4", drawbackInvoiceLine.JI_PartNo);
			AssertEquals("JI_Description", "DESCRIPTION5", drawbackInvoiceLine.JI_Description);
			AssertEquals("JI_InvoiceQuantity", 3m, drawbackInvoiceLine.JI_InvoiceQuantity);
			AssertEquals("JI_CustomsQuantity", 113.398m, drawbackInvoiceLine.JI_CustomsQuantity);
			AssertEquals("JI_CustomsUnitQty", "KG", drawbackInvoiceLine.JI_CustomsUnitQty);

			// set customs qty from matching invoice line invoice quantity
			drawbackInvoiceLine = drawbackDecInvoice.JobComInvoiceLines[4];
			AssertEquals("JI_PartNo", "PART4", drawbackInvoiceLine.JI_PartNo);
			AssertEquals("JI_Description", "DESCRIPTION6", drawbackInvoiceLine.JI_Description);
			AssertEquals("JI_LinePrice", 0m, drawbackInvoiceLine.JI_LinePrice);
			AssertEquals("JI_InvoiceQuantity", 300m, drawbackInvoiceLine.JI_InvoiceQuantity);
			AssertEquals("EDN", "EDNNO", drawbackInvoiceLine.AddInfo.ZA_EDN_Hidden);
			AssertEquals("JI_CustomsQuantity", 300m, drawbackInvoiceLine.JI_CustomsQuantity);
			AssertEquals("JI_CustomsUnitQty", "KG", drawbackInvoiceLine.JI_CustomsUnitQty);

			// set customs qty from matching invoice line invoice quantity with diverse weight units
			drawbackInvoiceLine = drawbackDecInvoice.JobComInvoiceLines[5];
			AssertEquals("JI_PartNo", "PART4", drawbackInvoiceLine.JI_PartNo);
			AssertEquals("JI_Description", "DESCRIPTION7", drawbackInvoiceLine.JI_Description);
			AssertEquals("JI_LinePrice", 0m, drawbackInvoiceLine.JI_LinePrice);
			AssertEquals("JI_InvoiceQuantity", 300m, drawbackInvoiceLine.JI_InvoiceQuantity);
			AssertEquals("EDN", "EDNNO", drawbackInvoiceLine.AddInfo.ZA_EDN_Hidden);
			AssertEquals("JI_CustomsQuantity", 136.0776m, drawbackInvoiceLine.JI_CustomsQuantity);
			AssertEquals("JI_CustomsUnitQty", "KG", drawbackInvoiceLine.JI_CustomsUnitQty);

			// set customs qty from linked import declaration line
			drawbackInvoiceLine = drawbackDecInvoice.JobComInvoiceLines[6];
			AssertEquals("JI_PartNo", "PART4", drawbackInvoiceLine.JI_PartNo);
			AssertEquals("JI_Description", "DESCRIPTION8", drawbackInvoiceLine.JI_Description);
			AssertEquals("JI_LinePrice", 0m, drawbackInvoiceLine.JI_LinePrice);
			AssertEquals("JI_InvoiceQuantity", 300m, drawbackInvoiceLine.JI_InvoiceQuantity);
			AssertEquals("EDN", "EDNNO", drawbackInvoiceLine.AddInfo.ZA_EDN_Hidden);
			AssertEquals("JI_CustomsQuantity", 150m, drawbackInvoiceLine.JI_CustomsQuantity);
			AssertEquals("JI_CustomsUnitQty", "KG", drawbackInvoiceLine.JI_CustomsUnitQty);

			// set customs qty from linked import declaration line with diverse weight units
			drawbackInvoiceLine = drawbackDecInvoice.JobComInvoiceLines[7];
			AssertEquals("JI_PartNo", "PART5", drawbackInvoiceLine.JI_PartNo);
			AssertEquals("JI_Description", "DESCRIPTION9", drawbackInvoiceLine.JI_Description);
			AssertEquals("JI_LinePrice", 0m, drawbackInvoiceLine.JI_LinePrice);
			AssertEquals("JI_InvoiceQuantity", 3000m, drawbackInvoiceLine.JI_InvoiceQuantity);
			AssertEquals("EDN", "EDNNO", drawbackInvoiceLine.AddInfo.ZA_EDN_Hidden);
			AssertEquals("JI_CustomsQuantity", 5669.9m, drawbackInvoiceLine.JI_CustomsQuantity);
			AssertEquals("JI_CustomsUnitQty", "NO", drawbackInvoiceLine.JI_CustomsUnitQty);

			// set customs qty from export line net weight
			drawbackInvoiceLine = drawbackDecInvoice.JobComInvoiceLines[8];
			AssertEquals("JI_PartNo", "PART4", drawbackInvoiceLine.JI_PartNo);
			AssertEquals("JI_Description", "DESCRIPTION10", drawbackInvoiceLine.JI_Description);
			AssertEquals("JI_LinePrice", 0m, drawbackInvoiceLine.JI_LinePrice);
			AssertEquals("JI_InvoiceQuantity", 300m, drawbackInvoiceLine.JI_InvoiceQuantity);
			AssertEquals("EDN", "EDNNO", drawbackInvoiceLine.AddInfo.ZA_EDN_Hidden);
			AssertEquals("JI_CustomsQuantity", 200m, drawbackInvoiceLine.JI_CustomsQuantity);
			AssertEquals("JI_CustomsUnitQty", "KG", drawbackInvoiceLine.JI_CustomsUnitQty);

			// set customs qty from export line net weight with diverse weight units
			drawbackInvoiceLine = drawbackDecInvoice.JobComInvoiceLines[9];
			AssertEquals("JI_PartNo", "PART4", drawbackInvoiceLine.JI_PartNo);
			AssertEquals("JI_Description", "DESCRIPTION11", drawbackInvoiceLine.JI_Description);
			AssertEquals("JI_LinePrice", 0m, drawbackInvoiceLine.JI_LinePrice);
			AssertEquals("JI_InvoiceQuantity", 300m, drawbackInvoiceLine.JI_InvoiceQuantity);
			AssertEquals("EDN", "EDNNO", drawbackInvoiceLine.AddInfo.ZA_EDN_Hidden);
			AssertEquals("JI_CustomsQuantity", 90.7184m, drawbackInvoiceLine.JI_CustomsQuantity);
			AssertEquals("JI_CustomsUnitQty", "KG", drawbackInvoiceLine.JI_CustomsUnitQty);
		}

		public void TestDetails()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_DeclarationReference = "B99999999";
			declaration.JE_HouseBill = "House Bill";
			declaration.JE_MasterBill = "Master Bill";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.DeclarationNumber = "12345";

			ZString expectedResult = "Declaration Reference: B99999999\r\nHousebill: House Bill\r\nMasterbill: Master Bill\r\nCAN: 12345\r\n";
			AssertEquals("Declaration details", true, declaration.Details.Contains(expectedResult));

			OrgHeader orgHeader = OrgHeader.LoadFromCode(Factory, "JAYSCH");
			declaration.JE_OH_Supplier = orgHeader.PK;
			declaration.JE_OH_Importer = orgHeader.PK;

			declaration.JE_RL_NKOrigin = "AUBNE";
			declaration.JE_RL_NKFinalDestination = "SGSIN";

			expectedResult = "Declaration Reference: B99999999\r\nHousebill: House Bill\r\nMasterbill: Master Bill\r\nConsignor: JAY SCHULZ\r\nConsignee: JAY SCHULZ\r\nOrigin: AUBNE\r\nDestination: SGSIN\r\n";
			AssertEquals("Declaration details", true, declaration.Details.Contains(expectedResult));
		}

		public void TestCalculateDrawbackTotals()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;

			JobComInvoiceHeader header = Factory.New<JobComInvoiceHeader>();
			header.JZ_JE = declaration.PK;

			JobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();
			line1.IsDrawbackLineValueOverriden = true;
			line1.AddInfo.ZA_DDT_Hidden = 1.0m;
			line1.AddInfo.ZA_DAM_Hidden = Enterprise.Customs.AU.Declaration.Business.JobDeclaration.DrawbackAssessmentMethods.ActualShipment;
			line1.JI_InvoiceQuantity = 6.0m;

			JobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();
			line2.IsDrawbackLineValueOverriden = true;
			line2.AddInfo.ZA_DDT_Hidden = 2.0m;
			line2.AddInfo.ZA_DAM_Hidden = Enterprise.Customs.AU.Declaration.Business.JobDeclaration.DrawbackAssessmentMethods.ActualShipment;
			line2.JI_InvoiceQuantity = 5.0m;

			JobComInvoiceLine line3 = header.JobComInvoiceLines.AddNew();
			line3.IsDrawbackLineValueOverriden = true;
			line3.AddInfo.ZA_DDT_Hidden = 3.0m;
			line3.AddInfo.ZA_DAM_Hidden = Enterprise.Customs.AU.Declaration.Business.JobDeclaration.DrawbackAssessmentMethods.Imputation;
			line3.JI_InvoiceQuantity = 4.0m;

			JobComInvoiceLine line4 = header.JobComInvoiceLines.AddNew();
			line4.IsDrawbackLineValueOverriden = true;
			line4.AddInfo.ZA_DDT_Hidden = 4.0m;
			line4.AddInfo.ZA_DAM_Hidden = Enterprise.Customs.AU.Declaration.Business.JobDeclaration.DrawbackAssessmentMethods.Imputation;
			line4.JI_InvoiceQuantity = 3.0m;

			JobComInvoiceLine line5 = header.JobComInvoiceLines.AddNew();
			line5.IsDrawbackLineValueOverriden = true;
			line5.AddInfo.ZA_DDT_Hidden = 5.0m;
			line5.AddInfo.ZA_DAM_Hidden = Enterprise.Customs.AU.Declaration.Business.JobDeclaration.DrawbackAssessmentMethods.RepresentativeShipment;
			line5.JI_InvoiceQuantity = 2.0m;

			JobComInvoiceLine line6 = header.JobComInvoiceLines.AddNew();
			line6.IsDrawbackLineValueOverriden = true;
			line6.AddInfo.ZA_DDT_Hidden = 6.0m;
			line6.AddInfo.ZA_DAM_Hidden = Enterprise.Customs.AU.Declaration.Business.JobDeclaration.DrawbackAssessmentMethods.RepresentativeShipment;
			line6.JI_InvoiceQuantity = 1.0m;

			Factory.Save();

			AssertEquals("TotalDrawbackClaimAmount", 21.0m, declaration.TotalDrawbackClaimAmount);
			AssertEquals("TotalDrawbackMethodAAmount (Actual Shipment)", 3.0m, declaration.TotalDrawbackMethodAAmount);
			AssertEquals("TotalDrawbackMethodBAmount (Representative)", 11.0m, declaration.TotalDrawbackMethodBAmount);
			AssertEquals("TotalDrawbackMethodCAmount (Imputation)", 7.0m, declaration.TotalDrawbackMethodCAmount);

			AssertEquals("TotalDrawbackClaimQuantity", 21.0m, declaration.TotalDrawbackClaimQuantity);
			AssertEquals("TotalDrawbackMethodAQuantity (Actual Shipment)", 11.0m, declaration.TotalDrawbackMethodAQuantity);
			AssertEquals("TotalDrawbackMethodBQuantity (Representative)", 3.0m, declaration.TotalDrawbackMethodBQuantity);
			AssertEquals("TotalDrawbackMethodCQuantity (Imputation)", 7.0m, declaration.TotalDrawbackMethodCQuantity);
		}

		public void TestShortDescription()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_DeclarationReference = "B99999999";

			AssertEquals("ShortDescription", "Declaration Reference: B99999999", declaration.ShortDescription);
		}

		public void TestDetailsForCMR()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_DeclarationReference = "B99999999";
			declaration.JE_HouseBill = "House Bill";
			declaration.JE_MasterBill = "Master Bill";

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "12345";

			ZString expectedResult = "Declaration Reference: B99999999\r\nHousebill: House Bill\r\nMasterbill: Master Bill\r\nEntry Number: 12345\r\n";
			AssertEquals("Declaration details", true, declaration.Details.Contains(expectedResult));

			OrgHeader orgHeader = OrgHeader.LoadFromCode(Factory, "JAYSCH");
			declaration.JE_OH_Supplier = orgHeader.PK;
			declaration.JE_OH_Importer = orgHeader.PK;

			declaration.JE_RL_NKOrigin = "AUBNE";
			declaration.JE_RL_NKFinalDestination = "SGSIN";

			expectedResult = "Declaration Reference: B99999999\r\nHousebill: House Bill\r\nMasterbill: Master Bill\r\nEntry Number: 12345\r\nConsignor: JAY SCHULZ\r\nConsignee: JAY SCHULZ\r\nOrigin: AUBNE\r\nDestination: SGSIN\r\n";
			AssertEquals("Declaration details", true, declaration.Details.Contains(expectedResult));
		}

		public void TestExportEntryNumber()
		{
			var entryNumber1 = Factory.New<CusEntryNumber>();
			entryNumber1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryNumber1.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entryNumber1.CE_EntryType = CusEntryNumberTypes.Australia.ECN;
			entryNumber1.CE_EntryNum = "TEST1";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNull(declaration.ExportEntryNumber);

			entryNumber1.Parent = declaration;
			AssertEquals("Get CusEntryNumber from dbo.JobDeclaration", "TEST1", declaration.ExportEntryNumber.CE_EntryNum);

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryNumber2 = Factory.New<CusEntryNumber>();
			entryNumber2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryNumber2.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entryNumber2.CE_EntryType = CusEntryNumberTypes.Australia.ECN;
			entryNumber2.Parent = entryHeader;
			entryNumber2.CE_EntryNum = "TEST2";
			AssertEquals("Get CusEntryNumber from dbo.CusEntryHeader", "TEST2", declaration.ExportEntryNumber.CE_EntryNum);
		}

		public void TestContingencyCAN()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(ZString.Empty, declaration.ContingencyCAN);
			declaration.ContingencyCAN = "123";
			AssertEquals("123", declaration.ContingencyCAN);
			declaration.ContingencyCAN = "456";
			AssertEquals("456", declaration.ContingencyCAN);
			declaration.ContingencyCAN = ZString.Empty;
			AssertEquals(ZString.Empty, declaration.ContingencyCAN);
		}

		public void TestChangingContingencyCANSetsHasChanges()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Factory.Save();
			Assert("DoesntHaveChanges", !declaration.HasChanges);
			declaration.ContingencyCAN = "123";
			Assert("HasChanges", declaration.HasChanges);
		}

		public void TestContingencyCANReadonlyIfWeHaveADeclarationNumber()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			Assert(!declaration.ContingencyCANInfo.ReadOnly);
			declaration.DeclarationNumber = "12345";
			Assert(declaration.ContingencyCANInfo.ReadOnly);
		}

		public void TestDefaultValuesIfWeChangeToPost()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_ExportGoodsType = JobDeclaration.ExportGoodsType.OwnPower;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			AssertEquals(JobDeclaration.ExportGoodsType.Postal, declaration.JE_ExportGoodsType);
			AssertEquals(Core.Constants.ContainerModes.NonContainerised, declaration.JE_ContainerMode);
		}

		public void TestShouldWeCompareDeclarations()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].Charges.AddNew();
			declaration.CusContainers.AddNew();
			Factory.Save();
			Assert(!declaration.ShouldWeCompareDeclarations);
			shipment.JS_NoCopyBills = 100;
			Assert(!declaration.ShouldWeCompareDeclarations);
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_InvoiceAmount = 100m;
			Assert(declaration.ShouldWeCompareDeclarations);
			Factory.Save();
			Assert(!declaration.ShouldWeCompareDeclarations);
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_Description = "123";
			Assert(declaration.ShouldWeCompareDeclarations);
			Factory.Save();
			declaration.CusContainers[0].CO_ContainerNumber = "123";
			Assert(declaration.ShouldWeCompareDeclarations);
			Factory.Save();
			JobComInvoiceHeader header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0];
			InvoiceCharge charge = header.Charges[0];
			charge.J7_Amount = 100m;
			Assert(declaration.ShouldWeCompareDeclarations);
		}

		public void TestHasMixedPaymenrModesBeenUsed1()
		{
			JobDeclaration testJobDeclaration = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader1 = testJobDeclaration.CustomsEntryHeaders.AddNew();
			CusEntryHeader entryHeader2 = testJobDeclaration.CustomsEntryHeaders.AddNew();
			CMRPAYRECMessage message1 = Factory.New<CMRPAYRECMessage>();
			entryHeader1.Messages.Add(message1);
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageType = CMRMessage.CMRMessageTypes.PAYREC;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::PAYREC+2C4B 403F EIFF:1+11'
NAD+AO+123456::215++LEON BALL'
UNT+4+000001'".Replace("\r\n", "");
			CMRPAYRECMessage message2 = Factory.New<CMRPAYRECMessage>();
			entryHeader1.Messages.Add(message2);
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = CMRMessage.CMRMessageTypes.PAYREC;
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message2.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::PAYREC+2C4B 403F EIFF:1+11'
NAD+AO+123456::215++LEON BALL'
UNT+4+000001'".Replace("\r\n", "");
			CMRPAYRECMessage message3 = Factory.New<CMRPAYRECMessage>();
			entryHeader2.Messages.Add(message3);
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message3.EM_MessageType = CMRMessage.CMRMessageTypes.PAYREC;
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message3.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::PAYREC+2C4B 403F EIFF:1+11'
NAD+AO+123456::215++LEON BALL'
UNT+4+000001'".Replace("\r\n", "");

			Assert("All the same bank account", !testJobDeclaration.HasMixedPaymentModesBeenUsed);
		}

		public void TestHasMixedPaymenrModesBeenUsed2()
		{
			JobDeclaration testJobDeclaration = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader1 = testJobDeclaration.CustomsEntryHeaders.AddNew();
			CusEntryHeader entryHeader2 = testJobDeclaration.CustomsEntryHeaders.AddNew();
			CMRPAYRECMessage message1 = Factory.New<CMRPAYRECMessage>();
			entryHeader1.Messages.Add(message1);
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageType = CMRMessage.CMRMessageTypes.PAYREC;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::PAYREC+2C4B 403F EIFF:1+11'
NAD+AO+123456::215++LEON BALL'
UNT+4+000001'".Replace("\r\n", "");
			CMRPAYRECMessage message2 = Factory.New<CMRPAYRECMessage>();
			entryHeader1.Messages.Add(message2);
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = CMRMessage.CMRMessageTypes.PAYREC;
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message2.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::PAYREC+2C4B 403F EIFF:1+11'
NAD+AO+123456::215++LEON BALL'
UNT+4+000001'".Replace("\r\n", "");
			CMRPAYRECMessage message3 = Factory.New<CMRPAYRECMessage>();
			entryHeader2.Messages.Add(message3);
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message3.EM_MessageType = CMRMessage.CMRMessageTypes.PAYREC;
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message3.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::PAYREC+2C4B 403F EIFF:1+11'
NAD+AO+7777777::215++LEON BALL'
UNT+4+000001'".Replace("\r\n", "");

			Assert("Message 3 has different bank account", testJobDeclaration.HasMixedPaymentModesBeenUsed);
		}

		public void TestHasMixedPaymenrModesBeenUsed3()
		{
			JobDeclaration testJobDeclaration = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader1 = testJobDeclaration.CustomsEntryHeaders.AddNew();
			CusEntryHeader entryHeader2 = testJobDeclaration.CustomsEntryHeaders.AddNew();
			CMRPAYRECMessage message1 = Factory.New<CMRPAYRECMessage>();
			entryHeader1.Messages.Add(message1);
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageType = CMRMessage.CMRMessageTypes.PAYREC;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::PAYREC+2C4B 403F EIFF:1+11'
NAD+AO+123456::215++LEON BALL'
UNT+4+000001'".Replace("\r\n", "");
			CMRPAYRECMessage message2 = Factory.New<CMRPAYRECMessage>();
			entryHeader1.Messages.Add(message2);
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = CMRMessage.CMRMessageTypes.PAYREC;
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message2.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::PAYREC+2C4B 403F EIFF:1+11'
NAD+AO+888888::215++LEON BALL'
UNT+4+000001'".Replace("\r\n", "");
			CMRPAYRECMessage message3 = Factory.New<CMRPAYRECMessage>();
			entryHeader2.Messages.Add(message3);
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message3.EM_MessageType = CMRMessage.CMRMessageTypes.PAYREC;
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message3.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::PAYREC+2C4B 403F EIFF:1+11'
NAD+AO+123456::215++LEON BALL'
UNT+4+000001'".Replace("\r\n", "");

			Assert("Message 2 has different bank account", testJobDeclaration.HasMixedPaymentModesBeenUsed);
		}

		public void TestLoad()
		{
			JobDeclaration dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "123";
			AssertNull(JobDeclaration.LoadFirstMatchingInCurrentCompanyIncludingInActive(Factory, ""));
			AssertNotNull(JobDeclaration.LoadFirstMatchingInCurrentCompanyIncludingInActive(Factory, "123"));
		}

		public void TestPackagesForLegacy()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_CL = declaration.CustomsEntryHeaders[0].MergedLines[0].PK;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].JobComInvoiceLines[0].JI_CL = declaration.CustomsEntryHeaders[1].MergedLines[0].PK;
			declaration.JE_TotalNoOfPacks = 10;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_Nature10PackCount = 10;

			AssertEquals("Packages", 10, declaration.CustomsEntryHeaders[0].PackagesCount);
			AssertEquals("Packages", 0, declaration.CustomsEntryHeaders[1].PackagesCount);
		}

		public void TestAllUnusedPortsAreEmptyForNature30()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_RL_NKPortOfFirstArrival = "AUSYD";
			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("JE_RL_NKFinalDestination", "AUSYD", declaration.JE_RL_NKFinalDestination);
			AssertEquals("JE_RL_NKOrigin", ZString.Empty, declaration.JE_RL_NKOrigin);
			AssertEquals("JE_RL_NKPortOfArrival", ZString.Empty, declaration.JE_RL_NKPortOfArrival);
			AssertEquals("JE_RL_NKPortOfFirstArrival", ZString.Empty, declaration.JE_RL_NKPortOfFirstArrival);
			AssertEquals("JE_RL_NKPortOfLoading", ZString.Empty, declaration.JE_RL_NKPortOfLoading);
		}

		public void TestAllDatesAreEmptyForNature30()
		{
			ZDateTime now = ZDateTime.Now;
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			declaration.JE_DateAtFinalDestination = now;
			declaration.JE_DateAtOrigin = now;
			declaration.JE_DateOfArrival = now;
			declaration.JE_DateOfFirstArrival = now;
			declaration.JE_ExportDate = now;
			AssertEquals("JE_DateAtFinalDestination", ZDateTime.Empty, declaration.JE_DateAtFinalDestination);
			AssertEquals("JE_DateAtOrigin", ZDateTime.Empty, declaration.JE_DateAtOrigin);
			AssertEquals("JE_DateAtOrigin", ZDateTime.Empty, declaration.JE_DateAtOrigin);
			AssertEquals("JE_DateOfFirstArrival", ZDateTime.Empty, declaration.JE_DateOfFirstArrival);
			AssertEquals("JE_ExportDate", ZDateTime.Empty, declaration.JE_ExportDate);
		}

		public void TestILandedCostHeader()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "LEG";

			CusEntryHeader entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.Charges.AddNew(CusEntryChargeTypeList.Codes.EntryFee, 1.52m);
			entryHeader1.Charges.AddNew(CusEntryChargeTypeList.Codes.MessageFee, 2.52m);
			entryHeader1.Charges.AddNew(CusEntryChargeTypeList.Codes.ScreenFree, 3.52m);
			entryHeader1.Charges.AddNew(CusEntryChargeTypeList.Codes.TradegateGST, 4.50m);
			entryHeader1.Charges.AddNew(CusEntryChargeTypeList.Codes.OtherCharges, 5.62m);
			entryHeader1.Charges.AddNew(CusEntryChargeTypeList.Codes.Woodlevy, 6.45m);
			entryHeader1.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISContainerCharges, 7.45m);
			entryHeader1.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISProcessingCharge, 8.62m);
			entryHeader1.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISServicePaymentAmount, 10.45m);

			CusEntryLine entryLine1 = entryHeader1.MergedLines.AddNew();
			entryLine1.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.LCTAmount, 7.56m);
			entryLine1.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.WetAmount, 8.41m);
			entryLine1.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.FlatDutyPortion, 11.90m);
			entryLine1.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DumpingDuty, 17.81m);
			entryLine1.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DutyAmount, 101m);

			CusEntryHeader entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.Charges.AddNew(CusEntryChargeTypeList.Codes.EntryFee, 1.62m);
			entryHeader2.Charges.AddNew(CusEntryChargeTypeList.Codes.MessageFee, 2.62m);
			entryHeader2.Charges.AddNew(CusEntryChargeTypeList.Codes.ScreenFree, 3.62m);
			entryHeader2.Charges.AddNew(CusEntryChargeTypeList.Codes.TradegateGST, 4.60m);
			entryHeader2.Charges.AddNew(CusEntryChargeTypeList.Codes.OtherCharges, 5.72m);
			entryHeader2.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISServicePaymentAmount, 7.78m);
			var estimatedWoodLevyNotPayable = entryHeader2.Charges.AddNew();
			estimatedWoodLevyNotPayable.C1_ChargeType = CusEntryChargeTypeList.Codes.Woodlevy;
			estimatedWoodLevyNotPayable.C1_ChargeAmount = 0.03m;
			estimatedWoodLevyNotPayable.C1_IsLandedCostOnly = true;
			entryHeader2.Charges.AddNew(CusEntryChargeTypeList.Codes.Woodlevy, 6.78m);

			CusEntryLine entryLine2 = entryHeader2.MergedLines.AddNew();
			entryLine2.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.LCTAmount, 7.46m);
			entryLine2.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.WetAmount, 8.31m);
			entryLine2.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.FlatDutyPortion, 10.90m);
			entryLine2.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DumpingDuty, 22.11m);
			entryLine2.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DutyAmount, 5m);

			CusEntryLine entryLine3 = entryHeader2.MergedLines.AddNew();
			var estimatedLCTNotPayable = entryLine2.Fees.AddNew();
			estimatedLCTNotPayable.CF_ChargeType = CusEntryChargeTypeList.Codes.LCTAmount;
			estimatedLCTNotPayable.CF_ChargeAmount = 0.01m;
			estimatedLCTNotPayable.CF_IsLandedCostOnly = true;
			var estimatedWETNotPayable = entryLine2.Fees.AddNew();
			estimatedWETNotPayable.CF_ChargeType = CusEntryChargeTypeList.Codes.WetAmount;
			estimatedWETNotPayable.CF_ChargeAmount = 0.02m;
			estimatedWETNotPayable.CF_IsLandedCostOnly = true;
			var estimatedDutyNotPayable = entryLine2.Fees.AddNew();
			estimatedDutyNotPayable.CF_ChargeType = CusEntryChargeTypeList.Codes.DutyAmount;
			estimatedDutyNotPayable.CF_ChargeAmount = 2m;
			estimatedDutyNotPayable.CF_IsLandedCostOnly = true;

			DutyTaxEntryFee total = ((ILandedCostHeader)declaration).TotalDutyTaxEntryFeeItems;

			AssertEquals("TotalEntryFees", 17.63m, total["ENT"]);
			AssertEquals("AQIS Fee", 18.23m, total["QUA"]);
			AssertEquals("WET, Special Tax1", 16.74m, total["ST1"]);
			AssertEquals("LCT, Special Tax2", 15.03m, total["ST2"]);
			AssertEquals("Wood Levy, Special Tax3", 13.26m, total["ST3"]);
			AssertEquals("Excise", 0m, total["EXC"]);
			AssertEquals("Other duty", 62.72m, total["OTH"]);

			declaration.JE_ApplicationCode = "CMR";
			total = ((ILandedCostHeader)declaration).TotalDutyTaxEntryFeeItems;
			AssertEquals("TotalEntryFees", 27.41m, total["ENT"]);
			AssertEquals("AQIS Fee", 18.23m, total["QUA"]);
			AssertEquals("Duty", 108m, total["TDT"]);
		}

		public void TestQuarantineInvoice()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertNull("There are no invoices and the job isn't quarantine", declaration.QuarantineInvoice);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			AssertNull("There are no invoices, job is quarantine", declaration.QuarantineInvoice);
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			AssertNotNull("Quarantine Invoice is not null", declaration.QuarantineInvoice);
			AssertEquals("Same object returned", invoiceHeader.PK, declaration.QuarantineInvoice.PK);
		}

		public void TestMessagesHaveBeenSentForQuarantine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			Assert("Messages not sent", !declaration.DeclarationMessagesHaveBeenSent());
			var message = invoiceHeader.QuarantineExDocHeader.Messages.AddNew();
			Assert("Messages sent", declaration.DeclarationMessagesHaveBeenSent());
			message.EM_Status = EDIMessage.Status.Discarded;
			Assert("Messages not sent", !declaration.DeclarationMessagesHaveBeenSent());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("Messages not sent", !declaration.DeclarationMessagesHaveBeenSent());
		}

		public void TestIsQuarantineChargeRatingSeparated()
		{
			var chargeCodeWithDate = new ChargeCodeWithDate();
			chargeCodeWithDate.ChargeCode = Guid.NewGuid();
			chargeCodeWithDate.ActiveTimeUtc = ZDateTime.Empty;

			RatingDataRegistry.Instance.CustomsQuarantineChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chargeCodeWithDate);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			Assert(declaration.IsQuarantineChargeRatingSeparated);

			chargeCodeWithDate.ActiveTimeUtc = ZDateTime.Now.AddDays(1);
			RatingDataRegistry.Instance.CustomsQuarantineChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chargeCodeWithDate);
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			Assert(!declaration.IsQuarantineChargeRatingSeparated);

			chargeCodeWithDate.ChargeCode = Guid.NewGuid();
			chargeCodeWithDate.ActiveTimeUtc = ZDateTime.Now;
			RatingDataRegistry.Instance.CustomsQuarantineChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chargeCodeWithDate);
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			Assert(declaration.IsQuarantineChargeRatingSeparated);
		}

		public void TestIOnUniversalEventAddedHandler()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert("JobDeclaration implements IOnUniversalEventAddedHandler", declaration is IOnUniversalEventAddedHandler);
		}

		[TestDate(2021, 06, 24, 12, 33, 22)]
		public void TestCreateQuarantineCertificateNumberIfNeeded()
		{
			// end-to-end test is in JobDeclarationEventParentFinder.

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			declaration.JE_DeclarationReference = "B00001139";
			declaration.Invoices.AddNew();
			var quarantineHeader = declaration.QuarantineInvoice.QuarantineExDocHeader;

			var serviceLogger = new UniversalDataBuss.Management.Testing.ServiceTaskLogForTesting();
			var messageLogger = new UniversalDataBuss.Management.XmlSessionTracker(serviceLogger);
			IOnUniversalEventAddedHandler eventAddedHandler = declaration;

			var uEvent = new UniversalEvent();
			uEvent.EventType = Events.DocumentImportedCode;
			AssertNoExceptionThrown("Handles a missing DataContext", () => eventAddedHandler.OnUniversalEventAdded(messageLogger, uEvent));
			AssertEquals(0, quarantineHeader.CertificateNumbers.Count);

			uEvent.DataContext = new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext()
			{
				ActionPurpose = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = Constants.ActionPurpose.ADD }
			};

			AssertNoExceptionThrown("Handles a missing DocumentCollection", () => eventAddedHandler.OnUniversalEventAdded(messageLogger, uEvent));
			AssertEquals(0, quarantineHeader.CertificateNumbers.Count);

			var doc1 = new UniversalDataBuss.DataObjects.Universal.AttachedDocument()
			{
				FileName = "Doc1.pdf"
			};

			uEvent.AttachedDocumentCollection = new List<UniversalDataBuss.DataObjects.Universal.AttachedDocument>();
			uEvent.AttachedDocumentCollection.Add(doc1);

			AssertNoExceptionThrown("Handles a missing DocumentType", () => eventAddedHandler.OnUniversalEventAdded(messageLogger, uEvent));
			AssertEquals(0, quarantineHeader.CertificateNumbers.Count);

			doc1.Type = new UniversalDataBuss.DataObjects.Universal.DocumentType() { Code = "QRP" };
			eventAddedHandler.OnUniversalEventAdded(messageLogger, uEvent);

			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, quarantineHeader.PK);
			query.AddToFilter(CusEntryNumSchema.CE_ParentTable, QuarantineExDocHeader.Schema.TableName);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Australia.QuarantineCertificateNumber);
			var certificateEntryNumbers = Factory.Load<CusEntryNumber>(query);
			AssertEquals("Creates QCN entry number", 1, certificateEntryNumbers.Length);

			var certificate = certificateEntryNumbers[0];
			AssertEquals("CE_EntryNum", "Doc1", certificate.CE_EntryNum);
			AssertEquals("CE_RN_NKCountryCode", Core.Constants.CountryCodes.Australia, certificate.CE_RN_NKCountryCode);
			AssertEquals("CE_Category", CusEntryNumber.Categories.CustomsPermitClearanceNumber, certificate.CE_Category);
			AssertEquals("CE_IssueDate is 'now' when event time is empty", "2021-06-24T12:33:22", certificate.CE_IssueDate.ToISO8601String());
			AssertEquals("CertificateNumbers collection picks up entry number", 1, quarantineHeader.CertificateNumbers.Count);

			// --------------

			quarantineHeader.CertificateNumbers.DeleteAll();

			uEvent.EventTime = new ZDateTimeOffset(2021, 06, 05, 04, 26, 23);
			uEvent.AttachedDocumentCollection.Add(new UniversalDataBuss.DataObjects.Universal.AttachedDocument()
			{
				FileName = "Doc2",
				Type = new UniversalDataBuss.DataObjects.Universal.DocumentType() { Code = "QRP" }
			});

			eventAddedHandler.OnUniversalEventAdded(messageLogger, uEvent);
			AssertEquals("Processes multiple documents", 2, quarantineHeader.CertificateNumbers.Count);

			var cert2 = quarantineHeader.CertificateNumbers.FirstOrDefault(n => n.CE_EntryNum == "Doc2");
			AssertEquals("CE_IssueDate is from EventTime", "2021-06-05T04:26:23", cert2.CE_IssueDate.ToISO8601String());

			// --------------

			quarantineHeader.CertificateNumbers.DeleteAll();

			uEvent.AttachedDocumentCollection.Add(new UniversalDataBuss.DataObjects.Universal.AttachedDocument()
			{
				FileName = "",
				Type = new UniversalDataBuss.DataObjects.Universal.DocumentType() { Code = "QRP" }
			});

			uEvent.AttachedDocumentCollection.Add(new UniversalDataBuss.DataObjects.Universal.AttachedDocument()
			{
				FileName = "Doc3.pdf",
				Type = new UniversalDataBuss.DataObjects.Universal.DocumentType() { Code = "XXX" }
			});

			eventAddedHandler.OnUniversalEventAdded(messageLogger, uEvent);
			AssertEquals("Accepts only QRP documents with filenames", 2, quarantineHeader.CertificateNumbers.Count);
		}

		public void TestJE_SettlementPeriodType() => CombineAssertions(() =>
		{
			testJobDeclaration.AddInfo.ZA_SettlementPeriodType_Hidden = "SM";
			AssertEquals("Getter", "SM", testJobDeclaration.JE_SettlementPeriodType);
			testJobDeclaration.JE_SettlementPeriodType = "SQ";
			AssertEquals("Setter", "SQ", testJobDeclaration.AddInfo.ZA_SettlementPeriodType_Hidden);
		});

		public void TestSettlementTypeSelected() => CombineAssertions(() =>
		{
			AssertEquals("ZA_SettlementPeriodType_Hidden not set", false, testJobDeclaration.SettlementTypeSelected);
			testJobDeclaration.AddInfo.ZA_SettlementPeriodType_Hidden = "SM";
			AssertEquals("ZA_SettlementPeriodType_Hidden set", true, testJobDeclaration.SettlementTypeSelected);
		});

		public void TestJE_SettlementPeriodEndDate() => CombineAssertions(() =>
		{
			testJobDeclaration.AddInfo.ZA_SettlementPeriodEndDate_Hidden = new ZDateTime(2023, 07, 04);
			AssertEquals("Getter", new ZDateTime(2023, 07, 04), testJobDeclaration.JE_SettlementPeriodEndDate);
			testJobDeclaration.JE_SettlementPeriodEndDate = new ZDateTime(2022, 10, 28);
			AssertEquals("Setter", new ZDateTime(2022, 10, 28), testJobDeclaration.AddInfo.ZA_SettlementPeriodEndDate_Hidden);
		});

		#region SAC

		public void TestIsSACWithoutLines()
		{
			testJobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			testJobDeclaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			AssertEquals("IsSACWithoutLines", true, testJobDeclaration.IsSACWithoutLines);
			AssertEquals("IsSACWithoutLines With lines", false, testJobDeclaration.IsSACWithLines);

			testJobDeclaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			AssertEquals("IsSACWithoutLines", false, testJobDeclaration.IsSACWithoutLines);

			testJobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			testJobDeclaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			AssertEquals("IsSACWithoutLines", false, testJobDeclaration.IsSACWithoutLines);
		}

		public void TestIsSACWithLines()
		{
			testJobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			testJobDeclaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			AssertEquals("IsSACWithoutLines With lines", true, testJobDeclaration.IsSACWithLines);
			AssertEquals("IsSACWithoutLines", false, testJobDeclaration.IsSACWithoutLines);

			testJobDeclaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			AssertEquals("IsSACWithoutLines with lines", false, testJobDeclaration.IsSACWithLines);

			testJobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			testJobDeclaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			AssertEquals("IsSACWithoutLines", false, testJobDeclaration.IsSACWithLines);
		}

		public void TestIsSACWhatever()
		{
			testJobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			testJobDeclaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			AssertEquals("IsSAC", true, testJobDeclaration.IsSAC);
			testJobDeclaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			AssertEquals("IsSAC", true, testJobDeclaration.IsSAC);
			testJobDeclaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			AssertEquals("IsSAC", false, testJobDeclaration.IsSAC);
		}

		[ExpectNoExceptions]
		public void TestSavingAFakeInvoiceLineDoesNotCauseAnyExceptionWhenSAC()
		{
			JobDeclaration testJobDeclaration = JobDeclaration.New(Factory);
			testJobDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(true);
			testJobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testJobDeclaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			testJobDeclaration.Invoices.AddNew();
			testJobDeclaration.DoMerge();
			Factory.Save();
		}

		#endregion

		#region ATA Tests

		public void TestDefaultATAOfDischargeToFirstArrivalIfPortsAreSame()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DateOfArrival = new ZDateTime(2003, 12, 30);
			declaration.JE_RL_NKPortOfFirstArrival = "AUSYD";
			AssertEquals("First arrival date is defaulted", declaration.JE_DateOfArrival, declaration.JE_DateOfFirstArrival);
		}

		public void TestNotDefaultATAOfDischargeToFirstArrivalIfPortsAreDifferent()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_RL_NKPortOfFirstArrival = "AUMEL";
			declaration.JE_DateOfArrival = new ZDateTime(2003, 12, 30);
			declaration.JE_DateOfFirstArrival = ZDateTime.Empty;
			Assert("PreCondition: First arrival date empty", declaration.JE_DateOfFirstArrival.IsEmpty);
			Assert("First arrival date not defaulted", declaration.JE_DateOfFirstArrival.IsEmpty);
		}

		public void TestATAOfDischargeNotOverrideATAOfFirstArrival()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DateOfArrival = new ZDateTime(2003, 12, 30);
			declaration.JE_DateOfFirstArrival = new ZDateTime(2003, 12, 29);
			declaration.JE_RL_NKPortOfFirstArrival = "AUSYD";
			AssertEquals("ATA of First Arrival is not overriden", new ZDateTime(2003, 12, 29), declaration.JE_DateOfFirstArrival);
		}

		#endregion

		#region DoChangesResultInADifferentMessage Tests

		public void TestDoChangesResultInADifferentMessageEX1OrgTrue()
		{
			ZGuid savedDecPK = SetUpAndSaveDeclaration();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var declaration = factory.Load<JobDeclaration>(savedDecPK);
			declaration.JE_ContainerCount = 1000;
			AssertEquals("DeclarationChangesResultInMessageChanges", true, declaration.DoChangesResultInADifferentMessage(ExportDeclarationType.Original));
		}

		public void TestDoChangesResultInADifferentMessageEX1RepFalse()
		{
			ZGuid savedDecPK = SetUpAndSaveDeclaration();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var declaration = factory.Load<JobDeclaration>(savedDecPK);
			declaration.JE_MasterBill = "sdfSDFsdf";
			AssertEquals("DeclarationChangesResultInMessageChanges", false, declaration.DoChangesResultInADifferentMessage(ExportDeclarationType.Confirming));
		}

		public void TestDoChangesResultInADifferentMessageEX1RepTrue()
		{
			ZGuid savedDecPK = SetUpAndSaveDeclaration();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var declaration = factory.Load<JobDeclaration>(savedDecPK);
			declaration.JE_ExportDate = new DateTime(2000, 12, 12);
			AssertEquals("DeclarationChangesResultInMessageChanges", true, declaration.DoChangesResultInADifferentMessage(ExportDeclarationType.Original));
		}

		public void TestDoChangesResultInADifferentMessageEX1IsAlwaysFalse()
		{
			ZGuid savedDecPK = SetUpAndSaveDeclaration();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var declaration = factory.Load<JobDeclaration>(savedDecPK);
			declaration.DeclarationNumber = "12345678901234";
			declaration.ExportEntryNumber.CE_EntryType = "ECN";
			factory.Save();
			declaration.JE_ExportDate = new DateTime(2000, 12, 12);
			AssertEquals("DeclarationChangesResultInMessageChanges", false, declaration.DoChangesResultInADifferentMessage(ExportDeclarationType.Original));
		}

		public void TestDoChangesResultInADifferentMessageEXDOrgFalse()
		{
			ZGuid savedDecPK = SetUpAndSaveDeclaration();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var declaration = factory.Load<JobDeclaration>(savedDecPK);
			declaration.JE_HouseBill = "sdfSDFsdf";

			AssertEquals("DeclarationChangesResultInMessageChanges", false, declaration.DoChangesResultInADifferentMessage(ExportDeclarationType.Original));
		}

		public void TestDoChangesResultInADifferentMessageEXDOrgTrue()
		{
			ZGuid savedDecPK = SetUpAndSaveDeclaration();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var declaration = factory.Load<JobDeclaration>(savedDecPK);
			declaration.JE_ContainerCount = 1000;
			AssertEquals("DeclarationChangesResultInMessageChanges", true, declaration.DoChangesResultInADifferentMessage(ExportDeclarationType.Confirming));
		}

		public void TestDoChangesResultInADifferentMessageEXDRepFalse()
		{
			ZGuid savedDecPK = SetUpAndSaveDeclaration();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var declaration = factory.Load<JobDeclaration>(savedDecPK);
			declaration.JE_MasterBill = "sdfSDFsdf";
			AssertEquals("DeclarationChangesResultInMessageChanges", false, declaration.DoChangesResultInADifferentMessage(ExportDeclarationType.Confirming));
		}

		public void TestDoChangesResultInADifferentMessageEXDRepTrue()
		{
			ZGuid savedDecPK = SetUpAndSaveDeclaration();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var declaration = factory.Load<JobDeclaration>(savedDecPK);
			declaration.JE_ExportDate = new DateTime(2000, 12, 12);
			AssertEquals("DeclarationChangesResultInMessageChanges", true, declaration.DoChangesResultInADifferentMessage(ExportDeclarationType.Original));
		}

		[TestDate(2004, 10, 5)]
		public void TestDoChangesResultInADifferentExitMessageWithTrailingDecimalPoints()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ExportDate = new ZDateTime(2004, 7, 16);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			JobComInvoiceLine line = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			line.JI_Weight = 100m;
			line.JI_WeightUQ = "KG";
			line.JI_InvoiceQuantity = 100m;
			line.JI_InvoiceUQ = "KG";
			line.JI_CustomsQuantity = 100m;
			line.JI_CustomsUnitQty = "KG";

			Factory.Save();
			bool differentMessage = declaration.DoChangesResultInADifferentMessage(ExportDeclarationType.Original);
			Assert("Not a different message", !differentMessage);
		}

		public void TestDoChangesResultInADifferentEXDMessage()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ExportDate = ZDateTime.Today;

			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine line2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();
			JobComInvoiceLine line1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].JobComInvoiceLines.AddNew();

			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_JZ = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].PK;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Delete(declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0]);

			line1.JI_Weight = 100m;
			line1.JI_WeightUQ = "KG";
			line1.JI_InvoiceQuantity = 100m;
			line1.JI_InvoiceUQ = "KG";
			line1.JI_CustomsQuantity = 100m;
			line1.JI_CustomsUnitQty = "KG";
			line1.AddInfo.ZA_AssayCU_Hidden = 100m;

			line2.JI_Weight = 200m;
			line2.JI_WeightUQ = "KG";
			line2.JI_InvoiceQuantity = 200m;
			line2.JI_InvoiceUQ = "KG";
			line2.JI_CustomsQuantity = 200m;
			line2.JI_CustomsUnitQty = "KG";

			line1.JI_LineNo = 1;
			line2.JI_LineNo = 2;
			Factory.Save();
			bool differentMessage = declaration.DoChangesResultInADifferentMessage(ExportDeclarationType.Original);
			Assert("Not a different message", !differentMessage);
		}

		#endregion

		#region Declaration Number Tests

		[TestDate(2004, 9, 1)]
		public void TestGetDeclarationNumberForExport()
		{
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("ExportNumber should be empty", string.Empty, testJobDeclaration.DeclarationNumber);

			var exportNumber1 = Factory.New<CusEntryNumber>();
			exportNumber1.CE_EntryNum = "TEST1";
			exportNumber1.CE_EntryType = CusEntryNumberTypes.Australia.ECN;
			exportNumber1.CE_ParentID = testJobDeclaration.PK;
			exportNumber1.CE_ParentTable = JobDeclaration.Schema.TableName;
			exportNumber1.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			AssertEquals("ExportNumber (CusEntryNumber linked with JobDeclaration)", exportNumber1.CE_EntryNum, testJobDeclaration.DeclarationNumber);

			var entryHeader = testJobDeclaration.ActiveEntryHeaders.AddNew();
			var exportNumber2 = Factory.New<CusEntryNumber>();
			exportNumber2.CE_EntryNum = "TEST2";
			exportNumber2.CE_EntryType = CusEntryNumberTypes.Australia.ECN;
			exportNumber2.CE_ParentID = entryHeader.PK;
			exportNumber2.CE_ParentTable = CusEntryHeader.Schema.TableName;
			exportNumber2.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			AssertEquals("ExportNumber (CusEntryNumber linked with CusEntryHeader)", exportNumber2.CE_EntryNum, testJobDeclaration.DeclarationNumber);
		}

		public void TestGetDeclarationNumberForImport()
		{
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("ImportNumber should be empty", string.Empty, testJobDeclaration.DeclarationNumber);

			CusEntryHeader entryHeader = testJobDeclaration.CustomsEntryHeaders.AddNew();
			CusEntryNumber exportNumber = Factory.New<CusEntryNumber>();
			exportNumber.CE_EntryNum = "TEST";
			exportNumber.CE_EntryType = Customs.Business.JobMessageTypeList.Codes.Import;
			exportNumber.CE_ParentID = entryHeader.PK;
			exportNumber.CE_ParentTable = CusEntryHeader.Schema.TableName;
			exportNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			CusEntryHeader entryHeader2 = testJobDeclaration.CustomsEntryHeaders.AddNew();
			CusEntryNumber exportNumber2 = Factory.New<CusEntryNumber>();
			exportNumber2.CE_EntryNum = "TEST2";
			exportNumber2.CE_EntryType = Customs.Business.JobMessageTypeList.Codes.Import;
			exportNumber2.CE_ParentID = entryHeader2.PK;
			exportNumber2.CE_ParentTable = CusEntryHeader.Schema.TableName;
			exportNumber2.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AssertEquals("Import Number", "TEST,TEST2", testJobDeclaration.DeclarationNumber);
		}

		public void TestSetDeclarationNumberForExport()
		{
			testJobDeclaration.ActiveEntryHeaders.RemoveAndDeleteAll();
			testJobDeclaration.JE_ApplicationCode = "";
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			testJobDeclaration.DeclarationNumber = "NUM";
			AssertSame("New CusEntryNumber is linked with JobDeclaration when no CusEntryHeader", testJobDeclaration, testJobDeclaration.ExportEntryNumber.Parent);
			AssertEquals("Declaration Number", "NUM", testJobDeclaration.DeclarationNumber);

			testJobDeclaration.DeclarationNumber = "";
			Assert("Export number is cleared", testJobDeclaration.ExportEntryNumber == null);
			AssertEquals("Declaration Number", "", testJobDeclaration.DeclarationNumber);

			var entryHeader = testJobDeclaration.ActiveEntryHeaders.AddNew();
			testJobDeclaration.DeclarationNumber = "ABC";
			AssertSame("New CusEntryNumber is linked with CusEntryHeader when CusEntryHeader exists", entryHeader, testJobDeclaration.ExportEntryNumber.Parent);
			AssertEquals("Declaration Number", "ABC", testJobDeclaration.DeclarationNumber);
		}

		#endregion

		#region Default Container Mode

		public void TestDefaultContainerModeIfUserSelectsSeaAndThenExport()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("ContainerMode", testDec.Lookups.CargoIdTypeList[0].Code, testDec.JE_ContainerMode);
		}

		public void TestDefaultContainerModeIfUserSelectsSeaAndThenImport()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			testDec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("ContainerMode", ZString.Empty, testDec.JE_ContainerMode);
		}

		public void TestDefaultContainerModeForOTH()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Other;
			AssertEquals("ContainerMode should default to 'OTH' for 'OTH' TransportMode", Core.Constants.ContainerModes.Other, testDec.JE_ContainerMode);

			testDec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("ContainerMode cleared out for user input when changed to SEA", ZString.Empty, testDec.JE_ContainerMode);

			testDec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			AssertEquals("ContainerMode for AIR", Core.Constants.ContainerModes.AIR, testDec.JE_ContainerMode);

			testDec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Other;
			AssertEquals("TransportMode set back to to 'OTH' again defaults 'OTH' ContainerMode", Core.Constants.ContainerModes.Other, testDec.JE_ContainerMode);
		}

		public void TestIsOther()
		{
			var testDec = Factory.NewWithValidTestData<JobDeclaration>();
			AssertEquals(false, testDec.IsOther);

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Other;
			AssertEquals(true, testDec.IsOther);
		}

		public void TestDefaultDeliveryCartageFromConsignee()
		{
			var consignee = Factory.New<OrgHeader>();

			var cntCartage = Factory.NewWithValidTestData<OrgHeader>();
			cntCartage.OH_IsShippingProvider = true;
			cntCartage.OH_IsLocalTransport = true;

			consignee.SetRelatedParty(cntCartage, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.Containerised);

			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testJobDeclaration.JE_OH_Importer = consignee.PK;
			testJobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			testJobDeclaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;

			AssertEquals("Cartage", cntCartage, testJobDeclaration.DocsAndCartage.DeliveryCartageCo);
		}

		public void TestDefaultPickupCartageFromSupplier()
		{
			var supplier = Factory.New<OrgHeader>();

			var cntCartage = Factory.NewWithValidTestData<OrgHeader>();
			cntCartage.OH_IsShippingProvider = true;
			cntCartage.OH_IsLocalTransport = true;

			supplier.SetRelatedParty(cntCartage, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.Containerised);

			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			testJobDeclaration.JE_OH_Supplier = supplier.PK;
			testJobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			testJobDeclaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;

			AssertEquals("Cartage", cntCartage, testJobDeclaration.DocsAndCartage.PickupCartageCo);
		}

		#endregion

		#region Default Values

		public void TestDefaultOriginWhenSupplierGetsChanged()
		{
			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_RL_NKClosestPort = "USLAX";
			testJobDeclaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("Origin gets defaulted", supplier.OH_RL_NKClosestPort, testJobDeclaration.JE_RL_NKOrigin);

			OrgHeader supplierInDifferentPort = Factory.New<OrgHeader>();
			supplierInDifferentPort.OH_RL_NKClosestPort = "JPTKO";
			testJobDeclaration.JE_OH_Supplier = supplierInDifferentPort.PK;
			AssertEquals("Origin gets defaulted", supplierInDifferentPort.OH_RL_NKClosestPort, testJobDeclaration.JE_RL_NKOrigin);
		}

		#endregion

		#region Note Field Tests

		public void TestMarksAndNumbers()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			StmNote note = declaration.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			note.ST_NoteText = "SOME MARKS";
			AssertEquals("MarksAndNumbers", note.ST_NoteText, declaration.MarksAndNumbers);
		}

		public void TestMarksAndNumbersNoteDoesNotCauseErrorOnImportJob()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var note = declaration.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			note.ST_NoteText = "SOME MARKS";
			note.Validation.ValidateST_Description();
			AssertNoErrors("Note description does not error for Import", note.ST_DescriptionInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			note.ST_NoteText = "Marks now showing on Import dec - e.g. when Universal xml export shipment is used to create an Import shipment.";
			note.Validation.ValidateST_Description();
			AssertNoErrors("Note description should also not error when job is changed to Import", note.ST_DescriptionInfo);
		}

		#endregion

		#region CMR Tests

		public void TestIsTransportModeOther()
		{
			testJobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("IsOther", false, testJobDeclaration.IsTransportModeOther);

			testJobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Other;
			AssertEquals("IsOther", true, testJobDeclaration.IsTransportModeOther);
		}

		#endregion

		#region IDocumentSupportable Test
		public void TestBusinessContext()
		{
			JobDeclaration testDec = GetJobDeclarationWithTwoDummyCusEntryHeaders();
			AssertEquals(BusinessContext.Customs, testDec.DocumentSupporter.BusinessContext);
		}

		public void TestFilters()
		{
			JobDeclaration testDec = GetJobDeclarationWithTwoDummyCusEntryHeaders();
			testDec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			AssertEquals(Enterprise.Core.Constants.TransportModes.Sea, testDec.DocumentSupporter.GetFilterValue(DocumentFilters.MOD));

			testDec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			AssertEquals(Enterprise.Core.Constants.TransportModes.Air, testDec.DocumentSupporter.GetFilterValue(DocumentFilters.MOD));

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("EXPIMP", testDec.DocumentSupporter.GetFilterValue(DocumentFilters.MSC));

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			AssertEquals("OTH", testDec.DocumentSupporter.GetFilterValue(DocumentFilters.MSC));

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("EXPIMP", testDec.DocumentSupporter.GetFilterValue(DocumentFilters.MSC));
		}

		public void TestDocBusinessObject()
		{
			JobDeclaration testDec = GetJobDeclarationWithTwoDummyCusEntryHeaders();
			DocumentWrapper[] wrapper = testDec.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Declaration, null);
			AssertEquals(1, wrapper.Length);
			AssertEquals(typeof(JobDeclaration), wrapper[0].WrappedObject.GetType());

			wrapper = testDec.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Notes, null);
			AssertEquals(1, wrapper.Length);
			AssertEquals(typeof(JobDeclaration), wrapper[0].WrappedObject.GetType());

			wrapper = testDec.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ChargeSheet, null);
			AssertEquals(1, wrapper.Length);
			AssertEquals(typeof(JobDeclaration), wrapper[0].WrappedObject.GetType());

			testDec.Invoices.AddNew();
			testDec.Invoices.AddNew();
			wrapper = testDec.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CommercialInvoice, null);
			AssertEquals(2, wrapper.Length);
		}

		public void TestTransportMode()
		{
			JobDeclaration testDec = GetJobDeclarationWithTwoDummyCusEntryHeaders();
			testDec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("TransportMode", testDec.JE_TransportMode, testDec.TransportMode);
			AssertEquals("DocumentSupporter TransportMode", testDec.JE_TransportMode, testDec.DocumentSupporter.TransportMode);
		}

		public void TestLocalPort()
		{
			JobDeclaration testDec = GetJobDeclarationWithTwoDummyCusEntryHeaders();
			testDec.JE_RL_NKPortOfLoading = "USLAX";
			testDec.JE_RL_NKPortOfArrival = "AUSYD";

			AssertEquals("Local Port for CNR", "USLAX", testDec.DocumentSupporter.LocalPort(ContactType.Consignor, DocumentDirection.ANY));
			AssertEquals("Local Port for CNE", "AUSYD", testDec.DocumentSupporter.LocalPort(ContactType.Consignee, DocumentDirection.ANY));
			AssertEquals("Local Port for other type", "", testDec.DocumentSupporter.LocalPort(ContactType.Receivables, DocumentDirection.ANY));
		}

		public void TestForeignPort()
		{
			JobDeclaration testDec = GetJobDeclarationWithTwoDummyCusEntryHeaders();
			testDec.JE_RL_NKPortOfLoading = "USLAX";
			testDec.JE_RL_NKPortOfArrival = "AUSYD";

			AssertEquals("Foreign Port for CNR", "AUSYD", testDec.DocumentSupporter.ForeignPort(ContactType.Consignor, DocumentDirection.ANY));
			AssertEquals("Foreign Port for CNE", "USLAX", testDec.DocumentSupporter.ForeignPort(ContactType.Consignee, DocumentDirection.ANY));
			AssertEquals("Foreign Port for other type", "", testDec.DocumentSupporter.ForeignPort(ContactType.Receivables, DocumentDirection.ANY));
		}

		public void TestContactOrganisation()
		{
			var testDec = GetJobDeclarationWithTwoDummyCusEntryHeaders();
			testDec.JE_OH_Supplier = ZArchitecture.Core.Utilities.GetGuidFromRandomRowInTable(OrgHeader.Schema.PK, OrgHeader.Schema.TableName);
			testDec.JE_OH_Importer = ZArchitecture.Core.Utilities.GetGuidFromRandomRowInTable(OrgHeader.Schema.PK, OrgHeader.Schema.TableName);
			testDec.DeliveryOrPickupCartageCoPK = ZArchitecture.Core.Utilities.GetGuidFromRandomRowInTable(OrgHeader.Schema.PK, OrgHeader.Schema.TableName);

			var contact = testDec.DocumentSupporter.GetContactOrganisation("", ContactType.Consignor, DocumentDirection.ANY);
			string name = (Factory.Load<OrgHeader>(testDec.JE_OH_Supplier)).OH_FullName;
			AssertEquals(name, contact.OrgHeader.FullName);

			contact = testDec.DocumentSupporter.GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ANY);
			name = (Factory.Load<OrgHeader>(testDec.JE_OH_Importer)).OH_FullName;
			AssertEquals(name, contact.OrgHeader.FullName);

			contact = testDec.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ANY);
			name = (Factory.Load<OrgHeader>(testDec.DeliveryOrPickupCartageCoPK)).OH_FullName;
			AssertEquals(name, contact.OrgHeader.FullName);
		}

		public void TestReceivableTypeContactOrganisation()
		{
			var testDec = GetJobDeclarationWithTwoDummyCusEntryHeaders();

			testDec.JE_DeclarationReference = "X0001000";
			var job = Factory.NewJobForTesting<JobHeader>();
			var localCharge = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsActive, ZBool.True));
			job.LocalChargesPK = localCharge.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_JobNum = testDec.JE_DeclarationReference;
			job.JH_ParentTableCode = "JE";
			job.JH_ParentID = testDec.PK;

			var contact = testDec.DocumentSupporter.GetContactOrganisation("", ContactType.Receivables, DocumentDirection.ANY);
			AssertEquals(localCharge.OH_FullName.ToUpper(), contact.OrgHeader.FullName.ToUpper());
			AssertEquals("INV", testDec.DocumentSupporter.GetFilterValue(DocumentFilters.LGR));
		}

		public void TestImportSeaBLKCartageCompany()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			SetFCL_LCL_AIRCartageBizo();

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.Bulk;

			Guid fCLCartageGuid = FreightDataRegistry.Instance.FCLCartageCompany.Value;
			Guid lCLCartageGuid = FreightDataRegistry.Instance.LCLCartageCompany.Value;
			Guid aIRCartageGuid = FreightDataRegistry.Instance.AIRCartageCompany.Value;

			try
			{
				FreightDataRegistry.Instance.FCLCartageCompany.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, fCLCartage.PK.ToGuid());
				FreightDataRegistry.Instance.LCLCartageCompany.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, lCLCartage.PK.ToGuid());
				FreightDataRegistry.Instance.AIRCartageCompany.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, aIRCartage.PK.ToGuid());

				AssertNull("Cartage company should not default for Bulk.", declaration.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ANY).OrgHeader);
				AssertNull("Cartage company should not default for Bulk.", declaration.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ANY).OrgContact);
			}
			finally
			{
				FreightDataRegistry.Instance.FCLCartageCompany.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, fCLCartageGuid);
				FreightDataRegistry.Instance.LCLCartageCompany.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, lCLCartageGuid);
				FreightDataRegistry.Instance.AIRCartageCompany.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, aIRCartageGuid);
			}
		}

		public void TestImportSeaLQDCartageCompany()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			SetFCL_LCL_AIRCartageBizo();

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.Liquid;

			Guid fCLCartageGuid = FreightDataRegistry.Instance.FCLCartageCompany.Value;
			Guid lCLCartageGuid = FreightDataRegistry.Instance.LCLCartageCompany.Value;
			Guid aIRCartageGuid = FreightDataRegistry.Instance.AIRCartageCompany.Value;

			try
			{
				FreightDataRegistry.Instance.FCLCartageCompany.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, fCLCartage.PK.ToGuid());
				FreightDataRegistry.Instance.LCLCartageCompany.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, lCLCartage.PK.ToGuid());
				FreightDataRegistry.Instance.AIRCartageCompany.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, aIRCartage.PK.ToGuid());

				AssertNull("Cartage company should not default for Liquid.", declaration.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ANY).OrgHeader);
				AssertNull("Cartage company should not default for Liquid.", declaration.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ANY).OrgContact);
			}
			finally
			{
				FreightDataRegistry.Instance.FCLCartageCompany.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, fCLCartageGuid);
				FreightDataRegistry.Instance.LCLCartageCompany.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, lCLCartageGuid);
				FreightDataRegistry.Instance.AIRCartageCompany.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, aIRCartageGuid);
			}
		}

		public void TestImportSeaCNTCartageCompany()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			SetFCL_LCL_AIRCartageBizo();

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.Containerised;

			Guid fCLCartageGuid = FreightDataRegistry.Instance.FCLCartageCompany.Value;
			Guid lCLCartageGuid = FreightDataRegistry.Instance.LCLCartageCompany.Value;
			Guid aIRCartageGuid = FreightDataRegistry.Instance.AIRCartageCompany.Value;

			try
			{
				FreightDataRegistry.Instance.FCLCartageCompany.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, fCLCartage.PK.ToGuid());
				FreightDataRegistry.Instance.LCLCartageCompany.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, lCLCartage.PK.ToGuid());
				FreightDataRegistry.Instance.AIRCartageCompany.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, aIRCartage.PK.ToGuid());

				AssertEquals("Cartage company should be Registry's LCL Cartage", "LCL Pty Ltd.", declaration.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ANY).OrgHeader.FullName);

				declaration.CusContainers.AddNew().CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

				AssertEquals("Cartage company should be Registry's FCL Cartage", "ABC Pty Ltd.", declaration.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ANY).OrgHeader.FullName);

				declaration.CusContainers.RemoveAll();
				declaration.CusContainers.AddNew().CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
				declaration.CusContainers.AddNew().CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

				AssertEquals("Cartage company should be Registry's LCL Cartage", "LCL Pty Ltd.", declaration.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ANY).OrgHeader.FullName);
			}
			finally
			{
				FreightDataRegistry.Instance.FCLCartageCompany.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, fCLCartageGuid);
				FreightDataRegistry.Instance.LCLCartageCompany.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, lCLCartageGuid);
				FreightDataRegistry.Instance.AIRCartageCompany.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, aIRCartageGuid);
			}
		}

		#endregion

		#region IDocManagerSupport Test
		public void TestDocManagerCode()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			AssertEquals("Code should be DEC. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "DEC", ((IDocManagerSupport)testDec).DocManagerInfo.DocManagerCode);
		}
		#endregion

		#region IProcessQueueParent Test

		public void TestTablePrefix()
		{
			AssertEquals("Constant has recently been changed, TablePrefix needs to be updated", JobDeclarationSchema.Constants.Prefix, Factory.New<JobDeclaration>().TablePrefix);
		}

		public void TestCurrentQueue()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			ProcessQueueParentHelper processQueueParentHelper = new ProcessQueueParentHelper(declaration);
			AssertEquals(processQueueParentHelper.CurrentQueue.PK, declaration.CurrentQueue.PK);
			AssertEquals("Default type should be base ProcessQueue", typeof(ProcessQueue), declaration.CurrentQueue.GetType());
			Assert("Has to be a registered editable child object", declaration.IsRegisteredEditableChildObject(declaration.CurrentQueue));
		}

		public void TestActiveProcessQueueForBinding()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			ProcessQueueParentHelper processQueueParentHelper = new ProcessQueueParentHelper(declaration);
			AssertEquals("The collection should have 1 child", 1, declaration.ActiveProcessQueueForBinding.Count);
			AssertEquals("Child should be the CurrentQueue", declaration.CurrentQueue, declaration.ActiveProcessQueueForBinding[0].ProcessQueue);
		}

		public void TestProcessQueueDeletedOnDeclarationDelete()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			ProcessQueue processQueue = declaration.CurrentQueue;
			Assert("Record should not be deleted", !processQueue.IsDeleted);
			declaration.Delete();
			Assert("Record should be deleted", processQueue.IsDeleted);
		}

		#endregion

		#region AQIS Documents

		public void TestAQISDocuments()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			AssertEquals("No values in collection", 0, declaration.AQISDocuments.Count);

			AQISDocument document = declaration.AQISDocuments.AddNew();
			document.Number = "1234";
			document.Type = "Type";
			AssertEquals("One value in the collection", 1, declaration.AQISDocuments.Count);

			Factory.Save();
			AssertEquals("Add Info Value", "Type/1234", declaration.AddInfo.ZA_AQISDocuments_Hidden);
		}

		public void TestAQISDocumentsWithOneValueInAddInfo()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.AddInfo.ZA_AQISDocuments_Hidden = "TT1/Num1";
			AssertEquals("One value in the collection", 1, declaration.AQISDocuments.Count);

			AQISDocument document = declaration.AQISDocuments.AddNew();
			document.Type = "TT2";
			document.Number = "Num2";
			AssertEquals("Two values in the collection", 2, declaration.AQISDocuments.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISDocuments_Hidden.Contains("TT1/Num1"));
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISDocuments_Hidden.Contains("TT2/Num2"));
		}

		public void TestAQISDocumentsWithMultipleValuesInAddInfo()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.AddInfo.ZA_AQISDocuments_Hidden = "TT1/Num1,TT2/Num2";
			AssertEquals("One value in the collection", 2, declaration.AQISDocuments.Count);

			AQISDocument document = declaration.AQISDocuments.AddNew();
			document.Type = "TT3";
			document.Number = "Num3";
			AssertEquals("Two values in the collection", 3, declaration.AQISDocuments.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISDocuments_Hidden.Contains("TT1/Num1"));
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISDocuments_Hidden.Contains("TT2/Num2"));
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISDocuments_Hidden.Contains("TT3/Num3"));
		}

		#endregion

		#region AQIS Premises Id And Processing Types

		public void TestAQISPremisesIdAndProcessingTypes()
		{
			TestCaseHelper.ClearTable(CMRAqisPremises.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			AssertEquals("No values in collection", 0, declaration.AQISPremisesIdAndProcessingTypes.Count);

			AQISPremisesIdAndProcessingType premisesIdAndProcessingType = declaration.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesIdAndProcessingType.PremisesId = "Prem1";
			premisesIdAndProcessingType.ProcessingType = "Process";
			AssertEquals("One value in the collection", 1, declaration.AQISPremisesIdAndProcessingTypes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", "Prem1/Process", declaration.AddInfo.ZA_AQISPremIdProcessType_Hidden);
		}

		public void TestAQISPremisesIdAndProcessingTypesWithOneValueInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisPremises.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.AddInfo.ZA_AQISPremIdProcessType_Hidden = "Prem1/Process1";
			AssertEquals("One value in the collection", 1, declaration.AQISPremisesIdAndProcessingTypes.Count);

			AQISPremisesIdAndProcessingType premisesIdAndProcessingType = declaration.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesIdAndProcessingType.PremisesId = "Prem2";
			premisesIdAndProcessingType.ProcessingType = "Process2";
			AssertEquals("Two values in the collection", 2, declaration.AQISPremisesIdAndProcessingTypes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISPremIdProcessType_Hidden.Contains("Prem1/Process1"));
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISPremIdProcessType_Hidden.Contains("Prem2/Process2"));
		}

		public void TestAQISPremisesIdAndProcessingTypesWithMultipleValuesInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisPremises.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.AddInfo.ZA_AQISPremIdProcessType_Hidden = "Prem1/Process1,Prem2/Process2";
			AssertEquals("One value in the collection", 2, declaration.AQISPremisesIdAndProcessingTypes.Count);

			AQISPremisesIdAndProcessingType premisesIdAndProcessingType = declaration.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesIdAndProcessingType.PremisesId = "Prem3";
			premisesIdAndProcessingType.ProcessingType = "Process3";
			AssertEquals("Two values in the collection", 3, declaration.AQISPremisesIdAndProcessingTypes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISPremIdProcessType_Hidden.Contains("Prem1/Process1"));
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISPremIdProcessType_Hidden.Contains("Prem2/Process2"));
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISPremIdProcessType_Hidden.Contains("Prem3/Process3"));
		}

		#endregion

		#region AQIS Commodity Codes

		public void TestAQISCommodityCodes()
		{
			TestCaseHelper.ClearTable(CMRAqisCommodity.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			AssertEquals("No values in collection", 0, declaration.AQISCommodityCodes.Count);

			AQISCommodityCode commodityCode = declaration.AQISCommodityCodes.AddNew();
			commodityCode.Code = "1";
			AssertEquals("One value in the collection", 1, declaration.AQISCommodityCodes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", "1", declaration.AddInfo.ZA_AQISCommCodes_Hidden);
		}

		public void TestAQISCommodityCodesWithOneValueInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisCommodity.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.AddInfo.ZA_AQISCommCodes_Hidden = "1";
			AssertEquals("One value in the collection", 1, declaration.AQISCommodityCodes.Count);

			AQISCommodityCode commodityCode = declaration.AQISCommodityCodes.AddNew();
			commodityCode.Code = "2";
			AssertEquals("Two values in the collection", 2, declaration.AQISCommodityCodes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISCommCodes_Hidden.Contains("1"));
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISCommCodes_Hidden.Contains("2"));
		}

		public void TestAQISCommodityCodesWithMultipleValuesInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisCommodity.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.AddInfo.ZA_AQISCommCodes_Hidden = "1,2";
			AssertEquals("One value in the collection", 2, declaration.AQISCommodityCodes.Count);

			AQISCommodityCode commodityCode = declaration.AQISCommodityCodes.AddNew();
			commodityCode.Code = "3";
			AssertEquals("Two values in the collection", 3, declaration.AQISCommodityCodes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISCommCodes_Hidden.Contains("1"));
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISCommCodes_Hidden.Contains("2"));
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISCommCodes_Hidden.Contains("3"));
		}

		public void TestReBuildAQISCommodityCodes()
		{
			TestCaseHelper.ClearTable(CMRAqisCommodity.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.AddInfo.ZA_AQISCommCodes_Hidden = "1,2";
			AssertEquals("One value in the collection", 2, declaration.AQISCommodityCodes.Count);

			AQISCommodityCode commodityCode = declaration.AQISCommodityCodes.AddNew();
			commodityCode.Code = "3";
			declaration.AddInfo.ReBuildAQISCommodityCodes();
			AssertEquals("Add Info Value", "1,2,3", declaration.AddInfo.ZA_AQISCommCodes_Hidden);
		}

		#endregion

		#region AQIS Entity Ids

		public void TestAQISEntityIds()
		{
			TestCaseHelper.ClearTable(CMRAqisEntity.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			AssertEquals("No values in collection", 0, declaration.AQISEntityIds.Count);

			AQISEntityId entityId = declaration.AQISEntityIds.AddNew();
			entityId.Code = "Code1";
			AssertEquals("One value in the collection", 1, declaration.AQISEntityIds.Count);

			Factory.Save();
			AssertEquals("Add Info Value", "Code1", declaration.AddInfo.ZA_AQISEntityIds_Hidden);
		}

		public void TestAQISEntityIdsWithOneValueInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisEntity.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.AddInfo.ZA_AQISEntityIds_Hidden = "Code1";
			AssertEquals("One value in the collection", 1, declaration.AQISEntityIds.Count);

			AQISEntityId entityId = declaration.AQISEntityIds.AddNew();
			entityId.Code = "Code2";
			AssertEquals("Two values in the collection", 2, declaration.AQISEntityIds.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISEntityIds_Hidden.Contains("Code1"));
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISEntityIds_Hidden.Contains("Code2"));
		}

		public void TestAQISEntityIdsWithMultipleValuesInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisEntity.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.AddInfo.ZA_AQISEntityIds_Hidden = "Code1,Code2";
			AssertEquals("One value in the collection", 2, declaration.AQISEntityIds.Count);

			AQISEntityId entityId = declaration.AQISEntityIds.AddNew();
			entityId.Code = "Code3";
			AssertEquals("Two values in the collection", 3, declaration.AQISEntityIds.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISEntityIds_Hidden.Contains("Code1"));
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISEntityIds_Hidden.Contains("Code2"));
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISEntityIds_Hidden.Contains("Code3"));
		}

		public void TestReBuildAQISEntityIds()
		{
			TestCaseHelper.ClearTable(CMRAqisEntity.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.AddInfo.ZA_AQISEntityIds_Hidden = "Code1,Code2";
			AssertEquals("One value in the collection", 2, declaration.AQISEntityIds.Count);

			AQISEntityId entityId = declaration.AQISEntityIds.AddNew();
			entityId.Code = "Code3";
			declaration.AddInfo.ReBuildAQISEntityIds();
			AssertEquals("Add Info Value", "Code1,Code2,Code3", declaration.AddInfo.ZA_AQISEntityIds_Hidden);
		}

		public void TestToOrder_ReadOnly()
		{
			AUCustomsDataRegistry.Instance.EnableAQISDeclarationMessaging.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			AssertEquals("'To order' field is editable.", false, declaration.JE_ToOrderInfo.ReadOnly);

			declaration.JE_OverrideFreightDefaults = true;
			AssertEquals("'To order' field is editable.", false, declaration.JE_ToOrderInfo.ReadOnly);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
			declaration.JE_OverrideFreightDefaults = false;

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CNRTEST";
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CNETEST";
			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			declaration.JE_JS = shipment.PK;
			AssertEquals("'To order' field is readonly.", true, declaration.JE_ToOrderInfo.ReadOnly);

			declaration.JE_OverrideFreightDefaults = true;
			AssertEquals("'To order' field is editable.", false, declaration.JE_ToOrder_ReadOnly);

			declaration.JE_ToOrder = true;
			AssertNull("Importer is empty.", declaration.Importer);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
			declaration.JE_OverrideFreightDefaults = false;

			AssertEquals("'To order' field is readonly.", true, declaration.JE_ToOrderInfo.ReadOnly);
			AssertEquals("'To order' field is not ticked.", false, declaration.JE_ToOrder);
			AssertEquals("Importer is not empty.", consignee.PK, declaration.Importer.PK);
		}

		#endregion

		#region AQIS Permit Ids

		public void TestAQISPermitIds()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			AssertEquals("No values in collection", 0, declaration.AQISPermitIds.Count);

			AQISPermitId permitId = declaration.AQISPermitIds.AddNew();
			permitId.Code = "Code1";
			AssertEquals("One value in the collection", 1, declaration.AQISPermitIds.Count);

			Factory.Save();
			AssertEquals("Add Info Value", "Code1", declaration.AddInfo.ZA_AQISPermitIds_Hidden);
		}

		public void TestAQISPermitIdsWithOneValueInAddInfo()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.AddInfo.ZA_AQISPermitIds_Hidden = "Code1";
			AssertEquals("One value in the collection", 1, declaration.AQISPermitIds.Count);

			AQISPermitId permitId = declaration.AQISPermitIds.AddNew();
			permitId.Code = "Code2";
			AssertEquals("Two values in the collection", 2, declaration.AQISPermitIds.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISPermitIds_Hidden.Contains("Code1"));
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISPermitIds_Hidden.Contains("Code2"));
		}

		public void TestAQISPermitIdsWithMultipleValuesInAddInfo()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.AddInfo.ZA_AQISPermitIds_Hidden = "Code1,Code2";
			AssertEquals("One value in the collection", 2, declaration.AQISPermitIds.Count);

			AQISPermitId permitId = declaration.AQISPermitIds.AddNew();
			permitId.Code = "Code3";
			AssertEquals("Two values in the collection", 3, declaration.AQISPermitIds.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISPermitIds_Hidden.Contains("Code1"));
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISPermitIds_Hidden.Contains("Code2"));
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISPermitIds_Hidden.Contains("Code3"));
		}

		public void TestReBuildAQISPermitIds()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.AddInfo.ZA_AQISPermitIds_Hidden = "Code1,Code2";
			AssertEquals("One value in the collection", 2, declaration.AQISPermitIds.Count);

			AQISPermitId permitId = declaration.AQISPermitIds.AddNew();
			permitId.Code = "Code3";
			declaration.AddInfo.ReBuildAQISPermitIds();
			AssertEquals("Add Info Value", "Code1,Code2,Code3", declaration.AddInfo.ZA_AQISPermitIds_Hidden);
		}

		#endregion

		#region AQIS Producer Codes

		public void TestAQISProducerCodes()
		{
			TestCaseHelper.ClearTable(CMRAqisProducer.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			AssertEquals("No values in collection", 0, declaration.AQISProducerCodes.Count);

			AQISProducerCode producerCode = declaration.AQISProducerCodes.AddNew();
			producerCode.Code = "Code1";
			AssertEquals("One value in the collection", 1, declaration.AQISProducerCodes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", "Code1", declaration.AddInfo.ZA_AQISProducerCodes_Hidden);
		}

		public void TestAQISProducerCodesWithOneValueInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisProducer.Schema.TableName);

			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "Code1";
			AssertEquals("One value in the collection", 1, declaration.AQISProducerCodes.Count);

			AQISProducerCode producerCode = declaration.AQISProducerCodes.AddNew();
			producerCode.Code = "Code2";
			AssertEquals("Two values in the collection", 2, declaration.AQISProducerCodes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISProducerCodes_Hidden.Contains("Code1"));
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISProducerCodes_Hidden.Contains("Code2"));
		}

		public void TestAQISProducerCodesWithMultipleValuesInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisProducer.Schema.TableName);

			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "Code1,Code2";
			AssertEquals("One value in the collection", 2, declaration.AQISProducerCodes.Count);

			AQISProducerCode producerCode = declaration.AQISProducerCodes.AddNew();
			producerCode.Code = "Code3";
			AssertEquals("Two values in the collection", 3, declaration.AQISProducerCodes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISProducerCodes_Hidden.Contains("Code1"));
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISProducerCodes_Hidden.Contains("Code2"));
			AssertEquals("Add Info Value", true, declaration.AddInfo.ZA_AQISProducerCodes_Hidden.Contains("Code3"));
		}

		public void TestReBuildAQISProcducerCode()
		{
			TestCaseHelper.ClearTable(CMRAqisProducer.Schema.TableName);

			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "Code1,Code2";
			AssertEquals("One value in the collection", 2, declaration.AQISProducerCodes.Count);

			AQISProducerCode producerCode = declaration.AQISProducerCodes.AddNew();
			producerCode.Code = "Code3";
			declaration.AddInfo.ReBuildAQISProducerCodes();
			AssertEquals("Add Info Value", "Code1,Code2,Code3", declaration.AddInfo.ZA_AQISProducerCodes_Hidden);
		}

		#endregion

		#region QueuedEntry

		[TestDate(2023, 09, 04)]
		public void TestJE_EDITransmitDate()
		{
			var declaration = JobDeclaration.New(Factory);
			AssertEquals(ZDateTime.Empty, declaration.JE_EDITransmitDate);

			declaration.JE_EDITransmitDate = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, declaration.JE_EDITransmitDate);
		}

		public void TestJE_EDITransmitDate_ReadOnly()
		{
			var declaration = JobDeclaration.New(Factory);
			AssertEquals("Editable without an Entry Header", false, declaration.JE_EDITransmitDate_ReadOnly);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("Precondition - expected default value", CustomsEntryStatus.NotSent.Code, entryHeader.CH_Status);
			AssertEquals("Editable with an Entry Header and no sent messages", false, declaration.JE_EDITransmitDate_ReadOnly);

			entryHeader.CH_Status = CustomsEntryStatus.ScheduledLodgeWithPayment.Code;
			AssertEquals("Readonly with an Entry Header and scheduled message (Paid)", true, declaration.JE_EDITransmitDate_ReadOnly);
			entryHeader.CH_Status = CustomsEntryStatus.ScheduledLodgeWithoutPayment.Code;
			AssertEquals("Readonly with an Entry Header and scheduled message (Not Paid)", true, declaration.JE_EDITransmitDate_ReadOnly);
			entryHeader.CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;
			AssertEquals("Readonly with an Entry Header and sent message", true, declaration.JE_EDITransmitDate_ReadOnly);
			entryHeader.CH_Status = CustomsEntryStatus.FailFormalLodge.Code;
			AssertEquals("Editable with an Entry Header and failed message", false, declaration.JE_EDITransmitDate_ReadOnly);
		}

		[TestDate(2023, 09, 04)]
		public void TestJE_MessageStatusDescription_ScheduledLodgement()
		{
			testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testJobDeclaration.CustomsEntryHeaders.AddNew();
			testJobDeclaration.JE_EDITransmitDate = ZDateTime.Today.AddDays(1);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				testJobDeclaration.JE_MessageStatus = CustomsEntryStatus.ScheduledLodgeWithPayment.Code;
				AssertEquals("Not Empty", "Scheduled Lodge with payment message to be sent 05-Sep-23", testJobDeclaration.JE_MessageStatusDescription);
				testJobDeclaration.JE_MessageStatus = CustomsEntryStatus.ScheduledLodgeWithoutPayment.Code;
				AssertEquals("Not Empty", "Scheduled Lodge without payment message to be sent 05-Sep-23", testJobDeclaration.JE_MessageStatusDescription);
			}
		}

		public void TestDequeueScheduledMessages()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			declaration.JE_MessageType = AUJobMessageTypeList.Codes.Import;

			declaration.Logs.CreateOrRecreateEventLog(new EventValue(AutoEvents.DeferredScheduledMessage, reference: nameof(CMRMessageTypes.LodgeWithPay), eventTime: ZDateTimeOffset.Today.AddDays(5), isEstimate: false));
			declaration.JE_MessageStatus = entryHeader.CH_Status = CustomsEntryStatus.ScheduledLodgeWithPayment.Code;
			AssertEquals("Pre-condition: declaration.ReadOnly", true, declaration.ReadOnly);

			declaration.DequeueScheduledMessages();

			AssertEquals("declaration.ReadOnly", false, declaration.ReadOnly);
			AssertEquals("LodgeWithPay DSM event is cancelled", true, declaration.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.DeferredScheduledMessageCode).All(x => x.SL_IsCancelled));
			AssertEquals("entryHeader.CH_Status", CustomsEntryStatus.NotSent.Code, entryHeader.CH_Status);
			AssertEquals("declaration.JE_MessageStatus", CustomsEntryStatus.NotSent.Code, declaration.JE_MessageStatus);

			declaration.Logs.CreateOrRecreateEventLog(new EventValue(AutoEvents.DeferredScheduledMessage, reference: nameof(CMRMessageTypes.LodgeWithoutPay), eventTime: ZDateTimeOffset.Today.AddDays(5), isEstimate: false));
			declaration.JE_MessageStatus = entryHeader.CH_Status = CustomsEntryStatus.ScheduledLodgeWithoutPayment.Code;
			AssertEquals("Pre-condition: declaration.ReadOnly", true, declaration.ReadOnly);

			declaration.DequeueScheduledMessages();

			AssertEquals("declaration.ReadOnly", false, declaration.ReadOnly);
			AssertEquals("LodgeWithoutPay DSM event is cancelled", true, declaration.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.DeferredScheduledMessageCode).All(x => x.SL_IsCancelled));
			AssertEquals("entryHeader.CH_Status", CustomsEntryStatus.NotSent.Code, entryHeader.CH_Status);
			AssertEquals("declaration.JE_MessageStatus", CustomsEntryStatus.NotSent.Code, declaration.JE_MessageStatus);

			declaration.Logs.CreateOrRecreateEventLog(new EventValue(AutoEvents.DeferredScheduledMessage, reference: nameof(CMRMessageTypes.Payment), eventTime: ZDateTimeOffset.Today.AddDays(5), isEstimate: false));
			declaration.JE_MessageStatus = entryHeader.CH_Status = CustomsEntryStatus.ScheduledPayment.Code;
			AssertEquals("Pre-condition: declaration.ReadOnly", true, declaration.ReadOnly);

			declaration.DequeueScheduledMessages();

			AssertEquals("declaration.ReadOnly", false, declaration.ReadOnly);
			AssertEquals("Payment DSM event is cancelled", true, declaration.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.DeferredScheduledMessageCode).All(x => x.SL_IsCancelled));
			AssertEquals("entryHeader.CH_Status", CustomsEntryStatus.NotSent.Code, entryHeader.CH_Status);
			AssertEquals("declaration.JE_MessageStatus", CustomsEntryStatus.NotSent.Code, declaration.JE_MessageStatus);
		}

		public void TestThrowAwayMergeClearsScheduledMessages()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = AUJobMessageTypeList.Codes.Import;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			declaration.Logs.CreateOrRecreateEventLog(new EventValue(AutoEvents.DeferredScheduledMessage, reference: nameof(CMRMessageTypes.LodgeWithPay), eventTime: ZDateTimeOffset.Today.AddDays(5), isEstimate: false));
			declaration.JE_MessageStatus = entryHeader.CH_Status = CustomsEntryStatus.ScheduledLodgeWithPayment.Code;
			AssertEquals("Pre-condition: declaration.ReadOnly", true, declaration.ReadOnly);

			declaration.ThrowAwayMerge();

			AssertEquals("declaration.ReadOnly", false, declaration.ReadOnly);
			AssertEquals("LodgeWithPay DSM event is cancelled", true, declaration.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.DeferredScheduledMessageCode).All(x => x.SL_IsCancelled));
			AssertEquals("entryHeader deleted", 0, declaration.ActiveEntryHeaders.Count);
			AssertEquals("declaration.JE_MessageStatus", CustomsEntryStatus.NotSent.Code, declaration.JE_MessageStatus);

			entryHeader = declaration.ActiveEntryHeaders.AddNew();
			declaration.Logs.CreateOrRecreateEventLog(new EventValue(AutoEvents.DeferredScheduledMessage, reference: nameof(CMRMessageTypes.LodgeWithoutPay), eventTime: ZDateTimeOffset.Today.AddDays(5), isEstimate: false));
			declaration.JE_MessageStatus = entryHeader.CH_Status = CustomsEntryStatus.ScheduledLodgeWithoutPayment.Code;
			AssertEquals("Pre-condition: declaration.ReadOnly", true, declaration.ReadOnly);

			declaration.ThrowAwayMerge();

			AssertEquals("declaration.ReadOnly", false, declaration.ReadOnly);
			AssertEquals("LodgeWithoutPay DSM event is cancelled", true, declaration.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.DeferredScheduledMessageCode).All(x => x.SL_IsCancelled));
			AssertEquals("entryHeader deleted", 0, declaration.ActiveEntryHeaders.Count);
			AssertEquals("declaration.JE_MessageStatus", CustomsEntryStatus.NotSent.Code, declaration.JE_MessageStatus);

			entryHeader = declaration.ActiveEntryHeaders.AddNew();
			declaration.Logs.CreateOrRecreateEventLog(new EventValue(AutoEvents.DeferredScheduledMessage, reference: nameof(CMRMessageTypes.Payment), eventTime: ZDateTimeOffset.Today.AddDays(5), isEstimate: false));
			declaration.JE_MessageStatus = entryHeader.CH_Status = CustomsEntryStatus.ScheduledPayment.Code;
			AssertEquals("Pre-condition: declaration.ReadOnly", true, declaration.ReadOnly);

			declaration.ThrowAwayMerge();

			AssertEquals("declaration.ReadOnly", false, declaration.ReadOnly);
			AssertEquals("Payment DSM event is cancelled", true, declaration.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.DeferredScheduledMessageCode).All(x => x.SL_IsCancelled));
			AssertEquals("entryHeader deleted", 0, declaration.ActiveEntryHeaders.Count);
			AssertEquals("declaration.JE_MessageStatus", CustomsEntryStatus.NotSent.Code, declaration.JE_MessageStatus);
		}

		#endregion

		#region QueuedPayment

		[TestDate(2023, 12, 12, 12, 12, 0)]
		public void TestJE_MessageStatusDescription_ScheduledPayment()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				var testCusEntryHeader = testJobDeclaration.CustomsEntryHeaders.AddNew();
				testCusEntryHeader.ScheduledPaymentDate = ZDateTime.Now.AddDays(1);
				testCusEntryHeader.CH_Status = CustomsEntryStatus.ScheduledPayment.Code;
				testJobDeclaration.JE_MessageStatus = CustomsEntryStatus.ScheduledPayment.Code;
				AssertEquals("Not Empty", "Scheduled Payment to be sent 13 Dec 2023 12:12", testJobDeclaration.JE_MessageStatusDescription);
			}
		}

		[TestDate(2023, 12, 12, 12, 12, 0)]
		public void TestDeclarationReadOnly_ScheduledPayment()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				var testCusEntryHeader = testJobDeclaration.CustomsEntryHeaders.AddNew();
				testCusEntryHeader.ScheduledPaymentDate = ZDateTime.Now.AddDays(1);
				testJobDeclaration.JE_MessageStatus = CustomsEntryStatus.ScheduledPayment.Code;
				AssertEquals("Readonly with an Entry Header and scheduled payment message", true, testJobDeclaration.ReadOnly);
			}
		}
		#endregion

		#region IDocManagerSupport Test

		public void TestReadOnlyForStandAloneJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.PlaceHold("TEST IDocManagerSupport");
			AssertEquals("DocManagerInfo should be editable when the parent is a stand-alone declaration", false, ((IDocManagerSupport)declaration).DocManagerInfo.ReadOnly);

			declaration.RemoveHold();
			AssertEquals("DocManagerInfo should be editable when the parent is a stand-alone declaration", false, ((IDocManagerSupport)declaration).DocManagerInfo.ReadOnly);
		}

		#endregion

		public void TestBusinessObjectsWithRelatedEvents()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "PMT05627";
			Factory.Save();

			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			colsHeader.QCH_ClusterKey = declaration.JE_ClusterKey;
			AssertEquals(2, declaration.BusinessObjectsWithRelatedEvents.Length);
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { entryHeader, colsHeader }, declaration.BusinessObjectsWithRelatedEvents);
		}

		#region Implementation
		JobDeclaration testJobDeclaration;
		RefCurrency aUDCurrency;
		RefCurrency uSDCurrency;

		protected override void SetUp()
		{
			base.SetUp();
			testJobDeclaration = (JobDeclaration)GetNewBusinessObject();
			testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testJobDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			aUDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			uSDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.New<JobDeclaration>();
		}

		ZGuid CreateAndSaveTestRecord()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			JobDeclaration declarationToSave = factory.New<JobDeclaration>();
			declarationToSave.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declarationToSave.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_InvoiceAmount = 111.00m;
			factory.Save();
			return declarationToSave.PK;
		}

		public static JobDeclaration SetUpImportDec(BusinessObjectFactory factory)
		{
			string currency = "AUD";
			OrgHeader supplier = OrgHeader.LoadFromCode(factory, "ABABEU");
			supplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.SupplierCode, "955411R");

			supplier.MainAddress.OA_Address1 = "asdasdqweqwe";
			supplier.OH_FullName = "asdasdqwesd";
			supplier.OH_Code = "a2s4d5";

			JobDeclaration declaration = factory.New<JobDeclaration>();
			declaration.AutoCreateChargesBasedOnIncoTerm = false;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			using (declaration.GetValidationSuspender())
			{
				OrgHeader importer = factory.New<OrgHeader>();
				declaration.JE_OH_Importer = importer.PK;
				importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = OrgConstants.MergeInvoiceLines.Tariff;
				importer.MainAddress.OA_Address1 = "asdasd";
				importer.OH_FullName = "asdasd";
				importer.OH_Code = "a5sd42";
				declaration.Importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.GSTCode, "90093519530");

				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				declaration.JE_RL_NKFinalDestination = "AUBNE";
				declaration.JE_RL_NKPortOfLoading = "HKHKG";
				declaration.JE_RL_NKPortOfArrival = "AUBNE";
				declaration.JE_RL_NKPortOfFirstArrival = "AUBNE";
				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;

				declaration.AddInfo.ZA_PrinterNumber_Hidden = "11273";
				declaration.JE_DateOfArrival = new ZDateTime(2005, 10, 1);          // ZDateTime.Today.AddDays(3);
				declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 10, 1);     // ZDateTime.Today.AddDays(3);
				declaration.JE_ExportDate = ZDateTime.Today;
				declaration.AddInfo.ZA_NumberOfEntryPrints_Hidden = 2;
				declaration.JE_HouseBill = "0866548910428539";
				declaration.JE_VoyageFlightNo = "NZ123";
				declaration.JE_TotalNoOfPacks = 63;
				declaration.JE_OwnerRef = ".";
				declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

				JobComInvoiceHeader header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				header.AddInfo.ZA_ValuationBasis_Hidden = "UT";

				header.JZ_Nature10PackCount = 63;
				JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();

				header.JZ_RX_NKInvoice_Currency = currency;
				header.JZ_InvoiceAmount = new ZDecimal(8525.70);
				header.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 2481.70m);
				header.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 21.31m);

				header.JZ_Weight = 1;
				header.JZ_WeightUQ = "KG";
				header.JZ_OH_Supplier = supplier.PK;
				header.JZ_InvoiceNumber = "1";
				header.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;

				header.AddInfo.ZA_ORG = "HK";

				line.JI_Tariff = "9101.11.00 36";
				line.JI_Description = "WATCHES";
				line.JI_Weight = new ZDecimal(25.00);
				line.JI_WeightUQ = "KG";
				line.JI_Weight = new ZDecimal(500.00);
				line.JI_LinePrice = new ZDecimal(8000);
				line.JI_InvoiceQuantity = new ZDecimal(6000);
				line.JI_InvoiceUQ = "NO";

				JobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();
				line2.JI_Weight = new ZDecimal(30.00);
				line2.JI_WeightUQ = "KG";
				line2.JI_LinePrice = new ZDecimal(525.70);

				line2.JI_Tariff = "9101.11.00 36";
				line2.JI_Description = "WATCHES";
				line2.JI_InvoiceQuantity = new ZDecimal(264);
				line2.JI_InvoiceUQ = "NO";

				//balance invoice
				header.JZ_InvoiceAmount = header.JZ_InvoiceAmount - header.JZ_Calc_Balance;
			}
			return declaration;
		}

		public static JobDeclaration SetUpAndMergeImportDec(BusinessObjectFactory factory)
		{
			JobDeclaration declaration = SetUpImportDec(factory);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			return declaration;
		}

		public static JobDeclaration SetUpImportDecWithCPDecsAnswered(BusinessObjectFactory factory, string entryStatus)
		{
			JobDeclaration declaration = SetUpAndMergeImportDec(factory);

			declaration.CustomsEntryHeaders[0].EntryNumber = "ICJ-01-1309/257";
			declaration.JE_EntryStatus = entryStatus;

			return declaration;
		}

		public static ZGuid SetUpAndSaveImportDecWithCPDecsAnswered(string entryStatus)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			JobDeclaration declaration = SetUpImportDecWithCPDecsAnswered(factory, entryStatus);
			factory.Save();
			return declaration.PK;
		}

		OrgHeader fCLCartage;
		OrgHeader lCLCartage;
		OrgHeader aIRCartage;

		void SetFCL_LCL_AIRCartageBizo()
		{
			fCLCartage = Factory.New<OrgHeader>();
			fCLCartage.OH_FullName = "ABC Pty Ltd.";
			fCLCartage.MainAddress.OA_Address1 = "ABC Street";
			fCLCartage.OH_Code = "ABCPTY";

			lCLCartage = Factory.New<OrgHeader>();
			lCLCartage.OH_FullName = "LCL Pty Ltd.";
			lCLCartage.MainAddress.OA_Address1 = "LCL Street";
			lCLCartage.OH_Code = "LCLPTY";

			aIRCartage = Factory.New<OrgHeader>();
			aIRCartage.OH_FullName = "Airway Pty Ltd.";
			aIRCartage.MainAddress.OA_Address1 = "Air Street";
			aIRCartage.OH_Code = "AIRPTY";

			Factory.Save();
		}

		ZGuid SetUpAndSaveDeclaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_RL_NKFinalDestination = "NZAKL";
			declaration.JE_RL_NKOrigin = "AUMEL";

			declaration.JE_DeclarationReference = "EFJ-01-804508";
			declaration.DeclarationNumber = "2M032681902UAE";

			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.GSTCode, "62004191324");
			supplier.MainAddress.OA_Phone = "9299 8865";
			supplier.MainAddress.OA_Address1 = "address";
			supplier.OH_FullName = "full name";
			supplier.OH_Code = "xTEMP01";
			declaration.JE_OH_Supplier = supplier.PK;

			OrgHeader importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "CARTER HOLT HARVEY";
			importer.MainAddress.OA_City = "AUCKLAND";
			importer.MainAddress.OA_Address1 = "address";
			importer.OH_Code = "xTEMP02";
			declaration.JE_OH_Importer = importer.PK;

			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			declaration.JE_ExportDate = new DateTime(2003, 10, 06);

			var vessel = declaration.Factory.LoadTop1<RefVessel>(new ZQuery());
			declaration.JE_VesselName = vessel.RV_Code;

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;

			JobComInvoiceHeader myHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine myLine = myHeader.JobComInvoiceLines.AddNew();

			myLine.JI_Tariff = "48181000";
			myLine.JI_AUState = "VIC";
			myLine.JI_Weight = new ZDecimal(120000.0);
			myLine.JI_WeightUQ = "KG";
			myLine.JI_CustomsQuantity = 100000;
			myLine.JI_CustomsUnitQty = "T";

			myLine.JI_Description = "TOILET PAPER";

			myLine.JI_LinePrice = new ZDecimal(100000.00);

			myHeader.JZ_InvoiceAmount = new ZDecimal(100000);
			myHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			declaration.JE_TotalNoOfPacks = 0;
			declaration.JE_ContainerCount = 4;

			ZGuid pK = declaration.PK;
			Factory.Save();
			return pK;
		}

		ZGuid SetUpAndSaveDeclarationWithAddInfoSet()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			declaration.JE_RL_NKFinalDestination = "NZAKL";
			declaration.JE_RL_NKOrigin = "AUMEL";

			declaration.JE_DeclarationReference = "EFJ-01-804508";
			declaration.DeclarationNumber = "2M032681902UAE";

			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.GSTCode, "62004191324");
			supplier.MainAddress.OA_Phone = "9299 8865";
			supplier.MainAddress.OA_Address1 = "address";
			supplier.OH_FullName = "full name";
			supplier.OH_Code = "xTEMP01";
			declaration.JE_OH_Supplier = supplier.PK;

			OrgHeader importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "CARTER HOLT HARVEY";
			importer.MainAddress.OA_City = "AUCKLAND";
			importer.MainAddress.OA_Address1 = "address";
			importer.OH_Code = "xTEMP02";
			declaration.JE_OH_Importer = importer.PK;

			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			declaration.JE_ExportDate = new DateTime(2003, 10, 06);

			var vessel = declaration.Factory.LoadTop1<RefVessel>(new ZQuery());
			declaration.JE_VesselName = vessel.RV_Code;

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;

			declaration.AddInfo.ZA_NumberOfEntryPrints_Hidden = 10;
			JobComInvoiceHeader myHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine myLine = myHeader.JobComInvoiceLines.AddNew();

			myLine.JI_Tariff = "48181000";
			myLine.JI_AUState = "VIC";
			myLine.JI_Weight = new ZDecimal(120000.0);
			myLine.JI_WeightUQ = "KG";
			myLine.JI_CustomsQuantity = 100000;
			myLine.JI_CustomsUnitQty = "T";
			AQISProducerCode aQISProducerCode = myLine.AQISProducerCodes.AddNew();
			aQISProducerCode.Code = "FRED";
			myLine.JI_Description = "TOILET PAPER";
			myLine.JI_LinePrice = new ZDecimal(100000.00);

			myHeader.JZ_InvoiceAmount = new ZDecimal(100000);
			myHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			declaration.JE_TotalNoOfPacks = 0;
			declaration.JE_ContainerCount = 4;

			ZGuid pK = declaration.PK;
			Factory.Save();
			return pK;
		}

		JobComInvoiceLine CreateInvoiceLine(JobComInvoiceHeader header,
			ZString addInfo,
			ZDecimal customsQuantity,
			ZString customsUQ,
			ZString description,
			ZDecimal invoiceQuantity,
			ZString invoiceUQ,
			ZShort lineNo,
			ZDecimal linePrice,
			ZString origin,
			ZString tariff,
			ZDecimal weight,
			ZString weightUQ)
		{
			JobComInvoiceLine result = header.JobComInvoiceLines.AddNew();
			using (result.GetValidationSuspender())
			{
				result.JI_AddInfo = addInfo;
				result.JI_CustomsQuantity = 2.0000m;
				result.JI_CustomsUnitQty = customsUQ;
				result.JI_Description = description;
				result.JI_InvoiceQuantity = invoiceQuantity;
				result.JI_InvoiceUQ = invoiceUQ;
				result.JI_LineNo = lineNo;
				result.JI_LinePrice = linePrice;
				result.JI_CountryOfOrigin = origin;
				result.JI_Tariff = tariff;
				result.JI_Weight = weight;
				result.JI_WeightUQ = weightUQ;
			}
			return result;
		}

		#region CreateInvoiceLine overrides

		JobComInvoiceLine CreateInvoiceLine(JobComInvoiceHeader header,
			ZString addInfo,
			ZString description,
			ZShort lineNo,
			ZDecimal linePrice,
			ZString origin,
			ZString tariff,
			ZString weightUQ)
		{
			return CreateInvoiceLine(header, addInfo, 0.0m, "", description, 0, "", lineNo, linePrice, origin, tariff, 0.0m, weightUQ);
		}

		JobComInvoiceLine CreateInvoiceLine(JobComInvoiceHeader header,
			ZString addInfo,
			ZDecimal customsQuantity,
			ZString description,
			ZDecimal invoiceQuantity,
			ZString invoiceUQ,
			ZShort lineNo,
			ZDecimal linePrice,
			ZString origin,
			ZString tariff,
			ZDecimal weight,
			ZString weightUQ)
		{
			return CreateInvoiceLine(header, addInfo, customsQuantity, "", description, invoiceQuantity, invoiceUQ, lineNo, linePrice, origin, tariff, weight, weightUQ);
		}

		JobComInvoiceLine CreateInvoiceLine(JobComInvoiceHeader header,
			ZDecimal customsQuantity,
			ZString customsUQ,
			ZString description,
			ZDecimal invoiceQuantity,
			ZString invoiceUQ,
			ZShort lineNo,
			ZDecimal linePrice,
			ZString tariff,
			ZDecimal weight,
			ZString weightUQ)
		{
			return CreateInvoiceLine(header, "", customsQuantity, customsUQ, description, invoiceQuantity, invoiceUQ, lineNo, linePrice, "", tariff, weight, weightUQ);
		}

		#endregion

		public static JobDeclaration SetUpContainerisedImportDeclaration(BusinessObjectFactory factory)
		{
			OrgHeader supplier = OrgHeader.LoadFromCode(factory, "ABIGAS");
			OrgHeader importer = OrgHeader.LoadFromCode(factory, "ABABEU");

			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.GSTCode, "90093519530");
			supplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.SupplierCode, "955411R");
			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, "0059781A");
			JobDeclaration testDec = factory.New<JobDeclaration>();
			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testDec.JE_OH_Importer = importer.PK;
			testDec.JE_OH_Supplier = supplier.PK;

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_AgentsReference = "194270 -JD";
			testDec.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.Containerised;
			testDec.JE_DateOfArrival = new ZDateTime(2003, 12, 2, 18, 40, 0);
			testDec.JE_DateOfFirstArrival = new ZDateTime(2003, 12, 2, 18, 40, 0);
			testDec.JE_DeclarationReference = "B00103502";
			testDec.JE_EntryStatus = CustomsEntryStatus.NotSent.Code;
			testDec.JE_ExportDate = new ZDateTime(2003, 11, 29, 18, 40, 0);
			testDec.JE_ExportGoodsType = "OT";
			testDec.JE_GB = GlbBranch.CurrentBranch.PK;
			testDec.JE_HouseBill = "66016274";
			testDec.JE_MasterBill = "PONLCHC66016274";
			testDec.JE_MessageSubType = "FRM";
			testDec.JE_OwnerRef = "MULTIPLE";
			testDec.JE_RL_NKFinalDestination = "AUMEL";
			testDec.JE_RL_NKOrigin = "DEFRA";
			testDec.JE_RL_NKPortOfArrival = "AUMEL";
			testDec.JE_RL_NKPortOfFirstArrival = "AUMEL";
			testDec.JE_RL_NKPortOfLoading = "NZAKL";
			testDec.JE_VesselName = "ADMIRALENGRACHT";
			testDec.JE_TotalNoOfPacks = 10;
			testDec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			testDec.JE_VoyageFlightNo = "2131";
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			testDec.JobComInvoiceGroupHeaders[0].JZ_GroupInvoice = true;
			testDec.JobComInvoiceGroupHeaders[0].JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testDec.JobComInvoiceGroupHeaders[0].JZ_InvoiceCurrExRate = 1.000000000m;
			testDec.JobComInvoiceGroupHeaders[0].JZ_InvoiceDate = new ZDateTime(2003, 11, 29);

			JobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 2385.0000m, "NZD");
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 319.380m, "NZD");

			CusContainer testContainer1 = testDec.CusContainers.AddNew();
			testContainer1.CO_ContainerNumber = "PONU2864065";
			testContainer1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			testContainer1.CO_RC = factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20FR").PK;
			testContainer1.CO_Seal = "4419151";

			CusContainer testContainer2 = testDec.CusContainers.AddNew();
			testContainer2.CO_ContainerNumber = "POCU2842191";
			testContainer2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			testContainer2.CO_RC = factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20FR").PK;
			testContainer2.CO_Seal = "4419152";

			CusContainer testContainer3 = testDec.CusContainers.AddNew();
			testContainer3.CO_ContainerNumber = "PONU2874973";
			testContainer3.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			testContainer3.CO_RC = factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20FR").PK;
			testContainer3.CO_Seal = "4419153";

			JobComInvoiceHeader testHeader1 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testHeader1.JZ_InvoiceAmount = 127753.1400m;
			testHeader1.JZ_InvoiceCurrExRate = 1.000000000m;
			testHeader1.JZ_InvoiceDate = new ZDateTime(2003, 11, 29);
			testHeader1.JZ_InvoiceNumber = "1";
			testHeader1.JZ_OH_Supplier = supplier.PK;
			testHeader1.JZ_RX_NKInvoice_Currency = "NZD";
			testHeader1.JZ_Volume = 75.000m;
			testHeader1.JZ_VolumeUQ = "M3";
			testHeader1.JZ_Weight = 29491.000m;
			testHeader1.JZ_WeightUQ = "KG";
			testHeader1.JZ_AddInfo = "PRF=S*ORG=NZ*PackCountForNature10_Hidden=10*ValuationBasis_Hidden=RT";

			JobComInvoiceLine testLine1 = testHeader1.JobComInvoiceLines.AddNew();
			testLine1.JI_AddInfo = "GSTE=417";
			testLine1.JI_CustomsQuantity = 497.0000m;
			testLine1.JI_CustomsUnitQty = "KG";
			testLine1.JI_Description = "Chocolate confectionery";
			testLine1.JI_InvoiceQuantity = 497.00000m;
			testLine1.JI_InvoiceUQ = "KG";

			//		AssertEquals("CustomsQuantity", 497m, TestLine1.JI_CustomsQuantity);

			testLine1.JI_LineNo = (short)1;
			testLine1.JI_LinePrice = 9194.5000m;
			testLine1.JI_Tariff = "1806.32.00 13";
			testLine1.JI_Weight = 497.000m;
			testLine1.JI_WeightUQ = "KG";
			//AssertEquals("CustomsUnitOfQuantity", "KG", TestLine1.JI_CustomsUnitQty);

			AssertEquals("CustomsQuantity", 497m, testLine1.JI_CustomsQuantity);

			JobComInvoiceLine testLine2 = testHeader1.JobComInvoiceLines.AddNew();
			testLine2.JI_CustomsQuantity = 884.0000m;
			testLine2.JI_CustomsUnitQty = "KG";
			testLine2.JI_Description = "4978";
			testLine2.JI_InvoiceQuantity = 884.00000m;
			testLine2.JI_InvoiceUQ = "KG";
			testLine2.JI_LineNo = (short)2;
			testLine2.JI_LinePrice = 10404.6800m;
			testLine2.JI_Tariff = "1806.32.00 13";
			testLine2.JI_Weight = 884.000m;
			testLine2.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine3 = testHeader1.JobComInvoiceLines.AddNew();
			testLine3.JI_CustomsQuantity = 588.0000m;
			testLine3.JI_CustomsUnitQty = "KG";
			testLine3.JI_Description = "4388";
			testLine3.JI_InvoiceQuantity = 588.00000m;
			testLine3.JI_InvoiceUQ = "KG";
			testLine3.JI_LineNo = (short)3;
			testLine3.JI_LinePrice = 33163.2000m;
			testLine3.JI_Tariff = "1704.90.00 44";
			testLine3.JI_Weight = 588.000m;
			testLine3.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine4 = testHeader1.JobComInvoiceLines.AddNew();
			testLine4.JI_CustomsQuantity = 882.0000m;
			testLine4.JI_CustomsUnitQty = "KG";
			testLine4.JI_Description = "4388";
			testLine4.JI_InvoiceQuantity = 882.00000m;
			testLine4.JI_InvoiceUQ = "KG";
			testLine4.JI_LineNo = (short)4;
			testLine4.JI_LinePrice = 49744.8000m;
			testLine4.JI_Tariff = "1704.90.00 44";
			testLine4.JI_Weight = 882.000m;
			testLine4.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine5 = testHeader1.JobComInvoiceLines.AddNew();
			testLine5.JI_CustomsQuantity = 400.0000m;
			testLine5.JI_CustomsUnitQty = "KG";
			testLine5.JI_Description = "5525";
			testLine5.JI_InvoiceQuantity = 400.00000m;
			testLine5.JI_InvoiceUQ = "KG";
			testLine5.JI_LineNo = (short)5;
			testLine5.JI_LinePrice = 4152.0000m;
			testLine5.JI_Tariff = "1704.90.00 44";
			testLine5.JI_Weight = 400.000m;
			testLine5.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine6 = testHeader1.JobComInvoiceLines.AddNew();
			testLine6.JI_CustomsQuantity = 640.0000m;
			testLine6.JI_CustomsUnitQty = "KG";
			testLine6.JI_Description = "5525";
			testLine6.JI_InvoiceQuantity = 640.00000m;
			testLine6.JI_InvoiceUQ = "KG";
			testLine6.JI_LineNo = (short)6;
			testLine6.JI_LinePrice = 6643.2000m;
			testLine6.JI_Tariff = "1704.90.00 44";
			testLine6.JI_Weight = 640.000m;
			testLine6.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine7 = testHeader1.JobComInvoiceLines.AddNew();
			testLine7.JI_CustomsQuantity = 1644.0000m;
			testLine7.JI_CustomsUnitQty = "KG";
			testLine7.JI_Description = "5524";
			testLine7.JI_InvoiceQuantity = 1644.00000m;
			testLine7.JI_InvoiceUQ = "KG";
			testLine7.JI_LineNo = (short)7;
			testLine7.JI_LinePrice = 14450.7600m;
			testLine7.JI_Tariff = "1704.90.00 44";
			testLine7.JI_Weight = 1644.000m;
			testLine7.JI_WeightUQ = "KG";

			return testDec;
		}

		JobDeclaration CreateSendableDeclaration()
		{
			return CreateSendableDeclaration(Factory);
		}

		public static JobDeclaration CreateSendableDeclaration(BusinessObjectFactory factory)
		{
			OrgHeader supplier = OrgHeader.LoadFromCode(factory, "ABIGAS");
			OrgHeader importer = OrgHeader.LoadFromCode(factory, "ABABEU");

			importer.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "90093519530");
			supplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.SupplierCode, "955411R");
			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, "0225785H");
			JobDeclaration testDec = factory.New<JobDeclaration>();
			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			testDec.JE_OH_Importer = importer.PK;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testDec.JE_OH_Supplier = supplier.PK;
			testDec.AddInfo.ZA_MergeBy_Hidden = OrgConstants.MergeInvoiceLines.Tariff;
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testDec.JE_AgentsReference = "198810 -MP";
			testDec.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_DateOfArrival = new ZDateTime(2003, 12, 1, 10, 2, 0);
			testDec.JE_DateOfFirstArrival = new ZDateTime(2003, 12, 1, 10, 2, 0);
			testDec.JE_EntryStatus = CustomsEntryStatus.ClearCreate.Code;
			testDec.JE_ExportDate = new ZDateTime(2003, 11, 28, 10, 2, 0);
			testDec.JE_ExportGoodsType = "OT";
			testDec.JE_GB = GlbBranch.CurrentBranch.PK;
			testDec.JE_MasterBill = "PONLCPH22002959";
			testDec.JE_MessageSubType = "FRM";
			testDec.JE_OwnerRef = "1200994221";
			testDec.JE_RL_NKFinalDestination = "AUMEL";
			testDec.JE_RL_NKOrigin = "DEFRA";
			testDec.JE_RL_NKPortOfArrival = "AUMEL";
			testDec.JE_RL_NKPortOfFirstArrival = "AUSYD";
			testDec.JE_RL_NKPortOfLoading = "DEHAM";
			testDec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			testDec.JE_VesselName = "ADMIRALENGRACHT";
			testDec.JE_TotalNoOfPacks = 1;
			testDec.JE_VoyageFlightNo = "2038";

			testDec.JobComInvoiceGroupHeaders[0].JZ_GroupInvoice = true;
			testDec.JobComInvoiceGroupHeaders[0].JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testDec.JobComInvoiceGroupHeaders[0].JZ_InvoiceCurrExRate = 1.000000000m;
			testDec.JobComInvoiceGroupHeaders[0].JZ_InvoiceDate = new ZDateTime(2003, 11, 28);

			JobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			BaseJobComInvHeaderCharge oFT = groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 2300.0000m, "USD");
			oFT.J7_IsIncludedInITOT = true;
			BaseJobComInvHeaderCharge oNS = groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 310.73m, "AUD");
			oNS.J7_IsIncludedInITOT = true;
			JobComInvoiceHeader testHeader1 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testHeader1.JZ_AddInfo = "ORG=DK*PackCountForNature10_Hidden=1*ValuationBasis_Hidden=UT";
			testHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			testHeader1.JZ_InvoiceAmount = 124295.5000m;
			testHeader1.JZ_InvoiceCurrExRate = 1.000000000m;
			testHeader1.JZ_InvoiceDate = new ZDateTime(2003, 11, 28);
			testHeader1.JZ_InvoiceNumber = "1";
			testHeader1.JZ_OH_Supplier = supplier.PK;
			testHeader1.JZ_RX_NKInvoice_Currency = "AUD";
			testHeader1.JZ_VolumeUQ = "M3";
			testHeader1.JZ_Weight = 25500.000m;
			testHeader1.JZ_WeightUQ = "KG";

			JobComInvoiceLine testLine1 = testHeader1.JobComInvoiceLines.AddNew();
			testLine1.JI_AddInfo = "GSTE=FOOD";
			testLine1.JI_CustomsQuantity = 24859.1000m;
			testLine1.JI_CustomsUnitQty = "KG";
			testLine1.JI_Description = "FROZDANPORK";
			testLine1.JI_InvoiceQuantity = 24859.10000m;
			testLine1.JI_InvoiceUQ = "KG";
			testLine1.JI_LineNo = (short)1;
			testLine1.JI_LinePrice = 124295.5000m;
			testLine1.JI_Tariff = "0203.29.00 41";
			testLine1.JI_Weight = 24859.100m;
			testLine1.JI_WeightUQ = "KG";
			return testDec;
		}

		JobDeclaration GetJobDeclarationWithTwoDummyCusEntryHeaders()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders[1].CH_CH_PrimeEntry = declaration.CustomsEntryHeaders[0].PK;
			declaration.CustomsEntryHeaders[0].CH_Status = "";
			declaration.CustomsEntryHeaders[1].CH_Status = "";
			return declaration;
		}

		#endregion
	}
}
