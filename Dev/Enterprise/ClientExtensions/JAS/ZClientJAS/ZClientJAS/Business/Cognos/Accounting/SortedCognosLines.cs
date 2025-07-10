using System.Collections.Generic;

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class SortedCognosLines
	{
		public SortedCognosLines(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		public void Add(CognosLineBizO line)
		{
			CognosLineSorter sorter;
			if (!InnerDictionary.ContainsKey(line.AccountPK))
			{
				sorter = new CognosLineSorter(Factory, line.AccountPK);
				InnerDictionary.Add(line.AccountPK, sorter);
			}
			else
			{
				sorter = InnerDictionary[line.AccountPK];
			}

			sorter.Add(line);
		}

		public string GetLinesAsString()
		{
			ZStringBuilder builder = new ZStringBuilder();
			foreach (CognosLineSorter cognosLineSorter in InnerDictionary.Values)
			{
				builder.Append(cognosLineSorter.GetSortedLinesAsString());
			}
			return builder.ToStringWithNewLineBetweenAppends();
		}

		Dictionary<ZGuid, CognosLineSorter> InnerDictionary
		{
			get
			{
				if (fInnerDictionary == null)
				{
					fInnerDictionary = new Dictionary<ZGuid, CognosLineSorter>();
				}
				return fInnerDictionary;
			}
		}

		Dictionary<ZGuid, CognosLineSorter> fInnerDictionary;
		readonly BusinessObjectFactory Factory;
	}
}

#region Implementation
#endregion
