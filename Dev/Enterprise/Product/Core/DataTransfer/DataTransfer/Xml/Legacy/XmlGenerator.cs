using System.ComponentModel;
using CargoWise.Types;

namespace Enterprise.DataTransfer.Xml
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class XmlGenerator
	{
		public XmlGenerator()
		{
		}

		public string XmlFragment(ZString value, ZString tag)
		{
			ZString result = "";
			if (!value.IsEmpty)
			{
				result = "<" + tag + ">" + value + "</" + tag + ">" + "\n";
			}
			return result;
		}
	}
}
