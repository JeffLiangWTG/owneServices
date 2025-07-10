using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Tools.SpellCheck;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using WTG.SpellCheck;

namespace ResourceStrings.SpellCheck.TestRunner
{
	public static class Runner
	{
		public static void CheckEnglishSpelling(string assemblyName)
		{
			CheckSpelling(assemblyName, Res.DefaultLanguage);
		}

		public static void CheckBritishEnglishSpelling(string assemblyName)
		{
			CheckSpelling(assemblyName, SharedConstants.Languages.EnglishBritish);
		}

		static void CheckSpelling(string assemblyName, string language)
		{
			var zrsFileName = GetZrsFileName(assemblyName);
			if (ZrsFileDictionary.Instance.Contains(zrsFileName))
			{
				var helpData = GetHelpData(language, zrsFileName);
				var errorsInfo = GetSpellingErrorsInfo(language, helpData);
				AssertSpellingErrorsInfo(language, errorsInfo);
				return;
			}

			Assertion.Assert(true);
		}

		static string GetZrsFileName(string assemblyName)
		{
			var asmName = Path.GetFileNameWithoutExtension(assemblyName);
			var id = unchecked((ushort)ZrsFile.CalculateAsmid(asmName));
			return string.Join(".", id, "xml");
		}

		static StringBuilder GetSpellingErrorsInfo(string language, HelpDataString[] data)
		{
			using (var spellingErrorHelper = SpellingErrorHelperProvider.GetHelper<HelpDataString>(
						language,
						ResourceStringDataLevels.List.ToArray().Select(level => level.Code).ToArray(),
						GetCaptionAtLevel,
						SpellCheckerDecider))
			{
				return SpellingErrorsInfo(data, spellingErrorHelper);
			}
		}

		static StringBuilder SpellingErrorsInfo(HelpDataString[] helpData,
			ISpellingErrorHelper<HelpDataString> spellingErrorHelper)
		{
			var spellingErrors = spellingErrorHelper.GetSpellingErrors(helpData);
			var errorsInfo = new StringBuilder();
			for (var i = 0; i < helpData.Length; i++)
			{
				var errorInfo = GetSpellingErrorInfo(helpData, spellingErrors, spellingErrorHelper, i);
				errorsInfo.Append(errorInfo);
			}

			return errorsInfo;
		}

		static StringBuilder GetSpellingErrorInfo(HelpDataString[] helpData, IList<ISpellingError>[][] spellingErrors,
			ISpellingErrorHelper<HelpDataString> spellingErrorHelper, int i)
		{
			var itemHadError = false;
			var dataLevels = ResourceStringDataLevels.List;
			var errorInfo = new StringBuilder();
			for (var dl = 0; dl < dataLevels.Count; dl++)
			{
				if (spellingErrors[dl][i].Count == 0)
				{
					continue;
				}

				var dataLevel = dataLevels[dl];
				errorInfo.Append(GetFileInfo(helpData[i], ref itemHadError));
				errorInfo.Append(dataLevel.Description);
				errorInfo.Append(": ");
				errorInfo.Append(TextWithSpellingErrors(spellingErrors[dl][i],
					helpData[i].GetCaptionAtLevel(dataLevel.Code)));
				errorInfo.Append("<br/>");
				errorInfo.Append(GetSuggestions(spellingErrors[dl][i], dataLevel.Code, spellingErrorHelper));
				errorInfo.Append("<br/>");
			}

			if (itemHadError)
			{
				errorInfo.AppendLine("<br/>");
			}

			return errorInfo;
		}

		static void AssertSpellingErrorsInfo(string language, StringBuilder errorsInfo)
		{
			if (errorsInfo.Length == 0)
			{
				Assertion.Assert(true);
				return;
			}

			errorsInfo.Insert(0, "Spelling errors found in resource strings:<br/><br/>\r\n\r\n");
			if (language != SharedConstants.Languages.EnglishBritish)
			{
				errorsInfo.Append(RuleDescription);
			}

			AssertionWithHtml.HtmlFail(errorsInfo.ToString());
		}

