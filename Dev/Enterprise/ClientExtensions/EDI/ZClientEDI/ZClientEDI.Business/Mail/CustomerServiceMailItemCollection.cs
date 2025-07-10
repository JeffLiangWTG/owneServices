using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Mail.Business
{
	public class CustomerServiceMailItemCollection : EDIMailItemCollection
	{
		public CustomerServiceMailItemCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CustomerServiceMailItemCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override string MailApplicationCode
		{
			get { return EDIMailApplication.CustomerService; }
		}
	}
}

