using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(TaxChangeAssessmentCollection))]
	class TaxChangeAssessmentCollectionTest : ActiveBusinessObjectCollectionTestCase<TaxChangeAssessmentCollection>
	{
		public void TestRelationshipFilter()
		{
			var taxChangeAssessment1 = CreateTaxChangeAssessment();

			var taxChangeAssessment2 = CreateTaxChangeAssessment();
			taxChangeAssessment2.EM_ApplicationCode = EDIInterchange.ApplicationCodes.DECustomsAesSystem;

			var taxChangeAssessment3 = CreateTaxChangeAssessment();
			taxChangeAssessment3.EM_MessageType = EDIMessageTypeList.Codes.AES;

			var taxChangeAssessment4 = CreateTaxChangeAssessment();
			taxChangeAssessment4.EM_MessageSubType = ImportMessageSubTypeList.Codes.BondedWarehouseCompletionInformation;

			var taxChangeAssessment5 = CreateTaxChangeAssessment();
			taxChangeAssessment5.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;

			var taxChangeAssessment6 = CreateTaxChangeAssessment();
			taxChangeAssessment6.EM_ApplicationReference = "ABCDEF";

			var taxChangeAssessment7 = CreateTaxChangeAssessment();
			taxChangeAssessment7.EM_ApplicationReference = "NSTAXK";

			var taxChangeAssessment8 = CreateTaxChangeAssessment();
			taxChangeAssessment8.EM_GB = Factory.New<GlbCompany>().Branches.AddNew().PK;

			var taxChangeAssessment9 = CreateTaxChangeAssessment();
			taxChangeAssessment9.EM_GB = GlbCompany.CurrentCompany.Branches.AddNew().PK;

			var collection = new TaxChangeAssessmentCollection(Factory);
			CombineAssertions(() =>
			{
				AssertEquals("Baseline", true, taxChangeAssessment1.MatchesFilter(collection.CompleteFilter));
				AssertEquals("EM_ApplicationCode <> 'DEA'", false, taxChangeAssessment2.MatchesFilter(collection.CompleteFilter));
				AssertEquals("EM_MessageType <> 'IMP'", false, taxChangeAssessment3.MatchesFilter(collection.CompleteFilter));
				AssertEquals("EM_MessageSubType <> 'NEE'", false, taxChangeAssessment4.MatchesFilter(collection.CompleteFilter));
				AssertEquals("EM_ReceiveTransmit <> 'RCV'", false, taxChangeAssessment5.MatchesFilter(collection.CompleteFilter));
				AssertEquals("EM_ApplicationReference doesn't start with 'NSTAX'", false, taxChangeAssessment6.MatchesFilter(collection.CompleteFilter));
				AssertEquals("EM_ApplicationReference starts with 'NSTAX'", true, taxChangeAssessment7.MatchesFilter(collection.CompleteFilter));
				AssertEquals("EM_GB is not from current company", false, taxChangeAssessment8.MatchesFilter(collection.CompleteFilter));
				AssertEquals("EM_GB is from current company", true, taxChangeAssessment9.MatchesFilter(collection.CompleteFilter));
			});
		}

		public void TestAllowNew()
		{
			AssertEquals(false, ((IBindingList)Collection).AllowNew);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => CreateTaxChangeAssessment();

		protected override TaxChangeAssessmentCollection GetCollectionToTest() => new TaxChangeAssessmentCollection(Factory);

		TaxChangeAssessment CreateTaxChangeAssessment()
		{
			var taxChangeAssessment = Factory.New<TaxChangeAssessment>();
			taxChangeAssessment.EM_ApplicationReference = "NSTAXJ";
			return taxChangeAssessment;
		}
	}
}
