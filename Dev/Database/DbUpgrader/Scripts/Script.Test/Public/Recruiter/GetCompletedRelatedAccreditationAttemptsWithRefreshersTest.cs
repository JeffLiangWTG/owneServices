using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Recruiter;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Recruiter
{
	[TestedType(typeof(GetCompletedRelatedAccreditationAttemptsWithRefreshers))]
	class GetCompletedRelatedAccrediationAttemptsWithRefreshersTest : DbCreateScriptTest
	{
		[TestDate(2020, 2, 2)]
		public void TestFunction()
		{
			var person = Guid.NewGuid();
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

			var attempt1 = Guid.NewGuid();
			var attempt2 = Guid.NewGuid();
			var attempt3 = Guid.NewGuid();
			var attempt4 = Guid.NewGuid();
			var attempt5 = Guid.NewGuid();
			var attempt6 = Guid.NewGuid();
			var attempt7 = Guid.NewGuid();
			var attempt8 = Guid.NewGuid();
			var attempt9 = Guid.NewGuid();

			var sql = $@"
insert into dbo.GlbPerson (PER_PK, PER_Fullname, PER_EmailAddress) values ('{person}', 'name', 'email@person.com')

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
insert into dbo.GlbAccreditationRequirementPivot (HAR_PK, HAR_HAC, HAR_HAC_Parent) values (newid(), '{otherRef}', '{otherAcc}')

insert into dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate)
							values ('{attempt1}', '{acc1}', '{person}', '01/01/2015', '01/01/2015', '01/01/2016', '02/02/2016')

insert into dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate)
							values ('{attempt2}', '{acc1}', '{person}', '01/01/2017', '01/01/2017', '01/01/2018', '02/02/2018')

insert into dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate)
							values ('{attempt3}', '{acc2}', '{person}', '01/01/2018', '01/01/2018', '01/01/2019', '02/02/2019')

insert into dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate)
							values ('{attempt4}', '{acc3}', '{person}', '01/01/2019', '01/01/2019', '01/01/2020', '02/02/2020')

insert into dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate)
							values ('{attempt5}', '{ref1}', '{person}', '05/05/2017', '05/05/2017', '05/05/2018', '06/06/2018')

insert into dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDueDate, HAA_ExpiryDate)
							values ('{attempt6}', '{ref2}', '{person}', '05/05/2018', '05/05/2019', '06/06/2019')

insert into dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate)
							values ('{attempt7}', '{ref3}', '{person}', '05/05/2019', '05/05/2019', '05/05/2020', '06/06/2020')

insert into dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate)
							values ('{attempt8}', '{otherAcc}', '{person}', '05/05/2018', '05/05/2018', '05/05/2019', '06/06/2019')

insert into dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate)
							values ('{attempt9}', '{otherRef}', '{person}', '05/05/2019', '05/05/2019', '05/05/2020', '06/06/2020')
";

			TestConnection.ExecuteNonQuery(sql);

			var result = GetFunctionResults(person, acc3);
			AssertEquals(6, result.Rows.Count);
			AssertRow(result, attempt1, new DateTime(2015, 1, 1), new DateTime(2016, 2, 2), false);
			AssertRow(result, attempt2, new DateTime(2017, 1, 1), new DateTime(2018, 2, 2), false);
			AssertRow(result, attempt3, new DateTime(2018, 1, 1), new DateTime(2019, 2, 2), false);
			AssertRow(result, attempt4, new DateTime(2019, 1, 1), new DateTime(2020, 2, 2), false);
			AssertRow(result, attempt5, new DateTime(2017, 5, 5), new DateTime(2018, 6, 6), true);
			AssertRow(result, attempt7, new DateTime(2019, 5, 5), new DateTime(2020, 6, 6), true);

			result = GetFunctionResults(person, acc2);
			AssertEquals(4, result.Rows.Count);
			AssertRow(result, attempt1, new DateTime(2015, 1, 1), new DateTime(2016, 2, 2), false);
			AssertRow(result, attempt2, new DateTime(2017, 1, 1), new DateTime(2018, 2, 2), false);
			AssertRow(result, attempt3, new DateTime(2018, 1, 1), new DateTime(2019, 2, 2), false);
			AssertRow(result, attempt5, new DateTime(2017, 5, 5), new DateTime(2018, 6, 6), true);

			result = GetFunctionResults(person, otherAcc);
			AssertEquals(2, result.Rows.Count);
			AssertRow(result, attempt8, new DateTime(2018, 5, 5), new DateTime(2019, 6, 6), false);
			AssertRow(result, attempt9, new DateTime(2019, 5, 5), new DateTime(2020, 6, 6), true);
		}

		void AssertRow(DataTable table, Guid attempt, DateTime commence, DateTime expiry, bool isRefresher)
		{
			DataRow dataRow = null;
			foreach (DataRow row in table.Rows)
			{
				if ((Guid)row[0] == attempt)
				{
					dataRow = row;
					break;
				}
			}

			AssertNotNull(dataRow);
			AssertEquals(commence, (DateTime)dataRow["HAA_CommencementDate"]);
			AssertEquals(expiry, (DateTime)dataRow["HAA_ExpiryDate"]);
			AssertEquals(isRefresher, (bool)dataRow["HAC_IsRefresher"]);
		}

		DataTable GetFunctionResults(Guid personPk, Guid accreditationPk)
		{
			var command = TestConnection.Command("SELECT * FROM dbo.GetCompletedRelatedAccreditationAttemptsWithRefreshers(@PersonPk, @AccreditationPk)");

			command.AddParameter("@PersonPk", SqlDbType.UniqueIdentifier, personPk);
			command.AddParameter("@AccreditationPk", SqlDbType.UniqueIdentifier, accreditationPk);

			return DataUtils.GetDataTableFromCommand(command);
		}
	}
}

