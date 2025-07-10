using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.ZArchitecture.Schema;
using CusGuaranteeHeader = Enterprise.Customs.IE.Business.CusGuaranteeHeader;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC228CProcessor : NCTSGuaranteeMessageProcessor<NCTSInboundEDIMessage, CC228CProvider>
	{
		public CC228CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override bool MustHaveLinkedObject => false;

		protected override string MessageFriendlyNameCore => Res.GetString("9B33DF31-0FF2-43E4-BF87-FA064C492434", "CC228C: COMPREHENSIVE GUARANTEE CANCELLATION LIABILITY LIBERATION");

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, CC228CProvider provider)
		{
			foreach (var guaranteeReference in provider.GuaranteeReferences)
			{
				var guarantees = new EU.Business.CusGuaranteeHeader.Loader(messageAttachee.Factory).Load<CusGuaranteeHeader>(
					guaranteeReference.GRN,
					new ZQuery(CusPermitHeaderSchema.CPH_RN_NKCountryCode,
						new ZString[]
						{
							Core.Constants.CountryCodes.Ireland, Core.Constants.CountryCodes.UnitedKingdom
						}));
				foreach (var guarantee in guarantees)
				{
					if (guarantee.CPH_EndDate < guaranteeReference.InvalidityDate)
					{
						guarantee.CPH_EndDate = (ZDate)guaranteeReference.InvalidityDate;
					}
				}
			}
		}

		protected override Type MessageInterpreterType => typeof(CC228CMessageInterpreter);
	}
}
