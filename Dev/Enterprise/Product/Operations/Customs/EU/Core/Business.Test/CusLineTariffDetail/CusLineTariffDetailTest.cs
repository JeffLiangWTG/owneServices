using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(CusLineTariffDetail))]
	public class CusLineTariffDetailTest : Customs.Business.Testing.CusLineTariffDetailTest
	{
		[ExpectNoExceptions]
		public void TestLookups()
		{
			NUnit.Framework.Assert.That(tariffDetail.Lookups, NUnit.Framework.Is.TypeOf<CusLineTariffDetailLookups>());
		}

		[ExpectNoExceptions]
		public void TestValidation()
		{
			NUnit.Framework.Assert.That(tariffDetail.Validation, NUnit.Framework.Is.TypeOf<CusLineTariffDetailValidation>());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var dec = Factory.New<JobDeclaration>();
			var invHeader = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			return invLine.CusLineTariffDetails.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			var dec = Factory.New<JobDeclaration>();
			var invHeader = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invLine = invHeader.InvoiceLines.AddNew();
			tariffDetail = invLine.CusLineTariffDetails.AddNew();
		}

		JobComInvoiceLine invLine;
		CusLineTariffDetail tariffDetail;

		#endregion
	}
}
