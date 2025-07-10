using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Client.EDI.Registry.Business.Test;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Client.EDI.Billing.Business.BillingConstants;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class BillingConstantsTest : TestCaseWithFactory
	{
		public void TestFeeTypeList()
		{
			var list = BillingConstants.GetFeeTypeList();
			AssertEquals("Count", 32, list.Count);

			AssertEquals("Included", list.GetDescriptionFromCode(BillingConstants.FeeType.Included));
			AssertEquals("Licence Entity", list.GetDescriptionFromCode(BillingConstants.FeeType.Licence));
			AssertEquals("Module Users", list.GetDescriptionFromCode(BillingConstants.FeeType.Module));
			AssertEquals("Named User", list.GetDescriptionFromCode(BillingConstants.FeeType.NamedUser));

			AssertEquals("Web Module Users", list.GetDescriptionFromCode(BillingConstants.FeeType.OldWebModule));
			AssertEquals("Upgrade", list.GetDescriptionFromCode(BillingConstants.FeeType.Upgrade));

			AssertEquals("Transactional", list.GetDescriptionFromCode(BillingConstants.FeeType.Transactional));
			AssertEquals("Transactional With Free Count Per Module User", list.GetDescriptionFromCode(BillingConstants.FeeType.TransactionalModule));
			AssertEquals("Transactional With One Price Volume Break For All", list.GetDescriptionFromCode(BillingConstants.FeeType.TransactionalOneVolumeBreak));
			AssertEquals("Volume Database Fee", list.GetDescriptionFromCode(BillingConstants.FeeType.VolumeDatabaseFee));

			AssertEquals("Per Vehicle", list.GetDescriptionFromCode(BillingConstants.FeeType.PerVehicle));
			AssertEquals("Agent+Entity", list.GetDescriptionFromCode(BillingConstants.FeeType.AgentEntity));
			AssertEquals("Database", list.GetDescriptionFromCode(BillingConstants.FeeType.Database));

			AssertEquals("Core Users", list.GetDescriptionFromCode(BillingConstants.FeeType.CoreUsers));
			AssertEquals("Self Hosted Database Unique Users", list.GetDescriptionFromCode(BillingConstants.FeeType.SelfHostedDatabaseUsers));
			AssertEquals("Database Unique Users", list.GetDescriptionFromCode(BillingConstants.FeeType.DatabaseUsers));
			AssertEquals("Per GB Per Month", list.GetDescriptionFromCode(BillingConstants.FeeType.PerGBPerMonth));
			AssertEquals("Per 10GB (free under 1GB)", list.GetDescriptionFromCode(BillingConstants.FeeType.Per10GBPerMonthMin1GB));
			AssertEquals("Per MB (1GB included)", list.GetDescriptionFromCode(BillingConstants.FeeType.PerMBPerMonthMin1GB));
			AssertEquals("Per GB (1GB included)", list.GetDescriptionFromCode(BillingConstants.FeeType.PerGBPerMonthMin1GB));
			AssertEquals("Per MailBox Per Month", list.GetDescriptionFromCode(BillingConstants.FeeType.PerMailBoxPerMonth));
			AssertEquals("Per Device Per Month", list.GetDescriptionFromCode(BillingConstants.FeeType.PerDevicePerMonth));
			AssertEquals("Per Message Element", list.GetDescriptionFromCode(BillingConstants.FeeType.PerMessageElement));
			AssertEquals("Per Page", list.GetDescriptionFromCode(BillingConstants.FeeType.PerPage));

			AssertEquals("Per Database (Multi Language/Multi Country(Region) customer)", list.GetDescriptionFromCode(BillingConstants.FeeType.DatabaseCountry));
			AssertEquals("Per Language (Multi Language/Multi Country(Region) customer)", list.GetDescriptionFromCode(BillingConstants.FeeType.DatabaseLanguage));
			AssertEquals("Per Language (Free if most users are local)", list.GetDescriptionFromCode(BillingConstants.FeeType.DatabaseLanguageZ));

			AssertEquals("Users Per Country Volume Break", list.GetDescriptionFromCode(BillingConstants.FeeType.UsersPerCountryVolumeBreak));

			AssertEquals("Minimum Fee Per Reference", list.GetDescriptionFromCode(BillingConstants.FeeType.MinimumFeePerReference));
			AssertEquals("Country Tier Pricing", list.GetDescriptionFromCode(BillingConstants.FeeType.CountryTier));
		}

		public void TestProcessingFeeList()
		{
			var list = EDIDataRegistry.Instance.InvoicingProcessingFeeLookup.Value;
			AssertEquals("Count", 3, list.Count);

			AssertEquals("None", list.GetDescriptionFromCode("NON"));
			AssertEquals("Direct Debit Discount", list.GetDescriptionFromCode("DDE"));
			AssertEquals("Manual Processing Fee", list.GetDescriptionFromCode("MPF"));
		}

		public void TestDiscountTypeList()
		{
			CodeDescriptionPairList list = BillingConstants.GetDiscountTypeList();
			AssertEquals("Count", 10, list.Count);

			AssertEquals("Volume", list.GetDescriptionFromCode(BillingConstants.DiscountType.Volume));
			AssertEquals("Incremental Volume", list.GetDescriptionFromCode(BillingConstants.DiscountType.IncrementalVolume));
			AssertEquals("Prepayment", list.GetDescriptionFromCode(BillingConstants.DiscountType.Prepayment));
			AssertEquals("Commitment", list.GetDescriptionFromCode(BillingConstants.DiscountType.Commitment));
			AssertEquals("Special", list.GetDescriptionFromCode(BillingConstants.DiscountType.Special));
			AssertEquals("Module Specific", list.GetDescriptionFromCode(BillingConstants.DiscountType.ModuleSpecific));
			AssertEquals("Capped", list.GetDescriptionFromCode(BillingConstants.DiscountType.Capped));
			AssertEquals("Minimum Fee", list.GetDescriptionFromCode(BillingConstants.DiscountType.MinimumFee));
			AssertEquals("Surcharge", list.GetDescriptionFromCode(BillingConstants.DiscountType.Surcharge));
			AssertEquals("WiseCloud", list.GetDescriptionFromCode(BillingConstants.DiscountType.WiseCloud));
		}

		public void TestBillingSystemsList()
		{
			AssertContainsExactElementsInAnyOrder(BillingConstants.GetBillingSystemList(), BillingConstants.BillingSystemList);

			CodeDescriptionPairList cptUsage = new CodeDescriptionPairList();
			cptUsage.AddPair("ACP", "ediACIReporting"); // Env.Licence.ACIReportingPerTransaction
			cptUsage.AddPair("AMS", "ediAMSReporting"); // Env.Licence.AMSReporting
			EDIDataRegistry.Instance.LicenceUsageBilledPerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cptUsage);

			CodeDescriptionPairList list = BillingConstants.GetBillingSystemList();
			AssertEquals("Count", 38 + cptUsage.Count, list.Count);

			AssertEquals("Product Fees", list.GetDescriptionFromCode(BillingConstants.BillingSystem.Fee));
			AssertEquals("On Demand", list.GetDescriptionFromCode(BillingConstants.BillingSystem.ODM));
			AssertEquals("eBACCa", list.GetDescriptionFromCode(BillingConstants.BillingSystem.eBACCA));
			AssertEquals("Importer Security Filing", list.GetDescriptionFromCode(BillingConstants.BillingSystem.ImporterSecurityFiling));
			AssertEquals("Fax", list.GetDescriptionFromCode(BillingConstants.BillingSystem.Fax));
			AssertEquals("Denied Party Screening", list.GetDescriptionFromCode(BillingConstants.BillingSystem.DeniedPartyScreening));
			AssertEquals("Quarantine ExDocs", list.GetDescriptionFromCode(BillingConstants.BillingSystem.ExDocs));
			AssertEquals("Distance Calculator - Generic", list.GetDescriptionFromCode(BillingConstants.BillingSystem.DistanceCalculatorGeneric));
			AssertEquals("Distance Calculator - PC Miler", list.GetDescriptionFromCode(BillingConstants.BillingSystem.DistanceCalculatorPcMiler));
			AssertEquals("Online Airline Schedules", list.GetDescriptionFromCode(BillingConstants.BillingSystem.S8Cargo));
			AssertEquals("Application Services Renewal", list.GetDescriptionFromCode(BillingConstants.BillingSystem.Maintenance));
			AssertEquals("WiseCloud Storage", list.GetDescriptionFromCode(BillingConstants.BillingSystem.HostingStorage));
			AssertEquals("WiseCloud Remote Devices", list.GetDescriptionFromCode(BillingConstants.BillingSystem.HostingRemoteDevices));
			AssertEquals("Read-only Access Excess", list.GetDescriptionFromCode(BillingConstants.BillingSystem.HostingDataAccess));
			AssertEquals("ABM Customs", list.GetDescriptionFromCode(BillingConstants.BillingSystem.ABMCustoms));
			AssertEquals("Japan AFR", list.GetDescriptionFromCode(BillingConstants.BillingSystem.JapanAFR));
			AssertEquals("US Customs", list.GetDescriptionFromCode(BillingConstants.BillingSystem.USCustoms));
			AssertEquals("Railinc", list.GetDescriptionFromCode(BillingConstants.BillingSystem.RailincByMessage));
			AssertEquals("Port Messaging", list.GetDescriptionFromCode(BillingConstants.BillingSystem.PortMessaging));
			AssertEquals("Container Automation", list.GetDescriptionFromCode(BillingConstants.BillingSystem.GlobalContainerTracking));
			AssertEquals("Ocean Tracing", list.GetDescriptionFromCode(BillingConstants.BillingSystem.OceanTracing));
			AssertEquals("Container Movement Tracking", list.GetDescriptionFromCode(BillingConstants.BillingSystem.OceanTracingLegacy));
			AssertEquals("GB Customs", list.GetDescriptionFromCode(BillingConstants.BillingSystem.GBCustoms));
			AssertEquals("Ocean Carrier Messaging", list.GetDescriptionFromCode(BillingConstants.BillingSystem.OceanCarrierMessaging));
			AssertEquals("ASYCUDA", list.GetDescriptionFromCode(BillingConstants.BillingSystem.ASYCUDA));

			AssertEquals("ediACIReporting", list.GetDescriptionFromCode("ACP"));
			AssertEquals("ediAMSReporting", list.GetDescriptionFromCode("AMS"));
			AssertEquals("ZA Customs", list.GetDescriptionFromCode(BillingConstants.BillingSystem.ZACustoms));
			AssertEquals("BorderWise", list.GetDescriptionFromCode(BillingConstants.BillingSystem.BorderWise));
			AssertEquals("Air Waybill Automation", list.GetDescriptionFromCode(BillingConstants.BillingSystem.FlightStats));
			AssertEquals("WiseCloud User", list.GetDescriptionFromCode(BillingConstants.BillingSystem.WiseCloudUser));
		}

		public void TestAllBillingSystems()
		{
			AssertContainsExactElementsInAnyOrder(BillingConstants.GetAllBillingSystems(), BillingConstants.AllBillingSystems);

			CodeDescriptionPairList list = BillingConstants.GetAllBillingSystems();
			CodeDescriptionPairList usageList = BillingConstants.GetBillingSystemList();
			AssertEquals("Count", usageList.Count + 1, list.Count);

			AssertEquals("All", list.GetDescriptionFromCode(BillingConstants.BillingSystem.All));
			AssertEquals("Maintenance", list.GetDescriptionFromCode(BillingConstants.BillingSystem.Maintenance));
		}

		public void TestLicenceEditionList()
		{
			CodeDescriptionPairList licenceEditionList = BillingConstants.GetLicenceEditionList();

			AssertEquals("Count", 4, licenceEditionList.Count);
			AssertEquals("Express", licenceEditionList.GetDescriptionFromCode(BillingConstants.LicenceEdition.Express));
			AssertEquals("Country", licenceEditionList.GetDescriptionFromCode(BillingConstants.LicenceEdition.Country));
			AssertEquals("Region", licenceEditionList.GetDescriptionFromCode(BillingConstants.LicenceEdition.Region));
			AssertEquals("Universal", licenceEditionList.GetDescriptionFromCode(BillingConstants.LicenceEdition.Universal));
		}

		public void TestNonProductionDatabase()
		{
			AssertEquals("#NC", BillingConstants.NonProductionDatabase.CoreModuleCode);
			AssertEquals("ediCore - Test License", BillingConstants.NonProductionDatabase.CoreModuleDescription);
		}

		public void TestHosting()
		{
			AssertEquals("#HO", BillingConstants.Hosting.Code);
			AssertEquals("Hosting", BillingConstants.Hosting.Description);
		}

		public void TestCoreModuleCode()
		{
			AssertEquals("COR", BillingConstants.CoreModuleCode);
		}

		public void TestRoundingDecimals()
		{
			AssertEquals(2, BillingConstants.RoundingDecimals);
			AssertEquals(3, BillingConstants.RoundingThreeDecimals);
			AssertEquals(2, BillingConstants.FormatOptions.GetOptionsByProduct(null, Factory).RoundingDecimals);
			AssertEquals(2, BillingConstants.FormatOptions.GetOptionsByProduct("", Factory).RoundingDecimals);
			AssertEquals(2, BillingConstants.FormatOptions.GetOptionsByProduct("ABC", Factory).RoundingDecimals);
			AssertEquals(2, BillingConstants.FormatOptions.GetOptionsByProduct("STL", Factory).RoundingDecimals);
			AssertEquals(2, BillingConstants.FormatOptions.GetOptionsByProduct("WTA", Factory).RoundingDecimals);

			var list = new CodeDescriptionPairList();
			list.AddPair("WTA", ".");
			EDIDataRegistry.Instance.ProductsWithThreeDecimalBillingSummary.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			AssertEquals(2, BillingConstants.FormatOptions.GetOptionsByProduct("WTA", Factory).RoundingDecimals);
			AssertEquals(3, BillingConstants.FormatOptions.GetOptionsByProduct("WTA", new BusinessObjectFactory()).RoundingDecimals);
		}

		public void TestAmountDecimalFormat()
		{
			AssertEquals("#,##0.00", BillingConstants.AmountDecimalFormat);
			AssertEquals("#,##0.000", BillingConstants.AmountThreeDecimalFormat);
			AssertEquals("#,##0.00", BillingConstants.FormatOptions.GetOptionsByProduct(null, Factory).DecimalFormat);
			AssertEquals("#,##0.00", BillingConstants.FormatOptions.GetOptionsByProduct("", Factory).DecimalFormat);
			AssertEquals("#,##0.00", BillingConstants.FormatOptions.GetOptionsByProduct("ABC", Factory).DecimalFormat);
			AssertEquals("#,##0.00", BillingConstants.FormatOptions.GetOptionsByProduct("STL", Factory).DecimalFormat);
			AssertEquals("#,##0.00", BillingConstants.FormatOptions.GetOptionsByProduct("WTA", Factory).DecimalFormat);

			var list = new CodeDescriptionPairList();
			list.AddPair("WTA", ".");
			EDIDataRegistry.Instance.ProductsWithThreeDecimalBillingSummary.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			AssertEquals("#,##0.00", BillingConstants.FormatOptions.GetOptionsByProduct("WTA", Factory).DecimalFormat);
			AssertEquals("#,##0.000", BillingConstants.FormatOptions.GetOptionsByProduct("WTA", new BusinessObjectFactory()).DecimalFormat);
		}

		public void TestIsTransactional()
		{
			foreach (ICodeDescription pair in BillingConstants.GetBillingSystemList())
			{
				bool expected = pair.Code != BillingConstants.BillingSystem.Fee
					&& pair.Code != BillingConstants.BillingSystem.ODM
					&& pair.Code != BillingConstants.BillingSystem.Maintenance
					&& pair.Code != BillingConstants.BillingSystem.HostingStorage
					&& pair.Code != BillingConstants.BillingSystem.HostingDataAccess
					&& pair.Code != BillingConstants.BillingSystem.HostingRemoteDevices;

				AssertEquals(pair.Code, expected, BillingConstants.IsTransactional(pair.Code));
			}
		}

		public void TestDiscountBreakUnits()
		{
			CodeDescriptionPairList licenceEditionList = BillingConstants.GetDiscountBreakUnitList();

			AssertEquals("Count", 2, licenceEditionList.Count);
			AssertEquals("Currency", licenceEditionList.GetDescriptionFromCode(BillingConstants.DiscountBreakUnit.Currency));
			AssertEquals("Licence Units", licenceEditionList.GetDescriptionFromCode(BillingConstants.DiscountBreakUnit.LicenceUnits));
		}

		public void TestFeeTypeIsOnDemand()
		{
			AssertEquals(false, BillingConstants.FeeType.IsOnDemand("XXX"));
			var codes = new List<string>(BillingConstants.GetFeeTypeList().GetAllCodes());

			foreach (string feeType in new string[] {
				BillingConstants.FeeType.Included,
				BillingConstants.FeeType.Licence,
				BillingConstants.FeeType.Module,
				BillingConstants.FeeType.NamedUser,
				BillingConstants.FeeType.OldWebModule,
				BillingConstants.FeeType.Database,
				BillingConstants.FeeType.CoreUsers,
				BillingConstants.FeeType.SelfHostedDatabaseUsers,
				BillingConstants.FeeType.DatabaseUsers,
				BillingConstants.FeeType.DatabaseCountry,
				BillingConstants.FeeType.DatabaseLanguage,
				BillingConstants.FeeType.DatabaseLanguageZ,
				BillingConstants.FeeType.Country,
				BillingConstants.FeeType.UsersPerCountryVolumeBreak,
			})
			{
				AssertEquals(feeType, true, BillingConstants.FeeType.IsOnDemand(feeType));
				codes.Remove(feeType);
			}

			foreach (string feeType in new string[] {
				BillingConstants.FeeType.Upgrade,
				BillingConstants.FeeType.Transactional,
				BillingConstants.FeeType.TransactionalOneVolumeBreak,
				BillingConstants.FeeType.PerVehicle,
				BillingConstants.FeeType.AgentEntity,
				BillingConstants.FeeType.PerGBPerMonth,
				BillingConstants.FeeType.Per10GBPerMonthMin1GB,
				BillingConstants.FeeType.PerMBPerMonthMin1GB,
				BillingConstants.FeeType.PerGBPerMonthMin1GB,
				BillingConstants.FeeType.PerMailBoxPerMonth,
				BillingConstants.FeeType.PerDevicePerMonth,
				BillingConstants.FeeType.PerMessageElement,
				BillingConstants.FeeType.PerPage,
				BillingConstants.FeeType.TransactionalModule,
				BillingConstants.FeeType.VolumeDatabaseFee,
				BillingConstants.FeeType.MinimumFee,
				BillingConstants.FeeType.MinimumFeePerReference,
				BillingConstants.FeeType.CountryTier,
			})
			{
				AssertEquals(feeType, false, BillingConstants.FeeType.IsOnDemand(feeType));
				codes.Remove(feeType);
			}

			AssertEquals("all fee types tested", 0, codes.Count);
		}

		public void TestFeeTypeIsMaintenance()
		{
			AssertEquals(false, BillingConstants.FeeType.IsMaintenance("XXX"));
			var codes = new List<string>(BillingConstants.GetFeeTypeList().GetAllCodes());

			foreach (string feeType in new string[] {
				BillingConstants.FeeType.Licence,
				BillingConstants.FeeType.Module,
				BillingConstants.FeeType.NamedUser,
				BillingConstants.FeeType.OldWebModule,
				BillingConstants.FeeType.Database })
			{
				AssertEquals(feeType, true, BillingConstants.FeeType.IsMaintenance(feeType));
				codes.Remove(feeType);
			}

			foreach (string feeType in new string[] {
				BillingConstants.FeeType.Included,
				BillingConstants.FeeType.Upgrade,
				BillingConstants.FeeType.Transactional,
				BillingConstants.FeeType.TransactionalOneVolumeBreak,
				BillingConstants.FeeType.PerVehicle,
				BillingConstants.FeeType.AgentEntity,
				BillingConstants.FeeType.Country,
				BillingConstants.FeeType.CountryTier,
				BillingConstants.FeeType.CoreUsers,
				BillingConstants.FeeType.SelfHostedDatabaseUsers,
				BillingConstants.FeeType.DatabaseUsers,
				BillingConstants.FeeType.PerGBPerMonth,
				BillingConstants.FeeType.Per10GBPerMonthMin1GB,
				BillingConstants.FeeType.PerMBPerMonthMin1GB,
				BillingConstants.FeeType.PerGBPerMonthMin1GB,
				BillingConstants.FeeType.PerMailBoxPerMonth,
				BillingConstants.FeeType.PerDevicePerMonth,
				BillingConstants.FeeType.PerMessageElement,
				BillingConstants.FeeType.PerPage,
				BillingConstants.FeeType.TransactionalModule,
				BillingConstants.FeeType.VolumeDatabaseFee,
				BillingConstants.FeeType.MinimumFee,
				BillingConstants.FeeType.MinimumFeePerReference,
				BillingConstants.FeeType.VolumeDatabaseFee,
				BillingConstants.FeeType.DatabaseCountry,
				BillingConstants.FeeType.DatabaseLanguage,
				BillingConstants.FeeType.DatabaseLanguageZ,
				BillingConstants.FeeType.UsersPerCountryVolumeBreak,
			})
			{
				AssertEquals(feeType, false, BillingConstants.FeeType.IsMaintenance(feeType));
				codes.Remove(feeType);
			}

			AssertEquals("all fee types tested", 0, codes.Count);
		}

		public void TestFeeTypeIsPerDatabase()
		{
			AssertEquals(false, BillingConstants.FeeType.IsPerDatabase("XXX"));
			var codes = new List<string>(BillingConstants.GetFeeTypeList().GetAllCodes());

			foreach (string feeType in new string[] {
				BillingConstants.FeeType.Database,
				BillingConstants.FeeType.SelfHostedDatabaseUsers,
				BillingConstants.FeeType.DatabaseUsers,
				BillingConstants.FeeType.VolumeDatabaseFee,
				BillingConstants.FeeType.DatabaseCountry,
				BillingConstants.FeeType.DatabaseLanguage,
				BillingConstants.FeeType.DatabaseLanguageZ,
			})
			{
				AssertEquals(feeType, true, BillingConstants.FeeType.IsPerDatabase(feeType));
				codes.Remove(feeType);
			}

			foreach (string feeType in new string[] {
				BillingConstants.FeeType.Licence,
				BillingConstants.FeeType.Module,
				BillingConstants.FeeType.NamedUser,
				BillingConstants.FeeType.OldWebModule,
				BillingConstants.FeeType.Included,
				BillingConstants.FeeType.Upgrade,
				BillingConstants.FeeType.Transactional,
				BillingConstants.FeeType.TransactionalOneVolumeBreak,
				BillingConstants.FeeType.PerVehicle,
				BillingConstants.FeeType.AgentEntity,
				BillingConstants.FeeType.Country,
				BillingConstants.FeeType.CountryTier,
				BillingConstants.FeeType.CoreUsers,
				BillingConstants.FeeType.PerGBPerMonth,
				BillingConstants.FeeType.Per10GBPerMonthMin1GB,
				BillingConstants.FeeType.PerMBPerMonthMin1GB,
				BillingConstants.FeeType.PerGBPerMonthMin1GB,
				BillingConstants.FeeType.PerMailBoxPerMonth,
				BillingConstants.FeeType.PerDevicePerMonth,
				BillingConstants.FeeType.PerMessageElement,
				BillingConstants.FeeType.PerPage,
				BillingConstants.FeeType.TransactionalModule,
				BillingConstants.FeeType.MinimumFee,
				BillingConstants.FeeType.MinimumFeePerReference,
				BillingConstants.FeeType.UsersPerCountryVolumeBreak,
			})
			{
				AssertEquals(feeType, false, BillingConstants.FeeType.IsPerDatabase(feeType));
				codes.Remove(feeType);
			}

			AssertEquals("all fee types tested", 0, codes.Count);
		}

		public void TestFeeTypeIsPerLicence()
		{
			AssertEquals(false, BillingConstants.FeeType.IsPerLicence("XXX"));
			var codes = new List<string>(BillingConstants.GetFeeTypeList().GetAllCodes());

			foreach (string feeType in new string[] {
				BillingConstants.FeeType.Licence })
			{
				AssertEquals(feeType, true, BillingConstants.FeeType.IsPerLicence(feeType));
				codes.Remove(feeType);
			}

			foreach (string feeType in new string[] {
				BillingConstants.FeeType.Database,
				BillingConstants.FeeType.Module,
				BillingConstants.FeeType.NamedUser,
				BillingConstants.FeeType.OldWebModule,
				BillingConstants.FeeType.Included,
				BillingConstants.FeeType.Upgrade,
				BillingConstants.FeeType.Transactional,
				BillingConstants.FeeType.TransactionalOneVolumeBreak,
				BillingConstants.FeeType.PerVehicle,
				BillingConstants.FeeType.AgentEntity,
				BillingConstants.FeeType.Country,
				BillingConstants.FeeType.CountryTier,
				BillingConstants.FeeType.CoreUsers,
				BillingConstants.FeeType.SelfHostedDatabaseUsers,
				BillingConstants.FeeType.DatabaseUsers,
				BillingConstants.FeeType.PerGBPerMonth,
				BillingConstants.FeeType.Per10GBPerMonthMin1GB,
				BillingConstants.FeeType.PerMBPerMonthMin1GB,
				BillingConstants.FeeType.PerGBPerMonthMin1GB,
				BillingConstants.FeeType.PerMailBoxPerMonth,
				BillingConstants.FeeType.PerDevicePerMonth,
				BillingConstants.FeeType.PerMessageElement,
				BillingConstants.FeeType.PerPage,
				BillingConstants.FeeType.TransactionalModule,
				BillingConstants.FeeType.VolumeDatabaseFee,
				BillingConstants.FeeType.MinimumFee,
				BillingConstants.FeeType.MinimumFeePerReference,
				BillingConstants.FeeType.DatabaseCountry,
				BillingConstants.FeeType.DatabaseLanguage,
				BillingConstants.FeeType.DatabaseLanguageZ,
				BillingConstants.FeeType.UsersPerCountryVolumeBreak,
			})
			{
				AssertEquals(feeType, false, BillingConstants.FeeType.IsPerLicence(feeType));
				codes.Remove(feeType);
			}

			AssertEquals("all fee types tested", 0, codes.Count);
		}

		public void TestGetBorderWiseModuleList()
		{
			var list = BillingConstants.BorderWise.GetBorderWiseModuleList();
			foreach (var code in new string[] {
					BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode,
					BillingConstants.BorderWise.AUSingleWindowPartnerCW1PriceCode,
					BillingConstants.BorderWise.AUSingleWindowPartnerOnlyPriceCode,
					BillingConstants.BorderWise.AUSingleWindowCW1OnlyPriceCode,
					BillingConstants.BorderWise.AUProPackPriceCode,
					BillingConstants.BorderWise.NZSingleWindowStandalonePriceCode,
					BillingConstants.BorderWise.NZSingleWindowPartnerCW1PriceCode,
					BillingConstants.BorderWise.NZSingleWindowPartnerOnlyPriceCode,
					BillingConstants.BorderWise.NZSingleWindowCW1OnlyPriceCode,
					BillingConstants.BorderWise.NZProPackPriceCode,
					BillingConstants.BorderWise.GlobalPartnerOrCW1PriceCode,
					BillingConstants.BorderWise.GlobalStandalonePriceCode,
					BillingConstants.BorderWise.GlobalProPackPriceCode
			})
			{
				Assert(code, list.ContainsCode(code));
			}
			Assert(list.ContainsCode(BillingConstants.BorderWise.StudentUserPriceCode));
			Assert(list.ContainsCode(BillingConstants.BorderWise.FreeTrialPriceCode));
			AssertEquals(15, list.Count);
		}

		public void TestPriceHeaderType()
		{
			AssertEquals(false, PriceHeaderType.IsUsedInBillingStl("PL0"));
			AssertEquals(false, PriceHeaderType.IsGlobal("PL0"));
			AssertEquals(false, PriceHeaderType.GetPriceHeaderTypeList().ContainsCode("PL0"));
			UsageBillingSettingsTest.SetupValidTestRegistry();
			AssertEquals(true, PriceHeaderType.IsUsedInBillingStl("PL0"));
			AssertEquals(false, PriceHeaderType.IsGlobal("PL0"));
			AssertEquals(true, PriceHeaderType.GetPriceHeaderTypeList().ContainsCode("PL0"));

			var globalPriceLists = new CodeDescriptionPairList();
			globalPriceLists.AddPair("PL0", "PL0");
			EDIDataRegistry.Instance.BillingStlGlobalPriceLists.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalPriceLists);
			AssertEquals(true, PriceHeaderType.IsGlobal("PL0"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			BillingConstants.ResetBillingSystemListForTest();
		}
	}
}
