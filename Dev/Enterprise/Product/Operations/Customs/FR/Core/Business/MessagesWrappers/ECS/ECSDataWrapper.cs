using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.MessagesWrappers.Common;
using Enterprise.Customs.FR.Messaging.Interfaces.ECS;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.ECS
{
	public abstract class ECSDataWrapper : IECSData
	{
		public ECSDataWrapper(CusExitDetail cusExitDetail)
		{
			ExitDetail = Argument.NotNull(cusExitDetail, nameof(cusExitDetail));
		}

		protected abstract ECSEnveloppeMessageWrapper GetECSEnveloppeMessageWrapper();

		public ECSEnveloppeMessageWrapper MessageWrapper => messageWrapper ?? (messageWrapper = GetECSEnveloppeMessageWrapper());
		ECSEnveloppeMessageWrapper messageWrapper;

		#region IECSData
		ZString IECSData.MRN => ExitDetail.CED_MovementReferenceNumber;

		ZString IECSData.Office => ExitDetail.CED_CustomsOffice;

		ZString IECSData.Location => ExitDetail.CED_LocationOfGoods;

		ZString IECSData.Agreement => ExitDetail.AgentOrDeclarant?.DeltaAgreementNumberCollection.GetOrgCusAccountForCode(OrgCusAccountCodeList.Codes.ECS)?.CZ_Account ?? ZString.Empty;

		ZString IECSData.EORI => ExitDetail.EORINumber;

		public ZString SchemaID => MessageWrapper.SchemaID;

		public ZString SchemaVersion => MessageWrapper.SchemaVersion;

		public ZString PartyId => MessageWrapper.PartnerId;

		public ZString TransactionId => MessageWrapper.TransactionId;

		public ZShort Numseq => MessageWrapper.NumSeq;
		#endregion

		protected CusExitDetail ExitDetail { get; }
	}
}
