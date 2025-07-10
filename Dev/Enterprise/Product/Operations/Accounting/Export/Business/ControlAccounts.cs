using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Export.Business
{
	public class ControlAccounts
	{
		public ControlAccounts(BatchExportDataAccess dataAccess, string companyCode)
		{
			this.dataAccess = dataAccess;

			LoadControlAccounts(companyCode);

			throwExceptionIfAnyControlAccountsMissing(companyCode);

			cfxAccounts = new Dictionary<Triple, GLAccount>();
		}

		readonly BatchExportDataAccess dataAccess;

		void LoadControlAccounts(string companyCode)
		{
			string sqlText = string.Format("EXEC ControlAccounts '{0}'", companyCode);
			using (var command = dataAccess.GetCommand(sqlText))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					string accountType = (string)reader["Type"];
					GLAccount glAccount = new GLAccount();
					glAccount.AccountCode = (ZString?)(string)reader[AccGLHeaderSchema.AG_AccountNum.Name];
					glAccount.Description = (ZString?)(string)reader[AccGLHeaderSchema.AG_Description.Name];

					switch (accountType)
					{
						case "ARControlAccount":
							ARControlAccount = glAccount;
							break;
						case "APControlAccount":
							APControlAccount = glAccount;
							break;
						case "ARControlSuspenseAccount":
							ARControlSuspenseAccount = glAccount;
							break;
						case "APControlSuspenseAccount":
							APControlSuspenseAccount = glAccount;
							break;
						case "WIPControlAccount":
							WIPControlAccount = glAccount;
							break;
						case "AccrualControlAccount":
							AccrualControlAccount = glAccount;
							break;
						case "GSTInputAccount":
							GSTInputAccount = glAccount;
							break;
						case "GSTOutputAccount":
							GSTOutputAccount = glAccount;
							break;
						case "PendingGSTInputAccount":
							PendingGSTInputAccount = glAccount;
							break;
						case "PendingGSTOutputAccount":
							PendingGSTOutputAccount = glAccount;
							break;
						case "JobRevenueJournalAccount":
							JobRevenueJournalAccount = glAccount;
							break;
					}
				}
			}
		}

		void throwExceptionIfAnyControlAccountsMissing(string companyCode)
		{
			bool isCashBasisVATEnabled = false;
			var command = dataAccess.GetCommand("SELECT GC_IsGSTCashBasis FROM dbo.GlbCompany WHERE GC_Code = @GC_Code", (Name: "@GC_Code", Value: companyCode));
			using (var reader = command.ExecuteReader())
			{
				if (reader.Read())
				{
					var gC_IsGSTCashBasis = (bool)reader[GlbCompanySchema.GC_IsGSTCashBasis.Name];
					isCashBasisVATEnabled = gC_IsGSTCashBasis;
				}
			}
			List<string> errorMessages = new List<string>();
			if (ARControlAccount == null || ARControlAccount.AccountCode.Value.IsEmpty || ARControlAccount.Description.Value.IsEmpty)
			{
				errorMessages.Add(Res.GetString("7266C0B2-94F9-4908-9714-CBBB87A654A4", "AR Control Account."));
			}
			if (ARControlSuspenseAccount == null || ARControlSuspenseAccount.AccountCode.Value.IsEmpty || ARControlSuspenseAccount.Description.Value.IsEmpty)
			{
				errorMessages.Add(Res.GetString("D555236B-9AA1-424F-8FCF-F887411187FB", "AR Suspense Control Account."));
			}
			if (APControlAccount == null || APControlAccount.AccountCode.Value.IsEmpty || APControlAccount.Description.Value.IsEmpty)
			{
				errorMessages.Add(Res.GetString("30D67067-EB89-4C2C-993C-25D5E34BD6BE", "AP Control Account."));
			}
			if (APControlSuspenseAccount == null || APControlSuspenseAccount.AccountCode.Value.IsEmpty || APControlSuspenseAccount.Description.Value.IsEmpty)
			{
				errorMessages.Add(Res.GetString("FAA9E987-6A0B-4BED-9527-C26C24C829C2", "AP Suspense Control Account."));
			}
			if (WIPControlAccount == null || WIPControlAccount.AccountCode.Value.IsEmpty || WIPControlAccount.Description.Value.IsEmpty)
			{
				errorMessages.Add(Res.GetString("C8D810E9-56F2-4E23-9622-6C654C2AFD2F", "WIP Control Account."));
			}
			if (AccrualControlAccount == null || AccrualControlAccount.AccountCode.Value.IsEmpty || AccrualControlAccount.Description.Value.IsEmpty)
			{
				errorMessages.Add(Res.GetString("8E711D67-BD89-4A39-A602-F111F2CFC78A", "Accrual Control Account."));
			}
			if (GSTInputAccount == null || GSTInputAccount.AccountCode.Value.IsEmpty || GSTInputAccount.Description.Value.IsEmpty)
			{
				errorMessages.Add(Res.GetString("9DA9130D-A272-4225-B01A-EE35BE50088C", "Reportable Tax Input Control Account."));
			}
			if (GSTOutputAccount == null || GSTOutputAccount.AccountCode.Value.IsEmpty || GSTOutputAccount.Description.Value.IsEmpty)
			{
				errorMessages.Add(Res.GetString("6DC94C2B-54A4-4DD6-BBEE-B7ADA6AC88B8", "Reportable Tax Output Control Account."));
			}
			if (isCashBasisVATEnabled && (PendingGSTInputAccount == null || PendingGSTInputAccount.AccountCode.Value.IsEmpty || PendingGSTInputAccount.Description.Value.IsEmpty))
			{
				errorMessages.Add(Res.GetString("2568EFDF-E408-429C-A80D-EC8130C5A81C", "Pending Tax Input Control Account."));
			}
			if (isCashBasisVATEnabled && (PendingGSTOutputAccount == null || PendingGSTOutputAccount.AccountCode.Value.IsEmpty || PendingGSTOutputAccount.Description.Value.IsEmpty))
			{
				errorMessages.Add(Res.GetString("A9D64A2E-2E99-45BB-8CD8-407D004B0E86", "Pending Tax Output Control Account."));
			}
			if (JobRevenueJournalAccount == null || JobRevenueJournalAccount.AccountCode.Value.IsEmpty || JobRevenueJournalAccount.Description.Value.IsEmpty)
			{
				errorMessages.Add(Res.GetString("46377281-4494-488C-948F-EC2F79BE45AA", "Job Revenue Journal Control Account."));
			}
			if (errorMessages.Any())
			{
				throw new DataObjectValidationException(Res.GetString("4AC1BB0C-920A-434E-A479-0EFD4E05B56E", "Please set up the control account(s) in the registry Accounting > General Ledger Defaults > Control Account: \r\n\r\n- {0}", string.Join("\r\n- ", errorMessages)));
			}
		}

		public GLAccount ARControlAccount { get; private set; }
		public GLAccount APControlAccount { get; private set; }
		public GLAccount ARControlSuspenseAccount { get; private set; }
		public GLAccount APControlSuspenseAccount { get; private set; }
		public GLAccount WIPControlAccount { get; private set; }
		public GLAccount AccrualControlAccount { get; private set; }
		public GLAccount GSTInputAccount { get; private set; }
		public GLAccount GSTOutputAccount { get; private set; }
		public GLAccount PendingGSTInputAccount { get; private set; }
		public GLAccount PendingGSTOutputAccount { get; private set; }
		public GLAccount JobRevenueJournalAccount { get; private set; }

		public class Triple
		{
			public Triple(Guid first, Guid second, Guid third)
			{
				First = first;
				Second = second;
				Third = third;
			}

			public readonly Guid First;
			public readonly Guid Second;
			public readonly Guid Third;

			public override int GetHashCode()
			{
				return First.GetHashCode() ^ Second.GetHashCode() ^ Third.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				Triple o = obj as Triple;
				if (o == null)
				{
					throw new ArgumentException("obj must be a Triple");
				}
				else
				{
					return o.First.Equals(First) && o.Second.Equals(Second) && o.Third.Equals(Third);
				}
			}
		}

		readonly Dictionary<Triple, GLAccount> cfxAccounts;

		public GLAccount GetCFXAccount(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			GLAccount result;
			var key = new Triple(companyPK, branchPK, departmentPK);
			if (!cfxAccounts.TryGetValue(key, out result))
			{
				result = dataAccess.GetCFXAccount(companyPK, branchPK, departmentPK);

				if (result == null || result.AccountCode.Value == ZString.Empty)
				{
					throw new DataObjectValidationException(string.Format("Unable to get CFX Account for Company={0} Branch={1} Department={2}", companyPK, branchPK, departmentPK));
				}

				cfxAccounts.Add(key, result);
			}

			return result;
		}
	}
}
