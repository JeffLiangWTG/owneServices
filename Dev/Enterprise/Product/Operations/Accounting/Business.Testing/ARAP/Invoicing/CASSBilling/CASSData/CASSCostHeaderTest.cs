using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	public abstract class CASSCostHeaderTest : CASSDataTest
	{
		public void TestDelete()
		{
			var line = CreatNewCASSCostLine();
			CASSCostHeaderForTest.Lines.Add(line);
			CASSCostHeaderForTest.Delete();

			Assert("Header is deleted.", CASSCostHeaderForTest.IsDeleted);
			Assert("Line is deleted.", line.IsDeleted);
		}

		public void TestReadonlyProperties()
		{
			base.AssertCommonReadonlyProperties();
			Assert("DatePeriodStart", CASSCostHeaderForTest.DatePeriodStartInfo.ReadOnly);
			Assert("DatePeriodEnd", CASSCostHeaderForTest.DatePeriodEndInfo.ReadOnly);
			Assert("DateOfBilling", CASSCostHeaderForTest.DateOfBillingInfo.ReadOnly);
			Assert("BillingCurrency", CASSCostHeaderForTest.BillingCurrencyInfo.ReadOnly);
		}

		public void TestUpdateOriginalAmounts()
		{
			var cassLine = CreatNewCASSCostLine();
			CASSCostHeaderForTest.Lines.Add(cassLine);
			cassLine.CurrencyCode = "AUD";
			cassLine.AirlinePrefix = "MH375";
			AssertEquals("Line Object Has Changed", true, cassLine.HasChanges);

			CASSCostHeaderForTest.UpdateOriginalAmountFields();
			AssertEquals("Line Object Has Changed", false, cassLine.HasChanges);
		}

		protected CASSCostHeader CASSCostHeaderForTest
		{
			get { return CASSDataForTest as CASSCostHeader; }
		}

		protected abstract CASSCostLine CreatNewCASSCostLine();
	}

	[TestedType(typeof(CASSCostHeader))]
	public class CASSCostExportHeaderTest : CASSCostHeaderTest
	{
		protected override CASSData GetCASSData()
		{
			var header = new CASSCostHeader();
			header.InitializeAsExportCASS();
			return header;
		}

		protected override CASSCostLine CreatNewCASSCostLine() => new CASSCostExportLine(Factory, CASSCostLineType.Billing);
	}

	[TestedType(typeof(CASSCostHeader))]
	public class CASSCostImportHeaderTest : CASSCostHeaderTest
	{
		protected override CASSData GetCASSData()
		{
			var header = new CASSCostHeader();
			header.InitializeAsImportCASS();
			return header;
		}

		protected override CASSCostLine CreatNewCASSCostLine() => new CASSCostImportLine(Factory, CASSCostLineType.Default);
	}

	[TestedType(typeof(CASSCostHeader))]
	public class CASSCostHeaderValueObjectTest : ValueObjectTestCase
	{
	}
}
