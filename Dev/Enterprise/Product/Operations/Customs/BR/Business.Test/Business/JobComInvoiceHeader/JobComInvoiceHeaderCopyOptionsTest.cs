using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeaderCopyOptions))]
	class JobComInvoiceHeaderCopyOptionsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var copyObject = new JobComInvoiceHeaderCopyOptions();

			AssertEquals("AllLines should be", ZBool.True, copyObject.AllLines);
			AssertEquals("OnlyLinesRequireLicense should be", ZBool.False, copyObject.OnlyLinesRequireLicense);
		}

		#region Overrides of BusinessObjectBaseTestCase

		protected override BusinessObject GetNewBusinessObject()
		{
			return new JobComInvoiceHeaderCopyOptions();
		}

		#endregion
	}
}

