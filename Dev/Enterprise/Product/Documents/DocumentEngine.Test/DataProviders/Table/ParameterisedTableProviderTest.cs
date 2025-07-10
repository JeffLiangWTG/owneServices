using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.RuntimeOptions;
using NUnit.Framework;
using static Enterprise.DocumentEngine.DataProviders.ParameterisedTableProvider;

namespace Enterprise.DocumentEngine.DataProviders.Testing
{
	public class ParameterisedTableProviderTest : TransactionedTestCase
	{
		public ParameterisedTableProviderTest() : base()
		{ }

		class ExpectsNoParametersDataProvider : ParameterisedTableProvider
		{
			protected override DataTable GetDataTable()
			{
				return new DataTable(this.ToString());
			}

			internal new DataTable GetDataTable(string dataSourceString, CollectionOfIFilter filters, Report report) => base.GetDataTable(dataSourceString, filters, report);
		}

		class UsesWhereClauseAndSqlParamsDataProvider : ParameterisedTableProvider
		{
			protected override DataTable GetDataTable()
			{
				return new DataTable(this.ToString() + WhereClause + SqlParameters[0].Value);
			}

			internal new DataTable GetDataTable(string dataSourceString, CollectionOfIFilter filters, Report report) => base.GetDataTable(dataSourceString, filters, report);
		}

		class ExpectsThreeParametersDataProvider : ParameterisedTableProvider
		{
			string SomeString;
			ZInt SomeDecimal;
			ZDateTime SomeDateTime;

			protected override DataTable GetDataTable()
			{
				return new DataTable(this.ToString() + SomeString + SomeDecimal + SomeDateTime);
			}
			
			internal new DataTable GetDataTable(string dataSourceString, CollectionOfIFilter filters, Report report) =>
				base.GetDataTable(dataSourceString, filters, report);

			protected override Parameter[] ExpectedParameters()
			{
				return new Parameter[] {
											   new Parameter("Some String", typeof(string)),
											   new Parameter("Some Int", typeof(ZInt)),
											   new Parameter("Some DateTime", typeof(ZDateTime))
										   };
			}

			protected override void SetupParameterValues(object[] values)
			{
				SomeString = (string)values[0];
				SomeDecimal = (ZInt)values[1];
				SomeDateTime = (ZDateTime)values[2];
			}
		}

		public void TestExpectsNoParameters()
		{
			ExpectsNoParametersDataProvider p = new ExpectsNoParametersDataProvider();
			AssertEquals("GetDataTable", p.ToString(), p.GetDataTable("data:a=b", new CollectionOfIFilter(), null).ToString());
		}

		public void TestExpectsParametersAllValid()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			ExpectsThreeParametersDataProvider p = new ExpectsThreeParametersDataProvider();
			CollectionOfIFilter f = new CollectionOfIFilter();

			TextField t = new TextField(factory);
			t.DisplayName = "T";
			t.Value = "foobar";
			f.Add(t);

			NumberField n = new NumberField(factory);
			n.DisplayName = "N";
			n.Value = new ZInt(42);
			f.Add(n);

			DateField d = new DateField(factory);
			d.DisplayName = "D";
			d.Value = new DateTime(1990, 3, 13);
			f.Add(d);

			AssertEquals("GetDataTable", p.ToString() + t.Value + n.Value.ToString() + d.Value.ToString(), p.GetDataTable("data:x=y(<T>, <N>, <D>)", f, null).ToString());
		}

		[ExpectException(typeof(MissingParameterException))]
		public void TestExpectsParametersWrongNumberOfParams()
		{
			ExpectsThreeParametersDataProvider p = new ExpectsThreeParametersDataProvider();
			CollectionOfIFilter f = new CollectionOfIFilter();

			TextField t = new TextField(new BusinessObjectFactory());
			t.DisplayName = "T";
			t.Value = "foobar";
			f.Add(t);

			NumberField n = new NumberField(new BusinessObjectFactory());
			n.DisplayName = "N";
			n.Value = 42;
			f.Add(n);

			DateField d = new DateField(new BusinessObjectFactory());
			d.DisplayName = "D";
			d.Value = new DateTime(1990, 3, 13);
			f.Add(d);

			p.GetDataTable("data:x=y(<T>, <N>)", f, null);
		}

		[ExpectException(typeof(InvalidParameterTypeException))]
		public void TestExpectsParametersWrongTypeOfParams()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			ExpectsThreeParametersDataProvider p = new ExpectsThreeParametersDataProvider();
			CollectionOfIFilter f = new CollectionOfIFilter();

			TextField t = new TextField(factory);
			t.DisplayName = "T";
			t.Value = "foobar";
			f.Add(t);

			TextField n = new TextField(factory);
			n.DisplayName = "N";
			n.Value = "hi";
			f.Add(n);

			DateField d = new DateField(factory);
			d.DisplayName = "D";
			d.Value = new DateTime(1990, 3, 13);
			f.Add(d);

			p.GetDataTable("data:x=y(<T>, <N>, <D>)", f, null);
		}

		[ExpectException(typeof(MissingFilterException))]
		public void TestExpectsParametersNamedFilterDoesNotExist()
		{
			ExpectsThreeParametersDataProvider p = new ExpectsThreeParametersDataProvider();
			CollectionOfIFilter f = new CollectionOfIFilter();

			TextField t = new TextField(new BusinessObjectFactory());
			t.DisplayName = "T";
			t.Value = "foobar";
			f.Add(t);

			NumberField n = new NumberField(new BusinessObjectFactory());
			n.DisplayName = "Oops!";
			n.Value = 42;
			f.Add(n);

			DateField d = new DateField(new BusinessObjectFactory());
			d.DisplayName = "D";
			d.Value = new DateTime(1990, 3, 13);
			f.Add(d);

			p.GetDataTable("data:x=y(<T>, <N>, <D>)", f, null);
		}

		public void TestGetsWhereClauseAndSqlParameters()
		{
			UsesWhereClauseAndSqlParamsDataProvider p = new UsesWhereClauseAndSqlParamsDataProvider();
			CollectionOfIFilter f = new CollectionOfIFilter();

			NumberField n = new NumberField(new BusinessObjectFactory());
			n.DisplayName = "N";
			n.FieldName = "XX_Number";
			n.Value = 42;
			f.Add(n);

			AssertEquals("GetDataTable", p.ToString() + f.WhereClause() + f.SqlParameters()[0].Value, p.GetDataTable("", f, null).ToString());
		}
	}
}
