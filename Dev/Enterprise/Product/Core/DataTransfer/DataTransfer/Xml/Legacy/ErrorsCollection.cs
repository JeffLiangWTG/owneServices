using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace Enterprise.DataTransfer.Xml
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal class ErrorsCollection : StringCollection
	{
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(Count);
			foreach (string error in this)
			{
				sb.Append(error);
				sb.Append("\n");
			}
			return sb.ToString();
		}
	}
}
