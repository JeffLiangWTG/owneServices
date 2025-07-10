using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.MasterFiles;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class ClassificationDetailsUpdaterTest : TestCaseWithFactory
	{
		public void TestUpdateWhenJI_CCIsSet()
		{
			var classification = Factory.NewWithValidTestData<CusClassification>();
			classification.CC_TariffNum = "6107110000";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CC = classification.PK;
			AssertEquals("IMP", "6107110000", invoiceLine.JI_Tariff);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_CC = ZGuid.Empty;
			invoiceLine.JI_CC = classification.PK;
			AssertEquals("EXP", "61071100", invoiceLine.JI_Tariff);
		}
	}
}
