using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BR.Business.Testing;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.BR.Business.ImportSiscomex.Testing
{
	class ImportSiscomexProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationImportProvider()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "C2";
			var brAddinfo = BROrgImpAddInfo.Get(orgHeader);
			brAddinfo.ZO_BankCode = "XXX";
			brAddinfo.ZO_BSBNumber = "111";
			brAddinfo.ZO_AccountNumber = "777";

			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_FullAccountNumber = "ZZZ";
			bankAccount.AB_BSB = "123";
			bankAccount.AB_AccountNum = "654";
			bankAccount.AB_Code = "1";

			using (BRCustomsDataRegistry.Instance.TaxFeeCustomsPaymentBankAccount.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, bankAccount.PK.ToGuid()))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateCusMapType(RefCusMapTypeList.Codes.Country, "BTH", "Country Codes Mapping", true);
				helper.CreateCusMap(RefCusMapTypeList.Codes.Country, "AU", "072", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), Core.Constants.CountryCodes.Brazil);
				helper.CreateCusMap(RefCusMapTypeList.Codes.Country, "001", "074", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), Core.Constants.CountryCodes.Brazil);

				helper.CreateCusMapType(Constants.RefCusMapType.ModalTransport, "BTH", "Transport Modes Mapping", true);
				helper.CreateCusMap(Constants.RefCusMapType.ModalTransport, "SEA", "01", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), Core.Constants.CountryCodes.Brazil);

				var forwarder = Factory.NewWithValidTestData<OrgHeader>();
				forwarder.PrimaryRegistrationNumber.Number = "00000000000000";

				ReferenceTestDataHelper.CreateSiscomexUsageEntryFees(Factory);
				ReferenceTestDataHelper.CreateTaxRevenueTypeList(Factory);

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				declaration.JE_MessageSubType = "01";
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_DateAtOrigin = new ZDateTime(2023, 3, 10);
				declaration.JE_DateAtFinalDestination = new ZDateTime(2023, 3, 15);
				declaration.JE_IsMultimodal = false;
				declaration.JE_GoodsOrigin = "AU";
				declaration.JE_OH_Forwarder = forwarder.PK;
				declaration.JE_OH_Importer = orgHeader.PK;
				declaration.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Broker;
				declaration.JE_TotalWeight = 11000m;
				declaration.JE_TotalWeightUnit = Core.Constants.Weight.Hectograms;

				var instruction = declaration.CustomsEntryInstructions.AddNew();
				instruction.CEI_Description = "DESCRIPTION";

				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
				invoice.JZ_InvoiceNumber = "TEST1";
				invoice.JZ_InvoiceAmount = 1800m;
				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				invoice.JZ_NoOfPacks = 55;

				var invoiceLine1 = invoice.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = instruction.PK;
				invoiceLine1.JI_Tariff = "1";
				invoiceLine1.JI_NetWeight = 100;
				invoiceLine1.JI_NetWeightUQ = "KG";
				invoiceLine1.JI_Weight = 1;
				invoiceLine1.JI_WeightUQ = "T";
				invoiceLine1.JI_LinePrice = 600m;

				var invoiceLine2 = invoice.InvoiceLines.AddNew();
				invoiceLine2.JI_CEI = instruction.PK;
				invoiceLine2.JI_Tariff = "2";
				invoiceLine2.JI_NetWeight = 1;
				invoiceLine2.JI_NetWeightUQ = "T";
				invoiceLine2.JI_Weight = 100;
				invoiceLine2.JI_WeightUQ = "KG";
				invoiceLine2.JI_LinePrice = 600m;

				var refNumber = declaration.DispatchInstructionNumbers.AddNew();
				refNumber.CE_EntryType = DispatchInstructionDocumentTypes.Codes._01;
				refNumber.CE_EntryNum = "FAT00001";

				var refNumber2 = declaration.AdditionalReferenceNumbers.AddNew();
				refNumber2.CE_EntryType = "CQN";
				refNumber2.CE_EntryNum = "NOTINSTRUCTIONSDOCUMENT";

				var procRelated = declaration.ProcessRelatedNumbers.AddNew();
				procRelated.CE_EntryType = ProcessRelatedTypeList.Codes.ADM;
				procRelated.CE_EntryNum = "PROC00001";

				procRelated = declaration.ProcessRelatedNumbers.AddNew();
				procRelated.CE_EntryType = ZString.Empty;
				procRelated.CE_EntryNum = "PROC00002";

				var packing = declaration.Packages.AddNew();
				packing.CW_PackType = "UNT";
				packing.CW_PackQty = 10;

				declaration.ResumeApportionment();
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));

				var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();

				var entryLine = entryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault();
				var dutyFee = entryLine.Fees.AddNew();
				dutyFee.CF_ChargeType = ChargeTypesList.Codes.DTY;
				dutyFee.CF_ChargeAmount = 10m;

				var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
				var dataProvider = new ImportSiscomexProvider(sendingObject);

				CombineAssertions(() =>
				{
					AssertEquals("DeclarationInstructionsDocuments should be", 1, dataProvider.DeclarationInstructionsDocuments.Count());
					AssertEquals("TransmissionReason should be", "1", dataProvider.TransmissionReason);

					sendingObject.MessageType = ImportSiscomexActionCodeList.Codes.ORI;
					AssertEquals("TransmissionReason should be", "2", dataProvider.TransmissionReason);
					AssertEquals("DeclarationProcessRelatedList should be", 1, dataProvider.DeclarationProcessRelatedList.Count());
					AssertEquals("DeclarationPackingList should be", 1, dataProvider.DeclarationPackingList.Count());
					AssertEquals("ModalTransport should be", "01", dataProvider.ModalTransport);
					AssertEquals("ArriveDate should be", new ZDateTime(2023, 3, 15).ToString("yyyyMMdd"), dataProvider.ArriveDate?.ToString("yyyyMMdd"));
					AssertEquals("DepartureDate should be", new ZDateTime(2023, 3, 10).ToString("yyyyMMdd"), dataProvider.DepartureDate?.ToString("yyyyMMdd"));
					AssertEquals("MultiModal should be", "N", dataProvider.MultiModal);
					AssertEquals("DeclarationPayment should be", 10m, dataProvider.DeclarationPayment.First(x => x.TaxRevenueCode == "0086").Amount);
					AssertEquals("CargoProvenance should be", "072", dataProvider.CargoProvenance);
					AssertEquals("BankCode should be ZZZ", "ZZZ", dataProvider.DeclarationBankAccountPayment.BankCode);
					AssertEquals("BankBSBNumber should be 123", "123", dataProvider.DeclarationBankAccountPayment.BankBSBNumber);
					AssertEquals("BankAccountNumber should be 654", "654", dataProvider.DeclarationBankAccountPayment.BankAccountNumber);
					AssertEquals("NetWeightInKG should be", new ZDecimal(1100), dataProvider.NetWeightInKG);
					AssertEquals("GrossWeightInKG should be", new ZDecimal(1100), dataProvider.GrossWeightInKG);
					AssertEquals("FOBTotalInLocalCurrency should be", 1200m, dataProvider.FOBInLocalCurrency);
					AssertEquals("ForwarderName should be", "00000000000000", dataProvider.ForwarderNumber);
				});
			}
		}

		ImportSiscomexProvider CreateImportSiscomexProvider(JobDeclaration declaration)
		{
			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();
			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			return new ImportSiscomexProvider(sendingObject);
		}

		public void TestOverseasFreightCharges()
		{
			var (invoiceLine1, invoiceLine2) = PrepareInvoice();
			var declaration = invoiceLine1.Declaration;
			var invoice = invoiceLine1.InvoiceHeader;

			invoice.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightCollect, 400.75m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid, 500.75m, Core.Constants.CurrencyCodes.UnitedStates);

			var dataProvider = CreateImportSiscomexProvider(declaration);
			CombineAssertions("OFC/OFP charges appoitioned", () =>
			{
				AssertEquals("FreightCurrency should be USD", "220", dataProvider.FreightCurrency);
				AssertEquals("FreightCollect should be", new ZDecimal(400.75), dataProvider.FreightCollect);
				AssertEquals("FreightPrepaid should be", new ZDecimal(500.75), dataProvider.FreightPrepaid);
			});

			var ofcCharge1 = invoiceLine1.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightCollect, 400.75m, Core.Constants.CurrencyCodes.UnitedStates);
			var ofpCharge1 = invoiceLine1.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid, 500.75m, Core.Constants.CurrencyCodes.UnitedStates);

			var ofcCharge2 = invoiceLine2.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightCollect, 400m, Core.Constants.CurrencyCodes.UnitedStates);
			var ofpCharge2 = invoiceLine2.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid, 500m, Core.Constants.CurrencyCodes.UnitedStates);

			var fntCharge1 = invoiceLine1.Charges.AddNew(ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory, 600.75m, Core.Constants.CurrencyCodes.EuropeanUnion)
				.J7_PrepaidCollect = Core.Constants.PaymentType.Collect;
			var fntCharge2 = invoiceLine2.Charges.AddNew(ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory, 600m, Core.Constants.CurrencyCodes.EuropeanUnion)
				.J7_PrepaidCollect = Core.Constants.PaymentType.Prepaid;

			dataProvider = CreateImportSiscomexProvider(declaration);
			CombineAssertions("All OFC/OFP charges have the same currency", () =>
			{
				AssertEquals("FreightCurrency should be USD", "220", dataProvider.FreightCurrency);
				AssertEquals("FreightCollect should be", 1309.92m, dataProvider.FreightCollect);
				AssertEquals("FreightPrepaid should be", 1509.29m, dataProvider.FreightPrepaid);
				AssertEquals("FreightNationalTerritory should be", 1017.71m, dataProvider.FreightNationalTerritory);
			});

			ofcCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			dataProvider = CreateImportSiscomexProvider(declaration);

			CombineAssertions("One OFC charge has different currency", () =>
			{
				AssertEquals("FreightCurrency should be BRL", "790", dataProvider.FreightCurrency);
				AssertEquals("FreightCollect should be", 898.52m, dataProvider.FreightCollect);
				AssertEquals("FreightPrepaid should be", 1085.82m, dataProvider.FreightPrepaid);
				AssertEquals("FreightNationalTerritory should be", 1017.71m, dataProvider.FreightNationalTerritory);
			});

			ofcCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			ofpCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			dataProvider = CreateImportSiscomexProvider(declaration);

			CombineAssertions("One OFP charge has different currency", () =>
			{
				AssertEquals("FreightCurrency should be BRL", "790", dataProvider.FreightCurrency);
				AssertEquals("FreightCollect should be", 942.39m, dataProvider.FreightCollect);
				AssertEquals("FreightPrepaid should be", 1030.98m, dataProvider.FreightPrepaid);
				AssertEquals("FreightNationalTerritory should be", 1017.71m, dataProvider.FreightNationalTerritory);
			});
		}

		public void TestOverseasInsuranceCharge()
		{
			var (invoiceLine1, invoiceLine2) = PrepareInvoice();
			var declaration = invoiceLine1.Declaration;
			var invoice = invoiceLine1.InvoiceHeader;
			invoice.Charges.AddNew(ImportLicenseChargesProvider.OverseasInsurance.Code, 100m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice.Charges.AddNew(ImportLicenseChargesProvider.OverseasInsurance.Code, 100m, Core.Constants.CurrencyCodes.UnitedStates);

			var dataProvider = CreateImportSiscomexProvider(invoiceLine1.Declaration);
			CombineAssertions("ONS charges appoitioned", () =>
			{
				AssertEquals("InsuranceCurrency should be", "220", dataProvider.InsuranceCurrency);
				AssertEquals("InsuranceLocalCurrencyValue should be", 143.88m, dataProvider.InsuranceInLocalCurrency);
				AssertEquals("InsuranceNegotiatedCurrencyValue should be", 200m, dataProvider.Insurance);
			});

			invoiceLine1.Charges.AddNew(ImportLicenseChargesProvider.OverseasInsurance.Code, 100m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLine2.Charges.AddNew(ImportLicenseChargesProvider.OverseasInsurance.Code, 100m, Core.Constants.CurrencyCodes.UnitedStates);
			dataProvider = CreateImportSiscomexProvider(invoiceLine1.Declaration);

			CombineAssertions("All ONS charges have the same currency", () =>
			{
				AssertEquals("InsuranceCurrency should be", "220", dataProvider.InsuranceCurrency);
				AssertEquals("InsuranceLocalCurrencyValue should be", 143.88m, dataProvider.InsuranceInLocalCurrency);
				AssertEquals("InsuranceNegotiatedCurrencyValue should be", 200m, dataProvider.Insurance);
			});

			invoiceLine2.Charges.AddNew(ImportLicenseChargesProvider.OverseasInsurance.Code, 100m, Core.Constants.CurrencyCodes.EuropeanUnion);
			dataProvider = CreateImportSiscomexProvider(invoiceLine1.Declaration);

			CombineAssertions("One ONS charges has different currency", () =>
			{
				AssertEquals("InsuranceCurrency should be", "790", dataProvider.InsuranceCurrency);
				AssertEquals("InsuranceLocalCurrencyValue should be", 204.86m, dataProvider.InsuranceInLocalCurrency);
				AssertEquals("InsuranceNegotiatedCurrencyValue should be", 204.86m, dataProvider.Insurance);
			});
		}

		(JobComInvoiceLine, JobComInvoiceLine) PrepareInvoice()
		{
			var localCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Brazil);
			var usdRate = localCurrency.ExchangeRates.AddNew();
			usdRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			usdRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			usdRate.RE_SellRate = 2m;
			var eurRate = localCurrency.ExchangeRates.AddNew();
			eurRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			eurRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			eurRate.RE_SellRate = 3m;

			var codes = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("USD", "220"),
					new KeyValuePair<string, string>("BRL", "790")
				};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Currency, "Currency Codes Mapping", codes);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 1800m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoice.JZ_NetWeight = 60;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_NetWeight = 100;
			invoiceLine1.JI_NetWeightUQ = "KG";
			invoiceLine1.JI_LinePrice = 600m;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2";
			invoiceLine2.JI_NetWeight = 100;
			invoiceLine2.JI_NetWeightUQ = "KG";
			invoiceLine2.JI_LinePrice = 600m;

			return (invoiceLine1, invoiceLine2);
		}

		public void TestVesselCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.Country, "BTH", "Country Codes Mapping", true);
			helper.CreateCusMap(RefCusMapTypeList.Codes.Country, "AU", "072", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), Core.Constants.CountryCodes.Brazil);
			helper.CreateCusMap(RefCusMapTypeList.Codes.Country, "BR", "074", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), Core.Constants.CountryCodes.Brazil);

			var refVessel = Factory.New<RefVessel>();
			refVessel.RV_Code = "VESSEL";
			refVessel.RV_RN_NKCountryOfReg = "AU";

			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_RL_NKClosestPort = "BRSAO";

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Description = "DESCRIPTION";

			declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;

			CombineAssertions("MessageSubType = 01", () =>
			{
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("TransportMode = Sea", string.Empty, dataProvider.VesselCountry);

				declaration.JE_VesselName = refVessel.RV_Code;
				AssertEquals("TransportMode = Sea", "072", dataProvider.VesselCountry);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("TransportMode = Air", string.Empty, dataProvider.VesselCountry);

				declaration.JE_OH_ShippingLine = shippingLine.PK;
				AssertEquals("TransportMode = Air", "074", dataProvider.VesselCountry);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
				AssertNull("TransportMode = Road", dataProvider.VesselCountry);
			});

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._11;
			CombineAssertions("MessageSubType = 11", () =>
			{
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_VesselName = refVessel.RV_Code;
				declaration.JE_OH_ShippingLine = shippingLine.PK;

				AssertNull("TransportMode = Sea", dataProvider.VesselCountry);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				AssertNull("TransportMode = Air", dataProvider.VesselCountry);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
				AssertNull("TransportMode = Road", dataProvider.VesselCountry);
			});
		}

		public void TestTruckRef()
		{
			var refVessel = Factory.New<RefVessel>();
			refVessel.RV_Code = "VESSEL";
			refVessel.RV_RN_NKCountryOfReg = "AU";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_VesselName = refVessel.RV_Code;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var messageBuilder = new ImportSiscomexProvider(sendingObject);

			CombineAssertions(() =>
			{
				AssertEquals("TruckRef should be", null, messageBuilder.TruckRef);
				declaration.JE_TransportMode = TransportTypeList.Codes.Road;
				declaration.JE_VesselName = refVessel.RV_Code;
				AssertEquals("TruckRef should be", "VESSEL", messageBuilder.TruckRef);
			});
		}

		public void TestDeclarationPayment()
		{
			ReferenceTestDataHelper.CreateTaxRevenueTypeList(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate(ChargeTypesList.Codes.DTY, 10m);

			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);

			var dutyPayment = dataProvider.DeclarationPayment.First();
			AssertEquals("TaxRevenueCode", "0086", dutyPayment.TaxRevenueCode);
			AssertEquals("Amount", 10m, dutyPayment.Amount);
		}

		public void TestPacking()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var cusEntryLine = Factory.New<CusEntryLine>();
			cusEntryLine.CL_CH = entryHeader.PK;

			var pack = declaration.Packages.AddNew();
			pack.CW_PackType = Core.Constants.PkgUnit.Unit;
			pack.CW_PackQty = 10;

			pack = declaration.Packages.AddNew();
			pack.CW_PackType = Core.Constants.PkgUnit.Container;
			pack.CW_PackQty = 5;

			pack = declaration.Packages.AddNew();
			pack.CW_PackType = Core.Constants.PkgUnit.Unit;
			pack.CW_PackQty = 20;

			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);
			var packing = dataProvider.DeclarationPackingList.ToArray();

			CombineAssertions(() =>
			{
				AssertEquals("DeclarationDiscountChargeList count should be", 2, packing.Length);
				AssertEquals("packing 1 should be", Core.Constants.PkgUnit.Unit, packing[0].PackingTypeCode);
				AssertEquals("packing 2 should be", Core.Constants.PkgUnit.Container, packing[1].PackingTypeCode);

				AssertEquals("packing 1 should be", 30, packing[0].PackingQty);
				AssertEquals("packing 2 should be", 5, packing[1].PackingQty);
			});
		}

		public void TestWarehouse()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var area = Factory.New<WarehouseArea>();
			area.CY_ParentTableCode = declaration.TablePrefix;
			area.CY_ParentID = declaration.PK;
			area.CY_Code = "W1";

			area = Factory.New<WarehouseArea>();
			area.CY_ParentTableCode = declaration.TablePrefix;
			area.CY_ParentID = declaration.PK;
			area.CY_Code = "W2";

			area = Factory.New<WarehouseArea>();
			area.CY_ParentTableCode = declaration.TablePrefix;
			area.CY_ParentID = declaration.PK;
			area.CY_Code = "W3";

			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);
			var warehouse = dataProvider.DeclarationWarehouseList.ToArray();

			CombineAssertions(() =>
			{
				AssertEquals("DeclarationWarehouseList count should be", 3, warehouse.Length);
				AssertEquals("Warehouse 1 should be", "W1", warehouse[0].WarehouseCode);
				AssertEquals("Warehouse 2 should be", "W2", warehouse[1].WarehouseCode);
				AssertEquals("Warehouse 3 should be", "W3", warehouse[2].WarehouseCode);
			});
		}

		public void TestCargoAndManifest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._11;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_UCR = "55";
			declaration.JE_HouseBill = "HOUSE_TEST";
			declaration.JE_CargoArrivalDocumentType = BRCargoArrivalDocList.Codes.CargoManifest;
			declaration.JE_CargoArrivalDocumentNumber = "2";
			declaration.JE_CargoArrivalDocumentUtilization = BRUtilizationList.Codes.Total;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Description = "DESCRIPTION";

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);

			CombineAssertions(() =>
			{
				AssertNull("ManifestType should be", dataProvider.ManifestType);
				AssertNull("MasterCargoDocNumber should be Empty", dataProvider.MasterCargoDocNumber);
				AssertNull("CargoDocNumber should be", dataProvider.CargoDocNumber);
				AssertNull("ManifestNumber should be", dataProvider.ManifestNumber);
				AssertNull("CargoArrivalDocUtilization should be", dataProvider.CargoArrivalDocUtilization);
			});

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;

			CombineAssertions(() =>
			{
				AssertEquals("CargoDocType should be", BillTypeList.Codes.HBL, dataProvider.CargoDocType);
				AssertEquals("ManifestType should be", BRCargoArrivalDocList.Codes.CargoManifest, dataProvider.ManifestType);
				AssertEquals("MasterCargoDocNumber should be", "55", dataProvider.MasterCargoDocNumber);
				AssertEquals("CargoDocNumber should be", ImportSiscomexProvider.Cemercante, dataProvider.CargoDocNumber);
				AssertEquals("ManifestNumber should be", "2", dataProvider.ManifestNumber);
				AssertEquals("CargoArrivalDocUtilization should be", BRUtilizationList.Codes.Total, dataProvider.CargoArrivalDocUtilization);
			});

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_UCR = ZString.Empty;
			declaration.JE_HouseBill = "HOUSE_TEST";
			declaration.JE_MasterBill = "MASTER_TEST";
			declaration.JE_CargoArrivalDocumentType = BRCargoArrivalDocList.Codes.CargoManifest;
			declaration.JE_CargoArrivalDocumentNumber = "2";
			declaration.JE_CargoArrivalDocumentUtilization = BRUtilizationList.Codes.Total;

			CombineAssertions(() =>
			{
				AssertEquals("CargoDocType should be", BillTypeList.Codes.HAWB, dataProvider.CargoDocType);
				AssertEquals("ManifestType should be", BRCargoArrivalDocList.Codes.CargoManifest, dataProvider.ManifestType);
				AssertEquals("MasterCargoDocNumber should be", "MASTER_TEST", dataProvider.MasterCargoDocNumber);
				AssertEquals("CargoDocNumber should be", "HOUSE_TEST", dataProvider.CargoDocNumber);
				AssertEquals("ManifestNumber should be", "2", dataProvider.ManifestNumber);
				AssertEquals("CargoArrivalDocUtilization should be", BRUtilizationList.Codes.Total, dataProvider.CargoArrivalDocUtilization);
			});

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			declaration.JE_UCR = "55";
			declaration.JE_HouseBill = "HOUSE_TEST";
			declaration.JE_MasterBill = "MASTER_TEST";
			declaration.JE_CargoArrivalDocumentType = BRCargoArrivalDocList.Codes.CargoManifest;
			declaration.JE_CargoArrivalDocumentNumber = "2";
			declaration.JE_CargoArrivalDocumentUtilization = BRUtilizationList.Codes.Total;

			CombineAssertions(() =>
			{
				AssertEquals("CargoDocType should be", BillTypeList.Codes.CRT, dataProvider.CargoDocType);
				AssertEquals("ManifestType should be", BRCargoArrivalDocList.Codes.CargoManifest, dataProvider.ManifestType);
				AssertNull("MasterCargoDocNumber should be Empty", dataProvider.MasterCargoDocNumber);
				AssertEquals("CargoDocNumber should be", "55", dataProvider.CargoDocNumber);
				AssertEquals("ManifestNumber should be", "2", dataProvider.ManifestNumber);
				AssertEquals("CargoArrivalDocUtilization should be", BRUtilizationList.Codes.Total, dataProvider.CargoArrivalDocUtilization);
			});

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_UCR = ZString.Empty;
			declaration.JE_HouseBill = ZString.Empty;
			declaration.JE_MasterBill = "MASTER_TEST";
			declaration.JE_CargoArrivalDocumentType = BRCargoArrivalDocList.Codes.CargoManifest;
			declaration.JE_CargoArrivalDocumentNumber = "2";
			declaration.JE_CargoArrivalDocumentUtilization = BRUtilizationList.Codes.Total;

			CombineAssertions(() =>
			{
				AssertEquals("CargoDocType should be", BillTypeList.Codes.AWB, dataProvider.CargoDocType);
				AssertEquals("ManifestType should be", BRCargoArrivalDocList.Codes.CargoManifest, dataProvider.ManifestType);
				AssertNull("MasterCargoDocNumber should be Empty", dataProvider.MasterCargoDocNumber);
				AssertEquals("CargoDocNumber should be", "MASTER_TEST", dataProvider.CargoDocNumber);
				AssertEquals("ManifestNumber should be", "2", dataProvider.ManifestNumber);
				AssertEquals("CargoArrivalDocUtilization should be", BRUtilizationList.Codes.Total, dataProvider.CargoArrivalDocUtilization);
			});

			declaration.JE_TransportMode = TransportTypeList.Codes.Own;

			CombineAssertions(() =>
			{
				AssertEquals("CargoDocType should be", ZString.Empty, dataProvider.CargoDocType);
				AssertNull("ManifestType should be", dataProvider.ManifestType);
				AssertNull("MasterCargoDocNumber should be Empty", dataProvider.MasterCargoDocNumber);
				AssertNull("CargoDocNumber should be", dataProvider.CargoDocNumber);
				AssertNull("ManifestNumber should be", dataProvider.ManifestNumber);
				AssertNull("CargoArrivalDocUtilization should be", dataProvider.CargoArrivalDocUtilization);
			});

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_UCR = ZString.Empty;
			declaration.JE_HouseBill = "HOUSE_TEST";
			declaration.JE_MasterBill = "MASTER_TEST";
			declaration.JE_CargoArrivalDocumentType = BRCargoArrivalDocList.Codes.CargoManifest;
			declaration.JE_CargoArrivalDocumentNumber = "2";
			declaration.JE_CargoArrivalDocumentUtilization = BRUtilizationList.Codes.Total;
			var refNumber = declaration.AdditionalReferenceNumbers.AddNew();
			refNumber.CE_EntryType = BrazilAdditionalReferenceNumberTypes.Codes.MBL;
			refNumber.CE_EntryNum = "1234_TEST";

			CombineAssertions(() =>
			{
				AssertEquals("CargoDocType should be", BillTypeList.Codes.HAWB, dataProvider.CargoDocType);
				AssertEquals("ManifestType should be", BRCargoArrivalDocList.Codes.CargoManifest, dataProvider.ManifestType);
				AssertEquals("MasterCargoDocNumber should be", "1234_TEST", dataProvider.MasterCargoDocNumber);
				AssertEquals("CargoDocNumber should be", "HOUSE_TEST", dataProvider.CargoDocNumber);
				AssertEquals("ManifestNumber should be", "2", dataProvider.ManifestNumber);
				AssertEquals("CargoArrivalDocUtilization should be", BRUtilizationList.Codes.Total, dataProvider.CargoArrivalDocUtilization);
			});

			refNumber.CE_EntryType = BrazilAdditionalReferenceNumberTypes.Codes.RUC;
			refNumber.CE_EntryNum = "1234_TEST";

			CombineAssertions(() =>
			{
				AssertEquals("CargoDocType should be", BillTypeList.Codes.HAWB, dataProvider.CargoDocType);
				AssertEquals("ManifestType should be", BRCargoArrivalDocList.Codes.CargoManifest, dataProvider.ManifestType);
				AssertEquals("MasterCargoDocNumber should be", "MASTER_TEST", dataProvider.MasterCargoDocNumber);
				AssertEquals("CargoDocNumber should be", "HOUSE_TEST", dataProvider.CargoDocNumber);
				AssertEquals("ManifestNumber should be", "2", dataProvider.ManifestNumber);
				AssertEquals("CargoArrivalDocUtilization should be", BRUtilizationList.Codes.Total, dataProvider.CargoArrivalDocUtilization);
			});

			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			declaration.JE_UCR = ZString.Empty;
			declaration.JE_HouseBill = "HOUSE_TEST";
			declaration.JE_MasterBill = "MASTER_TEST";

			refNumber.CE_EntryType = BrazilAdditionalReferenceNumberTypes.Codes.MBL;
			refNumber.CE_EntryNum = "1234_TEST";

			CombineAssertions(() =>
			{
				AssertEquals("CargoDocType should be", BillTypeList.Codes.HRWB, dataProvider.CargoDocType);
				AssertEquals("ManifestType should be", BRCargoArrivalDocList.Codes.CargoManifest, dataProvider.ManifestType);
				AssertEquals("MasterCargoDocNumber should be", "MASTER_TEST", dataProvider.MasterCargoDocNumber);
				AssertEquals("CargoDocNumber should be", "HOUSE_TEST", dataProvider.CargoDocNumber);
				AssertEquals("ManifestNumber should be", "2", dataProvider.ManifestNumber);
				AssertEquals("CargoArrivalDocUtilization should be", BRUtilizationList.Codes.Total, dataProvider.CargoArrivalDocUtilization);
			});
		}

		public void TestMercosulForeign()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var cusEntryLine = Factory.New<CusEntryLine>();
			cusEntryLine.CL_CH = entryHeader.PK;

			var mercosulForeign = entryInstruction.MercosulForeignDeclarations.AddNew();
			mercosulForeign.CSI_Description = "8922323000";
			mercosulForeign.CSI_ReferenceNumber = "1";
			mercosulForeign.CSI_ReferenceNumber2 = "10";

			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);
			var mercosul = dataProvider.DeclarationMercosulForeignList.ToArray();

			CombineAssertions(() =>
			{
				AssertEquals("DeclarationMercosulForeignList count should be", 1, mercosul.Length);
				AssertEquals("packing 1 should be", "8922323000", mercosul.First().DeclarationMercosulForeignNumber);
				AssertEquals("packing 1 should be", "1", mercosul.First().InicialNumber);
				AssertEquals("packing 1 should be", "10", mercosul.First().FinalNumber);
			});
		}

		public void TestImporterId()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "IMPORTER COMPANY";
			consignee.PrimaryRegistrationNumber.Number = "58.500.398/0001-05";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.DeclarantType = DeclarantTypeList.Codes.DiplomaticMission;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);

			AssertNull("ImporterId should be null", dataProvider.ImporterId);

			dataProvider = new ImportSiscomexProvider(sendingObject);
			declaration.JE_OH_Importer = consignee.PK;
			declaration.DeclarantType = DeclarantTypeList.Codes.LegalPerson;

			AssertEquals("ImporterId should be", "58500398000105", dataProvider.ImporterId);
		}

		public void TestImporterAddress()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "IMPORTER SISCOMEX";
			consignee.MainAddress.OA_PostCode = "02430000";
			consignee.MainAddress.UnrestrictedAdditionalAddressInformation = "COMPLEMENTARY ADDRESS";
			consignee.MainAddress.OA_Address1 = "MAIN ADDRESS";
			consignee.MainAddress.OA_City = "CAMPINAS";
			consignee.MainAddress.OA_State = "SP";
			consignee.MainAddress.OA_Phone = "55 11-35856000";

			var declaration = Factory.New<JobDeclaration>();
			declaration.DeclarantType = DeclarantTypeList.Codes.NaturalPerson;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_OH_Importer = consignee.PK;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);

			AssertNull("ImporterAddress is null when DeclarantType <> '4'", dataProvider.Importer);

			declaration.DeclarantType = DeclarantTypeList.Codes.DiplomaticMission;
			dataProvider = new ImportSiscomexProvider(sendingObject);

			CombineAssertions("DeclarantType = '4'", () =>
			{
				AssertEquals("ZipCode should be", "02430000", dataProvider.Importer.ZipCode);
				AssertEquals("AddressComplementary should be", "COMPLEMENTARY ADDRESS", dataProvider.Importer.AddressComplementary);
				AssertEquals("Address should be", "MAIN ADDRESS", dataProvider.Importer.Address);
				AssertEquals("CityName should be", "CAMPINAS", dataProvider.Importer.CityName);
				AssertEquals("AddressNumber should be", ZString.Empty, dataProvider.Importer.AddressNumber);
				AssertEquals("AddressStateCode should be", "SP", dataProvider.Importer.AddressStateCode);
				AssertEquals("PhoneNumber should be", "551135856000", dataProvider.Importer.PhoneNumber);
				AssertEquals("CompanyName should be", "IMPORTER SISCOMEX", dataProvider.Importer.Name);
			});
		}

		public void TestConsignee()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE COMPANY";
			consignee.PrimaryRegistrationNumber.Number = "58.500.398/0001-05";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.DeclarantType = DeclarantTypeList.Codes.DiplomaticMission;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);

			AssertNull("Consignee should be null", dataProvider.Consignee);

			dataProvider = new ImportSiscomexProvider(sendingObject);
			declaration.JE_OH_Consignee = consignee.PK;

			AssertEquals("Declarant ID should be", "58500398000105", dataProvider.Consignee.ID);
			AssertEquals("Declarant Name should be", "CONSIGNEE COMPANY", dataProvider.Consignee.Name);
		}

		public void TestDispatchModality()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);

			AssertNull("DispatchModality should be", dataProvider.DispatchModality);

			dataProvider = new ImportSiscomexProvider(sendingObject);
			declaration.JE_DispatchModality = DispatchModalityCodes.Codes.AnticipatedFractionalDelivery;

			AssertEquals("DispatchModality should be", DispatchModalityCodes.Codes.AnticipatedFractionalDelivery, dataProvider.DispatchModality);
		}

		public void TestSectorCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);

			AssertEquals("SectorCode must be empty", ZString.Empty, dataProvider.SectorCode);

			dataProvider = new ImportSiscomexProvider(sendingObject);
			declaration.JE_SubLocationOfGoods = "001";

			AssertEquals("SectorCode must be", "001", dataProvider.SectorCode);
		}

		public void TestDeclarantTypeCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);

			AssertEquals("DeclarantTypeCode must be empty", ZString.Empty, dataProvider.DeclarantTypeCode);

			dataProvider = new ImportSiscomexProvider(sendingObject);
			declaration.DeclarantType = "4";

			AssertEquals("DeclarantTypeCode must be", "4", dataProvider.DeclarantTypeCode);
		}

		public void TestTaxPaymentTypeCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);

			AssertEquals("TaxPaymentTypeCode must be", "1", dataProvider.TaxPaymentTypeCode);
		}

		public void TestEntranceLocationCustomsOfficeCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);

			AssertEquals("EntranceLocationCustomsOfficeCode must be empty", ZString.Empty, dataProvider.EntranceLocationCustomsOfficeCode);

			dataProvider = new ImportSiscomexProvider(sendingObject);
			declaration.EntranceOfficeCode = "123";

			AssertEquals("EntranceLocationCustomsOfficeCode must be", "123", dataProvider.EntranceLocationCustomsOfficeCode);
		}

		public void TestClearanceLocationCustomsOfficeCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);

			AssertEquals("ClearanceLocationCustomsOfficeCode must be empty", ZString.Empty, dataProvider.ClearanceLocationCustomsOfficeCode);

			dataProvider = new ImportSiscomexProvider(sendingObject);
			declaration.JE_CustomsOffice = "123";

			AssertEquals("ClearanceLocationCustomsOfficeCode must be", "123", dataProvider.ClearanceLocationCustomsOfficeCode);
		}

		public void TestClearanceLocationCustomsEnclosureCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);

			AssertEquals("ClearanceLocationCustomsEnclosureCode must be empty", ZString.Empty, dataProvider.ClearanceLocationCustomsEnclosureCode);

			dataProvider = new ImportSiscomexProvider(sendingObject);
			declaration.JE_LocationOfGoods = "3921101";

			AssertEquals("ClearanceLocationCustomsEnclosureCode must be", "3921101", dataProvider.ClearanceLocationCustomsEnclosureCode);
		}

		public void TestEntryReferenceNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();

			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);

			AssertEquals("EntryReferenceNumber must be", "B000010001", dataProvider.EntryReferenceNumber);
		}

		public void TestAdditionalInformation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);

			AssertEquals("AdditionalInformation must be empty", ZString.Empty, dataProvider.AdditionalInformation);

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_AdditionalInformationOption = AdditionalInformationOptions.Codes.FreeTextAndSystemGenerated;
			entryInstruction.AdditionalInformation = "TEST_AUTO";
			entryInstruction.AdditionalInformationManual = "TEST_MANUALLY";
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			dataProvider = new ImportSiscomexProvider(sendingObject);
			AssertEquals("AdditionalInformation must be", $"TEST_MANUALLY{System.Environment.NewLine}TEST_AUTO", dataProvider.AdditionalInformation);
		}

		public void TestPortOfLoading()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._11;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);

			AssertNull("PortOfLoading must be empty", dataProvider.PortOfLoading);

			dataProvider = new ImportSiscomexProvider(sendingObject);
			declaration.JE_RL_NKPortOfLoading = "BR6MO";

			AssertNull("PortOfLoading must be empty", dataProvider.PortOfLoading);

			dataProvider = new ImportSiscomexProvider(sendingObject);
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			declaration.JE_RL_NKPortOfLoading = "BR6MO";

			AssertEquals("PortOfLoading must be", "Moema", dataProvider.PortOfLoading);
		}

		public void TestPaymentBankNumber()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "C2";
			var orgImpAddInfo = BROrgImpAddInfo.Get(importer);
			orgImpAddInfo.ZO_BSBNumber = "111";
			orgImpAddInfo.ZO_BankCode = "XXX";
			orgImpAddInfo.ZO_AccountNumber = "777";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_OH_Importer = importer.PK;

			var accBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			accBankAccount.AB_AccountNum = "444";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);

			using (BRCustomsDataRegistry.Instance.TaxFeeCustomsPaymentBankAccount.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, accBankAccount.PK.ToGuid()))
			{
				declaration.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Importer;
				AssertEquals("JE_PaymentMethod=IMP", "777", dataProvider.PaymentBankNumber);
				declaration.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Broker;
				AssertEquals("JE_PaymentMethod=BRK", "444", dataProvider.PaymentBankNumber);
				declaration.JE_PaymentMethod = ZString.Empty;
				AssertEquals("JE_PaymentMethod=Empty", null, dataProvider.PaymentBankNumber);
			}
		}

		public void TestOperationType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.DeclarantType = DeclarantTypeList.Codes.LegalPerson;
			declaration.OperationType = TypeOfOperationImportList.Codes.OnItsOwn;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);

			AssertEquals("OperationType must be", declaration.OperationType, dataProvider.OperationType);

			declaration.DeclarantType = DeclarantTypeList.Codes.NaturalPerson;
			declaration.OperationType = TypeOfOperationImportList.Codes.AccountAndOrder;
			dataProvider = new ImportSiscomexProvider(sendingObject);
			AssertEquals("OperationType must be", declaration.OperationType, dataProvider.OperationType);

			declaration.DeclarantType = DeclarantTypeList.Codes.DiplomaticMission;
			dataProvider = new ImportSiscomexProvider(sendingObject);
			AssertEquals("OperationType must be", "1", dataProvider.OperationType);

			declaration.DeclarantType = DeclarantTypeList.Codes.DoorToDoor;
			dataProvider = new ImportSiscomexProvider(sendingObject);
			AssertEquals("OperationType must be", "3", dataProvider.OperationType);

			declaration.DeclarantType = ZString.Empty;
			dataProvider = new ImportSiscomexProvider(sendingObject);
			AssertEquals("OperationType must be", null, dataProvider.OperationType);
		}

		public void TestModalTransport()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(Constants.RefCusMapType.ModalTransport, "BTH", "Transport Modes Mapping", true);
			helper.CreateCusMap(Constants.RefCusMapType.ModalTransport, "SEA", "01", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), Core.Constants.CountryCodes.Brazil);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._11;
			declaration.BRTransportMode = "SEA";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);

			AssertNull("ModalTransport must be empty", dataProvider.ModalTransport);

			dataProvider = new ImportSiscomexProvider(sendingObject);
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;

			AssertEquals("ModalTransport must be", "01", dataProvider.ModalTransport);
		}

		public void TestDepartureDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._11;
			declaration.JE_DateAtOrigin = new ZDateTime(2023, 3, 10);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);

			AssertNull("DepartureDate must be empty", dataProvider.DepartureDate);

			dataProvider = new ImportSiscomexProvider(sendingObject);
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;

			AssertEquals("DepartureDate must be", new ZDateTime(2023, 3, 10).ToString("yyyyMMdd"), dataProvider.DepartureDate?.ToString("yyyyMMdd"));
		}

		public void TestCargoArrivalDocUtilization()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._11;
			declaration.JE_TransportMode = "SEA";
			declaration.JE_CargoArrivalDocumentUtilization = BRUtilizationList.Codes.Total;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);

			AssertNull("CargoArrivalDocUtilization must be empty", dataProvider.CargoArrivalDocUtilization);

			dataProvider = new ImportSiscomexProvider(sendingObject);
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;

			AssertEquals("CargoArrivalDocUtilization must be", BRUtilizationList.Codes.Total, dataProvider.CargoArrivalDocUtilization);
		}

		public void TestShipperName()
		{
			var messageSubTypeListForShipperName = new[] {
				MessageSubTypeList.Codes._01, MessageSubTypeList.Codes._02, MessageSubTypeList.Codes._03, MessageSubTypeList.Codes._04, MessageSubTypeList.Codes._05,
				MessageSubTypeList.Codes._06, MessageSubTypeList.Codes._07, MessageSubTypeList.Codes._08, MessageSubTypeList.Codes._09, MessageSubTypeList.Codes._10,
				MessageSubTypeList.Codes._12, MessageSubTypeList.Codes._13, MessageSubTypeList.Codes._14, MessageSubTypeList.Codes._15 };

			var transportModeListForShipperName = new[] {
				TransportTypeList.Codes.Sea, TransportTypeList.Codes.River, TransportTypeList.Codes.Lake,
				TransportTypeList.Codes.Air, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Road
			};

			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_FullName = "Test Shipping";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_OH_ShippingLine = shippingLine.PK;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);

			CombineAssertions(() =>
			{
				foreach (var messageSubType in declaration.Lookups.MessageSubTypeList.GetAllCodes())
				{
					declaration.JE_MessageSubType = messageSubType;
					foreach (var transportMode in declaration.Lookups.TransportTypeList.GetAllCodes())
					{
						declaration.JE_TransportMode = transportMode;
						if (messageSubTypeListForShipperName.Contains(messageSubType) && transportModeListForShipperName.Contains(transportMode))
						{
							AssertEquals($"ShipperName when Message Sub Type = {messageSubType} && Transport Mode = {transportMode}", "Test Shipping", dataProvider.ShipperName);
						}
						else
						{
							AssertNull($"ShipperName when Message Sub Type = {messageSubType} && Transport Mode = {transportMode}", dataProvider.ShipperName);
						}
					}
				}
			});
		}

		public void TestVesselName()
		{
			var messageSubTypeListForVesselName = new[] {
				MessageSubTypeList.Codes._01, MessageSubTypeList.Codes._02, MessageSubTypeList.Codes._03, MessageSubTypeList.Codes._04, MessageSubTypeList.Codes._05,
				MessageSubTypeList.Codes._06, MessageSubTypeList.Codes._07, MessageSubTypeList.Codes._08, MessageSubTypeList.Codes._09, MessageSubTypeList.Codes._10,
				MessageSubTypeList.Codes._12 };

			var transportModeListForVesselName = new[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.River, TransportTypeList.Codes.Lake };

			var refVessel = Factory.New<RefVessel>();
			refVessel.RV_Code = "VESSEL";
			refVessel.RV_RN_NKCountryOfReg = "AU";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);

			CombineAssertions(() =>
			{
				foreach (var messageSubType in declaration.Lookups.MessageSubTypeList.GetAllCodes())
				{
					declaration.JE_MessageSubType = messageSubType;
					foreach (var transportMode in declaration.Lookups.TransportTypeList.GetAllCodes())
					{
						declaration.JE_TransportMode = transportMode;
						declaration.JE_VesselName = refVessel.RV_Code;
						if (messageSubTypeListForVesselName.Contains(messageSubType) && transportModeListForVesselName.Contains(transportMode))
						{
							AssertEquals($"VesselName when Message Sub Type = {messageSubType} && Transport Mode = {transportMode}", "VESSEL", dataProvider.VesselName);
						}
						else
						{
							AssertNull($"VesselName when Message Sub Type = {messageSubType} && Transport Mode = {transportMode}", dataProvider.VesselName);
						}
					}
				}
			});
		}

		public void TestFreightCurrency()
		{
			var codes = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("USD", "220"),
					new KeyValuePair<string, string>("BRL", "790")
				};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Currency, "Currency Codes Mapping", codes);

			var localCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Brazil);
			var usdRate = localCurrency.ExchangeRates.AddNew();
			usdRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			usdRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			usdRate.RE_SellRate = 2m;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 1800m;

			invoice.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightCollect, 400m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice.Charges.AddNew(ImportLicenseChargesProvider.OverseasInsurance.Code, 100m, Core.Constants.CurrencyCodes.UnitedStates);

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightCollect, 30m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLine1.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid, 40m, Core.Constants.CurrencyCodes.UnitedStates);

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightCollect, 50m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLine2.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid, 50m, Core.Constants.CurrencyCodes.UnitedStates);

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();

			var sendingObject = new ImportSiscomexMessageSendingObject(entryHeader);
			var dataProvider = new ImportSiscomexProvider(sendingObject);

			AssertEquals("FreightCurrency should be", "220", dataProvider.FreightCurrency);

			invoiceLine1.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid, 1000m, Core.Constants.CurrencyCodes.Brazil);

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));

			dataProvider = new ImportSiscomexProvider(sendingObject);

			AssertEquals("FreightCurrency should be", "790", dataProvider.FreightCurrency);
		}
	}
}
