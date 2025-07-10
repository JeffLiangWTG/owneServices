using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.Common
{
	public class EDIMessageCollection : Enterprise.Messaging.Business.EDIMessageCollection
	{
		public EDIMessageCollection(BusinessObject master)
			: base(master)
		{
		}

		public new EDIMessage this[int index]
		{
			get { return (EDIMessage)base[index]; }
		}

		public new EDIMessage AddNew()
		{
			return (EDIMessage)base.AddNew();
		}
	}
}
