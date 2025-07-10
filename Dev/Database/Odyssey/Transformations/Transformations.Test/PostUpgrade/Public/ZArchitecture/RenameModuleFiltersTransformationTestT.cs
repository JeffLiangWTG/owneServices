using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using CargoWise.Data;
using CargoWise.IO;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Testing
{
	abstract class RenameModuleFiltersTransformationTest<T> : DataTransformationTestCase
				where T : RenameModuleFiltersTransformation
	{
		protected abstract T GetNewRenameModuleFiltersTransformation();
		protected abstract Tuple<string, string>[] GetBeforeAndExpectedAfterFilterXmlPairs();

		protected override void PrepareTestData()
		{
			var renameFiltersTransformation = GetNewRenameModuleFiltersTransformation();
			beforeAndExpectedAfterFilterXmlPairs = GetBeforeAndExpectedAfterFilterXmlPairs();

			pksByModule = new Dictionary<string, Guid[]>();
			foreach (var moduleID in renameFiltersTransformation.ModuleIDs)
			{
				pksByModule[moduleID] = new Guid[beforeAndExpectedAfterFilterXmlPairs.Length];
				for (var i = 0; i < beforeAndExpectedAfterFilterXmlPairs.Length; i++)
				{
					pksByModule[moduleID][i] = Guid.NewGuid();
					InsertFilter(pksByModule[moduleID][i], moduleID + "_" + i, moduleID, beforeAndExpectedAfterFilterXmlPairs[i].Item1);
				}
			}

			otherModuleFilterPks = new Guid[beforeAndExpectedAfterFilterXmlPairs.Length];
			const string otherModuleFilterID = "8F27283F-06CD-43AD-8441-F378D31711D4";
			for (var i = 0; i < beforeAndExpectedAfterFilterXmlPairs.Length; i++)
			{
				otherModuleFilterPks[i] = Guid.NewGuid();
				InsertFilter(otherModuleFilterPks[i], otherModuleFilterID + "_" + i, otherModuleFilterID, beforeAndExpectedAfterFilterXmlPairs[i].Item1);
			}
		}

		protected override void AssertTransformationResults()
		{
			var renameFiltersTransformation = GetNewRenameModuleFiltersTransformation();

			foreach (var moduleID in renameFiltersTransformation.ModuleIDs)
			{
				for (var i = 0; i < beforeAndExpectedAfterFilterXmlPairs.Length; i++)
				{
					AssertFilterData("Should be updated", pksByModule[moduleID][i], beforeAndExpectedAfterFilterXmlPairs[i].Item2);
				}
			}

			for (var i = 0; i < beforeAndExpectedAfterFilterXmlPairs.Length; i++)
			{
				AssertFilterData("Should not change for filters with a different ModuleID", otherModuleFilterPks[i], beforeAndExpectedAfterFilterXmlPairs[i].Item1);
			}
		}

		#region Implementation

		Tuple<string, string>[] beforeAndExpectedAfterFilterXmlPairs;
		Dictionary<string, Guid[]> pksByModule;
		Guid[] otherModuleFilterPks;

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return GetNewRenameModuleFiltersTransformation();
		}

		void InsertFilter(Guid filterPk, string filterName, string moduleId, string filterDataXml)
		{
			using (var command = Db.Connection.Command("INSERT INTO dbo.StmModuleFilter (S9_PK, S9_FilterName, S9_ModuleID, S9_FilterData) VALUES (@S9_PK, @S9_FilterName, @S9_ModuleID, @S9_FilterData)"))
			{
				command.AddParameter("@S9_PK", SqlDbType.UniqueIdentifier, filterPk);
				command.AddParameter("@S9_FilterName", SqlDbType.VarChar, filterName);
				command.AddParameter("@S9_ModuleID", SqlDbType.VarChar, moduleId);
				command.AddParameter("@S9_FilterData", SqlDbType.VarBinary, (filterDataXml != null) ? Compressor.Compress(Encoding.ASCII.GetBytes(filterDataXml)) : DBNull.Value);
				command.ExecuteNonQuery();
			}
		}

		void AssertFilterData(string message, Guid filterPk, string expectedFilterDataXml)
		{
			using (var command = Db.Connection.Command("SELECT S9_FilterName, S9_FilterData FROM dbo.StmModuleFilter WHERE S9_PK = @S9_PK"))
			{
				command.AddParameter("@S9_PK", SqlDbType.UniqueIdentifier, filterPk);

				using (var reader = command.ExecuteReader())
				{
					reader.Read();

					var filterName = (string)reader["S9_FilterName"];
					var actualFilterData = reader["S9_FilterData"];
					message = string.Format("S9_FilterData for [{0}]: {1}", filterName, message);

					if (expectedFilterDataXml == null)
					{
						AssertEquals(message, DBNull.Value, actualFilterData);
					}
					else if (actualFilterData == DBNull.Value)
					{
						AssertEquals(message, expectedFilterDataXml, DBNull.Value);
					}
					else
					{
						var actualFilterDataXml = Compressor.UncompressAsString((byte[])actualFilterData);
						AssertMultilineASCIIEquals(message, expectedFilterDataXml, actualFilterDataXml);
					}
				}
			}
		}

		#endregion
	}
}
