using System.Collections.Generic;
using System.Xml;

namespace Enterprise.Client.EDI.Licencing.Business
{
	public class LicenceBranchList : List<LicenceBranch>
	{
		public LicenceBranchList()
		{
		}

		public LicenceBranchList(LicenceBranchList value)
		{
			this.AddRange(value);
		}

		public LicenceBranchList(LicenceBranch[] value)
		{
			this.AddRange(value);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes")]
		public void AddToLicenceNode(XmlNode licenceKeyNode)
		{
			if (licenceKeyNode != null)
			{
				XmlDocument licenceKeyXml = licenceKeyNode.OwnerDocument;

				foreach (LicenceBranch branch in this)
				{
					XmlNode node = licenceKeyXml.CreateNode(XmlNodeType.Element, branch.Code, "");
					licenceKeyNode.AppendChild(node);
					branch.AddToLicenceNode(node);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes")]
		public void LoadFromLicenceNode(XmlNode licenceKeyNode)
		{
			if (licenceKeyNode != null)
			{
				foreach (XmlNode branchNode in licenceKeyNode.ChildNodes)
				{
					LicenceBranch branch = new LicenceBranch();
					branch.LoadFromLicenceNode(branchNode);
					Add(branch);
				}
			}
		}
	}
}

