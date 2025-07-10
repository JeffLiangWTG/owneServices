using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AUOrgSupplierPartDataLoad))]
	sealed class AUOrgSupplierPartDataLoadTest : DataLoadTestCase<AUOrgSupplierPartDataLoad>
	{
		public void TestOrgSupplierPartDataLoadType()
		{
			OrgSupplierPartDataLoad testLoader = OrgSupplierPartDataLoad.New();
			AssertEquals("OrgSupplierPartDataLoad Type", typeof(AUOrgSupplierPartDataLoad), testLoader.GetType());
		}

		public void TestPartNotCreatedIfLookupInOtherCountry()
		{
			var testLookup = CreateClassification("Test Lookup", "", Classification.ClassificationType.IMP);
			testLookup.CC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			Factory.Save();

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("P1234-4848X,RUBBER GASKET,KG,," + testLookup.CC_LookupCode + ",," + testOrganisation.OH_Code + ",,0");
					sw.Flush();
				}

				var testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, false, false);

				var checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				var enterprisePartsCreated = Factory.Load(typeof(AUOrgSupplierPart), checkFilter);
				AssertEquals("Part should not have been created", 0, enterprisePartsCreated.Length);
			}
		}

		public void TestPartWithMultiplePivotsInVariousCountriesLoadsCorrectPivot()
		{
			var testAuClient = Factory.NewWithValidTestData<OrgHeader>();
			var testNzClient = Factory.NewWithValidTestData<OrgHeader>();
			testAuClient.OH_RL_NKClosestPort = "AUSYD";
			testNzClient.OH_RL_NKClosestPort = "NZAKL";
			var part = Factory.New<AUOrgSupplierPart>();
			part.OP_PartNum = "P1234-4848X";
			var relAu = part.RelatedOrganisations.AddNew();
			relAu.OU_Relationship = "BTH";
			relAu.OU_OH = testAuClient.PK;
			var relNz = part.RelatedOrganisations.AddNew();
			relNz.OU_Relationship = "OWN";
			relNz.OU_OH = testNzClient.PK;
			var pivotAu = part.PivotsForBinding.AddNew();
			var testAuLookup = CreateClassification("Test Lookup AU", "", Classification.ClassificationType.IMP);
			testAuLookup.CC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			pivotAu.CI_CC = testAuLookup.PK;
			pivotAu.CI_OH = relAu.OU_OH;

			var testNzLookup = CreateClassification("Test Lookup NZ", "", Classification.ClassificationType.IMP);
			testNzLookup.CC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var pivotNz = (Customs.Business.BaseCusClassPartPivot)Factory.New<Integration.Customs.NZ.ICusClassPartPivot>();
			pivotNz.CI_RN_NKCountry = Core.Constants.CountryCodes.NewZealand;
			pivotNz.CI_OP = part.PK;
			pivotNz.CI_CC = testNzLookup.PK;
			pivotNz.CI_OH = relNz.OU_OH;

			Factory.Save();

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("P1234-4848X,RUBBER GASKET,KG,," + testAuLookup.CC_LookupCode + "," + testAuClient.OH_Code + ",,,0");
					sw.Flush();
				}

				var testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, true, false);
				AssertContains("Loaded OK; no error message about casting to NZ pivot...", "PART NO: P1234-4848X - Part has been UPDATED", testLoader.Log[1]);
			}
		}

		public void TestLoadUsageCommentAndDescription()
		{
			Classification testLookup = CreateClassification("Test Lookup", "", Classification.ClassificationType.IMP);

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					fileHeader = "Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Division,QtyinStock,ImportTariff,UsageComment,ClassificationDescription";
					sw.WriteLine(fileHeader);
					sw.WriteLine("P1234-4848M,RUBBER GASKET,KG,," + testLookup.CC_LookupCode + ",," + testOrganisation.OH_Code + ",,0,HTI:2121010000,TestUsage,TestDescription");
					sw.Flush();
				}

				OrgSupplierPartDataLoad testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, false, false);
				Factory.Save();

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load(typeof(AUOrgSupplierPart), checkFilter);
				AssertEquals("There should have been 1 part record created", 1, enterprisePartsCreated.Length);

				AUOrgSupplierPart enterprisePart = LoadPart("P1234-4848M");
				ZQuery cusClassPartFilter = new ZQuery(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
				CusClassPartPivot[] cusClassPartLinks = (CusClassPartPivot[])enterprisePart.Factory.Load(typeof(CusClassPartPivot), cusClassPartFilter);
				AssertEquals("TestUsage", cusClassPartLinks[0].CI_UsageComment);
				AssertEquals("TestDescription", cusClassPartLinks[0].CI_Description);
			}
		}

		public void TestLoadCusClassPartPivot()
		{
			Classification testLookup = CreateClassification("Test Lookup", "", Classification.ClassificationType.IMP);

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("P1234-4848X,RUBBER GASKET,KG,," + testLookup.CC_LookupCode + ",," + testOrganisation.OH_Code + ",,0");
					sw.Flush();
				}

				OrgSupplierPartDataLoad testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, false, false);

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load(typeof(AUOrgSupplierPart), checkFilter);
				AssertEquals("There should have been 1 part record created", 1, enterprisePartsCreated.Length);

				AUOrgSupplierPart enterprisePart = LoadPart("P1234-4848X");
				ZQuery cusClassPartFilter = new ZQuery(CusClassPartPivotSchema.CI_CC, testLookup.PK);
				cusClassPartFilter.AddToFilter(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
				var cusClassPartLink = Factory.LoadTop1<CusClassPartPivot>(cusClassPartFilter);
				AssertNotNull("Classification Part Pivot should have been created", cusClassPartLink);
			}
		}

		public void TestUQCanBeObtainedFromTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			var testTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "6112110025", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Having a chest measurement of 86 cm or more", taxOrFeeCode: "GST");
			helper.CreateTariffUOM(testTariff, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NO");
			Factory.Save();

			Classification testLookup = CreateClassification("Test Lookup", "6112.11.00 25", Classification.ClassificationType.IMP);

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("P1234,RUBBER GASKET,,," + testLookup.CC_LookupCode + ",," + testOrganisation.OH_Code + ",,0");
					sw.Flush();
				}

				OrgSupplierPartDataLoad testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, false, false);

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load(typeof(AUOrgSupplierPart), checkFilter);
				AssertEquals("There should have been 1 part record created", 1, enterprisePartsCreated.Length);

				AUOrgSupplierPart enterprisePart = LoadPart("P1234");
				AssertEquals("Stock UQ should have been obtained from tariff", "NO", enterprisePart.OP_StockKeepingUnit);
			}
		}

		public void TestUQCanBeObtainedFromTariff_AUCClass()
		{
			Classification testLookup = CreateClassification("Test Lookup", "6112.11.00 25", Classification.ClassificationType.IMP);

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("P1234,RUBBER GASKET,,," + testLookup.CC_LookupCode + ",," + testOrganisation.OH_Code + ",,0");
					sw.Flush();
				}

				OrgSupplierPartDataLoad testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, false, false);

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load(typeof(AUOrgSupplierPart), checkFilter);
				AssertEquals("There should have been 1 part record created", 1, enterprisePartsCreated.Length);

				AUOrgSupplierPart enterprisePart = LoadPart("P1234");
				AssertEquals("Stock UQ should have been obtained from tariff", "NO", enterprisePart.OP_StockKeepingUnit);
			}
		}

		public void TestSaveAndClearPreviousImportLookupDetailsIfPresentAndDifferent()
		{
			Classification testLookup = CreateClassification("Test Lookup", "6112.11.00 25", Classification.ClassificationType.IMP);

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("P1234,RUBBER GASKET,KG,," + testLookup.CC_LookupCode + ",," + testOrganisation.OH_Code + ",,0");
					sw.Flush();
				}

				OrgSupplierPartDataLoad testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, false, false);

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load(typeof(AUOrgSupplierPart), checkFilter);
				AssertEquals("There should have been 1 part record created", 1, enterprisePartsCreated.Length);

				AUOrgSupplierPart enterprisePart = LoadPart("P1234");
				AssertEquals("Stock UQ", "KG", enterprisePart.OP_StockKeepingUnit);
				AssertEquals("Division should be empty", "", enterprisePart.OP_Division);
				AssertEquals("Stock Qty should be 0", 0M, enterprisePart.OP_QtyInStock);
			}

			Classification changedLookup = CreateClassification("Changed Lookup", "6112.11.00 25", Classification.ClassificationType.IMP);

			using (TempFile updateFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(updateFile.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("P1234,RUBBER O RING,NO,," + changedLookup.CC_LookupCode + ",," + testOrganisation.OH_Code + ",PLUMBING,6000");
					sw.Flush();
				}

				OrgSupplierPartDataLoad testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(updateFile.Filename, true, false);
				Factory.Save();

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load(typeof(AUOrgSupplierPart), checkFilter);
				AssertEquals("There should still only be 1 part record", 1, enterprisePartsCreated.Length);

				AUOrgSupplierPart enterprisePart = LoadPart("P1234");
				enterprisePart.Reload();
				AssertEquals("Part Description", "RUBBER O RING", enterprisePart.OP_Desc);
				AssertEquals("Stock UQ", "NO", enterprisePart.OP_StockKeepingUnit);
				AssertEquals("Division", "PLUMBING", enterprisePart.OP_Division);
				AssertEquals("Stock Qty", 6000M, enterprisePart.OP_QtyInStock);

				ZQuery cusClassPartFilter = new ZQuery(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
				CusClassPartPivot[] cusClassPartLinks = (CusClassPartPivot[])enterprisePart.Factory.Load(typeof(CusClassPartPivot), cusClassPartFilter);
				AssertEquals("Should be 1 pivot record", 1, cusClassPartLinks.Length);
				AssertEquals("Link to this classification", changedLookup.PK, cusClassPartLinks[0].CI_CC);

				Assert(enterprisePart.Notes.HasNotes);
				StmNoteCollection testNotes = (StmNoteCollection)enterprisePart.Notes.GetAllNotes();
				AssertEquals("Data Import Update: Previous Lookup Details", testNotes[0].ST_Description);
				AssertEquals("Previous Import Lookup: Test Lookup" + System.Environment.NewLine, testNotes[0].ST_NoteDataAsText);
			}

			using (TempFile updateFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(updateFile.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("P1234,RUBBER O RING,NO,,,," + testOrganisation.OH_Code + ",PLUMBING,6000");
					sw.Flush();
				}

				OrgSupplierPartDataLoad testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(updateFile.Filename, true, false);
				Factory.Save();

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load(typeof(AUOrgSupplierPart), checkFilter);
				AssertEquals("There should still only be 1 part record", 1, enterprisePartsCreated.Length);

				AUOrgSupplierPart enterprisePart = LoadPart("P1234");
				enterprisePart.Reload();
				AssertEquals("Part Description", "RUBBER O RING", enterprisePart.OP_Desc);
				AssertEquals("Stock UQ", "NO", enterprisePart.OP_StockKeepingUnit);
				AssertEquals("Division", "PLUMBING", enterprisePart.OP_Division);
				AssertEquals("Stock Qty", 6000M, enterprisePart.OP_QtyInStock);

				ZQuery cusClassPartFilter = new ZQuery(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
				CusClassPartPivot[] cusClassPartLinks = (CusClassPartPivot[])enterprisePart.Factory.Load(typeof(CusClassPartPivot), cusClassPartFilter);
				AssertEquals("Should be 1 pivot record", 1, cusClassPartLinks.Length);
				AssertEquals("Link to this classification", changedLookup.PK, cusClassPartLinks[0].CI_CC);

				Assert(enterprisePart.Notes.HasNotes);
				StmNoteCollection testNotes = (StmNoteCollection)enterprisePart.Notes.GetAllNotes();
				AssertEquals("Data Import Update: Previous Lookup Details", testNotes[0].ST_Description);
				AssertEquals("Previous Import Lookup: Test Lookup" + System.Environment.NewLine, testNotes[0].ST_NoteDataAsText);
			}
		}

		public void TestSaveAndClearPreviousExportLookupDetailsIfPresentAndDifferent()
		{
			Classification exportLookup = CreateClassification("Export Lookup", "4901.10.00", Classification.ClassificationType.EXP);

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("LEAFLETS,PAPER BOOKLETS,NO," + exportLookup.CC_LookupCode + ",,," + testOrganisation.OH_Code + ",,0");
					sw.Flush();
				}

				OrgSupplierPartDataLoad testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, false, false);

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load(typeof(AUOrgSupplierPart), checkFilter);
				AssertEquals("There should have been 1 part record created", 1, enterprisePartsCreated.Length);

				AUOrgSupplierPart enterprisePart = LoadPart("LEAFLETS");
				AssertEquals("Stock UQ", "NO", enterprisePart.OP_StockKeepingUnit);
				AssertEquals("Division should be empty", "", enterprisePart.OP_Division);
				AssertEquals("Stock Qty should be 0", 0M, enterprisePart.OP_QtyInStock);
			}

			Classification changedLookup = CreateClassification("Changed Lookup", "4901.10.00", Classification.ClassificationType.EXP);

			using (TempFile updateFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(updateFile.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("LEAFLETS,ONE PAGE BROCHURES,NO," + changedLookup.CC_LookupCode + ",,," + testOrganisation.OH_Code + ",PRINTING,25850");
					sw.Flush();
				}

				OrgSupplierPartDataLoad testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(updateFile.Filename, true, false);
				Factory.Save();

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load(typeof(AUOrgSupplierPart), checkFilter);
				AssertEquals("There should still only be 1 part record", 1, enterprisePartsCreated.Length);

				AUOrgSupplierPart enterprisePart = LoadPart("LEAFLETS");
				enterprisePart.Reload();
				AssertEquals("Part Description", "ONE PAGE BROCHURES", enterprisePart.OP_Desc);
				AssertEquals("Stock UQ", "NO", enterprisePart.OP_StockKeepingUnit);
				AssertEquals("Division", "PRINTING", enterprisePart.OP_Division);
				AssertEquals("Stock Qty", 25850M, enterprisePart.OP_QtyInStock);

				ZQuery cusClassPartFilter = new ZQuery(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
				CusClassPartPivot[] cusClassPartLinks = (CusClassPartPivot[])enterprisePart.Factory.Load(typeof(CusClassPartPivot), cusClassPartFilter);
				AssertEquals("Should be 1 pivot record", 1, cusClassPartLinks.Length);
				AssertEquals("Link to this classification", changedLookup.PK, cusClassPartLinks[0].CI_CC);

				Assert(enterprisePart.Notes.HasNotes);
				StmNoteCollection testNotes = (StmNoteCollection)enterprisePart.Notes.GetAllNotes();
				AssertEquals("Data Import Update: Previous Lookup Details", testNotes[0].ST_Description);
				AssertEquals("Previous Export Lookup: Export Lookup" + System.Environment.NewLine, testNotes[0].ST_NoteDataAsText);
			}

			using (TempFile updateFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(updateFile.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("LEAFLETS,ONE PAGE BROCHURES,NO,,,," + testOrganisation.OH_Code + ",PRINTING,25850");
					sw.Flush();
				}

				OrgSupplierPartDataLoad testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(updateFile.Filename, true, false);
				Factory.Save();

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load(typeof(AUOrgSupplierPart), checkFilter);
				AssertEquals("There should still only be 1 part record", 1, enterprisePartsCreated.Length);

				AUOrgSupplierPart enterprisePart = LoadPart("LEAFLETS");
				enterprisePart.Reload();
				AssertEquals("Part Description", "ONE PAGE BROCHURES", enterprisePart.OP_Desc);
				AssertEquals("Stock UQ", "NO", enterprisePart.OP_StockKeepingUnit);
				AssertEquals("Division", "PRINTING", enterprisePart.OP_Division);
				AssertEquals("Stock Qty", 25850M, enterprisePart.OP_QtyInStock);

				ZQuery cusClassPartFilter = new ZQuery(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
				CusClassPartPivot[] cusClassPartLinks = (CusClassPartPivot[])enterprisePart.Factory.Load(typeof(CusClassPartPivot), cusClassPartFilter);
				AssertEquals("Should be 1 pivot record", 1, cusClassPartLinks.Length);
				AssertEquals("Link to this classification", changedLookup.PK, cusClassPartLinks[0].CI_CC);

				Assert(enterprisePart.Notes.HasNotes);
				StmNoteCollection testNotes = (StmNoteCollection)enterprisePart.Notes.GetAllNotes();
				AssertEquals("Data Import Update: Previous Lookup Details", testNotes[0].ST_Description);
				AssertEquals("Previous Export Lookup: Export Lookup" + System.Environment.NewLine, testNotes[0].ST_NoteDataAsText);
			}
		}

		public void TestUpdateAddInfo()
		{
			Classification testLookup = CreateClassification("Test Lookup", "6112.11.00 25", Classification.ClassificationType.IMP);

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo");
					sw.WriteLine("P1234,RUBBER GASKET,KG,," + testLookup.CC_LookupCode + ",," + testOrganisation.OH_Code + ",1.75,G,,,,6000,NZ,FJ,P50,NZ,AD,555821");
					sw.Flush();
				}

				OrgSupplierPartDataLoad testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, false, false);

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load(typeof(AUOrgSupplierPart), checkFilter);
				AssertEquals("There should have been 1 part record created", 1, enterprisePartsCreated.Length);

				AUOrgSupplierPart enterprisePart = LoadPart("P1234");
				AssertEquals("Stock Qty", 6000M, enterprisePart.OP_QtyInStock);

				ZQuery cusClassPartFilter = new ZQuery(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
				CusClassPartPivot[] cusClassPartLinks = (CusClassPartPivot[])enterprisePart.Factory.Load(typeof(CusClassPartPivot), cusClassPartFilter);
				AssertEquals("Should be 1 pivot record", 1, cusClassPartLinks.Length);
				AssertEquals("Linked to this classification", testLookup.PK, cusClassPartLinks[0].CI_CC);
				AssertEquals("Part AddInfo data should have been populated", "ORG=NZ*POC=FJ*PRI=AD:555821*PRT=P50*PST=NZ", cusClassPartLinks[0].CI_AddInfo);
			}
		}

		public void TestChangeAddInfo()
		{
			Classification testLookup = CreateClassification("Test Lookup", "6112.11.00 25", Classification.ClassificationType.IMP);

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo");
					sw.WriteLine("P1234,RUBBER GASKET,KG,," + testLookup.CC_LookupCode + ",," + testOrganisation.OH_Code + ",1.75,G,,,,6000,NZ,FJ,P50,NZ,AD,555821");
					sw.Flush();
				}

				OrgSupplierPartDataLoad testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, false, false);

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load(typeof(AUOrgSupplierPart), checkFilter);
				AssertEquals("There should have been 1 part record created", 1, enterprisePartsCreated.Length);

				AUOrgSupplierPart enterprisePart = LoadPart("P1234");
				AssertEquals("Stock Qty", 6000M, enterprisePart.OP_QtyInStock);

				ZQuery cusClassPartFilter = new ZQuery(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
				CusClassPartPivot[] cusClassPartLinks = (CusClassPartPivot[])enterprisePart.Factory.Load(typeof(CusClassPartPivot), cusClassPartFilter);
				AssertEquals("Should be 1 pivot record", 1, cusClassPartLinks.Length);
				AssertEquals("Linked to this classification", testLookup.PK, cusClassPartLinks[0].CI_CC);
				AssertEquals("Part AddInfo data should have been populated", "ORG=NZ*POC=FJ*PRI=AD:555821*PRT=P50*PST=NZ", cusClassPartLinks[0].CI_AddInfo);
			}

			using (TempFile updateFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(updateFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo");
					sw.WriteLine("P1234,RUBBER GASKET,KG,," + testLookup.CC_LookupCode + ",," + testOrganisation.OH_Code + ",1.75,G,,,,6000,FJ,FJ,P52,NZ,AD,555827");
					sw.Flush();
				}

				OrgSupplierPartDataLoad testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(updateFile.Filename, true, false);
				Factory.Save();

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load(typeof(AUOrgSupplierPart), checkFilter);
				AssertEquals("There should still only be 1 part record", 1, enterprisePartsCreated.Length);

				AUOrgSupplierPart enterprisePart = LoadPart("P1234");
				enterprisePart.Reload();

				ZQuery cusClassPartFilter = new ZQuery(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
				CusClassPartPivot[] cusClassPartLinks = (CusClassPartPivot[])enterprisePart.Factory.Load(typeof(CusClassPartPivot), cusClassPartFilter);
				AssertEquals("Should still be 1 pivot record", 1, cusClassPartLinks.Length);
				AssertEquals("Should still link to this classification", testLookup.PK, cusClassPartLinks[0].CI_CC);

				Assert(enterprisePart.Notes.HasNotes);
				StmNoteCollection testNotes = (StmNoteCollection)enterprisePart.Notes.GetAllNotes();
				AssertEquals("Data Import Update: Previous AddInfo Details", testNotes[0].ST_Description);
				AssertEquals("Previous Add Info for this part: ORG=NZ*POC=FJ*PRI=AD:555821*PRT=P50*PST=NZ", testNotes[0].ST_NoteDataAsText);
			}
		}

		public void TestChangeLookupAndAddInfo()
		{
			Classification testLookup = CreateClassification("Test Lookup", "6112.11.00 25", Classification.ClassificationType.IMP);

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo");
					sw.WriteLine("P1234,RUBBER GASKET,KG,," + testLookup.CC_LookupCode + ",," + testOrganisation.OH_Code + ",1.75,G,,,,6000,NZ,FJ,P50,NZ,AD,555821");
					sw.Flush();
				}

				OrgSupplierPartDataLoad testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, false, false);

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load(typeof(AUOrgSupplierPart), checkFilter);
				AssertEquals("There should have been 1 part record created", 1, enterprisePartsCreated.Length);

				AUOrgSupplierPart enterprisePart = LoadPart("P1234");
				AssertEquals("Stock Qty", 6000M, enterprisePart.OP_QtyInStock);

				ZQuery cusClassPartFilter = new ZQuery(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
				CusClassPartPivot[] cusClassPartLinks = (CusClassPartPivot[])enterprisePart.Factory.Load(typeof(CusClassPartPivot), cusClassPartFilter);
				AssertEquals("Should be 1 pivot record", 1, cusClassPartLinks.Length);
				AssertEquals("Linked to this classification", testLookup.PK, cusClassPartLinks[0].CI_CC);
				AssertEquals("Part AddInfo data should have been populated", "ORG=NZ*POC=FJ*PRI=AD:555821*PRT=P50*PST=NZ", cusClassPartLinks[0].CI_AddInfo);
			}

			Classification changedLookup = CreateClassification("Changed Lookup", "4901.10.00", Classification.ClassificationType.IMP);
			Factory.Save();

			using (TempFile updateFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(updateFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo");
					sw.WriteLine("P1234,RUBBER GASKET,KG,," + changedLookup.CC_LookupCode + ",," + testOrganisation.OH_Code + ",1.75,G,,,,6000,FJ,FJ,P52,NZ,AD,555827");
					sw.Flush();
				}

				OrgSupplierPartDataLoad testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(updateFile.Filename, true, false);
				Factory.Save();

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load(typeof(AUOrgSupplierPart), checkFilter);
				AssertEquals("There should still only be 1 part record", 1, enterprisePartsCreated.Length);

				AUOrgSupplierPart enterprisePart = LoadPart("P1234");
				enterprisePart.Reload();

				var classPivots = LoadPivots(enterprisePart);
				AssertEquals("There should still be only 1 CusClassPartPivot linked to this part.", 1, classPivots.Length);
				var updatedPivot = (CusClassPartPivot)classPivots[0];
				updatedPivot.Reload();
				AssertEquals("Part AddInfo should now be", "PRT=P52*PRI=AD:555827*POC=FJ*PST=NZ*ORG=FJ", updatedPivot.CI_AddInfo);

				bool hasChangedLookupNote = false;
				Assert(enterprisePart.Notes.HasNotes);
				StmNoteCollection testNotes = (StmNoteCollection)enterprisePart.Notes.GetAllNotes();
				foreach (StmNote note in testNotes)
				{
					if (note.ST_Description == "Data Import Update: Previous Lookup Details")
					{
						AssertEquals("Previous Import Lookup: Test Lookup" + System.Environment.NewLine + "Part Add Info: ORG=NZ*POC=FJ*PRI=AD:555821*PRT=P50*PST=NZ", note.ST_NoteDataAsText);
						hasChangedLookupNote = true;
					}
				}

				Assert(hasChangedLookupNote);
			}
		}

		public void TestUpdateAddInfoPrefData()
		{
			Classification testLookup = CreateClassification("Test Lookup", "6112.11.00 25", Classification.ClassificationType.IMP);

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Unit_NetWeight,Weight_Unit,Unit_Volume,Volume_Unit,Department,Division,QtyInStock,Origin,Last_Cost,UNDG_Code,LocalPartNumber,LocalPartDescription,Use_Attribute1,Use_Attribute2,Use_Attribute3,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage,UC3_QtyParent,UC3_Package,UC3_ParentPackage,UC4_QtyParent,UC4_Package,UC4_ParentPackage,UC5_QtyParent,UC5_Package,UC5_ParentPackage,Commodity,BrandName,Model,ImportTariff,ExportTariff,Weighted_Cost,Cost_Currency,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo");
					sw.WriteLine(@"2613162,""CUP BRUSH, KNOTTED STEEL WIRE, INTERCHANGABLE, MACHINE TOOL"",UNT,,," + testOrganisation.OH_Code + ",,0,,KG,0,M3,,,0,,,,,,,,,,,,,,,,,,,,,,,,,,,9603.50.00 71 ,,,,,505,,TC,942112");
					sw.Flush();
				}

				OrgSupplierPartDataLoad testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, false, false);

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load(typeof(AUOrgSupplierPart), checkFilter);
				AssertEquals("There should have been 1 part record created", 1, enterprisePartsCreated.Length);

				AUOrgSupplierPart enterprisePart = LoadPart("2613162");
				AssertEquals("Part Desc", "CUP BRUSH, KNOTTED STEEL WIRE, INTERCHANGABLE, MACHINE TOOL", enterprisePart.OP_Desc);

				ZQuery cusClassPartPivotFilter = new ZQuery(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
				CusClassPartPivot[] cusClassPartLinks = (CusClassPartPivot[])enterprisePart.Factory.Load(typeof(CusClassPartPivot), cusClassPartPivotFilter);
				AssertEquals("Should be 1 pivot record", 1, cusClassPartLinks.Length);
				AssertEquals("Part AddInfo data should have been populated", "PRI=TC:942112*PRT=505", cusClassPartLinks[0].CI_AddInfo);
			}
		}

		public void TestChangeAddInfoPrefData()
		{
			Classification testLookup = CreateClassification("Test Lookup", "6112.11.00 25", Classification.ClassificationType.IMP);

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Unit_NetWeight,Weight_Unit,Unit_Volume,Volume_Unit,Department,Division,QtyInStock,Origin,Last_Cost,UNDG_Code,LocalPartNumber,LocalPartDescription,Use_Attribute1,Use_Attribute2,Use_Attribute3,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage,UC3_QtyParent,UC3_Package,UC3_ParentPackage,UC4_QtyParent,UC4_Package,UC4_ParentPackage,UC5_QtyParent,UC5_Package,UC5_ParentPackage,Commodity,BrandName,Model,ImportTariff,ExportTariff,Weighted_Cost,Cost_Currency,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo");
					sw.WriteLine(@"2613162,""CUP BRUSH, KNOTTED STEEL WIRE, INTERCHANGABLE, MACHINE TOOL"",UNT,,," + testOrganisation.OH_Code + ",,0,,KG,0,M3,,,0,,,,,,,,,,,,,,,,,,,,,,,,,,,9603.50.00 71 ,,,,,505,,TC,942112");
					sw.Flush();
				}

				OrgSupplierPartDataLoad testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, false, false);

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load(typeof(AUOrgSupplierPart), checkFilter);
				AssertEquals("There should have been 1 part record created", 1, enterprisePartsCreated.Length);

				AUOrgSupplierPart enterprisePart = LoadPart("2613162");
				AssertEquals("Part Desc", "CUP BRUSH, KNOTTED STEEL WIRE, INTERCHANGABLE, MACHINE TOOL", enterprisePart.OP_Desc);

				ZQuery cusClassPartPivotFilter = new ZQuery(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
				CusClassPartPivot[] cusClassPartLinks = (CusClassPartPivot[])enterprisePart.Factory.Load(typeof(CusClassPartPivot), cusClassPartPivotFilter);
				AssertEquals("Should be 1 pivot record", 1, cusClassPartLinks.Length);
				AssertEquals("Part AddInfo data should have been populated in new pivot record", "PRI=TC:942112*PRT=505", cusClassPartLinks[0].CI_AddInfo);
			}

			using (TempFile updateFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(updateFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Unit_NetWeight,Weight_Unit,Unit_Volume,Volume_Unit,Department,Division,QtyInStock,Origin,Last_Cost,UNDG_Code,LocalPartNumber,LocalPartDescription,Use_Attribute1,Use_Attribute2,Use_Attribute3,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage,UC3_QtyParent,UC3_Package,UC3_ParentPackage,UC4_QtyParent,UC4_Package,UC4_ParentPackage,UC5_QtyParent,UC5_Package,UC5_ParentPackage,Commodity,BrandName,Model,ImportTariff,ExportTariff,Weighted_Cost,Cost_Currency,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo");
					sw.WriteLine(@"2613162,""CUP BRUSH, KNOTTED STEEL WIRE, INTERCHANGABLE, MACHINE TOOL"",UNT,,," + testOrganisation.OH_Code + ",,0,,KG,0,M3,,,0,,,,,,,,,,,,,,,,,,,,,,,,,,,9603.50.00 71 ,,,,FJ,503,,TC,917845");
					sw.Flush();
				}

				OrgSupplierPartDataLoad testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(updateFile.Filename, true, false);
				Factory.Save();

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load(typeof(AUOrgSupplierPart), checkFilter);
				AssertEquals("There should still only be 1 part record", 1, enterprisePartsCreated.Length);

				AUOrgSupplierPart enterprisePart = LoadPart("2613162");
				enterprisePart.Reload();

				ZQuery cusClassPartPivotFilter = new ZQuery(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
				CusClassPartPivot[] cusClassPartLinks = (CusClassPartPivot[])enterprisePart.Factory.Load(typeof(CusClassPartPivot), cusClassPartPivotFilter);
				AssertEquals("Should still be only 1 pivot record", 1, cusClassPartLinks.Length);

				var partPivot = cusClassPartLinks[0];
				partPivot.Reload();
				AssertEquals("Pivot should still link to this part record", enterprisePart.PK, partPivot.CI_OP);
				AssertEquals("There is no classification", ZGuid.Empty, partPivot.CI_CC);
				AssertEquals("Part AddInfo data in the pivot record should have been updated with new values", "PRT=503*PRI=TC:917845*POC=FJ", partPivot.CI_AddInfo);

				Assert(enterprisePart.Notes.HasNotes);
				StmNoteCollection testNotes = (StmNoteCollection)enterprisePart.Notes.GetAllNotes();
				AssertEquals("Previous details should have been recorded in note", "Data Import Update: Previous AddInfo Details", testNotes[0].ST_Description);
				AssertEquals("Previous Add Info for this part: PRI=TC:942112*PRT=505", testNotes[0].ST_NoteDataAsText);
			}
		}

		#region TestImportPartsClassTariffs
		public void TestImportedClassLookupEmpty_TariffNotEmpty()
		{
			/*
			 * TEST
			 *	a.	Remove the current functionality that creates a new Lookup code by setting a GUID as the name.
			 *	b.	Update Tariff and set Class. Lookup to ‘’ (if it isn’t already)
			 */
			ClearCustomsRecordsBeforeTesting();

			var loader = OrgSupplierPartDataLoad.New();
			var testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ImportTariff,Supplier");
					sw.WriteLine("TEST,Test_Part_No_Tariff,KG,7301100001," + testOrganisation.OH_Code);
					sw.Flush();
				}

				loader.ImportProductData(testFileName.Filename, false, false);
				var enterprisePart = LoadPart("TEST");
				AssertEquals("Test_Part_No_Tariff", enterprisePart.OP_Desc);
				AssertEquals("KG", enterprisePart.OP_StockKeepingUnit);
				var classPivots = LoadPivots(enterprisePart);
				AssertEquals("CusClassPartPivot should have been created and linked to this part.", 1, classPivots.Length);
				var linkedPivot = (CusClassPartPivot)classPivots[0];
				AssertEquals("Tariff number should have been updated on the new Pivot record", "7301.10.00 01", linkedPivot.CI_TariffNum);
				AssertEquals("Classification should be empty on the Pivot", ZGuid.Empty, linkedPivot.CI_CC);

				var testLookup = CreateClassification("Test Lookup", "6112.11.00 25", Classification.ClassificationType.IMP);
				linkedPivot.CI_CC = testLookup.PK;
				Factory.Save();

				classPivots = LoadPivots(enterprisePart);
				AssertEquals("Pre-condition - still only 1 pivot for this product", 1, classPivots.Length);
				linkedPivot = (CusClassPartPivot)classPivots[0];
				AssertEquals("Tariff number should still be on the new Pivot record", "7301.10.00 01", linkedPivot.CI_TariffNum);
				AssertEquals("Classification has now been manually linked on the Pivot", testLookup.PK, linkedPivot.CI_CC);

				// run another import file again with UpdateParts = true
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ImportTariff,Supplier");
					sw.WriteLine("TEST,Part_Updated,NO,6112.11.00 25," + testOrganisation.OH_Code);
					sw.Flush();
				}

				loader.ImportProductData(testFileName.Filename, true, false);
				Factory.Save();
				enterprisePart.Reload();
				AssertEquals("Product description has been updated", "Part_Updated", enterprisePart.OP_Desc);
				AssertEquals("Product UQ has been updated", "NO", enterprisePart.OP_StockKeepingUnit);

				classPivots = LoadPivots(enterprisePart);
				AssertEquals("There should still be only 1 CusClassPartPivot linked to this part.", 1, classPivots.Length);
				var updatedPivot = (CusClassPartPivot)classPivots[0];
				updatedPivot.Reload();
				AssertEquals("CusClassPivot should just have been updated", linkedPivot.PK, updatedPivot.PK);
				AssertEquals("Tariff number should have been updated on the Pivot", "6112.11.00 25", updatedPivot.CI_TariffNum);
				AssertEquals("Classification lookup should have now been removed from the Pivot", ZGuid.Empty, updatedPivot.CI_CC);
			}
		}

		public void TestLookupNotEmptyAndExists_TariffEmpty()
		{
			/*
			 * TEST
			 *	a.	Update Class. Lookup if it exists and set Tariff to ‘’ (if it isn’t already)
			 */
			ClearCustomsRecordsBeforeTesting();
			var importLookup = CreateClassification("B817F", "7301.10.00 01", Classification.ClassificationType.IMP);
			Factory.Save();

			var loader = OrgSupplierPartDataLoad.New();
			var testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner");
					sw.WriteLine("P1234-4848I,Test_Part_No_Tariff,BX,,B817F," + testOrganisation.OH_Code);
					sw.Flush();
				}

				loader.ImportProductData(testFileName.Filename, false, false);
				var enterprisePart = LoadPart("P1234-4848I");
				AssertEquals("Test_Part_No_Tariff", enterprisePart.OP_Desc);
				AssertEquals("BX", enterprisePart.OP_StockKeepingUnit);

				var classPivots = LoadPivots(enterprisePart);
				AssertEquals("Classification (LookUp) should be linked to this part.", 1, classPivots.Length);
				var linkedPivot = (CusClassPartPivot)classPivots[0];
				AssertEquals("Tariff number should be empty on the Pivot", "", linkedPivot.CI_TariffNum);
				AssertEquals("Classification lookup (importLookup) should have now been linked on the Pivot", importLookup.PK, linkedPivot.CI_CC);
			}
		}

		public void TestLookupNotEmptyAndDoesNotExist_TariffEmpty()
		{
			/*
			 * TEST
			 *	b.	If lookup does not exist fail the part update (Per current functionality)
			 */
			ClearCustomsRecordsBeforeTesting();

			var loader = OrgSupplierPartDataLoad.New();
			var testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner");
					sw.WriteLine("TEST,Dummy_Part,KG,Exp Lookup,," + testOrganisation.OH_Code);
					sw.WriteLine("P1234-4848I,Test Part,BX,,Imp Lookup," + testOrganisation.OH_Code);
					sw.Flush();
				}

				loader.ImportProductData(testFileName.Filename, false, false);
				var enterprisePart = LoadPart("TEST", false);
				AssertNull("Part (TEST) should not be created if data has no tariff and lookup does not exist", enterprisePart);

				loader.ImportProductData(testFileName.Filename, false, false);
				enterprisePart = LoadPart("P1234-4848I", false);
				AssertNull("Part (P1234-4848I) should not be created if data has no tariff and lookup does not exist", enterprisePart);

				var exportLookup = CreateClassification("Exp Lookup", "8529.90.11.01H", Classification.ClassificationType.EXP);
				var importLookup = CreateClassification("Imp Lookup", "7301.10.00 01", Classification.ClassificationType.IMP);
				Factory.Save();

				loader.ImportProductData(testFileName.Filename, false, false);
				enterprisePart = LoadPart("TEST");
				AssertEquals("Dummy_Part", enterprisePart.OP_Desc);
				AssertEquals("KG", enterprisePart.OP_StockKeepingUnit);
				var classPivots = LoadPivots(enterprisePart);
				AssertEquals("Classification lookup exists and should have been linked to this part.", 1, classPivots.Length);
				var linkedPivot = (CusClassPartPivot)classPivots[0];
				AssertEquals("Tariff number should be empty on the Pivot", "", linkedPivot.CI_TariffNum);
				AssertEquals("Classification lookup should have now been linked on the Pivot", exportLookup.PK, linkedPivot.CI_CC);

				enterprisePart = LoadPart("P1234-4848I");
				AssertEquals("Test Part", enterprisePart.OP_Desc);
				AssertEquals("BX", enterprisePart.OP_StockKeepingUnit);
				classPivots = LoadPivots(enterprisePart);
				AssertEquals("Classification lookup now exists, therefore the Part should have been created and lookup linked also.", 1, classPivots.Length);
				linkedPivot = (CusClassPartPivot)classPivots[0];
				AssertEquals("Tariff number should be empty on the Pivot", "", linkedPivot.CI_TariffNum);
				AssertEquals("Classification lookup (importLookup) should have now been linked on the Pivot", importLookup.PK, linkedPivot.CI_CC);
			}
		}

		public void TestLookupNotEmptyAndExistsAndTariffNotEmpty()
		{
			/*
			 * TEST
			 *	a.	Update Class. Lookup if it exists and set Tariff to ‘’ (if it isn’t already)
			 */
			ClearCustomsRecordsBeforeTesting();
			var importLookup = CreateClassification("B817F", "7301.10.00 01", Classification.ClassificationType.IMP);
			Factory.Save();

			var loader = OrgSupplierPartDataLoad.New();
			var testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ImportTariff,ExportClassification,ImportClassification,Owner");
					sw.WriteLine("P1234-4848I,Part with Tariff and valid Lookup,KG,7301.10.00 01,,B817F," + testOrganisation.OH_Code);
					sw.Flush();
				}

				loader.ImportProductData(testFileName.Filename, false, false);
				var enterprisePart = LoadPart("P1234-4848I");
				AssertEquals("Part with Tariff and valid Lookup", enterprisePart.OP_Desc);
				AssertEquals("KG", enterprisePart.OP_StockKeepingUnit);

				var classPivots = LoadPivots(enterprisePart);
				AssertEquals("There should be only 1 CusClassPartPivot linked to this part.", 1, classPivots.Length);
				var linkedPivot = (CusClassPartPivot)classPivots[0];
				linkedPivot.Reload();
				AssertEquals("Tariff number should have been updated on the Pivot", "7301.10.00 01", linkedPivot.CI_TariffNum);
				AssertEquals("Classification lookup should not have been added to the Pivot", ZGuid.Empty, linkedPivot.CI_CC);
			}
		}

		public void TestValidLookupWithDifferentTariffToExistingTariff()
		{
			ClearCustomsRecordsBeforeTesting();

			var testAuClient = Factory.NewWithValidTestData<OrgHeader>();
			testAuClient.OH_RL_NKClosestPort = "AUSYD";
			var part = Factory.New<AUOrgSupplierPart>();
			part.OP_PartNum = "P1234-4848X";
			var relAu = part.RelatedOrganisations.AddNew();
			relAu.OU_Relationship = "BTH";
			relAu.OU_OH = testAuClient.PK;
			var pivotAu = part.PivotsForBinding.AddNew();
			pivotAu.CI_OP = part.PK;
			pivotAu.CI_OH = relAu.OU_OH;
			pivotAu.CI_TariffNum = "2401.10.00 26";

			var importLookup = CreateClassification("B817F", "7301.10.00 01", Classification.ClassificationType.IMP);
			Factory.Save();

			var loader = OrgSupplierPartDataLoad.New();
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner");
					sw.WriteLine("P1234-4848X,Test_Part_ChangedLookupTariff,BX,,B817F," + testAuClient.OH_Code);
					sw.Flush();
				}

				loader.ImportProductData(testFileName.Filename, true, false);
				Factory.Save();
				part.Reload();
				AssertEquals("Test_Part_ChangedLookupTariff", part.OP_Desc);
				AssertEquals("BX", part.OP_StockKeepingUnit);

				var classPivots = LoadPivots(part);
				AssertEquals("Classification (LookUp) should be linked to this part.", 1, classPivots.Length);
				var linkedPivot = (CusClassPartPivot)classPivots[0];
				linkedPivot.Reload();
				AssertEquals("The old Tariff number should have been cleared out on the Pivot", "", linkedPivot.CI_TariffNum);
				AssertEquals("Classification lookup (importLookup) linked to changed tariff number should have now been linked on the Pivot", importLookup.PK, linkedPivot.CI_CC);
			}
		}

		BusinessObject[] LoadPivots(OrgSupplierPart lookupPart)
		{
			var pivotsQuery = new ZQuery(CusClassPartPivotSchema.CI_OP, lookupPart.PK);
			BusinessObject[] pivots = (BusinessObject[])Factory.Load<Integration.Customs.IBaseCusClassPartPivot>(pivotsQuery);
			return pivots;
		}

		#endregion

		#region Implementation

		ZString fileHeader;
		OrgHeader testOrganisation;

		#region CreateClassification

		Classification CreateClassification(ZString lookupCode, ZString tariff, ZString type)
		{
			Classification testLookup = Factory.NewWithValidTestData<Classification>();
			testLookup.CC_LookupCode = lookupCode;
			if (!tariff.IsEmpty)
			{
				testLookup.CC_TariffNum = tariff;
			}

			testLookup.CC_ClassificationType = type;
			Factory.Save();

			return testLookup;
		}

		#endregion

		void ClearCustomsRecordsBeforeTesting()
		{
			TestCaseHelper.ClearTable("JobComInvoiceLine");
			TestCaseHelper.ClearTable("CusClassPartPivot");
			TestCaseHelper.ClearTable("CusClassification");
			TestCaseHelper.ClearTable("OrgPartRelation");
			TestCaseHelper.ClearTable("OrgPartUnit");
			TestCaseHelper.ClearTable("OrgSupplierPart");
		}

		protected override void SetUp()
		{
			base.SetUp();
			ClearCustomsRecordsBeforeTesting();
			fileHeader = "Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Division,QtyinStock";
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
		}

		protected override AUOrgSupplierPartDataLoad GetNewDataLoader()
		{
			return new AUOrgSupplierPartDataLoad();
		}

		AUOrgSupplierPart LoadPart(ZString lookupPart, bool failIfNotExists = true)
		{
			ZQuery partFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, lookupPart);
			var enterprisePart = Factory.LoadTop1<AUOrgSupplierPart>(partFilter);
			if (failIfNotExists)
			{
				AssertNotNull("Expecting Part " + lookupPart + " to be found", enterprisePart);
			}

			return enterprisePart;
		}

		#endregion
	}
}
