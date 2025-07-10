using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(RecipientContactPK))]
	sealed class RecipientContactPKTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < RecipientContact PK >", ValueProviderToTest.IsResponsibleForReplacing("< RecipientContact PK >", Passes.FirstPass));
			Assert("should match < RecipientContactPK >", ValueProviderToTest.IsResponsibleForReplacing("< RecipientContactPK >", Passes.FirstPass));
			Assert("should match < RecipientContact    PK>", ValueProviderToTest.IsResponsibleForReplacing("< RecipientContact    PK>", Passes.FirstPass));
			Assert("should match <RecipientContact PK >", ValueProviderToTest.IsResponsibleForReplacing("<RecipientContact PK >", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			((IReportForUnitTesting)Report).DeliveryContact = null;
			AssertEquals(ZGuid.Empty.ToString(), ValueProviderToTest.GetReplacement("<RecipientContactPK>", Report));

			var factory = new BusinessObjectFactory();
			((IReportForUnitTesting)Report).DeliveryContact = new DocDeliveryContact(factory);
			((IReportForUnitTesting)Report).DeliveryContact.Name = "Test Contact";
			AssertEquals(ZGuid.Empty.ToString(), ValueProviderToTest.GetReplacement("<RecipientContactPK>", Report));

			Report.DeliveryContact.OrgHeaderPK = factory.NewWithValidTestData<OrgHeader>().PK;
			var contact = Report.DeliveryContact.OrgHeader.Contacts.AddNew();
			contact.OC_ContactName = "Test Contact";
			AssertEquals(ZGuid.Empty.ToString(), ValueProviderToTest.GetReplacement("<RecipientContactPK>", Report));
			factory.Save();
			AssertEquals(contact.PK.ToString(), ValueProviderToTest.GetReplacement("<RecipientContactPK>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new RecipientContactPK();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgContact = Factory.NewWithPrimaryKey<OrgContact>(new Guid("B003D3B0-A36D-4590-A489-4FAFDFE541F5"));
			orgContact.OC_OH = org.PK;
			orgContact.OC_ContactName = "name";
			Factory.Save();

			var deliveryContact = new DocDeliveryContact(Factory);
			deliveryContact.OrgHeaderPK = org.PK;
			deliveryContact.Name = "name";
			((IReportForUnitTesting)Report).DeliveryContact = deliveryContact;
		}
	}
}
