#if DEBUG
using System.Data;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	[CodeProperty(Schema.DummyCodeProperty, nameof(DummyBusinessObjectCodePropertyIsNotSchemaColumn.DummyFilter))]
	public class DummyBusinessObjectCodePropertyIsNotSchemaColumn : DummyBusinessObject
	{
		public new class Schema : AutoDummyBizo.Schema
		{
			public const string DummyCodeProperty = "DummyCodeProperty";
		}

		public DummyBusinessObjectCodePropertyIsNotSchemaColumn(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString DummyCodeProperty
		{
			get { return Z0_Code + " - " + Z0_Number; }
		}

		public static ZQuery DummyFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(DummyBusinessObjectCodePropertyIsNotSchemaColumn));

			return query;
		}
	}
}

#endif
