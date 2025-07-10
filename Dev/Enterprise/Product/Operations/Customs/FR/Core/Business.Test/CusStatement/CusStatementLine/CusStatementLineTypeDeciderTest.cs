using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.CusStatement.Testing
{
	class CusStatementLineTypeDeciderTest : TestCaseWithFactory
	{
		public void TestTypeDecider()
		{
			var statementLine = Factory.New<TestCusStatementLine>();
			var decider = new CusStatementLineTypeDecider();
			statementLine.B3_EntryType = MessageTypeList.Codes.DCG;
			AssertEquals(typeof(CusStatementChargesDetail), decider.GetTypeForLoad(((INeedRow)statementLine).Row, Factory));

			statementLine.B3_EntryType = StatementEntryTypeList.Codes.Export;
			AssertEquals(typeof(CusStatementEntry), decider.GetTypeForLoad(((INeedRow)statementLine).Row, Factory));

			statementLine.B3_EntryType = StatementEntryTypeList.Codes.Import;
			AssertEquals(typeof(CusStatementEntry), decider.GetTypeForLoad(((INeedRow)statementLine).Row, Factory));

			statementLine.B3_EntryType = "BLA";
			AssertEquals(typeof(BaseCusStatementLine), decider.GetTypeForLoad(((INeedRow)statementLine).Row, Factory));
			AssertEquals("The CusStatementLineTypeDecider for FR could not load the object as the  B3_EntryType 'BLA' is unknown. A shared BaseCusStatementLine was returned instead.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public class TestCusStatementLine : BaseCusStatementLine
		{
			public TestCusStatementLine(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}
	}
}
