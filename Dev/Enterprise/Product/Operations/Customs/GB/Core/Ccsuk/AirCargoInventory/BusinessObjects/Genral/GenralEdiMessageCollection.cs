using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.GENRAL;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral
{
	public class GenralEdiMessageCollection : BusinessObjectCollection<GenralEdiMessage>
	{
		public GenralEdiMessageCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override ZQuery CreateRelationshipFilter()
		{
			var filter = base.CreateRelationshipFilter();
			filter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
			filter.AddToFilter(EDIMessageSchema.EM_MessageType, GenralMessageGenerator.GenralMessageCodeShortForMessageType);
			return filter;
		}
	}
}
