using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Messaging;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using ManifestValidationRuleCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeList.ManifestValidationRuleCodes;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;
using universalAlias = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderBaseOnlyTest : AsycudaManifestHeaderAbstractTest
	{
		public void TestOnConsolWasUpdatedByDataRefreshIncludingChildren()
		{
			var sourceConsol = Factory.New<ForwardingConsol>();
			sourceConsol.JK_UniqueConsignRef = "C4321";
			sourceConsol.JK_RL_NKDischargePort = "VUVLI";
			sourceConsol.JK_RL_NKLoadPort = "GBFXT";
			sourceConsol.JK_TransportMode = TransportTypeList.Codes.Sea;
			sourceConsol.JK_ConsolMode = "FCL";
			sourceConsol.JK_CoLoadMasterBill = "COLOADMBL";
			sourceConsol.JK_MasterBillNum = "BOL123456";
			sourceConsol.Transports[0].JW_VoyageFlight = "W1";
			sourceConsol.Transports[0].JW_Vessel = "ADMIRALENGRACHT";
			sourceConsol.Transports[0].JW_ETA = ZDateTime.BrettsBirthday;
			sourceConsol.Transports[0].JW_ETD = ZDateTime.BrettsBirthday.AddDays(-1);

			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_TransportMode = TransportTypeList.Codes.Air;
			manifest.SetParent(sourceConsol);
			manifest.AMA_OverrideFreightDefaults = true;
			_ = manifest.Synchroniser;
			manifest.OnConsolWasUpdatedByDataRefreshIncludingChildren(null, new EventArgs());
			AssertEquals("Not sync from consol, because of AMA_OverrideFreightDefaults is true", TransportTypeList.Codes.Air, manifest.AMA_TransportMode);

			manifest.AMA_MessageStatus = MessageStatusCodeList.Codes.Sent;
			manifest.AMA_OverrideFreightDefaults = false;
			manifest.AMA_TransportMode = TransportTypeList.Codes.Air;
			manifest.OnConsolWasUpdatedByDataRefreshIncludingChildren(null, new EventArgs());
			AssertEquals("Not sync from consol, because of messages are sent", TransportTypeList.Codes.Air, manifest.AMA_TransportMode);

			manifest.AMA_MessageStatus = MessageStatusCodeList.Codes.NotSent;
			manifest.OnConsolWasUpdatedByDataRefreshIncludingChildren(null, new EventArgs());
			AssertEquals("sync from consol", TransportTypeList.Codes.Sea, manifest.AMA_TransportMode);
		}

		public void TestDefaultManifestTypeOnceAttachToManifestAndCountryIsSet()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "RFM";
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("HAB", header.AMA_ManifestType);

			header.AMA_ManifestType = "COH";
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("COH", header.AMA_ManifestType);
		}

		public void TestIEDIFACTMessageAttachee_TopLevelBusinessObject()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_HouseBill = "TEST_1";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.Add(shipment);
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			var provider = header as IEDIFACTMessageAttachee;
			AssertEquals(header, provider.TopLevelBusinessObject);

			header.SetParent(consol);
			AssertEquals(consol, provider.TopLevelBusinessObject);
		}

		public void TestRegistrationStatus()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "CustomsManifestStatus");
			var sg8 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "8", "Accepted", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var sg9 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "6", "Rejected", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var headerZA = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();

			headerZA.AMA_ManifestType = "RFM";
			Assert(!headerZA.IsBillLevelManifestType);

			var entryNum = CusEntryNumber.LoadOrCreate(headerZA, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.SouthAfrica);
			entryNum.CE_EntryStatus = "8";
			AssertEquals("8", headerZA.RegistrationStatus);
			AssertEquals("Accepted", headerZA.RegistrationStatusDescription);
			AssertEquals("8 - Accepted", headerZA.RegistrationStatusCodeAndDescription);

			headerZA.AMA_ManifestType = "COH";
			Assert(headerZA.IsBillLevelManifestType);

			entryNum.CE_EntryStatus = ZString.Empty;
			var billZA1 = headerZA.Bills.AddNew();
			AssertEquals(ZString.Empty, headerZA.RegistrationStatus);
			AssertEquals(ZString.Empty, headerZA.RegistrationStatusDescription);
			AssertEquals(ZString.Empty, headerZA.RegistrationStatusCodeAndDescription);

			billZA1.ABL_BillStatus = "6";
			AssertEquals("6", headerZA.RegistrationStatus);
			AssertEquals("Rejected", headerZA.RegistrationStatusDescription);
			AssertEquals("6 - Rejected", headerZA.RegistrationStatusCodeAndDescription);

			var billZA2 = headerZA.Bills.AddNew();
			billZA2.ABL_BillStatus = "6";
			AssertEquals("6", headerZA.RegistrationStatus);

			billZA2.ABL_BillStatus = "8";
			AssertEquals("MULTIPLE", headerZA.RegistrationStatus);
			AssertEquals("MULTIPLE Registration Statuses", headerZA.RegistrationStatusDescription);
			AssertEquals("MULTIPLE Registration Statuses", headerZA.RegistrationStatusCodeAndDescription);
		}

		public void TestRegistrationStatusDescriptionUsingXmlCodeList()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.GVMS, Core.Constants.CountryCodes.UnitedKingdom, ZDate.Today, true))
			{
				var header = (AsycudaManifestHeader)Factory.New<GB.GBGVMS.IAsycudaManifestHeader>();
				header.AMA_ManifestType = "GVM";
				header.RegistrationStatus = "OPN";
				AssertEquals("Get Description from Xml Code List", "Open and awaiting processing", header.RegistrationStatusDescription);
			}
		}

		public void TestCombineBillStatuses()
		{
			const string multipleStatusesText = "XYZ";
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			AssertForStatus(bill => bill.ABL_BillStatus, bill => bill.ABL_BillStatusInfo);
			AssertForStatus(bill => bill.ABL_MessageStatus, bill => bill.ABL_MessageStatusInfo);

			AssertNullOrEmpty("Null Bills status provider", header.CombineBillStatuses(null, multipleStatusesText));

			void AssertForStatus(Func<AsycudaBill, ZString> statusProvider, Func<AsycudaBill, ZPropertyInfo> statusInfoProvider)
			{
				AssertNullOrEmpty("Manifest header without bills", header.CombineBillStatuses(statusProvider, multipleStatusesText));

				statusInfoProvider(bill1).Value = (ZString)"A";
				statusInfoProvider(bill2).Value = ZString.Empty;
				AssertNullOrEmpty("Bill with empty status", header.CombineBillStatuses(statusProvider, multipleStatusesText));

				statusInfoProvider(bill2).Value = (ZString)"A";
				AssertEquals("Bills all have same status", "A", header.CombineBillStatuses(statusProvider, multipleStatusesText));

				statusInfoProvider(bill2).Value = (ZString)"B";
				AssertEquals("Bills with multiple statuses", multipleStatusesText, header.CombineBillStatuses(statusProvider, multipleStatusesText));
			}
		}

		public void TestReportErrorWhenChangingApplicationKeyResultsInDifferentType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			{
				var header = Factory.New<AsycudaManifestHeader>();

				AssertType("Unknown countries load ASYCUDAManifest.Business by default", ObjectFactory.GetType<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader>(), header);
				AssertEquals("AMA_RN_NKCountry set to ZString.Empty by default", ZString.Empty, header.AMA_RN_NKCountry);

				using (TemporarilySetManifestCountry(header, Core.Constants.CountryCodes.Singapore, "MGI"))
				{
					AssertEquals("Changing GlobalManifestApplicationKey from 'MGI' to 'SGMGI' results in a different type 'Enterprise.Customs.SG.Access.Business.AsycudaManifestHeader' for business object 'Enterprise.Customs.ASYCUDAManifest.Business.AsycudaManifestHeader'.", ErrorReporter.LastMessageReported);
				}

				header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
				AssertEquals("Setting AMA_RN_NKCountry='ER' doesn't change the class version.", string.Empty, ErrorReporter.LastMessageReported);

				using (TemporarilySetManifestCountry(header, Core.Constants.CountryCodes.SouthAfrica, "HAB"))
				{
					AssertEquals("Changing GlobalManifestApplicationKey from 'ERHAB' to 'ZAHAB' results in a different type 'Enterprise.Customs.ZA.Manifest.Business.AsycudaManifestHeader' for business object 'Enterprise.Customs.ASYCUDAManifest.Business.AsycudaManifestHeader'.", ErrorReporter.LastMessageReported);
				}
			}
		}

		public void TestIStatusSupporter_SupportsPackLevelMessages()
		{
			var sgRegistry = ObjectFactory.Get<SG.ISGCustomsRegistry>();
			sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var headerSG = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGE");
			headerSG.FillWithValidTestData();
			Assert(((IStatusSupporter)headerSG).SupportsPackLevelMessages);

			var headerZA = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB'");
			headerZA.FillWithValidTestData();

			Assert(!((IStatusSupporter)headerZA).SupportsPackLevelMessages);
		}

		public void TestLogMessageStatusChangeEventOnParent()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			header.AMA_MessageStatus = MessageStatusCodeList.Codes.Sent;

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageStatusChangeCode);
			logQuery.AddToFilter(StmALogSchema.SL_Reference, MessageStatusCodeList.Codes.Sent);

			Assert(header.Logs.HasLogWith(logQuery));
		}

		public void TestLogCustomsManifestStatusEventOnParent()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			header.RegistrationStatus = "ERR";

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsManifestStatusCode);
			logQuery.AddToFilter(StmALogSchema.SL_Reference, "ERR");

			Assert(header.Logs.HasLogWith(logQuery));
		}

		public void TestDeletionInCorrectOrder_2()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			header.RegistrationNumber = "ASD";
			var entryNum = header.RegistrationEntryNumber;
			var message = header.Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			entryNum.NotificationsChanged += (object sender, NotificationsChangedEventArgs e) =>
			{
				if (!header.IsDeleted && entryNum.IsDeleted)
				{
					var newEntryNum = Factory.New<CusEntryNumber>();
					newEntryNum.Parent = header;
					newEntryNum.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
				}
			};
			message.NotificationsChanged += (object sender, NotificationsChangedEventArgs e) =>
			{
				if (!header.IsDeleted && message.IsDeleted)
				{
					var newMessage = Factory.New<EDIMessage>();
					newMessage.EM_LinkedObject = header;
					newMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				}
			};
			header.Delete();
			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestClearAMA_OA_DeconsolidateAddressIfNeeded()
		{
			var org = Factory.NewWithValidTestData<OrgAddress>();

			foreach (var testCase in new[]
			{
				new { ManifestType = nameof(ManifestDocumentType.COM) },
				new { ManifestType = nameof(ManifestDocumentType.ECL) },
				new { ManifestType = nameof(ManifestDocumentType.FFM) },
				new { ManifestType = nameof(ManifestDocumentType.RMA) },
				new { ManifestType = nameof(ManifestDocumentType.RFM) },
				new { ManifestType = nameof(ManifestDocumentType.AQM) },
				new { ManifestType = nameof(ManifestDocumentType.ALM) },
			})
			{
				var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
				header.AMA_OA_DeconsolidateAddress = org.PK;
				header.AMA_Nature = ShipmentTypeList.Codes.Import23;
				header.AMA_ManifestType = testCase.ManifestType;
				AssertEquals(ZGuid.Empty, header.AMA_OA_DeconsolidateAddress);
			}
		}

		public void TestClearAMA_OA_DischargeTerminalAddressIfNeeded()
		{
			var org = Factory.NewWithValidTestData<OrgAddress>();

			foreach (var testCase in new[]
			{
				new { ManifestType = nameof(ManifestDocumentType.COM) },
				new { ManifestType = nameof(ManifestDocumentType.ECL) },
				new { ManifestType = nameof(ManifestDocumentType.FFM) },
				new { ManifestType = nameof(ManifestDocumentType.RMA) },
				new { ManifestType = nameof(ManifestDocumentType.RFM) },
				new { ManifestType = nameof(ManifestDocumentType.AQM) },
				new { ManifestType = nameof(ManifestDocumentType.ALM) },
			})
			{
				var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
				header.AMA_OA_DischargeTerminalAddress = org.PK;
				header.AMA_Nature = ShipmentTypeList.Codes.Import23;
				header.AMA_ManifestType = testCase.ManifestType;
				AssertEquals(ZGuid.Empty, header.AMA_OA_DischargeTerminalAddress);
			}
		}

		public void TestRegistrationNumber_WhenEntryNumberTypeIsNotAsy()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var entryNum = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, header.AMA_RN_NKCountry);
			entryNum.CE_EntryNum = "123";
			AssertEquals("123", header.RegistrationNumber);

			entryNum.CE_EntryType = "";
			AssertEquals("", header.RegistrationNumber);
		}

		public void TestManifestType()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "RFM";
			AssertEquals("RFM", header.ManifestType.Code);
			header.AMA_ManifestType = "ALH";
			AssertEquals("ManifestType is updated with AMA_ManifestType", "ALH", header.ManifestType.Code);
		}

		public void TestHasContainers()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			Assert("Containers are enabled by default", header.HasContainers);
		}

		public void TestHasBillsAndPacks()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			Assert("Bills and Packs are enabled by default", header.HasBillsAndPacks);
		}

		public void TestLockedBills()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			Assert("By default, Bill are not locked", !header.LockedBills);
		}

		public void TestIsDeclarationCreationEnabled()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			AssertEquals("Declaration Creation is disabled by default", false, header.IsDeclarationCreationEnabled);
		}

		public void TestIsBillLevelManifestType()
		{
			var headerZA = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			headerZA.AMA_ManifestType = "COH";
			Assert(headerZA.IsBillLevelManifestType);
			headerZA.AMA_ManifestType = "RFM";
			Assert(!headerZA.IsBillLevelManifestType);
		}

		public void TestIsPackedItemLevelManifestType()
		{
			var sgRegistry = ObjectFactory.Get<SG.ISGCustomsRegistry>();
			sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var headerSG = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			headerSG.AMA_ManifestType = "MGE";
			Assert(headerSG.IsPackedItemLevelManifestType);

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			Assert(!header.IsPackedItemLevelManifestType);
		}

		public void TestIStatusSupporterMembers()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			IStatusSupporter supporter = header;
			supporter.CustomsStatus = "C1";
			supporter.MessageStatus = "M1";
			AssertEquals("country.AMA_MessageStatus", "M1", header.AMA_MessageStatus);
			supporter.CustomsStatus = "C2";
			supporter.MessageStatus = "M3";
			AssertEquals("country.AMA_MessageStatus", "M3", header.AMA_MessageStatus);
		}

		public void TestIAgentCodeProviderForInterchanges()
		{
			GlbCompany.CurrentCompany.OrgProxy.SetAgentCode(RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.SouthAfrica), "DJC");
			GlbCompany.CurrentCompany.OrgProxy.SetCustomsCode(OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.SouthAfrica), "DUAL");

			var headerER = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Eritrea, "ASY");
			headerER.FillWithValidTestData();
			AssertEquals("", ((IInterchangeSenderIdProvider)headerER).SenderID);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var headerZA = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
				headerZA.FillWithValidTestData();
				AssertEquals("DJCDUAL", ((IInterchangeSenderIdProvider)headerZA).SenderID);
			}
		}

		public void TestShowPackedItems_2()
		{
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.PackedItemRelationshipOverrideForTesting = ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			header.AMA_TransportMode = "AIR";
			AssertEquals(true, header.IsManyPackedItemRelationship);
			AssertEquals(true, header.ShowPackedItems);

			var zaHeader = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			AssertEquals(true, zaHeader.IsNonePackedItemRelationship);
			AssertEquals(false, zaHeader.ShowPackedItems);

			var sgHeader = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			AssertEquals(true, sgHeader.IsOnePackedItemRelationship);
			AssertEquals(true, sgHeader.ShowPackedItems);

			var peHeader = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.PEManifest.IAsycudaManifestHeader>();
			AssertEquals(true, peHeader.IsOnePackedItemRelationship);
			AssertEquals(false, peHeader.ShowPackedItems);
		}

		public void TestTotalHouseBillsGrossWeight()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.MasterBill.ABL_GrossWeight = 1m;
			header.MasterBill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			var bill = header.Bills.AddNew();
			bill.ABL_GrossWeight = 1000m;
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Grams;

			CombineAssertions(() =>
			{
				AssertEquals("TotalWeight", 1m, header.TotalHouseBillsGrossWeight);
				bill = header.Bills.AddNew();
				bill.ABL_GrossWeight = 2000m;
				bill.ABL_GrossWeightUQ = Core.Constants.Weight.Grams;
				AssertEquals("TotalWeight", 3m, header.TotalHouseBillsGrossWeight);
				header.MasterBill.ABL_GrossWeightUQ = Core.Constants.Weight.Grams;
				AssertEquals("TotalWeight", 3000m, header.TotalHouseBillsGrossWeight);
				header.MasterBill.ABL_GrossWeightUQ = ZString.Empty;
				AssertEquals("TotalWeight", 0m, header.TotalHouseBillsGrossWeight);
			});
		}

		public void TestTotalHouseBillsPackages()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.MasterBill.ABL_ManifestQty = 1;
			header.MasterBill.ABL_ManifestUQ = Core.Constants.PkgUnit.Package;
			var bill = header.Bills.AddNew();
			bill.ABL_ManifestQty = 2;
			bill.ABL_ManifestUQ = Core.Constants.PkgUnit.Package;

			CombineAssertions(() =>
			{
				AssertEquals("TotalPackages", 2, header.TotalHouseBillsPackages);
				bill = header.Bills.AddNew();
				bill.ABL_ManifestUQ = Core.Constants.PkgUnit.Package;
				bill.ABL_ManifestQty = 2;
				AssertEquals("TotalPackages", 4, header.TotalHouseBillsPackages);
				header.MasterBill.ABL_ManifestUQ = ZString.Empty;
				AssertEquals("TotalPackages", 0, header.TotalHouseBillsPackages);
			});
		}

		public void TestAMA_ManifestType_Defaulting()
		{
			var headerASYCUDA = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			headerASYCUDA.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			headerASYCUDA.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			AssertEquals("ASYCUDAManifest manifest type defaults to ASY", "ASY", headerASYCUDA.AMA_ManifestType);

			var headerZA = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			headerZA.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			headerZA.AMA_ManifestType = "RFM";

			headerZA.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("HAB", headerZA.AMA_ManifestType);

			headerZA.AMA_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("RFM", headerZA.AMA_ManifestType);

			headerZA.AMA_TransportMode = "XXX";
			AssertEquals("AMA_ManifestType should never be set to empty, as it is part of the 'key'.", "RFM", headerZA.AMA_ManifestType);
		}

		public void TestDefaultCustomsOfficeCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			var version = "0.0.0.0";
			var lkPk = helper.CreateNewOrGetExistingCusCodeList("ZZ", universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "LK", "Sri Lanka", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue).PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(lkPk, universalAlias.RefCusCodeListTypes.Codes.NVC, version);
			var sbPk = helper.CreateNewOrGetExistingCusCodeList("ZZ", universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "SB", "Solomon Islands", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue).PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(sbPk, universalAlias.RefCusCodeListTypes.Codes.NVC, version);
			var pgPk = helper.CreateNewOrGetExistingCusCodeList("ZZ", universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "PG", "Papua New Guinea", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue).PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(pgPk, universalAlias.RefCusCodeListTypes.Codes.NVC, version);
			var vuPk = helper.CreateNewOrGetExistingCusCodeList("ZZ", universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "VU", "Vanuatu", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue).PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuPk, universalAlias.RefCusCodeListTypes.Codes.NVC, version);

			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var lkARKTM = helper.CreateNewOrGetExistingCusCodeList("LK", universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "ARKTM", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var lkSECMB = helper.CreateNewOrGetExistingCusCodeList("LK", universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "SECMB", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(lkARKTM.PK, "PORT", "LKCMB");
			helper.CreateTransportModeForCusCodeList(lkARKTM.PK, "AIR");
			helper.CreateNewOrGetExistingCusCodeListAttribute(lkSECMB.PK, "PORT", "LKCMB");
			helper.CreateTransportModeForCusCodeList(lkSECMB.PK, "SEA");

			var sbHIRA = helper.CreateNewOrGetExistingCusCodeList("SB", universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "HIRA", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var sbHIRS = helper.CreateNewOrGetExistingCusCodeList("SB", universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "HIRS", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var sbMUAA = helper.CreateNewOrGetExistingCusCodeList("SB", universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "MUAA", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var sbMUAP = helper.CreateNewOrGetExistingCusCodeList("SB", universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "MUAP", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var sbNORS = helper.CreateNewOrGetExistingCusCodeList("SB", universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "NORS", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sbHIRA.PK, "PORT", "SBHIR");
			helper.CreateTransportModeForCusCodeList(sbHIRA.PK, "AIR");
			helper.CreateNewOrGetExistingCusCodeListAttribute(sbHIRS.PK, "PORT", "SBHIR");
			helper.CreateTransportModeForCusCodeList(sbHIRS.PK, "SEA");
			helper.CreateNewOrGetExistingCusCodeListAttribute(sbMUAA.PK, "PORT", "SBMUA");
			helper.CreateTransportModeForCusCodeList(sbMUAA.PK, "AIR");
			helper.CreateNewOrGetExistingCusCodeListAttribute(sbMUAP.PK, "PORT", "SBMUA");
			helper.CreateTransportModeForCusCodeList(sbMUAP.PK, "MAI");
			helper.CreateNewOrGetExistingCusCodeListAttribute(sbNORS.PK, "PORT", "SBNOR");
			helper.CreateTransportModeForCusCodeList(sbNORS.PK, "SEA");

			var pgWWK = helper.CreateNewOrGetExistingCusCodeList("PG", universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "WWK", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(pgWWK.PK, "PORT", "PGWWK");

			var vuVAIR = helper.CreateNewOrGetExistingCusCodeList("VU", universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "VAIR", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var vuVSEA = helper.CreateNewOrGetExistingCusCodeList("VU", universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "VSEA", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var vuVPOST = helper.CreateNewOrGetExistingCusCodeList("VU", universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "VPOST", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var vuSAIR = helper.CreateNewOrGetExistingCusCodeList("VU", universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "SAIR", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var vuSSEA = helper.CreateNewOrGetExistingCusCodeList("VU", universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "SSEA", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var vuSPOST = helper.CreateNewOrGetExistingCusCodeList("VU", universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "SPOST", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuVAIR.PK, "PORT", "VUVLI");
			helper.CreateTransportModeForCusCodeList(vuVAIR.PK, "AIR");
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuVSEA.PK, "PORT", "VUVLI");
			helper.CreateTransportModeForCusCodeList(vuVSEA.PK, "SEA");
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuVPOST.PK, "PORT", "VUVLI");
			helper.CreateTransportModeForCusCodeList(vuVPOST.PK, "MAI");
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuSAIR.PK, "PORT", "VUSAN");
			helper.CreateTransportModeForCusCodeList(vuSAIR.PK, "AIR");
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuSSEA.PK, "PORT", "VUSAN");
			helper.CreateTransportModeForCusCodeList(vuSSEA.PK, "SEA");
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuSPOST.PK, "PORT", "VUSAN");
			helper.CreateTransportModeForCusCodeList(vuSPOST.PK, "MAI");

			var erPK = helper.CreateNewOrGetExistingCusCodeList("ZZ", universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "ER", "Eritrea", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue).PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(erPK, universalAlias.RefCusCodeListTypes.Codes.NVC, version);
			var erCUSAIR = helper.CreateNewOrGetExistingCusCodeList("ER", universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "ERAIR", "Eritrea AIR Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTransportModeForCusCodeList(erCUSAIR.PK, "AIR");
			var erCUSSEA = helper.CreateNewOrGetExistingCusCodeList("ER", universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "ERSEA", "Eritrea SEA Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTransportModeForCusCodeList(erCUSSEA.PK, "SEA");

			Factory.Save();

			var testCases = new List<string[]>
			{
				new string[] { "AIR", "LKCMB", "ARKTM" },
				new string[] { "SEA", "LKCMB", "SECMB" },

				new string[] { "AIR", "SBHIR", "HIRA" },
				new string[] { "SEA", "SBHIR", "HIRS" },
				new string[] { "AIR", "SBMUA", "MUAA" },
				new string[] { "MAI", "SBMUA", "MUAP" },
				new string[] { "SEA", "SBNOR", "NORS" },

				new string[] { "AIR", "PGWWK", "WWK" },

				new string[] { "AIR", "VUVLI", "VAIR" },
				new string[] { "SEA", "VUVLI", "VSEA" },
				new string[] { "MAI", "VUVLI", "VPOST" },
				new string[] { "AIR", "VUSAN", "SAIR" },
				new string[] { "SEA", "VUSAN", "SSEA" },
				new string[] { "MAI", "VUSAN", "SPOST" }
			};

			var testCasesWithOutPorts = new List<string[]>
			{
				new string[] { "AIR", "ERAIR" },
				new string[] { "SEA", "ERSEA" },
			};

			foreach (var trio in testCases)
			{
				RunOfficeCodeTest(trio);
			}

			foreach (var pair in testCasesWithOutPorts)
			{
				RunOfficeCodeTestWithOutPorts(pair);
			}
		}

		public void TestOfficeCodeNotUnnecessarilyWiped()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "FJ", "Fiji", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var office = helper.CreateNewOrGetExistingCusCodeList("FJ", universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "NADI", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(office.PK, "PORT", "FJNAN");
			helper.CreateTransportModeForCusCodeList(office.PK, "AIR");

			Factory.Save();

			var manifest1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest1.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;

			var consol = Factory.New<ForwardingConsol>();
			manifest1.SetParent(consol);
			manifest1.AMA_TransportMode = "AIR";
			manifest1.AMA_RL_NKPortOfLoading = "USATL";
			manifest1.AMA_RL_NKPortOfDischarge = "ERXXX";

			AssertEquals("", manifest1.AMA_CustomsOffice);
			manifest1.AMA_CustomsOffice = "DJC";
			manifest1.AMA_RL_NKPortOfDischarge = "ERXXX";  // simulates synching from a consol or loading from database - a null change
			AssertEquals("DJC", manifest1.AMA_CustomsOffice);
			manifest1.AMA_RL_NKPortOfDischarge = "AIR";   // ditto
			AssertEquals("DJC", manifest1.AMA_CustomsOffice);
		}

		[TestDate(1986, 3, 12, 4, 27, 0)]
		public void TestRegistrationDetails()
		{
			var headerBD = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Bangladesh, "ASY");
			headerBD.FillWithValidTestData();
			AssertEquals(ZDateTime.Empty, headerBD.RegistrationDate);
			AssertEquals(ZString.Empty, headerBD.RegistrationStatus);
			AssertEquals(ZString.Empty, headerBD.RegistrationNumber);
			AssertEquals(ZInt.Zero, headerBD.RegistrationYear);
			AssertEquals(false, headerBD.RegistrationStatusInfo.ReadOnly);
			AssertEquals(false, headerBD.RegistrationDateInfo.ReadOnly);
			AssertEquals(false, headerBD.RegistrationNumberInfo.ReadOnly);
			AssertEquals(false, headerBD.RegistrationYearInfo.ReadOnly);
			headerBD.RegistrationNumber = "Poop";
			AssertEquals(AsycudaRegistrationStatuses.Codes.Registered, headerBD.RegistrationStatus);
			AssertEquals(new ZDateTime(1986, 3, 12, 4, 27, 0), headerBD.RegistrationDate);
			AssertEquals(1986, headerBD.RegistrationYear);
			headerBD.RegistrationDate = new ZDateTime(2015, 8, 22, 14, 0, 0);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var headerReloaded = newFactory.Load<AsycudaManifestHeader>(headerBD.PK);
			AssertEquals("Poop", headerReloaded.RegistrationNumber);
			AssertEquals(new ZDateTime(2015, 8, 22, 14, 0, 0), headerReloaded.RegistrationDate);
			AssertEquals(2015, headerReloaded.RegistrationYear);
			AssertEquals(AsycudaRegistrationStatuses.Codes.Registered, headerReloaded.RegistrationStatus);

			// Joo has suspended this logic.....
			//countryReloaded.AMA_RN_NKCountry = "FJ";
			//AssertEquals("Wiped when country changes", "", countryReloaded.RegistrationNumber);
			//AssertEquals("Wiped when country changes", ZDateTime.Empty, countryReloaded.RegistrationDate);
			//AssertEquals("Wiped when country changes", "", countryReloaded.RegistrationStatus);

			foreach (var countryCode in new[]
			{
					Core.Constants.CountryCodes.SriLanka,
					Core.Constants.CountryCodes.PapuaNewGuinea,
					Core.Constants.CountryCodes.SolomonIslands,
					Core.Constants.CountryCodes.Vanuatu,
					Core.Constants.CountryCodes.Fiji
				})
			{
				var header = AsycudaManifestHeaderHelper.CreateNew(Factory, countryCode, "ASY");
				header.FillWithValidTestData();
				header.AMA_JobReference += countryCode;
				header.Factory.Save();
				newFactory = new BusinessObjectFactory();
				header = newFactory.Load<AsycudaManifestHeader>(header.PK);
				AssertEquals($"header.RegistrationStatusInfo.ReadOnly for '{countryCode}'", false, header.RegistrationStatusInfo.ReadOnly);
				AssertEquals($"header.RegistrationDateInfo.ReadOnly for '{countryCode}'", false, header.RegistrationDateInfo.ReadOnly);
				AssertEquals($"header.RegistrationYearInfo.ReadOnly for '{countryCode}'", false, header.RegistrationYearInfo.ReadOnly);
				AssertEquals($"header.RegistrationNumberInfo.ReadOnly for '{countryCode}'", false, header.RegistrationNumberInfo.ReadOnly);
			}

			foreach (var tuple in new (string CountryCode, string ManifestType)[]
			{
					(Core.Constants.CountryCodes.SouthAfrica, "HAB"),
					(Core.Constants.CountryCodes.UnitedStates, "IAM")
			})
			{
				var header = AsycudaManifestHeaderHelper.CreateNew(Factory, tuple.CountryCode, tuple.ManifestType);
				header.FillWithValidTestData();
				header.AMA_JobReference += tuple.CountryCode;
				header.Factory.Save();
				newFactory = new BusinessObjectFactory();
				header = newFactory.Load<AsycudaManifestHeader>(header.PK);
				AssertEquals($"country.RegistrationStatusInfo.ReadOnly for '{tuple.CountryCode}'", true, header.RegistrationStatusInfo.ReadOnly);
				AssertEquals($"country.RegistrationDateInfo.ReadOnly for '{tuple.CountryCode}'", true, header.RegistrationDateInfo.ReadOnly);
				AssertEquals($"country.RegistrationYearInfo.ReadOnly for '{tuple.CountryCode}'", true, header.RegistrationYearInfo.ReadOnly);
				AssertEquals($"country.RegistrationNumberInfo.ReadOnly for '{tuple.CountryCode}'", true, header.RegistrationNumberInfo.ReadOnly);
			}
		}

		public void TestRegistrationYear()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Bangladesh, "ASY");

			const int validYear = 1992;
			header.RegistrationYear = validYear;
			AssertEquals("RegistrationYear should be set to the valid value 1992", validYear, header.RegistrationYear);
			// setting the same value again
			header.RegistrationYear = validYear;
			AssertEquals("RegistrationYear should be the same value 1992", validYear, header.RegistrationYear);

			const int tooSmallYear = 1000;
			header.RegistrationYear = tooSmallYear;
			AssertEquals("RegistrationYear should be set to the smallest possible value when assigning a value that is too small",
				ZDateTime.MinSmallDateTimeValue.Year, header.RegistrationYear);

			const int tooBigYear = 9000;
			header.RegistrationYear = tooBigYear;
			AssertEquals("RegistrationYear should be set to the largest possible value when assigning a value that is too big",
				ZDateTime.MaxSmallDateTimeValue.Year, header.RegistrationYear);
		}

		public void TestMessageStatusAndReadOnlyAndDelete()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();

			Assert(header.CanDelete);
			Assert(!header.ReadOnly);

			header.AMA_MessageStatus = MessageStatusList.Codes.Sent;
			Assert(!header.CanDelete);
			Assert(!header.ReadOnly);
			Assert(header.AMA_RN_NKCountryInfo.ReadOnly);
		}

		public void TestCountryCodesAndNames()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "PG", "Papua New Guinea", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "PG";

			var consol = Factory.New<ForwardingConsol>();
			header.SetParent(consol);
			header.AMA_RL_NKPortOfLoading = "PGXXX";
			AssertEquals("PG", header.AMA_RN_NKCountry);
			AssertEquals("Papua New Guinea", header.CountryName);
		}

		public void TestDateAtCustomsOffice()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			header.FillWithValidTestData();
			header.AMA_DateAtCustomsOffice = new ZDate(1990, 1, 1);

			AssertEquals(new ZDateTime(1990, 1, 1), header.DateAtCustomsOffice);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			header = newFactory.Load<AsycudaManifestHeader>(header.PK);
			AssertEquals(new ZDateTime(1990, 1, 1), header.DateAtCustomsOffice);
		}

		public void TestNatureIsDefaultedBasedOnManifestTypeForSG()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			AssertEquals("AMA_Nature should default", "IMP", header.AMA_Nature);

			header.AMA_ManifestType = "MGE";
			AssertEquals("AMA_Nature should default", "EXP", header.AMA_Nature);
		}

		public void TestHeaderDelete()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "BD";
			header.RegistrationNumber = "ENT001";
			Factory.Save();
			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, header.PK);
			var entryNums = Factory.Load<CusEntryNumber>(query);
			AssertEquals(1, entryNums.Length);
			AssertEquals("ENT001", entryNums[0].CE_EntryNum);
			header.Delete();
			Factory.Save();
			entryNums = Factory.Load<CusEntryNumber>(query);
			AssertEquals(0, entryNums.Length);
		}

		public void TestIsPartOfEuropeanUnion()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var zzDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, parent: zzDataGrouping);
			Factory.Save();

			var headerZA = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			AssertNull("ParentDataGrouping", headerZA.ParentDataGrouping);
			Assert("IsPartOfEuropeanUnion", !headerZA.IsPartOfEuropeanUnion);

			var headerGB = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedKingdom, "ICS");
			headerGB.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedKingdom;
			AssertEquals("ParentDataGrouping", "EUN", headerGB.ParentDataGrouping.ZZZ_DataGrouping);
			Assert("IsPartOfEuropeanUnion", headerGB.IsPartOfEuropeanUnion);
		}

		public void TestIsRoutingEnabledDefault()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertEquals("Routing Tab is enabled by default", true, header.IsRoutingEnabled);
		}

		public void TestDefaultNatureFromConsol()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();

			var sourceConsol = Factory.New<ForwardingConsol>();
			sourceConsol.Transports.RemoveAndDeleteAll();
			sourceConsol.Transports.AddNew("BDJNB", "VUVLI");
			sourceConsol.Transports.AddNew("VUVLI", "LKCMB");

			manifestHeader.SetParent(sourceConsol);
			manifestHeader.AMA_RN_NKCountry = "ER";
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("No matching Transport is TSS", ShipmentTypeList.Codes.Transhipment28, manifestHeader.AMA_Nature);

			manifestHeader.AMA_RN_NKCountry = "BD";
			manifestHeader.AMA_Nature = ZString.Empty;
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Load and no Discharge is EXP", ShipmentTypeList.Codes.Export22, manifestHeader.AMA_Nature);

			manifestHeader.AMA_RN_NKCountry = "LK";
			manifestHeader.AMA_Nature = ZString.Empty;
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Discharge and no Load is IMP", ShipmentTypeList.Codes.Import23, manifestHeader.AMA_Nature);

			manifestHeader.AMA_RN_NKCountry = "VU";
			manifestHeader.AMA_Nature = ZString.Empty;
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Both Load and Discharge is TSS", ShipmentTypeList.Codes.Transhipment28, manifestHeader.AMA_Nature);
		}

		public void TestGetAsycudaManifestHeaderProcessTaskCollection()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType("WorkflowItems's type should be ProcessTaskCollection<AsycudaManifestHeaderProcessTask, AsycudaManifestHeader>", typeof(ProcessTaskCollection<AsycudaManifestHeaderProcessTask, AsycudaManifestHeader>), ((IWorkflowProvider)header).WorkflowItems);
		}

		public void TestDescriptionPropertyAttribute()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(header.HumanReadableName, DescriptionPropertyAttribute.DescriptionFromBusinessObject(header));
		}

		[ExpectNoExceptions]
		public void TestNoTypeDeciderIssuesForBase()
		{
			var header = Factory.New<AsycudaManifestHeaderForTest>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "CONT1234567";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "HB123";
			var pack = bill.Packs.AddNew();
			pack.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			pack.APA_GoodsDescription = "GOOD";
			var packItem = pack.PackedItems.AddNewPackedItem();
			packItem.API_CustomsQty = 1m;
			pack.ContainerPK = container.PK;
			var billNumber = bill.CustomsEntryNumbers.AddNew();
			billNumber.CE_EntryNum = "ABL123";
			var packPivot = billNumber.PackPivots.AddPivotFor(pack);
			var packNumber = packItem.CustomsEntryNumbers.AddNew();
			packNumber.CE_EntryNum = "API123";
			var person = header.Persons.AddNew();
			var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
			person.CPN_PER_Person = glbPerson.PK;
			var personCountry = person.Countries.AddNew();
			personCountry.CPC_Type = "DSA";
			personCountry.CPC_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			personCountry.CPC_Value = "SD";
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			header = factory2.Load<AsycudaManifestHeaderForTest>(header.PK);
			container = header.Containers[0];
			person = header.Persons[0];
			personCountry = person.Countries[0];
			bill = header.Bills[0];
			billNumber = bill.CustomsEntryNumbers[0];
			packPivot = billNumber.PackPivots[0];
			pack = bill.Packs[0];
			pack.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			packItem = (AsycudaPackedItem)pack.PackedItems[0].PackedItem;
			packNumber = packItem.CustomsEntryNumbers[0];
		}

		[ExpectNoExceptions]
		public void TestWrappedPropertyInfoAfterDelete()
		{
			var header = Factory.New<AsycudaManifestHeaderForTest>();
			header.ShouldPropertiesBeReadOnly(header.AMA_MasterBillIssueDateInfo.PropertyDescriptor);
			header.ShouldPropertiesBeReadOnly(header.AMA_E_ARVInfo.PropertyDescriptor);
			header.ShouldPropertiesBeReadOnly(header.AMA_E_DEPInfo.PropertyDescriptor);
			header.ShouldPropertiesBeReadOnly(header.AMA_A_DEPInfo.PropertyDescriptor);
			header.ShouldPropertiesBeReadOnly(header.AMA_A_ARVInfo.PropertyDescriptor);
			header.ShouldPropertiesBeReadOnly(header.AMA_RL_NKPortOfDischargeInfo.PropertyDescriptor);
			header.ShouldPropertiesBeReadOnly(header.AMA_CustomsOriginPortInfo.PropertyDescriptor);
			header.ShouldPropertiesBeReadOnly(header.AMA_CustomsLoadPortInfo.PropertyDescriptor);
			header.ShouldPropertiesBeReadOnly(header.AMA_CustomsDischargePortInfo.PropertyDescriptor);
			header.ShouldPropertiesBeReadOnly(header.AMA_CarrierReferenceInfo.PropertyDescriptor);
			header.ShouldPropertiesBeReadOnly(header.AMA_RL_NKPortOfLoadingInfo.PropertyDescriptor);
			header.ShouldPropertiesBeReadOnly(header.AMA_RL_NKOriginInfo.PropertyDescriptor);
			header.ShouldPropertiesBeReadOnly(header.AMA_RL_NKFinalDestinationInfo.PropertyDescriptor);
			header.ShouldPropertiesBeReadOnly(header.AMA_GoodsDescriptionInfo.PropertyDescriptor);

			header.Delete();
			header.ShouldPropertiesBeReadOnly(header.AMA_E_ARVInfo.PropertyDescriptor);
			header.ShouldPropertiesBeReadOnly(header.AMA_E_DEPInfo.PropertyDescriptor);
			header.ShouldPropertiesBeReadOnly(header.AMA_A_DEPInfo.PropertyDescriptor);
			header.ShouldPropertiesBeReadOnly(header.AMA_A_ARVInfo.PropertyDescriptor);
			header.ShouldPropertiesBeReadOnly(header.AMA_RL_NKPortOfDischargeInfo.PropertyDescriptor);
			header.ShouldPropertiesBeReadOnly(header.AMA_CustomsOriginPortInfo.PropertyDescriptor);
			header.ShouldPropertiesBeReadOnly(header.AMA_CustomsLoadPortInfo.PropertyDescriptor);
			header.ShouldPropertiesBeReadOnly(header.AMA_CustomsDischargePortInfo.PropertyDescriptor);
			header.ShouldPropertiesBeReadOnly(header.AMA_CarrierReferenceInfo.PropertyDescriptor);
			header.ShouldPropertiesBeReadOnly(header.AMA_RL_NKPortOfLoadingInfo.PropertyDescriptor);
			header.ShouldPropertiesBeReadOnly(header.AMA_RL_NKOriginInfo.PropertyDescriptor);
			header.ShouldPropertiesBeReadOnly(header.AMA_RL_NKFinalDestinationInfo.PropertyDescriptor);
			header.ShouldPropertiesBeReadOnly(header.AMA_GoodsDescriptionInfo.PropertyDescriptor);

			// Test accessing these properties after delete should not throw exceptions
			var totalHouseBillsGrossWeight = header.TotalHouseBillsGrossWeight;
			Assert(totalHouseBillsGrossWeight == ZInt.Zero);
			var totalHouseBillsPackages = header.TotalHouseBillsPackages;
			Assert(totalHouseBillsPackages == ZInt.Zero);
			var customsDischargePort = header.AMA_CustomsDischargePort;
			Assert(customsDischargePort == ZString.Empty);
			var carrierReference = header.AMA_CarrierReference;
			Assert(carrierReference == ZString.Empty);
		}

		public void TestAMA_MessageStatus_ReadOnly()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			Assert(header.AMA_MessageStatusInfo.ReadOnly);
		}

		public void TestShippingAgentOrgAddress()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = CreateAddress(org1, "111", "Address11", "Address12", "City1", "State1", "CpName1");

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_OA_ShippingAgent = address1.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			header = newFactory.Load<AsycudaManifestHeader>(header.PK);
			AssertEquals("Shipping Agent", org1.PK, header.ShippingAgentOrgPK);
			AssertEquals("Shipping AgentAddress", address1.PK, header.AMA_OA_ShippingAgent);
		}

		public void TestShippingAgentOrgReadOnly()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = CreateAddress(org1, "111", "Address11", "Address12", "City1", "State1", "CpName1");

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_HouseBill = "TEST_1";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.Add(shipment);
			Factory.Save();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_OA_ShippingAgent = address1.PK;

			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();
			consol.JK_MasterBillNum = "BOL123456";
			Factory.Save();

			var headerReloaded = Factory.Load<AsycudaManifestHeader>(manifestHeader.PK);
			headerReloaded.Synchroniser.SetEnabled(true, false);
			headerReloaded.Synchroniser.Synchronise();

			headerReloaded.AMA_OverrideFreightDefaults = true;
			headerReloaded.Synchroniser.Synchronise();
			AssertEquals("Address Sync", headerReloaded.AMA_OverrideFreightDefaults, !headerReloaded.ShippingAgentOrgPKReadOnly);
		}

		public void TestIsTSS()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Transhipment28;
			AssertEquals(true, header.IsTSS);

			header.AMA_Nature = ZString.Empty;
			AssertEquals(false, header.IsTSS);
		}

		public void TestAMA_NatureDescription()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			AssertEquals(ShipmentTypeList.Descriptions.Export22, header.AMA_NatureDescription);
		}

		public void TestResetManifestType()
		{
			var currentManifestType = string.Empty;
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			header.AMA_ManifestTypeInfo.ValueChanged += (o, e) => currentManifestType = header.ManifestType.Code;
			header.AMA_ManifestType = "COH";
			AssertEquals("ManifestType should be reset", "COH", currentManifestType);
		}

		public void TestAMA_CustomsOfficeDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");

			helper.CreateNewOrGetExistingCusCodeList("ZZ", universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "VU", "Vanuatu", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var vuSAIR = helper.CreateNewOrGetExistingCusCodeList("VU", universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "SAIR", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuSAIR.PK, "PORT", "VUSAN");
			helper.CreateTransportModeForCusCodeList(vuSAIR.PK, "AIR");

			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "VUSAN";
			header.AMA_RN_NKCountry = "VU";
			header.AMA_CustomsOffice = "SAIR";
			AssertEquals("Customs Office", header.AMA_CustomsOfficeDescription);
		}

		public void TestAMA_ManifestTypeDescription()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header.AMA_TransportMode = "SEA";
			header.AMA_ManifestType = "COH";

			AssertEquals("Container House", header.AMA_ManifestTypeDescription);
		}

		public void TestCarrierCCCCode()
		{
			var carrierOrgAddress = Factory.Load<OrgAddress>(new ZGuid("0EA85FB9-EC2E-4713-8A96-85B6290E97BB"));
			carrierOrgAddress.Header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OrgCCC", "ZA");
			Factory.Save();

			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			header.AMA_OA_Carrier = carrierOrgAddress.PK;

			AssertEquals("OrgCCC", header.CarrierCCCCode);

			carrierOrgAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "AddressCCCTW", "TW");
			carrierOrgAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "AddressCCCZA", "ZA");

			AssertEquals("AddressCCCZA", header.CarrierCCCCode);
		}

		public void TestIsOverrideFreightDefaultsVisible()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Eritrea, "ASY");
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			Assert("Should be false as the consol and sailing are all null.", !header.IsOverrideFreightDefaultsVisible);

			var consol = Factory.New<ForwardingConsol>();
			header.AMA_ParentId = consol.PK;
			header.AMA_ParentTableCode = JobConsolSchema.Constants.Prefix;

			Assert("Should be true as the consol is not null.", header.IsOverrideFreightDefaultsVisible);

			var voyage = GetVoyageWithSailing();
			var sailing = voyage.Sailings[0];

			header.ChangeSailing(sailing.PK);

			Assert("Should be true as the sailing is not null and the transport mode is sea.", header.IsOverrideFreightDefaultsVisible);

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			Assert("Should be false as the consol is null and the transport mode is not sea.", !header.IsOverrideFreightDefaultsVisible);
		}

		public void TestReadOnlyOfPropertiesFromSailing()
		{
			var voyage = GetVoyageWithSailing();
			var sailing = voyage.Sailings[0];

			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Eritrea, "ASY");
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			var propertyInfos = new[]
			{
				header.AMA_VoyageInfo,
				header.AMA_VesselNameInfo,
				header.AMA_RL_NKPortOfLoadingInfo,
				header.AMA_RL_NKPortOfDischargeInfo,
				header.AMA_E_DEPInfo,
				header.AMA_E_ARVInfo,
				header.AMA_OA_CarrierInfo,
				header.AMA_RN_NKConveyanceNationalityInfo
			};

			void AssertReadOnlyOfProperties(bool isReadOnly)
			{
				CombineAssertions(() =>
				{
					foreach (var info in propertyInfos)
					{
						var message = $"The readonly of {info.Name} should be {isReadOnly} as the ShouldSynchroniseWithSailing is {isReadOnly}.";
						AssertEquals(message, isReadOnly, info.ReadOnly);
					}
				});
			}

			Assert("Should be false as the sailing is null.", !header.ShouldSynchroniseWithSailing);
			AssertReadOnlyOfProperties(false);

			header.ChangeSailing(sailing.PK);

			Assert("Should be true as the sailing is not null and the transport mode is sea.", header.ShouldSynchroniseWithSailing);
			AssertReadOnlyOfProperties(true);

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			Assert("Should be false as the transport mode is not sea.", !header.ShouldSynchroniseWithSailing);
			AssertReadOnlyOfProperties(false);
		}

		public void TestReadOnlyOfPropertiesFromConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var header = Factory.New<AsycudaManifestHeader>();

			var propertyInfos = new[]
			{
				header.AMA_VoyageInfo,
				header.AMA_VesselNameInfo,
				header.AMA_RL_NKPortOfLoadingInfo,
				header.AMA_RL_NKPortOfDischargeInfo,
				header.AMA_E_DEPInfo,
				header.AMA_E_ARVInfo,
				header.AMA_OA_CarrierInfo
			};

			void AssertReadOnlyOfProperties(bool isReadOnly)
			{
				CombineAssertions(() =>
				{
					foreach (var info in propertyInfos)
					{
						var message = $"The readonly of {info.Name} should be {isReadOnly} as the ShouldSynchroniseWithConsolOrSailing is {isReadOnly}.";
						AssertEquals(message, isReadOnly, info.ReadOnly);
					}
				});
			}

			Assert("Should be false as the consol is null.", !header.ShouldSynchroniseWithConsolOrSailing);
			AssertReadOnlyOfProperties(false);

			header.AMA_ParentId = consol.PK;
			header.AMA_ParentTableCode = consol.TablePrefix;

			Assert("Should be true as the consol is not null.", header.ShouldSynchroniseWithConsolOrSailing);
			AssertReadOnlyOfProperties(true);

			Assert("Should be false as the sailing is null.", !header.ShouldSynchroniseWithSailing);
			Assert("AMA_RN_NKConveyanceNationalityInfo only be readonly when the ShouldSynchroniseWithSailing is true.", !header.AMA_RN_NKConveyanceNationalityInfo.ReadOnly);
		}

		public void TestSailingStatistics()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			CombineAssertions(() =>
			{
				AssertEquals("Should equal to the count of bills.", 0, header.AMA_NoOfBills);
				AssertEquals("Should equal to the count of bill of ladings.", 0, header.AMA_NoOfSailingBills);
				AssertEquals("Should equal to the count of containers.", 0, header.AMA_NoOfContainers);
				AssertEquals("Should equal to the count of containers on bill of ladings.", 0, header.AMA_NoOfSailingContainers);
			});

			header.Bills.AddNew();

			header.Containers.AddNew();
			header.Containers.AddNew();

			var voyage = GetVoyageWithSailing();
			var sailing = voyage.Sailings[0];

			var billOfLading1 = Factory.New<BillOfLading>();
			billOfLading1.JS_JX = sailing.PK;
			billOfLading1.JS_HouseBill = "SCACAAA";

			billOfLading1.RealContainers.AddNew();
			billOfLading1.RealContainers.AddNew();

			var billOfLading2 = Factory.New<BillOfLading>();
			billOfLading2.JS_JX = sailing.PK;
			billOfLading2.JS_HouseBill = "SCACBBB";

			billOfLading2.RealContainers.AddNew();

			var billOfLading3 = Factory.New<BillOfLading>();
			billOfLading3.JS_JX = sailing.PK;
			billOfLading3.JS_HouseBill = "SCACCCC";
			billOfLading3.JS_PackingMode = "BBK";

			billOfLading3.TopLevelPacks.AddNew();

			header.ChangeSailing(sailing.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Should equal to the count of bills.", 1, header.AMA_NoOfBills);
				AssertEquals("Should equal to the count of bill of ladings.", 3, header.AMA_NoOfSailingBills);
				AssertEquals("Should equal to the count of containers.", 2, header.AMA_NoOfContainers);
				AssertEquals("Should equal to the count of containers on bill of ladings.", 3, header.AMA_NoOfSailingContainers);
			});
		}

		public void TestGetNewMessageChooser()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals(typeof(MessageChooser), header.GetNewMessageChooser(new[] { bill }, string.Empty, false).GetType());
		}

		public void TestDeletionInCorrectOrder()
		{
			var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			header.MasterBill.ABL_BillNumber = "MASTER1";
			var bill = header.Bills.AddNew();
			var container = header.Containers.AddNew();
			var person = header.Persons.AddNew();
			person.CPN_PER_Person = glbPerson.PK;
			var message = header.Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var milestone = header.WorkflowItems.Milestones.AddNew();
			milestone.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
			var registrationNumber = CusEntryNumber.New<ABLEntryNum>(header, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.Eritrea);

			Factory.Save();

			header.MasterBill.NotificationsChanged += (object sender, NotificationsChangedEventArgs e) =>
			{
				if (header.IsDeleted)
				{
					Assert("This is wrong MasterBill should not be deleted after the Header", false);
				}
			};
			bill.NotificationsChanged += (object sender, NotificationsChangedEventArgs e) =>
			{
				if (header.IsDeleted)
				{
					Assert("This is wrong Bill should not be deleted after the Header", false);
				}
			};
			person.NotificationsChanged += (object sender, NotificationsChangedEventArgs e) =>
			{
				if (header.IsDeleted)
				{
					Assert("This is wrong Persons should not be deleted after the Header", false);
				}
			};
			message.NotificationsChanged += (object sender, NotificationsChangedEventArgs e) =>
			{
				if (header.IsDeleted)
				{
					Assert("This is wrong Messages should not be deleted after the Header", false);
				}
			};
			registrationNumber.NotificationsChanged += (object sender, NotificationsChangedEventArgs e) =>
			{
				if (header.IsDeleted)
				{
					Assert("This is wrong Entry Numbers should not be deleted after the Header", false);
				}
			};

			header.Delete();
			AssertNoExceptionThrown(Factory.Save);
			AssertEquals(true, milestone.IsDeleted); //There are already processTasks in the test database
			AssertEquals(0, Factory.GetDatabaseCount(typeof(CusPerson)));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIMessage)));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(CusEntryNumber)));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(AsycudaBill)));
		}

		public void TestAMA_OA_DeconsolidateAddress_ReadOnly()
		{
			foreach (var testCase in new[]
			{
				new { ManifestType = nameof(ManifestDocumentType.COM), Enabled = false },
				new { ManifestType = nameof(ManifestDocumentType.COH), Enabled = true },
				new { ManifestType = nameof(ManifestDocumentType.BBB), Enabled = true },
				new { ManifestType = nameof(ManifestDocumentType.ECL), Enabled = false },
				new { ManifestType = nameof(ManifestDocumentType.FFM), Enabled = false },
				new { ManifestType = nameof(ManifestDocumentType.FWB), Enabled = true },
				new { ManifestType = nameof(ManifestDocumentType.HAB), Enabled = true },
				new { ManifestType = nameof(ManifestDocumentType.RMA), Enabled = false },
				new { ManifestType = nameof(ManifestDocumentType.RFM), Enabled = false },
				new { ManifestType = nameof(ManifestDocumentType.AQM), Enabled = false },
				new { ManifestType = nameof(ManifestDocumentType.ALM), Enabled = false },
				new { ManifestType = nameof(ManifestDocumentType.ALH), Enabled = true },
			})
			{
				var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				header.AMA_Nature = ShipmentTypeList.Codes.Import23;
				header.AMA_ManifestType = testCase.ManifestType;
				AssertEquals(!testCase.Enabled, header.AMA_OA_DeconsolidateAddressInfo.ReadOnly);
			}
		}

		public void TestAMA_OA_DischargeTerminalAddress_ReadOnly()
		{
			foreach (var testCase in new[]
			{
				new { ManifestType = nameof(ManifestDocumentType.COM), Enabled = false },
				new { ManifestType = nameof(ManifestDocumentType.COH), Enabled = true },
				new { ManifestType = nameof(ManifestDocumentType.BBB), Enabled = true },
				new { ManifestType = nameof(ManifestDocumentType.ECL), Enabled = false },
				new { ManifestType = nameof(ManifestDocumentType.FFM), Enabled = false },
				new { ManifestType = nameof(ManifestDocumentType.FWB), Enabled = true },
				new { ManifestType = nameof(ManifestDocumentType.HAB), Enabled = true },
				new { ManifestType = nameof(ManifestDocumentType.RMA), Enabled = false },
				new { ManifestType = nameof(ManifestDocumentType.RFM), Enabled = false },
				new { ManifestType = nameof(ManifestDocumentType.AQM), Enabled = false },
				new { ManifestType = nameof(ManifestDocumentType.ALM), Enabled = false },
				new { ManifestType = nameof(ManifestDocumentType.ALH), Enabled = true },
			})
			{
				var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				header.AMA_Nature = ShipmentTypeList.Codes.Import23;
				header.AMA_ManifestType = testCase.ManifestType;
				AssertEquals(!testCase.Enabled, header.AMA_OA_DischargeTerminalAddressInfo.ReadOnly);
			}
		}

		public void TestAreDeconsolidatorAndDischargeTerminalEnabled()
		{
			foreach (var testCase in new[]
			{
				new { ManifestType = nameof(ManifestDocumentType.COM), Enabled = false },
				new { ManifestType = nameof(ManifestDocumentType.COH), Enabled = true },
				new { ManifestType = nameof(ManifestDocumentType.BBB), Enabled = true },
				new { ManifestType = nameof(ManifestDocumentType.ECL), Enabled = false },
				new { ManifestType = nameof(ManifestDocumentType.FFM), Enabled = false },
				new { ManifestType = nameof(ManifestDocumentType.FWB), Enabled = true },
				new { ManifestType = nameof(ManifestDocumentType.HAB), Enabled = true },
				new { ManifestType = nameof(ManifestDocumentType.RMA), Enabled = false },
				new { ManifestType = nameof(ManifestDocumentType.RFM), Enabled = false },
				new { ManifestType = nameof(ManifestDocumentType.AQM), Enabled = false },
				new { ManifestType = nameof(ManifestDocumentType.ALM), Enabled = false },
				new { ManifestType = nameof(ManifestDocumentType.ALH), Enabled = true },
			})
			{
				var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				header.AMA_Nature = ShipmentTypeList.Codes.Import23;
				header.AMA_ManifestType = testCase.ManifestType;
				AssertEquals(testCase.Enabled, header.IsDeconsolidatorEnabled);
				AssertEquals(testCase.Enabled, header.IsDischargeTerminalEnabled);
			}
		}

		public void TestIsContainerized()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ContainerMode = ZString.Empty;
			AssertEquals(false, header.IsContainerized);

			header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals(true, header.IsContainerized);
		}

		public void TestDocumentSupporter()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertType<AsycudaManifestHeaderDocumentSupporter>(header.DocumentSupporter);
		}

		public void TestMasterBol()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");

			var validationRule = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.MasterBOL, "MasterBOL", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(ManifestValidationRuleCodes.MANDATORYFORAGENTTYPE, "Desc.", RefCusCodeListTypes.Codes.ManifestValidationRule, Core.Constants.CountryCodes.SouthAfrica, RefCusCodeListTypes.Codes.ManifestValidationRule);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(ManifestValidationRuleCodes.MANDATORYFORMESSAGETYPE, "Desc.", RefCusCodeListTypes.Codes.ManifestValidationRule, Core.Constants.CountryCodes.SouthAfrica, RefCusCodeListTypes.Codes.ManifestValidationRule);
			validationRule.Attributes.AddNew(ManifestValidationRuleCodes.MANDATORYFORAGENTTYPE, Core.Constants.AgentType.CoLoad);
			validationRule.Attributes.AddNew(ManifestValidationRuleCodes.MANDATORYFORMESSAGETYPE, nameof(ManifestDocumentType.COH));
			validationRule.Attributes.AddNew(ManifestValidationRuleCodes.MANDATORYFORMESSAGETYPE, nameof(ManifestDocumentType.HAB));
			validationRule.Attributes.AddNew(ManifestValidationRuleCodes.MANDATORYFORMESSAGETYPE, nameof(ManifestDocumentType.ALH));
			Factory.Save();

			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			header.FillWithValidTestData();
			header.AMA_AgentType = Core.Constants.AgentType.CoLoad;
			header.MasterBOL = "123";

			header.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			AssertEquals(true, header.MasterBOLVisible);

			header.AMA_AgentType = Core.Constants.AgentType.AWBMaster;
			AssertEquals(ZString.Empty, header.MasterBOL);
			AssertEquals(false, header.MasterBOLVisible);
		}

		public void TestHasManifestBeenSubmittedToCustomsIncludingChildren()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			AssertEquals(false, header.HasManifestBeenSubmittedToCustomsIncludingChildren);
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var pack1 = bill1.Packs.AddNew();
			var pack1PackedItem = pack1.PackedItemForTesting();
			AssertEquals(false, header.HasManifestBeenSubmittedToCustomsIncludingChildren);
			pack1PackedItem.API_MessageStatus = MessageStatusCodeList.Codes.Awaiting;
			AssertEquals(true, header.HasManifestBeenSubmittedToCustomsIncludingChildren);
			pack1PackedItem.API_MessageStatus = MessageStatusCodeList.Codes.Error;
			AssertEquals(false, header.HasManifestBeenSubmittedToCustomsIncludingChildren);

			AssertEquals(false, header.HasManifestBeenSubmittedToCustomsIncludingChildren);
			bill2.ABL_MessageStatus = MessageStatusCodeList.Codes.Awaiting;
			AssertEquals(true, header.HasManifestBeenSubmittedToCustomsIncludingChildren);
			bill2.ABL_MessageStatus = MessageStatusCodeList.Codes.Error;
			AssertEquals(false, header.HasManifestBeenSubmittedToCustomsIncludingChildren);
		}

		public void TestWhenChangeManifestTypeShouldClearBillIssuerCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCarrierCode("CCC", "CarrierCode", "ZA");
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "Manifest Validation");
			var validationRuleZa = helper.CreateNewOrGetExistingCusCodeList("ZA", RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.BillIssuer, "A Bill issuer is required", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(validationRuleZa.PK, "MANDATORYFORMESSAGETYPE", nameof(ManifestDocumentType.HAB));
			helper.CreateNewOrGetExistingCusCodeListAttribute(validationRuleZa.PK, "MANDATORYFORMANIFESTTYPE", nameof(ManifestDocumentType.COH));
			Factory.Save();

			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			AssertEquals(true, bill1.IsIssuerCodeMandatory);
			AssertEquals(true, bill2.IsIssuerCodeMandatory);

			bill1.Validation.ValidateAll();
			bill2.Validation.ValidateAll();
			AssertEquals(true, bill1.ABL_BillIssuerInfo.HasMessageErrors());
			AssertEquals(true, bill2.ABL_BillIssuerInfo.HasMessageErrors());

			bill1.ABL_BillIssuer = "1";
			bill2.ABL_BillIssuer = "2";

			header.AMA_ManifestType = nameof(ManifestDocumentType.COH);

			AssertEquals("1", bill1.ABL_BillIssuer);
			AssertEquals("2", bill2.ABL_BillIssuer);

			header.AMA_ManifestType = nameof(ManifestDocumentType.ALM);

			AssertEquals(string.Empty, bill1.ABL_BillIssuer);
			AssertEquals(string.Empty, bill2.ABL_BillIssuer);
			AssertEquals(false, bill1.ABL_BillIssuerInfo.HasMessageErrors());
			AssertEquals(false, bill2.ABL_BillIssuerInfo.HasMessageErrors());
		}

		public void TestIsLargeJobWithLotsOfBillsOrPackLinesSoDontTryToConvertXmlToHtml()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertEquals(false, header.IsLargeJobWithLotsOfBillsOrPackLinesSoDontTryToConvertXmlToHtml());
			for (int i = 0; i < 100; i++)
			{
				header.Bills.AddNew().Packs.AddNew();  // new bill and new pack
			}
			AssertEquals(false, header.IsLargeJobWithLotsOfBillsOrPackLinesSoDontTryToConvertXmlToHtml());
			var bill101 = header.Bills.AddNew();
			AssertEquals(true, header.IsLargeJobWithLotsOfBillsOrPackLinesSoDontTryToConvertXmlToHtml());
			bill101.Delete();
			AssertEquals(false, header.IsLargeJobWithLotsOfBillsOrPackLinesSoDontTryToConvertXmlToHtml());
			var anyBill = header.Bills[0];
			AssertEquals(false, header.IsLargeJobWithLotsOfBillsOrPackLinesSoDontTryToConvertXmlToHtml());
			anyBill.Packs.AddNew();
			anyBill.Packs.AddNew();
			AssertEquals(true, header.IsLargeJobWithLotsOfBillsOrPackLinesSoDontTryToConvertXmlToHtml());
		}

		public void TestSetParent()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var consol = Factory.New<ForwardingConsol>();
			header.SetParent(consol);

			AssertEquals(consol.PK, header.AMA_ParentId);
			AssertEquals("JK", header.AMA_ParentTableCode);
		}

		public void TestEnableBillsLock()
		{
			var helper = new ZZDataTestHelper(Factory);
			Factory.Save();

			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.FillWithValidTestData();
			header.AMA_TransportMode = "AIR";
			header.AMA_ManifestType = "MGI";

			var bill = header.Bills.AddNew();
			bill.ABL_ShipmentType = Universal.Helper.ShipmentTypeList.Codes.Export22;

			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemForTesting();
			AssertEquals(false, bill.ReadOnly);
			AssertEquals(bill.ABL_RemarksInfo.Name, false, bill.ABL_RemarksInfo.ReadOnly);
			AssertEquals(false, pack.ReadOnly);

			packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Sent;
			AssertEquals(false, bill.ReadOnly);
			AssertEquals(bill.ABL_RemarksInfo.Name, false, bill.ABL_RemarksInfo.ReadOnly);
			AssertEquals(false, pack.ReadOnly);

			var pack1 = bill.Packs.AddNew();
			var packedItem1 = pack1.PackedItemForTesting();

			header.EnableBillsLock(true);
			Assert(!bill.ReadOnly);
			foreach (ZPropertyInfo propInfo in bill.ZPropertyInfoHash)
			{
				AssertEquals(propInfo.Name, propInfo.Name != bill.ABL_RemarksInfo.Name, propInfo.ReadOnly);
			}

			packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Updated;
			Assert(!bill.ReadOnly);
			Assert(bill.ABL_RemarksInfo.Name, !bill.ABL_RemarksInfo.ReadOnly);

			packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Sent;
			packedItem1.API_MessageStatus = MessageStatusCodeList.Codes.Error;
			Assert(bill.ReadOnly);
			Assert(pack.ReadOnly);
			Assert(!pack1.ReadOnly);

			header.EnableBillsLock(false);

			var alwaysOrConditionalReadOnly = new HashSet<ZString>()
			{
				bill.ABL_CustomsValueInfo.Name,
				bill.ABL_RX_NKCustomsValueCurrencyInfo.Name,
				"ShortStatusDescription",
				"SG_PartyStatus",
				"SG_PayeeIndicator",
				"StatusDescription",
				"BillIssuerName",
				"DutyAmount",
				"TaxAmount",
				"CustomsJobNumber",
				"RegistrationNumber",
				"RegistrationDate",
				"ABL_BillStatusDescription",
				"ABL_MessageStatus",
				"ABL_ShipmentType",
				"CountryCode"
			};

			VoidParameterlessDelegate tests = () => { };

			foreach (ZPropertyInfo propInfo in bill.ZPropertyInfoHash)
			{
				tests += () => { Assert(propInfo.Name, !propInfo.ReadOnly || alwaysOrConditionalReadOnly.Contains(propInfo.Name)); };
			}

			CombineAssertions(tests);
		}

		public void TestEnableBillsLockFetchStrategy()
		{
			var helper = new ZZDataTestHelper(Factory);
			Factory.Save();

			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGI");
			header.FillWithValidTestData();
			for (var i = 1; i <= 10; i++)
			{
				var billCountAsString = i.ToString();
				var bill = header.Bills.AddNew();
				bill.ABL_BillNumber = "TEST" + billCountAsString;
				bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Sent;
				var billEntryNumber = bill.CustomsEntryNumbers.AddNew();
				billEntryNumber.CE_EntryNum = "BILLENTRY" + billCountAsString;

				for (var j = 1; j <= 10; j++)
				{
					var pack = bill.Packs.AddNew();
					var packedItem = pack.PackedItemForTesting();
					packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Sent;
					var packEntryNumber = packedItem.CustomsEntryNumbers.AddNew();
					packEntryNumber.CE_EntryNum = "PACKENTRY" + j.ToString();
				}
			}
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var headerReloaded = newFactory.Load<AsycudaManifestHeader>(header.PK);
			headerReloaded.EnableBillsLock(true);
			AssertEquals(1, newFactory.GetTableHitCount(AsycudaBillSchema.Constants.TableName));
			AssertEquals(1, newFactory.GetTableHitCount(AsycudaPackSchema.Constants.TableName));
			AssertEquals(1, newFactory.GetTableHitCount(AsycudaPackedItemSchema.Constants.TableName));
			AssertEquals(1, newFactory.GetTableHitCount(CusEntryNumSchema.Constants.TableName));
		}

		public void TestTransportModeChangeDeletesPersons()
		{
			var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("Road requires Persons Tab", true, header.NeedPersonsTab);
			var person = header.Persons.AddNew();
			person.CPN_PER_Person = glbPerson.PK;
			Factory.Save();
			var invalidPerson = header.Persons.AddNew();
			invalidPerson.CPN_PER_Person = ZGuid.NewZGuid();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Air requires no person", false, header.NeedPersonsTab);
			AssertEquals("Person is deleted", true, person.IsDeleted);
			AssertEquals("Invalid Person is deleted", true, invalidPerson.IsDeleted);
			AssertEquals("Person collection is empty", 0, header.Persons.Count);
			AssertNoExceptionThrown("No Foreign Key Constraint Exception as the Invalid Person is gone", () => Factory.Save());
		}

		public void TestIsTransportModes()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertEquals(false, header.IsAir);
			AssertEquals(false, header.IsSea);
			AssertEquals(false, header.IsRoad);
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(true, header.IsAir);
			AssertEquals(false, header.IsSea);
			AssertEquals(false, header.IsRoad);
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(false, header.IsAir);
			AssertEquals(true, header.IsSea);
			AssertEquals(false, header.IsRoad);
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals(false, header.IsAir);
			AssertEquals(false, header.IsSea);
			AssertEquals(true, header.IsRoad);
		}

		public void TestAMA_TransportModeDescription()
		{
			AsycudaManifestHeaderLookupsTest.EnsureOrCreateRefZZRecordsToControlVisibilityBasedOnModeAndCountry(Factory, new ModeAndCountry("AIR", "VU"), new ModeAndCountry("SEA", "VU"));
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Vanuatu;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(Core.Constants.TransportModeDescriptions.Air, header.AMA_TransportModeDescription);
		}

		public void TestLabels()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertEquals("Flight/Voyage", header.VoyageFlightNoLabel.Caption);
			AssertEquals("Manifest No.", header.MasterBillLabel.Caption);
			header.AMA_TransportMode = "AIR";
			AssertEquals("Flight", header.VoyageFlightNoLabel.Caption);
			AssertEquals("MAWB", header.MasterBillLabel.Caption);
			header.AMA_TransportMode = "SEA";
			AssertEquals("Voyage", header.VoyageFlightNoLabel.Caption);
			AssertEquals("BOL", header.MasterBillLabel.Caption);
		}

		public void TestMasterBillShouldLoadOldest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			header.AMA_MasterBill = "MB0";
			var bill0 = header.MasterBill;
			bill0.ABL_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(2);
			var bill1 = Factory.New<AsycudaBill>();
			bill1.ABL_BolType = AsycudaBill.ChildBolCode;
			bill1.ABL_BillNumber = "MB1";
			bill1.ABL_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(10);
			bill1.ABL_AMA = header.PK;
			var bill2 = Factory.New<AsycudaBill>();
			bill2.ABL_BolType = AsycudaBill.ChildBolCode;
			bill2.ABL_BillNumber = "MB2";
			bill2.ABL_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(1);
			bill2.ABL_AMA = header.PK;
			var bill3 = Factory.New<AsycudaBill>();
			bill3.ABL_BolType = AsycudaBill.ChildBolCode;
			bill3.ABL_BillNumber = "MB3";
			bill3.ABL_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(11);
			bill3.ABL_AMA = header.PK;
			AssertEquals("header.AMA_MasterBill", "MB0", header.AMA_MasterBill);
			AssertEquals("header.MasterBill", bill0, header.MasterBill);
			Factory.Save();
			header = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			AssertEquals("header.AMA_MasterBill", "MB2", header.AMA_MasterBill);
		}

		public void TestChildBill()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "XX";

			var realBill = header.Bills.AddNew();
			header.AMA_MasterBillIssueDate = ZDate.BrettsBirthday;
			header.AMA_MasterBill = "123";
			header.AMA_E_ARV = new ZDateTime(2017, 12, 19);
			header.AMA_E_DEP = new ZDateTime(2017, 12, 13);
			header.AMA_A_DEP = new ZDateTimeOffset(2017, 12, 13);
			header.AMA_RL_NKPortOfDischarge = "CNSHA";
			header.AMA_RL_NKPortOfLoading = "AUSYD";

			Factory.Save();
			AssertEquals(2, Factory.GetDatabaseCount(typeof(AsycudaBill)));

			var headerReloaded = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			var realBillReloaded = new BusinessObjectFactory().Load<AsycudaBill>(realBill.PK);
			var query = new ZQuery(AsycudaBillSchema.PK, SQLComparisonOperator.NotEqual, realBill.PK);
			query.AddToFilter(AsycudaBillSchema.ABL_AMA, header.PK);
			var childBillReloaded = new BusinessObjectFactory().LoadTop1<AsycudaBill>(query);
			AssertEquals("Worker bill is excluded from the Bills collection", 1, headerReloaded.Bills.Count);

			AssertEquals("123", headerReloaded.AMA_MasterBill);
			AssertEquals(new ZDateTime(2017, 12, 19), headerReloaded.AMA_E_ARV);
			AssertEquals(new ZDateTime(2017, 12, 13), headerReloaded.AMA_E_DEP);
			AssertEquals(new ZDateTimeOffset(2017, 12, 13), headerReloaded.AMA_A_DEP);
			AssertEquals("CNSHA", headerReloaded.AMA_RL_NKPortOfDischarge);
			AssertEquals("AUSYD", headerReloaded.AMA_RL_NKPortOfLoading);

			AssertEquals("123", childBillReloaded.ABL_BillNumber);
			AssertEquals(new ZDateTime(2017, 12, 19), childBillReloaded.ABL_E_ARV);
			AssertEquals(new ZDateTime(2017, 12, 13), childBillReloaded.ABL_E_DEP);
			AssertEquals(new ZDateTimeOffset(2017, 12, 13), childBillReloaded.ABL_A_DEP);
			AssertEquals("CNSHA", childBillReloaded.ABL_RL_NKPortOfDischarge);
			AssertEquals("AUSYD", childBillReloaded.ABL_RL_NKPortOfLoading);

			AssertEquals(ZDateTime.BrettsBirthday, headerReloaded.AMA_MasterBillIssueDate);
			AssertEquals(ZDateTime.BrettsBirthday, childBillReloaded.ABL_BillIssueDate);
			AssertNotEquals(AsycudaBill.ChildBolCode, realBillReloaded.ABL_BolType);
			AssertEquals(false, realBillReloaded.IsChildMasterBill);
			AssertEquals(AsycudaBill.ChildBolCode, childBillReloaded.ABL_BolType);
			AssertEquals(true, childBillReloaded.IsChildMasterBill);
		}

		public void TestCanBeParentOfEDIMessage()
		{
			// NB - no longer support doing Header.Message.Add(), but if we set EM_LInk* manually then we shoudl still see EM_LinkedObject working OK
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var message = header.Messages.AddNew();
			message.EM_ReceiveTransmit = "RCV"; // to make GetMessageReferenceNumber STFU
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = AsycudaManifestHeader.Schema.TableName;
			Factory.Save();
			var messageReloaded = new BusinessObjectFactory().Load<EDIMessage>(message.PK);
			var headerReloadedViaLinkedObject = messageReloaded.EM_LinkedObject as AsycudaManifestHeader;
			AssertEquals(header.PK, headerReloadedViaLinkedObject.PK);
		}

		public void TestSetOfficeCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			var sb = helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, "SB", "Solomon Islands", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sb.PK, universalAlias.RefCusCodeListTypes.Codes.NVC, "0.0.0.0");
			var vu = helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, "VU", "Vanuatu", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(vu.PK, universalAlias.RefCusCodeListTypes.Codes.NVC, "0.0.0.0");

			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var sbHIRS = helper.CreateNewOrGetExistingCusCodeList("SB", RefCusCodeListTypes.Codes.CustomsOffice, "HIRS", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sbHIRS.PK, "PORT", "SBHIR");
			helper.CreateTransportModeForCusCodeList(sbHIRS.PK, "SEA");
			var vuVAIR = helper.CreateNewOrGetExistingCusCodeList("VU", RefCusCodeListTypes.Codes.CustomsOffice, "VAIR", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var vuSAIR = helper.CreateNewOrGetExistingCusCodeList("VU", RefCusCodeListTypes.Codes.CustomsOffice, "SAIR", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var vuSSEA = helper.CreateNewOrGetExistingCusCodeList("VU", RefCusCodeListTypes.Codes.CustomsOffice, "SSEA", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuVAIR.PK, "PORT", "VUVLI");
			helper.CreateTransportModeForCusCodeList(vuVAIR.PK, "AIR");
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuSAIR.PK, "PORT", "VUSAN");
			helper.CreateTransportModeForCusCodeList(vuSAIR.PK, "AIR");
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuSSEA.PK, "PORT", "VUSAN");
			helper.CreateTransportModeForCusCodeList(vuSSEA.PK, "SEA");
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Vanuatu;

			var consol = Factory.New<ForwardingConsol>();
			header.SetParent(consol);

			header.AMA_TransportMode = "AIR";
			header.AMA_RL_NKPortOfLoading = "AUSYD";
			header.AMA_RL_NKPortOfDischarge = "VUVLI";

			AssertEquals("VAIR", header.AMA_CustomsOffice);

			header.AMA_CustomsOffice = string.Empty;
			header.AMA_RL_NKPortOfDischarge = "VUSAN";

			AssertEquals("SAIR", header.AMA_CustomsOffice);

			header.AMA_CustomsOffice = string.Empty;
			header.AMA_TransportMode = "SEA";
			AssertEquals("SSEA", header.AMA_CustomsOffice);

			header.AMA_CustomsOffice = string.Empty;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.SolomonIslands;
			header.AMA_RL_NKPortOfDischarge = "SBHIR";

			AssertEquals("HIRS", header.AMA_CustomsOffice);  // HoniaraPointCruzSeaport
		}

		public void TestShowPackedItems()
		{
			var accessEnable = (RegistryItemWrapper)ObjectFactory.Get<SG.ISGCustomsRegistry>().ACCESSEnable;
			using (accessEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGI");
				header.AMA_TransportMode = "AIR";

				Assert(header.ShowPackedItems);
			}
		}

		public void TestShowVINNumbers()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("ShowVINNumbers", true, header.ShowVINNumbers);
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("ShowVINNumbers", false, header.ShowVINNumbers);
			header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("ShowVINNumbers", false, header.ShowVINNumbers);
		}

		public void TestArrivalHeaders()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderForTest>();
			AssertType<AsycudaArrivalHeaderCollection<AsycudaArrivalHeader>>(header.ArrivalHeaders);
			AssertEquals(typeof(AsycudaArrivalHeader), header.GetArrivalHeaderType());
		}

		public void TestBills()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertEquals(0, header.Bills.Count);

			header.Bills.AddNew();
			AssertEquals(1, header.Bills.Count);

			Factory.Save();
			var headerReloaded = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			AssertEquals(1, headerReloaded.Bills.Count);
		}

		public void TestBillSynchroniserWithNonAlphaNumericChar()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_HouseBill = "TEST_1";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.Add(shipment);
			Factory.Save();

			var manifestHeader = (AsycudaManifestHeader)GetNewBusinessObject();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();
			AssertEquals("One house bill should exist", 1, manifestHeader.Bills.Count);

			consol.JK_MasterBillNum = "BOL123456";
			Factory.Save();

			var headerReloaded = Factory.Load<AsycudaManifestHeader>(manifestHeader.PK);
			headerReloaded.Synchroniser.SetEnabled(true, false);
			headerReloaded.Synchroniser.Synchronise();
			AssertEquals("One house bill should exist after reloading", 1, headerReloaded.Bills.Count);
		}

		public void TestCopyAdditionalConsolShipmentToBills()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_HouseBill = "TEST_1";
			shipment.JS_GoodsDescription = "DESC_1";
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.Add(shipment);
			Factory.Save();
			var manifestHeader = (AsycudaManifestHeader)GetNewBusinessObject();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();
			AssertEquals("One bill should exist", 1, manifestHeader.Bills.Count);

			manifestHeader.AMA_OverrideFreightDefaults = true;
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_HouseBill = "TEST_2";
			shipment2.JS_GoodsDescription = "DESC_2";
			consol.Shipments.Add(shipment2);
			AssertEquals("One bill should exist", 1, manifestHeader.Bills.Count);

			manifestHeader.CopyAdditionalConsolShipmentToBills();
			AssertEquals("Two bill should exist", 2, manifestHeader.Bills.Count);

			manifestHeader.Bills[1].ABL_GoodsDescription = "DESC_2_MODIFIED_AT_BILL";
			manifestHeader.CopyAdditionalConsolShipmentToBills();
			AssertEquals("Bill should remain unchanged", "DESC_2_MODIFIED_AT_BILL", manifestHeader.Bills[1].ABL_GoodsDescription);

			shipment2.JS_GoodsDescription = "DESC_2_MODIFIED_AT_SHIPMENT";
			manifestHeader.CopyAdditionalConsolShipmentToBills();
			AssertEquals("Bill should remain unchanged", "DESC_2_MODIFIED_AT_BILL", manifestHeader.Bills[1].ABL_GoodsDescription);

			consol.Shipments.Remove(shipment2);
			AssertEquals("Two bill should exist", 2, manifestHeader.Bills.Count);
		}

		public void TestContainers()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertEquals(0, header.Containers.Count);
			AssertEquals(0, header.ContainersAsICusInBondContainerCollectionForSynching.Count);
			header.Containers.AddNew();
			AssertEquals(1, header.Containers.Count);
			AssertEquals(1, header.ContainersAsICusInBondContainerCollectionForSynching.Count);
			AssertEquals(header.ContainersAsICusInBondContainerCollectionForSynching[0], header.Containers[0]);
			Factory.Save();
			var headerReloaded = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			AssertEquals(1, headerReloaded.Containers.Count);
			AssertEquals(1, headerReloaded.ContainersAsICusInBondContainerCollectionForSynching.Count);
			AssertEquals(headerReloaded.ContainersAsICusInBondContainerCollectionForSynching[0], headerReloaded.Containers[0]);
		}

		public void TestDelete()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			var bill = header.Bills.AddNew();
			var cont = header.Containers.AddNew();
			header.Delete();
			Assert(cont.IsDeleted);
			Assert(bill.IsDeleted);
		}

		public void TestSetVesselsSetsNationality()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			vessel.RV_RN_NKCountryOfReg = "GB";
			AssertEquals("", header.AMA_RN_NKConveyanceNationality);
			header.AMA_VesselName = vessel.RV_Code;
			AssertEquals("GB", header.AMA_RN_NKConveyanceNationality);
		}

		public void TestShippingAgent()
		{
			var orgAddress = Factory.LoadTop1<OrgAddress>(new ZQuery());
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_OA_ShippingAgent = orgAddress.PK;
			AssertEquals(orgAddress.PK, header.ShippingAgent.PK);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var headerReloaded = newFactory.Load<AsycudaManifestHeader>(header.PK);
			AssertEquals(orgAddress.PK, headerReloaded.ShippingAgent.PK);
		}

		public void TestMawbNumberWithHyphen()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_MasterBill = "!ABC--1234 56789";
			AssertEquals("ABC-12345678", header.MawbNumberWithHyphen);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1140:ColumnNameCaseAnalyzer", Justification = "String comes from error notification")]
		public void TestUSAirAMSValidFieds()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			var us = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.UnitedStates, "United States", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(us.PK, RefCusCodeListTypes.Codes.NVC, "17.3.29.1");

			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");

			var usPortOfFirstArrival = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.PortOfFirstArrival, "A Port Of First Arrival is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTransportModeForCusCodeList(usPortOfFirstArrival.PK, Core.Constants.TransportModes.Air);

			var usETA = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.EstimatedTimeOfArrivalAtBorder, "An Estimated Time of Arrival at Border is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTransportModeForCusCodeList(usETA.PK, Core.Constants.TransportModes.Air);

			var usGoodsDescription = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.GoodsDescription, "A Goods Description is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(usGoodsDescription.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);

			var usCarrierCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.CarrierCode, "A Carrier Code is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTransportModeForCusCodeList(usCarrierCode.PK, Core.Constants.TransportModes.Air);

			var usConsignee = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.Consignee, "Consignee''s detail is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(usConsignee.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);

			var usConsigneeState = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.ConsigneeState, "Consignee''s state is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(usConsigneeState.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);

			var usConsigneePhone = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.ConsigneePhone, "Consignee''s phone is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(usConsigneePhone.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);

			var usConsignor = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.Consignor, "Shipper''s detail is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(usConsignor.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);

			var usConsignorState = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.ConsignorState, "Shipper''s state is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(usConsignorState.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);

			var usIATAPortOfOrigin = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.IATAPortOfOrigin, "IATA Code is required for Port of Origin", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(usIATAPortOfOrigin.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);

			var usIATAPortOfFirstArrival = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.IATAPortOfFirstArrival, "IATA Code is required for Port of First Arrival", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(usIATAPortOfFirstArrival.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);

			var usIATAPortOfDischarge = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.IATAPortOfDischarge, "IATA Code is required for Port of Discharge", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(usIATAPortOfDischarge.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);

			var usEstimatedDepartureTime = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.EstimatedDepartureTime, "(Optional)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(usEstimatedDepartureTime.PK, ManifestValidationRuleCodes.Optional, ZString.Empty);

			var usFinalDestination = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.FinalDestination, "(Optional)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(usFinalDestination.PK, ManifestValidationRuleCodes.Optional, ZString.Empty);

			var usOfficeCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.OfficeCode, "(Optional)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(usOfficeCode.PK, ManifestValidationRuleCodes.Optional, ZString.Empty);

			var usNature = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.Nature, "(Optional)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(usNature.PK, ManifestValidationRuleCodes.Optional, ZString.Empty);

			var usShipmentType = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.ShipmentType, "(Optional)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(usShipmentType.PK, ManifestValidationRuleCodes.Optional, ZString.Empty);

			AsycudaBillForRegularBillValidationTest.FindOrMakeRefPackAndCusCodeList("AAA", Core.Constants.PkgUnit.Package, Core.Constants.CountryCodes.UnitedStates, Factory);
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			header.AMA_OverrideFreightDefaults = true;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			ClearData(header, new[] { AsycudaManifestHeader.Schema.AMA_ParentId, AsycudaManifestHeader.Schema.AMA_JobReference, AsycudaManifestHeader.Schema.AMA_OverrideFreightDefaults, AsycudaManifestHeader.Schema.AMA_TransportMode });
			var bill = header.Bills.AddNew();
			ClearData(bill, new[] { AsycudaBill.Schema.ABL_AMA });
			header.SetParent(consol);
			var transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "USLAX";
			AssertNoExceptionThrown("Should be able to save", Factory.Save);

			const string defaultRequiredFields = @"Error - AMA_ManifestType: Please enter a Manifest Type.
