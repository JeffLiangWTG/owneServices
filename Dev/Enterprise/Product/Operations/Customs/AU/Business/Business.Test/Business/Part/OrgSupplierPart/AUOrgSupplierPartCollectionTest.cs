using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AUOrgSupplierPartCollection))]
	sealed class AUOrgSupplierPartCollectionTest : Customs.Business.Testing.OrgSupplierPartCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AUOrgSupplierPartCollection(Factory);
		}

		public void TestAddingNewPartExport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			Classification cusClass = Factory.New<Classification>();
			cusClass.CC_ClassificationType = Classification.ClassificationType.EXP;
			cusClass.CC_TariffNum = "01213231";

			invoiceLine.JI_Tariff = "0101.10.10";
			invoiceLine.JI_CC = cusClass.PK;
			AUOrgSupplierPartCollection collection = new AUOrgSupplierPartCollection(Factory, invoiceLine, true);
			AUOrgSupplierPart part = collection.AddNew();
			AssertEquals(cusClass.PK, part.PivotsForBinding[0].CI_CC);
			AssertEquals("", part.PivotsForBinding[0].CI_TariffNum);
			AssertEquals("Tariff Number", "0121.32.31", part.PivotsForBinding[0].Classification.CC_TariffNum);

			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0101.10.10";
			collection = new AUOrgSupplierPartCollection(Factory, invoiceLine, invoiceLine.InvoiceHeader?.IsExport ?? ZBool.False);
			part = collection.AddNew();
			AssertEquals(ZGuid.Empty, part.PivotsForBinding[0].CI_CC);
			AssertEquals("0101.10.10", part.PivotsForBinding[0].CI_TariffNum);
		}

		public void TestAddingNewPartWithQurantineExDocLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			Classification cusClass = Factory.New<Classification>();
			cusClass.CC_ClassificationType = Classification.ClassificationType.EXP;
			cusClass.CC_TariffNum = "01213231";

			invoiceLine.JI_Tariff = "0101.10.10";
			invoiceLine.JI_CC = cusClass.PK;

			invoiceLine.QuarantineExDocLine.QL_ProductType = "A";
			invoiceLine.QuarantineExDocLine.QL_SupplimentaryCode = "BB";
			invoiceLine.QuarantineExDocLine.QL_PackType = "BC";
			invoiceLine.QuarantineExDocLine.QL_PreservationType = "C";
			invoiceLine.QuarantineExDocLine.QL_CutCode = "1122";
			invoiceLine.QuarantineExDocLine.QL_Category = "CUT";

			AUOrgSupplierPartCollection collection = new AUOrgSupplierPartCollection(Factory, invoiceLine, invoiceLine.InvoiceHeader?.IsExport ?? ZBool.False);
			AUOrgSupplierPart part = collection.AddNew();
			AssertEquals(cusClass.PK, part.PivotsForBinding[0].CI_CC);
			AssertEquals("", part.PivotsForBinding[0].CI_TariffNum);
			AssertEquals("Tariff Number", "0121.32.31", part.PivotsForBinding[0].Classification.CC_TariffNum);

			var pivotAddInfo = part.PivotsForBinding[0].AddInfo;
			AssertEquals("Produce Type Egg", "EGG", pivotAddInfo.ZA_AQISProduceType_Hidden);
			AssertEquals("Product A", "A", pivotAddInfo.ZA_AQISProduct_Hidden);
			AssertEquals("SupplimentaryCode BB", "BB", pivotAddInfo.ZA_AQISSupplementaryCode_Hidden);
			AssertEquals("PackType", "BC", pivotAddInfo.ZA_AQISPackType_Hidden);
			AssertEquals("PreservationType ", "C", pivotAddInfo.ZA_AQISPreservation_Hidden);
			AssertEquals("CutCode", "1122", pivotAddInfo.ZA_AQISCutCode_Hidden);
			AssertEquals("Category", "CUT", pivotAddInfo.ZA_AQISCategoryCode_Hidden);

			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0101.10.10";
			collection = new AUOrgSupplierPartCollection(Factory, invoiceLine, invoiceLine.InvoiceHeader?.IsExport ?? ZBool.False);
			part = collection.AddNew();

			AssertEquals(ZGuid.Empty, part.PivotsForBinding[0].CI_CC);
			AssertEquals("0101.10.10", part.PivotsForBinding[0].CI_TariffNum);
		}

		public override void TestAddingNewPart()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			Classification cusClass = Factory.New<Classification>();
			cusClass.CC_ClassificationType = Enterprise.Customs.Business.BaseCusClassification.ClassificationType.IMP;
			cusClass.CC_TariffNum = "4901.99.90 03";
			cusClass.AddInfo.AddInfoLine = "DCX=FR*ORG=DE*TCI=BL:2345678";
			cusClass.AddInfo.ZA_TreatmentCode_Hidden = "505";
			cusClass.AddInfo.ZA_InstrumentType_Hidden = "BL";
			cusClass.AddInfo.ZA_InstrumentCode_Hidden = "123";

			invoiceLine.JI_Tariff = "8704.22.00 07";
			invoiceLine.JI_CC = cusClass.PK;
			invoiceLine.JI_AddInfo = "InstrumentCode_Hidden=10183*InstrumentType_Hidden=BL*DRE=1.0000*TreatmentCode_Hidden=505*AMB=D*DMP=1.00*DCX=FR*ORG=DE*TCI=BL:2345678*AQISCommCodes_Hidden=5555*AQISDocuments_Hidden=AVASG/1,IC/2*AQISEntityIds_Hidden=3333*AQISPermitIds_Hidden=2222*AQISPremIdProcessType_Hidden=A0004/RNPNT,A0083/RNPT*AQISProducerCodes_Hidden=4444*InstrumentCode_Hidden=123";
			AUOrgSupplierPartCollection collection = new AUOrgSupplierPartCollection(Factory, invoiceLine, invoiceLine.InvoiceHeader?.IsExport ?? ZBool.False);
			AUOrgSupplierPart part = collection.AddNew();

			var newPivot = part.PivotsForBinding[0];
			AssertEquals(cusClass.PK, newPivot.CI_CC);
			AssertEquals("", newPivot.CI_TariffNum);

			AssertEquals("AddInfoLine", ZString.Empty, newPivot.AddInfo.AddInfoLine);
			AssertEquals("Treatment Code", ZString.Empty, newPivot.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("Instrument Type", ZString.Empty, newPivot.AddInfo.ZA_InstrumentType_Hidden);
			AssertEquals("Instrument Code", ZString.Empty, newPivot.AddInfo.ZA_InstrumentCode_Hidden);

			AssertEquals("AddInfoLine", "DCX=FR*ORG=DE*TCI=BL:2345678", newPivot.EffectiveAddInfo.AddInfoLine);
			AssertEquals("Treatment Code", "505", newPivot.EffectiveAddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("Instrument Type", "BL", newPivot.EffectiveAddInfo.ZA_InstrumentType_Hidden);
			AssertEquals("Instrument Code", "123", newPivot.EffectiveAddInfo.ZA_InstrumentCode_Hidden);
			AssertEquals("Tariff Number", "4901.99.90 03", newPivot.Classification.CC_TariffNum);
			AssertEquals("ZA_AQISPermitIds_Hidden", "2222", newPivot.EffectiveAddInfo.ZA_AQISPermitIds_Hidden);
			AssertEquals("ZA_AQISEntityIds_Hidden", "3333", newPivot.EffectiveAddInfo.ZA_AQISEntityIds_Hidden);
			AssertEquals("ZA_AQISProducerCodes_Hidden", "4444", newPivot.EffectiveAddInfo.ZA_AQISProducerCodes_Hidden);
			AssertEquals("ZA_AQISCommCodes_Hidden", "5555", newPivot.EffectiveAddInfo.ZA_AQISCommCodes_Hidden);
			AssertEquals("ZA_AQISDocuments_Hidden", "AVASG/1,IC/2", newPivot.EffectiveAddInfo.ZA_AQISDocuments_Hidden);
			AssertEquals("ZA_AQISPremIdProcessType_Hidden", "A0004/RNPNT,A0083/RNPT", newPivot.EffectiveAddInfo.ZA_AQISPremIdProcessType_Hidden);

			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8704.22.00 07";
			collection = new AUOrgSupplierPartCollection(Factory, invoiceLine, invoiceLine.InvoiceHeader?.IsExport ?? ZBool.False);
			part = collection.AddNew();

			newPivot = part.PivotsForBinding[0];
			AssertEquals(ZGuid.Empty, newPivot.CI_CC);
			AssertEquals("8704.22.00 07", newPivot.CI_TariffNum);
		}
	}
}
