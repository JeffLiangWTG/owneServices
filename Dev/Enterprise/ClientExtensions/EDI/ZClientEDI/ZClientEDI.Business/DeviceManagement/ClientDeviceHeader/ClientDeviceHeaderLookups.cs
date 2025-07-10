using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.RemoteDeviceManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DeviceManagement.Business
{
	public class ClientDeviceHeaderLookups : DmgDeviceHeaderLookups
	{
		public ClientDeviceHeaderLookups(AutoDmgDeviceHeader parent)
			: base(parent)
		{
		}

		public LicenceEnterpriseCollectionForEntCodeFilter EnterpriseCodes => new LicenceEnterpriseCollectionForEntCodeFilter(Factory);

		public LicenceDatabaseNonDependentCollection ServerCodes
		{
			get
			{
				var code = ParentClientDeviceHeader.CDH_EnterpriseCode;
				if (string.IsNullOrEmpty(code))
				{
					return new LicenceDatabaseNonDependentCollection(Factory, new ZQuery() { IsNoResultQuery = true });
				}

				var licenceEnterpriseQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceEnterpriseSchema.PK);
				licenceEnterpriseQuery.AddToFilter(LicenceEnterpriseSchema.LE_EnterpriseCode, code);
				var licenceDatabaseQuery = new ZDBOnlyQuery(typeof(LicenceDatabase));
				licenceDatabaseQuery.AddSubQuery(LicenceDatabaseSchema.LD_LE, licenceEnterpriseQuery, JoinCondition.And);

				var collection = new LicenceDatabaseNonDependentCollection(Factory, licenceDatabaseQuery);

				var licenceEnterprise = Factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, code));
				if (licenceEnterprise != null)
				{
					collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Enterprise Code", "Property", licenceEnterprise.PK, false));
				}

				return collection;
			}
		}

		public OrgHeaderCollection Organisations => new OrgHeaderCollection(Factory);

		ClientDeviceHeader ParentClientDeviceHeader => (ClientDeviceHeader)base.Parent;

		public CodeDescriptionPairList ClientParentTypes
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(GlbStaffSchema.Constants.Prefix, "Staff");
				result.AddPair(RefEquipmentSchema.Constants.Prefix, "Equipment");
				return result;
			}
		}

		public CodeDescriptionPairList StatusTypes
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Statuses.Active, "Active");
				result.AddPair(Statuses.Destroyed, "Destroyed");
				result.AddPair(Statuses.Faulty, "Faulty");
				result.AddPair(Statuses.Retired, "Retired");
				result.AddPair(Statuses.Lost, "Lost");
				result.AddPair(Statuses.Inactive, "Inactive");
				return result;
			}
		}

		public CodeDescriptionPairList DeviceKinds
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Kinds.Android, "Android");
				result.AddPair(Kinds.AppleMobility, "iOS (Apple)");
				result.AddPair(Kinds.WiseTechEmbedded, "WTG IVU/RSA Unit");
				result.AddPair(Kinds.WindowsMobilityLegacy, "Windows CE/Mobile");
				result.AddPair(Kinds.Unknown, "Unknown/Other");
				return result;
			}
		}

		public static class Statuses
		{
			public const string Active = "ACT";
			public const string Destroyed = "DES";
			public const string Faulty = "FAU";
			public const string Retired = "RET";
			public const string Lost = "LOS";
			public const string Inactive = "IAC";
		}

		public static class Kinds
		{
			public const string Android = "AND";
			public const string AppleMobility = "IOS";
			public const string WiseTechEmbedded = "EMB";
			public const string WindowsMobilityLegacy = "WIN";
			public const string Unknown = "UNK";
		}
	}
}

