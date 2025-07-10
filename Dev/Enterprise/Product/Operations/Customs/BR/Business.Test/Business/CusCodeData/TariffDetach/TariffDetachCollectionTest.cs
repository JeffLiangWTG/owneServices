using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(TariffDetachCollection))]
	class TariffDetachCollectionTest : CusCodeDataCollectionTest<TariffDetach>
	{
		protected override CusCodeDataCollection<TariffDetach> GetCusCodeDataCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new TariffDetachCollection(jobComInvoice);
		}

		public void TestReadOnly()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var invHeader = declaration.Invoices.AddNew();
			var invLine1 = invHeader.InvoiceLines.AddNew();

			var invLine2 = invHeader.InvoiceLines.AddNew();
			invLine2.JI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			invLine2.JI_ParentID = ZGuid.NewZGuid();

			Assert("TariffDetachCollection should NOT be ReadOnly", !invLine1.TariffDetachs.ReadOnly);
			Assert("Cloned TariffDetachCollection should be ReadOnly", invLine2.TariffDetachs.ReadOnly);
		}
	}
}

