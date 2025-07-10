using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public abstract class ECSEnveloppeMessageWrapper : IMessageEnvelope
	{
		public ECSEnveloppeMessageWrapper(CusExitDetail exitDetail)
		{
			this.exitDetail = Argument.NotNull(exitDetail, nameof(exitDetail));
		}

		protected abstract ZString GetSchemaID();

		public ZString SchemaID => GetSchemaID();

		public ZString SchemaVersion => "01012012";

		public ZString PartnerId => exitDetail.SiretNumber;

		public ZString TransactionId
		{
			get
			{
				return string.Join("+", GlbCompany.CurrentCompany.LicenceKeyIdentifier, GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsECS, EDIInterchange.InterchangeNumberPlaceHolder);
			}
		}

		public ZShort NumSeq
		{
			get
			{
				return (ZShort)exitDetail.Messages.Cast<EDIMessage>().Count(m => m.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit);
			}
		}

		protected CusExitDetail exitDetail;
	}
}
