using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(RatingWrapperCollection))]
	sealed class RatingWrapperCollectionTest : Base.Testing.GenericWrapperCollectionTest<RatingWrapperCollection>
	{
		public void TestLoadFromOrgOpportunity()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Some Enquiry Org";
			org.MainAddress.OA_Address1 = "1 Street";
			org.OH_RL_NKClosestPort = "AUSYD";

			var opportunity = org.SalesOpportunities.AddNew();
			var pivots1 = opportunity.RelatedChildActivityPivotCollection;

			var rating3 = Factory.New<Quote>();
			rating3.TH_QuoteNumber = "003";
			rating3.TH_OH = org.PK;
			rating3.TH_OneTimeQuote = false;
			var pivot3 = pivots1.AddNew();
			pivot3.RAP_ParentActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			pivot3.RAP_ChildActivityTableCode = RatingHeaderSchema.Constants.Prefix;
			pivot3.RAP_ChildActivityID = rating3.PK;

			var rating2 = Factory.New<Quote>();
			rating2.TH_QuoteNumber = "002";
			rating2.TH_OH = org.PK;
			rating2.TH_OneTimeQuote = false;
			var pivot2 = pivots1.AddNew();
			pivot2.RAP_ParentActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			pivot2.RAP_ChildActivityTableCode = RatingHeaderSchema.Constants.Prefix;
			pivot2.RAP_ChildActivityID = rating2.PK;

			var rating1 = Factory.New<Quote>();
			rating1.TH_QuoteNumber = "001";
			rating1.TH_OH = org.PK;
			rating1.TH_OneTimeQuote = false;
			var pivot1 = pivots1.AddNew();
			pivot1.RAP_ParentActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			pivot1.RAP_ChildActivityTableCode = RatingHeaderSchema.Constants.Prefix;
			pivot1.RAP_ChildActivityID = rating1.PK;

			Factory.Save();

			var wrapper = FreightWrapper.New(opportunity, Factory);
			var resultList = wrapper[0].SalesRelations.Quotations;

			AssertEquals("First node should be 001", (resultList[0].WrappedObject as IRatingHeader).TH_QuoteNumber, "001/A");
			AssertEquals("First node should be 002", (resultList[1].WrappedObject as IRatingHeader).TH_QuoteNumber, "002/A");
			AssertEquals("First node should be 003", (resultList[2].WrappedObject as IRatingHeader).TH_QuoteNumber, "003/A");
		}

		public void TestExcludeRatingsFromOtherCompanies()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Some Enquiry Org";
			org.MainAddress.OA_Address1 = "1 Street";
			org.OH_RL_NKClosestPort = "AUSYD";
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			var opportunity = org.SalesOpportunities.AddNew();
			var pivots1 = opportunity.RelatedChildActivityPivotCollection;

			var rating3 = Factory.NewWithValidTestData<Quote>();
			rating3.TH_QuoteNumber = "003";
			rating3.TH_OH = org.PK;
			rating3.TH_OneTimeQuote = false;
			rating3.TH_GC = Env.CurrentCompanyPK;
			var pivot3 = pivots1.AddNew();
			pivot3.RAP_ParentActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			pivot3.RAP_ChildActivityTableCode = RatingHeaderSchema.Constants.Prefix;
			pivot3.RAP_ChildActivityID = rating3.PK;

			var rating2 = Factory.NewWithValidTestData<Quote>();
			rating2.TH_QuoteNumber = "002";
			rating2.TH_OH = org.PK;
			rating2.TH_OneTimeQuote = false;
			rating2.TH_GC = otherCompany.PK;
			var pivot2 = pivots1.AddNew();
			pivot2.RAP_ParentActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			pivot2.RAP_ChildActivityTableCode = RatingHeaderSchema.Constants.Prefix;
			pivot2.RAP_ChildActivityID = rating2.PK;

			var rating1 = Factory.NewWithValidTestData<Quote>();
			rating1.TH_QuoteNumber = "001";
			rating1.TH_OH = org.PK;
			rating1.TH_OneTimeQuote = false;
			rating1.TH_GC = otherCompany.PK;
			var pivot1 = pivots1.AddNew();
			pivot1.RAP_ParentActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			pivot1.RAP_ChildActivityTableCode = RatingHeaderSchema.Constants.Prefix;
			pivot1.RAP_ChildActivityID = rating1.PK;

			Factory.Save();

			var wrapper = FreightWrapper.New(opportunity, Factory);
			var resultList = wrapper[0].SalesRelations.Quotations;

			AssertEquals("Only 1 node can be found", resultList.Count, 1);
			AssertEquals("First node should be 003", (resultList[0].WrappedObject as IRatingHeader).TH_QuoteNumber, "003/A");
		}
		#region Imeplementation
		protected override RatingWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new RatingWrapperCollection(Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new RatingWrapperFromQuotation(null, Factory);
		}

		#endregion
	}
}
