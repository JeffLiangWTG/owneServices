using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(csfn_GetFTZStatusDescriptionInline))]
	class csfn_GetFTZStatusDescriptionInlineTest : DbCreateScriptTest
	{
		public void TestFTZStatusDescription()
		{
			var script = @"SELECT FTZStatusDescription FROM csfn_GetFTZStatusDescriptionInline('{0}')";
			AssertEquals("Awaiting FTZ Admission Add", TestConnection.Command(string.Format(script, "AFA")).ExecuteScalar());
			AssertEquals("Awaiting FTZ Admission Delete", TestConnection.Command(string.Format(script, "AFD")).ExecuteScalar());
			AssertEquals("Awaiting FTZ Admission Amend", TestConnection.Command(string.Format(script, "AFM")).ExecuteScalar());
			AssertEquals("Awaiting Permit To Transfer", TestConnection.Command(string.Format(script, "APT")).ExecuteScalar());
			AssertEquals("Awaiting Goods Arrival", TestConnection.Command(string.Format(script, "AGA")).ExecuteScalar());
			AssertEquals("Awaiting Concurrence", TestConnection.Command(string.Format(script, "ACC")).ExecuteScalar());
			AssertEquals("Awaiting Delivery Of Goods", TestConnection.Command(string.Format(script, "ADG")).ExecuteScalar());
			AssertEquals("Clear FTZ Admission Add", TestConnection.Command(string.Format(script, "CFA")).ExecuteScalar());
			AssertEquals("Clear FTZ Admission Add With Warnings", TestConnection.Command(string.Format(script, "CFW")).ExecuteScalar());
			AssertEquals("Clear FTZ Admission Delete", TestConnection.Command(string.Format(script, "CFD")).ExecuteScalar());
			AssertEquals("Clear FTZ Admission Amend", TestConnection.Command(string.Format(script, "CFM")).ExecuteScalar());
			AssertEquals("Clear Permit To Transfer", TestConnection.Command(string.Format(script, "CPT")).ExecuteScalar());
			AssertEquals("Clear Goods Arrival", TestConnection.Command(string.Format(script, "CGA")).ExecuteScalar());
			AssertEquals("Clear Concurrence", TestConnection.Command(string.Format(script, "CCC")).ExecuteScalar());
			AssertEquals("Clear Delivery Of Goods", TestConnection.Command(string.Format(script, "CGD")).ExecuteScalar());
			AssertEquals("Error FTZ Admission Add", TestConnection.Command(string.Format(script, "EFA")).ExecuteScalar());
			AssertEquals("Error FTZ Admission Delete", TestConnection.Command(string.Format(script, "EFD")).ExecuteScalar());
			AssertEquals("Error FTZ Admission Amend", TestConnection.Command(string.Format(script, "EFM")).ExecuteScalar());
			AssertEquals("Error Permit To Transfer", TestConnection.Command(string.Format(script, "EPT")).ExecuteScalar());
			AssertEquals("Error Goods Arrival", TestConnection.Command(string.Format(script, "EGA")).ExecuteScalar());
			AssertEquals("Error Concurrence", TestConnection.Command(string.Format(script, "ECC")).ExecuteScalar());
			AssertEquals("Error Delivery Of Goods", TestConnection.Command(string.Format(script, "EDG")).ExecuteScalar());
			AssertEquals("Awaiting Cancel Permit To Transfer", TestConnection.Command(string.Format(script, "ACP")).ExecuteScalar());
			AssertEquals("Permit To Transfer Cancel Accepted", TestConnection.Command(string.Format(script, "PCA")).ExecuteScalar());
			AssertEquals("Permit To Transfer Cancel Unauthorized", TestConnection.Command(string.Format(script, "PCU")).ExecuteScalar());
			AssertEquals("Awaiting Permit To Transfer Arrival", TestConnection.Command(string.Format(script, "APA")).ExecuteScalar());
			AssertEquals("Awaiting Permit to Transfer Un-Arrival", TestConnection.Command(string.Format(script, "APU")).ExecuteScalar());
			AssertEquals("Error Permit To Transfer Arrival", TestConnection.Command(string.Format(script, "EPA")).ExecuteScalar());
			AssertEquals("Permit To Transfer Un-Arrived", TestConnection.Command(string.Format(script, "PUA")).ExecuteScalar());
			AssertEquals("Permit To Transfer Arrival Cancel Unauthorized", TestConnection.Command(string.Format(script, "PAU")).ExecuteScalar());
			AssertEquals("Permit To Transfer Arrived", TestConnection.Command(string.Format(script, "PAR")).ExecuteScalar());
		}
	}
}
