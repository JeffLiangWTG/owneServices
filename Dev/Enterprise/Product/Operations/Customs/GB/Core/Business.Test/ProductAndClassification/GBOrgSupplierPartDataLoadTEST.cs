using System.IO;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.ProductAndClassification.Testing
{
	[TestedType(typeof(GBOrgSupplierPartDataLoad))]
	class GBOrgSupplierPartDataLoadTEST : DataLoadTestCase<GBOrgSupplierPartDataLoad>
	{
		[ExpectNoExceptions]
		public void TestGbProductLoadLinksToPivot()
		{
			var exportClass = Factory.New<CusClassification>();
			exportClass.CC_LookupCode = "BEERe";
			var importClass = Factory.New<CusClassification>();
			importClass.CC_LookupCode = "BEERi";
			var existingExportProductAlreadyLinkedToBeerE = Factory.New<OrgSupplierPart>();
			existingExportProductAlreadyLinkedToBeerE.OP_PartNum = "P00e";
			var relation = existingExportProductAlreadyLinkedToBeerE.RelatedOrganisations.AddNew();
			relation.OU_OH = moesTavernOrganisation.PK;
			relation.OU_Relationship = "OWN";
			var pivot = existingExportProductAlreadyLinkedToBeerE.PivotsForBinding.AddNew();
			pivot.CI_CC = exportClass.PK;
			var anotherExistingClassification = Factory.New<CusClassification>();
			anotherExistingClassification.CC_LookupCode = "WHATEVER";
			pivot = existingExportProductAlreadyLinkedToBeerE.PivotsForBinding.AddNew();
			pivot.CI_CC = anotherExistingClassification.PK;
			Factory.Save();

			var dataLoad = new GBOrgSupplierPartDataLoad();
			using (TempFile tempFile1 = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(tempFile1.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,ExportTariff,ImportTariff,ECSUPPLEMENT1,ECSUPPLEMENT2,CPC    ,Origin,THIRDQTY,ECSUPPLEMENT,UsageComment,ClassificationDescription");
					sw.WriteLine("P00e,Suds 4 exp, KG,BEERe				  ,		               ,ORG01,            ,             ,			 ,			   ,       ,AU    ,69      ,            ,            ,                         ");
					sw.WriteLine("P00i,Suds 4 imp, KG,					  ,BEERi               ,ORG01,            ,             ,			 ,			   ,       ,AU    ,69      ,            ,            ,                         ");
					sw.WriteLine("P0Te,Trf 4 exp , KG,					  ,                    ,ORG01,1234.56 78  ,             ,			 ,			   ,       ,AU    ,69      ,            ,            ,                         ");
					sw.WriteLine("P0Ti,Trf 4 imp , KG,					  ,NonExistent         ,ORG01,            ,1234.56 78 00,			 ,			   ,       ,AU    ,69      ,            ,            ,                         ");
					sw.WriteLine("Pbth,bth 4 imp , KG,					  ,BEERi               ,ORG01,            ,1234.56 78 00,1111		 ,2222X		   ,4000000,AU    ,69      ,            ,            ,                         ");
					sw.WriteLine("Pbth1,bth1 4 imp , KG,				  ,BEERi               ,ORG01,            ,1234.56 78 00,1111		 ,2222X		   ,4000000,AU    ,69      ,   3333;4444,Test Usage  ,Test Description         ");
					sw.WriteLine("Pbth2,bth2 4 imp , KG,				  ,BEERi               ,ORG01,            ,1234.56 78 00,1111		 ,     		   ,4000000,AU    ,69      ,   3333;4444,            ,                         ");
					sw.WriteLine("Pbth3,bth3 4 imp , KG,				  ,BEERi               ,ORG01,            ,1234.56 78 00,    		 ,2222X		   ,4000000,AU    ,69      ,   3333;4444,            ,                         ");
					sw.WriteLine("Pbth4,bth4 4 imp , KG,				  ,BEERi               ,ORG01,            ,1234.56 78 00,    		 ,     		   ,4000000,AU    ,69      ,   3333;4444,            ,                         ");
				}

				dataLoad.ImportProductData(tempFile1.Filename, true, false);
			}

			var secondFactory = new BusinessObjectFactory();

			AssertEquals("Products Created", 8, dataLoad.RunCounters.RecsCreated);
			AssertEquals("Products Updated", 1, dataLoad.RunCounters.RecsUpdated);
			var p00eProduct = secondFactory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "P00e"));
			var p00iProduct = secondFactory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "P00i"));
			var p0TeProduct = secondFactory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "P0Te"));
			var p0TiProduct = secondFactory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "P0Ti"));
			var pbthProduct = secondFactory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "Pbth"));
			AssertEquals(1, p00eProduct.ClassificationsForBinding.Count);  // Classification already linked, not unlinked
			AssertEquals("Suds 4 imp", p00iProduct.OP_Desc);
			AssertEquals(0, p00iProduct.PivotsForBinding.Count);  // No tariff data given, so no pivot created
			AssertEquals(0, p00iProduct.ClassificationsForBinding.Count); // No classification created
			AssertEquals("12345678", p0TeProduct.PivotsForBinding[0].CI_TariffNum);  // Pivot contains tariff data
			AssertEquals(0, p0TeProduct.ClassificationsForBinding.Count); //  No classification created
			AssertEquals(0, p0TiProduct.ClassificationsForBinding.Count);  // No classification created
			AssertEquals("1234567800", p0TiProduct.PivotsForBinding[0].CI_TariffNum);   // Pivot contains tariff data
			AssertEquals(0, pbthProduct.ClassificationsForBinding.Count);  // Classification totally ignored
			AssertEquals("1111", pbthProduct.PivotsForBinding[0].CI_Supplement1); // Pivot contains additional data
			AssertEquals("Length validation is now a messsage error. Truncates only if > CY_CodeMaxLength", "2222X", pbthProduct.PivotsForBinding[0].CI_Supplement2);
			AssertEquals("4000000", pbthProduct.PivotsForBinding[0].CI_CPC);
			AssertEquals("AU", pbthProduct.PivotsForBinding[0].CI_RN_NKCountryOfOrigin);
			AssertEquals(69m, pbthProduct.PivotsForBinding[0].CI_ThirdQty);

			var pbth1Product = secondFactory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "Pbth1"));
			var pbth2Product = secondFactory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "Pbth2"));
			var pbth3Product = secondFactory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "Pbth3"));
			var pbth4Product = secondFactory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "Pbth4"));
			AssertEquals("1111", pbth1Product.PivotsForBinding[0].CI_Supplement1);
			AssertEquals("2222X", pbth1Product.PivotsForBinding[0].CI_Supplement2);
			AssertEquals("3333,4444", pbth1Product.PivotsForBinding[0].CI_AdditionalSupplements);
			AssertEquals("Test Usage", pbth1Product.PivotsForBinding[0].CI_UsageComment);
			AssertEquals("Test Description", pbth1Product.PivotsForBinding[0].CI_Description);
			AssertEquals("1111", pbth2Product.PivotsForBinding[0].CI_Supplement1);
			AssertEquals("3333", pbth2Product.PivotsForBinding[0].CI_Supplement2);
			AssertEquals("4444", pbth2Product.PivotsForBinding[0].CI_AdditionalSupplements);
			AssertEquals("2222X", pbth3Product.PivotsForBinding[0].CI_Supplement1);
			AssertEquals("3333", pbth3Product.PivotsForBinding[0].CI_Supplement2);
			AssertEquals("4444", pbth3Product.PivotsForBinding[0].CI_AdditionalSupplements);
			AssertEquals("3333", pbth4Product.PivotsForBinding[0].CI_Supplement1);
			AssertEquals("4444", pbth4Product.PivotsForBinding[0].CI_Supplement2);
			AssertEquals("", pbth4Product.PivotsForBinding[0].CI_AdditionalSupplements);
		}

		#region Implementation

		protected override GBOrgSupplierPartDataLoad GetNewDataLoader()
		{
			return new GBOrgSupplierPartDataLoad();
		}

		protected override void SetUp()
		{
			base.SetUp();
			moesTavernOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());

			moesTavernOrganisation.OH_Code = "ORG01";
		}

		OrgHeader moesTavernOrganisation;

		#endregion
	}
}
