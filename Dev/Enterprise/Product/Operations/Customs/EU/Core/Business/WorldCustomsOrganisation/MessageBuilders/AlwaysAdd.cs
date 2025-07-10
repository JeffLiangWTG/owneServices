using System.Xml.Linq;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders
{
	public class AlwaysAdd
	{
		public string Name;
		public int Index;
		public XElement Node;
		public string RootName;

		public AlwaysAdd(string name, int index, XElement node, string rootName)
		{
			Name = name;
			Index = index;
			Node = node;
			RootName = rootName;
		}
	}
}
