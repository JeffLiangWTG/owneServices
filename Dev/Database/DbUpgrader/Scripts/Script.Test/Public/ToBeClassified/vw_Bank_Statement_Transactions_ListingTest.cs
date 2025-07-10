using CargoWise.DbUpgrader.Scripts.Definitions.ToBeClassified;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.ToBeClassified
{
	[TestedType(typeof(vw_Bank_Statement_Transactions_Listing))]
	class vw_Bank_Statement_Transactions_ListingTest : DbCreateScriptTest
	{
	}
}

