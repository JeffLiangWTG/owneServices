using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.DE;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.MonthlyClosing;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Business.WarehouseIntegration;
using Enterprise.Customs.DE.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using WhsDataTestHelper = Enterprise.Customs.DE.Business.Testing.WhsDataTestHelper;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclaration))]
	sealed class JobDeclarationTest : EU.Business.Declaration.Testing.JobDeclarationAbstractTest<JobDeclaration>
	{
		public void TestWarehouseTransactionStatus()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var entryHeader1 = jobDeclaration.ActiveEntryHeaders.AddNew();
			var entryHeader2 = jobDeclaration.ActiveEntryHeaders.AddNew();

			AssertEquals("PreReq header1", entryHeader1.CH_WarehouseTransactionStatus, ZString.Empty);
			AssertEquals("PreReq header2", entryHeader2.CH_WarehouseTransactionStatus, ZString.Empty);

			CombineAssertions(() =>
			{
				jobDeclaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCanceledPendingWithdrawal;
				AssertEquals("header1 status updated", WarehouseTransactionStatusList.Codes.OutwardCanceledPendingWithdrawal, entryHeader1.CH_WarehouseTransactionStatus);
				AssertEquals("header2 status updated", WarehouseTransactionStatusList.Codes.OutwardCanceledPendingWithdrawal, entryHeader2.CH_WarehouseTransactionStatus);
			});
		}

		public void TestSetDefaultValuesForDeclarant()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsShippingProvider = false;
			orgHeader.OH_IsForwarder = false;
			orgHeader.OH_IsBroker = false;
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_GC = currentCompany.PK;
			currentBranch.GB_OH_OrgProxy = orgHeader.PK;
			currentBranch.GB_IsActive = true;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, currentBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				CombineAssertions(() =>
				{
					var declaration_Export = Factory.New<JobDeclaration>();
					AssertEquals("Declarant is OrgProxy", orgHeader.MainAddress.PK, declaration_Export.JE_OA_DeclarantAddress);
					AssertEquals("Representative empty", ZGuid.Empty, declaration_Export.JE_OA_Representative);
				});
			}
		}

		public void TestSetDefaultValuesForRepresentative_IsShippingProvider()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsShippingProvider = true;
			orgHeader.OH_IsForwarder = false;
			orgHeader.OH_IsBroker = false;
			AssertSetDefaultValuesForRepresentative(orgHeader);
		}

		public void TestSetDefaultValuesForRepresentative_IsForwarder()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsShippingProvider = false;
			orgHeader.OH_IsForwarder = true;
			orgHeader.OH_IsBroker = false;
			AssertSetDefaultValuesForRepresentative(orgHeader);
		}

		public void TestSetDefaultValuesForRepresentative_IsBroker()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsShippingProvider = false;
			orgHeader.OH_IsForwarder = false;
			orgHeader.OH_IsBroker = true;
			AssertSetDefaultValuesForRepresentative(orgHeader);
		}

		public void TestUpdateDeclarantAddressIfEmpty_IsConsignor()
		{
			var declaration_Export = Factory.New<JobDeclaration>();
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_IsConsignor = true;
			supplier.OH_IsConsignee = false;

			CombineAssertions(() =>
			{
				Assert("Declarant is empty initially", declaration_Export.JE_OA_DeclarantAddress.IsEmpty);

				declaration_Export.SupplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;
				AssertEquals("Declarant filled from Supplier", declaration_Export.SupplierDocumentaryAddress.Address.PK, declaration_Export.JE_OA_DeclarantAddress);
			});
		}

		public void TestUpdateDeclarantAddressIfEmpty_IsConsignee()
		{
			var declaration_Export = Factory.New<JobDeclaration>();
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_IsConsignor = false;
			supplier.OH_IsConsignee = true;

			CombineAssertions(() =>
			{
				Assert("Declarant is empty initially", declaration_Export.JE_OA_DeclarantAddress.IsEmpty);

				declaration_Export.SupplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;
				AssertEquals("Declarant filled from Supplier", declaration_Export.SupplierDocumentaryAddress.Address.PK, declaration_Export.JE_OA_DeclarantAddress);
			});
		}

		public void TestKeepDeclarantAddressIfEmpty()
		{
			var declaration_Export = Factory.New<JobDeclaration>();
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_IsConsignor = false;
			supplier.OH_IsConsignee = false;

			CombineAssertions(() =>
			{
				Assert("Declarant is empty initially", declaration_Export.JE_OA_DeclarantAddress.IsEmpty);

				declaration_Export.SupplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;
				AssertEquals("Declarant not filled from Supplier", ZGuid.Empty, declaration_Export.JE_OA_DeclarantAddress);
			});
		}

		public void TestKeepDeclarantAddressIfNotEmpty()
		{
			var declaration_Export = Factory.New<JobDeclaration>();
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_IsConsignor = true;
			supplier.OH_IsConsignee = false;
			var orgAddressPK = Factory.New<OrgHeader>().MainAddress.PK;
			declaration_Export.JE_OA_DeclarantAddress = orgAddressPK;
			declaration_Export.SupplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;
			AssertEquals(orgAddressPK, declaration_Export.JE_OA_DeclarantAddress);
		}

		public void TestDocumentSupport()
		{
			AssertType<JobDeclarationDocumentSupporter>(declaration.DocumentSupporter);
		}

		public void TestDefaultBorderTransportIsSetOnTransportModeChange()
		{
			CombineAssertions(() =>
			{
				foreach (var (isImport, transportMode, expectedDefaultId) in ExpectedDefaultBorderTransports)
				{
					declaration.JE_MessageType = isImport ? "IMP" : "EXP";
					declaration.JE_TransportMode = transportMode;
					AssertEquals(declaration.ZG_BorderTransportMeans, expectedDefaultId);
				}
			});
		}

		static readonly ImmutableArray<(bool IsImport, string TransportMode, string ExpectedDefaultId)>
			ExpectedDefaultBorderTransports = ImmutableArray.Create(
				(true, TransportTypeList.Codes.Road, ImportBorderTransportMeansList.Codes.Truck),
				(true, TransportTypeList.Codes.Sea, ImportBorderTransportMeansList.Codes.Vessel),
				(true, TransportTypeList.Codes.Rail, ImportBorderTransportMeansList.Codes.Wagon),
				(true, TransportTypeList.Codes.Air, ImportBorderTransportMeansList.Codes.Aircraft),
				(false, TransportTypeList.Codes.Road, ExportBorderTransportMeansList.Codes._30),
				(false, TransportTypeList.Codes.Sea, ExportBorderTransportMeansList.Codes._10),
				(false, TransportTypeList.Codes.Rail, ExportBorderTransportMeansList.Codes._21),
				(false, TransportTypeList.Codes.Air, ExportBorderTransportMeansList.Codes._40)
			);

		public void TestInlandVessel()
		{
			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Code = "VSCD";
			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Code = "ABCD";
			Factory.Save();
			CombineAssertions(() =>
			{
				declaration.JE_TransportIDInland = "NoShip";
				AssertNull("When vessel does not exist in database", declaration.InlandVessel);
				declaration.JE_TransportIDInland = "VSCD";
				AssertEquals("When vessel does exist in database", vessel1, declaration.InlandVessel);
				declaration.JE_TransportIDInland = "ABCD";
				AssertEquals("Updated JE_TransportIDInland", vessel2, declaration.InlandVessel);
				AssertType<RefVessel>(declaration.InlandVessel);
			});
		}

		public override void TestCustomsOfficeOfEntry()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.JE_CustomsOffice = "LV001000";
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, "LV002000");
			AssertEquals("LV002000", declaration.OfficeOfEntry);
		}

		public void TestClearDeferralFieldsIfNeeded()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_PaymentMethod = "DEC";
			declaration.JE_DefermentAccountNumber = "123";
			declaration.ZG_VATDeferType = "DEF";
			declaration.ZG_VATDeferNumber = "456";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				AssertEquals("Duty Payment Party: JE_PaymentMethod", ZString.Empty, declaration.JE_PaymentMethod);
				AssertEquals("Duty Account No.: JE_DefermentAccountNumber", ZString.Empty, declaration.JE_DefermentAccountNumber);
				AssertEquals("VAT Payment Party: ZG_VATPaymentParty", ZString.Empty, declaration.ZG_VATDeferType);
				AssertEquals("VAT Account No.: ZG_VATAccountNumber", ZString.Empty, declaration.ZG_VATDeferNumber);
			});
		}

		public void TestJE_PaymentMethod_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_PaymentMethodInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Payment Party 1", resourceStringData.Caption);
				AssertEquals("MediumCaption", "Pmt. Party 1", resourceStringData.MediumCaption);
				AssertEquals("ShortCaption", "Party 1", resourceStringData.ShortCaption);
			});
		}

		public void TestJE_DefermentAccountNumber_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_DefermentAccountNumberInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Account No. 1", resourceStringData.Caption);
				AssertEquals("MediumCaption", "Acc. No. 1", resourceStringData.MediumCaption);
				AssertEquals("ShortCaption", "Acc. 1", resourceStringData.ShortCaption);
			});
		}

		public void TestZG_VATDeferType_MaxLength()
		{
			AssertEquals(3, declaration.ZG_VATDeferTypeInfo.MaxLength);
		}

		public void TestZG_VATDeferType_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.ZG_VATDeferTypeInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Payment Party 2", resourceStringData.Caption);
				AssertEquals("MediumCaption", "Pmt. Party 2", resourceStringData.MediumCaption);
				AssertEquals("ShortCaption", "Party 2", resourceStringData.ShortCaption);
			});
		}

		public void TestZG_VATDeferNumber_MaxLength()
		{
			AssertEquals(10, declaration.ZG_VATDeferNumberInfo.MaxLength);
		}

		public void TestZG_VATDeferNumber_List()
		{
			AssertEquals("AddInfoLookups.VATAccountNumberList", declaration.ZG_VATDeferNumberInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestZG_VATDeferNumber_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.ZG_VATDeferNumberInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Account No. 2", resourceStringData.Caption);
				AssertEquals("MediumCaption", "Acc. No. 2", resourceStringData.MediumCaption);
				AssertEquals("ShortCaption", "Acc. 2", resourceStringData.ShortCaption);
			});
		}

		public void TestImportLookups()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<ImportJobDeclarationLookups>(declaration.Lookups);
		}

		public void TestExportLookups()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<ExportJobDeclarationLookups>(declaration.Lookups);
		}

		public void TestMiscellaneousCustomsLookups()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<JobDeclarationLookups>(declaration.Lookups);
		}

		public void TestLookupsNotCached()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertType<ImportJobDeclarationLookups>("Import Lookups", declaration.Lookups);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertType<ExportJobDeclarationLookups>("Export Lookups", declaration.Lookups);
			});
		}

		public override void TestAutoRating()
		{
			Assert("Germany Autorating works but DE has a special autorating for duties&taxes", true);
		}

		public override void TestIsExWarehouse()
		{
			Assert("Germany ExWarehouse is handled by procedure codes", true);
		}

		public override void TestMessageTypeForDocumentFilter()
		{
			declaration.JE_MessageType = DefaultImportMessageType;
			AssertEquals("Import type", JobMessageTypeList.Codes.Import, declaration.MessageTypeForDocumentFilter);

			declaration.JE_MessageType = DefaultExportMessageType;
			AssertEquals("Export type", JobMessageTypeList.Codes.Export, declaration.MessageTypeForDocumentFilter);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals("Export type", JobMessageTypeList.Codes.Drawback, declaration.MessageTypeForDocumentFilter);
		}

		public override void TestMessageTypesForDocumentFilter()
		{
			declaration.JE_MessageType = DefaultImportMessageType;
			AssertEquals("Import type", "," + JobMessageTypeList.Codes.Import + ",", declaration.MessageTypesForDocumentFilter);

			declaration.JE_MessageType = DefaultExportMessageType;
			AssertEquals("Export type", "," + JobMessageTypeList.Codes.Export + ",", declaration.MessageTypesForDocumentFilter);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals("Export type", "," + JobMessageTypeList.Codes.Drawback + ",", declaration.MessageTypesForDocumentFilter);

			declaration.JE_MessageType = ZString.Empty;
			AssertEquals("Empty type", "", declaration.MessageTypesForDocumentFilter);
		}

		public override void TestDefermentPartyDocAddressRequirement_ValidateOrganisationPK() => CombineAssertions(() =>
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var cusAccountCollection = new OrgCusAccountCollection(orgHeader2, Core.Constants.CountryCodes.Germany);
			cusAccountCollection.AddNew();
			orgHeader2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(
				OrgCusCode.EuropeanUnionSharedCodeTypes.Eori,
				"1234", Core.Constants.CountryCodes.Greece);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ZG_MethodOfPayment = UniversalReferenceConstants.MethodOfPaymentTypes.E;
			declaration.DefermentPartyDocAddress.OrganisationPK = orgHeader1.PK;
			AssertHasMessageError("MethodOfPayment:E, MessageType:Import, Deferment Account Number",
				declaration.DefermentPartyDocAddress.OrganisationPKInfo,
				"The Deferment Party must have a Deferment Account Number");
			AssertHasMessageError("MethodOfPayment:E, MessageType:Import, Registration Number",
				declaration.DefermentPartyDocAddress.OrganisationPKInfo,
				"The Deferment Party must have a Registration Number / Code of Type 'EOR'");

			declaration.ZG_MethodOfPayment = UniversalReferenceConstants.MethodOfPaymentTypes.G;
			declaration.DefermentPartyDocAddress.OrganisationPK = orgHeader2.PK;
			AssertNoNotifications("Other address", declaration.DefermentPartyDocAddress.OrganisationPKInfo);

			declaration.ZG_MethodOfPayment = UniversalReferenceConstants.MethodOfPaymentTypes.A;
			declaration.DefermentPartyDocAddress.OrganisationPK = orgHeader1.PK;
			AssertNoNotifications("MethodOfPayment:A, MessageType:Import, No notifications",
				declaration.DefermentPartyDocAddress.OrganisationPKInfo);
		});

		public void TestDefermentPartyDocAddressRequirement_ValidateOrganisationPK_PaymentMethod()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.DefermentPartyDocAddress.OrganisationPKInfo);

				declaration.ZG_MethodOfPayment = Business.UniversalReferenceConstants.MethodOfPaymentTypes.E;
				ValidationTestHelper.AssertFieldIsNotMandatory(declaration.DefermentPartyDocAddress.OrganisationPKInfo);
			});
		}

		public void TestZG_PresentationStartDate_Caption()
		{
			AssertEquals("Start", DataBoundResourceStrings.GetDataForProperty(declaration.ZG_PresentationStartDateInfo).Caption);
		}

		public void TestZG_PresentationEndDate_Caption()
		{
			AssertEquals("End", DataBoundResourceStrings.GetDataForProperty(declaration.ZG_PresentationEndDateInfo).Caption);
		}

		public void TestAcquirerAddressIsClearedOnWhenMessageTypeChangesFromImport()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var docAddress = declaration.AcquirerDocAddress;

			CombineAssertions(() =>
			{
				AssertCollectionContains("Acquirer Doc Address created", docAddress, declaration.DocAddresses);
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertCollectionNotContains("Change to Export removed Acquirer", docAddress, declaration.DocAddresses);
				AssertEquals("Doc Address for Acquirer is deleted", true, docAddress.IsDeleted);
			});
		}

		public override void TestGetCusCodeDataType()
		{
			AssertEquals(typeof(DEOfficeCode), ((ICusCodeDataTypeSupporter)declaration).GetCusCodeDataTypes()[EU.Business.CusCodeDataTypeList.Codes.OfficeCode]);
		}

		public void TestCustomsOffices()
		{
			AssertType<DEOfficeCodeCollection>(declaration.CustomsOffices);
		}

		public override void TestGetCustomsEntryInstructionProviderCore()
		{
			AssertType(typeof(EntryInstructionProvider), declaration.CustomsEntryInstructionProvider);
		}

		public void TestCustomsEntryInstructions()
		{
			AssertType<EU.Business.Declaration.CusEntryInstructionCollection<CusEntryInstruction>>(declaration.CustomsEntryInstructions);
		}

		public void TestJE_StatisticStatus()
		{
			AddInfoManagerTestHelper.AssertWrappedProperty(declaration,
				nameof(declaration.JE_StatisticStatus),
				DEJobDeclarationSchema.Constants.JE_StatisticsGoodsStatus,
				(ZString)StatisticStatusCodeList.Codes.C01);
		}

		public void TestJE_StatisticStatus_Caption()
		{
			AssertEquals("Statistic Status", DataBoundResourceStrings.GetDataForProperty(declaration.JE_StatisticStatusInfo).Caption);
		}

		public void TestMessageTypeChanged_NewValueIsIMP()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_StatisticStatus = ZString.Empty;

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("JE_StatisticStatus is empty default", StatisticStatusCodeList.Codes.C04, declaration.JE_StatisticStatus);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				declaration.JE_StatisticStatus = StatisticStatusCodeList.Codes.C01;
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("JE_StatisticStatus isn't empty default", StatisticStatusCodeList.Codes.C01, declaration.JE_StatisticStatus);
			});
		}

		public void TestMessageTypeChanged_OldValueIsIMP_StaticStatus()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_StatisticStatus = StatisticStatusCodeList.Codes.C01;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("JE_StatisticStatus", ZString.Empty, declaration.JE_StatisticStatus);
		}

		public void TestMessageTypeChanged_OldValueIsIMP_FiscalReferences()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.FiscalReferences.AddNew();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("FiscalReferences", 0, instruction.FiscalReferences.Count);
		}

		public void TestMessageTypeChanged_OldValueIsIMP_DV1Details()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.ZG_IsHighValueOvrd = ZBool.True;
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("Default DV1Details.Count", 1, declaration.DV1Details.Count);
				AssertEquals("Default DV1DetailsPivots.Count", 1, instruction.DV1DetailsPivots.Count);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms;
				AssertEquals("ZG_IsHighValueOvrd", ZBool.False, declaration.ZG_IsHighValueOvrd);
				AssertEquals("DV1Details.Count", 0, declaration.DV1Details.Count);
				AssertEquals("DV1DetailsPivots.Count", 0, instruction.DV1DetailsPivots.Count);
			});
		}

		public void TestMessageTypeChanged_OldValueIsIMP_CustomsFourthQtyAndUnit()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CustomsFourthQuantity = 12.34m;
			invoiceLine.JI_CustomsFourthUnitQty = "002";

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				AssertEquals("JI_CustomsFourthQuantity", decimal.Zero, invoiceLine.JI_CustomsFourthQuantity);
				AssertEquals("JI_CustomsFourthUnitQty", string.Empty, invoiceLine.JI_CustomsFourthUnitQty);
			});
		}

		public void TestMessageTypeChanged_OldValueIsEXP()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CusSupplyChainActorReferences.AddNew();

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("CusSupplyChainActorReferences", 0, instruction.CusSupplyChainActorReferences.Count);
		}

		public void TestTradersOwnReferenceFullForBox7()
		{
			AssertEquals("", declaration.TradersOwnReferenceFullForBox7);
			declaration.JE_DeclarationReference = "B00069";
			AssertEquals("B00069", declaration.TradersOwnReferenceFullForBox7);
			declaration.JE_OwnerRef = "POOP";
			AssertEquals("POOP", declaration.TradersOwnReferenceFullForBox7);
		}

		public void TestAcquirerDocAddress()
		{
			JobDocAddress address = declaration.AcquirerDocAddress;
			AssertNotNull("docAddresses.GetDocAddress(DocAddressType.AcquirerDocAddress)", address);
			AssertEquals(DocAddressType.Acquirer, address.DocAddressType);
			AssertEquals(ContactType.Administration, address.DefaultContactType);
		}

		public void TestJE_OwnerRef_MaxLength()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			AssertEquals(35, declaration.JE_OwnerRefInfo.MaxLength);

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			AssertEquals(35, declaration.JE_OwnerRefInfo.MaxLength);
		}

		public void TestJE_LocationOfGoods()
		{
			AssertEquals(35, declaration.JE_LocationOfGoodsInfo.MaxLength);
		}

		public void TestReciprocalRates()
		{
			AssertEquals(false, declaration.IsReciprocalRates);
		}

		public override void TestZG_SpecificCircumstanceIndicator()
		{
			AssertEquals(3, declaration.ZG_SpecificCircumstanceIndicatorInfo.MaxLength);
		}

		public override void TestLocalCurrencyCoreOverride()
		{
			declaration.DisableDefaultPackingInformation = true;
			AssertEquals(Core.Constants.CurrencyCodes.Germany, declaration.LocalCurrencyCode);
		}

		public void TestSupportEquipments()
		{
			AssertEquals(false, declaration.SupportEquipments);
		}

		public void TestSupportingDocuments()
		{
			AssertType<SupportingDocumentCollection>(declaration.SupportingDocuments);
		}

		public override void TestCustomsOfficeRequirementHelper()
		{
			AssertType<JobDeclarationCustomsOfficeRequirementHelper>(declaration.CustomsOfficeRequirementHelper);
		}

		public void TestEntryHeaders()
		{
			AssertType<EU.Business.Declaration.CusEntryHeaderCollection<CusEntryHeader>>(declaration.CustomsEntryHeaders);
		}

		public void TestJE_VesselName()
		{
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_VesselName = "MYVESSEL";
			AssertEquals("Sea Job", ZString.Empty, declaration.JE_MasterBill);
			declaration.JE_TransportMode = declaration.TransportModeRoadCodeForTesting;
			declaration.JE_VesselName = "TRANSPORTID";
			AssertEquals("Road Job", "TRANSPORTID", declaration.JE_MasterBill);
		}

		public void TestCountryContext()
		{
			CombineAssertions(() =>
			{
				var holder = (Common.IApportionInvoiceHolder)declaration;
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertEquals("Export", "DE", holder.CountryContext);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("Import", "DEIMP", holder.CountryContext);
			});
		}

		public void TestIsDeclarantEntitledToClaimBackVAT()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Declarant doesn't exist", false, declaration.IsDeclarantEntitledToClaimBackVAT);

				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
				var orgImpAddInfo = (DEOrgImpAddInfo)declarant.CountryData.ImpAddInfo;
				AssertEquals("ZO_VATClaimBack->Empty", false, declaration.IsDeclarantEntitledToClaimBackVAT);
				orgImpAddInfo.ZO_VATClaimBack = YesNoList.Codes.Yes;
				AssertEquals("ZO_VATClaimBack->Yes", true, declaration.IsDeclarantEntitledToClaimBackVAT);
				orgImpAddInfo.ZO_VATClaimBack = YesNoList.Codes.No;
				AssertEquals("ZO_VATClaimBack->No", false, declaration.IsDeclarantEntitledToClaimBackVAT);
			});
		}

		public void TestVATClaimBackProperties_CompanyIsNotDE()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var deCountryData = organization.GetCountryData(Core.Constants.CountryCodes.Germany);
			((DEOrgImpAddInfo)deCountryData.ImpAddInfo).ZO_VATClaimBack = YesNoList.Codes.Yes;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				CombineAssertions(() =>
				{
					declaration.JE_OA_DeclarantAddress = organization.MainAddress.PK;
					declaration.JE_OH_Importer = organization.PK;
					AssertNoExceptionThrown("IsDeclarantEntitledToClaimBackVAT", () => _ = declaration.IsDeclarantEntitledToClaimBackVAT);
					AssertNoExceptionThrown("VATClaimBack", () => _ = declaration.JE_VATClaimBack);
				});
			}
		}

		public void TestControlMessageUnreadEDocStatus()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default", ZString.Empty, declaration.ControlMessageUnreadEDocStatus);
				declaration.ControlMessageUnreadEDocStatus = YesNoList.Codes.Yes;
				AssertEquals("True", YesNoList.Codes.Yes, declaration.ControlMessageUnreadEDocStatus);
			});
		}

		public void TestControlMessageUnreadEDocStatus_GenAddOnColumn()
		{
			var query = new ZQuery(GenAddOnColumnSchema.XA_Name, nameof(JobDeclaration.ControlMessageUnreadEDocStatus));
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, JobDeclarationSchema.Constants.Prefix);
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentID, declaration.PK);

			CombineAssertions(() =>
			{
				AssertEquals("No GenAddOn", false, Factory.Exists(typeof(GenAddOnColumn), query));
				declaration.ControlMessageUnreadEDocStatus = YesNoList.Codes.Yes;
				AssertEquals("Column Exists True", true, Factory.Exists(typeof(GenAddOnColumn), query));
			});
		}

		public override void TestCustomsOfficeOfExit()
		{
			JobDeclaration decEcs = Factory.New<JobDeclaration>();
			decEcs.JE_MessageType = "EXP";
			AssertEquals("", decEcs.OfficeOfExit);
			decEcs.CustomsOffices.RemoveAndDeleteAll();
			Factory.Save();
			var cusOffice = decEcs.CustomsOffices.AddNew();
			cusOffice.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExit;
			cusOffice.CY_Data = "GB000001";
			AssertEquals("GB000001", decEcs.OfficeOfExit);

			decEcs.JE_MessageType = "1";
			decEcs.JE_ApplicationCode = "EMC";
			Assert(decEcs.IsEMCS);
		}

		public void TestDeclarantAddressRequired_Import()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = ZString.Empty;
			AssertEquals("DeclarantType empty", true, declaration.IsDeclarantAddressRequired);
			foreach (var declarantType in new RepresentationTypeList().GetAllCodes())
			{
				declaration.JE_DeclarantType = declarantType;
				AssertEquals($"JE_DeclarantType {declarantType}", true, declaration.IsDeclarantAddressRequired);
			}
		}

		public void TestJE_OA_SellerAddress_GetDefaultAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			declaration.JE_OA_SellerAddress_ZAddress.OrgPK = orgHeader.PK;

			AssertEquals(orgHeader.MainAddress.PK, declaration.JE_OA_SellerAddress);
		}

		public void TestJE_OA_BuyingAgentAddress_GetDefaultAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			declaration.JE_OA_BuyingAgentAddress_ZAddress.OrgPK = orgHeader.PK;

			AssertEquals(orgHeader.MainAddress.PK, declaration.JE_OA_BuyingAgentAddress);
		}

		public void TestJE_OA_ConsigneeAddress_GetDefaultAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			declaration.JE_OA_ConsigneeAddress_ZAddress.OrgPK = orgHeader.PK;

			AssertEquals(orgHeader.MainAddress.PK, declaration.JE_OA_ConsigneeAddress);
		}

		public void TestJE_OA_DeclarantAddress_GetDefaultAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			declaration.JE_OA_DeclarantAddress_ZAddress.OrgPK = orgHeader.PK;

			AssertEquals(orgHeader.MainAddress.PK, declaration.JE_OA_DeclarantAddress);
		}

		public void TestJE_OA_Representative_GetDefaultAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			declaration.JE_OA_Representative_ZAddress.OrgPK = orgHeader.PK;

			AssertEquals(orgHeader.MainAddress.PK, declaration.JE_OA_Representative);
		}

		public void TestGetAuthorizationOrgAddress_IMP_RepresentationTypeDIR()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			var declarantOrgAddressPK = Factory.New<OrgHeader>().MainAddress.PK;
			var representativeOrgAddressPK = Factory.New<OrgHeader>().MainAddress.PK;
			declaration.JE_OA_DeclarantAddress = declarantOrgAddressPK;
			declaration.JE_OA_Representative = representativeOrgAddressPK;
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { declarantOrgAddressPK, representativeOrgAddressPK }, declaration.GetAuthorizationOrgAddress().Select(x => x.PK));
		}

		public void TestGetAuthorizationOrgAddress_IMP_RepresentationTypeSEL()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			var declarantOrgAddressPK = Factory.New<OrgHeader>().MainAddress.PK;
			declaration.JE_OA_DeclarantAddress = declarantOrgAddressPK;
			AssertEquals("IMP|SEL|Declarant", declarantOrgAddressPK, declaration.GetAuthorizationOrgAddress().Single().PK);
		}

		public void TestGetAuthorizationOrgAddress_IMP_RepresentationTypeIND()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			var declarantOrgAddressPK = Factory.New<OrgHeader>().MainAddress.PK;
			declaration.JE_OA_DeclarantAddress = declarantOrgAddressPK;
			AssertEquals("IMP|IND|Declarant", declarantOrgAddressPK, declaration.GetAuthorizationOrgAddress().Single().PK);
		}

		public void TestGetAuthorizationOrgAddress_EXP()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var declarantOrgAddressPK = Factory.New<OrgHeader>().MainAddress.PK;
			var representativeOrgAddressPK = Factory.New<OrgHeader>().MainAddress.PK;
			var sellerOrgAddressPK = Factory.New<OrgHeader>().MainAddress.PK;
			var supplierOrgAddressPK = Factory.New<OrgHeader>().MainAddress.PK;
			declaration.JE_OA_DeclarantAddress = declarantOrgAddressPK;
			declaration.JE_OA_Representative = representativeOrgAddressPK;
			declaration.JE_OA_SellerAddress = sellerOrgAddressPK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplierOrgAddressPK;
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { declarantOrgAddressPK, representativeOrgAddressPK, sellerOrgAddressPK, supplierOrgAddressPK }, declaration.GetAuthorizationOrgAddress().Select(x => x.PK));
		}

		public void TestAuthorizationNumberDefaultedFromDeclarant_IMP_NoAuthorization()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			instruction1.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			var previousDocument1 = instruction1.GetOnlyPreviousDocument();
			previousDocument1.AuthorizationNumber = ZString.Empty;

			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			var previousDocument2 = instruction2.GetOnlyPreviousDocument();
			previousDocument2.AuthorizationNumber = ZString.Empty;

			var declarantOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var declarantOrgAddress = declarantOrgHeader.MainAddress;
			declaration.JE_OA_DeclarantAddress = declarantOrgAddress.PK;

			AssertEquals("No Default", "", previousDocument1.AuthorizationNumber);
			AssertEquals("No Default", "", previousDocument2.AuthorizationNumber);
		}

		public void TestAuthorizationNumberDefaultedFromDeclarant_IMP_OneAuthorization()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			instruction1.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			var previousDocument1 = instruction1.GetOnlyPreviousDocument();
			previousDocument1.AuthorizationNumber = ZString.Empty;

			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			var previousDocument2 = instruction2.GetOnlyPreviousDocument();
			previousDocument2.AuthorizationNumber = ZString.Empty;

			var declarantOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var declarantOrgAddress = declarantOrgHeader.MainAddress;
			declarantOrgAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "DEFAULT1");
			declarantOrgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "DEFAULT2");

			declaration.JE_OA_DeclarantAddress = declarantOrgAddress.PK;

			AssertEquals("DEFAULT1", previousDocument1.AuthorizationNumber);
			AssertEquals("DEFAULT2", previousDocument2.AuthorizationNumber);
		}

		public void TestAuthorizationNumberDefaultedFromDeclarant_IMP_MultipleAuthorization()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			instruction1.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			var previousDocument1 = instruction1.GetOnlyPreviousDocument();
			previousDocument1.AuthorizationNumber = ZString.Empty;

			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			var previousDocument2 = instruction2.GetOnlyPreviousDocument();
			previousDocument2.AuthorizationNumber = ZString.Empty;

			var declarantOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var declarantOrgAddress = declarantOrgHeader.MainAddress;

			declarantOrgAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "DEFAULT1");
			declarantOrgAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "DEFAULT2");
			declarantOrgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "DEFAULT3");
			declarantOrgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "DEFAULT4");
			declaration.JE_OA_DeclarantAddress = declarantOrgAddress.PK;

			AssertEquals("No Default", "", previousDocument1.AuthorizationNumber);
			AssertEquals("No Default", "", previousDocument2.AuthorizationNumber);
		}

		public void TestAuthorizationNumberDefaultedFromDeclarant_EXP()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			var previousDocument = instruction.GetOnlyPreviousDocument();
			previousDocument.AuthorizationNumber = ZString.Empty;

			var declarantOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var declarantOrgAddress = declarantOrgHeader.MainAddress;
			declarantOrgAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "DEFAULT1");

			declaration.JE_OA_DeclarantAddress = declarantOrgAddress.PK;
			AssertEquals(ZString.Empty, previousDocument.AuthorizationNumber);
		}

		public void TestAuthorizationNumberDefaultedFromRepresentative()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			instruction1.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			var previousDocument1 = instruction1.GetOnlyPreviousDocument();
			previousDocument1.AuthorizationNumber = ZString.Empty;

			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			var previousDocument2 = instruction2.GetOnlyPreviousDocument();
			previousDocument2.AuthorizationNumber = ZString.Empty;

			var representativeOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var representativeOrgAddress = representativeOrgHeader.MainAddress;
			representativeOrgAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "DEFAULT1");
			representativeOrgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "DEFAULT2");

			declaration.JE_OA_Representative = representativeOrgAddress.PK;

			AssertEquals("DEFAULT1", previousDocument1.AuthorizationNumber);
			AssertEquals("DEFAULT2", previousDocument2.AuthorizationNumber);
		}

		public void TestJE_IsHighValueOvrd_SetValuesForCharges()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var groupInvoiceCharge = declaration.TopGroupInvoice.Charges.AddNew();
			var groupInvoiceCharge2 = declaration.TopGroupInvoice.Charges.AddNew();

			var invoice = declaration.Invoices.AddNew();
			var invoiceCharge = invoice.Charges.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceCharge2 = invoice.Charges.AddNew();

			var invoiceLine = invoice.InvoiceLines.AddNew();
			var invoiceLineCharge = invoiceLine.Charges.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			var invoiceLineCharge2 = invoiceLine2.Charges.AddNew();

			CombineAssertions(() =>
			{
				groupInvoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes._001;
				groupInvoiceCharge.J7_IsDutiable = ZBool.False;
				groupInvoiceCharge2.J7_ChargeType = ImportChargeCodeList.Codes._002;
				groupInvoiceCharge2.J7_IsStatisticalValueApplicable = ZBool.False;

				invoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes._003;
				invoiceCharge.J7_IsGSTApplicable = ZBool.False;
				invoiceCharge2.J7_ChargeType = ImportChargeCodeList.Codes._014;
				invoiceCharge2.J7_Calc_IsIncludedInInvoiceAmount = ZBool.False;

				invoiceLineCharge.J7_ChargeType = ImportChargeCodeList.Codes._015;
				invoiceLineCharge.J7_IsIncludedInITOT = ZBool.False;
				invoiceLineCharge2.J7_ChargeType = ImportChargeCodeList.Codes.AIR;
				invoiceLineCharge2.IsJ7_ExchangeRateIATA = ZBool.False;

				declaration.ZG_IsHighValueOvrd = ZBool.True;
				AssertEquals("J7_ChargeType is 001, groupInvoiceCharge.J7_IsDutiable", true, groupInvoiceCharge.J7_IsDutiable);
				AssertEquals("J7_ChargeType is 002, groupInvoiceCharge2.J7_IsStatisticalValueApplicable", true, groupInvoiceCharge2.J7_IsStatisticalValueApplicable);
				AssertEquals("J7_ChargeType is 003, invoiceCharge.J7_IsGSTApplicable", true, invoiceCharge.J7_IsGSTApplicable);
				AssertEquals("J7_ChargeType is 014, invoiceCharge2.J7_Calc_IsIncludedInInvoiceAmount", true, invoiceCharge2.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("J7_ChargeType is 015, invoiceLineCharge.J7_IsIncludedInITOT", true, invoiceLineCharge.J7_IsIncludedInITOT);
				AssertEquals("J7_ChargeType is AIR, invoiceLineCharge2.IsJ7_ExchangeRateIATA", true, invoiceLineCharge2.IsJ7_ExchangeRateIATA);
			});
		}

		public void TestZG_IsHighValueOvrd_CreateDV1Details()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				AssertEquals("Initially there are no DV1Details", false, declaration.DV1Details.Any());

				declaration.ZG_IsHighValueOvrd = true;
				var newDV1Detail = (CusDV1Detail)declaration.DV1Details.SingleOrDefault();
				AssertNotNull("Setting ZG_IsHighValueOvrd to True creates a new DV1Detail", newDV1Detail);
				AssertEquals("Sequence of newDV1Detail", (ZShort)1, newDV1Detail.Sequence);
				AssertEquals("DV1_Relationship of newDV1Detail", YesNoList.Codes.No, newDV1Detail.DV1_Relationship);
				AssertEquals("DV1_Restrictions of newDV1Detail", YesNoList.Codes.No, newDV1Detail.DV1_Restrictions);
				AssertEquals("DV1_Consideration of newDV1Detail", YesNoList.Codes.No, newDV1Detail.DV1_Consideration);
				AssertEquals("DV1_RoyaltiesLicence of newDV1Detail", YesNoList.Codes.No, newDV1Detail.DV1_RoyaltiesLicence);
				AssertEquals("DV1_Resale of newDV1Detail", YesNoList.Codes.No, newDV1Detail.DV1_Resale);

				declaration.DV1Details.RemoveAndDeleteAll();
				declaration.ZG_IsHighValueOvrd = false;
				AssertEquals("Setting ZG_IsHighValueOvrd to False doesn't create DV1Detail", false, declaration.DV1Details.Any());
			});
		}

		public void TestDocsAndCartageType() => AssertType<ForwardingDocsAndCartage>(declaration.DocsAndCartage);

		public void TestDocsAndCartageParentType() => AssertType<JobDeclaration>(declaration.DocsAndCartage.Parent);

		public void TestGetNumericIncoTermModeCodeFromWtgCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.Germany))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				AssertEquals("3", declaration.GetNumericIncoTermModeCodeFromWtgCode(OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.THS));
				AssertEquals("2", declaration.GetNumericIncoTermModeCodeFromWtgCode(OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.OTH));
				AssertEquals("1", declaration.GetNumericIncoTermModeCodeFromWtgCode(OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.OUT));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.Switzerland))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				AssertEquals("3", declaration.GetNumericIncoTermModeCodeFromWtgCode(OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.THS));
				AssertEquals("2", declaration.GetNumericIncoTermModeCodeFromWtgCode(OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.OTH));
				AssertEquals("1", declaration.GetNumericIncoTermModeCodeFromWtgCode(OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.OUT));
			}
		}

		public void TestContractualPartnerDocAddressRequirement_ValidateOrganisationPK()
		{
			const string message = "The chosen Party Constellation requires a Contractual Partner to be entered.";
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

			AssertNoMessageError("Contractual partner is empty and party constellation is empty", declaration.ContractualPartnerDocAddress.OrganisationPKInfo, message);

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.ZG_PartyConstellation = "0001";
			AssertNoMessageError("Contractual partner is empty and party constellation starts with 0", declaration.ContractualPartnerDocAddress.OrganisationPKInfo, message);

			entryInstruction.ZG_PartyConstellation = "1001";
			AssertHasMessageError("Contractual partner is empty and party constellation doesn't start with 0", declaration.ContractualPartnerDocAddress.OrganisationPKInfo, message);

			declaration.ContractualPartnerDocAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			AssertNoMessageError("Contractual isn't empty, no message error", declaration.ContractualPartnerDocAddress.OrganisationPKInfo, message);
		}

		public void TestExporterDocAddressRequirement_ValidateOrganisationPK()
		{
			const string message = "The chosen Party Constellation requires an Exporter to be entered.";
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

			AssertNoMessageError("Exporter is empty and party constellation is empty", declaration.ExporterDocAddress.OrganisationPKInfo, message);

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.ZG_PartyConstellation = "0000";
			AssertNoMessageError("Exporter is empty and second digit of ZG_PartyConstellation isn't 1", declaration.ExporterDocAddress.OrganisationPKInfo, message);

			entryInstruction.ZG_PartyConstellation = "0100";
			AssertHasMessageError("Exporter is empty and second digit of ZG_PartyConstellation is 1", declaration.ExporterDocAddress.OrganisationPKInfo, message);

			declaration.ExporterDocAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			AssertNoMessageError("Exporter isn't empty, no message error", declaration.ExporterDocAddress.OrganisationPKInfo, message);
		}

		public void TestValidation()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertType<ImportJobDeclarationValidation>("JE_MessageType is 'IMP'", declaration.Validation);

				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
				AssertType<StockMovementJobDeclarationValidation>("JE_MessageType is 'IMP' and all CEI_Style of CustomsEntryInstructions is 'LUZ'", declaration.Validation);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertType<ExportJobDeclarationValidation>("JE_MessageType is 'EXP'", declaration.Validation);

				declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment;
				AssertType<WarehouseAdjustmentJobDeclarationValidation>("JE_MessageType is 'WAD'", declaration.Validation);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms;
				AssertType<JobDeclarationValidation>("JE_MessageType is 'MSC'", declaration.Validation);
			});
		}

		public void TestJE_ContainerMode_ClearPackagesSealNumber()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
			var package = declaration.Packages.AddNew();
			package.CW_Seal = "Seal123";

			CombineAssertions(() =>
			{
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.Liquid;
				AssertEquals("Change JE_ContainerMode to a different non-containerized mode", "Seal123", package.CW_Seal);

				declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
				AssertEquals("Change JE_ContainerMode to a containerized mode", ZString.Empty, package.CW_Seal);
			});
		}

		public void TestIsStockMovement()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				AssertEquals("No Instruction", ZBool.False, declaration.IsStockMovement);

				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
				AssertEquals("All CEI_Style of Instructions is 'LUZ'", ZBool.True, declaration.IsStockMovement);

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
				AssertEquals("Not all CEI_Style of Instructions is 'LUZ'", ZBool.False, declaration.IsStockMovement);
			});
		}

		public void TestIsImportAndNotStockMovement()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				AssertEquals("No Instruction", ZBool.True, declaration.IsImportAndNotStockMovement);

				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
				AssertEquals("All CEI_Style of Instructions is 'LUZ'", ZBool.False, declaration.IsImportAndNotStockMovement);

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
				AssertEquals("Not all CEI_Style of Instructions is 'LUZ'", ZBool.True, declaration.IsImportAndNotStockMovement);
			});
		}

		public void TestJE_MergeBy_GetsDefaultedIfIMPorWAD()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
				declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.Import;
				AssertEquals("Set JE_MessageType to IMP", OrgConstants.MergeInvoiceLines.NotMerge, declaration.JE_MergeBy);

				declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
				declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment;
				AssertEquals("Set JE_MessageType to WAD", OrgConstants.MergeInvoiceLines.NotMerge, declaration.JE_MergeBy);
			});
		}

		public void TestCusContainers()
		{
			AssertType<CusContainerCollection>(declaration.CusContainers);
		}

		public void TestAuthorizationUsageUpdater_Import()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<CusAuthorizationUsageImportUpdater>(declaration.AuthorizationUsageUpdater);
		}

		public void TestAuthorizationUsageUpdater_Export()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<CusAuthorizationUsageExportUpdater>(declaration.AuthorizationUsageUpdater);
		}

		public void TestAuthorizationUsageUpdater_MiscellaneousCustoms()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<CusAuthorizationUsageUpdater>(declaration.AuthorizationUsageUpdater);
		}

		public void TestSaveCreatesSnapshot()
		{
			var orgHeader = declaration.Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.EntryNumber = "ATBENTRYNUMBER";
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AZ;
			var reconEntry = declaration.Factory.New<CusReconEntry>();
			reconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			reconEntry.CRE_EntryType = CusReconConstants.Lodged;
			var snapshot = reconEntry.CusReconSnapshots.AddNew();
			snapshot.CRS_Type = CusReconConstants.Lodged;
			snapshot.CRS_SnapshotXml = CusReconEntrySnapshotBuilder.Serialize(new DEMonthlyClosingEntrySnapshot()
			{
				Document = new DEMonthlyClosingEntrySnapshotDocument[] {
					new DEMonthlyClosingEntrySnapshotDocument { Division = DEMonthlyClosingEntrySnapshotDocumentDivision.Item4, Type = "N380" }
				}
			});
			declaration.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
			Factory.Save();

			var orgHeader2 = declaration.Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OA_DeclarantAddress = orgHeader2.MainAddress.PK;

			declaration.Factory.Save();

			AssertEquals(2, reconEntry.CusReconSnapshots.Count);
		}

		public void TestSaveDoesntCreateSnapshot_SkipSnapshotUpdateTrue()
		{
			var orgHeader = declaration.Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.EntryNumber = "ATBENTRYNUMBER";
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AZ;
			var reconEntry = declaration.Factory.New<CusReconEntry>();
			reconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			reconEntry.CRE_EntryType = CusReconConstants.Lodged;
			var snapshot = reconEntry.CusReconSnapshots.AddNew();
			snapshot.CRS_Type = CusReconConstants.Lodged;
			snapshot.CRS_SnapshotXml = CusReconEntrySnapshotBuilder.Serialize(new DEMonthlyClosingEntrySnapshot()
			{
				Document = new DEMonthlyClosingEntrySnapshotDocument[] {
					new DEMonthlyClosingEntrySnapshotDocument { Division = DEMonthlyClosingEntrySnapshotDocumentDivision.Item4, Type = "N380" }
				}
			});
			declaration.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
			Factory.Save();

			var orgHeader2 = declaration.Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OA_DeclarantAddress = orgHeader2.MainAddress.PK;

			declaration.SkipSnapshotUpdate = true;
			declaration.Factory.Save();

			AssertEquals(1, reconEntry.CusReconSnapshots.Count);
		}

		public void TestOnFactorySavedSetsSkipSnapshotUpdateToFalse()
		{
			CombineAssertions(() =>
			{
				declaration.SkipSnapshotUpdate = true;
				declaration.Factory.Save();
				AssertEquals("true => false", false, declaration.SkipSnapshotUpdate);

				declaration.Factory.Save();
				AssertEquals("false => false", false, declaration.SkipSnapshotUpdate);
			});
		}

		public void TestUpdateCusReconEntriesDeclarantAddress()
		{
			AssertSimplifiedCusReconEntriesAddressUpdated((dec, address) => dec.JE_OA_DeclarantAddress = address, entry => entry.CRE_OA_DeclarantAddress);
		}

		public void TestUpdateCusReconEntriesRepresentativeAddress()
		{
			AssertSimplifiedCusReconEntriesAddressUpdated((dec, address) => dec.JE_OA_Representative = address, entry => entry.CRE_OA_RepresentativeAddress);
		}

		public void TestUpdateCusReconEntriesImporterAddress()
		{
			AssertSimplifiedCusReconEntriesAddressUpdated((dec, address) => dec.ImporterDocumentaryAddress.E2_OA_Address = address, entry => entry.CRE_OA_ImporterAddress);
		}

		public void TestUpdateCusReconEntriesBuyingAgentAddress()
		{
			AssertSimplifiedCusReconEntriesAddressUpdated((dec, address) => dec.JE_OA_BuyingAgentAddress = address, entry => entry.CRE_OA_BuyingAgentAddress);
		}

		public void TestSkipSnapshotUpdate()
		{
			AssertEquals("default", false, declaration.SkipSnapshotUpdate);
		}

		public void TestDefaultMessageTypeFromSupplierOrImporter()
		{
			var deOrg = Factory.New<OrgHeader>();
			deOrg.OH_RL_NKClosestPort = "DEBER";
			var deOrg2 = Factory.New<OrgHeader>();
			deOrg2.OH_RL_NKClosestPort = "DEBER";
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			CombineAssertions(() =>
			{
				declaration.SupplierDocumentaryAddress.E2_OA_Address = deOrg.MainAddress.PK;
				AssertEquals("Import and no Instruction, JE_MessageType is changed to 'EXP'", Common.Shared.SharedJobMessageTypeList.Codes.Export, declaration.JE_MessageType);

				declaration.CustomsEntryInstructions.AddNew();
				declaration.ImporterDocumentaryAddress.E2_OA_Address = deOrg.MainAddress.PK;
				AssertEquals("Export, JE_MessageType is changed to 'IMP'", Common.Shared.SharedJobMessageTypeList.Codes.Import, declaration.JE_MessageType);

				declaration.SupplierDocumentaryAddress.E2_OA_Address = deOrg2.MainAddress.PK;
				AssertEquals("Import and has Instruction, JE_MessageType isn't changed", Common.Shared.SharedJobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			});
		}

		public void TestJE_EntryStyleNotReadOnly_Import()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.Import;
			AssertEquals(false, declaration.JE_EntryStyleInfo.ReadOnly);
		}

		public void TestJE_EntryStyleNotReadOnly_Export()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.Export;
			AssertEquals(false, declaration.JE_EntryStyleInfo.ReadOnly);
		}

		public void TestJE_GoodsDestinationNotReadOnly_Import()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.Import;
			AssertEquals(false, declaration.JE_GoodsDestinationInfo.ReadOnly);
		}

		public void TestJE_GoodsDestinationNotReadOnly_Export()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.Export;
			AssertEquals(false, declaration.JE_GoodsDestinationInfo.ReadOnly);
		}

		public void TestJE_DeclarantType_DIR_EmptyRepresentative()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.Import;
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			declaration.JE_DeclarantType = string.Empty; // Defaults as Export and Direct, also importer calls set strategy for JE_DeclarantType
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			CombineAssertions(() =>
			{
				AssertEquals("JE_OA_Representative", GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, declaration.JE_OA_Representative);
				AssertEquals("JE_OA_DeclarantAddress", importer.MainAddress.PK, declaration.JE_OA_DeclarantAddress);
			});
		}

		public void TestJE_DeclarantType_DIR_OverrideExistingRepresentative()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.Import;
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			declaration.JE_OA_Representative = Factory.New<OrgAddress>().PK;
			declaration.JE_DeclarantType = string.Empty; // Defaults as Export and Direct, also importer calls set strategy for JE_DeclarantType
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			CombineAssertions(() =>
			{
				AssertEquals("JE_OA_Representative", GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, declaration.JE_OA_Representative);
				AssertEquals("JE_OA_DeclarantAddress", importer.MainAddress.PK, declaration.JE_OA_DeclarantAddress);
			});
		}

		public void TestJE_DeclarantType_IND_EmptyRepresentedParty()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.Import;
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			declaration.JE_DeclarantType = string.Empty; // Defaults as Export and Direct, also importer calls set strategy for JE_DeclarantType
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			CombineAssertions(() =>
			{
				AssertEquals("JE_OA_BuyingAgentAddress", importer.MainAddress.PK, declaration.JE_OA_BuyingAgentAddress);
				AssertEquals("JE_OA_DeclarantAddress", GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, declaration.JE_OA_DeclarantAddress);
			});
		}

		public void TestJE_DeclarantType_IND_OverrideExistingRepresentedParty()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.Import;
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			declaration.JE_OA_BuyingAgentAddress = ZGuid.BrettsGuid;
			declaration.JE_DeclarantType = string.Empty; // Defaults as Export and Direct, also importer calls set strategy for JE_DeclarantType
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			CombineAssertions(() =>
			{
				AssertEquals("JE_OA_BuyingAgentAddress", importer.MainAddress.PK, declaration.JE_OA_BuyingAgentAddress);
				AssertEquals("JE_OA_DeclarantAddress", GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, declaration.JE_OA_DeclarantAddress);
			});
		}

		public void TestJE_DeclarantType_SEL_EmptyDeclarant()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.Import;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.JE_DeclarantType = string.Empty; // Defaults as Export and Direct, also importer calls set strategy for JE_DeclarantType
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			AssertEquals(GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, declaration.JE_OA_DeclarantAddress);
		}

		public void TestJE_DeclarantType_SEL_OverrideExistingDeclarant()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.Import;
			declaration.JE_OA_DeclarantAddress = ZGuid.BrettsGuid;
			declaration.JE_DeclarantType = string.Empty; // Defaults as Export and Direct, also importer calls set strategy for JE_DeclarantType
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			AssertEquals(GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, declaration.JE_OA_DeclarantAddress);
		}

		public void TestFilteredInvoiceLines()
		{
			AssertType<EU.Business.Declaration.InvoiceLineViewCollection<JobComInvoiceLine>>(declaration.FilteredInvoiceLines);
		}

		public void TestOnSaving_OrganizationSellerDefaultValue_IsSet()
		{
			var supplierOrg = Factory.NewWithValidTestData<OrgHeader>();

			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplierOrg.MainAddress.PK;
			declaration.ZG_IsHighValueOvrd = true;

			Factory.Save();

			AssertEquals(supplierOrg.MainAddress.PK, declaration.JE_OA_SellerAddress);
		}

		public void TestOnSaving_OrganizationSellerDefaultValue_IsNotSetWhenValuePresent()
		{
			var supplierOrg = Factory.NewWithValidTestData<OrgHeader>();
			var sellerOrg = Factory.NewWithValidTestData<OrgHeader>();

			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplierOrg.MainAddress.PK;
			declaration.JE_OA_SellerAddress = sellerOrg.MainAddress.PK;
			declaration.ZG_IsHighValueOvrd = true;

			Factory.Save();

			AssertEquals(sellerOrg.MainAddress.PK, declaration.JE_OA_SellerAddress);
		}

		public void TestOnSaving_OrganizationSellerDefaultValue_IsNotSetWhenDV1NotChecked()
		{
			var supplierOrg = Factory.NewWithValidTestData<OrgHeader>();

			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplierOrg.MainAddress.PK;

			Factory.Save();

			AssertEquals(true, declaration.JE_OA_SellerAddress.IsEmpty);
		}

		public void TestOnSaving_OrganizationBuyerDefaultValue_IsSet()
		{
			var importerOrg = Factory.NewWithValidTestData<OrgHeader>();

			declaration.ImporterDocumentaryAddress.E2_OA_Address = importerOrg.MainAddress.PK;
			declaration.ZG_IsHighValueOvrd = true;

			Factory.Save();

			AssertEquals(importerOrg.MainAddress.PK, declaration.JE_OA_ConsigneeAddress);
		}

		public void TestOnSaving_OrganizationBuyerDefaultValue_IsNotSetWhenValuePresent()
		{
			var importerOrg = Factory.NewWithValidTestData<OrgHeader>();
			var buyerOrg = Factory.NewWithValidTestData<OrgHeader>();

			declaration.ImporterDocumentaryAddress.E2_OA_Address = importerOrg.MainAddress.PK;
			declaration.JE_OA_ConsigneeAddress = buyerOrg.MainAddress.PK;
			declaration.ZG_IsHighValueOvrd = true;

			Factory.Save();

			AssertEquals(buyerOrg.MainAddress.PK, declaration.JE_OA_ConsigneeAddress);
		}

		public void TestOnSaving_OrganizationBuyerDefaultValue_IsNotSetWhenDV1NotChecked()
		{
			var importerOrg = Factory.NewWithValidTestData<OrgHeader>();
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importerOrg.MainAddress.PK;

			Factory.Save();

			AssertEquals(true, declaration.JE_OA_ConsigneeAddress.IsEmpty);
		}

		public void TestOnSaving_DefaultReferralData_Export()
		{
			var defermentParty = CreateOrgWithAccounts("DefermentParty", "DEFER", OrgCusAccountCodeList.Codes.ImportDutiesOneMonth, false);
			declaration.DefermentPartyDocAddress.OrganisationPK = defermentParty.PK;

			declaration.ZG_MethodOfPayment = UniversalReferenceConstants.MethodOfPaymentTypes.E;
			declaration.OnSaving();

			AssertEquals("JE_PaymentMethod wasn't updated", ZString.Empty, declaration.JE_PaymentMethod);
		}

		public void TestOnSaving_DefaultReferralData_DataIsFilled()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var defermentParty = CreateOrgWithAccounts("DefermentParty", "DEFER", OrgCusAccountCodeList.Codes.ImportDutiesOneMonth, createVatAccount: false);
			declaration.DefermentPartyDocAddress.OrganisationPK = defermentParty.PK;

			CombineAssertions(() =>
			{
				declaration.ZG_MethodOfPayment = UniversalReferenceConstants.MethodOfPaymentTypes.E;
				declaration.JE_PaymentMethod = DeferralPaymentPartyList.Codes.Representative;
				declaration.OnSaving();
				AssertEquals("JE_PaymentMethod isn't empty", DeferralPaymentPartyList.Codes.Representative, declaration.JE_PaymentMethod);

				declaration.JE_PaymentMethod = ZString.Empty;
				declaration.ZG_VATDeferType = DeferralPaymentPartyList.Codes.DefermentParty;
				declaration.OnSaving();
				AssertEquals("ZG_VATDeferType isn't empty", ZString.Empty, declaration.JE_PaymentMethod);
			});
		}

		public void TestOnSaving_DefaultReferralData_DefermentParty10()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var defermentParty = CreateOrgWithAccounts("DefermentParty", "DEFER", OrgCusAccountCodeList.Codes.ImportDutiesOneMonth, createVatAccount: false);
			declaration.DefermentPartyDocAddress.OrganisationPK = defermentParty.PK;
			var declarant = CreateOrgWithAccounts("Declarant", "DECL", OrgCusAccountCodeList.Codes.ImportDutiesOneMonth, createVatAccount: true);
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			var representative = CreateOrgWithAccounts("Representative", "REP", OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT, createVatAccount: true);
			declaration.JE_OA_Representative = representative.MainAddress.PK;

			AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.E, DeferralPaymentPartyList.Codes.DefermentParty, "DEFER10");
		}

		public void TestOnSaving_DefaultReferralData_DefermentParty15()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var defermentParty = CreateOrgWithAccounts("DefermentParty", "DEFER", OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT, createVatAccount: false);
			declaration.DefermentPartyDocAddress.OrganisationPK = defermentParty.PK;
			var declarant = CreateOrgWithAccounts("Declarant", "DECL", OrgCusAccountCodeList.Codes.ImportDutiesOneMonth, createVatAccount: false);
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			var representative = CreateOrgWithAccounts("Representative", "REP", OrgCusAccountCodeList.Codes.ImportDutiesOneMonth, createVatAccount: false);
			declaration.JE_OA_Representative = representative.MainAddress.PK;

			AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.E, DeferralPaymentPartyList.Codes.DefermentParty, "DEFER15");
		}

		public void TestOnSaving_DefaultReferralData_DefermentParty20()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var defermentParty = CreateOrgWithAccounts("DefermentParty", "DEFER", string.Empty, createVatAccount: true);
			declaration.DefermentPartyDocAddress.OrganisationPK = defermentParty.PK;

			AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.F, string.Empty, string.Empty, DeferralPaymentPartyList.Codes.DefermentParty, "DEFER20");
		}

		public void TestOnSaving_DefaultReferralData_DefermentParty10_20()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var defermentParty = CreateOrgWithAccounts("DefermentParty", "DEFER", OrgCusAccountCodeList.Codes.ImportDutiesOneMonth, createVatAccount: true);
			declaration.DefermentPartyDocAddress.OrganisationPK = defermentParty.PK;
			var declarant = CreateOrgWithAccounts("Declarant", "DECL", OrgCusAccountCodeList.Codes.ImportDutiesOneMonth, createVatAccount: true);
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			var representative = CreateOrgWithAccounts("Representative", "REP", OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT, createVatAccount: true);
			declaration.JE_OA_Representative = representative.MainAddress.PK;

			CombineAssertions(() =>
			{
				AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.A);
				AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.C);
				AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.D);
				AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.E, DeferralPaymentPartyList.Codes.DefermentParty, "DEFER10", DeferralPaymentPartyList.Codes.DefermentParty, "DEFER20");
				AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.F, DeferralPaymentPartyList.Codes.DefermentParty, "DEFER10", DeferralPaymentPartyList.Codes.DefermentParty, "DEFER20");
				AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.G, DeferralPaymentPartyList.Codes.DefermentParty, "DEFER10", DeferralPaymentPartyList.Codes.DefermentParty, "DEFER20");
				AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.Z, DeferralPaymentPartyList.Codes.DefermentParty, "DEFER10", DeferralPaymentPartyList.Codes.DefermentParty, "DEFER20");
			});
		}

		public void TestOnSaving_DefaultReferralData_DefermentParty15_20()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var defermentParty = CreateOrgWithAccounts("DefermentParty", "DEFER", OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT, createVatAccount: true);
			declaration.DefermentPartyDocAddress.OrganisationPK = defermentParty.PK;
			var representative = CreateOrgWithAccounts("Representative", "REP", OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT, createVatAccount: true);
			declaration.JE_OA_Representative = representative.MainAddress.PK;

			AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.E, DeferralPaymentPartyList.Codes.DefermentParty, "DEFER15", DeferralPaymentPartyList.Codes.DefermentParty, "DEFER20");
		}

		public void TestOnSaving_DefaultReferralData_DefermentParty15_Declarant20()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var defermentParty = CreateOrgWithAccounts("DefermentParty", "DEFER", OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT, createVatAccount: false);
			declaration.DefermentPartyDocAddress.OrganisationPK = defermentParty.PK;
			var declarant = CreateOrgWithAccounts("Declarant", "DECL", OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT, createVatAccount: true);
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			var representative = CreateOrgWithAccounts("Representative", "REP", OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT, createVatAccount: true);
			declaration.JE_OA_Representative = representative.MainAddress.PK;

			AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.E, DeferralPaymentPartyList.Codes.DefermentParty, "DEFER15", DeferralPaymentPartyList.Codes.Declarant, "DECL20");
		}

		public void TestOnSaving_DefaultReferralData_DefermentParty15_Representative20()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var defermentParty = CreateOrgWithAccounts("DefermentParty", "DEFER", OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT, createVatAccount: false);
			declaration.DefermentPartyDocAddress.OrganisationPK = defermentParty.PK;
			var representative = CreateOrgWithAccounts("Representative", "REP", OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT, createVatAccount: true);
			declaration.JE_OA_Representative = representative.MainAddress.PK;

			AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.E, DeferralPaymentPartyList.Codes.DefermentParty, "DEFER15", DeferralPaymentPartyList.Codes.Representative, "REP20");
		}

		public void TestOnSaving_DefaultReferralData_Declarant10()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var defermentParty = CreateOrgWithAccounts("DefermentParty", string.Empty, string.Empty, createVatAccount: false);
			declaration.DefermentPartyDocAddress.OrganisationPK = defermentParty.PK;
			var declarant = CreateOrgWithAccounts("Declarant", "DECL", OrgCusAccountCodeList.Codes.ImportDutiesOneMonth, createVatAccount: false);
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			var representative = CreateOrgWithAccounts("Representative", "REP", OrgCusAccountCodeList.Codes.ImportDutiesOneMonth, createVatAccount: false);
			declaration.JE_OA_Representative = representative.MainAddress.PK;

			AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.E, DeferralPaymentPartyList.Codes.Declarant, "DECL10");
		}

		public void TestOnSaving_DefaultReferralData_Declarant15()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var declarant = CreateOrgWithAccounts("Declarant", "DECL", OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT, createVatAccount: false);
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			var representative = CreateOrgWithAccounts("Representative", "REP", OrgCusAccountCodeList.Codes.ImportDutiesOneMonth, createVatAccount: false);
			declaration.JE_OA_Representative = representative.MainAddress.PK;

			AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.E, DeferralPaymentPartyList.Codes.Declarant, "DECL15");
		}

		public void TestOnSaving_DefaultReferralData_Declarant20()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var declarant = CreateOrgWithAccounts("Declarant", "DECL", string.Empty, createVatAccount: true);
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.E, string.Empty, string.Empty, DeferralPaymentPartyList.Codes.Declarant, "DECL20");
		}

		public void TestOnSaving_DefaultReferralData_Declarant10_20()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var declarant = CreateOrgWithAccounts("Declarant", "DECL", OrgCusAccountCodeList.Codes.ImportDutiesOneMonth, createVatAccount: true);
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			var representative = CreateOrgWithAccounts("Representative", "REP", OrgCusAccountCodeList.Codes.ImportDutiesOneMonth, createVatAccount: true);
			declaration.JE_OA_Representative = representative.MainAddress.PK;

			CombineAssertions(() =>
			{
				AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.A);
				AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.C);
				AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.D);
				AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.E, DeferralPaymentPartyList.Codes.Declarant, "DECL10", DeferralPaymentPartyList.Codes.Declarant, "DECL20");
				AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.F, DeferralPaymentPartyList.Codes.Declarant, "DECL10", DeferralPaymentPartyList.Codes.Declarant, "DECL20");
				AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.G, DeferralPaymentPartyList.Codes.Declarant, "DECL10", DeferralPaymentPartyList.Codes.Declarant, "DECL20");
				AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.Z, DeferralPaymentPartyList.Codes.Declarant, "DECL10", DeferralPaymentPartyList.Codes.Declarant, "DECL20");
			});
		}

		public void TestOnSaving_DefaultReferralData_Declarant15_20()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var defermentParty = CreateOrgWithAccounts("DefermentParty", "DEFER", string.Empty, createVatAccount: false);
			declaration.DefermentPartyDocAddress.OrganisationPK = defermentParty.PK;
			var declarant = CreateOrgWithAccounts("Declarant", "DECL", OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT, createVatAccount: true);
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			var representative = CreateOrgWithAccounts("Representative", "REP", OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT, createVatAccount: true);
			declaration.JE_OA_Representative = representative.MainAddress.PK;

			AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.E, DeferralPaymentPartyList.Codes.Declarant, "DECL15", DeferralPaymentPartyList.Codes.Declarant, "DECL20");
		}

		public void TestOnSaving_DefaultReferralData_Declarant15_DefermentParty20()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var defermentParty = CreateOrgWithAccounts("DefermentParty", "DEF", string.Empty, createVatAccount: true);
			declaration.DefermentPartyDocAddress.OrganisationPK = defermentParty.PK;
			var declarant = CreateOrgWithAccounts("Declarant", "DECL", OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT, createVatAccount: true);
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			var representative = CreateOrgWithAccounts("Representative", "REP", OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT, createVatAccount: true);
			declaration.JE_OA_Representative = representative.MainAddress.PK;

			AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.E, DeferralPaymentPartyList.Codes.Declarant, "DECL15", DeferralPaymentPartyList.Codes.DefermentParty, "DEF20");
		}

		public void TestOnSaving_DefaultReferralData_Declarant15_Representative20()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var declarant = CreateOrgWithAccounts("Declarant", "DECL", OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT, createVatAccount: false);
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			var representative = CreateOrgWithAccounts("Representative", "REP", OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT, createVatAccount: true);
			declaration.JE_OA_Representative = representative.MainAddress.PK;

			AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.E, DeferralPaymentPartyList.Codes.Declarant, "DECL15", DeferralPaymentPartyList.Codes.Representative, "REP20");
		}

		public void TestOnSaving_DefaultReferralData_RepresentativeIsNull()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.ZG_MethodOfPayment = UniversalReferenceConstants.MethodOfPaymentTypes.E;

			AssertNoExceptionThrown(() => declaration.OnSaving());
		}

		public void TestOnSaving_DefaultReferralData_Representative10()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var defermentParty = CreateOrgWithAccounts("DefermentParty", string.Empty, string.Empty, createVatAccount: false);
			declaration.DefermentPartyDocAddress.OrganisationPK = defermentParty.PK;
			var declarant = CreateOrgWithAccounts("Declarant", string.Empty, string.Empty, createVatAccount: false);
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			var representative = CreateOrgWithAccounts("Representative", "REP", OrgCusAccountCodeList.Codes.ImportDutiesOneMonth, createVatAccount: false);
			declaration.JE_OA_Representative = representative.MainAddress.PK;

			AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.E, DeferralPaymentPartyList.Codes.Representative, "REP10");
		}

		public void TestOnSaving_DefaultReferralData_Representative15()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var representative = CreateOrgWithAccounts("Representative", "REP", OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT, createVatAccount: false);
			declaration.JE_OA_Representative = representative.MainAddress.PK;

			AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.E, DeferralPaymentPartyList.Codes.Representative, "REP15");
		}

		public void TestOnSaving_DefaultReferralData_Representative20()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var representative = CreateOrgWithAccounts("Representative", "REP", string.Empty, createVatAccount: true);
			declaration.JE_OA_Representative = representative.MainAddress.PK;

			AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.E, string.Empty, string.Empty, DeferralPaymentPartyList.Codes.Representative, "REP20");
		}

		public void TestOnSaving_DefaultReferralData_Representative10_20()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var representative = CreateOrgWithAccounts("Representative", "REP", OrgCusAccountCodeList.Codes.ImportDutiesOneMonth, createVatAccount: true);
			declaration.JE_OA_Representative = representative.MainAddress.PK;

			CombineAssertions(() =>
			{
				AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.A);
				AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.C);
				AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.D);
				AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.E, DeferralPaymentPartyList.Codes.Representative, "REP10", DeferralPaymentPartyList.Codes.Representative, "REP20");
				AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.F, DeferralPaymentPartyList.Codes.Representative, "REP10", DeferralPaymentPartyList.Codes.Representative, "REP20");
				AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.G, DeferralPaymentPartyList.Codes.Representative, "REP10", DeferralPaymentPartyList.Codes.Representative, "REP20");
				AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.Z, DeferralPaymentPartyList.Codes.Representative, "REP10", DeferralPaymentPartyList.Codes.Representative, "REP20");
			});
		}

		public void TestOnSaving_DefaultReferralData_Representative15_20()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var defermentParty = CreateOrgWithAccounts("DefermentParty", "DEFER", string.Empty, createVatAccount: false);
			declaration.DefermentPartyDocAddress.OrganisationPK = defermentParty.PK;
			var declarant = CreateOrgWithAccounts("Declarant", "DECL", string.Empty, createVatAccount: false);
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			var representative = CreateOrgWithAccounts("Representative", "REP", OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT, createVatAccount: true);
			declaration.JE_OA_Representative = representative.MainAddress.PK;

			AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.E, DeferralPaymentPartyList.Codes.Representative, "REP15", DeferralPaymentPartyList.Codes.Representative, "REP20");
		}

		public void TestOnSaving_DefaultReferralData_Representative15_DefermentParty20()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var defermentParty = CreateOrgWithAccounts("DefermentParty", "DEF", string.Empty, createVatAccount: true);
			declaration.DefermentPartyDocAddress.OrganisationPK = defermentParty.PK;
			var declarant = CreateOrgWithAccounts("Declarant", "DECL", string.Empty, createVatAccount: false);
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			var representative = CreateOrgWithAccounts("Representative", "REP", OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT, createVatAccount: true);
			declaration.JE_OA_Representative = representative.MainAddress.PK;

			AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.E, DeferralPaymentPartyList.Codes.Representative, "REP15", DeferralPaymentPartyList.Codes.DefermentParty, "DEF20");
		}

		public void TestOnSaving_DefaultReferralData_Representative15_Declarant20()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var defermentParty = CreateOrgWithAccounts("DefermentParty", string.Empty, string.Empty, createVatAccount: false);
			declaration.DefermentPartyDocAddress.OrganisationPK = defermentParty.PK;
			var declarant = CreateOrgWithAccounts("Declarant", "DECL", string.Empty, createVatAccount: true);
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			var representative = CreateOrgWithAccounts("Representative", "REP", OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT, createVatAccount: true);
			declaration.JE_OA_Representative = representative.MainAddress.PK;

			AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.E, DeferralPaymentPartyList.Codes.Representative, "REP15", DeferralPaymentPartyList.Codes.Declarant, "DECL20");
		}

		public void TestOnSaving_DefaultReferralData_Representative10_Declarant20()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var defermentParty = CreateOrgWithAccounts("DefermentParty", string.Empty, string.Empty, createVatAccount: false);
			declaration.DefermentPartyDocAddress.OrganisationPK = defermentParty.PK;
			var declarant = CreateOrgWithAccounts("Declarant", "DECL", string.Empty, createVatAccount: true);
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			var representative = CreateOrgWithAccounts("Representative", "REP", OrgCusAccountCodeList.Codes.ImportDutiesOneMonth, createVatAccount: true);
			declaration.JE_OA_Representative = representative.MainAddress.PK;

			AssertDeferralFields(UniversalReferenceConstants.MethodOfPaymentTypes.E, DeferralPaymentPartyList.Codes.Representative, "REP10", DeferralPaymentPartyList.Codes.Declarant, "DECL20");
		}

		public void TestOnSaving_DefaultReferralData_ExceedNumberMaxLength()
		{
			var defermentParty = Factory.NewWithValidTestData<OrgHeader>();
			defermentParty.OH_FullName = "DefermentParty";
			var cusAccount = defermentParty.DefermentAccountNumberCollection.AddNew();
			cusAccount.CZ_Code = OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT;
			cusAccount.CZ_Account = ZString.Empty.PadLeft(declaration.JE_DefermentAccountNumberInfo.MaxLength + 1, 'A');
			var vatAccount = defermentParty.DefermentAccountNumberCollection.AddNew();
			vatAccount.CZ_Code = OrgCusAccountCodeList.Codes.ImportVATWithoutSecurity;
			vatAccount.CZ_Account = ZString.Empty.PadLeft(declaration.ZG_VATDeferNumberInfo.MaxLength + 1, 'B');

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.DefermentPartyDocAddress.OrganisationPK = defermentParty.PK;
			declaration.ZG_MethodOfPayment = UniversalReferenceConstants.MethodOfPaymentTypes.E;

			CombineAssertions(() =>
			{
				declaration.OnSaving();

				AssertEquals("JE_DefermentAccountNumber was truncated", ZString.Empty.PadLeft(declaration.JE_DefermentAccountNumberInfo.MaxLength, 'A'), declaration.JE_DefermentAccountNumber);
				AssertEquals("ZG_VATDeferNumber was truncated", ZString.Empty.PadLeft(declaration.ZG_VATDeferNumberInfo.MaxLength, 'B'), declaration.ZG_VATDeferNumber);
			});
		}

		public void TestSetRepresentationTypeIfMatchingEORICodes_IMP_DoesNotSetRepTypeToSEL_IfDeclarantAndImportersEORIsMatch()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.Import;
			var testOrg = Factory.GetOrgHeaderWithEori("OHTEST", "12345", Core.Constants.CountryCodes.Germany);
			declaration.ImporterDocumentaryAddress.E2_OA_Address = testOrg.MainAddress.PK;
			declaration.JE_OA_DeclarantAddress = testOrg.MainAddress.PK;
			AssertEquals("DeclarantType keeps default value DIR", RepresentationTypeList.Codes._2Direct, declaration.JE_DeclarantType);
		}

		public void TestJE_MessageType_ChangeToIMPTriggersUpdateAddressesDependingOnDeclarantType()
		{
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("DeclarantType 'SEL'", RepresentationTypeList.Codes._1Self, declaration.JE_DeclarantType);
				AssertEquals("MessageType 'EXP'", Common.DE.DEJobMessageTypeList.Codes.Export, declaration.JE_MessageType);
				AssertEquals("JE_OA_DeclarantAddress not OrgProxy", ZGuid.Empty, declaration.JE_OA_DeclarantAddress);

				declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.Import;
				AssertEquals("DeclarantType stays 'SEL'", RepresentationTypeList.Codes._1Self, declaration.JE_DeclarantType);
				AssertEquals("MessageType 'IMP'", Common.DE.DEJobMessageTypeList.Codes.Import, declaration.JE_MessageType);
				AssertEquals("JE_OA_DeclarantAddress is OrgProxy", GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, declaration.JE_OA_DeclarantAddress);
			});
		}

		public void TestJE_MessageType_ClearsEntryInstructionGoodsLocation()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var goodsLocation = entryInstruction.GoodsLocation;
			goodsLocation.CGL_Qualifier = Customs.Business.CusGoodsLocationQualifierList.Codes.Address;
			CombineAssertions(() =>
			{
				AssertEquals("Type", "D", goodsLocation.CGL_Type);

				declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.Import;
				AssertEquals("Qualifier is empty", ZString.Empty, goodsLocation.CGL_Qualifier);
				AssertEquals("Type is empty", ZString.Empty, goodsLocation.CGL_Type);
			});
		}

		public void TestImporterDocumentaryAddressChangedTriggersUpdateAddressesDependingOnDeclarantType()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.Import;
			var testOrg = Factory.New<OrgHeader>();
			var mainAddressPK = testOrg.MainAddress.PK;
			var secondAddressPK = testOrg.Addresses.AddNew().PK;

			CombineAssertions(() =>
			{
				AssertEquals("Default Representative is OrgProxy", GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, declaration.JE_OA_Representative);
				AssertEquals("Default DeclarantType", RepresentationTypeList.Codes._2Direct, declaration.JE_DeclarantType);

				declaration.ImporterDocumentaryAddress.E2_OA_Address = mainAddressPK;
				AssertEquals("ImporterDocumentaryAddress.E2_OA_Address = MainAddress", mainAddressPK, declaration.JE_OA_DeclarantAddress);

				declaration.ImporterDocumentaryAddress.E2_OA_Address = secondAddressPK;
				AssertEquals("ImporterDocumentaryAddress.E2_OA_Address = SecondAddress", secondAddressPK, declaration.JE_OA_DeclarantAddress);
			});
		}

		public void TestImporterDocumentaryAddressChanged_ShouldUpdateInvoiceHeadersConsigneeAddressesFromPreviousImporter_WhenExport()
		{
			// Arrange
			declaration.JE_MessageType = DEJobMessageTypeList.Codes.Export;

			var importerInitial = Factory.NewWithValidTestData<OrgHeader>();
			var importerNew = Factory.NewWithValidTestData<OrgHeader>();

			declaration.ImporterDocumentaryAddress.E2_OA_Address = importerInitial.MainAddress.PK;

			var invoiceHeaderWithInitialImporter = declaration.Invoices.AddNew();
			AssertEquals("Precondition", importerInitial.MainAddress, invoiceHeaderWithInitialImporter.ConsigneeAddress);

			var invoiceHeaderWithoutInitialImporter = declaration.Invoices.AddNew();
			invoiceHeaderWithoutInitialImporter.JZ_OA_ConsigneeAddress = Factory.NewWithValidTestData<OrgHeader>().PK;

			// Act
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importerNew.MainAddress.PK;

			// Assert
			AssertEquals("Consignee address is updated for invoice header that has consignee address from previous importer", importerNew.MainAddress, invoiceHeaderWithInitialImporter.ConsigneeAddress);
			AssertNotEquals("Consignee address is not updated for invoice header that has consignee address different from previous importer", importerNew.MainAddress, invoiceHeaderWithoutInitialImporter.ConsigneeAddress);
		}

		public void TestImporterDocumentaryAddressChanged_ShouldEmptyInvoiceHeadersConsigneeAddressesIfImporterIsEmpty_WhenExport()
		{
			// Arrange
			declaration.JE_MessageType = DEJobMessageTypeList.Codes.Export;

			var importerInitial = Factory.NewWithValidTestData<OrgHeader>();
			var importerNew = Factory.NewWithValidTestData<OrgHeader>();

			declaration.ImporterDocumentaryAddress.E2_OA_Address = importerInitial.MainAddress.PK;

			var invoiceHeaderWithInitialImporter = declaration.Invoices.AddNew();
			AssertEquals("Precondition", importerInitial.MainAddress, invoiceHeaderWithInitialImporter.ConsigneeAddress);

			var invoiceHeaderWithoutInitialImporter = declaration.Invoices.AddNew();
			invoiceHeaderWithoutInitialImporter.JZ_OA_ConsigneeAddress = Factory.NewWithValidTestData<OrgHeader>().PK;

			// Act
			declaration.ImporterDocumentaryAddress.E2_OA_Address = ZGuid.Empty;

			// Assert
			AssertNull("Consignee address bcomes mepty, if importe becomes empty", invoiceHeaderWithInitialImporter.ConsigneeAddress);
		}

		public void TestJobDeclarationValueSetStrategy()
		{
			CombineAssertions(() =>
			{
				var declarationValueSetStrategy = declaration.DeclarationValueSetStrategy;
				AssertType<JobDeclarationValueSetStrategy>("Type DE", declarationValueSetStrategy);
				AssertSame("Cached", declarationValueSetStrategy, declaration.DeclarationValueSetStrategy);
			});
		}

		public void TestShipmentSynchronizer()
		{
			declaration.JE_JS = Factory.New<ForwardingShipment>().PK;
			AssertType<JobDeclarationSynchroniser>(declaration.ShipmentSynchroniser);
		}

		public void TestEntryCreationStrategy()
		{
			AssertType<EntryCreationStrategy>(declaration.CreateEntryCreationStrategy());
		}

		public void TestMergeManager()
		{
			AssertType<MergeManager>(declaration.MergeManager);
		}

		public void TestEntryFeePaymentPartyUnderstander()
		{
			var header = declaration.CustomsEntryHeaders.AddNew();
			AssertType<EntryFeePaymentPartyUnderstander>(declaration.GetEntryFeePaymentPartyUnderstander(header));
		}

		public void TestIsDefermentAllowed_empty() => AssertIsDefermentAllowed(string.Empty, false);
		public void TestIsDefermentAllowed_A() => AssertIsDefermentAllowed("A", false);
		public void TestIsDefermentAllowed_C() => AssertIsDefermentAllowed("C", false);
		public void TestIsDefermentAllowed_D() => AssertIsDefermentAllowed("D", false);
		public void TestIsDefermentAllowed_E() => AssertIsDefermentAllowed("E", true);
		public void TestIsDefermentAllowed_F() => AssertIsDefermentAllowed("F", true);
		public void TestIsDefermentAllowed_G() => AssertIsDefermentAllowed("G", true);
		public void TestIsDefermentAllowed_Y() => AssertIsDefermentAllowed("Y", false);
		public void TestIsDefermentAllowed_Z() => AssertIsDefermentAllowed("Z", true);
		void AssertIsDefermentAllowed(string moP, bool expected)
		{
			declaration.ZG_MethodOfPayment = moP;
			AssertEquals($"MoP {moP} should give {expected}", expected, declaration.IsDefermentAllowed);
		}

		public void TestIsDefermentAllowedStateChange() => CombineAssertions(() =>
		{
			declaration.ZG_MethodOfPayment = Business.UniversalReferenceConstants.MethodOfPaymentTypes.A;
			AssertEquals(false, declaration.IsDefermentAllowed);
			declaration.ZG_MethodOfPayment = Business.UniversalReferenceConstants.MethodOfPaymentTypes.E;
			AssertEquals(true, declaration.IsDefermentAllowed);
			declaration.ZG_MethodOfPayment = string.Empty;
			AssertEquals(false, declaration.IsDefermentAllowed);
		});

		public void TestDefermentPartyDocAddressRequirement_ValidateOrganisationPK_InwardProcessingAVABR()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			instruction.CEI_JE = declaration.PK;
			AssertNoNotifications(declaration.DefermentPartyDocAddress.OrganisationPKInfo);
		}

		public void TestEUD_AgreedPlaceCodeValidationSupport()
		{
			AssertEquals(false, declaration.EUD_AgreedPlaceCodeValidationSupport);
		}

		public void TestZG_AgreedPlaceCodeValidationSupport()
		{
			AssertEquals(false, declaration.ZG_AgreedPlaceCodeValidationSupport);
		}

		public void TestDoMerge_CreatePreviousProceduresFromInventoryForEntryInstruction_ApplicableProviderCalled()
		{
			var declarationMock = Factory.NewMoq<JobDeclaration>();
			List<Mock<PreviousProceduresFromInventoryForEntryInstructionProvider>> providers = new ();
			declarationMock
				.Setup(e => e.GetPreviousProceduresProvidersForInstruction(It.IsAny<CusEntryInstruction>()))
				.Returns<CusEntryInstruction>(i =>
				{
					providers.Add(SetUpMock(i, false, false));
					providers.Add(SetUpMock(i, true, true));
					providers.Add(SetUpMock(i, true, false));
					return providers.Select(e => e.Object);
				});
			declaration = declarationMock.Object;
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			declaration.Invoices.AddNew().InvoiceLines.AddNew().JI_CEI = instruction.PK;
			declaration.DoMerge();

			Mock<PreviousProceduresFromInventoryForEntryInstructionProvider> SetUpMock(CusEntryInstruction instruction, bool isApplicable, bool shouldBeCalled)
			{
				var mock = new Mock<PreviousProceduresFromInventoryForEntryInstructionProvider>(instruction);
				mock.Protected().SetupGet<bool>("IsApplicableCore").Returns(isApplicable);
				mock.Protected().Setup("CreatePreviousProceduresCore")
					.Callback(() => Assert("Right CreatePreviousProceduresCore was called", shouldBeCalled))
					.Verifiable(shouldBeCalled ? Times.Once() : Times.Never())
					;
				return mock;
			}

			foreach (var provider in providers)
			{
				provider.Verify();
			}
		}

		public void TestDoMerge_CreatePreviousProceduresFromInventoryForEntryInstruction_Import_MultipleEntries()
		{
			CreateOutOfWarehouseRefCusProcedure(Common.EU.EUJobMessageTypeList.Codes.Import);
			CreateOutOfInwardProcessingRefCusProcedure(Common.EU.EUJobMessageTypeList.Codes.Import);

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var fromWarehouseOrg = Factory.NewWithValidTestData<OrgHeader>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_OA_Warehouse = fromWarehouseOrg.MainAddress.PK;
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.CEI_OA_Warehouse = fromWarehouseOrg.MainAddress.PK;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			entryHeader.CH_CEI_Instruction = instruction.PK;

			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			entryHeader2.CH_CEI_Instruction = instruction2.PK;

			var invoiceHeader = declaration.Invoices.AddNew();
			CreateBondedWareHouseInvoiceLine(invoiceHeader, instruction, "4071", "ATH7100001", 1, "33049900000", 1.23M, "KGM");
			CreateBondedWareHouseInvoiceLine(invoiceHeader, instruction, "4071", "ATH7100001", 2, "33049900000", 2.34M, "KGM");

			var invoiceHeader2 = declaration.Invoices.AddNew();
			CreateBondedWareHouseInvoiceLine(invoiceHeader2, instruction2, "4051", "ATH7100002", 2, "33049900000", 3.45M, "KGM");

			CreateBondedWareHouseInvoiceLine(invoiceHeader2, instruction2, "4051", "ATH7100002", 2, "33049900000", 3.45M, "KGM");

			CreateBondedWareHouseInvoiceLine(invoiceHeader2, instruction2, "4051", "ATH7100002", 2, "33049900000", 3.45M, "KGM");

			declaration.DoMerge();

			CombineAssertions(() =>
			{
				AssertEquals("instruction", 2, instruction.PreviousDocuments.Count);
				AssertEquals("instruction2", 3, instruction2.PreviousDocuments.Count);
			});
		}

		public void TestDoMerge_CreatePreviousProceduresFromInventoryForEntryInstruction_Import_MergeFailed()
		{
			CreateOutOfWarehouseRefCusProcedure(Common.EU.EUJobMessageTypeList.Codes.Import);

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var fromWarehouseOrg = Factory.NewWithValidTestData<OrgHeader>();
			instruction.CEI_OA_Warehouse = fromWarehouseOrg.MainAddress.PK;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			entryHeader.CH_CEI_Instruction = instruction.PK;

			var invoiceHeader = declaration.Invoices.AddNew();
			CreateBondedWareHouseInvoiceLine(invoiceHeader, instruction, "4071", "ATH7100001", 1, "33049900000", 1.23M, "KGM");
			declaration.Invoices.AddNew();

			try
			{
				declaration.DoMerge();
			}
			catch (Exception)
			{
			}
			AssertEquals(0, instruction.PreviousDocuments.Count);
		}

		public void TestDoMerge_CreatePreviousProceduresFromInventoryForEntryInstruction_Export()
		{
			CreateOutOfWarehouseRefCusProcedure(Common.EU.EUJobMessageTypeList.Codes.Export);

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			var entryHeader = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			CreateBondedWareHouseInvoiceLine(invoiceHeader, instruction, "4071", "ATH7100001", 1, "33049900000", 1.23M, "KGM");

			declaration.DoMerge();
			AssertEquals(0, instruction.PreviousDocuments.Count);
		}

		public void TestDoMerge_CreatePreviousProcedures_DoesNotThrowWhenNoEntryInstruction()
		{
			CreateOutOfWarehouseRefCusProcedure(Common.EU.EUJobMessageTypeList.Codes.Import);

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var instruction = declaration.CustomsEntryInstructions.AddNew();

			declaration.ActiveEntryHeaders.AddNew();

			var invoiceHeader = declaration.Invoices.AddNew();
			CreateBondedWareHouseInvoiceLine(invoiceHeader, instruction, "4071", "ATH7100001", 1, "33049900000", 1.23M, "KGM");
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("DoMerge should not throw an exception if the EntryHeader has no linked Instruction", () => declaration.DoMerge());
				AssertEquals("No Previous Procedures should be created", 0, instruction.PreviousDocuments.Count);
			});
		}

		[GuiTest]
		public void TestDoMergeAndSaveCreatesWarehouseOrder_Export()
		{
			CreateOutOfWarehouseRefCusProcedure(Common.EU.EUJobMessageTypeList.Codes.Export);

			var testHelper = new WhsDataTestHelper(Factory);
			testHelper.WhsWarehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;

			var whsReceive = testHelper.GetNewWhsReceive(testHelper.WhsWarehouse.PK, testHelper.Importer.PK, "3-DECL1234");
			whsReceive.WD_CustomsParentReference = "3-DECL1234-EDIDATEDI";
			var receiveLine = testHelper.GetNewWhsReceiveLine(whsReceive.PK, testHelper.Part.PK, "12345", 11, 50, 50, bondedEntryKey: "ATH7100001");
			testHelper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KGM", ZString.Empty, 100m, "KGM", ZString.Empty, "ATH7100001", 1);

			var declaration = testHelper.GetNewDeclaration("EXP", "DECL1234", "23DE1234567890", 5.0m);
			declaration.SupplierDocumentaryAddress.E2_OA_Address = testHelper.Importer.MainAddress.PK;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_OA_Warehouse2 = testHelper.WhsWarehouse.WW_OA_WarehouseAddress;

			var invoiceHeader = declaration.Invoices[0];
			CreateBondedWareHouseInvoiceLine(invoiceHeader, instruction, "4071", "ATH7100001", 1, ZString.Empty, 1m, "KGM", invoiceHeader.InvoiceLines[0]);

			declaration.DoMerge();
			Factory.Save();

			var universalShipmentMessage = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, "UDM"));
			AssertEquals("shipment has been exported", 1, universalShipmentMessage.Length);
		}

		[GuiTest]
		public void TestDoMergeAndSaveCreatesWarehouseOrder_WarehouseAdjustment()
		{
			CreateWarehouseAdjustmentRefCusProcedure();

			var testHelper = new WhsDataTestHelper(Factory);
			testHelper.WhsWarehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;

			var whsReceive = testHelper.GetNewWhsReceive(testHelper.WhsWarehouse.PK, testHelper.Importer.PK, "3-DECL1234");
			whsReceive.WD_CustomsParentReference = "3-DECL1234-EDIDATEDI";
			var receiveLine = testHelper.GetNewWhsReceiveLine(whsReceive.PK, testHelper.Part.PK, "12345", 11, 50, 50, bondedEntryKey: "ATH7100001");
			testHelper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KGM", ZString.Empty, 100m, "KGM", ZString.Empty, "ATH7100001", 1);

			var declaration = testHelper.GetNewDeclaration("WAD", "DECL1234", "23DE1234567890", 5.0m);
			declaration.SupplierDocumentaryAddress.E2_OA_Address = testHelper.Importer.MainAddress.PK;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_OA_Warehouse = testHelper.WhsWarehouse.WW_OA_WarehouseAddress;

			var invoiceHeader = declaration.Invoices[0];
			CreateWarehouseAdjustmentInvoiceLine(invoiceHeader, instruction, "ATH7100001", 1, 1m, "KGM", invoiceHeader.InvoiceLines[0]);

			declaration.DoMerge();
			Factory.Save();

			var universalShipmentMessage = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, "UDM"));
			AssertEquals("shipment has been exported", 1, universalShipmentMessage.Length);
		}

		[GuiTest]
		public void TestDoMergeAndSaveCreatesWarehouseOrder_Export_OutOfInwardProcessing()
		{
			CreateOutOfInwardProcessingRefCusProcedure(Common.EU.EUJobMessageTypeList.Codes.Export);

			var testHelper = new WhsDataTestHelper(Factory);
			testHelper.WhsWarehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;

			var whsReceive = testHelper.GetNewWhsReceive(testHelper.WhsWarehouse.PK, testHelper.Importer.PK, "3-DECL1234");
			whsReceive.WD_CustomsParentReference = "3-DECL1234-EDIDATEDI";
			var receiveLine = testHelper.GetNewWhsReceiveLine(whsReceive.PK, testHelper.Part.PK, "12345", 11, 50, 50, bondedEntryKey: "ATH7100001");
			testHelper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KGM", ZString.Empty, 100m, "KGM", ZString.Empty, "ATH7100001", 1);

			var declaration = testHelper.GetNewDeclaration("EXP", "DECL1234", "23DE1234567890", 5.0m);
			declaration.SupplierDocumentaryAddress.E2_OA_Address = testHelper.Importer.MainAddress.PK;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_OA_Warehouse2 = testHelper.WhsWarehouse.WW_OA_WarehouseAddress;

			var invoiceHeader = declaration.Invoices[0];
			CreateBondedWareHouseInvoiceLine(invoiceHeader, instruction, "4051", "ATH7100001", 1, ZString.Empty, 1m, "KGM", invoiceHeader.InvoiceLines[0]);

			declaration.DoMerge();
			Factory.Save();

			var universalShipmentMessage = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, "UDM"));
			AssertEquals("shipment has been exported", 1, universalShipmentMessage.Length);
		}

		public void TestPreviousEntryNumberIsUpdated()
		{
			var helper = WhsDataTestHelper.New(Factory);
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = ATLASVersionNumberList.Codes._101 } };

			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var into = helper.GetNewDeclarationWithInstruction(Factory, "IMP", "B001", "ENT001", 1000, true);

				into.CustomsEntryHeaders[0].PublishShipmentForWHSInward(false);
				into.CustomsEntryHeaders[0].PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);

				Factory.Save();

				var outOf = helper.GetNewDeclarationWithInstruction(Factory, "IMP", "B002", "ENT002", 20, false, "<PendingCustomsResponse>");

				var invoiceLine = outOf.InvoiceLines[0];

				outOf.CustomsEntryHeaders[0].MergedLines.DeleteAll();
				outOf.CustomsEntryHeaders.DeleteAll();
				invoiceLine.JI_CEI = outOf.CustomsEntryInstructions[0].PK;
				invoiceLine.JI_BondedWhsQuantity = 20m;
				invoiceLine.JI_InvoiceQuantity = 20m;
				invoiceLine.JI_BondedWhsUnitQty = "KG";
				invoiceLine.JI_InvoiceUQ = "KG";
				invoiceLine.JI_BondedWHSOrderLineNumber = 1;
				invoiceLine.JI_PreviousEntryLineNumber = 0;

				Factory.Save();

				BondedWarehousingHelper.PublishShipmentForWHSOutward(outOf);

				outOf.PublishCancelEventForWHSOutwardAndSaveIfNeeded(true);

				BondedWarehousingHelper.PublishShipmentForWHSOutward(outOf);

				outOf.DoMerge();

				CombineAssertions(() =>
				{
					AssertEquals("<PendingCustomsResponse>", invoiceLine.JI_PreviousEntryNumber);
					AssertEquals((short)0, invoiceLine.JI_PreviousEntryLineNumber);

					UpdateBWHAttributeEntryDetails("B002", 1, "ENTRY-123", 2);

					outOf.DoMerge();

					AssertEquals("ENTRY-123", invoiceLine.JI_PreviousEntryNumber);
					AssertEquals((short)2, invoiceLine.JI_PreviousEntryLineNumber);
				});
			}
		}

		[GuiTest]
		public void TestLinkCreatedExportBWH()
		{
			Business.Testing.WarehouseIntegration.BondedWarehousingHelperTest.CreateCustomsStatus(Factory, "TX6", true, iCancel: false);
			const string prevReferenceNumber = "ATE150000620520195873";

			var testHelper = new WhsDataTestHelper(Factory);
			var inwardDecl = testHelper.GetNewDeclarationWithInstruction(Factory, "IMP", "B0001", prevReferenceNumber, 20, true);
			var inwardEntryHeader = (CusEntryHeader)inwardDecl.ActiveEntryHeaders.Single();
			inwardDecl.InvoiceLines[0].CusEntryLine.ZG_CustomsStatus = "TX6";

			JobDeclarationWarehouseExtensions.PublishShipmentForWHSInward(inwardEntryHeader, false);
			JobDeclarationWarehouseExtensions.PublishAcceptEventForWHSInwardAndSaveIfNeeded(inwardEntryHeader);

			Factory.Save();

			WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardEntryHeader.EntryNumber, 1, 20);

			var outOf = testHelper.GetNewDeclarationWithInstruction(Factory, "EXP", "B002", "ENT002", 1, false, "ATE150000620520195873");

			var invoiceLine = outOf.InvoiceLines[0];

			invoiceLine.JI_CEI = outOf.CustomsEntryInstructions[0].PK;
			invoiceLine.CusEntryLine.ZG_CustomsStatus = "TX6";

			Factory.Save();

			var result = outOf.CustomsEntryHeaders[0].PublishShipmentForWHSOutward(true);
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, result.ErrorMessage);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardEntryHeader.EntryNumber, 1, 19);
				var link = Factory.Load<IWhsDocketJobPivot>(new ZQuery(WhsDocketJobPivotSchema.WV_ParentId, outOf.CustomsEntryHeaders[0].PK));
				AssertEquals("docket link for this declaration created", 1, link.Length);
			});
		}

		public override void TestSupportsCalculateInsurance()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("Insurance Calculation on invoice header enabled for import", true, declaration.SupportsCalculateInsurance);
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("Insurance Calculation on invoice header disabled for export", false, declaration.SupportsCalculateInsurance);
			});
		}

		#region JE_ShipmentIncoTerm

		public void TestJE_ShipmentIncoTerm_WhenDeclarationIsImportOrExport_AndValueChanged_ShouldUpdateZG_AgreedPlaceCode()
		{
			AssertGreaterThan("date", ZDateTime.Now, Core.Constants.IncoTerms.Incoterms2020EffectiveDate);

			CombineAssertions("Field values should be empty by default, nothing to set.", () =>
			{
				AssertEquals(ZString.Empty, declaration.JE_ShipmentIncoTerm);
				AssertEquals(ZString.Empty, declaration.ZG_AgreedPlaceCode);
			});

			AssertJE_ShipmentIncoTermChangedForMessageType(DEJobMessageTypeList.Codes.Export);
			AssertJE_ShipmentIncoTermChangedForMessageType(DEJobMessageTypeList.Codes.Import);

			void AssertJE_ShipmentIncoTermChangedForMessageType(string messageType)
			{
				declaration.JE_MessageType = messageType;
				var checkedCodes = new List<string>(15);

				CombineAssertions("Message Type: " + messageType, () =>
				{
					AssertJE_ShipmentIncoTermChangedAndTrackCheckedCodes(Core.Constants.IncoTerms.CostAndFreight, UniversalReferenceConstants.AgreedPlaceCodes._3);
					AssertJE_ShipmentIncoTermChangedAndTrackCheckedCodes(Core.Constants.IncoTerms.CostInsuranceAndFreight, UniversalReferenceConstants.AgreedPlaceCodes._3);
					AssertJE_ShipmentIncoTermChangedAndTrackCheckedCodes(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, UniversalReferenceConstants.AgreedPlaceCodes._3);
					AssertJE_ShipmentIncoTermChangedAndTrackCheckedCodes(Core.Constants.IncoTerms.CarriagePaidTo, UniversalReferenceConstants.AgreedPlaceCodes._3);
					AssertJE_ShipmentIncoTermChangedAndTrackCheckedCodes(Core.Constants.IncoTerms.DeliveredAtPlace, UniversalReferenceConstants.AgreedPlaceCodes._3);
					AssertJE_ShipmentIncoTermChangedAndTrackCheckedCodes(Core.Constants.IncoTerms.DeliveredAtTerminal, UniversalReferenceConstants.AgreedPlaceCodes._3);
					AssertJE_ShipmentIncoTermChangedAndTrackCheckedCodes(Core.Constants.IncoTerms.DeliveredDutyPaid, UniversalReferenceConstants.AgreedPlaceCodes._3);
					AssertJE_ShipmentIncoTermChangedAndTrackCheckedCodes(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, UniversalReferenceConstants.AgreedPlaceCodes._3);
					AssertJE_ShipmentIncoTermChangedAndTrackCheckedCodes(Core.Constants.IncoTerms.ExWorks, UniversalReferenceConstants.AgreedPlaceCodes._1);
					AssertJE_ShipmentIncoTermChangedAndTrackCheckedCodes(Core.Constants.IncoTerms.FreeAlongsideShip, UniversalReferenceConstants.AgreedPlaceCodes._1);
					AssertJE_ShipmentIncoTermChangedAndTrackCheckedCodes(Core.Constants.IncoTerms.FreeCarrier, UniversalReferenceConstants.AgreedPlaceCodes._1);
					AssertJE_ShipmentIncoTermChangedAndTrackCheckedCodes(Core.Constants.IncoTerms.FreeOnBoard, UniversalReferenceConstants.AgreedPlaceCodes._1);
					AssertJE_ShipmentIncoTermChangedAndTrackCheckedCodes(Core.Constants.IncoTerms.Other, ZString.Empty);
					AssertJE_ShipmentIncoTermChangedAndTrackCheckedCodes(Core.Constants.IncoTerms.FreeCarrierSeller, ZString.Empty);
					AssertJE_ShipmentIncoTermChangedAndTrackCheckedCodes(Core.Constants.IncoTerms.FreeCarrierBuyer, ZString.Empty);
				});

				void AssertJE_ShipmentIncoTermChangedAndTrackCheckedCodes(string incoterm, ZString expectedPlaceCode)
				{
					checkedCodes.Add(incoterm);
					AssertJE_ShipmentIncoTermChanged(incoterm, expectedPlaceCode);
				}

				var allCodes = declaration.Lookups.IncoTermList.GetAllCodes();
				var codesNotChecked = allCodes.Except(checkedCodes);

				AssertContainsExactElementsInAnyOrder("All current codes should be checked. If you've added a new code, please add it to this test and decide how it should behave with this functionality.",
					Array.Empty<string>(), codesNotChecked);
			}
		}

		public void TestJE_ShipmentIncoTerm_WhenDeclarationNotImportOrExport_AndValueChanged_ShouldNotUpdateZG_AgreedPlaceCode()
		{
			CombineAssertions("Field values should be empty by default, nothing to set.", () =>
			{
				AssertEquals(ZString.Empty, declaration.JE_ShipmentIncoTerm);
				AssertEquals(ZString.Empty, declaration.ZG_AgreedPlaceCode);
			});

			var messageTypes = new DEJobMessageTypeList().GetAllCodes().Where(x => x != DEJobMessageTypeList.Codes.Export && x != DEJobMessageTypeList.Codes.Import);

			foreach (var messageType in messageTypes)
			{
				declaration.JE_MessageType = messageType;

				CombineAssertions("For everything other than import and export we should not set this field. Message Type: " + messageType, () =>
				{
					foreach (var incoterm in declaration.Lookups.IncoTermList.GetAllCodes())
					{
						AssertJE_ShipmentIncoTermChanged(incoterm, ZString.Empty);
					}
				});
			}
		}

		void AssertJE_ShipmentIncoTermChanged(string incoterm, ZString expectedPlaceCode)
		{
			declaration.JE_ShipmentIncoTerm = incoterm;
			AssertEquals("Entered incoterm: " + incoterm, expectedPlaceCode, declaration.ZG_AgreedPlaceCode);
		}

		#endregion

		public void TestJE_VATClaimBack()
		{
			declaration.JE_MessageType = DEJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;

			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "OH1";
			var declarant_orgImpAddInfo = (DEOrgImpAddInfo)declarant.CountryData.ImpAddInfo;
			declarant_orgImpAddInfo.ZO_VATClaimBack = YesNoList.Codes.Yes;
			Factory.Save();

			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Defaulted VAT Claim Back", "Y", declaration.JE_VATClaimBack);

				var declarant2 = Factory.New<OrgHeader>();
				declarant2.OH_Code = "OH2";
				var declarant_orgImpAddInfo2 = (DEOrgImpAddInfo)declarant2.CountryData.ImpAddInfo;
				declarant_orgImpAddInfo2.ZO_VATClaimBack = YesNoList.Codes.No;
				Factory.Save();

				declaration.JE_OA_DeclarantAddress = declarant2.MainAddress.PK;
				AssertEquals("Defaulted VAT Claim Back, if Declarant changed", "N", declaration.JE_VATClaimBack);

				declaration.JE_VATClaimBack = "Y";
				AssertEquals("Can override value", "Y", declaration.JE_VATClaimBack);
			});
		}

		public void TestInventorySelectionHeader_Import()
		{
			declaration.JE_MessageType = DEJobMessageTypeList.Codes.Import;
			AssertType<ImportInventorySelectionHeader>(declaration.InventorySelectionHeader);
		}

		public void TestInventorySelectionHeader_Export()
		{
			declaration.JE_MessageType = DEJobMessageTypeList.Codes.Export;
			AssertType<ExportInventorySelectionHeader>(declaration.InventorySelectionHeader);
		}

		public void TestInventorySelectionHeader_WarehouseAdjustment()
		{
			declaration.JE_MessageType = DEJobMessageTypeList.Codes.WarehouseAdjustment;
			AssertType<WarehouseAdjustmentInventorySelectionHeader>(declaration.InventorySelectionHeader);
		}

		public void TestExitControlNotVisible_WarehouseAdjustment()
		{
			declaration.JE_MessageType = DEJobMessageTypeList.Codes.WarehouseAdjustment;
			AssertEquals("Exit Control tab disabled for adjustment", false, declaration.ExitControlTabVisible);
		}

		public void TestWarehouseClient_WarehouseAdjustment() => CombineAssertions(() =>
		{
			var supplier = Factory.New<OrgHeader>();
			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;
			var importer = Factory.New<OrgHeader>();
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;

			declaration.JE_MessageType = DEJobMessageTypeList.Codes.Import;
			AssertSame("code from base still works", declaration.Importer, declaration.WarehouseClient);

			declaration.JE_MessageType = DEJobMessageTypeList.Codes.WarehouseAdjustment;
			AssertSame(declaration.Supplier, declaration.WarehouseClient);
		});

		public void TestIsInventorySelectionEnabled() => CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var warehouseAddress = Factory.NewWithValidTestData<OrgAddress>();
			entryInstruction.CEI_OA_Warehouse = warehouseAddress.PK;

			using (CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				Factory.InvalidateCachedProperties();
				entryInstruction.Warehouse.Header.CompanyData.OB_IMUsedBondedWhs = true;
				declaration.JE_MessageType = "IMP";
				AssertEquals("InventorySelectionEnabledCore disabled for IMP declaration", false, declaration.IsInventorySelectionEnabled);

				Factory.InvalidateCachedProperties();
				declaration.JE_MessageType = "EXP";
				AssertEquals("InventorySelectionEnabledCore enabled for EXP declaration", true, declaration.IsInventorySelectionEnabled);
			}
		});

		public void TestBondedWarehousingHelper()
		{
			AssertType<BondedWarehousingHelper>(declaration.BondedWarehousingHelper);
		}

		public override void TestIsAutoUpdateBondedWarehouseEnabled()
		{
			declaration.JE_MessageType = DEJobMessageTypeList.Codes.Import;
			AssertEquals(false, declaration.IsAutoUpdateBondedWarehouseEnabled);

			declaration.JE_MessageType = DEJobMessageTypeList.Codes.WarehouseAdjustment;
			AssertEquals(false, declaration.IsAutoUpdateBondedWarehouseEnabled);

			declaration.JE_MessageType = DEJobMessageTypeList.Codes.Export;
			AssertEquals(true, declaration.IsAutoUpdateBondedWarehouseEnabled);
		}

		public void TestIsOutwardOrderImported()
		{
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				AssertEquals(false, declaration.IsOutwardOrderImported);

				declaration.IsOutwardOrderImported = true;

				AssertEquals(true, declaration.IsOutwardOrderImported);
			});
		}

		public void TestIsOutwardOrderImported_GenAddOnColumn()
		{
			var query = new ZQuery(GenAddOnColumnSchema.XA_Name, nameof(JobDeclaration.IsOutwardOrderImported));
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, JobDeclarationSchema.Constants.Prefix);
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentID, declaration.PK);

			CombineAssertions(() =>
			{
				AssertEquals("No GenAddOn", false, Factory.Exists(typeof(GenAddOnColumn), query));
				declaration.IsOutwardOrderImported = true;
				AssertEquals("Column Exists True", true, Factory.Exists(typeof(GenAddOnColumn), query));
			});
		}

		public void TestPreviousEntryIsATLASSet() => AssertContainsExactElementsInAnyOrder(new[] { "ATC51", "ATD51", "ATE51", "ATP51", "ATC71", "ATD71", "ATE71", "ATH71", "ATT71" }, JobDeclaration.PreviousEntryIsATLASSet);

		public void TestIsIntegrationWithAccountingSupported()
		{
			var option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = false;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, option);

			var declaration = Factory.New<JobDeclarationForTest>();
			AssertEquals("IsIntegrationWithAccountingSupported is false when EnableAccountingIntegration is false", false, declaration.IsIntegrationWithAccountingSupportedExposed);

			option.EnableAccountingIntegration = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, option);
			AssertEquals("IsIntegrationWithAccountingSupported is true when EnableAccountingIntegration is true", true, declaration.IsIntegrationWithAccountingSupportedExposed);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<JobDeclaration>();

		protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

		protected override BusinessObject GetNewBusinessObject() => Factory.New<JobDeclaration>();

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest() => GetNewBusinessObject();

		protected override Hashtable ExpectedDocAddressTypes
		{
			get
			{
				var result = base.ExpectedDocAddressTypes;
				result[DocAddressTypes.Codes.CustomsSupervisingOffice] = DocAddressType.CustomsSupervisingOffice;
				result[DocAddressTypes.Codes.GovernmentContractor] = DocAddressType.GovernmentContractor;
				result[DocAddressTypes.Codes.CustomsPlaceOfLoading] = DocAddressType.CustomsPlaceOfLoading;
				return result;
			}
		}

		protected override Type ExpectedDeclarationLevelPackageCollectionType => typeof(BaseDeclarationLevelPackageCollection<Package>);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;

		void AssertSimplifiedCusReconEntriesAddressUpdated(Action<JobDeclaration, ZGuid> setDeclarationAddress, Func<CusReconEntry, ZGuid> getReconEntryAddress)
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				var orgAddress = orgHeader.MainAddress;
				AssertNoExceptionThrown("Declaration doesn't have CusReconEntries", () => setDeclarationAddress(declaration, orgAddress.PK));

				var (reconEntry1, reconEntry2) = CreateCusReconEntries();
				setDeclarationAddress(declaration, ZGuid.Empty);
				setDeclarationAddress(declaration, orgAddress.PK);
				AssertEquals("Simplified CEI_Style, entry1.address updated", orgAddress.PK, getReconEntryAddress(reconEntry1));
				AssertEquals("Simplified CEI_Style, entry2.address updated", orgAddress.PK, getReconEntryAddress(reconEntry2));
			});

			(CusReconEntry, CusReconEntry) CreateCusReconEntries()
			{
				var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
				var reconEntry1 = Factory.New<CusReconEntry>();
				reconEntry1.CRE_CH_OriginalEntry = entryHeader1.PK;

				var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
				var reconEntry2 = Factory.New<CusReconEntry>();
				reconEntry2.CRE_CH_OriginalEntry = entryHeader2.PK;

				return (reconEntry1, reconEntry2);
			}
		}

		OrgHeader CreateOrgWithAccounts(string name, string accountPrefix, string accountType, bool createVatAccount)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = name;
			if (!string.IsNullOrEmpty(accountPrefix))
			{
				if (!string.IsNullOrEmpty(accountType))
				{
					var cusAccount = orgHeader.DefermentAccountNumberCollection.AddNew();
					cusAccount.CZ_Code = accountType;
					cusAccount.CZ_Account = accountPrefix + accountType;
				}
				if (createVatAccount)
				{
					var vatAccount = orgHeader.DefermentAccountNumberCollection.AddNew();
					vatAccount.CZ_Code = OrgCusAccountCodeList.Codes.ImportVATWithoutSecurity;
					vatAccount.CZ_Account = accountPrefix + OrgCusAccountCodeList.Codes.ImportVATWithoutSecurity;
				}
			}
			return orgHeader;
		}

		void AssertDeferralFields(ZString methodOfPayment, string expectedPaymentMethod = "", string expectedDeferralAccount = "", string expectedVATDeferralType = "", string expectedVATAccount = "")
		{
			declaration.ZG_MethodOfPayment = methodOfPayment;
			declaration.JE_PaymentMethod = ZString.Empty;
			declaration.JE_DefermentAccountNumber = ZString.Empty;
			declaration.ZG_VATDeferType = ZString.Empty;
			declaration.ZG_VATDeferNumber = ZString.Empty;
			declaration.OnSaving();
			AssertEquals($"Method of payment: {methodOfPayment}, JE_PaymentMethod", expectedPaymentMethod, declaration.JE_PaymentMethod);
			AssertEquals($"Method of payment: {methodOfPayment}, JE_DefermentAccountNumber", expectedDeferralAccount, declaration.JE_DefermentAccountNumber);
			AssertEquals($"Method of payment: {methodOfPayment}, ZG_VATDeferType", expectedVATDeferralType, declaration.ZG_VATDeferType);
			AssertEquals($"Method of payment: {methodOfPayment}, ZG_VATDeferNumber", expectedVATAccount, declaration.ZG_VATDeferNumber);
		}

		void AssertSetDefaultValuesForRepresentative(OrgHeader orgHeaderOfOrgProxy)
		{
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_GC = currentCompany.PK;
			currentBranch.GB_OH_OrgProxy = orgHeaderOfOrgProxy.PK;
			currentBranch.GB_IsActive = true;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, currentBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				CombineAssertions(() =>
				{
					var declaration_Export = Factory.New<JobDeclaration>();
					AssertEquals("Representative is OrgProxy", orgHeaderOfOrgProxy.MainAddress.PK, declaration_Export.JE_OA_Representative);
					AssertEquals("Declarant is not OrgProxy", ZGuid.Empty, declaration_Export.JE_OA_DeclarantAddress);
				});
			}
		}

		JobComInvoiceLine CreateBondedWareHouseInvoiceLine(JobComInvoiceHeader invoiceHeader, CusEntryInstruction instruction, ZString procedure,
			ZString previousEntryNumber, ZShort previousEntryLineNumber, ZString tariff, ZDecimal bondedWhsQuantity, ZString bondedWhsUnitQty, JobComInvoiceLine invoiceLine = null)
		{
			if (invoiceLine == null)
			{
				invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			}
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = procedure;
			invoiceLine.JI_PreviousEntryNumber = previousEntryNumber;
			invoiceLine.JI_PreviousEntryLineNumber = previousEntryLineNumber;
			invoiceLine.JI_Tariff = tariff;
			invoiceLine.JI_BondedWhsQuantity = bondedWhsQuantity;
			invoiceLine.JI_BondedWhsUnitQty = bondedWhsUnitQty;
			return invoiceLine;
		}

		JobComInvoiceLine CreateWarehouseAdjustmentInvoiceLine(JobComInvoiceHeader invoiceHeader, CusEntryInstruction instruction,
			ZString previousEntryNumber, ZShort previousEntryLineNumber, ZDecimal bondedWhsQuantity, ZString bondedWhsUnitQty, JobComInvoiceLine invoiceLine = null)
		{
			if (invoiceLine == null)
			{
				invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			}
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = "01";
			invoiceLine.JI_PreviousEntryNumber = previousEntryNumber;
			invoiceLine.JI_PreviousEntryLineNumber = previousEntryLineNumber;
			invoiceLine.JI_BondedWhsQuantity = bondedWhsQuantity;
			invoiceLine.JI_BondedWhsUnitQty = bondedWhsUnitQty;
			return invoiceLine;
		}

		void CreateOutOfWarehouseRefCusProcedure(ZString shipmentType)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "40";
			procedure.ZZ6_PreviousProcedureCode = "71";
			procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Germany;
			procedure.ZZ6_ShipmentType = shipmentType;
			procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure.ZZ6_Description = "Out of Warehouse";
		}

		void CreateOutOfInwardProcessingRefCusProcedure(ZString shipmentType)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "40";
			procedure.ZZ6_PreviousProcedureCode = "51";
			procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Germany;
			procedure.ZZ6_ShipmentType = shipmentType;
			procedure.ZZ6_OutOfInwardProcessing = WarehouseMoveStatus.Codes.Yes;
			procedure.ZZ6_Description = "Out of Warehouse";
		}

		void CreateWarehouseAdjustmentRefCusProcedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "01";
			procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Germany;
			procedure.ZZ6_ShipmentType = DEJobMessageTypeList.Codes.WarehouseAdjustment;
			procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure.ZZ6_Description = "Adjustment";
		}

		void UpdateBWHAttributeEntryDetails(string docketReference, short oldLineNo, string newEntryKey, short newLineNo)
		{
			var query = new ZQuery(WhsDocketSchema.WD_ExternalReference, SQLComparisonOperator.Equal, docketReference);
			var dockets = Factory.Load<IWhsDocket>(query);
			var docket = dockets.MaxBy(d => d.WD_ExternalReferenceSplit);

			if (docket != null)
			{
				var docketLineQuery = new ZQuery(WhsDocketLineSchema.WE_WD, docket.PK);
				docketLineQuery.AddToFilter(WhsDocketLineSchema.WE_LineNo, oldLineNo);

				var docketLine = Factory.LoadTop1<IWhsDocketLine>(docketLineQuery);

				var pickLineQuery = new ZDBOnlyQuery(typeof(IWhsPickLine));
				pickLineQuery.AddToFilter(WhsPickLineSchema.WZ_WE_TransactionLine, docketLine.PK);

				var pickLine = Factory.LoadTop1<IWhsPickLine>(pickLineQuery);

				var bwhAttributeQuery = new ZDBOnlyQuery(typeof(IWhsBondedWarehouseAttribute));
				bwhAttributeQuery.AddToFilter(WhsBondedWarehouseAttributeSchema.WB_ParentID, pickLine.WZ_WE_InventoryLine);

				var bwhAttribute = Factory.LoadTop1<IWhsBondedWarehouseAttribute>(bwhAttributeQuery);

				bwhAttribute.WB_EntryKey = newEntryKey;
				bwhAttribute.WB_EntryLineNo = newLineNo;
			}
		}

		class JobDeclarationForTest : JobDeclaration
		{
			public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool IsIntegrationWithAccountingSupportedExposed => IsIntegrationWithAccountingSupported;
		}
	}
}
