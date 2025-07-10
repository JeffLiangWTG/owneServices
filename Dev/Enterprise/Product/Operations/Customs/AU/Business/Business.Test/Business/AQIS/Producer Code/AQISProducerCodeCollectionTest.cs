using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AQISProducerCodeCollection))]
	sealed class AQISProducerCodeCollectionTest : AQISSingleValueCollectionTest<AQISProducerCodeCollection>
	{
		public override ZPropertyInfo AddInfoProperty => invoiceLine.AddInfo.ZA_AQISProducerCodes_HiddenInfo;

		public override AQISSingleValueBusinessObject FirstBizObjToAdd
		{
			get
			{
				var producer = invoiceLine.AQISProducerCodes.AddNew();
				producer.Code = "1";
				return producer;
			}
		}

		public override AQISSingleValueBusinessObject SecondBizObjToAdd
		{
			get
			{
				var producer = invoiceLine.AQISProducerCodes.AddNew();
				producer.Code = "2";
				return producer;
			}
		}

		public override AQISSingleValueCollection CollectionToTestWith => invoiceLine.AQISProducerCodes;

		protected override BusinessObject GetNewElementToAddToTheCollection() => new AQISProducerCode(Factory);

		protected override AQISProducerCodeCollection GetCollectionToTest() => new AQISProducerCodeCollection(Factory);

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = JobDeclaration.New(Factory);
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden = "1";
		}
		JobComInvoiceLine invoiceLine;
	}
}
