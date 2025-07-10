using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EDIWebSalesInquiry.BusinessTypeOption))]
	class BusinessTypeOptionTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new EDIWebSalesInquiry.BusinessTypeOption("Warehouse");
		}
	}
}
