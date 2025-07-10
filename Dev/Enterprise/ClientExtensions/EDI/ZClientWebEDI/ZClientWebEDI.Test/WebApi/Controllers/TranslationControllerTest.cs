using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http.Results;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZClientWebCargoWiseEDI.Translations;
using Enterprise.ZClientWebCargoWiseEDI.WebApi.Controllers;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class TranslationControllerTest : TransactionedTestCase
	{
		public void TestImportDoesntThrowForNullContent()
		{
			var adapter = new TranslationControllerForTest();
			AssertNoExceptionThrown(delegate
			{
				adapter.Import(new ImportRequestData());
			});
		}

		public void TestExport()
		{
			var adapter = new TranslationControllerForTest();
			var request = new ExportRequestData();
			request.language = null;
			request.contentModule = "Billing";
			var response = ((NegotiatedContentResult<Dictionary<string, string>>)adapter.GetDataForExport(request)).Content;
			AssertEquals(2, response.Count);
			Assert("Should contain [name1]", response.ContainsKey("name1"));
			Assert("Should contain [name2]", response.ContainsKey("name2"));
			AssertEquals("<EnterpriseResources Language=\"EN\">" + @"
  <Res>
    <Key>key1</Key>
    <Caption>value1</Caption>
  </Res>
  <Res>
    <Key>key2</Key>
    <Caption>value2</Caption>
  </Res>
</EnterpriseResources>", response["name1"]);
			AssertEquals("<EnterpriseResources Language=\"EN\">" + @"
  <Res>
    <Key>key3</Key>
    <Caption>value3</Caption>
  </Res>
  <Res>
    <Key>key4</Key>
    <Caption>value4</Caption>
  </Res>
</EnterpriseResources>", response["name2"]);
		}

		public void TestExport_EmptyDataSource()
		{
			var adapter = new TranslationControllerForTest()
			{ ShouldUseEmptyDataSource = true };
			var request = new ExportRequestData();
			request.language = null;
			request.contentModule = "Billing";
			var response = ((NegotiatedContentResult<Dictionary<string, string>>)adapter.GetDataForExport(request)).Content;
			AssertEquals(0, response.Count);
		}

		public void TestExport_InvalidLanguage()
		{
			var adapter = new TranslationControllerForTest();
			var request = new ExportRequestData();
			request.language = "xxx";
			var result = adapter.GetDataForExport(request) as NegotiatedContentResult<string>;
			AssertEquals(HttpStatusCode.Forbidden, result.StatusCode);
			Assert("Should contain 'Culture is not supported'", result.Content.Contains("Culture is not supported"));
		}

		public void TestExportUntranslated()
		{
			var adapter = new TranslationControllerForTest();
			var importRequest = new ImportRequestData();
			importRequest.language = "UK-UA";
			importRequest.contents = "<EnterpriseResources Language=\"EN\">" + @"
  <Res>
    <Key>key3</Key>
    <Caption>value3 ukr</Caption>
  </Res> 
</EnterpriseResources>";
			adapter.Import(importRequest);
			var source = new DatabaseResourceStringSource("UK-UA");
			var saved = source.ReadAll().ToArray();
			AssertEquals(1, saved.Length);
			AssertEquals("key3", saved[0].Key);
			AssertEquals("value3 ukr", saved[0].Caption);
			var exportRequest = new ExportRequestData();
			exportRequest.language = "uk-UA";
			var response = ((NegotiatedContentResult<Dictionary<string, string>>)adapter.GetDataForExport(exportRequest)).Content;
			Assert("Should contain [name1]", response.ContainsKey("name1"));
			Assert("Should contain [name2]", response.ContainsKey("name2"));
			AssertEquals("<EnterpriseResources Language=\"EN\">" + @"
  <Res>
    <Key>key1</Key>
    <Caption>value1</Caption>
  </Res>
  <Res>
    <Key>key2</Key>
    <Caption>value2</Caption>
  </Res>
</EnterpriseResources>", response["name1"]);
			AssertEquals("<EnterpriseResources Language=\"EN\">" + @"
  <Res>
    <Key>key4</Key>
    <Caption>value4</Caption>
  </Res>
</EnterpriseResources>", response["name2"]);
			exportRequest.language = "ru-RU";
			response = ((NegotiatedContentResult<Dictionary<string, string>>)adapter.GetDataForExport(exportRequest)).Content;
			Assert("Should contain [name1]", response.ContainsKey("name1"));
			Assert("Should contain [name2]", response.ContainsKey("name2"));
			AssertEquals("<EnterpriseResources Language=\"EN\">" + @"
  <Res>
    <Key>key1</Key>
    <Caption>value1</Caption>
  </Res>
  <Res>
    <Key>key2</Key>
    <Caption>value2</Caption>
  </Res>
</EnterpriseResources>", response["name1"]);
			AssertEquals("<EnterpriseResources Language=\"EN\">" + @"
  <Res>
    <Key>key3</Key>
    <Caption>value3</Caption>
  </Res>
  <Res>
    <Key>key4</Key>
    <Caption>value4</Caption>
  </Res>
</EnterpriseResources>", response["name2"]);
		}

		public void TestImport()
		{
			var importRequest = new ImportRequestData();
			importRequest.language = "UK-UA";
			importRequest.contents = "<EnterpriseResources Language=\"EN\">" + @"
  <Res>
    <Key>key3</Key>
    <Caption>value3</Caption>
  </Res>
  <Res>
    <Key>key4</Key>
    <Caption>value4</Caption>
  </Res>
</EnterpriseResources>";
			var adapter = new TranslationControllerForTest();
			var result = adapter.Import(importRequest) as NegotiatedContentResult<string>;
			AssertEquals(HttpStatusCode.OK, result.StatusCode);
			AssertEquals("4 records have been updated.", result.Content);
			var source = new DatabaseResourceStringSource("UK-UA");
			var saved = source.ReadAll().ToArray();
			AssertEquals(2, saved.Length);
			AssertEquals("key3", saved[0].Key);
			AssertEquals("value3", saved[0].Caption);
			AssertEquals("key4", saved[1].Key);
			AssertEquals("value4", saved[1].Caption);
		}

		public void TestImport_ConvertLanguage()
		{
			var importRequest = new ImportRequestData();
			importRequest.language = "UK-UA";
			importRequest.contents = "<EnterpriseResources Language=\"EN\">" + @"
  <Res>
    <Key>key3</Key>
    <Caption>value3</Caption>
  </Res>
  <Res>
    <Key>key4</Key>
    <Caption>value4</Caption>
  </Res>
</EnterpriseResources>";
			var adapter = new TranslationControllerForTest();
			var result = adapter.Import(importRequest) as NegotiatedContentResult<string>;
			AssertEquals(HttpStatusCode.OK, result.StatusCode);
			var source = new DatabaseResourceStringSource("UK-UA");
			var saved = source.ReadAll().ToArray();
			AssertEquals(2, saved.Length);
			AssertEquals("key3", saved[0].Key);
			AssertEquals("value3", saved[0].Caption);
			AssertEquals("key4", saved[1].Key);
			AssertEquals("value4", saved[1].Caption);
		}

		public void TestImport_ConvertZhLanguage()
		{
			var adapter = new TranslationControllerForTest();
			var importRequest = new ImportRequestData();
			importRequest.language = "ZH-TW";
			importRequest.contents = "<EnterpriseResources Language=\"EN\">" + @"
  <Res>
    <Key>key3</Key>
    <Caption>value3</Caption>
  </Res>
  <Res>
    <Key>key4</Key>
    <Caption>value4</Caption>
  </Res>
</EnterpriseResources>";
			var result = adapter.Import(importRequest) as NegotiatedContentResult<string>;
			AssertEquals(HttpStatusCode.OK, result.StatusCode);
			var source = new DatabaseResourceStringSource("ZH-TW");
			var saved = source.ReadAll().ToArray();
			AssertEquals(2, saved.Length);
			AssertEquals("key3", saved[0].Key);
			AssertEquals("value3", saved[0].Caption);
			AssertEquals("key4", saved[1].Key);
			AssertEquals("value4", saved[1].Caption);
		}

		public void TestImport_EmptyContent()
		{
			var adapter = new TranslationControllerForTest();
			var importRequest = new ImportRequestData();
			importRequest.language = "UK-UA";
			var result = adapter.Import(importRequest) as NegotiatedContentResult<string>;
			AssertEquals(HttpStatusCode.NoContent, result.StatusCode);
			AssertEquals("Empty request content", result.Content);
		}

		public void TestImport_UnknownLocale()
		{
			var adapter = new TranslationControllerForTest();
			var importRequest = new ImportRequestData();
			importRequest.language = "12-12";
			importRequest.contents = "<EnterpriseResources Language=\"EN\">" + @"
  <Res>
    <Key>key3</Key>
    <Caption>value3</Caption>
  </Res>
  <Res>
    <Key>key4</Key>
    <Caption>value4</Caption>
  </Res>
</EnterpriseResources>";
			var result = adapter.Import(importRequest) as NegotiatedContentResult<string>;
			AssertEquals(HttpStatusCode.BadRequest, result.StatusCode);
			AssertEquals(@"Culture is not supported.
Parameter name: name
12-12 is an invalid culture identifier.", result.Content);
		}

		public void TestImport_BadLanguage()
		{
			var adapter = new TranslationControllerForTest();
			var importRequest = new ImportRequestData();
			importRequest.language = "HO-HO";
			importRequest.contents = "<EnterpriseResources Language=\"EN\">" + @"
  <Res>
    <Key>key3</Key>
    <Caption>value3</Caption>
  </Res>
  <Res>
    <Key>key4</Key>
    <Caption>value4</Caption>
  </Res>
</EnterpriseResources>";
			var result = adapter.Import(importRequest) as NegotiatedContentResult<string>;
			AssertEquals(HttpStatusCode.BadRequest, result.StatusCode);
		}

		public void TestImport_NoStrings()
		{
			var adapter = new TranslationControllerForTest();
			var importRequest = new ImportRequestData();
			importRequest.language = "UK-UA";
			importRequest.contents = "<EnterpriseResources Language=\"EN\"></EnterpriseResources>";
			var result = adapter.Import(importRequest) as NegotiatedContentResult<string>;
			AssertEquals(HttpStatusCode.OK, result.StatusCode);
			AssertEquals("Empty translated resources", result.Content);
		}

		public void TestImport_SendEmail()
		{
			var adapter = new TranslationControllerForTest();
			var importRequest = new ImportRequestData();
			importRequest.language = "HO-HO";
			importRequest.contents = "<EnterpriseResources Language=\"EN\">" + @"
  <Res>
    <Key>key3</Key>
    <Caption>value3</Caption>
  </Res>
  <Res>
    <Key>key4</Key>
    <Caption>value4</Caption>
  </Res>
</EnterpriseResources>";
			var result = adapter.Import(importRequest) as NegotiatedContentResult<string>;
			AssertEquals(HttpStatusCode.BadRequest, result.StatusCode);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Could not import exam translation data", email.Subject);
			Assert(email.Body.StartsWith($"Could not process data. Language = HO-HO, Contents = {importRequest.contents}."));
		}

		public void TestEmptyRequest()
		{
			var adapter = new TranslationControllerForTest();
			var result = adapter.GetDataForExport(null) as NegotiatedContentResult<string>;
			AssertEquals(HttpStatusCode.NoContent, result.StatusCode);
			AssertEquals("Empty request", result.Content);
			result = adapter.Import(null) as NegotiatedContentResult<string>;
			AssertEquals(HttpStatusCode.NoContent, result.StatusCode);
			AssertEquals("Empty request", result.Content);
		}

		void InitialiseMailManager()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "LDK";
			staff.GS_EmailAddress = "someone@test.com";
			var group = factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staff);
			factory.Save();
			EnvProxy.Instance.Registry.MailboxEmailAddress = "test@example.com";
			EDIDataRegistry.Instance.InternalNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
		}

		protected override void SetUp()
		{
			base.SetUp();
			InitialiseMailManager();
		}

		public class TranslationControllerForTest : ContentTranslationController
		{
			public bool ShouldUseEmptyDataSource;
			protected override IContentTranslationDataSource GetDataSource(string contentModule)
			{
				return new TranslationDataSourceForTest()
				{ ShouldUseEmptyDataSource = ShouldUseEmptyDataSource };
			}

			protected override bool IsValidClientIp()
			{
				return true;
			}
		}

		class TranslationDataSourceForTest : IContentTranslationDataSource
		{
			public IEnumerable<IContentTranslationDataItem> GetTranslatableResources()
			{
				if (!ShouldUseEmptyDataSource)
				{
					yield return new TranslationDataItem("name1", new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } });
					yield return new TranslationDataItem("name2", new Dictionary<string, string> { { "key3", "value3" }, { "key4", "value4" } });
				}
				else
				{
					yield return new TranslationDataItem("empty", new Dictionary<string, string>());
				}
			}

			public bool ShouldUseEmptyDataSource;
		}

		class TranslationDataItem : IContentTranslationDataItem
		{
			public TranslationDataItem(string name, Dictionary<string, string> res)
			{
				Resources = res;
				Id = name;
			}

			public string Id { get; }

			public IDictionary<string, string> Resources { get; }
		}
	}
}
