using System;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.SDF.Testing
{
	[TestedType(typeof(StmSystemDefinedFieldColumn))]
	sealed class StmSystemDefinedFieldColumnTest : StmSystemDefinedFieldBaseTestCase
	{
		public void TestLookupsType()
		{
			AssertEquals("Lookups.GetType()", typeof(StmSystemDefinedFieldColumnLookups), Field.Lookups.GetType());
		}

		#region Implementation

		protected override Type ExpectedValidationType
		{
			get { return typeof(StmSystemDefinedFieldColumnValidation); }
		}

		#endregion
	}
}
