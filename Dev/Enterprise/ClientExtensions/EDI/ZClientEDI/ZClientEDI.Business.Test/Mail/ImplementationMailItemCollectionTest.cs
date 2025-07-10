using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Mail.Business.Test
{
	[TestedType(typeof(ImplementationMailItemCollection))]
	internal class ImplementationMailItemCollectionTest : EDIMailItemCollectionTestCase
	{
		protected override string ExpectedMailApplicationCode
		{
			get { return EDIMailApplication.Implementation; }
		}

		protected override EDIMailItemCollection GetEDIMailItemCollection(BusinessObjectFactory factory)
		{
			return new ImplementationMailItemCollection(factory);
		}
	}
}
