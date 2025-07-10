namespace Enterprise.StlAnalysis.Load
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
#if NETFRAMEWORK
	using System.Linq;
#endif

	class HostedClientCollection : ICollection<HostedClient>
	{
		readonly Dictionary<string, HostedClient> internalDictionary = new Dictionary<string, HostedClient>();

		public int Count
		{
			get { return internalDictionary.Count; }
		}

		public void CopyTo(HostedClient[] array, int arrayIndex)
		{
			internalDictionary.Values.CopyTo(array, arrayIndex);
		}

		public bool ContainsKey(string enterpriseCode, string dbServerCode, string companyCode)
		{
			string key = enterpriseCode + dbServerCode + companyCode.Trim();
			return internalDictionary.Keys.Contains(key);
		}

		#region Singleton

		HostedClientCollection()
		{
		}

		public static HostedClientCollection Instance
		{
			get { return instance ?? (instance = GetLoadedCollection()); }
		}
		static HostedClientCollection instance;

		static HostedClientCollection GetLoadedCollection()
		{
			var result = new HostedClientCollection();

			using (var connection = DbManager.OpenNewStlAnalysisDataWarehouseConnection())
			{
				result.Reload(connection);
			}

			return result;
		}

		#endregion

		#region Load

		/// <summary>
		/// Get list of WiseCloud hosted clients from DW
		/// </summary>
		public void Reload(SqlConnection connection)
		{
			internalDictionary.Clear();

			const string gethostedCLientListSql = @"
				SELECT
					dhcc.ClientId,
					dhcc.EnterpriseCode,
					dhcc.DatabaseServerCode,
					dhcc.CompanyCode,
					dhcc.OrganisationName
				FROM
					dbo.DimHostedClientCompany dhcc
				WHERE
					dhcc.IsCurrent = 1
				ORDER BY
					dhcc.OrganisationName,
					dhcc.EnterpriseCode,
					dhcc.DatabaseServerCode,
					dhcc.CompanyCode";

			using (var cmd = DbManager.NewSqlCommand(gethostedCLientListSql, connection, timeoutInSeconds: 5 * 60))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					int id = Convert.ToInt32(reader[0]);
					string enterpriseCode = reader[1].ToString();
					string dbServerCode = reader[2].ToString();
					string companyCode = reader[3].ToString().Trim();
					string orgName = reader[4].ToString();

					internalDictionary.Add(
						enterpriseCode + dbServerCode + companyCode,
						new HostedClient(id, enterpriseCode, dbServerCode, companyCode, orgName));
				}
			}
		}

		#endregion // Load

		#region ICollection members

		public void Add(HostedClient item)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(HostedClient item)
		{
			throw new NotImplementedException();
		}

		public bool IsReadOnly
		{
			get { throw new NotImplementedException(); }
		}

		public bool Remove(HostedClient item)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<HostedClient> GetEnumerator()
		{
			return internalDictionary.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion // ICollection members
	}
}
