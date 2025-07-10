using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public class MessageLoggingFactory : RestrictedTableBusinessObjectFactoryForSave
	{
		public MessageLoggingFactory() : base()
		{
			NameForDebugging = "Universal Message Logging";
		}

		public override BusinessObjectFactory CreateNewFactory(bool usingMyThreadSentry = false)
		{
			return new MessageLoggingFactory();
		}

		protected override ICollection<string> GetAllowedTablesToSave()
		{
			return new HashSet<string> {
				EDIMessageSchema.Constants.TableName,
				StmALogSchema.Constants.TableName,
				StmNoteSchema.Constants.TableName,
				EDIInterchangeSchema.Constants.TableName,
				GenPivotSchema.Constants.TableName,
				MailDBItemsSchema.Constants.TableName,
				MailDBRecipientsSchema.Constants.TableName,
				MailDBAttachmentsSchema.Constants.TableName,
				MailDBAttachmentsSchema.Constants.TableName
			};
		}
	}
}
