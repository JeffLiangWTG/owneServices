using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.ValueProviders;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.ValueReplacers
{
	class TranslateMacroForDocumentFilter
	{
		public TranslateMacroForDocumentFilter(DataProviderList boDocDataProviders)
		{
			this.dataProviderList = boDocDataProviders;
		}

		readonly DataProviderList dataProviderList;

		public string ReplaceMacros(string inputFilterString)
		{
			do
			{
				inputFilterString = RegexProvider.InnermostMacrosRegex.Replace(inputFilterString, new MatchEvaluator(ReplaceSingleMacro));
			}
			while (RegexProvider.InnermostMacrosRegex.IsMatch(inputFilterString));
			return inputFilterString;
		}

		string ReplaceSingleMacro(Match match)
		{
			string singleMacro = match.Groups[0].Value;
			string fieldName = singleMacro.Substring(1, singleMacro.Length - 2);

			if (DocumentCommandCollection.TryGetCachedMacroValue(singleMacro, out var cachedValue))
			{
				return cachedValue;
			}

			object result = null;

			if (result == null)
			{
				SystemDataProvider.SystemDataProviderDelegate getDataDelegate;
				if (systemDataProvider.TryGetValue(fieldName, out getDataDelegate))
				{
					result = getDataDelegate();
				}
			}

			if (result == null)
			{
				var replacementValue = macroStringReplacer.ReplaceMacros(singleMacro);
				if (replacementValue != singleMacro)
				{
					result = replacementValue;
				}
			}

			var returnValue = result != null ? result.ToString().EscapeBackslashes().EscapeQuotes() : string.Empty;
			DocumentCommandCollection.CacheMacroValue(singleMacro, returnValue);
			return returnValue;
		}

		SystemDataProvider _systemDataProvider;
		SystemDataProvider systemDataProvider
		{
			get { return _systemDataProvider ?? (_systemDataProvider = new SystemDataProvider()); }
		}

		MacroStringReplacer _macroStringReplacer;
		MacroStringReplacer macroStringReplacer
		{
			get { return _macroStringReplacer ?? (_macroStringReplacer = new MacroStringReplacer(dataProviderList)); }
		}

		class SystemDataProvider
		{
			public SystemDataProvider()
			{
				dataProviders = new Dictionary<string, SystemDataProviderDelegate>
				{
					{ "CURRENTCOMPANY", GetCurrentCompany },
					{ "USEDOCBUILDERFREIGHTDOCS", GetUseDocBuilderFreightDocs },
					{ "USEDOCBUILDERRATINGANDQUOTATIONDOCS", GetUseDocBuilderRatingAndQuotationDocs },
					{ "USEDOCBUILDERWAREHOUSEDOCSONLY", GetUseDocBuilderWarehouseDocsOnly },
					{ "USEDOCBUILDERORDERDOCS", GetUseDocBuilderOrderDocs },
					{
						"USEDOCBUILDERORGANIZATIONDOCS",
						() => DocumentsDataRegistry.Instance.UseNewDocBuilderOrganizationDocumentsOnly.Value ? "Y" : "N"
					},
					{
						"USEDOCBUILDERLINERANDAGENCYDOCS",
						() => DocumentsDataRegistry.Instance.UseNewDocBuilderLinerAndAgencyDocumentsOnly.Value ? "Y" : "N"
					}
				};
			}
			readonly Dictionary<string, SystemDataProviderDelegate> dataProviders;

			public delegate string SystemDataProviderDelegate();

			public bool TryGetValue(string fieldName, out SystemDataProviderDelegate getDataDelegate)
			{
				string[] splitedFieldName = fieldName.Split(new char[] { '.' }, 2);
				string dataSource = splitedFieldName.Length > 0 ? splitedFieldName[0] : "";
				dataField = splitedFieldName.Length > 1 ? splitedFieldName[1] : "";
				return dataProviders.TryGetValue(dataSource.ToUpperInvariant(), out getDataDelegate);
			}

			string dataField;

			string GetCurrentCompany()
			{
				var dataProvider = new BusinessObjectDataProvider(new DataProviderList(BODocDataProvider.Get(GlbCompany.CurrentCompany)), null);
				var dataValue = dataProvider.GetColumnValue(null, 0, dataField);
				return dataValue is null ? string.Empty : dataValue.ToString();
			}

			string GetUseDocBuilderFreightDocs()
			{
				return DocumentsDataRegistry.Instance.UseNewDocBuilderForwardingDocuments.Value ? "Y" : "N";
			}

			string GetUseDocBuilderRatingAndQuotationDocs()
			{
				return DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.Value ? "Y" : "N";
			}

			string GetUseDocBuilderWarehouseDocsOnly()
			{
				return DocumentsDataRegistry.Instance.UseNewDocBuilderWarehouseDocumentsOnly.Value ? "Y" : "N";
			}

			string GetUseDocBuilderOrderDocs()
			{
				return DocumentsDataRegistry.Instance.UseNewDocBuilderOrderManagerDocuments.Value ? "Y" : "N";
			}
		}
	}
}
