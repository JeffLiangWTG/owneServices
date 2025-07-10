using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.EInvoicing.Testing;
using Enterprise.MasterFiles.Business.CustomValues;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.EInvoicing.Fiji.Testing
{
	[TestedType(typeof(FijiAccTransactionHeaderAuthorisationRecord))]
	public class FijiAccTransactionHeaderAuthorisationRecordTest : AccTransactionHeaderAuthorisationRecordWithGenAddOnColumnsTest
	{
		public override void TestGenAddOnColumnNameAndTypes()
		{
			var fijiAuthorizationRecord = Factory.NewWithValidTestData<FijiAccTransactionHeaderAuthorisationRecord>();
			fijiAuthorizationRecord.BusinessName = "Business1";
			fijiAuthorizationRecord.LocationName = "LocationName1";
			fijiAuthorizationRecord.Address = "Address1";
			fijiAuthorizationRecord.District = "District1";
			AssertGenAddOnColumn(nameof(fijiAuthorizationRecord.BusinessName), fijiAuthorizationRecord.BusinessName);
			AssertGenAddOnColumn(nameof(fijiAuthorizationRecord.LocationName), fijiAuthorizationRecord.LocationName);
			AssertGenAddOnColumn(nameof(fijiAuthorizationRecord.Address), fijiAuthorizationRecord.Address);
			AssertGenAddOnColumn(nameof(fijiAuthorizationRecord.District), fijiAuthorizationRecord.District);
		}

		protected override GenAddOnColumnCollection GenAddOnColumns =>
			Factory.NewWithValidTestData<FijiAccTransactionHeaderAuthorisationRecordWtihGenAddOnColumnsExposed>().GetAddOnColumns();

		public class FijiAccTransactionHeaderAuthorisationRecordWtihGenAddOnColumnsExposed : FijiAccTransactionHeaderAuthorisationRecord
		{
			public FijiAccTransactionHeaderAuthorisationRecordWtihGenAddOnColumnsExposed(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public GenAddOnColumnCollection GetAddOnColumns() => AddOnColumns;
		}
	}
}
