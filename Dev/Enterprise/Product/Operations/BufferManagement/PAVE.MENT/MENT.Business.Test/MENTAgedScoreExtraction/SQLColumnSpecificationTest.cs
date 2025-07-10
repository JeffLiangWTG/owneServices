using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Business.Test
{
	[TestedType(typeof(SQLColumnSpecification))]
	class SQLColumnSpecificationTest : NonPersistentBusinessObjectTestCase
	{
		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return "Code";
				yield return "Column";
				yield return "ColumnFunction";
				yield return "ColumnType";
				yield return "RequireCoalesce";
				yield return "Selected";
				yield return "Sequence";
			}
		}
	}
}
