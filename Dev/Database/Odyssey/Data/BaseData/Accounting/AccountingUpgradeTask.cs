using System;
using System.Collections.Generic;
using System.Data;
using Enterprise.DbUpgrader.Data.BaseData.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data.BaseData.Accounting
{
	public class AccountingUpgradeTask : SystemInstallDataUpgradeTask
	{
		public AccountingUpgradeTask()
			: base(new AccountingDataFile())
		{
		}

		/// <summary>
		/// Existing records are not updated
		/// </summary>
		protected override void UpdateColumn(string columnName, DataRow targetRow, DataRow sourceRow)
		{
			// Skips update
		}

		protected override void DoInsert(DataRow sourceRow, DataTable targetTable, ref int targetIndex)
		{
			bool shouldInsert = true;

			switch (targetTable.TableName)
			{
				case OrgCreditorGroupSchema.Constants.TableName:
					// NK = OG_Code
					if (SameNkExists(OrgCreditorGroupSchema.OG_Code, sourceRow[OrgCreditorGroupSchema.OG_Code.Name].ToString()))
					{
						shouldInsert = false;
					}
					break;

				case OrgDebtorGroupSchema.Constants.TableName:
					// NK = OJ_Code
					if (SameNkExists(OrgDebtorGroupSchema.OJ_Code, sourceRow[OrgDebtorGroupSchema.OJ_Code.Name].ToString()))
					{
						shouldInsert = false;
					}
					break;

				case AccGroupsSchema.Constants.TableName:
					// NK = AR_Code
					if (SameNkExists(AccGroupsSchema.AR_Code, sourceRow[AccGroupsSchema.AR_Code.Name].ToString()))
					{
						shouldInsert = false;
					}
					break;

				case AccGLHeaderSchema.Constants.TableName:
					// NK = AG_AccountNum
					// There may be self-references. Do not need to watch out because 99.99% probability that the referenced record will not exist until later
					if (SameNkExists(AccGLHeaderSchema.AG_AccountNum, sourceRow[AccGLHeaderSchema.AG_AccountNum.Name].ToString()))
					{
						nonInsertedGLHeaderPks.Add((Guid)sourceRow[AccGLHeaderSchema.PK.Name]);
						shouldInsert = false;
					}
					break;

				case AccTaxRateSchema.Constants.TableName:
					// Relies on:
					// RefCountry
					// NK: AT_Code + AT_RN_NKCountry

					if (!SameNkExists(RefCountrySchema.RN_Code, sourceRow[AccTaxRateSchema.AT_RN_NKCountry.Name].ToString()) ||
						SameCompositeAccTaxRateNkExists(sourceRow[AccTaxRateSchema.Constants.AT_Code].ToString(), sourceRow[AccTaxRateSchema.Constants.AT_RN_NKCountry].ToString()))
					{
						shouldInsert = false;
						nonInsertedTaxRatesPks.Add((Guid)sourceRow[AccTaxRateSchema.PK.Name]);
					}
					break;

				case AccChargeCodeSchema.Constants.TableName:
					// 	Relies on:
					//  GlbCompany (DEMO = 03052ED3-2C64-49AC-97D8-C6079D5015B5)
					//  AccGLHeader
					//  AccGroups
					//  AccTaxRate
					object taxRatePk = sourceRow[AccChargeCodeSchema.AC_AT_GSTRate.Name];
					object glHeader1Pk = sourceRow[AccChargeCodeSchema.AC_AG_AccrualAccount.Name];
					object glHeader2Pk = sourceRow[AccChargeCodeSchema.AC_AG_CostAccount.Name];
					object glHeader3Pk = sourceRow[AccChargeCodeSchema.AC_AG_RevenueAccount.Name];
					object glHeader4Pk = sourceRow[AccChargeCodeSchema.AC_AG_WIPAccount.Name];
					object group1Pk = sourceRow[AccChargeCodeSchema.AC_AR_ExpenseGroup.Name];
					object group2Pk = sourceRow[AccChargeCodeSchema.AC_AR_SalesGroup.Name];

					if (
						taxRatePk is Guid && nonInsertedTaxRatesPks.Contains((Guid)taxRatePk) ||
						glHeader1Pk is Guid &&  nonInsertedGLHeaderPks.Contains((Guid)glHeader1Pk) ||
						glHeader2Pk is Guid && nonInsertedGLHeaderPks.Contains((Guid)glHeader2Pk) ||
						glHeader3Pk is Guid && nonInsertedGLHeaderPks.Contains((Guid)glHeader3Pk) ||
						glHeader4Pk is Guid && nonInsertedGLHeaderPks.Contains((Guid)glHeader4Pk) ||
						SameCompositeAccChargeCodeNkExists(sourceRow[AccChargeCodeSchema.Constants.AC_Code].ToString(), sourceRow[AccChargeCodeSchema.Constants.AC_GC])
						)
					{
						shouldInsert = false;
					}
					break;
			}

			if (shouldInsert)
			{
				base.DoInsert(sourceRow, targetTable, ref targetIndex);
			}
		}

		bool SameCompositeAccTaxRateNkExists(string code, string countryCode)
		{
			string sqlText = String.Format("SELECT TOP 1 AT_PK FROM dbo.AccTaxRate WHERE AT_Code = '{0}' AND AT_RN_NKCountry = '{1}'", code, countryCode);
			return IsRecordInDatabase(sqlText);
		}

		bool SameCompositeAccChargeCodeNkExists(string code, object gcFk)
		{
			string sqlText = String.Format("SELECT TOP 1 AC_PK FROM dbo.AccChargeCode WHERE AC_Code = '{0}' AND {1}", code, GetGuidWhere("AC_GC", gcFk));
			return IsRecordInDatabase(sqlText);
		}

		string GetGuidWhere(string guidColumn, object value)
		{
			return (value != DBNull.Value) ? string.Format("{0} = '{1}'", guidColumn, value.ToString()) : string.Format("{0} is null", guidColumn);
		}

		readonly List<Guid> nonInsertedGLHeaderPks = new List<Guid>();
		readonly List<Guid> nonInsertedTaxRatesPks = new List<Guid>();
	}
}
