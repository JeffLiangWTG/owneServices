using System.Collections;
using System.Globalization;
using Enterprise.UniversalCopy.Business;

namespace Enterprise.UniversalCopy.GUI
{
	class NodeSorter : IComparer
	{
		public int Compare(object x, object y)
		{
			var nodeX = x as UniversalCopyTreeNode;
			var nodeY = y as UniversalCopyTreeNode;

			var nodeXBizo = nodeX.BizO as CollectionCopyTemplateBizo;
			var nodeYBizo = nodeY.BizO as CollectionCopyTemplateBizo;

			if ((nodeXBizo != null && nodeXBizo.IsSplitCollection) && (nodeYBizo == null || !nodeYBizo.IsSplitCollection))
			{
				return -1;
			}
			else if ((nodeXBizo == null || !nodeXBizo.IsSplitCollection) && (nodeYBizo != null && nodeYBizo.IsSplitCollection))
			{
				return 1;
			}
			else
			{
				CaseInsensitiveComparer c = new CaseInsensitiveComparer(CultureInfo.CurrentCulture);
				return c.Compare(nodeX?.Text, nodeY?.Text);
			}
		}
	}
}
