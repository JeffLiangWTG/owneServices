using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	class ChargeGroupValidation
	{
		public ChargeGroupValidation()
		{
			chargeCodeLookup = new Dictionary<ZGuid, ChargeGroupValidationInfo>();
		}

		readonly Dictionary<ZGuid, ChargeGroupValidationInfo> chargeCodeLookup;

		public void AddItem(Charge charge)
		{
			if (charge != null)
			{
				var chargeCodePk = charge.JR_AC;

				if (!chargeCodeLookup.TryGetValue(chargeCodePk, out ChargeGroupValidationInfo chargeGroupValidation))
				{
					chargeGroupValidation = new ChargeGroupValidationInfo();
					chargeCodeLookup.Add(chargeCodePk, chargeGroupValidation);
				}

				chargeGroupValidation.Prepare(charge);
			}
		}

		public bool HasSameRelatedJobNumberAndSellReferenceNumberMoreThanOnce(ChargeWithCost charge)
		{
			if (charge != null && chargeCodeLookup.TryGetValue(charge.JR_AC, out ChargeGroupValidationInfo value))
			{
				return value.HasSameRelatedJobNumberMoreThanOnce(charge.JR_Calc_RelatedJobNumber)
					&& value.HasSameSellReferenceNumberMoreThanOnce(charge.JR_SellReference);
			}
			return false;
		}

		public bool HasMultipleGatewayChargesUsingSameChargeCode(Charge charge, (IOrgHeader sendingAgent, IOrgHeader receivingAgent) gatewayAgents)
		{
			if (charge != null && chargeCodeLookup.TryGetValue(charge.JR_AC, out ChargeGroupValidationInfo value))
			{
				return value.HasMultipleGatewayChargesUsingSameChargeCode(gatewayAgents);
			}
			return false;
		}

		class ChargeGroupValidationInfo
		{
			readonly List<ZString> relatedJobNumbers = new List<ZString>();
			readonly List<ZString> sellReferenceNumbers = new List<ZString>();
			readonly List<ZGuid> debtors = new List<ZGuid>();

			public void Prepare(Charge charge)
			{
				if (!charge.IsRevenuePosted)
				{
					var relatedJobNumber = charge.JR_Calc_RelatedJobNumber;
					relatedJobNumber = UseEmptyJobNumberStringIfRelatedJobNumberIsEmpty(relatedJobNumber);
					relatedJobNumbers.Add(relatedJobNumber);
				}

				if (charge.JR_LocalSellAmt != 0 && !charge.JR_OH_SellAccount.IsEmpty)
				{
					debtors.Add(charge.JR_OH_SellAccount);
				}

				sellReferenceNumbers.Add(charge.JR_SellReference);
			}

			public bool HasSameRelatedJobNumberMoreThanOnce(ZString relatedJobNumber)
			{
				relatedJobNumber = UseEmptyJobNumberStringIfRelatedJobNumberIsEmpty(relatedJobNumber);
				return relatedJobNumbers.Count(x => x == relatedJobNumber) > 1;
			}

			public bool HasSameSellReferenceNumberMoreThanOnce(ZString sellReferenceNumber)
			{
				return sellReferenceNumbers.Count(x => x == sellReferenceNumber) > 1;
			}

			public bool HasMultipleGatewayChargesUsingSameChargeCode((IOrgHeader sendingAgent, IOrgHeader receivingAgent) gatewayAgents)
			{
				var sendingAgent = gatewayAgents.sendingAgent;
				var chargesWithSendingAgentAsDebtor = sendingAgent == null ? 0 : debtors.Count(x => x == sendingAgent.PK);
				var receivingAgent = gatewayAgents.receivingAgent;
				var chargesWithReceivingAgentAsDebtor = receivingAgent == null ? 0 : debtors.Count(x => x == receivingAgent.PK);
				return (chargesWithSendingAgentAsDebtor + chargesWithReceivingAgentAsDebtor) > 1;
			}

			ZString UseEmptyJobNumberStringIfRelatedJobNumberIsEmpty(ZString relatedJobNumber)
			{
				if (relatedJobNumber.IsEmpty)
				{
					relatedJobNumber = (NoResString)"<empty>"; //existing logic
				}
				return relatedJobNumber;
			}
		}
	}
}
