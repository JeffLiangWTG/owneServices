using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC229CProcessor : NCTSLinkedGuaranteeMessageProcessor<NCTSInboundEDIMessage, CC229CProvider>
	{
		public CC229CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override bool MustHaveLinkedObject => false;

		protected override string MessageFriendlyNameCore => Res.GetString("7DEB4E35-AEBD-40B3-9752-EF7871CFD7B2", "CC229C: INDIVIDUAL GUARANTEE VOUCHER REVOCATION NOTIFICATION");

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, CC229CProvider provider)
		{
			if (messageAttachee is IE.Business.CusGuaranteeHeader cusGuaranteeeader)
			{
				cusGuaranteeeader.CPH_EndDate = provider.InvalidityDate;
			}
		}

		protected override Type MessageInterpreterType => typeof(CC229CMessageInterpreter);

		protected override BusinessObject GetLinkedObject(BusinessObjectFactory factory, NCTSInboundEDIMessage message)
		{
			var provider = GetDataProvider(message);
			var guaranteeNumber = provider.GRN;
			CusGuaranteeHeader result = null;
			var guarantees = new CusGuaranteeHeader.Loader(factory).Load<CusGuaranteeHeader>(guaranteeNumber, new ZQuery(CusPermitHeaderSchema.CPH_RN_NKCountryCode, new ZString[] { Core.Constants.CountryCodes.Ireland, Core.Constants.CountryCodes.UnitedKingdom }));
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
	}
}
