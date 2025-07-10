using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Module
{
	public class ManifestForwardEDIMessageCollection : Enterprise.Messaging.Business.NonDependentEDIMessageCollection
	{
		public ManifestForwardEDIMessageCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new EDIMessage AddNew()
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
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.CAACI);
			result.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.ACIHouseBill);
			result.AddToFilter(EDIMessageSchema.EM_MessageSubType, MessageTypeList.Codes.ManifestForwardHouse);
			return result;
		}
	}
}
