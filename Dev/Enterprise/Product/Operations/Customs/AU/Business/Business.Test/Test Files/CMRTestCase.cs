using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class CMRTestCase : TestCaseWithFactory
	{
		internal JobComInvoiceHeader invoiceHeader;
		internal JobComInvoiceLine invoiceLine;
		internal JobDeclaration testDec;
		internal Bill houseBill1;
		internal Bill houseBill2;

		#region AUDCurrency

		internal RefCurrency AUDCurrency
		{
			get
			{
				if (fAUDCurrency == null)
				{
					fAUDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
				}
				return fAUDCurrency;
			}
		}

		RefCurrency fAUDCurrency;

		#endregion

		#region USDCurrency

		RefCurrency USDCurrency
		{
			get
			{
				if (fUSDCurrency == null)
				{
					fUSDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
				}
				return fUSDCurrency;
			}
		}

		RefCurrency fUSDCurrency;

		#endregion

		#region HKDCurrency

		internal RefCurrency HKDCurrency
		{
			get
			{
				if (fHKDCurrency == null)
				{
					fHKDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "HKD");
				}
				return fHKDCurrency;
			}
		}

		RefCurrency fHKDCurrency;

		#endregion

		internal CusEntryHeader GetCusEntryHeaderForAir()
		{
			testDec = GetBasicJobDeclaration();
			var flightNo = testDec.JE_VoyageFlightNo;
			testDec.JE_TransportMode = Core.Constants.TransportModes.Air;
			testDec.JE_VoyageFlightNo = flightNo;
			testDec.DoMerge();
			return testDec.CustomsEntryHeaders[0];
		}

		internal CusEntryHeader GetCusEntryHeaderForSea()
		{
			return GetCusEntryHeaderForSea(false, false);
		}

		internal CusEntryHeader GetCusEntryHeaderForSea(bool isForSAC, bool isForSACwithLines)
		{
			testDec = GetBasicJobDeclaration(isForSAC, isForSACwithLines);
			var voyage = testDec.JE_VoyageFlightNo;
			testDec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			testDec.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			testDec.JE_VoyageFlightNo = voyage;

			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, "9203473"));
			testDec.JE_VesselName = vessel.RV_Code;

			testDec.DoMerge();
			return testDec.CustomsEntryHeaders[0];
		}

		internal CusEntryHeader GetCusEntryHeaderForMail()
		{
			testDec = GetBasicJobDeclaration();
			testDec.JE_TransportMode = Core.Constants.TransportModes.Mail;
			testDec.DoMerge();
			return testDec.CustomsEntryHeaders[0];
		}

		internal OrgHeader GetWarehouseOrg()
		{
			OrgHeader warehouse = OrgHeader.New(Factory);
			warehouse.OH_FullName = "PJG WAREHOUSING & DISTRIBUTION PTY LTD";
			warehouse.OH_IsWarehouseClient = true;
			warehouse.MainAddress.OA_Address1 = "ADDRESS1";
			warehouse.OH_Code = "WARECODE";
			warehouse.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "WAREHOUSEID");

			return warehouse;
		}

		internal OrgHeader GetSupplierOrg()
		{
			OrgHeader supplier = OrgHeader.LoadFromCode(Factory, "ABABEU");
			supplier.CustomsClientID = "AAA3336347E";
			supplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.SupplierCode, "1234567V");

			return supplier;
		}

		internal OrgHeader GetImporterOrg()
		{
			OrgHeader importer = OrgHeader.New(Factory);
			importer.OH_Code = "IMPCODE";
			importer.MainAddress.OA_Address1 = "ADDRESS1";
			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, "IMPORTERID");
			importer.CustomsClientID = "55006646046";
			importer.LocalBusinessRegNo = "12345678901234";
			importer.MiscServ.OM_IMEFTBankAccount = "1234567890";
			importer.MiscServ.OM_IMEFTBankBSB = "1234567890";
			importer.OH_FullName = "Name of Importer";
			importer.OH_IsConsignee = true;
			importer.OH_RL_NKClosestPort = "AUSYD";

			return importer;
		}

		OrgAddress AddDeliveryAddressToImporter(OrgHeader importer)
		{
			OrgAddress deliveryAddress = importer.Addresses.AddNew();
			deliveryAddress.OA_Address1 = "12345678901234567890123456789012345678901234567890";
			deliveryAddress.OA_Address2 = "12345678901234567890123456789012345678901234567890";
			deliveryAddress.AddressCapability.SetCapabilityEnabled(nameof(AddressType.PIC));
			deliveryAddress.OA_City = "1234567890123456789012345";
			deliveryAddress.OA_State = "NSW";
			deliveryAddress.OA_PostCode = "1234567890";

			return deliveryAddress;
		}

		internal JobDeclaration GetBasicJobDeclaration()
		{
			return GetBasicJobDeclaration(false, false);
		}

		JobDeclaration GetBasicJobDeclaration(bool isForSAC, bool isForSACwithLines)
		{
			testDec = JobDeclaration.New(Factory);
			testDec.DisableDefaultPackingInformation = true;
			PopulateWithValidTestData(testDec);

			testDec.JE_HouseBill = "AQT1HBL";
			testDec.JE_MasterBill = "AQT1";
			houseBill1 = testDec.Bills.FindByBillNumberAndType("AQT1HBL", Customs.Business.BillTypeList.Codes.HouseBill);

			if (!isForSAC && !isForSACwithLines)
			{
				houseBill2 = testDec.Bills.AddNew();
				houseBill2.CU_HouseBill = "AQT2HBL";
				houseBill2.CU_MasterBill = "AQT1";
			}

			invoiceHeader = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			PopulateWithValidTestData(invoiceHeader, true);

			if (!isForSAC)
			{
				invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				PopulateWithValidTestData(invoiceLine);
			}

			if (isForSAC || isForSACwithLines)
			{
				testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				testDec.JE_MessageSubType = isForSAC ? JobDeclaration.MessageSubType.SelfAssessedClearance : JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			}

			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer();
			testDec.MessageInitiator = sender;
			return testDec;
		}

		internal void PopulateWithValidTestData(JobDeclaration testDec)
		{
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_DeclarationReference = "B9999999";
			testDec.JE_MessageType = "IMP";
			testDec.JE_OH_Importer = GetImporterOrg().PK;
			testDec.JE_VoyageFlightNo = "QF1234";
			testDec.JE_RL_NKFinalDestination = "AUMEL";
			testDec.JE_RL_NKPortOfLoading = "SGSIN";
			testDec.JE_RL_NKPortOfArrival = "AUSYD";
			testDec.JE_RL_NKPortOfFirstArrival = "AUSYD";
			testDec.WarehouseDocAddress.E2_OA_Address = GetWarehouseOrg().Addresses[0].PK;
			testDec.JE_DateOfArrival = new ZDateTime(2010, 01, 01);
			testDec.JE_DateOfFirstArrival = new ZDateTime(2010, 01, 01);
			testDec.JE_ExportDate = ZDateTime.Today;
			testDec.JE_DateAtFinalDestination = new ZDateTime(2010, 01, 01);
			testDec.JE_TotalNoOfPacks = 123;
			testDec.JE_TotalNoOfPacksPackType = Core.Constants.PkgUnit.Piece;
			testDec.JE_MasterBill = "AQT1";
			testDec.JE_OwnerRef = "OWNERS REF";
			testDec.ImporterDeliveryAddress.E2_OA_Address = AddDeliveryAddressToImporter(testDec.Importer).PK;
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Importer;

			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer();
			testDec.MessageInitiator = sender;
		}

		internal void PopulateWithValidTestData(JobComInvoiceHeader invoiceHeader, bool addCharges)
		{
			invoiceHeader.JZ_InvoiceNumber = "InvNum";
			invoiceHeader.JZ_Weight = 123.45M;
			invoiceHeader.JZ_WeightUQ = "KG";
			invoiceHeader.AddInfo.ZA_EFD = "070604";
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_RX_NKInvoice_Currency = USDCurrency.RX_Code;
			invoiceHeader.JZ_InvoiceAmount = 987.65M;
			invoiceHeader.JZ_OH_Supplier = GetSupplierOrg().PK;
			invoiceHeader.JZ_Nature10PackCount = 987;
			invoiceHeader.JZ_PiecesForRelease = 100;
			invoiceHeader.AddInfo.ZA_ORG = "JP";

			if (addCharges)
			{
				InvoiceCharge pACCharge = invoiceHeader.Charges.AddNew();
				pACCharge.J7_ChargeType = AUChargeCodeList.Codes.PackingCost;
				pACCharge.J7_Amount = 123.45M;
				pACCharge.J7_RX_NKCurrency = AUDCurrency.RX_Code;
				pACCharge.J7_IsIncludedInITOT = true;

				InvoiceCharge fIFCharge = invoiceHeader.Charges.AddNew();
				fIFCharge.J7_ChargeType = AUChargeCodeList.Codes.ForeignInlandFreight;
				fIFCharge.J7_Amount = 678.90M;
				fIFCharge.J7_RX_NKCurrency = "ZAR";
				fIFCharge.J7_IsIncludedInITOT = true;

				InvoiceCharge cOMCharge = invoiceHeader.Charges.AddNew();
				cOMCharge.J7_ChargeType = Enterprise.Customs.Business.CustomsChargeTypeList.Codes.Commission;
				cOMCharge.J7_Amount = 60.60M;
				cOMCharge.J7_RX_NKCurrency = AUDCurrency.RX_Code;
				cOMCharge.J7_IsIncludedInITOT = true;

				InvoiceCharge dISCharge = invoiceHeader.Charges.AddNew();
				dISCharge.J7_ChargeType = AUChargeCodeList.Codes.Discount;
				dISCharge.J7_Amount = 10M;
				dISCharge.J7_RX_NKCurrency = USDCurrency.RX_Code;
				dISCharge.J7_IsIncludedInITOT = true;

				InvoiceCharge oTHCharge = invoiceHeader.Charges.AddNew();
				oTHCharge.J7_ChargeType = AUChargeCodeList.Codes.OtherCharges;
				oTHCharge.J7_Amount = 45.89M;
				oTHCharge.J7_RX_NKCurrency = AUDCurrency.RX_Code;
				oTHCharge.J7_IsIncludedInITOT = true;
			}
		}

		internal void PopulateWithValidTestData(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.AddInfo.ZA_IsPackToBondForLine_Hidden = "N";
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_Description = "Description of the goods for line 1";
			invoiceLine.JI_LinePrice = 997.65M;
			invoiceLine.JI_Tariff = "4410.90.00 25";
			invoiceLine.JI_CustomsUnitQty = "CU";
			invoiceLine.JI_CustomsQuantity = 5.00M;
			invoiceLine.AddInfo.ZA_VALB_Hidden = "UT";
			invoiceLine.AddInfo.AdjustmentDollarPercentage_Hidden = "$";
			invoiceLine.AddInfo.AdjustmentAmount_Hidden = 456.78M;
			invoiceLine.AddInfo.AdjustmentCurrency_Hidden = USDCurrency.RX_Code;
		}

		internal string longNoteValue = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
			"1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
			"12345678901234567890123456789012345678901234567890";

		#region Containers

		internal CusContainer container1;
		internal CusContainer container2;
		internal CusContainer container3;
		internal Package packageForContainer1;
		internal Package packageForContainer2;
		internal Package packageForContainer2SecondHBL;
		internal Package packageForContainer3;

		internal void AddContainersAndPacksToDeclaration()
		{
			container1 = testDec.CusContainers.AddNew();
			container1.CO_ContainerNumber = "OCLU10000020";
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			packageForContainer1 = testDec.Packages.AddNew();
			packageForContainer1.CW_HouseBill = houseBill1.CU_BillUniqueCode;
			packageForContainer1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			packageForContainer1.CW_PackQty = 100;
			packageForContainer1.CW_OuterPacks = 1;
			packageForContainer1.PackingGroup.CR_HouseContainerNumber = 1;

			container2 = testDec.CusContainers.AddNew();
			container2.CO_ContainerNumber = "OCLU10000031";
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			packageForContainer2 = testDec.Packages.AddNew();
			packageForContainer2.CW_HouseBill = houseBill1.CU_BillUniqueCode;
			packageForContainer2.CW_ContainerNoOrEquipmentNo = container2.CO_ContainerNumber;
			packageForContainer2.CW_PackQty = 200;
			packageForContainer2.CW_OuterPacks = 2;
			packageForContainer2.PackingGroup.CR_HouseContainerNumber = 2;

			packageForContainer2SecondHBL = testDec.Packages.AddNew();
			packageForContainer2SecondHBL.CW_HouseBill = houseBill2.CU_BillUniqueCode;
			packageForContainer2SecondHBL.CW_ContainerNoOrEquipmentNo = container2.CO_ContainerNumber;
			packageForContainer2SecondHBL.CW_PackQty = 300;
			packageForContainer2SecondHBL.CW_OuterPacks = 3;
			packageForContainer2SecondHBL.PackingGroup.CR_HouseContainerNumber = 3;

			container3 = testDec.CusContainers.AddNew();
			container3.CO_ContainerNumber = "OCLU10000042";
			container3.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			packageForContainer3 = testDec.Packages.AddNew();
			packageForContainer3.CW_HouseBill = houseBill1.CU_BillUniqueCode;
			packageForContainer3.CW_ContainerNoOrEquipmentNo = container3.CO_ContainerNumber;
			packageForContainer3.CW_PackQty = 400;
			packageForContainer3.CW_OuterPacks = 4;
			packageForContainer3.PackingGroup.CR_HouseContainerNumber = 4;

			testDec.DoMerge();
		}

		#endregion
	}
}
