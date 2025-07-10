using System;
using System.Linq;
#if NETFRAMEWORK
using System.Web.UI;
#endif
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.CA.GUI
{
	public static class AIRSSelectionLoader
	{
#if NETFRAMEWORK
		const string TrTag = nameof(HtmlTextWriterTag.Tr);
		const string ThTag = nameof(HtmlTextWriterTag.Th);
		const string TdTag = nameof(HtmlTextWriterTag.Td);
#else
		const string TrTag = "Tr";
		const string ThTag = "Th";
		const string TdTag = "Td";
#endif

		[CodeAlive("Used Code")]
		enum GridTitleText { Materialized, DeMaterialized, AIRSRegistration, OR, UnDefined }

		public static void InitializeAIRSNavigator(HtmlDocument webContent, AIRSWebpageNavigator navigator)
		{
			var configuration = navigator.WebPageConfiguration;
			navigator.AG_EndUseCode = GetElementTextById(webContent, configuration.EndUseTextID).Left(3);
			navigator.AG_ExtensionCode = GetElementTextById(webContent, configuration.ExtensionTextID).Left(6);
			navigator.AG_Miscellaneous = GetElementTextById(webContent, configuration.MiscTextID).Left(3);

			var tableCodes = webContent?.GetElementById(configuration.IIDTableID);
			var rowElements = tableCodes?.GetElementsByTagName(TrTag);
			GridTitleText currentTitle = GridTitleText.UnDefined;

			var lpcoList = navigator.LPCOList;
			lpcoList.Clear();
			if (tableCodes != null)
			{
				foreach (HtmlElement row in rowElements.Cast<HtmlElement>())
				{
					var cellElements = row.Children.Cast<HtmlElement>();
					var randomElement = cellElements.FirstOrDefault();
					if (randomElement != null && ((ZString)ThTag).EqualsIgnoringCase(randomElement.TagName))
					{
						var thInnerText = randomElement.InnerText;
						if (thInnerText.StartsWith(configuration.MaterializedGridTitle, StringComparison.OrdinalIgnoreCase))
						{
							currentTitle = GridTitleText.Materialized;
						}
						else if (thInnerText.StartsWith(configuration.DeMaterializedGridTitle, StringComparison.OrdinalIgnoreCase))
						{
							currentTitle = GridTitleText.DeMaterialized;
						}
						else if (thInnerText.StartsWith(configuration.AIRSRegistrationGridTitle, StringComparison.OrdinalIgnoreCase))
						{
							currentTitle = GridTitleText.AIRSRegistration;
						}
						else if (thInnerText.StartsWith(configuration.OrText, StringComparison.OrdinalIgnoreCase))
						{
							currentTitle = GridTitleText.OR;
						}
						else
						{
							continue;
						}

						if (currentTitle == GridTitleText.Materialized || currentTitle == GridTitleText.DeMaterialized || currentTitle == GridTitleText.AIRSRegistration)
						{
							if (!lpcoList.Any())
							{
								lpcoList.Add(new AIRSLPCOSelection(navigator.Factory));
							}
						}
						else if (currentTitle == GridTitleText.OR)
						{
							lpcoList.Add(new AIRSLPCOSelection(navigator.Factory, true));
						}
					}
					else if (randomElement != null && randomElement.TagName.ToUpper() == TdTag.ToUpper())
					{
						var currentDataSet = lpcoList.LastOrDefault();
						if (currentDataSet != null)
						{
							ZString code = cellElements.FirstOrDefault(x => x.GetAttribute(configuration.LPCOCodeAttributeKey) == configuration.LPCOCodeAttributeCode)?.InnerText ?? ZString.Empty;
							ZString description = cellElements.FirstOrDefault(x => x.GetAttribute(configuration.LPCOCodeAttributeKey) == configuration.LPCOCodeAttributeDescription)?.InnerText ?? ZString.Empty;

							if (!code.IsEmpty && !description.IsEmpty)
							{
								if (currentTitle == GridTitleText.Materialized)
								{
									currentDataSet.MaterializedLPCOs.AddPairIfNotExist(code, description);
								}
								else if (currentTitle == GridTitleText.DeMaterialized)
								{
									currentDataSet.DeMaterializedLPCOs.AddPairIfNotExist(code, description);
								}
								else if (currentTitle == GridTitleText.AIRSRegistration)
								{
									currentDataSet.AIRSRegistrations.AddPairIfNotExist(code, description);
								}
							}
						}
					}
				}
				lpcoList.Reverse();
				foreach (var set in lpcoList)
				{
					set.BuildWebContent();
				}
			}
		}

		public static ZString GetElementTextById(HtmlDocument document, ZString elementId)
		{
			if (document != null)
			{
				return document.GetElementById(elementId)?.InnerText ?? ZString.Empty;
			}
			return ZString.Empty;
		}
	}
}
