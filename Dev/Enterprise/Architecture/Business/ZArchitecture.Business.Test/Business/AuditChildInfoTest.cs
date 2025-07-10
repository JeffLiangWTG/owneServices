using System;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class AuditChildInfoTest : TestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new AuditChildInfo((SchemaGuidColumn)null, null));
			AssertExceptionThrown<ArgumentNullException>(() => new AuditChildInfo((SchemaIntColumn)null, null));

			AssertNoExceptionThrown(() => new AuditChildInfo(JobComInvoiceHeaderSchema.JZ_JE, null));
			AssertNoExceptionThrown(() => new AuditChildInfo(JobComInvoiceHeaderSchema.JZ_ClusterKey, null));
		}

		public void TestEquals()
		{
			var auditChildInfo1 = new AuditChildInfo(JobComInvoiceHeaderSchema.JZ_JE, JobComInvoiceHeaderSchema.JZ_InvoiceNumber);
			var auditChildInfo2 = new AuditChildInfo(JobComInvoiceHeaderSchema.JZ_JE, JobComInvoiceHeaderSchema.JZ_InvoiceNumber);
			var auditChildInfo3 = new AuditChildInfo(JobComInvoiceHeaderSchema.JZ_ClusterKey, JobComInvoiceHeaderSchema.JZ_InvoiceNumber);
			var auditChildInfo4 = new AuditChildInfo(JobComInvoiceHeaderSchema.JZ_JE, JobComInvoiceHeaderSchema.JZ_Description);

			AssertEquals(auditChildInfo1, auditChildInfo2);
			AssertNotEquals(auditChildInfo1, auditChildInfo3);
			AssertNotEquals(auditChildInfo1, auditChildInfo4);

			AssertEquals(auditChildInfo2, auditChildInfo1);
			AssertNotEquals(auditChildInfo2, auditChildInfo3);
			AssertNotEquals(auditChildInfo2, auditChildInfo4);

			AssertNotEquals(auditChildInfo3, auditChildInfo1);
			AssertNotEquals(auditChildInfo3, auditChildInfo2);
			AssertNotEquals(auditChildInfo3, auditChildInfo4);
		}

		public void TestToString()
		{
			AssertEquals("JZ_JE - JZ_InvoiceNumber", new AuditChildInfo(JobComInvoiceHeaderSchema.JZ_JE, JobComInvoiceHeaderSchema.JZ_InvoiceNumber).ToString());
			AssertEquals("JZ_ClusterKey - JZ_InvoiceNumber", new AuditChildInfo(JobComInvoiceHeaderSchema.JZ_ClusterKey, JobComInvoiceHeaderSchema.JZ_InvoiceNumber).ToString());
		}
	}
}
