using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using ZClientEDI.Business.Licencing;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	#region Dependent LicenceDatabase Collection

	[ModuleID("LicenceDatabase")]
	public class LicenceDatabaseCollection : DependentBusinessObjectCollection<LicenceDatabase, LicenceEnterprise>
	{
		public LicenceDatabaseCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public LicenceDatabaseCollection(LicenceEnterprise licEnterprise, ZQuery filter)
			: base(licEnterprise, filter)
		{
		}

		public LicenceDatabaseCollection(LicenceEnterprise licEnterprise, BusinessObjectFactory factory)
			: base(licEnterprise, factory)
		{
		}

		#region Implementation

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);

			// If Master is null, you probably want to use a LicenceDatabaseNonDependentCollection (below)
			((LicenceDatabase)child).LD_LE = Master.PK;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var licenceDatabase = (LicenceDatabase)child;

			var enterpriseOrg = Master.Header;
			if (enterpriseOrg != null)
			{
				if (enterpriseOrg.UNLOCO != null)
				{
					licenceDatabase.LD_ServerCode = enterpriseOrg.UNLOCO.Code.SubstringSafe(2, 3);
				}
				licenceDatabase.LD_OH_WebAccessOrg = enterpriseOrg.PK;
			}

			if (Master.LE_IsInternal)
			{
				licenceDatabase.LD_Billable = DatabaseBillableFlagList.Codes.No;
			}
		}

		#endregion
	}

	#endregion

	#region Non-Dependent LicenceDatabase Collection

	[ModuleID("LicenceDatabase")]
	public class LicenceDatabaseNonDependentCollection : BusinessObjectCollection<LicenceDatabase>
	{
		public LicenceDatabaseNonDependentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public LicenceDatabaseNonDependentCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			if (property.Name is AutoLicenceDatabase.Schema.LD_HL_CurrentRunningVersion)
			{
				return new ComparerForSort<LicenceDatabase, VersionNumber>(l =>
					l?.CurrentVersion?.VersionNumber ?? default, direction);
			}

			if (property.Name is AutoLicenceDatabase.Schema.LD_HL_CurrentSentVersion)
			{
				return new ComparerForSort<LicenceDatabase, VersionNumber>(l =>
					l?.SentVersion?.VersionNumber ?? default, direction);
			}

			return base.GetComparerForSort(property, direction);
		}
	}

	public class LicenceDatabaseNonDependentActiveCollection : ActiveBusinessObjectCollection<LicenceDatabase>
	{
		public LicenceDatabaseNonDependentActiveCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public LicenceDatabaseNonDependentActiveCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}

	#endregion

	/// <summary>
	/// LicenceDatabase with a globally unique CodeProperty.
	/// </summary>
	[CodeProperty(nameof(LD_DatabaseNumberAsText))]
	public class LicenceDatabaseGlobal : LicenceDatabase
	{
		public LicenceDatabaseGlobal(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		public ZString LD_DatabaseNumberAsText => LD_DatabaseNumber.ToString("G", CultureInfo.InvariantCulture);
		public virtual ZPropertyInfo LD_DatabaseNumberAsTextInfo => GetWrappedZPropertyInfo(nameof(LD_DatabaseNumberAsText), x => LD_DatabaseNumberInfo);
	}

	[ModuleID("LicenceDatabase")]
	public class LicenceDatabaseGlobalCollection : ActiveBusinessObjectCollection<LicenceDatabaseGlobal>
	{
		public LicenceDatabaseGlobalCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}

