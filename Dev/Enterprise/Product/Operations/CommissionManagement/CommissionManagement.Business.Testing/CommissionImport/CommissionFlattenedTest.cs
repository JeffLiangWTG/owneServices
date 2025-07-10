using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(CommissionFlattened))]
	public class CommissionFlattenedTest : NonPersistentBusinessObjectTestCase
	{
		#region Properties

		public void TestPropertyMaxLengths()
		{
			AssertEquals(GlbCompany.Schema.GC_CodeMaxLength, CommissionFlattened.Schema.CompanyCodeMaxLength);
			AssertEquals(Math.Max(AccTransactionHeader.Schema.AH_TransactionNumMaxLength, JobHeader.Schema.JH_JobNumMaxLength), CommissionFlattened.Schema.GroupingSourceCodeMaxLength);
			AssertEquals(AccTransactionHeader.Schema.AH_TransactionTypeMaxLength, CommissionFlattened.Schema.InvoiceTypeMaxLength);
			AssertEquals(OrgHeader.Schema.OH_CodeMaxLength, CommissionFlattened.Schema.PartyCodeMaxLength);
			AssertEquals(GlbStaff.Schema.GS_CodeMaxLength, CommissionFlattened.Schema.StaffCodeMaxLength);
			AssertEquals(RefCurrency.Schema.RX_CodeMaxLength, CommissionFlattened.Schema.CommissionCurrencyCodeMaxLength);
			AssertEquals(AccCommissionLine.Schema.CL0_CommissionTypeMaxLength, CommissionFlattened.Schema.CL0_CommissionTypeMaxLength);
		}

		#endregion
	}
}
