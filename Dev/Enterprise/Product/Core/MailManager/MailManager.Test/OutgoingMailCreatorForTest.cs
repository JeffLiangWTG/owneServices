using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MailManager.Testing
{
	public class OutgoingMailCreatorForTest : OutgoingMailCreator
	{
		protected override MailItem Create_Core(BusinessObjectFactory factory, EmailDef emailToSend, string groupDetail)
		{
			base.Create_Core(factory, emailToSend, groupDetail);
			LastEmailDefSent = emailToSend;

			return null;
		}

		public MailItem LastMailItemSentInDB
		{
			get
			{
				MailItem result = null;
				if (LastEmailDefSent != null)
				{
					var filter = new ZQuery();
					filter.AddToFilter(MailDBItemsSchema.MI_Direction, SQLComparisonOperator.Equal, MailDirection.Transmit);
					filter.AddToFilter(MailDBItemsSchema.MI_Status, SQLComparisonOperator.Equal, MailStatus.Queued);
					filter.AddToFilter(MailDBItemsSchema.MI_Subject, SQLComparisonOperator.Equal, LastEmailDefSent.Subject);
					filter.OrderBy = MailDBItemsSchema.MI_SendDateTime.Name + " DESC";

					var factory = new BusinessObjectFactory();
					result = factory.LoadTop1<MailItem>(filter);
				}
				return result;
			}
		}

		public EmailDef LastEmailDefSent;
		public ZGuid LastRecipientGroupPK = ZGuid.Empty;
	}
}
