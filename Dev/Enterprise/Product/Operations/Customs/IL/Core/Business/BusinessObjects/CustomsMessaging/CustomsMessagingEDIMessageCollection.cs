using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IL.Business
{
	public class CustomsMessagingEDIMessageCollection : ActiveBusinessObjectCollection<ILEDIMessage>
	{
		public CustomsMessagingEDIMessageCollection(BusinessObject owner)
			: base(owner.Factory)
		{
			this.owner = owner;
			SetReadOnlyIncludingChildren(true);
			ApplySort(EDIMessage.Schema.EM_SystemCreateTimeUtc, ListSortDirection.Descending);
		}

		#region Implementation
		
		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, owner.PK);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ILCustoms);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, new[] { ILMessageTypeList.Codes.DLO, ILMessageTypeList.Codes.GPM });
			return query;
		}
		
		#endregion

		readonly BusinessObject owner;
	}
}
