using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class JobDeclarationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestWarningInInvoicesToAttach()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.MakeNonPersistent();
			testDec.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			testDec.JE_OH_Supplier = ZGuid.NewZGuid();
			testDec.JE_OH_Importer = ZGuid.NewZGuid();

			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = testDec.JE_MessageType;
			invoice.JZ_OH_Buyer = testDec.JE_OH_Importer;
			invoice.JZ_OH_Supplier = testDec.JE_OH_Supplier;
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;

			AssertNoRowWarnings("No warning", invoice);
		}

		public void TestInvoicesToAttach()
		{
			AssertType(typeof(JobDeclarationLookups.CAAttachInvoiceCollection), declaration.Lookups.InvoicesToAttach);
		}

		public void TestProperties()
		{
			AssertEquals(declaration.Lookups.Declaration, declaration);
			AssertEquals(typeof(OrganisationsFindBoxCollection), declaration.Lookups.Organizations.GetType());
			AssertEquals(typeof(ZZRefCusCodeListCombinedCollection), declaration.Lookups.CBSAOffices.GetType());
			AssertEquals(typeof(CAJobMessageTypeList), declaration.Lookups.MessageTypeList.GetType());
			AssertEquals(typeof(TransportTypeList), declaration.Lookups.TransportTypeList.GetType());
			AssertEquals(typeof(CBSATransportTypeList), declaration.Lookups.CBSATransportTypeList.GetType());
			AssertEquals(typeof(EDIReleaseImportEntryStatusList), declaration.Lookups.EntryStatusList.GetType());
			AssertEquals(typeof(ZZRefCarrierCombinedCollection), declaration.Lookups.JE_CarrierCodeList.GetType());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(typeof(B3EntryTypeList), declaration.Lookups.MessageSubTypeList.GetType());
			AssertNull("F (LVS) entry type should be excluded from Import declaration", declaration.Lookups.MessageSubTypeList.GetDescriptionFromCode(B3EntryTypeList.Codes.LowValueShipments));
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals(typeof(LowValueShipmentsTypes), declaration.Lookups.MessageSubTypeList.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(new CodeDescriptionPairList(), declaration.Lookups.MessageSubTypeList);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Misc;
			AssertEquals(new CodeDescriptionPairList(), declaration.Lookups.MessageSubTypeList);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("NON, NOP, TRF, TRD, CLS, CLD, PNO, PNP, TRM", declaration.Lookups.MergeByList.CodesAsString);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals(JobMessageTypeList.Codes.LowValueShipments, declaration.Lookups.MergeByList.CodesAsString);
			AssertEquals("LVS Data Merge", declaration.Lookups.MergeByList.GetDescriptionFromCode(JobMessageTypeList.Codes.LowValueShipments));

			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			AssertEquals(typeof(B3XPaymentCodeList), declaration.Lookups.PaymentPartyList.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(typeof(Customs.Business.PaymentPartyCodeDescriptionList), declaration.Lookups.PaymentPartyList.GetType());
		}

		public void TestMessageTypeList()
		{
			var list = declaration.Lookups.MessageTypeList;
			var list2 = declaration.Lookups.MessageTypeList;
			AssertEquals(true, object.ReferenceEquals(list2, list));
			var editableMessageTypeList1 = declaration.Lookups.EditableMessageTypeList;
			var editableMessageTypeList2 = declaration.Lookups.EditableMessageTypeList;
			AssertEquals(true, object.ReferenceEquals(editableMessageTypeList2, editableMessageTypeList1));
			Assert("LVS should not be removed", list.ContainsCode(JobMessageTypeList.Codes.LowValueShipments));
			Assert("B2 Adjustments should not be removed", list.ContainsCode(JobMessageTypeList.Codes.B2Adjustments));
			Assert("LVX Jobs should not be removed", list.ContainsCode(JobMessageTypeList.Codes.LVSForConsolidation));
			Assert("LVS should be removed", !editableMessageTypeList1.ContainsCode(JobMessageTypeList.Codes.LowValueShipments));
			Assert("B2 Adjustments should be removed", !editableMessageTypeList1.ContainsCode(JobMessageTypeList.Codes.B2Adjustments));
			Assert("LVX Jobs should be removed", !editableMessageTypeList1.ContainsCode(JobMessageTypeList.Codes.LVSForConsolidation));
			Assert("IM2 Jobs should be removed", !editableMessageTypeList1.ContainsCode(JobMessageTypeList.Codes.ImportCopyforB2));
			Assert("B3X Jobs should be removed", !editableMessageTypeList1.ContainsCode(JobMessageTypeList.Codes.XTypeEntry));

			var list3 = declaration.Lookups.MessageTypeList;
			var list4 = declaration.Lookups.MessageTypeList;
			AssertEquals(true, object.ReferenceEquals(list3, list4));
			var editableMessageTypeList3 = declaration.Lookups.EditableMessageTypeList;
			var editableMessageTypeList4 = declaration.Lookups.EditableMessageTypeList;
			AssertEquals(true, object.ReferenceEquals(editableMessageTypeList3, editableMessageTypeList4));
			Assert("IMP present when Import delaraction actve", list3.ContainsCode(JobMessageTypeList.Codes.Import));
			Assert("IMP present when Import delaraction actve", editableMessageTypeList3.ContainsCode(JobMessageTypeList.Codes.Import));
		}

		public void TestCargoIdTypeList()
		{
			var list = declaration.Lookups.CargoIdTypeList;
			AssertEquals(4, list.Count);
			AssertCargoIdType(list[0], Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
			AssertCargoIdType(list[1], Core.Constants.ContainerModes.BreakBulk, Core.Constants.ContainerModeDescriptions.BreakBulk);
			AssertCargoIdType(list[2], Core.Constants.ContainerModes.Bulk, Core.Constants.ContainerModeDescriptions.Bulk);
			AssertCargoIdType(list[3], Core.Constants.ContainerModes.Liquid, Core.Constants.ContainerModeDescriptions.Liquid);
		}

		void AssertCargoIdType(ICodeDescription codeDescription, string code, string description)
		{
			AssertEquals("Code", code, codeDescription.Code);
			AssertEquals("Description", description, codeDescription.Description);
		}

		public void TestMessageStatusList()
		{
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Accepted Data Loading Module Original", declaration.Lookups.MessageStatusList.GetDescriptionFromCode(MessageStatusList.Codes.ClearOriginal));

			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Accepted G7 Export Message Original", declaration.Lookups.MessageStatusList.GetDescriptionFromCode(MessageStatusList.Codes.ClearOriginal));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var message = declaration.ReleaseEntryHeader.Messages.AddNew();
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			AssertEquals("Accepted Across/IID EDI Release Original", declaration.Lookups.MessageStatusList.GetDescriptionFromCode(MessageStatusList.Codes.ClearOriginal));

			message = declaration.B3EntryHeader.Messages.AddNew();
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddHours(2);
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			AssertEquals("Accepted B3 CUSDEC Original", declaration.Lookups.MessageStatusList.GetDescriptionFromCode(MessageStatusList.Codes.ClearOriginal));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Misc;
			AssertEquals("Accepted Original", declaration.Lookups.MessageStatusList.GetDescriptionFromCode(MessageStatusList.Codes.ClearOriginal));
		}

		public void TestJE_TotalNoOfPacksPackType_List()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"AAA", "AAAAA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"BG", "BG DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"BBB", "BBB DESC", new ZDateTime(1994, 3, 3), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			Factory.Save();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
			AssertEquals(typeof(ACROSSPackageTypes), declaration.Lookups.JE_TotalNoOfPacksPackType_List.GetType());
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			// Test if new added code exists in list
			Assert("AAA", declaration.Lookups.JE_TotalNoOfPacksPackType_List.ContainsCode("AAA"));
			Assert("BAG should not appear as it is not in the list", !declaration.Lookups.JE_TotalNoOfPacksPackType_List.ContainsCode("BAG"));
			Assert("BBB should not appear as it is too new", !declaration.Lookups.JE_TotalNoOfPacksPackType_List.ContainsCode("BBB"));
		}

		public void TestSubLocationCodes()
		{
			var location1 = CACSubLocationTest.CreateSubLocation(Factory, "1111");
			var location2 = CACSubLocationTest.CreateSubLocation(Factory, "2222");
			Factory.Save();

			var filter = declaration.Lookups.SubLocationCodes.CompleteFilter;
			Assert("SubLocationCodes should contains location1", location1.MatchesFilter(filter));
			Assert("SubLocationCodes should contains location2", location2.MatchesFilter(filter));
			declaration.JE_CustomsOffice = "0001";
			AssertEquals("0001", declaration.Lookups.SubLocationCodes.FilterBusinessObjectDefaults["Port:Property"].Value);
		}

		public void TestExamLocationCodes()
		{
			var location1 = CACSubLocationTest.CreateSubLocation(Factory, "1111");
			var location2 = CACSubLocationTest.CreateSubLocation(Factory, "2222");
			Factory.Save();

			var filter = declaration.Lookups.SubLocationCodes.CompleteFilter;
			Assert("ExamLocationCodes should contains location1", location1.MatchesFilter(filter));
			Assert("ExamLocationCodes should contains location2", location2.MatchesFilter(filter));
			declaration.JE_CustomsOffice = "0001";
			AssertEquals("0001", declaration.Lookups.ExamLocationCodes.FilterBusinessObjectDefaults["Port:Property"].Value);
		}

		public void TestCBSAOffices()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Port");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1111", "CA Customs Office Code", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1111", "US Customs Office Code", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "1111", "CA Customs Port Code", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1234", "CA Customs Office Code (Expired)", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(-1));
			Factory.Save();
			var cbsaOffices = declaration.Lookups.CBSAOffices;
			cbsaOffices.Load();

			AssertEquals(1, cbsaOffices.Count);
			AssertType<ZZRefCusCodeListCombinedCollection>(cbsaOffices);
			Assert(cbsaOffices.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "1111"));
			AssertEquals("CA Customs Office Code", cbsaOffices.Cast<ZZRefCusCodeListCombined>().FirstOrDefault(x => x.ZZD_Code == "1111").ZZD_Description);
			Assert(!cbsaOffices.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "1234"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
		}

		JobDeclaration declaration;

		public class CAAttachInvoiceCollectionForTesting : JobDeclarationLookups.CAAttachInvoiceCollection
		{
			public CAAttachInvoiceCollectionForTesting(JobDeclaration declaration)
				: base(declaration)
			{
			}
		}
	}
}
