using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.ESConstants;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants.RefCusCodeList;

namespace Enterprise.Customs.ES.Business.Declaration.Testing;

[TestedType(typeof(CusEntryLine))]
class CusEntryLineTest : EU.Business.Declaration.Testing.CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
{
	public void TestBox31MaxLength()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Box 31 Max Length", 1030, Factory.New<CusEntryLine>().Box31MaxLength);
		});
	}

	public void TestBox44MaxLength()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Import Box 44 Max Length", 900, Factory.New<CusEntryLine>().ImportBox44MaxLength);
			AssertEquals("Export Box 44 Max Length", 987, Factory.New<CusEntryLine>().ExportBox44MaxLength);
			AssertEquals("Import BIS Page Box 44 Max Length", 800, Factory.New<CusEntryLine>().ImportBox44BISPageMaxLength);
			AssertEquals("Export BIS Page Box 44 Max Length", 881, Factory.New<CusEntryLine>().ExportBox44BISPageMaxLength);
		});
	}

	public void TestFirstPackQty()
	{
		var inv = declaration.Invoices.AddNew();
		var invoiceLine1 = inv.InvoiceLines.AddNew();

		var pack1 = Factory.New<InvoiceLinePackagePivot>();
		var packageInfo1 = Factory.New<BasePackage>();
		pack1.CHC_CW = packageInfo1.PK;
		pack1.CHC_NumberOfPacks = 4;
		invoiceLine1.PackagesPivot.Add(pack1);

		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		var entryHeader = declaration.CustomsEntryHeaders[0];
		var entryLine = entryHeader.MergedLines[0];

		AssertEquals("Expected FirstCSI_PackQty", 4, entryLine.FirstPackQty());
	}

	public void TestFirstPackType()
	{
		var inv = declaration.Invoices.AddNew();
		var invoiceLine1 = inv.InvoiceLines.AddNew();

		var pack1 = Factory.New<InvoiceLinePackagePivot>();
		var packageInfo1 = Factory.New<BasePackage>();
		pack1.CHC_CW = packageInfo1.PK;
		pack1.Package.CW_PackType = "AA";
		invoiceLine1.PackagesPivot.Add(pack1);

		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		var entryHeader = declaration.CustomsEntryHeaders[0];
		var entryLine = entryHeader.MergedLines[0];

		AssertEquals("Expected FirstCSI_PackType", "AA", entryLine.FirstPackType());
	}

	public void TestVehiclesOrPackagesQty_Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryLine), CusEntryLine.Schema.VehiclesOrPackagesQty);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Packages", captionResourceString.Caption);
			AssertEquals("MediumCaption", "Packs", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "Pkg", captionResourceString.ShortCaption);
			AssertEquals("FullDescription", "Total Packages for Entry Line", captionResourceString.FullDescription);
		});
	}

	public void TestVehiclesOrPackagesQty()
	{
		var billPackingGroup = declaration.Bills.AddNew().PackingGroups.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		var package1 = AddPackage();
		var package2 = AddPackage();
		var package3 = AddPackage();

		CombineAssertions(() =>
		{
			AddInvoiceLinePackagePivot(package1, 2, true);
			AddInvoiceLinePackagePivot(package2, 3, false);
			AddInvoiceLinePackagePivot(package3, 4, true);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryLine = declaration.CustomsEntryHeaders.FirstOrDefault().AllEntryLines.FirstOrDefault();
			AssertEquals("2 linked packages", 6, entryLine.VehiclesOrPackagesQty);

			AddInvoiceLineVehicle("vin1");
			AddInvoiceLineVehicle("vin2");
			AddInvoiceLineVehicle("vin2");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			entryLine = entryLine = declaration.CustomsEntryHeaders.FirstOrDefault().AllEntryLines.FirstOrDefault();
			AssertEquals("3 InvoiceLines with 3 VehicleVin", 3, entryLine.VehiclesOrPackagesQty);
		});

		BasePackage AddPackage()
		{
			var package = billPackingGroup.Packages.AddNew();
			package.CW_PackQty = 10;
			package.CW_PackType = EU.Business.UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkGas;
			return package;
		}

		void AddInvoiceLinePackagePivot(BasePackage package, int quantity, bool isLinked)
		{
			var pivot = invoiceLine.PackagesPivot.AddNew();
			pivot.CHC_CW = package.PK;
			pivot.CHC_NumberOfPacks = quantity;

			var linkPackage = invoiceLine.PackagesForInvoiceLinesForBindingOnly.AddNew();
			linkPackage.Package = package;
			linkPackage.IsLinked = isLinked;
			if (isLinked)
			{
				linkPackage.PackQty = quantity;
			}
		}

		void AddInvoiceLineVehicle(string vin)
		{
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var vehicle = invoiceLine.Vehicles.AddNew();
			vehicle.CVH_VehicleIdentificationNumber = vin;
		}
	}

	public void TestTotalGrossWeightInKG()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MergeBy = "TRF";
		declaration.JE_ApplicationCode = "BLT";
		var inv = declaration.Invoices.AddNew();

		var invoiceLine1 = inv.InvoiceLines.AddNew();
		invoiceLine1.JI_WeightUQ = Core.Constants.Weight.Grams;
		invoiceLine1.JI_Weight = 100000.00m;

		var invoiceLine2 = inv.InvoiceLines.AddNew();
		invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Milligrams;
		invoiceLine2.JI_Weight = 200200000.00m;

		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders[0];
		entryHeader.ZG_UCC6Version = UCC6VersionCodes.NoUCC6;
		AssertEquals("Total gross weight is sum of single invoice lines ceiling gross Weight", 301.00m, (entryHeader.AllEntryLines[0]).TotalGrossWeightInKG);

		entryHeader.ZG_UCC6Version = UCC6VersionCodes.UCC6;
		AssertEquals("Total gross weight is sum of single invoice lines gross Weight", 300.20m, (entryHeader.AllEntryLines[0]).TotalGrossWeightInKG);

		entryHeader.ZG_UCC6Version = UCC6VersionCodes.NoUCC6;
		entryHeader.ZG_POUSVersion = POUSVersionCodes.NoPOUS;
		AssertEquals("Total gross weight is sum of single invoice lines gross Weight with ZG_POUSVersion = 0", 301.00m, (entryHeader.AllEntryLines[0]).TotalGrossWeightInKG);

		entryHeader.ZG_POUSVersion = POUSVersionCodes.POUS;
		AssertEquals("Total gross weight is sum of single invoice lines gross Weight with ZG_POUSVersion = 1", 300.20m, (entryHeader.AllEntryLines[0]).TotalGrossWeightInKG);

		entryHeader.ZG_POUSVersion = POUSVersionCodes.NoPOUS;
		AssertEquals("No H2 Header, No POUS, No UCC6", 301.00m, (entryHeader.AllEntryLines[0]).TotalGrossWeightInKG);

		entryHeader.ZG_POUSVersion = POUSVersionCodes.POUS2;
		AssertEquals("Total gross weight is sum of single invoice lines gross Weight with ZG_POUSVersion = 2", 300.20m, (entryHeader.AllEntryLines[0]).TotalGrossWeightInKG);

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
		AssertEquals("H2 Header, No POUS, No UCC6", 300.20m, (entryHeader.AllEntryLines[0]).TotalGrossWeightInKG);
	}

	public void TestHasPRECustomsOffice()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MergeBy = "TRF";
		declaration.JE_ApplicationCode = "BLT";
		var inv = declaration.Invoices.AddNew();
		var invoiceLine1 = inv.InvoiceLines.AddNew();
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders[0];

		var office = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit);
		CombineAssertions(() =>
		{
			AssertEquals("CustomsOffice not has PRE", false, entryHeader.AllEntryLines[0].HasPRECustomsOffice);

			office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
			AssertEquals("CustomsOffice has PRE", true, entryHeader.AllEntryLines[0].HasPRECustomsOffice);
		});
	}

	public void TestMergeKeysCreateDifferentEntryLines_Generic()
	{
		var genericKeysToTest = new[] { "JI_SupplementaryCode2", "JI_FormattedTariff", "JI_CountryOfOrigin", "JI_FormattedProcedure", "JI_CustomsSecondUnitQty", "JI_CustomsThirdUnitQty" };
		var invoice = declaration.Invoices.AddNew();
		declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;

		CombineAssertions(() =>
		{
			AssertEquals("Prereq: no invoice lines", 0, invoice.InvoiceLines.Count);
			foreach (var key in genericKeysToTest)
			{
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.GetType().GetProperty(key).SetValue(invoiceLine, new ZString("B"));
			}
			invoice.InvoiceLines.AddNew().JI_SupplementaryCode1 = "A";

			AssertEquals("Prereq: created invoice line per key", genericKeysToTest.Length + 1, invoice.InvoiceLines.Count);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("created entry line per invoice line", invoice.InvoiceLines.Count, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
		});
	}

	public void TestSealsFromContainers()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		CombineAssertions(() =>
		{
			var entryLine = entryHeader.AllEntryLines.AddNew();
			AssertArrayEqualsByElements(Array.Empty<ZString>(), entryLine.SealsFromContainers().ToArray());

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_Seal = "SEAL1";
			container1.CO_SecondSeal = "SEAL2";
			container1.AdditionalSeals.AddNew().BK_SealNumber = "ADDSEAL1";
			container1.AdditionalSeals.AddNew().BK_SealNumber = "ADDSEAL2";

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_Seal = "SEAL2";
			container2.CO_SecondSeal = "SEAL3";
			container2.AdditionalSeals.AddNew().BK_SealNumber = "ADDSEAL2";
			container2.AdditionalSeals.AddNew().BK_SealNumber = "ADDSEAL3";

			var container3 = declaration.CusContainers.AddNew();
			container3.CO_Seal = "SEAL2";
			container3.CO_SecondSeal = "SEAL3";

			var container4 = declaration.CusContainers.AddNew();
			container4.CO_Seal = "";

			var container5 = declaration.CusContainers.AddNew();
			container5.CO_Seal = "SEAL4";
			container5.CO_SecondSeal = "SEAL5";
			container5.AdditionalSeals.AddNew().BK_SealNumber = "ADDSEAL4";

			var invoiceLine1 = entryLine.InvoiceLines.AddNew();
			invoiceLine1.ContainersPivot.AddNew().C2_CO = container1.PK;

			var invoiceLine2 = entryLine.InvoiceLines.AddNew();
			invoiceLine2.ContainersPivot.AddNew().C2_CO = container2.PK;
			invoiceLine2.ContainersPivot.AddNew().C2_CO = container3.PK;
			invoiceLine2.ContainersPivot.AddNew().C2_CO = container4.PK;
			AssertArrayEqualsByElements(new ZString[] { "SEAL1", "SEAL2", "SEAL3", "ADDSEAL1", "ADDSEAL2", "ADDSEAL3" }, entryLine.SealsFromContainers().ToArray());
		});
	}

	public void TestSealsFromEquipments()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		CombineAssertions(() =>
		{
			var entryLine = entryHeader.AllEntryLines.AddNew();
			AssertArrayEqualsByElements(Array.Empty<ZString>(), entryLine.SealsFromEquipments.ToArray());

			var packingGroups = declaration.Bills.AddNew().PackingGroups.AddNew();

			var equipment1 = declaration.Equipments.AddNew();
			equipment1.CEQ_IdentificationNumber = "EQUIP1";
			equipment1.Seals.AddNew().BK_SealNumber = "SEAL1";
			equipment1.Seals.AddNew().BK_SealNumber = "SEAL2";
			var pack1 = packingGroups.Packages.AddNew();
			pack1.CW_PackQty = 1;
			pack1.CW_ContainerNoOrEquipmentNo = equipment1.CEQ_IdentificationNumber;

			var equipment2 = declaration.Equipments.AddNew();
			equipment2.CEQ_IdentificationNumber = "EQUIP2";
			equipment2.Seals.AddNew().BK_SealNumber = "SEAL2";
			equipment2.Seals.AddNew().BK_SealNumber = "SEAL3";
			var pack2 = packingGroups.Packages.AddNew();
			pack2.CW_PackQty = 1;
			pack2.CW_ContainerNoOrEquipmentNo = equipment2.CEQ_IdentificationNumber;

			var equipment3 = declaration.Equipments.AddNew();
			equipment3.CEQ_IdentificationNumber = "EQUIP3";
			equipment3.Seals.AddNew().BK_SealNumber = "";
			var pack3 = packingGroups.Packages.AddNew();
			pack3.CW_PackQty = 1;
			pack3.CW_ContainerNoOrEquipmentNo = equipment3.CEQ_IdentificationNumber;

			var equipment4 = declaration.Equipments.AddNew();
			equipment4.CEQ_IdentificationNumber = "EQUIP4";
			equipment4.Seals.AddNew().BK_SealNumber = "SEAL4";
			var pack4 = packingGroups.Packages.AddNew();
			pack4.CW_PackQty = 1;
			pack4.CW_ContainerNoOrEquipmentNo = equipment4.CEQ_IdentificationNumber;

			var invoiceLine1 = entryLine.InvoiceLines.AddNew();
			invoiceLine1.PackagesPivot.AddPivotFor(pack1);

			var invoiceLine2 = entryLine.InvoiceLines.AddNew();
			invoiceLine2.PackagesPivot.AddPivotFor(pack2);
			invoiceLine2.PackagesPivot.AddPivotFor(pack3);
			AssertArrayEqualsByElements(new ZString[] { "SEAL1", "SEAL2", "SEAL3" }, entryLine.SealsFromEquipments.ToArray());
		});
	}

	public void TestMergeKeysCreateDifferentEntryLines_Import()
	{
		var importKeysToTest = new[] { "JI_ConcessionOrder", "JI_PrimaryPreference", "ZG_REAProductCode", "ZG_ExciseExemption", "JI_ZZF_NKTaxType", "ZG_AIEMType", "ZG_ExciseCode" };
		var invoice = declaration.Invoices.AddNew();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("Prereq: no invoice lines", 0, invoice.InvoiceLines.Count);

		foreach (var key in importKeysToTest)
		{
			var invoiceLine = invoice.InvoiceLines.AddNew();
			if (key == nameof(invoiceLine.JI_ZZF_NKTaxType))
			{
				var taxOrFeeDetailEntity = new EU.Business.Declaration.TaxOrFeeDetailEntity();
				taxOrFeeDetailEntity.Code = new ZString("B");
				invoiceLine.Lookups.TaxOrFeeDetailEntities.Add(taxOrFeeDetailEntity);
				invoiceLine.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;
			}
			else
			{
				invoiceLine.GetType().GetProperty(key).SetValue(invoiceLine, new ZString("B"));
			}
		}

		AssertEquals("Prereq: created invoice line per key", importKeysToTest.Length, invoice.InvoiceLines.Count);

		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals("created entry line per invoice line", invoice.InvoiceLines.Count, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
	}

	public void TestMergeKeysCreateDifferentEntryLines_Export()
	{
		var exportKeysToTest = new[] { "JI_StateOrRegionOfOrigin" };
		var invoice = declaration.Invoices.AddNew();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		CombineAssertions(() =>
		{
			AssertEquals("Prereq: no invoice lines", 0, invoice.InvoiceLines.Count);
			foreach (var key in exportKeysToTest)
			{
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.GetType().GetProperty(key).SetValue(invoiceLine, new ZString("B"));
			}

			AssertEquals("Prereq: created invoice line per key", exportKeysToTest.Length, invoice.InvoiceLines.Count);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("created entry line per invoice line", invoice.InvoiceLines.Count, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
		});
	}

	public void TestContainersPivot()
	{
		var invoice = declaration.Invoices.AddNew();
		var invLine1 = invoice.JobComInvoiceLines.AddNew();
		var invLine2 = invoice.JobComInvoiceLines.AddNew();
		var repeatedContainer = CreateNewContainer("C1", invLine1);
		CreateNewContainer("C2", invLine1);
		CreateNewContainer("C3", invLine1);

		invLine2.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(repeatedContainer.CO_ContainerNumber).IsForInvoiceLine = true;
		CreateNewContainer("C4", invLine2);
		CreateNewContainer("C5", invLine2);

		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		AssertEquals("3 Containers from invLine1 + 2 Containers from invLine2 (the third is repeated)", 5, declaration.CustomsEntryHeaders[0].AllEntryLines[0].ContainersPivot.Count);
	}

	public void TestEffectiveGrossWeightIsApplicable()
	{
		var orphanEntryLine = Factory.New<CusEntryLine>();
		Assert("Effective Gross Weight Is Applicable should be true also when entry line is not linked to a declaration", orphanEntryLine.EffectiveGrossWeightIsApplicable);

		var declaration = Factory.New<JobDeclaration>();
		var entryLineLinkedToDeclaration = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		Assert("Effective Gross Weight Is Applicable should be true also when entry line is linked to a declaration", entryLineLinkedToDeclaration.EffectiveGrossWeightIsApplicable);
	}

	public void TestBox31CompleteText()
	{
		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
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
			cw1.CW_PackType = "PK";
			cw1.CW_MarksAndNos = "RED";
			var cw2 = bill.PackingGroups[0].Packages.AddNew();
			cw2.CW_PackQty = 11;
			cw2.CW_PackType = "BA";
			cw2.CW_MarksAndNos = "BLUE";

			var packing1 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
			packing1.IsLinked = true;
			packing1.PackQty = 6;

			var packing2 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[1];
			packing2.IsLinked = true;
			packing2.PackQty = 7;

			string expected = @"6 PK, RED. 7 BA, BLUE.
--------------------------------------------------------------------------------
MOTORES DE EMBOLO ALTERNATIVO O ROTATIVO, DE ENCENDIDO POR CHISPA (MOTORES DE EXPLOSION), DE CILINDRADA IGUAL O SUPERIOR A 300€CM3€Y POTENCIA IGUAL O SUPERIOR A 6€KW, PERO INFERIOR O IGUAL A 20,0€KW, PARA LA FABRICACION: - DE CORTADORAS DE CESPED DE LAS SUBPARTIDAS 8433€11, 8433€19€Y 8433€20, DE TRACTORES DE LAS SUBPARTIDAS 8701€91€90€Y 8701€92€90€CUYA FUNCION PRINCIPAL SEA LA DE CORTAR EL CESPED-DE CORTADORAS CON UN MOTOR DE CUATRO TIEMPOS DE UNA CILINDRADA IGUAL O SUPERIOR A 300€CM³ DE LA SUBPARTIDA 8433€";
			AssertEquals("Box31 without containers", expected, entryLine.Box31CompleteText);

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
			var expectedNoUCC6OrImport = @"6 PK, RED. 7 BA, BLUE. 1 CONTENEDOR OOCL3219032.
--------------------------------------------------------------------------------
MOTORES DE EMBOLO ALTERNATIVO O ROTATIVO, DE ENCENDIDO POR CHISPA (MOTORES DE EXPLOSION), DE CILINDRADA IGUAL O SUPERIOR A 300€CM3€Y POTENCIA IGUAL O SUPERIOR A 6€KW, PERO INFERIOR O IGUAL A 20,0€KW, PARA LA FABRICACION: - DE CORTADORAS DE CESPED DE LAS SUBPARTIDAS 8433€11, 8433€19€Y 8433€20, DE TRACTORES DE LAS SUBPARTIDAS 8701€91€90€Y 8701€92€90€CUYA FUNCION PRINCIPAL SEA LA DE CORTAR EL CESPED-DE CORTADORAS CON UN MOTOR DE CUATRO TIEMPOS DE UNA CILINDRADA IGUAL O SUPERIOR A 300€CM³ DE LA SUBPARTIDA 8433€";
			AssertEquals("Box31 with 1 container (No UCC6 and Export)", expectedNoUCC6OrImport, entryLine.Box31CompleteText);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Box31 with 1 container (No UCC6 and Import)", expectedNoUCC6OrImport, entryLine.Box31CompleteText);

			entryHeader.ZG_UCC6Version = UCC6VersionCodes.UCC6;
			AssertEquals("Box31 with 1 container (UCC6 and Import)", expectedNoUCC6OrImport, entryLine.Box31CompleteText);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var expectedUCC6AndExport = @"6 PK, RED. 7 BA, BLUE. 1 CONTENEDOR OOCL3219032 - SEAL01.
--------------------------------------------------------------------------------
MOTORES DE EMBOLO ALTERNATIVO O ROTATIVO, DE ENCENDIDO POR CHISPA (MOTORES DE EXPLOSION), DE CILINDRADA IGUAL O SUPERIOR A 300€CM3€Y POTENCIA IGUAL O SUPERIOR A 6€KW, PERO INFERIOR O IGUAL A 20,0€KW, PARA LA FABRICACION: - DE CORTADORAS DE CESPED DE LAS SUBPARTIDAS 8433€11, 8433€19€Y 8433€20, DE TRACTORES DE LAS SUBPARTIDAS 8701€91€90€Y 8701€92€90€CUYA FUNCION PRINCIPAL SEA LA DE CORTAR EL CESPED-DE CORTADORAS CON UN MOTOR DE CUATRO TIEMPOS DE UNA CILINDRADA IGUAL O SUPERIOR A 300€CM³ DE LA SUBPARTIDA 8433€";
			AssertEquals("Box31 with 1 container (UCC6 and Export)", expectedUCC6AndExport, entryLine.Box31CompleteText);

			invoiceLine.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = true;

			entryHeader.ZG_UCC6Version = UCC6VersionCodes.NoUCC6;
			expectedNoUCC6OrImport = @"6 PK, RED. 7 BA, BLUE. 2 CONTENEDORES OOCL3219032, OOCL3127895.
--------------------------------------------------------------------------------
MOTORES DE EMBOLO ALTERNATIVO O ROTATIVO, DE ENCENDIDO POR CHISPA (MOTORES DE EXPLOSION), DE CILINDRADA IGUAL O SUPERIOR A 300€CM3€Y POTENCIA IGUAL O SUPERIOR A 6€KW, PERO INFERIOR O IGUAL A 20,0€KW, PARA LA FABRICACION: - DE CORTADORAS DE CESPED DE LAS SUBPARTIDAS 8433€11, 8433€19€Y 8433€20, DE TRACTORES DE LAS SUBPARTIDAS 8701€91€90€Y 8701€92€90€CUYA FUNCION PRINCIPAL SEA LA DE CORTAR EL CESPED-DE CORTADORAS CON UN MOTOR DE CUATRO TIEMPOS DE UNA CILINDRADA IGUAL O SUPERIOR A 300€CM³ DE LA SUBPARTIDA 8433€";
			AssertEquals("Box31 with 2 containers (No UCC6 and Export)", expectedNoUCC6OrImport, entryLine.Box31CompleteText);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Box31 with 2 containers (No UCC6 and Import)", expectedNoUCC6OrImport, entryLine.Box31CompleteText);

			entryHeader.ZG_UCC6Version = UCC6VersionCodes.UCC6;
			AssertEquals("Box31 with 2 containers (UCC6 and Import)", expectedNoUCC6OrImport, entryLine.Box31CompleteText);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			expectedUCC6AndExport = @"6 PK, RED. 7 BA, BLUE. 2 CONTENEDORES OOCL3219032 - SEAL01, OOCL3127895 - SEAL02/SEAL03/SEAL04/SEAL05.
--------------------------------------------------------------------------------
MOTORES DE EMBOLO ALTERNATIVO O ROTATIVO, DE ENCENDIDO POR CHISPA (MOTORES DE EXPLOSION), DE CILINDRADA IGUAL O SUPERIOR A 300€CM3€Y POTENCIA IGUAL O SUPERIOR A 6€KW, PERO INFERIOR O IGUAL A 20,0€KW, PARA LA FABRICACION: - DE CORTADORAS DE CESPED DE LAS SUBPARTIDAS 8433€11, 8433€19€Y 8433€20, DE TRACTORES DE LAS SUBPARTIDAS 8701€91€90€Y 8701€92€90€CUYA FUNCION PRINCIPAL SEA LA DE CORTAR EL CESPED-DE CORTADORAS CON UN MOTOR DE CUATRO TIEMPOS DE UNA CILINDRADA IGUAL O SUPERIOR A 300€CM³ DE LA SUBPARTIDA 8433€";
			AssertEquals("Box31 with 2 containers (UCC6 and export)", expectedUCC6AndExport, entryLine.Box31CompleteText);

			packing2.IsLinked = false;
			entryHeader.ZG_UCC6Version = UCC6VersionCodes.NoUCC6;
			expectedNoUCC6OrImport = "6 PK, RED. 2 CONTENEDORES OOCL3219032, OOCL3127895";
			AssertContains("Box31 with only one package (No UCC6 and Export)", expectedNoUCC6OrImport, entryLine.Box31CompleteText);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertContains("Box31 with only one package (No UCC6 and Import)", expectedNoUCC6OrImport, entryLine.Box31CompleteText);

			entryHeader.ZG_UCC6Version = UCC6VersionCodes.UCC6;
			AssertContains("Box31 with only one package (UCC6 and Import)", expectedNoUCC6OrImport, entryLine.Box31CompleteText);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertContains("Box31 with only one package (UCC6)", "6 PK, RED. 2 CONTENEDORES OOCL3219032 - SEAL01, OOCL3127895 - SEAL02/SEAL03/SEAL04/SEAL05.", entryLine.Box31CompleteText);

			var vehicle = invoiceLine.Vehicles.AddNew();
			vehicle.CVH_VehicleIdentificationNumber = "VINCODE";
			vehicle.CVH_ModelName = "Model";
			vehicle.CVH_BrandName = "Brand Name";

			var vehicle1 = invoiceLine.Vehicles.AddNew();
			vehicle1.CVH_VehicleIdentificationNumber = "VINCODE1";
			vehicle1.CVH_ModelName = "Model1";
			vehicle1.CVH_BrandName = "Brand Name1";

			entryHeader.ZG_UCC6Version = UCC6VersionCodes.NoUCC6;
			expectedNoUCC6OrImport = @"2 BASTIDORES, VINCODE Brand Name Model, VINCODE1 Brand Name1 Model1. 2 CONTENEDORES OOCL3219032, OOCL3127895.
--------------------------------------------------------------------------------
MOTORES DE EMBOLO ALTERNATIVO O ROTATIVO, DE ENCENDIDO POR CHISPA (MOTORES DE EXPLOSION), DE CILINDRADA IGUAL O SUPERIOR A 300€CM3€Y POTENCIA IGUAL O SUPERIOR A 6€KW, PERO INFERIOR O IGUAL A 20,0€KW, PARA LA FABRICACION: - DE CORTADORAS DE CESPED DE LAS SUBPARTIDAS 8433€11, 8433€19€Y 8433€20, DE TRACTORES DE LAS SUBPARTIDAS 8701€91€90€Y 8701€92€90€CUYA FUNCION PRINCIPAL SEA LA DE CORTAR EL CESPED-DE CORTADORAS CON UN MOTOR DE CUATRO TIEMPOS DE UNA CILINDRADA IGUAL O SUPERIOR A 300€CM³ DE LA SUBPARTIDA 8433€";
			AssertEquals("Box31 with 1 vehicle and 2 containers (No UCC6 and Export)", expectedNoUCC6OrImport, entryLine.Box31CompleteText);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Box31 with 1 vehicle and 2 containers (No UCC6 and Import)", expectedNoUCC6OrImport, entryLine.Box31CompleteText);

			entryHeader.ZG_UCC6Version = UCC6VersionCodes.UCC6;
			AssertEquals("Box31 with 1 vehicle and 2 containers (UCC6 and Import)", expectedNoUCC6OrImport, entryLine.Box31CompleteText);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			expectedUCC6AndExport = @"2 BASTIDORES, VINCODE Brand Name Model, VINCODE1 Brand Name1 Model1. 2 CONTENEDORES OOCL3219032 - SEAL01, OOCL3127895 - SEAL02/SEAL03/SEAL04/SEAL05.
--------------------------------------------------------------------------------
MOTORES DE EMBOLO ALTERNATIVO O ROTATIVO, DE ENCENDIDO POR CHISPA (MOTORES DE EXPLOSION), DE CILINDRADA IGUAL O SUPERIOR A 300€CM3€Y POTENCIA IGUAL O SUPERIOR A 6€KW, PERO INFERIOR O IGUAL A 20,0€KW, PARA LA FABRICACION: - DE CORTADORAS DE CESPED DE LAS SUBPARTIDAS 8433€11, 8433€19€Y 8433€20, DE TRACTORES DE LAS SUBPARTIDAS 8701€91€90€Y 8701€92€90€CUYA FUNCION PRINCIPAL SEA LA DE CORTAR EL CESPED-DE CORTADORAS CON UN MOTOR DE CUATRO TIEMPOS DE UNA CILINDRADA IGUAL O SUPERIOR A 300€CM³ DE LA SUBPARTIDA 8433€";
			AssertEquals("Box31 with 1 vehicle and 2 containers (UCC6 and Export)", expectedUCC6AndExport, entryLine.Box31CompleteText);

			packing1.PackQty = 0;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = false;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = false;

			vehicle.Delete();
			vehicle1.Delete();
			expected = @"0 PK, RED.
--------------------------------------------------------------------------------
MOTORES DE EMBOLO ALTERNATIVO O ROTATIVO, DE ENCENDIDO POR CHISPA (MOTORES DE EXPLOSION), DE CILINDRADA IGUAL O SUPERIOR A 300€CM3€Y POTENCIA IGUAL O SUPERIOR A 6€KW, PERO INFERIOR O IGUAL A 20,0€KW, PARA LA FABRICACION: - DE CORTADORAS DE CESPED DE LAS SUBPARTIDAS 8433€11, 8433€19€Y 8433€20, DE TRACTORES DE LAS SUBPARTIDAS 8701€91€90€Y 8701€92€90€CUYA FUNCION PRINCIPAL SEA LA DE CORTAR EL CESPED-DE CORTADORAS CON UN MOTOR DE CUATRO TIEMPOS DE UNA CILINDRADA IGUAL O SUPERIOR A 300€CM³ DE LA SUBPARTIDA 8433€";
			AssertEquals("Box31 without packages", expected, entryLine.Box31CompleteText);
		});
	}

	public void TestBox37ProcedureCompleteText()
	{
		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.JI_Procedure = "4000APC";

		CombineAssertions(() =>
		{
			Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];

			AssertEquals("Box37ProcedureCompleteText correct format", "40.00 | APC", entryLine.Box37ProcedureCompleteText);
		});
	}

	public void TestBox37_2ProcedureCompleteText()
	{
		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.JI_Procedure = "4000APC";

		CombineAssertions(() =>
		{
			Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];

			AssertEquals("Box37_2ProcedureCompleteText empty when no AdditionalProcedureCodes are declared", ZString.Empty, entryLine.Box37_2ProcedureCompleteText);

			invoiceLine.AdditionalProcedureCodes.AddNew("456F89");
			invoiceLine.AdditionalProcedureCodes.AddNew("789100");
			AssertEquals("Box37_2ProcedureCompleteText has correct data when AdditionalProcedureCodes are declared", "F89 100", entryLine.Box37_2ProcedureCompleteText);
		});
	}

	public void TestBox44Contents_OnlyWithAcceptedSupportingDocuments()
	{
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine.TablePrefix, "L001", "LineDoc1", new ZDateTime(2020, 03, 12)));
		invoiceLine.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine.TablePrefix, "L002", "LineDoc2", new ZDateTime(2020, 12, 31)));

		Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.CH_EntryStatus = "CLP";

		var entryLine = entryHeader.AllEntryLines[0];

		AddSupDoc(entryLine.PK, entryLine.TablePrefix, "X001", "ES3600000001", new ZDateTime(2020, 03, 12), DocumentStatus.Accepted);
		AddSupDoc(entryLine.PK, entryLine.TablePrefix, "X002", "ES3600000002", ZDateTime.Empty, DocumentStatus.Accepted);
		AddSupDoc(entryLine.PK, entryLine.TablePrefix, "X003", "ES3600000003", new ZDateTime(2020, 12, 31), DocumentStatus.Accepted);
		AddSupDoc(entryLine.PK, entryLine.TablePrefix, "X004", "ES3600000004", new ZDateTime(2020, 12, 31), DocumentStatus.Cancelled);

		CombineAssertions(() =>
		{
			var expected = "X001: ES3600000001 12-03-2020; X002: ES3600000002; X003: ES3600000003 31-12-2020";
			AssertEquals("Box44CompleteText correct format", expected, entryLine.Box44CompleteText);

			var entryLine2 = entryHeader.AllEntryLines.AddNew();

			for (int i = 0; i < 50; i++)
			{
				AddSupDoc(entryLine2.PK, entryLine2.TablePrefix, "AA" + i, "ES3600000001", new ZDateTime(2020, 03, 12), DocumentStatus.Accepted);
			}

			entryHeader.CH_EntryStatus = ZString.Empty;
			AssertEquals("Box44CompleteText correct format when EntryStatus empty", ZString.Empty, entryLine2.Box44CompleteText);

			entryHeader.CH_EntryStatus = "CLP";
			// Note only first 150 chars of complete text are shown
			expected = "AA0: ES3600000001 12-03-2020; AA1: ES3600000001 12-03-2020; AA2: ES3600000001 12-03-2020; AA3: ES3600000001 12-03-2020; AA4: ES3600000001 12-03-2020; AA5: ES3600000001 12-03-2020; " +
			"AA6: ES3600000001 12-03-2020; AA7: ES3600000001 12-03-2020; AA8: ES3600000001 12-03-2020; AA9: ES3600000001 12-03-2020; AA10: ES3600000001 12-03-2020; AA11: ES3600000001 12-03-2020; AA12: ES3600000001 12-03-2020; AA13: ES3600000001 12-03-2020; AA14: ES3600000001 12-03-2020; " +
			"AA15: ES3600000001 12-03-2020; AA16: ES3600000001 12-03-2020; AA17: ES3600000001 12-03-2020; AA18: ES3600000001 12-03-2020; AA19: ES3600000001 12-03-2020; AA20: ES3600000001 12-03-2020; AA21: ES3600000001 12-03-2020; AA22: ES3600000001 12-03-2020; AA23: ES3600000001 12-03-2020; " +
			"AA24: ES3600000001 12-03-2020; AA25: ES3600000001 12-03-2020; AA26: ES3600000001 12-03-2020; AA27: ES3600000001 12-03-2020; AA28: ES3600000001 12-03-2020; AA29: ES3600000001 12-03-2020; AA30: ES3600000001 12-03-2020; AA31: ES3600000001 12-03-2020; AA32: ES3600000001 12-03-2020; " +
			"AA33: ES3600000001 12-03-2020; AA34: ES3600000001 12-03-2020; AA35: ES3600000001 12-03-2020; AA36: ES3600000001 12-03-2020; AA37: ES3600000001 12-03-2020; AA38: ES3600000001 12-03-2020; AA39: ES3600000001 12-03-2020; AA40: ES3600000001 12-03-2020; AA41: ES3600000001 12-03-2020; " +
			"AA42: ES3600000001 12-03-2020; AA43: ES3600000001 12-03-2020; AA44: ES3600000001 12-03-2020; AA45: ES3600000001 12-03-2020; AA46: ES3600000001 12-03-2020; AA47: ES3600000001 12-03-2020; AA48: ES3600000001 12-03-2020; AA49: ES3600000001 12-03-2020";
			AssertEquals("Box44CompleteText correct format when EntryStatus = CLP", expected, entryLine2.Box44CompleteText);

			entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
			AssertEquals("Box44CompleteText correct format when EntryStatus PDI", ZString.Empty, entryLine2.Box44CompleteText);
		});
	}

	public void TestBox44Contents_WithAllSupportingDocuments()
	{
		declaration.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, declaration.TablePrefix, "D001", "DeclarationDoc1", ZDateTime.Empty));
		declaration.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, declaration.TablePrefix, "D002", "DeclarationDoc2", new ZDateTime(2020, 12, 31)));

		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		invoiceHeader.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceHeader.TablePrefix, "H001", "HeaderDoc1", new ZDateTime(2020, 03, 12)));
		invoiceHeader.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceHeader.TablePrefix, "H002", "HeaderDoc2", ZDateTime.Empty));

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine.TablePrefix, "L001", "LineDoc1", new ZDateTime(2020, 03, 12)));
		invoiceLine.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine.TablePrefix, "L002", "LineDoc2", new ZDateTime(2020, 12, 31)));
		invoiceLine.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine.TablePrefix, "L003", "LineDoc3", ZDateTime.Empty));
		invoiceLine.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine.TablePrefix, "L004", "LineDoc4", new ZDateTime(2020, 09, 01)));

		Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.CH_EntryStatus = ZString.Empty;

		AddSupDoc(entryHeader.PK, entryHeader.TablePrefix, "C001", "ES3700000001", new ZDateTime(2020, 03, 12), DocumentStatus.Accepted);
		AddSupDoc(entryHeader.PK, entryHeader.TablePrefix, "C002", "ES3700000002", new ZDateTime(2020, 12, 31), DocumentStatus.Accepted, SupportingDocumentSubType.LIQ);
		AddSupDoc(entryHeader.PK, entryHeader.TablePrefix, "C003", "ES3700000003", new ZDateTime(2020, 12, 31), DocumentStatus.Cancelled, SupportingDocumentSubType.LIQ);

		var entryLine = entryHeader.AllEntryLines[0];

		AddSupDoc(entryLine.PK, entryLine.TablePrefix, "X001", "ES3600000001", new ZDateTime(2020, 03, 12), DocumentStatus.Accepted);
		AddSupDoc(entryLine.PK, entryLine.TablePrefix, "X002", "ES3600000002", ZDateTime.Empty, DocumentStatus.Accepted);
		AddSupDoc(entryLine.PK, entryLine.TablePrefix, "X003", "ES3600000003", new ZDateTime(2020, 12, 31), DocumentStatus.Accepted, SupportingDocumentSubType.LIQ);
		AddSupDoc(entryLine.PK, entryLine.TablePrefix, "X004", "ES3600000004", new ZDateTime(2020, 03, 31), DocumentStatus.Cancelled, SupportingDocumentSubType.LIQ);

		CombineAssertions(() =>
		{
			var expected = "D001: DeclarationDoc1; D002: DeclarationDoc2 31-12-2020; " +
				"H001: HeaderDoc1 12-03-2020; H002: HeaderDoc2; " +
				"L001: LineDoc1 12-03-2020; L002: LineDoc2 31-12-2020; L003: LineDoc3; L004: LineDoc4 01-09-2020; " +
				"X003: ES3600000003 31-12-2020; X004: ES3600000004 31-03-2020; C002: ES3700000002 31-12-2020; C003: ES3700000003 31-12-2020";
			AssertEquals("Box44CompleteText correct format when EntryStatus empty", expected, entryLine.Box44CompleteText);

			entryHeader.CH_EntryStatus = "CLP";

			AssertEquals("Box44CompleteText correct format when EntryStatus = CLP", "X001: ES3600000001 12-03-2020; X002: ES3600000002; X003: ES3600000003 31-12-2020; C001: ES3700000001 12-03-2020; C002: ES3700000002 31-12-2020", entryLine.Box44CompleteText);

			entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

			AssertEquals("Box44CompleteText correct format when EntryStatus = PDI", expected, entryLine.Box44CompleteText);
		});
	}

	public void TestBox44Contents_OnlyWithAcceptedAdditionalInfos()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine.TablePrefix, "L001", "LineDoc1"));
		invoiceLine.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine.TablePrefix, "L002", "LineDoc2"));

		Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.CH_EntryStatus = "CLP";

		var entryLine = entryHeader.AllEntryLines[0];

		AddAddInf(entryLine.PK, entryLine.TablePrefix, "X001", "ES3600000001", status: DocumentStatus.Accepted);
		AddAddInf(entryLine.PK, entryLine.TablePrefix, "X002", "ES3600000002", status: DocumentStatus.Accepted);
		AddAddInf(entryLine.PK, entryLine.TablePrefix, "X003", "ES3600000003", status: DocumentStatus.Accepted);
		AddAddInf(entryLine.PK, entryLine.TablePrefix, "X005", "ES3600000004", subtype: "INF", status: DocumentStatus.Accepted);
		AddAddInf(entryLine.PK, entryLine.TablePrefix, "X005", "ES3600000005", status: DocumentStatus.Cancelled);

		CombineAssertions(() =>
		{
			var expected = "X001: ES3600000001; X002: ES3600000002; X003: ES3600000003";
			AssertEquals("Box44CompleteText correct format", expected, entryLine.Box44CompleteText);

			var entryLine2 = entryHeader.AllEntryLines.AddNew();

			for (int i = 0; i < 50; i++)
			{
				AddAddInf(entryLine2.PK, entryLine2.TablePrefix, "AA" + i, "ES3600000001", status: DocumentStatus.Accepted);
			}

			entryHeader.CH_EntryStatus = ZString.Empty;
			AssertEquals("Box44CompleteText correct format when EntryStatus empty", ZString.Empty, entryLine2.Box44CompleteText);

			entryHeader.CH_EntryStatus = "CLP";
			// Note only first 150 chars of complete text are shown
			expected = "AA0: ES3600000001; AA1: ES3600000001; AA2: ES3600000001; AA3: ES3600000001; AA4: ES3600000001; AA5: ES3600000001; " +
			"AA6: ES3600000001; AA7: ES3600000001; AA8: ES3600000001; AA9: ES3600000001; AA10: ES3600000001; AA11: ES3600000001; AA12: ES3600000001; AA13: ES3600000001; AA14: ES3600000001; " +
			"AA15: ES3600000001; AA16: ES3600000001; AA17: ES3600000001; AA18: ES3600000001; AA19: ES3600000001; AA20: ES3600000001; AA21: ES3600000001; AA22: ES3600000001; AA23: ES3600000001; " +
			"AA24: ES3600000001; AA25: ES3600000001; AA26: ES3600000001; AA27: ES3600000001; AA28: ES3600000001; AA29: ES3600000001; AA30: ES3600000001; AA31: ES3600000001; AA32: ES3600000001; " +
			"AA33: ES3600000001; AA34: ES3600000001; AA35: ES3600000001; AA36: ES3600000001; AA37: ES3600000001; AA38: ES3600000001; AA39: ES3600000001; AA40: ES3600000001; AA41: ES3600000001; " +
			"AA42: ES3600000001; AA43: ES3600000001; AA44: ES3600000001; AA45: ES3600000001; AA46: ES3600000001; AA47: ES3600000001; AA48: ES3600000001; AA49: ES3600000001";
			AssertEquals("Box44CompleteText correct format when EntryStatus = CLP", expected, entryLine2.Box44CompleteText);
		});
	}

	public void TestBox44Contents_WithAllAdditionalInfos()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		declaration.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, declaration.TablePrefix, "D001", "DeclarationDoc1"));
		declaration.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, declaration.TablePrefix, "D002", "DeclarationDoc2"));
		declaration.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, declaration.TablePrefix, "D003", "DeclarationDoc3", subtype: "INF"));

		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		invoiceHeader.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceHeader.TablePrefix, "H001", "HeaderDoc1"));
		invoiceHeader.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceHeader.TablePrefix, "H002", "HeaderDoc2"));
		invoiceHeader.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceHeader.TablePrefix, "H003", "HeaderDoc3", subtype: "INF"));

		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine.TablePrefix, "L001", "LineDoc1"));
		invoiceLine.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine.TablePrefix, "L002", "LineDoc2"));
		invoiceLine.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine.TablePrefix, "L003", "LineDoc3"));
		invoiceLine.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine.TablePrefix, "L004", "LineDoc4"));
		invoiceLine.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine.TablePrefix, "L005", "LineDoc5", subtype: "INF"));

		Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.CH_EntryStatus = ZString.Empty;

		AddSupDoc(entryHeader.PK, entryHeader.TablePrefix, "C001", "ES3700000001", new ZDateTime(2020, 03, 12), DocumentStatus.Accepted);
		AddSupDoc(entryHeader.PK, entryHeader.TablePrefix, "C002", "ES3700000002", new ZDateTime(2020, 12, 31), DocumentStatus.Cancelled);

		var entryLine = entryHeader.AllEntryLines[0];

		AddAddInf(entryLine.PK, entryLine.TablePrefix, "X001", "ES3600000001", status: DocumentStatus.Accepted);
		AddAddInf(entryLine.PK, entryLine.TablePrefix, "X002", "ES3600000002", status: DocumentStatus.Accepted);
		AddAddInf(entryLine.PK, entryLine.TablePrefix, "X003", "ES3600000003", subtype: "INF", status: DocumentStatus.Accepted);
		AddAddInf(entryLine.PK, entryLine.TablePrefix, "X004", "ES3600000004", status: DocumentStatus.Cancelled);

		CombineAssertions(() =>
		{
			var expected = "D001: DeclarationDoc1; D002: DeclarationDoc2; " +
				"H001: HeaderDoc1; H002: HeaderDoc2; " +
				"L001: LineDoc1; L002: LineDoc2; L003: LineDoc3; L004: LineDoc4";
			AssertEquals("Box44CompleteText correct format when EntryStatus empty", expected, entryLine.Box44CompleteText);

			entryHeader.CH_EntryStatus = "CLP";

			AssertEquals("Box44CompleteText correct format when EntryStatus = CLP", "C001: ES3700000001 12-03-2020; X001: ES3600000001; X002: ES3600000002", entryLine.Box44CompleteText);
		});
	}

	public void TestBox44Contents_WithAllAuthorizations()
	{
		SetUpMapData();

		var holder = Factory.New<OrgHeader>();
		holder.OH_Code = "AA";

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		entryInstruction.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, entryInstruction.TablePrefix, holder.PK, "DPO", "EntryInstruction1"));
		entryInstruction.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, entryInstruction.TablePrefix, holder.PK, "AAA", "EntryInstruction2"));

		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine1.JI_Tariff = "111";
		invoiceLine1.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine1.TablePrefix, holder.PK, "SAS", "InvoiceLine11"));
		invoiceLine1.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine1.TablePrefix, holder.PK, "BBB", "InvoiceLine12"));

		var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_Tariff = "222";
		invoiceLine2.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine2.TablePrefix, holder.PK, "OTE", "InvoiceLine21"));
		invoiceLine2.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine2.TablePrefix, holder.PK, "CCC", "InvoiceLine22"));

		Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.CH_EntryStatus = ZString.Empty;

		var entryLine1 = entryHeader.AllEntryLines.FirstOrDefault(x => x.Tariff == "111");
		var entryLine2 = entryHeader.AllEntryLines.FirstOrDefault(x => x.Tariff == "222");

		var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine3.JI_CEI = entryInstruction.PK;
		entryLine2.InvoiceLines.Add(invoiceLine3);
		invoiceLine3.JI_Tariff = "222";
		invoiceLine3.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine3.TablePrefix, holder.PK, "DDD", "InvoiceLine31"));
		invoiceLine3.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine3.TablePrefix, holder.PK, "EEE", "InvoiceLine32"));

		CombineAssertions(() =>
		{
			var expectedForEntryLine1 = "C506: EntryInstruction1; AAA: EntryInstruction2; " +
				"CSAS: InvoiceLine11; BBB: InvoiceLine12";
			AssertEquals("Box44CompleteText correct format for entryLine1", expectedForEntryLine1, entryLine1.Box44CompleteText);

			var expectedForEntryLine2 = "C506: EntryInstruction1; AAA: EntryInstruction2; " +
				"CES: InvoiceLine21; CCC: InvoiceLine22; " +
				"DDD: InvoiceLine31; EEE: InvoiceLine32";
			AssertEquals("Box44CompleteText correct format for entryLine2", expectedForEntryLine2, entryLine2.Box44CompleteText);
		});
	}

	public void TestBox44Contents_WithEverything_Export()
	{
		SetUpMapData();

		var holder = Factory.New<OrgHeader>();
		holder.OH_Code = "AA";

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, declaration.TablePrefix, "DS01", "DeclarationSDoc1", ZDateTime.Empty));
		declaration.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, declaration.TablePrefix, "DS02", "DeclarationSDoc2", new ZDateTime(2020, 12, 31)));
		declaration.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, declaration.TablePrefix, "DA01", "DeclarationADoc1"));
		declaration.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, declaration.TablePrefix, "DA02", "DeclarationADoc2"));
		declaration.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, declaration.TablePrefix, "DA03", "DeclarationADoc3", subtype: "INF"));

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		entryInstruction.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, entryInstruction.TablePrefix, holder.PK, "DPO", "EntryInstruction1"));
		entryInstruction.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, entryInstruction.TablePrefix, holder.PK, "AAA", "EntryInstruction2"));

		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		invoiceHeader.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceHeader.TablePrefix, "HS01", "HeaderSDoc1", new ZDateTime(2020, 03, 12)));
		invoiceHeader.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceHeader.TablePrefix, "HS02", "HeaderSDoc2", ZDateTime.Empty));
		invoiceHeader.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceHeader.TablePrefix, "HA01", "HeaderADoc1"));
		invoiceHeader.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceHeader.TablePrefix, "HA02", "HeaderADoc2"));
		invoiceHeader.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceHeader.TablePrefix, "HA03", "HeaderADoc3", subtype: "INF"));

		var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine1.JI_Tariff = "111";
		invoiceLine1.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine1.TablePrefix, holder.PK, "SAS", "InvoiceLine11"));
		invoiceLine1.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine1.TablePrefix, holder.PK, "BBB", "InvoiceLine12"));

		invoiceLine1.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine1.TablePrefix, "LS01", "LineSDoc1", new ZDateTime(2020, 03, 12)));
		invoiceLine1.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine1.TablePrefix, "LS02", "LineSDoc2", new ZDateTime(2020, 12, 31)));
		invoiceLine1.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine1.TablePrefix, "LA01", "LineADoc1"));
		invoiceLine1.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine1.TablePrefix, "LA02", "LineADoc2"));

		var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_Tariff = "222";
		invoiceLine2.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine2.TablePrefix, holder.PK, "OTE", "InvoiceLine21"));
		invoiceLine2.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine2.TablePrefix, holder.PK, "CCC", "InvoiceLine22"));

		invoiceLine2.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine2.TablePrefix, "LS03", "LineSDoc3", ZDateTime.Empty));
		invoiceLine2.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine2.TablePrefix, "LA03", "LineADoc3"));

		Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.CH_EntryStatus = ZString.Empty;

		AddSupDoc(entryHeader.PK, entryHeader.TablePrefix, "C001", "ES3700000001", new ZDateTime(2020, 03, 12), DocumentStatus.Accepted);
		AddSupDoc(entryHeader.PK, entryHeader.TablePrefix, "C002", "ES3700000002", new ZDateTime(2020, 12, 31), DocumentStatus.Accepted, SupportingDocumentSubType.LIQ);
		AddSupDoc(entryHeader.PK, entryHeader.TablePrefix, "C003", "ES3700000003", new ZDateTime(2020, 03, 12), DocumentStatus.Cancelled);

		var entryLine1 = entryHeader.AllEntryLines.FirstOrDefault(x => x.Tariff == "111");
		AddSupDoc(entryLine1.PK, entryLine1.TablePrefix, "XS01", "ES36000000S1", new ZDateTime(2020, 03, 12), DocumentStatus.Accepted);
		AddSupDoc(entryLine1.PK, entryLine1.TablePrefix, "XS02", "ES36000000S2", ZDateTime.Empty, DocumentStatus.Accepted, SupportingDocumentSubType.LIQ);
		AddAddInf(entryLine1.PK, entryLine1.TablePrefix, "XA01", "ES36000000A1", status: DocumentStatus.Accepted);
		AddAddInf(entryLine1.PK, entryLine1.TablePrefix, "XA02", "ES36000000A2", status: DocumentStatus.Accepted);

		var entryLine2 = entryHeader.AllEntryLines.FirstOrDefault(x => x.Tariff == "222");

		AddSupDoc(entryLine2.PK, entryLine2.TablePrefix, "XS03", "ES36000000S3", new ZDateTime(2020, 12, 31), DocumentStatus.Accepted, SupportingDocumentSubType.LIQ);
		AddSupDoc(entryLine2.PK, entryLine2.TablePrefix, "XS04", "ES36000000S4", new ZDateTime(2020, 03, 31), DocumentStatus.Cancelled, SupportingDocumentSubType.LIQ);
		AddSupDoc(entryLine2.PK, entryLine2.TablePrefix, "XS05", "ES36000000S5", new ZDateTime(2020, 12, 31), DocumentStatus.Accepted);
		AddAddInf(entryLine2.PK, entryLine2.TablePrefix, "XA03", "ES36000000A3", subtype: "INF", status: DocumentStatus.Accepted);
		AddAddInf(entryLine2.PK, entryLine2.TablePrefix, "XA04", "ES36000000A4", status: DocumentStatus.Cancelled);
		AddAddInf(entryLine2.PK, entryLine2.TablePrefix, "XA05", "ES36000000A5", status: DocumentStatus.Accepted);

		var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine3.JI_CEI = entryInstruction.PK;
		entryLine2.InvoiceLines.Add(invoiceLine3);
		invoiceLine3.JI_Tariff = "222";
		invoiceLine3.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine3.TablePrefix, holder.PK, "DDD", "InvoiceLine31"));
		invoiceLine3.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine3.TablePrefix, holder.PK, "EEE", "InvoiceLine32"));

		invoiceLine3.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine3.TablePrefix, "LS04", "LineSDoc4", new ZDateTime(2020, 09, 01)));
		invoiceLine3.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine3.TablePrefix, "LA04", "LineADoc4"));
		invoiceLine3.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine3.TablePrefix, "LA05", "LineADoc5", subtype: "INF"));

		CombineAssertions(() =>
		{
			var expectedForEntryLine1 = "DS01: DeclarationSDoc1; DS02: DeclarationSDoc2 31-12-2020; " +
			"HS01: HeaderSDoc1 12-03-2020; HS02: HeaderSDoc2; " +
			"LS01: LineSDoc1 12-03-2020; LS02: LineSDoc2 31-12-2020; " +
			"XS02: ES36000000S2; " +
			"C002: ES3700000002 31-12-2020; " +
			"DA01: DeclarationADoc1; DA02: DeclarationADoc2; " +
			"HA01: HeaderADoc1; HA02: HeaderADoc2; " +
			"LA01: LineADoc1; LA02: LineADoc2; " +
			"C506: EntryInstruction1; AAA: EntryInstruction2; " +
			"CSAS: InvoiceLine11; BBB: InvoiceLine12";
			AssertEquals("Box44CompleteText correct format for entryLine1 when EntryStatus empty", expectedForEntryLine1, entryLine1.Box44CompleteText);

			var expectedForEntryLine2 = "DS01: DeclarationSDoc1; DS02: DeclarationSDoc2 31-12-2020; " +
			"HS01: HeaderSDoc1 12-03-2020; HS02: HeaderSDoc2; " +
			"LS03: LineSDoc3; LS04: LineSDoc4 01-09-2020; " +
			"XS03: ES36000000S3 31-12-2020; XS04: ES36000000S4 31-03-2020; " +
			"C002: ES3700000002 31-12-2020; " +
			"DA01: DeclarationADoc1; DA02: DeclarationADoc2; " +
			"HA01: HeaderADoc1; HA02: HeaderADoc2; " +
			"LA03: LineADoc3; LA04: LineADoc4; " +
			"C506: EntryInstruction1; AAA: EntryInstruction2; " +
			"CES: InvoiceLine21; CCC: InvoiceLine22; " +
			"DDD: InvoiceLine31; EEE: InvoiceLine32";
			AssertEquals("Box44CompleteText correct format for entryLine2 when EntryStatus empty", expectedForEntryLine2, entryLine2.Box44CompleteText);

			entryHeader.CH_EntryStatus = "CLP";

			var expectedForEntryLine1WithStatus = "XS01: ES36000000S1 12-03-2020; XS02: ES36000000S2; C001: ES3700000001 12-03-2020; C002: ES3700000002 31-12-2020; XA01: ES36000000A1; XA02: ES36000000A2; " +
			"C506: EntryInstruction1; AAA: EntryInstruction2; " +
			"CSAS: InvoiceLine11; BBB: InvoiceLine12";
			AssertEquals("Box44CompleteText correct format for entryLine1 when EntryStatus = CLP", expectedForEntryLine1WithStatus, entryLine1.Box44CompleteText);

			var expectedForEntryLine2WithStatus = "XS03: ES36000000S3 31-12-2020; XS05: ES36000000S5 31-12-2020; C001: ES3700000001 12-03-2020; C002: ES3700000002 31-12-2020; XA05: ES36000000A5; " +
			"C506: EntryInstruction1; AAA: EntryInstruction2; " +
			"CES: InvoiceLine21; CCC: InvoiceLine22; " +
			"DDD: InvoiceLine31; EEE: InvoiceLine32";
			AssertEquals("Box44CompleteText correct format for entryLine2 when EntryStatus = CLP", expectedForEntryLine2WithStatus, entryLine2.Box44CompleteText);
		});
	}

	public void TestBox44Contents_WithEverything_Export_T2L()
	{
		SetUpMapData();

		var holder = Factory.New<OrgHeader>();
		holder.OH_Code = "AA";

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, declaration.TablePrefix, "DS01", "DeclarationSDoc1", ZDateTime.Empty));
		declaration.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, declaration.TablePrefix, "DS02", "DeclarationSDoc2", new ZDateTime(2020, 12, 31)));
		declaration.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, declaration.TablePrefix, "DA01", "DeclarationADoc1"));
		declaration.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, declaration.TablePrefix, "DA02", "DeclarationADoc2"));
		declaration.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, declaration.TablePrefix, "DA03", "DeclarationADoc3", subtype: "INF"));

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
		entryInstruction.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, entryInstruction.TablePrefix, holder.PK, "DPO", "EntryInstruction1"));
		entryInstruction.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, entryInstruction.TablePrefix, holder.PK, "AAA", "EntryInstruction2"));

		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		invoiceHeader.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceHeader.TablePrefix, "HS01", "HeaderSDoc1", new ZDateTime(2020, 03, 12)));
		invoiceHeader.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceHeader.TablePrefix, "HS02", "HeaderSDoc2", ZDateTime.Empty));
		invoiceHeader.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceHeader.TablePrefix, "HA01", "HeaderADoc1"));
		invoiceHeader.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceHeader.TablePrefix, "HA02", "HeaderADoc2"));
		invoiceHeader.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceHeader.TablePrefix, "HA03", "HeaderADoc3", subtype: "INF"));

		var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine1.JI_Tariff = "111";
		invoiceLine1.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine1.TablePrefix, holder.PK, "SAS", "InvoiceLine11"));
		invoiceLine1.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine1.TablePrefix, holder.PK, "BBB", "InvoiceLine12"));

		invoiceLine1.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine1.TablePrefix, "LS01", "LineSDoc1", new ZDateTime(2020, 03, 12)));
		invoiceLine1.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine1.TablePrefix, "LS02", "LineSDoc2", new ZDateTime(2020, 12, 31)));
		invoiceLine1.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine1.TablePrefix, "LA01", "LineADoc1"));
		invoiceLine1.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine1.TablePrefix, "LA02", "LineADoc2"));

		var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_Tariff = "222";
		invoiceLine2.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine2.TablePrefix, holder.PK, "OTE", "InvoiceLine21"));
		invoiceLine2.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine2.TablePrefix, holder.PK, "CCC", "InvoiceLine22"));

		invoiceLine2.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine2.TablePrefix, "LS03", "LineSDoc3", ZDateTime.Empty));
		invoiceLine2.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine2.TablePrefix, "LA03", "LineADoc3"));

		Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.CH_EntryStatus = ZString.Empty;

		AddSupDoc(entryHeader.PK, entryHeader.TablePrefix, "C001", "ES3700000001", new ZDateTime(2020, 03, 12), DocumentStatus.Accepted);
		AddSupDoc(entryHeader.PK, entryHeader.TablePrefix, "C002", "ES3700000002", new ZDateTime(2020, 12, 31), DocumentStatus.Accepted, SupportingDocumentSubType.LIQ);
		AddSupDoc(entryHeader.PK, entryHeader.TablePrefix, "C003", "ES3700000003", new ZDateTime(2020, 03, 12), DocumentStatus.Cancelled);

		var entryLine1 = entryHeader.AllEntryLines.FirstOrDefault(x => x.Tariff == "111");
		AddSupDoc(entryLine1.PK, entryLine1.TablePrefix, "XS01", "ES36000000S1", new ZDateTime(2020, 03, 12), DocumentStatus.Accepted);
		AddSupDoc(entryLine1.PK, entryLine1.TablePrefix, "XS02", "ES36000000S2", ZDateTime.Empty, DocumentStatus.Accepted, SupportingDocumentSubType.LIQ);
		AddAddInf(entryLine1.PK, entryLine1.TablePrefix, "XA01", "ES36000000A1", status: DocumentStatus.Accepted);
		AddAddInf(entryLine1.PK, entryLine1.TablePrefix, "XA02", "ES36000000A2", status: DocumentStatus.Accepted);

		var entryLine2 = entryHeader.AllEntryLines.FirstOrDefault(x => x.Tariff == "222");

		AddSupDoc(entryLine2.PK, entryLine2.TablePrefix, "XS03", "ES36000000S3", new ZDateTime(2020, 12, 31), DocumentStatus.Accepted, SupportingDocumentSubType.LIQ);
		AddSupDoc(entryLine2.PK, entryLine2.TablePrefix, "XS04", "ES36000000S4", new ZDateTime(2020, 03, 31), DocumentStatus.Cancelled, SupportingDocumentSubType.LIQ);
		AddSupDoc(entryLine2.PK, entryLine2.TablePrefix, "XS05", "ES36000000S5", new ZDateTime(2020, 12, 31), DocumentStatus.Accepted);
		AddAddInf(entryLine2.PK, entryLine2.TablePrefix, "XA03", "ES36000000A3", subtype: "INF", status: DocumentStatus.Accepted);
		AddAddInf(entryLine2.PK, entryLine2.TablePrefix, "XA04", "ES36000000A4", status: DocumentStatus.Cancelled);
		AddAddInf(entryLine2.PK, entryLine2.TablePrefix, "XA05", "ES36000000A5", status: DocumentStatus.Accepted);

		var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine3.JI_CEI = entryInstruction.PK;
		entryLine2.InvoiceLines.Add(invoiceLine3);
		invoiceLine3.JI_Tariff = "222";
		invoiceLine3.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine3.TablePrefix, holder.PK, "DDD", "InvoiceLine31"));
		invoiceLine3.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine3.TablePrefix, holder.PK, "EEE", "InvoiceLine32"));

		invoiceLine3.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine3.TablePrefix, "LS04", "LineSDoc4", new ZDateTime(2020, 09, 01)));
		invoiceLine3.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine3.TablePrefix, "LA04", "LineADoc4"));
		invoiceLine3.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine3.TablePrefix, "LA05", "LineADoc5", subtype: "INF"));

		CombineAssertions(() =>
		{
			var expectedForEntryLine1 = "DS01: DeclarationSDoc1; DS02: DeclarationSDoc2 31-12-2020; " +
			"HS01: HeaderSDoc1 12-03-2020; HS02: HeaderSDoc2; " +
			"LS01: LineSDoc1 12-03-2020; LS02: LineSDoc2 31-12-2020; " +
			"XS02: ES36000000S2; " +
			"C002: ES3700000002 31-12-2020";
			AssertEquals("Box44CompleteText correct format for entryLine1 when EntryStatus empty", expectedForEntryLine1, entryLine1.Box44CompleteText);

			var expectedForEntryLine2 = "DS01: DeclarationSDoc1; DS02: DeclarationSDoc2 31-12-2020; " +
			"HS01: HeaderSDoc1 12-03-2020; HS02: HeaderSDoc2; " +
			"LS03: LineSDoc3; LS04: LineSDoc4 01-09-2020; " +
			"XS03: ES36000000S3 31-12-2020; XS04: ES36000000S4 31-03-2020; " +
			"C002: ES3700000002 31-12-2020";
			AssertEquals("Box44CompleteText correct format for entryLine2 when EntryStatus empty", expectedForEntryLine2, entryLine2.Box44CompleteText);

			entryHeader.CH_EntryStatus = "CLP";

			var expectedForEntryLine1WithStatus = "XS01: ES36000000S1 12-03-2020; XS02: ES36000000S2; C001: ES3700000001 12-03-2020; C002: ES3700000002 31-12-2020";
			AssertEquals("Box44CompleteText correct format for entryLine1 when EntryStatus = CLP", expectedForEntryLine1WithStatus, entryLine1.Box44CompleteText);

			var expectedForEntryLine2WithStatus = "XS03: ES36000000S3 31-12-2020; XS05: ES36000000S5 31-12-2020; C001: ES3700000001 12-03-2020; C002: ES3700000002 31-12-2020";
			AssertEquals("Box44CompleteText correct format for entryLine2 when EntryStatus = CLP", expectedForEntryLine2WithStatus, entryLine2.Box44CompleteText);
		});
	}

	public void TestBox44Contents_WithEverything_Export_EXS()
	{
		SetUpMapData();

		var holder = Factory.New<OrgHeader>();
		holder.OH_Code = "AA";

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		declaration.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, declaration.TablePrefix, "DS01", "DeclarationSDoc1", ZDateTime.Empty));
		declaration.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, declaration.TablePrefix, "DS02", "DeclarationSDoc2", new ZDateTime(2020, 12, 31)));
		declaration.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, declaration.TablePrefix, "DA01", "DeclarationADoc1"));
		declaration.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, declaration.TablePrefix, "DA02", "DeclarationADoc2"));
		declaration.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, declaration.TablePrefix, "DA03", "DeclarationADoc3", subtype: "INF"));

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
		entryInstruction.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, entryInstruction.TablePrefix, holder.PK, "DPO", "EntryInstruction1"));
		entryInstruction.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, entryInstruction.TablePrefix, holder.PK, "AAA", "EntryInstruction2"));

		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		invoiceHeader.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceHeader.TablePrefix, "HS01", "HeaderSDoc1", new ZDateTime(2020, 03, 12)));
		invoiceHeader.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceHeader.TablePrefix, "HS02", "HeaderSDoc2", ZDateTime.Empty));
		invoiceHeader.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceHeader.TablePrefix, "HA01", "HeaderADoc1"));
		invoiceHeader.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceHeader.TablePrefix, "HA02", "HeaderADoc2"));
		invoiceHeader.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceHeader.TablePrefix, "HA03", "HeaderADoc3", subtype: "INF"));

		var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine1.JI_Tariff = "111";
		invoiceLine1.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine1.TablePrefix, holder.PK, "SAS", "InvoiceLine11"));
		invoiceLine1.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine1.TablePrefix, holder.PK, "BBB", "InvoiceLine12"));

		invoiceLine1.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine1.TablePrefix, "LS01", "LineSDoc1", new ZDateTime(2020, 03, 12)));
		invoiceLine1.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine1.TablePrefix, "LS02", "LineSDoc2", new ZDateTime(2020, 12, 31)));
		invoiceLine1.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine1.TablePrefix, "LA01", "LineADoc1"));
		invoiceLine1.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine1.TablePrefix, "LA02", "LineADoc2"));

		var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_Tariff = "222";
		invoiceLine2.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine2.TablePrefix, holder.PK, "OTE", "InvoiceLine21"));
		invoiceLine2.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine2.TablePrefix, holder.PK, "CCC", "InvoiceLine22"));

		invoiceLine2.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine2.TablePrefix, "LS03", "LineSDoc3", ZDateTime.Empty));
		invoiceLine2.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine2.TablePrefix, "LA03", "LineADoc3"));

		Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.CH_EntryStatus = ZString.Empty;

		AddSupDoc(entryHeader.PK, entryHeader.TablePrefix, "C001", "ES3700000001", new ZDateTime(2020, 03, 12), DocumentStatus.Accepted);
		AddSupDoc(entryHeader.PK, entryHeader.TablePrefix, "C002", "ES3700000002", new ZDateTime(2020, 12, 31), DocumentStatus.Accepted, SupportingDocumentSubType.LIQ);
		AddSupDoc(entryHeader.PK, entryHeader.TablePrefix, "C003", "ES3700000003", new ZDateTime(2020, 03, 12), DocumentStatus.Cancelled);

		var entryLine1 = entryHeader.AllEntryLines.FirstOrDefault(x => x.Tariff == "111");
		AddSupDoc(entryLine1.PK, entryLine1.TablePrefix, "XS01", "ES36000000S1", new ZDateTime(2020, 03, 12), DocumentStatus.Accepted);
		AddSupDoc(entryLine1.PK, entryLine1.TablePrefix, "XS02", "ES36000000S2", ZDateTime.Empty, DocumentStatus.Accepted, SupportingDocumentSubType.LIQ);
		AddAddInf(entryLine1.PK, entryLine1.TablePrefix, "XA01", "ES36000000A1", status: DocumentStatus.Accepted);
		AddAddInf(entryLine1.PK, entryLine1.TablePrefix, "XA02", "ES36000000A2", status: DocumentStatus.Accepted);

		var entryLine2 = entryHeader.AllEntryLines.FirstOrDefault(x => x.Tariff == "222");

		AddSupDoc(entryLine2.PK, entryLine2.TablePrefix, "XS03", "ES36000000S3", new ZDateTime(2020, 12, 31), DocumentStatus.Accepted, SupportingDocumentSubType.LIQ);
		AddSupDoc(entryLine2.PK, entryLine2.TablePrefix, "XS04", "ES36000000S4", new ZDateTime(2020, 03, 31), DocumentStatus.Cancelled, SupportingDocumentSubType.LIQ);
		AddSupDoc(entryLine2.PK, entryLine2.TablePrefix, "XS05", "ES36000000S5", new ZDateTime(2020, 12, 31), DocumentStatus.Accepted);
		AddAddInf(entryLine2.PK, entryLine2.TablePrefix, "XA03", "ES36000000A3", subtype: "INF", status: DocumentStatus.Accepted);
		AddAddInf(entryLine2.PK, entryLine2.TablePrefix, "XA04", "ES36000000A4", status: DocumentStatus.Cancelled);
		AddAddInf(entryLine2.PK, entryLine2.TablePrefix, "XA05", "ES36000000A5", status: DocumentStatus.Accepted);

		var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine3.JI_CEI = entryInstruction.PK;
		entryLine2.InvoiceLines.Add(invoiceLine3);
		invoiceLine3.JI_Tariff = "222";
		invoiceLine3.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine3.TablePrefix, holder.PK, "DDD", "InvoiceLine31"));
		invoiceLine3.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine3.TablePrefix, holder.PK, "EEE", "InvoiceLine32"));

		invoiceLine3.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine3.TablePrefix, "LS04", "LineSDoc4", new ZDateTime(2020, 09, 01)));
		invoiceLine3.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine3.TablePrefix, "LA04", "LineADoc4"));
		invoiceLine3.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine3.TablePrefix, "LA05", "LineADoc5", subtype: "INF"));

		CombineAssertions(() =>
		{
			var expectedForEntryLine1 = "DS01: DeclarationSDoc1; DS02: DeclarationSDoc2 31-12-2020; " +
			"HS01: HeaderSDoc1 12-03-2020; HS02: HeaderSDoc2; " +
			"LS01: LineSDoc1 12-03-2020; LS02: LineSDoc2 31-12-2020; " +
			"XS02: ES36000000S2; " +
			"C002: ES3700000002 31-12-2020";
			AssertEquals("Box44CompleteText correct format for entryLine1 when EntryStatus empty", expectedForEntryLine1, entryLine1.Box44CompleteText);

			var expectedForEntryLine2 = "DS01: DeclarationSDoc1; DS02: DeclarationSDoc2 31-12-2020; " +
			"HS01: HeaderSDoc1 12-03-2020; HS02: HeaderSDoc2; " +
			"LS03: LineSDoc3; LS04: LineSDoc4 01-09-2020; " +
			"XS03: ES36000000S3 31-12-2020; XS04: ES36000000S4 31-03-2020; " +
			"C002: ES3700000002 31-12-2020";
			AssertEquals("Box44CompleteText correct format for entryLine2 when EntryStatus empty", expectedForEntryLine2, entryLine2.Box44CompleteText);

			entryHeader.CH_EntryStatus = "CLP";

			var expectedForEntryLine1WithStatus = "XS01: ES36000000S1 12-03-2020; XS02: ES36000000S2; C001: ES3700000001 12-03-2020; C002: ES3700000002 31-12-2020";
			AssertEquals("Box44CompleteText correct format for entryLine1 when EntryStatus = CLP", expectedForEntryLine1WithStatus, entryLine1.Box44CompleteText);

			var expectedForEntryLine2WithStatus = "XS03: ES36000000S3 31-12-2020; XS05: ES36000000S5 31-12-2020; C001: ES3700000001 12-03-2020; C002: ES3700000002 31-12-2020";
			AssertEquals("Box44CompleteText correct format for entryLine2 when EntryStatus = CLP", expectedForEntryLine2WithStatus, entryLine2.Box44CompleteText);
		});
	}

	public void TestBox44Contents_WithEverything_Import()
	{
		SetUpMapData();

		var holder = Factory.New<OrgHeader>();
		holder.OH_Code = "AA";

		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, declaration.TablePrefix, "DS01", "DeclarationSDoc1", ZDateTime.Empty));
		declaration.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, declaration.TablePrefix, "DS02", "DeclarationSDoc2", new ZDateTime(2020, 12, 31)));
		declaration.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, declaration.TablePrefix, "DA01", "DeclarationADoc1"));
		declaration.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, declaration.TablePrefix, "DA02", "DeclarationADoc2"));
		declaration.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, declaration.TablePrefix, "DA03", "DeclarationADoc3", subtype: "INF"));

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		entryInstruction.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, entryInstruction.TablePrefix, holder.PK, "DPO", "EntryInstruction1"));
		entryInstruction.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, entryInstruction.TablePrefix, holder.PK, "AAA", "EntryInstruction2"));

		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		invoiceHeader.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceHeader.TablePrefix, "HS01", "HeaderSDoc1", new ZDateTime(2020, 03, 12)));
		invoiceHeader.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceHeader.TablePrefix, "HS02", "HeaderSDoc2", ZDateTime.Empty));
		invoiceHeader.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceHeader.TablePrefix, "HA01", "HeaderADoc1"));
		invoiceHeader.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceHeader.TablePrefix, "HA02", "HeaderADoc2"));
		invoiceHeader.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceHeader.TablePrefix, "HA03", "HeaderADoc3", subtype: "INF"));

		var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine1.JI_Tariff = "111";
		invoiceLine1.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine1.TablePrefix, holder.PK, "SAS", "InvoiceLine11"));
		invoiceLine1.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine1.TablePrefix, holder.PK, "BBB", "InvoiceLine12"));

		invoiceLine1.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine1.TablePrefix, "LS01", "LineSDoc1", new ZDateTime(2020, 03, 12)));
		invoiceLine1.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine1.TablePrefix, "LS02", "LineSDoc2", new ZDateTime(2020, 12, 31)));
		invoiceLine1.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine1.TablePrefix, "LA01", "LineADoc1"));
		invoiceLine1.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine1.TablePrefix, "LA02", "LineADoc2"));

		var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_Tariff = "222";
		invoiceLine2.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine2.TablePrefix, holder.PK, "OTE", "InvoiceLine21"));
		invoiceLine2.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine2.TablePrefix, holder.PK, "CCC", "InvoiceLine22"));

		invoiceLine2.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine2.TablePrefix, "LS03", "LineSDoc3", ZDateTime.Empty));
		invoiceLine2.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine2.TablePrefix, "LA03", "LineADoc3"));

		Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.CH_EntryStatus = ZString.Empty;

		AddSupDoc(entryHeader.PK, entryHeader.TablePrefix, "C001", "ES3700000001", new ZDateTime(2020, 03, 12), DocumentStatus.Accepted);
		AddSupDoc(entryHeader.PK, entryHeader.TablePrefix, "C002", "ES3700000002", new ZDateTime(2020, 12, 31), DocumentStatus.Accepted, SupportingDocumentSubType.LIQ);
		AddSupDoc(entryHeader.PK, entryHeader.TablePrefix, "C003", "ES3700000003", new ZDateTime(2020, 03, 12), DocumentStatus.Cancelled);

		var entryLine1 = entryHeader.AllEntryLines.FirstOrDefault(x => x.Tariff == "111");
		AddSupDoc(entryLine1.PK, entryLine1.TablePrefix, "XS01", "ES36000000S1", new ZDateTime(2020, 03, 12), DocumentStatus.Accepted);
		AddSupDoc(entryLine1.PK, entryLine1.TablePrefix, "XS02", "ES36000000S2", ZDateTime.Empty, DocumentStatus.Accepted, SupportingDocumentSubType.LIQ);
		AddAddInf(entryLine1.PK, entryLine1.TablePrefix, "XA01", "ES36000000A1", status: DocumentStatus.Accepted);
		AddAddInf(entryLine1.PK, entryLine1.TablePrefix, "XA02", "ES36000000A2", status: DocumentStatus.Accepted);

		var entryLine2 = entryHeader.AllEntryLines.FirstOrDefault(x => x.Tariff == "222");

		AddSupDoc(entryLine2.PK, entryLine2.TablePrefix, "XS03", "ES36000000S3", new ZDateTime(2020, 12, 31), DocumentStatus.Accepted, SupportingDocumentSubType.LIQ);
		AddSupDoc(entryLine2.PK, entryLine2.TablePrefix, "XS04", "ES36000000S4", new ZDateTime(2020, 03, 31), DocumentStatus.Cancelled, SupportingDocumentSubType.LIQ);
		AddSupDoc(entryLine2.PK, entryLine2.TablePrefix, "XS05", "ES36000000S5", new ZDateTime(2020, 12, 31), DocumentStatus.Accepted);
		AddAddInf(entryLine2.PK, entryLine2.TablePrefix, "XA03", "ES36000000A3", subtype: "INF", status: DocumentStatus.Accepted);
		AddAddInf(entryLine2.PK, entryLine2.TablePrefix, "XA04", "ES36000000A4", status: DocumentStatus.Cancelled);
		AddAddInf(entryLine2.PK, entryLine2.TablePrefix, "XA05", "ES36000000A5", status: DocumentStatus.Accepted);

		var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine3.JI_CEI = entryInstruction.PK;
		entryLine2.InvoiceLines.Add(invoiceLine3);
		invoiceLine3.JI_Tariff = "222";
		invoiceLine3.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine3.TablePrefix, holder.PK, "DDD", "InvoiceLine31"));
		invoiceLine3.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine3.TablePrefix, holder.PK, "EEE", "InvoiceLine32"));

		invoiceLine3.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine3.TablePrefix, "LS04", "LineSDoc4", new ZDateTime(2020, 09, 01)));
		invoiceLine3.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine3.TablePrefix, "LA04", "LineADoc4"));
		invoiceLine3.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine3.TablePrefix, "LA05", "LineADoc5", subtype: "INF"));

		CombineAssertions(() =>
		{
			var expectedForEntryLine1 = "DS01: DeclarationSDoc1; DS02: DeclarationSDoc2 31-12-2020; " +
			"HS01: HeaderSDoc1 12-03-2020; HS02: HeaderSDoc2; " +
			"LS01: LineSDoc1 12-03-2020; LS02: LineSDoc2 31-12-2020; " +
			"XS02: ES36000000S2; " +
			"C002: ES3700000002 31-12-2020";
			AssertEquals("Box44CompleteText correct format for entryLine1 when EntryStatus empty", expectedForEntryLine1, entryLine1.Box44CompleteText);

			var expectedForEntryLine2 = "DS01: DeclarationSDoc1; DS02: DeclarationSDoc2 31-12-2020; " +
			"HS01: HeaderSDoc1 12-03-2020; HS02: HeaderSDoc2; " +
			"LS03: LineSDoc3; LS04: LineSDoc4 01-09-2020; " +
			"XS03: ES36000000S3 31-12-2020; XS04: ES36000000S4 31-03-2020; " +
			"C002: ES3700000002 31-12-2020";
			AssertEquals("Box44CompleteText correct format for entryLine2 when EntryStatus empty", expectedForEntryLine2, entryLine2.Box44CompleteText);

			entryHeader.CH_EntryStatus = "CLP";

			var expectedForEntryLine1WithStatus = "XS01: ES36000000S1 12-03-2020; XS02: ES36000000S2; C001: ES3700000001 12-03-2020; C002: ES3700000002 31-12-2020";
			AssertEquals("Box44CompleteText correct format for entryLine1 when EntryStatus = CLP", expectedForEntryLine1WithStatus, entryLine1.Box44CompleteText);

			var expectedForEntryLine2WithStatus = "XS03: ES36000000S3 31-12-2020; XS05: ES36000000S5 31-12-2020; C001: ES3700000001 12-03-2020; C002: ES3700000002 31-12-2020";
			AssertEquals("Box44CompleteText correct format for entryLine2 when EntryStatus = CLP", expectedForEntryLine2WithStatus, entryLine2.Box44CompleteText);
		});
	}

	SupportingDocument AddSupDoc(ZGuid parentPK, ZString parentTableCode, ZString code, ZString refNum, ZDateTime dateOfExpiry, string status = "", string subtype = "")
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

	AdditionalInfo AddAddInf(ZGuid parentPK, ZString parentTableCode, ZString code, ZString refNum, string subtype = "TRA", string status = "")
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

	CusAuthorizationUsage AddAuthorization(ZGuid parentPK, ZString parentTableCode, ZGuid holderPK, ZString authType, ZString authNumber)
	{
		var authorisation = Factory.New<CusAuthorisationHeader>();
		authorisation.CPH_OH_PermitHolder = holderPK;
		authorisation.CPH_Type = authType;
		authorisation.CPH_StartDate = new ZDate(2021, 11, 03);
		authorisation.CPH_Number = authNumber;

		var auth = Factory.New<CusAuthorizationUsage>();
		auth.AGC_ParentID = parentPK;
		auth.AGC_ParentTableCode = parentTableCode;
		auth.AGC_Code = authType;
		auth.AGC_Number = authNumber;
		auth.AGC_OH_Owner = holderPK;
		return auth;
	}

	void SetUpMapData()
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

	public void TestCalculateStatisticalValueExport()
	{
		/*
		--Declaration
			--Invoice1
				--InvoiceLine1_1 (LinePrice = 1m) => Contribute 1m to StatisticalValue.
					--Charges
						--Charge1_1_1 (ONS, Amount = 10m EUR) => Contribute 10m to StatisticalValue.
						--Charge1_1_2 (ABC, Amount = 100m EUR) => Exclusive as wrong charge code.
				--InvoiceLine1_2 (LinePrice = 1000m) => Contribute 1000m to StatisticalValue
			--Invoice2
				--InvoiceLine2_1 (LinePrice = 10000m) => Contribute 10000m to StatisticalValue
		*/

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var localCurrencyCode = declaration.LocalCurrencyCode;

		var invoice1 = declaration.Invoices.AddNew();
		invoice1.JZ_RX_NKInvoice_Currency = localCurrencyCode;
		var invoiceLine1_1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1_1.JI_LinePrice = 1m;

		var charge1_1_1 = invoiceLine1_1.Charges.AddNew();
		charge1_1_1.J7_ChargeType = "ONS";
		charge1_1_1.J7_RX_NKCurrency = localCurrencyCode;
		charge1_1_1.J7_Amount = 10m;
		charge1_1_1.J7_IsStatisticalValueApplicable = true;
		charge1_1_1.J7_IsDutiable = true;
		charge1_1_1.J7_IsIncludedInITOT = true;

		var charge1_1_2 = invoiceLine1_1.Charges.AddNew();
		charge1_1_2.J7_ChargeType = "ABC";
		charge1_1_2.J7_RX_NKCurrency = localCurrencyCode;
		charge1_1_2.J7_Amount = 100m;
		charge1_1_1.J7_IsStatisticalValueApplicable = true;

		var invoiceLine1_2 = invoice1.InvoiceLines.AddNew();
		invoiceLine1_2.JI_LinePrice = 1000m;

		var invoice2 = declaration.Invoices.AddNew();
		invoice2.JZ_RX_NKInvoice_Currency = localCurrencyCode;
		var invoiceLine2_1 = invoice2.InvoiceLines.AddNew();
		invoiceLine2_1.JI_LinePrice = 10000m;

		CombineAssertions("Export Declaration Statistical Value", () =>
		{
			AssertEquals("Test Export Declaration Invoice Line Statistical value with charges(1-10 = -9)", 1m - 10m, invoiceLine1_1.JI_Calc_StatisticalValue);
			AssertEquals("Test Export Declaration Invoice Line Statistical value without charges", 1000m, invoiceLine1_2.JI_Calc_StatisticalValue);
			AssertEquals("Test Export Declaration Invoice Line Statistical value new invoice without charges", 10000m, invoiceLine2_1.JI_Calc_StatisticalValue);

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			AssertEquals("Test Export Declaration Statistical value with charges(1+1000+10000-10 = 10991)", 1m + 1000m + 10000m - 10m, ((CusEntryHeader)declaration.ActiveEntryHeaders[0]).AllEntryLines[0].CL_StatisticalValue);
		});
	}

	public void TestBox45CompleteText()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = declaration.InvoiceLines.AddNew();
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

		AssertEquals("Merge done", true, declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));

		var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

		AssertEquals("Test Box 45 Complete test without values", "+0 -0", entryLine.Box45CompleteText());

		invoice.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
		var groupHeader = invoice.GroupHeader;

		var oNS = groupHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge, 30m, declaration.LocalCurrencyCode);
		oNS.J7_IsDutiable = true;
		oNS.J7_IsIncludedInITOT = false;
		var oFT = invoiceLine.Charges.AddNew(UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge, 2000.05m, declaration.LocalCurrencyCode);
		oFT.J7_IsDutiable = false;
		oFT.J7_IsIncludedInITOT = true;
		var oL1 = invoiceLine.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 3m, declaration.LocalCurrencyCode);
		oL1.J7_IsDutiable = true;
		oL1.J7_IsIncludedInITOT = false;
		var oL2 = invoiceLine.Charges.AddNew(UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge, 2m, declaration.LocalCurrencyCode);
		oL2.J7_IsDutiable = false;
		oL2.J7_IsIncludedInITOT = true;
		invoiceLine.Charges.AddNew(ESCustomsChargeTypeList.Codes.ExportedGoodsValueForOutwardProcessing, 33m, declaration.LocalCurrencyCode);

		invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
		invoice.JZ_InvoiceAmount = 100m;
		invoiceLine.JI_LinePrice = 100m;
		declaration.ResumeApportionment();

		entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

		AssertEquals("Test Box 45 Complete test with values", "+66 -2,002.05", entryLine.Box45CompleteText());
	}

	public void TestHasNonEmptyPackage()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine1.JI_InvoiceQuantity = 0;
		var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
		package1.CW_PackQty = 0;
		package1.CW_PackType = "AA";
		package1.CW_MarksAndNos = "AAAAAAAAAAAA";

		var packing1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0];
		packing1.IsLinked = true;
		packing1.PackQty = 0;

		var entryLine = Factory.New<CusEntryLine>();
		invoiceLine1.JI_CL = entryLine.PK;
		AssertEquals("Empty package when is not unpacked and it has no packages", false, entryLine.HasNonEmptyPackage);

		package1.CW_PackType = EU.Business.UniversalReferenceConstants.RefCusCodeUnPackedPackageUnitType.Unpacked;
		AssertEquals("Empty package when is unpacked and it has no packages", false, entryLine.HasNonEmptyPackage);

		invoiceLine1.JI_InvoiceQuantity = 1;
		AssertEquals("Non empty package when is unpacked and has 1 item (JI_InvoiceQuantity)", true, entryLine.HasNonEmptyPackage);

		package1.CW_PackType = "AA";
		package1.CW_PackQty = 1;
		packing1.PackQty = 1;
		AssertEquals("Non empty package when is not unpacked and has packages", true, entryLine.HasNonEmptyPackage);
	}

	public void TestPackage()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine1.JI_InvoiceQuantity = 10;
		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine2.JI_InvoiceQuantity = 20;
		var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine3.JI_InvoiceQuantity = 40;
		var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine4.JI_InvoiceQuantity = 80;
		var invoiceLine5 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine5.JI_InvoiceQuantity = 160;

		var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
		package1.CW_PackQty = 1;
		package1.CW_PackType = "AA";
		package1.CW_MarksAndNos = "AAAAAAAAAAAA";

		var package2 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
		package2.CW_PackQty = 2;
		package2.CW_PackType = "AA";
		package2.CW_MarksAndNos = "AAAAAAAAAAAA";

		var package3 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
		package3.CW_PackQty = 4;
		package3.CW_PackType = "AA";
		package3.CW_MarksAndNos = "BBBBBBBBBBBB";

		var package4 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
		package4.CW_PackQty = 8;
		package4.CW_PackType = "BB";
		package4.CW_MarksAndNos = "AAAAAAAAAAAA";

		var package5 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
		package5.CW_PackQty = 16;
		package5.CW_PackType = "BB";
		package5.CW_MarksAndNos = "BBBBBBBBBBBB";

		var packing1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0];
		packing1.IsLinked = true;
		packing1.PackQty = 1;

		var packing2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly[1];
		packing2.IsLinked = true;
		packing2.PackQty = 2;

		var packing3 = invoiceLine3.PackagesForInvoiceLinesForBindingOnly[2];
		packing3.IsLinked = true;
		packing3.PackQty = 4;

		var packing4 = invoiceLine4.PackagesForInvoiceLinesForBindingOnly[3];
		packing4.IsLinked = true;
		packing4.PackQty = 8;

		var packing5 = invoiceLine4.PackagesForInvoiceLinesForBindingOnly[4];
		packing5.IsLinked = true;
		packing5.PackQty = 16;

		var entryLine = Factory.New<CusEntryLine>();

		invoiceLine1.JI_CL = entryLine.PK;
		invoiceLine2.JI_CL = entryLine.PK;
		invoiceLine3.JI_CL = entryLine.PK;
		invoiceLine4.JI_CL = entryLine.PK;
		invoiceLine5.JI_CL = entryLine.PK;

		AssertEquals(5, entryLine.PackagingDetails.Count());
		AssertEquals("AA", entryLine.Package.Type);
		AssertEquals("AAAAAAAAAAAA", entryLine.Package.MarksAndNos);
		AssertEquals(3, entryLine.Package.PackCount);
		AssertEquals(30m, entryLine.Package.ItemsCount);
	}

	public void TestHasSupportingDocumentsToSend()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_Tariff = "2203001011";

		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		var entryHeader = declaration.CustomsEntryHeaders[0];
		var entryLine = entryHeader.MergedLines[0];

		CombineAssertions(() =>
		{
			AssertEquals("Expected false when no supporting documents are declared", false, entryLine.HasSupportingDocumentsToSend());

			var supdoc1 = declaration.SupportingDocuments.AddNew();
			supdoc1.CSI_Code = "9001";
			AssertEquals("Expected true when there is at least one supporting document declared to send (in declaration)", true, entryLine.HasSupportingDocumentsToSend());

			var supdoc2 = Factory.New<SupportingDocument>();
			supdoc2.CSI_ParentID = entryLine.PK;
			supdoc2.CSI_ParentTableCode = entryLine.TablePrefix;
			supdoc2.CSI_Status = "ACC";
			supdoc2.CSI_Code = "9001";
			AssertEquals("Expected false when the supporting document declared has already been sent (in declaration and in entryline)", false, entryLine.HasSupportingDocumentsToSend());

			var supdoc3 = invoiceHeader.SupportingDocuments.AddNew();
			supdoc3.CSI_Code = "9002";
			AssertEquals("Expected true when there is at least one supporting document declared to send (in invoiceHeader)", true, entryLine.HasSupportingDocumentsToSend());

			var supdoc4 = Factory.New<SupportingDocument>();
			supdoc4.CSI_ParentID = entryLine.PK;
			supdoc4.CSI_ParentTableCode = entryLine.TablePrefix;
			supdoc4.CSI_Status = "ACC";
			supdoc4.CSI_Code = "9002";
			AssertEquals("Expected false when the supporting document declared has already been sent (in invoiceHeader and in entryline)", false, entryLine.HasSupportingDocumentsToSend());

			var supdoc5 = invoiceLine.SupportingDocuments.AddNew();
			supdoc5.CSI_Code = "9003";
			AssertEquals("Expected true when there is at least one supporting document declared to send (in invoiceLine)", true, entryLine.HasSupportingDocumentsToSend());

			var supdoc6 = Factory.New<SupportingDocument>();
			supdoc6.CSI_ParentID = entryLine.PK;
			supdoc6.CSI_ParentTableCode = entryLine.TablePrefix;
			supdoc6.CSI_Status = "ACC";
			supdoc6.CSI_Code = "9003";
			AssertEquals("Expected false when the supporting document declared has already been sent (in invoiceLine and in entryline)", false, entryLine.HasSupportingDocumentsToSend());
		});
	}

	public void TestGetPreviouslySentSupportingDocuments()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "EntryNum";
		var entryLine = entryHeader.AllEntryLines.AddNew();

		var suppDoc1 = declaration.SupportingDocuments.AddNew();
		suppDoc1.CSI_Code = "X001";
		suppDoc1.CSI_ReferenceNumber = "ES3600000001";

		CombineAssertions(() =>
		{
			AssertEquals("There are no entry line SupportingDocuments", 0, entryLine.GetPreviouslySentSupportingDocuments().Length);

			var supDoc = Factory.New<SupportingDocument>();
			supDoc.CSI_Code = "X002";
			supDoc.CSI_ReferenceNumber = "ES3600000002";
			supDoc.CSI_ParentTableCode = entryLine.TablePrefix;
			supDoc.CSI_ParentID = entryLine.PK;

			var clSupDocs = entryLine.GetPreviouslySentSupportingDocuments();
			AssertEquals("There is 1 entry line SupportingDocument", 1, clSupDocs.Length);
			AssertEquals("CSI_Code is correct", "X002", clSupDocs[0].CSI_Code);
			AssertEquals("CSI_ReferenceNumber is correct", "ES3600000002", clSupDocs[0].CSI_ReferenceNumber);
		});
	}

	public void TestGetPreviouslySentPreviousDocuments()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "EntryNum";
		var entryLine = entryHeader.AllEntryLines.AddNew();

		var prevDoc1 = declaration.PreviousDocuments.AddNew();
		prevDoc1.CSI_Code = "X001";
		prevDoc1.CSI_ReferenceNumber = "ES3600000001";

		CombineAssertions(() =>
		{
			AssertEquals("There are no entry line PreviousDocuments", 0, entryLine.GetPreviouslySentPreviousDocuments().Length);

			var prevDoc = Factory.New<PreviousDocument>();
			prevDoc.CSI_Code = "X002";
			prevDoc.CSI_ReferenceNumber = "ES3600000002";
			prevDoc.CSI_ParentTableCode = entryLine.TablePrefix;
			prevDoc.CSI_ParentID = entryLine.PK;

			var clPrevDocs = entryLine.GetPreviouslySentPreviousDocuments();
			AssertEquals("There is 1 entry line PreviousDocument", 1, clPrevDocs.Length);
			AssertEquals("CSI_Code is correct", "X002", clPrevDocs[0].CSI_Code);
			AssertEquals("CSI_ReferenceNumber is correct", "ES3600000002", clPrevDocs[0].CSI_ReferenceNumber);
		});
	}

	public void TestClearReadOnlyPreviousDocumentsWhenDelete()
	{
		var entryLine = GetEntryLineWithPreviousDocuments();
		var readOnlyPreviousDocuments = entryLine.ReadOnlyPreviousDocuments;
		CombineAssertions(() =>
		{
			AssertEquals("ReadOnly Previous Documents count is 5", 5, readOnlyPreviousDocuments.Count);
			entryLine.Delete();
			AssertEquals("ReadOnly Previous Documents cleared after entryLine is deleted", 0, readOnlyPreviousDocuments.Count);
		});
	}

	public void TestNoLoadReadOnlyPreviousDocumentsForDeletedEntryLine()
	{
		var entryLine = GetEntryLineWithPreviousDocuments();
		CombineAssertions(() =>
		{
			AssertEquals("ReadOnly Previous Documents count is 5", 5, entryLine.ReadOnlyPreviousDocuments.Count);
			entryLine.ResetReadOnlyPreviousDocuments();
			entryLine.Delete();
			AssertEquals("ReadOnly Previous Documents do not load after entryLine is deleted", 0, entryLine.ReadOnlyPreviousDocuments.Count);
		});
	}

	public void TestReadOnlyPreviousDocuments()
	{
		SetPreviousDocumentsRefData();

		var declaration = GetJobDeclarationForTest();
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		var invoiceHeader1 = declaration.Invoices.AddNew();

		foreach (var prevDoc in GetPreviousDocsForAggregationMergeTest())
		{
			var invLine = invoiceHeader1.JobComInvoiceLines.AddNew();
			invLine.PreviousDocuments.Add(prevDoc);
		}

		DoMerge(declaration);

		CombineAssertions(() =>
		{
			AssertEquals("Expecting 1 entry", 1, declaration.ActiveEntryHeaders.Count);
			var lines = declaration.ActiveEntryHeaders[0].MergedLines.Cast<CusEntryLine>().ToArray();
			foreach (var line in lines)
			{
				AssertEquals("ReadOnly Previous Documents count", 1, line.ReadOnlyPreviousDocuments.Count);
			}
		});
	}

	public void TestResetReadOnlyPreviousDocuments()
	{
		SetPreviousDocumentsRefData();

		var declaration = GetJobDeclarationForTest();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = (CusEntryLine)entryHeader.MergedLines.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine.PK;

		CombineAssertions(() =>
		{
			AssertEquals("ReadOnly Previous Documents count is 0", 0, entryLine.ReadOnlyPreviousDocuments.Count);

			foreach (var prevDoc in GetPreviousDocsForAggregationMergeTest())
			{
				invoiceLine1.PreviousDocuments.Add(prevDoc);
			}

			AssertEquals("ReadOnly Previous Documents count is 0 before reset", 0, entryLine.ReadOnlyPreviousDocuments.Count);
			entryLine.ResetReadOnlyPreviousDocuments();
			AssertEquals("ReadOnly Previous Documents count is correct after reset", 5, entryLine.ReadOnlyPreviousDocuments.Count);
		});
	}

	public void TestClearReadOnlyAdditionalInfosWhenDelete()
	{
		var entryLine = GetEntryLineWithAdditionalInfos();
		var readOnlyAdditionalInfos = entryLine.ReadOnlyAdditionalInfos;
		CombineAssertions(() =>
		{
			AssertEquals("ReadOnly Additional Infos count is 5", 5, readOnlyAdditionalInfos.Count);
			entryLine.Delete();
			AssertEquals("ReadOnly Additional Infos cleared after entryLine is deleted", 0, readOnlyAdditionalInfos.Count);
		});
	}

	public void TestNoLoadReadOnlyAdditionalInfosForDeletedEntryLine()
	{
		var entryLine = GetEntryLineWithAdditionalInfos();
		CombineAssertions(() =>
		{
			AssertEquals("ReadOnly Additional Infos count is 5", 5, entryLine.ReadOnlyAdditionalInfos.Count);
			entryLine.ResetReadOnlyAdditionalInfos();
			entryLine.Delete();
			AssertEquals("ReadOnly Additional Infos do not load after entryLine is deleted", 0, entryLine.ReadOnlyAdditionalInfos.Count);
		});
	}

	public void TestReadOnlyAdditionalInfos()
	{
		var declaration = GetJobDeclarationForTest();
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		var invoiceHeader1 = declaration.Invoices.AddNew();

		var additionalInfos = GetAdditionalInfosForAggregationMergeTest();
		for (int i = 0; i < additionalInfos.Count; i++)
		{
			var invLine = invoiceHeader1.JobComInvoiceLines.AddNew();
			invLine.JI_Tariff = "11" + i;
			invLine.AdditionalInfos.Add(additionalInfos[i]);
		}

		DoMerge(declaration);
		CombineAssertions(() =>
		{
			AssertEquals("Expecting 1 entry", 1, declaration.ActiveEntryHeaders.Count);
			var lines = declaration.ActiveEntryHeaders[0].MergedLines.Cast<CusEntryLine>().ToArray();
			foreach (var line in lines)
			{
				AssertEquals("ReadOnly Additional Infos count", 1, line.ReadOnlyAdditionalInfos.Count);
			}
		});
	}

	public void TestResetReadOnlyAdditionalInfos()
	{
		var declaration = GetJobDeclarationForTest();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = (CusEntryLine)entryHeader.MergedLines.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine.PK;

		CombineAssertions(() =>
		{
			AssertEquals("ReadOnly Additional Infos count is 0", 0, entryLine.ReadOnlyAdditionalInfos.Count);

			foreach (var addInf in GetAdditionalInfosForAggregationMergeTest())
			{
				invoiceLine1.AdditionalInfos.Add(addInf);
			}

			AssertEquals("ReadOnly Additional Infos count is 0 before reset", 0, entryLine.ReadOnlyAdditionalInfos.Count);
			entryLine.ResetReadOnlyAdditionalInfos();
			AssertEquals("ReadOnly Additional Infos count is correct after reset", 5, entryLine.ReadOnlyAdditionalInfos.Count);
		});
	}

	public void TestGetPreviouslySentAdditionalInfos()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "EntryNum";
		var entryLine = entryHeader.AllEntryLines.AddNew();

		var prevDoc1 = declaration.PreviousDocuments.AddNew();
		prevDoc1.CSI_Code = "X001";
		prevDoc1.CSI_ReferenceNumber = "ES3600000001";

		CombineAssertions(() =>
		{
			AssertEquals("There are no entry line SentAdditionalInfos", 0, entryLine.GetPreviouslySentAdditionalInfos().Length);

			var addInf = Factory.New<AdditionalInfo>();
			addInf.CSI_Code = "X002";
			addInf.CSI_ReferenceNumber = "ES3600000002";
			addInf.CSI_ParentTableCode = entryLine.TablePrefix;
			addInf.CSI_ParentID = entryLine.PK;

			var clAddInfos = entryLine.GetPreviouslySentAdditionalInfos();
			AssertEquals("There is 1 entry line SentAdditionalInfo", 1, clAddInfos.Length);
			AssertEquals("CSI_Code is correct", "X002", clAddInfos[0].CSI_Code);
			AssertEquals("CSI_ReferenceNumber is correct", "ES3600000002", clAddInfos[0].CSI_ReferenceNumber);
		});
	}

	public void TestVehiclesQty()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		var vehicle1 = invoiceLine1.Vehicles.AddNew();
		invoiceLine1.JI_Tariff = "2203001011";
		vehicle1.CVH_VehicleIdentificationNumber = "VIN1";
		var vehicle11 = invoiceLine1.Vehicles.AddNew();
		vehicle11.CVH_VehicleIdentificationNumber = "VIN11";
		var vehicle12 = invoiceLine1.Vehicles.AddNew();
		vehicle12.CVH_VehicleIdentificationNumber = "VIN12";

		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		var vehicle2 = invoiceLine2.Vehicles.AddNew();
		invoiceLine2.JI_Tariff = "2203001011";
		vehicle2.CVH_VehicleIdentificationNumber = "VIN2";

		var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
		var vehicle3 = invoiceLine3.Vehicles.AddNew();
		invoiceLine3.JI_Tariff = "2203001011";
		vehicle3.CVH_VehicleIdentificationNumber = "VIN3";

		var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew();
		var vehicle4 = invoiceLine4.Vehicles.AddNew();
		invoiceLine4.JI_Tariff = "2203001011";
		vehicle4.CVH_VehicleIdentificationNumber = "VIN3";

		var invoiceLine5 = invoiceHeader.InvoiceLines.AddNew();
		var vehicle5 = invoiceLine5.Vehicles.AddNew();
		invoiceLine5.JI_Tariff = "2203001012";
		vehicle5.CVH_VehicleIdentificationNumber = "VIN4";

		var declarationBill = declaration.Bills.AddNew();
		var billPackingGroup = (EU.Business.Declaration.PackingGroup)declarationBill.PackingGroups.AddNew();

		var package = declaration.Packages.AddNew();
		package.CW_CR_HouseContainer = billPackingGroup.PK;
		package.CW_PackType = PackageType.Frame;

		var collection1 = (InvoiceLineCusLinkPackageCollection)invoiceLine5.PackagesForInvoiceLinesForBindingOnly;

		var linkPackage = collection1.AddNew();
		linkPackage.Package = package;
		linkPackage.IsLinked = true;
		linkPackage.PackQty = 5;

		var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge", true, mergeResult);

		var entryHeader = declaration.CustomsEntryHeaders[0];

		CombineAssertions(() =>
		{
			AssertEquals("VehiclesQty is the sum of Vehicles in Entry lines that has no FR", 6, entryHeader.AllEntryLines[0].VehiclesQty);
			AssertEquals("VehiclesQty is the sum of FR in Entry lines that has FR", 5, entryHeader.AllEntryLines[1].VehiclesQty);
		});
	}

	public void TestInvoiceLinesWithVehicles()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		var vehicle1 = invoiceLine1.Vehicles.AddNew();
		invoiceLine1.JI_Tariff = "2203001011";
		vehicle1.CVH_VehicleIdentificationNumber = "VIN1";

		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		var vehicle2 = invoiceLine2.Vehicles.AddNew();
		invoiceLine2.JI_Tariff = "2203001011";
		vehicle2.CVH_VehicleIdentificationNumber = "VIN1";

		var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
		var vehicle3 = invoiceLine3.Vehicles.AddNew();
		invoiceLine3.JI_Tariff = "2203001011";
		vehicle3.CVH_VehicleIdentificationNumber = "VIN2";

		var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew();
		var vehicle4 = invoiceLine4.Vehicles.AddNew();
		invoiceLine4.JI_Tariff = "2203001011";
		vehicle4.CVH_BrandName = "brand1";

		var invoiceLine5 = invoiceHeader.InvoiceLines.AddNew();
		var vehicle5 = invoiceLine5.Vehicles.AddNew();
		invoiceLine5.JI_Tariff = "2203001011";
		vehicle5.CVH_ModelName = "model1";

		var invoiceLine6 = invoiceHeader.InvoiceLines.AddNew();
		var vehicle6 = invoiceLine6.Vehicles.AddNew();
		invoiceLine6.JI_Tariff = "2203001012";
		vehicle6.CVH_VehicleIdentificationNumber = "VIN2";

		var invoiceLine7 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine7.JI_Tariff = "2203001012";

		var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge", true, mergeResult);

		var entryHeader = declaration.CustomsEntryHeaders[0];

		CombineAssertions(() =>
		{
			AssertEquals("InvoiceLines for first entry line has 5 merged invoiceLines", 5, entryHeader.AllEntryLines[0].InvoiceLines.Count);
			AssertEquals("InvoiceLinesWithVehicles for first entry line has 5 invoiceLines although there are duplications of and lines with vin/brand/model", 5, entryHeader.AllEntryLines[0].InvoiceLinesWithVehicles.Count());
			AssertEquals("InvoiceLines for second entry line has 2 merged invoiceLines", 2, entryHeader.AllEntryLines[1].InvoiceLines.Count);
			AssertEquals("InvoiceLinesWithVehicles for second entry line has 1 invoiceLine since there is one without vin/brand/model", 1, entryHeader.AllEntryLines[1].InvoiceLinesWithVehicles.Count());
		});
	}

	public void TestGrossWeightInKGForImport()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = declaration.InvoiceLines.AddNew();
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Merge done", true, declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));

			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			invoiceLine.JI_Weight = 200.4455M;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals("Expected filled GrossWeightInKGForImport when weight > 1 rounded to the upper integer unit", 201M, entryLine.GrossWeightInKGForImport);

			invoiceLine.JI_Weight = 0.9886M;
			AssertEquals("Expected filled GrossWeightInKGForImport when weight < 1", 0.989M, entryLine.GrossWeightInKGForImport);
		});
	}

	public override EU.Business.Declaration.JobDeclaration GetJobDeclarationForTest() => declaration;

	public override BaseJobDeclaration SetUpDeclarationForMoneyTest()
	{
		var dec = (JobDeclaration)ImportJobDeclaration;
		dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		return dec;
	}

	protected override EU.Business.Declaration.JobDeclaration SetUpDeclarationForCalculateStatisticalValueTest()
	{
		var declaration = base.SetUpDeclarationForCalculateStatisticalValueTest();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		return declaration;
	}

	protected override void DoMerge(BaseJobDeclaration declaration)
	{
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
	}

	protected override Type GetExpectedTaxBoxSupporterType() => typeof(CusEntryLineFee);

	protected override SupportingDocTestHelper GetSupportingDocTestHelper() => new ESSupportingDocsTestHelper(Factory);

	protected override Type ExpectedTypeOfFees => typeof(EU.Business.Declaration.CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>);

	protected override int ExpectedReadOnlySupportingDocumentsCount => 4;

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
	}
	JobDeclaration declaration;

	EU.Business.Declaration.CusContainer CreateNewContainer(ZString name, JobComInvoiceLine invLine)
	{
		var container = declaration.CusContainers.AddNew();
		container.CO_ContainerNumber = name;
		invLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(container.CO_ContainerNumber).IsForInvoiceLine = true;
		return container;
	}

	void SetPreviousDocumentsRefData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);

		var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection;
		var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection;
		var attributeNameValuePairs = new Dictionary<string, string[]>();

		attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
		helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
			new string[] { importCodeType, exportCodeType }, "1234", "1234", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		Factory.Save();
	}

	CusEntryLine GetEntryLineWithPreviousDocuments()
	{
		SetPreviousDocumentsRefData();
		var declaration = GetJobDeclarationForTest();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		foreach (var prevDoc in GetPreviousDocsForAggregationMergeTest())
		{
			invoiceLine.PreviousDocuments.Add(prevDoc);
		}

		var entryLine = (CusEntryLine)entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		return entryLine;
	}

	List<PreviousDocument> GetPreviousDocsForAggregationMergeTest()
	{
		return new List<PreviousDocument>
			{
				GetPreviousDoc(1, "REF111"),
				GetPreviousDoc(2, "REF222"),
				GetPreviousDoc(2, "REF222"),
				GetPreviousDoc(3, "REF444"),
				GetPreviousDoc(4, "REF555"),
				GetPreviousDoc(5, "REF222"),
				GetPreviousDoc(3, "REF444"),
				GetPreviousDoc(5, "REF222")
			};
	}

	PreviousDocument GetPreviousDoc(int i, ZString refNumber)
	{
		var prevDoc = Factory.New<PreviousDocument>();
		prevDoc.SuspendValidation();

		prevDoc.CSI_Code = "1234";
		prevDoc.CSI_SubType = "Y";
		prevDoc.CSI_ReferenceNumber = refNumber;
		prevDoc.CSI_DateOfIssue = new ZDateTime(2020, 01, 01);
		prevDoc.CSI_LineNo = i;
		prevDoc.CSI_Quantity = i * 10;
		prevDoc.CSI_UnitOfQuantity = "BAG";
		prevDoc.CSI_Status = "QWE";

		return prevDoc;
	}

	CusEntryLine GetEntryLineWithAdditionalInfos()
	{
		var declaration = GetJobDeclarationForTest();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		foreach (var addInf in GetAdditionalInfosForAggregationMergeTest())
		{
			invoiceLine.AdditionalInfos.Add(addInf);
		}

		var entryLine = (CusEntryLine)entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		return entryLine;
	}

	List<AdditionalInfo> GetAdditionalInfosForAggregationMergeTest()
	{
		return new List<AdditionalInfo>
			{
				GetAdditionalInfo("REF111", "TRA"),
				GetAdditionalInfo("REF222", "TRA"),
				GetAdditionalInfo("REF222", "TRA"),
				GetAdditionalInfo("REF444", "INF"),
				GetAdditionalInfo("REF555", "INF"),
				GetAdditionalInfo("REF222", "INF"),
				GetAdditionalInfo("REF444", "INF"),
				GetAdditionalInfo("REF222", "INF")
			};
	}

	AdditionalInfo GetAdditionalInfo(ZString refNumber, ZString kind)
	{
		var addInf = Factory.New<AdditionalInfo>();
		addInf.SuspendValidation();

		addInf.CSI_Code = "1234";
		addInf.CSI_Description = "description";
		addInf.CSI_SubType = kind;
		addInf.CSI_ReferenceNumber = refNumber;
		addInf.CSI_ReferenceNumber2 = refNumber + "Extra";
		addInf.CSI_RX_NKCurrency = "EUR";
		addInf.CSI_Value = 20;
		addInf.CSI_Status = "QWE";

		return addInf;
	}

	class ESSupportingDocsTestHelper : SupportingDocTestHelper
	{
		public ESSupportingDocsTestHelper(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override Type GetSupportingDocumentType() => typeof(SupportingDocument);

		protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocument GetSupportingDoc(int i, ZString refNumber, bool alternateSubType, decimal qty3 = 10.0m, int flag = 0)
		{
			var supDoc = factory.New<SupportingDocument>();
			supDoc.SuspendValidation();

			supDoc.CSI_Code = "1234";
			supDoc.CSI_ReferenceNumber = refNumber;
			supDoc.CSI_SubType = alternateSubType ? "B" : "A"; //Part

			supDoc.CSI_Quantity = i * 10;
			supDoc.CSI_UnitOfQuantity = "BAG";
			supDoc.CSI_Quantity2 = i * 10.1;
			supDoc.CSI_UnitOfQuantity2 = "PKT";
			supDoc.CSI_Value = i * 1000;
			supDoc.CSI_RX_NKCurrency = "GBP";
			supDoc.CSI_DateOfIssue = new ZDateTime(2020, 01, 01);
			supDoc.CSI_DateOfExpiry = new ZDateTime(2020, 12, 31);
			supDoc.CSI_Quantity3 = qty3;

			supDoc.CSI_Description = "Testing"; //reason
			supDoc.CSI_ReferenceNumber2 = "REFNUM2"; //Issueing Authority
			supDoc.CSI_AdditionalDescription = "AddDescr";
			supDoc.CSI_CustomsOffice = "ABC";
			supDoc.CSI_Procedure = "X";
			supDoc.CSI_RN_NKCountryCode = "GB";
			supDoc.CSI_Status = "QWE";
			supDoc.CSI_Tariff = "12345";
			supDoc.CSI_Type = "SUP";
			supDoc.CSI_UnitOfQuantity3 = "U3";
			supDoc.CSI_AdditionalDescription = "AddInfo";
			supDoc.CSI_ItemNumber = 2;

			return supDoc;
		}
	}
}
