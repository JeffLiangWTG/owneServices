using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core.Testing
{
	[TestedType(typeof(UpdateEmptyCompanyNameToPlaceholderValueAndMakeInactive))]
	public class UpdateEmptyCompanyNameToPlaceholderValueAndMakeInactiveTest : DataTransformationTestCase
	{
		readonly Guid companyPK1 = Guid.NewGuid();
		readonly Guid companyPK2 = Guid.NewGuid();
		readonly Guid companyPK3 = Guid.NewGuid();
		readonly Guid companyPK4 = Guid.NewGuid();

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateEmptyCompanyNameToPlaceholderValueAndMakeInactive();
		}

		protected override void PrepareTestData()
		{
			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(TestConnection, GlbCompanySchema.Constants.SqlSchemaName, GlbCompanySchema.Constants.TableName, "Constraint_GC_Name"))
			{
				CreateCompany(companyPK1, "GZ1", "", true);
				CreateCompany(companyPK2, "GZ2", "", false);
				CreateCompany(companyPK3, "GZ3", "US", true);
				CreateCompany(companyPK4, "GZ4", "AU", false);
			}
		}

		protected override void AssertTransformationResults()
		{
			AssertGlbCompany(companyPK1, "** Name not provided **", false);
			AssertGlbCompany(companyPK2, "** Name not provided **", false);
			AssertGlbCompany(companyPK3, "US", true);
			AssertGlbCompany(companyPK4, "AU", false);
		}

		void AssertGlbCompany(Guid companyPK, string companyName, bool isActive)
		{
			var companies = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT GC_Name, GC_IsActive FROM dbo.GlbCompany WHERE GC_PK = '{companyPK}'");
			AssertEquals("Count", 1, companies.Rows.Count);
			AssertEquals("Company name", companyName, companies.Rows[0]["GC_Name"]);
			AssertEquals("Is active", isActive, companies.Rows[0]["GC_IsActive"]);
		}

		public Guid CreateCompany(Guid companyPK, string companyCode, string companyName, bool isActive)
		{
			const string createCompanySql = @"INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_IsActive, GC_RN_NKCountryCode, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser)
VALUES (@companyPK, @companyCode, @companyName, @isActive, 'AU', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			using (var command = Db.Connection.Command(createCompanySql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@companyCode", SqlDbType.VarChar, companyCode);
				command.AddParameter("@companyName", SqlDbType.VarChar, companyName);
				command.AddParameter("@isActive", SqlDbType.Bit, isActive);
				command.ExecuteNonQuery();
			}
			return companyPK;
		}
	}
}
