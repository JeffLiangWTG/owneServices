using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI.Internal;
using Microsoft.AspNetCore.Components;
using static Enterprise.Integration.Initialisation;
using static Enterprise.Integration.MarketingManager;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZGuidFindBoxColumnStyle
	{
		protected override MarkupString GetRenderContent(CurrencyManager source, int rowNum, bool alignToRight)
		{
			var oldCurrentItem = ((ZFindBoxUserControl)FindBox).CurrentItem;
			var oldList = ((ZFindBoxUserControl)FindBox).List;
			try
			{
				if (source.List != null && source.List.Count > rowNum)
				{
					((ZFindBoxUserControl)FindBox).CurrentItem = source.List[rowNum];
					((ZFindBoxUserControl)FindBox).DataPropertyName = MappingName;
					((ZFindBoxUserControl)FindBox).PullList();
				}
				return base.GetRenderContent(source, rowNum, alignToRight);
			}
			finally
			{
				((ZFindBoxUserControl)FindBox).List = oldList;
				((ZFindBoxUserControl)FindBox).CurrentItem = oldCurrentItem;
			}
		}
	}
}
