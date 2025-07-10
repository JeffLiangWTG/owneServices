using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Shipment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Shipment
{
	/// <summary>
	/// Tests can also be found in Rating solution, where it's easier to create test data.
	/// </summary>
	[TestedType(typeof(XT_Report_FreightRatesReport))]
	class XT_Report_FreightRatesReportTest : DbCreateScriptTest
	{
		public void TestSalesRepFilter()
		{
			var salesRep1Code = "CAR";
			var salesRep2Code = "BUS";
			var companyPK = Guid.NewGuid();

			AddGlbCompany(companyPK);

			AddFreightChargeCode("OAQF", companyPK, chargeCodePK);
			AddFreightChargeCode("OAQF", companyPK: Guid.Empty, chargeCodePK: globalChargeCodePK);

			var orgHeader1PK = AddOrgHeader("ORG1", "1st Organisation");
			var orgHeader2PK = AddOrgHeader("ORG2", "2nd Organisation");
			var orgHeader3PK = AddOrgHeader("ORG3", "3rd Organisation");

			AddSalesRep(salesRep1Code, orgHeader1PK, companyPK);
			AddSalesRep(salesRep2Code, orgHeader2PK, companyPK);
			AddSalesRep(salesRep2Code, orgHeader3PK, companyPK);

			var categoryAndModes = new[] { ("FCL", "SEA"), ("LCL", "LCL"), ("AIR", "LSE") };

			AddRates(orgHeader1PK, companyPK, publisherPK: companyPK, categoryAndModes: categoryAndModes, chargeCodePK: chargeCodePK, origin: "AUSYD", destination: "USLAX");
			AddRates(orgHeader2PK, companyPK, publisherPK: companyPK, categoryAndModes: categoryAndModes, chargeCodePK: chargeCodePK, origin: "AUBNE", destination: "USCHI");
			AddRates(orgHeader3PK, companyPK, publisherPK: companyPK, categoryAndModes: categoryAndModes, chargeCodePK: chargeCodePK, origin: "AUPER", destination: "USNYC");

			AddRates(orgHeader1PK, companyPK: Guid.Empty, publisherPK: companyPK, categoryAndModes: categoryAndModes, chargeCodePK: globalChargeCodePK, origin: "NZAKL", destination: "CNBJS");
			AddRates(orgHeader2PK, companyPK: Guid.Empty, publisherPK: companyPK, categoryAndModes: categoryAndModes, chargeCodePK: globalChargeCodePK, origin: "NZQQN", destination: "CHSHA");
			AddRates(orgHeader3PK, companyPK: Guid.Empty, publisherPK: companyPK, categoryAndModes: categoryAndModes, chargeCodePK: globalChargeCodePK, origin: "NZPON", destination: "CNNJI");

			AssertSalesRepFilter
			(
				companyPK,
				salesRepCode: "",
				modes: new[] { "FCL" },
				includeGlobal: 'N',
				expected: new Dictionary<string, string[]>()
				{
					{ "FCL", Array.Empty<string>() },
				},
				message: "Default freight charge code is FRT, so report will shown nothing"
			);

			UpdateChargeCode("FRT", chargeCodePK);

			AssertSalesRepFilter
			(
				companyPK,
				salesRepCode: "",
				modes: new[] { "FCL" },
				includeGlobal: 'N',
				expected: new Dictionary<string, string[]>()
				{
					{ "FCL", new[] { "AUSYD>USLAX: Local", "AUBNE>USCHI: Local", "AUPER>USNYC: Local" } },
				},
				message: "All FCL client rates are shown"
			);

			AssertSalesRepFilter
			(
				companyPK,
				salesRepCode: salesRep1Code,
				modes: new[] { "FCL" },
				includeGlobal: 'N',
				expected: new Dictionary<string, string[]>()
				{
					{ "FCL", new[] { "AUSYD>USLAX: Local" } },
				},
				message: "Client rate linked to Sales Reps 1' organisation is shown"
			);

			AssertSalesRepFilter
			(
				companyPK,
				salesRepCode: salesRep2Code,
				modes: new[] { "FCL" },
				includeGlobal: 'N',
				expected: new Dictionary<string, string[]>()
				{
					{ "FCL", new[] { "AUBNE>USCHI: Local", "AUPER>USNYC: Local" } },
				},
				message: "Client rates linked to Sales Reps 2' organisations are shown"
			);

			AssertSalesRepFilter
			(
				companyPK,
				salesRep2Code,
				modes: new[] { "FCL", "LCL", "AIR" },
				includeGlobal: 'Y',
				expected: new Dictionary<string, string[]>()
				{
					{ "FCL", new[] { "AUBNE>USCHI: Local", "AUPER>USNYC: Local" } },
					{ "LCL", new[] { "AUBNE>USCHI: Local", "AUPER>USNYC: Local" } } ,
					{ "AIR", new[] { "AUBNE>USCHI: Local", "AUPER>USNYC: Local" } }
				},
				message: "Global charge code was not updated to FRT, so report will only show local for sales rep"
			);

			AssertSalesRepFilter
			(
				companyPK,
				salesRep2Code,
				modes: new[] { "FCL", "LCL", "AIR" },
				includeGlobal: 'N',
				expected: new Dictionary<string, string[]>()
				{
					{ "FCL", new[] { "AUBNE>USCHI: Local", "AUPER>USNYC: Local" } },
					{ "LCL", new[] { "AUBNE>USCHI: Local", "AUPER>USNYC: Local" } } ,
					{ "AIR", new[] { "AUBNE>USCHI: Local", "AUPER>USNYC: Local" } }
				},
				message: "Global charge code was not updated to FRT, so report will only show local for sales rep"
			);

			UpdateChargeCode("FRT", globalChargeCodePK);

			AssertSalesRepFilter
			(
				companyPK,
				salesRep2Code,
				modes: new[] { "FCL", "LCL", "AIR" },
				includeGlobal: 'Y',
				expected: new Dictionary<string, string[]>()
				{
					{ "FCL", new[] { "AUBNE>USCHI: Local", "AUPER>USNYC: Local", "NZQQN>CHSHA: Global", "NZPON>CNNJI: Global" } },
					{ "LCL", new[] { "AUBNE>USCHI: Local", "AUPER>USNYC: Local", "NZQQN>CHSHA: Global", "NZPON>CNNJI: Global" } } ,
					{ "AIR", new[] { "AUBNE>USCHI: Local", "AUPER>USNYC: Local", "NZQQN>CHSHA: Global", "NZPON>CNNJI: Global" } }
				},
				message: "Global charge code was updated to FRT, so report shows all FRT results for sales rep"
			);

			AssertSalesRepFilter
			(
				companyPK,
				salesRep2Code,
				modes: new[] { "FCL", "LCL", "AIR" },
				includeGlobal: 'N',
				expected: new Dictionary<string, string[]>()
				{
					{ "FCL", new[] { "AUBNE>USCHI: Local", "AUPER>USNYC: Local" } },
					{ "LCL", new[] { "AUBNE>USCHI: Local", "AUPER>USNYC: Local" } } ,
					{ "AIR", new[] { "AUBNE>USCHI: Local", "AUPER>USNYC: Local" } }
				},
				message: "Global charge code was updated to FRT, so report shows all FRT results for sales rep"
			);
		}

		void AddGlbCompany(Guid companyPK, string countryCode = "AU", string currencyCode = "AUD")
		{
			using (var command = TestConnection.Command("INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@GC_PK, @GC_Code, 'AU company', @GC_RN_NKCountryCode, @GC_RX_NKLocalCurrency)"))
			{
				command.AddParameterBasedOnDbColumn("@GC_PK", companyPK, GlbCompanySchema.PK);
				command.AddParameterBasedOnDbColumn("@GC_Code", "DDD", GlbCompanySchema.GC_Code);
				command.AddParameterBasedOnDbColumn("@GC_RN_NKCountryCode", countryCode, GlbCompanySchema.GC_RN_NKCountryCode);
				command.AddParameterBasedOnDbColumn("@GC_RX_NKLocalCurrency", currencyCode, GlbCompanySchema.GC_RX_NKLocalCurrency);
				command.ExecuteNonQuery();
			}
		}

		void AssertSalesRepFilter(Guid companyPK, string salesRepCode, string[] modes, char includeGlobal, Dictionary<string, string[]> expected, string message)
		{
			CombineAssertions(message, () =>
			{
				foreach (var mode in modes)
				{
					var result = new DataTable();
					result.Load(FilteredSalesRepReportCommand(companyPK, salesRepCode, mode, includeGlobal).ExecuteReader());
					var rowList = result.Rows.Cast<DataRow>();
					AssertContainsExactElementsInAnyOrder
					(
						message,
						expected: expected[mode],
						actual: rowList.Select(row => $"{row["Origin"]}>{row["Destination"]}: {row["Published"]}")
					);
				}
			});
		}

		void AddRates(Guid orgHeaderPK, Guid companyPK, (string category, string mode)[] categoryAndModes, Guid publisherPK, Guid chargeCodePK, string origin, string destination)
		{
			var ratingHeaderPK = Guid.NewGuid();
			AddRatingHeader(ratingHeaderPK, orgHeaderPK, companyPK);
			foreach (var (category, mode) in categoryAndModes)
			{
				AddRateEntry(ratingHeaderPK, chargeCodePK, publisherPK, category, mode, origin, destination);
			}
		}

		void AddFreightChargeCode(string code, Guid companyPK, Guid chargeCodePK)
		{
			var hasCompanyPK = companyPK != Guid.Empty;

			using (var command = TestConnection.Command($@"
				INSERT INTO 
					dbo.AccChargeCode (
						AC_PK,
						AC_CODE,
						AC_CHARGEGROUP
						{(hasCompanyPK ? ", AC_GC" : "")})
				VALUES (
						@AC_PK,
						@AC_CODE,
						@AC_CHARGEGROUP
						{(hasCompanyPK ? ", @AC_GC" : "")})"))
			{
				command.AddParameterBasedOnDbColumn("@AC_PK", chargeCodePK, AccChargeCodeSchema.PK);
				command.AddParameterBasedOnDbColumn("@AC_CODE", code, AccChargeCodeSchema.AC_Code);
				command.AddParameterBasedOnDbColumn("@AC_CHARGEGROUP", "FRT", AccChargeCodeSchema.AC_Code);
				command.AddParameterBasedOnDbColumn("@AC_GC", companyPK, AccChargeCodeSchema.AC_GC);
				command.ExecuteNonQuery();
			}
		}

		Guid AddOrgHeader(string code, string fullname)
		{
			var orgHeaderPK = Guid.NewGuid();

			using (var command = TestConnection.Command(@"
				INSERT INTO 
					dbo.OrgHeader (
						OH_PK,
						OH_Code,
						OH_FullName)
				VALUES (
						@OH_PK,
						@OH_Code,
						@OH_FullName)"))
			{
				command.AddParameterBasedOnDbColumn("@OH_PK", orgHeaderPK, OrgHeaderSchema.PK);
				command.AddParameterBasedOnDbColumn("@OH_Code", code, OrgHeaderSchema.OH_Code);
				command.AddParameterBasedOnDbColumn("@OH_FullName", fullname, OrgHeaderSchema.OH_FullName);
				command.ExecuteNonQuery();
			}

			return orgHeaderPK;
		}

		void AddRatingHeader(Guid pk, Guid orgHeaderPK, Guid companyPK)
		{
			var hasCompanyPK = companyPK != Guid.Empty;

			using (var command = TestConnection.Command($@"
				INSERT INTO 
					dbo.RatingHeader (
						TH_PK,
						TH_OH,
						{(hasCompanyPK ? "TH_GC," : string.Empty)}
						TH_RateType,
						TH_SystemLastEditTimeUtc,
						TH_SystemLastEditUser,
						TH_SystemCreateTimeUtc,
						TH_SystemCreateUser)
				VALUES (
						@TH_PK,
						@TH_OH,
						{(hasCompanyPK ? "@TH_GC," : string.Empty)}
						@TH_RateType,
						GetUtcDate(),
						'~BP',
						GetUtcDate(),
						'~BP')"))
			{
				command.AddParameterBasedOnDbColumn("@TH_PK", pk, RatingHeaderSchema.PK);
				command.AddParameterBasedOnDbColumn("@TH_OH", orgHeaderPK, RatingHeaderSchema.TH_OH);
				if (hasCompanyPK)
				{
					command.AddParameterBasedOnDbColumn("@TH_GC", companyPK, RatingHeaderSchema.TH_GC);
				}
				command.AddParameterBasedOnDbColumn("@TH_RateType", "SAL", RatingHeaderSchema.TH_RateType);
				command.ExecuteNonQuery();
			}
		}

		void AddRateEntry(Guid ratingHeaderPK, Guid accChargeCodePK, Guid publisherPK, string rateCategory, string mode, string origin, string destination)
		{
			var rateEntryPK = Guid.NewGuid();
			using (var command = TestConnection.Command(@"
				INSERT INTO 
					dbo.RateEntry (
						TI_PK,
						TI_TH,
						TI_GC_Publisher,
						TI_Mode,
						TI_RateCategory,
						TI_OriginLRC,
						TI_DestinationLRC,
						TI_RateStartDate, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser, TI_SystemCreateTimeUtc, TI_SystemCreateUser)
				VALUES (
						@TI_PK,
						@TI_TH,
						@TI_GC_Publisher,
						@TI_Mode,
						@TI_RateCategory,
						@TI_OriginLRC,
						@TI_DestinationLRC,
						@TI_RateStartDate, GetUtcDate(), '~BP', GetUtcDate(), '~BP')"))
			{
				command.AddParameterBasedOnDbColumn("@TI_PK", rateEntryPK, RateEntrySchema.PK);
				command.AddParameterBasedOnDbColumn("@TI_TH", ratingHeaderPK, RateEntrySchema.TI_TH);
				command.AddParameterBasedOnDbColumn("@TI_GC_Publisher", publisherPK, RateEntrySchema.TI_GC_Publisher);
				command.AddParameterBasedOnDbColumn("@TI_Mode", mode, RateEntrySchema.TI_Mode);
				command.AddParameterBasedOnDbColumn("@TI_RateCategory", rateCategory, RateEntrySchema.TI_RateCategory);
				command.AddParameterBasedOnDbColumn("@TI_OriginLRC", origin, RateEntrySchema.TI_OriginLRC);
				command.AddParameterBasedOnDbColumn("@TI_DestinationLRC", destination, RateEntrySchema.TI_DestinationLRC);
				command.AddParameterBasedOnDbColumn("@TI_RateStartDate", DateTime.Today.AddMonths(-1), RateEntrySchema.TI_RateStartDate);
				command.ExecuteNonQuery();
			}

			var linePK = Guid.NewGuid();
			using (var command = TestConnection.Command(@"
				INSERT INTO
					dbo.RateLines (
						TL_PK,
						TL_TI,
						TL_AC,
						TL_RX_NKCurrency,
						TL_RateCalculator, TL_SystemLastEditTimeUtc, TL_SystemLastEditUser, TL_SystemCreateTimeUtc, TL_SystemCreateUser)
				VALUES (
						@TL_PK,
						@TL_TI,
						@TL_AC,
						'USD',
						@TL_RateCalculator, GetUtcDate(), '~BP', GetUtcDate(), '~BP')"))
			{
				command.AddParameterBasedOnDbColumn("@TL_PK", linePK, RateLinesSchema.PK);
				command.AddParameterBasedOnDbColumn("@TL_TI", rateEntryPK, RateLinesSchema.TL_TI);
				command.AddParameterBasedOnDbColumn("@TL_AC", accChargeCodePK, RateLinesSchema.TL_AC);
				command.AddParameterBasedOnDbColumn("@TL_RateCalculator", "FLT", RateLinesSchema.TL_RateCalculator);
				command.ExecuteNonQuery();
			}

			using (var command = TestConnection.Command(@"
				INSERT INTO 
					dbo.RateLineItems (
						TM_PK,
						TM_TL,
						TM_Type,
						TM_Value, TM_SystemLastEditTimeUtc, TM_SystemLastEditUser, TM_SystemCreateTimeUtc, TM_SystemCreateUser)
				VALUES (
						newid(),
						@TM_TL,
						@TM_Type,
						@TM_Value, GetUtcDate(), '~BP', GetUtcDate(), '~BP')"))
			{
				command.AddParameterBasedOnDbColumn("@TM_TL", linePK, RateLineItemsSchema.TM_TL);
				command.AddParameterBasedOnDbColumn("@TM_Type", "UNT", RateLineItemsSchema.TM_Type);
				command.AddParameterBasedOnDbColumn("@TM_Value", 20, RateLineItemsSchema.TM_Value);
				command.ExecuteNonQuery();
			}
		}

		void AddSalesRep(string code, Guid orgPk, Guid companyPK)
		{
			using (var command = TestConnection.Command(@"
				INSERT INTO
					dbo.OrgStaffAssignments (
						O8_PK,
						O8_Role,
						O8_Department,
						O8_OH,
						O8_GC,
						O8_GS_NKPersonResponsible)
				VALUES (
						newid(),
						@O8_Role,
						@O8_Department,
						@O8_OH,
						@O8_GC,
						@O8_GS_NKPersonResponsible)"))
			{
				command.AddParameterBasedOnDbColumn("@O8_Role", "SAL", OrgStaffAssignmentsSchema.O8_Role);
				command.AddParameterBasedOnDbColumn("@O8_Department", "ALL", OrgStaffAssignmentsSchema.O8_Department);
				command.AddParameterBasedOnDbColumn("@O8_OH", orgPk, OrgStaffAssignmentsSchema.O8_OH);
				command.AddParameterBasedOnDbColumn("@O8_GC", companyPK, OrgStaffAssignmentsSchema.O8_GC);
				command.AddParameterBasedOnDbColumn("@O8_GS_NKPersonResponsible", code, OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible);
				command.ExecuteNonQuery();
			}
		}

		void UpdateChargeCode(string code, Guid chargeCodePk)
		{
			using (var command = TestConnection.Command(@"
				UPDATE
					dbo.AccChargeCode
				SET 
					AC_CODE = @AC_CODE,
					AC_SystemLastEditTimeUtc = GETUTCDATE(),
					AC_SystemLastEditUser = 'TST'
				WHERE 
					AC_PK = @AC_PK"))
			{
				command.AddParameterBasedOnDbColumn("@AC_PK", chargeCodePk, AccChargeCodeSchema.PK);
				command.AddParameterBasedOnDbColumn("@AC_CODE", code, AccChargeCodeSchema.AC_Code);
				command.ExecuteNonQuery();
			}
		}

		DbCommand FilteredSalesRepReportCommand(Guid companyPK, string salesRepCode, string mode, char includeGlobal = 'N')
		{
			var command = TestConnection.Command(@"
				EXEC XT_Report_FreightRatesReport
					@CompanyPK,
					@RateType,
					@Mode,
					@OriginPK,
					@DestinationPK,
					@ServiceLevelPK,
					@CommodityCodePK,
					@OriginCountryPK,
					@DestinationCountryPK,
					@SupplierPK,
					@CarrierPK,
					@SalesRepCode,
					@IncludeGlobal");
			command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);
			command.AddParameter("@RateType", SqlDbType.Char, "SAL");
			command.AddParameter("@Mode", SqlDbType.Char, mode);
			command.AddParameter("@OriginPK", SqlDbType.UniqueIdentifier, DBNull.Value);
			command.AddParameter("@DestinationPK", SqlDbType.UniqueIdentifier, DBNull.Value);
			command.AddParameter("@ServiceLevelPK", SqlDbType.UniqueIdentifier, DBNull.Value);
			command.AddParameter("@CommodityCodePK", SqlDbType.UniqueIdentifier, DBNull.Value);
			command.AddParameter("@OriginCountryPK", SqlDbType.UniqueIdentifier, DBNull.Value);
			command.AddParameter("@DestinationCountryPK", SqlDbType.UniqueIdentifier, DBNull.Value);
			command.AddParameter("@SupplierPK", SqlDbType.UniqueIdentifier, DBNull.Value);
			command.AddParameter("@CarrierPK", SqlDbType.UniqueIdentifier, DBNull.Value);
			command.AddParameter("@SalesRepCode", SqlDbType.VarChar, string.IsNullOrEmpty(salesRepCode) ? DBNull.Value : salesRepCode);
			command.AddParameter("@IncludeGlobal", SqlDbType.Char, includeGlobal);
			return command;
		}

		readonly Guid chargeCodePK = Guid.NewGuid();
		readonly Guid globalChargeCodePK = Guid.NewGuid();
	}
}

