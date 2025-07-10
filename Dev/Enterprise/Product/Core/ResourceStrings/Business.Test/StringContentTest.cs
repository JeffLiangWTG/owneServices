using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.BuildTools;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Cache.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Mono.Cecil;
using NUnit.Framework;
using WTG.SpellCheck;

namespace Enterprise.ResourceStrings.Business.Testing
{
	public class StringContentTest : TestCase
	{
		[FrequentlyFailing]
		public void TestTranslationParameterConsistency()
		{
			var humanReadableErrorBuilder = new StringBuilder();
			foreach (var language in DataFile.GetAvailableLanguages())
			{
				var translations = ResourceStringCacheBuilder.Instance.GetResourceStringResearch(language);
				foreach (var key in translations.AllKeys)
				{
					var defaultString = GetCaption(LookupDefaultString(key));
					var translationString = GetCaption(translations.Get(key));
					if (translationString != null && defaultString != null)
					{
						var notifications = new NotificationCollection();
						ParameterConsistencyChecker.Check(defaultString, translationString, notifications);
						if (notifications.HasNotifications())
						{
							var errorBuilder = new StringBuilder();
							errorBuilder.AppendFormat("{0} resource string with key '{1}' has problems with parameter formatting.\r\n{2}\r\n{3}\r\n", language, key, defaultString, translationString);
							foreach (var notification in notifications)
							{
								errorBuilder.AppendLine(notification.Message);
							}
							humanReadableErrorBuilder.AppendLine(errorBuilder.ToString());
							ResourceStringContentTestTracker.LogFailure(language, key, errorBuilder.ToString(), defaultString, translationString);
						}
					}
				}
			}
			if (humanReadableErrorBuilder.Length > 0)
			{
				Fail(humanReadableErrorBuilder.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		static string GetCaption(ResourceStringData data)
		{
			return data?.GetCaptions().FirstOrDefault();
		}

		public void TestFullDescriptionDiffersFromCaption()
		{
			var errors = new StringBuilder();
			foreach (var data in ((SimpleResourceStringCache)ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(Res.DefaultLanguage)).Source.ReadAll())
			{
				if (!string.IsNullOrEmpty(data?.FullDescription) && !string.IsNullOrEmpty(data.Caption) && (data.FullDescription.Equals(data.Caption, StringComparison.InvariantCultureIgnoreCase) || data.FullDescription.Equals(data.Caption + ".", StringComparison.InvariantCultureIgnoreCase)))
				{
					errors.AppendLine(data.Key + ": " + data.Caption);
				}
			}
			if (errors.Length > 0)
			{
				errors.Insert(0, "The following Resource Strings have a FullDescription that is the same as the Caption. Enter a valuable FullDescription or none at all.\r\n\r\n");
				Fail(errors.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		[FrequentlyFailing]
		public void TestNoDuplicateKeys()
		{
			CombineAssertions(delegate
			{
				foreach (var langauge in DataFile.GetAvailableLanguages())
				{
					var keys = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
					var duplicated = new List<string>();
					foreach (var data in ((SimpleResourceStringCache)ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(langauge)).Source.ReadAll())
					{
						if (keys.Contains(data.Key))
						{
							duplicated.Add(data.Key);
						}
						else
						{
							keys.Add(data.Key);
						}
					}
					if (duplicated.Count > 0)
					{
						if (langauge == Res.DefaultLanguage)
						{
							Fail("The following resource keys are used in two different assemblies:\r\n" + string.Join("\r\n", duplicated.ToArray()));
						}
						else
						{
							Fail("The following resource keys are translated twice in " + langauge + ":\r\n" + string.Join("\r\n", duplicated.ToArray()));
						}
					}
					else
					{
						Assert(true);
					}
				}
			});
		}

		public void TestResourceStringsAnalyserIsWorking()
		{
			var readonlyResourceStrings = ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(Res.DefaultLanguage);
			Assert("There should be more than 25000 readonly strings, actual amount of readonly strings: " + readonlyResourceStrings.AllKeys.Count(), readonlyResourceStrings.AllKeys.Count() > 25000);

			// this is a mostly random list of strings that appear accross a few different modules:
			AssertReadOnlyString(readonlyResourceStrings, "7b37fba6-f0ca-4168-84e4-deb03abe3558", "The {0} must be after {1}.");
			AssertReadOnlyString(readonlyResourceStrings, "713ca8e1-5356-47a1-a9c3-0f7ecec9041f", "You must set up the Email Notification Group in order for errors to be sent");
			AssertReadOnlyString(readonlyResourceStrings, "72769dc8-2d24-4159-92e5-acf29ae1edb0", "Direct Receipt");
			AssertReadOnlyString(readonlyResourceStrings, "702dba13-4280-48d2-89d3-a18b20b42d86", "Container Event Import Notification Group");
			AssertReadOnlyString(readonlyResourceStrings, "78bc4b82-3da9-46b5-8074-206531d4623e", "Allow Payables Match Date To Be Back Dated");
			AssertReadOnlyString(readonlyResourceStrings, "717e2fea-f5b5-42b2-adb6-adce8600fdc4", "Notify Party Contact");
			AssertReadOnlyString(readonlyResourceStrings, "70cf1c1f-0df6-4131-85d2-4242d56749c0", "Difference between Invoices and Accruals totals exceeds maximum value set.");
			AssertReadOnlyString(readonlyResourceStrings, "72fb01b6-67dd-472b-bda6-e3f9d8c5bc0e", "Please create a new shipment before importing pack lines.");
			AssertReadOnlyString(readonlyResourceStrings, "7e6f1c73-0149-48ce-9e43-823957542993", "Shipment with House Bill '{0}' not found.");
			AssertReadOnlyString(readonlyResourceStrings, "73765901-c32e-4493-8991-47031b70a06c", "Export Broker");
			AssertReadOnlyString(readonlyResourceStrings, "78bf80bc-712c-4c56-9a3a-4be88da084d4", "Show all legs with and without dangerous goods");
			AssertReadOnlyString(readonlyResourceStrings, "7b3ba7b1-07f2-423b-86be-c0fc9818526d", "Service Level");
			AssertReadOnlyString(readonlyResourceStrings, "52846125-9637-4630-9fcb-1ede8812f699", "This is a stand-alone or template task.");
			AssertReadOnlyString(readonlyResourceStrings, "77edc2c4-74d5-4640-8bd6-20c70026dfdc", "Package Volume cannot be less than 0.");
			AssertReadOnlyString(readonlyResourceStrings, "702461a6-b410-41c7-bfa2-7a30ff13d5fd", "Accept");
			AssertReadOnlyString(readonlyResourceStrings, "7d0a8b3d-fb26-4e10-b4e5-cb474fd5e9e5", "Payment Term");
			AssertReadOnlyString(readonlyResourceStrings, "7dd0c1e0-0135-48c7-a515-d8e1fbe7ac4a", "View Receipt");
			AssertReadOnlyString(readonlyResourceStrings, "ProcessTaskNotification|PQ_FieldName", "Field Name");
			AssertReadOnlyString(readonlyResourceStrings, "NPBO:Enterprise.Freight.Forwarding.Business.AWB.AWBActions|PrintBarcodeLabel", "Print Barcode Labels");
			AssertReadOnlyString(readonlyResourceStrings, "AutoratedValueType|ClientRate", "Client Rate"); // ResourceStringData on enum
		}

		public void TestWarehouseRFResourceStringsAnalyserIsWorking()
		{
			var readonlyResourceStrings = ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(Res.DefaultLanguage);
			AssertReadOnlyString(readonlyResourceStrings, "56c9348d-afed-4aed-875f-b121b4069628", "Warehouse RF");
		}

		public void TestXamlContentAnalyserIsWorking()
		{
			var readonlyResourceStrings = ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(Res.DefaultLanguage);
			var count = readonlyResourceStrings.AllKeys.Count(key => key.StartsWith("xc|"));
			Assert("There should be more than 10 XAML strings, actual amount of readonly strings: " + count, count > 10);
			AssertReadOnlyString(readonlyResourceStrings, "xc|CargoWise.NetworkVisualisation.GUI|Q2hhbmdlIHRoZSB6b29tIGxldmVsIG9mIHRoZSBjb250ZW50", "Change the zoom level of the content");
			AssertReadOnlyString(readonlyResourceStrings, "xc|CargoWise.NetworkVisualisation.GUI|Rml0", "Fit");
			AssertReadOnlyString(readonlyResourceStrings, "xc|CargoWise.NetworkVisualisation.GUI|Rml0IGFsbCBub2RlcyB0byB0aGUgdmlldy1wb3J0", "Fit all nodes to the view-port");
			AssertReadOnlyString(readonlyResourceStrings, "xc|CargoWise.NetworkVisualisation.GUI|RmlsbA==", "Fill");
			AssertReadOnlyString(readonlyResourceStrings, "xc|CargoWise.NetworkVisualisation.GUI|U2NhbGUgdGhlIGNvbnRlbnQgdG8gMTAwJQ==", "Scale the content to 100%");
			AssertReadOnlyString(readonlyResourceStrings, "xc|CargoWise.NetworkVisualisation.GUI|UG9wIE91dA==", "Pop Out");
			AssertReadOnlyString(readonlyResourceStrings, "xc|CargoWise.NetworkVisualisation.GUI|UmVmcmVzaA==", "Refresh");
			AssertReadOnlyString(readonlyResourceStrings, "xc|CargoWise.NetworkVisualisation.GUI|UmVtb3ZlcyB0aGlzIGRlcGVuZGVuY3kgZnJvbSB0aGUgZGlhZ3JhbS4=", "Removes this dependency from the diagram.");
			AssertReadOnlyString(readonlyResourceStrings, "xc|CargoWise.NetworkVisualisation.GUI|Wm9vbSBpbiBvbiB0aGUgY29udGVudCAoQ3RybCsrKQ==", "Zoom in on the content (Ctrl++)");
			AssertReadOnlyString(readonlyResourceStrings, "xc|CargoWise.NetworkVisualisation.GUI|Wm9vbSBvdXQgZnJvbSB0aGUgY29udGVudCAoQ3RybCstKQ==", "Zoom out from the content (Ctrl+-)");
		}

		public void TestWebContentAnalyserIsWorking()
		{
			var readonlyResourceStrings = ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(Res.DefaultLanguage);
			var count = readonlyResourceStrings.AllKeys.Count(key => key.StartsWith("wc|"));
			Assert("There should be more than 400 web tracker strings, actual amount of readonly strings: " + count, count > 400);
			AssertReadOnlyString(readonlyResourceStrings, "wc|Enterprise.Tracking.Web|TG9naW4=", "Login");
			AssertReadOnlyString(readonlyResourceStrings, "wc|Enterprise.Tracking.Web|UGFzc3dvcmQ6", "Password:");
			AssertReadOnlyString(readonlyResourceStrings, "wc|Enterprise.Tracking.Web|TWFzdGVyIEJpbGw=", "Master Bill");
			AssertReadOnlyString(readonlyResourceStrings, "wc|Enterprise.Tracking.Web|U2F2ZSBQaWNrdXAgQWRkcmVzcw==", "Save Pickup Address");
			AssertReadOnlyString(readonlyResourceStrings, "wc|Enterprise.WebCFS.Web|Q29udGFpbmVycw==", "Containers");
			AssertReadOnlyString(readonlyResourceStrings, "wc|Enterprise.WebCFS.Web|Q29udGFpbmVyIE51bWJlcjo=", "Container Number:");
			AssertReadOnlyString(readonlyResourceStrings, "wc|Enterprise.ZArchitecture.Web.GUI|U3RhcnRzIFdpdGg=", "Starts With");
		}

		public void TestServiceTaskCaptionAnalyserIsWorking()
		{
			var readonlyResourceStrings = ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(Res.DefaultLanguage);
			AssertReadOnlyString(readonlyResourceStrings, "RXZlbnQgTG9nIFdhbGtlciBNYXN0ZXIgU2VydmljZQ==", "Event Log Walker Master Service");
			AssertReadOnlyString(readonlyResourceStrings, "VGFnIFNlcnZpY2UgVGFzaw==", "Tag Service Task");
			AssertReadOnlyString(readonlyResourceStrings, "U3lzdGVtIE1lc3NhZ2UgU2VydmljZQ==", "System Message Service");
			AssertReadOnlyString(readonlyResourceStrings, "U3lzdGVtIExpY2Vuc2UgU2VydmljZQ==", "System License Service");
			AssertReadOnlyString(readonlyResourceStrings, "RGlmZmVyZW50aWFsIEJhY2t1cCBTZXJ2aWNl", "Differential Backup Service");
		}

		public void TestReportLabelAnalyserIsWorking()
		{
			var readonlyResourceStrings = ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(Res.DefaultLanguage);
			AssertReadOnlyString(readonlyResourceStrings, "ReportLabel|Client - Job And Invoiced Charge Summary for Selected Client|Sm9iIE9wZXJhdG9y", "Job Operator");
			AssertReadOnlyString(readonlyResourceStrings, "ReportLabel|Client - Job And Invoiced Charge Summary for Selected Client|VmVzc2Vs", "Vessel");
			AssertReadOnlyString(readonlyResourceStrings, "ReportLabel|CFS Container Stock Report|Q29udC4gVHlwZQ==", "Cont. Type");
			AssertReadOnlyString(readonlyResourceStrings, "ReportLabel|Client - Job And Invoiced Charge Summary for Selected Client|SW5jbHVkZSBGQ0wvQkNOIFNoaXBtZW50IFZvbHVtZXM=", "Include FCL/BCN Shipment Volumes");
			AssertReadOnlyString(readonlyResourceStrings, "ReportLabel|Communication Reporting|T3ZlcmFsbCBEaXNwb3NpdGlvbjogezB9", "Overall Disposition: {0}");
			AssertReadOnlyString(readonlyResourceStrings, "ReportLabel|Agency Client Summary Analysis Report|Sm9iIERlcHQu", "Job Dept.");
			AssertReadOnlyString(readonlyResourceStrings, "ReportLabel|Current Export Shipments By Client|QnJlYWsgQnVsaw==", "Break Bulk");
			AssertReadOnlyString(readonlyResourceStrings, "ReportLabel|Current Export Shipments By Client|Rmlyc3QgTG9hZCBFVEQ=", "First Load ETD");
			AssertReadOnlyString(readonlyResourceStrings, "ReportLabel|Receipt Listing Report|Q2FzaA==", "Cash");
		}

		public void TestClientExtensionTextIsNotExported()
		{
			var readonlyResourceStrings = ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(Res.DefaultLanguage);
			var errorBuilder = new ZStringBuilder();
			foreach (var key in readonlyResourceStrings.AllKeys)
			{
				var stringData = LookupDefaultString(key);
				var metaData = stringData.MetaData;
				if (metaData != null && metaData.ContextClassName.StartsWith("ZClient"))
				{
					errorBuilder.AppendFormat("Key: {0}, Caption: {1}, ContextClassName: {2}.\r\n", key, stringData.Caption, metaData.ContextClassName);
				}
			}

			if (errorBuilder.Length > 0)
			{
				errorBuilder.Prepend("The following texts are used in client extension assemblies and should not be exported as resource string, consider to remove them.\r\n\r\n");
				Fail(errorBuilder.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		void AssertReadOnlyString(ISimpleResourceStringCache readonlyResourceStrings, string key, string caption)
		{
			var match = readonlyResourceStrings.Get(key);
			AssertNotNull("Expecting to find a string with key '" + key + "'. The string might have been removed, otherwise the resource string analyser is not working", match);
			AssertEquals("Expected caption for resource string with key '" + key + "'. The string might have been changed, otherwise the resource string analyser is not working", caption, match.Caption);
		}

		public void TestAllReadonlyStringsHaveContextSourceFile()
		{
			CombineAssertions(delegate
			{
				var readOnlyResourceStrings = ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(Res.DefaultLanguage);
				foreach (var key in readOnlyResourceStrings.AllKeys)
				{
					if (!key.StartsWith("DocLabel|")
						&& !key.StartsWith("FormLabel|")
						&& !key.StartsWith("ReportName|")
						&& !key.StartsWith("ReportTitle|"))
					{
						var data = readOnlyResourceStrings.Get(key);
						Assert($"Missing ContextClassName information for string with key '{key}'", !string.IsNullOrEmpty(data.MetaData?.ContextClassName));
					}
				}
			});
		}

		[FrequentlyFailing]
		public void TestLengthOfAllReportSheetNamesAreNotExceeded()
		{
			Assert(true);
			var lengthExceedErrorBuilder = new ZStringBuilder();

			foreach (var language in DataFile.GetAvailableLanguages())
			{
				if (language != Res.DefaultLanguage)
				{
					var translations = ResourceStringCacheBuilder.Instance.GetResourceStringResearch(language);
					foreach (string key in translations.AllKeys)
					{
						if (key.StartsWith("ReportName|"))
						{
							var defaultString = GetCaption(LookupDefaultString(key));
							var translationString = GetCaption(translations.Get(key));
							if (translationString != null && defaultString != null)
							{
								if (defaultString.Length <= ExcelSheetNameMaxLength && translationString.Length > ExcelSheetNameMaxLength)
								{
									lengthExceedErrorBuilder.AppendLine($"Language: {language}, Key: {key}, Source: {defaultString}, Translation: {translationString}");
									ResourceStringContentTestTracker.LogFailure(language, key, "The translations of the reports' sheet name cannot be longer than Excel's max length"
										, defaultString, translationString);
								}
							}
						}
					}
				}
			}

			if (lengthExceedErrorBuilder.Length > 0)
			{
				lengthExceedErrorBuilder.Prepend(
					"The translations of the reports' sheet name cannot be longer than Excel's max length, please check and fix the following translations. \r\n");
				Fail(lengthExceedErrorBuilder.ToString());
			}
		}

		public void TestResourceStringsDoNotContainsAnyInvalidCharacters()
		{
			Assert(true);
			var errorBuilder = new ZStringBuilder();

			foreach (var language in DataFile.GetAvailableLanguages())
			{
				var resourceString = ResourceStringCacheBuilder.Instance.GetResourceStringResearch(language);
				foreach (var key in resourceString.AllKeys)
				{
					var data = resourceString.Get(key);
					if (data != null)
					{
						if (data.Caption != null && data.Caption.ToCharArray().Any(c => !XmlConvert.IsXmlChar(c))
							|| data.ShortCaption != null && data.ShortCaption.ToCharArray().Any(c => !XmlConvert.IsXmlChar(c))
							|| data.MediumCaption != null && data.MediumCaption.ToCharArray().Any(c => !XmlConvert.IsXmlChar(c))
							|| data.FullDescription != null && data.FullDescription.ToCharArray().Any(c => !XmlConvert.IsXmlChar(c)))
						{
							errorBuilder.AppendLine($"Language: {language}, Key: {key}, Caption: {data.Caption}, ShortCaption: {data.ShortCaption}, MediumCaption: {data.MediumCaption}, FullDescription:{data.FullDescription}");
						}
					}
				}
			}

			if (errorBuilder.Length > 0)
			{
				errorBuilder.Prepend(
					"The resource string cannot contains any invalid xml characters, please check and fix the following resource strings. \r\n");
				Fail(errorBuilder.ToString());
			}
		}

		[FrequentlyFailing]
		public void TestNoDuplicateTranslationForReportNames()
		{
			Assert(true);
			var uniqueTranslations = new Dictionary<string, Dictionary<string, string>>();
			bool isPassed = true;

			foreach (var language in DataFile.GetAvailableLanguages())
			{
				if (language != Res.DefaultLanguage)
				{
					var translations = ResourceStringCacheBuilder.Instance.GetResourceStringResearch(language);
					foreach (string key in translations.AllKeys)
					{
						if (key.StartsWith("ReportName|"))
						{
							var defaultString = LookupDefaultString(key)?.Caption;
							var translationString = translations.Get(key)?.Caption;
							if (translationString != null && defaultString != null)
							{
								var languangeTranslationPair = language + ":" + translationString;
								if (!uniqueTranslations.ContainsKey(languangeTranslationPair))
								{
									uniqueTranslations.Add(languangeTranslationPair, new Dictionary<string, string> { { key, defaultString } });
								}
								else
								{
									if (!uniqueTranslations[languangeTranslationPair].ContainsKey(key))
									{
										isPassed = false;
										uniqueTranslations[languangeTranslationPair].Add(key, defaultString);
									}
								}
							}
						}
					}
				}
			}

			if (!isPassed)
			{
				var duplicateErrorBuilder = new ZStringBuilder();
				var messagePrefix = "Different report names should have different translation in same language, please fix these translations \r\n";
				duplicateErrorBuilder.Append(messagePrefix);
				foreach (var item in uniqueTranslations)
				{
					if (item.Value.Count > 1)
					{
						var languageAndTranslationStrArr = item.Key.Split(':');
						var language = languageAndTranslationStrArr[0];
						var translation = languageAndTranslationStrArr[1];
						var message = $"Language: {language}, Translation: {translation}, ResourceStringKeys: ";
						foreach (var keyAndSourceStringPair in item.Value)
						{
							message += keyAndSourceStringPair.Key + ";";
							ResourceStringContentTestTracker.LogFailure(language, keyAndSourceStringPair.Key, messagePrefix, keyAndSourceStringPair.Value, translation);
						}

						duplicateErrorBuilder.AppendLine(message);
					}
				}

				Fail(duplicateErrorBuilder.ToString());
			}
		}

		const int ExcelSheetNameMaxLength = 31;

		public void TestAllTranslationEntriesAreDifferentThanDefault()
		{
			var sameStrings = new List<string>();
			foreach (var language in DataFile.GetAvailableLanguages())
			{
				if (language != Res.DefaultLanguage)
				{
					var zrsFiles = ZrsFile.GetFilesForLanguage(language);
					foreach (var zrsFilePath in zrsFiles)
					{
						var entries = ZrsFile.ListEntries(zrsFilePath);
						foreach (var entryId in entries)
						{
							var translations = ZrsFile.Read(zrsFilePath, entryId);
							foreach (var translation in translations)
							{
								var defaultData = LookupDefaultString(translation.Key);
								if (defaultData != null && defaultData.Equals(translation))
								{
									sameStrings.Add(language + ": " + translation.Key);
									ResourceStringContentTestTracker.LogFailure(language, translation.Key, "The translation resource strings are the same as the default resource strings and should be deleted",
										defaultData.Caption, translation.Caption);
								}
							}
						}
					}
				}
			}
			if (sameStrings.Count > 0)
			{
				Fail("The following translation resource strings are the same as the default resource strings and should be deleted:\r\n\r\n" + string.Join("\r\n", sameStrings));
			}
			Assert(true);
		}

		public void TestAllTranslationEntriesAreOnTheSameLevelAsDefault()
		{
			CombineAssertions(delegate
			{
				foreach (var language in DataFile.GetAvailableLanguages())
				{
					if (language != Res.DefaultLanguage)
					{
						ResourceStringsFactory.GetResourceStringCache(language);
						var translations = ResourceStringCacheBuilder.Instance.GetResourceStringResearch(language);
						foreach (var key in translations.AllKeys)
						{
							var defaultData = LookupDefaultString(key);
							var translationData = translations.Get(key);
							if (defaultData != null && translationData != null)
							{
								var defaultResourceString = HelpDataString.CreateFromResourceStringData(defaultData, Res.DefaultLanguage);
								var translationResourceString = HelpDataString.CreateFromResourceStringData(translationData, language);
								var message = string.Format("EN and {0} strings with key {1} have different caption levels", language, key);
								try
								{
									AssertEquals(message, defaultResourceString.GetCaptions().CodesAsString, translationResourceString.GetCaptions().CodesAsString);
								}
								catch (AssertionFailedError)
								{
									ResourceStringContentTestTracker.LogFailure(language, key, message, defaultData.Caption, translationData.Caption);
									throw;
								}
							}
						}
					}
				}
			});
		}

		public static IEnumerable<string> KeyPrefixesToExclude
		{
			get
			{
				yield return "GF_Summary$";
				yield return "EConversationMessageEmailTemplate"; // Email template with HTML
			}
		}

		void AppendFileInfo(HelpDataString item, StringBuilder sb, ref bool itemHadError)
		{
			if (!itemHadError)
			{
				if (!item.HD_ContextSourceFile.IsEmpty)
				{
					sb.AppendFormat("File: <a href='vsnet:{0}#{1}'>{0}: line {1}</a>", item.HD_ContextSourceFile, item.HD_ContextSourceFileLine);
				}
				else
				{
					sb.AppendFormat("Class: {0}", item.HD_ContextClassName);
				}
				sb.AppendFormat("<br/>Key: {0}<br/>", item.HD_Code);
				itemHadError = true;
			}
		}

		void AppendTextWithSpellingErrors(IEnumerable<ISpellingError> spellingErrors, string text, StringBuilder sb)
		{
			var textPosition = 0;
			foreach (var error in spellingErrors)
			{
				sb.Append(text.Substring(textPosition, error.WordIndex - textPosition));
				sb.Append("<span style=\"font-weight:bold;color:#ff0000;\">").Append(error.Word).Append("</span>");
				textPosition = error.WordIndex + error.Word.Length;
			}
			sb.Append(text.Substring(textPosition));
		}

		public void TestChequeIsUsedInBritishEnglish()
		{
			var resourceStringKeysThatShouldUseCheckNotCheque = new HashSet<string>(new[] {
				"5166e6aa-9d16-4dda-a3ad-71d3420a5b85",
				"d7cdbaa3-5057-467e-879b-0dbda3e24772",
				"17380c1c-f486-4901-8cc6-039bd74fab3c",
				"GlbGroupForm|1bd7f953-8b33-433e-8d3f-82a9f1147724",
				"EmailDiagnosticsForm|84030cd1-cffc-415c-b1c2-d0438fb3b0dd",
				"f905b728-eef1-49fa-a3a3-d03851fa1e15",
				"f6f279f2-3714-46e0-a303-a948093b44b6",
				"Freight|ShipmentInspectionType|VCK",
				"Freight|ShipmentInspectionType_JP|VCK",
				"b5154feb-bb72-4bb4-a6ca-551dec3759cb",
				"fef68037-8924-4a52-b469-b605065c509a",
				"c1ba1bfa-69d7-47da-87c7-b8b3971d658c",
				"672DC88A-C75E-41F5-9C9D-4E0F37392C53",
				"68ff85e9-5cc1-4257-a549-e4cfe97954f6",
				"ImportMessageStatusList|ClearDeparturePartialAmendment",
				"ImportMessageStatusList|ClearDeparturePartialOriginal",
				"ImportMessageStatusList|ClearDeparturePartialWithdraw",
				"e741be52-ede4-49e6-bcc1-160a4470f3c9",
				"9f6a8c55-889e-4ed6-a42c-ffbc700c03a6",
				"b4ce8957-d65f-4dcf-b42d-5032d5376a16",							//Credit Controlled Documents Check Registry
				"ScreeningMethodsJP|VisualCheck",
				"ScreeningMethods|VisualCheck",
				"AdditionalScreeningMethods|VisualCheck",
				"4a9da26a-a792-46f9-8232-5ff9cc8fafaf",
				"daa90fa2-c65b-4d3f-b1e5-6a1f56e7611e",
				"7ddfc1ba-fcc2-4ffe-9902-96f25c08371f",
				"c4bfbd87-77e0-417c-9df1-a61e8f8fcfd0",
				"d4917218-63b9-4a91-9a80-75ef02680c24",
				"e607d415-ff56-4e85-b88c-f73d607a1383",
				"f5759ee6-63c8-48dc-a857-c225485016a1",
				"fe2686de-270a-4d6e-8af2-160e5b610b81",
				"SU_MenuName$Q2hlY2sgRXF1YWxpemVkIENvbnRyYWN0cw==",
				"3D52C744-DEAE-4307-83B1-5C4D9A25E297",
				"ScreeningMethodsSG|VisualCheck",
				"f2455b00-edb9-4076-bd89-2c96313572ae",
				"0b64dcf8-3ebf-4080-a5ed-da9065643778",
				"DocLabel|VW5rbm93biBDYXJnbzogaGFzIHVuZGVyZ29uZSBhIHNlY3VyaXR5IGNoZWNrLg==",
				"DocLabel|VGhpcyBkb2N1bWVudCBwcm92aWRlcyBldmlkZW5jZSBvZiBzZWN1cml0eSBjaGVjayBhbmQgdmVyaWZpY2F0aW9uIG9mIGNhcmdvIHRvIGJlIHVwbGlmdGVkIGJ5IGFpci4=",
				"ReportTitle|GB CCSUK Bond Check",
				"ReportName|Bond Check",
				"ReportLabel|GB CCSUK Bond Check|Q0NTVUsgLSBCb25kIENoZWNr",		// CCSUK - Bond Check
				"ReportLabel|Import Bill Of Lading Report|RHV0eSBDaGVjaw==",	// Duty Check
				"ReportLabel|Import Container Report|RHV0eSBDaGVjaw==",			// Duty Check
				"ReportLabel|Import Declaration Report|RHV0eSBDaGVjaw==",		// Duty Check
				"ReportLabel|GL Balance Sheet Multilingual Report|UGxlYXNlwqBjaGVja8KgQWNjb3VudGluZz5GcmFtZXdvcms+UmVwb3J0wqBPcmRlcj5SZXBvcnTCoE9yZGVywqBSZWdpc3RyecKgc2V0dGluZy4=",		//Please check Accounting>Framework>Report Order>Report Order Registry setting.
				"ReportLabel|GL Profit And Loss Mulitlingual Report|UGxlYXNlwqBjaGVja8KgQWNjb3VudGluZz5GcmFtZXdvcms+UmVwb3J0wqBPcmRlcj5SZXBvcnTCoE9yZGVywqBSZWdpc3RyecKgc2V0dGluZy4=",		//Please check Accounting>Framework>Report Order>Report Order Registry setting.
				"ReportLabel|AR Profile Report|UUEgQ2hlY2s=",					// QA Check
				"ReportName|Hot Checks Transaction Listing",
				"674ae115-2892-4c30-91f1-dd2a59c21b23",
				"e0f2259a-77f9-4ea1-8c50-848594a97843",
				"db72e39f-5630-472f-b589-a13b44c670ee",							// Visual Check
				"ScreeningMethodsZA|VisualCheck",								// Visual Check
				"SpellCheck|6e892f85-5b7a-426a-bb45-75a204aaf4a3",				// 'Check' as in 'SpellCheck'
				"SpellCheck|EE95189C-6A49-44BD-BA66-8E86E41EDA1D",				// Enable Spellcheck
				"SpellCheck|30BE773A-0DAC-4AEE-8619-829D6F74AFDA",				// Disable Spellcheck
				"9688ef86-b337-41cf-9567-c835a6feaf14",							// Credit Check
				"8c425b60-f953-46e3-a5e4-1dec4e4de9d5",							//Last License Check
				"GlbReleaseNoteEditForm|24f7d6b6-63d8-494e-9eb3-4e1e8974c298",	//Check In
				"IncidentMain|IM_PatchTo",										//Check In To Release
				"WhsPickingUserControl|9FCC84D6-532F-4706-B619-26DEBF61A4F8",
				"e62ac25b-5c61-4eaa-8425-107dc0245f01",
				"78fdc376-5697-4c7e-a929-b749aa987f70",							//Credit Controlled Documents Check Registry
				"34234A49-B1E4-4365-8F75-E0E8D26F632C",
				"7ac88ddb-8c8e-4687-a175-fa57b193f012",							// existing usage of US-English word 'check', which is properly translated for GB-English usage
				"ec130fb7-6d0b-4384-800f-055a2936cb69",							// existing usage of US-English word 'check', which is properly translated for GB-English usage
				"b9e84e04-4cac-4ce5-8bb2-864f68d99744",
				"810C97F2-71BC-494E-8A76-421C1C1505EA",
				"BF28830F-DE3F-4A07-B20E-2DF4DED996CF",
				"OrgCompanyData|OB_ARDoNotCheckOverdueInvoicesStatus",							// existing usage of US-English word 'check', which is properly translated for GB-English usage
				"OrgMiscServ|OM_GlobalDoNotCheckOverdueInvoicesStatus",							// existing usage of US-English word 'check', which is properly translated for GB-English usage
				"StmEvent|Desc|CCE",											//Credit Check Event. Relating to checking credit status reports.
				"SE_Desc$Q3JlZGl0IENoZWNrIEV2ZW50",								//Credit Check Event. Relating to checking credit status reports.
				"SU_MenuName$Q3VzdG9tcyBDaGVjayBDQUVEIChGUik=",
				"2a58e370-2884-48e2-a745-898688cb115f",
				"76d96e10-faff-4ad8-9e13-6a1e5799e460",
				"3C444262-20AD-4143-9646-618DFA749BCE",
				"EdiUserAgreementTypes|CreditCheckService",
				"e6ba5280-b637-446d-8a03-66501d355454",
				"6660f284-1a9d-4d9b-8343-330861aa7c5f",
				"c07de801-67aa-4f52-888e-0b2eef2841a8",		//Check E-Pay Rate
				"617c0b94-a129-4591-a7e3-9212a9b9e5c6",		//Check E-Pay Rate
				"5034b080-6b9c-44e5-90f8-381c8ac56bdd",		// Check Ex Rate for E-Payment
				"8e222354-dbab-418c-bae4-4e1fda0dd674",		// Check Ex Rate for E-Payment
				"432d6819-b1c7-46a3-a6e4-ea57319cae78",
				"ae99fe09-eccb-4965-8582-5695d1e04763",		//Check only non-security groups on removal of last staff
				"ServiceMessages|DutyGuarantee",
				"ServiceMessages|VatArticle23",
				"c5e10f9e-f906-4144-88a7-16824acd96b5",	//Receipt Type Check
				"9c392431-366d-4dfd-8783-044adb3ea9f7", //Check Or Reference
				"0d22a1e8-2a89-4aa7-a42b-ce926a7596cd", // Check as in investigate
				"65626bda-4479-4a63-8125-f9c704b7b390", // Check as in investigate
				"fe904b15-d96a-4e8f-ad7f-90e7188a2c62", // Check as in investigate
				"d7bb187b-af21-4d1e-bc9d-1fbf4c04c17a", // Check as in investigate
				"216496e5-9415-4a87-8bfc-f19bd520868f", // Check as in investigate
				"e1066367-e23b-4d90-acdd-4e4ac5fca630", // Check as in investigate
				"1E72D49E-6BD4-4871-A2E6-5EE76C310561", // Check as in investigate
				"ImportClearanceStatuses|EntryManuallyClearedWithOverrideOfInventoryCheckFailureOrWhereAnInventoryCustomsMatchIsNotEstablished",
				"0df021e4-f081-430d-9f27-d9da5f8aaf44",
				"79d1db2b-ce0a-46b2-9e22-89952d5e0c10",
				"29B2531F-8B13-45CC-9264-FD02669BB172",
				"A77C63D1-2717-4219-9D50-2B7E9A13281A",
				"137261e8-2200-49df-8855-7add6b354edb", //ES Check for inbox notifications (as in find)
				"E3C3BE86-86DB-4617-B9B9-D11742343960", //ES Check for inbox notifications (as in find)
				"58F7267E-35D2-443D-81BF-EF355EE47154", //ES Check for inbox notifications (as in find)
				"F595915C-8038-44CA-81FB-D390C3D77AE5",
				"C3838B9E-FE32-4F91-914E-DFD8037EF4DB",
				"AccHotCheque",							//Check In
				"17d316cd-f231-4f6e-9925-3fdc106d7bf8",		//Credit Controlled Documents Check Registry
				"8c8bb111-9b14-4307-83da-4db868f4b387",		//Credit Controlled Documents Check Registry
				"SU_MenuName$RGVjbGFyYXRpb24gUHJlLUNoZWNrIChDQUVEKQ==",  // FR Customs Declaration Pre-Check document
				"7c9e2cd4-85e6-440e-a6c1-9ccb4bb4b914",		// FR Customs Data Registry
				"9237e8a9-9b49-4aeb-ab61-4724b3084cff",		// FR Customs Data Registry
				"c85b3af6-fd43-4fc3-80b0-88b71f89fd0f",		// FR Customs Data Registry
				"d0452dee-4cd3-4256-8314-e23725d4ac78",		// FR Customs Data Registry
				"e5d87d1d-edee-4a6a-9d73-f20278ea03bf",		// FR Customs Data Registry
				"3F5B0382-2AD3-427A-A92F-FBA8936BE7CD",		// Authorized Location Check
				"C4E04705-2591-4D2E-9620-421B672F6C98",		// Authorized Location Check
				"182642DD-FED6-488E-B534-3DC49E294F98", // Credit Check for CLVS in CA Customs
				"1D9A7F32-E200-42D1-8463-5DE19EAA6DA2", // Credit Check for CLVS in CA Customs
				"CustomsWareEntryStatusList|Valid", // Validation Check
				"29B713A4-FC62-4F84-B193-353DAE665FAE", //ES Check transit status (as in review)
				"51B8817A-2F63-4DB7-93BB-A5C86D7A5569", //ES Check accounting status (as in review)
				"A45FE09F-531D-44B2-B1CE-AE59F901DED3", //ES EntryHeaderModule ...check accounting status (as in review)
				"B3EEFAF3-3CBF-49F8-9168-2EF2222BA5FB", //ES JobDeclarationModule ...check accounting status (as in review)
				"813323EF-F87D-41E8-AE7E-732084A5D163", // CH MessageR144abcE013ab_1: ... does not require a ... check
				"6A9E5FA1-483E-420E-B579-E85EBB1E120C", // CH MessageR144abcE013ab_2: ... does not require a ... check
				"RT_Desc$SWRlbnRpZmljYXRpb24gQ2hlY2s=", // Identification Check
				"NCTS5TypeOfControlTypes|NuclearRadioactiveMaterialCheck",
				"BEIncomingMessageSubTypes|CC537C",
				"6F0695A6-84BA-49CC-9554-1F233DAA129D", // check sum digit
				"E1E9D9952-21EF-404E-8D33-CA8D4018C0EB", // check sum digit
				"C0716TypeOfControlsCodeList|_20", // Check as in investigate
				"9502A8F5-01BA-4630-BB0F-F9FCF3043552", // CH.NCTS Check Representative
				"A7EC7AA4-43D0-4C26-BB5F-44F6F49B6D57", // CH.NCTS Check Departure
				"6a863077-79bc-4f71-9ca7-2bf08a3b5d3d", // Check Export Notification (755) Report
				"CertificateTypePairList|BK1", // Background Check
				"0993c5e3-98c0-46b4-b98b-9c8e9df70e02",
				"6618de88-ac75-41a7-8ea3-ccb5cf3ff084",
				"03E26B11-6F7D-4F2A-9243-18FE3582BAE2", // Requirement check
				"A64150C3-6AA0-4EC6-992B-F4E4015C39C4", // Requirement check
				"RS1SZXBvcnRpbmcgQ2VydGlmaWNhdGUvVG9rZW4gRXhwaXJ5IERhdGUgQ2hlY2sgU2VydmljZQ==", // ECE - "E-Reporting Certificate/Token Expiry Date Check Service"
				"VHJhbnNhY3Rpb25zIFBlbmRpbmcgZm9yIEUtUmVwb3J0aW5nIENoZWNrIFNlcnZpY2UgVGFzaw==", // ETP - "Transactions Pending for E-Reporting Check Service Task"
				"223790BC-7104-47E1-8B7C-8DCEFDAD3288", //"Risk Check in progress"
				"AD1F4F9C-11BB-48C5-91F1-C0A57F60CEBA", //"Risk Check in progress"
				"ProductReceiveWeightOrDimsCheckTypeList|DoNotCheckWeightOrDims",
				"OrgMiscServ|OM_WhsCheckPartWeightOrDimsOnReceive",
				"182AF098-976C-49C1-9B75-411AC348ADD1", // Customs EnableComplianceWise - Registry caption
				"A4ED2392-B816-4933-B57A-7DAAFB4CD150", // Customs EnableComplianceWise - Registry hint
				"39464742-5CE8-4B1B-BD84-835212086AC9", // Freight EnableComplianceWise - Registry caption
				"76888B6B-EDED-492F-BD9C-B07204E05F19", // Freight EnableComplianceWise - Registry hint
				"CCF55064-02B5-4A2C-8D68-2C3ED059D407", // LinerAgency EnableComplianceWise - Registry caption
				"ADAF20D0-E8CD-4058-B55D-B731F4D73AFF", // LinerAgency EnableComplianceWise - Registry hint
				"6a17ad8f-9a81-4cb8-8ec9-f39fc9e58a81", // Check as in investigate
				"eca3f12d-f589-4184-b045-787a0ced3db0", // Compliance check button
				"SU_Hint$+bqO850P+fJoZeSZk7G4Iw==#VGhlIEhvdCBDaGVjayBUcmFuc2FjdGlvbiBMaXN0aW5nIFJlcG9ydCB3aWxsIGxpc3QgaG90IGNoZWNrcyBhbmQgdGhlaXIgYXNzb2NpYXRlZCBkZXRhaWxzLg0KQnkgZGVmYXVsdCwgdGhpcyByZXBvcnQgb25seSBsaXN",
				"WhsLocationView|FormattedCheckDigit", // Check Digit
				"b4ce8957-d65f-4dcf-b42d-5032d5376a16",
				"1a08cedb-8aa8-4f5c-8318-731996c6fa06", // Decline Commodity Risk Check
				"F9B9CA2C-3C41-4B89-81DC-41158268BB8C", // Allow Commodity Risk Check
				"458887F1-62CD-4985-BD53-CC52D07F337B", // AllowComplianceCommodityRiskAssessment - Registry caption
				"52BE4482-ECB8-461C-BF3E-1AEF52441C25", // “Price Check Description” in JP Customs
				"AE2E2511-EBEE-41A7-909B-EC0502246561", // “Price Check” in JP Customs
				"94689568-0AF3-49BD-991B-1164C7742D22", // “Price Check” in JP Customs
				"D03C733A-6244-4A94-9DCD-13A48E73EA06",	// "Check In All Child Pieces"
				"D0A2E4F1-3C5B-4F7A-8E6C-9D1B0F5A2D3E", // "checks will need to be performed" in GB Customs
			});

			var regex = new Regex(@"(?<!\b(please|to|should|always|consistency|compliance|health|stability|seal|safety|protection|credit limit|appearance|background) )\bcheck(s?)\b(?!( (the|if|that|this|its|your|with|digit(s?)|letter|box|failed|now|(not )?performed))|(-(in|out|digit(s?)|letter))\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			var sb = new StringBuilder();
			using (ResourceStringsFactory.HoldCacheReferences())
			{
				var allKeys = ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(Res.DefaultLanguage).AllKeys;
				foreach (var item in allKeys.Select(key => ResourceStringsFactory.LookupWithLanguageFallback(SharedConstants.Languages.EnglishBritish, key)))
				{
					if (!resourceStringKeysThatShouldUseCheckNotCheque.Contains(item.HD_Code) && !KeyPrefixesToExclude.Any(key => item.HD_Code.StartsWith(key)))
					{
						var itemHadError = false;
						foreach (CodeDescriptionPair captionLevel in item.GetCaptions())
						{
							var matches = regex.Matches(captionLevel.Description);
							if (matches.Count > 0)
							{
								AppendFileInfo(item, sb, ref itemHadError);
								sb.Append(captionLevel.Code).Append(": ");
								AppendTextWithSpellingErrors(matches.Cast<Capture>().Select(capture => new SpellingError(capture.Value, capture.Index)), captionLevel.Description, sb);
								sb.Append("<br/>");
							}
						}
						if (itemHadError)
						{
							sb.AppendLine("<br/>");
						}
					}
				}
			}
			if (sb.Length > 0)
			{
				sb.Insert(0, "Use 'cheque' in British English instead of 'check' when referring to the bank document. Fix by adding a resource string in language EGB - English (British) using the resource strings module. If 'check' is correct, then check <b>that</b> your grammar is correct, or add to the list of exceptions in this test.<br/><br/>\r\n\r\n");
				HtmlFail(sb.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		public void TestWhitespace()
		{
			var sb = new StringBuilder();
			var dataLevels = ResourceStringDataLevels.List;
			var data = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_Language, Res.DefaultLanguage));
			data = Array.FindAll(data, item => item.HD_IsCheckedOut || ResourceStringsFactory.Lookup(Res.DefaultLanguage, item.HD_Code, true) == null);
			data = Array.FindAll(data, item => !item.HD_ContextClassName.StartsWith("Enterprise.Customs") && !item.HD_Code.StartsWith("ReportLabel|") && !KeyPrefixesToExclude.Any(key => item.HD_Code.StartsWith(key)));// Ignore doc builder, webtracker, customs strings, freight strings and report labels
			foreach (var item in data)
			{
				var itemHadError = false;
				foreach (CodeDescriptionPair captionLevel in item.GetCaptions())
				{
					var caption = captionLevel.Description;

					for (var i = 0; i < caption.Length; i++)
					{
						if (caption[i] == '\n' && (i == 0 || caption[i - 1] != '\r'))
						{
							AppendFileInfo(item, sb, ref itemHadError);
							sb.AppendFormat("{0} contains a new line ('\\n') character without a carriage return ('\\r'). New lines for Windows should always be '\\r\\n'", dataLevels[captionLevel.Code].Description);
							sb.Append("<br/>");
							break;
						}
					}

					if (!item.HD_Code.StartsWith("DocLabel|") && !item.HD_ContextClassName.StartsWith("Enterprise.DocumentWrappers") && !item.HD_Code.StartsWith("wc|") && !item.HD_Code.StartsWith("Enterprise.Freight|"))
					{
						if (char.IsWhiteSpace(caption[0]) || char.IsWhiteSpace(caption[caption.Length - 1]))
						{
							AppendFileInfo(item, sb, ref itemHadError);
							sb.AppendFormat("{0} has leading or trailing whitespace. Resource strings cannot start or end with whitespace characters.", dataLevels[captionLevel.Code].Description);
							sb.Append("<br/>");
						}

						var lines = caption.Replace("\r", "").Split('\n');
						for (var i = 0; i < lines.Length - 1; i++)
						{
							if (!IsTerminated(lines[i]) && IsContinuation(lines[i + 1]))
							{
								AppendFileInfo(item, sb, ref itemHadError);
								sb.AppendFormat("{0} appears to be manually wrapped as it has a non-terminated line of text:\r\n\r\n{1}\r\n\r\nDo not manually wrap sentences in resource strings, line breaks are used a segmentation markers for the purpose of translation. Text should be wrapped automatically depending on the size of the control in it is displayed.\r\nTerminate lines of text that are complete sentences with . ? ! : ; or indent lines with points or examples.", dataLevels[captionLevel.Code].Description, lines[i]);
								sb.Append("<br/>");
								break;
							}
						}
					}
				}
				if (itemHadError)
				{
					sb.AppendLine("<br/>");
				}
			}
			if (sb.Length > 0)
			{
				sb.Insert(0, "Whitespace problems found in resource strings:<br/><br/>\r\n\r\n");
				HtmlFail(sb.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		bool IsTerminated(string line)
		{
			if (line.Length > 0 && char.IsWhiteSpace(line, 0))
			{
				return true;
			}

			if (line.EndsWith("OR"))
			{
				return true;
			}

			var ignore = false;
			for (var i = line.Length - 1; i >= 0; i--)
			{
				if (line[i] == '.' || line[i] == '?' || line[i] == '!' || line[i] == ':' || line[i] == ';' || line[i] == '-' || line[i] == '=')
				{
					return true;
				}
				else if (line[i] == '{')
				{
					ignore = false;
				}
				else if (line[i] == '}')
				{
					ignore = true;
				}
				else if (char.IsLetter(line, i) && !ignore)
				{
					return false;
				}
			}
			return true;
		}

		bool IsContinuation(string line)
		{
			var ignore = false;
			for (var i = 0; i < line.Length; i++)
			{
				if (char.IsWhiteSpace(line, i) || line[i] == '-' || line[i] == '*' || line[i] == '+' || line[i] == 'o' || line[i] == '(')
				{
					return false;
				}
				else if (line[i] == '{')
				{
					ignore = true;
				}
				else if (line[i] == '}')
				{
					ignore = false;
				}
				if (char.IsLetter(line, i) && !ignore)
				{
					return true;
				}
			}
			return false;
		}

		public void TestMorphologyIsNotImplementedUsingStringFormatting()
		{
			var regex = new Regex(@"([a-zA-Z]\{[0-9]+.*?\})|(\{[0-9]+.*?\}[a-zA-Z])", RegexOptions.Compiled);
			var whiteList = new Regex(@"(\{[0-9]+.*?\}[xC][\s\{])|(\{[0-9]+.*?\}[MK]B[\s\)\.])|((UEN|UTC|WGT)\{[0-9]+.*?\}[,\:\s\)])|(\\[a-zA-Z]\{[0-9]+.*?\})", RegexOptions.Compiled);

			var errors = new StringBuilder();
			foreach (var data in ((SimpleResourceStringCache)ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(Res.DefaultLanguage)).Source.ReadAll())
			{
				if (data != null && !data.Key.StartsWith("DocLabel|") && !data.Key.StartsWith("ReportLabel|") && data.GetCaptions().Any(caption => regex.IsMatch(caption) && !whiteList.IsMatch(caption)))
				{
					errors.AppendLine(data.Key + ": " + string.Join(" / ", data.GetCaptions()));
				}
			}
			if (errors.Length > 0)
			{
				errors.Insert(0, "Do not use formatting parameters to inject whitespace into your resource string, put the whitespace directly in the string or concatenate if it as the beginning or end. Do not try to implement morphology or other grammatical constructions using string formatting parameters. This will make it difficult or impossible to translate into some non-English languages.\r\n\r\n");
				Fail(errors.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		public void TestDependencyStringsAreMerged()
		{
#if NETFRAMEWORK
			var parentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#else
			var parentDirectory = Path.GetDirectoryName((Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)));
#endif
			foreach (var depdencyFile in BuildXml.Instance.GetDependencyFiles(true))
			{
				if (ExcludedDependencyFiles.Contains(depdencyFile))
				{
					continue;
				}

				try
				{
					var fullPath = Path.Combine(parentDirectory, depdencyFile);
					var extension = Path.GetExtension(fullPath);
					if (extension.Equals(".dll", StringComparison.OrdinalIgnoreCase) || extension.Equals(".exe", StringComparison.OrdinalIgnoreCase))
					{
						using (var assembly = AssemblyDefinition.ReadAssembly(fullPath))
						{
							var assemblyIdAttribute = assembly.CustomAttributes.SingleOrDefault(a => a.AttributeType.FullName == typeof(ResourceStringAssemblyIdAttribute).FullName);
							if (assemblyIdAttribute != null)
							{
								var assemblyId = (ushort)assemblyIdAttribute.ConstructorArguments.First().Value;
								AssertNotEquals($"ResourceStringAssemblyIdAttribute found in dependency assembly {Path.GetFileName(depdencyFile)} should have a non-zero value", 0, assemblyId);
								Assert($"Resource strings in dependency assembly {Path.GetFileName(depdencyFile)} have not been merged into the main ZRS file, be sure the dependency ZRS is copied to the res subdirectory", ZrsFile.Read(ZrsFile.GetDefaultFilePath(Res.DefaultLanguage, string.Empty), assemblyId).Any());
							}
						}
					}
				}
				catch (BadImageFormatException)
				{
				}
			}
		}

		static readonly string[] ExcludedDependencyFiles = new[]
		{
			"Warehouse.RF.Core.dll"
		};

		ResourceStringData LookupDefaultString(string key)
		{
			return DefaultSystemDefinedStrings.Get(key);
		}

		ISimpleResourceStringCache DefaultSystemDefinedStrings
		{
			get { return defaultSystemDefinedStrings ?? (defaultSystemDefinedStrings = ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(Res.DefaultLanguage)); }
		}
		ISimpleResourceStringCache defaultSystemDefinedStrings;
	}
}
