using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using YesNoList = Enterprise.Customs.Business.YesNoList;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CAOrgSupplierPartDataLoad))]
	sealed class OrgSupplierPartDataLoadTest : DataLoadTestCase<CAOrgSupplierPartDataLoad>
	{
		public void TestImportingCountrySpecificFields()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "1234567890", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(tariff, "CU1", "NUM");
			var tariff1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "2222222222", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(tariff1, "CU1", "LTA");

			var hSTClass1 = CreateClassification("C1", "1234567890", CusClassification.ClassificationType.IMP);
			var hSTClass2 = CreateClassification("C2", "2222222222", CusClassification.ClassificationType.IMP);
			var exportClass1 = CreateClassification("C3", "12345678", CusClassification.ClassificationType.EXP);
			var exportClass2 = CreateClassification("C4", "99999999", CusClassification.ClassificationType.EXP);
			var cACClassHeader1 = Factory.New<CACClassHeader>();
			cACClassHeader1.ZA_ClassificationNumber = "1234567890";
			cACClassHeader1.ZA_AreaCode = "AAA";
			cACClassHeader1.ZA_StatisticalUOMCode = "NUM";
			cACClassHeader1.ZA_EffectiveDate = ZDateTime.Today.AddDays(-1);
			cACClassHeader1.ZA_ExpiryDate = ZDateTime.Today.AddDays(1);
			var cACClassHeader2 = Factory.New<CACClassHeader>();
			cACClassHeader2.ZA_ClassificationNumber = "2222222222";
			cACClassHeader2.ZA_AreaCode = "BBB";
			cACClassHeader2.ZA_StatisticalUOMCode = "LTA";
			cACClassHeader2.ZA_EffectiveDate = ZDateTime.Today.AddDays(-1);
			cACClassHeader2.ZA_ExpiryDate = ZDateTime.Today.AddDays(1);
			var cACExportTariff = Factory.New<CACExportTariff>();
			cACExportTariff.CE_Code = "12345678";
			cACExportTariff.CE_Unit = "LTR";
			cACExportTariff.CE_Description = "XXXXX";
			Factory.Save();

			var dataLoad = new CAOrgSupplierPartDataLoad();
			using (TempFile tempFile1 = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(tempFile1.Filename))
				{
					sw.WriteLine(fileHeader);
					//            Code,Description,UQ,ExportClassification,ExportTariff,ImportClassification,ImportTariff,Owner,Supplier,Origin,OriginState
					sw.WriteLine("P001,DESCP001   ,KG,C2                  ,            ,C1                  ,            ,     ,ORG01   ,US    ,FLX");
					sw.WriteLine("P002,DESCP002   ,KG,C3                  ,            ,C1                  ,            ,     ,ORG01   ,CA    ,NU");
					sw.WriteLine("P003,DESCP003   ,KG,C3                  ,            ,                    ,            ,     ,ORG01   ,      ,");
					sw.WriteLine("P004,DESCP004   ,KG,                    ,            ,C2                  ,            ,     ,ORG01   ,      ,");
					sw.WriteLine("P005,DESCP005   ,KG,                    ,1111.22.33  ,                    ,1111.22.33 44,    ,ORG01   ,      ,");
					sw.WriteLine("P006,DESCP006   ,KG,                    ,2222334455  ,                    ,            ,     ,ORG01   ,CA    ,ON");
					sw.WriteLine("P007,DESCP007   ,  ,                    ,            ,                    ,1234567890  ,     ,ORG01   ,      ,");
					sw.WriteLine("P008,DESCP008   ,  ,                    ,            ,C1                  ,            ,     ,ORG01   ,      ,");
					sw.WriteLine("P009,DESCP009   ,  ,C2                  ,            ,                    ,            ,     ,ORG01   ,      ,");
					sw.WriteLine("P010,DESCP010   ,  ,                    ,1234567890  ,                    ,            ,     ,ORG01   ,      ,");
					sw.WriteLine("P011,DESCP011   ,  ,                    ,12345678    ,                    ,            ,     ,ORG01   ,      ,");
					sw.WriteLine("P012,DESCP012   ,  ,C3                  ,            ,                    ,            ,     ,ORG01   ,      ,");
					sw.WriteLine("P013,DESCP013   ,  ,C2                  ,            ,                    ,            ,     ,ORG01   ,      ,");
					sw.WriteLine("P014,DESCP014   ,  ,C3                  ,            ,                    ,            ,     ,ORG01   ,      ,");
				}

				dataLoad.ImportProductData(tempFile1.Filename, false, false);
				Factory.Save();
				AssertEquals("Products Created", 14, dataLoad.RunCounters.RecsCreated);

				var product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P001", null, testOrganisation);
				AssertNotNull("Product", product);
				AssertEquals("Units from file", "KG", product.OP_StockKeepingUnit);
				AssertEquals("2 pivots", 2, product.PivotsForBinding.Count);
				var exportPivot = product.PivotsForBinding.GetExportMatch(true, ZGuid.Empty, ZGuid.Empty);
				AssertNull("No Export Export Pivot", exportPivot);
				exportPivot = product.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("HTS Export Pivot", exportPivot);
				var importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot);
				AssertEquals("Export pivot country/region", Core.Constants.CountryCodes.Canada, exportPivot.CI_RN_NKCountry);
				AssertEquals("Export pivot type", ClassificationTypeList.Codes.HTE, exportPivot.CI_ChildType);
				AssertEquals("Export pivot class", hSTClass2.PK, exportPivot.CI_CC);
				AssertEquals("Export pivot org", "US", exportPivot.CCA_RN_NKOrigin);
				AssertEquals("Export pivot province truncated without explosion", "FL", exportPivot.CCA_ProvinceOfOrigin);
				AssertEquals("Import pivot country/region", Core.Constants.CountryCodes.Canada, importPivot.CI_RN_NKCountry);
				AssertEquals("Import pivot type", ClassificationTypeList.Codes.HTI, importPivot.CI_ChildType);
				AssertEquals("Import pivot class", hSTClass1.PK, importPivot.CI_CC);
				AssertEquals("Import pivot org", "US", importPivot.CCA_RN_NKOrigin);
				AssertEquals("Import pivot state", "FL", importPivot.CCA_ProvinceOfOrigin);

				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P002", null, testOrganisation);
				AssertNotNull("Product", product);
				AssertEquals("Units from file", "KG", product.OP_StockKeepingUnit);
				AssertEquals("2 pivots", 2, product.PivotsForBinding.Count);
				exportPivot = product.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, ZGuid.Empty);
				AssertNull("No HTS Export Pivot", exportPivot);
				exportPivot = product.PivotsForBinding.GetExportMatch(true, ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Export Export Pivot", exportPivot);
				importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot);
				AssertEquals("Export pivot country/region", Core.Constants.CountryCodes.Canada, exportPivot.CI_RN_NKCountry);
				AssertEquals("Export pivot type", ClassificationTypeList.Codes.SHB, exportPivot.CI_ChildType);
				AssertEquals("Export pivot class", exportClass1.PK, exportPivot.CI_CC);
				AssertEquals("Export pivot org", "CA", exportPivot.CCA_RN_NKOrigin);
				AssertEquals("Export pivot state", "NU", exportPivot.CCA_ProvinceOfOrigin);
				AssertEquals("Import pivot country/region", Core.Constants.CountryCodes.Canada, importPivot.CI_RN_NKCountry);
				AssertEquals("Import pivot type", ClassificationTypeList.Codes.HTI, importPivot.CI_ChildType);
				AssertEquals("Import pivot class", hSTClass1.PK, importPivot.CI_CC);
				AssertEquals("Import pivot org", "CA", importPivot.CCA_RN_NKOrigin);
				AssertEquals("Import pivot state", ZString.Empty, importPivot.CCA_ProvinceOfOrigin);

				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P003", null, testOrganisation);
				AssertNotNull("Product", product);
				AssertEquals("Units from file", "KG", product.OP_StockKeepingUnit);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				exportPivot = product.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, ZGuid.Empty);
				AssertNull("No HTS Export Pivot", exportPivot);
				exportPivot = product.PivotsForBinding.GetExportMatch(true, ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Export Export Pivot", exportPivot);
				importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNull("No Import pivot", importPivot);
				AssertEquals("Export pivot country/region", Core.Constants.CountryCodes.Canada, exportPivot.CI_RN_NKCountry);
				AssertEquals("Export pivot type", ClassificationTypeList.Codes.SHB, exportPivot.CI_ChildType);
				AssertEquals("Export pivot class", exportClass1.PK, exportPivot.CI_CC);
				AssertEquals("Export pivot org", ZString.Empty, exportPivot.CCA_RN_NKOrigin);
				AssertEquals("Export pivot state", ZString.Empty, exportPivot.CCA_ProvinceOfOrigin);

				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P004", null, testOrganisation);
				AssertNotNull("Product", product);
				AssertEquals("Units from file", "KG", product.OP_StockKeepingUnit);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				exportPivot = product.PivotsForBinding.GetExportMatch(true, ZGuid.Empty, ZGuid.Empty);
				AssertNull("No Export Export Pivot", exportPivot);
				exportPivot = product.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, ZGuid.Empty);
				AssertNull("No HTS Export Pivot", exportPivot);
				importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot);
				AssertEquals("Import pivot country/region", Core.Constants.CountryCodes.Canada, importPivot.CI_RN_NKCountry);
				AssertEquals("Import pivot type", ClassificationTypeList.Codes.HTI, importPivot.CI_ChildType);
				AssertEquals("Import pivot class", hSTClass2.PK, importPivot.CI_CC);
				AssertEquals("Import pivot org", ZString.Empty, importPivot.CCA_RN_NKOrigin);
				AssertEquals("Import pivot state", ZString.Empty, importPivot.CCA_ProvinceOfOrigin);

				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P005", null, testOrganisation);
				AssertNotNull("Product", product);
				AssertEquals("Units from file", "KG", product.OP_StockKeepingUnit);
				AssertEquals("2 pivots", 2, product.PivotsForBinding.Count);
				exportPivot = product.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, ZGuid.Empty);
				AssertNull("No HTS Export Pivot", exportPivot);
				exportPivot = product.PivotsForBinding.GetExportMatch(true, ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Export Export Pivot", exportPivot);
				importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot);
				AssertEquals("Export pivot country/region", Core.Constants.CountryCodes.Canada, exportPivot.CI_RN_NKCountry);
				AssertEquals("Export pivot type", ClassificationTypeList.Codes.SHB, exportPivot.CI_ChildType);
				AssertEquals("No Export pivot class", ZGuid.Empty, exportPivot.CI_CC);
				AssertEquals("Export pivot tariff num", "11112233", exportPivot.CI_TariffNum);
				AssertEquals("Import pivot country/region", Core.Constants.CountryCodes.Canada, importPivot.CI_RN_NKCountry);
				AssertEquals("Import pivot type", ClassificationTypeList.Codes.HTI, importPivot.CI_ChildType);
				AssertEquals("No Import pivot class", ZGuid.Empty, importPivot.CI_CC);
				AssertEquals("mport pivot tariff num", "1111223344", importPivot.CI_TariffNum);

				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P006", null, testOrganisation);
				AssertNotNull("Product", product);
				AssertEquals("Units from file", "KG", product.OP_StockKeepingUnit);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				exportPivot = product.PivotsForBinding.GetExportMatch(true, ZGuid.Empty, ZGuid.Empty);
				AssertNull("No Export Export Pivot", exportPivot);
				exportPivot = product.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("HTS Export Pivot", exportPivot);
				importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNull("No Import pivot", importPivot);
				AssertEquals("Export pivot country/region", Core.Constants.CountryCodes.Canada, exportPivot.CI_RN_NKCountry);
				AssertEquals("Export pivot type", ClassificationTypeList.Codes.HTE, exportPivot.CI_ChildType);
				AssertEquals("No Export pivot class", ZGuid.Empty, exportPivot.CI_CC);
				AssertEquals("Export pivot tariff num", "2222334455", exportPivot.CI_TariffNum);
				AssertEquals("Export pivot org", "CA", exportPivot.CCA_RN_NKOrigin);
				AssertEquals("Export pivot state", "ON", exportPivot.CCA_ProvinceOfOrigin);

				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P007", null, testOrganisation);
				AssertNotNull("Product", product);
				AssertEquals("Units from tariff num", "NUM", product.OP_StockKeepingUnit);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot);
				AssertEquals("Correct tariff num", "1234567890", importPivot.CI_TariffNum);

				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P008", null, testOrganisation);
				AssertNotNull("Product", product);
				AssertEquals("Units from import class", "NUM", product.OP_StockKeepingUnit);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot);

				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P009", null, testOrganisation);
				AssertNotNull("Product", product);
				AssertEquals("Units from export HTS class", "LTA", product.OP_StockKeepingUnit);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				exportPivot = product.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("HTS Export Pivot", exportPivot);
				AssertEquals("Correct class", hSTClass2.PK, exportPivot.CI_CC);

				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P010", null, testOrganisation);
				AssertNotNull("Product", product);
				AssertEquals("Units from export HTS tariff num", "NUM", product.OP_StockKeepingUnit);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				exportPivot = product.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("HTS Export Pivot", exportPivot);

				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P011", null, testOrganisation);
				AssertNotNull("Product", product);
				AssertEquals("Units from export Export tariff num", "LTR", product.OP_StockKeepingUnit);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				exportPivot = product.PivotsForBinding.GetExportMatch(true, ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Export Export Pivot", exportPivot);

				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P012", null, testOrganisation);
				AssertNotNull("Product", product);
				AssertEquals("Units from export Export class", "LTR", product.OP_StockKeepingUnit);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				exportPivot = product.PivotsForBinding.GetExportMatch(true, ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Export Export Pivot", exportPivot);
				AssertEquals("Correct class", exportClass1.PK, exportPivot.CI_CC);
			}
			using (TempFile tempFile2 = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(tempFile2.Filename))
				{
					sw.WriteLine(fileHeader);
					//            Code,Description,UQ,ExportClassification,ExportTariff,ImportClassification,ImportTariff,Owner,Supplier,Origin,OriginState,GSTCode
					sw.WriteLine("P003,           ,  ,C2                  ,            ,C1                  ,            ,     ,ORG01   ,US    ,FL");
					sw.WriteLine("P004,           ,  ,                    ,            ,C1                  ,            ,     ,ORG01   ,US    ,IL");
					sw.WriteLine("P009,           ,  ,C1                  ,            ,                    ,            ,     ,ORG01   ,      ,");
					sw.WriteLine("P012,           ,  ,C4                  ,            ,                    ,            ,     ,ORG01   ,      ,");
					sw.WriteLine("P007,           ,  ,                    ,            ,                    ,9999999999  ,     ,ORG01   ,      ,");
					sw.WriteLine("P010,           ,  ,                    ,8888888888  ,                    ,            ,     ,ORG01   ,      ,");
					sw.WriteLine("P011,           ,  ,                    ,77777777    ,                    ,            ,     ,ORG01   ,      ,");
					sw.WriteLine("P008,           ,  ,                    ,            ,                    ,6666666666  ,     ,ORG01   ,      ,");
					sw.WriteLine("P013,           ,  ,                    ,5555555555  ,                    ,            ,     ,ORG01   ,      ,");
					sw.WriteLine("P014,           ,  ,                    ,44444444    ,                    ,            ,     ,ORG01   ,      ,");
					sw.WriteLine("P006,           ,  ,                    ,            ,                    ,3333333333  ,     ,ORG01   ,      ,");
				}

				dataLoad.ImportProductData(tempFile2.Filename, true, false);
				Factory.Save();
				AssertEquals("Products Updated", 11, dataLoad.RunCounters.RecsUpdated);

				BusinessObjectFactory factory2 = new BusinessObjectFactory();

				var product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(factory2, typeof(OrgSupplierPart)).Load("P003", null, testOrganisation);
				AssertNotNull("Product", product);
				AssertEquals("3 pivot", 3, product.PivotsForBinding.Count);
				var exportPivot = product.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("HTS Export Pivot", exportPivot);
				AssertEquals("Export pivot country/region", Core.Constants.CountryCodes.Canada, exportPivot.CI_RN_NKCountry);
				AssertEquals("Export pivot type", ClassificationTypeList.Codes.HTE, exportPivot.CI_ChildType);
				AssertEquals("Export pivot class", hSTClass2.PK, exportPivot.CI_CC);
				AssertEquals("Export pivot org", "US", exportPivot.CCA_RN_NKOrigin);
				AssertEquals("Export pivot province", "FL", exportPivot.CCA_ProvinceOfOrigin);
				exportPivot = product.PivotsForBinding.GetExportMatch(true, ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Export Export Pivot", exportPivot);
				AssertEquals("Export pivot country/region", Core.Constants.CountryCodes.Canada, exportPivot.CI_RN_NKCountry);
				AssertEquals("Export pivot type", ClassificationTypeList.Codes.SHB, exportPivot.CI_ChildType);
				AssertEquals("Export pivot class", exportClass1.PK, exportPivot.CI_CC);
				AssertEquals("Export pivot org", ZString.Empty, exportPivot.CCA_RN_NKOrigin);
				AssertEquals("Export pivot state", ZString.Empty, exportPivot.CCA_ProvinceOfOrigin);
				var importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot);
				AssertEquals("Import pivot country/region", Core.Constants.CountryCodes.Canada, importPivot.CI_RN_NKCountry);
				AssertEquals("Import pivot type", ClassificationTypeList.Codes.HTI, importPivot.CI_ChildType);
				AssertEquals("Import pivot class", hSTClass1.PK, importPivot.CI_CC);
				AssertEquals("Import pivot org", "US", importPivot.CCA_RN_NKOrigin);
				AssertEquals("Import pivot state", "FL", importPivot.CCA_ProvinceOfOrigin);

				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(factory2, typeof(OrgSupplierPart)).Load("P004", null, testOrganisation);
				AssertNotNull("Product", product);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot);
				AssertEquals("Import pivot country/region", Core.Constants.CountryCodes.Canada, importPivot.CI_RN_NKCountry);
				AssertEquals("Import pivot type", ClassificationTypeList.Codes.HTI, importPivot.CI_ChildType);
				AssertEquals("Import pivot class", hSTClass1.PK, importPivot.CI_CC);
				AssertEquals("Import pivot org", "US", importPivot.CCA_RN_NKOrigin);
				AssertEquals("Import pivot state", "IL", importPivot.CCA_ProvinceOfOrigin);

				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(factory2, typeof(OrgSupplierPart)).Load("P009", null, testOrganisation);
				AssertNotNull("Product", product);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				exportPivot = product.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("HTS Export Pivot", exportPivot);
				AssertEquals("Correct class", hSTClass1.PK, exportPivot.CI_CC);

				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(factory2, typeof(OrgSupplierPart)).Load("P012", null, testOrganisation);
				AssertNotNull("Product", product);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				exportPivot = product.PivotsForBinding.GetExportMatch(true, ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Export Export Pivot", exportPivot);
				AssertEquals("Correct class", exportClass2.PK, exportPivot.CI_CC);

				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(factory2, typeof(OrgSupplierPart)).Load("P007", null, testOrganisation);
				AssertNotNull("Product", product);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot);
				AssertEquals("Correct tariff num", "9999999999", importPivot.CI_TariffNum);

				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(factory2, typeof(OrgSupplierPart)).Load("P010", null, testOrganisation);
				AssertNotNull("Product", product);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				exportPivot = product.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("HTS Export Pivot", exportPivot);
				AssertEquals("Correct tariff num", "8888888888", exportPivot.CI_TariffNum);

				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(factory2, typeof(OrgSupplierPart)).Load("P011", null, testOrganisation);
				AssertNotNull("Product", product);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				exportPivot = product.PivotsForBinding.GetExportMatch(true, ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Export Export Pivot", exportPivot);
				AssertEquals("Correct tariff num", "77777777", exportPivot.CI_TariffNum);

				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(factory2, typeof(OrgSupplierPart)).Load("P008", null, testOrganisation);
				AssertNotNull("Product", product);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot);
				AssertEquals("Class removed", ZGuid.Empty, importPivot.CI_CC);
				AssertEquals("Traiff Num added", "6666666666", importPivot.CI_TariffNum);

				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(factory2, typeof(OrgSupplierPart)).Load("P013", null, testOrganisation);
				AssertNotNull("Product", product);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				exportPivot = product.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot);
				AssertNotNull("HTS Export Pivot", exportPivot);
				AssertEquals("Class removed", ZGuid.Empty, exportPivot.CI_CC);
				AssertEquals("Traiff Num added", "5555555555", exportPivot.CI_TariffNum);

				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(factory2, typeof(OrgSupplierPart)).Load("P014", null, testOrganisation);
				AssertNotNull("Product", product);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				exportPivot = product.PivotsForBinding.GetExportMatch(true, ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot);
				AssertNotNull("Export Export Pivot", exportPivot);
				AssertEquals("Class removed", ZGuid.Empty, exportPivot.CI_CC);
				AssertEquals("Traiff Num added", "44444444", exportPivot.CI_TariffNum);

				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(factory2, typeof(OrgSupplierPart)).Load("P006", null, testOrganisation);
				AssertNotNull("Product", product);
				AssertEquals("2 pivot", 2, product.PivotsForBinding.Count);
				exportPivot = product.PivotsForBinding.GetExportMatch(true, ZGuid.Empty, ZGuid.Empty);
				AssertNull("No Export Export Pivot", exportPivot);
				exportPivot = product.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("HTS Export Pivot", exportPivot);
				AssertEquals("Export pivot tariff num", "2222334455", exportPivot.CI_TariffNum);
				AssertEquals("Export pivot org", "CA", exportPivot.CCA_RN_NKOrigin);
				AssertEquals("Export pivot state", "ON", exportPivot.CCA_ProvinceOfOrigin);
				importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot);
				AssertEquals("Traiff Num added", "3333333333", importPivot.CI_TariffNum);
			}
		}

		public void TestImportingMoreData()
		{
			Factory.Save();
			using (TempFile tempFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var cfiaIndicators = ",PGA_CFIA_Indicator";
					var cfiaFields = ",PGA_CFIA_AIRSExtensionCode,PGA_CFIA_LPCOs,PGA_CFIA_AIRSRegistrations,PGA_CFIA_AIRSEndUse,PGA_CFIA_AIRSMiscellaneous,PGA_CFIA_SourceCountry,PGA_CFIA_SourceState";
					var cnscIndicators = ",PGA_CNSC_Indicator";
					var dfoIndicators = ",PGA_DFO_ABIInd,PGA_DFO_AISInd,PGA_DFO_TTPInd";
					var ecccIndicators = ",PGA_ECCC_WRMInd,PGA_ECCC_ODSInd,PGA_ECCC_WENInd,PGA_ECCC_VEEInd";
					var gacIndicators = ",PGA_GAC_Indicator";
					var hcIndicators = ",PGA_HC_APIInd,PGA_HC_BBCInd,PGA_HC_CTOInd,PGA_HC_CPRInd,PGA_HC_DSEInd,PGA_HC_HDRInd,PGA_HC_OCSInd,PGA_HC_MDEInd,PGA_HC_NHPInd,PGA_HC_PESInd,PGA_HC_REDInd,PGA_HC_VETInd";
					var nrcanIndicators = ",PGA_NRCan_EEFInd,PGA_NRCan_EXPInd,PGA_NRCan_RDAInd";
					var phacIndicators = ",PGA_PHAC_HAPInd";
					var tcIndicators = ",PGA_TC_TPRInd,PGA_TC_VPRInd";

					string lpcos = "AAAA/11111111111111/22222222222222;BBBB/44444444444444;//";
					string regs = "CCC/55555555555555;6666666666;/";
					var cfiaFieldsValue = ",ABCDEF                    ," + lpcos + " ," + regs + "              ,987                ,654,US,AL";
					var pgaIndicatorValues = ",Y" + cfiaFieldsValue + ",Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y";
					sw.WriteLine("Code,Description,UQ,ImportTariff,Owner,VFDCode,Tariff99Code,AuthorityNum,TRSNum,TariffTreatment,GSTCode,ExciseExemptCode"
						+ cfiaIndicators + cfiaFields + cnscIndicators + dfoIndicators + ecccIndicators + gacIndicators + hcIndicators + nrcanIndicators + phacIndicators + tcIndicators);
					sw.WriteLine("P099,DESC       ,KG,1234567890  ,ORG01,13     ,9901        ,1           ,2     ,9              ,50     ,85              "
						+ pgaIndicatorValues);
				}
				var dataLoad = new CAOrgSupplierPartDataLoad();
				dataLoad.ImportProductData(tempFile.Filename, false, false);
				Factory.Save();
				AssertEquals("Products Created", 1, dataLoad.RunCounters.RecsCreated);
				var product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P099", testOrganisation, null);
				AssertNotNull("Product", product);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				var importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot);
				AssertEquals("VFDCode", "13", importPivot.CCA_ValueForDutyCode);
				AssertEquals("TariffTreatment", "09", importPivot.CCA_TreatmentCode);
				AssertEquals("Tariff99Code", "9901", importPivot.CCA_99TariffCode);
				AssertEquals("AuthorityNum", "1", importPivot.CCA_AuthorityNumber);
				AssertEquals("TRSNum", "2", importPivot.CCA_TRSNumber);
				AssertEquals("GSTCode", "50", importPivot.CCA_GSTStatusCode);
				AssertEquals("ExciseExemptCode", "85", importPivot.CCA_ETExemption);

				AssertEquals("CFIAIndicator", YesNoList.Codes.Yes, importPivot.CCA_CFIAIndicator);
				AssertEquals("Source Country/region", "US", importPivot.CCA_RN_NKSource);
				AssertEquals("Source State", "AL", importPivot.CCA_StateOfSource);
				AssertEquals("CNSCIndicator", YesNoList.Codes.Yes, importPivot.CCA_CNSCIndicator);
				AssertEquals("DFOIndicator", YesNoList.Codes.Yes, importPivot.CCA_DFOIndicator);
				AssertEquals("ECCCIndicator", YesNoList.Codes.Yes, importPivot.CCA_ECCCIndicator);
				AssertEquals("GACIndicator", YesNoList.Codes.Yes, importPivot.CCA_GACIndicator);
				AssertEquals("HCIndicator", YesNoList.Codes.Yes, importPivot.CCA_HCIndicator);
				AssertEquals("NRCanIndicator", YesNoList.Codes.Yes, importPivot.CCA_NRCanIndicator);
				AssertEquals("PHACIndicator", YesNoList.Codes.Yes, importPivot.CCA_PHACIndicator);
				AssertEquals("TCIndicator", YesNoList.Codes.Yes, importPivot.CCA_TCIndicator);

				var cfia = importPivot.CFIAPGAHeader;
				AssertEquals(YesNoList.Codes.Yes, cfia.CA_AllProgramInd);
				AssertEquals("AIRSExtensionCode", "ABCDEF", cfia.CA_AIRSExtensionCode);
				AssertEquals("LPCOs Count", 3, cfia.LPCOViews.Count);

				var lpco1 = cfia.LPCOViews[0];
				AssertEquals("LPCO Document Type", "AAAA", lpco1.CLP_Type);
				AssertEquals("LPCO Ref No.", "11111111111111", lpco1.CLP_RefNo);
				AssertEquals("LPCO DIF URN", "22222222222222", lpco1.CLP_DIFRefNumberOrLocation);
				AssertEquals("Regs Count", 2, cfia.AIRSRegistrationNumbers.Count);

				var lpco2 = cfia.LPCOViews[1];
				AssertEquals("LPCO Document Type", "BBBB", lpco2.CLP_Type);
				AssertEquals("LPCO Ref No.", "44444444444444", lpco2.CLP_RefNo);
				AssertEquals("LPCO DIF URN", ZString.Empty, lpco2.CLP_DIFRefNumberOrLocation);
				AssertEquals("Regs Count", 2, cfia.AIRSRegistrationNumbers.Count);

				var lpco3 = cfia.LPCOViews[2];
				AssertEquals("LPCO Document Type", ZString.Empty, lpco3.CLP_Type);
				AssertEquals("LPCO Ref No.", ZString.Empty, lpco3.CLP_RefNo);
				AssertEquals("LPCO DIF URN", ZString.Empty, lpco3.CLP_DIFRefNumberOrLocation);
				AssertEquals("Regs Count", 2, cfia.AIRSRegistrationNumbers.Count);

				var reg1 = cfia.AIRSRegistrationNumbers[0];
				AssertEquals("Reg Type", "CCC", reg1.CY_Code);
				AssertEquals("Reg No.", "55555555555555", reg1.CY_Data);
				AssertEquals("AIRSEndUse", "987", cfia.CA_AIRSEndUse);
				AssertEquals("AIRSMiscellaneous", "654", cfia.CA_AIRSMiscellaneous);

				var reg2 = cfia.AIRSRegistrationNumbers[1];
				AssertEquals("Reg Type", ZString.Empty, reg2.CY_Code);
				AssertEquals("Reg No.", ZString.Empty, reg2.CY_Data);
				AssertEquals("AIRSEndUse", "987", cfia.CA_AIRSEndUse);
				AssertEquals("AIRSMiscellaneous", "654", cfia.CA_AIRSMiscellaneous);

				AssertEquals(YesNoList.Codes.Yes, importPivot.CNSCPGAHeader.CA_AllProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.GACPGAHeader.CA_AllProgramInd);

				AssertEquals(YesNoList.Codes.Yes, importPivot.DFOPGAHeader.CA_ABIProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.DFOPGAHeader.CA_AISProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.DFOPGAHeader.CA_TTPProgramInd);

				AssertEquals(YesNoList.Codes.Yes, importPivot.ECCCPGAHeader.CA_ODSProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.ECCCPGAHeader.CA_WENProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.ECCCPGAHeader.CA_WRMProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.ECCCPGAHeader.CA_VEEProgramInd);

				AssertEquals(YesNoList.Codes.Yes, importPivot.HCPGAHeader.CA_APIProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.HCPGAHeader.CA_BBCProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.HCPGAHeader.CA_CTOProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.HCPGAHeader.CA_CPRProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.HCPGAHeader.CA_DSEProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.HCPGAHeader.CA_HDRProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.HCPGAHeader.CA_OCSProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.HCPGAHeader.CA_MDEProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.HCPGAHeader.CA_NHPProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.HCPGAHeader.CA_PESProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.HCPGAHeader.CA_REDProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.HCPGAHeader.CA_VETProgramInd);

				AssertEquals(YesNoList.Codes.Yes, importPivot.NRCanPGAHeader.CA_EEFProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.NRCanPGAHeader.CA_EXPProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.NRCanPGAHeader.CA_RDAProgramInd);

				AssertEquals(YesNoList.Codes.Yes, importPivot.PHACPGAHeader.CA_HAPProgramInd);

				AssertEquals(YesNoList.Codes.Yes, importPivot.TCPGAHeader.CA_TPRProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.TCPGAHeader.CA_VPRProgramInd);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var cfiaIndicators = ",PGA_CFIA_Indicator";
					var cfiaFields = ",PGA_CFIA_AIRSExtensionCode,PGA_CFIA_LPCOs,PGA_CFIA_AIRSRegistrations,PGA_CFIA_AIRSEndUse,PGA_CFIA_AIRSMiscellaneous,PGA_CFIA_SourceCountry,PGA_CFIA_SourceState";
					var cnscIndicators = ",PGA_CNSC_Indicator";
					var dfoIndicators = ",PGA_DFO_ABIInd,PGA_DFO_AISInd,PGA_DFO_TTPInd";
					var ecccIndicators = ",PGA_ECCC_WRMInd,PGA_ECCC_ODSInd,PGA_ECCC_WENInd,PGA_ECCC_VEEInd";
					var gacIndicators = ",PGA_GAC_Indicator";
					var hcIndicators = ",PGA_HC_APIInd,PGA_HC_BBCInd,PGA_HC_CTOInd,PGA_HC_CPRInd,PGA_HC_DSEInd,PGA_HC_HDRInd,PGA_HC_OCSInd,PGA_HC_MDEInd,PGA_HC_NHPInd,PGA_HC_PESInd,PGA_HC_REDInd,PGA_HC_VETInd";
					var nrcanIndicators = ",PGA_NRCan_EEFInd,PGA_NRCan_EXPInd,PGA_NRCan_RDAInd";
					var phacIndicators = ",PGA_PHAC_HAPInd";
					var tcIndicators = ",PGA_TC_TPRInd,PGA_TC_VPRInd";

					string lpcos = "AAAA/11111111111111/22222222222222;BBBB/44444444444444;//";
					string regs = "CCC/55555555555555;6666666666;/";
					var cfiaFieldsValue = ",ABCDEF                    ," + lpcos + " ," + regs + "              ,987                ,654,US,AL";
					var pgaIndicatorValues = "," + cfiaFieldsValue + ",,,,,,,,,,,,,,,,,,,,,,,,,,,";
					sw.WriteLine("Code,Description,UQ,ImportTariff,Owner,VFDCode,Tariff99Code,AuthorityNum,TRSNum,TariffTreatment,GSTCode,ExciseExemptCode"
						+ cfiaIndicators + cfiaFields + cnscIndicators + dfoIndicators + ecccIndicators + gacIndicators + hcIndicators + nrcanIndicators + phacIndicators + tcIndicators);
					sw.WriteLine("P099,DESC       ,KG,1234567890  ,ORG01,13     ,9901        ,1           ,2     ,9              ,50     ,85              "
						+ pgaIndicatorValues);
				}
				var dataLoad2 = new CAOrgSupplierPartDataLoad();
				dataLoad2.ImportProductData(tempFile.Filename, true, false);
				Factory.Save();
				AssertEquals("Products Created", 1, dataLoad2.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull("Product", product);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot);
				AssertEquals("VFDCode", "13", importPivot.CCA_ValueForDutyCode);
				AssertEquals("TariffTreatment", "09", importPivot.CCA_TreatmentCode);
				AssertEquals("Tariff99Code", "9901", importPivot.CCA_99TariffCode);
				AssertEquals("AuthorityNum", "1", importPivot.CCA_AuthorityNumber);
				AssertEquals("TRSNum", "2", importPivot.CCA_TRSNumber);
				AssertEquals("GSTCode", "50", importPivot.CCA_GSTStatusCode);
				AssertEquals("ExciseExemptCode", "85", importPivot.CCA_ETExemption);

				AssertEquals("CFIAIndicator", YesNoList.Codes.Yes, importPivot.CCA_CFIAIndicator);
				AssertEquals("Source Country", "US", importPivot.CCA_RN_NKSource);
				AssertEquals("Source State", "AL", importPivot.CCA_StateOfSource);
				AssertEquals("CNSCIndicator", YesNoList.Codes.Yes, importPivot.CCA_CNSCIndicator);
				AssertEquals("DFOIndicator", YesNoList.Codes.Yes, importPivot.CCA_DFOIndicator);
				AssertEquals("ECCCIndicator", YesNoList.Codes.Yes, importPivot.CCA_ECCCIndicator);
				AssertEquals("GACIndicator", YesNoList.Codes.Yes, importPivot.CCA_GACIndicator);
				AssertEquals("HCIndicator", YesNoList.Codes.Yes, importPivot.CCA_HCIndicator);
				AssertEquals("NRCanIndicator", YesNoList.Codes.Yes, importPivot.CCA_NRCanIndicator);
				AssertEquals("PHACIndicator", YesNoList.Codes.Yes, importPivot.CCA_PHACIndicator);
				AssertEquals("TCIndicator", YesNoList.Codes.Yes, importPivot.CCA_TCIndicator);

				cfia = importPivot.CFIAPGAHeader;
				AssertEquals(YesNoList.Codes.Yes, cfia.CA_AllProgramInd);
				AssertEquals("AIRSExtensionCode", "ABCDEF", cfia.CA_AIRSExtensionCode);
				AssertEquals("LPCOs Count", 3, cfia.LPCOViews.Count);

				lpco1 = cfia.LPCOViews[0];
				AssertEquals("LPCO Document Type", "AAAA", lpco1.CLP_Type);
				AssertEquals("LPCO Ref No.", "11111111111111", lpco1.CLP_RefNo);
				AssertEquals("LPCO DIF URN", "22222222222222", lpco1.CLP_DIFRefNumberOrLocation);
				AssertEquals("Regs Count", 2, cfia.AIRSRegistrationNumbers.Count);

				lpco2 = cfia.LPCOViews[1];
				AssertEquals("LPCO Document Type", "BBBB", lpco2.CLP_Type);
				AssertEquals("LPCO Ref No.", "44444444444444", lpco2.CLP_RefNo);
				AssertEquals("LPCO DIF URN", ZString.Empty, lpco2.CLP_DIFRefNumberOrLocation);
				AssertEquals("Regs Count", 2, cfia.AIRSRegistrationNumbers.Count);

				lpco3 = cfia.LPCOViews[2];
				AssertEquals("LPCO Document Type", ZString.Empty, lpco3.CLP_Type);
				AssertEquals("LPCO Ref No.", ZString.Empty, lpco3.CLP_RefNo);
				AssertEquals("LPCO DIF URN", ZString.Empty, lpco3.CLP_DIFRefNumberOrLocation);
				AssertEquals("Regs Count", 2, cfia.AIRSRegistrationNumbers.Count);

				reg1 = cfia.AIRSRegistrationNumbers[0];
				AssertEquals("Reg Type", "CCC", reg1.CY_Code);
				AssertEquals("Reg No.", "55555555555555", reg1.CY_Data);
				AssertEquals("AIRSEndUse", "987", cfia.CA_AIRSEndUse);
				AssertEquals("AIRSMiscellaneous", "654", cfia.CA_AIRSMiscellaneous);

				AssertEquals(YesNoList.Codes.Yes, importPivot.CNSCPGAHeader.CA_AllProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.GACPGAHeader.CA_AllProgramInd);

				AssertEquals(YesNoList.Codes.Yes, importPivot.DFOPGAHeader.CA_ABIProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.DFOPGAHeader.CA_AISProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.DFOPGAHeader.CA_TTPProgramInd);

				AssertEquals(YesNoList.Codes.Yes, importPivot.ECCCPGAHeader.CA_ODSProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.ECCCPGAHeader.CA_WENProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.ECCCPGAHeader.CA_WRMProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.ECCCPGAHeader.CA_VEEProgramInd);

				AssertEquals(YesNoList.Codes.Yes, importPivot.HCPGAHeader.CA_APIProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.HCPGAHeader.CA_BBCProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.HCPGAHeader.CA_CTOProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.HCPGAHeader.CA_CPRProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.HCPGAHeader.CA_DSEProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.HCPGAHeader.CA_HDRProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.HCPGAHeader.CA_OCSProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.HCPGAHeader.CA_MDEProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.HCPGAHeader.CA_NHPProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.HCPGAHeader.CA_PESProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.HCPGAHeader.CA_REDProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.HCPGAHeader.CA_VETProgramInd);

				AssertEquals(YesNoList.Codes.Yes, importPivot.NRCanPGAHeader.CA_EEFProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.NRCanPGAHeader.CA_EXPProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.NRCanPGAHeader.CA_RDAProgramInd);

				AssertEquals(YesNoList.Codes.Yes, importPivot.PHACPGAHeader.CA_HAPProgramInd);

				AssertEquals(YesNoList.Codes.Yes, importPivot.TCPGAHeader.CA_TPRProgramInd);
				AssertEquals(YesNoList.Codes.Yes, importPivot.TCPGAHeader.CA_VPRProgramInd);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var cfiaIndicators = ",PGA_CFIA_Indicator";
					var cfiaFields = ",PGA_CFIA_AIRSExtensionCode,PGA_CFIA_LPCOs,PGA_CFIA_AIRSRegistrations,PGA_CFIA_AIRSEndUse,PGA_CFIA_AIRSMiscellaneous,PGA_CFIA_SourceCountry,PGA_CFIA_SourceState";
					var cnscIndicators = ",PGA_CNSC_Indicator";
					var dfoIndicators = ",PGA_DFO_ABIInd,PGA_DFO_AISInd,PGA_DFO_TTPInd";
					var ecccIndicators = ",PGA_ECCC_WRMInd,PGA_ECCC_ODSInd,PGA_ECCC_WENInd,PGA_ECCC_VEEInd";
					var gacIndicators = ",PGA_GAC_Indicator";
					var hcIndicators = ",PGA_HC_APIInd,PGA_HC_BBCInd,PGA_HC_CTOInd,PGA_HC_CPRInd,PGA_HC_DSEInd,PGA_HC_HDRInd,PGA_HC_OCSInd,PGA_HC_MDEInd,PGA_HC_NHPInd,PGA_HC_PESInd,PGA_HC_REDInd,PGA_HC_VETInd";
					var nrcanIndicators = ",PGA_NRCan_EEFInd,PGA_NRCan_EXPInd,PGA_NRCan_RDAInd";
					var phacIndicators = ",PGA_PHAC_HAPInd";
					var tcIndicators = ",PGA_TC_TPRInd,PGA_TC_VPRInd";

					string lpcos = "AAAA/11111111111111/22222222222222;BBBB/44444444444444;//";
					string regs = "CCC/55555555555555;6666666666;/";
					var cfiaFieldsValue = ",ABCDEF                    ," + lpcos + " ," + regs + "              ,987                ,654,US,AL";
					var pgaIndicatorValues = ",N" + cfiaFieldsValue + ",N,N,N,N,N,N,N,N,N,N,N,N,N,N,N,N,N,N,N,N,N,N,N,N,N,N,N";
					sw.WriteLine("Code,Description,UQ,ImportTariff,Owner,VFDCode,Tariff99Code,AuthorityNum,TRSNum,TariffTreatment,GSTCode,ExciseExemptCode"
						+ cfiaIndicators + cfiaFields + cnscIndicators + dfoIndicators + ecccIndicators + gacIndicators + hcIndicators + nrcanIndicators + phacIndicators + tcIndicators);
					sw.WriteLine("P099,DESC       ,KG,1234567890  ,ORG01,13     ,9901        ,1           ,2     ,9              ,50     ,85              "
						+ pgaIndicatorValues);
				}
				var dataLoad3 = new CAOrgSupplierPartDataLoad();
				dataLoad3.ImportProductData(tempFile.Filename, true, false);
				Factory.Save();
				AssertEquals("Products Created", 1, dataLoad3.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull("Product", product);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot);
				AssertEquals("VFDCode", "13", importPivot.CCA_ValueForDutyCode);
				AssertEquals("TariffTreatment", "09", importPivot.CCA_TreatmentCode);
				AssertEquals("Tariff99Code", "9901", importPivot.CCA_99TariffCode);
				AssertEquals("AuthorityNum", "1", importPivot.CCA_AuthorityNumber);
				AssertEquals("TRSNum", "2", importPivot.CCA_TRSNumber);
				AssertEquals("GSTCode", "50", importPivot.CCA_GSTStatusCode);
				AssertEquals("ExciseExemptCode", "85", importPivot.CCA_ETExemption);

				AssertEquals("CFIAIndicator", YesNoList.Codes.No, importPivot.CCA_CFIAIndicator);
				AssertEquals("Source Country", "US", importPivot.CCA_RN_NKSource);
				AssertEquals("Source State", "AL", importPivot.CCA_StateOfSource);
				AssertEquals("CNSCIndicator", YesNoList.Codes.No, importPivot.CCA_CNSCIndicator);
				AssertEquals("DFOIndicator", YesNoList.Codes.No, importPivot.CCA_DFOIndicator);
				AssertEquals("ECCCIndicator", YesNoList.Codes.No, importPivot.CCA_ECCCIndicator);
				AssertEquals("GACIndicator", YesNoList.Codes.No, importPivot.CCA_GACIndicator);
				AssertEquals("HCIndicator", YesNoList.Codes.No, importPivot.CCA_HCIndicator);
				AssertEquals("NRCanIndicator", YesNoList.Codes.No, importPivot.CCA_NRCanIndicator);
				AssertEquals("PHACIndicator", YesNoList.Codes.No, importPivot.CCA_PHACIndicator);
				AssertEquals("TCIndicator", YesNoList.Codes.No, importPivot.CCA_TCIndicator);

				cfia = importPivot.CFIAPGAHeader;
				AssertEquals(YesNoList.Codes.No, cfia.CA_AllProgramInd);
				AssertEquals("AIRSExtensionCode", ZString.Empty, cfia.CA_AIRSExtensionCode);
				AssertEquals("LPCOs Count", 0, cfia.LPCOViews.Count);

				AssertEquals(YesNoList.Codes.No, importPivot.CNSCPGAHeader.CA_AllProgramInd);
				AssertEquals(YesNoList.Codes.No, importPivot.GACPGAHeader.CA_AllProgramInd);

				AssertEquals(YesNoList.Codes.No, importPivot.DFOPGAHeader.CA_ABIProgramInd);
				AssertEquals(YesNoList.Codes.No, importPivot.DFOPGAHeader.CA_AISProgramInd);
				AssertEquals(YesNoList.Codes.No, importPivot.DFOPGAHeader.CA_TTPProgramInd);

				AssertEquals(YesNoList.Codes.No, importPivot.ECCCPGAHeader.CA_ODSProgramInd);
				AssertEquals(YesNoList.Codes.No, importPivot.ECCCPGAHeader.CA_WENProgramInd);
				AssertEquals(YesNoList.Codes.No, importPivot.ECCCPGAHeader.CA_WRMProgramInd);
				AssertEquals(YesNoList.Codes.No, importPivot.ECCCPGAHeader.CA_VEEProgramInd);

				AssertEquals(YesNoList.Codes.No, importPivot.HCPGAHeader.CA_APIProgramInd);
				AssertEquals(YesNoList.Codes.No, importPivot.HCPGAHeader.CA_BBCProgramInd);
				AssertEquals(YesNoList.Codes.No, importPivot.HCPGAHeader.CA_CTOProgramInd);
				AssertEquals(YesNoList.Codes.No, importPivot.HCPGAHeader.CA_CPRProgramInd);
				AssertEquals(YesNoList.Codes.No, importPivot.HCPGAHeader.CA_DSEProgramInd);
				AssertEquals(YesNoList.Codes.No, importPivot.HCPGAHeader.CA_HDRProgramInd);
				AssertEquals(YesNoList.Codes.No, importPivot.HCPGAHeader.CA_OCSProgramInd);
				AssertEquals(YesNoList.Codes.No, importPivot.HCPGAHeader.CA_MDEProgramInd);
				AssertEquals(YesNoList.Codes.No, importPivot.HCPGAHeader.CA_NHPProgramInd);
				AssertEquals(YesNoList.Codes.No, importPivot.HCPGAHeader.CA_PESProgramInd);
				AssertEquals(YesNoList.Codes.No, importPivot.HCPGAHeader.CA_REDProgramInd);
				AssertEquals(YesNoList.Codes.No, importPivot.HCPGAHeader.CA_VETProgramInd);

				AssertEquals(YesNoList.Codes.No, importPivot.NRCanPGAHeader.CA_EEFProgramInd);
				AssertEquals(YesNoList.Codes.No, importPivot.NRCanPGAHeader.CA_EXPProgramInd);
				AssertEquals(YesNoList.Codes.No, importPivot.NRCanPGAHeader.CA_RDAProgramInd);

				AssertEquals(YesNoList.Codes.No, importPivot.PHACPGAHeader.CA_HAPProgramInd);

				AssertEquals(YesNoList.Codes.No, importPivot.TCPGAHeader.CA_TPRProgramInd);
				AssertEquals(YesNoList.Codes.No, importPivot.TCPGAHeader.CA_VPRProgramInd);
			}
		}

		public void TestImportingHCCommonData()
		{
			Factory.Save();
			var dataLoad = new CAOrgSupplierPartDataLoad();
			using (TempFile tempFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ImportTariff,Owner,VFDCode,Tariff99Code,AuthorityNum,TRSNum,TariffTreatment,GSTCode,ExciseExemptCode" +
						",PGA_HC_CPRInd,PGA_HC_GTINNumber,PGA_HC_BatchLotNumber");
					sw.WriteLine("P098,DESC       ,KG,1234567890  ,ORG01,13     ,9901        ,1           ,2     ,9              ,50     ,85              " +
						",Y,TEST,TEST2");
				}
				dataLoad.ImportProductData(tempFile.Filename, false, false);
				Factory.Save();
				AssertEquals("Products Created", 1, dataLoad.RunCounters.RecsCreated);
				var product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P098", testOrganisation, null);
				AssertNotNull("Product", product);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				var importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot);
				AssertEquals("VFDCode", "13", importPivot.CCA_ValueForDutyCode);
				AssertEquals("TariffTreatment", "09", importPivot.CCA_TreatmentCode);
				AssertEquals("Tariff99Code", "9901", importPivot.CCA_99TariffCode);
				AssertEquals("AuthorityNum", "1", importPivot.CCA_AuthorityNumber);
				AssertEquals("TRSNum", "2", importPivot.CCA_TRSNumber);
				AssertEquals("GSTCode", "50", importPivot.CCA_GSTStatusCode);
				AssertEquals("ExciseExemptCode", "85", importPivot.CCA_ETExemption);

				AssertEquals("HCIndicator", YesNoList.Codes.Yes, importPivot.CCA_HCIndicator);

				var hc = importPivot.HCPGAHeader;
				AssertEquals(YesNoList.Codes.Yes, hc.CA_CPRProgramInd);
				AssertEquals("PGA_HC_GTINNumber", "TEST", hc.CA_GTINNumber);
				AssertEquals("PGA_HC_BatchLotNumber", "TEST2", hc.CA_BatchLotNumber);
			}
		}

		public void TestImportingMoreOGDData()
		{
			Factory.Save();
			var dataLoad = new CAOrgSupplierPartDataLoad();
			using (TempFile tempFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ImportTariff,Owner,ImportReason,ProductModel,ProductNum,ProductBrand,TypeOrSize,TireImporterID,TireCompliantCompletion,TireImportDateCompliant");
					sw.WriteLine("P099,DESC       ,KG,1234567890  ,ORG01,10          ,PM          ,PN        ,PB          ,TS        ,TID           ,Y                      ,Yes");
					sw.WriteLine("P098,DESC       ,KG,1234567890  ,ORG01,10          ,PM          ,PN        ,PB          ,TS        ,TID           ,Yuck                   ,N");
				}
				dataLoad.ImportProductData(tempFile.Filename, false, false);
				Factory.Save();
				AssertEquals("Products Created", 2, dataLoad.RunCounters.RecsCreated);
				var product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P099", testOrganisation, null);
				AssertNotNull("Product", product);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				var importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot);
				AssertEquals("ImportReason", "10", importPivot.CCA_ImportReasonCode);
				AssertEquals("ProductModel", "PM", importPivot.CCA_Model);
				AssertEquals("ProductNum", "PN", importPivot.CCA_ModelNumber);
				AssertEquals("ProductBrand", "PB", importPivot.CCA_BrandName);
				AssertEquals("TypeOrSize", "TS", importPivot.CCA_TypeSize);
				AssertEquals("TireImporterID", "TID", importPivot.CCA_TIIN);
				Assert("TireCompliantCompletion", importPivot.CCA_CompliantCompletion);
				Assert("TireImportDateCompliant", importPivot.CCA_CompliantImportDateIndicator);
				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P098", testOrganisation, null);
				AssertNotNull("Product", product);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				Assert("TireCompliantCompletion", !importPivot.CCA_CompliantCompletion);
				Assert("TireImportDateCompliant", !importPivot.CCA_CompliantImportDateIndicator);
			}
		}

		public void TestImportingMultipleCFIARegNums()
		{
			Factory.Save();
			using (TempFile tempFile1 = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(tempFile1.Filename))
				{
					string lpcos = "AAAA/11111111111111/22222222222222;BBBB/44444444444444;//";
					string regs = "CCC/55555555555555;6666666666;/";
					sw.WriteLine("Code,Description,UQ,ImportTariff,Owner,PGA_CFIA_Indicator,PGA_CFIA_AIRSExtensionCode,PGA_CFIA_LPCOs,PGA_CFIA_AIRSRegistrations,PGA_CFIA_AIRSEndUse,PGA_CFIA_AIRSMiscellaneous");
					sw.WriteLine("P099,DESC       ,KG,1234567890  ,ORG01");
					sw.WriteLine("P098,DESC       ,KG,1234567890  ,ORG01,Y                 ,ABCDEF                    ," + lpcos + " ," + regs + "              ,987                ,654");
					sw.WriteLine("P097,DESC       ,KG,1234567890  ,ORG01");
				}
				var dataLoad = new CAOrgSupplierPartDataLoad();
				dataLoad.ImportProductData(tempFile1.Filename, false, false);
				Factory.Save();
				AssertEquals("Products Created", 3, dataLoad.RunCounters.RecsCreated);
				var product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P099", testOrganisation, null);
				AssertNotNull("Product created", product);
				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P098", testOrganisation, null);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				var importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot);
				AssertEquals("CFIAIndicator", YesNoList.Codes.Yes, importPivot.CCA_CFIAIndicator);
				var cfia = importPivot.CFIAPGAHeader;
				AssertEquals("AIRSExtensionCode", "ABCDEF", cfia.CA_AIRSExtensionCode);
				AssertEquals("LPCOs Count", 3, cfia.LPCOViews.Count);

				var lpco1 = cfia.LPCOViews[0];
				AssertEquals("LPCO Document Type", "AAAA", lpco1.CLP_Type);
				AssertEquals("LPCO Ref No.", "11111111111111", lpco1.CLP_RefNo);
				AssertEquals("LPCO DIF URN", "22222222222222", lpco1.CLP_DIFRefNumberOrLocation);
				AssertEquals("Regs Count", 2, cfia.AIRSRegistrationNumbers.Count);

				var lpco2 = cfia.LPCOViews[1];
				AssertEquals("LPCO Document Type", "BBBB", lpco2.CLP_Type);
				AssertEquals("LPCO Ref No.", "44444444444444", lpco2.CLP_RefNo);
				AssertEquals("LPCO DIF URN", ZString.Empty, lpco2.CLP_DIFRefNumberOrLocation);
				AssertEquals("Regs Count", 2, cfia.AIRSRegistrationNumbers.Count);

				var lpco3 = cfia.LPCOViews[2];
				AssertEquals("LPCO Document Type", ZString.Empty, lpco3.CLP_Type);
				AssertEquals("LPCO Ref No.", ZString.Empty, lpco3.CLP_RefNo);
				AssertEquals("LPCO DIF URN", ZString.Empty, lpco3.CLP_DIFRefNumberOrLocation);
				AssertEquals("Regs Count", 2, cfia.AIRSRegistrationNumbers.Count);

				var reg1 = cfia.AIRSRegistrationNumbers[0];
				AssertEquals("Reg Type", "CCC", reg1.CY_Code);
				AssertEquals("Reg No.", "55555555555555", reg1.CY_Data);
				AssertEquals("AIRSEndUse", "987", cfia.CA_AIRSEndUse);
				AssertEquals("AIRSMiscellaneous", "654", cfia.CA_AIRSMiscellaneous);

				var reg2 = cfia.AIRSRegistrationNumbers[1];
				AssertEquals("Reg Type", ZString.Empty, reg2.CY_Code);
				AssertEquals("Reg No.", ZString.Empty, reg2.CY_Data);
				AssertEquals("AIRSEndUse", "987", cfia.CA_AIRSEndUse);
				AssertEquals("AIRSMiscellaneous", "654", cfia.CA_AIRSMiscellaneous);

				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P097", testOrganisation, null);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot);
			}
			using (TempFile tempFile2 = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(tempFile2.Filename))
				{
					string lpcos = "EEEE/77777777777777/88888888888888;FFFF/99999999999999;//";
					string regs = "GGG/33333333333333;4444444444;/";
					sw.WriteLine("Code,Description,UQ,ImportTariff,Owner,PGA_CFIA_Indicator,PGA_CFIA_AIRSExtensionCode,PGA_CFIA_LPCOs,PGA_CFIA_AIRSRegistrations,PGA_CFIA_AIRSEndUse,PGA_CFIA_AIRSMiscellaneous");
					sw.WriteLine("P098,DESC       ,KG,1234567890  ,ORG01,Y                 ,GHIJKL                    ," + lpcos + " ," + regs + "              ,789                ,456");
				}
				var dataLoad2 = new CAOrgSupplierPartDataLoad();
				dataLoad2.ImportProductData(tempFile2.Filename, true, false);
				Factory.Save();
				AssertEquals("Products Updated", 1, dataLoad2.RunCounters.RecsUpdated);
				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				var product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(factory2, typeof(OrgSupplierPart)).Load("P098", testOrganisation, null);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				var importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot);

				AssertEquals("CFIAIndicator", YesNoList.Codes.Yes, importPivot.CCA_CFIAIndicator);
				var cfia = importPivot.CFIAPGAHeader;
				AssertEquals("AIRSExtensionCode", "GHIJKL", cfia.CA_AIRSExtensionCode);
				AssertEquals("LPCOs Count", 3, cfia.LPCOViews.Count);
				var lpco1 = cfia.LPCOViews[0];
				AssertEquals("LPCO Document Type", ZString.Empty, lpco1.CLP_Type);
				AssertEquals("LPCO Ref No.", ZString.Empty, lpco1.CLP_RefNo);
				AssertEquals("LPCO DIF URN", ZString.Empty, lpco1.CLP_DIFRefNumberOrLocation);
				AssertEquals("Regs Count", 2, cfia.AIRSRegistrationNumbers.Count);

				var lpco2 = cfia.LPCOViews[1];
				AssertEquals("LPCO Document Type", "EEEE", lpco2.CLP_Type);
				AssertEquals("LPCO Ref No.", "77777777777777", lpco2.CLP_RefNo);
				AssertEquals("LPCO DIF URN", "88888888888888", lpco2.CLP_DIFRefNumberOrLocation);
				AssertEquals("Regs Count", 2, cfia.AIRSRegistrationNumbers.Count);

				var lpco3 = cfia.LPCOViews[2];
				AssertEquals("LPCO Document Type", "FFFF", lpco3.CLP_Type);
				AssertEquals("LPCO Ref No.", "99999999999999", lpco3.CLP_RefNo);
				AssertEquals("LPCO DIF URN", ZString.Empty, lpco3.CLP_DIFRefNumberOrLocation);
				AssertEquals("Regs Count", 2, cfia.AIRSRegistrationNumbers.Count);

				var reg1 = cfia.AIRSRegistrationNumbers[0];
				AssertEquals("Reg Type", ZString.Empty, reg1.CY_Code);
				AssertEquals("Reg No.", ZString.Empty, reg1.CY_Data);
				AssertEquals("AIRSEndUse", "789", cfia.CA_AIRSEndUse);
				AssertEquals("AIRSMiscellaneous", "456", cfia.CA_AIRSMiscellaneous);

				var reg2 = cfia.AIRSRegistrationNumbers[1];
				AssertEquals("Reg Type", "GGG", reg2.CY_Code);
				AssertEquals("Reg No.", "33333333333333", reg2.CY_Data);
				AssertEquals("AIRSEndUse", "789", cfia.CA_AIRSEndUse);
				AssertEquals("AIRSMiscellaneous", "456", cfia.CA_AIRSMiscellaneous);
			}
		}

		public void TestImportingMultipleSITTCertNums()
		{
			Factory.Save();
			var dataLoad = new CAOrgSupplierPartDataLoad();
			using (TempFile tempFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ImportTariff,Owner,TireCompliantCompletion,TireImportDateCompliant,SITTCertNums");
					sw.WriteLine("P099,DESC       ,KG,1234567890  ,ORG01,YES                    ,N,111111");
					sw.WriteLine("P098,DESC       ,KG,1234567890  ,ORG01,N                      ,Y,111111;222222;333333;222222");
					sw.WriteLine("P097,DESC       ,KG,1234567890  ,ORG01,N                      ,Y,\"111111;222222,333333;222222\"");
				}
				dataLoad.ImportProductData(tempFile.Filename, false, false);
				Factory.Save();
				AssertEquals("Products Created", 3, dataLoad.RunCounters.RecsCreated);
				var product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P099", testOrganisation, null);
				AssertNotNull("Product", product);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				var importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot);
				Assert("TireCompliantCompletion", importPivot.CCA_CompliantCompletion);
				Assert("TireImportDateCompliant", !importPivot.CCA_CompliantImportDateIndicator);
				AssertEquals("1 SITT Cert Num", 1, importPivot.SITTCertificationNumbers.Count);
				AssertEquals("111111", "111111", importPivot.SITTCertificationNumbers[0].CY_Data);
				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P098", testOrganisation, null);
				AssertNotNull("Product", product);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				Assert("TireCompliantCompletion", !importPivot.CCA_CompliantCompletion);
				Assert("TireImportDateCompliant", importPivot.CCA_CompliantImportDateIndicator);
				AssertEquals("3 SITT Cert Nums", 3, importPivot.SITTCertificationNumbers.Count);
				Assert("Contains 111111", importPivot.SITTCertificationNumbers.ContainsNumber("111111"));
				Assert("Contains 222222", importPivot.SITTCertificationNumbers.ContainsNumber("222222"));
				Assert("Contains 333333", importPivot.SITTCertificationNumbers.ContainsNumber("333333"));
				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P097", testOrganisation, null);
				AssertNotNull("Product", product);
				AssertEquals("1 pivot", 1, product.PivotsForBinding.Count);
				importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				Assert("TireCompliantCompletion", !importPivot.CCA_CompliantCompletion);
				Assert("TireImportDateCompliant", importPivot.CCA_CompliantImportDateIndicator);
				AssertEquals("3 SITT Cert Nums", 3, importPivot.SITTCertificationNumbers.Count);
				Assert("Contains 111111", importPivot.SITTCertificationNumbers.ContainsNumber("111111"));
				Assert("Contains 222222", importPivot.SITTCertificationNumbers.ContainsNumber("222222"));
				Assert("Contains 333333", importPivot.SITTCertificationNumbers.ContainsNumber("333333"));
			}
		}

		public void TestImportingAddInfoFields()
		{
			var dataLoad = new CAOrgSupplierPartDataLoad();
			using (var tempFile = TempFile.New())
			{
				using (var sw = new StreamWriter(tempFile.Filename))
				{
					var headings = new ZStringBuilder();
					for (var i = 0; i < exclusionList.Count; i++)
					{
						if (i != 0)
						{
							headings.Append(",");
						}
						headings.Append($"{exclusionList[i]}");
					}
					sw.WriteLine(headings);
				}
				dataLoad.ImportProductData(tempFile.Filename, false, false);
				AssertEquals(1, dataLoad.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, dataLoad.RunCounters.RecsCreated);
				AssertEquals(0, dataLoad.RunCounters.RecsUpdated);
				AssertEquals(0, dataLoad.RunCounters.RecsExcluded);
				AssertEquals("Products to Import = 0", dataLoad.Log[0]);

				for (var i = 0; i < exclusionList.Count; i++)
				{
					AssertEquals($"Unknown column heading : {exclusionList[i]}", dataLoad.Log[i + 1]);
				}
			}
		}

		public void TestImportingSIMAData()
		{
			#region Setup Universal Tariff

			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var chinaTradeGroup = universalHelper.CreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.China, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.AddCountry(chinaTradeGroup, Core.Constants.CountryCodes.China, ZDate.Today.AddYears(-1), ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var simaTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, DutyAndTaxManager.SIMATariffType);
			var antiDumpingRateType = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.AntiDumping);
			var antiDumpingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.ADD, antiDumpingRateType.PK);
			var surTaxRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.SUR, antiDumpingRateType.PK);
			var countervailingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.CVD, antiDumpingRateType.PK);
			Factory.Save();
			var surtaxTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "1234567890", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(surtaxTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KGM");
			var surTaxRate = universalHelper.CreateRate(surtaxTariff, surTaxRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "0.1 * VFD");
			universalHelper.CreateCusApplicability(surTaxRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var antiDumpingRelTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "1234567890", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(antiDumpingRelTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NMB");
			var antiDumpingTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1407", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, relatedTariffCode: "1234567890");
			universalHelper.CreateTariffRelationship(antiDumpingTariff.PK, harmonizedTariffType.PK, "1234567890");
			var antiDumpingRate = universalHelper.CreateRate(antiDumpingTariff, antiDumpingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "100.5 * [NMB]");
			universalHelper.CreateCusApplicability(antiDumpingRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var countervailingRate = universalHelper.CreateRate(antiDumpingTariff, countervailingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "200 * [KGM]");
			universalHelper.CreateCusApplicability(countervailingRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			Factory.Save();

			#endregion

			Factory.Save();
			var dataLoad = new CAOrgSupplierPartDataLoad();
			using (var tempFile = TempFile.New())
			{
				using (var sw = new StreamWriter(tempFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ImportTariff,Owner,Origin,SIMADumpingNum,SIMACode");
					sw.WriteLine("P099,DESC       ,KG,1234567890  ,ORG01,CN		,AD1407			,10");
					sw.WriteLine("P098,DESC       ,KG,9876543210  ,ORG01,CN		,				,10");
				}
				dataLoad.ImportProductData(tempFile.Filename, false, false);
				Factory.Save();
				AssertEquals("Products Created", 2, dataLoad.RunCounters.RecsCreated);

				var product1 = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P099", testOrganisation, null);
				AssertNotNull("Product", product1);
				AssertEquals("1 pivot", 1, product1.PivotsForBinding.Count);
				var importPivot1 = product1.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot1);
				AssertEquals("AD1407", importPivot1.CCA_SIMADumpingNumber);
				AssertEquals(3, importPivot1.DutiesAndTaxes.Count);
				AssertEquals("10", importPivot1.DutiesAndTaxes.OfType<DutyAndTax>().First().C1_ExemptCode);

				var product2 = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P098", testOrganisation, null);
				AssertNotNull("Product", product2);
				AssertEquals("1 pivot", 1, product2.PivotsForBinding.Count);
				var importPivot2 = product2.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot2);
				AssertEquals(ZString.Empty, importPivot2.CCA_SIMADumpingNumber);
				AssertEquals(0, importPivot2.DutiesAndTaxes.Count);
			}
		}

		public void TestImportingSIMADataWithoutDumpingCodesAndMultipleADDs()
		{
			#region Setup Universal Tariff
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var chinaTradeGroup = universalHelper.CreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.China, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.AddCountry(chinaTradeGroup, Core.Constants.CountryCodes.China, ZDate.Today.AddYears(-1), ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var simaTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, DutyAndTaxManager.SIMATariffType);
			var antiDumpingRateType = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.AntiDumping);
			var antiDumpingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.ADD, antiDumpingRateType.PK);
			var surTaxRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.SUR, antiDumpingRateType.PK);
			var countervailingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.CVD, antiDumpingRateType.PK);
			Factory.Save();
			var surtaxTariff4 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "5656677889", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(surtaxTariff4, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KGM");
			var surTaxRate4 = universalHelper.CreateRate(surtaxTariff4, surTaxRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "0.1 * VFD");
			universalHelper.CreateCusApplicability(surTaxRate4, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var antiDumpingRelTariff4 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "5656677889", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(antiDumpingRelTariff4, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NMB");
			var antiDumpingTariff4 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD5507", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, relatedTariffCode: "5656677889");
			universalHelper.CreateTariffRelationship(antiDumpingTariff4.PK, harmonizedTariffType.PK, "5656677889");
			var antiDumpingRate4 = universalHelper.CreateRate(antiDumpingTariff4, antiDumpingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "55.5 * [NMB]");
			universalHelper.CreateCusApplicability(antiDumpingRate4, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var countervailingRate4 = universalHelper.CreateRate(antiDumpingTariff4, countervailingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "22 * [KGM]");
			universalHelper.CreateCusApplicability(countervailingRate4, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);

			var surtaxTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "1234567890", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(surtaxTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KGM");
			var surTaxRate = universalHelper.CreateRate(surtaxTariff, surTaxRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "0.1 * VFD");
			universalHelper.CreateCusApplicability(surTaxRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var antiDumpingRelTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "1234567890", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(antiDumpingRelTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NMB");
			var antiDumpingTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1407", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, relatedTariffCode: "1234567890");
			universalHelper.CreateTariffRelationship(antiDumpingTariff.PK, harmonizedTariffType.PK, "1234567890");
			var antiDumpingRate = universalHelper.CreateRate(antiDumpingTariff, antiDumpingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "100.5 * [NMB]");
			universalHelper.CreateCusApplicability(antiDumpingRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var countervailingRate = universalHelper.CreateRate(antiDumpingTariff, countervailingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "200 * [KGM]");
			universalHelper.CreateCusApplicability(countervailingRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var antiDumpingTariff2 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1907", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, relatedTariffCode: "1234567890");
			universalHelper.CreateTariffRelationship(antiDumpingTariff2.PK, harmonizedTariffType.PK, "1234567890");
			var antiDumpingRate2 = universalHelper.CreateRate(antiDumpingTariff2, antiDumpingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "100.5 * [NMB]");
			universalHelper.CreateCusApplicability(antiDumpingRate2, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var countervailingRate2 = universalHelper.CreateRate(antiDumpingTariff2, countervailingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "200 * [KGM]");
			universalHelper.CreateCusApplicability(countervailingRate2, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);

			Factory.Save();
			#endregion

			Factory.Save();
			var dataLoad = new CAOrgSupplierPartDataLoad();
			using (var tempFile = TempFile.New())
			{
				using (var sw = new StreamWriter(tempFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ImportTariff,Owner,Origin,SIMADumpingNum,SIMACode");
					sw.WriteLine("P097,DESC7      ,KG,1234567890  ,ORG01,CN		,				,30");
					sw.WriteLine("P096,DESC6      ,KG,1234567890  ,ORG01,CN		,				,51");
					sw.WriteLine("P095,DESC6      ,KG,5656677889  ,ORG01,CN		,				,51");
				}
				dataLoad.ImportProductData(tempFile.Filename, false, false);
				Factory.Save();
				AssertEquals("Products Created", 3, dataLoad.RunCounters.RecsCreated);

				var product2 = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P095", testOrganisation, null);
				AssertNotNull("Product", product2);
				AssertEquals("1 pivot", 1, product2.PivotsForBinding.Count);
				var importPivot2 = product2.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot2);
				AssertEquals("Single ADDs should have SIMA Code 51", "51", importPivot2.DutiesAndTaxes.OfType<DutyAndTax>().First().C1_ExemptCode);

				var product4 = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P096", testOrganisation, null);
				AssertNotNull("Product", product4);
				AssertEquals("1 pivot", 1, product4.PivotsForBinding.Count);
				var importPivot4 = product4.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot4);
				AssertEquals(0, importPivot4.DutiesAndTaxes.Count);

				var product3 = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("P097", testOrganisation, null);
				AssertNotNull("Product", product3);
				AssertEquals("1 pivot", 1, product3.PivotsForBinding.Count);
				var importPivot3 = product3.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNotNull("Import pivot", importPivot3);
				AssertEquals(ZString.Empty, importPivot3.CCA_SIMADumpingNumber);
				AssertEquals(3, importPivot3.DutiesAndTaxes.Count);
				AssertEquals("30", importPivot3.DutiesAndTaxes.OfType<DutyAndTax>().First().C1_ExemptCode);
			}
		}

		public void TestImportPGADFO()
		{
			Factory.Save();

			using (TempFile tempFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var indFields = ",PGA_DFO_ABIInd,PGA_DFO_AISInd,PGA_DFO_TTPInd";
					var baseValues = "TEST1,TestProduct1,KG,0106120000,ORG01";
					var indValues = ",Y,Y,N";
					sw.WriteLine(baseFields + indFields);
					sw.WriteLine(baseValues + indValues);
				}

				var dataLoad = new CAOrgSupplierPartDataLoad();
				dataLoad.ImportProductData(tempFile.Filename, false, false);
				Factory.Save();

				AssertEquals("Products Created", 1, dataLoad.RunCounters.RecsCreated);

				var product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("TEST1", testOrganisation, null);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);

				var pivot = product.PivotsForBinding[0];
				AssertEquals("CCA_DFOIndicator", "Y", pivot.CCA_DFOIndicator);

				var dfo = pivot.DFOPGAHeader;
				AssertEquals("CA_ABIProgramInd", "Y", dfo.CA_ABIProgramInd);
				AssertEquals("CA_AISProgramInd", "Y", dfo.CA_AISProgramInd);
				AssertEquals("CA_TTPProgramInd", "N", dfo.CA_TTPProgramInd);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var indFields = ",PGA_DFO_ABIInd,PGA_DFO_AISInd,PGA_DFO_TTPInd";
					var baseValues = "TEST1,TestProduct1,KG,0106120000,ORG01";
					var indValues = ",,,";
					sw.WriteLine(baseFields + indFields);
					sw.WriteLine(baseValues + indValues);
				}

				var dataLoad2 = new CAOrgSupplierPartDataLoad();
				dataLoad2.ImportProductData(tempFile.Filename, true, false);
				Factory.Save();
				AssertEquals("Products updated", 1, dataLoad2.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);
				var pivot2 = product.PivotsForBinding[0];
				AssertEquals("pivot2.PK", pivot.PK, pivot2.PK);
				AssertEquals("CCA_DFOIndicator", "Y", pivot2.CCA_DFOIndicator);
				var dfo2 = pivot2.DFOPGAHeader;
				AssertEquals("dfo2.PK", dfo.PK, dfo2.PK);

				AssertEquals("CA_ABIProgramInd", "Y", dfo2.CA_ABIProgramInd);
				AssertEquals("CA_AISProgramInd", "Y", dfo2.CA_AISProgramInd);
				AssertEquals("CA_TTPProgramInd", "N", dfo2.CA_TTPProgramInd);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var indFields = ",PGA_DFO_ABIInd,PGA_DFO_AISInd,PGA_DFO_TTPInd";
					var baseValues = "TEST1,TestProduct1,KG,0106120000,ORG01";
					var indValues = ",N,N,N";
					sw.WriteLine(baseFields + indFields);
					sw.WriteLine(baseValues + indValues);
				}

				var dataLoad3 = new CAOrgSupplierPartDataLoad();
				dataLoad3.ImportProductData(tempFile.Filename, true, false);
				Factory.Save();
				AssertEquals("Products updated", 1, dataLoad3.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);
				pivot2 = product.PivotsForBinding[0];
				AssertEquals("pivot2.PK", pivot.PK, pivot2.PK);
				AssertEquals("CCA_DFOIndicator", "N", pivot2.CCA_DFOIndicator);
				var dfo3 = pivot2.DFOPGAHeader;
				AssertEquals("dfo3.PK", dfo.PK, dfo3.PK);

				AssertEquals("CA_ABIProgramInd", "N", dfo3.CA_ABIProgramInd);
				AssertEquals("CA_AISProgramInd", "N", dfo3.CA_AISProgramInd);
				AssertEquals("CA_TTPProgramInd", "N", dfo3.CA_TTPProgramInd);
			}
		}

		public void TestImportPGATC()
		{
			Factory.Save();

			using (TempFile tempFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var indFields = ",PGA_TC_TPRInd,PGA_TC_VPRInd";
					var baseValues = "TEST1,TestProduct1,KG,4011100000,ORG01";
					var indValues = ",Y,N";
					sw.WriteLine(baseFields + indFields);
					sw.WriteLine(baseValues + indValues);
				}

				var dataLoad = new CAOrgSupplierPartDataLoad();
				dataLoad.ImportProductData(tempFile.Filename, false, false);
				Factory.Save();

				AssertEquals("Products Created", 1, dataLoad.RunCounters.RecsCreated);

				var product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("TEST1", testOrganisation, null);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);

				var pivot = product.PivotsForBinding[0];
				AssertEquals("CCA_TCIndicator", "Y", pivot.CCA_TCIndicator);

				var tc = pivot.TCPGAHeader;
				AssertEquals("CA_TPRProgramInd", "Y", tc.CA_TPRProgramInd);
				AssertEquals("CA_VPRProgramInd", "N", tc.CA_VPRProgramInd);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var indFields = ",PGA_TC_TPRInd,PGA_TC_VPRInd";
					var baseValues = "TEST1,TestProduct1,KG,4011100000,ORG01";
					var indValues = ",,";
					sw.WriteLine(baseFields + indFields);
					sw.WriteLine(baseValues + indValues);
				}

				var dataLoad2 = new CAOrgSupplierPartDataLoad();
				dataLoad2.ImportProductData(tempFile.Filename, true, false);
				Factory.Save();
				AssertEquals("Products updated", 1, dataLoad2.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);
				var pivot2 = product.PivotsForBinding[0];
				AssertEquals("pivot.PK", pivot.PK, pivot2.PK);
				AssertEquals("CCA_TCIndicator", "Y", pivot2.CCA_TCIndicator);
				var tc2 = pivot2.TCPGAHeader;
				AssertEquals("tc2.PK", tc.PK, tc2.PK);

				AssertEquals("CA_TPRProgramInd", "Y", tc2.CA_TPRProgramInd);
				AssertEquals("CA_VPRProgramInd", "N", tc2.CA_VPRProgramInd);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var indFields = ",PGA_TC_TPRInd,PGA_TC_VPRInd";
					var baseValues = "TEST1,TestProduct1,KG,4011100000,ORG01";
					var indValues = ",N,N";
					sw.WriteLine(baseFields + indFields);
					sw.WriteLine(baseValues + indValues);
				}

				var dataLoad3 = new CAOrgSupplierPartDataLoad();
				dataLoad3.ImportProductData(tempFile.Filename, true, false);
				Factory.Save();
				AssertEquals("Products updated", 1, dataLoad3.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);
				pivot2 = product.PivotsForBinding[0];
				AssertEquals("pivot.PK", pivot.PK, pivot2.PK);
				AssertEquals("CCA_TCIndicator", "N", pivot2.CCA_TCIndicator);
				var tc3 = pivot2.TCPGAHeader;
				AssertEquals("tc3.PK", tc.PK, tc3.PK);

				AssertEquals("CA_TPRProgramInd", "N", tc3.CA_TPRProgramInd);
				AssertEquals("CA_VPRProgramInd", "N", tc3.CA_VPRProgramInd);
			}
		}

		public void TestImportPGANRCan()
		{
			Factory.Save();

			using (TempFile tempFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var indFields = ",PGA_NRCan_EEFInd,PGA_NRCan_EXPInd,PGA_NRCan_RDAInd";
					var baseValues = "TEST1,TestProduct1,KG,7321111000,ORG01";
					var indValues = ",Y,Y,N";
					sw.WriteLine(baseFields + indFields);
					sw.WriteLine(baseValues + indValues);
				}

				var dataLoad = new CAOrgSupplierPartDataLoad();
				dataLoad.ImportProductData(tempFile.Filename, false, false);
				Factory.Save();

				AssertEquals("Products Created", 1, dataLoad.RunCounters.RecsCreated);

				var product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("TEST1", testOrganisation, null);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);

				var pivot = product.PivotsForBinding[0];
				AssertEquals("CCA_NRCanIndicator", "Y", pivot.CCA_NRCanIndicator);

				var nRcan = pivot.NRCanPGAHeader;
				AssertEquals("CA_EEFProgramInd", "Y", nRcan.CA_EEFProgramInd);
				AssertEquals("CA_EXPProgramInd", "Y", nRcan.CA_EXPProgramInd);
				AssertEquals("CA_RDAProgramInd", "N", nRcan.CA_RDAProgramInd);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var indFields = ",PGA_NRCan_EEFInd,PGA_NRCan_EXPInd,PGA_NRCan_RDAInd";
					var baseValues = "TEST1,TestProduct1,KG,7321111000,ORG01";
					var indValues = ",,,";
					sw.WriteLine(baseFields + indFields);
					sw.WriteLine(baseValues + indValues);
				}

				var dataLoad2 = new CAOrgSupplierPartDataLoad();
				dataLoad2.ImportProductData(tempFile.Filename, true, false);
				Factory.Save();
				AssertEquals("Products updated", 1, dataLoad2.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);
				var pivot2 = product.PivotsForBinding[0];
				AssertEquals("pivot.PK", pivot.PK, pivot2.PK);
				AssertEquals("CCA_NRCanIndicator", "Y", pivot2.CCA_NRCanIndicator);
				var nRcan2 = pivot2.NRCanPGAHeader;
				AssertEquals("nRcan2.PK", nRcan.PK, nRcan2.PK);

				AssertEquals("CA_EEFProgramInd", "Y", nRcan2.CA_EEFProgramInd);
				AssertEquals("CA_EXPProgramInd", "Y", nRcan2.CA_EXPProgramInd);
				AssertEquals("CA_RDAProgramInd", "N", nRcan2.CA_RDAProgramInd);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var indFields = ",PGA_NRCan_EEFInd,PGA_NRCan_EXPInd,PGA_NRCan_RDAInd";
					var baseValues = "TEST1,TestProduct1,KG,7321111000,ORG01";
					var indValues = ",N,N,N";
					sw.WriteLine(baseFields + indFields);
					sw.WriteLine(baseValues + indValues);
				}

				var dataLoad3 = new CAOrgSupplierPartDataLoad();
				dataLoad3.ImportProductData(tempFile.Filename, true, false);
				Factory.Save();
				AssertEquals("Products updated", 1, dataLoad3.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);
				pivot2 = product.PivotsForBinding[0];
				AssertEquals("pivot2.PK", pivot.PK, pivot2.PK);
				AssertEquals("CCA_NRCanIndicator", "N", pivot2.CCA_NRCanIndicator);
				var nRcan3 = pivot2.NRCanPGAHeader;
				AssertEquals("nRcan3.PK", nRcan.PK, nRcan3.PK);

				AssertEquals("CA_EEFProgramInd", "N", nRcan3.CA_EEFProgramInd);
				AssertEquals("CA_EXPProgramInd", "N", nRcan3.CA_EXPProgramInd);
				AssertEquals("CA_RDAProgramInd", "N", nRcan3.CA_RDAProgramInd);
			}
		}

		public void TestImportPGACNSC()
		{
			Factory.Save();

			using (TempFile tempFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var cnscFileds = ",PGA_CNSC_Indicator,PGA_CNSC_Category,PGA_CNSC_NNIECRSchedulePartNo,PGA_CNSC_PackMarks,PGA_CNSC_LPCOs";
					var baseValues = "TEST1,TESTProduct1,KG,1234567890,ORG01";
					var cnscValues = ",Y,NE,TEST,TEST2,7000/NO123;7001/NO456";
					sw.WriteLine(baseFields + cnscFileds);
					sw.WriteLine(baseValues + cnscValues);
				}

				var dataLoad = new CAOrgSupplierPartDataLoad();
				dataLoad.ImportProductData(tempFile.Filename, false, false);
				Factory.Save();

				AssertEquals("products created", 1, dataLoad.RunCounters.RecsCreated);

				var product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("TEST1", testOrganisation, null);

				AssertNotNull(product);
				AssertEquals("pivot count", 1, product.PivotsForBinding.Count);

				var pivot = product.PivotsForBinding[0];
				AssertEquals("pivot.CCA_CNSCIndicator", "Y", pivot.CCA_CNSCIndicator);

				var cnsc = pivot.CNSCPGAHeader;
				var lpcoViews = cnsc.LPCOViews;
				AssertEquals("lpcos Count", 2, lpcoViews.Count);

				var lpco1 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "7000");
				var lpco2 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "7001");
				AssertPGACNSC(cnsc, lpco1, lpco2);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var cnscFileds = ",PGA_CNSC_Indicator,PGA_CNSC_Category,PGA_CNSC_NNIECRSchedulePartNo,PGA_CNSC_PackMarks";
					var baseValues = "TEST1,TESTProduct1,KG,1234567890,ORG01";
					var cnscValues = ",Y,NE,TEST,TEST2";
					sw.WriteLine(baseFields + cnscFileds);
					sw.WriteLine(baseValues + cnscValues);
				}

				var dataLoad2 = new CAOrgSupplierPartDataLoad();
				dataLoad2.ImportProductData(tempFile.Filename, true, false);
				Factory.Save();

				AssertEquals("products updated", 1, dataLoad2.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull(product);
				AssertEquals("pivot count", 1, product.PivotsForBinding.Count);
				var pivot2 = product.PivotsForBinding[0];
				AssertEquals("pivot2.PK", pivot.PK, pivot2.PK);
				AssertEquals("pivot2.CCA_CNSCIndicator", "Y", pivot2.CCA_CNSCIndicator);
				var cnsc2 = pivot2.CNSCPGAHeader;
				AssertEquals("cnsc2.PK", cnsc2.PK, cnsc2.PK);

				var lpcosView2 = cnsc2.LPCOViews;
				AssertEquals("lpcos2 count", 2, lpcosView2.Count);

				lpco1 = lpcosView2.Cast<LPCOView>().First(x => x.CLP_Type == "7000");
				lpco2 = lpcosView2.Cast<LPCOView>().First(x => x.CLP_Type == "7001");
				AssertPGACNSC(cnsc2, lpco1, lpco2);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var cnscFileds = ",PGA_CNSC_Indicator,PGA_CNSC_Category,PGA_CNSC_NNIECRSchedulePartNo,PGA_CNSC_PackMarks";
					var baseValues = "TEST1,TESTProduct1,KG,1234567890,ORG01";
					var cnscValues = ",,,,";
					sw.WriteLine(baseFields + cnscFileds);
					sw.WriteLine(baseValues + cnscValues);
				}

				var dataLoad3 = new CAOrgSupplierPartDataLoad();
				dataLoad3.ImportProductData(tempFile.Filename, true, false);
				Factory.Save();

				AssertEquals("products updated", 1, dataLoad3.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull(product);
				AssertEquals("pivot count", 1, product.PivotsForBinding.Count);
				var pivot3 = product.PivotsForBinding[0];
				AssertEquals("pivot3.PK", pivot.PK, pivot3.PK);
				AssertEquals("pivot3.CCA_CNSCIndicator", "Y", pivot3.CCA_CNSCIndicator);
				var cnsc3 = pivot3.CNSCPGAHeader;
				AssertEquals("cnsc3.PK", cnsc.PK, cnsc3.PK);

				var lpcosView3 = cnsc3.LPCOViews;
				AssertEquals("lpcos3 count", 2, lpcosView3.Count);

				lpco1 = lpcosView3.Cast<LPCOView>().First(x => x.CLP_Type == "7000");
				lpco2 = lpcosView3.Cast<LPCOView>().First(x => x.CLP_Type == "7001");
				AssertPGACNSC(cnsc3, lpco1, lpco2);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var cnscFileds = ",PGA_CNSC_Indicator,PGA_CNSC_Category,PGA_CNSC_NNIECRSchedulePartNo,PGA_CNSC_PackMarks,PGA_CNSC_LPCOs";
					var baseValues = "TEST1,TESTProduct1,KG,1234567890,ORG01";
					var cnscValues = ",N,NE,TEST,TEST2,7000/NO123;7001/NO456";
					sw.WriteLine(baseFields + cnscFileds);
					sw.WriteLine(baseValues + cnscValues);
				}

				var dataLoad4 = new CAOrgSupplierPartDataLoad();
				dataLoad4.ImportProductData(tempFile.Filename, true, false);
				Factory.Save();

				AssertEquals("products updated", 1, dataLoad4.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull(product);
				AssertEquals("pivot count", 1, product.PivotsForBinding.Count);
				var pivot4 = product.PivotsForBinding[0];
				AssertEquals("pivot4.PK", pivot.PK, pivot4.PK);
				AssertEquals("pivot4.CCA_CNSCIndicator", "N", pivot4.CCA_CNSCIndicator);
				var cnsc4 = pivot4.CNSCPGAHeader;
				AssertEquals("cnsc4.PK", cnsc.PK, cnsc4.PK);

				var lpcos4 = cnsc4.LPCOViews;
				AssertEquals("lpcos4 count", 0, lpcos4.Count);

				AssertEquals("CA_AllProgramInd", "N", cnsc4.CA_AllProgramInd);
				AssertEquals("CA_Category", ZString.Empty, cnsc4.CA_Category);
				AssertEquals("CA_NNIECRSchePartNo", ZString.Empty, cnsc4.CA_NNIECRSchePartNo);
				AssertEquals("CA_PackMarks", ZString.Empty, cnsc4.CA_PackMarks);
			}
		}

		void AssertPGACNSC(CNSCPGAHeader cnsc, LPCOView lpco1, LPCOView lpco2)
		{
			AssertEquals("CA_AllProgramInd", "Y", cnsc.CA_AllProgramInd);
			AssertEquals("CA_Category", "NE", cnsc.CA_Category);
			AssertEquals("CA_NNIECRSchePartNo", "TEST", cnsc.CA_NNIECRSchePartNo);
			AssertEquals("CA_PackMarks", "TEST2", cnsc.CA_PackMarks);

			AssertEquals("LPCO #1 CA_Type.", "7000", lpco1.CLP_Type);
			AssertEquals("LPCO #1 CA_RefNo.", "NO123", lpco1.CLP_RefNo);
			AssertEquals("LPCO #2 CA_Type.", "7001", lpco2.CLP_Type);
			AssertEquals("LPCO #2 CA_RefNo.", "NO456", lpco2.CLP_RefNo);
		}

		public void TestImportPGACNSCLPCOS()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "TEST1";
			part.OP_Desc = "TESTProduct1";
			var orgRel = part.RelatedOrganisations.AddOrganisationIfNotExist(testOrganisation.PK, OrgPartRelation.RelationshipTypes.Owner);

			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "LK234";
			classification.CC_TariffNum = "0101210000";

			var partPivot = Factory.New<CusClassPartPivot>();
			partPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			partPivot.CI_CC = classification.PK;
			partPivot.CI_OP = part.PK;
			partPivot.CCA_CNSCIndicator = YesNoList.Codes.Yes;

			var cnscHeader = partPivot.CNSCPGAHeader;
			cnscHeader.CA_AllProgramInd = YesNoList.Codes.Yes;
			cnscHeader.CA_Category = "NE";
			cnscHeader.CA_NNIECRSchePartNo = "TEST";
			cnscHeader.CA_PackMarks = "TEST2";
			var lpco = cnscHeader.LPCOViews.AddNew();
			lpco.CLP_Type = "7000";
			lpco.CLP_RefNo = "A001";

			Factory.Save();

			var dataLoad = new CAOrgSupplierPartDataLoad();
			using (TempFile tempFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var cnscFileds = ",PGA_CNSC_Indicator,PGA_CNSC_Category,PGA_CNSC_NNIECRSchedulePartNo,PGA_CNSC_PackMarks,PGA_CNSC_LPCOs";
					var baseValues = "TEST1,TESTProduct1,KG,0101210000,ORG01";
					var cnscValues = ",Y,NE,TEST,TEST2,7000/A001;7001/NO456;";
					sw.WriteLine(baseFields + cnscFileds);
					sw.WriteLine(baseValues + cnscValues);
				}

				dataLoad.ImportProductData(tempFile.Filename, true, false);
				Factory.Save();

				AssertEquals("Products created", 1, dataLoad.RunCounters.RecsUpdated);

				var product = new BusinessObjectFactory().Load<OrgSupplierPart>(part.PK);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);

				var pivot = product.PivotsForBinding[0];
				AssertEquals("CCA_CNSCIndicator", "Y", pivot.CCA_CNSCIndicator);

				var cnsc = pivot.CNSCPGAHeader;
				var lpcoViews = cnsc.LPCOViews;
				AssertEquals("LPCO Count", 2, lpcoViews.Count);

				var lpco1 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "7000");
				var lpco2 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "7001");

				AssertEquals("LPCO #1 CA_Type.", "7000", lpco1.CLP_Type);
				AssertEquals("LPCO #1 CA_RefNo.", "A001", lpco1.CLP_RefNo);

				AssertEquals("LPCO #2 CA_Type.", "7001", lpco2.CLP_Type);
				AssertEquals("LPCO #2 CA_RefNo.", "NO456", lpco2.CLP_RefNo);
			}
		}

		public void TestImportPGAECCCBlockEC01()
		{
			Factory.Save();

			using (TempFile tempFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var wrmFields = ",PGA_ECCC_WRMInd";
					var odsFields = ",PGA_ECCC_ODSInd,PGA_ECCC_CASNumber";
					var wenFields = ",PGA_ECCC_WENInd,PGA_ECCC_SourceOfSpecimen,PGA_ECCC_LifeStage,PGA_ECCC_Age,PGA_ECCC_Sex,PGA_ECCC_Regulated,PGA_ECCC_ScientificName,PGA_ECCC_TSN,PGA_ECCC_AphiaID,PGA_ECCC_Identities";
					var veeFields = ",PGA_ECCC_VEEInd,PGA_ECCC_ProcessCode,PGA_ECCC_NationalMark,PGA_ECCC_EPACertified,PGA_ECCC_Transition,PGA_ECCC_Incomplete,PGA_ECCC_CanadaUnique,PGA_ECCC_BulkReporting"
						+ ",PGA_ECCC_VehicleClass"
						+ ",PGA_ECCC_EngineClass,PGA_ECCC_EngineMake,PGA_ECCC_EngineModel,PGA_ECCC_EngineModelYear,PGA_ECCC_EngineIDNumber,PGA_ECCC_EngineManufacturer,PGA_ECCC_EngineFamilyName,PGA_ECCC_EngineTestGroup"
						+ ",PGA_ECCC_EngineEvaporativeFamily,PGA_ECCC_EnginePowerRating,PGA_ECCC_EnginePowerRatingUQ"
						+ ",PGA_ECCC_MachineMake,PGA_ECCC_MachineModel,PGA_ECCC_MachineModelYear,PGA_ECCC_MachineManufacturer";
					var otherFields = ",PGA_ECCC_IntendedUseCode,Manufacturer,PGA_ECCC_LPCOs";
					var baseValues = "TEST1,TestProduct1,KG,1234567890,ORG01";
					var wrmValues = ",Y";
					var odsValues = ",Y,CAS001";
					var wenValues = ",Y,SPECIMEN1,LS01,12,MALE,Y,SCN01,TSN01,APID01,AA/BB;CC/DD;EE";
					var veeValues = ",Y,EC01,Y,Y,Y,Y,Y,Y,VC01,ECL01,EM01,EMD01,2020,EIDN01,EMF01,EFN01,ETG01,EEF01,12,M,MCM01,MCMD01,2019,ORG02";
					var otherValues = ",IUC01,ORG01,AAA/111/222/333/M;BBB/444//6/M;DDD";
					sw.WriteLine(baseFields + wrmFields + odsFields + wenFields + veeFields + otherFields);
					sw.WriteLine(baseValues + wrmValues + odsValues + wenValues + veeValues + otherValues);
				}

				var dataLoad = new CAOrgSupplierPartDataLoad();
				dataLoad.ImportProductData(tempFile.Filename, false, false);
				Factory.Save();

				AssertEquals("Products Created", 1, dataLoad.RunCounters.RecsCreated);

				var product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("TEST1", testOrganisation, null);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);

				var pivot = product.PivotsForBinding[0];
				AssertEquals("CCA_ECCCIndicator", "Y", pivot.CCA_ECCCIndicator);

				var eccc = pivot.ECCCPGAHeader;
				var identities = eccc.Components;
				AssertEquals("Count of Identities.", 2, identities.Count);
				var identity01 = identities.Cast<Component>().First(x => x.CA_Type == "AA");
				var identity02 = identities.Cast<Component>().First(x => x.CA_Type == "CC");

				var lpcoViews = eccc.LPCOViews;
				AssertEquals("LPCO Count", 4, lpcoViews.Count);

				var lpco1 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "AAA");
				var lpco2 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "BBB");
				var lpco3 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8000");
				var lpco4 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8001");
				AssertPGAECCCBlockEC01(eccc, identity01, identity02, lpco1, lpco2, lpco3, lpco4);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var veeFields = ",PGA_ECCC_VEEInd,PGA_ECCC_ProcessCode";
					var baseValues = "TEST1,TestProduct1,KG,1234567890,ORG01";
					var veeValues = ",Y,EC01";
					sw.WriteLine(baseFields + veeFields);
					sw.WriteLine(baseValues + veeValues);
				}

				var dataLoad2 = new CAOrgSupplierPartDataLoad();
				dataLoad2.ImportProductData(tempFile.Filename, true, false);
				Factory.Save();
				AssertEquals("Products updated", 1, dataLoad2.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);
				var pivot2 = product.PivotsForBinding[0];
				AssertEquals("pivot2.PK", pivot.PK, pivot2.PK);
				var eccc2 = pivot2.ECCCPGAHeader;
				AssertEquals("eccc2.PK", eccc.PK, eccc2.PK);

				identities = eccc2.Components;
				AssertEquals("Count of Identities.", 2, identities.Count);
				identity01 = identities.Cast<Component>().First(x => x.PK == identity01.PK);
				identity02 = identities.Cast<Component>().First(x => x.PK == identity02.PK);

				lpcoViews = eccc2.LPCOViews;
				AssertEquals("LPCO Count", 4, lpcoViews.Count);

				lpco1 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "AAA");
				lpco2 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "BBB");
				lpco3 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8000");
				lpco4 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8001");
				AssertPGAECCCBlockEC01(eccc2, identity01, identity02, lpco1, lpco2, lpco3, lpco4);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var veeFields = ",PGA_ECCC_VEEInd";
					var baseValues = "TEST1,TestProduct1,KG,1234567890,ORG01";
					var veeValues = ",Y";
					sw.WriteLine(baseFields + veeFields);
					sw.WriteLine(baseValues + veeValues);
				}

				var dataLoad3 = new CAOrgSupplierPartDataLoad();
				dataLoad3.ImportProductData(tempFile.Filename, true, false);
				Factory.Save();
				AssertEquals("Products updated", 1, dataLoad3.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);
				pivot2 = product.PivotsForBinding[0];
				AssertEquals("pivot2.PK", pivot.PK, pivot2.PK);
				eccc2 = pivot2.ECCCPGAHeader;
				AssertEquals("eccc2.PK", eccc.PK, eccc2.PK);

				identities = eccc2.Components;
				AssertEquals("Count of Identities.", 2, identities.Count);
				identity01 = identities.Cast<Component>().First(x => x.PK == identity01.PK);
				identity02 = identities.Cast<Component>().First(x => x.PK == identity02.PK);

				lpcoViews = eccc2.LPCOViews;
				AssertEquals("LPCO Count", 4, lpcoViews.Count);

				lpco1 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "AAA");
				lpco2 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "BBB");
				lpco3 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8000");
				lpco4 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8001");
				AssertPGAECCCBlockEC01(eccc2, identity01, identity02, lpco1, lpco2, lpco3, lpco4);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var baseValues = "TEST1,TestProduct1,KG,1234567890,ORG01";
					sw.WriteLine(baseFields);
					sw.WriteLine(baseValues);
				}

				var dataLoad4 = new CAOrgSupplierPartDataLoad();
				dataLoad4.ImportProductData(tempFile.Filename, true, false);
				Factory.Save();
				AssertEquals("Products updated", 1, dataLoad4.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);
				pivot2 = product.PivotsForBinding[0];
				AssertEquals("pivot2.PK", pivot.PK, pivot2.PK);
				eccc2 = pivot2.ECCCPGAHeader;
				AssertEquals("eccc2.PK", eccc.PK, eccc2.PK);

				identities = eccc2.Components;
				AssertEquals("Count of Identities.", 2, identities.Count);
				identity01 = identities.Cast<Component>().First(x => x.PK == identity01.PK);
				identity02 = identities.Cast<Component>().First(x => x.PK == identity02.PK);

				lpcoViews = eccc2.LPCOViews;
				AssertEquals("LPCO Count", 4, lpcoViews.Count);

				lpco1 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "AAA");
				lpco2 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "BBB");
				lpco3 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8000");
				lpco4 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8001");
				AssertPGAECCCBlockEC01(eccc2, identity01, identity02, lpco1, lpco2, lpco3, lpco4);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var veeFields = ",PGA_ECCC_VEEInd,PGA_ECCC_ProcessCode";
					var baseValues = "TEST1,TestProduct1,KG,1234567890,ORG01";
					var veeValues = ",EC00";
					sw.WriteLine(baseFields + veeFields);
					sw.WriteLine(baseValues + veeValues);
				}

				var dataLoad5 = new CAOrgSupplierPartDataLoad();
				dataLoad5.ImportProductData(tempFile.Filename, true, false);
				Factory.Save();
				AssertEquals("Products updated", 1, dataLoad5.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);
				pivot2 = product.PivotsForBinding[0];
				AssertEquals("pivot2.PK", pivot.PK, pivot2.PK);
				eccc2 = pivot2.ECCCPGAHeader;
				AssertEquals("eccc2.PK", eccc.PK, eccc2.PK);

				identities = eccc2.Components;
				AssertEquals("Count of Identities.", 2, identities.Count);
				identity01 = identities.Cast<Component>().First(x => x.PK == identity01.PK);
				identity02 = identities.Cast<Component>().First(x => x.PK == identity02.PK);

				lpcoViews = eccc2.LPCOViews;
				AssertEquals("LPCO Count", 4, lpcoViews.Count);

				lpco1 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "AAA");
				lpco2 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "BBB");
				lpco3 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8000");
				lpco4 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8001");
				AssertPGAECCCBlockEC01(eccc2, identity01, identity02, lpco1, lpco2, lpco3, lpco4);
			}
		}

		void AssertPGAECCCBlockEC01(ECCCPGAHeader eccc, Component identity01, Component identity02, LPCOView lpco1, LPCOView lpco2, LPCOView lpco3, LPCOView lpco4)
		{
			AssertEquals("CA_WRMProgramInd", "Y", eccc.CA_WRMProgramInd);
			AssertEquals("CA_ODSProgramInd", "Y", eccc.CA_ODSProgramInd);
			AssertEquals("CA_WENProgramInd", "Y", eccc.CA_WENProgramInd);
			AssertEquals("CA_VEEProgramInd", "Y", eccc.CA_VEEProgramInd);

			AssertEquals("CA_CASNumber", "CAS001", eccc.CA_CASNumber);

			AssertEquals("CA_SourceOfSpecimen", "SPEC", eccc.CA_SourceOfSpecimen);
			AssertEquals("CA_LifeStage", "LS01", eccc.CA_LifeStage);
			AssertEquals("CA_Age", new ZInt(12), eccc.CA_Age);
			AssertEquals("CA_Sex", "MALE", eccc.CA_Sex);
			AssertEquals("CA_ComplianceDeclaration", true, eccc.CA_ComplianceDeclaration);
			AssertEquals("CA_ScientificName", "SCN01", eccc.CA_ScientificName);
			AssertEquals("CA_TSN", "TSN01", eccc.CA_TSN);
			AssertEquals("CA_AphiaID", "APID01", eccc.CA_AphiaID);
			AssertNotNull(identity01);
			AssertNotNull(identity02);
			AssertEquals("BB", identity01.CA_Name);
			AssertEquals("DD", identity02.CA_Name);

			AssertEquals("CA_ProcessCode", "EC01", eccc.CA_ProcessCode);
			AssertEquals("CA_NationalMark", true, eccc.CA_NationalMark);
			AssertEquals("CA_EPACertified", true, eccc.CA_EPACertified);
			AssertEquals("CA_Transition", false, eccc.CA_Transition);
			AssertEquals("CA_Incomplete", true, eccc.CA_Incomplete);
			AssertEquals("CA_CanadaUnique", true, eccc.CA_CanadaUnique);
			AssertEquals("CA_BulkReporting", true, eccc.CA_BulkReporting);

			AssertEquals("CA_VehicleClass", "VC01", eccc.CA_VehicleClass);

			AssertEquals("CA_EngineClass", "ECL0", eccc.CA_EngineClass);
			AssertEquals("CA_MakeOfEngine", "EM01", eccc.CA_MakeOfEngine);
			AssertEquals("CA_ModelOfEngine", "EMD01", eccc.CA_ModelOfEngine);
			AssertEquals("CA_EngineModelYear", "2020", eccc.CA_EngineModelYear);
			AssertEquals("CA_EngineIDNumber", "EIDN01", eccc.CA_EngineIDNumber);
			AssertEquals("CA_EngineManufacturer", "EMF01", eccc.CA_EngineManufacturer);
			AssertEquals("CA_EngineFamilyName", "EFN01", eccc.CA_EngineFamilyName);
			AssertEquals("CA_TestGroupName", "ETG01", eccc.CA_TestGroupName);
			AssertEquals("CA_EvaporativeFamily", "", eccc.CA_EvaporativeFamily);
			AssertEquals("CA_EnginePowerRating", 0m, eccc.CA_EnginePowerRating);
			AssertEquals("CA_PowerRatingUQ", "", eccc.CA_PowerRatingUQ);

			AssertEquals("CA_MakeOfMachine", "", eccc.CA_MakeOfMachine);
			AssertEquals("CA_ModelOfMachine", "", eccc.CA_ModelOfMachine);
			AssertEquals("CA_MachineModelYear", "", eccc.CA_MachineModelYear);
			AssertEquals("CA_MachineManufacturer", ZGuid.Empty, eccc.CA_MachineManufacturer);

			AssertEquals("CA_IntendedUseCode", "IUC01", eccc.CA_IntendedUseCode);
			AssertEquals("OA_Manufacturer", testOrganisation.MainAddress.PK, eccc.OA_Manufacturer);

			AssertNotNull(lpco1);
			AssertNotNull(lpco2);
			AssertEquals("LPCO #1 DIF URN", "222", lpco1.CLP_DIFRefNumberOrLocation);
			AssertEquals("LPCO #2 Ref No.", "444", lpco2.CLP_RefNo);
			AssertEquals("LPCO #2 DIF URN", ZString.Empty, lpco2.CLP_DIFRefNumberOrLocation);
			AssertEquals("LPCO #3 Ref No.", ZString.Empty, lpco3.CLP_RefNo);
			AssertEquals("LPCO #3 DIF URN", ZString.Empty, lpco3.CLP_DIFRefNumberOrLocation);
			AssertEquals("LPCO #4 Ref No.", ZString.Empty, lpco4.CLP_RefNo);
			AssertEquals("LPCO #4 DIF URN", ZString.Empty, lpco4.CLP_DIFRefNumberOrLocation);
		}

		public void TestImportPGAECCCBlockEC02()
		{
			Factory.Save();

			using (TempFile tempFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var wrmFields = ",PGA_ECCC_WRMInd";
					var odsFields = ",PGA_ECCC_ODSInd,PGA_ECCC_CASNumber";
					var wenFields = ",PGA_ECCC_WENInd,PGA_ECCC_SourceOfSpecimen,PGA_ECCC_LifeStage,PGA_ECCC_Age,PGA_ECCC_Sex,PGA_ECCC_Regulated,PGA_ECCC_ScientificName,PGA_ECCC_TSN,PGA_ECCC_AphiaID,PGA_ECCC_Identities";
					var veeFields = ",PGA_ECCC_VEEInd,PGA_ECCC_ProcessCode,PGA_ECCC_NationalMark,PGA_ECCC_EPACertified,PGA_ECCC_Transition,PGA_ECCC_Incomplete,PGA_ECCC_CanadaUnique,PGA_ECCC_BulkReporting"
						+ ",PGA_ECCC_VehicleClass"
						+ ",PGA_ECCC_EngineClass,PGA_ECCC_EngineMake,PGA_ECCC_EngineModel,PGA_ECCC_EngineModelYear,PGA_ECCC_EngineIDNumber,PGA_ECCC_EngineManufacturer,PGA_ECCC_EngineFamilyName,PGA_ECCC_EngineTestGroup"
						+ ",PGA_ECCC_EngineEvaporativeFamily,PGA_ECCC_EnginePowerRating,PGA_ECCC_EnginePowerRatingUQ"
						+ ",PGA_ECCC_MachineMake,PGA_ECCC_MachineModel,PGA_ECCC_MachineModelYear,PGA_ECCC_MachineManufacturer,PGA_ECCC_EngineLocation,PGA_ECCC_EvidenceOfConfirmityLocation";
					var otherFields = ",PGA_ECCC_IntendedUseCode,Manufacturer,PGA_ECCC_LPCOs";
					var baseValues = "TEST1,TestProduct1,KG,1234567890,ORG01";
					var wrmValues = ",Y";
					var odsValues = ",Y,CAS001";
					var wenValues = ",Y,SPECIMEN1,LS01,12,MALE,Y,SCN01,TSN01,APID01,AA/BB;CC/DD;EE";
					var veeValues = ",Y,EC02,Y,Y,Y,Y,Y,Y,VC01,ECL01,EM01,EMD01,2020,EIDN01,EMF01,EFN01,ETG01,EEF01,12,M,MCM01,MCMD01,2019,ORG02,EGL03,ECL04";
					var otherValues = ",IUC01,ORG01,AAA/111/222/333/M;BBB/444//6/M;DDD";
					sw.WriteLine(baseFields + wrmFields + odsFields + wenFields + veeFields + otherFields);
					sw.WriteLine(baseValues + wrmValues + odsValues + wenValues + veeValues + otherValues);
				}

				var dataLoad = new CAOrgSupplierPartDataLoad();
				dataLoad.ImportProductData(tempFile.Filename, false, false);
				Factory.Save();

				AssertEquals("Products Created", 1, dataLoad.RunCounters.RecsCreated);

				var product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("TEST1", testOrganisation, null);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);

				var pivot = product.PivotsForBinding[0];
				AssertEquals("CCA_ECCCIndicator", "Y", pivot.CCA_ECCCIndicator);

				var eccc = pivot.ECCCPGAHeader;
				var identities = eccc.Components;
				AssertEquals("Count of Identities.", 2, identities.Count);
				var identity01 = identities.Cast<Component>().First(x => x.CA_Type == "AA");
				var identity02 = identities.Cast<Component>().First(x => x.CA_Type == "CC");

				var lpcoViews = eccc.LPCOViews;
				AssertEquals("LPCO Count", 4, lpcoViews.Count);

				var lpco1 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "AAA");
				var lpco2 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "BBB");
				var lpco3 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8000");
				var lpco4 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8001");
				AssertPGAECCCBlockEC02(eccc, identity01, identity02, lpco1, lpco2, lpco3, lpco4);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var veeFields = ",PGA_ECCC_VEEInd,PGA_ECCC_ProcessCode";
					var baseValues = "TEST1,TestProduct1,KG,1234567890,ORG01";
					var veeValues = ",Y,EC02";
					sw.WriteLine(baseFields + veeFields);
					sw.WriteLine(baseValues + veeValues);
				}

				var dataLoad2 = new CAOrgSupplierPartDataLoad();
				dataLoad2.ImportProductData(tempFile.Filename, true, false);
				AssertEquals("Products updated", 1, dataLoad2.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);
				var pivot2 = product.PivotsForBinding[0];
				AssertEquals("pivot2.PK", pivot.PK, pivot2.PK);
				var eccc2 = pivot2.ECCCPGAHeader;
				AssertEquals("eccc2.PK", eccc.PK, eccc2.PK);

				identities = eccc2.Components;
				AssertEquals("Count of Identities.", 2, identities.Count);
				identity01 = identities.Cast<Component>().First(x => x.PK == identity01.PK);
				identity02 = identities.Cast<Component>().First(x => x.PK == identity02.PK);

				lpcoViews = eccc2.LPCOViews;
				AssertEquals("LPCO Count", 4, lpcoViews.Count);

				lpco1 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "AAA");
				lpco2 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "BBB");
				lpco3 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8000");
				lpco4 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8001");
				AssertPGAECCCBlockEC02(eccc2, identity01, identity02, lpco1, lpco2, lpco3, lpco4);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var veeFields = ",PGA_ECCC_VEEInd";
					var baseValues = "TEST1,TestProduct1,KG,1234567890,ORG01";
					var veeValues = ",Y";
					sw.WriteLine(baseFields + veeFields);
					sw.WriteLine(baseValues + veeValues);
				}

				var dataLoad3 = new CAOrgSupplierPartDataLoad();
				dataLoad3.ImportProductData(tempFile.Filename, true, false);
				AssertEquals("Products updated", 1, dataLoad3.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);
				pivot2 = product.PivotsForBinding[0];
				AssertEquals("pivot2.PK", pivot.PK, pivot2.PK);
				eccc2 = pivot2.ECCCPGAHeader;
				AssertEquals("eccc2.PK", eccc.PK, eccc2.PK);

				identities = eccc2.Components;
				AssertEquals("Count of Identities.", 2, identities.Count);
				identity01 = identities.Cast<Component>().First(x => x.PK == identity01.PK);
				identity02 = identities.Cast<Component>().First(x => x.PK == identity02.PK);

				lpcoViews = eccc2.LPCOViews;
				AssertEquals("LPCO Count", 4, lpcoViews.Count);

				lpco1 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "AAA");
				lpco2 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "BBB");
				lpco3 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8000");
				lpco4 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8001");
				AssertPGAECCCBlockEC02(eccc2, identity01, identity02, lpco1, lpco2, lpco3, lpco4);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var baseValues = "TEST1,TestProduct1,KG,1234567890,ORG01";
					sw.WriteLine(baseFields);
					sw.WriteLine(baseValues);
				}

				var dataLoad4 = new CAOrgSupplierPartDataLoad();
				dataLoad4.ImportProductData(tempFile.Filename, true, false);
				AssertEquals("Products updated", 1, dataLoad4.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);
				pivot2 = product.PivotsForBinding[0];
				AssertEquals("pivot2.PK", pivot.PK, pivot2.PK);
				eccc2 = pivot2.ECCCPGAHeader;
				AssertEquals("eccc2.PK", eccc.PK, eccc2.PK);

				identities = eccc2.Components;
				AssertEquals("Count of Identities.", 2, identities.Count);
				identity01 = identities.Cast<Component>().First(x => x.PK == identity01.PK);
				identity02 = identities.Cast<Component>().First(x => x.PK == identity02.PK);

				lpcoViews = eccc2.LPCOViews;
				AssertEquals("LPCO Count", 4, lpcoViews.Count);

				lpco1 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "AAA");
				lpco2 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "BBB");
				lpco3 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8000");
				lpco4 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8001");
				AssertPGAECCCBlockEC02(eccc2, identity01, identity02, lpco1, lpco2, lpco3, lpco4);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var veeFields = ",PGA_ECCC_VEEInd,PGA_ECCC_ProcessCode";
					var baseValues = "TEST1,TestProduct1,KG,1234567890,ORG01";
					var veeValues = ",EC00";
					sw.WriteLine(baseFields + veeFields);
					sw.WriteLine(baseValues + veeValues);
				}

				var dataLoad5 = new CAOrgSupplierPartDataLoad();
				dataLoad5.ImportProductData(tempFile.Filename, true, false);
				Factory.Save();
				AssertEquals("Products updated", 1, dataLoad5.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);
				pivot2 = product.PivotsForBinding[0];
				AssertEquals("pivot2.PK", pivot.PK, pivot2.PK);
				eccc2 = pivot2.ECCCPGAHeader;
				AssertEquals("eccc2.PK", eccc.PK, eccc2.PK);

				identities = eccc2.Components;
				AssertEquals("Count of Identities.", 2, identities.Count);
				identity01 = identities.Cast<Component>().First(x => x.PK == identity01.PK);
				identity02 = identities.Cast<Component>().First(x => x.PK == identity02.PK);

				lpcoViews = eccc2.LPCOViews;
				AssertEquals("LPCO Count", 4, lpcoViews.Count);

				lpco1 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "AAA");
				lpco2 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "BBB");
				lpco3 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8000");
				lpco4 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8001");
				AssertPGAECCCBlockEC02(eccc2, identity01, identity02, lpco1, lpco2, lpco3, lpco4);
			}
		}

		void AssertPGAECCCBlockEC02(ECCCPGAHeader eccc, Component identity01, Component identity02, LPCOView lpco1, LPCOView lpco2, LPCOView lpco3, LPCOView lpco4)
		{
			AssertEquals("CA_WRMProgramInd", "Y", eccc.CA_WRMProgramInd);
			AssertEquals("CA_ODSProgramInd", "Y", eccc.CA_ODSProgramInd);
			AssertEquals("CA_WENProgramInd", "Y", eccc.CA_WENProgramInd);
			AssertEquals("CA_VEEProgramInd", "Y", eccc.CA_VEEProgramInd);

			AssertEquals("CA_CASNumber", "CAS001", eccc.CA_CASNumber);

			AssertEquals("CA_SourceOfSpecimen", "SPEC", eccc.CA_SourceOfSpecimen);
			AssertEquals("CA_LifeStage", "LS01", eccc.CA_LifeStage);
			AssertEquals("CA_Age", new ZInt(12), eccc.CA_Age);
			AssertEquals("CA_Sex", "MALE", eccc.CA_Sex);
			AssertEquals("CA_ComplianceDeclaration", true, eccc.CA_ComplianceDeclaration);
			AssertEquals("CA_ScientificName", "SCN01", eccc.CA_ScientificName);
			AssertEquals("CA_TSN", "TSN01", eccc.CA_TSN);
			AssertEquals("CA_AphiaID", "APID01", eccc.CA_AphiaID);
			AssertNotNull(identity01);
			AssertNotNull(identity02);
			AssertEquals("BB", identity01.CA_Name);
			AssertEquals("DD", identity02.CA_Name);

			AssertEquals("CA_ProcessCode", "EC02", eccc.CA_ProcessCode);
			AssertEquals("CA_NationalMark", true, eccc.CA_NationalMark);
			AssertEquals("CA_EPACertified", true, eccc.CA_EPACertified);
			AssertEquals("CA_Transition", true, eccc.CA_Transition);
			AssertEquals("CA_Incomplete", true, eccc.CA_Incomplete);
			AssertEquals("CA_CanadaUnique", true, eccc.CA_CanadaUnique);
			AssertEquals("CA_BulkReporting", true, eccc.CA_BulkReporting);

			AssertEquals("CA_VehicleClass", "", eccc.CA_VehicleClass);

			AssertEquals("CA_EngineClass", "ECL0", eccc.CA_EngineClass);
			AssertEquals("CA_MakeOfEngine", "EM01", eccc.CA_MakeOfEngine);
			AssertEquals("CA_ModelOfEngine", "EMD01", eccc.CA_ModelOfEngine);
			AssertEquals("CA_EngineModelYear", "2020", eccc.CA_EngineModelYear);
			AssertEquals("CA_EngineIDNumber", "EIDN01", eccc.CA_EngineIDNumber);
			AssertEquals("CA_EngineManufacturer", "EMF01", eccc.CA_EngineManufacturer);
			AssertEquals("CA_EngineFamilyName", "EFN01", eccc.CA_EngineFamilyName);
			AssertEquals("CA_TestGroupName", "", eccc.CA_TestGroupName);
			AssertEquals("CA_EvaporativeFamily", "", eccc.CA_EvaporativeFamily);
			AssertEquals("CA_EnginePowerRating", 12m, eccc.CA_EnginePowerRating);
			AssertEquals("CA_PowerRatingUQ", "M", eccc.CA_PowerRatingUQ);

			AssertEquals("CA_MakeOfMachine", "MCM01", eccc.CA_MakeOfMachine);
			AssertEquals("CA_ModelOfMachine", "MCMD01", eccc.CA_ModelOfMachine);
			AssertEquals("CA_MachineModelYear", "2019", eccc.CA_MachineModelYear);
			AssertEquals("CA_MachineManufacturer", testOrganisation2.MainAddress.PK, eccc.CA_MachineManufacturer);
			AssertEquals("CA_OA_EngineLocation", testOrganisation3.MainAddress.PK, eccc.CA_OA_EngineLocation);
			AssertEquals("CA_OA_EvidenceOfConfirmityLocation", testOrganisation4.MainAddress.PK, eccc.CA_OA_EvidenceOfConformityLocation);

			AssertEquals("CA_IntendedUseCode", "IUC01", eccc.CA_IntendedUseCode);
			AssertEquals("OA_Manufacturer", testOrganisation.MainAddress.PK, eccc.OA_Manufacturer);

			AssertNotNull(lpco1);
			AssertNotNull(lpco2);
			AssertEquals("LPCO #1 Ref No.", "111", lpco1.CLP_RefNo);
			AssertEquals("LPCO #1 DIF URN", "222", lpco1.CLP_DIFRefNumberOrLocation);
			AssertEquals("LPCO #2 Ref No.", "444", lpco2.CLP_RefNo);
			AssertEquals("LPCO #2 DIF URN", ZString.Empty, lpco2.CLP_DIFRefNumberOrLocation);
			AssertEquals("LPCO #3 Ref No.", ZString.Empty, lpco3.CLP_RefNo);
			AssertEquals("LPCO #3 DIF URN", ZString.Empty, lpco3.CLP_DIFRefNumberOrLocation);
			AssertEquals("LPCO #4 Ref No.", ZString.Empty, lpco4.CLP_RefNo);
			AssertEquals("LPCO #4 DIF URN", ZString.Empty, lpco4.CLP_DIFRefNumberOrLocation);
		}

		public void TestImportPGAECCCBlockEC03()
		{
			Factory.Save();

			using (TempFile tempFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var wrmFields = ",PGA_ECCC_WRMInd";
					var odsFields = ",PGA_ECCC_ODSInd,PGA_ECCC_CASNumber";
					var wenFields = ",PGA_ECCC_WENInd,PGA_ECCC_SourceOfSpecimen,PGA_ECCC_LifeStage,PGA_ECCC_Age,PGA_ECCC_Sex,PGA_ECCC_Regulated,PGA_ECCC_ScientificName,PGA_ECCC_TSN,PGA_ECCC_AphiaID,PGA_ECCC_Identities";
					var veeFields = ",PGA_ECCC_VEEInd,PGA_ECCC_ProcessCode,PGA_ECCC_NationalMark,PGA_ECCC_EPACertified,PGA_ECCC_Transition,PGA_ECCC_Incomplete,PGA_ECCC_CanadaUnique,PGA_ECCC_BulkReporting"
						+ ",PGA_ECCC_VehicleClass"
						+ ",PGA_ECCC_EngineClass,PGA_ECCC_EngineMake,PGA_ECCC_EngineModel,PGA_ECCC_EngineModelYear,PGA_ECCC_EngineIDNumber,PGA_ECCC_EngineManufacturer,PGA_ECCC_EngineFamilyName,PGA_ECCC_EngineTestGroup"
						+ ",PGA_ECCC_EngineEvaporativeFamily,PGA_ECCC_EnginePowerRating,PGA_ECCC_EnginePowerRatingUQ"
						+ ",PGA_ECCC_MachineMake,PGA_ECCC_MachineModel,PGA_ECCC_MachineModelYear,PGA_ECCC_MachineManufacturer";
					var otherFields = ",PGA_ECCC_IntendedUseCode,Manufacturer,PGA_ECCC_LPCOs";
					var baseValues = "TEST1,TestProduct1,KG,1234567890,ORG01";
					var wrmValues = ",Y";
					var odsValues = ",Y,CAS001";
					var wenValues = ",Y,SPECIMEN1,LS01,12,MALE,Y,SCN01,TSN01,APID01,AA/BB;CC/DD;EE";
					var veeValues = ",Y,EC03,Y,Y,Y,Y,Y,Y,VC01,ECL01,EM01,EMD01,2020,EIDN01,EMF01,EFN01,ETG01,EEF01,12,M,MCM01,MCMD01,2019,ORG02";
					var otherValues = ",IUC01,ORG01,AAA/111/222/333/M;BBB/444//6/M;DDD";
					sw.WriteLine(baseFields + wrmFields + odsFields + wenFields + veeFields + otherFields);
					sw.WriteLine(baseValues + wrmValues + odsValues + wenValues + veeValues + otherValues);
				}

				var dataLoad = new CAOrgSupplierPartDataLoad();
				dataLoad.ImportProductData(tempFile.Filename, false, false);
				Factory.Save();

				AssertEquals("Products Created", 1, dataLoad.RunCounters.RecsCreated);

				var product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("TEST1", testOrganisation, null);

				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);

				var pivot = product.PivotsForBinding[0];
				AssertEquals("CCA_ECCCIndicator", "Y", pivot.CCA_ECCCIndicator);

				var eccc = pivot.ECCCPGAHeader;
				var identities = eccc.Components;
				AssertEquals("Count of Identities.", 2, identities.Count);
				var identity01 = identities.Cast<Component>().First(x => x.CA_Type == "AA");
				var identity02 = identities.Cast<Component>().First(x => x.CA_Type == "CC");

				var lpcoViews = eccc.LPCOViews;
				AssertEquals("LPCO Count", 4, lpcoViews.Count);

				var lpco1 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "AAA");
				var lpco2 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "BBB");
				var lpco3 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8000");
				var lpco4 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8001");
				AssertPGAECCCBlockEC03(eccc, identity01, identity02, lpco1, lpco2, lpco3, lpco4);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var veeFields = ",PGA_ECCC_VEEInd,PGA_ECCC_ProcessCode";
					var baseValues = "TEST1,TestProduct1,KG,1234567890,ORG01";
					var veeValues = ",Y,EC03";
					sw.WriteLine(baseFields + veeFields);
					sw.WriteLine(baseValues + veeValues);
				}

				var dataLoad2 = new CAOrgSupplierPartDataLoad();
				dataLoad2.ImportProductData(tempFile.Filename, true, false);
				AssertEquals("Products updated", 1, dataLoad2.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);
				var pivot2 = product.PivotsForBinding[0];
				AssertEquals("pivot2.PK", pivot.PK, pivot2.PK);
				var eccc2 = pivot2.ECCCPGAHeader;
				AssertEquals("eccc2.PK", eccc.PK, eccc2.PK);

				identities = eccc2.Components;
				AssertEquals("Count of Identities.", 2, identities.Count);
				identity01 = identities.Cast<Component>().First(x => x.PK == identity01.PK);
				identity02 = identities.Cast<Component>().First(x => x.PK == identity02.PK);

				lpcoViews = eccc2.LPCOViews;
				AssertEquals("LPCO Count", 4, lpcoViews.Count);

				lpco1 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "AAA");
				lpco2 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "BBB");
				lpco3 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8000");
				lpco4 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8001");
				AssertPGAECCCBlockEC03(eccc2, identity01, identity02, lpco1, lpco2, lpco3, lpco4);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var veeFields = ",PGA_ECCC_VEEInd";
					var baseValues = "TEST1,TestProduct1,KG,1234567890,ORG01";
					var veeValues = ",Y";
					sw.WriteLine(baseFields + veeFields);
					sw.WriteLine(baseValues + veeValues);
				}

				var dataLoad3 = new CAOrgSupplierPartDataLoad();
				dataLoad3.ImportProductData(tempFile.Filename, true, false);
				AssertEquals("Products updated", 1, dataLoad3.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);
				pivot2 = product.PivotsForBinding[0];
				AssertEquals("pivot2.PK", pivot.PK, pivot2.PK);
				eccc2 = pivot2.ECCCPGAHeader;
				AssertEquals("eccc2.PK", eccc.PK, eccc2.PK);

				identities = eccc2.Components;
				AssertEquals("Count of Identities.", 2, identities.Count);
				identity01 = identities.Cast<Component>().First(x => x.PK == identity01.PK);
				identity02 = identities.Cast<Component>().First(x => x.PK == identity02.PK);

				lpcoViews = eccc2.LPCOViews;
				AssertEquals("LPCO Count", 4, lpcoViews.Count);

				lpco1 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "AAA");
				lpco2 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "BBB");
				lpco3 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8000");
				lpco4 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8001");
				AssertPGAECCCBlockEC03(eccc2, identity01, identity02, lpco1, lpco2, lpco3, lpco4);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var baseValues = "TEST1,TestProduct1,KG,1234567890,ORG01";
					sw.WriteLine(baseFields);
					sw.WriteLine(baseValues);
				}

				var dataLoad4 = new CAOrgSupplierPartDataLoad();
				dataLoad4.ImportProductData(tempFile.Filename, true, false);
				AssertEquals("Products updated", 1, dataLoad4.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);
				pivot2 = product.PivotsForBinding[0];
				AssertEquals("pivot2.PK", pivot.PK, pivot2.PK);
				eccc2 = pivot2.ECCCPGAHeader;
				AssertEquals("eccc2.PK", eccc.PK, eccc2.PK);

				identities = eccc2.Components;
				AssertEquals("Count of Identities.", 2, identities.Count);
				identity01 = identities.Cast<Component>().First(x => x.PK == identity01.PK);
				identity02 = identities.Cast<Component>().First(x => x.PK == identity02.PK);

				lpcoViews = eccc2.LPCOViews;
				AssertEquals("LPCO Count", 4, lpcoViews.Count);

				lpco1 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "AAA");
				lpco2 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "BBB");
				lpco3 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8000");
				lpco4 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8001");
				AssertPGAECCCBlockEC03(eccc2, identity01, identity02, lpco1, lpco2, lpco3, lpco4);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var veeFields = ",PGA_ECCC_VEEInd,PGA_ECCC_ProcessCode";
					var baseValues = "TEST1,TestProduct1,KG,1234567890,ORG01";
					var veeValues = ",EC00";
					sw.WriteLine(baseFields + veeFields);
					sw.WriteLine(baseValues + veeValues);
				}

				var dataLoad5 = new CAOrgSupplierPartDataLoad();
				dataLoad5.ImportProductData(tempFile.Filename, true, false);
				Factory.Save();
				AssertEquals("Products updated", 1, dataLoad5.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);
				pivot2 = product.PivotsForBinding[0];
				AssertEquals("pivot2.PK", pivot.PK, pivot2.PK);
				eccc2 = pivot2.ECCCPGAHeader;
				AssertEquals("eccc2.PK", eccc.PK, eccc2.PK);

				identities = eccc2.Components;
				AssertEquals("Count of Identities.", 2, identities.Count);
				identity01 = identities.Cast<Component>().First(x => x.PK == identity01.PK);
				identity02 = identities.Cast<Component>().First(x => x.PK == identity02.PK);

				lpcoViews = eccc2.LPCOViews;
				AssertEquals("LPCO Count", 4, lpcoViews.Count);

				lpco1 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "AAA");
				lpco2 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "BBB");
				lpco3 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8000");
				lpco4 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8001");
				AssertPGAECCCBlockEC03(eccc2, identity01, identity02, lpco1, lpco2, lpco3, lpco4);
			}
		}

		void AssertPGAECCCBlockEC03(ECCCPGAHeader eccc, Component identity01, Component identity02, LPCOView lpco1, LPCOView lpco2, LPCOView lpco3, LPCOView lpco4)
		{
			CombineAssertions(() =>
			{
				AssertEquals("CA_WRMProgramInd", "Y", eccc.CA_WRMProgramInd);
				AssertEquals("CA_ODSProgramInd", "Y", eccc.CA_ODSProgramInd);
				AssertEquals("CA_WENProgramInd", "Y", eccc.CA_WENProgramInd);
				AssertEquals("CA_VEEProgramInd", "Y", eccc.CA_VEEProgramInd);

				AssertEquals("CA_CASNumber", "CAS001", eccc.CA_CASNumber);

				AssertEquals("CA_SourceOfSpecimen", "SPEC", eccc.CA_SourceOfSpecimen);
				AssertEquals("CA_LifeStage", "LS01", eccc.CA_LifeStage);
				AssertEquals("CA_Age", new ZInt(12), eccc.CA_Age);
				AssertEquals("CA_Sex", "MALE", eccc.CA_Sex);
				AssertEquals("CA_ComplianceDeclaration", true, eccc.CA_ComplianceDeclaration);
				AssertEquals("CA_ScientificName", "SCN01", eccc.CA_ScientificName);
				AssertEquals("CA_TSN", "TSN01", eccc.CA_TSN);
				AssertEquals("CA_AphiaID", "APID01", eccc.CA_AphiaID);
				AssertNotNull(identity01);
				AssertNotNull(identity02);
				AssertEquals("BB", identity01.CA_Name);
				AssertEquals("DD", identity02.CA_Name);

				AssertEquals("CA_ProcessCode", "EC03", eccc.CA_ProcessCode);
				AssertEquals("CA_NationalMark", true, eccc.CA_NationalMark);
				AssertEquals("CA_EPACertified", true, eccc.CA_EPACertified);
				AssertEquals("CA_Transition", false, eccc.CA_Transition);
				AssertEquals("CA_Incomplete", true, eccc.CA_Incomplete);
				AssertEquals("CA_CanadaUnique", true, eccc.CA_CanadaUnique);
				AssertEquals("CA_BulkReporting", true, eccc.CA_BulkReporting);

				AssertEquals("CA_VehicleClass", "", eccc.CA_VehicleClass);

				AssertEquals("CA_EngineClass", "ECL0", eccc.CA_EngineClass);
				AssertEquals("CA_MakeOfEngine", "EM01", eccc.CA_MakeOfEngine);
				AssertEquals("CA_ModelOfEngine", "EMD01", eccc.CA_ModelOfEngine);
				AssertEquals("CA_EngineModelYear", "2020", eccc.CA_EngineModelYear);
				AssertEquals("CA_EngineIDNumber", "EIDN01", eccc.CA_EngineIDNumber);
				AssertEquals("CA_EngineManufacturer", "EMF01", eccc.CA_EngineManufacturer);
				AssertEquals("CA_EngineFamilyName", "EFN01", eccc.CA_EngineFamilyName);
				AssertEquals("CA_TestGroupName", "", eccc.CA_TestGroupName);
				AssertEquals("CA_EvaporativeFamily", "", eccc.CA_EvaporativeFamily);
				AssertEquals("CA_EnginePowerRating", 12m, eccc.CA_EnginePowerRating);
				AssertEquals("CA_PowerRatingUQ", "M", eccc.CA_PowerRatingUQ);

				AssertEquals("CA_MakeOfMachine", "MCM01", eccc.CA_MakeOfMachine);
				AssertEquals("CA_ModelOfMachine", "MCMD01", eccc.CA_ModelOfMachine);
				AssertEquals("CA_MachineModelYear", "", eccc.CA_MachineModelYear);
				AssertEquals("CA_MachineManufacturer", testOrganisation2.MainAddress.PK, eccc.CA_MachineManufacturer);

				AssertEquals("CA_IntendedUseCode", "IUC01", eccc.CA_IntendedUseCode);
				AssertEquals("OA_Manufacturer", testOrganisation.MainAddress.PK, eccc.OA_Manufacturer);

				AssertNotNull(lpco1);
				AssertNotNull(lpco2);
				AssertEquals("LPCO #1 Ref No.", "111", lpco1.CLP_RefNo);
				AssertEquals("LPCO #1 DIF URN", "222", lpco1.CLP_DIFRefNumberOrLocation);
				AssertEquals("LPCO #2 Ref No.", "444", lpco2.CLP_RefNo);
				AssertEquals("LPCO #2 DIF URN", ZString.Empty, lpco2.CLP_DIFRefNumberOrLocation);
				AssertEquals("LPCO #3 Ref No.", ZString.Empty, lpco3.CLP_RefNo);
				AssertEquals("LPCO #3 DIF URN", ZString.Empty, lpco3.CLP_DIFRefNumberOrLocation);
				AssertEquals("LPCO #4 Ref No.", ZString.Empty, lpco4.CLP_RefNo);
				AssertEquals("LPCO #4 DIF URN", ZString.Empty, lpco4.CLP_DIFRefNumberOrLocation);
			});
		}

		public void TestImportPGAECCCBlockEC04()
		{
			Factory.Save();

			using (TempFile tempFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var wrmFields = ",PGA_ECCC_WRMInd";
					var odsFields = ",PGA_ECCC_ODSInd,PGA_ECCC_CASNumber";
					var wenFields = ",PGA_ECCC_WENInd,PGA_ECCC_SourceOfSpecimen,PGA_ECCC_LifeStage,PGA_ECCC_Age,PGA_ECCC_Sex,PGA_ECCC_Regulated,PGA_ECCC_ScientificName,PGA_ECCC_TSN,PGA_ECCC_AphiaID,PGA_ECCC_Identities";
					var veeFields = ",PGA_ECCC_VEEInd,PGA_ECCC_ProcessCode,PGA_ECCC_NationalMark,PGA_ECCC_EPACertified,PGA_ECCC_Transition,PGA_ECCC_Incomplete,PGA_ECCC_CanadaUnique,PGA_ECCC_BulkReporting"
						+ ",PGA_ECCC_VehicleClass"
						+ ",PGA_ECCC_EngineClass,PGA_ECCC_EngineMake,PGA_ECCC_EngineModel,PGA_ECCC_EngineModelYear,PGA_ECCC_EngineIDNumber,PGA_ECCC_EngineManufacturer,PGA_ECCC_EngineFamilyName,PGA_ECCC_EngineTestGroup"
						+ ",PGA_ECCC_EngineEvaporativeFamily,PGA_ECCC_EnginePowerRating,PGA_ECCC_EnginePowerRatingUQ"
						+ ",PGA_ECCC_MachineMake,PGA_ECCC_MachineModel,PGA_ECCC_MachineModelYear,PGA_ECCC_MachineManufacturer";
					var otherFields = ",PGA_ECCC_IntendedUseCode,Manufacturer,PGA_ECCC_LPCOs";
					var baseValues = "TEST1,TestProduct1,KG,1234567890,ORG01";
					var wrmValues = ",Y";
					var odsValues = ",Y,CAS001";
					var wenValues = ",Y,SPECIMEN1,LS01,12,MALE,Y,SCN01,TSN01,APID01,AA/BB;CC/DD;EE";
					var veeValues = ",Y,EC04,Y,Y,Y,Y,Y,Y,VC01,ECL01,EM01,EMD01,2020,EIDN01,EMF01,EFN01,ETG01,EEF01,12,M,MCM01,MCMD01,2019,ORG02";
					var otherValues = ",IUC01,ORG01,AAA/111/222/333/M;BBB/444//6/M;DDD";
					sw.WriteLine(baseFields + wrmFields + odsFields + wenFields + veeFields + otherFields);
					sw.WriteLine(baseValues + wrmValues + odsValues + wenValues + veeValues + otherValues);
				}

				var dataLoad = new CAOrgSupplierPartDataLoad();
				dataLoad.ImportProductData(tempFile.Filename, false, false);
				Factory.Save();

				AssertEquals("Products Created", 1, dataLoad.RunCounters.RecsCreated);

				var product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("TEST1", testOrganisation, null);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);

				var pivot = product.PivotsForBinding[0];
				var eccc = pivot.ECCCPGAHeader;

				var identities = eccc.Components;
				AssertEquals("Count of Identities.", 2, identities.Count);
				var identity01 = identities.Cast<Component>().First(x => x.CA_Type == "AA");
				var identity02 = identities.Cast<Component>().First(x => x.CA_Type == "CC");

				var lpcoViews = eccc.LPCOViews;
				AssertEquals("LPCO Count", 4, lpcoViews.Count);

				var lpco1 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "AAA");
				var lpco2 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "BBB");
				var lpco3 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8000");
				var lpco4 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8001");
				AssertPGAECCCBlockEC04(pivot, eccc, identity01, identity02, lpco1, lpco2, lpco3, lpco4);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var veeFields = ",PGA_ECCC_VEEInd,PGA_ECCC_ProcessCode";
					var baseValues = "TEST1,TestProduct1,KG,1234567890,ORG01";
					var veeValues = ",Y,EC04";
					sw.WriteLine(baseFields + veeFields);
					sw.WriteLine(baseValues + veeValues);
				}

				var dataLoad2 = new CAOrgSupplierPartDataLoad();
				dataLoad2.ImportProductData(tempFile.Filename, true, false);
				AssertEquals("Products updated", 1, dataLoad2.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);
				var pivot2 = product.PivotsForBinding[0];
				AssertEquals("pivot2.PK", pivot.PK, pivot2.PK);
				var eccc2 = pivot2.ECCCPGAHeader;
				AssertEquals("eccc2.PK", eccc.PK, eccc2.PK);

				identities = eccc2.Components;
				AssertEquals("Count of Identities.", 2, identities.Count);
				identity01 = identities.Cast<Component>().First(x => x.PK == identity01.PK);
				identity02 = identities.Cast<Component>().First(x => x.PK == identity02.PK);

				lpcoViews = eccc2.LPCOViews;
				AssertEquals("LPCO Count", 4, lpcoViews.Count);

				lpco1 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "AAA");
				lpco2 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "BBB");
				lpco3 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8000");
				lpco4 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8001");
				AssertPGAECCCBlockEC04(pivot2, eccc2, identity01, identity02, lpco1, lpco2, lpco3, lpco4);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var veeFields = ",PGA_ECCC_VEEInd";
					var baseValues = "TEST1,TestProduct1,KG,1234567890,ORG01";
					var veeValues = ",Y";
					sw.WriteLine(baseFields + veeFields);
					sw.WriteLine(baseValues + veeValues);
				}

				var dataLoad3 = new CAOrgSupplierPartDataLoad();
				dataLoad3.ImportProductData(tempFile.Filename, true, false);
				AssertEquals("Products updated", 1, dataLoad3.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);
				pivot2 = product.PivotsForBinding[0];
				AssertEquals("pivot2.PK", pivot.PK, pivot2.PK);
				eccc2 = pivot2.ECCCPGAHeader;
				AssertEquals("eccc2.PK", eccc.PK, eccc2.PK);

				identities = eccc2.Components;
				AssertEquals("Count of Identities.", 2, identities.Count);
				identity01 = identities.Cast<Component>().First(x => x.PK == identity01.PK);
				identity02 = identities.Cast<Component>().First(x => x.PK == identity02.PK);

				lpcoViews = eccc2.LPCOViews;
				AssertEquals("LPCO Count", 4, lpcoViews.Count);

				lpco1 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "AAA");
				lpco2 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "BBB");
				lpco3 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8000");
				lpco4 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8001");
				AssertPGAECCCBlockEC04(pivot2, eccc2, identity01, identity02, lpco1, lpco2, lpco3, lpco4);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var baseValues = "TEST1,TestProduct1,KG,1234567890,ORG01";
					sw.WriteLine(baseFields);
					sw.WriteLine(baseValues);
				}

				var dataLoad4 = new CAOrgSupplierPartDataLoad();
				dataLoad4.ImportProductData(tempFile.Filename, true, false);
				AssertEquals("Products updated", 1, dataLoad4.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);
				pivot2 = product.PivotsForBinding[0];
				AssertEquals("pivot2.PK", pivot.PK, pivot2.PK);
				eccc2 = pivot2.ECCCPGAHeader;
				AssertEquals("eccc2.PK", eccc.PK, eccc2.PK);

				identities = eccc2.Components;
				AssertEquals("Count of Identities.", 2, identities.Count);
				identity01 = identities.Cast<Component>().First(x => x.PK == identity01.PK);
				identity02 = identities.Cast<Component>().First(x => x.PK == identity02.PK);

				lpcoViews = eccc2.LPCOViews;
				AssertEquals("LPCO Count", 4, lpcoViews.Count);

				lpco1 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "AAA");
				lpco2 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "BBB");
				lpco3 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8000");
				lpco4 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8001");
				AssertPGAECCCBlockEC04(pivot2, eccc2, identity01, identity02, lpco1, lpco2, lpco3, lpco4);

				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var veeFields = ",PGA_ECCC_VEEInd,PGA_ECCC_ProcessCode";
					var baseValues = "TEST1,TestProduct1,KG,1234567890,ORG01";
					var veeValues = ",EC00";
					sw.WriteLine(baseFields + veeFields);
					sw.WriteLine(baseValues + veeValues);
				}

				var dataLoad5 = new CAOrgSupplierPartDataLoad();
				dataLoad5.ImportProductData(tempFile.Filename, true, false);
				Factory.Save();
				AssertEquals("Products updated", 1, dataLoad5.RunCounters.RecsUpdated);

				product = new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK);
				AssertNotNull(product);
				AssertEquals("Pivot Count", 1, product.PivotsForBinding.Count);
				pivot2 = product.PivotsForBinding[0];
				AssertEquals("pivot2.PK", pivot.PK, pivot2.PK);
				eccc2 = pivot2.ECCCPGAHeader;
				AssertEquals("eccc2.PK", eccc.PK, eccc2.PK);

				identities = eccc2.Components;
				AssertEquals("Count of Identities.", 2, identities.Count);
				identity01 = identities.Cast<Component>().First(x => x.PK == identity01.PK);
				identity02 = identities.Cast<Component>().First(x => x.PK == identity02.PK);

				lpcoViews = eccc2.LPCOViews;
				AssertEquals("LPCO Count", 4, lpcoViews.Count);

				lpco1 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "AAA");
				lpco2 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "BBB");
				lpco3 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8000");
				lpco4 = lpcoViews.Cast<LPCOView>().First(x => x.CLP_Type == "8001");
				AssertPGAECCCBlockEC04(pivot2, eccc2, identity01, identity02, lpco1, lpco2, lpco3, lpco4);
			}
		}

		void AssertPGAECCCBlockEC04(CusClassPartPivot pivot, ECCCPGAHeader eccc, Component identity01, Component identity02, LPCOView lpco1, LPCOView lpco2, LPCOView lpco3, LPCOView lpco4)
		{
			CombineAssertions(() =>
			{
				AssertEquals("CCA_ECCCIndicator", "Y", pivot.CCA_ECCCIndicator);
				AssertEquals("CA_WRMProgramInd", "Y", eccc.CA_WRMProgramInd);
				AssertEquals("CA_ODSProgramInd", "Y", eccc.CA_ODSProgramInd);
				AssertEquals("CA_WENProgramInd", "Y", eccc.CA_WENProgramInd);
				AssertEquals("CA_VEEProgramInd", "Y", eccc.CA_VEEProgramInd);

				AssertEquals("CA_CASNumber", "CAS001", eccc.CA_CASNumber);

				AssertEquals("CA_SourceOfSpecimen", "SPEC", eccc.CA_SourceOfSpecimen);
				AssertEquals("CA_LifeStage", "LS01", eccc.CA_LifeStage);
				AssertEquals("CA_Age", new ZInt(12), eccc.CA_Age);
				AssertEquals("CA_Sex", "MALE", eccc.CA_Sex);
				AssertEquals("CA_ComplianceDeclaration", true, eccc.CA_ComplianceDeclaration);
				AssertEquals("CA_ScientificName", "SCN01", eccc.CA_ScientificName);
				AssertEquals("CA_TSN", "TSN01", eccc.CA_TSN);
				AssertEquals("CA_AphiaID", "APID01", eccc.CA_AphiaID);
				AssertEquals("BB", identity01.CA_Name);
				AssertEquals("DD", identity02.CA_Name);

				AssertEquals("CA_ProcessCode", "EC04", eccc.CA_ProcessCode);
				AssertEquals("CA_NationalMark", true, eccc.CA_NationalMark);
				AssertEquals("CA_EPACertified", true, eccc.CA_EPACertified);
				AssertEquals("CA_Transition", false, eccc.CA_Transition);
				AssertEquals("CA_Incomplete", true, eccc.CA_Incomplete);
				AssertEquals("CA_CanadaUnique", true, eccc.CA_CanadaUnique);
				AssertEquals("CA_BulkReporting", true, eccc.CA_BulkReporting);

				AssertEquals("CA_VehicleClass", "VC01", eccc.CA_VehicleClass);

				AssertEquals("CA_EngineClass", "ECL0", eccc.CA_EngineClass);
				AssertEquals("CA_MakeOfEngine", "EM01", eccc.CA_MakeOfEngine);
				AssertEquals("CA_ModelOfEngine", "EMD01", eccc.CA_ModelOfEngine);
				AssertEquals("CA_EngineModelYear", "2020", eccc.CA_EngineModelYear);
				AssertEquals("CA_EngineIDNumber", "EIDN01", eccc.CA_EngineIDNumber);
				AssertEquals("CA_EngineManufacturer", "EMF01", eccc.CA_EngineManufacturer);
				AssertEquals("CA_EngineFamilyName", "EFN01", eccc.CA_EngineFamilyName);
				AssertEquals("CA_TestGroupName", "", eccc.CA_TestGroupName);
				AssertEquals("CA_EvaporativeFamily", "EEF01", eccc.CA_EvaporativeFamily);
				AssertEquals("CA_EnginePowerRating", 0m, eccc.CA_EnginePowerRating);
				AssertEquals("CA_PowerRatingUQ", "", eccc.CA_PowerRatingUQ);

				AssertEquals("CA_MakeOfMachine", "", eccc.CA_MakeOfMachine);
				AssertEquals("CA_ModelOfMachine", "", eccc.CA_ModelOfMachine);
				AssertEquals("CA_MachineModelYear", "", eccc.CA_MachineModelYear);
				AssertEquals("CA_MachineManufacturer", testOrganisation2.MainAddress.PK, eccc.CA_MachineManufacturer);

				AssertEquals("CA_IntendedUseCode", "IUC01", eccc.CA_IntendedUseCode);
				AssertEquals("OA_Manufacturer", testOrganisation.MainAddress.PK, eccc.OA_Manufacturer);

				AssertEquals("LPCO #1 Ref No.", "111", lpco1.CLP_RefNo);
				AssertEquals("LPCO #1 DIF URN", "222", lpco1.CLP_DIFRefNumberOrLocation);
				AssertEquals("LPCO #2 Ref No.", "444", lpco2.CLP_RefNo);
				AssertEquals("LPCO #2 DIF URN", ZString.Empty, lpco2.CLP_DIFRefNumberOrLocation);
				AssertEquals("LPCO #3 Ref No.", ZString.Empty, lpco3.CLP_RefNo);
				AssertEquals("LPCO #3 DIF URN", ZString.Empty, lpco3.CLP_DIFRefNumberOrLocation);
				AssertEquals("LPCO #4 Ref No.", ZString.Empty, lpco4.CLP_RefNo);
				AssertEquals("LPCO #4 DIF URN", ZString.Empty, lpco4.CLP_DIFRefNumberOrLocation);
			});
		}

		public void TestPGAFields()
		{
			var dataLoadForTest = new CAOrgSupplierPartDataLoadForTest();
			var properties = dataLoadForTest.GetFieldNames_Exposed();
			var pgaProperties = typeof(IPGADataToLoad).GetProperties().Select(x => x.Name);
			Assert(pgaProperties.All(x => properties.Contains(x)));
		}

		public void TestGetPGAPropertiesToSuspendSetting()
		{
			Factory.Save();

			using (TempFile tempFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ImportTariff,Owner,PGA_ECCC_WRMInd,PGA_CFIA_Indicator,PGA_CNSC_Indicator,PGA_GAC_Indicator,PGA_DFO_ABIInd,PGA_HC_APIInd,PGA_NRCan_EEFInd,PGA_PHAC_HAPInd,PGA_TC_TPRInd");
					sw.WriteLine("TEST1,TestProduct1,KG,1234567890,ORG01,Y,N,Y,N,Y,N,Y,N,Y");
				}
				var dataLoad = new CAOrgSupplierPartDataLoad();
				dataLoad.ImportProductData(tempFile.Filename, true, false);
				var suspendedPGAProperties = CAOrgSupplierPartAndClassificationDataLoad_PGAHelper.PGAFieldNames.GetPGAPropertiesToSuspendSetting(dataLoad).ToArray();
				var expectPGAProperties = new ZString[] {
					CusClassPartPivot.Schema.CCA_ECCCIndicator,
					CusClassPartPivot.Schema.CCA_CFIAIndicator,
					CusClassPartPivot.Schema.CCA_CNSCIndicator,
					CusClassPartPivot.Schema.CCA_GACIndicator,
					CusClassPartPivot.Schema.CCA_DFOIndicator,
					CusClassPartPivot.Schema.CCA_HCIndicator,
					CusClassPartPivot.Schema.CCA_NRCanIndicator,
					CusClassPartPivot.Schema.CCA_PHACIndicator,
					CusClassPartPivot.Schema.CCA_TCIndicator
				};
				AssertContainsExactElementsInAnyOrder(expectPGAProperties, suspendedPGAProperties);
			}
		}

		public void TestGetStringValue_LogMessage()
		{
			Factory.Save();
			using (TempFile tempFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					var baseFields = "Code,Description,UQ,ImportTariff,Owner";
					var cnscFields = ",PGA_CNSC_Indicator,PGA_CNSC_Category,PGA_CNSC_NNIECRSchedulePartNo,PGA_CNSC_PackMarks,PGA_CNSC_LPCOs";
					var baseValues = "TEST1,TESTProduct1,KG,1234567890,ORG01";
					var cnscValues = ",Y,NE,TEST,TEST21234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789,7000/NO123;7001/NO456";
					sw.WriteLine(baseFields + cnscFields);
					sw.WriteLine(baseValues + cnscValues);
				}
				
				var loader = GetNewDataLoader();
				var logs = new List<string>();
				loader.LogUpdated += (s, e) => logs.Add(e.LogMessage);
				loader.ImportProductData(tempFile.Filename, true, false);
				Factory.Save();

				AssertEquals("Pack Marks 'TEST21234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789' is too long. Storing 'TEST2123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567' instead.", loader.Log[1]);
				AssertEquals("Pack Marks 'TEST21234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789' is too long. Storing 'TEST2123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567' instead.", logs[1]);
			}
		}

		#region Implementation

		ZString fileHeader;
		OrgHeader testOrganisation;
		OrgHeader testOrganisation2;
		OrgHeader testOrganisation3;
		OrgHeader testOrganisation4;

		CusClassification CreateClassification(ZString lookupCode, ZString tariff, ZString type)
		{
			var lookup = Factory.NewWithValidTestData<CusClassification>();
			lookup.CC_LookupCode = lookupCode;
			if (!tariff.IsEmpty)
			{
				lookup.CC_TariffNum = tariff;
			}

			lookup.CC_ClassificationType = type;
			return lookup;
		}

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
			fileHeader = "Code,Description,UQ,ExportClassification,ExportTariff,ImportClassification,ImportTariff,Owner,Supplier,Origin,OriginState";
			testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			testOrganisation.OH_Code = "ORG01";
			testOrganisation2 = Factory.New<OrgHeader>();
			testOrganisation2.OH_Code = "ORG02";
			testOrganisation2.MainAddress.Address1 = "TEST Address 2";
			testOrganisation3 = Factory.New<OrgHeader>();
			testOrganisation3.OH_Code = "EGL03";
			testOrganisation3.MainAddress.Address1 = "TEST Address 3";
			testOrganisation4 = Factory.New<OrgHeader>();
			testOrganisation4.OH_Code = "ECL04";
			testOrganisation4.MainAddress.Address1 = "TEST Address 4";
		}

		protected override CAOrgSupplierPartDataLoad GetNewDataLoader()
		{
			return new CAOrgSupplierPartDataLoad();
		}

		List<string> exclusionList => new List<string>()
		{
			"RN_NKOrigin",
			"ETRateCode",
			"OA_Manufacturer",
			"ProvinceOfOrigin",
			"DestinationProvince",
			"ValueForDutyCode",
			"TreatmentCode",
			"99TariffCode",
			"AuthorityNumber",
			"TRSNumber",
			"RequirementID",
			"RequirementVer",
			"AirsCode",
			"RN_NKCFIAOrigin",
			"CFIAUSStateOfOrigin",
			"EndUse",
			"MiscID",
			"ImportReasonCode",
			"ModelNumber",
			"TypeSize",
			"TIIN",
			"GSTStatusCode",
			"ETExemption",
			"CompliantCompletion",
			"CompliantImportDate",
			"CFIAInd",
			"CNSCInd",
			"DFOInd",
			"ECCCInd",
			"GACIndicator",
			"HCIndicator",
			"NRCanIndicator",
			"PHACIndicator",
			"TCIndicator"
		};

		#endregion

		public class CAOrgSupplierPartDataLoadForTest : CAOrgSupplierPartDataLoad
		{
			public IEnumerable<string> GetFieldNames_Exposed()
			{
				return base.GetFieldNames();
			}
		}
	}
}
