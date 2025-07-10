using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Customs.Shared.WorldCustomsOrganisation.MessageDefinitions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation
{
	public abstract class FriendlyCodeWithPointers
	{
		public FriendlyCodeWithPointers(IPointerParser pointerParser, ICodeWithPointers codeWithPointers, XElement requestXml)
		{
			CodeWithPointers = codeWithPointers;
			RequestXml = requestXml;
			Parser = pointerParser;
		}

		protected readonly XElement RequestXml;
		protected readonly ICodeWithPointers CodeWithPointers;
		protected readonly IPointerParser Parser;

		public string Code => CodeWithPointers.Code;

		public string Description => CodeWithPointers.Description;

		public string PseudoXpath => pseudoXpath ?? (pseudoXpath = Parser.GetFullNamesFromPointers(TagIdsPath));

		public string ActualXPath => ActualXPathInfo.Path;

		public string TagIdsPath => tagIdsPath ?? (tagIdsPath = CodeWithPointers.GetPointers());

		public string LineNumber => GoodsItemInfo.LineNumber;

		public string FinalFieldName => finalFieldName ?? (finalFieldName = Parser.GetFinalElementNameFromPointers(TagIdsPath));

		public string FinalFieldValue => finalFieldValue ?? (finalFieldValue = !ActualXPathInfo.HasInvalidNode && GoodsItemInfo.HasSpecifiedIndex ? RequestXml?.XPathSelectElement(ActualXPath)?.Value ?? PointerParserAbstract.ABSENT : PointerParserAbstract.ABSENT);

		public bool ErrorPointIsToGovernmentAgencyGoodsItem
		{
			get
			{
				var pointer = CodeWithPointers.Pointers.LastOrDefault();
				return pointer != null && pointer.DocumentSectionCode.EndsWith(PointerParserAbstract.IdOfGoodsItem, true, CultureInfo.InvariantCulture) && string.IsNullOrEmpty(pointer.TagId);
			}
		}
		public string DataElement => Parser.GetDataElementFromPath(TagIdsPath);

		#region Implement

		(string Path, bool HasInvalidNode) ActualXPathInfo
		{
			get
			{
				if (actualXPathInfo == null)
				{
					actualXPathInfo = Parser.GetXPathFromPointer(TagIdsPath);
				}

				return (actualXPathInfo.Path, actualXPathInfo.HasInvalidNode);
			}
		}

		(string LineNumber, bool HasSpecifiedIndex) GoodsItemInfo
		{
			get
			{
				if (goodsItemInfo == null)
				{
					var lineNumber = string.Empty;
					var hasSpecifiedIndex = true;

					var sequenceNumeric = CodeWithPointers.Find(PointerParserAbstract.IdOfGoodsItem)?.SequenceNumeric;

					if (sequenceNumeric != null)
					{
						if (sequenceNumeric > 0)
						{
							lineNumber = sequenceNumeric.ToString();
						}
						else
						{
							var countOfGoodsItem = RequestXml?.XPathSelectElements((NoResString)@"/*[local-name()='Declaration']/*[local-name()='GoodsShipment']/*[local-name()='GovernmentAgencyGoodsItem']").Take(2).Count() ?? 0;

							if (countOfGoodsItem == 1)
							{
								lineNumber = PointerParserAbstract.DefaultIndex;
							}

							hasSpecifiedIndex = false;
						}
					}

					goodsItemInfo = (lineNumber, hasSpecifiedIndex);
				}

				return goodsItemInfo.Value;
			}
		}

		string pseudoXpath;
		string tagIdsPath;
		string finalFieldName;
		string finalFieldValue;
		GetXPathFromPointerResult actualXPathInfo;
		(string LineNumber, bool HasSpecifiedGoodsLine)? goodsItemInfo;

		#endregion
	}
}
