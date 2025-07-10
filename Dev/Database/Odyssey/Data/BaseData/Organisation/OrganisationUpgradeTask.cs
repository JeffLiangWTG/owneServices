namespace Enterprise.DbUpgrader.Data.BaseData.Organisation
{
	using System;
	using System.Collections.Generic;
	using Enterprise.DbUpgrader.Data.BaseData.Common;
	using Enterprise.ZArchitecture.Schema;

	public class OrganisationUpgradeTask : SystemInstallDataUpgradeTask
	{
		public OrganisationUpgradeTask()
			: base(new OrganisationDataFile())
		{
		}

		/// <summary>
		/// Does not update existing records
		/// </summary>
		protected override void UpdateColumn(string columnName, System.Data.DataRow targetRow, System.Data.DataRow sourceRow)
		{
			// Skips update
		}

		/// <summary>
		/// Only insert if there are no NK conflicts.
		/// </summary>
		protected override void DoInsert(System.Data.DataRow sourceRow, System.Data.DataTable targetTable, ref int targetIndex)
		{
			bool shouldInsert = true;

			switch (targetTable.TableName)
			{
				case OrgHeaderSchema.Constants.TableName:
					// OrgHeader = OH_Code
					if (SameNkExists(OrgHeaderSchema.OH_Code, sourceRow[OrgHeaderSchema.OH_Code.Name].ToString()))
					{
						nonInsertedOrgPks.Add((Guid)sourceRow[OrgHeaderSchema.Constants.PK]);
						shouldInsert = false;
					}
					break;

				case OrgAddressSchema.Constants.TableName:
					// OrgAddress = OA_OH + OA_Code
					Guid oaOrgFk = (Guid)sourceRow[OrgAddressSchema.Constants.OA_OH];
					if (
						nonInsertedOrgPks.Contains(oaOrgFk) ||
						SameNkExists_OrgAddress(oaOrgFk, sourceRow[OrgAddressSchema.Constants.OA_Code].ToString()))
					{
						nonInsertedAddressPks.Add((Guid)sourceRow[OrgAddressSchema.Constants.PK]);
						shouldInsert = false;
					}
					break;

				case OrgAddressCapabilitySchema.Constants.TableName:
					// OrgAddressCapability = PZ_OA + PZ_AddressType (FK_UX__PZ_OA_PZ_AddressType)
					Guid pzAddrFk = (Guid)sourceRow[OrgAddressCapabilitySchema.Constants.PZ_OA];
					if (
						nonInsertedAddressPks.Contains(pzAddrFk) ||
						SameNkExists_OrgAddressCapability(pzAddrFk, sourceRow[OrgAddressCapabilitySchema.Constants.PZ_AddressType].ToString()))
					{
						shouldInsert = false;
					}
					break;

				case OrgWebURLSchema.Constants.TableName:
					// OrgWebURL = none (use PU_OH + PU_Type)
					Guid puOrgFk = (Guid)sourceRow[OrgWebURLSchema.Constants.PU_OH];
					if (
						nonInsertedOrgPks.Contains(puOrgFk) ||
						SameNkExists_OrgWebURL(puOrgFk, sourceRow[OrgWebURLSchema.Constants.PU_Type].ToString()))
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

		bool SameNkExists_OrgAddress(Guid orgPk, string addrCode)
		{
			string sqlText = String.Format("SELECT TOP 1 OA_Code FROM dbo.OrgAddress WHERE OA_OH = '{0}' AND OA_Code = '{1}'", orgPk.ToString(), addrCode);
			return IsRecordInDatabase(sqlText);
		}

		bool SameNkExists_OrgAddressCapability(Guid addrPk, string addrType)
		{
			string sqlText = String.Format("SELECT TOP 1 PZ_AddressType FROM dbo.OrgAddressCapability WHERE PZ_OA = '{0}' AND PZ_AddressType = '{1}'", addrPk.ToString(), addrType);
			return IsRecordInDatabase(sqlText);
		}

		bool SameNkExists_OrgWebURL(Guid orgPk, string urlType)
		{
			string sqlText = String.Format("SELECT TOP 1 PU_Type FROM dbo.OrgWebURL WHERE PU_OH = '{0}' AND PU_Type = '{1}'", orgPk.ToString(), urlType);
			return IsRecordInDatabase(sqlText);
		}

		readonly List<Guid> nonInsertedOrgPks = new List<Guid>();
		readonly List<Guid> nonInsertedAddressPks = new List<Guid>();
	}
}
