using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AE.Business;

public class LayoutBuilder
{
	public void BuildLayout(ManifestLine root, ForwardingConsol consol, ManifestLayout layout)
	{
		if (layout == ManifestLayout.General)
		{
			root = BuildGeneralLayout(root);
		}
		//else: already FCL layout
	}

	internal ManifestLine BuildGeneralLayout(ManifestLine root)
	{
		if (root != null)
		{
			ManifestLine newParent = root is BOLLine ? root : null;
			ManifestLineCollection newChildren = root is BOLLine ? new ManifestLineCollection() : null;

			foreach (ManifestLine line in root.Children)
			{
				if (newParent != null)
				{
					newChildren.Add(line.Children);
				}
				else
				{
					BuildGeneralLayout(line);
				}
			}
			if (newParent != null)
			{
				newParent.Children = newChildren;
			}
		}
		return root;
	}
}

public enum ManifestLayout { FCL, General }
