using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Chief.EdiFact.UKCTRL
{
	public class GroupTwoCollection : List<GroupTwo>
	{
		public List<ZString> AllFreeText
		{
			get
			{
				List<ZString> list = new List<ZString>();
				foreach (GroupTwo g2 in this)
				{
					list.AddRange(g2.AllFreeText);
				}
				return list;
			}
		}
	}
}
