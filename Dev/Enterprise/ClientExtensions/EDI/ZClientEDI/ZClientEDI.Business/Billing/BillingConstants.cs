using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.Billing.Business
{
	public static class BillingConstants
	{
		#region Price Fee Type

		public static class FeeType
		{
			public const string Included = "INC";
			public const string Licence = "LEM";
			public const string Module = "MUM";
			public const string NamedUser = "NUM";

			public const string OldWebModule = "MU0";
			public const string Upgrade = "UPG";

			public const string Transactional = "TRA";
			public const string TransactionalModule = "TRU";
			public const string TransactionalOneVolumeBreak = "TRB";
			public const string VolumeDatabaseFee = "VDF";
			public const string MinimumFee = "MFE";
			public const string MinimumFeePerReference = "MFR";

			public const string PerVehicle = "VEH";
			public const string AgentEntity = "AGE";
			public const string Database = "DAT";
			public const string Country = "COU";

			public const string CoreUsers = "CUM";
			public const string SelfHostedDatabaseUsers = "SHU";
			public const string DatabaseUsers = "DBU";
			public const string PerGBPerMonth = "GBM";
			public const string Per10GBPerMonthMin1GB = "G1M";
			public const string PerMBPerMonthMin1GB = "M1G";
			public const string PerGBPerMonthMin1GB = "G1G";
			public const string PerMailBoxPerMonth = "MBM";
			public const string PerDevicePerMonth = "DEM";
			public const string PerMessageElement = "MSE";
			public const string PerPage = "PEP";

			public const string DatabaseLanguage = "DAL";
			public const string DatabaseLanguageZ = "DAZ";
			public const string DatabaseCountry  = "DAC";

			public const string UsersPerCountryVolumeBreak = "UCB";

			public const string CountryTier = "COT";

			public static bool IsOnDemand(string feeType)
			{
				return
					feeType == BillingConstants.FeeType.Included ||
					feeType == BillingConstants.FeeType.Licence ||
					feeType == BillingConstants.FeeType.Module ||
					feeType == BillingConstants.FeeType.NamedUser ||
					feeType == BillingConstants.FeeType.OldWebModule ||
					feeType == BillingConstants.FeeType.Database ||
					feeType == BillingConstants.FeeType.CoreUsers ||
					feeType == BillingConstants.FeeType.SelfHostedDatabaseUsers ||
					feeType == BillingConstants.FeeType.DatabaseUsers ||
					feeType == BillingConstants.FeeType.DatabaseLanguage ||
					feeType == BillingConstants.FeeType.DatabaseLanguageZ ||
					feeType == BillingConstants.FeeType.DatabaseCountry ||
					feeType == BillingConstants.FeeType.Country ||
					feeType == BillingConstants.FeeType.UsersPerCountryVolumeBreak;
			}

			public static bool IsMaintenance(string feeType)
			{
				return
					feeType == BillingConstants.FeeType.Licence ||
					feeType == BillingConstants.FeeType.Module ||
					feeType == BillingConstants.FeeType.NamedUser ||
					feeType == BillingConstants.FeeType.OldWebModule ||
					feeType == BillingConstants.FeeType.Database;
			}

			public static bool IsPerDatabaseUser(string feeType)
			{
				return feeType == BillingConstants.FeeType.DatabaseUsers
					|| feeType == BillingConstants.FeeType.SelfHostedDatabaseUsers;
			}

			public static bool IsPerDatabaseInstance(string feeType)
			{
				return feeType == BillingConstants.FeeType.Database
					|| feeType == BillingConstants.FeeType.VolumeDatabaseFee
					|| feeType == BillingConstants.FeeType.DatabaseLanguage
					|| feeType == BillingConstants.FeeType.DatabaseLanguageZ
					|| feeType == BillingConstants.FeeType.DatabaseCountry;
			}

			public static bool IsPerDatabase(string feeType)
			{
				return
					IsPerDatabaseInstance(feeType) ||
					IsPerDatabaseUser(feeType);
			}

			public static bool IsPerLicence(string feeType)
			{
				return feeType == BillingConstants.FeeType.Licence;
			}

			public static bool IsVolumeBreak(string feeType)
			{
				return feeType == BillingConstants.FeeType.TransactionalOneVolumeBreak || feeType == BillingConstants.FeeType.UsersPerCountryVolumeBreak;
			}
		}

		public static class FeeTypeDescriptions
		{
			public const string Included = "Included";
			public const string Licence = "Licence Entity";
			public const string Module = "Module Users";
			public const string NamedUser = "Named User";
			public const string RegisteredUser = "Registered User";

			public const string OldWebModule = "Web Module Users";
			public const string Upgrade = "Upgrade";

			public const string Transactional = "Transactional";
			public const string TransactionalModule = "Transactional With Free Count Per Module User";
			public const string TransactionalOneVolumeBreak = "Transactional With One Price Volume Break For All";
			public const string VolumeDatabaseFee = "Volume Database Fee";
			public const string MinimumFee = "Minimum Fee";
			public const string MinimumFeePerReference = "Minimum Fee Per Reference";

			public const string PerVehicle = "Per Vehicle";
			public const string AgentEntity = "Agent+Entity";
			public const string Database = "Database";
			public const string Country = "Country";
			public const string CountryTier = "Country Tier Pricing";

			public const string CoreUsers = "Core Users";
			public const string SelfHostedDatabaseUsers = "Self Hosted Database Unique Users";
			public const string DatabaseUsers = "Database Unique Users";
			public const string PerGBPerMonth = "Per GB Per Month";
			public const string Per10GBPerMonthMin1GB = "Per 10GB (free under 1GB)";
			public const string PerMBPerMonthMin1GB = "Per MB (1GB included)";
			public const string PerGBPerMonthMin1GB = "Per GB (1GB included)";
			public const string PerMailBoxPerMonth = "Per MailBox Per Month";
			public const string PerDevicePerMonth = "Per Device Per Month";
			public const string PerMessageElement = "Per Message Element";
			public const string PerPage = "Per Page";

			public const string DatabaseLanguage = "Per Language (Multi Language/Multi Country(Region) customer)";
			public const string DatabaseLanguageZ = "Per Language (Free if most users are local)";
			public const string DatabaseCountry = "Per Database (Multi Language/Multi Country(Region) customer)";

			public const string UsersPerCountryVolumeBreak = "Users Per Country Volume Break";
		}

		static CodeDescriptionPairList GetFeeTypeListInternal()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(FeeType.Included, FeeTypeDescriptions.Included);
			result.AddPair(FeeType.Licence, FeeTypeDescriptions.Licence);
			result.AddPair(FeeType.Module, FeeTypeDescriptions.Module);
			result.AddPair(FeeType.NamedUser, FeeTypeDescriptions.NamedUser);

			result.AddPair(FeeType.OldWebModule, FeeTypeDescriptions.OldWebModule);
			result.AddPair(FeeType.Upgrade, FeeTypeDescriptions.Upgrade);

			result.AddPair(FeeType.Transactional, FeeTypeDescriptions.Transactional);
			result.AddPair(FeeType.TransactionalModule, FeeTypeDescriptions.TransactionalModule);
			result.AddPair(FeeType.TransactionalOneVolumeBreak, FeeTypeDescriptions.TransactionalOneVolumeBreak);
			result.AddPair(FeeType.VolumeDatabaseFee, FeeTypeDescriptions.VolumeDatabaseFee);
			result.AddPair(FeeType.MinimumFee, FeeTypeDescriptions.MinimumFee);
			result.AddPair(FeeType.MinimumFeePerReference, FeeTypeDescriptions.MinimumFeePerReference);

			result.AddPair(FeeType.PerVehicle, FeeTypeDescriptions.PerVehicle);
			result.AddPair(FeeType.AgentEntity, FeeTypeDescriptions.AgentEntity);
			result.AddPair(FeeType.Database, FeeTypeDescriptions.Database);
			result.AddPair(FeeType.Country, FeeTypeDescriptions.Country);

			result.AddPair(FeeType.CoreUsers, FeeTypeDescriptions.CoreUsers);
			result.AddPair(FeeType.SelfHostedDatabaseUsers, FeeTypeDescriptions.SelfHostedDatabaseUsers);
			result.AddPair(FeeType.DatabaseUsers, FeeTypeDescriptions.DatabaseUsers);
			result.AddPair(FeeType.PerGBPerMonth, FeeTypeDescriptions.PerGBPerMonth);
			result.AddPair(FeeType.Per10GBPerMonthMin1GB, FeeTypeDescriptions.Per10GBPerMonthMin1GB);
			result.AddPair(FeeType.PerMBPerMonthMin1GB, FeeTypeDescriptions.PerMBPerMonthMin1GB);
			result.AddPair(FeeType.PerGBPerMonthMin1GB, FeeTypeDescriptions.PerGBPerMonthMin1GB);
			result.AddPair(FeeType.PerMailBoxPerMonth, FeeTypeDescriptions.PerMailBoxPerMonth);
			result.AddPair(FeeType.PerDevicePerMonth, FeeTypeDescriptions.PerDevicePerMonth);
			result.AddPair(FeeType.PerMessageElement, FeeTypeDescriptions.PerMessageElement);
			result.AddPair(FeeType.PerPage, FeeTypeDescriptions.PerPage);

			result.AddPair(FeeType.DatabaseLanguage, FeeTypeDescriptions.DatabaseLanguage);
			result.AddPair(FeeType.DatabaseLanguageZ, FeeTypeDescriptions.DatabaseLanguageZ);
			result.AddPair(FeeType.DatabaseCountry, FeeTypeDescriptions.DatabaseCountry);

			result.AddPair(FeeType.UsersPerCountryVolumeBreak, FeeTypeDescriptions.UsersPerCountryVolumeBreak);
			result.AddPair(FeeType.CountryTier, FeeTypeDescriptions.CountryTier);

			return result;
		}

		static CodeDescriptionPairList GetStlFeeTypeListInternal()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();

			result.AddPair(FeeType.Transactional, ResString.GetMultilingualString("FeeTypeDescriptions|Transactional", FeeTypeDescriptions.Transactional));
			result.AddPair(FeeType.TransactionalOneVolumeBreak, ResString.GetMultilingualString("FeeTypeDescriptions|TransactionalOneVolumeBreak", FeeTypeDescriptions.TransactionalOneVolumeBreak));
			result.AddPair(FeeType.MinimumFee, ResString.GetMultilingualString("FeeTypeDescriptions|MinimumFee", FeeTypeDescriptions.MinimumFee));
			result.AddPair(FeeType.MinimumFeePerReference, ResString.GetMultilingualString("FeeTypeDescriptions|MinimumFeePerReference", FeeTypeDescriptions.MinimumFeePerReference));

			result.AddPair(FeeType.Database, ResString.GetMultilingualString("FeeTypeDescriptions|Database", FeeTypeDescriptions.Database));

			result.AddPair(FeeType.PerGBPerMonth, ResString.GetMultilingualString("FeeTypeDescriptions|PerGBPerMonth", FeeTypeDescriptions.PerGBPerMonth));
			result.AddPair(FeeType.Per10GBPerMonthMin1GB, ResString.GetMultilingualString("FeeTypeDescriptions|Per10GBPerMonthMin1GB", FeeTypeDescriptions.Per10GBPerMonthMin1GB));
			result.AddPair(FeeType.PerMBPerMonthMin1GB, ResString.GetMultilingualString("FeeTypeDescriptions|PerMBPerMonthMin1GB", FeeTypeDescriptions.PerMBPerMonthMin1GB));
			result.AddPair(FeeType.PerGBPerMonthMin1GB, ResString.GetMultilingualString("FeeTypeDescriptions|PerGBPerMonthMin1GB", FeeTypeDescriptions.PerGBPerMonthMin1GB));
			result.AddPair(FeeType.PerDevicePerMonth, ResString.GetMultilingualString("FeeTypeDescriptions|PerDevicePerMonth", FeeTypeDescriptions.PerDevicePerMonth));

			result.AddPair(FeeType.DatabaseLanguage, ResString.GetMultilingualString("FeeTypeDescriptions|DatabaseLanguage", FeeTypeDescriptions.DatabaseLanguage));
			result.AddPair(FeeType.DatabaseLanguageZ, ResString.GetMultilingualString("FeeTypeDescriptions|DatabaseLanguageZ", FeeTypeDescriptions.DatabaseLanguageZ));
			result.AddPair(FeeType.DatabaseCountry, ResString.GetMultilingualString("FeeTypeDescriptions|DatabaseCountry", FeeTypeDescriptions.DatabaseCountry));

			result.AddPair(FeeType.Country, ResString.GetMultilingualString("FeeTypeDescriptions|Country", FeeTypeDescriptions.Country));
			result.AddPair(FeeType.UsersPerCountryVolumeBreak, ResString.GetMultilingualString("FeeTypeDescriptions|UsersPerCountryVolumeBreak", FeeTypeDescriptions.UsersPerCountryVolumeBreak));
			result.AddPair(FeeType.CountryTier, ResString.GetMultilingualString("FeeTypeDescriptions|CountryTier", FeeTypeDescriptions.CountryTier));

			return result;
		}

		public static ReadOnlyCodeDescriptionPairList GetCachedFeeTypeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("BillingConstants.FeeTypeList", () =>
			{
				return GetFeeTypeListInternal();
			});
		}

		public static ReadOnlyCodeDescriptionPairList GetFeeTypeList()
		{
			return GetFeeTypeListInternal();
		}

		public static ReadOnlyCodeDescriptionPairList GetCachedStlFeeTypeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("BillingConstants.StlFeeTypeList", () =>
			{
				return GetStlFeeTypeListInternal();
			});
		}

		#endregion

		#region Price Header Type

		public static class PriceHeaderType
		{
			public const string ODM = BillingConstants.BillingSystem.ODM;
			public const string STL = BillingConstants.BillingSystem.STL;
			public const string Maintenance = BillingConstants.BillingSystem.Maintenance;
			public const string ABMCustoms = BillingConstants.BillingSystem.ABMCustoms;
			public const string EHub = "HUB";
			public const string GlobalContainerTracking = BillingConstants.BillingSystem.GlobalContainerTracking;
			public const string LDaaS = "LDS";
			public const string Other = "MSC";
			public const string BorderWise = "BOR";
			public const string GoldenTax = "GTS";
			public const string FlightStats = "FMS";
			public const string CargoWiseNext = "CWN";

			public static CodeDescriptionPairList PriceHeaderTypeList
			{
				get { return GetPriceHeaderTypeList(); }
			}

			public static bool IsUsedInBillingStl(string code)
			{
				return code == STL
					|| code == EHub
					|| code == GlobalContainerTracking
					|| code == LDaaS
					|| code == ABMCustoms
					|| code == BorderWise
					|| code == GoldenTax
					|| code == FlightStats
					|| code == CargoWiseNext
					|| EDIDataRegistry.Instance.BillingStlGlobalPriceLists.Value.ContainsCode(code)
					|| EDIDataRegistry.Instance.UsageBillingSettings.Value.PriceLists.ContainsPriceListCode(code);
			}

			public static bool IsGlobal(string code)
			{
				return code == GlobalContainerTracking
					|| code == LDaaS
					|| code == ABMCustoms
					|| code == GoldenTax
					|| code == FlightStats
					|| code == CargoWiseNext
					|| EDIDataRegistry.Instance.BillingStlGlobalPriceLists.Value.ContainsCode(code);
			}

			public static bool IsAlwaysStandard(string code)
			{
				return IsUsedInBillingStl(code)
					&& code != EHub;
			}

			public static bool CanHaveMultipleCurrencies(string code)
			{
				return code == STL
					|| code == LDaaS
					|| code == BorderWise;
			}

			public static CodeDescriptionPairList GetPriceHeaderTypeList()
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(PriceHeaderType.ODM, BillingSystem.Descriptions.ODM);
				result.AddPair(PriceHeaderType.Maintenance, BillingSystem.Descriptions.Maintenance);
				result.AddPair(PriceHeaderType.ABMCustoms, "ABM Customs");
				result.AddPair(PriceHeaderType.EHub, "eHub");
				result.AddPair(PriceHeaderType.GlobalContainerTracking, "Container Automation");
				result.AddPair(PriceHeaderType.LDaaS, "Logistics Devices as a Service");
				result.AddPair(PriceHeaderType.Other, "Miscellaneous");
				result.AddPair(PriceHeaderType.STL, BillingSystem.Descriptions.STL);
				result.AddPair(PriceHeaderType.BorderWise, "BorderWise");
				result.AddPair(PriceHeaderType.GoldenTax, "GoldenTax");
				result.AddPair(PriceHeaderType.FlightStats, "Air Waybill Automation");
				result.AddPair(PriceHeaderType.CargoWiseNext, "CargoWise Next");

				foreach (ICodeDescription pair in EDIDataRegistry.Instance.BillingStlGlobalPriceLists.Value)
				{
					result.AddPairIfNotExist(pair.Code, pair.Description);
				}

				result.AddPairsIfNotExist(EDIDataRegistry.Instance.UsageBillingSettings.Value.PriceLists.GetPriceListCodeDescriptionPairList().OfType<ICodeDescription>());
				return result;
			}
		}

		#endregion

		#region Billing Model

		public static class BillingModel
		{
			public const string ODM = BillingConstants.BillingSystem.ODM;
			public const string STL = BillingConstants.BillingSystem.STL;

			public static CodeDescriptionPairList BillingModelList
			{
				get { return GetBillingModelList(); }
			}

			public static CodeDescriptionPairList GetBillingModelList()
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(PriceHeaderType.ODM, BillingSystem.Descriptions.ODM);
				result.AddPair(PriceHeaderType.STL, BillingSystem.Descriptions.STL);
				return result;
			}
		}

		#endregion

		#region Discount Calculators

		public static class DiscountCalculator
		{
			public const string Percentage = "PER";
			public const string Volume = "VOL";
			public const string Prepayment = "PRE";

			public const string DomesticEntity = "DOM";
			public const string DevelopingCountry = "DCO";
			public const string WiseCloud = "WIS";
			public const string SingleCountry = "COU";
			public const string OrgMembership = "MEM";
			public const string MasterOrgDevelopingCountry = "MDC";
			public const string ProductBundle = "BUN";

			public static class Descriptions
			{
				public const string Percentage = "Percentage";
				public const string Volume = "Volume";
				public const string Prepayment = "Prepayment";

				public const string DomesticEntity = "Domestic Entity (No Overseas Office)";
				public const string DevelopingCountry = "Developing Country";
				public const string WiseCloud = "For WiseCloud Customers Only";
				public const string SingleCountry = "Country";
				public const string OrgMembership = "Membership";
				public const string MasterOrgDevelopingCountry = "Master Org Developing Country";
				public const string ProductBundle = "Product Bundle";
			}

			public static bool IsCustomerPercentageSettingApplicable(string discountType)
			{
				return discountType != DomesticEntity;
			}
		}

		public static CodeDescriptionPairList GetStlDiscountCalculatorList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(DiscountCalculator.Percentage, DiscountCalculator.Descriptions.Percentage);
			result.AddPair(DiscountCalculator.Volume, DiscountCalculator.Descriptions.Volume);
			result.AddPair(DiscountCalculator.DomesticEntity, DiscountCalculator.Descriptions.DomesticEntity);
			result.AddPair(DiscountCalculator.DevelopingCountry, DiscountCalculator.Descriptions.DevelopingCountry);
			result.AddPair(DiscountCalculator.Prepayment, DiscountCalculator.Descriptions.Prepayment);
			result.AddPair(DiscountCalculator.WiseCloud, DiscountCalculator.Descriptions.WiseCloud);
			result.AddPair(DiscountCalculator.SingleCountry, DiscountCalculator.Descriptions.SingleCountry);
			result.AddPair(DiscountCalculator.OrgMembership, DiscountCalculator.Descriptions.OrgMembership);
			result.AddPair(DiscountCalculator.MasterOrgDevelopingCountry, DiscountCalculator.Descriptions.MasterOrgDevelopingCountry);
			result.AddPair(DiscountCalculator.ProductBundle, DiscountCalculator.Descriptions.ProductBundle);

			return result;
		}

		#endregion

		#region Discount Type

		public static class DiscountType
		{
			public const string Volume = "VOL";
			public const string IncrementalVolume = "IVO";
			public const string Prepayment = "PRE";
			public const string Commitment = "COM";
			public const string Special = "SPE";
			public const string ModuleSpecific = "MOD";
			public const string Capped = "CAP";
			public const string MinimumFee = "MFE";
			public const string Surcharge = "SUR";
			public const string WiseCloud = "WIS";
		}

		public static CodeDescriptionPairList GetDiscountTypeList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(DiscountType.Volume, "Volume");
			result.AddPair(DiscountType.IncrementalVolume, "Incremental Volume");
			result.AddPair(DiscountType.Prepayment, "Prepayment");
			result.AddPair(DiscountType.Commitment, "Commitment");
			result.AddPair(DiscountType.Special, "Special");
			result.AddPair(DiscountType.ModuleSpecific, "Module Specific");
			result.AddPair(DiscountType.Capped, "Capped");
			result.AddPair(DiscountType.MinimumFee, "Minimum Fee");
			result.AddPair(DiscountType.Surcharge, "Surcharge");
			result.AddPair(DiscountType.WiseCloud, "WiseCloud");

			return result;
		}

		#endregion

		#region core discounts

		public static class CoreDiscount
		{
			public const string SingleEntityDomesticUnder20Users = "DS1";
			public const string SingleEntityDomestic20PlusUsers = "DS2";
			public const string MultiEntityDomestic = "DM1";

			public const string ChinaDomesticUnder20Users = "DDC";
			public const string IndiaDomesticUnder20Users = "DDI";
			public const string Tier4DomesticUnder20Users = "DD4";
		}

		#endregion

		#region System Types

		public static class Category
		{
			public const string GoldenTax = "ACC";
		}

		public static class BillingSystem
		{
			public const string All = "ALL";
			public const string Fee = "FEE";
			public const string ODM = "ODM"; // LicenceTypes.Codes.ODM
			public const string STL = "STL";
			public const string eBACCA = "IQM"; // Licences.ImportQuarantineMessaging
			public const string ImporterSecurityFiling = "ISF"; // Licences.ImporterSecurityFiling
			public const string Fax = "FAX"; // ? Licences.FaxEngine
			public const string DeniedPartyScreening = "DPS";
			public const string ExDocs = "AED";
			public const string DistanceCalculatorGeneric = "DCG";
			public const string DistanceCalculatorPcMiler = "DCP";
			public const string S8Cargo = "RSH";
			public const string Maintenance = "PUR"; // LicenceTypes.Codes.PUR
			public const string HostingStorage = "HOS";
			public const string WiseCloudUser = "WCU";
			public const string HostingRemoteDevices = "HRD";
			public const string HostingDataAccess = "HDA";
			public const string AirlineMessaging = "AMG";
			public const string ABMCustoms = "ABM";
			public const string NZCustoms = "NZC";
			public const string ClientMapping = "CMP";
			public const string eAdaptor = "EAD";
			public const string E2E = "E2E";
			public const string JapanAFR = "JPC";
			public const string USCustoms = "USC";
			public const string RailincByMessage = "RIM";
			public const string PortMessaging = "PMG";
			public const string GlobalContainerTracking = "CTR";
			public const string OceanTracing = "OCT";
			public const string OceanTracingLegacy = "OCK";
			public const string ForwardAir = "FWA";
			public const string ShippingPortMessaging = "SPM";
			public const string GBCustoms = "GBC";
			public const string OceanCarrierMessaging = "SHI";
			public const string ASYCUDA = "ASC";
			public const string Service = "SVC";
			public const string ZACustoms = "ZAC";
			public const string BorderWise = "BOR";
			public const string FlightStats = "FMS";
			public const string DocumentSigning = "DOS";
			public const string CargoWiseNext = "CWN";

			public static class Descriptions
			{
				public const string ODM = "On Demand";
				public const string STL = "Seat Transaction License";
				public const string Maintenance = "One Time/Maintenance";
			}
		}

		public static CodeDescriptionPairList GetBillingSystemList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(BillingSystem.Fee, ResString.GetMultilingualString("BillingSystem|Fee", "Product Fees"));
			result.AddPair(BillingSystem.ODM, ResString.GetMultilingualString("BillingSystem|ODM", BillingSystem.Descriptions.ODM));
			result.AddPair(BillingSystem.STL, ResString.GetMultilingualString("BillingSystem|STL", BillingSystem.Descriptions.STL));
			result.AddPair(BillingSystem.eBACCA, ResString.GetMultilingualString("BillingSystem|eBACCA", "eBACCa"));
			result.AddPair(BillingSystem.ImporterSecurityFiling, ResString.GetMultilingualString("BillingSystem|ImporterSecurityFiling", "Importer Security Filing"));
			result.AddPair(BillingSystem.Fax, ResString.GetMultilingualString("BillingSystem|Fax", "Fax"));
			result.AddPair(BillingSystem.DeniedPartyScreening, ResString.GetMultilingualString("BillingSystem|DeniedPartyScreening", "Denied Party Screening"));
			result.AddPair(BillingSystem.ExDocs, ResString.GetMultilingualString("BillingSystem|ExDocs", "Quarantine ExDocs"));
			result.AddPair(BillingSystem.DistanceCalculatorGeneric, ResString.GetMultilingualString("BillingSystem|DistanceCalculatorGeneric", "Distance Calculator - Generic"));
			result.AddPair(BillingSystem.DistanceCalculatorPcMiler, ResString.GetMultilingualString("BillingSystem|DistanceCalculatorPcMiler", "Distance Calculator - PC Miler"));
			result.AddPair(BillingSystem.S8Cargo, ResString.GetMultilingualString("BillingSystem|S8Cargo", "Online Airline Schedules"));
			result.AddPair(BillingSystem.Maintenance, ResString.GetMultilingualString("BillingSystem|Maintenance", "Application Services Renewal"));
			result.AddPair(BillingSystem.HostingStorage, ResString.GetMultilingualString("BillingSystem|HostingStorage", "WiseCloud Storage"));
			result.AddPair(BillingSystem.HostingRemoteDevices, ResString.GetMultilingualString("BillingSystem|HostingRemoteDevices", "WiseCloud Remote Devices"));
			result.AddPair(BillingSystem.HostingDataAccess, ResString.GetMultilingualString("BillingSystem|HostingDataAccess", "Read-only Access Excess"));
			result.AddPair(BillingSystem.AirlineMessaging, ResString.GetMultilingualString("BillingSystem|AirlineMessaging", "Airline Messaging"));
			result.AddPair(BillingSystem.ABMCustoms, ResString.GetMultilingualString("BillingSystem|ABMCustoms", "ABM Customs"));
			result.AddPair(BillingSystem.NZCustoms, ResString.GetMultilingualString("BillingSystem|NZCustoms", "NZ Customs"));
			result.AddPair(BillingSystem.ClientMapping, ResString.GetMultilingualString("BillingSystem|ClientMapping", "eHub Interfaces - Transactional Fee"));
			result.AddPair(BillingSystem.eAdaptor, ResString.GetMultilingualString("BillingSystem|eAdaptor", "eAdaptor"));
			result.AddPair(BillingSystem.E2E, ResString.GetMultilingualString("BillingSystem|E2E", "E2E Messaging"));
			result.AddPair(BillingSystem.JapanAFR, ResString.GetMultilingualString("BillingSystem|JapanAFR", "Japan AFR"));
			result.AddPair(BillingSystem.USCustoms, ResString.GetMultilingualString("BillingSystem|USCustoms", "US Customs"));
			result.AddPair(BillingSystem.RailincByMessage, ResString.GetMultilingualString("BillingSystem|RailincByMessage", "Railinc"));
			result.AddPair(BillingSystem.PortMessaging, ResString.GetMultilingualString("BillingSystem|PortMessaging", "Port Messaging"));
			result.AddPair(BillingSystem.GlobalContainerTracking, ResString.GetMultilingualString("BillingSystem|GlobalContainerTracking", "Container Automation"));
			result.AddPair(BillingSystem.OceanTracing, ResString.GetMultilingualString("BillingSystem|OceanTracing", "Ocean Tracing"));
			result.AddPair(BillingSystem.OceanTracingLegacy, ResString.GetMultilingualString("BillingSystem|OceanTracingLegacy", "Container Movement Tracking"));
			result.AddPair(BillingSystem.ForwardAir, ResString.GetMultilingualString("BillingSystem|ForwardAir", "Forward Air"));
			result.AddPair(BillingSystem.ShippingPortMessaging, ResString.GetMultilingualString("BillingSystem|ShippingPortMessaging", "Shipping Port Messaging"));
			result.AddPair(BillingSystem.GBCustoms, ResString.GetMultilingualString("BillingSystem|GBCustoms", "GB Customs"));
			result.AddPair(BillingSystem.OceanCarrierMessaging, ResString.GetMultilingualString("BillingSystem|OceanCarrierMessaging", "Ocean Carrier Messaging"));
			result.AddPair(BillingSystem.ASYCUDA, ResString.GetMultilingualString("BillingSystem|ASYCUDA", "ASYCUDA"));
			result.AddPair(BillingSystem.Service, ResString.GetMultilingualString("BillingSystem|Service", "Service (Premium/LDaaS/other)"));
			result.AddPair(BillingSystem.ZACustoms, ResString.GetMultilingualString("BillingSystem|ZACustoms", "ZA Customs"));
			result.AddPair(BillingSystem.BorderWise, ResString.GetMultilingualString("BillingSystem|BorderWise", "BorderWise"));
			result.AddPair(BillingSystem.FlightStats, ResString.GetMultilingualString("BillingSystem|FlightStats", "Air Waybill Automation"));
			result.AddPair(BillingSystem.WiseCloudUser, ResString.GetMultilingualString("BillingSystem|WiseCloudUser", "WiseCloud User"));

			result.AddRange(EDIDataRegistry.Instance.LicenceUsageBilledPerTransaction.Value);

			return result;
		}

		public static bool IsTransactional(string systemCode)
		{
			return systemCode != BillingSystem.Fee
				&& systemCode != BillingSystem.ODM
				&& systemCode != BillingSystem.Maintenance
				&& systemCode != BillingSystem.HostingStorage
				&& systemCode != BillingSystem.HostingRemoteDevices
				&& systemCode != BillingSystem.HostingDataAccess;
		}

		[ThreadStatic]
		static CodeDescriptionPairList billingSystemList;

		public static CodeDescriptionPairList BillingSystemList
		{
			get { return billingSystemList ?? (billingSystemList = GetBillingSystemList()); }
		}

		internal static void ResetBillingSystemListForTest()
		{
			billingSystemList = null;
		}

		public static CodeDescriptionPairList GetAllBillingSystems()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(BillingSystem.All, "All");
			// Ensure these come first in the list
			result.AddPair(BillingSystem.ODM, "On Demand");
			result.AddPair(BillingSystem.Maintenance, "Maintenance");
			foreach (ICodeDescription pair in GetBillingSystemList())
			{
				result.AddPairIfNotExist(pair.Code, pair.Description);
			}
			return result;
		}

		public static CodeDescriptionPairList AllBillingSystems
		{
			get { return allBillingSystems ?? (allBillingSystems = GetAllBillingSystems()); }
		}

		[ThreadStatic]
		static CodeDescriptionPairList allBillingSystems;

		public static CodeDescriptionPairList OnDemandOrOneTimeBillingSystems
		{
			get { return onDemandOrOneTimeBillingSystems ?? (onDemandOrOneTimeBillingSystems = GetOnDemandOrOneTimeBillingSystems()); }
		}

		[ThreadStatic]
		static CodeDescriptionPairList onDemandOrOneTimeBillingSystems;

		public static CodeDescriptionPairList GetOnDemandOrOneTimeBillingSystems()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(BillingSystem.ODM, BillingSystem.Descriptions.ODM);
			result.AddPair(BillingSystem.Maintenance, BillingSystem.Descriptions.Maintenance);
			return result;
		}

		#endregion

		#region Licence Edition

		public static class LicenceEdition
		{
			public const string Express = "EXP";
			public const string Country = "COU";
			public const string Region = "REG";
			public const string Universal = "UNI";
		}

		public static CodeDescriptionPairList GetLicenceEditionList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(LicenceEdition.Express, "Express");
			result.AddPair(LicenceEdition.Country, "Country");
			result.AddPair(LicenceEdition.Region, "Region");
			result.AddPair(LicenceEdition.Universal, "Universal");

			return result;
		}

		#endregion

		#region NonProduction Database

		public static class NonProductionDatabase
		{
			public const string CoreModuleCode = "#NC";
			public const string CoreModuleDescription = "ediCore - Test License";
		}

		#endregion

		#region Hosting

		public static class Hosting
		{
			public const string Code = "#HO";
			public const string Description = "Hosting";

			public const string DataStorageCode = "#HD";
			public const string eDocsStorageCode = "#HE";
			public const string UltraFastStorageCode = "#HA";
			public const string NonProductionStorageCode = "#HN";
			public const string RemoteDevicesCode = "#HR";
			public const string PrintServersCode = "#HS";
			public const string DataAccessCode = "#HG";
			public const string DataAccessByGBCode = "#RG";
			public const string WiseCloudUserFeeCode = "COW";
			public const string WiseCloudProductionLicenceCode = "#HP";

			public const int MBperGB = 1024;
		}

		#endregion

		#region Core Module Code

		public const string CoreModuleCode = "COR";
		public const string RegisteredUserModuleCode = "USO";

		#endregion

		#region Amount Formating

		public const int RoundingDecimals = 2;
		public const int RoundingThreeDecimals = 3;
		public const string AmountOneDecimalFormat = "#,##0.0";
		public const string AmountDecimalFormat = "#,##0.00";
		public const string AmountThreeDecimalFormat = "#,##0.000";
		public const string AmountFourDecimalFormat = "#,##0.0000";
		public const string ExchangeRateDecimalFormat = "#,##0.00000";

		public const string MessageTimeFormat = "dd-MMM-yy HH:mm:ss"; // need a date format string with seconds

		public class FormatOptions
		{
			public FormatOptions()
			{
				DecimalFormat = BillingConstants.AmountDecimalFormat;
				RoundingDecimals = BillingConstants.RoundingDecimals;
			}

			public int RoundingDecimals { get; private set; }
			public string DecimalFormat { get; private set; }

			public static FormatOptions GetOptionsByProduct(string productCode, BusinessObjectFactory factoryForCache)
			{
				return factoryForCache.GetCachedValue($"FormatOptions.GetOptionsByProduct.{productCode}", () =>
				{
					var result = new FormatOptions();
					if (!string.IsNullOrEmpty(productCode) && productCode != BillingConstants.BillingSystem.STL)
					{
						if (EDIDataRegistry.Instance.ProductsWithThreeDecimalBillingSummary.Value.ContainsCode(productCode))
						{
							result.DecimalFormat = BillingConstants.AmountThreeDecimalFormat;
							result.RoundingDecimals = BillingConstants.RoundingThreeDecimals;
						}
					}

					return result;
				});
			}
		}

		#endregion

		#region Licence Settings

		public static class LicenceSetting
		{
			public const string BuyingGroup = "BUY";
			public const string Discount = "DIS";
			public const string Fixed = "FIX";
			public const string Price = "PRI";
			public const string PriceTier = "PRT";
			public const string ConversionCredit = "CCR";
			public const string Commitment = "COM";
			public const string BorderWisePurchasedLicences = "BWL";
			public const string HighVolumeFeature = "HVF";
			public const string VersionSurcharge = "GPR";
			public const string MinSpend = "MIN";
			public const string DiscountSuspensionPolicy = "DSP";
			public const string BillingSummaryCurrency = "BSC";

			public static class Descriptions
			{
				public const string BuyingGroup = "Buying Group";
				public const string Discount = "Discount";
				public const string Price = "Price";
				public const string PriceTier = "Price (Tier)";
				public const string ConversionCredit = "Conversion Credit";
				public const string Commitment = "Commitment";
				public const string BorderWisePurchasedLicences = "BorderWise Purchased Licences";
				public const string HighVolumeFeature = "High Volume Feature";
				public const string VersionSurcharge = "Non-Current Version Surcharge";
				public const string MinSpend = "Minimum Spend";
				public const string DiscountSuspensionPolicy = "Discount Suspension Policy";
				public const string BillingSummaryCurrency = "Billing Summary Currency";
			}

			public static CodeDescriptionPairList GetTypeList()
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(LicenceSetting.BuyingGroup, Descriptions.BuyingGroup);
				result.AddPair(LicenceSetting.Commitment, Descriptions.Commitment);
				result.AddPair(LicenceSetting.ConversionCredit, Descriptions.ConversionCredit);
				result.AddPair(LicenceSetting.Discount, Descriptions.Discount);
				result.AddPair(LicenceSetting.HighVolumeFeature, Descriptions.HighVolumeFeature);
				result.AddPair(LicenceSetting.MinSpend, Descriptions.MinSpend);
				result.AddPair(LicenceSetting.Price, Descriptions.Price);
				result.AddPair(LicenceSetting.PriceTier, Descriptions.PriceTier);
				result.AddPair(LicenceSetting.VersionSurcharge, Descriptions.VersionSurcharge);
				result.AddPair(LicenceSetting.BorderWisePurchasedLicences, Descriptions.BorderWisePurchasedLicences);
				result.AddPair(LicenceSetting.DiscountSuspensionPolicy, Descriptions.DiscountSuspensionPolicy);
				result.AddPair(LicenceSetting.BillingSummaryCurrency, Descriptions.BillingSummaryCurrency);
				return result;
			}
		}

		#endregion

		#region Client Licence Billing Discount

		public static class DiscountBreakUnit
		{
			public const string Currency = "CUR";
			public const string LicenceUnits = "LIC";
		}

		public static CodeDescriptionPairList GetDiscountBreakUnitList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(DiscountBreakUnit.Currency, "Currency");
			result.AddPair(DiscountBreakUnit.LicenceUnits, "Licence Units");
			return result;
		}

		#endregion

		#region GeographicCompliance

		public static class GeographicCompliance
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "Static fields in this class do not need to be thread-static")]
			public static readonly IEnumerable<string> CountryUsagePriceCodes = new [] { "DCC", "MCC" };
		}

		#endregion

		#region BorderWise

		public static class BorderWise
		{
			public const string UserPriceCode = "BW1";
			public const string UserPriceCode2 = "BW2";
			public const string UserPriceCode3 = "BW3";
			public const string ExtraMachinePriceCode = "BW9";
			public const string StudentUserPriceCode = "BS1";
			public const string StudentUserPriceCode2 = "BS2";
			public const string StudentUserPriceCode3 = "BS3";
			public const string StudentExtraMachinePriceCode = "BS9";
			public const string TradefoxOrDigeratiUserPriceCode = "BT1";
			public const string TradefoxOrDigeratiUserPriceCode2 = "BT2";
			public const string TradefoxOrDigeratiUserPriceCode3 = "BT3";
			public const string TradefoxOrDigeratiExtraMachinePriceCode = "BT9";

			public const string AUSingleWindowPartnerOnlyPriceCode = "AUP";
			public const string AUSingleWindowCW1OnlyPriceCode = "AUW";
			public const string AUSingleWindowPartnerCW1PriceCode = "AUX";
			public const string AUSingleWindowStandalonePriceCode = "AUS";
			public const string AUProPackPriceCode = "AU3";

			public const string NZSingleWindowPartnerOnlyPriceCode = "NZP";
			public const string NZSingleWindowCW1OnlyPriceCode = "NZW";
			public const string NZSingleWindowPartnerCW1PriceCode = "NZX";
			public const string NZSingleWindowStandalonePriceCode = "NZS";
			public const string NZProPackPriceCode = "NZ3";

			public const string FreeTrialPriceCode = "BF1";

			public const string GlobalPartnerOrCW1PriceCode = "BWX";
			public const string GlobalStandalonePriceCode = "BWS";
			public const string GlobalProPackPriceCode = "BW3";

			public static CodeDescriptionPairList GetBorderWiseModuleList()
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();

				result.AddPair(AUSingleWindowStandalonePriceCode, "Single Window (AU) Standalone");
				result.AddPair(AUSingleWindowPartnerCW1PriceCode, "Single Window (AU) CW1 + Partner");
				result.AddPair(AUSingleWindowPartnerOnlyPriceCode, "Single Window (AU) Partner");
				result.AddPair(AUSingleWindowCW1OnlyPriceCode, "Single Window (AU) CW1");
				result.AddPair(AUProPackPriceCode, "Pro Pack (AU)");
				result.AddPair(NZSingleWindowStandalonePriceCode, "Single Window (NZ) Standalone");
				result.AddPair(NZSingleWindowPartnerCW1PriceCode, "Single Window (NZ) CW1 + Partner");
				result.AddPair(NZSingleWindowPartnerOnlyPriceCode, "Single Window (NZ) Partner");
				result.AddPair(NZSingleWindowCW1OnlyPriceCode, "Single Window (NZ) CW1");
				result.AddPair(NZProPackPriceCode, "Pro Pack (NZ)");
				result.AddPair(GlobalPartnerOrCW1PriceCode, "Global Partner and/or CW1");
				result.AddPair(GlobalStandalonePriceCode, "Global Standalone");
				result.AddPair(GlobalProPackPriceCode, "Pro Pack (Global)");

				result.AddPair(StudentUserPriceCode, "Student");
				result.AddPair(FreeTrialPriceCode, "Free Trial");

				return result;
			}

			public static ReadOnlyCodeDescriptionPairList GetCachedBorderWiseModuleList(BusinessObjectFactory factory)
			{
				return factory.GetCachedValue("BillingConstants.BorderWiseModuleList", () =>
				{
					return GetBorderWiseModuleList();
				});
			}
			public static ReadOnlyCodeDescriptionPairList GetCachedBorderWiseModuleAndGroupList(BusinessObjectFactory factory)
			{
				return factory.GetCachedValue("BillingConstants.BorderWiseModuleList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddRange(EDIDataRegistry.Instance.BorderWisePurchasedGroups.Value);
					result.AddRange(GetBorderWiseModuleList());
					return result;
				});
			}
		}

		public static class FileExtensions
		{
			public const string Pdf = "pdf";
			public const string Csv = "csv";
		}

		#endregion

		public static class Fee
		{
			public static class TaxDateCode
			{
				public const string FeeTaxAtCurrentDate = "CUR";
				public const string FeeTaxAtStartDate = "STA";
				public const string FeeTaxAtEndDate = "END";

				public static class Descriptions
				{
					public const string FeeTaxAtInvoiceDate = "Current Date";
					public const string FeeTaxAtStartDate = "Service Start Date";
					public const string FeeTaxAtEndDate = "Service End Date";
				}
			}
		}
	}
}

