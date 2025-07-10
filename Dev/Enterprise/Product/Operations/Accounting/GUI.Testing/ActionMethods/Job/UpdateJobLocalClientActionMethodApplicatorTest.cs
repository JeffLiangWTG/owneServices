using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(UpdateJobLocalClientActionMethodApplicator))]
	public class UpdateJobLocalClientActionMethodApplicatorTest : UpdateJobActionMethodApplicatorBaseTest
	{
		public void TestLocalChargesAddr()
		{
			Applicator.LocalChargesAddr = TestObjectCreator.Debtor.MainAddress.PK;

			var collection = new OrganisationsFindBoxCollection(Factory);
			collection.FilterBusinessObjectDefaults.Add(
				new FilterBusinessObjectDefault("Organisation Types", "Property0",
					ZBool.True));

			AssertEquals(TestObjectCreator.Debtor.MainAddress.PK, Applicator.LocalChargesAddr);
			AssertEquals(collection.Count, Applicator.AddressList.Count);
			AssertNotNull(Applicator.LocalChargesAddrInfo);
		}

		public void TestValidateLocalChargesAddr()
		{
			Applicator.LocalChargesAddr = ZGuid.Invalid;

			AssertHasError(Applicator.LocalChargesAddrInfo, "Enter a valid selection.");
		}

		public void TestLocalBillingContact()
		{
			var contact = TestObjectCreator.CreateContact(TestObjectCreator.Debtor);
			Applicator.LocalBillingContact = contact.PK;

			AssertEquals(contact.PK, Applicator.LocalBillingContact);
			AssertNotNull(Applicator.LocalBillingContactInfo);
		}

		public void TestValidateLocalBillingContact()
		{
			Applicator.LocalBillingContact = ZGuid.Invalid;
			AssertHasError(Applicator.LocalBillingContactInfo, "Enter a valid selection.");
		}

		public void TestLocalZAddressWithContact()
		{
			AssertNotNull(Applicator.LocalZAddressWithContact);
		}

		protected override void SetupJobPropertySameWithApplicatorValue()
		{
			var contact = TestObjectCreator.CreateContact(TestObjectCreator.Debtor);
			Applicator.LocalBillingContact = contact.PK;
			Applicator.LocalChargesAddr = TestObjectCreator.Debtor.MainAddress.PK;
			Shipment1.Job.JH_OA_LocalChargesAddr = TestObjectCreator.Debtor.MainAddress.PK;
			Shipment1.Job.JH_OC_LocalBillingContact = contact.PK;
		}

		protected override void SetupJobPropertyInfoReadOnly()
		{
			var contact = TestObjectCreator.CreateContact(TestObjectCreator.Debtor);
			Applicator.LocalBillingContact = contact.PK;
			Applicator.LocalChargesAddr = TestObjectCreator.Debtor.MainAddress.PK;
			Shipment1.Job.SetReadOnlyIncludingChildren(true);
		}

		protected override void SetupApplicatorPropertyForValidation()
		{
			Applicator.LocalBillingContact = ZGuid.Invalid;
			Applicator.LocalChargesAddr = ZGuid.Invalid;
		}

		protected override void AssertUpdateJobPropertySuccessful()
		{
			var contact = TestObjectCreator.CreateContact(TestObjectCreator.Debtor);
			Applicator.LocalBillingContact = contact.PK;

			AssertNotEquals(contact.PK, Shipment1.Job.JH_OC_LocalBillingContact);
			AssertNotEquals(TestObjectCreator.Debtor.MainAddress.PK, Shipment1.Job.JH_OA_LocalChargesAddr);
			AssertNotEquals(contact.PK, Shipment2.Job.JH_OC_LocalBillingContact);
			AssertNotEquals(TestObjectCreator.Debtor.MainAddress.PK, Shipment2.Job.JH_OA_LocalChargesAddr);

			Applicator.LocalBillingContact = contact.PK;
			Applicator.LocalChargesAddr = TestObjectCreator.Debtor.MainAddress.PK;
			var jobNumber1 = Shipment1.Job.JH_JobNum;
			var jobNumber2 = Shipment2.Job.JH_JobNum;

			ApplyApplicator(new BusinessObject[] { Shipment1, Shipment2 }, $@"INFO: Job {jobNumber1}: Start Process.
INFO: Job {jobNumber1}: Processed.

INFO: Job {jobNumber2}: Start Process.
INFO: Job {jobNumber2}: Processed.");

			AssertEquals(contact.PK, Shipment1.Job.JH_OC_LocalBillingContact);
			AssertEquals(TestObjectCreator.Debtor.MainAddress.PK, Shipment1.Job.JH_OA_LocalChargesAddr);
			AssertEquals(contact.PK, Shipment2.Job.JH_OC_LocalBillingContact);
			AssertEquals(TestObjectCreator.Debtor.MainAddress.PK, Shipment2.Job.JH_OA_LocalChargesAddr);
		}

		public override string ExpectedValidationErrorLog => $@"INFO: Job {Shipment1.Job.JH_JobNum}: Start Process.
ERROR: Job has errors: Enter a valid Local Client.
";

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UpdateJobLocalClientActionMethodApplicator(Factory);
		}

		new UpdateJobLocalClientActionMethodApplicator Applicator => (UpdateJobLocalClientActionMethodApplicator)base.Applicator;
	}
}
