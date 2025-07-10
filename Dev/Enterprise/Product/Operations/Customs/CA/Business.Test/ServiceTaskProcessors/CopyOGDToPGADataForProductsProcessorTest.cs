using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CopyOGDToPGADataForProductsProcessorTest : TestCaseWithFactory
	{
		[TestDate(2017, 12, 2)]
		public void TestFullProcessCycle()
		{
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, "TC", "3824600000");
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, "CFIA", "3824600000");
			var product1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product1.OP_PartNum = "LON01";
			var pivot11 = product1.PivotsForBinding.AddNew();
			pivot11.CCA_AirsCode = "A001";
			var pivot12 = product1.PivotsForBinding.AddNew();
			pivot12.CCA_AirsCode = "A001";

			var product2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product2.OP_PartNum = "LON02";
			var pivot2 = product2.PivotsForBinding.AddNew();
			pivot2.CCA_EndUse = "05";

			var product3 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product3.OP_PartNum = "LON03";
			var pivot3 = product3.PivotsForBinding.AddNew();
			pivot3.CCA_MiscID = "011";

			var product4 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product4.OP_PartNum = "LON04";
			var pivot4 = product4.PivotsForBinding.AddNew();
			pivot4.CCA_ImportReasonCode = "06";
			pivot4.CI_TariffNum = "3824600000";

			var product5 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product5.OP_PartNum = "LON05";
			var pivot5 = product5.PivotsForBinding.AddNew();
			var registrationNumber = pivot5.CFIARegistrationNumbers.AddNew();
			registrationNumber.CY_Code = "2";
			registrationNumber.CY_Data = "XCV0001";

			var product6 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product6.OP_PartNum = "LON06";

			var product7 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product7.OP_PartNum = "LON07";
			var pivot7 = product7.PivotsForBinding.AddNew();
			pivot7.CCA_ImportReasonCode = "06";
			pivot7.CCA_TCIndicator = "Y";

			var product8 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product8.OP_PartNum = "LON08";
			var pivot8 = product8.PivotsForBinding.AddNew();
			pivot8.CCA_EndUse = "05";
			pivot8.CCA_CFIAIndicator = "Y";

			var product9 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product9.OP_PartNum = "LON09";
			var pivot9 = product9.PivotsForBinding.AddNew();
			pivot9.CCA_ImportReasonCode = "0";

			Factory.Save();

			var indicationResult = CopyOGDToPGADataForProductsProcessor.GetIndicationForCopyingOGDToPGAData();

			AssertEquals("A new transformation cycle has be successfully triggered. 5 product(s) registered to be transformed. This will be processed in the background by the CCP service task.", indicationResult.Item1.ToString());
			Assert(indicationResult.Item2);

			var logger = new SimpleLogger();
			var processor = new CopyOGDToPGADataForProductsProcessor(logger);
			processor.Process(new CancellationToken());

			AssertProcessResult(pivot11.PK, "Y", string.Empty, "A001", string.Empty, string.Empty, string.Empty);
			AssertProcessResult(pivot12.PK, "Y", string.Empty, "A001", string.Empty, string.Empty, string.Empty);
			AssertProcessResult(pivot2.PK, "Y", string.Empty, string.Empty, "05", string.Empty, string.Empty);
			AssertProcessResult(pivot3.PK, "Y", string.Empty, string.Empty, string.Empty, "011", string.Empty);
			AssertProcessResult(pivot4.PK, "Y", "Y", string.Empty, string.Empty, string.Empty, "TC04");

			AssertProcessResult(pivot5.PK, "Y", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			var reloadedPivot5 = Factory.Load<CusClassPartPivot>(pivot5.PK);
			AssertEquals(1, reloadedPivot5.CFIAPGAHeader.LPCOViews.Count);
			var lpcoView = (LPCOView)reloadedPivot5.CFIAPGAHeader.LPCOViews.First();
			AssertEquals("2", lpcoView.CLP_Type);
			AssertEquals("XCV0001", lpcoView.CLP_RefNo);

			AssertProcessResult(pivot7.PK, string.Empty, "Y", string.Empty, string.Empty, string.Empty, string.Empty);
			AssertProcessResult(pivot8.PK, "Y", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertProcessResult(pivot9.PK, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

			AssertEquals(@"Start Copy OGD data to PGA on products
Successfully copied OGD data to PGA for 5 products.
", logger.ToString());

			indicationResult = CopyOGDToPGADataForProductsProcessor.GetIndicationForCopyingOGDToPGAData();
			AssertEquals("There is no product to be transformed.", indicationResult.Item1.ToString());
			Assert(!indicationResult.Item2);

			logger = new SimpleLogger();
			processor = new CopyOGDToPGADataForProductsProcessor(logger);
			processor.Process(new CancellationToken());

			AssertEquals(@"Start Copy OGD data to PGA on products
There is no product to be transformed.
", logger.ToString());
		}

		public void TestOnlyCopyTCForTCTariff()
		{
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, "TC", "3824600000");
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, "CFIA", "3824600000");
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			product.OP_PartNum = "LON01";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CCA_ImportReasonCode = ImportReasonCodes.Codes.Sale;
			pivot.CI_TariffNum = "3824500000";
			pivot.CCA_Model = "NRCAN MODEL";
			pivot.CCA_ModelNumber = "NRCAN PRODUCT";
			pivot.CCA_BrandName = "NRCAN BRAND";

			Factory.Save();

			var indicationResult = CopyOGDToPGADataForProductsProcessor.GetIndicationForCopyingOGDToPGAData();

			AssertEquals("A new transformation cycle has be successfully triggered. 1 product(s) registered to be transformed. This will be processed in the background by the CCP service task.", indicationResult.Item1.ToString());
			Assert(indicationResult.Item2);

			var logger = new SimpleLogger();
			var processor = new CopyOGDToPGADataForProductsProcessor(logger);
			processor.Process(new CancellationToken());

			AssertProcessResult(pivot.PK, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

			pivot.CI_TariffNum = "3824600000";
			Factory.Save();
			indicationResult = CopyOGDToPGADataForProductsProcessor.GetIndicationForCopyingOGDToPGAData();
			AssertEquals("A new transformation cycle has be successfully triggered. 1 product(s) registered to be transformed. This will be processed in the background by the CCP service task.", indicationResult.Item1.ToString());
			Assert(indicationResult.Item2);

			logger = new SimpleLogger();
			processor = new CopyOGDToPGADataForProductsProcessor(logger);
			processor.Process(new CancellationToken());
			AssertProcessResult(pivot.PK, "Y", "Y", string.Empty, string.Empty, string.Empty, TCIntendedUseCodes.Codes.TC01);
		}

		void AssertProcessResult(ZGuid pivotPK, ZString cfiaInd, ZString tcInd, ZString airsCode, ZString endUseCode, ZString miscID, ZString importReasonCode)
		{
			var reloadedPivot = Factory.Load<CusClassPartPivot>(pivotPK);
			var reloadedProduct = Factory.Load<OrgSupplierPart>(reloadedPivot.CI_OP);

			CombineAssertions(() =>
			{
				AssertEquals(0, LoadAddedGenAddOnColumnCount(reloadedProduct.PK));

				AssertEquals("CFIA PGA enabled", cfiaInd, reloadedPivot.CCA_CFIAIndicator);
				AssertEquals("CA_AIRSExtensionCode for CFIAPGAHeader", airsCode, reloadedPivot.CFIAPGAHeader.CA_AIRSExtensionCode);
				AssertEquals("CA_AIRSEndUse for CFIAPGAHeader", endUseCode, reloadedPivot.CFIAPGAHeader.CA_AIRSEndUse);
				AssertEquals("CA_AIRSMiscellaneous for CFIAPGAHeader", miscID, reloadedPivot.CFIAPGAHeader.CA_AIRSMiscellaneous);

				AssertEquals("TC PGA enabled", tcInd, reloadedPivot.CCA_TCIndicator);
				AssertEquals("TPR enabled", !importReasonCode.IsEmpty, reloadedPivot.TCPGAHeader.CA_TPRProgramInd == YesNoList.Codes.Yes);
				AssertEquals("CA_ImportReasonCode for TCPGAHeader", importReasonCode, reloadedPivot.TCPGAHeader.CA_ImportReasonCode);

				var edtEventZQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
				edtEventZQuery.AddToFilter(StmALogSchema.SL_Reference, "Copy OGD data to PGA");

				AssertEquals("EDT event added", !airsCode.IsEmpty || !endUseCode.IsEmpty || !miscID.IsEmpty || reloadedPivot.CFIARegistrationNumbers.Any() || !importReasonCode.IsEmpty
					, reloadedProduct.Logs.HasLogWith(edtEventZQuery));
			});
		}

		int LoadAddedGenAddOnColumnCount(ZGuid productPK)
		{
			var genAddOnQuery = new ZDBOnlyQuery(typeof(GenAddOnColumn));
			genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, "CopyOGDToPGAForProduct");
			genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, OrgSupplierPartSchema.Constants.Prefix);
			genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentID, productPK);

			return Factory.GetDatabaseCount(typeof(GenAddOnColumn), genAddOnQuery);
		}
	}
}
