using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public class CAInvoiceLineComparer : IComparer<JobComInvoiceLine>, IComparer
	{
		readonly bool invert;
		readonly string lineName;
		readonly bool cacheValue4Comparing;
		readonly Dictionary<string, Values4Comparing> cachedValues4Comparing;

		public CAInvoiceLineComparer()
			: this(false, ZString.Empty) { }

		public CAInvoiceLineComparer(bool invert, ZString propertyName, bool cacheValue4Comparing = false)
		{
			this.invert = invert;
			this.lineName = propertyName;
			this.cacheValue4Comparing = cacheValue4Comparing;
			cachedValues4Comparing = new Dictionary<string, Values4Comparing>();
		}

		#region IComparer<JobComInvoiceLine>

		public int Compare(JobComInvoiceLine lineLeft, JobComInvoiceLine lineRight)
		{
			var result = 0;
			var lineLeftKeyValues = CalculateKeyValues4Comparing(lineLeft);
			var lineRightKeyValues = CalculateKeyValues4Comparing(lineRight);

			if (lineLeftKeyValues.ValidNO && lineRightKeyValues.ValidNO)
			{
				result = lineLeftKeyValues.LineNO.CompareTo(lineRightKeyValues.LineNO);
				if (result == 0)
				{
					if (lineLeftKeyValues.HasSL && !lineRightKeyValues.HasSL)
					{
						result = 1;
					}
					else if (!lineLeftKeyValues.HasSL && lineRightKeyValues.HasSL)
					{
						result = -1;
					}
				}
			}
			return invert ? -result : result;
		}

		#endregion

		#region IComparer

		public int Compare(object x, object y)
		{
			return Compare((JobComInvoiceLine)x, (JobComInvoiceLine)y);
		}

		#endregion

		struct Values4Comparing
		{
			public bool HasSL;
			public bool ValidNO;
			public decimal LineNO;
		}

		Values4Comparing CalculateKeyValues4Comparing(JobComInvoiceLine line)
		{
			Values4Comparing result;
			if (cacheValue4Comparing)
			{
				var cacheKey = line.PK.IsEmpty ? line.GetHashCode().ToString() : line.PK.ToString();
				if (cachedValues4Comparing.TryGetValue(cacheKey, out var value))
				{
					result = value;
				}
				else
				{
					result = CalculateKeyValues4ComparingWithoutCache(line);
					cachedValues4Comparing.Add(cacheKey, result);
				}
			}
			else
			{
				result = CalculateKeyValues4ComparingWithoutCache(line);
			}
			return result;
		}

		Values4Comparing CalculateKeyValues4ComparingWithoutCache(JobComInvoiceLine line)
		{
			ZString oriLineNo1 = ZString.Empty;
			if (JobComInvoiceLine.Schema.CA_OriginalLineNo == lineName)
			{
				oriLineNo1 = line.CA_OriginalLineNo;
			}
			else if (JobComInvoiceLine.Schema.JI_B3LineNumber == lineName)
			{
				oriLineNo1 = line.JI_B3LineNumber;
			}
			decimal lineNo;
			var hasSL = oriLineNo1.EndsWith(JobComInvoiceLine.SplitLine);
			var parseSuccess = hasSL ? decimal.TryParse(oriLineNo1.Substring(0, oriLineNo1.Length - 3), out lineNo) : decimal.TryParse(oriLineNo1, out lineNo);
			return new Values4Comparing()
			{
				HasSL = hasSL,
				ValidNO = parseSuccess,
				LineNO = lineNo
			};
		}
	}
}
