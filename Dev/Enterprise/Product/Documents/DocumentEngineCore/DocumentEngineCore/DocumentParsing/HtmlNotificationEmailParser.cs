using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.DocumentParsing
{
	public class HtmlNotificationEmailParser<TBusinessObject, TDocumentWrapper> : DocumentParser<TBusinessObject>
		where TBusinessObject : BusinessObject
		where TDocumentWrapper : DocumentWrapper
	{
		public HtmlNotificationEmailParser(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public EmailDef CreateEmail(TBusinessObject objectToWrap, ZString subject, ZString htmlBody)
		{
			return NotificationEmail.CreateEmail(subject, Parse(objectToWrap, htmlBody));
		}

		#region Implementation

		readonly HtmlNotificationEmailSender NotificationEmail = new HtmlNotificationEmailSender();

		protected override Type TypeOfWrapper
		{
			get { return typeof(TDocumentWrapper); }
		}

		#endregion
	}
}
