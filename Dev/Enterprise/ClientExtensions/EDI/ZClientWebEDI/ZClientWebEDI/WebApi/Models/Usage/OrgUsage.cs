using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class OrgUsage
	{
		public OrgUsage(OrgHeader org, ZDateTime periodStart, string product)
		{
			this.org = org;
			this.periodStart = periodStart;
			this.product = product;
			LoadUsages();
		}

		readonly OrgHeader org;
		readonly ZDateTime periodStart;
		readonly string product;

		public ZString PeriodStart { get { return periodStart.ToString("MMM yyyy", CultureInfo.InvariantCulture); } }
		public ZString PeriodStartYYYYMM { get { return periodStart.ToString("yyyyMM", CultureInfo.InvariantCulture); } }
		public Collection<StlUsage> StlUsages { get; private set; }
		public Collection<OdplUsage> OdplUsages { get; private set; }
		public Collection<OdplUsage> BorderWiseUsages { get; private set; }

		static readonly string InvalidFileNameChars = new string(Path.GetInvalidFileNameChars()) + ",";

		void LoadUsages()
		{
			StlUsages = new Collection<StlUsage>();
			OdplUsages = new Collection<OdplUsage>();
			BorderWiseUsages = new Collection<OdplUsage>();

			if (org != null && !periodStart.IsEmpty && periodStart.IsValid)
			{
				LoadStlUsages();
				if (string.IsNullOrEmpty(product))
				{
					LoadOdplAndBorderWiseUsages();
				}
			}
		}

		#region STL

		void LoadStlUsages()
		{
			var lineCollection = LoadStlUsageLines();
			PopulateStlUsageData(lineCollection);
		}

		Collection<UsageLine> LoadStlUsageLines()
		{
			var lineCollection = new Collection<UsageLine>();
			string query = string.IsNullOrEmpty(product) ?
				"SELECT LD_PK, LD_ServerCode, LE_EnterpriseCode, LCC_PK, LCC_Code, L7_PK, L7_Description, L7_Order, U1_Code, U1_UnitCount FROM " + Enterprise.Client.EDI.BillingUsageSchema.GetStlUsageSummary + "(@OrgPk, @PeriodStart);"
			  : "SELECT LD_PK, LD_ServerCode, LE_EnterpriseCode, LCC_PK, LCC_Code, L7_PK, L7_Description, L7_Order, U1_Code, U1_UnitCount FROM " + Enterprise.Client.EDI.BillingUsageSchema.GetStlGenericUsageSummary + "(@OrgPk, @PeriodStart, @Product);";

			using (var command = Db.Connection.Command(query))
			{
				command.AddParameter("@OrgPk", SqlDbType.UniqueIdentifier, org.PK.ToGuid());
				command.AddParameter("@PeriodStart", SqlDbType.DateTime, periodStart.ToDateTime());
				if (!string.IsNullOrEmpty(product))
				{
					command.AddParameter("@Product", SqlDbType.VarChar, product);
				}

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var line = new UsageLine();

						line.DatabasePK = (Guid)reader[LicenceDatabaseSchema.Constants.PK];
						line.Product = Client.EDI.Licencing.Business.ProductTypes.Codes.CargoWiseOne;
						line.DatabaseServerCode = (string)reader[LicenceDatabaseSchema.Constants.LD_ServerCode];
						line.EnterpriseCode = (string)reader[LicenceEnterpriseSchema.Constants.LE_EnterpriseCode];
						line.PriceItemPK = (Guid)reader[ClientLicencePriceItemSchema.Constants.PK];
						line.PriceItemDescription = (string)reader[ClientLicencePriceItemSchema.Constants.L7_Description];
						line.PriceItemOrder = (short)reader[ClientLicencePriceItemSchema.Constants.L7_Order];
						line.ClientCompanyPK = !reader.IsDBNull(reader.GetOrdinal(ClientCompanySchema.Constants.PK)) ? (Guid)reader[ClientCompanySchema.Constants.PK] : Guid.Empty;
						line.CompanyCode = (string)reader[ClientCompanySchema.Constants.LCC_Code];
						line.SystemCode = (string)reader[ClientChargeableUsageSchema.Constants.U1_Code];
						line.UnitCount = (int)reader[ClientChargeableUsageSchema.Constants.U1_UnitCount];
						lineCollection.Add(line);
					}
				}
			}
			return lineCollection;
		}

		void PopulateStlUsageData(Collection<UsageLine> lineCollection)
		{
			foreach (var serverGroup in lineCollection.GroupBy(x => new { x.DatabasePK, x.DatabaseServerCode, x.EnterpriseCode }).OrderBy(x => x.Key.DatabaseServerCode))
			{
				var stlUsage = new StlUsage(serverGroup.Key.DatabaseServerCode, serverGroup.Key.DatabasePK);
				StlUsages.Add(stlUsage);

				foreach (var priceItemGroup in serverGroup.GroupBy(y => new { y.PriceItemPK, y.PriceItemDescription, y.PriceItemOrder }))
				{
					var usageData = stlUsage.AddUsageData(priceItemGroup.Key.PriceItemPK, priceItemGroup.Key.PriceItemDescription, priceItemGroup.Key.PriceItemOrder);
					var csvFileNameForAllCompanies = GetStlReportFileName(periodStart, priceItemGroup.Key.PriceItemDescription.Trim(), serverGroup.Key.EnterpriseCode, serverGroup.Key.DatabaseServerCode, "");
					var systemCodesForAllCompanies = string.Join(",", priceItemGroup.Select(line => line.SystemCode).Distinct().ToArray());
					var csvLinkUrlForAllCompanies = GetStlReportUrl(periodStart, serverGroup.Key.DatabasePK, priceItemGroup.Key.PriceItemPK, ZGuid.Empty, csvFileNameForAllCompanies, BillingConstants.FileExtensions.Csv, systemCodesForAllCompanies);
					usageData.AddReportLink(serverGroup.Key.DatabaseServerCode, "", "", csvLinkUrlForAllCompanies);

					foreach (var companyGroup in priceItemGroup.GroupBy(z => new { z.ClientCompanyPK, z.LicenceCompanyPK, z.CompanyCode }))
					{
						var systemCodes = string.Join(",", companyGroup.Select(line => line.SystemCode).ToArray());
						var fileName = GetStlReportFileName(periodStart, priceItemGroup.Key.PriceItemDescription.Trim(), serverGroup.Key.EnterpriseCode, serverGroup.Key.DatabaseServerCode, companyGroup.Key.CompanyCode);
						var pdfLinkUrl = IsReportTooLargeForPdfFormat(companyGroup) ? "#" : GetStlReportUrl(periodStart, serverGroup.Key.DatabasePK, priceItemGroup.Key.PriceItemPK, companyGroup.Key.ClientCompanyPK, fileName, BillingConstants.FileExtensions.Pdf, systemCodes);
						var csvLinkUrl = !companyGroup.Key.CompanyCode.IsEmpty ? GetStlReportUrl(periodStart, serverGroup.Key.DatabasePK, priceItemGroup.Key.PriceItemPK, companyGroup.Key.ClientCompanyPK, fileName, BillingConstants.FileExtensions.Csv, systemCodes) : string.Empty;
						usageData.AddReportLink(serverGroup.Key.DatabaseServerCode, companyGroup.Key.CompanyCode, pdfLinkUrl, csvLinkUrl);
					}
				}
			}
		}

		static string GetStlReportFileName(ZDateTime periodStart, ZString priceItemDescription, ZString enterpriseCode, ZString databaseServerCode, ZString companyCode)
		{
			var fileName = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}_{3}",
				periodStart.ToString("yyyyMM", CultureInfo.InvariantCulture),
				databaseServerCode.IsEmpty ? "N/A" : enterpriseCode + "-" + databaseServerCode,
				companyCode.IsEmpty ? "ALL" : companyCode.ToString(),
				TrimDescription(priceItemDescription));
			var regex = new Regex(string.Format(CultureInfo.InvariantCulture, "[{0}]", Regex.Escape(InvalidFileNameChars)));
			fileName = regex.Replace(fileName, "");
			return fileName;
		}

		static string GetStlReportUrl(ZDateTime periodStart, ZGuid databasePk, ZGuid priceItemPk, ZGuid companyPk, ZString fileName, ZString fileType, ZString systemCodes)
		{
			var queryString = new SecureQueryString
			{
				[StlUsageReportRequestHelper.Constants.PeriodStart] = periodStart.ToDateTime().ToString(ZDateTime.ISO8601ShortDateFormat, CultureInfo.InvariantCulture),
				[StlUsageReportRequestHelper.Constants.DatabasePk] = databasePk.ToString(),
				[StlUsageReportRequestHelper.Constants.PriceItemPk] = priceItemPk.ToString(),
				[StlUsageReportRequestHelper.Constants.CompanyPk] = companyPk.ToString(),
				[StlUsageReportRequestHelper.Constants.FileName] = fileName,
				[StlUsageReportRequestHelper.Constants.FileType] = fileType,
				[StlUsageReportRequestHelper.Constants.SystemCodes] = systemCodes
			};

			var urlTemplate = fileType == BillingConstants.FileExtensions.Csv ? "StlUsageReportStreamRequestHandler.axd?{0}={1}" : "StlUsageReportRequestHandler.axd?{0}={1}";
			return string.Format(CultureInfo.InvariantCulture, urlTemplate, SecureQueryString.QueryStringKey, WebUtility.UrlEncode(queryString.ToString()));
		}

		#endregion

		#region ODPL

		void LoadOdplAndBorderWiseUsages()
		{
			var systemList = BillingConstants.GetBillingSystemList();
			var lineCollection = LoadOdplAndBorderWiseUsageLines(systemList);
			PopulateOdplAndBorderWiseUsageData(systemList, lineCollection);
		}

		Collection<UsageLine> LoadOdplAndBorderWiseUsageLines(CodeDescriptionPairList systemList)
		{
			var lineCollection = new Collection<UsageLine>();

			// Use literals rather than parameters or ediProd will choose bad query plans (e.g., 100x slower)
			string query = "SELECT OH_PK, OH_Code, OH_FullName, U1_Code, ServerCode, CompanyCode, LCC_PK, LC_PK, LD_PK, U1_UnitCount, L7_PK, L7_Description FROM " + Enterprise.Client.EDI.BillingUsageSchema.GetSystemUsage +
				"('" + org.PK + "', '" + periodStart.SqlFormat + "');";

			using (var command = Db.Connection.Command(query))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var line = new UsageLine();

						line.OrgPK = (Guid)reader[OrgHeaderSchema.Constants.PK];
						line.OrgName = (string)reader[OrgHeaderSchema.Constants.OH_FullName];
						line.SystemCode = (string)reader[ClientChargeableUsageSchema.Constants.U1_Code];
						line.Product = line.SystemCode != BillingConstants.BillingSystem.BorderWise
							? Client.EDI.Licencing.Business.ProductTypes.Codes.CargoWiseOne
							: Client.EDI.Licencing.Business.ProductTypes.Codes.BorderWise;
						line.DatabaseServerCode = (string)reader["ServerCode"];
						line.CompanyCode = (string)reader["CompanyCode"];
						line.ClientCompanyPK = !reader.IsDBNull(reader.GetOrdinal(ClientCompanySchema.Constants.PK)) ? (Guid)reader[ClientCompanySchema.Constants.PK] : Guid.Empty;
						line.LicenceCompanyPK = !reader.IsDBNull(reader.GetOrdinal(LicenceCompanySchema.Constants.PK)) ? (Guid)reader[LicenceCompanySchema.Constants.PK] : Guid.Empty;
						line.DatabasePK = !reader.IsDBNull(reader.GetOrdinal(LicenceDatabaseSchema.Constants.PK)) ? (Guid)reader[LicenceDatabaseSchema.Constants.PK] : Guid.Empty;
						line.UnitCount = (int)reader[ClientChargeableUsageSchema.Constants.U1_UnitCount];
						line.PriceItemPK = GetValue<Guid>(reader, ClientLicencePriceItemSchema.Constants.PK);
						line.PriceItemDescription = GetValue<string>(reader, ClientLicencePriceItemSchema.Constants.L7_Description);

						if (systemList.ContainsCode(line.SystemCode))
						{
							lineCollection.Add(line);
						}
					}
				}
			}

			return lineCollection;
		}

		static T GetValue<T>(IDataReader reader, string columnName) => !reader.IsDBNull(reader.GetOrdinal(columnName)) ? (T)reader[columnName] : default;

		void PopulateOdplAndBorderWiseUsageData(CodeDescriptionPairList systemList, Collection<UsageLine> lineCollection)
		{
			foreach (var orgGroup in lineCollection
				.GroupBy(x => new { x.OrgPK, x.OrgName, x.Product })
				.OrderByDescending(x => x.Key.Product)
				.ThenBy(x => x.Key.OrgName))
			{
				var odplUsage = new OdplUsage(orgGroup.Key.OrgName);
				if (orgGroup.Key.Product == Client.EDI.Licencing.Business.ProductTypes.Codes.BorderWise)
				{
					BorderWiseUsages.Add(odplUsage);
				}
				else
				{
					OdplUsages.Add(odplUsage);
				}

				foreach (var systemGroup in orgGroup.GroupBy(y => new { y.SystemCode, y.PriceItemPK }))
				{
					var systemDescription = systemGroup.Key.PriceItemPK.IsEmpty ? systemList.GetDescriptionFromCode(systemGroup.Key.SystemCode) : systemGroup.First().PriceItemDescription.ToString();
					var usageData = odplUsage.AddUsageData(systemDescription);

					foreach (var serverCompanyGroup in systemGroup.GroupBy(z => new { z.DatabasePK, z.DatabaseServerCode, z.CompanyCode }))
					{
						foreach (var line in serverCompanyGroup)
						{
							var fileName = GetOdplReportFileName(periodStart, line.OrgName, systemDescription, line.DatabaseServerCode, line.CompanyCode);
							var pdfLinkUrl = IsReportTooLargeForPdfFormat(new [] { line }) ? "#" : GetOdplReportUrl(periodStart, line.SystemCode, fileName, BillingConstants.FileExtensions.Pdf, line.OrgPK, line.ClientCompanyPK, line.LicenceCompanyPK, line.DatabasePK, line.PriceItemPK);
							var csvLinkUrl = GetOdplReportUrl(periodStart, line.SystemCode, fileName, BillingConstants.FileExtensions.Csv, line.OrgPK, line.ClientCompanyPK, line.LicenceCompanyPK, line.DatabasePK, line.PriceItemPK);
							usageData.AddReportLink(line.DatabaseServerCode, line.CompanyCode, pdfLinkUrl, csvLinkUrl);
						}
					}
				}
			}
		}

		static string GetOdplReportFileName(ZDateTime periodStart, ZString orgName, ZString systemDescription, ZString databaseServerCode, ZString companyCode)
		{
			var fileName = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}_{3}_{4}",
				periodStart.ToString("yyyyMM", CultureInfo.InvariantCulture),
				orgName,
				TrimDescription(systemDescription),
				databaseServerCode.IsEmpty ? "N/A" : databaseServerCode.ToString(),
				companyCode.IsEmpty ? "ALL" : companyCode.ToString());
			var regex = new Regex(string.Format(CultureInfo.InvariantCulture, "[{0}]", Regex.Escape(InvalidFileNameChars)));
			fileName = regex.Replace(fileName, "");
			return fileName;
		}

		static string GetOdplReportUrl(ZDateTime periodStart, string systemCode, string fileName, string fileType, ZGuid orgPk, ZGuid clientCompanyPk, ZGuid licenceCompanyPk, ZGuid databasePk, ZGuid priceItemPk)
		{
			if (!priceItemPk.IsEmpty)
			{
				return GetStlReportUrl(periodStart, databasePk, priceItemPk, clientCompanyPk, fileName, fileType, systemCode);
			}

			var queryString = new SecureQueryString
			{
				[OdplUsageReportRequestHelper.Constants.PeriodStart] = periodStart.ToDateTime().ToString(ZDateTime.ISO8601ShortDateFormat, CultureInfo.InvariantCulture),
				[OdplUsageReportRequestHelper.Constants.SystemCode] = systemCode,
				[OdplUsageReportRequestHelper.Constants.OrganisationPk] = orgPk.ToString(),
				[OdplUsageReportRequestHelper.Constants.ClientCompanyPk] = clientCompanyPk.ToString(),
				[OdplUsageReportRequestHelper.Constants.LicenceCompanyPk] = licenceCompanyPk.ToString(),
				[OdplUsageReportRequestHelper.Constants.DatabasePk] = databasePk.ToString(),
				[OdplUsageReportRequestHelper.Constants.FileName] = fileName,
				[OdplUsageReportRequestHelper.Constants.FileType] = fileType
			};

			var urlTemplate = fileType == BillingConstants.FileExtensions.Csv ? "OdplUsageReportStreamRequestHandler.axd?{0}={1}" : "OdplUsageReportRequestHandler.axd?{0}={1}";
			return string.Format(CultureInfo.InvariantCulture, urlTemplate, SecureQueryString.QueryStringKey, WebUtility.UrlEncode(queryString.ToString()));
		}

		const int MaxUnitCountForPdfFormat = 5000;
		const int MaxFileNameLength = 25;

		bool IsReportTooLargeForPdfFormat(IEnumerable<UsageLine> usageLines)
		{
			return usageLines.Sum(x => x.UnitCount) > MaxUnitCountForPdfFormat;
		}

		static ZString TrimDescription(ZString description) => (description.Length > MaxFileNameLength) ? description.Trim().Split(' ').FirstOrDefault().SubstringSafe(0, MaxFileNameLength) : description;

		#endregion
	}
}
