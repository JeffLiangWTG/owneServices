using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Mail.Business
{
	public class ImplementationMailItemCollection : EDIMailItemCollection
	{
		public ImplementationMailItemCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ImplementationMailItemCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override string MailApplicationCode
		{
			get { return EDIMailApplication.Implementation; }
		}
	}
}

