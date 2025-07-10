using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CargoControlNumberCollection))]
	sealed class CACargoControlNumberCollectionTest : ActiveBusinessObjectCollectionTestCase<CargoControlNumberCollection>
	{
		public void TestAddNewWithCCNumber()
		{
			var ccNumber = "ccn";
			var coll = GetCollectionToTest();
			var ccn = coll.AddNew(ccNumber);
			AssertEquals(ccNumber, ccn.CY_CargoControlNumber);
		}

		public void TestRemoveAndDelete()
		{
			var coll = GetCollectionToTest();
			var ccn = coll.AddNew();
			AssertEquals("Collection 1 element", 1, coll.Count);
			coll.RemoveAndDelete(ccn);
			AssertEquals("Collection 0 element", 0, coll.Count);
		}

		public void TestRemoveAndDeleteAll()
		{
			var coll = GetCollectionToTest();
			coll.AddNew();

			AssertNoExceptionThrown(() => coll.RemoveAndDeleteAll());
			AssertEquals(0, coll.Count);
		}

		public void TestNoExceptionThrownWhenDeleteCCN()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var ccn2 = declaration.CargoControlNumbers.AddNew();
			ccn2.CY_CargoControlNumber = "CCN12345678";

			var ccn = declaration.AdditionalReferenceNumbers.AddNew();
			ccn.CE_EntryNum = "CCN12345678";
			ccn.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			var releaseStatuses = declaration.ReleaseStatuses.Cast<ReleaseStatus>().First();
			Factory.Save();

			releaseStatuses.CACCN.Delete();

			AssertNoExceptionThrown(() => declaration.CargoControlNumbers.RemoveAndDelete(releaseStatuses.CACCN));
		}

		protected override CargoControlNumberCollection GetCollectionToTest()
		{
			return new CargoControlNumberCollection(Factory.New<JobDeclaration>());
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
		}
	}
}
