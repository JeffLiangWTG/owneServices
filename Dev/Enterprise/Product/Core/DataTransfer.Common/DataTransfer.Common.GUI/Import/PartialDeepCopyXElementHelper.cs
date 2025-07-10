using System;
using System.Linq;
using System.Xml.Linq;

namespace Enterprise.DataTransfer.Common.GUI.Import
{
	public static class PartialXmlHelper
	{
		const string TerminatingPartialCopyXElement = "TERMINATING_PARTIAL_COPY_XELEMENT";

		public static string GeneratePartialXML(XElement xelement, int maxNodeCopied = 25)
		{
			string sourceString;
			var newXElement = DeepCopyNodeNameAndAttribute(xelement);
			GeneratePartialXElement(xelement, newXElement, maxNodeCopied);
			sourceString = newXElement.ToString();
			var terminatingPosition = sourceString.LastIndexOf(TerminatingPartialCopyXElement, StringComparison.Ordinal);
			if (terminatingPosition > 0)
			{
				sourceString = sourceString.Remove(terminatingPosition);
			}

			return sourceString;
		}

		static int GeneratePartialXElement(XElement original, XElement partialElement, int maxNodeCopied, int nodeCopiedCount = 0)
		{
			foreach (var element in original.Elements())
			{
				var beforeTerminating = nodeCopiedCount == maxNodeCopied - 1;
				XElement newXElement = DeepCopyNodeNameAndAttribute(element);
				nodeCopiedCount++;
				if (nodeCopiedCount >= maxNodeCopied)
				{
					if (beforeTerminating)
					{
						partialElement.Add(System.Environment.NewLine + "..." + System.Environment.NewLine + TerminatingPartialCopyXElement);
					}

					return nodeCopiedCount;
				}
				if (element.HasElements)
				{
					nodeCopiedCount = GeneratePartialXElement(element, newXElement, maxNodeCopied, nodeCopiedCount);
				}
				else
				{
					newXElement.Add(element.Value);
				}
				partialElement.Add(newXElement);
			}
			return nodeCopiedCount;
		}

#if DEBUG
		public
#endif
		static XElement DeepCopyNodeNameAndAttribute(XElement element)
		{
			var newXElement = new XElement(element.Name);
			newXElement.Add(element.Attributes().ToArray());
			return newXElement;
		}
	}
}
