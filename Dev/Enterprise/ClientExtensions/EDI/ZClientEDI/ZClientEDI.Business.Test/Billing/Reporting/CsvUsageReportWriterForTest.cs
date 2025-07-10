using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class CsvUsageReportWriterForTest : ICsvUsageReportWriter
	{
		ZStringBuilder ReportLines = new ZStringBuilder();

		public void WriteCsvUsageReport(ZDateTime usageTime, ZString company, ZString branch, ZString staff, ZString reference, ZString priceCode, ZString priceItemDescription, ZInt unitCount, ZDecimal? adjustedUnitCount = null, ZString? tenantID = null)
		{
			var dataValuesList = new List<string>(9);
			dataValuesList.Add(usageTime.ToLongTimeString());
			dataValuesList.Add(company);
			dataValuesList.Add(branch);
			dataValuesList.Add(staff);
			dataValuesList.Add(reference);
			dataValuesList.Add(priceCode);
			dataValuesList.Add(priceItemDescription);
			dataValuesList.Add(unitCount.ToString());
			if (adjustedUnitCount.HasValue)
			{
				dataValuesList.Add(adjustedUnitCount.Value.ToString());
			}
			if (tenantID.HasValue)
			{
				dataValuesList.Add(tenantID.Value.ToString());
			}
			var dataCsvLine = new OCsvLine(dataValuesList.ToArray());
			ReportLines.AppendLine(dataCsvLine.ToString());
		}

		public override string ToString()
		{
			return ReportLines.ToString();
		}

		public void Clear()
		{
			ReportLines = new ZStringBuilder();
		}
	}
}
