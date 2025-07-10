using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC225CProcessor : NCTSLinkedGuaranteeMessageProcessor<NCTSInboundEDIMessage, CC225CProvider>
	{
		public CC225CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("BCA328C7-C616-4BDC-AE30-42842712D420", "CC225C: GUARANTEE UPDATE NOTIFICATION");

		protected override Type MessageInterpreterType => typeof(CC225CMessageInterpreter);

		protected override bool MustHaveLinkedObject => false;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, NCTSInboundEDIMessage message, CC225CProvider provider)
		{
			foreach (var guaranteeReferenceProvider in provider.GuaranteeReferences)
			{
				var cusGuaranteeHeader = FindGuarantee(factory, guaranteeReferenceProvider);
				if (cusGuaranteeHeader != null)
				{
					UpdateGuarantee(cusGuaranteeHeader, guaranteeReferenceProvider);
				}
			}
		}

		CusGuaranteeHeader FindGuarantee(BusinessObjectFactory factory, CC225CGuaranteeReferenceProvider guaranteeReferenceProvider)
		{
			CusGuaranteeHeader result = null;
			var cusGuaranteeHeaders = new EU.Business.CusGuaranteeHeader.Loader(factory).Load<CusGuaranteeHeader>(guaranteeReferenceProvider.GRN, new ZQuery(CusPermitHeaderSchema.CPH_RN_NKCountryCode, new ZString[] { Core.Constants.CountryCodes.Ireland, Core.Constants.CountryCodes.UnitedKingdom }));
			switch (cusGuaranteeHeaders.Length)
			{
				case 0:
					break;
				case 1:
					result = cusGuaranteeHeaders[0];
					break;
				default:
					foreach (var guarantee in cusGuaranteeHeaders)
					{
						if (result == null || result.CPH_EndDate < guarantee.CPH_EndDate)
						{
							result = guarantee;
						}
					}
					break;
			}
			return result;
		}

		void UpdateGuarantee(CusGuaranteeHeader cusGuaranteeHeader, CC225CGuaranteeReferenceProvider guaranteeReferenceProvider)
		{
			cusGuaranteeHeader.CPH_StartDate = guaranteeReferenceProvider.ValidityDate;
			cusGuaranteeHeader.CPH_EndDate = guaranteeReferenceProvider.InvalidityDate;
		}

		protected override bool NeedToSendEmailNotification(EDIMessage message) => false;
	}
}
