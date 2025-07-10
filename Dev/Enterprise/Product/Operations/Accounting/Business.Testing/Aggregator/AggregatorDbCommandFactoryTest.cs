using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Aggregator.Testing
{
	public class AggregatorDbCommandFactoryTest : TestCase
	{
		public void TestAppendQueryLineGetCommands()
		{
			DbCommand[] commands = DbCommandFactory.GetCommands();
			AssertEquals("Two commands should be created", 2, commands.Length);
			AssertEquals("First command's text", "@param1@param2", commands[0].CommandText);
			AssertEquals("Second command's text", "@param3, @param4, @param5", commands[1].CommandText);
		}

		public void TestGetCommandForLine()
		{
			AssertEquals("First command's text", "@param1@param2", DbCommandFactory.GetCommandForLine(0).CommandText);
			AssertEquals("Second command's text", "@param1@param2", DbCommandFactory.GetCommandForLine(1).CommandText);
			AssertEquals("Third command's text", "@param3, @param4, @param5", DbCommandFactory.GetCommandForLine(2).CommandText);
		}

		#region Implementation

		AggregatorDbCommandFactory DbCommandFactory;

		protected override void SetUp()
		{
			base.SetUp();
			DbCommandFactory = new AggregatorDbCommandFactory(4);
			DbCommandFactory.AppendQueryLine(0, "@param1");
			DbCommandFactory.AppendQueryLine(1, "@param2");
			DbCommandFactory.AppendQueryLine(2, "@param3, @param4, @param5");
		}

		#endregion
	}
}