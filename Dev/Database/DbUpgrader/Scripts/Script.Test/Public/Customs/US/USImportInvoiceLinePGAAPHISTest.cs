using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(USImportInvoiceLinePGAAPHIS))]
	class USImportInvoiceLinePGAAPHISTest : DbCreateScriptTest
	{
		public void TestUSImportInvoiceLinePGAAPHIS()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var declarationPK1 = Guid.NewGuid();
			var importerPK = TestDataCreator.CreateOrganisation("oh1", "OrgH1");
			var declarationSql = @"
				INSERT INTO dbo.JobDeclaration (JE_OH_Importer, JE_DataModel, JE_PK, JE_MessageType, JE_GB, JE_GC, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, JE_AddInfo, JE_ClusterKey) VALUES (@importerPK, 'US', @declarationPK1, 'IMP', @branchPK, @companyPK, 'JOB1', 'ACS', 0, 'PGAExpeditedRelease=Y', 1);";

			using (var command = Db.Connection.Command(declarationSql))
			{
				command.AddParameter("@declarationPK1", SqlDbType.UniqueIdentifier, declarationPK1);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				command.ExecuteNonQuery();
			}

			var org1PK = TestDataCreator.CreateOrganisation("ORG1", "Org1 AAA");
			var address1PK = TestDataCreator.CreateAddress(org1PK, "AAA", "Address 1");
			var org2PK = TestDataCreator.CreateOrganisation("ORG2", "Org2 BBB");
			var address2PK = TestDataCreator.CreateAddress(org2PK, "BBB", "Address 2");
			var org3PK = TestDataCreator.CreateOrganisation("ORG3", "Org3 CCC");
			var address3PK = TestDataCreator.CreateAddress(org3PK, "CCC", "Address 3");
			var org4PK = TestDataCreator.CreateOrganisation("ORG4", "Org4 DDD");
			var address4PK = TestDataCreator.CreateAddress(org4PK, "DDD", "Address 4");
			var org5PK = TestDataCreator.CreateOrganisation("ORG5", "Org5 EEE");
			var address5PK = TestDataCreator.CreateAddress(org5PK, "EEE", "Address 5");
			var org6PK = TestDataCreator.CreateOrganisation("ORG6", "Org6 FFF");
			var address6PK = TestDataCreator.CreateAddress(org6PK, "FFF", "Address 6");

			var invoiceHeaderPK1 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK1, false, 1, dataModel: "US");
			var invoiceLinePK1 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderPK1, 1, dataModel: "US");

			var cusLineTariffDetailSql = @"
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99038823', 'AT1', 'SupDuty=200', 10, 110, 'US');
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99038805', 'AT2', 'SupDuty=150', 20, 120, 'US')
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99030121', 'AT3', '', 0, 0, 'US')
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99030122', 'AT4', '', 0, 0, 'US')
				INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_ParentID, BZ_ParentTableCode, BZ_Tariff, BZ_Type, BZ_NAddInfo, BZ_Qty1, BZ_Value, BZ_DataModel) VALUES(NEWID(), @invoiceLinePK1, 'JI', '99030123', 'AT5', '', 0, 0, 'US');";
			using (var command = Db.Connection.Command(cusLineTariffDetailSql))
			{
				command.AddParameter("@invoiceHeaderPK", SqlDbType.UniqueIdentifier, invoiceHeaderPK1);
				command.AddParameter("@invoiceLinePK1", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.ExecuteNonQuery();
			}

			var ap0100APHISPK = TestDataCreator.CreateCusAddInfo("APH", $"ProgramType=AAC*ProcessingCode=A01*IsDocSubmitted=Y*IndentedUseCode=1*IntendedUseDescription=2*CategoryType=AP0100*CategoryCode=3*ReMarks=4*VehicleNumber=5*VehicleLength=6*Qty1=7*UQ1=8*Qty2=9*UQ2=10*Qty3=11*UQ3=12*StockKeepingUnitNumber=13*ProductType=14*ProductNumber=15*ProductPhysicalState=17*ProductCondition=18*ProductStatus=19*BouquetGroupingNumber=20*ProductComponent=21*GrowingMedia=22*CommoditySpecificName=23*ScientificGenusName=24*ScientificSpeciesName=25*ScientificSubSpeciesName=26*OA_ApplicantAddress={address1PK}*OA_CropGrowerAddress={address2PK}*OA_USDAAPHISGrowerAddress={address3PK}*OA_PermittedAddress={address4PK}*OA_ShipperAddress={address5PK}", "JI", invoiceLinePK1);
			var product1PK = TestDataCreator.CreateCusAddInfo(new Guid("C3E25996-CB49-42B7-BEE3-0001CA0AE11B"), "APP", "Age=1*AgeRangeDesc=2*BreedVariety=3*Color=4*CountryCode=5*Gender=6*GeneralName=7*Genus=8*GeographicLocation=9*GestationalAgeIfPregnant=10*IsProtectedSpecies=Y*Origin=11*ProcessingDescription=12*ProcessingEndDate=2021-07-01*ProcessingStartDate=2021-07-02*ProcessingTypeCode=13*ShowBreed=14*SourceTypeCode=15*Species=16*SpecificName=17*Type=18*Variety=19", "B7", ap0100APHISPK);
			var product2PK = TestDataCreator.CreateCusAddInfo(new Guid("6A3FBE12-E6D4-49EF-AA97-00028A6E845F"), "APP", "Age=21*AgeRangeDesc=22*BreedVariety=23*Color=24*CountryCode=25*Gender=26*GeneralName=27*Genus=28*GeographicLocation=29*GestationalAgeIfPregnant=30*IsProtectedSpecies=Y*Origin=31*ProcessingDescription=32*ProcessingEndDate=2021-07-03*ProcessingStartDate=2021-07-04*ProcessingTypeCode=33*ShowBreed=34*SourceTypeCode=35*Species=36*SpecificName=37*Type=38*Variety=39", "B7", ap0100APHISPK);
			var product3PK = TestDataCreator.CreateCusAddInfo(new Guid("B032BAC9-49EE-4955-A7B3-000423BDEDA2"), "APP", "Age=41*AgeRangeDesc=42*BreedVariety=43*Color=44*CountryCode=45*Gender=46*GeneralName=47*Genus=48*GeographicLocation=49*GestationalAgeIfPregnant=50*IsProtectedSpecies=Y*Origin=51*ProcessingDescription=52*ProcessingEndDate=2021-07-05*ProcessingStartDate=2021-07-06*ProcessingTypeCode=53*ShowBreed=54*SourceTypeCode=55*Species=56*SpecificName=57*Type=58*Variety=59", "B7", ap0100APHISPK);
			var source1PK = TestDataCreator.CreateCusAddInfo(new Guid("7E1A730E-1662-4192-988D-000505052C33"), "APS", "CountryCode=1*GeographicLocation=2*ProcessingDescription=3*ProcessingEndDate=2021-07-11*ProcessingStartDate=2021-07-10*ProcessingTypeCode=4*SourceTypeCode=5", "B7", ap0100APHISPK);
			var source2PK = TestDataCreator.CreateCusAddInfo(new Guid("DE9D962E-3DB4-4124-83A1-0005284E530B"), "APS", "CountryCode=11*GeographicLocation=12*ProcessingDescription=13*ProcessingEndDate=2021-07-13*ProcessingStartDate=2021-07-12*ProcessingTypeCode=14*SourceTypeCode=15", "B7", ap0100APHISPK);
			var source3PK = TestDataCreator.CreateCusAddInfo(new Guid("07664214-9210-4308-A6C6-0005CEEE4D17"), "APS", "CountryCode=21*GeographicLocation=22*ProcessingDescription=23*ProcessingEndDate=2021-07-15*ProcessingStartDate=2021-07-14*ProcessingTypeCode=24*SourceTypeCode=25", "B7", ap0100APHISPK);
			var license1PK = TestDataCreator.CreateCusAddInfo(new Guid("EAC750A7-F3F0-430D-AFA2-00074BFAF8C3"), "APL", "Date=2021-07-20*DateQualifier=1*Number=2*Quantity=3*RN_CountryCode=4*StateDescription=5*Type=6*UnitOfMeasure=7", "B7", ap0100APHISPK);
			var license2PK = TestDataCreator.CreateCusAddInfo(new Guid("800FB302-5C1A-4722-9D48-0009818E5E4D"), "APL", "Date=2021-07-21*DateQualifier=11*Number=12*Quantity=13*RN_CountryCode=14*StateDescription=15*Type=16*UnitOfMeasure=17", "B7", ap0100APHISPK);
			var license3PK = TestDataCreator.CreateCusAddInfo(new Guid("4AFCEFD8-D935-4EEB-B112-00098DCE69D5"), "APL", "Date=2021-07-22*DateQualifier=21*Number=22*Quantity=23*RN_CountryCode=24*StateDescription=25*Type=26*UnitOfMeasure=27", "B7", ap0100APHISPK);
			var inspection1PK = TestDataCreator.CreateCusAddInfo(new Guid("9D248DE4-DFA2-4134-9FDA-0009C6BADFBC"), "APN", "Date=2021-07-25*Location=1*TestingStatus=2", "B7", ap0100APHISPK);
			var inspection2PK = TestDataCreator.CreateCusAddInfo(new Guid("E49C5346-3873-40EF-A62B-000A65B1CA57"), "APN", "Date=2021-07-26*Location=11*TestingStatus=12", "B7", ap0100APHISPK);
			var inspection3PK = TestDataCreator.CreateCusAddInfo(new Guid("2AF5C6DE-5B8F-45D8-A863-000B0DA88C32"), "APN", "Date=2021-07-27*Location=21*TestingStatus=22", "B7", ap0100APHISPK);
			var routing1PK = TestDataCreator.CreateCusAddInfo(new Guid("CBC3F2C1-27BF-4A4A-BD66-000C29FA3FB9"), "APR", "Country=1*State=2*Type=3", "B7", ap0100APHISPK);
			var routing2PK = TestDataCreator.CreateCusAddInfo(new Guid("7CE70BF2-4B6A-46E6-88DA-000E053790EA"), "APR", "Country=11*State=12*Type=13", "B7", ap0100APHISPK);
			var routing3PK = TestDataCreator.CreateCusAddInfo(new Guid("9765F919-552F-4DD1-88C9-000E0DCDB857"), "APR", "Country=21*State=22*Type=23", "B7", ap0100APHISPK);

			var ap0300APHISPK = TestDataCreator.CreateCusAddInfo("APH", $"ProgramType=AVS*ProcessingCode=A04*IsDocSubmitted=Y*IndentedUseCode=1*IntendedUseDescription=2*CategoryType=AP0300*CategoryCode=3*ReMarks=4*VehicleNumber=5*VehicleLength=6*Qty1=7*UQ1=8*Qty2=9*UQ2=10*Qty3=11*UQ3=12*StockKeepingUnitNumber=13*ProductType=14*ProductNumber=15*ProductPhysicalState=17*ProductCondition=18*ProductStatus=19*BouquetGroupingNumber=20*ProductComponent=21*GrowingMedia=22*CommoditySpecificName=23*ScientificGenusName=24*ScientificSpeciesName=25*ScientificSubSpeciesName=26*OA_ApplicantAddress={address1PK}*OA_CropGrowerAddress={address2PK}*OA_USDAAPHISGrowerAddress={address3PK}*OA_PermittedAddress={address4PK}*OA_ShipperAddress={address5PK}", "JI", invoiceLinePK1);
			product1PK = TestDataCreator.CreateCusAddInfo(new Guid("4363E162-1168-4F92-8B32-00118CCB65E5"), "APP", "Age=1*AgeRangeDesc=2*BreedVariety=3*Color=4*CountryCode=5*Gender=6*GeneralName=7*Genus=8*GeographicLocation=9*GestationalAgeIfPregnant=10*IsProtectedSpecies=Y*Origin=11*ProcessingDescription=12*ProcessingEndDate=2021-07-01*ProcessingStartDate=2021-07-02*ProcessingTypeCode=13*ShowBreed=14*SourceTypeCode=15*Species=16*SpecificName=17*Type=18*Variety=19", "B7", ap0300APHISPK);
			product2PK = TestDataCreator.CreateCusAddInfo(new Guid("872B69EA-3578-4E66-9E67-0012E37617FB"), "APP", "Age=21*AgeRangeDesc=22*BreedVariety=23*Color=24*CountryCode=25*Gender=26*GeneralName=27*Genus=28*GeographicLocation=29*GestationalAgeIfPregnant=30*IsProtectedSpecies=Y*Origin=31*ProcessingDescription=32*ProcessingEndDate=2021-07-03*ProcessingStartDate=2021-07-04*ProcessingTypeCode=33*ShowBreed=34*SourceTypeCode=35*Species=36*SpecificName=37*Type=38*Variety=39", "B7", ap0300APHISPK);
			product3PK = TestDataCreator.CreateCusAddInfo(new Guid("20A32814-7F4B-45E4-AD3E-001458495FBF"), "APP", "Age=41*AgeRangeDesc=42*BreedVariety=43*Color=44*CountryCode=45*Gender=46*GeneralName=47*Genus=48*GeographicLocation=49*GestationalAgeIfPregnant=50*IsProtectedSpecies=Y*Origin=51*ProcessingDescription=52*ProcessingEndDate=2021-07-05*ProcessingStartDate=2021-07-06*ProcessingTypeCode=53*ShowBreed=54*SourceTypeCode=55*Species=56*SpecificName=57*Type=58*Variety=59", "B7", ap0300APHISPK);

			var apqAPHISPK = TestDataCreator.CreateCusAddInfo("APH", $"ProgramType=APQ*ProcessingCode=A03*IsDocSubmitted=Y*IndentedUseCode=1*IntendedUseDescription=2*CategoryType=AP0400*CategoryCode=3*ReMarks=4*VehicleNumber=5*VehicleLength=6*Qty1=7*UQ1=8*Qty2=9*UQ2=10*Qty3=11*UQ3=12*StockKeepingUnitNumber=13*ProductType=14*ProductNumber=15*ProductPhysicalState=17*ProductCondition=18*ProductStatus=19*BouquetGroupingNumber=20*ProductComponent=21*GrowingMedia=22*CommoditySpecificName=23*ScientificGenusName=24*ScientificSpeciesName=25*ScientificSubSpeciesName=26*OA_ApplicantAddress={address6PK}*OA_CropGrowerAddress={address2PK}*OA_USDAAPHISGrowerAddress={address3PK}*OA_PermittedAddress={address4PK}*OA_ShipperAddress={address5PK}", "JI", invoiceLinePK1);
			product1PK = TestDataCreator.CreateCusAddInfo(new Guid("9D5BC952-64C7-42AC-A729-0014AB8A0591"), "APP", "Age=1*AgeRangeDesc=2*BreedVariety=3*Color=4*CountryCode=5*Gender=6*GeneralName=7*Genus=8*GeographicLocation=9*GestationalAgeIfPregnant=10*IsProtectedSpecies=Y*Origin=11*ProcessingDescription=12*ProcessingEndDate=2021-07-01*ProcessingStartDate=2021-07-02*ProcessingTypeCode=13*ShowBreed=14*SourceTypeCode=15*Species=16*SpecificName=17*Type=18*Variety=19", "B7", apqAPHISPK);
			product2PK = TestDataCreator.CreateCusAddInfo(new Guid("789AB39D-0CAD-4BCE-AEE2-0014DCC38A1D"), "APP", "Age=21*AgeRangeDesc=22*BreedVariety=23*Color=24*CountryCode=25*Gender=26*GeneralName=27*Genus=28*GeographicLocation=29*GestationalAgeIfPregnant=30*IsProtectedSpecies=Y*Origin=31*ProcessingDescription=32*ProcessingEndDate=2021-07-03*ProcessingStartDate=2021-07-04*ProcessingTypeCode=33*ShowBreed=34*SourceTypeCode=35*Species=36*SpecificName=37*Type=38*Variety=39", "B7", apqAPHISPK);
			product3PK = TestDataCreator.CreateCusAddInfo(new Guid("186C8041-0AEB-4694-B78F-001705E3DE7F"), "APP", "Age=41*AgeRangeDesc=42*BreedVariety=43*Color=44*CountryCode=45*Gender=46*GeneralName=47*Genus=48*GeographicLocation=49*GestationalAgeIfPregnant=50*IsProtectedSpecies=Y*Origin=51*ProcessingDescription=52*ProcessingEndDate=2021-07-05*ProcessingStartDate=2021-07-06*ProcessingTypeCode=53*ShowBreed=54*SourceTypeCode=55*Species=56*SpecificName=57*Type=58*Variety=59", "B7", apqAPHISPK);

			var reportSql = @"SELECT * FROM USImportInvoiceLinePGAAPHIS(@companyPK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
				,'AAC', NUll, NULL)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var count = 0;
					while (reader.Read())
					{
						count++;
						CombineAssertions(() =>
						{
							AssertEquals("99038823", reader["ProvProgAddtionalTariff1"].ToString());
							AssertEquals("99038805", reader["ProvProgAddtionalTariff2"].ToString());
							AssertEquals("99030121", reader["ProvProgAddtionalTariff3"].ToString());
							AssertEquals("99030122", reader["ProvProgAddtionalTariff4"].ToString());
							AssertEquals("99030123", reader["ProvProgAddtionalTariff5"].ToString());
							AssertEquals(200m, (decimal)reader["ProvProgAddtionalDuty1"]);
							AssertEquals(150m, (decimal)reader["ProvProgAddtionalDuty2"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalDuty3"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalDuty4"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalDuty5"]);
							AssertEquals(10m, (decimal)reader["ProvProgAddtionalQty1"]);
							AssertEquals(20m, (decimal)reader["ProvProgAddtionalQty2"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalQty3"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalQty4"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalQty5"]);
							AssertEquals(110m, (decimal)reader["ProvProgAddtionalGoodsValue1"]);
							AssertEquals(120m, (decimal)reader["ProvProgAddtionalGoodsValue2"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalGoodsValue3"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalGoodsValue4"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalGoodsValue5"]);

							AssertEquals("AAC", reader["APHISProgramType"].ToString());
							AssertEquals("A01", reader["APHISProcessingCode"].ToString());
							AssertEquals("Y", reader["APHISIsDocSubmitted"].ToString());
							AssertEquals("1", reader["APHISIndentedUseCode"].ToString());
							AssertEquals("2", reader["APHISIntendedUseDescription"].ToString());
							AssertEquals("AP0100", reader["APHISCategoryType"].ToString());
							AssertEquals("3", reader["APHISCategoryCode"].ToString());
							AssertEquals("4", reader["APHISReMarks"].ToString());
							AssertEquals("5", reader["APHISVehicleNumber"].ToString());
							AssertEquals("6", reader["APHISVehicleLength"].ToString());
							AssertEquals("7", reader["APHISQty1"].ToString());
							AssertEquals("8", reader["APHISUQ1"].ToString());
							AssertEquals("9", reader["APHISQty2"].ToString());
							AssertEquals("10", reader["APHISUQ2"].ToString());
							AssertEquals("11", reader["APHISQty3"].ToString());
							AssertEquals("12", reader["APHISUQ3"].ToString());
							AssertEquals("13", reader["APHISStockKeepingUnitNumber"].ToString());
							AssertEquals("14", reader["APHISProductType"].ToString());
							AssertEquals("15", reader["APHISProductNumber"].ToString());
							AssertEquals("17", reader["APHISProductPhysicalState"].ToString());
							AssertEquals("18", reader["APHISProductCondition"].ToString());
							AssertEquals("19", reader["APHISProductStatus"].ToString());
							AssertEquals("20", reader["APHISBouquetGroupingNumber"].ToString());
							AssertEquals("21", reader["APHISProductComponent"].ToString());
							AssertEquals("22", reader["APHISGrowingMedia"].ToString());
							AssertEquals("23", reader["APHISCommoditySpecificName"].ToString());
							AssertEquals("24", reader["APHISScientificGenusName"].ToString());
							AssertEquals("25", reader["APHISScientificSpeciesName"].ToString());
							AssertEquals("26", reader["APHISScientificSubSpeciesName"].ToString());
							AssertEquals("ORG1", reader["APHISApplicantHeaderCode"].ToString());
							AssertEquals("Org1 AAA", reader["APHISApplicantHeaderName"].ToString());
							AssertEquals("ORG2", reader["APHISCropGrowerHeaderCode"].ToString());
							AssertEquals("Org2 BBB", reader["APHISCropGrowerHeaderName"].ToString());
							AssertEquals("ORG3", reader["APHISUSDAAPHISGrowerHeaderCode"].ToString());
							AssertEquals("Org3 CCC", reader["APHISUSDAAPHISGrowerHeaderName"].ToString());
							AssertEquals("ORG4", reader["APHISPermittedHeaderCode"].ToString());
							AssertEquals("Org4 DDD", reader["APHISPermittedHeaderName"].ToString());
							AssertEquals("ORG5", reader["APHISShipperHeaderCode"].ToString());
							AssertEquals("Org5 EEE", reader["APHISShipperHeaderName"].ToString());

							AssertEquals("14", reader["AP0100Product1ShowBreed"].ToString());
							AssertEquals("3", reader["AP0100Product1BreedVariety"].ToString());
							AssertEquals("1", reader["AP0100Product1Age"].ToString());
							AssertEquals("2", reader["AP0100Product1AgeRangeDesc"].ToString());
							AssertEquals("4", reader["AP0100Product1Color"].ToString());
							AssertEquals("6", reader["AP0100Product1Gender"].ToString());
							AssertEquals("10", reader["AP0100Product1GestationalAgeIfPregnant"].ToString());
							AssertEquals("N", reader["AP0100Product1IsFertilizedPregnantGestating"].ToString());
							AssertEquals("Y", reader["AP0100Product1IsProtectedSpecies"].ToString());
							AssertEquals("34", reader["AP0100Product2ShowBreed"].ToString());
							AssertEquals("23", reader["AP0100Product2BreedVariety"].ToString());
							AssertEquals("21", reader["AP0100Product2Age"].ToString());
							AssertEquals("22", reader["AP0100Product2AgeRangeDesc"].ToString());
							AssertEquals("24", reader["AP0100Product2Color"].ToString());
							AssertEquals("26", reader["AP0100Product2Gender"].ToString());
							AssertEquals("30", reader["AP0100Product2GestationalAgeIfPregnant"].ToString());
							AssertEquals("N", reader["AP0100Product2IsFertilizedPregnantGestating"].ToString());
							AssertEquals("Y", reader["AP0100Product2IsProtectedSpecies"].ToString());

							AssertEquals("", reader["AP0300Product1Type"].ToString());
							AssertEquals("", reader["AP0300Product1Origin"].ToString());
							AssertEquals("", reader["AP0300Product1SpecificName"].ToString());
							AssertEquals("", reader["AP0300Product1GeneralName"].ToString());
							AssertEquals("", reader["AP0300Product2Type"].ToString());
							AssertEquals("", reader["AP0300Product2Origin"].ToString());
							AssertEquals("", reader["AP0300Product2SpecificName"].ToString());
							AssertEquals("", reader["AP0300Product2GeneralName"].ToString());

							AssertEquals("", reader["APQProduct1Genus"].ToString());
							AssertEquals("", reader["APQProduct1Species"].ToString());
							AssertEquals("", reader["APQProduct1Variety"].ToString());
							AssertEquals("", reader["APQProduct1SourceTypeCode"].ToString());
							AssertEquals("", reader["APQProduct1CountryCode"].ToString());
							AssertEquals("", reader["APQProduct1GeographicLocation"].ToString());
							AssertEquals("", reader["APQProduct1ProcessingStartDate"].ToString());
							AssertEquals("", reader["APQProduct1ProcessingEndDate"].ToString());
							AssertEquals("", reader["APQProduct1ProcessingTypeCode"].ToString());
							AssertEquals("", reader["APQProduct1ProcessingDescription"].ToString());
							AssertEquals("", reader["APQProduct1SpecificName"].ToString());
							AssertEquals("", reader["APQProduct2Genus"].ToString());
							AssertEquals("", reader["APQProduct2Species"].ToString());
							AssertEquals("", reader["APQProduct2Variety"].ToString());
							AssertEquals("", reader["APQProduct2SourceTypeCode"].ToString());
							AssertEquals("", reader["APQProduct2CountryCode"].ToString());
							AssertEquals("", reader["APQProduct2GeographicLocation"].ToString());
							AssertEquals("", reader["APQProduct2ProcessingStartDate"].ToString());
							AssertEquals("", reader["APQProduct2ProcessingEndDate"].ToString());
							AssertEquals("", reader["APQProduct2ProcessingTypeCode"].ToString());
							AssertEquals("", reader["APQProduct2ProcessingDescription"].ToString());
							AssertEquals("", reader["APQProduct2SpecificName"].ToString());

							AssertEquals("1", reader["Source1CountryCode"].ToString());
							AssertEquals("2", reader["Source1GeographicLocation"].ToString());
							AssertEquals("3", reader["Source1ProcessingDescription"].ToString());
							AssertEquals("2021-07-11", reader["Source1ProcessingEndDate"].ToString());
							AssertEquals("2021-07-10", reader["Source1ProcessingStartDate"].ToString());
							AssertEquals("4", reader["Source1ProcessingTypeCode"].ToString());
							AssertEquals("5", reader["Source1SourceTypeCode"].ToString());
							AssertEquals("11", reader["Source2CountryCode"].ToString());
							AssertEquals("12", reader["Source2GeographicLocation"].ToString());
							AssertEquals("13", reader["Source2ProcessingDescription"].ToString());
							AssertEquals("2021-07-13", reader["Source2ProcessingEndDate"].ToString());
							AssertEquals("2021-07-12", reader["Source2ProcessingStartDate"].ToString());
							AssertEquals("14", reader["Source2ProcessingTypeCode"].ToString());
							AssertEquals("15", reader["Source2SourceTypeCode"].ToString());

							AssertEquals("2021-07-20", reader["License1Date"].ToString());
							AssertEquals("1", reader["License1DateQualifier"].ToString());
							AssertEquals("2", reader["License1Number"].ToString());
							AssertEquals("3", reader["License1Quantity"].ToString());
							AssertEquals("4", reader["License1RN_CountryCode"].ToString());
							AssertEquals("5", reader["License1StateDescription"].ToString());
							AssertEquals("6", reader["License1Type"].ToString());
							AssertEquals("7", reader["License1UnitOfMeasure"].ToString());
							AssertEquals("2021-07-21", reader["License2Date"].ToString());
							AssertEquals("11", reader["License2DateQualifier"].ToString());
							AssertEquals("12", reader["License2Number"].ToString());
							AssertEquals("13", reader["License2Quantity"].ToString());
							AssertEquals("14", reader["License2RN_CountryCode"].ToString());
							AssertEquals("15", reader["License2StateDescription"].ToString());
							AssertEquals("16", reader["License2Type"].ToString());
							AssertEquals("17", reader["License2UnitOfMeasure"].ToString());

							AssertEquals("2021-07-25", reader["Inspection1Date"].ToString());
							AssertEquals("1", reader["Inspection1Location"].ToString());
							AssertEquals("2", reader["Inspection1TestingStatus"].ToString());
							AssertEquals("2021-07-26", reader["Inspection2Date"].ToString());
							AssertEquals("11", reader["Inspection2Location"].ToString());
							AssertEquals("12", reader["Inspection2TestingStatus"].ToString());

							AssertEquals("1", reader["Routing1Country"].ToString());
							AssertEquals("2", reader["Routing1State"].ToString());
							AssertEquals("3", reader["Routing1Type"].ToString());
							AssertEquals("11", reader["Routing2Country"].ToString());
							AssertEquals("12", reader["Routing2State"].ToString());
							AssertEquals("13", reader["Routing2Type"].ToString());
						});
					}
					AssertEquals(1, count);
				}
			}

			reportSql = @"SELECT * FROM USImportInvoiceLinePGAAPHIS(@companyPK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
				,NULL, 'A04', NULL)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var count = 0;
					while (reader.Read())
					{
						count++;
						CombineAssertions(() =>
						{
							AssertEquals("99038823", reader["ProvProgAddtionalTariff1"].ToString());
							AssertEquals("99038805", reader["ProvProgAddtionalTariff2"].ToString());
							AssertEquals("99030121", reader["ProvProgAddtionalTariff3"].ToString());
							AssertEquals("99030122", reader["ProvProgAddtionalTariff4"].ToString());
							AssertEquals("99030123", reader["ProvProgAddtionalTariff5"].ToString());
							AssertEquals(200m, (decimal)reader["ProvProgAddtionalDuty1"]);
							AssertEquals(150m, (decimal)reader["ProvProgAddtionalDuty2"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalDuty3"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalDuty4"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalDuty5"]);
							AssertEquals(10m, (decimal)reader["ProvProgAddtionalQty1"]);
							AssertEquals(20m, (decimal)reader["ProvProgAddtionalQty2"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalQty3"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalQty4"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalQty5"]);
							AssertEquals(110m, (decimal)reader["ProvProgAddtionalGoodsValue1"]);
							AssertEquals(120m, (decimal)reader["ProvProgAddtionalGoodsValue2"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalGoodsValue3"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalGoodsValue4"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalGoodsValue5"]);

							AssertEquals("", reader["AP0100Product1ShowBreed"].ToString());
							AssertEquals("", reader["AP0100Product1BreedVariety"].ToString());
							AssertEquals("", reader["AP0100Product1Age"].ToString());
							AssertEquals("", reader["AP0100Product1AgeRangeDesc"].ToString());
							AssertEquals("", reader["AP0100Product1Color"].ToString());
							AssertEquals("", reader["AP0100Product1Gender"].ToString());
							AssertEquals("", reader["AP0100Product1GestationalAgeIfPregnant"].ToString());
							AssertEquals("", reader["AP0100Product1IsFertilizedPregnantGestating"].ToString());
							AssertEquals("", reader["AP0100Product1IsProtectedSpecies"].ToString());
							AssertEquals("", reader["AP0100Product2ShowBreed"].ToString());
							AssertEquals("", reader["AP0100Product2BreedVariety"].ToString());
							AssertEquals("", reader["AP0100Product2Age"].ToString());
							AssertEquals("", reader["AP0100Product2AgeRangeDesc"].ToString());
							AssertEquals("", reader["AP0100Product2Color"].ToString());
							AssertEquals("", reader["AP0100Product2Gender"].ToString());
							AssertEquals("", reader["AP0100Product2GestationalAgeIfPregnant"].ToString());
							AssertEquals("", reader["AP0100Product2IsFertilizedPregnantGestating"].ToString());
							AssertEquals("", reader["AP0100Product2IsProtectedSpecies"].ToString());

							AssertEquals("18", reader["AP0300Product1Type"].ToString());
							AssertEquals("11", reader["AP0300Product1Origin"].ToString());
							AssertEquals("17", reader["AP0300Product1SpecificName"].ToString());
							AssertEquals("7", reader["AP0300Product1GeneralName"].ToString());
							AssertEquals("38", reader["AP0300Product2Type"].ToString());
							AssertEquals("31", reader["AP0300Product2Origin"].ToString());
							AssertEquals("37", reader["AP0300Product2SpecificName"].ToString());
							AssertEquals("27", reader["AP0300Product2GeneralName"].ToString());

							AssertEquals("", reader["APQProduct1Genus"].ToString());
							AssertEquals("", reader["APQProduct1Species"].ToString());
							AssertEquals("", reader["APQProduct1Variety"].ToString());
							AssertEquals("", reader["APQProduct1SourceTypeCode"].ToString());
							AssertEquals("", reader["APQProduct1CountryCode"].ToString());
							AssertEquals("", reader["APQProduct1GeographicLocation"].ToString());
							AssertEquals("", reader["APQProduct1ProcessingStartDate"].ToString());
							AssertEquals("", reader["APQProduct1ProcessingEndDate"].ToString());
							AssertEquals("", reader["APQProduct1ProcessingTypeCode"].ToString());
							AssertEquals("", reader["APQProduct1ProcessingDescription"].ToString());
							AssertEquals("", reader["APQProduct1SpecificName"].ToString());
							AssertEquals("", reader["APQProduct2Genus"].ToString());
							AssertEquals("", reader["APQProduct2Species"].ToString());
							AssertEquals("", reader["APQProduct2Variety"].ToString());
							AssertEquals("", reader["APQProduct2SourceTypeCode"].ToString());
							AssertEquals("", reader["APQProduct2CountryCode"].ToString());
							AssertEquals("", reader["APQProduct2GeographicLocation"].ToString());
							AssertEquals("", reader["APQProduct2ProcessingStartDate"].ToString());
							AssertEquals("", reader["APQProduct2ProcessingEndDate"].ToString());
							AssertEquals("", reader["APQProduct2ProcessingTypeCode"].ToString());
							AssertEquals("", reader["APQProduct2ProcessingDescription"].ToString());
							AssertEquals("", reader["APQProduct2SpecificName"].ToString());
						});
					}
					AssertEquals(1, count);
				}
			}

			reportSql = @"SELECT * FROM USImportInvoiceLinePGAAPHIS(@companyPK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
				,NULL, NULL, @orgPK)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@orgPK", SqlDbType.UniqueIdentifier, org6PK);
				using (var reader = command.ExecuteReader())
				{
					var count = 0;
					while (reader.Read())
					{
						count++;
						CombineAssertions(() =>
						{
							AssertEquals("99038823", reader["ProvProgAddtionalTariff1"].ToString());
							AssertEquals("99038805", reader["ProvProgAddtionalTariff2"].ToString());
							AssertEquals("99030121", reader["ProvProgAddtionalTariff3"].ToString());
							AssertEquals("99030122", reader["ProvProgAddtionalTariff4"].ToString());
							AssertEquals("99030123", reader["ProvProgAddtionalTariff5"].ToString());
							AssertEquals(200m, (decimal)reader["ProvProgAddtionalDuty1"]);
							AssertEquals(150m, (decimal)reader["ProvProgAddtionalDuty2"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalDuty3"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalDuty4"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalDuty5"]);
							AssertEquals(10m, (decimal)reader["ProvProgAddtionalQty1"]);
							AssertEquals(20m, (decimal)reader["ProvProgAddtionalQty2"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalQty3"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalQty4"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalQty5"]);
							AssertEquals(110m, (decimal)reader["ProvProgAddtionalGoodsValue1"]);
							AssertEquals(120m, (decimal)reader["ProvProgAddtionalGoodsValue2"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalGoodsValue3"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalGoodsValue4"]);
							AssertEquals(0m, (decimal)reader["ProvProgAddtionalGoodsValue5"]);

							AssertEquals("", reader["AP0100Product1ShowBreed"].ToString());
							AssertEquals("", reader["AP0100Product1BreedVariety"].ToString());
							AssertEquals("", reader["AP0100Product1Age"].ToString());
							AssertEquals("", reader["AP0100Product1AgeRangeDesc"].ToString());
							AssertEquals("", reader["AP0100Product1Color"].ToString());
							AssertEquals("", reader["AP0100Product1Gender"].ToString());
							AssertEquals("", reader["AP0100Product1GestationalAgeIfPregnant"].ToString());
							AssertEquals("", reader["AP0100Product1IsFertilizedPregnantGestating"].ToString());
							AssertEquals("", reader["AP0100Product1IsProtectedSpecies"].ToString());
							AssertEquals("", reader["AP0100Product2ShowBreed"].ToString());
							AssertEquals("", reader["AP0100Product2BreedVariety"].ToString());
							AssertEquals("", reader["AP0100Product2Age"].ToString());
							AssertEquals("", reader["AP0100Product2AgeRangeDesc"].ToString());
							AssertEquals("", reader["AP0100Product2Color"].ToString());
							AssertEquals("", reader["AP0100Product2Gender"].ToString());
							AssertEquals("", reader["AP0100Product2GestationalAgeIfPregnant"].ToString());
							AssertEquals("", reader["AP0100Product2IsFertilizedPregnantGestating"].ToString());
							AssertEquals("", reader["AP0100Product2IsProtectedSpecies"].ToString());

							AssertEquals("", reader["AP0300Product1Type"].ToString());
							AssertEquals("", reader["AP0300Product1Origin"].ToString());
							AssertEquals("", reader["AP0300Product1SpecificName"].ToString());
							AssertEquals("", reader["AP0300Product1GeneralName"].ToString());
							AssertEquals("", reader["AP0300Product2Type"].ToString());
							AssertEquals("", reader["AP0300Product2Origin"].ToString());
							AssertEquals("", reader["AP0300Product2SpecificName"].ToString());
							AssertEquals("", reader["AP0300Product2GeneralName"].ToString());

							AssertEquals("8", reader["APQProduct1Genus"].ToString());
							AssertEquals("16", reader["APQProduct1Species"].ToString());
							AssertEquals("19", reader["APQProduct1Variety"].ToString());
							AssertEquals("15", reader["APQProduct1SourceTypeCode"].ToString());
							AssertEquals("5", reader["APQProduct1CountryCode"].ToString());
							AssertEquals("9", reader["APQProduct1GeographicLocation"].ToString());
							AssertEquals("2021-07-02", reader["APQProduct1ProcessingStartDate"].ToString());
							AssertEquals("2021-07-01", reader["APQProduct1ProcessingEndDate"].ToString());
							AssertEquals("13", reader["APQProduct1ProcessingTypeCode"].ToString());
							AssertEquals("12", reader["APQProduct1ProcessingDescription"].ToString());
							AssertEquals("17", reader["APQProduct1SpecificName"].ToString());
							AssertEquals("28", reader["APQProduct2Genus"].ToString());
							AssertEquals("36", reader["APQProduct2Species"].ToString());
							AssertEquals("39", reader["APQProduct2Variety"].ToString());
							AssertEquals("35", reader["APQProduct2SourceTypeCode"].ToString());
							AssertEquals("25", reader["APQProduct2CountryCode"].ToString());
							AssertEquals("29", reader["APQProduct2GeographicLocation"].ToString());
							AssertEquals("2021-07-04", reader["APQProduct2ProcessingStartDate"].ToString());
							AssertEquals("2021-07-03", reader["APQProduct2ProcessingEndDate"].ToString());
							AssertEquals("33", reader["APQProduct2ProcessingTypeCode"].ToString());
							AssertEquals("32", reader["APQProduct2ProcessingDescription"].ToString());
							AssertEquals("37", reader["APQProduct2SpecificName"].ToString());
						});
					}
					AssertEquals(1, count);
				}
			}
		}
	}
}
