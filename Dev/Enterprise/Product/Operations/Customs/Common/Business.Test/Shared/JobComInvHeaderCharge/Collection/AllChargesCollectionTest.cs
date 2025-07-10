using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Common.Testing
{
	[TestedType(typeof(AllChargesCollection))]
	class AllChargesCollectionTest : BusinessObjectCollectionTestCase
	{
		[ExpectNoExceptions]
		public override void TestAddNew()
		{
			NUnit.Framework.Assert.That(true);
		}

		[ExpectNoExceptions]
		public void TestDoesNotAllowNew()
		{
			NUnit.Framework.Assert.That(((ICommonInvoice)Declaration).AllCharges.AllowNew, Is.EqualTo(false), "AllowNew");
		}

		[ExpectException(typeof(NoConcreteTypeException))]
		public void TestAddNewThrowException()
		{
			((ICommonInvoice)Declaration).AllCharges.AddNew();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return ((ICommonInvoice)Declaration).AllCharges;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<TestCharge>();
		}

		TestDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<TestDeclaration>()); }
		}
		TestDeclaration declaration;
	}
}
