using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class NotificationEmailTemplateRegistryItem : TranslatableVariantLengthStringCaptionsRegistryItem<NotificationEmailTemplate, NotificationEmailTemplate>, IRegistryItemVariantLengthCaptionSource
	{
		public NotificationEmailTemplateRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, Type docSourceType, string defaultSubject, string defaultBody, bool shouldHideEmailBody = false)
			: this(name, category, caption, hint, storage, options, new NotificationEmailTemplateRegistryDataType(docSourceType), new NotificationEmailTemplate(docSourceType, defaultSubject, defaultBody, shouldHideEmailBody))
		{
		}

		public NotificationEmailTemplateRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, NotificationEmailTemplateRegistryDataType dataType, NotificationEmailTemplate template)
			: base(new RegistryItemImpl(name, category, caption, hint, dataType, null, storage, options, template, false))
		{
		}

		#region TranslatableVariantLengthStringCaptionsRegistryItem

		protected override NotificationEmailTemplate Convert(NotificationEmailTemplate value)
		{
			value.RawEmailSubject = GetMultilingualString(value.EnglishEmailSubject);
			value.RawEmailBody = GetMultilingualString(value.EnglishEmailBody);
			return value;
		}

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get { return Array.Empty<ResourceString>(); }
		}

		protected override IEnumerable<ZPropertyInfoString> StringCaptionProperties
		{
			get
			{
				yield return Value.EnglishEmailSubjectInfo as ZPropertyInfoString;
				yield return Value.EnglishEmailBodyInfo as ZPropertyInfoString;
			}
		}

		int IRegistryItemVariantLengthCaptionSource.GetMaxLength(string caption)
		{
			if (Value.RawEmailSubject.ToString().Equals(caption))
			{
				return Value.EnglishEmailSubjectInfo.MaxLength;
			}
			else if (Value.RawEmailBody.ToString().Equals(caption))
			{
				return Value.EnglishEmailBodyInfo.MaxLength;
			}

			return MaxLength;
		}

		public override bool IsTranslatable
		{
			get { return true; }
		}

		#endregion TranslatableVariantLengthStringCaptionsRegistryItem
	}

	public class EConversationMessageEmailTemplateRegistryItem : NotificationEmailTemplateRegistryItem
	{
		public EConversationMessageEmailTemplateRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, Type docSourceType)
			: base(name, category, caption, hint, storage, options, new EConversationMessageEmailTemplateRegistryDataType(docSourceType), new NotificationEmailTemplate(docSourceType, DefaultSubject.GetUnresolvedString(), DefaultBody.GetUnresolvedString()))
		{
		}

		public static ResourceString DefaultSubject => ResString.GetMultilingualString("EConversationMessageEmailTemplateDefaultSubject", defaultSubject);

		public static ResourceString DefaultBody => ResString.GetMultilingualString("EConversationMessageEmailTemplateDefaultBody", defaultBodyContent);

		public override IEnumerable<ResourceString> DefaultStrings => new[] { DefaultSubject, DefaultBody };

		#region SuppressResourceStringsCheckRegion

		const string defaultSubject = "New Messages in (*ID*)";

		const string defaultBodyContent = @"<html>
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

		#endregion
	}

	[RegistryEditor("Enterprise.Registry.GUI.NotificationEmailTemplateRegistryItemEditor, Enterprise.Registry.GUI")]
	public class NotificationEmailTemplateRegistryDataType : NonPersistentBusinessObjectRegistryDataType<NotificationEmailTemplate>
	{
		public NotificationEmailTemplateRegistryDataType(Type docSourceType)
			: base(new NotificationEmailTemplate(docSourceType))
		{
			this.docSourceType = docSourceType;
		}

		protected override NotificationEmailTemplate DeserialiseCore(byte[] value)
		{
			NotificationEmailTemplate result = base.DeserialiseCore(value);
			result.DocSourceType = docSourceType;
			return result;
		}

		readonly Type docSourceType;
	}

	class EConversationMessageEmailTemplateRegistryDataType : NotificationEmailTemplateRegistryDataType
	{
		public EConversationMessageEmailTemplateRegistryDataType(Type docSourceType)
			: base(docSourceType)
		{
		}

		const string idSpecialField = "(*ID*)";
		const string emailIdentifierSpecialField = "(*EmailIdentifier*)";

		protected override void ValidateCore(IRegistryItem registryItem, NotificationEmailTemplate proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (!proposedValue.EmailSubject.Contains(idSpecialField))
			{
				throw new RegistryValidationException(Res.GetString("b5d7f819-f820-45a6-b5eb-141f3dea410f", "Email subject must contain the {0} special field.", idSpecialField));
			}

			if (!proposedValue.EmailBody.Contains(emailIdentifierSpecialField))
			{
				throw new RegistryValidationException(Res.GetString("5aca07d6-afef-4fb0-a6cb-cd5794cec165", "Email body must contain the {0} special field.", emailIdentifierSpecialField));
			}
		}
	}
}
