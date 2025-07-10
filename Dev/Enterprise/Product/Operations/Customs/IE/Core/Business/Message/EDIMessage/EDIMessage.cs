using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business
{
	public abstract class EDIMessage : Enterprise.Messaging.Business.EDIMessage
	{
		protected EDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new static readonly TypeDecider TypeDecider = new EDIMessageTypeDecider();
	}
}
