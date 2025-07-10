using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(APInvoiceChargesCollection))]
	class APInvoiceChargesCollectionTest : NonPersistentBusinessObjectCollectionTestCase<APInvoiceChargesCollection>
	{
		public void TestDefaults()
		{
			var collection = GetCollectionToTest();
			Assert("AllowNew", !collection.AllowNew);
			Assert("AllowRemove", !collection.AllowRemove);
			Assert("HumanReadableName", collection.HumanReadableName.IsEmpty);
			AssertExceptionThrown<NotSupportedException>(() => collection.AddNew());
		}

		protected override APInvoiceChargesCollection GetCollectionToTest()
		{
			return new APInvoiceChargesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new APInvoiceCharges("", "", ZGuid.Empty, "", null);
		}
	}
}
