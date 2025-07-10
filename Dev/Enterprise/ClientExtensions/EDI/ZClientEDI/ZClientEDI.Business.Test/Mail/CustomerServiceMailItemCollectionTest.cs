using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Mail.Business.Test
{
	[TestedType(typeof(CustomerServiceMailItemCollection))]
	internal class CustomerServiceMailItemCollectionTest : EDIMailItemCollectionTestCase
	{
		protected override string ExpectedMailApplicationCode
		{
			get { return EDIMailApplication.CustomerService; }
		}

		protected override EDIMailItemCollection GetEDIMailItemCollection(BusinessObjectFactory factory)
		{
			return new CustomerServiceMailItemCollection(factory);
		}
	}
}
