using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AQISCommodityCodeCollection))]
	sealed class AQISCommodityCodeCollectionTest : AQISSingleValueCollectionTest<AQISCommodityCodeCollection>
	{
		public override ZPropertyInfo AddInfoProperty => invoiceLine.AddInfo.ZA_AQISCommCodes_HiddenInfo;

		public override AQISSingleValueBusinessObject FirstBizObjToAdd
		{
			get
			{
				var commodityCode = invoiceLine.AQISCommodityCodes.AddNew();
				commodityCode.Code = "1";
				return commodityCode;
			}
		}

		public override AQISSingleValueBusinessObject SecondBizObjToAdd
		{
			get
			{
				var commodityCode = invoiceLine.AQISCommodityCodes.AddNew();
				commodityCode.Code = "2";
				return commodityCode;
			}
		}

		public override AQISSingleValueCollection CollectionToTestWith => invoiceLine.AQISCommodityCodes;

		protected override BusinessObject GetNewElementToAddToTheCollection() => new AQISCommodityCode(Factory);

		protected override AQISCommodityCodeCollection GetCollectionToTest() => new AQISCommodityCodeCollection(Factory);

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = JobDeclaration.New(Factory);
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_AQISCommCodes_Hidden = "1";
		}
		JobComInvoiceLine invoiceLine;
	}
}
