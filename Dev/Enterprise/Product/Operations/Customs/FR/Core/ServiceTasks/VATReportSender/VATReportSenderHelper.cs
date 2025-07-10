using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Registry;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Transformation;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.ServiceTasks
{
	public static class VATReportSenderHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public static IEnumerable<IGrouping<string, VATReportFee>> GetRecords(Guid companyPK, DateTime from, DateTime to)
		{
			var fees = new List<VATReportFee>();

			Db.Connection.ExecuteReader("SELECT * FROM Report_FRVAT(@companyPK, null, null, null, @from, @to)", command =>
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@from", SqlDbType.SmallDateTime, from);
				command.AddParameter("@to", SqlDbType.SmallDateTime, to);
			}, reader =>
			{
				var fee = new VATReportFee();
				fee.JobNumber = ConvertNullToEmpty(reader["JobNumber"]);
				fee.DUCR = ConvertNullToEmpty(reader["DUCR"]);
				fee.EntryNumber = ConvertNullToEmpty(reader["EntryNumber"]);
				fee.EntryReference = ConvertNullToEmpty(reader["EntryReference"]);
				fee.BAEDate = (DateTime)reader["BAEDate"];
				fee.CPC = ConvertNullToEmpty(reader["CPC"]);
				fee.CountriesOfSupply = ConvertNullToEmpty(reader["CountriesOfSupply"]);
				fee.InvoiceNumbers = ConvertNullToEmpty(reader["InvoiceNumbers"]);
				fee.TotalInvoiceAmount = ConvertNullToZero(reader["TotalInvoiceAmount"]);
				fee.Currency = ConvertNullToEmpty(reader["Currency"]);
				fee.Rate = ConvertNullToZero(reader["Rate"]);
				fee.ImporterCode = ConvertNullToEmpty(reader["ImporterCode"]);
				fee.ImporterName = ConvertNullToEmpty(reader["ImporterName"]);
				fee.SupplierName = ConvertNullToEmpty(reader["SupplierName"]);
				fee.EORI = ConvertNullToEmpty(reader["EORI"]);
				fee.VATNumber = ConvertNullToEmpty(reader["VATNumber"]);
				fee.TotalVATBaseAmount = ConvertNullToZero(reader["TotalVATBaseAmount"]);
				fee.TotalVATAmount = ConvertNullToZero(reader["TotalVATAmount"]);
				fee.VATProcedureCode = ConvertNullToEmpty(reader["VATProcedureCode"]);
				fee.VATProcedureDescription = ConvertNullToEmpty(reader["VATProcedureDescription"]);
				fee.OrderRefs = ConvertNullToEmpty(reader["OrderRefs"]);
				fees.Add(fee);
			});

			return fees
				.OrderBy(x => x.BAEDate)
				.GroupBy(x => x.ImporterCode).ToArray();
		}

		static string ConvertNullToEmpty(object inputValue)
		{
			if (inputValue == DBNull.Value || inputValue == null)
			{
				return String.Empty;
			}
			else
			{
				return (string)inputValue;
			}
		}

		static decimal ConvertNullToZero(object inputValue)
		{
			if (inputValue == DBNull.Value || inputValue == null)
			{
				return Decimal.Zero;
			}
			else
			{
				return (decimal)inputValue;
			}
		}

		public static void ConvertReportToFile(IEnumerable<VATReportFee> fees, string filePathAndName, GlbCompany company, BusinessObjectFactory factory)
		{
			var rows = ConvertFeesToStringCollectionList(fees, company, factory);
			SaveToFile(filePathAndName, rows);
		}

		static List<StringCollectionX> ConvertFeesToStringCollectionList(IEnumerable<VATReportFee> fees, GlbCompany company, BusinessObjectFactory factory)
		{
			var rows = new List<StringCollectionX>();
			var properties = GetPropertyInfosToPrint(company, factory);
			rows.Add(new StringCollectionX(properties.Select(x => DataBoundResourceStrings.GetDataForProperty(x).Caption).ToArray()));
			rows.AddRange(fees.Select(fee => new StringCollectionX(properties.Select(x => x.GetValue(fee).ToString()).ToArray())));

			return rows;
		}

		static PropertyInfo[] GetPropertyInfosToPrint(GlbCompany company, BusinessObjectFactory factory)
		{
			var cacheKey = "FR.VATReportSenderHelper.PropertyInfos." + company.GC_Code;

			return factory.GetCachedValue(cacheKey, () =>
			{
				var configurationDescription = FRCustomsDataRegistry.Instance.FRVATReportConfiguration.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
				var properties = VATReportFeePropertiesInOrder.ToArray();
				if (!string.IsNullOrEmpty(configurationDescription))
				{
					var headerCaptions = GetHeaderCaptions(configurationDescription, company, factory);
					var selectedVATReportFeePropertiesInOrder = VATReportFeePropertiesInOrder.Where(x => headerCaptions.Contains(DataBoundResourceStrings.GetDataForProperty(x).Caption)).ToArray();
					if (selectedVATReportFeePropertiesInOrder.Length != 0)
					{
						properties = selectedVATReportFeePropertiesInOrder;
					}
				}
				return properties;
			});
		}

		static IEnumerable<ZString> GetHeaderCaptions(string configurationDescription, GlbCompany company, BusinessObjectFactory factory)
		{
			var key = new ReportColumnSettingRegistryPrefixHelper().GetKey(configurationDescription, company.GC_Code);
			var helper = new ReportColumnSettingHelper();
			var result = helper.LoadReportColumnSettings(FRCustomsDataRegistry.FRVATReportPK);
			ReportColumnSettings reportColumnSettings = null;
			foreach (var keyvaluePair in result)
			{
				var stmData = factory.Load<StmData>(keyvaluePair.Key);
				if (stmData != null && stmData.SD_Name == key)
				{
					reportColumnSettings = keyvaluePair.Value;
					break;
				}
			}

			var headerCaptions = reportColumnSettings?.Worksheets[0]?.ColumnHeadings?.Cast<ColumnHeading>().Select(x => x.Description) ?? new List<ZString> { };
			return headerCaptions;
		}

		static void SaveToFile(ZString filePathAndName, List<StringCollectionX> rows)
		{
			using (var excel = new ExcelInterface())
			{
				excel.NewExcelFile(1);

				for (int i = 0; i < rows.Count; i++)
				{
					var row = rows.ElementAt(i);
					for (int j = 0; j < row.Count; j++)
					{
						excel.Xls.SetCellValue(i + 1, j + 1, row[j]);
					}
				}
				excel.SaveToFile(filePathAndName);
			}
		}

		static ImmutableArray<PropertyInfo> VATReportFeePropertiesInOrder => (vatReportFeePropertiesInOrder ?? (vatReportFeePropertiesInOrder = typeof(VATReportFee).GetProperties(BindingFlags.Instance | BindingFlags.Public)
			.Where(x => x.GetCustomAttribute<VATReportReturnedParameterAttribute>() != null)
			.OrderBy(x => x.GetCustomAttribute<VATReportReturnedParameterAttribute>().Order).ToImmutableArray())).Value;
		[ThreadStatic]
		static ImmutableArray<PropertyInfo>? vatReportFeePropertiesInOrder;
	}
}
