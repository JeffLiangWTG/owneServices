using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	public class BMQueryParameterisationHelperTest : TestCase
	{
		public void TestReplaceAutoParamNames_ShouldIgnoreTableValuedParameters()
		{
			var column = new SchemaStringColumn(ProcessTasksSchema.Instance, "Quadrant", 1, SqlDbType.NVarChar, string.Empty, false, 100, tvpName: "@TVP"); // to allow table valued parameters
			var parameters = new ZSqlParameterCollection();
			parameters.Add("@CWO_jan", "Hamuri", column);
			parameters.Add(ZSqlParameter.New("@michael", new[] { "Ghost in a Jar", "Pencilvester", "Photography Raptor" }, column, isTableValued: true));
			parameters.Add("@CWO_vincent", "Reverse Giraffe", column);

			var textAndParameters = new QueryTextAndParameters("SELECT * FROM dbo.ProcessTasks WHERE Quadrant = @CWO_jan OR Quadrant IN (SELECT Value FROM @michael) OR Quadrant = @CWO_vincent", parameters);
			QueryTextAndParameters result = null;

			AssertNoExceptionThrown("The function should not try to mess with the table valued parameter. SAD!", () => result = BMQueryParameterisationHelper.ReplaceAutoParamNames(textAndParameters, "@StrawberrySmiggles"));
			AssertContainsExactElementsInAnyOrder(new[] { "@StrawberrySmiggles_jan", "@michael", "@StrawberrySmiggles_vincent" }, result.Parameters.ToArray().Select(x => x.ParameterName));
			AssertEquals("SELECT * FROM dbo.ProcessTasks WHERE Quadrant = @StrawberrySmiggles_jan OR Quadrant IN (SELECT Value FROM @michael) OR Quadrant = @StrawberrySmiggles_vincent", result.QueryText);
		}
	}
}