		static HelpDataString[] GetHelpData(string language, string zrsFileName)
		{
			var resourceStringKeyList = ZrsFileDictionary.Instance.GetResourceStringKeys(zrsFileName);
			var checkKeys = resourceStringKeyList.Where(ShouldCheck);
			HelpDataString[] data;
			using (ResourceStringsFactory.HoldCacheReferences())
			{
				data = checkKeys.Select(key => ResourceStringsFactory.LookupWithLanguageFallback(language, key))
					.ToArray();
			}

			return data;
		}

		static StringBuilder GetSuggestions(IList<ISpellingError> errors, string dataLevelCode,
			ISpellingErrorHelper<HelpDataString> spellingErrorHelper)
		{
			var builder = new StringBuilder();
			var handledWords = new HashSet<string>();
			foreach (var error in errors)
			{
				var suggestions = spellingErrorHelper.GetSuggestions(error, dataLevelCode);
				if (handledWords.Contains(error.Word.ToLower()) || suggestions.Count <= 0)
				{
					continue;
				}

				AppendSuggestions(builder, error, suggestions);
				handledWords.Add(error.Word.ToLower());
			}

			return builder;
		}

		static void AppendSuggestions(StringBuilder builder, ISpellingError error, IList<string> suggestions)
		{
			builder.Append("Suggestions for <i>").Append(error.Word).Append("</i>: ");
			builder.Append(suggestions[0]);
			for (var i = 1; i < suggestions.Count; i++)
			{
				builder.Append(", ").Append(suggestions[i]);
			}

			builder.AppendLine("<br/>");
		}

		static StringBuilder TextWithSpellingErrors(IEnumerable<ISpellingError> errors, string text)
		{
			var builder = new StringBuilder();
			var textPosition = 0;
			foreach (var error in errors)
			{
				builder.Append(text.Substring(textPosition, error.WordIndex - textPosition));
				builder.Append("<span style=\"font-weight:bold;color:#ff0000;\">").Append(error.Word).Append("</span>");
				textPosition = error.WordIndex + error.Word.Length;
			}

			builder.Append(text.Substring(textPosition));
			return builder;
		}

		static StringBuilder GetFileInfo(HelpDataString item, ref bool itemHadError)
		{
			var builder = new StringBuilder();
			if (itemHadError)
			{
				return builder;
			}

			if (!item.HD_ContextSourceFile.IsEmpty)
			{
				builder.AppendFormat("File: <a href='vsnet:{0}#{1}'>{0}: line {1}</a>", item.HD_ContextSourceFile,
					item.HD_ContextSourceFileLine);
			}
			else
			{
				builder.AppendFormat("Class: {0}", item.HD_ContextClassName);
			}

			builder.AppendFormat("<br/>Key: {0}<br/>", item.HD_Code);
			itemHadError = true;
			return builder;
		}

		static string GetCaptionAtLevel(HelpDataString dataString, string dataLevel)
		{
			return dataString.GetCaptionAtLevel(dataLevel);
		}

		static ISpellChecker SpellCheckerDecider(string dataLevel, ISpellChecker captionSpellChecker,
			ISpellChecker fullDescriptionSpellChecker)
		{
			return dataLevel == ResourceStringDataLevels.Codes.FullDescription
				? fullDescriptionSpellChecker
				: captionSpellChecker;
		}

		static bool ShouldCheck(string key)
		{
			return !(PrefixesFromSpellCheck.Any(key.StartsWith) ||
					KeyPrefixesToExclude.Any(key.StartsWith));
		}

		public static IEnumerable<string> KeyPrefixesToExclude
		{
			get
			{
				yield return "GF_Summary$";
				yield return "EConversationMessageEmailTemplate"; // Email template with HTML
			}
		}

