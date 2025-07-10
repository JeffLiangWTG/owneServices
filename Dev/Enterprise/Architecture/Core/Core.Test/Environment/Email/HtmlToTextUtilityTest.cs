using System;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	public class HtmlToTextUtilityTest : TestCase
	{
		public void TestHtmlToText()
		{
			var fileContent = resourceRetriever.Value.GetString("Enterprise.ZArchitecture.Core.Test.Environment.Email.TestFiles.TestHtml2Text.htm");
			var htmlToTextUtility = new HtmlToTextUtility();
			var plainText = htmlToTextUtility.GetPlainText(fileContent);

			AssertEquals(@"Paragraphs
At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Github [www.github.com]
At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.
Printed table
Article Price Taxes Amount Total
Product 1
Contains: 1x Product 1
6,99€ 7% 1 6,99€
Shipment costs 3,25€ 7% 1 3,25€
Lists
* At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.
* At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.
* At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.
* At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.
Mailto formating
Some Company
Some Street 42
Somewhere
E-Mail: Click here [mailto:test@example.com]!
Printed Source Code
htmlToText.fromFile(new URL('test.html', import.meta.url), { tables: ['#invoice', '.address'] }, function(err, text) { if (err) return console.error(err); console.log(text); });
Comments", plainText);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
