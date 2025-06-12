using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace eServices.eHubAdmin.IntegrationTests
{
	public static class TestHelper
	{

		public static void ReplaceInnerHtml(HtmlDocument document, string xpath, string oldValue, string newValue)
		{
			var selectedNodes = document.DocumentNode.SelectNodes(xpath);
			foreach (var node in selectedNodes)
			{
				if (node.InnerText.Contains(oldValue))
					node.InnerHtml = node.InnerText.Replace(oldValue, newValue);
			}
		}
	}
}