		static IEnumerable<string> PrefixesFromSpellCheck
		{
			get
			{
				yield return "RN_Desc$"; // Country names
				yield return "RW_Description$"; // State names

				yield return "RX_Desc$"; // Currencies
				yield return "RX_UnitName$";
				yield return "RX_SubUnitName$";

				yield return "FZ_Description$";
				yield return "GF_Summary$";

				yield return "SU_MenuName_ACT$"; // Operational actions

				yield return
					"75dd0e63-7a98-4da2-b95f-0b016151e7a8"; // UnmatchedOrgDetails Note Type, ignore to avoid StmNote data transform

				yield return "9be29c2d-30c5-4e1b-89c2-328dcdca5608"; // Macros in the registry

				yield return
					"f4384e5b-7c6d-44c1-9282-afbaba3bb2f3"; // Exception Message : Duplicated Composite Keys(ProviderCode-LevelCode): {0}

				yield return "B8CB5880-5B7F-4846-83E0-D56B4BF296FF"; // Registry Help Text contains sample code snippet

				yield return "AC_Desc$"; // Charge Code descriptions

				yield return "eb0a8d3d-9b17-4bbe-a6ca-44a2a9d7b684"; // eDocs External storage access key/value pair

				yield return
					"SU_MenuName$ZVRlcm1pbmFsIFJlbGVhc2UgTWFuaWZlc3Q"; // Document Name : eTerminal Release Manifest

				foreach (var format in DateTimeFormatStrings.AllDateTimeFormats)
				{
					yield return ((ResourceString)format).ResourceKey;
				}
				foreach (var format in DateTimeOffsetFormatStrings.AllDateTimeFormats)
				{
					yield return ((ResourceString)format).ResourceKey;
				}

				yield return "B1B77987-6A32-40FC-89CA-96A6A9E0FA5B"; // Registry Help Text contains sample code snippet

				yield return "LegacyDocLabel|Customs+EU+SADH C88"; // Customs EU SADH C88 document
				yield return "LegacyDocLabel|Customs+EU+ESS and ESSLoI"; // Customs ESS and ESSLoI document
				yield return "LegacyDocLabel|Customs+EU+SAD Copy 3"; // Customs SAD Copy 3 document
				yield return "LegacyDocLabel|Customs+EU+SADH C88 Plain Paper"; // Customs SADH C88 Plain Paper document
				yield return "LegacyDocLabel|Customs+EU+TSAD and LoI"; // Customs TSAD and LoI document
				yield return "ReportName|Sad3"; // Customs SAD Copy 3 document
				yield return "LegacyDocLabel|Customs+ES+SADExport"; // Customs ES SADExport document

				yield return "ReportName|SadC88"; // Customs EU SADH C88 document
				yield return "Form|EUR1Certificate"; // Customs EU EUR1 Certificate Form

				yield return
					"CusAuthorizationHeaderTypeList|AuthorizedWeighersOfBananas"; // Customs EU.CusAuthorizationHeaderTypeList
				yield return "UNDGSubstancesRIDForm|223b5da7-3fb4-e9aa-4e90-0ef2b0ed1ba2"; // UNDG RID Dataset Label
				yield return "UNDGSubstanceADNForm|89545eb1-5a5a-2db4-4a83-068acea97c57"; // UNDG ADN Dataset Label

				yield return "LegacyDocLabel|Customs+EU+TAD and LoI"; // Customs EU TAD and LoI
				yield return
					"NctsCargoDescFeeMethodOfCalculationList|Hectokilogram"; // Customs IT NctsCargoDescFeeMethodOfCalculationList
				yield return "xT"; // xTMessaging Registry Class

				yield return "LegacyDocLabel|Customs+EU+EAD and ELOI"; // Customs EU EAD and ELOI
				yield return "ReportName|EAD and ELoI"; // Customs EU EAD and ELOI
				yield return "ReportName|ESS & ESSLoI"; // Customs Export/Security SAD (ESS) and ESSLOI document
				yield return "ReportName|Libro Giornale"; // ComplianceReport Libro Giornale document

				yield return "DD480B1A-13A2-47B1-B65C-829A56DC3B5B"; // Customs JP
				yield return "JPJobDeclaration|JE_PaymentDeadlineExtension"; // Customs JP

				yield return "JP.Business.JobComInvoiceLine|JI_TradeControlOrderAppendix"; // Customs JP

				yield return "TaxTypeList|ECImportTaxes"; //Customs PL ECImportTaxes Description

				yield return "ImportPLOfficeCode|CheckRuleR1586"; //Customs PL Office R1586 Description

				yield return "NctsGuarantee|GuaranteeNotRequiredForTheJourneyBetweenOodepAndOotra"; //Customs EU
				yield return "140f27f0-445b-4f72-b83b-fd6212eb9c54"; //COLS

				yield return "CusEntryInstruction|CEI_LoadingConfirmationIsRequired"; //Customs JP
				yield return "JP.Business.CusClassPartPivot|CI_TradeControlOrderAppendix"; //Customs JP
				yield return "Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_CustomsOfficeForSpecialDeclarations"; //Customs JP
				yield return "Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_CustomsOfficeDepartmentForSpecialDeclarations"; //Customs JP

				yield return "USSpell"; // American Spelling
				yield return "ReportLabel|EU IST Accounting Report"; // EU IST Accounting Report
				yield return "EConversationMessageEmailTemplate"; // Email template with HTML

				yield return
					"038BAAA1-A72C-4D3C-886E-2892247A7765"; //contains tariff ‘Customs Favour Hint Code’ attribute name
				yield return "7DEA1329-10A6-4CC0-84CA-057CCD848CB9"; //contains 'Customs Favour Code'
				yield return "0C430FFD-C49F-4312-9A61-B37D5B1F14D3"; //contains 'Customs Favour Code'

				yield return "6F0695A6-84BA-49CC-9554-1F233DAA129D"; // contains check sum digit validation for Norway
				yield return "E1E9D9952-21EF-404E-8D33-CA8D4018C0EB"; // contains check sum digit validation for Norway
				yield return "7AB87982-285C-4A70-BF9D-5EA093AFC1D9"; // Abbreviation for 'Postcode'
				yield return "FBE987CD-54AE-43BD-8180-6A5A2507243A"; // Quarantine Features: Color/Characteristics/Botanical Nomenclature

				yield return "SU_MenuName$UmVnaXN0cm8gSVZBIChBUkMvQVBDKQ=="; // SU_MenuName = Registro IVA (ARC/APC), SU_PK=3bd29369-06c1-44cc-a763-fc15f4b22a81
				yield return "SU_MenuName$T3JnYW5pemF0aW9uIC0gTGlzdGluZyBvZiBFeHBvcnRlciBFeGVtcHRpb24gRG9jdW1lbnRz"; //SU_PK=388959a7-088f-450c-928d-2c4a5a6aff65
				yield return "SU_Hint$/semeXB+e6yMjqBxaHeBvg==#VGhpcyByZXBvcnQgd2lsbCBsaXN0IGFsbCAnRVhWJyBkb2N1bWVudCBpc3N1ZSBpbiBJdGFseSAoSVQpIHRoYXQgZXhwaXJlZCB3aXRoaW4gdGhlIHNwZWNpZmllZCBkYXRlIHJhbmdlLiBUaGlzIHJlcG9ydCBjYW4gb3B"; //SU_PK=388959a7-088f-450c-928d-2c4a5a6aff65
				yield return "SU_Hint$qGVR2Xt+JI1uAF67ePShAQ==#VGhpcyByZXBvcnQgd2lsbCBzdW1tYXJpc2UgYW5kIGdyb3VwIEFMTCBBUiBJbnZvaWNlIGFuZCBBUiBDcmVkaXQgTm90ZSB0cmFuc2FjdGlvbnMgcG9zdGVkIHdpdGhpbiBhIHNlbGVjdGVkIHBlcmlvZC9zIGludG8gdHd"; //SU_PK=f18e822c-20c7-4228-af61-1546c0dba0ad
				yield return "SU_Hint$RrL4nf4Oq15NDjL8LMUtxg==#VGhpcyByZXBvcnQgY29uc2lzdHMgb2YgdHdvIG9wdGlvbmFsIHRlbXBsYXRlcy4NCjEuIFRoZSBmaXJzdCBvcHRpb25hbCB0ZW1wbGF0ZSBsaXN0cyBmb3IgYSBub21pbmF0ZWQgcGVyaW9kIG9yIGRhdGUgcmFuZ2UsIGF"; //SU_PK=15542a26-e290-4ffb-8f32-ad4dacce8e50
				yield return "SU_MenuName$TGlicm8gR2lvcm5hbGU="; //SU_PK=127708ff-dad7-45ce-88b3-fd6cf6623ace
				yield return "SU_MenuName$UmllcGlsb2dvIExpcXVpZGF6aW9uZSBJVkE="; //SU_PK=41271964-41b7-4084-b378-70a8f3c1e44b
				yield return "SU_MenuName$UmVnaXN0cm8gSVZBIEFjcXVpc2l0aQ=="; //SU_PK=f8014fdd-2137-4165-af9b-f61fe1ac2db7
				yield return "SU_MenuName$UmVnaXN0cm8gSVZBIFZlbmRpdGU="; //SU_PK=cf8658fe-f6cd-44c1-94ec-d536fdbf1dd1
				yield return "D7A526F5-CF55-483F-BFCC-8F6140FE6D8D";
			}
		}

