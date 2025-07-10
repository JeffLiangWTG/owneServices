using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceEnterpriseFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public LicenceEnterpriseFetchStrategy(LicenceEnterprise licEnterprise)
			: base(licEnterprise)
		{
		}

		LicenceEnterprise LicEnterprise
		{
			get { return BusinessObject as LicenceEnterprise; }
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			bool needsDatabases = false;
			bool needsOrgs = false;
			bool needsOrgAddress = false;

			foreach (TableColumn column in columns)
			{
				if (column.ColumnName.StartsWith("Organisation", StringComparison.Ordinal))
				{
					needsOrgs = true;
				}

				if (column.ColumnName == "NumOfDatabasesRegistered")
				{
					needsDatabases = true;
				}

				if (column.ColumnName.Contains("MainAddress"))
				{
					needsOrgs = true;
					needsOrgAddress = true;
				}
			}

			if (needsOrgs)
			{
				AddHint(OrgHeaderSchema.PK, LicEnterprise.LE_OH);
			}

			if (needsDatabases)
			{
				AddHint(LicenceDatabaseSchema.LD_LE, LicEnterprise.PK);
			}

			if (needsOrgAddress)
			{
				AddHint(OrgAddressSchema.OA_OH, LicEnterprise.LE_OH);
			}
		}

		protected virtual void AddHint(SchemaColumn column, ZGuid key)
		{
			Factory.AddFetchHint(column, key);
		}
	}
}

