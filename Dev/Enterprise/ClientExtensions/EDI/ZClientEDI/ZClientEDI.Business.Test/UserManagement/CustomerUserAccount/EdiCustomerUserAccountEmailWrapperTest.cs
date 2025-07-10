using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.UserManagement.Business.Test
{
	[TestedType(typeof(EdiCustomerUserAccountEmailWrapper))]
	class EdiCustomerUserAccountEmailWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new EdiCustomerUserAccountEmailWrapper();
		}
	}
}
