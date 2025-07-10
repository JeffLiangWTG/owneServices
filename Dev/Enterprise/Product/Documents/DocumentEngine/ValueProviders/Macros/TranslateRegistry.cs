using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.ResourceStrings.Cache;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class TranslateRegistry : ValueProviderWithLoadControlFactory
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<TranslateRegistry({RegistryID}, {English Text} [,{Language}])>",
				ResString.GetMultilingualString("cd6ac891-3a49-4ec1-9ebb-62661dbbb0e0", @"Translate the English text stored in a particular translatable registry to the selected language in the report filter."),
				new List<(string example, object expectedResult)> {
					((NoResString)"<TranslateRegistry(CashFlowActivityConfiguration, <EnglishText>, ZH-CN)>",
					((CashFlowActivityConfiguration)AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration.DefaultValue.FirstOrDefault()).Description.ToString(Core.SharedConstants.Languages.ChineseSimplified)) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			Match match = Regex.Match(macro);
			string registryId = match.Groups[1].Value.TrimEnd();
			string base64EnglishText = match.Groups[2].Value;
			byte[] textAsBytes = Convert.FromBase64String(base64EnglishText);
			string englishText = Encoding.UTF8.GetString(textAsBytes).TrimEnd();
			string language = match.Groups[3].Value;
			object result = englishText;

			if (string.IsNullOrEmpty(language) && report.FilterCollection["Translation Language"] != null && report.FilterCollection["Translation Language"].ValueAsObject != null)
			{
				language = report.FilterCollection["Translation Language"].ValueAsObject.ToString();
			}
			else if (Culture.LanguageCodeMapping.ContainsKey(language))
			{
				report.ErrorManager.Add(new ReportProcessingError(GetLanguageCodeErrorMessage(macro, language, Culture.LanguageCodeMapping[language]), ReportProcessingErrorSeverity.Warning));
				language = Culture.LanguageCodeMapping[language];
			}

			// You can add more search path for registries from other name space here, if needed.
			var registryItem = AccountingMasterFilesRegistry.Instance.FindByName(registryId);
			if (registryItem != null && registryItem is ICustomizableDataCaptionSource)
			{
				var dataString = CustomizableDataResourceStrings.GetMultilingualString((ICustomizableDataCaptionSource)registryItem, null, englishText);
				if (dataString != null && !string.IsNullOrEmpty(language))
				{
					result = dataString.ToString(language);
				}
			}
			return result;
		}

		protected override bool PassNestedMacroFormulaAsText { get { return true; } }

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)TranslateRegistry(?:[\s]*)\((?:[\s]*)([^,]*)(?:[\s]*),(?:[\s]*)([^,]*)(?:[\s]*)\s*(?:,\s*([^\s,]*)\s*)?\)\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
