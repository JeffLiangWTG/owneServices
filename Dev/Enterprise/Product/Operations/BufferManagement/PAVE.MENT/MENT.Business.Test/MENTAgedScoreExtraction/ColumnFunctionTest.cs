using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.PAVE.MENT.Shared;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Business.Test
{
	[TestedType(typeof(ColumnFunction))]
	class ColumnFunctionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetFunctionFormatNone()
		{
			var columnFunction = new ColumnFunction(MENTConstants.DecimalColumn);

			columnFunction.FunctionType = ColumnFunctionTypes.Codes.None;

			AssertEquals("{0}", columnFunction.GetFunctionAsStringWithFormat());
		}

		public void TestGetFunctionFormatRound()
		{
			var columnFunction = new ColumnFunction(MENTConstants.DecimalColumn);

			columnFunction.FunctionType = ColumnFunctionTypes.Codes.Round;
			columnFunction.Parameter1 = 10;

			AssertEquals("FLOOR({0} / 10) * 10", columnFunction.GetFunctionAsStringWithFormat());
		}

		public void TestGetFunctionFormatRound_NoParameter()
		{
			var columnFunction = new ColumnFunction(MENTConstants.DecimalColumn);

			columnFunction.FunctionType = ColumnFunctionTypes.Codes.Round;

			AssertEquals("FLOOR({0})", columnFunction.GetFunctionAsStringWithFormat());
		}

		public void TestParameter1Readonly()
		{
			var columnFunction = new ColumnFunction(MENTConstants.DecimalColumn);

			columnFunction.FunctionType = ColumnFunctionTypes.Codes.Round;

			AssertEquals(false, columnFunction.Parameter1Info.ReadOnly);

			columnFunction.FunctionType = ColumnFunctionTypes.Codes.None;

			AssertEquals(true, columnFunction.Parameter1Info.ReadOnly);
		}

		public void TestColumnFunctionPersistence()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "George Patton loves his peanuts");
			var scoreColumn = extraction.CategoryColumns.Cast<SQLColumnSpecification>().First(c => c.Code == MENTColumns.Codes.Score);

			AssertEquals(MENTConstants.DecimalColumn, scoreColumn.ColumnFunction.ColumnType);

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var loadedExtraction = anotherFactory.Load<MENTAgedScoreExtraction>(extraction.PK);
			var loadedScoreColumn = loadedExtraction.CategoryColumns.Cast<SQLColumnSpecification>().First(c => c.Code == MENTColumns.Codes.Score);

			AssertEquals(MENTConstants.DecimalColumn, scoreColumn.ColumnFunction.ColumnType);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ColumnFunction(MENTConstants.StringColumn);
		}

		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return "FunctionType";
				yield return "Parameter1";
			}
		}

		#endregion
	}
}
