using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public sealed class ScavengingPurgeSettings : RegistryBusinessObjectCollectionTemplate<ScavengingPurgeItem>
	{
		public const string ClientStatisticsArchiveCode = "CSA";
		public const string ClientOrgConsolCode  = "COC";
		public const string ClientOrgImportHistory = "OIH";

		public ScavengingPurgeSettings()
			: base(null, null)
		{
		}

		public static ZDateTime GetMaxPurgeDateTime(ScavengingPurgeItem purgeItem)
		{
			if (purgeItem.PurgeTimeUnit == ScavengingPurgeItem.TimeUnit.Week)
			{
				return ZDateTime.UtcNow.AddDays(-purgeItem.PurgeTime * 7);
			}
			if (purgeItem.PurgeTimeUnit == ScavengingPurgeItem.TimeUnit.Month)
			{
				return ZDateTime.UtcNow.AddMonths(-purgeItem.PurgeTime);
			}
			if (purgeItem.PurgeTimeUnit == ScavengingPurgeItem.TimeUnit.Year)
			{
				return ZDateTime.UtcNow.AddYears(-purgeItem.PurgeTime);
			}

			return ZDateTime.UtcNow.AddYears(-100);
		}

		public static ScavengingPurgeSettings GetDefaults()
		{
			var result = new ScavengingPurgeSettings();
			var clientStatistics = result.AddNew();
			clientStatistics.Code = ClientStatisticsArchiveCode;
			clientStatistics.Description = ResString.GetMultilingualString("f1b752dc-34f8-424d-98c0-0c3218e6ec7d", "Client Statistics XML Archive");
			clientStatistics.PurgeTime = 6;
			clientStatistics.PurgeTimeUnit = ScavengingPurgeItem.TimeUnit.Month;
			var clientOrgConsol = result.AddNew();
			clientOrgConsol.Code = ClientOrgConsolCode;
			clientOrgConsol.Description = ResString.GetMultilingualString("4f8cff5d-a290-4b38-8cd0-b12cdfbdcf10", "Client Consol");
			clientOrgConsol.PurgeTime = 5;
			clientOrgConsol.PurgeTimeUnit = ScavengingPurgeItem.TimeUnit.Year;
			var clientOrgImportHistory = result.AddNew();
			clientOrgImportHistory.Code = ClientOrgImportHistory;
			clientOrgImportHistory.Description = ResString.GetMultilingualString("86edf566-c489-49b5-9e0a-eb68003c39c8", "Client Org. Import History");
			clientOrgImportHistory.PurgeTime = 6;
			clientOrgImportHistory.PurgeTimeUnit = ScavengingPurgeItem.TimeUnit.Month;
			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ScavengingPurgeItem();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ScavengingPurgeSettings();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}

