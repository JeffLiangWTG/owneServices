using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class DummyCMRMessageRespondee : DummyEnterpriseBusinessObject, ICMRMessageRespondee
	{
		public DummyCMRMessageRespondee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString Details
		{
			get { return "Details"; }
		}

		public EDIMessageCollection Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new EDIMessageCollection(this);
				}
				return messages;
			}
		}
		EDIMessageCollection messages;

		public ZString ShortDescription
		{
			get { return "ShortDescription"; }
		}
	}
}
