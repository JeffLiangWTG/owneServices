using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Edifact;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.REMADV;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPAYSTDMessage : CMRMessage
	{
		public CMRPAYSTDMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool HasCustomsPayment
		{
			get
			{
				if (!isHasCustomsPaymentCalculated)
				{
					foreach (MOASegment mOA in REMADV.MOA)
					{
						if (mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier == MonetaryAmountTypeCodeQualifierList.TotalAmount)
						{
							fHasCustomsPayment = true;
							break;
						}
					}
					isHasCustomsPaymentCalculated = true;
				}
				return fHasCustomsPayment;
			}
		}
		bool fHasCustomsPayment;
		bool isHasCustomsPaymentCalculated;

		REMADVMessage REMADV
		{
			get
			{
				if (fREMADV == null)
				{
					fREMADV = GetAutoEdifactMessageUsingNamedFactory(AuEdifactMessageFactory.AUCMessageFactory, new UNOCCMRCharacterSet()) as REMADVMessage;
				}
				return fREMADV;
			}
		}
		REMADVMessage fREMADV;

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.PAYSTD;
		}

		#endregion
	}
}
