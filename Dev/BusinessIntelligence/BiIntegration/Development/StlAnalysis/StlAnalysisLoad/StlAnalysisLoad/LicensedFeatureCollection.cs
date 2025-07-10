namespace Enterprise.StlAnalysis.Load
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	class LicensedFeatureCollection : ICollection<LicensedFeature>
	{
		readonly Dictionary<int, LicensedFeature> internalDictionary = new Dictionary<int, LicensedFeature>();

		public IEnumerator<LicensedFeature> GetEnumerator()
		{
			return internalDictionary.Values.GetEnumerator();
		}

		public void CollectBillingDataPerPeriod(HostedClient client, DateTime startDateInclusive, DateTime endDateExclusive)
		{
			Parallel.ForEach(internalDictionary.Values, m => m.LoadTransactionCountPerPeriod(client, startDateInclusive, endDateExclusive));
		}

		public void ResetBillingPeriodData()
		{
			foreach (var module in internalDictionary.Values)
			{
				module.ResetTransactionCount();
			}
		}

		#region Singleton

		LicensedFeatureCollection()
		{
		}

		public static LicensedFeatureCollection Instance
		{
			get { return instance ?? (instance = GetLoadedCollection()); }
		}
		static LicensedFeatureCollection instance;

		static LicensedFeatureCollection GetLoadedCollection()
		{
			var result = new LicensedFeatureCollection();

			using (var connection = DbManager.OpenNewStlAnalysisDataWarehouseConnection())
			{
				result.ReloadModules(connection);
			}

			return result;
		}

		#endregion

		#region Implementation

		/// <summary>
		/// Get list of WiseCloud hosted clients
		/// </summary>
		void ReloadModules(SqlConnection connection)
		{
			internalDictionary.Clear();

			const string gethostedCLientListSql = @"
				SELECT
					FeatureId,
					FeatureCode = isnull(FeatureCode, ''),
					RoleName,
					ModuleName,
					FunctionName,
					FeatureName,
					IsSystemLevel,
					IsCompanyLevel,
					StlBasis,
					StlEntityCounted,
					StlUnitWeight,
					StlQuery,
					StlDetailSqlExpression,
					StlDetailCountExpression,
					StlRules,
					DataSource
				FROM
					dbo.DimLicensedFeature dlf
				WHERE
					dlf.FeatureId > 0
				ORDER BY
					dlf.RoleName, dlf.ModuleName, dlf.FunctionName, dlf.FeatureName";

			using (var cmd = DbManager.NewSqlCommand(gethostedCLientListSql, connection, timeoutInSeconds: 5 * 60))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					int id = Convert.ToInt32(reader[0]);
					string code = reader[1].ToString();
					string role = reader[2].ToString();
					string module = reader[3].ToString();
					string function = reader[4].ToString();
					string feature = reader[5].ToString();
					bool isSystemLevel = Convert.ToBoolean(reader[6]);
					bool isCompanyLevel = Convert.ToBoolean(reader[7]);
					string stlBasis = reader[8].ToString();
					string stlEntity = reader[9].ToString();
					decimal stlUnits = Convert.ToDecimal(reader[10]);
					string stlSummaryQuery = reader[11].ToString();
					string detailSqlExpression = reader[12].ToString();
					string detailCountExpression = reader[13].ToString();
					string stlRules = reader[14].ToString();
					string dataSource = reader[15].ToString();
					var newFeature = new LicensedFeature(
						id, code, role, module, function, feature, isSystemLevel, isCompanyLevel,
						stlBasis, stlEntity, stlUnits,
						stlSummaryQuery, detailSqlExpression, detailCountExpression,
						stlRules, dataSource);
					internalDictionary.Add(id, newFeature);
				}
			}
		}

		#endregion // Implementation

		#region ICollection members

		public void Add(LicensedFeature item)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(LicensedFeature item)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(LicensedFeature[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public int Count
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsReadOnly
		{
			get { throw new NotImplementedException(); }
		}

		public bool Remove(LicensedFeature item)
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		#endregion // ICollection members
	}
}
