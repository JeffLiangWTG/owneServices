using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public sealed class ZTestHelper
	{
		public ZTestHelper(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		public BusinessObjectFactory Factory;
		public JobDeclaration Declaration;

		public JobComInvoiceHeader Header1;
		public JobComInvoiceLine Line1;
		public JobComInvoiceLine Line2;
		public JobComInvoiceLine Line3;
		public JobComInvoiceLine Line4;
		public JobComInvoiceLine Line5;
		public JobComInvoiceLine Line6;

		public string TestTariffNumber1 = "66660000";
		public string TestTariffNumber2 = "77770000";
		public string TestTariffNumber3 = "88880000";
		public string StatisticalCode1 = "22";
		public string StatisticalCode2 = "12";
		public string StatisticalCode3 = "42";
		public string StatisticalCode4 = "23";

		public const short TestCPDecNumber1 = 5660;
		public const short TestCPDecNumber2 = 5661;
		public const short TestCPDecNumber3 = 5662;
		public const short TestCPDecNumber4 = 5663;
		public const short TestCPDecNumber5 = 5664;
		public const short TestCPDecNumber6 = 5665;
		public const short TestCPDecNumber7 = 5667;

		public const short TestCPDecVersion1 = 1;
		public const short TestCPDecVersion2 = 2;
		public const short TestCPDecVersion3 = 3;
		public const short TestCPDecVersion4 = 4;
		public const short TestCPDecVersion5 = 5;
		public const short TestCPDecVersion6 = 6;
		public const short TestCPDecVersion7 = 7;

		public RefCurrency AUDCurrency
		{
			get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD"); }
		}

		public RefCurrency USDCurrency
		{
			get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD"); }
		}

		public RefCurrency EURCurrency
		{
			get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "EUR"); }
		}

		public RefCurrency MYRCurrency
		{
			get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "MYR"); }
		}

		public CusHAWB CreateTestHouseBill(CusMAWB masterBill)
		{
			CusHAWB house = masterBill.ChildBills.AddNew();
			house.CS_Weight = 56.4m;
			house.CS_WeightUQ = "KG";
			house.CS_HAWB = "30000000000";

			var consignor = Factory.New<OrgHeader>();
			consignor.FillWithValidTestData();
			consignor.OH_FullName = "DONNELLY MIRRORS";
			consignor.MainAddress.OA_Address1 = "1079 FARRINGTON DR";
			consignor.MainAddress.OA_City = "CHICAGO";
			consignor.MainAddress.OA_State = "IL";
			consignor.MainAddress.OA_PostCode = "60646";
			consignor.OH_RL_NKClosestPort = "USLAX";
			house.CS_OA_ConsignorAddress = consignor.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.OH_FullName = "RAINSFORD PTY LTD";
			consignee.MainAddress.OA_Address1 = "450 EUSTON RD";
			consignee.MainAddress.OA_City = "BROOKVALE";
			consignee.MainAddress.OA_State = "NSW";
			consignee.MainAddress.OA_PostCode = "2100";
			consignee.OH_RL_NKClosestPort = "AUSYD";

			house.CS_OA_ConsigneeAddress = consignee.MainAddress.PK;
			house.CS_RL_NKDestination = "AUSYD";
			house.CS_RL_NKOrigin = "GBLHR";
			house.CS_FreightPrepaidCollect = "PO";
			house.CS_GoodsDescription = "10 AUTOMOTIVE MIRRORS CHROME COATED";
			house.CS_PiecesManifested = 10;
			house.CS_GoodsValue = 745m;
			house.CS_RX_NKGoodsCurrency = "USD";

			return house;
		}

		public CusHAWB CreateTestHouseBill()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;

			var consignor = Factory.New<OrgHeader>();
			consignor.FillWithValidTestData();

			var consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();

			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			ForwardingConsol consol = shipment.Consols.AddNew();
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_VoyageFlight = "QF1";
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;

			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_JK = consol.PK;
			masterBill.CM_FlightNo = "QF1";
			masterBill.CM_ArrivalDate = ZDateTime.Today;
			masterBill.CM_MAWB = "081-11111111";
			masterBill.CM_RL_NKDischargePort = "AUSYD";
			masterBill.CM_RL_NKLoadPort = "NZAKL";
			CusHAWB hAWB = CreateTestHouseBill(masterBill);
			hAWB.CS_JS = shipment.PK;
			return hAWB;
		}

		public void PopulateSimpleImportDeclaration()
		{
			Declaration = GetNewJobDec();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);
			Header1 = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			Header1.JZ_RX_NKInvoice_Currency = AUDCurrency.RX_Code;
			Line1 = Header1.JobComInvoiceLines.AddNew();
			SetLineValues(Line1, TestTariffNumber1, StatisticalCode1);
		}

		public void PopulateSimpleExportDeclaration()
		{
			Declaration = GetNewJobDec();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();
		}

		public void PopulateSimpleQuarantineDeclaration(string declarationReference = "B0000001")
		{
			Declaration = GetNewJobDec();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			Declaration.JE_DeclarationReference = declarationReference;
			Header1 = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			Line1 = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();
		}

		public void PopulateComplexImportDeclaration()
		{
			PopulateSimpleImportDeclaration();
			Line2 = Header1.JobComInvoiceLines.AddNew();
			Line3 = Header1.JobComInvoiceLines.AddNew();
			Line4 = Header1.JobComInvoiceLines.AddNew();
			Line5 = Header1.JobComInvoiceLines.AddNew();
			Line6 = Header1.JobComInvoiceLines.AddNew();

			SetLineValues(Line2, TestTariffNumber1, StatisticalCode1);
			SetLineValues(Line3, TestTariffNumber1, StatisticalCode1);
			SetLineValues(Line4, TestTariffNumber2, StatisticalCode2);
			SetLineValues(Line5, TestTariffNumber2, StatisticalCode3);
			SetLineValues(Line6, TestTariffNumber3, StatisticalCode4);
		}

		public void PopulateMockEdificeDeclaration()
		{
			Declaration = GetNewJobDec();
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			Declaration.JE_DateOfFirstArrival = new ZDateTime(2003, 11, 13);
			Declaration.JE_ExportDate = new ZDateTime(2003, 11, 12);
			Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_RX_NKInvoice_Currency = AUDCurrency.RX_Code;
			JobComInvoiceLine line1 = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();
			SetLineValues(line1, TestTariffNumber1, StatisticalCode1);
		}

		public void SetupCertificates()
		{
			Common.AU.CMR.Testing.CMRUtilitiesTest.SetupCertificates();
		}

		public void PopulateExchangeRates()
		{
			var uSDExchangeRate = Factory.LoadTop1<RefExchangeRate>(new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, USDCurrency.RX_Code));
			uSDExchangeRate.RE_ExpiryDate = new ZDateTime(2004, 1, 1);
			uSDExchangeRate.RE_ExRateType = "CUS";
			uSDExchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
			uSDExchangeRate.RE_RX_NKExCurrency = USDCurrency.RX_Code;
			uSDExchangeRate.RE_SellRate = 0.8484m;
			uSDExchangeRate.RE_StartDate = new ZDateTime(2003, 10, 1);
		}

		public void SetExchangeRate(ZDateTime startDate, ZDateTime endDate, ZDecimal exchangeRate, RefCurrency foreignCurrency)
		{
			SetExchangeRate(startDate, endDate, exchangeRate, foreignCurrency, "CUS");
		}

		public void SetExchangeRate(ZDateTime startDate, ZDateTime endDate, ZDecimal exchangeRate, RefCurrency foreignCurrency, string exRateType)
		{
			ZQuery sQLFilter = new ZQuery();
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_RX_NKExCurrency, foreignCurrency.RX_Code);
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_GC, GlbCompany.CurrentCompany.PK);
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_ExRateType, exRateType);
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThan, startDate.AddDays(1));
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualTo, startDate);

			var exchangeRateDuty = Factory.LoadTop1<RefExchangeRate>(sQLFilter);
			if (exchangeRateDuty == null)
			{
				exchangeRateDuty = Factory.New<RefExchangeRate>();
				exchangeRateDuty.RE_ExpiryDate = endDate;
				exchangeRateDuty.RE_ExRateType = exRateType;
				exchangeRateDuty.RE_GC = GlbCompany.CurrentCompany.PK;
				exchangeRateDuty.RE_RX_NKExCurrency = foreignCurrency.RX_Code;
				exchangeRateDuty.RE_StartDate = startDate;
				exchangeRateDuty.RE_SellRate = exchangeRate;
				Factory.Save();
			}
			else
			{
				exchangeRateDuty.RE_SellRate = exchangeRate;
			}
		}

		public void SetExchangeRate(ZDateTime startDate, ZDateTime endDate, ZDecimal exchangeRate)
		{
			SetExchangeRate(startDate, endDate, exchangeRate, USDCurrency);
		}

		public void PopulateDutiableDeclaration(ZDateTime dTVA, ZDateTime dTAF, ZString tariff, ZString stat, ZDecimal amount)
		{
			Declaration = GetNewJobDec();
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			Declaration.JE_ExportDate = dTVA;
			Declaration.JE_DateOfFirstArrival = dTAF;
			Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_InvoiceAmount = amount;
			Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_RX_NKInvoice_Currency = USDCurrency.RX_Code;
			Line1 = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();
			SetLineValues(Line1, tariff, stat);
			Line1.JI_LinePrice = amount;
		}

		public void PopulateMergedDutiableDeclaration(ZDateTime dTVA, ZDateTime dTAF, ZString tariff, ZString stat, ZDecimal amount)
		{
			PopulateDutiableDeclaration(dTVA, dTAF, tariff, stat, amount);
			Declaration.DoMerge();
		}

		public void CreateMockInvoiceForCPDecQuestions()
		{
			Declaration = GetNewJobDec();
			using (Declaration.GetValidationSuspender())
			{
				Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_RX_NKInvoice_Currency = AUDCurrency.RX_Code;
				JobComInvoiceLine line1 = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();
				SetLineValues(line1, TestTariffNumber1, StatisticalCode1);
			}
		}

		public void CreateEntryFromDeliveranceN10A()
		{
			Declaration = GetNewJobDec();
			using (Declaration.GetValidationSuspender())
			{
				Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

				Declaration.JE_ExportDate = new ZDateTime(2003, 10, 24);
				Declaration.JE_DateOfFirstArrival = new ZDateTime(2003, 10, 28);

				JobComInvoiceHeader header = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				header.JZ_OH_Supplier = Factory.New(typeof(OrgHeader)).PK;
				header.JZ_InvoiceAmount = 1000m;

				header.JZ_RX_NKInvoice_Currency = USDCurrency.RX_Code;
				JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
				line.JI_Tariff = "2203.00.31 15";
				line.JI_Description = "1 Litre Alcohol";
				line.JI_LinePrice = 1000m;
				line.JI_CustomsQuantity = 1.0m;
			}
		}

		public void CreateEntryFromDeliveranceN10A_2()
		{
			Declaration = GetNewJobDec();
			using (Declaration.GetValidationSuspender())
			{
				Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				Declaration.JE_ExportDate = new ZDateTime(2000, 12, 31);
				Declaration.JE_DateOfFirstArrival = new ZDateTime(2001, 1, 3);

				JobComInvoiceGroupHeader groupHeader = Declaration.JobComInvoiceGroupHeaders[0];
				JobComInvoiceHeader header = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				header.JZ_RX_NKInvoice_Currency = USDCurrency.RX_Code;
				header.JZ_InvoiceAmount = 1250m;
				header.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;

				Customs.Business.BaseJobComInvHeaderCharge oFT = groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 381.90m, USDCurrency.RX_Code);
				oFT.J7_IsIncludedInITOT = true;
				Customs.Business.BaseJobComInvHeaderCharge oNS = groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 3.13m, USDCurrency.RX_Code);
				oNS.J7_IsIncludedInITOT = true;
				JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
				line.JI_Tariff = "5407.61.00 30";
				line.JI_Description = "POLYESTER PGOODS UNDER";
				line.JI_LinePrice = 1250m;
				line.JI_CustomsQuantity = 1000.0m;
				line.AddInfo.ZA_TreatmentCode_Hidden = "680";
				line.AddInfo.ZA_ORG = "RKOR";
				line.AddInfo.ZA_PRF = "S";
			}
		}

		public void CreateEntryFromDeliveranceN10S()
		{
			Declaration = GetNewJobDec();
			using (Declaration.GetValidationSuspender())
			{
				Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
				Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

				Declaration.JE_ExportDate = new ZDateTime(2003, 08, 29);
				Declaration.JE_DateOfFirstArrival = new ZDateTime(2003, 09, 16);

				CusContainer container = Declaration.CusContainers.AddNew();
				container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCLMixedShipper;

				JobComInvoiceHeader header = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

				header.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				header.JZ_InvoiceAmount = 1909.84m;
				header.JZ_RX_NKInvoice_Currency = USDCurrency.RX_Code;
				Customs.Business.BaseJobComInvHeaderCharge charge = Declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 137.70m, AUDCurrency.RX_Code);
				charge.J7_IsIncludedInITOT = false;

				JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
				line.JI_Tariff = "4420.10.00 17";
				line.JI_Description = "Wooden Ornaments";
				line.JI_LinePrice = 1779.84m;
				line.JI_CustomsQuantity = 18540.0m;
				line.AddInfo.ZA_ORG = "CHIN";
				line.AddInfo.ZA_PRF = "T";

				line = header.JobComInvoiceLines.AddNew();
				line.JI_Tariff = "9405.50.90 19";
				line.JI_Description = "Metal Candle HLDRS";
				line.JI_LinePrice = 130m;
				line.JI_CustomsQuantity = 0.0m;
				line.AddInfo.ZA_TreatmentCode_Hidden = "505";
				line.AddInfo.ZA_ORG = "CHIN";
				line.AddInfo.ZA_PRF = "T";
			}
		}

		public void CreateEntryFromDeliveranceN10S_CAndF()
		{
			Declaration = GetNewJobDec();
			using (Declaration.GetValidationSuspender())
			{
				Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

				Declaration.JE_ExportDate = new ZDateTime(2003, 07, 20);
				Declaration.JE_DateOfFirstArrival = new ZDateTime(2000, 08, 17);

				CusContainer container = Declaration.CusContainers.AddNew();
				container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;

				JobComInvoiceHeader header = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

				header.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
				header.JZ_InvoiceAmount = 10367.00m;
				header.JZ_RX_NKInvoice_Currency = USDCurrency.RX_Code;
				header.JZ_Nature10PackCount = 1;
				Declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 2200, USDCurrency.RX_Code);

				JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
				line.JI_Tariff = "9506.91.00 31";
				line.JI_Description = "Equipment/Physical Exert";
				line.JI_LinePrice = 10367.00m;
				line.JI_CustomsQuantity = 0.0m;
				line.AddInfo.ZA_ORG = "CHIN";
				line.AddInfo.ZA_PRF = "T";
				line.AddInfo.ZA_TILV = "3858.42AUD";
			}
		}

		public void CreateEntryFromDeliveranceN10A_UAF()
		{
			Declaration = GetNewJobDec();
			using (Declaration.GetValidationSuspender())
			{
				Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

				Declaration.JE_DateOfFirstArrival = new ZDateTime(2001, 01, 18);
				Declaration.JE_ExportDate = new ZDateTime(2001, 01, 15);

				JobComInvoiceGroupHeader groupHeader = Declaration.JobComInvoiceGroupHeaders[0];
				JobComInvoiceHeader header = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

				header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedAtFactory;
				header.JZ_InvoiceAmount = 9390.00m;
				header.JZ_RX_NKInvoice_Currency = EURCurrency.RX_Code;

				Customs.Business.BaseJobComInvHeaderCharge charge = Declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 294, EURCurrency.RX_Code);
				charge.J7_IsIncludedInITOT = false;

				JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
				line.JI_Tariff = "8472.90.90 82";
				line.JI_Description = "OTHER OFFICE MACHINERY";
				line.JI_LinePrice = 9390.00m;
				line.JI_CustomsQuantity = 600.0m;
			}
		}

		public void CreateEntryFromDeliveranceN10S_UFB()
		{
			Declaration = GetNewJobDec();
			using (Declaration.GetValidationSuspender())
			{
				Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;

				Declaration.JE_DateOfFirstArrival = new ZDateTime(2001, 02, 02);
				Declaration.JE_ExportDate = new ZDateTime(2000, 12, 07);

				JobComInvoiceGroupHeader groupHeader = Declaration.JobComInvoiceGroupHeaders[0];
				JobComInvoiceHeader header = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

				header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedFreeOnBoard;
				header.JZ_InvoiceAmount = 68601910.0m;
				header.JZ_RX_NKInvoice_Currency = EURCurrency.RX_Code;

				Customs.Business.BaseJobComInvHeaderCharge charge = Declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 1007030m, EURCurrency.RX_Code);
				charge.J7_IsIncludedInITOT = false;

				JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
				line.JI_Tariff = "8418.50.00 31";
				line.JI_Description = "OTHER OFFICE MACHINERY";
				line.JI_LinePrice = 68601910.00m;
				line.JI_CustomsQuantity = 5223.0m;
				line.AddInfo.ZA_TILV = "4343AUD";
			}
		}

		public void CreateEntryFromDeliveranceN20S()
		{
			Declaration = GetNewJobDec();
			using (Declaration.GetValidationSuspender())
			{
				Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;

				Declaration.JE_DateOfFirstArrival = new ZDateTime(2001, 10, 24);
				Declaration.JE_ExportDate = new ZDateTime(2001, 10, 13);

				JobComInvoiceGroupHeader groupHeader = Declaration.JobComInvoiceGroupHeaders[0];
				JobComInvoiceHeader header = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

				header.JZ_IncoTerm = Core.Constants.IncoTerms.LandedIntoStore;
				header.JZ_InvoiceAmount = 12051.15m;
				header.JZ_RX_NKInvoice_Currency = AUDCurrency.RX_Code;

				Customs.Business.BaseJobComInvHeaderCharge oFT = groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 450, AUDCurrency.RX_Code);
				oFT.J7_IsIncludedInITOT = true;
				Customs.Business.BaseJobComInvHeaderCharge oNS = groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 120, AUDCurrency.RX_Code);
				oNS.J7_IsIncludedInITOT = true;
				Customs.Business.BaseJobComInvHeaderCharge lCH = groupHeader.Charges.AddNew(AUChargeCodeList.Codes.LandingCharges, 600, AUDCurrency.RX_Code);
				lCH.J7_IsIncludedInITOT = true;

				JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
				line.JI_Tariff = "6912.00.00 37";
				line.JI_Description = "MUG PACIFIC SKY";
				line.JI_LinePrice = 6435.72m;
				line.JI_CustomsQuantity = 2400.0m;
				line.JI_IsPackToBondForLine = true;

				line = header.JobComInvoiceLines.AddNew();
				line.JI_Tariff = "7013.29.00 35";
				line.JI_Description = "S/GLASS PACIFIC ST";
				line.JI_LinePrice = 2670.27m;
				line.JI_CustomsQuantity = 2448.0m;
				line.JI_IsPackToBondForLine = true;

				line = header.JobComInvoiceLines.AddNew();
				line.JI_Tariff = "8205.51.00 09";
				line.JI_Description = "METAL GOODS";
				line.JI_LinePrice = 2945.16m;
				line.JI_CustomsQuantity = 1200.0m;
				line.JI_IsPackToBondForLine = true;
			}
		}

		public void CreateEntryFromDeliveranceN10S_PAF()
		{
			Declaration = GetNewJobDec();
			using (Declaration.GetValidationSuspender())
			{
				Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

				Declaration.JE_DateOfFirstArrival = new ZDateTime(2001, 01, 28);
				Declaration.JE_ExportDate = new ZDateTime(2001, 01, 08);

				JobComInvoiceGroupHeader groupHeader = Declaration.JobComInvoiceGroupHeaders[0];
				JobComInvoiceHeader header = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

				header.JZ_IncoTerm = Core.Constants.IncoTerms.PackedAtFactory;
				header.JZ_InvoiceAmount = 42.1m;
				header.JZ_RX_NKInvoice_Currency = USDCurrency.RX_Code;

				Customs.Business.BaseJobComInvHeaderCharge charge = groupHeader.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 5.22m, USDCurrency.RX_Code);
				charge.J7_IsIncludedInITOT = false;

				JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
				line.JI_Tariff = "9999.40.26 24";
				line.JI_Description = "FLANNEL GRAPH";
				line.JI_LinePrice = 42.1m;
			}
		}

		public void CreateEntryFromDeliveranceN10_UCF()
		{
			Declaration = GetNewJobDec();
			using (Declaration.GetValidationSuspender())
			{
				Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;

				Declaration.JE_DateOfFirstArrival = new ZDateTime(2000, 10, 16);
				Declaration.JE_ExportDate = new ZDateTime(2000, 10, 08);

				JobComInvoiceGroupHeader groupHeader = Declaration.JobComInvoiceGroupHeaders[0];
				JobComInvoiceHeader header = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

				header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedCostAndFreight;
				header.JZ_InvoiceAmount = 66420.00m;
				header.JZ_RX_NKInvoice_Currency = USDCurrency.RX_Code;

				Customs.Business.BaseJobComInvHeaderCharge oFT = groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 2600m, USDCurrency.RX_Code);
				oFT.J7_IsIncludedInITOT = true;
				Customs.Business.BaseJobComInvHeaderCharge charge = groupHeader.Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 900m, USDCurrency.RX_Code);

				JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
				line.JI_Tariff = "9999.40.26 24";
				line.JI_Description = "FLANNEL GRAPH";
				line.JI_LinePrice = 66420.00m;
			}
		}

		//Entry Joo made up, if Deliverance has the entry with this incoterm, replace this with that.
		public void CreateEntryFromDeliveranceN10_UCI()
		{
			Declaration = GetNewJobDec();
			using (Declaration.GetValidationSuspender())
			{
				Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;

				Declaration.JE_DateOfFirstArrival = new ZDateTime(2000, 10, 16);
				Declaration.JE_ExportDate = new ZDateTime(2000, 10, 08);

				JobComInvoiceGroupHeader groupHeader = Declaration.JobComInvoiceGroupHeaders[0];
				JobComInvoiceHeader header = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

				header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedCostInsuranceAndFreight;
				header.JZ_InvoiceAmount = 66420.00m;
				header.JZ_RX_NKInvoice_Currency = USDCurrency.RX_Code;

				Customs.Business.BaseJobComInvHeaderCharge oNS = groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 2600m, USDCurrency.RX_Code);
				oNS.J7_IsIncludedInITOT = true;
				groupHeader.Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 900m, USDCurrency.RX_Code);

				JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
				line.JI_Tariff = "9999.40.26 24";
				line.JI_Description = "FLANNEL GRAPH";
				line.JI_LinePrice = 66420.00m;
			}
		}

		public void SetLineValues(JobComInvoiceLine line, string tariff, string stat)
		{
			line.JI_Tariff = tariff + " " + stat;
		}

		#region Implementation

		JobDeclaration GetNewJobDec()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			return declaration;
		}

		#endregion

	}
}
