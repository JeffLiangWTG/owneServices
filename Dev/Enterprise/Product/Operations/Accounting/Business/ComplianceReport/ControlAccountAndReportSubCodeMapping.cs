using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public sealed class ControlAccountAndReportSubCodeMapping
	{
		public ReadOnlyDictionary<Guid, HashSet<string>> AccountPkToSubCodesMapping => accountPkToSubCodesMapping ??= new ReadOnlyDictionary<Guid, HashSet<string>>(GenerateAccountPkToSubCodesMapping());

		public ReadOnlyDictionary<ZGuid, string> AccountPkToNumberMapping => accountPkToNumberMapping ??= new ReadOnlyDictionary<ZGuid, string>(GenerateAccountPkToNumberMapping());

		public ReadOnlyCollection<string> ControlAccountsNumbers => controlAccountsNumbers ??= new ReadOnlyCollection<string>(GenerateControlAccountsNumbers());

		public DataTable AccountPkToSubCodesTable => accountPkToSubCodesTable ??= GenerateAccountPkToSubCodesMappingTable();

		public string GetAccountNumber(string subCode, string defaultAccount)
		{
			var controlAccountPK = AccountPkToSubCodesMapping.FirstOrDefault(x => x.Value.Contains(subCode)).Key;

			if (controlAccountPK != ZGuid.Empty)
			{
				var result = GetAccountNumber(controlAccountPK);

				if (!string.IsNullOrEmpty(result))
				{
					return result;
				}
			}

			return defaultAccount;
		}

		public string GetAccountNumber(ZGuid primaryKey)
		{
			AccountPkToNumberMapping.TryGetValue(primaryKey, out var accountNumber);

			return accountNumber;
		}

		Dictionary<Guid, HashSet<string>> GenerateAccountPkToSubCodesMapping()
		{
			var mapping = new Dictionary<Guid, HashSet<string>>();

			#region SuppressResourceStringsCheckRegion

			AddMappingToDictionary(mapping, AccountingConfigurationRegistry.Instance.ARControlAccount.Value, new List<string>()
			{
				"*AR*CTR*ARCtrl*",
				"*AR*JNL*ARCtrl*",
				"*AR*OVP*ARCtrl*",
				"*AR*DSC*ARCtrl*",
				"*AR*EXX*ARCtrl*",
				"*AR*INV*ARCtrl*Total",
				"*AR*CRD*ARCtrl*Total",
				"*AR*ADJ*ARCtrl*Total",
				"*AR*PAY*ARCtrl*",
				"*AR*REC*ARCtrl*",
				"*AR*TRF*ARCtrl*",
			});

			AddMappingToDictionary(mapping, AccountingConfigurationRegistry.Instance.APControlAccount.Value, new List<string>()
			{
				"*AP*CTR*APCtrl*",
				"*AP*JNL*APCtrl*",
				"*AP*OVP*APCtrl*",
				"*AP*DSC*APCtrl*",
				"*AP*EXX*APCtrl*",
				"*AP*INV*APCtrl*Total",
				"*AP*CRD*APCtrl*Total",
				"*AP*ADJ*APCtrl*Total",
				"*AP*PAY*APCtrl*",
				"*AP*REC*APCtrl*",
				"*AP*TRF*APCtrl*",
			});

			AddMappingToDictionary(mapping, AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.Value, new List<string>()
			{
				"*AR*INV*ARSusp*-",
				"*AR*CRD*ARSusp*-",
				"*AR*ADJ*ARSusp*-",
				"*AR*INV*ARSusp*Rev",
				"*AR*CRD*ARSusp*Rev",
				"*AR*ADJ*ARSusp*Rev",
				"*JC*JNL*ARSusp*-",
				"*JC*JNL*ARSusp*Rev",
				"*JC*JRJ*ARSusp*-",
				"*JC*JRJ*ARSusp*Rev",
			});

			AddMappingToDictionary(mapping, AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.Value, new List<string>()
			{
				"*AP*INV*APSusp*-",
				"*AP*CRD*APSusp*-",
				"*AP*ADJ*APSusp*-",
				"*AP*INV*APSusp*Rev",
				"*AP*CRD*APSusp*Rev",
				"*AP*ADJ*APSusp*Rev",
			});

			AddMappingToDictionary(mapping, AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value, new List<string>()
			{
				"*CB*DPY*GSTIn*-",
				"*AP*INV*GSTIn*-",
				"*AP*CRD*GSTIn*-",
				"*AP*ADJ*GSTIn*-",
				"*CT*CBT*GSTIn*-",
			});

			AddMappingToDictionary(mapping, AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.Value, new List<string>()
			{
				"*AP*INV*PenGSTIn*-",
				"*AP*CRD*PenGSTIn*-",
				"*AP*ADJ*PenGSTIn*-",
				"*CT*CBT*PenGSTIn*",
			});

			AddMappingToDictionary(mapping, AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Value, new List<string>()
			{
				"*CB*DRC*GSTOut*-",
				"*AR*INV*GSTOut*-",
				"*AR*CRD*GSTOut*-",
				"*AR*ADJ*GSTOut*-",
				"*CT*CBT*GSTOut*-",
			});

			AddMappingToDictionary(mapping, AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.Value, new List<string>()
			{
				"*AR*INV*PenGSTOut*-",
				"*AR*CRD*PenGSTOut*-",
				"*AR*ADJ*PenGSTOut*-",
				"*CT*CBT*PenGSTOut*",
			});

			AddMappingToDictionary(mapping, AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Value, new List<string>()
			{
				"*JC*WIP*WIPCtrl*-",
				"*JC*WIP*WIPCtrl*Rev",
			});

			AddMappingToDictionary(mapping, AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value, new List<string>()
			{
				"*JC*ACR*ACRCtrl*-",
				"*JC*ACR*ACRCtrl*Rev",
			});

			AddMappingToDictionary(mapping, AccountingConfigurationRegistry.Instance.CFXAccount.Value, new List<string>()
			{
				"*JC*JNL*CFX*",
			});

			AddMappingToDictionary(mapping, AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.Value, new List<string>()
			{
				"*JC*JRJ*JRJCtrl*",
			});

			#endregion

			return mapping;
		}

		Dictionary<ZGuid, string> GenerateAccountPkToNumberMapping()
		{
			var keys = AccountPkToSubCodesMapping.Keys.ToList();
			keys.Add(new Guid(AccountingConfigurationRegistry.Instance.PLAppropriationAccount.Value.ToString()));

			return RetrieveAccountMapping(keys);
		}

		DataTable GenerateAccountPkToSubCodesMappingTable()
		{
			var result = new DataTable();
			result.Locale = CultureInfo.InvariantCulture;
			result.Columns.Add((NoResString)"Code", typeof(string));
			result.Columns.Add((NoResString)"Guid", typeof(Guid));

			foreach (var controlAccount in AccountPkToSubCodesMapping)
			{
				foreach (var code in controlAccount.Value)
				{
					var row = result.NewRow();
					row["Guid"] = controlAccount.Key;
					row["Code"] = code;

					result.Rows.Add(row);
				}
			}

			return result;
		}

		List<string> GenerateControlAccountsNumbers()
		{
			return AccountPkToNumberMapping.Select(kvp => kvp.Value).ToList();
		}

		Dictionary<ZGuid, string> RetrieveAccountMapping(IEnumerable<Guid> primaryKeys)
		{
			var accounts = "'" + string.Join("','", primaryKeys) + "'";
			var controlAccountsSQL = $"SELECT AG_AccountNum, AG_PK FROM AccGLHeader WHERE AG_PK IN ( {accounts} )";

			var factory = new BusinessObjectFactory();
			var controlAccounts = new DynamicBusinessObjectCollection(factory);
			controlAccounts.Load(controlAccountsSQL);

			var result = new Dictionary<ZGuid, string>();

			foreach (var account in controlAccounts)
			{
				var accountPK = new ZGuid(account[AccGLHeaderSchema.PK]);
				result.Add(accountPK, account[AccGLHeaderSchema.AG_AccountNum].ToString());
			}

			return result;
		}

		void AddMappingToDictionary(Dictionary<Guid, HashSet<string>> dictionary, Guid key, IEnumerable<string> values)
		{
			if (dictionary.ContainsKey(key))
			{
				values.ForEach(v => dictionary[key].Add(v));
			}
			else
			{
				dictionary.Add(key, new HashSet<string>(values));
			}
		}

		ReadOnlyDictionary<ZGuid, string> accountPkToNumberMapping;
		ReadOnlyDictionary<Guid, HashSet<string>> accountPkToSubCodesMapping;
		ReadOnlyCollection<string> controlAccountsNumbers;
		DataTable accountPkToSubCodesTable;
	}
}
