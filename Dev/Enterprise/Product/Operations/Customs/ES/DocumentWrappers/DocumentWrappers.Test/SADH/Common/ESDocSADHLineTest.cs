using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentWrappers.Customs.EU;
using Enterprise.DocumentWrappers.Customs.EU.Testing;
using static Enterprise.Customs.ES.Business.ESConstants;
using ESCusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;
using JobDeclaration = Enterprise.Customs.ES.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH.Testing
{
	[MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Spain)]
	abstract class ESDocSADHLineTest : DocSADHLineTest
	{
		public new void TestBox31Contents()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Description = "MOTORES DE EMBOLO ALTERNATIVO O ROTATIVO, DE ENCENDIDO POR CHISPA (MOTORES DE EXPLOSION), DE CILINDRADA IGUAL O SUPERIOR A 300€CM3€Y POTENCIA IGUAL O SUPERIOR A 6€KW, PERO INFERIOR O IGUAL A 20,0€KW, PARA LA FABRICACION: - DE CORTADORAS DE CESPED DE LAS SUBPARTIDAS 8433€11, 8433€19€Y 8433€20, DE TRACTORES DE LAS SUBPARTIDAS 8701€91€90€Y 8701€92€90€CUYA FUNCION PRINCIPAL SEA LA DE CORTAR EL CESPED-DE CORTADORAS CON UN MOTOR DE CUATRO TIEMPOS DE UNA CILINDRADA IGUAL O SUPERIOR A 300€CM³ DE LA SUBPARTIDA 8433€20€10€O - ";

			CombineAssertions(() =>
			{
				Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
				var entryHeader = declaration.CustomsEntryHeaders[0];
				var entryLine = entryHeader.MergedLines[0];

				declaration.JE_MasterBill = "X";
				var bill = declaration.PrimaryMasterBill;
				var cw1 = bill.PackingGroups[0].Packages[0];
				cw1.CW_PackQty = 10;
				cw1.CW_PackType = "BX";
				cw1.CW_MarksAndNos = "123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789";
				var cw2 = bill.PackingGroups[0].Packages.AddNew();
				cw2.CW_PackQty = 11;
				cw2.CW_PackType = "NE";
				cw2.CW_MarksAndNos = "NO MARKS";

				var packing1 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
				packing1.IsLinked = true;
				packing1.PackQty = 6;

				var packing2 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[1];
				packing2.IsLinked = true;
				packing2.PackQty = 7;

				var line = GetNewDocSADHLine(entryLine);

				string expected = @"6 BX, 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789. 7 NE, NO MARKS.
--------------------------------------------------------------------------------
MOTORES DE EMBOLO ALTERNATIVO O ROTATIVO, DE ENCENDIDO POR CHISPA (MOTORES DE EXPLOSION), DE CILINDRADA IGUAL O SUPERIOR A";
				AssertEquals("Box31 only first 1254 chars of complete text are shown", expected, line.Box31PackagesAndDescriptionOfGoods);

				cw1.CW_MarksAndNos = "RTDAS";
				line = GetNewDocSADHLine(entryLine);

				expected = @"6 BX, RTDAS. 7 NE, NO MARKS.
--------------------------------------------------------------------------------
MOTORES DE EMBOLO ALTERNATIVO O ROTATIVO, DE ENCENDIDO POR CHISPA (MOTORES DE EXPLOSION), DE CILINDRADA IGUAL O SUPERIOR A 300€CM3€Y POTENCIA IGUAL O SUPERIOR A 6€KW, PERO INFERIOR O IGUAL A 20,0€KW, PARA LA FABRICACION: - DE CORTADORAS DE CESPED DE LAS SUBPARTIDAS 8433€11, 8433€19€Y 8433€20, DE TRACTORES DE LAS SUBPARTIDAS 8701€91€90€Y 8701€92€90€CUYA FUNCION PRINCIPAL SEA LA DE CORTAR EL CESPED-DE CORTADORAS CON UN MOTOR DE CUATRO TIEMPOS DE UNA CILINDRADA IGUAL O SUPERIOR A 300€CM³ DE LA SUBPARTIDA 8433€";
				AssertEquals("Box31 without containers", expected, line.Box31PackagesAndDescriptionOfGoods);

				var firstContainer = declaration.CusContainers.AddNew();
				firstContainer.CO_ContainerNumber = "OOCL3219032";
				firstContainer.CO_Seal = "SEAL01";

				var secondContainer = declaration.CusContainers.AddNew();
				secondContainer.CO_ContainerNumber = "OOCL3127895";
				secondContainer.CO_Seal = "SEAL02";
				secondContainer.CO_SecondSeal = "SEAL03";
				secondContainer.AdditionalSeals.AddNew().BK_SealNumber = "SEAL04";
				secondContainer.AdditionalSeals.AddNew().BK_SealNumber = "SEAL05";

				invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
				entryHeader.ZG_UCC6Version = UCC6VersionCodes.NoUCC6;
				line = GetNewDocSADHLine(entryLine);

				var expectedNoUCC6OrImport = @"6 BX, RTDAS. 7 NE, NO MARKS. 1 CONTENEDOR OOCL3219032.
--------------------------------------------------------------------------------
MOTORES DE EMBOLO ALTERNATIVO O ROTATIVO, DE ENCENDIDO POR CHISPA (MOTORES DE EXPLOSION), DE CILINDRADA IGUAL O SUPERIOR A 300€CM3€Y POTENCIA IGUAL O SUPERIOR A 6€KW, PERO INFERIOR O IGUAL A 20,0€KW, PARA LA FABRICACION: - DE CORTADORAS DE CESPED DE LAS SUBPARTIDAS 8433€11, 8433€19€Y 8433€20, DE TRACTORES DE LAS SUBPARTIDAS 8701€91€90€Y 8701€92€90€CUYA FUNCION PRINCIPAL SEA LA DE CORTAR EL CESPED-DE CORTADORAS CON UN MOTOR DE CUATRO TIEMPOS DE UNA CILINDRADA IGUAL O SUPERIOR A 300€CM³ DE LA SUBPARTIDA 8433€";
				AssertEquals("Box31 with 1 container (No UCC6 and Export)", expectedNoUCC6OrImport, line.Box31PackagesAndDescriptionOfGoods);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				line = GetNewDocSADHLine(entryLine);
				AssertEquals("Box31 with 1 container (No UCC6 and Import)", expectedNoUCC6OrImport, line.Box31PackagesAndDescriptionOfGoods);

				entryHeader.ZG_UCC6Version = UCC6VersionCodes.UCC6;
				line = GetNewDocSADHLine(entryLine);
				AssertEquals("Box31 with 1 container (UCC6 and Import)", expectedNoUCC6OrImport, line.Box31PackagesAndDescriptionOfGoods);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				line = GetNewDocSADHLine(entryLine);
				var expectedUCC6AndExport = @"6 BX, RTDAS. 7 NE, NO MARKS. 1 CONTENEDOR OOCL3219032 - SEAL01.
--------------------------------------------------------------------------------
MOTORES DE EMBOLO ALTERNATIVO O ROTATIVO, DE ENCENDIDO POR CHISPA (MOTORES DE EXPLOSION), DE CILINDRADA IGUAL O SUPERIOR A 300€CM3€Y POTENCIA IGUAL O SUPERIOR A 6€KW, PERO INFERIOR O IGUAL A 20,0€KW, PARA LA FABRICACION: - DE CORTADORAS DE CESPED DE LAS SUBPARTIDAS 8433€11, 8433€19€Y 8433€20, DE TRACTORES DE LAS SUBPARTIDAS 8701€91€90€Y 8701€92€90€CUYA FUNCION PRINCIPAL SEA LA DE CORTAR EL CESPED-DE CORTADORAS CON UN MOTOR DE CUATRO TIEMPOS DE UNA CILINDRADA IGUAL O SUPERIOR A 300€CM³ DE LA SUBPARTIDA 8433€";
				AssertEquals("Box31 with 1 container (UCC6 and Export)", expectedUCC6AndExport, line.Box31PackagesAndDescriptionOfGoods);

				invoiceLine.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = true;

				entryHeader.ZG_UCC6Version = UCC6VersionCodes.NoUCC6;
				line = GetNewDocSADHLine(entryLine);
				expectedNoUCC6OrImport = @"6 BX, RTDAS. 7 NE, NO MARKS. 2 CONTENEDORES OOCL3219032, OOCL3127895.
--------------------------------------------------------------------------------
MOTORES DE EMBOLO ALTERNATIVO O ROTATIVO, DE ENCENDIDO POR CHISPA (MOTORES DE EXPLOSION), DE CILINDRADA IGUAL O SUPERIOR A 300€CM3€Y POTENCIA IGUAL O SUPERIOR A 6€KW, PERO INFERIOR O IGUAL A 20,0€KW, PARA LA FABRICACION: - DE CORTADORAS DE CESPED DE LAS SUBPARTIDAS 8433€11, 8433€19€Y 8433€20, DE TRACTORES DE LAS SUBPARTIDAS 8701€91€90€Y 8701€92€90€CUYA FUNCION PRINCIPAL SEA LA DE CORTAR EL CESPED-DE CORTADORAS CON UN MOTOR DE CUATRO TIEMPOS DE UNA CILINDRADA IGUAL O SUPERIOR A 300€CM³ DE LA SUBPARTIDA 8433€";
				AssertEquals("Box31 with 2 containers (No UCC6 and Export)", expectedNoUCC6OrImport, line.Box31PackagesAndDescriptionOfGoods);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				line = GetNewDocSADHLine(entryLine);
				AssertEquals("Box31 with 2 containers (No UCC6 and Import)", expectedNoUCC6OrImport, line.Box31PackagesAndDescriptionOfGoods);

				entryHeader.ZG_UCC6Version = UCC6VersionCodes.UCC6;
				line = GetNewDocSADHLine(entryLine);
				AssertEquals("Box31 with 2 containers (UCC6 and Import)", expectedNoUCC6OrImport, line.Box31PackagesAndDescriptionOfGoods);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				line = GetNewDocSADHLine(entryLine);
				expectedUCC6AndExport = @"6 BX, RTDAS. 7 NE, NO MARKS. 2 CONTENEDORES OOCL3219032 - SEAL01, OOCL3127895 - SEAL02/SEAL03/SEAL04/SEAL05.
--------------------------------------------------------------------------------
MOTORES DE EMBOLO ALTERNATIVO O ROTATIVO, DE ENCENDIDO POR CHISPA (MOTORES DE EXPLOSION), DE CILINDRADA IGUAL O SUPERIOR A 300€CM3€Y POTENCIA IGUAL O SUPERIOR A 6€KW, PERO INFERIOR O IGUAL A 20,0€KW, PARA LA FABRICACION: - DE CORTADORAS DE CESPED DE LAS SUBPARTIDAS 8433€11, 8433€19€Y 8433€20, DE TRACTORES DE LAS SUBPARTIDAS 8701€91€90€Y 8701€92€90€CUYA FUNCION PRINCIPAL SEA LA DE CORTAR EL CESPED-DE CORTADORAS CON UN MOTOR DE CUATRO TIEMPOS DE UNA CILINDRADA IGUAL O SUPERIOR A 300€CM³ DE LA SUBPARTIDA 8433€";
				AssertEquals("Box31 with 2 containers (UCC6 and Export)", expectedUCC6AndExport, line.Box31PackagesAndDescriptionOfGoods);

				packing2.IsLinked = false;
				entryHeader.ZG_UCC6Version = UCC6VersionCodes.NoUCC6;
				line = GetNewDocSADHLine(entryLine);
				expectedNoUCC6OrImport = "6 BX, RTDAS. 2 CONTENEDORES OOCL3219032, OOCL3127895";
				AssertContains("Box31 with only one package (No UCC6 and Export)", expectedNoUCC6OrImport, line.Box31PackagesAndDescriptionOfGoods);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				line = GetNewDocSADHLine(entryLine);
				AssertContains("Box31 with only one package (No UCC6 and Import)", expectedNoUCC6OrImport, line.Box31PackagesAndDescriptionOfGoods);

				entryHeader.ZG_UCC6Version = UCC6VersionCodes.UCC6;
				line = GetNewDocSADHLine(entryLine);
				AssertContains("Box31 with only one package (UCC6 and Import)", expectedNoUCC6OrImport, line.Box31PackagesAndDescriptionOfGoods);

				packing2.IsLinked = false;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				line = GetNewDocSADHLine(entryLine);
				AssertContains("Box31 with only one package (UCC6 and Export)", "6 BX, RTDAS. 2 CONTENEDORES OOCL3219032 - SEAL01, OOCL3127895 - SEAL02/SEAL03/SEAL04/SEAL05", line.Box31PackagesAndDescriptionOfGoods);

				var vehicle = invoiceLine.Vehicles.AddNew();
				vehicle.CVH_VehicleIdentificationNumber = "12345";
				vehicle.CVH_ModelName = "FIESTA";
				vehicle.CVH_BrandName = "FORD";

				entryHeader.ZG_UCC6Version = UCC6VersionCodes.NoUCC6;
				line = GetNewDocSADHLine(entryLine);
				expectedNoUCC6OrImport = @"1 BASTIDORES, 12345 FORD FIESTA. 2 CONTENEDORES OOCL3219032, OOCL3127895.
--------------------------------------------------------------------------------
MOTORES DE EMBOLO ALTERNATIVO O ROTATIVO, DE ENCENDIDO POR CHISPA (MOTORES DE EXPLOSION), DE CILINDRADA IGUAL O SUPERIOR A 300€CM3€Y POTENCIA IGUAL O SUPERIOR A 6€KW, PERO INFERIOR O IGUAL A 20,0€KW, PARA LA FABRICACION: - DE CORTADORAS DE CESPED DE LAS SUBPARTIDAS 8433€11, 8433€19€Y 8433€20, DE TRACTORES DE LAS SUBPARTIDAS 8701€91€90€Y 8701€92€90€CUYA FUNCION PRINCIPAL SEA LA DE CORTAR EL CESPED-DE CORTADORAS CON UN MOTOR DE CUATRO TIEMPOS DE UNA CILINDRADA IGUAL O SUPERIOR A 300€CM³ DE LA SUBPARTIDA 8433€";
				AssertEquals("Box31 with 1 vehicle and 2 containers (No UCC6 and Export)", expectedNoUCC6OrImport, line.Box31PackagesAndDescriptionOfGoods);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				line = GetNewDocSADHLine(entryLine);
				AssertEquals("Box31 with 1 vehicle and 2 containers (No UCC6 and Import)", expectedNoUCC6OrImport, line.Box31PackagesAndDescriptionOfGoods);

				entryHeader.ZG_UCC6Version = UCC6VersionCodes.UCC6;
				line = GetNewDocSADHLine(entryLine);
				AssertEquals("Box31 with 1 vehicle and 2 containers (UCC6 and Import)", expectedNoUCC6OrImport, line.Box31PackagesAndDescriptionOfGoods);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				line = GetNewDocSADHLine(entryLine);
				expectedUCC6AndExport = @"1 BASTIDORES, 12345 FORD FIESTA. 2 CONTENEDORES OOCL3219032 - SEAL01, OOCL3127895 - SEAL02/SEAL03/SEAL04/SEAL05.
--------------------------------------------------------------------------------
MOTORES DE EMBOLO ALTERNATIVO O ROTATIVO, DE ENCENDIDO POR CHISPA (MOTORES DE EXPLOSION), DE CILINDRADA IGUAL O SUPERIOR A 300€CM3€Y POTENCIA IGUAL O SUPERIOR A 6€KW, PERO INFERIOR O IGUAL A 20,0€KW, PARA LA FABRICACION: - DE CORTADORAS DE CESPED DE LAS SUBPARTIDAS 8433€11, 8433€19€Y 8433€20, DE TRACTORES DE LAS SUBPARTIDAS 8701€91€90€Y 8701€92€90€CUYA FUNCION PRINCIPAL SEA LA DE CORTAR EL CESPED-DE CORTADORAS CON UN MOTOR DE CUATRO TIEMPOS DE UNA CILINDRADA IGUAL O SUPERIOR A 300€CM³ DE LA SUBPARTIDA 8433€";
				AssertEquals("Box31 with 1 vehicle and 2 containers (UCC6 and Export)", expectedUCC6AndExport, line.Box31PackagesAndDescriptionOfGoods);

				packing1.PackQty = 0;
				invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = false;
				invoiceLine.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = false;

				invoiceLine.Vehicles.Delete();
				line = GetNewDocSADHLine(entryLine);
				expected = @"0 BX, RTDAS.
--------------------------------------------------------------------------------
MOTORES DE EMBOLO ALTERNATIVO O ROTATIVO, DE ENCENDIDO POR CHISPA (MOTORES DE EXPLOSION), DE CILINDRADA IGUAL O SUPERIOR A 300€CM3€Y POTENCIA IGUAL O SUPERIOR A 6€KW, PERO INFERIOR O IGUAL A 20,0€KW, PARA LA FABRICACION: - DE CORTADORAS DE CESPED DE LAS SUBPARTIDAS 8433€11, 8433€19€Y 8433€20, DE TRACTORES DE LAS SUBPARTIDAS 8701€91€90€Y 8701€92€90€CUYA FUNCION PRINCIPAL SEA LA DE CORTAR EL CESPED-DE CORTADORAS CON UN MOTOR DE CUATRO TIEMPOS DE UNA CILINDRADA IGUAL O SUPERIOR A 300€CM³ DE LA SUBPARTIDA 8433€";
				AssertEquals("Box31 without packages", expected, line.Box31PackagesAndDescriptionOfGoods);
			});
		}

		public void TestBox35GrossWeightInKG()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			invoiceLine.JI_Weight = 166.75m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Pounds;

			CombineAssertions(() =>
			{
				Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
				var entryHeader = declaration.CustomsEntryHeaders[0];
				var entryLine = entryHeader.MergedLines[0];

				entryHeader.ZG_UCC6Version = UCC6VersionCodes.NoUCC6;

				var line = GetNewDocSADHLine(entryLine);
				AssertEquals("weight is calculated correctly for kilograms (H2)", "75.637", line.Box35GrossWeightInKG);

				invoiceLine.JI_Weight = 160222.7777m;
				invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
				line = GetNewDocSADHLine(entryLine);
				AssertEquals("Format is correct, weight > 1 (H2)", "160,222.778", line.Box35GrossWeightInKG);

				invoiceLine.JI_Weight = 0.12345678m;
				invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
				line = GetNewDocSADHLine(entryLine);
				AssertEquals("Format is correct, weight < 1 (H2)", "0.123", line.Box35GrossWeightInKG);

				entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.IM;

				invoiceLine.JI_Weight = 100.00m;
				invoiceLine.JI_WeightUQ = Core.Constants.Weight.Pounds;
				line = GetNewDocSADHLine(entryLine);
				AssertEquals("weight is calculated correctly for kilograms (no UCC6)", "46", line.Box35GrossWeightInKG);

				invoiceLine.JI_Weight = 111222.666m;
				invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
				line = GetNewDocSADHLine(entryLine);
				AssertEquals("Format is correct, weight > 1 (no UCC6)", "111,223", line.Box35GrossWeightInKG);

				invoiceLine.JI_Weight = 222222.222m;
				invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
				line = GetNewDocSADHLine(entryLine);
				AssertEquals("Format is correct, weight > 1 no round (no UCC6)", "222,223", line.Box35GrossWeightInKG);

				invoiceLine.JI_Weight = 0.111222333m;
				invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
				line = GetNewDocSADHLine(entryLine);
				AssertEquals("Format is correct, weight < 1 (no UCC6)", "0.111", line.Box35GrossWeightInKG);

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				entryHeader.ZG_UCC6Version = UCC6VersionCodes.UCC6;

				invoiceLine.JI_Weight = 153.33m;
				invoiceLine.JI_WeightUQ = Core.Constants.Weight.Pounds;
				line = GetNewDocSADHLine(entryLine);
				AssertEquals("weight is calculated correctly for kilograms (UCC6)", "69.549", line.Box35GrossWeightInKG);

				invoiceLine.JI_Weight = 111222.6667m;
				invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
				line = GetNewDocSADHLine(entryLine);
				AssertEquals("Format is correct, weight > 1 (UCC6)", "111,222.667", line.Box35GrossWeightInKG);

				invoiceLine.JI_Weight = 0.111222333m;
				invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
				line = GetNewDocSADHLine(entryLine);
				AssertEquals("Format is correct, weight < 1 (UCC6)", "0.111", line.Box35GrossWeightInKG);
			});
		}

		public void TestBox37Procedure()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "4000APC";

			CombineAssertions(() =>
			{
				Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
				var entryHeader = declaration.CustomsEntryHeaders[0];
				var entryLine = entryHeader.MergedLines[0];

				var line = GetNewDocSADHLine(entryLine);
				AssertEquals("Box37Procedure correct format", "40.00 | APC", line.Box37Procedure);
			});
		}

		public void TestBox37_2Procedure()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "4000APC";

			CombineAssertions(() =>
			{
				Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
				var entryHeader = declaration.CustomsEntryHeaders[0];
				var entryLine = entryHeader.MergedLines[0];

				var line = GetNewDocSADHLine(entryLine);
				AssertEquals("Box37_2Procedure empty when no AdditionalProcedureCodes are declared", ZString.Empty, line.Box37_2Procedure);

				invoiceLine.AdditionalProcedureCodes.AddNew("456F89");
				invoiceLine.AdditionalProcedureCodes.AddNew("789100");
				line = GetNewDocSADHLine(entryLine);
				AssertEquals("Box37_2Procedure has correct data when AdditionalProcedureCodes are declared", "F89 100", line.Box37_2Procedure);
			});
		}

		public void TestBox38NetWeightInKG()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_NetWeight = 5100.0512m;

			CombineAssertions(() =>
			{
				Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
				var entryHeader = declaration.CustomsEntryHeaders[0];
				var entryLine = entryHeader.MergedLines[0];

				var line = GetNewDocSADHLine(entryLine);
				AssertEquals("Box38NetWeight takes the value from JI_NetWeight and with correct format", "5,100.051", line.Box38NetWeightInKG);

				invoiceLine.JI_NetWeight = 100m;
				line = GetNewDocSADHLine(entryLine);
				AssertEquals("Box38NetWeight takes the value from JI_NetWeight and with correct format", "100", line.Box38NetWeightInKG);
			});
		}

		public void TestBox41ES()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, MapDirectionList.Codes.BTH, "Description", false);
			helper.CreateCusMapType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ImportAddDocAdditionalInformation, MapDirectionList.Codes.BTH, "Description", false);
			helper.CreateCusMap(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ImportAddDocAdditionalInformation, "KGM", "KN1", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);
			helper.CreateCusMap(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KGM", "KN2", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Brazil);
			helper.CreateCusMap(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KGM", "KN", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsSecondQuantity = 1005.3465657m;
			invoiceLine.JI_CustomsSecondUnitQty = "ABC";

			CombineAssertions("Assert properties", () =>
			{
				Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
				var entryHeader = declaration.CustomsEntryHeaders[0];
				var entryLine = entryHeader.MergedLines[0];

				var line = GetNewDocSADHLine(entryLine);
				AssertEquals("41 Supplementary Units", ExpectedBox41SupplementaryUnitsWithDecimals, line.Box41SupplementaryUnits);
				AssertEquals("41 Supplementary Quantity", ExpectedBox41SupplementaryQtyWithDecimals, line.Box41SupplementaryQty);
				AssertEquals("41 Supplementary UQ Desc", ZString.Empty, line.Box41SupplementaryUQDescription);

				invoiceLine.JI_CustomsSecondQuantity = 1000.00m;
				invoiceLine.JI_CustomsSecondUnitQty = "KGM";
				line = GetNewDocSADHLine(entryLine);
				AssertEquals("41 Supplementary Units", "1,000 KN", line.Box41SupplementaryUnits);
				AssertEquals("41 Supplementary Quantity", "1,000", line.Box41SupplementaryQty);
				AssertEquals("41 Supplementary UQ Desc", ZString.Empty, line.Box41SupplementaryUQDescription);
			});
		}

		protected SupportingDocument AddSupDoc(ZGuid parentPK, ZString parentTableCode, ZString code, ZString refNum, ZDateTime dateOfExpiry, ZString status, string subtype = "")
		{
			var suppDoc = Factory.New<SupportingDocument>();
			suppDoc.CSI_ParentID = parentPK;
			suppDoc.CSI_ParentTableCode = parentTableCode;
			suppDoc.CSI_Code = code;
			suppDoc.CSI_ReferenceNumber = refNum;
			suppDoc.CSI_DateOfExpiry = dateOfExpiry;
			suppDoc.CSI_Status = status;
			suppDoc.CSI_SubType = subtype;
			return suppDoc;
		}

		protected AdditionalInfo AddAddInf(ZGuid parentPK, ZString parentTableCode, ZString code, ZString refNum, string subtype = "TRA", string status = "")
		{
			var addInf = Factory.New<AdditionalInfo>();
			addInf.CSI_ParentID = parentPK;
			addInf.CSI_ParentTableCode = parentTableCode;
			addInf.CSI_Code = code;
			addInf.CSI_ReferenceNumber = refNum;
			addInf.CSI_SubType = subtype;
			addInf.CSI_Status = status;
			return addInf;
		}

		protected Business.CusAuthorizationUsage AddAuthorization(ZGuid parentPK, ZString parentTableCode, ZGuid holderPK, ZString authType, ZString authNumber)
		{
			var authorisation = Factory.New<CusAuthorisationHeader>();
			authorisation.CPH_OH_PermitHolder = holderPK;
			authorisation.CPH_Type = authType;
			authorisation.CPH_StartDate = new ZDate(2021, 11, 03);
			authorisation.CPH_Number = authNumber;

			var auth = Factory.New<Business.CusAuthorizationUsage>();
			auth.AGC_ParentID = parentPK;
			auth.AGC_ParentTableCode = parentTableCode;
			auth.AGC_Code = authType;
			auth.AGC_Number = authNumber;
			auth.AGC_OH_Owner = holderPK;
			return auth;
		}

		protected void SetUpMapData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
			helper.CreateCusCodeType("AUTH", "Authorisation");
			helper.CreateCusCodeList("EUN", "AUTH", "SAS", "Self-Assessment", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusMapType("EUNAU", "OUT", "Authorisation Codes to Document Type Code", true);
			helper.CreateCusMap("EUNAU", "DPO", "C506", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "EUN");
			helper.CreateCusMap("EUNAU", "SAS", "C515", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "EUN");
			helper.CreateCusMap("EUNAU", "SAS", "CSAS", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, Core.Constants.CountryCodes.Spain);
			helper.CreateCusMap("EUNAU", "OTE", "CES", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, Core.Constants.CountryCodes.Spain);
			helper.CreateCusMap("EUNAU", "OTI", "CIT", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, Core.Constants.CountryCodes.Italy);
			Factory.Save();
		}

		protected override DocSADHLine GetNewDocSADHLine(EU.Business.Declaration.CusEntryLine entryLine)
			=> ESDocSADHLine.New((ESCusEntryLine)entryLine, Factory);

		protected override DocSADHLine GetSADHLineForBox47Taxes() => GetSADHLineWithoutTaxations();

		protected override DocSADHLine GetSADHLineForBox47TaxesOrder() => GetSADHLineWithoutTaxations();

		DocSADHLine GetSADHLineWithoutTaxations()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
			Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];

			return GetNewDocSADHLine(entryLine);
		}

		protected abstract ZString ExpectedBox41SupplementaryUnitsWithDecimals { get; }
		protected abstract ZString ExpectedBox41SupplementaryQtyWithDecimals { get; }

		protected override void AssertBox47Taxes(DocSADHLineTaxCollection taxCollection)
		{
			AssertNotNull(taxCollection);
			AssertEquals("TaxCollection count", 0, taxCollection.Count);
		}
		protected override ZString ExpectedProcedure => "10.00 | 000";
		protected override ZString ExpectedAdditionalProcedure => "F89";
		protected override ZString ExpectedGrossMassForCommericalPurposesOnly => "45.359";
		protected override ZString ExpectedGrossMass => Math.Ceiling(45.359).ToString();
		protected override ZString ExpectedNetMass => "1.417";
		protected override ZBool ExpectedShowBox41SupplementaryUnits => true;
		protected override ZString ExpectedSupplementaryUnits => "15.34 ABC";
		protected override ZString ExpectedSupplementaryQuantity => "15.34";
		protected override ZString ExpectedSupplementaryUQDescription => ZString.Empty;
		protected override ZString ExpectedBox49WarehouseForC88 => "";
		protected override ZString[] TaxTypesToTestOrder => Array.Empty<ZString>();
		protected override ZString[] ExpectedTaxTypesOrder => Array.Empty<ZString>();
	}
}
