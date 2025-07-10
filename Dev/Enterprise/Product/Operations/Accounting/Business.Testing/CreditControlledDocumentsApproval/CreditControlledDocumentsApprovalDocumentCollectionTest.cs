using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(CreditControlledDocumentsApprovalDocumentCollection))]
	public class CreditControlledDocumentsApprovalDocumentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CreditControlledDocumentsApprovalDocumentCollection>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(CreditControlledDocumentsApprovalDocumentCollection);
		}

		protected override CreditControlledDocumentsApprovalDocumentCollection GetCollectionToTest()
		{
			return new CreditControlledDocumentsApprovalDocumentCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CreditControlledDocumentsApprovalDocument("hello!");
		}
	}
}
