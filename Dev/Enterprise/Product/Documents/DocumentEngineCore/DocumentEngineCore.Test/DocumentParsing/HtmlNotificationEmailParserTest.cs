using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.DocumentParsing.Testing
{
	sealed class HtmlNotificationEmailParserTest : TestCaseWithFactory
	{
		public void TestCreateEmail()
		{
			const string unformattedEmailHtmlBody = @"This text will go into the body.
'(*Apple*)' is a field value that has been substituted from the document wrapper.
";
			var email = new TestHtmlNotificationEmailParser(Factory).CreateEmail(Factory.New<WrappedBusinessObject>(), "Email Subject", unformattedEmailHtmlBody);
			CombineAssertions(() =>
			{
				AssertEquals("Subject", "Email Subject", email.Subject);
				AssertMultilineASCIIEquals("Body", ExpectedEmailHtmlBody, email.Body);
				AssertEquals("ContentType", EmailContentTypes.HTML, email.ContentType);
				AssertEquals("Attachments.Count", 2, email.Attachments.Count);
				AssertEquals("Attachments[0].DisplayName", "Banner.jpg", email.Attachments[0].DisplayName);
				AssertEquals("Attachments[1].DisplayName", "Footer.jpg", email.Attachments[1].DisplayName);
			});
		}

		string ExpectedEmailHtmlBody
		{
			get
			{
				using (var resourceRetriever = new EmbeddedResourceRetriever())
				{
					var result = resourceRetriever.GetString("Enterprise.DocumentEngineCore.DocumentParsing.Testing.EmailBodyTestFile.htm");
					result = result.Replace("(*StyleSheet*)", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
					result = result.Replace("(*Body*)",
	@"This text will go into the body.
'I like Apples' is a field value that has been substituted from the document wrapper.");
					return result;
				}
			}
		}

		sealed class TestHtmlNotificationEmailParser : HtmlNotificationEmailParser<WrappedBusinessObject, DummyWrapper>
		{
			public TestHtmlNotificationEmailParser(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}

		sealed class WrappedBusinessObject : DummyBusinessObject
		{
			public WrappedBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZString Apple
			{
				get { return "I like Apples"; }
			}
		}

		sealed class DummyWrapper : DocumentWrapper
		{
			public DummyWrapper(WrappedBusinessObject objectToBeWrapped)
			{
				this.objectToBeWrapped = objectToBeWrapped;
			}

			readonly WrappedBusinessObject objectToBeWrapped;

			public static DummyWrapper New(WrappedBusinessObject objectToBeWrapped, BusinessObjectFactory factory)
			{
				return new DummyWrapper(objectToBeWrapped);
			}

			[DocumentField("All about Apples version 1")]
			public ZString Apple
			{
				get { return objectToBeWrapped.Apple; }
			}

			public override string ToString()
			{
				return "";
			}
		}
	}
}
