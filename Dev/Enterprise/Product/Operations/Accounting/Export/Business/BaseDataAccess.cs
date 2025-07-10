using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Export.Business
{
	public class BaseDataAccess
	{
		public BaseDataAccess(System.Data.Common.DbConnection connection) : this(connection, null)
		{
		}

		public BaseDataAccess(System.Data.Common.DbConnection connection, System.Data.Common.DbTransaction transaction)
		{
			Connection = connection;
			Transaction = transaction;
		}

		public readonly System.Data.Common.DbConnection Connection;
		public readonly System.Data.Common.DbTransaction Transaction;
		protected int? CommandTimeout;

#if DEBUG
		public List<string> CreatedCommandTexts_ForTestOnly = new List<string>();
#endif

		public System.Data.Common.DbCommand GetCommandWithTransaction(
			string sqlText,
			System.Data.Common.DbTransaction transaction,
			params Action<System.Data.Common.DbParameter>[] parameters)
		{
			if (Connection == null)
			{
				throw new InvalidOperationException($"{nameof(BaseDataAccess)}: Connection property has not been initialized.");
			}

			var result = Connection.CreateCommand();
			result.CommandText = sqlText;

			if (transaction != null)
			{
				result.Transaction = transaction;
			}

			if (parameters != null && parameters.Any())
			{
				foreach (var prepareParam in parameters)
				{
					var newParam = result.CreateParameter();
					prepareParam(newParam);
					result.Parameters.Add(newParam);
				}
			}

			if (CommandTimeout.HasValue)
			{
				result.CommandTimeout = CommandTimeout.Value;
			}

#if DEBUG
			CreatedCommandTexts_ForTestOnly.Add(sqlText);
#endif

			return result;
		}

		public System.Data.Common.DbCommand GetCommand(
			string sqlText,
			Action<System.Data.Common.DbParameter>[] parameters)
		{
			return GetCommandWithTransaction(sqlText, Transaction, parameters);
		}

		public System.Data.Common.DbCommand GetCommand(
			string sqlText,
			params (string Name, object Value)[] parameters)
		{
			return GetCommand(sqlText, Transaction, parameters);
		}

		public System.Data.Common.DbCommand GetCommand(
			string sqlText,
			System.Data.Common.DbTransaction transaction,
			params (string Name, object Value)[] parameters)
		{
			var sqlParameters = parameters.Select(p =>
				new Action<System.Data.Common.DbParameter>(newParam =>
				{
					newParam.ParameterName = p.Name;
					newParam.Value = p.Value;
				})).ToArray();
			return GetCommandWithTransaction(sqlText, transaction, sqlParameters);
		}

		public string ValidateLoginDetails(string userName, string password)
		{
			var result = string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password) ? (NoResString)"Please, provide both User Name and Password to log in CargoWise Accounting Web Service." : string.Empty;
			if (string.IsNullOrEmpty(result))
			{
				var command = "DECLARE @result CHAR(1) EXEC @result = ValidateAccountingWebServiceLogin @userName, @password SELECT @result";

#pragma warning disable CW1161 // Res.GetString Analyzer
				using (var cmd = GetCommand(
					command,
					(Name: "@userName", Value: userName),
					(Name: "@password", Value: password)))
				{
					object checking = cmd.ExecuteScalar();
					if ((string)checking != "Y")
					{
						result = (NoResString)"Invalid User Name/Password";
					}
				}
#pragma warning restore CW1161 // Res.GetString Analyzer
			}
			return result;
		}

		public bool DoesDateBelongToOpenAccountingPeriod(string companyCode, DateTime date)
		{
			var companyPK = GetCompanyData(companyCode).CompanyPK;
			return DoesDateBelongToOpenAccountingPeriod(companyPK, date);
		}

		public bool DoesDateBelongToOpenAccountingPeriod(Guid companyPK, DateTime date)
		{
			bool result = false;

			string command = "select top 1 AM_PK from dbo.accPeriodManagement where AM_GC_Company = @CompanyPK and @Date >= AM_StartDate and @Date <= AM_EndDate and AM_IsSubLedgerClosed = 0 and AM_IsGeneralLedgerClosed = 0";

			using (var cmd = GetCommand(command, (Name: "@CompanyPK", Value: companyPK), (Name: "@Date", Value: date)))
			{
				object periodPK = cmd.ExecuteScalar();
				result = periodPK != null;
			}

			return result;
		}

		protected System.Data.Common.DbDataReader GetCompanyReader(string companyCode)
		{
			string sqlText = @"SELECT TOP 1 GC_PK, GC_Code, GC_Name, GC_OH_OrgProxy, GC_RN_NKCountryCode, RN_PK, RN_Desc, RX_PK, RX_Desc, RX_SubUnitRatio
FROM dbo.GlbCompany
JOIN dbo.RefCountry ON GC_RN_NKCountryCode = RN_Code
JOIN dbo.RefCurrency ON GC_RX_NKLocalCurrency = RX_Code
WHERE GC_Code = @companyCode";

			System.Data.Common.DbDataReader result = null;
			using (var command = GetCommand(sqlText, (Name: "@companyCode", Value: companyCode)))
			{
				result = command.ExecuteReader();
				if (result.Read())
				{
					return result;
				}
				else
				{
					result.Close();
					return null;
				}
			}
		}

		protected CompanyData GetCompanyData(string companyCode)
		{
			if (!CompanyPKDictionary.TryGetValue(companyCode, out CompanyData companyData))
			{
				using (var companyReader = GetCompanyReader(companyCode))
				{
					if (companyReader != null)
					{
						companyData = new CompanyData();
						companyData.CompanyPK = (Guid)companyReader["GC_PK"];
						companyData.CountryCode = (string)companyReader["GC_RN_NKCountryCode"];
					}
				}
				CompanyPKDictionary.Add(companyCode, companyData);
			}

			return companyData;
		}

		Dictionary<string, CompanyData> CompanyPKDictionary => companyPKDictionary ?? (companyPKDictionary = new Dictionary<string, CompanyData>());
		Dictionary<string, CompanyData> companyPKDictionary;

		protected struct CompanyData
		{
			public Guid CompanyPK { get; set; }
			public string CountryCode { get; set; }
		}
	}
}
