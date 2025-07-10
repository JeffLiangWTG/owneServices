using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.DataConverters.CustomsFiles;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DataConverters.Testing.CustomsFiles.AU
{
	class ConverterTest : TestCaseWithFactory
	{
		[ExpectException(typeof(FileNotFoundException))]
		public void TestValidationFile()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			var testConverter = new Converter("bad data", OleDBConnectionTypes.Excel, true, true, false);
			testConverter.ImportData();
		}

		[ExpectException(typeof(ImportValidationException))]
		public void TestValidationContent()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					// Add some text to the file.
					sw.Write("This is the ");
					sw.WriteLine("header for the file.");
					sw.WriteLine("-------------------");
					sw.Flush();
				}

				var testConverter = new Converter(testFileName.Filename, OleDBConnectionTypes.Excel, true, true, false);
				testConverter.ImportData();
			}
		}

		public void TestImportExcelProducts()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			ClearCustomsRecordsBeforeTesting();

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					// Add some text to the file. This test file will have the header line + 3 lines of data, 2 that duplicate a part when buyer/supplier organisations can not be found
					sw.WriteLine("PartNo,Part Description,Stock Unit,Exp Tariff Code,Imp Tariff Code,Owner,Supplier,Division,Qty in Stock");
					sw.WriteLine("1,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",KG,,,KNO001  ,FOR     ,,0");
					sw.WriteLine("1,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",NO,,,KNO001  ,FREU    ,,0");
					sw.WriteLine("912.226,\"TAPS,COCKS,VALVES AND SIMILAR APPLIANCES FOR PIPES BOILER SHELLS,TANKS,VATS OR LIKE,INCL PRESSURE RE- DUCING VALVES & THERMOSTATICALLY CONTROLLED VALVES\",NO,,,KNO001  ,GRA WHI ,,0");
					sw.Flush();
				}

				var testConverter = new Converter(testFileName.Filename, OleDBConnectionTypes.Excel, false, true, false);
				testConverter.ImportData();

				var checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				var enterprisePartsCreated = Factory.Load(typeof(AUOrgSupplierPart), checkFilter);
				AssertEquals("There should have been 2 part records created", 2, enterprisePartsCreated.Length);

				var enterprisePart = LoadPart("912.226");
				AssertEquals("TAPS,COCKS,VALVES AND SIMILAR APPLIANCES FOR PIPES BOILER SHELLS,TANKS,VATS OR L", enterprisePart.OP_Desc);
				AssertEquals("NO", enterprisePart.OP_StockKeepingUnit);
				Assert(enterprisePart.Notes.HasNotes);
				var testNotes = (StmNoteCollection)enterprisePart.Notes.GetAllNotes();
				AssertEquals("Full Product Description", testNotes[0].ST_Description);
				AssertEquals("TAPS,COCKS,VALVES AND SIMILAR APPLIANCES FOR PIPES BOILER SHELLS,TANKS,VATS OR LIKE,INCL PRESSURE RE- DUCING VALVES & THERMOSTATICALLY CONTROLLED VALVES", testNotes[0].ST_NoteDataAsText);
			}
		}

		public void TestUQCanBeObtainedFromTariff()
		{
			var testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var testImpClass = Factory.NewWithValidTestData<BaseCusClassification>();
			testImpClass.CC_LookupCode = "TestLookup";
			testImpClass.CC_ClassificationType = "IMP";
			testImpClass.CC_TariffNum = "8540.81.00 10";

			Factory.Save();

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("P1234,Bandaids,,," + testImpClass.CC_LookupCode + "," + testOrganisation.OH_Code + ",,,");
					sw.Flush();
				}

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				var partConverter = new Converter(testFileName.Filename, OleDBConnectionTypes.Excel, false, true, false);
				partConverter.ImportData();

				var importPart = LoadPart("P1234");
				Assert("Stock unit should have been obtained from tariff", importPart.OP_StockKeepingUnit == "NO");
			}
		}

		public void TestSamePartCreatedForDifferentBuyer()
		{
			// re-run the same file but after setting up buyer legacy codes - it should now create two part 1 records - one for each buyer
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			ClearCustomsRecordsBeforeTesting();

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("1,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",KG,,,KNO001  ,FOR     ,,0");
					sw.WriteLine("1,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",NO,,,KNO002  ,FREU    ,,0");
					sw.WriteLine("912.226,\"TAPS,COCKS,VALVES AND SIMILAR APPLIANCES FOR PIPES BOILER SHELLS,TANKS,VATS OR LIKE,INCL PRESSURE RE- DUCING VALVES & THERMOSTATICALLY CONTROLLED VALVES\",NO,,,KNO001  ,GRA WHI ,,0");
					sw.Flush();
				}

				var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
				var testOrgLegacyCode1 = testOrg1.CustomsCodes.AddNew();
				testOrgLegacyCode1.OK_CodeType = OrgCusCode.CodeTypes.LegacySystemCode;
				testOrgLegacyCode1.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				testOrgLegacyCode1.OK_CustomsRegNo = "KNO001";

				var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
				var testOrgLegacyCode2 = testOrg2.CustomsCodes.AddNew();
				testOrgLegacyCode2.OK_CodeType = OrgCusCode.CodeTypes.LegacySystemCode;
				testOrgLegacyCode2.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				testOrgLegacyCode2.OK_CustomsRegNo = "KNO002";
				Factory.Save();

				var testConverter = new Converter(testFileName.Filename, OleDBConnectionTypes.Excel, false, true, false);
				testConverter.ImportData();

				var checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				var enterprisePartsCreated = Factory.Load(typeof(AUOrgSupplierPart), checkFilter);
				AssertEquals("There should have been 3 part records created", 3, enterprisePartsCreated.Length);
			}
		}

		public void TestMultipleSuppliersForSamePart()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			ClearCustomsRecordsBeforeTesting();

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("1,OTHER PARTS OF VULCANISED RUBBER,KG,,,KNO001  ,SUPPLIER1     ,,0");
					sw.WriteLine("1,OTHER PARTS OF VULCANISED RUBBER,NO,,,KNO001  ,SUPPLIER2    ,,0");
					sw.WriteLine("1,OTHER PARTS OF VULCANISED RUBBER,NO,,,KNO001  ,SUPPLIER3    ,,0");
					sw.Flush();
				}

				var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
				var testOrgLegacyCode1 = testOrg1.CustomsCodes.AddNew();
				testOrgLegacyCode1.OK_CodeType = OrgCusCode.CodeTypes.LegacySystemCode;
				testOrgLegacyCode1.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				testOrgLegacyCode1.OK_CustomsRegNo = "SUPPLIER1";

				var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
				var testOrgLegacyCode2 = testOrg2.CustomsCodes.AddNew();
				testOrgLegacyCode2.OK_CodeType = OrgCusCode.CodeTypes.LegacySystemCode;
				testOrgLegacyCode2.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				testOrgLegacyCode2.OK_CustomsRegNo = "SUPPLIER2";

				var testOrg3 = Factory.NewWithValidTestData<OrgHeader>();
				var testOrgLegacyCode3 = testOrg3.CustomsCodes.AddNew();
				testOrgLegacyCode3.OK_CodeType = OrgCusCode.CodeTypes.LegacySystemCode;
				testOrgLegacyCode3.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				testOrgLegacyCode3.OK_CustomsRegNo = "SUPPLIER3";
				Factory.Save();

				var testConverter = new Converter(testFileName.Filename, OleDBConnectionTypes.Excel, false, true, false);
				testConverter.ImportData();

				var checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				var enterprisePartsCreated = Factory.Load(typeof(AUOrgSupplierPart), checkFilter);
				AssertEquals("There should have been 3 part records with 3 supplierpart relations", 3, enterprisePartsCreated.Length);

				var enterprisePart = LoadPart("1");
				var suppliersFilter = new ZQuery(OrgPartRelationSchema.OU_OP, SQLComparisonOperator.Equal, enterprisePart.PK);
				suppliersFilter.AddToFilter(OrgPartRelationSchema.OU_Relationship, "SUP");
				var suppliersLinked = Factory.Load(typeof(OrgPartRelation), suppliersFilter);
				AssertEquals("Should have been only 1 supplier linked to this part:", 1, suppliersLinked.Length);
			}
		}

		public void TestImportExcelLookups()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			ClearCustomsRecordsBeforeTesting();

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Lookup Code,Type,Lookup Description,Tariff,Treatment,Instrument,Concession/By Law");
					sw.WriteLine("Cars,IMP,Auto - commercial 1 tonne,8703.24.20 84,505,TC1,9611358");
					sw.WriteLine("Motor Vehicle,IMP,Auto - commercial 1 tonne,8703.24.20,505,TC1,9611358");
					sw.WriteLine("Utility,IMP,Auto - commercial 1 tonne,87032420,505,TC1,9611358");
					sw.WriteLine("Beer,EXP,Bottled beer made from malt,2203.00.10,,,");
					sw.Flush();
				}

				var testConverter = new Converter(testFileName.Filename, OleDBConnectionTypes.Excel, true, true, false);
				testConverter.ImportData();

				var checkFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, SQLComparisonOperator.GreaterThan, "");
				var enterpriseLookupsCreated = Factory.Load(typeof(Classification), checkFilter);
				AssertEquals("There should have been 4 classification lookups created", 4, enterpriseLookupsCreated.Length);

				var enterpriseClass = LoadClassification("Cars", "IMP");
				AssertEquals("Auto - commercial 1 tonne", enterpriseClass.CC_Description);
				AssertEquals("8703.24.20 84", enterpriseClass.CC_TariffNum);
				AssertEquals("IMP", enterpriseClass.CC_ClassificationType);

				AssertEquals("Classification lookup Add info contains Instrument code", true, enterpriseClass.CC_AddInfo.Contains("InstrumentCode_Hidden=9611358"));
				AssertEquals("Classification lookup Add info contains Instrument type", true, enterpriseClass.CC_AddInfo.Contains("InstrumentType_Hidden=TC1"));
				AssertEquals("Classification lookup Add info contains treatment code", true, enterpriseClass.CC_AddInfo.Contains("TreatmentCode_Hidden=505"));

				enterpriseClass = LoadClassification("Beer", "EXP");
				AssertEquals("Bottled beer made from malt", enterpriseClass.CC_Description);
				AssertEquals("2203.00.10", enterpriseClass.CC_TariffNum);
				AssertEquals("EXP", enterpriseClass.CC_ClassificationType);
				Assert(enterpriseClass.CC_AddInfo.IsEmpty);
			}
		}

		public void TestGetOrgPKFromLegacyCode()
		{
			var testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var testOrgLegacyCode = testOrganisation.CustomsCodes.AddNew();
			testOrgLegacyCode.OK_CodeType = OrgCusCode.CodeTypes.LegacySystemCode;
			testOrgLegacyCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			testOrgLegacyCode.OK_CustomsRegNo = "LegacyCodeTest";
			Factory.Save();

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.Write("P1234-4848X,Test Part,KG,,,LegacyCodeTest,TESTSYD,Major Accounts,175850");
					sw.Flush();
				}

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				var partConverter = new Converter(testFileName.Filename, OleDBConnectionTypes.Excel, false, true, false);
				partConverter.ImportData();

				var enterprisePart = LoadPart("P1234-4848X");
				AssertEquals("Test Part", enterprisePart.OP_Desc);
				Assert(enterprisePart.RelatedOrganisations.Count > 0);
				AssertEquals("Legacy Code found", testOrganisation.PK, enterprisePart.RelatedOrganisations[0].OU_OH);
			}
		}

		public void TestGetOrgPKFromRawOrgCode()
		{
			var testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.Write("P1234-4848X,Test Part,KG,,," + testOrganisation.OH_Code + ",TESTSYD,Major Accounts,175850");
					sw.Flush();
				}

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				var partConverter = new Converter(testFileName.Filename, OleDBConnectionTypes.Excel, false, true, false);
				partConverter.ImportData();

				var enterprisePart = LoadPart("P1234-4848X");
				AssertEquals("Test Part", enterprisePart.OP_Desc);
				Assert(enterprisePart.RelatedOrganisations.Count > 0);
				AssertEquals("Organisation found", testOrganisation.PK, enterprisePart.RelatedOrganisations[0].OU_OH);
			}
		}

		#region Implementation

		protected void ClearCustomsRecordsBeforeTesting()
		{
			TestCaseHelper.ClearTable("CusClassPartPivot");
			TestCaseHelper.ClearTable("CusClassification");
			TestCaseHelper.ClearTable("OrgPartUnit");
			TestCaseHelper.ClearTable("OrgPartRelation");
			TestCaseHelper.ClearTable("OrgSupplierPart");
		}

		protected Classification LoadClassification(ZString lookupCode, ZString classType)
		{
			var classFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, lookupCode);
			classFilter.AddToFilter(CusClassificationSchema.CC_ClassificationType, classType);
			classFilter.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var enterpriseClass = Factory.LoadTop1<Classification>(classFilter);
			AssertNotNull(enterpriseClass);
			return enterpriseClass;
		}

		protected AUOrgSupplierPart LoadPart(ZString lookupPart)
		{
			var partFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, lookupPart);
			var enterprisePart = Factory.LoadTop1<AUOrgSupplierPart>(partFilter);
			AssertNotNull("Expecting Part " + lookupPart + " to be found", enterprisePart);
			return enterprisePart;
		}

		#endregion
	}
}
