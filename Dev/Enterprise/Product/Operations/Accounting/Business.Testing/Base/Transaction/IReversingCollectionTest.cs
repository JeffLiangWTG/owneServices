using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	[TestedType(typeof(IReversingCollection))]
	public class IReversingCollectionTest : ITransactionCollectionTest
	{
		[ExpectException(typeof(NotSupportedException))]
		public new void TestAddNew()
		{
			TestCollection.AddNew();
		}

		public void TestAllowNew()
		{
			AssertEquals("Allow New by default", false, TestCollection.AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals("Allow New by default", false, TestCollection.AllowRemove);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new IReversingCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new IReversingImplicitlyImplementedWrapperForBinding(Factory.New<APInvoice>());
		}

		protected IReversingCollection TestCollection;

		protected override void SetUp()
		{
			base.SetUp();
			TestCollection = (IReversingCollection)GetCollectionToTest();
		}

		#endregion
	}
}
