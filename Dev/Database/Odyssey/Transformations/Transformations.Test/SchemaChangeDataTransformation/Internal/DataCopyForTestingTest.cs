using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations
{
	[TestedType(typeof(DataCopyForTesting))]
	sealed class DataCopyForTestingTest : DataCopyTestCase
	{
		protected override SchemaChangeDataTransformation GetNewTestTransformationInstance()
		{
			SchemaChangeDataTransformation transformation = new DataCopyForTesting();
			return transformation;
		}

		protected override void PrepareOriginalData()
		{
		}

		protected override void AssertDataCopyResults()
		{
			string sqlText = "SELECT count(*) FROM dbo.RefCountry WHERE RN_NumberOfStates is not null";
			int rowCount = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Number of RefCountry rows updated", 2, rowCount);
			sqlText = "SELECT RN_NumberOfStates FROM dbo.RefCountry WHERE RN_Code = 'AU'";
			int numberOfStates = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Number of States - Australia(AU)", 8, numberOfStates);
			sqlText = "SELECT RN_NumberOfStates FROM dbo.RefCountry WHERE RN_Code = 'NZ'";
			numberOfStates = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Number of States - New Zealand(NZ)", 16, numberOfStates);
		}

		protected override List<string> IAcknowledgeTheseSourceColumnsHaveNoReplacementValue
		{
			get
			{
				List<string> result = base.IAcknowledgeTheseSourceColumnsHaveNoReplacementValue;
				result.Add("RefCountry.RN_Code");
				result.Add("RefCountry.RN_Desc");
				result.Add("RefCountryStates.RW_RN");
				result.Add("RefCountryStates.RW_Code");
				result.Add("RefCountryStates.RW_Description");
				return result;
			}
		}
	}
}
