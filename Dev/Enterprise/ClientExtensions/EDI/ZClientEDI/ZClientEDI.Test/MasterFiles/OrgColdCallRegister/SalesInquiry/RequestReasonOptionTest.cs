using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EDIWebSalesInquiry.RequestReasonOption))]
	class RequestReasonOptionTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new EDIWebSalesInquiry.RequestReasonOption(new CodeDescriptionBool());
		}
	}
}
