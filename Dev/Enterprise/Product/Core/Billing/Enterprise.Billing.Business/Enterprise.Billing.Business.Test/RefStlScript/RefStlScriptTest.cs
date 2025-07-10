using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Billing.Business.Testing
{
	[TestedType(typeof(RefStlScript))]
	sealed class RefStlScriptTest : EnterpriseBusinessObjectTestCase
	{
		public void TestRefDataExceptionThrownForZBlobExceptionWhenGettingNVARCHARMAXFields()
		{
			var stlScript = Factory.NewWithValidTestData<RefStlScript>();
			stlScript.STL_PreparationScript = new string('x', 1050);
			stlScript.STL_FromClause = new string('x', 1050);
			stlScript.STL_WhereClause = new string('x', 1050);
			Factory.Save();
			var newFactory = NewFactory();
			var stlScriptInSecondFactory = newFactory.Load<RefStlScript>(stlScript.PK);
			Db.Connection.ExecuteNonQuery($"UPDATE RefDatabase_RefStlScript SET STL_PK = NEWID() WHERE STL_PK = '{stlScript.PK}'"); // This is a test
			AssertExceptionThrown<RefDataException>(() => { var temp = stlScriptInSecondFactory.PreparationScript; });
			AssertExceptionThrown<RefDataException>(() => { var temp = stlScriptInSecondFactory.FromClause; });
			AssertExceptionThrown<RefDataException>(() => { var temp = stlScriptInSecondFactory.WhereClause; });
		}
	}
}