		[SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp")]
		const string RuleDescription = @"<p><strong><font size='4'>Spelling Rules</font></strong></p>
<p><strong><font size='4'>Please be sure your English resource strings conform to the following rules.</font></strong></p>
<p><strong><font size='2'>American Spelling</font></strong></p>
<ul>
<li><font size='2'><em>Organization</em>, not <em>Organisation</em>; <em>Authorize</em> instead of <em>Authorise</em></font></li>
<li><font size='2'><em>License</em>, not <em>Licence</em></font></li>
<li><font size='2'><em>Canceled</em> rather than <em>Cancelled</em></font></li>
<li><font size='2'><em>Labor</em> and <em>Color</em> instead of <em>Labour</em> and <em>Colour</em></font></li>
<li><font size='2'><em>Meter</em> instead of<em> Metre</em></font></li>
<li><font size='2'>etc.</font></li>
</ul>
<p><strong><font size='2'>Abbreviations</font></strong></p>
<font size='2'>Captions can be shortened with abbreviations such as using <em>Org.</em> to mean <em>Organization</em> or <em>Desc.</em> to mean <em>Description</em>. A special dictionary is used at CargoWise to define allowable abbreviations.</font>
<ul>
<li><font size='2'>Abbreviations are not allowed in the Full Description field, use the full word here instead.</font></li>
<li><font size='2'>Abbreviations must end with a period. <em>Org.</em> not <em>Org</em></font></li>
<li><font size='2'>Acronyms and abbreviations are not the same thing</font></li>
</ul>
<p><font size='2'><strong>Acronyms and Initialisms</strong></font></p>
<ul>
<li><font size='2'>Acronyms and initialisms are in all capital letters: <em>XML</em> not <em>Xml</em></font></li>
<li><font size='2'>It is preferred that acronyms and initialisms are defined in the Full Description field, however this is not currently validated by the spell checker.</font></li>
</ul>
<p><strong><font size='2'>Compounding</font></strong></p>
<ul>
<li><font size='2'>Space between words should not be removed to shorten a caption: <em>WaitPoint</em> is not a word, use <em>Wait Point</em> instead.</font></li>
<li><font size='2'>Valid compound words should not be camel cased: <em>Postcode</em> not <em>PostCode</em></font></li>
</ul>
<p><strong><font size='2'>E.g. and I.e.</font></strong></p>
<ul>
<li><font size='2'>Use <em>e.g.</em>, not <em>eg</em>, to mean &quot;for example&quot;</font></li>
<li><font size='2'>Use <em>i.e.</em>, not <em>ie</em>, to mean &quot;that is&quot;</font></li>
</ul>
<p><strong><font size='2'>URLs and File Paths</font></strong></p>
<font size='2'>Spell checker cannot guarantee your URL will not contain spelling errors and parsing URLs is bad, and URLs/File paths shouldn't be translated anyway. Instead, use string formatting wherever you need a URL or file path.</font>
<ul>
<li><font size='2'>e.g. for <code>GetString()</code> use <code>Res.GetString(&quot;00000000-0000-0000-0000-000000000000&quot;, &quot;{0}&quot;, &quot;www.someurl.com&quot;)</code></font></li>
<li><font size='2'>e.g. for <code>GetMultilingualString()</code> use the above</font></li>
<li><font size='2'>e.g. for <code>GetData()</code> use <code>Res.GetData(&quot;00000000-0000-0000-0000-000000000000&quot;, &quot;{0}&quot;, &quot;Some caption&quot;, &quot;Some description {0}&quot;).Format(&quot;www.someurl.com&quot;)</code></font></li>
<li><font size='2'>e.g. for DocBuilder Templates use <code>&lt;UrlHyperlink(www.someurl.com, Some text, Some tooltip)&gt;</code> see <a href='https://wisetechglobal.sharepoint.com/Development/Development%20Team%20Workspace/Shared%20Documents/Development%20Productivity/Share%20folder/Document%20Engine%20Reference.PDF'>Document Engine Reference</a> for more details</font></li>
</ul>
<p><strong><font size='2'>Code Terms</font></strong></p>
<font size='2'>See URLs and File Paths. Code Terms refer to &quot;words&quot; that mean something in the context of the program but aren't really valid English. Typically they are a multiple words without spaces, such as variable names.</font>
<p>
</p>
<p><font size='2'></font></p>
<strong><font size='3'>Spelling Dictionaries</font></strong><p></p>
<p><font size='2'>If your resource string contains a valid English term that conforms to the above rules but is still flagged as a spelling error, you may add it to the appropriate spelling dictionary under&#160;CWShared\SpellCheck\SpellCheck\Resources\Enterprise</font></p>
<p><font size='2'><strong>Dictionaries</strong></font></p>
<ul>
<li><font size='2'>en.Enterprise.dic - spelling dictionary for CargoWise One - add your term here if it is a valid English word that&#160;has the same Australian/American/British spelling</font></li>
<li><font size='2'>en_AU.Enterprise.dic - Australian/British spelling specific dictionary</font></li>
<li><font size='2'>en_US.Enterprise.dic - American spelling specific dictionary</font></li>
<li><font size='2'>en.Enterprise-Abbreviations.dic - a list of accepted abbreviations and the word each represents</li>
<li><font size='2'>en.Enterprise-Expressions - multiword expressions, add an expression here if it is only valid when used in the entire expression</font></li>
<li><font size='2'>Enterprise-CodeTerms.dic - obsolete, do not edit</font></li>
</ul>
<font size='2'></font>&#160;<strong style=‘font-size&#58;small;’>Dictionary Entry Format</strong>
<ul>
<li><font size='2'>Case - dictionary entries&#160;should be all lower case except for proper nouns that must always be capitalized. Acronyms are always all upper case&#160;and do not need to be added to any dictionary currently.</font></li>
<li><font size='2'>Plural and other forms - only add the root word to the dictionary and use the morphology flags, such as /s for plural with s,&#160;to allow the spell checker to accept other forms.</font></li></ul>";
	}
}
