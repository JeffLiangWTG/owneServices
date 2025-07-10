using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AQISEntityIdCollection))]
	sealed class AQISEntityIdCollectionTest : AQISSingleValueCollectionTest<AQISEntityIdCollection>
	{
		public override ZPropertyInfo AddInfoProperty => invoiceLine.AddInfo.ZA_AQISEntityIds_HiddenInfo;

		public override AQISSingleValueBusinessObject FirstBizObjToAdd
		{
			get
			{
				var entityId = invoiceLine.AQISEntityIds.AddNew();
				entityId.Code = "1";
				return entityId;
			}
		}

		public override AQISSingleValueBusinessObject SecondBizObjToAdd
		{
			get
			{
				var entityId = invoiceLine.AQISEntityIds.AddNew();
				entityId.Code = "2";
				return entityId;
			}
		}

		public override AQISSingleValueCollection CollectionToTestWith => invoiceLine.AQISEntityIds;

		protected override BusinessObject GetNewElementToAddToTheCollection() => new AQISEntityId(Factory);

		protected override AQISEntityIdCollection GetCollectionToTest() => new AQISEntityIdCollection(Factory);

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = JobDeclaration.New(Factory);
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_AQISEntityIds_Hidden = "1";
		}
		JobComInvoiceLine invoiceLine;
	}
}
