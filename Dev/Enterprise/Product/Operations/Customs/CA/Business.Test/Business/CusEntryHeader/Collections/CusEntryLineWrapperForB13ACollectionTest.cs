using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusEntryLineWrapperForB13ACollection))]
	sealed class CusEntryLineWrapperForB13ACollectionTest : NonPersistentBusinessObjectCollectionTestCase<CusEntryLineWrapperForB13ACollection>
	{
		#region Overrides of BusinessObjectCollectionBaseTestCase<BusinessObjectCollection>

		protected override CusEntryLineWrapperForB13ACollection GetCollectionToTest()
		{
			return new CusEntryLineWrapperForB13ACollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CusEntryLineWrapperForB13A();
		}

		#endregion
	}
}
