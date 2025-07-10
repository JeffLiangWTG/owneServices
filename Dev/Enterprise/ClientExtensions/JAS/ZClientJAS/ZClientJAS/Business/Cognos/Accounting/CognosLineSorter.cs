using System;
using System.Collections.Generic;

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class CognosLineSorter : IComparer<CognosLineBizO>
	{
		public CognosLineSorter(BusinessObjectFactory factory, ZGuid accountPK)
		{
			ZQuery filter = new ZQuery(ClientCognosGroupingFlagsSchema.T4_AJ, accountPK);
			CognosGroupingFlags groupingFlags = factory.LoadTop1<CognosGroupingFlags>(filter);
			SortingOrder = (groupingFlags != null) ? GetSortingOrder(groupingFlags) : Array.Empty<string>();
		}

		public void Add(CognosLineBizO line)
		{
			Lines.Add(line);
		}

		public string GetSortedLinesAsString()
		{
			ZStringBuilder builder = new ZStringBuilder();

			CognosLineHelper cognosLineHelper = new CognosLineHelper();
			Lines.Sort(this);
			foreach (CognosLineBizO line in Lines)
			{
				builder.Append(cognosLineHelper.ConvertToString(line));
			}

			return builder.ToStringWithNewLineBetweenAppends();
		}

		List<CognosLineBizO> Lines
		{
			get
			{
				if (fLines == null)
				{
					fLines = new List<CognosLineBizO>();
				}
				return fLines;
			}
		}

		string[] GetSortingOrder(CognosGroupingFlags groupingFlags)
		{
			SortedDictionary<ZByte, string> groupingOrderDictionary = new SortedDictionary<ZByte, string>();
			if (groupingFlags.T4_Mode > 0)
			{
				groupingOrderDictionary.Add(groupingFlags.T4_Mode, CognosLineBizO.Schema.Mode);
			}

			if (groupingFlags.T4_Branch > 0)
			{
				groupingOrderDictionary.Add(groupingFlags.T4_Branch, CognosLineBizO.Schema.Branch);
			}

			if (groupingFlags.T4_BusinessType > 0)
			{
				groupingOrderDictionary.Add(groupingFlags.T4_BusinessType, CognosLineBizO.Schema.Business);
			}

			if (groupingFlags.T4_Geographical > 0)
			{
				groupingOrderDictionary.Add(groupingFlags.T4_Geographical, CognosLineBizO.Schema.Geographical);
			}

			if (groupingFlags.T4_Company != ClientCognosGroupingFlagsLookups.IntercompanyCodes.Exclude)
			{
				groupingOrderDictionary.Add(5, CognosLineBizO.Schema.CounterCompany);
				groupingOrderDictionary.Add(6, CognosLineBizO.Schema.TransactionCurrency);
			}

			return new List<string>(groupingOrderDictionary.Values).ToArray();
		}

		readonly string[] SortingOrder;
		List<CognosLineBizO> fLines;

		#region IComparer<CognosLine> Members

		int IComparer<CognosLineBizO>.Compare(CognosLineBizO x, CognosLineBizO y)
		{
			int result = 0;

			foreach (string propertyName in SortingOrder)
			{
				result = ((IComparable)x[propertyName]).CompareTo(y[propertyName]);
				if (result != 0)
				{
					break;
				}
			}

			return result;
		}

		#endregion
	}
}

#region Scenario 1
#endregion
#region Scenario 2
#endregion
#region Scenario 3
#endregion
#region Scenario 4
#endregion
