using System;
using System.Text;
using System.Xml;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	public class CallStackReader
	{
		public CallStackReader(string xmlText)
		{
			xmlDoc = new ExceptionXml(xmlText);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes")]
		public CallStackReader(XmlDocument xmlDoc)
		{
			this.xmlDoc = xmlDoc;
		}

		readonly XmlDocument xmlDoc;

		public string CallStackText(IAppendStrategy appendStrategy)
		{
			if (appendStrategy == null)
			{
				throw new ArgumentNullException(nameof(appendStrategy));
			}

			StringBuilder builder = new StringBuilder();
			if (xmlDoc.DocumentElement != null)
			{
				XmlNodeList stackTraces = xmlDoc.DocumentElement.SelectNodes("//StackTrace");
				for (int i = stackTraces.Count - 1; i >= 0; i--)
				{
					foreach (XmlNode node in stackTraces[i])
					{
						if (node.Name == "Call")
						{
							appendStrategy.AppendMatch(node.InnerText, builder);
						}
					}
				}
			}

			return builder.ToString();
		}
	}

	public interface IAppendStrategy
	{
		void AppendMatch(string text, StringBuilder builder);
	}

	public class AppendAllStrategy : IAppendStrategy
	{
		public void AppendMatch(string text, StringBuilder builder)
		{
			builder.Append(text.Trim());
			builder.Append(System.Environment.NewLine);
		}
	}
}

