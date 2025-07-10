using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.Business.UniversalReferenceConstants;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Customs.FR;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclaration))]
	sealed class JobDeclarationTest : EU.Business.Declaration.Testing.JobDeclarationAbstractTest<JobDeclaration>
	{
		protected override EUCommonConstants.TransportModeSource ExpectedTransportMeansDependencyForImport => EUCommonConstants.TransportModeSource.TransportModeAtBorder;
		protected override EUCommonConstants.TransportModeSource ExpectedTransportMeansDependencyForExport => EUCommonConstants.TransportModeSource.TransportModeAtBorder;
		protected override EUCommonConstants.TransportModeSource ExpectedTransportMeansDependencyForMiscellaneousCustoms => EUCommonConstants.TransportModeSource.TransportModeAtBorder;

		public void TestCustomsOffices()
		{
			AssertType<FROfficeCodeCollection>(Factory.New<JobDeclaration>().CustomsOffices);
		}

		public override void TestGetCusCodeDataType()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(typeof(OfficeCode), ((ICusCodeDataTypeSupporter)declaration).GetCusCodeDataTypes()[EU.Business.CusCodeDataTypeList.Codes.OfficeCode]);
		}

		public void TestEntryDetails()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "BGM1";
			AddCusEntryNumber(entry, CusEntryNumberTypes.Standard.ImportControlNumber, "ICN1");
			AddCusEntryNumber(entry, CusEntryNumberTypes.Standard.MovementReferenceNumber, "MRN1");
			AddCusEntryNumber(entry, CusEntryHeader.Schema.FallbackEntryType, "FBK1");
			declaration.ShipmentSynchroniser.Synchronise();
			AssertEquals("The entry numbers with entry type FBK will be filtered out.", 2, shipment.CusEntryNumbers.Count);
			AssertEquals(true, shipment.CusEntryNumbers.Cast<CusEntryNumber>().Any(x => x.CE_EntryType == CusEntryNumberTypes.Standard.ImportControlNumber));
			AssertEquals(true, shipment.CusEntryNumbers.Cast<CusEntryNumber>().Any(x => x.CE_EntryType == CusEntryNumberTypes.Standard.MovementReferenceNumber));
			AssertEquals(false, shipment.CusEntryNumbers.Cast<CusEntryNumber>().Any(x => x.CE_EntryType == CusEntryHeader.Schema.FallbackEntryType));
		}

		CusEntryNumber AddCusEntryNumber(CusEntryHeader entry, string entryType, string entryNum, string countryCode = Core.Constants.CountryCodes.France)
		{
			var entryNumber = CusEntryNumber.New(entry, entryType, countryCode);
			entryNumber.CE_EntryNum = entryNum;
			return entryNumber;
		}

		public void TestDeltaImportRepresentationModeCalculationUsesOwnLogic()
		{
			var ucc6Importdeclaration = Factory.New<JobDeclaration>().WithFlux(EUJobMessageTypeList.Codes.Import);
			ucc6Importdeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			var deltaGImportdeclaration = Factory.New<JobDeclaration>().WithFlux(EUJobMessageTypeList.Codes.Import);
			deltaGImportdeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "1234567890", Enterprise.Core.Constants.CountryCodes.France);
			declarant.MainAddress.Address1 = "DeclarantAddress";

			ucc6Importdeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			deltaGImportdeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "1234567890", Enterprise.Core.Constants.CountryCodes.France);
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCN, "DIE003", ZString.Empty, ZString.Empty, "B26F06FF");
			EU.Business.EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.France).ZO_Box14UseIndirectRepresentation = false;
			importer.MainAddress.Address1 = "ImporterAddress";
			var alternateImporterAddress = declarant.Addresses.AddNew();
			alternateImporterAddress.Address1 = "AlternateImporterAddress";

			ucc6Importdeclaration.ImporterDocumentaryAddress.E2_AddressOverride = true;
			ucc6Importdeclaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;

			deltaGImportdeclaration.ImporterDocumentaryAddress.E2_AddressOverride = true;
			deltaGImportdeclaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;

			AssertEquals("Specific logic should be used for UCC6 Import Declaration", RepresentationTypeList.Codes.DIR, ucc6Importdeclaration.JE_DeclarantType);
			AssertEquals("EU logic should be used for DeltaG Import declaration", RepresentationTypeList.Codes.SEL, deltaGImportdeclaration.JE_DeclarantType);

			ucc6Importdeclaration.ImporterDocumentaryAddress.E2_AddressOverride = true;
			ucc6Importdeclaration.ImporterDocumentaryAddress.E2_OA_Address = alternateImporterAddress.PK;
			AssertEquals("Specific UCC6 Import logic also applies when recalculation occurs on Importer address change.", RepresentationTypeList.Codes.IND, ucc6Importdeclaration.JE_DeclarantType);
		}

		public void TestNonStandardCountryCodes()
		{
			var declaration = Factory.New<JobDeclaration>();
			foreach (var code in JobDeclaration.NonStandardCountryCodes)
			{
				declaration.JE_GoodsOrigin = code;
				Assert(declaration.HasNonStandardCountryOfOrigin);
				declaration.JE_GoodsDestination = code;
				Assert(declaration.HasNonStandardCountryOfDestination);
			}

			declaration.JE_GoodsOrigin = "QA";
			Assert(!declaration.HasNonStandardCountryOfOrigin);
			declaration.JE_GoodsDestination = "QA";
			Assert(!declaration.HasNonStandardCountryOfDestination);
		}

		public void TestJE_EntryStatusDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			declaration.JE_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			AssertEquals("Entry status description should be human readable (BAE in this case)", EntryStatusDescriptionCodeList.Descriptions.ES100, declaration.JE_EntryStatusDescription);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertEquals("Entry status description should be empty", "Unknown", declaration.JE_EntryStatusDescription);

			declaration.JE_EntryStatus = DeltaIEImportCusEntryStatusList.Codes.DeclarationAcceptedMrnAllocated;
			AssertEquals("Entry status description should be Declaration Accepted (MRN Allocated)", DeltaIEImportCusEntryStatusList.Descriptions.DeclarationAcceptedMrnAllocated, declaration.JE_EntryStatusDescription);
		}

		public void TestTariffTypeIsFixedToImportWhenTemplateCopy()
		{
			var declaration = Factory.New<JobDeclaration>();
			Factory.Save();
			AssertEquals("FR declaration should have TariffType=IMP by default.", Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff, declaration.JE_TariffType);
			var copiedDeclaration = (JobDeclaration)declaration.TemplateCopy();
			AssertEquals("JE_TariffType is fixed to IMP when copying.", Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff, copiedDeclaration.JE_TariffType);

			declaration.JE_TariffType = ZString.Empty;
			Factory.Save();
			AssertEquals(ZString.Empty, declaration.JE_TariffType);
			copiedDeclaration = (JobDeclaration)declaration.TemplateCopy();
			AssertEquals("TariffType on the source declaration is still empty.", ZString.Empty, declaration.JE_TariffType);
			AssertEquals("JE_TariffType is fixed to IMP when copying, even when it is empty in the source.", Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff, copiedDeclaration.JE_TariffType);
		}

		public void TestTariffTypeIsFixedToImportWhenGetNewRelated()
		{
			var declaration = Factory.New<JobDeclaration>();
			Factory.Save();
			AssertEquals(Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff, declaration.JE_TariffType);
			var relatedDeclaration = (JobDeclaration)declaration.GetNewRelatedDeclaration(Factory);
			AssertEquals("JE_TariffType is fixed to IMP when getting new related.", Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff, relatedDeclaration.JE_TariffType);

			declaration.JE_TariffType = ZString.Empty;
			Factory.Save();
			AssertEquals(ZString.Empty, declaration.JE_TariffType);
			relatedDeclaration = (JobDeclaration)declaration.GetNewRelatedDeclaration(Factory);
			AssertEquals("TariffType on the source declaration is still empty.", ZString.Empty, declaration.JE_TariffType);
			AssertEquals("JE_TariffType is fixed to IMP when getting new related, even when it is empty in the source.", Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff, relatedDeclaration.JE_TariffType);
		}

		public override void TestDoNotDefaultCartageEquipmentWhenObjectIsInitialised()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_RL_NKClosestPort = "AUSYD";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_RL_NKClosestPort = "FRPAR";

			var mainAddress = importer.Addresses.AddNewMainAddress();
			mainAddress.OA_AIREquipmentNeeded = "ABC";
			mainAddress.OA_FCLEquipmentNeeded = "DEF";
			mainAddress.OA_LCLEquipmentNeeded = "GHI";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = "AIR";
			declaration.JE_MessageType = ImportMessageTypeForTest;
			AssertEquals("Pre-req - this needs to be an import declaration.", true, declaration.IsImport);

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplier.Addresses.AddNewMainAddress().PK;
			declaration.SupplierPickupAddress.E2_OA_Address = declaration.SupplierDocumentaryAddress.E2_OA_Address;

			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = mainAddress.PK;
			declaration.ImporterDeliveryAddress.E2_OA_Address = mainAddress.PK;
			AssertEquals("JE_FCLDeliveryOrPickupEquipmentNeeded should be defaulted with main address equipement defined for transport mode.", "ABC", declaration.JE_FCLDeliveryOrPickupEquipmentNeeded);

			declaration.JE_FCLDeliveryOrPickupEquipmentNeeded = "DEF";
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var declarationLoaded = factory2.Load<BaseJobDeclaration>(declaration.PK);
			AssertEquals("JE_FCLDeliveryOrPickupEquipmentNeeded should not be recalculated.", "DEF", declarationLoaded.JE_FCLDeliveryOrPickupEquipmentNeeded);
		}

		public override void TestMergeByDefaultsFromClientWhenClientChanges()
		{
			var supplier = OrgHeader.New(Factory);
			supplier.OH_FullName = "Supplier";
			supplier.OH_RL_NKClosestPort = "AUSYD";

			var declaration = GetJobDeclaration();
			declaration.DisableMessageTypeChangeOnSupplierChangeForTesting = true;
			declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("Merge type should not be defaulted when no covenient registry settings is available.", "NON", declaration.JE_MergeBy);

			Env.Registry.SetCommercialInvoiceLineMergeMethod(GlbCompany.CurrentCompany.PK.ToGuid(), "CLS");

			var importer1 = OrgHeader.New(Factory);
			importer1.OH_FullName = "Importer1";
			importer1.OH_RL_NKClosestPort = "FRPAR";
			importer1.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "PNO";

			var importer2 = OrgHeader.New(Factory);
			importer2.OH_FullName = "Importer2";
			importer2.OH_RL_NKClosestPort = "FRPAR";
			importer2.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "TRD";

			var importer3 = OrgHeader.New(Factory);
			importer3.OH_FullName = "Importer3";
			importer3.OH_RL_NKClosestPort = "FRPAR";

			declaration.JE_OH_Importer = importer1.PK;
			AssertEquals(DefaultImportMessageType, declaration.JE_MessageType);
			AssertEquals("Merge type should be defaulted from client when able.", "PNO", declaration.JE_MergeBy);

			declaration.JE_OH_Importer = importer2.PK;
			AssertEquals(DefaultImportMessageType, declaration.JE_MessageType);
			AssertEquals("Merge type should be refreshed when client changes.", "TRD", declaration.JE_MergeBy);

			declaration.JE_OH_Importer = importer3.PK;
			AssertEquals(DefaultImportMessageType, declaration.JE_MessageType);
			AssertEquals("Merge type should be defaulted from registry if client default merge type has not been defined.", "CLS", declaration.JE_MergeBy);
		}

		public void TestShipmentSyncroniser()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = GetJobDeclaration();
			declaration.JE_JS = shipment.PK;

			AssertType<JobDeclarationSynchroniser>("ShipmentSynchroniser", declaration.ShipmentSynchroniser);
		}

		public override void TestDoNotDefaultPortsFromImporterWhenImporterCountryDifferentFromEnv()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_RL_NKClosestPort = "SGMIK";

			var importer = Factory.New<OrgHeader>();
			importer.OH_RL_NKClosestPort = "BEBRX";

			var declaration = GetJobDeclaration();

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;

			CombineAssertions("Import declaration port should not be defaulted from imùporter when importer country is not company country. Which is not likely to happen anyway.", () =>
			{
				Assert("JE_RL_NKPortOfArrival", declaration.JE_RL_NKPortOfArrival.IsEmpty);
				Assert("JE_RL_NKPortOfFirstArrival", declaration.JE_RL_NKPortOfFirstArrival.IsEmpty);
				Assert("JE_RL_NKFinalDestination", declaration.JE_RL_NKFinalDestination.IsEmpty);
			});
		}

		public void TestDefaultMessageTypeFromSupplierOrImporter()
		{
			var outsideEuOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			outsideEuOrgHeader.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Eritrea;

			var euOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			euOrgHeader.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Latvia;

			var frOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			frOrgHeader.OH_RL_NKClosestPort = Core.Constants.CountryCodes.France;

			var mqOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			mqOrgHeader.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Martinique;

			var gpOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			gpOrgHeader.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Guadeloupe;

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				AssertMessageType(EUJobMessageTypeList.Codes.Import, outsideEuOrgHeader, euOrgHeader);
				AssertMessageType(EUJobMessageTypeList.Codes.Import, outsideEuOrgHeader, frOrgHeader);
				AssertMessageType(EUJobMessageTypeList.Codes.Import, mqOrgHeader, euOrgHeader);
				AssertMessageType(EUJobMessageTypeList.Codes.Import, mqOrgHeader, frOrgHeader);
				AssertMessageType(EUJobMessageTypeList.Codes.Export, euOrgHeader, outsideEuOrgHeader);
				AssertMessageType(EUJobMessageTypeList.Codes.Export, euOrgHeader, mqOrgHeader);
				AssertMessageType(EUJobMessageTypeList.Codes.Export, frOrgHeader, outsideEuOrgHeader);
				AssertMessageType(EUJobMessageTypeList.Codes.Export, frOrgHeader, mqOrgHeader);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Martinique))
			{
				AssertMessageType(EUJobMessageTypeList.Codes.Import, outsideEuOrgHeader, mqOrgHeader);
				AssertMessageType(EUJobMessageTypeList.Codes.Import, euOrgHeader, mqOrgHeader);
				AssertMessageType(EUJobMessageTypeList.Codes.Import, frOrgHeader, mqOrgHeader);
				AssertMessageType(EUJobMessageTypeList.Codes.Import, gpOrgHeader, mqOrgHeader);
				AssertMessageType(EUJobMessageTypeList.Codes.Export, mqOrgHeader, outsideEuOrgHeader);
				AssertMessageType(EUJobMessageTypeList.Codes.Export, mqOrgHeader, euOrgHeader);
				AssertMessageType(EUJobMessageTypeList.Codes.Export, mqOrgHeader, frOrgHeader);
				AssertMessageType(EUJobMessageTypeList.Codes.Export, mqOrgHeader, gpOrgHeader);
			}

			void AssertMessageType(string messageType, OrgHeader supplier, OrgHeader importer)
			{
				declaration.SetupSupplier(supplier);
				declaration.SetupImporter(importer);
				AssertEquals(messageType, declaration.JE_MessageType);
			}
		}

		public void TestBusinessObjectsWithRelatedEventsForJobDocumentData()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var documentData = (BusinessObject)Factory.New<IVisualizerDocumentData>();
			documentData[JobDocumentDataSchema.Constants.JDD_ParentID] = entryHeader.PK;
			documentData[JobDocumentDataSchema.Constants.JDD_ParentTableCode] = "CH";

			AssertEquals(2, declaration.BusinessObjectsWithRelatedEvents.Length);
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { entryHeader, documentData }, declaration.BusinessObjectsWithRelatedEvents);
		}

		[TestDate(2017, 08, 14)]
		public override void TestResetInvoiceDateForGroupingInvoiceInTemplateCopy()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var groupinvoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			groupinvoice.JZ_InvoiceDate = new ZDateTime(2007, 08, 14);
			var normalinvoice = declaration.Invoices.AddNew();
			groupinvoice.JZ_InvoiceDate = new ZDateTime(2006, 08, 14);
			foreach (CusEntryInstruction instruction in declaration.CustomsEntryInstructions)
			{
				_ = instruction.GoodsLocation;
			}
			Factory.Save();

			var declarationCopy = (BaseJobDeclaration)declaration.TemplateCopy();
			var groupinvoiceCopy = (BaseJobComInvoiceGroupHeader)declarationCopy.AllGroupHeaders.First();
			AssertEquals(new ZDateTime(2017, 08, 14), groupinvoiceCopy.JZ_InvoiceDate);

			var normalinvoiceCopy = declarationCopy.Invoices.First(x => !x.JZ_GroupInvoice);
			AssertEquals(new ZDateTime(2006, 08, 14), normalinvoiceCopy.JZ_InvoiceDate);
		}

		public override void TestChangeMessageTypeFromSupplierOrImporter()
		{
			var uNLOCOAU = Factory.New<RefUNLOCO>();
			uNLOCOAU.RL_Code = "AU";
			uNLOCOAU.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var uNLOCOFR = Factory.New<RefUNLOCO>();
			uNLOCOFR.RL_Code = "FR";
			uNLOCOFR.RL_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var orgHeaderSupplier1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderSupplier1.OH_RL_NKClosestPort = "AU";
			var supplieradr1 = orgHeaderSupplier1.Addresses.AddNewMainAddress();
			var orgHeaderSupplier2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderSupplier2.OH_RL_NKClosestPort = "AU";
			var supplieradr2 = orgHeaderSupplier1.Addresses.AddNewMainAddress();

			var orgHeaderImporter1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderImporter1.OH_RL_NKClosestPort = "AU";
			var importeradr1 = orgHeaderImporter1.Addresses.AddNewMainAddress();
			var orgHeaderImporter2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderImporter2.OH_RL_NKClosestPort = "AU";
			var importeradr2 = orgHeaderImporter2.Addresses.AddNewMainAddress();

			CombineAssertions(() =>
			{
				using (EUCustomsDataRegistry.Instance.CheckChangeMessageTypeFromSupplierOrImporter.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.France))
					{
						GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.France;
						declaration.ImporterDocumentaryAddress.E2_OA_Address = orgHeaderImporter1.MainAddress.PK;
						declaration.SupplierDocumentaryAddress.E2_OA_Address = orgHeaderSupplier1.MainAddress.PK;
						declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
						declaration.SupplierDocumentaryAddress.E2_OA_Address = orgHeaderSupplier2.MainAddress.PK;
						AssertEquals("Registry = false, Supplier", JobMessageTypeList.Codes.Export, declaration.JE_MessageType);

						GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
						declaration.ImporterDocumentaryAddress.E2_OA_Address = orgHeaderImporter1.MainAddress.PK;
						declaration.SupplierDocumentaryAddress.E2_OA_Address = orgHeaderSupplier1.MainAddress.PK;
						declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
						declaration.ImporterDocumentaryAddress.E2_OA_Address = orgHeaderImporter2.MainAddress.PK;
						AssertEquals("Registry = false, Importer", JobMessageTypeList.Codes.Export, declaration.JE_MessageType);
					}
				}

				using (EUCustomsDataRegistry.Instance.CheckChangeMessageTypeFromSupplierOrImporter.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.France;
					declaration.ImporterDocumentaryAddress.E2_OA_Address = orgHeaderImporter1.MainAddress.PK;
					declaration.SupplierDocumentaryAddress.E2_OA_Address = orgHeaderSupplier1.MainAddress.PK;
					declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
					declaration.SupplierDocumentaryAddress.E2_OA_Address = orgHeaderSupplier2.MainAddress.PK;
					AssertEquals("Registry = false, Supplier", JobMessageTypeList.Codes.Export, declaration.JE_MessageType);

					GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
					declaration.ImporterDocumentaryAddress.E2_OA_Address = orgHeaderImporter1.MainAddress.PK;
					declaration.SupplierDocumentaryAddress.E2_OA_Address = orgHeaderSupplier1.MainAddress.PK;
					declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
					declaration.ImporterDocumentaryAddress.E2_OA_Address = orgHeaderImporter2.MainAddress.PK;
					AssertEquals("Registry = false, Importer", JobMessageTypeList.Codes.Export, declaration.JE_MessageType);
				}
			});
		}

		public void TestValidateZG_VATDeferNumberIsCalledAfterZG_VATDeferTypeModified()
		{
			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			AssertHasMessageErrors("Validation shall be made for ZG_VATDeferNumber when ZG_VATDeferType is modifed.", declaration.ZG_VATDeferNumberInfo);
		}

		public void TestIsIntegrationWithAccountingSupported()
		{
			var declaration = Factory.New<JobDeclarationForTesting>();
			Assert("Integration with accounting should be enabled in FR solution (mandatory for autobilling).", declaration.IsIntegrationWithAccountingSupportedExposed);
		}

		[TestDate(2023, 5, 9)]
		public void TestFormatOfDUCR()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var jobDec1 = Factory.New<JobDeclaration>();
				jobDec1.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
				jobDec1.JE_MessageType = JobMessageTypeList.Codes.Export;
				var declarant1 = Factory.New<OrgHeader>();
				declarant1.OH_Code = "declarant1";
				jobDec1.JE_OA_DeclarantAddress = declarant1.MainAddress.PK;
				jobDec1.JE_DeclarationReference = "B12345678";
				var trn1 = declarant1.CustomsCodes.AddNew();
				trn1.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				trn1.OK_CustomsRegNo = "987654321999";
				Factory.Save();
				AssertEquals("DUCR should contain '-' When declaration is not UCC6", "3" + Core.Constants.CountryCodes.France + "987654321999" + "-" + "B12345678", jobDec1.JE_UCR);

				var jobDec2 = Factory.New<JobDeclaration>();
				jobDec2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				jobDec2.JE_MessageType = JobMessageTypeList.Codes.Export;
				var declarant2 = Factory.New<OrgHeader>();
				declarant2.OH_Code = "declarant2";
				jobDec2.JE_OA_DeclarantAddress = declarant2.MainAddress.PK;
				jobDec2.JE_DeclarationReference = "B12345679";
				var trn2 = declarant2.CustomsCodes.AddNew();
				trn2.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				trn2.OK_CustomsRegNo = "887654321999";
				Factory.Save();
				AssertEquals("DUCR should not contain '-' When declaration is UCC6", "3" + Core.Constants.CountryCodes.France + "887654321999" + "B12345679", jobDec2.JE_UCR);
			}
		}

		public void TestJE_TariffType()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("IMP", declaration.JE_TariffType);
		}

		public override void TestGetCreditCheckMessage()
		{
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				base.TestGetCreditCheckMessage();

				var importer = Factory.New<OrgHeader>();
				importer.OH_Code = "IMPORTER";
				var declaration = GetNewBusinessObject() as JobDeclaration;
				declaration.JE_OH_Importer = importer.PK;
				declaration.ActiveEntryHeaders.AddNew();
				Factory.Save();

				var transaction = Factory.New<AccTransactionHeader>();
				transaction.AH_TransactionNum = "ACC001";
				transaction.AH_JobNumber = declaration.JobNumber;
				transaction.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
				transaction.AH_InvoiceTerm = Core.Constants.InvoiceTerms.PaymentInAdvance;
				transaction.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
				transaction.AH_OH = declaration.PK;
				transaction.AH_GB = GlbBranch.CurrentBranch.PK;
				transaction.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;
				transaction.AH_InvoiceDate = ZDateTime.Now;
				transaction.AH_OH = importer.PK;
				transaction.AH_GB = GlbBranch.CurrentBranch.PK;
				transaction.AH_GE = GlbDepartment.CurrentDepartment.PK;
				Factory.Save();

				using (FRCustomsDataRegistry.Instance.CheckAtEverySubmission.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true))
				using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					AssertContains("Delivery of this message is restricted because:\r\n       The Importer, Supplier, Local Client for Billing or any Debtors in associated Shipment\r\n\t  a) Has unpaid PIA (Payment In Advance) invoices posted on this shipment, OR\r\n\t  b) Has no invoices posted on this shipment and has credit terms PIA (Payment In Advance).", ((IBaseJobDeclaration)declaration).GetCreditCheckMessage());
				}

				using (FRCustomsDataRegistry.Instance.CheckAtEverySubmission.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, false))
				using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					AssertContains(ZString.Empty, ((IBaseJobDeclaration)declaration).GetCreditCheckMessage());
				}

				using (FRCustomsDataRegistry.Instance.CheckAtEverySubmission.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true))
				using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					AssertContains(ZString.Empty, ((IBaseJobDeclaration)declaration).GetCreditCheckMessage());
				}
			}
		}

		public void TestDefaultDataGrouping()
		{
			foreach (var country in Core.Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					var declaration = Factory.New<JobDeclaration>();
					CombineAssertions($"Default data groupings for {country}", () =>
					{
						AssertEquals("Default data grouping.", Core.Constants.CountryCodes.France, declaration.GetDefaultDataGroupingCode());
						AssertEquals("Default tariff data grouping.", Core.Constants.CountryCodes.France, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));
						AssertEquals("Default duties data grouping.", Core.Constants.CountryCodes.France, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.DutyRateCodes));
						AssertEquals("Default customs procedure data grouping.", Core.Constants.CountryCodes.France, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.CusProcedure));
						AssertEquals("Default additional documents data grouping.", Core.Constants.CountryCodes.France, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.AdditionalDocumentCodes));
					});
				}
			}
		}

		public override void TestDefaultDataGroupingCodeForCusProcedure()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			Factory.Save();

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertEquals(Core.Constants.CountryCodes.France, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.CusProcedure));

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertEquals(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.CusProcedure));
		}

		public override void TestDefaultDataGroupingCodeForAdditionalDocumentCodes()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			Factory.Save();

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertEquals(Core.Constants.CountryCodes.France, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.AdditionalDocumentCodes));

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertEquals(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.AdditionalDocumentCodes));
		}

		public void TestValidationType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertType<DeltaGJobDeclarationValidation>(declaration.Validation);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertType<DeltaIEJobDeclarationValidation>(declaration.Validation);
		}

		#region IHarbourJob Properties

		public void TestContainerMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bizObj = declaration as IHarbourJob;
			AssertEquals("ContainerMode should reflect JE_ContainerMode.", ZString.Empty, bizObj.ContainerMode);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("CustoContainerModemsOffice should reflect JE_ContainerMode.", Core.Constants.ContainerModes.FCL, bizObj.ContainerMode);
		}

		public void TestCustomsOffice()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bizObj = declaration as IHarbourJob;
			AssertEquals("CustomsOffice should reflect JE_CustomsOffice.", ZString.Empty, bizObj.CustomsOffice);

			declaration.JE_CustomsOffice = "FR002300";
			AssertEquals("CustomsOffice should reflect JE_CustomsOffice.", "FR002300", bizObj.CustomsOffice);
		}

		public void TestDataGrouping()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bizObj = declaration as IHarbourJob;
			AssertEquals("DataGrouping should reflect the declaration default data grouping.", "FR", bizObj.DataGrouping);
		}

		public void TestHarbourType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bizObj = declaration as IHarbourJob;
			declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
			AssertEquals("HarbourType should reflect JE_MessageType.", Customs.Common.EU.EUJobMessageTypeList.Codes.Import, bizObj.HarbourType);

			declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
			AssertEquals("HarbourType should reflect JE_MessageType.", Customs.Common.EU.EUJobMessageTypeList.Codes.Export, bizObj.HarbourType);
		}

		public void TestValuationDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bizObj = declaration as IHarbourJob;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			AssertNull(declaration.FirstActiveEntryHeaderWithEntryNum);
			AssertEquals("ValuationDate should be defaulted to current date when no entry header with number is available.", ZDateTime.Today, bizObj.ValuationDate);

			entryHeader.EntryNumber = "2200000001";
			var testDate = new ZDate(2022, 01, 01);
			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_DateForDuty = testDate;
			entryHeader.CH_CEI_Instruction = cei.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var cusEntryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = cusEntryLine.PK;
			invoiceLine.JI_CEI = cei.PK;

			AssertEquals("Prerequisite.", testDate, declaration.FirstActiveEntryHeaderWithEntryNum.EffectiveValuationDate);
			AssertEquals("ValuationDate should equal first entry header with entry number effective valuation date if any.", testDate, bizObj.ValuationDate);
		}

		#endregion

		#region JobDocAddresses

		public void TestDocAdresses()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<FRJobDocAddressDependentCollection>(declaration.DocAddresses);
		}

		public void TestImporterDocumentaryAddress()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "ORG1";

			var pstAddress = importer.Addresses.AddNew();
			pstAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Postal);
			var ofcAddress = importer.Addresses.AddNew();
			ofcAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);
			importer.MainAddress.SettingDefaults = true;
			importer.MainAddress.AddressCapability.SetCapabilityDisabled(OrgConstants.AddressType.Office);
			var ecaAddress = importer.Addresses.AddNew();
			ecaAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.EUCustomsAddress);

			var declaration = Factory.New<JobDeclaration>();
			declaration.ImporterDocumentaryAddress.Delete();
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("If the importer has an ECA address, Default address type should be EU Customs Address type", AddressType.ECA, declaration.ImporterDocumentaryAddress.DefaultAddressType);
			AssertEquals("If the importer has an ECA address, the existing EU Customs Address should be selected by default", ecaAddress.PK, declaration.ImporterDocumentaryAddress.E2_OA_Address);

			ecaAddress.Delete();
			declaration.ImporterDocumentaryAddress.Delete();
			importer.MainAddress.Delete();
			importer.Addresses.MainAddress.Delete();
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("If importer without ECA address, Default address type should be Office Address type", AddressType.OFC, declaration.ImporterDocumentaryAddress.DefaultAddressType);
			AssertEquals("If importer without ECA address, the existing OFC Address should be selected by default", ofcAddress.PK, declaration.ImporterDocumentaryAddress.E2_OA_Address);
		}

		public void TestExporterDocumentaryAddress()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "ORG1";

			var pstAddress = supplier.Addresses.AddNew();
			pstAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Postal);
			var ofcAddress = supplier.Addresses.AddNew();
			ofcAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);
			supplier.MainAddress.SettingDefaults = true;
			supplier.MainAddress.AddressCapability.SetCapabilityDisabled(OrgConstants.AddressType.Office);
			var ecaAddress = supplier.Addresses.AddNew();
			ecaAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.EUCustomsAddress);

			var declaration = Factory.New<JobDeclaration>();
			declaration.SupplierDocumentaryAddress.Delete();
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("If the supplier has an ECA address, Default address type should be EU Customs Address type", AddressType.ECA, declaration.SupplierDocumentaryAddress.DefaultAddressType);
			AssertEquals("If the supplier has an ECA address, the existing EU Customs Address should be selected by default", ecaAddress.PK, declaration.SupplierDocumentaryAddress.E2_OA_Address);

			ecaAddress.Delete();
			declaration.SupplierDocumentaryAddress.Delete();
			supplier.MainAddress.Delete();
			supplier.Addresses.MainAddress.Delete();
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("If supplier without ECA address, Default address type should be Office Address type", AddressType.OFC, declaration.SupplierDocumentaryAddress.DefaultAddressType);
			AssertEquals("If supplier without ECA address, the existing OFC Address should be selected by default", ofcAddress.PK, declaration.SupplierDocumentaryAddress.E2_OA_Address);
		}

		#endregion

		public void TestEUD_AgreedPlaceCodeValidationSupport()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(false, declaration.EUD_AgreedPlaceCodeValidationSupport);
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.AddInfoChildValidation.ValidateEUD_AgreedPlaceCode();
				AssertNoMessageErrors(declaration.EUD_AgreedPlaceCodeInfo);
			}
		}

		public void TestChargePaymentOrDestinationID_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Payment/Destination", DataBoundResourceStrings.GetDataForProperty(declaration.ChargePaymentOrDestinationIDInfo).Caption);
		}

		public void TestPaymentDestinationDefaulting()
		{
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateHarbourRate("IMP", "108", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "IF([TLCL] > 1, MAX(2, 0.5 * [TLCL]), 0)", "FR");
			referenceDataHelper.CreateHarbourRate("IMP", "395", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "[40LCL] * 10 + [20LCL] * 5 + [45LCL] * 2", "FR");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "010", "BORDEAUX BASSENS", "FRBAS", "FR000100");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "202", "BORDEAUX BASSENS 2", "FRBAS", "FR000100");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "108", "BORDEAUX BASSENS 3", "FRNTE", "FR005340");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "230", "BORDEAUX BASSENS 4", "FRLEH", "FR004560");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "395", "BORDEAUX BASSENS 5", "FRLEH", "FR004560");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_CustomsOffice = "FR000100";
			declaration.JE_RL_NKPortOfArrival = "FRBAS";
			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew(typeof(CusEntryHeader)) as CusEntryHeader;

			var entryLine = cusEntryHeader.AllEntryLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var container20LCL = declaration.CusContainers.AddNew();
			container20LCL.CO_Weight = 999.1m;
			container20LCL.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;

			var container40LCL = declaration.CusContainers.AddNew();
			container40LCL.CO_Weight = 2999.1m;
			container40LCL.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;

			var container45LCL = declaration.CusContainers.AddNew();
			container45LCL.CO_Weight = 4999.1m;
			container45LCL.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;

			var empty45LCLContainer = declaration.CusContainers.AddNew();
			empty45LCLContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;

			Factory.Save();

			var refContainer20LCL = Factory.New<RefContainer>();
			refContainer20LCL.RC_StorageClass = "20";
			refContainer20LCL.RC_Code = "C20LCL";
			container20LCL.CO_RC = refContainer20LCL.PK;

			var refContainer40LCL = Factory.New<RefContainer>();
			refContainer40LCL.RC_StorageClass = "40";
			refContainer40LCL.RC_Code = "C40LCL";
			container40LCL.CO_RC = refContainer40LCL.PK;

			var refContainer45LCL = Factory.New<RefContainer>();
			refContainer45LCL.RC_StorageClass = "45";
			refContainer45LCL.RC_Code = "C45LCL";
			container45LCL.CO_RC = refContainer45LCL.PK;

			empty45LCLContainer.CO_RC = refContainer45LCL.PK;

			Factory.Save();

			container20LCL.JobContainer.JC_RC = refContainer20LCL.PK;
			container40LCL.JobContainer.JC_RC = refContainer40LCL.PK;
			container45LCL.JobContainer.JC_RC = refContainer45LCL.PK;
			empty45LCLContainer.JobContainer.JC_RC = refContainer45LCL.PK;

			Factory.Save();

			AssertEquals("FRBAS/FR000100 is not unique key in PortTaxHelper class. ChargePaymentOrDestinationID should not be defaulted, because no Harbour fee that matches any of ChargePaymentOrDestinationIDs (010 and 202) was found in the DB.", ZString.Empty, declaration.ChargePaymentOrDestinationID);

			declaration.JE_CustomsOffice = "FR005340";
			declaration.JE_RL_NKPortOfArrival = "FRNTE";
			AssertEquals("FRNTE/FR005340 leads to the unique port code 108. ChargePaymentOrDestinationID should be defaulted to 108, because a formula exists for port 108.", "108", declaration.ChargePaymentOrDestinationID);

			declaration.JE_CustomsOffice = "FR004560";
			declaration.JE_RL_NKPortOfArrival = "FRLEH";
			AssertEquals("FRLEH/FR004560 is not unique key in PortTaxHelper class. ChargePaymentOrDestinationID should be defaulted, because a formula that matches one of ChargePaymentOrDestinationIDs (namely 395) was found in the DB.", "395", declaration.ChargePaymentOrDestinationID);
		}

		public void TestChargePaymentOrDestinationID()
		{
			var factory1 = new BusinessObjectFactory();
			var declaration1 = factory1.New<JobDeclaration>();
			declaration1.ChargePaymentOrDestinationID = "123";
			factory1.Save();

			var factory2 = new BusinessObjectFactory();
			var declarations = factory2.Load<JobDeclaration>(new ZQuery());
			AssertEquals("ChargePaymentOrDestinationID", "123", declarations[0].ChargePaymentOrDestinationID);
		}

		public void TestIsUCC6()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertEquals("Only DeltaIE declaration should be flagged as UCC6.", false, declaration.IsUCC6);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertEquals("DeltaIE declaration should be flagged as UCC6.", true, declaration.IsUCC6);
		}

		public void TestJE_ApplicationCodeReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("JE_ApplicationCode is never read only", false, declaration.JE_ApplicationCodeInfo.ReadOnly);
		}

		public void TestDefaultJE_ApplicationCode()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			AssertEquals("Application code default value should DG as the registry is not overidden.", DeclarationApplicationCodeList.Codes.DeltaG, declaration.JE_ApplicationCode);

			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, true))
			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForExports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, true))
			{
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
				AssertEquals("Delta IE is enabled for both message => the value shouldn't have been changed.", DeclarationApplicationCodeList.Codes.DeltaIE, declaration.JE_ApplicationCode);
			}

			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, true))
			{
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
				AssertEquals("Delta IE is not enabled for export => the value should have been changed to default value.", DeclarationApplicationCodeList.Codes.DeltaG, declaration.JE_ApplicationCode);
			}

			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForExports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, true))
			{
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				AssertEquals("Delta IE is not enabled for import => the value should have been changed to default value.", DeclarationApplicationCodeList.Codes.DeltaG, declaration.JE_ApplicationCode);
			}

			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.CurrentFallbackLevel = new FallbackLevel(GlbCompany.CurrentCompany, null, null);
			customsInterface.RecipientID = "123";
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				AssertEquals("LocalCountryCustomsInterface is ITF => the value should have been changed to ITF.", DeclarationApplicationCodeList.Codes.Interface, declaration.JE_ApplicationCode);
			}
		}

		public void TestIncoTermAndCustomsChargeFactoryTypeUpdatedWhenMessageTypeChanges()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			AssertType<IncoTermAndCustomsChargeFactory>(declaration.IncoTermAndChargeFactory);
			AssertType<IncoTermAndCustomsChargeFactory>(invoice.IncoTermAndChargeFactory);
			AssertType<IncoTermAndCustomsChargeFactory>(invoice.GroupHeader.IncoTermAndChargeFactory);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			AssertType<ExportIncoTermAndCustomsChargeFactory>(declaration.IncoTermAndChargeFactory);
			AssertType<ExportIncoTermAndCustomsChargeFactory>(invoice.IncoTermAndChargeFactory);
			AssertType<ExportIncoTermAndCustomsChargeFactory>(invoice.GroupHeader.IncoTermAndChargeFactory);
		}

		public void TestCusEntryHeaderGetsTheRightDocumentSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertType<CusEntryHeader>(entryHeader);
			AssertType<CusEntryHeaderDocumentSupporter>(entryHeader.DocumentSupporter);

			var docSupporter = (CusEntryHeaderDocumentSupporter)entryHeader.DocumentSupporter;
			AssertEquals(true, docSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.SADH)));
			AssertEquals(1, docSupporter.GetDocumentWrappers(Core.Constants.DataContext.SADH, null).Length);
		}

		public void TestPackageMarksAndNumbersAlwaysRequiredValidationMessage()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
				AssertEquals("Not Import", false, declaration.PackageMarksAndNumbersAlwaysRequiredValidationMessage.IsEmpty);
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				AssertEquals("Import", true, declaration.PackageMarksAndNumbersAlwaysRequiredValidationMessage.IsEmpty);
			});
		}

		public void TestZG_CTStatusID()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = new ZDateTime(2021, 8, 4, 11, 0, 0);
			AssertEquals(0, declaration.Invoices.Count);

			declaration.ZG_CTStatusID = "T2LF";
			AssertEquals(1, declaration.Invoices.Count);

			var invoice = declaration.Invoices.FirstOrDefault() as JobComInvoiceHeader;
			AssertEquals(1, invoice.SupportingDocuments.Count);

			var supportingDocument = invoice.SupportingDocuments.OfType<SupportingDocument>().FirstOrDefault();
			AssertEquals("C620", supportingDocument.CSI_Code);
			AssertEquals("T2LF", supportingDocument.CSI_ReferenceNumber);
			AssertEquals(new ZDateTime(2021, 8, 4, 11, 0, 0), supportingDocument.CSI_DateOfIssue);

			declaration.ZG_CTStatusID = ZString.Empty;
			declaration.ZG_CTStatusID = "T2LF";
			AssertEquals(1, invoice.SupportingDocuments.Count);

			supportingDocument = invoice.SupportingDocuments.OfType<SupportingDocument>().FirstOrDefault();
			AssertEquals("C620", supportingDocument.CSI_Code);
			AssertEquals("T2LF", supportingDocument.CSI_ReferenceNumber);
			AssertEquals(new ZDateTime(2021, 8, 4, 11, 0, 0), supportingDocument.CSI_DateOfIssue);
		}

		public override void TestCustomsOfficeOfEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.JE_CustomsOffice = "LV001000";
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, "LV002000");
			AssertEquals("LV002000", declaration.OfficeOfEntry);
		}

		public void TestGetIApportionInvoiceHolderCountryContextCore()
		{
			var declaration = GetJobDeclarationForTesting();
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("Import", "FR", declaration.GetIApportionInvoiceHolderCountryContextCore());

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertEquals("Export", "FREXP", declaration.GetIApportionInvoiceHolderCountryContextCore());
			});
		}

		public void TestCustomsProfileRelatedAccountRepresentativeID()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusAccount = Factory.New<OrgCusAccount>();
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			orgCusAccount.CZ_Account = "TESTACC";
			orgCusAccount.CZ_RepresentativeID = "TESTREPID";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_OH = importer.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_CustomsProfile = ZString.Empty;
			AssertEquals("CustomsProfileRelatedAccountRepresentativeID is empty because no account is selected.", ZString.Empty, declaration.CustomsProfileRelatedAccountRepresentativeID);
			declaration.JE_CustomsProfile = "TESTACC";
			AssertEquals("CustomsProfileRelatedAccountRepresentativeID comes from CZ_RepresentativeID of the selected account.", "TESTREPID", declaration.CustomsProfileRelatedAccountRepresentativeID);
		}

		public void TestJE_CustomsProfileAuthorizedLocations()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var orgCusAccount = Factory.New<OrgCusAccount>();

			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			orgCusAccount.CZ_Account = "TESTACC";
			orgCusAccount.CZ_RepresentativeID = "TESTREPID";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_CustomsProfile = ZString.Empty;
			AssertEquals("JE_LocationOfGoodsRelatedCusAuthorisation should be empty when JE_customsProfile is empty.", Enumerable.Empty<ZString>(), declaration.JE_CustomsProfileAuthorizedLocations);

			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_CustomsProfile = "TestWithoutAccount";
			AssertEquals("JE_LocationOfGoodsRelatedCusAuthorisation should be empty when the selected Customs profile does not match any account.", Enumerable.Empty<ZString>(), declaration.JE_CustomsProfileAuthorizedLocations);

			declaration.JE_CustomsProfile = "TESTACC";
			AssertEquals("JE_LocationOfGoodsRelatedCusAuthorisation should be empty when selected Customs profile account has no organisation.", Enumerable.Empty<ZString>(), declaration.JE_CustomsProfileAuthorizedLocations);

			orgCusAccount.CZ_OH = importer.PK;
			AssertEquals("JE_LocationOfGoodsRelatedCusAuthorisation should be empty when selected Customs profile account has no authorisation.", 0, declaration.JE_CustomsProfileAuthorizedLocations.Count());

			declaration.Importer.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir).WithNumber("TIR123456");
			AssertEquals("JE_LocationOfGoodsRelatedCusAuthorisation should be empty when selected Customs profile account has no AUL type of authorisation.", 0, declaration.JE_CustomsProfileAuthorizedLocations.Count());

			declaration.Importer.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation).WithNumber("AUL123456");
			AssertContainsExactElementsInAnyOrder("JE_LocationOfGoodsRelatedCusAuthorisation has one value because selected Customs profile account has one AUL authorisation.", new List<ZString>() { "AUL123456" }, declaration.JE_CustomsProfileAuthorizedLocations.ToList());

			declaration.Importer.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation).WithNumber("AUL234567");
			AssertContainsExactElementsInAnyOrder("JE_LocationOfGoodsRelatedCusAuthorisation should return all selected Customs profile account reference numbers.", new List<ZString>() { "AUL123456", "AUL234567" }, declaration.JE_CustomsProfileAuthorizedLocations.ToList());
		}

		public void TestDeriveEntryStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_Status = MessageStatusList.Codes.AwaitingResponse;
			declaration.DeriveDeclarationStatus();
			AssertEquals(EntryStatusDescriptionCodeList.Codes.ES010, declaration.JE_EntryStatus);
		}

		public void Test_IsOfficeOfLodgementDifferentFromOfficeOfExit()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "FRXXXXX";
			AssertEquals(ZString.Empty, declaration.OfficeOfExit);
			AssertEquals(false, declaration.IsOfficeOfLodgementDifferentFromOfficeOfExit);

			var officeOfExit = declaration.CustomsOffices.Cast<EuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfExit);
			officeOfExit.CY_Data = "FRXXXXX";
			AssertEquals(false, declaration.IsOfficeOfLodgementDifferentFromOfficeOfExit);

			officeOfExit.CY_Data = "FRYYYYY";
			AssertEquals(true, declaration.IsOfficeOfLodgementDifferentFromOfficeOfExit);
		}

		public void TestJE_DefermentAccountNumber()
		{
			var declaration = Factory.New<JobDeclaration>();

			var testImporter = Factory.NewWithValidTestData<OrgHeader>();
			testImporter.OH_FullName = "Singapore Test Importer Pte. Ltd.";
			testImporter.MainAddress.OA_Address1 = "Changi Airport";
			testImporter.MainAddress.OA_Address2 = "Building 3C";
			testImporter.OH_Code = "TEST";
			testImporter.OH_RL_NKClosestPort = "FRPAR";
			testImporter.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "DGI001", ZString.Empty, ZString.Empty, "CC1F1BB8");
			testImporter.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "12345678", Core.Constants.CountryCodes.France);

			var testSupplier = Factory.NewWithValidTestData<OrgHeader>();
			testSupplier.OH_FullName = "London Test Supplier DHL Ltd.";
			testSupplier.MainAddress.OA_Address1 = "Heathrow Airport";
			testSupplier.MainAddress.OA_Address2 = "Building 1B";
			testSupplier.OH_Code = "TEST";
			testSupplier.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals(true, declaration.JE_DefermentAccountNumber == ZString.Empty);

			declaration.JE_OH_Importer = testImporter.PK;
			declaration.JE_OH_Supplier = testSupplier.PK;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes.SEL;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_CustomsProfile = "DGI001";
			AssertEquals(true, declaration.JE_DefermentAccountNumber == ZString.Empty);

			declaration.JE_PaymentMethod = MethodOfPaymentList.Codes.R;
			AssertEquals(true, declaration.JE_DefermentAccountNumber == "12345678");
		}

		public void TestHasValidPreviousDocumentForExportExitType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			declaration.JE_ExportExitType = ExportExitTypeList.Codes.TRA;
			AssertEquals("A declaration with Exit Type = TRA requires a previous document with specific code and a valid reference", false, declaration.HasValidPreviousDocumentForExportExitType);

			var declarationPreviousDocument = declaration.PreviousDocuments.AddNew();
			AssertEquals("A declaration with Exit Type = TRA requires a previous document with specific code and a valid reference", false, declaration.HasValidPreviousDocumentForExportExitType);

			declarationPreviousDocument.CSI_ReferenceNumber = "ZZZ";
			declarationPreviousDocument.CSI_Code = "000";
			AssertEquals("A declaration with Exit Type = TRA requires a previous document with specific code and a valid reference", false, declaration.HasValidPreviousDocumentForExportExitType);

			declarationPreviousDocument.CSI_ReferenceNumber = ZString.Empty;
			declarationPreviousDocument.CSI_Code = declaration.AllowedPreviousDocsForTRAExportExitType[0];
			AssertEquals("A declaration with Exit Type = TRA requires a previous document with specific code and a valid reference", false, declaration.HasValidPreviousDocumentForExportExitType);

			declarationPreviousDocument.CSI_ReferenceNumber = "ZZZ";
			declarationPreviousDocument.CSI_Code = declaration.AllowedPreviousDocsForTRAExportExitType[0];
			AssertEquals("A declaration with Exit Type = TRA requires a previous document with specific code and a valid reference", true, declaration.HasValidPreviousDocumentForExportExitType);

			declarationPreviousDocument.Delete();
			var invoicePreviousDocument = invoice.PreviousDocuments.AddNew();
			invoicePreviousDocument.CSI_ReferenceNumber = "ZZZ";
			invoicePreviousDocument.CSI_Code = declaration.AllowedPreviousDocsForTRAExportExitType[0];
			AssertEquals("A declaration with Exit Type = TRA requires a previous document with specific code and a valid reference", true, declaration.HasValidPreviousDocumentForExportExitType);

			invoicePreviousDocument.Delete();
			var invoiceLinePreviousDocument = invoiceLine.PreviousDocuments.AddNew();
			invoiceLinePreviousDocument.CSI_ReferenceNumber = "ZZZ";
			invoiceLinePreviousDocument.CSI_Code = declaration.AllowedPreviousDocsForTRAExportExitType[0];
			AssertEquals("A declaration with Exit Type = TRA requires a previous document with specific code and a valid reference", true, declaration.HasValidPreviousDocumentForExportExitType);

			declaration.JE_ExportExitType = ExportExitTypeList.Codes.EMC;
			AssertEquals("A declaration with Exit Type = EMC requires a previous document with specific code and a valid reference", false, declaration.HasValidPreviousDocumentForExportExitType);

			invoiceLinePreviousDocument.CSI_Code = declaration.AllowedPreviousDocsForEMCExportExitType[0];
			AssertEquals("A declaration with Exit Type = EMC requires a previous document with specific code and a valid reference", true, declaration.HasValidPreviousDocumentForExportExitType);

			invoiceLinePreviousDocument.Delete();
			AssertEquals("A declaration with Exit Type = EMC requires a previous document with specific code and a valid reference", false, declaration.HasValidPreviousDocumentForExportExitType);

			string[] exportExitTypeList = new string[] { ExportExitTypeList.Codes.OTH, ExportExitTypeList.Codes.STC, ExportExitTypeList.Codes.ECS, ZString.Empty };
			foreach (var exportExitType in exportExitTypeList)
			{
				declaration.JE_ExportExitType = exportExitType;
				AssertEquals("A declaration with Exit Type different from EMC or TRA doesn't require any specific previous document", true, declaration.HasValidPreviousDocumentForExportExitType);
			}
		}

		public void TestHasValidExportExitTypeCTStatusCombination()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.EU.Business.MessageTypeList.Codes.Export;
			string[] exportExitTypeList = new string[] { ExportExitTypeList.Codes.OTH, ExportExitTypeList.Codes.STC, ExportExitTypeList.Codes.ECS, ExportExitTypeList.Codes.EMC, ZString.Empty };
			foreach (var exportExitType in exportExitTypeList)
			{
				declaration.JE_ExportExitType = exportExitType;
				AssertEquals("A declaration with Exit Type different from TRA doesn't require any specific CT status", true, declaration.HasValidExportExitTypeCTStatusCombination);
			}

			declaration.JE_ExportExitType = ExportExitTypeList.Codes.TRA;
			AssertEquals("A declaration with Exit Type different from TRA doesn't requires its CT status to be either T1, T2 or T-", false, declaration.HasValidExportExitTypeCTStatusCombination);

			declaration.ZG_CTStatusID = ExportCommunityTransitStatusList.Codes.T1;
			AssertEquals("A declaration with Exit Type different from TRA doesn't requires its CT status to be either T1, T2 or T-", true, declaration.HasValidExportExitTypeCTStatusCombination);

			declaration.ZG_CTStatusID = ExportCommunityTransitStatusList.Codes.T2;
			AssertEquals("A declaration with Exit Type different from TRA doesn't requires its CT status to be either T1, T2 or T-", true, declaration.HasValidExportExitTypeCTStatusCombination);

			declaration.ZG_CTStatusID = "T-";
			AssertEquals("A declaration with Exit Type different from TRA doesn't requires its CT status to be either T1, T2 or T-", true, declaration.HasValidExportExitTypeCTStatusCombination);
		}

		public void TestIsContainerizedAndHasContainer()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = "IMP";

			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.FillWithValidTestData();

			var entryheader = declaration.CustomsEntryHeaders.AddNew();
			var entryline = entryheader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryline.PK;

			AssertIsContainerizedAndHasContainer(declaration, false);

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = ZString.Empty;
			AssertIsContainerizedAndHasContainer(declaration, false);

			container.CO_ContainerNumber = "container";
			AssertIsContainerizedAndHasContainer(declaration, true);
		}

		void AssertIsContainerizedAndHasContainer(JobDeclaration declaration, bool containerTra)
		{
			declaration.JE_ContainerMode = ZString.Empty;
			AssertEquals(false, declaration.IsContainerizedAndHasContainer);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals(false, declaration.IsContainerizedAndHasContainer);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
			AssertEquals(false, declaration.IsContainerizedAndHasContainer);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.RollOnRollOff;
			AssertEquals(false, declaration.IsContainerizedAndHasContainer);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals(false, declaration.IsContainerizedAndHasContainer);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Loose;
			AssertEquals(false, declaration.IsContainerizedAndHasContainer);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LTL;
			AssertEquals(false, declaration.IsContainerizedAndHasContainer);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FTL;
			AssertEquals(false, declaration.IsContainerizedAndHasContainer);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals(false, declaration.IsContainerizedAndHasContainer);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(containerTra, declaration.IsContainerizedAndHasContainer);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals(containerTra, declaration.IsContainerizedAndHasContainer);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.ULD;
			AssertEquals(containerTra, declaration.IsContainerizedAndHasContainer);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Liquid;
			AssertEquals(containerTra, declaration.IsContainerizedAndHasContainer);
		}

		public void TestGetAllowedPreviousDocsForExportExitType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.EU.Business.MessageTypeList.Codes.Export;
			AssertEquals("Only Exit Type EMC requires a specific previous document", 0, declaration.GetAllowedPreviousDocsForExportExitType().Length);

			declaration.JE_ExportExitType = ExportExitTypeList.Codes.EMC;
			AssertContainsExactElementsInAnyOrder("Exit Type EMC requires a specific previous document of code AAD", new ZString[] { "AAD" }, declaration.GetAllowedPreviousDocsForExportExitType());
		}

		public void TestJE_ExportExitTypeList()
		{
			AssertEquals("Lookups.ExportExitTypeList", Factory.New<JobDeclaration>().JE_ExportExitTypeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestJE_RegionOrTerritoryOfDestinationList()
		{
			AssertEquals("Lookups.RegionOrTerritoryOfDestinationList", Factory.New<JobDeclaration>().JE_RegionOrTerritoryOfDestinationInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestSetRegionOrTerritoryOfDestinationDefaultValueForImport()
		{
			SetUpRefUNLOCO();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_RL_NKFinalDestination = "FRPAR";
			AssertEquals("CONTI", declaration.JE_RegionOrTerritoryOfDestination);
			declaration.JE_RL_NKFinalDestination = "FR2AC";
			AssertEquals("CORSE", declaration.JE_RegionOrTerritoryOfDestination);
			declaration.JE_RL_NKFinalDestination = "FR2BC";
			AssertEquals("CORSE", declaration.JE_RegionOrTerritoryOfDestination);
			declaration.JE_RL_NKFinalDestination = "MQAAA";
			AssertEquals("MARTI", declaration.JE_RegionOrTerritoryOfDestination);
			declaration.JE_RL_NKFinalDestination = "GPAAA";
			AssertEquals("GUADE", declaration.JE_RegionOrTerritoryOfDestination);
			declaration.JE_RL_NKFinalDestination = "REAAA";
			AssertEquals("REUNI", declaration.JE_RegionOrTerritoryOfDestination);
			declaration.JE_RL_NKFinalDestination = "GFAAA";
			AssertEquals("GUYAN", declaration.JE_RegionOrTerritoryOfDestination);
			declaration.JE_RL_NKFinalDestination = "MFAAA";
			AssertEquals("GUADE", declaration.JE_RegionOrTerritoryOfDestination);
			declaration.JE_RL_NKFinalDestination = "YTAAA";
			AssertEquals("MAYOT", declaration.JE_RegionOrTerritoryOfDestination);
			declaration.JE_RL_NKFinalDestination = "USATL";
			AssertEquals(ZString.Empty, declaration.JE_RegionOrTerritoryOfDestination);
			declaration.JE_RL_NKFinalDestination = ZString.Empty;
			AssertEquals(ZString.Empty, declaration.JE_RegionOrTerritoryOfDestination);
		}

		public void TestSetRegionOrTerritoryOfDestinationDefaultValueForExport()
		{
			SetUpRefUNLOCO();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_RL_NKOrigin = "FRPAR";
			AssertEquals("CONTI", declaration.JE_RegionOrTerritoryOfDestination);
			declaration.JE_RL_NKOrigin = "FR2AC";
			AssertEquals("CORSE", declaration.JE_RegionOrTerritoryOfDestination);
			declaration.JE_RL_NKOrigin = "FR2BC";
			AssertEquals("CORSE", declaration.JE_RegionOrTerritoryOfDestination);
			declaration.JE_RL_NKOrigin = "MQAAA";
			AssertEquals("MARTI", declaration.JE_RegionOrTerritoryOfDestination);
			declaration.JE_RL_NKOrigin = "GPAAA";
			AssertEquals("GUADE", declaration.JE_RegionOrTerritoryOfDestination);
			declaration.JE_RL_NKOrigin = "REAAA";
			AssertEquals("REUNI", declaration.JE_RegionOrTerritoryOfDestination);
			declaration.JE_RL_NKOrigin = "GFAAA";
			AssertEquals("GUYAN", declaration.JE_RegionOrTerritoryOfDestination);
			declaration.JE_RL_NKOrigin = "MFAAA";
			AssertEquals("GUADE", declaration.JE_RegionOrTerritoryOfDestination);
			declaration.JE_RL_NKOrigin = "YTAAA";
			AssertEquals("MAYOT", declaration.JE_RegionOrTerritoryOfDestination);
			declaration.JE_RL_NKOrigin = "USATL";
			AssertEquals(ZString.Empty, declaration.JE_RegionOrTerritoryOfDestination);
			declaration.JE_RL_NKOrigin = ZString.Empty;
			AssertEquals(ZString.Empty, declaration.JE_RegionOrTerritoryOfDestination);
		}

		public void TestJEGoodsDestinationAndFrenchTerritories()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.EUCTY, MapDirectionList.Codes.BTH, "Description", false);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "JE", "GB", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "RS", "XS", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_RL_NKFinalDestination = "FRPAR";
			AssertEquals("For FR territories we should have the code of the territory : France.", "FR", declaration.JE_GoodsDestination);
			declaration.JE_RL_NKFinalDestination = "MQAAA";
			AssertEquals("For FR territories we should have the code of the territory : Martinique.", "MQ", declaration.JE_GoodsDestination);
			declaration.JE_RL_NKFinalDestination = "GPAAA";
			AssertEquals("For FR territories we should have the code of the territory : guadalupe.", "GP", declaration.JE_GoodsDestination);
			declaration.JE_RL_NKFinalDestination = "REAAA";
			AssertEquals("For FR territories we should have the code of the territory : reunion.", "RE", declaration.JE_GoodsDestination);
			declaration.JE_RL_NKFinalDestination = "GFAAA";
			AssertEquals("For FR territories we should have the code of the territory : french guyana.", "GF", declaration.JE_GoodsDestination);
			declaration.JE_RL_NKFinalDestination = "MFAAA";
			AssertEquals("For FR territories we should have the code of the territory : saint martin.", "MF", declaration.JE_GoodsDestination);
			declaration.JE_RL_NKFinalDestination = "YTAAA";
			AssertEquals("For FR territories we should have the code of the territory : mayotte.", "YT", declaration.JE_GoodsDestination);
			declaration.JE_RL_NKFinalDestination = "RS4SN";
			AssertEquals("For JE_RL_NKFinalDestination starting with RS we should have XS as configured in cusmap.", "XS", declaration.JE_GoodsDestination);
			declaration.JE_RL_NKFinalDestination = "JESTH";
			AssertEquals("For JE_RL_NKFinalDestination starting with JE we should have GB as configured in cusmap.", "GB", declaration.JE_GoodsDestination);
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			AssertEquals("In general we should have the first two char.", "AU", declaration.JE_GoodsDestination);
			declaration.JE_RL_NKFinalDestination = "GPAIF";
			AssertEquals("In general we should have the first two char.", "GP", declaration.JE_GoodsDestination);
		}

		public void TestJEGoodsOriginAndFrenchTerritories()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.EUCTY, MapDirectionList.Codes.BTH, "Description", false);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "JE", "GB", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "RS", "XS", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_RL_NKOrigin = "FRPAR";
			AssertEquals("For FR territories we should have the code of the territory : France.", "FR", declaration.JE_GoodsOrigin);
			declaration.JE_RL_NKOrigin = "MQAAA";
			AssertEquals("For FR territories we should have the code of the territory : Martinique.", "MQ", declaration.JE_GoodsOrigin);
			declaration.JE_RL_NKOrigin = "GPAAA";
			AssertEquals("For FR territories we should have the code of the territory : guadalupe.", "GP", declaration.JE_GoodsOrigin);
			declaration.JE_RL_NKOrigin = "REAAA";
			AssertEquals("For FR territories we should have the code of the territory : reunion.", "RE", declaration.JE_GoodsOrigin);
			declaration.JE_RL_NKOrigin = "GFAAA";
			AssertEquals("For FR territories we should have the code of the territory : french guyana.", "GF", declaration.JE_GoodsOrigin);
			declaration.JE_RL_NKOrigin = "MFAAA";
			AssertEquals("For FR territories we should have the code of the territory : saint martin.", "MF", declaration.JE_GoodsOrigin);
			declaration.JE_RL_NKOrigin = "YTAAA";
			AssertEquals("For FR territories we should have the code of the territory : mayotte.", "YT", declaration.JE_GoodsOrigin);
			declaration.JE_RL_NKOrigin = "RS4SN";
			AssertEquals("For JE_RL_NKFinalDestination starting with RS we should have XS as configured in cusmap.", "XS", declaration.JE_GoodsOrigin);
			declaration.JE_RL_NKOrigin = "JESTH";
			AssertEquals("For JE_RL_NKFinalDestination starting with JE we should have GB as configured in cusmap.", "GB", declaration.JE_GoodsOrigin);
			declaration.JE_RL_NKOrigin = "AUSYD";
			AssertEquals("In general we should have the first two char.", "AU", declaration.JE_GoodsOrigin);
			declaration.JE_RL_NKOrigin = "GPAIF";
			AssertEquals("In general we should have the first two char.", "GP", declaration.JE_GoodsOrigin);
		}

		public void TestJE_AirRouteType()
		{
			AssertEquals("Lookups.AirRouteTypeList", Factory.New<JobDeclaration>().JE_AirRouteTypeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestJE_AirRouteTypeMaxLength()
		{
			AssertEquals(2, Factory.New<JobDeclaration>().JE_AirRouteTypeInfo.MaxLength);
		}

		public void TestOfficeOfDeclaration_BindToCAUOfficeCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep).CY_Data = "1001";
			AssertEquals("1001", declaration.OfficeOfDeclaration);
			declaration.OfficeOfDeclaration = "2002";
			AssertEquals("2002", declaration.CustomsOffices.Cast<EuOfficeCode>().Single(x => x.CY_Code == EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep).CY_Data);
		}

		public void TestEntryStyleCalculation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);

			var euctp = helper.CreateTradeGroup("EUN", "EUCTP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(euctp, "CH", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var eusft = helper.CreateTradeGroup("EUN", "EUSFT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(eusft, "GF", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var eusfr = helper.CreateTradeGroup("EUN", "EUSFR", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(eusfr, "GB", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			Factory.Save();

			var orgHeaderFR = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderFR.OH_RL_NKClosestPort = "FR";

			//An EU country without the special territories
			var orgHeaderDE = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderDE.OH_RL_NKClosestPort = "DE";

			//A country eligible to a common transit procedure
			var orgHeaderCH = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderCH.OH_RL_NKClosestPort = "CH";

			//An EU special territories
			var orgHeaderGF = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderGF.OH_RL_NKClosestPort = "GF";

			//An outside of EU country
			var orgHeaderUS = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderUS.OH_RL_NKClosestPort = "US";

			Factory.Save();

			var dec = Factory.New<JobDeclaration>();

			dec.SupplierDocumentaryAddress.Delete();
			dec.ImporterDocumentaryAddress.Delete();
			dec.JE_MessageType = Enterprise.Customs.EU.Business.MessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("FR", dec.JE_EntryStyle);

			dec.SupplierDocumentaryAddress.Delete();
			dec.SetupImporter(orgHeaderFR);
			AssertEquals("FR", dec.JE_EntryStyle);

			dec.SetupSupplier(orgHeaderDE);
			dec.SetupImporter(orgHeaderFR);
			AssertEquals("FR", dec.JE_EntryStyle);

			dec.SetupSupplier(orgHeaderCH);
			dec.SetupImporter(orgHeaderFR);
			AssertEquals("EU", dec.JE_EntryStyle);

			dec.SetupSupplier(orgHeaderFR);
			dec.SetupImporter(orgHeaderGF);
			AssertEquals("CO", dec.JE_EntryStyle);

			dec.SetupSupplier(orgHeaderUS);
			dec.SetupImporter(orgHeaderFR);
			AssertEquals("IM", dec.JE_EntryStyle);

			dec.SetupSupplier(orgHeaderFR);
			dec.ImporterDocumentaryAddress.Delete();
			AssertEquals("FR", dec.JE_EntryStyle);

			dec.SetupSupplier(orgHeaderFR);
			dec.SetupImporter(orgHeaderFR);
			AssertEquals("FR", dec.JE_EntryStyle);

			dec.SetupSupplier(orgHeaderGF);
			dec.SetupImporter(orgHeaderFR);
			AssertEquals("CO", dec.JE_EntryStyle);

			dec.SetupSupplier(orgHeaderFR);
			dec.SetupImporter(orgHeaderCH);
			AssertEquals("EU", dec.JE_EntryStyle);

			dec.SetupSupplier(orgHeaderFR);
			dec.SetupImporter(orgHeaderUS);
			AssertEquals("EX", dec.JE_EntryStyle);
		}

		public void TestIEntryStyleCalculatorFallbackInfoProviderOverrideMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			var fallbackInfoProvider = (EU.Business.Declaration.IEntryStyleCalculatorFallbackInfoProvider)declaration;
			AssertEquals("GetEntryStyleForInwardProcessingVATPayment", "FR", fallbackInfoProvider.GetEntryStyleForInwardProcessingVATPayment());
		}

		public override void TestLocalCurrencyCoreOverride()
		{
			var declaration = GetJobDeclarationForTesting();
			declaration.DisableDefaultPackingInformation = true;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			AssertEquals("Replace this with the correct currency code when implemented in a real country", Core.Constants.CurrencyCodes.France, declaration.LocalCurrencyCodeCoreExposed);
		}

		public override void TestAreMultipleEntryInstructionsAllowed()
		{
			var dec = Factory.New<JobDeclaration>();
			Assert(dec.AreMultipleEntryInstructionsAllowed);
		}

		public void TestSetDefaultValues_ShouldAddAnEntryWithSubStyleBlank()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals(1, dec.CustomsEntryInstructions.Count);
			AssertEquals(ZString.Empty, dec.CustomsEntryInstructions[0].CEI_SubStyle);
		}

		public override void TestGetCustomsEntryInstructionProviderCore()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertType<EntryInstructionProvider>(dec.CustomsEntryInstructionProvider);
		}

		public void TestCustomsEntryInstructions()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertType<CusEntryInstructionCollection>(dec.CustomsEntryInstructions);
		}

		public void TestIATALoadPort()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.France;
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(countryCode, "France", eun);

			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUIATA;
			helper.CreateNewOrGetExistingCusCodeType(codeType, "Airline Codes and Percentages for EU AIR freight calculation");

			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("PercentOutEU", "Desc.", codeType, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("PercentInEu", "Desc.", codeType, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("PercentDomestic", "Desc.", codeType, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("PercentOutEU", "Desc.", codeType, Core.Constants.CountryCodes.France);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("PercentInEu", "Desc.", codeType, Core.Constants.CountryCodes.France);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("PercentDomestic", "Desc.", codeType, Core.Constants.CountryCodes.France);

			var cusCode1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, codeType, "AAA", "aaaa", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCode1.Attributes.AddNew("PercentOutEU", "70");
			cusCode1.Attributes.AddNew("PercentInEu", "30");
			cusCode1.Attributes.AddNew("PercentDomestic", "0");

			var cusCode2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.France, codeType, "AAA", "aaaa", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Percentage", "Desc.", codeType, Core.Constants.CountryCodes.France);
			cusCode2.Attributes.AddNew("PercentOutEU", "80");
			cusCode2.Attributes.AddNew("PercentInEu", "10");

			var cusCode3 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, codeType, "BBB", "bbbb", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCode3.Attributes.AddNew("PercentOutEU", "70");
			cusCode3.Attributes.AddNew("PercentInEu", "30");

			var cusCode4 = helper.CreateCusCodeList(Core.Constants.CountryCodes.France, codeType, "BBB", "bbbb", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCode4.Attributes.AddNew("PercentOutEU", "80");
			cusCode4.Attributes.AddNew("PercentInEu", "15");
			cusCode4.Attributes.AddNew("PercentDomestic", "5");

			var cusCode5 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, codeType, "CCC", "cccc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCode5.Attributes.AddNew("PercentOutEU", "70");
			cusCode5.Attributes.AddNew("PercentInEu", "30");

			var cusCode6 = helper.CreateCusCodeList(Core.Constants.CountryCodes.France, codeType, "CCC", "cccc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCode6.Attributes.AddNew("PercentOutEU", "80");
			cusCode6.Attributes.AddNew("PercentInEu", "15");

			Factory.Save();

			var declaration = GetJobDeclaration() as JobDeclaration;
			declaration.JE_IATALoadPort = "";
			AssertNull(declaration.IATALoadPort);

			declaration.JE_IATALoadPort = "AAA";
			AssertEquals(cusCode1.PK, declaration.IATALoadPort.PK);

			declaration.JE_IATALoadPort = "BBB";
			AssertEquals(cusCode4.PK, declaration.IATALoadPort.PK);

			declaration.JE_IATALoadPort = "CCC";
			AssertNull(declaration.IATALoadPort);
		}

		public void TestOrganisationAuthorizationOwnerAndSiretCode()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.ZG_CTStatusID = ExportCommunityTransitStatusList.Codes.X;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			var testImporter = Factory.NewWithValidTestData<OrgHeader>();
			testImporter.OH_FullName = "Singapore Test Importer Pte. Ltd.";
			testImporter.MainAddress.OA_Address1 = "Changi Airport";
			testImporter.MainAddress.OA_Address2 = "Building 3C";
			testImporter.OH_Code = "TEST";
			testImporter.OH_RL_NKClosestPort = "FRPAR";

			var testSupplier = Factory.NewWithValidTestData<OrgHeader>();
			testSupplier.OH_FullName = "London Test Supplier DHL Ltd.";
			testSupplier.MainAddress.OA_Address1 = "Heathrow Airport";
			testSupplier.MainAddress.OA_Address2 = "Building 1B";
			testSupplier.OH_Code = "TEST";
			testSupplier.OH_RL_NKClosestPort = "AUSYD";

			AssertEquals("No CustomsRegistration Number of type SRT should been assigned to the declarant when importer has no SRT code itself.", ZString.Empty, declaration.SiretCode);

			var declarantAddress = Factory.New<OrgAddress>();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "London test Declarant Corp";
			declarantAddress.OA_OH = orgHeader.PK;
			declarantAddress.OA_Address1 = "Eugene Leroy Street ";
			declarantAddress.CompanyName = "test Declarant";

			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			declaration.JE_OH_Importer = testImporter.PK;
			declaration.JE_OH_Supplier = testSupplier.PK;

			declarantAddress.OA_OH = testImporter.PK;
			testImporter.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.Siret, "CODESRTTEST", Core.Constants.CountryCodes.France);

			declaration.JE_DeclarantType = RepresentationTypeList.Codes.SEL;

			AssertEquals("A CustomsRegistration Number of type SRT shoube have been assigned to the declarant because importer has such code itself.", "CODESRTTEST", declaration.SiretCode);
		}

		public void TestEntryCreationStrategyMergeKey()
		{
			var declaration = Factory.New<JobDeclaration>();

			var cei = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();

			var invoice = declaration.Invoices.AddNew();

			invoice.JZ_IncoTermPlace = "SHANGAI";

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = cei.PK;
			invoiceLine.JI_TariffBypassCode = "E";
			invoiceLine.JI_TariffBypassReason = "Unit Test";
			var cana1 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			cana1.CY_Code = "V902";
			var cana2 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			cana2.CY_Code = "V901";
			var creationStrat = declaration.CreateEntryCreationStrategy();

			AssertEquals(creationStrat.GetType(), typeof(EntryCreationStrategy));

			var key = creationStrat.GetKeyForLine(invoiceLine);
			Assert(key.Contains(invoiceLine.JI_TariffBypassCode));
			Assert(key.Contains(invoiceLine.JI_TariffBypassReason));
			Assert(creationStrat.GetKeyForLine(invoiceLine).Contains((ZString)"V902,V901"));
			Assert(creationStrat.GetKeyForLine(invoiceLine).Contains((ZString)"V901_V902"));

			var taxes = invoiceLine.Taxes.AddNew();

			taxes.JLT_MethodOfCalculation = "Test2";
			Assert(creationStrat.GetKeyForLine(invoiceLine).Contains(invoiceLine.Taxes[0].JLT_MethodOfCalculation));
		}

		public override void TestCustomsOfficeRequirementHelper()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<JobDeclarationCustomsOfficeRequirementHelper>(declaration.CustomsOfficeRequirementHelper);
		}

		[TestDate(2020, 6, 5)]
		public override void TestResetValuesOnTemplateCopyAfterClone()
		{
			base.TestResetValuesOnTemplateCopyAfterClone();

			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions[0];
			instruction.CEI_DateForDuty = new ZDateTime(2016, 09, 13);

			var clonedDeclaration = declaration.TemplateCopy() as JobDeclaration;
			var clonedInstruction = clonedDeclaration.CustomsEntryInstructions[0];

			AssertNotEquals("Reset CEI_DateForDuty to today", instruction.CEI_DateForDuty, clonedInstruction.CEI_DateForDuty);
			AssertEquals("Reset CEI_DateForDuty to today", new ZDateTime(2020, 6, 5), clonedInstruction.CEI_DateForDuty);
		}

		[TestDate(2020, 6, 5)]
		public void TestResetValuesOnTemplateCopyAfterCloneApplicationCode()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "IFT";
			Factory.Save();

			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, true))
			{
				var clonedDeclaration = declaration.TemplateCopy() as JobDeclaration;
				AssertEquals("Source application Code is wrong and Is Delta IE => clone declaration application code is DI", DeclarationApplicationCodeList.Codes.DeltaIE, clonedDeclaration.JE_ApplicationCode);
			}

			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, false))
			{
				var clonedDeclaration = declaration.TemplateCopy() as JobDeclaration;
				AssertEquals("Source application Code is wrong and Is Delta G => clone declaration application code is DG", DeclarationApplicationCodeList.Codes.DeltaG, clonedDeclaration.JE_ApplicationCode);
			}

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			Factory.Save();

			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, true))
			{
				var clonedDeclaration = declaration.TemplateCopy() as JobDeclaration;
				AssertEquals("Source application Code is good and Is Delta IE => clone declaration application code is DG", DeclarationApplicationCodeList.Codes.DeltaG, clonedDeclaration.JE_ApplicationCode);
			}
		}

		public void TestDeltaGAccountCode_Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(OrgCusAccountCodeList.Codes.DGE, declaration.DeltaGAccountCode);
		}

		public void TestDeltaGAccountCode_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(OrgCusAccountCodeList.Codes.DGI, declaration.DeltaGAccountCode);
		}

		public void TestDeltaGAccountCode_OtherTypes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("follow export", OrgCusAccountCodeList.Codes.DGE, declaration.DeltaGAccountCode);
		}

		public void TestDeltaGAuthorisationHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.SetupImporter();
			declaration.SetupSupplier();
			declaration.WithFlux(EU.Business.MessageTypeList.Codes.Import);
			AssertEquals(declaration.Importer, declaration.ActualClient);
			declaration.WithFlux(EU.Business.MessageTypeList.Codes.Export);
			AssertEquals(declaration.Supplier, declaration.ActualClient);
		}
		public void TestCorrelationIDData()
		{
			var dec1 = Factory.New<JobDeclaration>();
			var entryHeader1 = dec1.CustomsEntryHeaders.AddNew();

			var entryNum1 = Factory.New<CusEntryNumber>();
			entryNum1.CE_EntryNum = "1234567890";
			entryNum1.CE_EntryType = "LRN";
			entryNum1.Parent = entryHeader1;

			Factory.Save();

			AssertEquals("1234567890", dec1.CorrelationID.ToString());
			var dec2 = Factory.New<JobDeclaration>();
			var entryHeader2 = dec2.CustomsEntryHeaders.AddNew();

			var entryNum2 = Factory.New<CusEntryNumber>();
			entryNum2.CE_EntryNum = "1234567890";
			entryNum2.CE_EntryType = "LRN";
			entryNum2.Parent = entryHeader2;
			Factory.Save();
			var entryHeader3 = dec2.CustomsEntryHeaders.AddNew();

			var entryNum3 = Factory.New<CusEntryNumber>();
			entryNum3.CE_EntryNum = "9876543210";
			entryNum3.CE_EntryType = "LRN";
			entryNum3.Parent = entryHeader3;
			Factory.Save();
			AssertEquals("Multiple", dec2.CorrelationID.ToString());
		}

		public void TestFallbackEntryData()
		{
			var fallbackSetting = new FallbackSettings();
			fallbackSetting.End = ZDateTime.Empty;
			fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

			var declaration = Factory.New<JobDeclaration>();

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CustomsQuantity = 10;
			invoiceLine1.JI_Weight = 5;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CustomsQuantity = 10;
			invoiceLine2.JI_Weight = 5;

			Factory.Save();

			AssertEquals("", declaration.FallbackEntryNumber);
			AssertEquals("", declaration.FallbackEntryStatus);
			AssertEquals("", declaration.FallbackEntryDate);

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			cusEntryHeader.CH_BGMReference = "19212081311";

			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();
			invoiceLine1.JI_CL = cusEntryLine.PK;
			invoiceLine2.JI_CL = cusEntryLine.PK;

			Factory.Save();

			CusEntryNumber entryNum = Factory.New<CusEntryNumber>();
			entryNum.CE_EntryType = CusEntryHeader.Schema.FallbackEntryType;
			entryNum.CE_ParentID = cusEntryHeader.PK;
			entryNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			entryNum.CE_EntryStatus = "PPW";
			entryNum.CE_IssueDate = ZDateTime.Today;
			entryNum.CE_EntryNum = "1234567890";
			cusEntryHeader.InitFRCustomsFallbackEntryNumber();

			Factory.Save();

			AssertEquals("1234567890", declaration.FallbackEntryNumber);
			AssertEquals("PPW", declaration.FallbackEntryStatus);
			AssertEquals(ZDateTime.Today.ToString(DateTimeFormatStrings.ShortDateFormat), declaration.FallbackEntryDate);

			var cusEntryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader2.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			cusEntryHeader2.CH_BGMReference = "19212081312";

			var cusEntryLine2 = cusEntryHeader2.AllEntryLines.AddNew();
			invoiceLine1.JI_CL = cusEntryLine.PK;
			invoiceLine2.JI_CL = cusEntryLine.PK;

			Factory.Save();

			CusEntryNumber entryNum2 = Factory.New<CusEntryNumber>();
			entryNum2.CE_EntryType = CusEntryHeader.Schema.FallbackEntryType;
			entryNum2.CE_ParentID = cusEntryHeader2.PK;
			entryNum2.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			entryNum2.CE_EntryStatus = "PPS";
			entryNum2.CE_IssueDate = ZDateTime.Today.AddDays(1);
			entryNum2.CE_EntryNum = "0987654321";
			cusEntryHeader2.InitFRCustomsFallbackEntryNumber();
			Factory.Save();

			AssertEquals("Multiple", declaration.FallbackEntryNumber);
			AssertEquals("Multiple", declaration.FallbackEntryStatus);
			AssertEquals("Multiple", declaration.FallbackEntryDate);
		}

		public void TestAi2Permit()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Address1 = "Address1";
			orgAddress.OA_Address2 = "Address2";

			var guaranteeHeader1 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader1.CPH_Type = GuaranteeTypeList.Codes.AI2;
			guaranteeHeader1.CPH_Number = "0000001";
			guaranteeHeader1.CPH_OH_PermitHolder = orgHeader.PK;

			var guaranteeHeader2 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader2.CPH_Type = GuaranteeTypeList.Codes.AI2;
			guaranteeHeader2.CPH_Number = "0000002";
			guaranteeHeader2.CPH_OH_PermitHolder = orgHeader.PK;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			declaration.ZG_VATDeferNumber = "0000001";

			AssertNull(declaration.Ai2Permit);

			declaration.JE_OH_Importer = orgHeader.PK;

			var ai2Permit = declaration.Ai2Permit;
			AssertNotNull(declaration.Ai2Permit);
			AssertEquals(guaranteeHeader1.PK, ai2Permit.PK);

			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OA_DeclarantAddress = orgAddress.PK;

			ai2Permit = declaration.Ai2Permit;
			AssertNotNull(ai2Permit);
			AssertEquals(guaranteeHeader1.PK, ai2Permit.PK);

			declaration.ZG_VATDeferNumber = "0000002";

			ai2Permit = declaration.Ai2Permit;
			AssertNotNull(ai2Permit);
			AssertEquals(guaranteeHeader2.PK, ai2Permit.PK);

			declaration.ZG_VATDeferNumber = "0000003";

			AssertNull(declaration.Ai2Permit);

			declaration.ZG_VATDeferNumber = "0000002";
			declaration.ZG_VATDeferType = VATProcedureList.Codes.L;

			AssertNull(declaration.Ai2Permit);

			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			declaration.ZG_VATDeferNumber = "";
			AssertNull(declaration.Ai2Permit);
		}

		public void TestJE_CustomsProfileRelatedAccount()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusAccount = Factory.New<OrgCusAccount>();
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			orgCusAccount.CZ_Account = "TESTACC";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			CombineAssertions(() =>
			{
				AssertNull("JE_CustomsProfile Empty.", declaration.JE_CustomsProfileRelatedAccount);
				declaration.JE_CustomsProfile = "TESTACC";
				AssertNull("No Account was found for the Org.", declaration.JE_CustomsProfileRelatedAccount);
				orgCusAccount.CZ_OH = importer.PK;
				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
				AssertNull("No Account of correct type was found.", declaration.JE_CustomsProfileRelatedAccount);
				orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G2;
				AssertEquals("Account found.", orgCusAccount, declaration.JE_CustomsProfileRelatedAccount);
			});
		}

		public void TestDefaultDeltaGAccountOrgHeader()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes.SEL;
			AssertNotNull(declaration.Declarant);
			AssertEquals(null, declaration.ActualClient);
			AssertEquals("Declaration DefaultDeltaGAccountOrgHeader should fallback to Declarant organisation when Actual client is null.", declaration.Declarant.Header, declaration.DefaultDeltaAccountOrgHeader);

			declaration.JE_OH_Importer = importer.PK;
			AssertNotNull(declaration.ActualClient);
			AssertEquals("Declaration DefaultDeltaGAccountOrgHeader should return the actual client when available.", importer, declaration.DefaultDeltaAccountOrgHeader);
		}

		public void TestCustomsGuarantee()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.OH_FullName = "The declarant";
			declarant.MainAddress.Address1 = "DeclarantAddress";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;

			declaration.JE_MessageType = ZString.Empty;

			var guarantee1 = GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "GUAA", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "REFA", Core.Constants.CountryCodes.France);
			var guarantee2 = GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "GUAB", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "REFB", Core.Constants.CountryCodes.France);

			var guarantee3 = GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "GUAC", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "REF3", Core.Constants.CountryCodes.France);
			var guarantee4 = GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "GUAD", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "REFD", Core.Constants.CountryCodes.France);
			guarantee4.CPH_IsActive = ZBool.False;

			Factory.Save();
			Factory.ClearCachedValue<CusGuaranteeHeader[]>("FR.JobDeclarationLookups.CustomsGuarantees,G1,,,,DeclarantAddress,PST: 10 HUTCHESON STREET,PST: 10 HUTCHESON STREET");

			AssertNull(declaration.CustomsGuarantee);

			declaration.JE_CustomsGuaranteeNumber = "GUAA";
			AssertEquals(guarantee1, declaration.CustomsGuarantee);

			declaration.JE_CustomsGuaranteeNumber = "GUAB";
			AssertEquals(guarantee2, declaration.CustomsGuarantee);

			declaration.JE_CustomsGuaranteeNumber = "GUAC";
			AssertNull("not match because reference is not made of 4 uppercase letters", declaration.CustomsGuarantee);

			declaration.JE_CustomsGuaranteeNumber = "GUAD";
			AssertNull("not match because reference is not active", declaration.CustomsGuarantee);
		}

		public override void TestAutoRating()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = Core.Constants.CountryCodes.France;
			var eunId = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(currentCountry, parent: eunId);
			Factory.Save();

			var dut = helper.CreateNewOrGetExistingRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Duty, "Duty");
			var add = helper.CreateNewOrGetExistingRateType(currentCountry, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.AntiDumpingDuty, "Anti-dumping Duty");
			var interest = helper.CreateNewOrGetExistingRateType(currentCountry, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Interest, "Interest");
			dut.ZZR_IsPayable = true;
			add.ZZR_IsPayable = true;
			interest.ZZR_IsPayable = true;
			helper.LoadOrCreateNewCusRateCode(Factory, EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dut.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, add.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, EU.Business.UniversalReferenceConstants.RefCusRateCodes.CompensatoryInterestVat, interest.PK);
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_PaymentMethod = "A";
			dec.JE_DeclarantType = "IND";
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = dec.ActiveEntryHeaders.AddNew();

			var line = entry.MergedLines.AddNew();
			line.CL_CustomsPostedStatus = "ACT";
			line.Fees.AddOrUpdate(EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 57.25).CF_MethodOfPayment = "2";
			line.Fees.AddOrUpdate(EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, 22.44).CF_MethodOfPayment = "2";
			line.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 79.69).CF_MethodOfPayment = "2";
			line.Fees.AddOrUpdate(EU.Business.UniversalReferenceConstants.RefCusRateCodes.CompensatoryInterestVat, 5.86).CF_MethodOfPayment = "2";
			line.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 16.13).CF_MethodOfPayment = "2";

			entry.Charges.AddNew(EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 57.25).C1_MethodOfPayment = "2";

			Factory.Save();
			AssertAutoRateResult(dec, entry, 142m, 1); // Round(57.25 + 22.44 + 5.86 + 57.25) Include entry charge
		}

		public void TestIsDeltaTypeC()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			CombineAssertions(() =>
			{
				AssertEquals("G1", true, declaration.IsDeltaC);
				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
				AssertEquals("Others should be", false, declaration.IsDeltaC);
			});
		}

		public void TestIsDeltaTypeD()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			CombineAssertions(() =>
			{
				AssertEquals("G2", true, declaration.IsDeltaD);
				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
				AssertEquals("Others should be", false, declaration.IsDeltaD);
			});
		}

		public void TestJE_LocationOfGoodsWithSetterSuspender()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var jobDec = Factory.New<JobDeclaration>();
			jobDec.JE_SubLocationOfGoods = "SUB";
			jobDec.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;

			var authorisationHeader = orgHeader.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation);
			authorisationHeader.CPH_Number = "AAA";

			var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
			authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.OFC;
			authorisationRule.CPR_ValueFrom = "CCC";

			using (jobDec.SetterSuspender.SuspendSetting(JobDeclaration.Schema.JE_LocationOfGoods))
			{
				jobDec.JE_LocationOfGoods = "AAA";
				AssertNotEquals("JE_SubLocationOfGoods", "", jobDec.JE_SubLocationOfGoods);
				AssertNotEquals("JE_CustomsOffice", "CCC", jobDec.JE_CustomsOffice);
			}
		}

		public void TestCustomsProfileAndDeltaModeMatch()
		{
			var declaration = Factory.New<JobDeclaration>();
			var importer = declaration.SetupImporter();
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "DGI001", ZString.Empty, ZString.Empty, "D416421B");
			var declarant = declaration.SetupDeclarant().Header;
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_CustomsProfile = ZString.Empty;

			CombineAssertions(() =>
			{
				declaration.JE_CustomsProfile = "DGI001";
				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
				AssertEquals("Profile and Delta do not match", false, declaration.CustomsProfileAndDeltaModeMatch);

				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
				AssertEquals("Profile and Delta match declarant Account", true, declaration.CustomsProfileAndDeltaModeMatch);
			});
		}

		public void TestDeltaGFallbackAnnounced()
		{
			var declaration = Factory.New<JobDeclaration>();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, "Fallback");
			helper.CreateNewOrGetExistingCusCodeType("BOF", "BOF");

			AssertDeltaGFallbackAnnounced(Core.Constants.CountryCodes.France, "BOF", JobDeclaration.Schema.DeltaG, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			AssertDeltaGFallbackAnnounced(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, JobDeclaration.Schema.DeltaG, ZDate.Today.AddDays(-1), ZDateTime.Today.AddDays(2));

			AssertDeltaGFallbackAnnounced(Core.Constants.CountryCodes.France, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, JobDeclaration.Schema.DeclarationNumber, ZDate.Today.AddDays(-1), ZDateTime.Today.AddDays(2));

			AssertDeltaGFallbackAnnounced(Core.Constants.CountryCodes.France, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, JobDeclaration.Schema.DeltaG, ZDate.Today.AddDays(2), ZDate.Today.AddDays(3));

			AssertDeltaGFallbackAnnounced(Core.Constants.CountryCodes.France, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, JobDeclaration.Schema.DeltaG, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(-1));

			AssertDeltaGFallbackAnnounced(Core.Constants.CountryCodes.France, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, JobDeclaration.Schema.DeltaG, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), true);

			AssertDeltaGFallbackAnnounced(Core.Constants.CountryCodes.France, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, JobDeclaration.Schema.DeltaG, ZDate.Today.AddDays(1), ZDate.Today.AddDays(3), true);
		}

		public void AssertDeltaGFallbackAnnounced(string country, string codeType, string code, ZDateTime start, ZDateTime end, bool test = false)
		{
			CreateRefCusCodeList(codeType, country, code, start, end);
			AssertEquals(test, Factory.New<JobDeclaration>().DeltaGFallbackAnnounced);
		}

		public void TestDeltaGFallbackAnnouncedButNotActiveDeltaGFallbackIsAnnouncedFallBackNotActive()
		{
			var declaration = Factory.New<JobDeclaration>();
			var fallbackSetting = new FallbackSettings();
			fallbackSetting.End = ZDateTime.Today.AddDays(1);
			fallbackSetting.Start = ZDateTime.Today.AddDays(1);
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

			AssertEquals(false, FRCustomsDataRegistry.DeltaGFallbackIsActive);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, "Fallback");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, JobDeclaration.Schema.DeltaG, ZDate.Today.AddDays(-1), ZDateTime.Today.AddDays(2));
			Factory.Save();

			AssertEquals(true, declaration.DeltaGFallbackAnnounced);
			AssertEquals(true, declaration.DeltaGFallbackAnnouncedButNotActive);
		}
		public void TestDeltaGFallbackAnnouncedButNotActiveDeltaGFallbackIsAnnouncedFallBackActive()
		{
			var declaration = Factory.New<JobDeclaration>();
			var fallbackSetting = new FallbackSettings();
			fallbackSetting.End = ZDateTime.Empty;
			fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

			AssertEquals(true, FRCustomsDataRegistry.DeltaGFallbackIsActive);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, "Fallback");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, JobDeclaration.Schema.DeltaG, ZDate.Today.AddDays(-1), ZDateTime.Today.AddDays(2));
			Factory.Save();
			AssertEquals(true, declaration.DeltaGFallbackAnnounced);
			AssertEquals(false, declaration.DeltaGFallbackAnnouncedButNotActive);
		}

		public void TestDeltaGFallbackAnnouncedButNotActiveDeltaGFallbackIsNotAnnouncedFAllBackNotActive()
		{
			var declaration = Factory.New<JobDeclaration>();
			var fallbackSetting = new FallbackSettings();
			fallbackSetting.End = ZDateTime.Today.AddDays(1);
			fallbackSetting.Start = ZDateTime.Today.AddDays(1);
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

			AssertEquals(false, FRCustomsDataRegistry.DeltaGFallbackIsActive);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, "Fallback");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, JobDeclaration.Schema.DeltaG, ZDate.Today.AddDays(-1), ZDateTime.Today.AddDays(2));
			Factory.Save();
			AssertEquals(false, declaration.DeltaGFallbackAnnounced);
			AssertEquals(false, declaration.DeltaGFallbackAnnouncedButNotActive);
		}

		public void TestDeltaGFallbackAnnouncedButNotActiveDeltaGFallbackIsNotAnnouncedFAllBackIsActive()
		{
			var declaration = Factory.New<JobDeclaration>();
			var fallbackSetting = new FallbackSettings();
			fallbackSetting.End = ZDateTime.Empty;
			fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

			AssertEquals(true, FRCustomsDataRegistry.DeltaGFallbackIsActive);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, "Fallback");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, JobDeclaration.Schema.DeltaG, ZDate.Today.AddDays(-1), ZDateTime.Today.AddDays(2));
			Factory.Save();
			AssertEquals(false, declaration.DeltaGFallbackAnnounced);
			AssertEquals(false, declaration.DeltaGFallbackAnnouncedButNotActive);
		}

		public void TestIsInventorySelectionEnabled()
		{
			var warehouse = Factory.New<OrgHeader>();
			warehouse.CompanyData.OB_IMUsedBondedWhs = false;

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			entryInstruction.CEI_OA_Warehouse = warehouse.MainAddress.PK;
			AssertEquals("Inventory selection is disabled when there is no automated from warehouse.", false, declaration.IsInventorySelectionEnabled);

			warehouse.CompanyData.OB_IMUsedBondedWhs = true;
			AssertEquals("Inventory selection is disabled when there is any automated from warehouse.", true, declaration.IsInventorySelectionEnabled);
		}

		public override void TestProcedureProcessings()
		{
			using (CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testItem = GetJobDeclarationForTesting();
				AssertEquals("SupportInwardProcessing", true, testItem.SupportInwardProcessing);
				AssertEquals("SupportOutwardProcessing", false, testItem.SupportOutwardProcessing);
				AssertEquals("SupportsBondedWarehousingCore", true, testItem.SupportsBondedWarehousingCoreExposed);
			}
		}

		public void TestGetVariousOperationCreditNumber()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.OH_FullName = "The declarant";
			declarant.MainAddress.Address1 = "DeclarantAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "DGUA", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "REFA", Core.Constants.CountryCodes.France);
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "DGUB", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "REFB", Core.Constants.CountryCodes.France);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_MessageType = ZString.Empty;

			declaration.JE_CustomsGuaranteeNumber = ZString.Empty;
			AssertNull(declaration.CustomsGuarantee);
			AssertEquals(ZString.Empty, declaration.GetVariousOperationCreditNumber());

			declaration.JE_CustomsGuaranteeNumber = "DGUA";
			AssertEquals("REFA", declaration.GetVariousOperationCreditNumber());

			declaration.JE_CustomsGuaranteeNumber = "DGUB";
			AssertEquals("REFB", declaration.GetVariousOperationCreditNumber());
		}

		public void TestDocumentSupport()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			AssertType<JobDeclarationDocumentSupporter>(declaration.DocumentSupporter);
		}

		public void TestIsG2WithMixedStatusEntries()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			var entry1 = declaration.ActiveEntryHeaders.AddNew();
			var entry2 = declaration.ActiveEntryHeaders.AddNew();
			entry1.CH_EntryStatus = "100";
			entry2.CH_EntryStatus = "130";
			Assert(declaration.IsG2WithMixedStatusEntries);

			entry2.CH_EntryStatus = "100";
			Assert(!declaration.IsG2WithMixedStatusEntries);

			entry2.CH_EntryStatus = "130";
			Assert(declaration.IsG2WithMixedStatusEntries);

			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			Assert(!declaration.IsG2WithMixedStatusEntries);

			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			Assert(declaration.IsG2WithMixedStatusEntries);

			entry1.CH_EntryStatus = "100";
			entry2.CH_EntryStatus = "100";
			Assert(!declaration.IsG2WithMixedStatusEntries);

			entry2.CH_EntryStatus = "130";
			Assert(declaration.IsG2WithMixedStatusEntries);
		}

		public void TestFilteredInvoiceLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<EU.Business.Declaration.InvoiceLineViewCollection<JobComInvoiceLine>>(declaration.FilteredInvoiceLines);
		}

		public void TestApplicationExtenderChangeWithJE_ApplicationCode()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			CombineAssertions(() =>
			{
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
				AssertType<DeltaGApplicationExtender>(declaration.ApplicationExtender);
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				AssertType<DeltaIEApplicationExtender>(declaration.ApplicationExtender);
			});
		}

		public void TestGetValueSetStrategyChangeWithJE_ApplicationCode()
		{
			var declaration = GetJobDeclarationForTesting();
			CombineAssertions(() =>
			{
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
				AssertType<DeltaGJobDeclarationValueSetStrategy>(declaration.GetValueSetStrategyExposed());
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				AssertType<DeltaIEJobDeclarationValueSetStrategy>(declaration.GetValueSetStrategyExposed());
			});
		}

		public void TestTypeOfJobDeclarationDeepCloneStrategy()
		{
			var dec = Factory.New<JobDeclarationForTesting>();
			AssertType<JobDeclarationDeepCloneStrategy>("The Application Extender should be of the right type after cloning", dec.GetCloneStrategy());
		}

		public void TestGetNewValidation()
		{
			var declaration = Factory.New<JobDeclarationForTesting>();
			CombineAssertions(() =>
			{
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
				AssertType<DeltaGJobDeclarationValidation>(declaration.Validation);
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				AssertType<DeltaIEJobDeclarationValidation>(declaration.Validation);
			});
		}

		public void TestEntryExitedStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			var resStringData = declaration.EntryExitedStatusInfo.GetAttribute<ResourceStringDataAttribute>();
			CombineAssertions(() =>
			{
				AssertEquals("ECS Status", resStringData.ShortCaption);
				AssertEquals("Export Control Status", resStringData.Caption);
				AssertEquals("empty when no entry header linked.", "", declaration.EntryExitedStatus);
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_ExitedStatus = "SOR";
				AssertEquals("from single entry header.", "SOR", declaration.EntryExitedStatus);
				var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader1.CH_ExitedStatus = "SOR";
				AssertEquals("from entry headers with all SOR", "SOR", declaration.EntryExitedStatus);
				var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader2.CH_ExitedStatus = "ENF";
				AssertEquals("Multiple", declaration.EntryExitedStatus);
			});
		}

		public void TestIsDeltaIEEnable()
		{
			CombineAssertions(() =>
			{
				AssertIsDeltaIEEnable(true, EU.Business.MessageTypeList.Codes.Import, true, false);
				AssertIsDeltaIEEnable(true, EU.Business.MessageTypeList.Codes.Import, true, true);
				AssertIsDeltaIEEnable(false, EU.Business.MessageTypeList.Codes.Import, false, true);
				AssertIsDeltaIEEnable(false, EU.Business.MessageTypeList.Codes.Import, false, false);

				AssertIsDeltaIEEnable(true, EU.Business.MessageTypeList.Codes.Export, false, true);
				AssertIsDeltaIEEnable(true, EU.Business.MessageTypeList.Codes.Export, false, true);
				AssertIsDeltaIEEnable(false, EU.Business.MessageTypeList.Codes.Export, false, false);
				AssertIsDeltaIEEnable(false, EU.Business.MessageTypeList.Codes.Export, true, false);
			});
		}

		public void TestJE_DeclarationLanguage()
		{
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				AssertEquals("JE_DeclarationLanguage Caption", "Language", DataBoundResourceStrings.GetDataForProperty(declaration.JE_DeclarationLanguageInfo).Caption);
				AssertEquals("JE_DeclarationLanguage ShortCaption", "Lang.", DataBoundResourceStrings.GetDataForProperty(declaration.JE_DeclarationLanguageInfo).ShortCaption);
				AssertEquals("JE_DeclarationLanguage ReadOnly", true, declaration.JE_DeclarationLanguageInfo.ReadOnly);
			});
		}

		public void TestJE_ApplicationCodeReadOnly_Import_Delta()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, false))
				{
					Factory.InvalidateCachedProperties();
					AssertEquals("Should be readonly when DeltaIE is disable", true, declaration.JE_ApplicationCodeInfo.ReadOnly);
				}
				using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, true))
				{
					Factory.InvalidateCachedProperties();
					AssertEquals("Should not be readonly when DeltaIE is enable", false, declaration.JE_ApplicationCodeInfo.ReadOnly);
				}
			}
		}

		public void TestJE_ApplicationCodeReadOnly_Export_Delta()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				using (FRCustomsDataRegistry.Instance.EnableDeltaIEForExports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, false))
				{
					Factory.InvalidateCachedProperties();
					AssertEquals("Should be readonly when DeltaIE is disable", true, declaration.JE_ApplicationCodeInfo.ReadOnly);
				}
				using (FRCustomsDataRegistry.Instance.EnableDeltaIEForExports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, true))
				{
					Factory.InvalidateCachedProperties();
					AssertEquals("Should not be readonly when DeltaIE is enable", false, declaration.JE_ApplicationCodeInfo.ReadOnly);
				}
			}
		}

		void AssertIsDeltaIEEnable(bool expectReadonly, ZString messageType, bool enableDeltaIEForImports, bool enableDeltaIEForExports)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;

			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, enableDeltaIEForImports))
			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForExports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, enableDeltaIEForExports))
			{
				AssertEquals($"messageType: {messageType}, enableDeltaIEForImports: {enableDeltaIEForImports}, enableDeltaIEForExports: {enableDeltaIEForExports}", expectReadonly, declaration.IsDeltaIEEnable);
			}
		}

		public void TestIsDeltaIEEnableCompanyLevel()
		{
			CombineAssertions(() =>
			{
				AssertIsDeltaIEEnableCompanyLevel(true, EU.Business.MessageTypeList.Codes.Import, true, false);
				AssertIsDeltaIEEnableCompanyLevel(true, EU.Business.MessageTypeList.Codes.Import, true, true);
				AssertIsDeltaIEEnableCompanyLevel(false, EU.Business.MessageTypeList.Codes.Import, false, true);
				AssertIsDeltaIEEnableCompanyLevel(false, EU.Business.MessageTypeList.Codes.Import, false, false);

				AssertIsDeltaIEEnableCompanyLevel(true, EU.Business.MessageTypeList.Codes.Export, false, true);
				AssertIsDeltaIEEnableCompanyLevel(true, EU.Business.MessageTypeList.Codes.Export, false, true);
				AssertIsDeltaIEEnableCompanyLevel(false, EU.Business.MessageTypeList.Codes.Export, false, false);
				AssertIsDeltaIEEnableCompanyLevel(false, EU.Business.MessageTypeList.Codes.Export, true, false);
			});
		}

		void AssertIsDeltaIEEnableCompanyLevel(bool expectReadonly, ZString messageType, bool enableDeltaIEForImports, bool enableDeltaIEForExports)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;

			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, enableDeltaIEForImports))
			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForExports.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, enableDeltaIEForExports))
			{
				AssertEquals($"messageType: {messageType}, enableDeltaIEForImports: {enableDeltaIEForImports}, enableDeltaIEForExports: {enableDeltaIEForExports}", expectReadonly, declaration.IsDeltaIEEnable);
			}
		}

		public void TestJE_ApplicationCodeReadonly_DG()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Builtin;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
				var entry = declaration.ActiveEntryHeaders.AddNew();
				using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				{
					Factory.InvalidateCachedProperties();
					AssertEquals("EnableDeltaIEForImports:false - no message", true, declaration.JE_ApplicationCodeInfo.ReadOnly);
				}
				using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					Factory.InvalidateCachedProperties();
					AssertEquals("EnableDeltaIEForImports:true - no message", false, declaration.JE_ApplicationCodeInfo.ReadOnly);
				}

				entry.Messages.AddNew();
				using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				{
					Factory.InvalidateCachedProperties();
					AssertEquals("EnableDeltaIEForImports:false - message", true, declaration.JE_ApplicationCodeInfo.ReadOnly);
				}
				using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					Factory.InvalidateCachedProperties();
					AssertEquals("EnableDeltaIEForImports:false - message", true, declaration.JE_ApplicationCodeInfo.ReadOnly);
				}
			}
		}

		public void TestJE_ApplicationCodeReadonly_DI()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Builtin;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				var entry = declaration.ActiveEntryHeaders.AddNew();
				using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				{
					Factory.InvalidateCachedProperties();
					AssertEquals("EnableDeltaIEForImports:false - no message", true, declaration.JE_ApplicationCodeInfo.ReadOnly);
				}
				using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					Factory.InvalidateCachedProperties();
					AssertEquals("EnableDeltaIEForImports:true - no message", false, declaration.JE_ApplicationCodeInfo.ReadOnly);
				}
				entry.Messages.AddNew();
				using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				{
					Factory.InvalidateCachedProperties();
					AssertEquals("EnableDeltaIEForImports:false - message", true, declaration.JE_ApplicationCodeInfo.ReadOnly);
				}
				using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					Factory.InvalidateCachedProperties();
					AssertEquals("EnableDeltaIEForImports:false - message", true, declaration.JE_ApplicationCodeInfo.ReadOnly);
				}
			}
		}

		public void TestAssessmentDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryinstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryinstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryheader1 = declaration.ActiveEntryHeaders.AddNew();
			entryheader1.CH_CEI_Instruction = entryinstruction1.PK;
			var entryheader2 = declaration.ActiveEntryHeaders.AddNew();
			entryheader2.CH_CEI_Instruction = entryinstruction2.PK;
			var entryheader3 = declaration.ActiveEntryHeaders.AddNew();

			entryinstruction1.CEI_DateForDuty = new ZDateTime(1986, 08, 22);
			entryinstruction2.CEI_DateForDuty = new ZDateTime(1986, 08, 22);

			AssertEquals("Declaration AssessmentDate should be 22/08/1986 as the 2 entryinstructions linked to the entry header have the same CEI_DateForDuty.Date.", "22/08/1986", declaration.AssessmentDate);

			entryinstruction1.CEI_DateForDuty = new ZDateTime(1986, 08, 22, 09, 05, 10);
			entryinstruction2.CEI_DateForDuty = new ZDateTime(1986, 08, 22, 10, 05, 10);

			AssertEquals("Declaration AssessmentDate should be 22/08/1986 as the 2 entryinstructions linked to the entry header have the same CEI_DateForDuty.Date.", "22/08/1986", declaration.AssessmentDate);

			entryinstruction1.CEI_DateForDuty = new ZDateTime(1986, 08, 23);
			entryinstruction2.CEI_DateForDuty = new ZDateTime(1986, 08, 22);

			AssertEquals("Declaration AssessmentDate should be MLT as the 2 entryinstructions linked to the entry header have different CEI_DateForDuty.Date.", "MLT", declaration.AssessmentDate);
		}

		public void TestIsDeclarationIntegrated()
		{
			var declaration = base.Factory.New<JobDeclaration>();
			CustomsDataRegistry.Instance.CustomsWareCompany.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "TEST");
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			Assertion.Assert("IsDeclarationIntegrated should be false as application code is not ITF (Delta G here).", !declaration.IsDeclarationIntegrated);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			Assertion.Assert("IsDeclarationIntegrated should be false as application code is not ITF (Delta IE here).", !declaration.IsDeclarationIntegrated);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interface;
			Assertion.Assert("IsDeclarationIntegrated should be true as application code is equal to ITF.", declaration.IsDeclarationIntegrated);
		}

		public void TestShowSubmitMenuItem()
		{
			var declaration = base.Factory.New<JobDeclaration>();
			CustomsDataRegistry.Instance.CustomsWareCompany.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "TEST");
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			Assertion.Assert("ShowSubmitMenuItem should be false as application code is not ITF (Delta G here).", !declaration.ShowSubmitMenuItem);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			Assertion.Assert("ShowSubmitMenuItem should be false as application code is not ITF (Delta IE here).", !declaration.ShowSubmitMenuItem);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interface;
			Assertion.Assert("ShowSubmitMenuItem should be true as application code is equal to ITF.", declaration.ShowSubmitMenuItem);
		}

		public void TestApplicationCodeIsCopiedAfterClone()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interface;
			var clonedDeclaration = declaration.TemplateCopy() as JobDeclaration;

			AssertEquals("ITF value in application code should have been copied", DeclarationApplicationCodeList.Codes.Interface, clonedDeclaration.JE_ApplicationCode);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var clonedDeclaration2 = declaration.TemplateCopy() as JobDeclaration;
			AssertEquals("DeltaG value in application code should have been copied", DeclarationApplicationCodeList.Codes.DeltaG, clonedDeclaration2.JE_ApplicationCode);
		}

		public void TestCustomsLastEntryStatusDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryheader1 = declaration.ActiveEntryHeaders.AddNew();
			var entryheader2 = declaration.ActiveEntryHeaders.AddNew();
			entryheader1.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			entryheader2.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES050;
			var firstDate = new ZDateTime(2022, 10, 22);
			var lastDate = new ZDateTime(2022, 11, 22);

			var msg = Factory.New<DeltaCImportFREDIMessage>();
			msg.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			msg.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			msg.EM_SystemCreateTimeUtc = firstDate;
			entryheader1.Messages.Add(msg);

			AssertEquals("CustomsLastEntryStatusDate should be empty as declaration ActiveEntryHeaders have different status.", ZDateTime.Empty, declaration.CustomsLastEntryStatusDate);

			var msg2 = Factory.New<DeltaCImportFREDIMessage>();
			msg2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			msg2.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			msg2.EM_SystemCreateTimeUtc = lastDate;
			entryheader2.Messages.Add(msg2);

			entryheader2.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			AssertEquals("CustomsLastEntryStatusDate should be not empty as declaration ActiveEntryHeaders have same status.", lastDate.ToLocalBranchTime(), declaration.CustomsLastEntryStatusDate);
		}

		public void TestZG_AgreedPlaceCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(String.Empty, DataBoundResourceStrings.GetDataForProperty(declaration.ZG_AgreedPlaceCodeInfo).FullDescription);
		}

		public void TestTriggeringPointForValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("TriggeringPointForValidation should be Empty as there is no entries.", ZString.Empty, declaration.TriggeringPointForValidation);

			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew(typeof(CusEntryHeader)) as CusEntryHeader;
			var cusEntryHeader2 = declaration.ActiveEntryHeaders.AddNew(typeof(CusEntryHeader)) as CusEntryHeader;
			cusEntryHeader.CH_TriggeringPointForValidation = "VAA";
			cusEntryHeader2.CH_TriggeringPointForValidation = "VAA";

			AssertEquals("TriggeringPointForValidation should be equal VAA as CH_TriggeringPointForValidation value is VAA for the 2 entries.", "VAA", declaration.TriggeringPointForValidation);

			cusEntryHeader2.CH_TriggeringPointForValidation = "OTH";
			AssertEquals("TriggeringPointForValidation should be equal Multiple as CH_TriggeringPointForValidation values are different between the 2 entries(1 is OTH).", "Multiple", declaration.TriggeringPointForValidation);

			cusEntryHeader2.CH_TriggeringPointForValidation = ZString.Empty;
			AssertEquals("TriggeringPointForValidation should be equal Multiple as CH_TriggeringPointForValidation values are different between the 2 entries (1 is empty).", "Multiple", declaration.TriggeringPointForValidation);
		}

		public void TestInventorySelectionHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<InventorySelectionHeader>(declaration.InventorySelectionHeader);
		}

		public void TestJE_OwnerRefInfoMaxLength()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals("Maximum length of JE_OwnerRef", 35, dec.JE_OwnerRefInfo.MaxLength);
		}

		public void TestJE_CustomsGuaranteeNumberMaxLength()
		{
			AssertEquals(35, Factory.New<JobDeclaration>().JE_CustomsGuaranteeNumberInfo.MaxLength);
		}

		public void TestJE_RegionOrTerritoryOfDestination()
		{
			AssertEquals("Lookups.RegionOrTerritoryOfDestinationList", Factory.New<JobDeclaration>().JE_RegionOrTerritoryOfDestinationInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestJE_TransportModeInland()
		{
			var declaration = Factory.New<JobDeclaration>().WithFlux(EUJobMessageTypeList.Codes.Import);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			declaration.JE_TransportMeans = string.Empty;
			declaration.JE_TransportModeInland = Core.Constants.TransportModes.Air;
			AssertEquals("JE_TransportMeans should not be set on JE_TransportModeInland change.", string.Empty, declaration.JE_TransportMeans);

			declaration.JE_TransportMeans = "99";
			declaration.JE_TransportModeInland = Core.Constants.TransportModes.Sea;
			AssertEquals("JE_TransportMeans should not be altered on JE_TransportModeInland change.", "99", declaration.JE_TransportMeans);
		}

		public void TestJE_ExportExitTypeReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "FRXXXXX";
			var officeOfExit = declaration.CustomsOffices.Cast<EuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfExit);
			officeOfExit.CY_Data = "FRXXXXX";
			Factory.Save();
			AssertEquals("Exit type should not be read only when office of exit matches the office of lodgement.", ZBool.False, declaration.JE_ExportExitTypeInfo.ReadOnly);
			officeOfExit.CY_Data = "FRYYYYY";
			Factory.Save();
			AssertEquals("Exit type should be read only when office of exit doesn't match the office of lodgement.", ZBool.True, declaration.JE_ExportExitTypeInfo.ReadOnly);
		}

		public void TestTemplateCopy_TariffTypeIsSetToImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			Factory.Save();
			AssertEquals("JE_TariffType should be set to IMP by default on a french declaration.", CusTariffTypes.ImportTariff, declaration.JE_TariffType);

			var clonedDeclaration = declaration.TemplateCopy().As<JobDeclaration>();
			AssertEquals("JE_TariffType is set to IMP on the copy when copying a french declaration.", CusTariffTypes.ImportTariff, clonedDeclaration.JE_TariffType);

			declaration.JE_TariffType = ZString.Empty;
			Factory.Save();
			AssertEquals(ZString.Empty, declaration.JE_TariffType);

			clonedDeclaration = declaration.TemplateCopy().As<JobDeclaration>();
			AssertEquals("JE_TariffType on the source declaration is still empty.", ZString.Empty, declaration.JE_TariffType);
			AssertEquals("JE_TariffType is set to IMP on the copy, even when it is empty in the source.", CusTariffTypes.ImportTariff, clonedDeclaration.JE_TariffType);
		}

		public void TestHasSimplifiedEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("HasSimplifiedEntry should be false if there is no entryInstruction al all.", false, declaration.HasSimplifiedEntry);

			var entryinstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryinstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryinstruction1.CEI_Style = "H1";
			entryinstruction1.CEI_SubStyle = "A";
			entryinstruction2.CEI_Style = "H2";
			entryinstruction2.CEI_SubStyle = "C";
			AssertEquals("Prerequisite: entry instruction is not simplified.", false, entryinstruction1.IsSimplified);
			AssertEquals("Prerequisite: entry instruction is not simplified.", false, entryinstruction2.IsSimplified);
			AssertEquals("HasSimplifiedEntry should be false if none of the entryInstruction is IsSimplified.", false, declaration.HasSimplifiedEntry);

			entryinstruction2.CEI_Style = "I1";
			AssertEquals("Prerequisite: entry instruction is not simplified.", false, entryinstruction1.IsSimplified);
			AssertEquals("Prerequisite: entry instruction is simplified.", true, entryinstruction2.IsSimplified);
			AssertEquals("HasSimplifiedEntry should be true if any one of the entryInstruction is IsSimplified.", true, declaration.HasSimplifiedEntry);
		}

		public override void TestSupportsCalculateInsurance()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertEquals("The default value should be false for export", false, declaration.SupportsCalculateInsurance);
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				AssertEquals("The default value should be false for import ucc6", false, declaration.SupportsCalculateInsurance);
			}
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC5Core", true))
			{
				AssertEquals("The default value should be false for import ucc5", false, declaration.SupportsCalculateInsurance);
			}
		}

		public void TestIsAllocatedQuantityRequiredForBondedWarehouse()
		{
			using (CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = GetJobDeclarationForTesting();
				var instruction = Factory.New<CusEntryInstruction>();
				declaration.CustomsEntryInstructions.Add(instruction);

				CombineAssertions("TestIsAllocatedQuantityRequiredForBondedWarehouse in DeltaIE", () =>
				{
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
					AssertEquals("SupportInwardProcessing", true, declaration.SupportInwardProcessing);
					AssertEquals("IsEntryStyleOutOfInward is always false in DeltaIE", false, instruction.IsEntryStyleOutOfInward);
					AssertEquals("IsAllocatedQuantityRequiredForBondedWarehouse is false when either SupportInwardProcessing or IsEntryStyleOutOfInward is false", false, declaration.IsAllocatedQuantityRequiredForBondedWarehouseExposed);
				});

				CombineAssertions("TestIsAllocatedQuantityRequiredForBondedWarehouse in DeltaG", () =>
				{
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
					AssertEquals("SupportInwardProcessing", true, declaration.SupportInwardProcessing);
					AssertEquals("IsEntryStyleOutOfInward is false when EntryInstruction is not 31P", false, instruction.IsEntryStyleOutOfInward);
					AssertEquals("IsAllocatedQuantityRequiredForBondedWarehouse is false when either SupportInwardProcessing or IsEntryStyleOutOfInward is false", false, declaration.IsAllocatedQuantityRequiredForBondedWarehouseExposed);

					instruction.CEI_Style = DeltaGExportDeclarationTypeList.Codes.ReExportOfNonUnionGoodsWithEI;
					AssertEquals("IsEntryStyleOutOfInward is true when EntryInstruction is 31P", true, instruction.IsEntryStyleOutOfInward);
					AssertEquals("IsAllocatedQuantityRequiredForBondedWarehouse is true when both SupportInwardProcessing and IsEntryStyleOutOfInward are true", true, declaration.IsAllocatedQuantityRequiredForBondedWarehouseExposed);
				});
			}
		}

		public void TestIsBondedWhsQuantityRequiredForBondedWarehouse()
		{
			using (CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = GetJobDeclarationForTesting();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
				var instruction = Factory.New<CusEntryInstruction>();
				declaration.CustomsEntryInstructions.Add(instruction);

				AssertEquals("IsBondedWhsQuantityRequiredForBondedWarehouse is true when IsAllocatedQuantityRequiredForBondedWarehouse is false", true, declaration.IsBondedWhsQuantityRequiredForBondedWarehouseExposed);

				instruction.CEI_Style = DeltaGExportDeclarationTypeList.Codes.ReExportOfNonUnionGoodsWithEI;
				AssertEquals("IsBondedWhsQuantityRequiredForBondedWarehouse is false when IsAllocatedQuantityRequiredForBondedWarehouse is true", false, declaration.IsBondedWhsQuantityRequiredForBondedWarehouseExposed);
			}
		}

		public void TestIsDeclarationStandard()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("When the declaration does not have an E0001 additionalInfo, IsDeclarationStandard should be false.", false, declaration.IsDeclarationStandard);

			var additionalInfo = declaration.AdditionalInfos.AddNew();
			additionalInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.StandardDeclaration;
			AssertEquals("When the declaration has an E0001 additionalInfo, IsDeclarationStandard should be true.", true, declaration.IsDeclarationStandard);
		}

		public void TestShouldDefaultContainerModeAndIsContainerised()
		{
			var declaration = Factory.New<JobDeclarationForTesting>();
			AssertEquals("ShouldDefaultContainerModeAndIsContainerised should return true when containerMode is containerised.", true, declaration.ShouldDefaultContainerModeAndIsContainerisedExposed(Core.Constants.ContainerModes.FCL));
			AssertEquals("ShouldDefaultContainerModeAndIsContainerised should return false when containerMode is not containerised.", false, declaration.ShouldDefaultContainerModeAndIsContainerisedExposed("Tes"));
		}

		protected override Type ExpectedDeclarationLevelPackageCollectionType => typeof(BaseDeclarationLevelPackageCollection<Package>);

		protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

		JobDeclarationForTesting GetJobDeclarationForTesting()
		{
			var dec = Factory.New<JobDeclarationForTesting>();

			dec.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			dec.ActiveEntryHeaders.AddNew().CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			dec.ActiveEntryHeaders.AddNew().CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES130;

			return dec;
		}

		void SetUpRefUNLOCO()
		{
			CreateNewOrGetExistingRefUNLOCO("USATL");
			var state1 = CreateNewOrGetExistingRefCountryStates("2A", "Corse", "FR");
			CreateNewOrGetExistingRefUNLOCO("FR2AC").RL_RW = state1.PK;
			var state2 = CreateNewOrGetExistingRefCountryStates("2B", "Haute-Corse", "FR");
			CreateNewOrGetExistingRefUNLOCO("FR2BC").RL_RW = state2.PK;
			CreateNewOrGetExistingRefUNLOCO("FRPAR");
			CreateNewOrGetExistingRefUNLOCO("MQAAA");
			CreateNewOrGetExistingRefUNLOCO("GPAAA");
			CreateNewOrGetExistingRefUNLOCO("REAAA");
			CreateNewOrGetExistingRefUNLOCO("GFAAA");
			CreateNewOrGetExistingRefUNLOCO("YTAAA");
			CreateNewOrGetExistingRefUNLOCO("MFAAA");
		}

		RefCountryStates CreateNewOrGetExistingRefCountryStates(ZString code, ZString description, ZString countryCode)
		{
			var result = new RefCountryStates.Loader(Factory).LoadRefCountryStatesFromCode(code, countryCode);
			if (result == null)
			{
				result = Factory.NewWithValidTestData<RefCountryStates>();
				result.RW_Code = code;
				result.RW_Description = description;
				result.RW_RN_NKCountryCode = countryCode;
			}
			return result;
		}

		RefUNLOCO CreateNewOrGetExistingRefUNLOCO(ZString code)
		{
			var result = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, code);
			if (result == null)
			{
				result = Factory.New<RefUNLOCO>();
				result.RL_Code = code;
			}
			return result;
		}

		ZZRefCusCodeListCombined CreateRefCusCodeList(string type, string countryCode, string code, ZDateTime startDate, ZDateTime endDate)
		{
			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.FillWithValidTestData();
			cusCodeList.ZZD_CodeType = type;
			cusCodeList.ZZD_CountryOrGrouping = countryCode;
			cusCodeList.ZZD_Code = code;
			cusCodeList.ZZD_EndDate = endDate;
			cusCodeList.ZZD_StartDate = startDate;
			return cusCodeList;
		}

		sealed class JobDeclarationForTesting : JobDeclaration
		{
			public JobDeclarationForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZString LocalCurrencyCodeCoreExposed
			{
				get { return LocalCurrencyCodeCore; }
			}

			public bool SupportsBondedWarehousingCoreExposed => SupportsBondedWarehousingCore;

			public new string GetIApportionInvoiceHolderCountryContextCore() => base.GetIApportionInvoiceHolderCountryContextCore();

			public IValueSetStrategy GetValueSetStrategyExposed() => GetValueSetStrategy();

			public Customs.Business.JobDeclarationDeepCloneStrategy GetCloneStrategy()
			{
				return GetTemplateCopyStrategy(new BusinessObjectFactory(), CloneType.TemplateCopy);
			}

			public bool IsIntegrationWithAccountingSupportedExposed => IsIntegrationWithAccountingSupported;

			public bool IsBondedWhsQuantityRequiredForBondedWarehouseExposed => IsBondedWhsQuantityRequiredForBondedWarehouse;

			public bool IsAllocatedQuantityRequiredForBondedWarehouseExposed => IsAllocatedQuantityRequiredForBondedWarehouse;

			public bool ShouldDefaultContainerModeAndIsContainerisedExposed(ZString containerMode) => ShouldDefaultContainerModeAndIsContainerisedCore(containerMode);
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