Message Error - ABL_BillNumber: MAWB is required.
Message Error - ABL_BillNumber: You have not entered a Bill Number.
Message Error - ABL_ConsigneeCity: Consignee''s detail is required for US.
Message Error - ABL_ConsigneeName: Consignee''s detail is required for US.
Message Error - ABL_ConsigneePhone: Consignee''s phone is required for US.
Message Error - ABL_ConsigneeState: Consignee''s state is required for US.
Message Error - ABL_ConsigneeStreet1: Consignee''s detail is required for US.
Message Error - ABL_E_ARV: You have not entered an Estimated Time of Arrival at Border.
Message Error - ABL_GoodsDescription: A Goods Description is required for US.
Message Error - ABL_GrossWeight: You have not entered a Gross Weight.
Message Error - ABL_GrossWeightUQ: You have not entered a Gross Weight Unit.
Message Error - ABL_ManifestQty: You have not entered a Quantity (on Bill).
Message Error - ABL_OA_Consignee: Consignee''s detail is required for US.
Message Error - ABL_OA_Shipper: Shipper''s detail is required for US.
Message Error - ABL_RL_NKOrigin: You have not entered an Origin.
Message Error - ABL_RL_NKPortOfDischarge: You have not entered a value.
Message Error - ABL_RL_NKPortOfLoading: You have not entered a value.
Message Error - ABL_RN_NKConsigneeCountry: Consignee''s detail is required for US.
Message Error - ABL_RN_NKShipperCountry: Shipper''s detail is required for US.
Message Error - ABL_ShipperCity: Shipper''s detail is required for US.
Message Error - ABL_ShipperName: Shipper''s detail is required for US.
Message Error - ABL_ShipperState: Shipper''s state is required for US.
Message Error - ABL_ShipperStreet1: Shipper''s detail is required for US.
Message Error - AMA_CarrierCode: A Carrier Code is required for US.
Message Error - AMA_MasterBill: MAWB is required.
Message Error - AMA_RL_NKPortOfFirstArrival: A Port Of First Arrival is required for US.
Message Error - AMA_Voyage: You have not entered a Flight.
Message Error - GoodsOrigin: You have not entered a Country/Region of Origin.";
			AssertMultilineASCIIEquals("Default required fields", defaultRequiredFields, GetErrorNotification(header));

			var masterBill = header.MasterBill;
			header.AMA_ManifestType = "IAM";
			header.AMA_MasterBill = "081-09232121";
			header.AMA_Voyage = "QF3342";
			header.AMA_E_ARV = ZDateTime.Today.AddDays(1);
			header.AMA_RL_NKPortOfLoading = "AUSYD";
			header.AMA_RL_NKPortOfDischarge = "USLAX";
			header.AMA_CarrierCode = "QF";
			header.AMA_RL_NKPortOfFirstArrival = "USLAX";
			masterBill.ABL_ManifestQty = 100;
			masterBill.ABL_ManifestUQ = Core.Constants.PkgUnit.Package;
			masterBill.ABL_GrossWeight = 50m;
			masterBill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			((Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader)header).EstDateAtFirstArrival = ZDateTime.Today;
			bill.ABL_BillNumber = "HB24223";
			bill.ABL_RL_NKOrigin = "AUSYD";
			bill.ABL_GoodsDescription = "GOOD STUFF";
			bill.ABL_ManifestQty = 100;
			bill.ABL_ManifestUQ = Core.Constants.PkgUnit.Package;
			bill.ABL_GrossWeight = 50m;
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			bill.ABL_ConsigneeName = "BOB THE BUILDER";
			bill.ABL_ConsigneeStreet1 = "BOB ADDRESS 1";
			bill.ABL_ConsigneeCity = "BOB CITY";
			bill.ABL_ConsigneeState = "BOB STATE";
			bill.ABL_ConsigneePostcode = "102023";
			bill.ABL_ConsigneePhone = "43532";
			bill.ABL_RN_NKConsigneeCountry = "US";
			bill.ABL_ShipperName = "JOE THE BUILDER";
			bill.ABL_ShipperStreet1 = "JOE ADDRESS 1";
			bill.ABL_ShipperCity = "JOE CITY";
			bill.ABL_ShipperState = "JOE STATE";
			bill.ABL_ShipperPostcode = "65444";
			bill.ABL_RN_NKShipperCountry = "ZA";
			AssertMultilineASCIIEquals("No Error", ZString.Empty, GetErrorNotification(header));
		}

		public void TestSavingSetsJobReference()
		{
			TestConnection.BeginTransaction(); // Updating next number fountain value for the test
			try
			{
				Env.NumberFountains.ManifestJobReference.SetNext(Factory, 6789);
				var asycudaManifestHeader = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
				asycudaManifestHeader.OnSaving();
				AssertEquals("MAN0006789", asycudaManifestHeader.AMA_JobReference);

				var newFactory = NewFactory();
				var consol = newFactory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_UniqueConsignRef = "CON000427";

				var wrapper = new ManifestHeadersWrapper(consol);

				var headerOnConsol1 = wrapper.Headers.AddNew();
				headerOnConsol1.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
				AssertEquals(string.Empty, headerOnConsol1.AMA_JobReference);

				newFactory.Save();

				AssertEquals("CON000427_1", headerOnConsol1.AMA_JobReference);

				newFactory = NewFactory();
				consol = newFactory.Load<ForwardingConsol>(consol.PK);
				wrapper = new ManifestHeadersWrapper(consol);

				var headerOnConsol2 = wrapper.Headers.AddNew();
				headerOnConsol2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Vanuatu;
				AssertEquals(string.Empty, headerOnConsol2.AMA_JobReference);

				newFactory.Save();

				AssertEquals("CON000427_2", headerOnConsol2.AMA_JobReference);
			}
			finally
			{
				TestConnection.RollbackTransaction(); // Updating next number fountain value for the test
			}
		}

		public void TestIsStandAlone()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var manifestHeader2 = Factory.New<AsycudaManifestHeader>();
			var consol = Factory.New<ForwardingConsol>();
			manifestHeader2.SetParent(consol);
			Assert(manifestHeader.IsStandAlone);
			Assert(!manifestHeader2.IsStandAlone);
		}

		public void TestHumanReadableName()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			header.AMA_JobReference = "MAN001";
			AssertEquals("SB Manifest MAN001", header.HumanReadableName);
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			var manifestHeader = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGI");
			manifestHeader.FillWithValidTestData();
			var bill = manifestHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.GetSGPackedItemForTesting();
			Assert("Should be something in the related logs", manifestHeader.BusinessObjectsWithRelatedEvents.Length > 0);
			Assert("Bill should be in the list of related objects", ((IList)manifestHeader.BusinessObjectsWithRelatedEvents).Contains(bill));
			Assert("Pack should be in the list of related objects", ((IList)manifestHeader.BusinessObjectsWithRelatedEvents).Contains(pack));
			Assert("Pack country should not be in the list of related objects", !((IList)manifestHeader.BusinessObjectsWithRelatedEvents).Contains(packedItem));
		}

		public void TestContainerModeWhenTransportModeIsAir()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("ContainerMode should be OTH when Air TransportMode", "OTH", manifestHeader.AMA_ContainerMode);
		}

		public void TestLoadRelatedMessages()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeaderForTest>();
			var sentMessage = manifestHeader.Messages.AddNew();
			sentMessage.FillWithValidTestData();
			sentMessage.MessageNumberStrategy = new TestMessageNumberStrategy("sentMessage");
			sentMessage.EM_LinkedObject = manifestHeader;

			var mrrMessage = CreateRelatedEDIMessage(AutoEvents.MessageReceivedCode, manifestHeader.PK);
			var mrjMessage = CreateRelatedEDIMessage(AutoEvents.MessageRejectedCode, manifestHeader.PK);
			Factory.Save();

			var reloadedManifestHeaderSupportingUXML = new BusinessObjectFactory().Load<AsycudaManifestHeaderForTest>(manifestHeader.PK);
			AssertEquals("3 Messages", 3, reloadedManifestHeaderSupportingUXML.Messages.Count);
			AssertContainsExactElementsInAnyOrder(new[] { sentMessage.PK, mrrMessage.PK, mrjMessage.PK }, reloadedManifestHeaderSupportingUXML.Messages.Select(x => x.PK));

			var reloadedManifestHeaderNotSupportingUXML = new BusinessObjectFactory().Load<AsycudaManifestHeader>(manifestHeader.PK);
			AssertEquals("1 Message only", 1, reloadedManifestHeaderNotSupportingUXML.Messages.Count);
			AssertContainsExactElementsInAnyOrder(new[] { sentMessage.PK }, reloadedManifestHeaderNotSupportingUXML.Messages.Select(x => x.PK));
		}

		public void TestConsolReferenceIsAbleToCalculateNextAvailableValue()
		{
			var newFactory = new BusinessObjectFactory();
			var consol = newFactory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00000001";

			var header1 = AsycudaManifestHeaderHelper.CreateNew(newFactory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			header1.AMA_JobReference = "C00000001_1";
			header1.AMA_ParentId = consol.PK;
			header1.AMA_ParentTableCode = JobConsolSchema.Constants.Prefix;

			var header2 = AsycudaManifestHeaderHelper.CreateNew(newFactory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			header2.AMA_JobReference = "C00000001_3";
			header2.AMA_ParentId = consol.PK;
			header2.AMA_ParentTableCode = JobConsolSchema.Constants.Prefix;
			newFactory.Save();

			var header3 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			header3.AMA_ParentId = consol.PK;
			header3.AMA_ParentTableCode = JobConsolSchema.Constants.Prefix;
			AssertNoExceptionThrown("No exception with The duplicate key value is (C00000001_3)", () => Factory.Save());
			AssertEquals("C00000001_4", header3.AMA_JobReference);
		}

		public void TestClusterKey()
		{
			CombineAssertions(() =>
			{
				var sgClusterKey = AssertClusterKeyWaterfall(Core.Constants.CountryCodes.Singapore, "MGI");
				var fjClusterKey = AssertClusterKeyWaterfall(Core.Constants.CountryCodes.Fiji, "ASY");
				AssertNotEquals("Cluster keys are always different", sgClusterKey, fjClusterKey);
			});
		}

		[ExpectNoExceptions]
		public void TestConsolReferenceHasNoExceptionsForInvalidExistingReferences()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00000001";

			var header1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			header1.AMA_JobReference = "C00000001_INV";
			header1.AMA_ParentId = consol.PK;
			header1.AMA_ParentTableCode = JobConsolSchema.Constants.Prefix;

			var header2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			header2.AMA_JobReference = "C00000001__";
			header2.AMA_ParentId = consol.PK;
			header2.AMA_ParentTableCode = JobConsolSchema.Constants.Prefix;

			var header3 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			header3.AMA_JobReference = "C00000001_3";
			header3.AMA_ParentId = consol.PK;
			header3.AMA_ParentTableCode = JobConsolSchema.Constants.Prefix;

			var header4 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			header4.AMA_JobReference = "C00000010_3";
			header4.AMA_ParentId = consol.PK;
			header4.AMA_ParentTableCode = JobConsolSchema.Constants.Prefix;

			var header5 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			header5.AMA_JobReference = "C00000099";
			header5.AMA_ParentId = consol.PK;
			header5.AMA_ParentTableCode = JobConsolSchema.Constants.Prefix;

			Factory.Save();

			var header6 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			header6.AMA_ParentId = consol.PK;
			header6.AMA_ParentTableCode = JobConsolSchema.Constants.Prefix;
			Factory.Save();
			AssertEquals("C00000001_6", header6.AMA_JobReference);
		}

		public void TestClearDeconcolidateAddressAndTerminalAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = CreateAddress(org, "2203", "12 Some Street", "Alexandria", "Sydney", "NSW", "TestOrg");

			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.HAB);
			header.AMA_OA_DeconsolidateAddress = address.PK;
			header.AMA_OA_DischargeTerminalAddress = address.PK;

			AssertEquals(org.PK, header.AMA_OA_DeconsolidateAddress_ZAddress.OrgPK);
			AssertEquals(org.PK, header.AMA_OA_DischargeTerminalAddress_ZAddress.OrgPK);

			header.AMA_ManifestType = nameof(ManifestDocumentType.RFM);

			AssertEquals(ZGuid.Empty, header.AMA_OA_DeconsolidateAddress_ZAddress.OrgPK);
			AssertEquals(ZGuid.Empty, header.AMA_OA_DischargeTerminalAddress_ZAddress.OrgPK);
			AssertEquals(ZGuid.Empty, header.AMA_OA_DeconsolidateAddress);
			AssertEquals(ZGuid.Empty, header.AMA_OA_DischargeTerminalAddress);
		}

		public void TestGetMasterBillQuery()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Eritrea, "ASY");
			var masterBill = header.MasterBill;
			masterBill.ABL_BillNumber = "TESTMASTER";
			var bill = header.Bills.AddNew();
			var masterBillQuery = header.GetMasterBillQuery();
			AssertEquals(true, masterBill.MatchesFilter(masterBillQuery));
			AssertEquals(false, bill.MatchesFilter(masterBillQuery));
		}

		public void TestGetCusCodeDataTypes()
		{
			var cusCodeDataTypeSupporter = (ICusCodeDataTypeSupporter)Factory.New<AsycudaManifestHeader>();
			AssertEquals(0, cusCodeDataTypeSupporter.GetCusCodeDataTypes().Count);
		}

		public void TestCusCodeDataSupporterFetchStrategies()
		{
			var cusCodeDataTypeSupporter = (ICusCodeDataTypeSupporter)Factory.New<AsycudaManifestHeader>();
			AssertEquals("Base has no CusCodeDatTypes so no Fetch Strategy", false, cusCodeDataTypeSupporter.GetFetchStrategies().Any());
		}

		public void TestCustomsLoadingPortAndCustomsDischargePort()
		{
			var headerForDischarge = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGI");
			headerForDischarge.AMA_TransportMode = "AIR";
			headerForDischarge.AMA_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			headerForDischarge.AMA_Voyage = "PW980";
			headerForDischarge.AMA_RL_NKPortOfLoading = "AUSDY";
			headerForDischarge.AMA_RL_NKPortOfDischarge = "SGSIN";
			headerForDischarge.AMA_CustomsDischargePort = "SGZZZ";
			headerForDischarge.AMA_MasterBill = "74784787776";

			var headerForLoading = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGE");
			headerForLoading.AMA_TransportMode = "SEA";
			headerForLoading.AMA_Voyage = "PW980";
			headerForLoading.AMA_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			headerForLoading.AMA_RL_NKPortOfLoading = "SGSIN";
			headerForLoading.AMA_RL_NKPortOfDischarge = "AUSDY";
			headerForLoading.AMA_CustomsLoadPort = "SGZZZ";
			headerForLoading.AMA_MasterBill = "74784787777";
			Factory.Save();

			var queryForDischarge = new ZQuery(AsycudaManifestHeaderSchema.PK, headerForDischarge.PK);
			var headerPortForDischarge = new BusinessObjectFactory().LoadTop1<AsycudaManifestHeader>(queryForDischarge);

			var queryForLoading = new ZQuery(AsycudaManifestHeaderSchema.PK, headerForLoading.PK);
			var headerPortForLoading = new BusinessObjectFactory().LoadTop1<AsycudaManifestHeader>(queryForLoading);

			AssertEquals("SGZZZ", headerPortForDischarge.AMA_CustomsDischargePort);
			AssertEquals("SGZZZ", headerPortForLoading.AMA_CustomsLoadPort);
		}

		public void TestDefaultCustomsLoadingPortAndDefaultCustomsDischargePort()
		{
			var headerForDischarge = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGI");
			headerForDischarge.AMA_TransportMode = "AIR";
			headerForDischarge.AMA_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			headerForDischarge.AMA_Voyage = "PW980";
			headerForDischarge.AMA_RL_NKPortOfLoading = "AUSDY";
			headerForDischarge.AMA_RL_NKPortOfDischarge = "SGSIN";
			headerForDischarge.AMA_CustomsDischargePort = "SGZZZ";
			headerForDischarge.AMA_MasterBill = "74784787776";

			var headerForLoading = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGE");
			headerForLoading.AMA_TransportMode = "SEA";
			headerForLoading.AMA_Voyage = "PW980";
			headerForLoading.AMA_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			headerForLoading.AMA_RL_NKPortOfLoading = "SGSIN";
			headerForLoading.AMA_RL_NKPortOfDischarge = "AUSDY";
			headerForLoading.AMA_CustomsLoadPort = "SGZZZ";
			headerForLoading.AMA_MasterBill = "74784787777";
			Factory.Save();

			var queryForDischarge = new ZQuery(AsycudaManifestHeaderSchema.PK, headerForDischarge.PK);
			var headerPortForDischarge = new BusinessObjectFactory().LoadTop1<AsycudaManifestHeader>(queryForDischarge);

			var queryForLoading = new ZQuery(AsycudaManifestHeaderSchema.PK, headerForLoading.PK);
			var headerPortForLoading = new BusinessObjectFactory().LoadTop1<AsycudaManifestHeader>(queryForLoading);
			AssertNoErrors(headerPortForLoading.AMA_CustomsLoadPortInfo);
			AssertNoNotifications(headerPortForLoading.AMA_CustomsDischargePortInfo);
			AssertNoErrors(headerPortForDischarge.AMA_CustomsDischargePortInfo);
			AssertNoNotifications(headerPortForDischarge.AMA_CustomsLoadPortInfo);

			headerPortForLoading.AMA_CustomsDischargePort = "XXYYX";
			headerPortForDischarge.AMA_CustomsLoadPort = "YYYYY";
			Factory.Save();

			AssertHasNotifications(headerPortForDischarge.AMA_CustomsLoadPortInfo);
			AssertHasNotifications(headerPortForLoading.AMA_CustomsDischargePortInfo);

			headerPortForLoading.AMA_CustomsDischargePort = "AUSDY";
			headerPortForDischarge.AMA_CustomsLoadPort = "AUSDY";
			Factory.Save();

			AssertNoErrors(headerPortForLoading.AMA_CustomsDischargePortInfo);
			AssertNoErrors(headerPortForDischarge.AMA_CustomsLoadPortInfo);
		}

		public void TestAMA_CustomsOriginPort()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var masterBill = header.MasterBill;

			CombineAssertions(() =>
			{
				masterBill.ABL_CustomsOriginPort = "AUSYD";
				AssertEquals("AMA_CustomsOriginPort set from Master Bill", "AUSYD", header.AMA_CustomsOriginPort);

				header.AMA_CustomsOriginPort = "SGSIN";
				AssertEquals("Master Bill ABL_CustomsOriginPort is updated", "SGSIN", masterBill.ABL_CustomsOriginPort);
			});
		}

		public void TestSelectedVesselImoNumber()
		{
			var testVessel = Factory.New<RefVessel>();
			testVessel.RV_Code = "VESSEL1";
			testVessel.RV_LloydsNumber = "9060443";
			testVessel.RV_RN_NKCountryOfReg = "TR";
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = "SEA";
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, SQLComparisonOperator.Equal, "VESSEL1"));
			header.AMA_LloydsNumber = vessel.RV_LloydsNumber;
			AssertEquals("9060443", header.AMA_LloydsNumber);
		}

		public void TestClearVesselVoyage()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = "SEA";
			header.AMA_Voyage = "XXXX";
			header.AMA_VesselName = "XXXX";
			header.AMA_RadioCallSign = "CALLME";
			header.AMA_RN_NKConveyanceNationality = "TR";
			header.AMA_LloydsNumber = "9060443";

			header.AMA_TransportMode = "AIR";
			CombineAssertions(() =>
			{
				AssertEquals("AMA_Voyage", ZString.Empty, header.AMA_Voyage);
				AssertEquals("AMA_VesselName", ZString.Empty, header.AMA_VesselName);
				AssertEquals("AMA_RadioCallSign", ZString.Empty, header.AMA_RadioCallSign);
				AssertEquals("AMA_RN_NKConveyanceNationality", ZString.Empty, header.AMA_RN_NKConveyanceNationality);
				AssertEquals("AMA_LloydsNumber", ZString.Empty, header.AMA_LloydsNumber);
			});
		}

		public void TestNatureChangeAfterPortsChange()
		{
			var manifestHeader = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");

			manifestHeader.AMA_Nature = ZString.Empty;
			manifestHeader.AMA_RL_NKPortOfLoading = "ZATLR";

			AssertEquals("EXP", manifestHeader.AMA_Nature);

			manifestHeader.AMA_Nature = ZString.Empty;
			manifestHeader.AMA_RL_NKPortOfLoading = "TRTLR";
			manifestHeader.AMA_RL_NKPortOfDischarge = "ZALOP";

			AssertEquals("IMP", manifestHeader.AMA_Nature);
		}

		public void TestManifestApplicationType()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			header.AMA_ApplicationCode = ZString.Empty;
			AssertEquals(ZString.Empty, header.ManifestApplicationType);

			header.AMA_ApplicationCode = "VOC";
			AssertEquals("Carrier", header.ManifestApplicationType);

			header.AMA_ApplicationCode = "NVC";
			AssertEquals("Forwarder", header.ManifestApplicationType);
		}

		public void TestIEDIMessageCollectionOwner()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var owner = header as IEDIMessageCollectionOwner;

			AssertNotNull("Header cast successful", owner);
			AssertSame("BO", header, owner.MessageOwner);
			AssertSame("Message collection", header.Messages, owner.Messages);
		}

		public void TestAMA_RL_NKOrigin()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RL_NKOrigin = "ABC";
			AssertEquals("ABC", header.MasterBill.ABL_RL_NKOrigin);
		}

		public void TestAMA_RL_NKFinalDestination()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RL_NKFinalDestination = "ABC";
			AssertEquals("ABC", header.MasterBill.ABL_RL_NKFinalDestination);
		}

		public void TestAMA_GoodsDescription()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_GoodsDescription = "Goods description";
			AssertEquals("Goods description", header.MasterBill.ABL_GoodsDescription);
		}

		public void TestAMA_GoodsDescriptionMaxLength()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals("MaxLength", AsycudaBill.Schema.ABL_GoodsDescriptionMaxLength, header.AMA_GoodsDescriptionInfo.MaxLength);
		}

		public void TestAMA_A_DEP_Caption()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertEquals("Caption", "Actual Departure Time", DataBoundResourceStrings.GetDataForProperty(header.AMA_A_DEPInfo).Caption);
			AssertEquals("Medium Caption", "Act. Departure", DataBoundResourceStrings.GetDataForProperty(header.AMA_A_DEPInfo).MediumCaption);
			AssertEquals("Short Caption", "ATD", DataBoundResourceStrings.GetDataForProperty(header.AMA_A_DEPInfo).ShortCaption);
		}

		public void TestAMA_MasterBillIssueDate()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var masterBill = header.MasterBill;
			masterBill.ABL_BillIssueDate = ZDate.BrettsBirthday;
			CombineAssertions(() =>
			{
				AssertEquals(ZDate.BrettsBirthday, header.AMA_MasterBillIssueDate);
				AssertEquals("Issue Date", DataBoundResourceStrings.GetDataForProperty(header.AMA_MasterBillIssueDateInfo).Caption);
			});
		}

		public void TestJobDirection()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var headerIImportExport = (IImportExport)header;
			AssertEquals("Default value should be Unknown, and implement IImportExport", headerIImportExport.JobDirection, Directions.Unknown);
		}

		public void TestDataGrouping()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			AssertEquals("DataGrouping", "ER", header.DataGrouping);
		}

		public void TestRoutingSupport() => CombineAssertions(() =>
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = "A";
			var routingSupport = (IRoutingSupport)header;
			AssertEquals("AdditionalETAUpdateMsg", string.Empty, routingSupport.AdditionalETAUpdateMsg);
			AssertEquals("AdditionalETDUpdateMsg", string.Empty, routingSupport.AdditionalETDUpdateMsg);
			AssertEquals("Transports count", 0, routingSupport.Transports.Count);
			AssertEquals("TransportsIncludingRelated count", 0, routingSupport.TransportsIncludingRelated.Count);

			header.TransportMeans.AddNew();
			AssertEquals("Transports count", 1, routingSupport.Transports.Count);
			AssertEquals("TransportsIncludingRelated count", 1, routingSupport.TransportsIncludingRelated.Count);
		});

		public void TestJobHeaderCompany()
		{
			var manifestHeader = (AsycudaManifestHeader)GetNewBusinessObject();
			IWorkflowTriggerEventSource source = manifestHeader;
			AssertSame(manifestHeader.Branch.Company, source.JobHeaderCompany);
		}

		public void TestParentWorkflowProviders()
		{
			var manifestHeader = (AsycudaManifestHeader)GetNewBusinessObject();
			IWorkflowTriggerEventSource source = manifestHeader;

			CombineAssertions(() =>
			{
				AssertEquals("Manifest is not linked to a Consol", 0, source.ParentWorkflowProviders.Count);

				var consol = Factory.New<ForwardingConsol>();
				manifestHeader.SetParent(consol);
				AssertContainsExactElementsInAnyOrder(new[] { consol }, source.ParentWorkflowProviders);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			ZZDataTestHelper.SetupZZ(Factory, Core.Constants.CountryCodes.SouthAfrica);
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_JobReference = "C1234";
			header.AMA_E_ARV = ZDateTime.Today;
			header.AMA_RL_NKPortOfLoading = "SBHIR";
			header.AMA_RL_NKPortOfDischarge = "GBFXT";
			header.SuspendCheckBusinessObjectType();
			header.AMA_RN_NKCountry = "SB";
			header.AMA_Nature = "ABC";
			return header;
		}

		protected override Type ExpectedTypeOfContainer
		{
			get
			{
				var headerType = new AsycudaManifestHeaderTypeDecider().GetGlobalManifestType(null,
					GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "", "");
				return ((AsycudaManifestHeader)Factory.New(headerType)).Containers.GetType();
			}
		}

		IDisposable TemporarilySetManifestCountry(AsycudaManifestHeader header, string countryCode, string manifestType)
		{
			var oldCountry = header.AMA_RN_NKCountry;
			header.AMA_ManifestType = manifestType;
			header.AMA_RN_NKCountry = countryCode;

			return new DisposableAction(() =>
			{
				header.AMA_RN_NKCountry = oldCountry;
				ErrorReporter.Clear();
			});
		}

		void RunOfficeCodeTest(string[] trio)
		{
			string mode = trio[0];
			string port = trio[1];
			string expectedOffice = trio[2];

			var manifest1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest1.AMA_RN_NKCountry = port.Substring(0, 2);

			var consol = Factory.New<ForwardingConsol>();
			manifest1.SetParent(consol);
			manifest1.AMA_TransportMode = mode;
			manifest1.AMA_RL_NKPortOfLoading = "USATL";
			manifest1.AMA_RL_NKPortOfDischarge = port;
			AssertEquals("Expected office code for port=" + port + ", mode=" + mode, expectedOffice, manifest1.AMA_CustomsOffice);
		}

		void RunOfficeCodeTestWithOutPorts(string[] pair)
		{
			string mode = pair[0];
			string expectedOffice = pair[1];

			var manifest1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest1.AMA_RN_NKCountry = expectedOffice.Substring(0, 2);

			var consol = Factory.New<ForwardingConsol>();
			manifest1.SetParent(consol);
			manifest1.AMA_TransportMode = mode;
			manifest1.AMA_RL_NKPortOfLoading = ZString.Empty;
			manifest1.AMA_RL_NKPortOfDischarge = ZString.Empty;
			AssertEquals("Expected office code for mode=" + mode, expectedOffice, manifest1.AMA_CustomsOffice);
		}

		OrgAddress CreateAddress(OrgHeader org, string postCode, string address1, string address2, string city, string state, string companyName)
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = org.PK;
			address.CompanyName = companyName;
			address.Address1 = address1;
			address.Address2 = address2;
			address.City = city;
			address.State = state;
			address.Postcode = postCode;
			return address;
		}

		JobVoyage GetVoyageWithSailing()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "AALSMEERGRACHT";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "123SD";
			voyage.JV_RV_NKVessel = vessel.RV_FK;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDateTime(2018, 1, 1);

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			destination.JB_E_ARV = new ZDateTime(2018, 9, 1);

			voyage.GenerateSailings();

			return voyage;
		}

		void ClearData(BusinessObject bizObj, string[] ignoreFields)
		{
			foreach (var info in bizObj.ZPropertyInfoHash.OfType<ZPropertyInfo>().Where(x => x.HasSetter))
			{
				var name = info.Name;
				if (!ignoreFields.Contains(name) && !IsSystemFields(name))
				{
					info.Value = info.Value.Default;
				}
			}
		}

		bool IsSystemFields(string name)
		{
			return !name.EndsWith(CargoWise.Schema.Schema.IsValidColumnSuffix) && !name.EndsWith(CargoWise.Schema.Schema.IsActiveColumnSuffix) && !name.Contains("_System");
		}

		ZString GetErrorNotification(BusinessObject bizObj)
		{
			bizObj.RunPreSaveValidation();
			return new ZStringBuilder(bizObj.NotificationsIncludingChildren.Where(x => x.Type == CargoWise.EntityFramework.NotificationType.MessageError || x.Type == CargoWise.EntityFramework.NotificationType.Error).Select(x => x.Message).OrderBy(x => x)).ToStringWithNewLineBetweenAppends();
		}

		EDIMessage CreateRelatedEDIMessage(ZString eventType, ZGuid headerPK)
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_ApplicationCode = "UDM";
			message.EM_MessageType = EDIMessageTypeList.Codes.XMS;
			message.MessageNumberStrategy = new TestMessageNumberStrategy($"{eventType}Message");

			var stmLog = Factory.New<StmALog>();
			using (stmLog.LockForUpdatingKeyFieldsForTesting())
			{
				stmLog.SL_SE_NKEvent = eventType;
				stmLog.SL_Table = AsycudaManifestHeader.Schema.TableName;
				stmLog.SL_Parent = headerPK;
			}

			var genPivot = Factory.New<GenPivot>();
			genPivot.Relation1Object = stmLog;
			genPivot.XX_RelationType = Core.Constants.GenPivotTypes.XmlEdiMessage;
			genPivot.Relation2Object = message;

			return message;
		}

		ZInt AssertClusterKeyWaterfall(ZString country, ZString manifestType)
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, country, manifestType);
			var masterBill = header.MasterBill;
			masterBill.ABL_BillNumber = "TESTMAWB";
			var bill = header.Bills.AddNew();
			var container = header.Containers.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemForTesting();
			pack.ContainerPK = container.PK;
			var pivot = pack.Pivot;

			Factory.Save();

			var masterClusterValue = header.AMA_ClusterKey;
			AssertNotEquals($"{country} - Cluster key is not 0", 0, masterClusterValue);
			AssertEquals($"{country} - Bill cluster key is set correctly", bill.ABL_ClusterKey, masterClusterValue);
			AssertEquals($"{country} - MasterBill cluster key is set correctly", masterBill.ABL_ClusterKey, masterClusterValue);
			AssertEquals($"{country} - Container cluster key is set correctly", container.ACN_ClusterKey, masterClusterValue);
			AssertEquals($"{country} - Pack cluster key is set correctly", pack.APA_ClusterKey, masterClusterValue);
			if (pack.IsNonePackedItemRelationship)
			{
				AssertEquals($"{country} - Packed Item is deleted", true, packedItem.IsDeleted);
			}
			else
			{
				AssertEquals($"{country} - Packed Item cluster key is set correctly", packedItem.API_ClusterKey, masterClusterValue);
			}
			AssertEquals($"{country} - Container-Pack Pivot cluster key is set correctly", pivot.APC_ClusterKey, masterClusterValue);

			return masterClusterValue;
		}

		internal sealed class AsycudaManifestHeaderForTest : AsycudaManifestHeader
		{
			public AsycudaManifestHeaderForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.Eritrea;

			protected override bool ShouldAddUXMLMessagesToCollection => true;

			public bool ShouldPropertiesBeReadOnly(PropertyDescriptor property)
			{
				return GetShouldPropertiesBeReadOnly(property);
			}
		}
	}
}
