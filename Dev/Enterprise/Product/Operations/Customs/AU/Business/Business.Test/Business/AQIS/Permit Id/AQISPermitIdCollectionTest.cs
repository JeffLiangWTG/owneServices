using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AQISPermitIdCollection))]
	sealed class AQISPermitIdCollectionTest : AQISSingleValueCollectionTest<AQISPermitIdCollection>
	{
		public override ZPropertyInfo AddInfoProperty => invoiceLine.AddInfo.ZA_AQISPermitIds_HiddenInfo;

		public override AQISSingleValueBusinessObject FirstBizObjToAdd
		{
			get
			{
				var permitId = invoiceLine.AQISPermitIds.AddNew();
				permitId.Code = "1";
				return permitId;
			}
		}

		public override AQISSingleValueBusinessObject SecondBizObjToAdd
		{
			get
			{
				var permitId = invoiceLine.AQISPermitIds.AddNew();
				permitId.Code = "2";
				return permitId;
			}
		}

		public override AQISSingleValueCollection CollectionToTestWith => invoiceLine.AQISPermitIds;

		protected override BusinessObject GetNewElementToAddToTheCollection() => new AQISPermitId(Factory);

		protected override AQISPermitIdCollection GetCollectionToTest() => new AQISPermitIdCollection(Factory);

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = JobDeclaration.New(Factory);
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_AQISPermitIds_Hidden = "1";
		}
		JobComInvoiceLine invoiceLine;
	}
}
