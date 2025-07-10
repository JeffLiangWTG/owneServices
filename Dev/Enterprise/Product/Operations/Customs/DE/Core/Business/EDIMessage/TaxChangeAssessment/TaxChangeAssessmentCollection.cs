using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business
{
	public class TaxChangeAssessmentCollection : ActiveBusinessObjectCollection<TaxChangeAssessment>
	{
		public TaxChangeAssessmentCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();

			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.DECustomsAtlasSystem);
			result.AddToFilter(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.Import);
			result.AddToFilter(EDIMessageSchema.EM_MessageSubType, ImportMessageSubTypeList.Codes.SubsequentRaiseRefundOrAbatement);
			result.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIInterchange.Direction.Receive);
			result.AddToFilter(EDIMessageSchema.EM_ApplicationReference, SQLComparisonOperator.StartsWith, ApplicationReferencePrefix);
			result.AddToFilter(EDIMessageSchema.EM_GB, GlbCompany.CurrentCompany.Branches.GetPKs());

			return result;
		}

		protected override bool AllowNew => false;

		const string ApplicationReferencePrefix = "NSTAX";
	}
}
