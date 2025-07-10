using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC231CProcessor : NCTSLinkedGuaranteeMessageProcessor<NCTSInboundEDIMessage, CC231CProvider>
	{
		public CC231CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override bool MustHaveLinkedObject => false;

		protected override string MessageFriendlyNameCore => Res.GetString("AACB23D2-A7EC-46FD-9227-FA53FC4E8BEB", "CC231C: Comprehensive guarantee cancellation notification");

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, CC231CProvider provider)
		{
			if (messageAttachee is CusGuaranteeHeader cusGuaranteeHeader)
			{
				cusGuaranteeHeader.CPH_EndDate = provider.InvalidityDate;
			}
		}

		protected override BusinessObject GetLinkedObject(BusinessObjectFactory factory, NCTSInboundEDIMessage message)
		{
			var provider = GetDataProvider(message);
			var guaranteeNumber = provider.GRN;
			CusGuaranteeHeader result = null;
			var guarantees = new EU.Business.CusGuaranteeHeader.Loader(factory).Load<CusGuaranteeHeader>(guaranteeNumber, new ZQuery(CusPermitHeaderSchema.CPH_RN_NKCountryCode, new ZString[] { Core.Constants.CountryCodes.Ireland, Core.Constants.CountryCodes.UnitedKingdom }));
			switch (guarantees.Length)
			{
				case 0:
					// do nothing
					break;
				case 1:
					result = guarantees[0];
					break;
				default:
					foreach (var guarante in guarantees)
					{
						if (result == null || result.CPH_EndDate < guarante.CPH_EndDate)
						{
							result = guarante;
						}
					}
					break;
			}
			return result;
		}

		protected override Type MessageInterpreterType => typeof(CC231CMessageInterpreter);
	}
}
