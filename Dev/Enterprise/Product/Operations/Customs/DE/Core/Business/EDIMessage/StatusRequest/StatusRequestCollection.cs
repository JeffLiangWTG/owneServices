using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.DE.Business
{
	public class StatusRequestCollection : ActiveBusinessObjectCollection<StatusRequest>
	{
		public StatusRequestCollection(BusinessObjectFactory factory, GlbBranch branch)
			: base(factory)
		{
			this.branch = Argument.NotNull(branch, nameof(branch));
		}
		protected readonly GlbBranch branch;

		protected override object[] GetCollectionState() => new object[] { branch };

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();

			result.AddToFilter(EDIMessageSchema.EM_GB, branch.PK);
			result.AddToFilter(JoinCondition.And, EDIMessageSchema.EM_MessageSubType, SQLComparisonOperator.Equal, new string[] { NctsMessageSubTypeList.Codes.StatusRequestMessage, ExportMessageSubTypeList.Codes.EXQ });
			result.AddToFilter(JoinCondition.And, EDIMessageSchema.EM_ReceiveTransmit, SQLComparisonOperator.Equal, EDIMessage.Direction.Transmit);
			result.AddToFilter(JoinCondition.And, EDIMessageSchema.EM_ApplicationCode, SQLComparisonOperator.Equal, new string[] { ApplicationCodes.DECustomsAtlasSystem, ApplicationCodes.DECustomsAesSystem });
			result.AddToFilter(JoinCondition.And, EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, new string[] { EDIMessageTypeList.Codes.NCTS, EDIMessageTypeList.Codes.AES });

			return result;
		}

		protected override void SetRelationshipDefaultsForElementCore(StatusRequest message, bool throwIfRelationshipNotSupported)
		{
			message.EM_GB = branch.PK;
			base.SetRelationshipDefaultsForElementCore(message, throwIfRelationshipNotSupported);
		}

		protected override bool AllowNew => false;
	}
}
