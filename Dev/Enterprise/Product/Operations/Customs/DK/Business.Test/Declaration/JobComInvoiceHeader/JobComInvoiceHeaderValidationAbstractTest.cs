using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DK.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(JobComInvoiceHeaderValidation))]
	abstract class JobComInvoiceHeaderValidationAbstractTest<T> : BusinessObjectValidationTestCase
		where T : JobComInvoiceHeaderValidation
	{
		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = MessageType;
			invoice = jobDeclaration.Invoices.AddNew();
			validation = GetValidation();
		}
		protected JobDeclaration jobDeclaration;
		protected JobComInvoiceHeader invoice;
		protected T validation;

		protected abstract string MessageType { get; }

		protected abstract T GetValidation();
	}
}
