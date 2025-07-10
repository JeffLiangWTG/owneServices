using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	class GatewayProfitRedistributionApportionmentCriteria
	{
		public GatewayProfitRedistributionApportionmentCriteria(BusinessObjectFactory factory, ForwardingProfitShareRedistribution forwardingProfitShareRedistribution, OrgProfitShareDetails orgProfitShareDetails)
			: this(factory,
				  forwardingProfitShareRedistribution,
				  orgProfitShareDetails?.O4_GatewayProfitApportionmentMethod,
				  orgProfitShareDetails?.O4_AgreementType,
				  orgProfitShareDetails != null && orgProfitShareDetails.O4_ShareLosses,
				  orgProfitShareDetails != null && orgProfitShareDetails.O4_AgreementType == OrgProfitShareDetailsLookups.AgreementTypeUserDefined ? orgProfitShareDetails.GetUserDefinedChargeCodes() : null)
		{
		}

		public GatewayProfitRedistributionApportionmentCriteria(BusinessObjectFactory factory, ForwardingProfitShareRedistribution forwardingProfitShareRedistribution, string profitApportionmentMethod, string agreementType, bool shareLosses, AccChargeCode[] userDefinedChargeCodes)
		{
			//Note we have validation for ProfitApportionmentMethod, ShareLosses and ApplyTo fields to be common across all rules passed for processing
			//Assigning more properties without adding validation will be harmful
			this.ProfitShareRedistribution = forwardingProfitShareRedistribution;
			this.ProfitApportionmentMethod = profitApportionmentMethod;
			this.ShareLosses = shareLosses;
			this.agreementType = agreementType;
			this.orgProfitShareDetailsHelper = new OrgProfitShareDetailsHelper(factory, userDefinedChargeCodes);
		}

		readonly OrgProfitShareDetailsHelper orgProfitShareDetailsHelper;
		readonly string agreementType;

		public ForwardingProfitShareRedistribution ProfitShareRedistribution { get; }
		public string ProfitApportionmentMethod { get; }
		public bool ShareLosses { get; }

		public bool IsChargeGroupProfitShared(AccChargeCode chargeCode, PaymentTermInfos paymentTermInfos) => orgProfitShareDetailsHelper.IsChargeGroupProfitShared(agreementType, chargeCode, paymentTermInfos);
	}
}
