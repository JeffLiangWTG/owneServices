using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common.Collections;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public class RateCodeDescriptionPairList : CodeDescriptionPairList
	{
		public RateCodeDescriptionPairList()
		{
		}

		public RateCodeDescriptionPairList(IEnumerable<ICodeDescription> listToClone)
		{
			foreach (ICodeDescription element in listToClone)
			{
				Elements.Add(new CodeDescriptionPair(element.Code, (NoResString)element.Description));
			}
		}

		public new void InsertInSortOrder(ICodeDescription item)
		{
			Elements.InsertInSortOrder(item, new RateCodeComparer());
		}

		public new void Sort()
		{
			Elements.Sort(new RateCodeComparer());
		}
	}

	class RateCodeComparer : IComparer<ICodeDescription>
	{
		public int Compare(ICodeDescription x, ICodeDescription y)
		{
			if (x != null && y != null)
			{
				if (Char.IsDigit(x.Code.FirstOrDefault()) && !Char.IsDigit(y.Code.FirstOrDefault()))
				{
					return 1;
				}
				else if (!Char.IsDigit(x.Code.FirstOrDefault()) && Char.IsDigit(y.Code.FirstOrDefault()))
				{
					return -1;
				}
			}
			return string.Compare(x.Code, y.Code, StringComparison.OrdinalIgnoreCase);
		}
	}
}
