using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Recruiter;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Recruiter
{
	[TestedType(typeof(GetAccreditationsRequirementsStructure))]
	class GetAccreditationsRequirementsStructureTest : DbCreateScriptTest
	{
		public void TestFunction()
		{
			var acc1 = Guid.NewGuid();
			var acc2 = Guid.NewGuid();
			var acc3 = Guid.NewGuid();
			var ref1 = Guid.NewGuid();
			var ref2 = Guid.NewGuid();
			var ref3 = Guid.NewGuid();
			var otherAcc = Guid.NewGuid();
			var otherRef = Guid.NewGuid();

			var acc1Code = "AC1";
			var acc2Code = "AC2";
			var acc3Code = "AC3";
			var ref1Code = "RE1";
			var ref2Code = "RE2";
			var ref3Code = "RE3";
			var otherAccCode = "ACX";
			var otherRefCode = "REX";

			var sql = $@"
insert into dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_IsRefresher, HAC_CertificateCode) values ('{acc1}', '{acc1Code}', '{acc1Code} desc', 0, 'XX1')
insert into dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_IsRefresher, HAC_CertificateCode) values ('{acc2}', '{acc2Code}', '{acc2Code} desc', 0, 'XX2')
insert into dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_IsRefresher, HAC_CertificateCode) values ('{acc3}', '{acc3Code}', '{acc3Code} desc', 0, 'XX3')
insert into dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_IsRefresher, HAC_CertificateCode, HAC_RefresherCertificateExpiryType) values ('{ref1}', '{ref1Code}', '{ref1Code} desc', 1, 'XX1', 'RCD')
insert into dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_IsRefresher, HAC_CertificateCode, HAC_RefresherCertificateExpiryType) values ('{ref2}', '{ref2Code}', '{ref2Code} desc', 1, 'XX2', 'RCD')
insert into dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_IsRefresher, HAC_CertificateCode, HAC_RefresherCertificateExpiryType) values ('{ref3}', '{ref3Code}', '{ref3Code} desc', 1, 'XX3', 'RCD')
insert into dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_IsRefresher, HAC_CertificateCode) values ('{otherAcc}', '{otherAccCode}', '{otherAccCode} desc', 0, 'YY1')
insert into dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_IsRefresher, HAC_CertificateCode, HAC_RefresherCertificateExpiryType) values ('{otherRef}', '{otherRefCode}', '{otherRefCode} desc', 1, 'YY1', 'RCD')

insert into dbo.GlbAccreditationRequirementPivot (HAR_PK, HAR_HAC, HAR_HAC_Parent) values (newid(), '{acc2}', '{acc1}')
insert into dbo.GlbAccreditationRequirementPivot (HAR_PK, HAR_HAC, HAR_HAC_Parent) values (newid(), '{acc3}', '{acc2}')
insert into dbo.GlbAccreditationRequirementPivot (HAR_PK, HAR_HAC, HAR_HAC_Parent) values (newid(), '{ref1}', '{acc1}')
insert into dbo.GlbAccreditationRequirementPivot (HAR_PK, HAR_HAC, HAR_HAC_Parent) values (newid(), '{ref2}', '{acc2}')
insert into dbo.GlbAccreditationRequirementPivot (HAR_PK, HAR_HAC, HAR_HAC_Parent) values (newid(), '{ref3}', '{acc3}')
insert into dbo.GlbAccreditationRequirementPivot (HAR_PK, HAR_HAC, HAR_HAC_Parent) values (newid(), '{otherRef}', '{otherAcc}')";

			TestConnection.ExecuteNonQuery(sql);

			var result = GetFunctionResults(acc3);
			AssertEquals(6, result.Rows.Count);

			AssertRow(result, acc1Code, "", false);
			AssertRow(result, acc2Code, acc1Code, false);
			AssertRow(result, acc3Code, acc2Code, false);
			AssertRow(result, ref1Code, acc1Code, true);
			AssertRow(result, ref2Code, acc2Code, true);
			AssertRow(result, ref3Code, acc3Code, true);

			result = GetFunctionResults(acc2);
			AssertEquals(4, result.Rows.Count);

			AssertRow(result, acc1Code, "", false);
			AssertRow(result, acc2Code, acc1Code, false);
			AssertRow(result, ref1Code, acc1Code, true);
			AssertRow(result, ref2Code, acc2Code, true);

			result = GetFunctionResults(otherAcc);
			AssertEquals(2, result.Rows.Count);

			AssertRow(result, otherAccCode, "", false);
			AssertRow(result, otherRefCode, otherAccCode, true);
		}

		void AssertRow(DataTable table, string code, string codeParent, bool isRefresher)
		{
			DataRow dataRow = null;
			foreach (DataRow row in table.Rows)
			{
				if ((string)row[0] == code)
				{
					dataRow = row;
					break;
				}
			}

			AssertNotNull(dataRow);
			AssertEquals(code, (string)dataRow["HAC_Code"]);
			AssertEquals(codeParent, (string)dataRow["HAC_Code_Parent"]);
			AssertEquals(isRefresher, (bool)dataRow["HAC_IsRefresher"]);
		}

		DataTable GetFunctionResults(Guid accreditationPk)
		{
			var command = TestConnection.Command("SELECT HAC_Code, HAC_Code_Parent, HAC_IsRefresher FROM dbo.GetAccreditationsRequirementsStructure(@AccreditationPk)");
			command.AddParameter("@AccreditationPk", SqlDbType.UniqueIdentifier, accreditationPk);

			return DataUtils.GetDataTableFromCommand(command);
		}
	}
}

