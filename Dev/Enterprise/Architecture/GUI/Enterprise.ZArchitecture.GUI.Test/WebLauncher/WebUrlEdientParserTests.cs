using System;
using Enterprise.ZArchitecture.GUI.WebLauncher;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Test.WebLauncher
{
	public class WebUrlEdientParserTest : TestCase
	{
		IWebUrlEdientParser _webUrlEdientParser;

		protected override void SetUp()
		{
			base.SetUp();

			var uris = new[] { new Uri("https://test.cw.wisetechglobal.com/"), new Uri("https://svr-test.wtg.zone/Services/") };
			_webUrlEdientParser = new WebUrlEdientParser(uris);
		}

		public void TestParse_BasicGood_Success()
		{
			const string testString = $"https://test.cw.wisetechglobal.com/link/DummyCommand/DummyController/DummyEntity?SomeQuery=123";
			var result = _webUrlEdientParser.Parse(testString);
			AssertEquals("Not Matching", "edient:Command=DummyCommand&ControllerID=DummyController&BusinessEntityPK=DummyEntity&SomeQuery=123", result);
		}

		public void TestParse_BadDomain_Failure()
		{
			const string testString = $"http://badhost/link/DummyCommand/DummyController/DummyEntity?SomeQuery=123";
			AssertExceptionThrown<ArgumentException>(() => _webUrlEdientParser.Parse(testString));
		}

		public void TestParse_GoodDomainAndPath_Success()
		{
			const string testString = $"https://svr-test.wtg.zone/Services/link/DummyCommand/DummyController/DummyEntity?SomeQuery=123";
			var result = _webUrlEdientParser.Parse(testString);
			AssertEquals("Not Matching", "edient:Command=DummyCommand&ControllerID=DummyController&BusinessEntityPK=DummyEntity&SomeQuery=123", result);
		}

		public void TestParse_GoodDomainBadPath_Failure()
		{
			const string testString = $"https://svr-test.wtg.zone/link/DummyCommand/DummyController/DummyEntity?SomeQuery=123";
			AssertExceptionThrown<ArgumentException>(() => _webUrlEdientParser.Parse(testString));
		}

		public void TestParse_GoodDomainBadPath2_Failure()
		{
			const string testString = $"https://svr-test.wtg.zone/Services/extra/link/DummyCommand/DummyController/DummyEntity?SomeQuery=123";
			AssertExceptionThrown<ArgumentException>(() => _webUrlEdientParser.Parse(testString));
		}

		public void TestTryParse_MultiCombinations_ExpectedResult()
		{
			// Arrange / Act / Assert
			var goodProtocols = new[] { "https" };
			var badProtocols = new[] { "ftp", "bad", "edient", "http" };

			var goodBaseDomainAndPath = new[] { "test.cw.wisetechglobal.com", "svr-test.wtg.zone/Services" };
			var badBaseDomainAndPath = new[] { "www.google.com", "somedomain.url", "svr-test.wtg.zone" };

			var goodUrls = new[] { "link/DummyCommand/DummyController/DummyEntity", "link/ShowEditForm/Dummy/1", "link/DummyCommand/DummyController/DummyEntity/", "link/DummyCommand/DummyController/DummyEntity?somequery=123" };
			var badUrls = new[] { "link/DummyCommand/DummyController/", "link/DummyCommand/DummyController", "notlink/DummyCommand/DummyController/DummyEntity", "linkb/DummyCommand/DummyController/DummyEntity", "ink/DummyCommand/DummyController/DummyEntity", "DummyCommand/DummyController/DummyEntity" };

			CombineAssertions("Test of all combinations", () =>
			{
				//Test of good combinations
				TestCombinations(goodProtocols, goodBaseDomainAndPath, goodUrls, expected: true);
				//Tests of bad combinations (mix of protocol, base domain/path and paths)
				TestCombinations(goodProtocols, goodBaseDomainAndPath, badUrls, expected: false);
				TestCombinations(goodProtocols, badBaseDomainAndPath, goodUrls, expected: false);
				TestCombinations(badProtocols, goodBaseDomainAndPath, goodUrls, expected: false);
			});
		}

		void TestCombinations(string[] protocols, string[] domains, string[] urls, bool expected)
		{
			foreach (var protocol in protocols)
			{
				foreach (var domain in domains)
				{
					foreach (var url in urls)
					{
						var testString = $"{protocol}://{domain}/{url}";
						var success = _webUrlEdientParser.TryParse(testString, out var _);
						Assert($"Failed TryParse({testString}), Expected Result: {expected}, Got Result: {success}", success == expected);
					}
				}
			}
		}
	}
}
