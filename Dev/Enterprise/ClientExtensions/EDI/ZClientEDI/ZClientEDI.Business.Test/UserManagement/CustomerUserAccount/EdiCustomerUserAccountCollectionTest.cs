namespace Enterprise.Client.EDI.UserManagement.Business.Testing
{
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(EdiCustomerUserAccountCollection))]
	public class EdiCustomerUserAccountCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new EdiCustomerUserAccountCollection(Factory);
		}

		#endregion
	}
}
