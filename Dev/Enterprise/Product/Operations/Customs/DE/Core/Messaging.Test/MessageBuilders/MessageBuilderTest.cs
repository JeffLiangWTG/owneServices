using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Customs.DE.MessageContracts;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.Testing
{
	abstract class MessageBuilderTest<T, K> : TestCase
		where T : MessageBuilder<K> where K : class
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructor()
		{
			Activator.CreateInstance(typeof(T), new object[] { null });
		}

		public static void AssertXMLContainsUnformatted(string message, string expected, string actual)
		{
			AssertXMLContainsUnformattedCore(message, expected, actual, true, false);
		}
		public static void AssertXMLContainsUnformatted(string expected, string actual)
		{
			AssertXMLContainsUnformatted(string.Empty, expected, actual);
		}
		public static void AssertNotXMLContainsUnformatted(string message, string expected, string actual)
		{
			AssertXMLContainsUnformattedCore(message, expected, actual, false, false);
		}
		public static void AssertXMLNotContainsUnformatted(string expected, string actual)
		{
			AssertNotXMLContainsUnformatted(string.Empty, expected, actual);
		}

		protected Mock<IAmount> GetAmount(decimal quantity, string measurementUnit, string qualifier)
		{
			var amountMock = new Mock<IAmount>();
			amountMock.Setup(a => a.Quantity).Returns(quantity);
			amountMock.Setup(a => a.MeasurementUnit).Returns(measurementUnit);
			amountMock.Setup(m => m.Qualifier).Returns(qualifier);
			return amountMock;
		}

		static readonly Regex regex = new Regex(@">\s*<", RegexOptions.Compiled); // static, so it just compiles once. Please do not inline.
		static void AssertXMLContainsUnformattedCore(string message, string expected, string actualContainingExpected, bool shouldActualContainExpected, bool ignoreCase)
		{
			string RemoveXMLWhitespace(string stringToClean)
			{
				return regex.Replace(stringToClean, "><").Trim();
			}
			var expectedForCompare = RemoveXMLWhitespace(expected);
			var actualForCompare = RemoveXMLWhitespace(actualContainingExpected);

			var messageResult = new StringBuilder(message);
			var isRunningOnDAT = TestingState.IsRunningOnDAT;

			messageResult.Append("\r\n\r\nExpected String ");

			if (!shouldActualContainExpected)
			{
				messageResult.Append("NOT ");
			}

			if (isRunningOnDAT)
			{
				messageResult.Append("Containing:\r\n");
				messageResult.Append(expectedForCompare);
				messageResult.Append("\r\nActual Result:\r\n");
				messageResult.Append(actualForCompare);
				messageResult.Append("\r\n\r\n");
				AssertContainsCore(Assert, messageResult, expectedForCompare, actualForCompare, shouldActualContainExpected, ignoreCase);
			}
			else
			{
				var expectedIndented = TryIndentXml(expected);
				var actualIndented = TryIndentXml(actualContainingExpected);

				StringBuilder MessageExtender(StringBuilder originalMessage)
				{
					originalMessage.Append("Containing:\r\n");
					originalMessage.Append(HtmlFormatBadValue(expectedIndented));
					originalMessage.Append("\r\nActual Result:\r\n");
					originalMessage.Append(HtmlFormatGoodValue(actualIndented));
					originalMessage.Append("\r\n\r\n");
					var key = Guid.NewGuid();
					var compareDir = Path.Combine(GetTestFilesPath(), "NUnit Compare Files");
					EnsureDirectory(compareDir);
					var expectedFile = SaveFile(compareDir, key, "EXP", expectedIndented);
					var actualFile = SaveFile(compareDir, key, "ACT", actualIndented);
					var applicationPadding = "";
					var applicationString = new StringBuilder();

					foreach (var comparisonApplication in ComparisonApplication.GetApplicableApplications())
					{
						applicationString.Append(applicationPadding);
						applicationString.AppendFormat("<a href=\"{0}\">Show {1} Comparison</a>",
							GenerateCompareBridge(key, compareDir, expectedFile, actualFile, comparisonApplication),
							comparisonApplication);

						applicationPadding = " | ";
					}

					applicationString.Append("<br/>");
					applicationString.Append(originalMessage.ToString().Replace("\r\n", "<BR>").Replace("\n", "<BR>"));

					return applicationString;
				}

				AssertContainsCore(HtmlAssert, messageResult, expectedForCompare, actualForCompare, shouldActualContainExpected, ignoreCase, MessageExtender);
			}
		}
		static string TryIndentXml(string input)
		{
			try
			{
				var stringBuilder = new StringBuilder();

				var element = XElement.Parse(input);

				var settings = new XmlWriterSettings();
				settings.OmitXmlDeclaration = true;
				settings.Indent = true;
				settings.NewLineOnAttributes = true;

				using (var xmlWriter = XmlWriter.Create(stringBuilder, settings))
				{
					element.Save(xmlWriter);
				}

				return stringBuilder.ToString();
			}
			catch (XmlException)
			{
				return input;
			}
		}

		static void AssertContainsCore(Action<string, bool> assertionAction, StringBuilder message, string expected, string actualContainingExpected, bool shouldActualContainExpected, bool ignoreCase,
			Func<StringBuilder, StringBuilder> messageExtenderAction = null)
		{
			if (expected != null && ignoreCase)
			{
				expected = expected.ToUpperInvariant();
			}

			if (actualContainingExpected != null && ignoreCase)
			{
				actualContainingExpected = actualContainingExpected.ToUpperInvariant();
			}

			var condition = shouldActualContainExpected == (actualContainingExpected ?? string.Empty).Contains(expected ?? string.Empty);
			if (!condition && messageExtenderAction != null)
			{
				message = messageExtenderAction(message);
			}

			assertionAction(message.ToString(), condition);
		}
	}
}
