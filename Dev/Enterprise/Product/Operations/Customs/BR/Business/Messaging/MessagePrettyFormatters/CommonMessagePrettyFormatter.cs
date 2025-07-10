using System.Collections.Generic;
using System.Collections.Specialized;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.BR.Business
{
	#region SuppressResourceStringsCheckRegion

	public abstract class CommonMessagePrettyFormatter
	{
		protected HtmlTableCreator GetNewTableCreator(IEnumerable<string> columnTitles = null)
		{
			var htmlAttributes =
				new NameValueCollection
				{
					{ "border", "1" },
					{ "cellpadding", "2" },
					{ "cellspacing", "0" },
					{ "class", "table" },
					{ "style", "white-space:pre" },
					TableInterpretation.Attributes.FullWidth,
				};

			return new HtmlTableCreator(htmlAttributes, columnTitles) { EnableHTMLEncoding = false };
		}

		protected string GetJobLink(CusEntryHeader entryHeader) => EmailDefBuilder.GetJobLink(entryHeader.Declaration, entryHeader.Declaration.JE_DeclarationReference);

		protected string GetHtmlWithTemplateEmptyWithDynamicHtml5(string dynamicHtmlHeading, string dynamicHtml1, string dynamicHtml2 = "", string dynamicHtml3 = "", string dynamicHtml4 = "", string dynamicHtml5 = "")
		{
			var emailBuilder = new EmailDefBuilder("", EmailDefBuilder.HtmlTemplates.EmptyWithDynamicHtml5);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtmlHeading, dynamicHtmlHeading);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, dynamicHtml1);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml2, dynamicHtml2);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml3, dynamicHtml3);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml4, dynamicHtml4);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml5, dynamicHtml5);

			return emailBuilder.ToString();
		}

		protected const string ResponseFromBrazilianCustoms = "A response message has been received from Brazilian Customs.<br/>";
		protected const string ReceiveFollowingUpdates = "The message sent for the above mentioned job has received the following update(s):<br/>";
	}

	#endregion
}
