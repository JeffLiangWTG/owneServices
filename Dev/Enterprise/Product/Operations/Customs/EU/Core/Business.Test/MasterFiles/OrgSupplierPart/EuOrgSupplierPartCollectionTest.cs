using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.MasterFiles.Testing
{
	[TestedType(typeof(EuOrgSupplierPartCollection))]
	class EuOrgSupplierPartCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new EuOrgSupplierPartCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return OrgSupplierPart.New(Factory);
		}

		public virtual void TestAddingNewPart()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = (JobComInvoiceHeader)declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "84314901";
			var collection = new EuOrgSupplierPartCollection(Factory, invoiceLine, false);
			var part = collection.AddNew();
			AssertEquals("Tariff Number", "84314901", part.PivotsForBinding[0].CI_TariffNum);
		}
	}
}
