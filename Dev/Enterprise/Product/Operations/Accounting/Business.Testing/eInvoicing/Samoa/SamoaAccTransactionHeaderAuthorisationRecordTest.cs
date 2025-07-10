using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.EInvoicing.Testing;
using Enterprise.MasterFiles.Business.CustomValues;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.EInvoicing.Samoa.Testing
{
	[TestedType(typeof(SamoaAccTransactionHeaderAuthorisationRecord))]
	public class SamoaAccTransactionHeaderAuthorisationRecordTest : AccTransactionHeaderAuthorisationRecordWithGenAddOnColumnsTest
	{
		public override void TestGenAddOnColumnNameAndTypes()
		{
			var samoaAuthorizationRecord = Factory.NewWithValidTestData<SamoaAccTransactionHeaderAuthorisationRecord>();
			samoaAuthorizationRecord.BusinessName = "Business1";
			samoaAuthorizationRecord.LocationName = "LocationName1";
			samoaAuthorizationRecord.Address = "Address1";
			samoaAuthorizationRecord.District = "District1";
			AssertGenAddOnColumn(nameof(samoaAuthorizationRecord.BusinessName), samoaAuthorizationRecord.BusinessName);
			AssertGenAddOnColumn(nameof(samoaAuthorizationRecord.LocationName), samoaAuthorizationRecord.LocationName);
			AssertGenAddOnColumn(nameof(samoaAuthorizationRecord.Address), samoaAuthorizationRecord.Address);
			AssertGenAddOnColumn(nameof(samoaAuthorizationRecord.District), samoaAuthorizationRecord.District);
		}

		protected override GenAddOnColumnCollection GenAddOnColumns =>
			Factory.NewWithValidTestData<SamoaAccTransactionHeaderAuthorisationRecordWtihGenAddOnColumnsExposed>().GetAddOnColumns();

		public class SamoaAccTransactionHeaderAuthorisationRecordWtihGenAddOnColumnsExposed : SamoaAccTransactionHeaderAuthorisationRecord
		{
			public SamoaAccTransactionHeaderAuthorisationRecordWtihGenAddOnColumnsExposed(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public GenAddOnColumnCollection GetAddOnColumns() => AddOnColumns;
		}
	}
}

