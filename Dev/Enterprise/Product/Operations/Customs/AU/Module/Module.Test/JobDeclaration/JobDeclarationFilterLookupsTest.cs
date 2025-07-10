using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Module.Testing
{
	sealed class JobDeclarationFilterLookupsTest : TestCaseWithFactory
	{
		public void TestApplicationCodeList()
		{
			var applicationCodeList = lookups.ApplicationCodeList();
			AssertEquals("ApplicationCodeList of correct type.", typeof(CodeDescriptionPairList), applicationCodeList.GetType());
			AssertEquals("ApplicationCodeList should have 3 values.", 3, applicationCodeList.Count);
			AssertEquals("Send Legacy Message", applicationCodeList["LEG"].Description);
			AssertEquals("Send CMR Message", applicationCodeList["CMR"].Description);
			AssertEquals(Customs.Business.DeclarationApplicationCodeList.Descriptions.Interfaced, applicationCodeList["ITF"].Description);
			AssertSame(applicationCodeList, filterBizObj.Factory.GetCachedValue<CodeDescriptionPairList>("AUDeclarationApplicationCodeList", () => null));
		}

		public void TestCOLSEntryNumberStatusList()
		{
			var list = lookups.COLSEntryNumberStatusList;
			AssertEquals(new COLSEntryStatusList().CodesAsString, list.CodesAsString);
			AssertSame("Cached", list, lookups.COLSEntryNumberStatusList);
		}

		public void TestCOLSLodgementStatusList()
		{
			var list = lookups.COLSLodgementStatusList;
			AssertType<COLSLodgementStatusList>("List Type", list);
			AssertSame("Cached", list, lookups.COLSLodgementStatusList);
		}

		public void TestCOLSMessageStatusList()
		{
			var list = lookups.COLSMessageStatusList;
			AssertEquals($"{new COLSHeaderStatusList().CodesAsString}, Failed, Success", list.CodesAsString);
			AssertSame("Cached", list, lookups.COLSMessageStatusList);
		}

		public void TestMessageTypeList()
		{
			AssertEquals("AQS, DRW, EXP, EXW, EXX, IMP, IMX, MSC, WEA", lookups.MessageTypeList.CodesAsString);
		}

		public void TestMessageSubTypeList()
		{
			AssertEquals(true, lookups.MessageSubTypeList().ContainsOnly(
				"NCF", "CFM",
				JobDeclaration.MessageSubType.FormalEntry,
				JobDeclaration.MessageSubType.SimplifiedEntry,
				JobDeclaration.MessageSubType.SelfAssessedClearance,
				JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines,
				JobDeclaration.MessageSubType.RequestForCargoRelease,
				JobDeclaration.MessageSubType.PeriodDeclarationType1,
				JobDeclaration.MessageSubType.PeriodDeclarationType2));
		}

		public void TestContainerModeList()
		{
			AssertEquals(true, lookups.ContainerModeList.ContainsOnly(Core.Constants.ContainerModes.Containerised,
			Core.Constants.ContainerModes.BreakBulk,
			Core.Constants.ContainerModes.Bulk,
			Core.Constants.ContainerModes.Liquid,
			Core.Constants.ContainerModes.NonContainerised,
			Core.Constants.ContainerModes.Combination,
			Core.Constants.ContainerModes.FCL,
			Core.Constants.ContainerModes.FCLMixedShipper,
			Core.Constants.ContainerModes.LCL
			));
		}

		public void TestEntryStatusLists()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var filter = (ModuleTextFilter)filterBizObj[DeclarationFilterConstants.ShipmentType];
				filter.Property = JobMessageTypeList.Codes.Import;
				AssertEquals("Should come from " + nameof(EdificeCustomsEntryStatusList) + "in addition to SUB, ACK because AU is an integrated country now", new EdificeCustomsEntryStatusList().Count + 2, lookups.EntryStatusList().Count);
				filter.Property = JobMessageTypeList.Codes.Export;
				AssertEquals("Should come from " + nameof(ExportCustomsEntryStatusList) + "in addition to SUB, ACK because AU is an integrated country now", new ExportCustomsEntryStatusList().Count + 2, lookups.EntryStatusList().Count);
				filter.Property = "";
				AssertEquals("Should come from " + nameof(LegacyCustomsEntryStatusList) + "in addition to SUB, ACK because AU is an integrated country now", new LegacyCustomsEntryStatusList().Count + 2, lookups.EntryStatusList().Count);
			}
		}

		public void TestPaymentStatusList()
		{
			AssertEquals(true, lookups.PaymentStatusList.ContainsOnly(DeclarationFilterConstants.PaymentStatus.Paid, DeclarationFilterConstants.PaymentStatus.NotPaid));
			AssertEquals("Show only paid", lookups.PaymentStatusList.GetDescriptionFromCode(DeclarationFilterConstants.PaymentStatus.Paid));
			AssertEquals("Show only unpaid", lookups.PaymentStatusList.GetDescriptionFromCode(DeclarationFilterConstants.PaymentStatus.NotPaid));
		}

		public void TestCMREntryStatusList()
		{
			var consolidatedEntryStatusList = new ConsolidatedEntryStatusList().CodesAsString;

			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Should come from CMRImportEntryAdviceList and ConsolidatedEntryStatusList when ConsolidatedEntries is enabled", $"{new CMRImportEntryAdviceList().CodesAsString}, {consolidatedEntryStatusList}", lookups.CMREntryStatusList.CodesAsString);
			}

			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Should come from CMRImportEntryAdviceList when ConsolidatedEntries is not enabled", $"{new CMRImportEntryAdviceList().CodesAsString}", lookups.CMREntryStatusList.CodesAsString);
			}
		}

		public void TestNatureList()
		{
			AssertEquals("NatureTypeList", true, lookups.NatureTypeList.ContainsOnly(
				DeclarationFilterConstants.NatureTypes.Nature10,
				DeclarationFilterConstants.NatureTypes.Nature20,
				DeclarationFilterConstants.NatureTypes.Nature30));
		}

		public void TestCMRImportMessageStatusList()
		{
			AssertEquals("Should come from CMRImportMessageStatusList", new CMRImportMessageStatusList().Count, lookups.CMRMessageStatusList.Count);
			AssertEquals("A blank code should not exist", true, lookups.CMRMessageStatusList.IndexOfCode("") == -1);
			AssertEquals("Not Sent should be of code 'NOT'", "Not Sent", lookups.CMRMessageStatusList.GetDescriptionFromCode(DeclarationFilterConstants.EntryStatus.NotSentForFilter));
		}

		public void TestTransportTypeList()
		{
			AssertEquals("AIR, SEA, MAI, OTH", lookups.TransportTypeList.CodesAsString);
		}

		public void TestRFPStatusList()
		{
			AssertType<EXDOCComplianceStatusCodes>(lookups.RFPStatusList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			filterBizObj = new JobDeclarationFilterBusinessObject();
			lookups = filterBizObj.Lookups;
		}

		JobDeclarationFilterBusinessObject filterBizObj;
		JobDeclarationFilterLookups lookups;
	}
}
