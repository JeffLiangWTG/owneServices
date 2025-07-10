using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Registry;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Registry
{
	[TestedType(typeof(AddDefaultIncidentClosedEmailTemplate))]
	public class AddDefaultIncidentClosedEmailTemplateTest : RegistryDataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new AddDefaultIncidentClosedEmailTemplate();

		readonly string testData = @"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfCodeDescriptionIncidentEmailTemplatePair
	xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
	xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<CodeDescriptionIncidentEmailTemplatePair>
		<CodeMaxLength>3</CodeMaxLength>
		<Code>AUT</Code>
		<Description>Auto Close Feature Request</Description>
		<Product />
		<IncidentEmailTemplatePair>
			<NotificationEmailTemplate>
				<EmailSubject>eRequest: (*IncidentNumber*) has been closed as new feature suggestion received.</EmailSubject>
				<EmailBody>We &lt;b&gt;&lt;i&gt;LOVE&lt;/i&gt;&lt;/b&gt; the fact you are thinking of ways to enhance our product!!!!    We are committed to providing our clients with outstanding &lt;b&gt;&lt;i&gt;productivity, functionality, integration and Customs, Supply Chain and Accounting Compliance&lt;/i&gt;&lt;/b&gt; and the &lt;b&gt;&lt;i&gt;Global Reach&lt;/i&gt;&lt;/b&gt; of our product.    Please keep your great ideas coming, we do review all these feature requests.    Ideas and improvements which address areas of high pain, high cost that occur frequently in our Customer’s business are valuable to us and our customers. In these cases we can offer Customers a development service to enable this improvement to be partially funded by the Customer and built to solve the Customers core problem in a deep and beneficial way.    We ask for this partial contribution to allow us to continue creating great software that continuously improves your productivity and adds real value to your business. We fund over 500 new product improvements every year without customer assistance, however we receive 500 improvement ideas per month from our customers and this is far more than we can fund without some assistance from you, the Customer.    If you believe that your feature suggestion will improve your business in any of the above areas and is high pain, cost or repetition, we strongly suggest you &lt;b&gt;reclassify this eRequest as “CR7” (Feature Request).&lt;/b&gt;     CargoWise will provide a no obligation estimate and if this proves acceptable, you may then request a firm and accurate formal Quote and Design Document.    Otherwise, this incident will be filed as a Feature Request suggestion and may be considered as part of our Long Range Planning enhancements at some time in the future.</EmailBody>
				<ShouldHideEmailBody>N</ShouldHideEmailBody>
			</NotificationEmailTemplate>
			<NotificationEmailTemplate>
				<EmailSubject>eRequest: (*IncidentNumber*) has been closed as new feature suggestion received.</EmailSubject>
				<EmailBody>We &lt;b&gt;&lt;i&gt;LOVE&lt;/i&gt;&lt;/b&gt; the fact you are thinking of ways to enhance our product!!!!    We are committed to providing our clients with outstanding &lt;b&gt;&lt;i&gt;productivity, functionality, integration and Customs, Supply Chain and Accounting Compliance&lt;/i&gt;&lt;/b&gt; and the &lt;b&gt;&lt;i&gt;Global Reach&lt;/i&gt;&lt;/b&gt; of our product.    Please keep your great ideas coming, we do review all these feature requests.    Ideas and improvements which address areas of high pain, high cost that occur frequently in our Customer’s business are valuable to us and our customers. In these cases we can offer Customers a development service to enable this improvement to be partially funded by the Customer and built to solve the Customers core problem in a deep and beneficial way.    We ask for this partial contribution to allow us to continue creating great software that continuously improves your productivity and adds real value to your business. We fund over 500 new product improvements every year without customer assistance, however we receive 500 improvement ideas per month from our customers and this is far more than we can fund without some assistance from you, the Customer.    If you believe that your feature suggestion will improve your business in any of the above areas and is high pain, cost or repetition, we strongly suggest you &lt;b&gt;reclassify this eRequest as “CR7” (Feature Request).&lt;/b&gt;     CargoWise will provide a no obligation estimate and if this proves acceptable, you may then request a firm and accurate formal Quote and Design Document.    Otherwise, this incident will be filed as a Feature Request suggestion and may be considered as part of our Long Range Planning enhancements at some time in the future.</EmailBody>
				<ShouldHideEmailBody>N</ShouldHideEmailBody>
			</NotificationEmailTemplate>
		</IncidentEmailTemplatePair>
	</CodeDescriptionIncidentEmailTemplatePair>
	<CodeDescriptionIncidentEmailTemplatePair>
		<CodeMaxLength>3</CodeMaxLength>
		<Code>DFT</Code>
		<Description>Default Close Notification Email Template</Description>
		<Product />
		<IncidentEmailTemplatePair>
			<NotificationEmailTemplate>
				<EmailSubject>eRequest: (*IncidentNumber*) has been (*SupportIncidentCloseType*).</EmailSubject>
				<EmailBody>Incident (*IncidentNumber*) - (*Summary*) - has been closed with a disposition of (*DetailedDispositionDescription*).(*ChargeableWorkBillingNotice*)(*ResolutionNoteText*)    Should you need any further help with regards to this incident, please let us know by replying directly to this email and we will re-open this incident and assist you further.  </EmailBody>
				<ShouldHideEmailBody>N</ShouldHideEmailBody>
			</NotificationEmailTemplate>
			<NotificationEmailTemplate>
				<EmailSubject>eRequest: (*IncidentNumber*) has been (*SupportIncidentCloseType*).</EmailSubject>
				<EmailBody>eRequest (*IncidentNumber*) - (*Summary*) - has been closed with a disposition of (*DetailedDispositionDescription*).(*ChargeableWorkBillingNotice*)(*ResolutionNoteText*)    Should you need any further help with regards to this eRequest, please let us know by sending an eConversation message through the eRequest. All eConversations will re-open your eRequest for review by WiseTech Global.</EmailBody>
				<ShouldHideEmailBody>N</ShouldHideEmailBody>
			</NotificationEmailTemplate>
		</IncidentEmailTemplatePair>
	</CodeDescriptionIncidentEmailTemplatePair>
	<CodeDescriptionIncidentEmailTemplatePair>
		<CodeMaxLength>3</CodeMaxLength>
		<Code>FQP</Code>
		<Description>Formal Quote Provided</Description>
		<Product />
		<IncidentEmailTemplatePair>
			<NotificationEmailTemplate>
				<EmailSubject>Customer Service eRequest (*IncidentNumber*) – Formal quote</EmailSubject>
				<EmailBody>Thank you for expressing interest in engaging us to develop (*IncidentNumber*) - (*Summary*).(*ResolutionNoteText*)</EmailBody>
				<ShouldHideEmailBody>N</ShouldHideEmailBody>
			</NotificationEmailTemplate>
			<NotificationEmailTemplate>
				<EmailSubject>Customer Service eRequest (*IncidentNumber*) – Formal quote</EmailSubject>
				<EmailBody>Thank you for expressing interest in engaging us to develop (*IncidentNumber*) - (*Summary*).(*ResolutionNoteText*)</EmailBody>
				<ShouldHideEmailBody>N</ShouldHideEmailBody>
			</NotificationEmailTemplate>
		</IncidentEmailTemplatePair>
	</CodeDescriptionIncidentEmailTemplatePair>
</ArrayOfCodeDescriptionIncidentEmailTemplatePair>";

		readonly string sdName = "CustomerServiceIncidentClosedNotificationEmailTemplates";

		protected override void PrepareTestData()
		{
			Helper.DeleteStmDataRow(sdName);
			Helper.InsertStmDataRow(sdName, "BIN", Encoding.Unicode.GetBytes(testData));
		}

		protected override void AssertTransformationResults()
		{
			var value = Helper.GetStmDataValue(sdName);
			using (var memoryStream = new MemoryStream(value))
			{
				var parsedXml = XDocument.Load(memoryStream);

				var newTemplateNode = parsedXml.Root
							.Elements("CodeDescriptionIncidentEmailTemplatePair")
							.FirstOrDefault(c => c.Elements("Code").SingleOrDefault()?.Value == "NEV");

				AssertNotNull(newTemplateNode);

				var defaultTemplateNode = parsedXml.Root
							.Elements("CodeDescriptionIncidentEmailTemplatePair")
							.FirstOrDefault(c => c.Elements("Code").SingleOrDefault()?.Value == "DFT");

				var dftIncidentEmailTemplatePair = defaultTemplateNode.Element("IncidentEmailTemplatePair").Element("NotificationEmailTemplate");
				var nevIncidentEmailTemplatePair = newTemplateNode.Element("IncidentEmailTemplatePair").Element("NotificationEmailTemplate");

				AssertEquals(dftIncidentEmailTemplatePair.Element("EmailSubject").Value, nevIncidentEmailTemplatePair.Element("EmailSubject").Value);
				AssertEquals(dftIncidentEmailTemplatePair.Element("EmailBody").Value, nevIncidentEmailTemplatePair.Element("EmailBody").Value);
			}
		}
	}
}
