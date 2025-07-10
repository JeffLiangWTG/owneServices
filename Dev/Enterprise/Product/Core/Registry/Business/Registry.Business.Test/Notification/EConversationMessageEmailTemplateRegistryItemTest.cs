using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EConversationMessageEmailTemplateRegistryItem))]
	sealed class EConversationMessageEmailTemplateRegistryItemTest : StronglyTypedRegistryItemTestCase<NotificationEmailTemplate>
	{
		protected override StronglyTypedRegistryItem<NotificationEmailTemplate, NotificationEmailTemplate> GetNewRegistryItem()
		{
			return new EConversationMessageEmailTemplateRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, typeof(BusinessObject));
		}

		public void TestDefaultSubject()
		{
			AssertEquals("New Messages in (*ID*)", EConversationMessageEmailTemplateRegistryItem.DefaultSubject);
		}

		public void TestDefaultBody()
		{
			const string expectedBody = @"<html>
<head>
	<style>
		body, td, p, h1, h2, a {
			background-color: #FFFFFF;
			font-family: Arial, sans-serif;
			font-size: 12px;
		}

		body {
			width: 600px;
		}

		h1 {
			font-size: 16px;
		}

		h2 {
			font-size: 14px;
		}

		table {
			border-style: none;
		}
	</style>
</head>
<body>
	<h1>(*BusinessObjectName*) - New Messages</h1>
	<p>
		New messages have been added to <a href=""(*BusinessObjectHyperlink*)"">(*BusinessObjectName*)</a>
		<br />
		<br />
		You will find the new messages below, with some previous messages to provide additional context.
	</p>
	<strong><a href=""(*BusinessObjectHyperlink*)"">Reply via eConversation</a></strong>
	(*NewMessages*)
	(*PreviousMessages*)
	(*IF(""(*IsInternalRecipient*)""==""Y"", ""<p><em>If you no longer wish to be notified about <a href=""(*BusinessObjectHyperlink*)"">(*BusinessObjectName*)</a>, please unsubscribe yourself through the eConversation tab.</em></p>"", """")*)
	<p><em>All times are displayed in the senders local time of UTC(*UtcOffset*)</em></p>
	(*EmailIdentifier*)
</body>
</html>";

			AssertEquals(expectedBody, EConversationMessageEmailTemplateRegistryItem.DefaultBody);
		}

		public void TestDefaultSubjectTranslatable()
		{
			using (var mockCha = Res.GetLanguageInstance("ZH-CN").UseMockData())
			{
				mockCha.Put("EConversationMessageEmailTemplateDefaultSubject", new ResourceStringData("EConversationMessageEmailTemplateDefaultSubject", "Chinese email subject (*ID*)"));

				AssertEquals("Chinese email subject (*ID*)", EConversationMessageEmailTemplateRegistryItem.DefaultSubject.ToString(mockCha.Language));
			}
		}

		public void TestDefaultBodyTranslatable()
		{
			using (var mockCha = Res.GetLanguageInstance("ZH-CN").UseMockData())
			{
				mockCha.Put("EConversationMessageEmailTemplateDefaultBody", new ResourceStringData("EConversationMessageEmailTemplateDefaultBody", "Chinese email body"));

				AssertEquals("Chinese email body", EConversationMessageEmailTemplateRegistryItem.DefaultBody.ToString(mockCha.Language));
			}
		}

		public void TestDefaultStrings()
		{
			var registryItem = new EConversationMessageEmailTemplateRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, typeof(BusinessObject));

			AssertEquals(2, registryItem.DefaultStrings.Count());
			AssertCollectionContains(EConversationMessageEmailTemplateRegistryItem.DefaultSubject, registryItem.DefaultStrings);
			AssertCollectionContains(EConversationMessageEmailTemplateRegistryItem.DefaultBody, registryItem.DefaultStrings);
		}

		public void TestGetCaptions()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.EnglishBritish))
			using (var mockRes = Res.UseMockData())
			using (var britishRes = Res.GetLanguageInstance(Core.SharedConstants.Languages.EnglishBritish).UseMockData())
			{
				britishRes.Put("EConversationMessageEmailTemplateDefaultSubject", new ResourceStringData("EConversationMessageEmailTemplateDefaultSubject", "British English Subject"));
				britishRes.Put("EConversationMessageEmailTemplateDefaultBody", new ResourceStringData("EConversationMessageEmailTemplateDefaultBody", "British English Body"));

				var registryItem = new EConversationMessageEmailTemplateRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, typeof(BusinessObject));

				var actualCaptions = new List<string>(registryItem.GetCaptions(registryItem.Value));
				AssertEquals("Precondition", 2, actualCaptions.Count);
				AssertEquals(EConversationMessageEmailTemplateRegistryItem.DefaultSubject.GetUnresolvedString(), actualCaptions[0]);
				AssertEquals(EConversationMessageEmailTemplateRegistryItem.DefaultBody.GetUnresolvedString(), actualCaptions[1]);
			}
		}
	}
}
