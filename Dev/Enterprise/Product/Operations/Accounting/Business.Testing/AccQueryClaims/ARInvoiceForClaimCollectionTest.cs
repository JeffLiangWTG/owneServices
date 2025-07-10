using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	[TestedType(typeof(ARInvoiceForClaimCollection))]
	public class ARInvoiceForClaimCollectionTest : TransactionHeaderCollectionForClaimTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ARInvoiceForClaimCollection(QueryClaim, new ZQuery());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(ARInvoice));
		}

		protected override Type GetQueryClaimType()
		{
			return typeof(ARAccQueryClaim);
		}

		protected override Type GetTransactionType()
		{
			return typeof(ARInvoice);
		}
	}
}
