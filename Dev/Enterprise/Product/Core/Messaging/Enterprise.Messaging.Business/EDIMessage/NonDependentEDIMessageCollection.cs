using System;
using CargoWise.EntityFramework;

namespace Enterprise.Messaging.Business
{
	public class NonDependentEDIMessageCollection : BusinessObjectCollection<EDIMessage>
	{
		public NonDependentEDIMessageCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public virtual new EDIMessage AddNew(Type businessObjectType)
		{
			return base.AddNew(businessObjectType);
		}
	}
}
