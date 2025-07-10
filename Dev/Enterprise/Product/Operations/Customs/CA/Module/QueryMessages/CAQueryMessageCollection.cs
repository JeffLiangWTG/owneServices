using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Module
{
	/// <summary>
	/// Query messages that are not attached to any jobs therefore, cannot be viewed anywhere in the system
	/// </summary>
	public class CAQueryMessageCollection : Enterprise.Messaging.Business.NonDependentEDIMessageCollection
	{
		public CAQueryMessageCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public virtual new EDIMessage AddNew()
		{
			return (EDIMessage)base.AddNew();
		}

		public new EDIMessage this[int index]
		{
			get { return (EDIMessage)base[index]; }
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.CAIMP);
			result.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, null);
			var messageTypeFilter = new ZQuery();
			messageTypeFilter.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.Query);
			messageTypeFilter.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.SyntaxError);
			result.AddToFilter(messageTypeFilter);
			return result;
		}
	}
}
