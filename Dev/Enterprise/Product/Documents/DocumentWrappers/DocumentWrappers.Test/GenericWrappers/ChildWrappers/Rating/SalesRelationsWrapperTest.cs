using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.MasterFiles
{
	[TestedType(typeof(SalesRelationsWrapper))]
	sealed class SalesRelationsWrapperTest : GenericWrapperTest
	{
		#region test functions
		public void TestQuotationsFromOpportunity()
		{
			Rating1.TH_OneTimeQuote = false;
			Rating2.TH_OneTimeQuote = false;
			Rating3.TH_OneTimeQuote = false;
			Factory.Save();

			var wrapper = FreightWrapper.New(Opportunity, Factory);
			var resultList = wrapper[0].SalesRelations.Quotations;

			AssertEquals("First node should be 001", (resultList[0].WrappedObject as IRatingHeader).TH_QuoteNumber, "001/A");
			AssertEquals("First node should be 002", (resultList[1].WrappedObject as IRatingHeader).TH_QuoteNumber, "002/A");
			AssertEquals("First node should be 003", (resultList[2].WrappedObject as IRatingHeader).TH_QuoteNumber, "003/A");
		}

		public void TestOneOffQuotesFromOpportunity()
		{
			Rating1.TH_OneTimeQuote = true;
			Rating2.TH_OneTimeQuote = true;
			Rating3.TH_OneTimeQuote = true;
			Factory.Save();

			var wrapper = FreightWrapper.New(Opportunity, Factory);
			var resultList = wrapper[0].SalesRelations.OneOffQuotes;

			AssertEquals("First node should be 001", (resultList[0].WrappedObject as IRatingHeader).TH_QuoteNumber, "001/A");
			AssertEquals("First node should be 002", (resultList[1].WrappedObject as IRatingHeader).TH_QuoteNumber, "002/A");
			AssertEquals("First node should be 003", (resultList[2].WrappedObject as IRatingHeader).TH_QuoteNumber, "003/A");
		}

		public override void TestWrapperMappingsEmpty()
		{
			var opp = Factory.New<OrgOpportunity>();
			SalesRelationsWrapper wrapperEmpty = new SalesRelationsWrapper(opp, Factory);
			AssertEquals("wrapperEmpty.Quotations", 0, wrapperEmpty.Quotations.Count);
			AssertEquals("wrapperEmpty.OneOffQuotes", 0, wrapperEmpty.OneOffQuotes.Count);
		}

		#endregion

		#region Implementation

		OrgOpportunity Opportunity;
		OrgHeader Org;
		Quote Rating1;
		Quote Rating2;
		Quote Rating3;

		void InitialVariables()
		{
			Org = Factory.NewWithValidTestData<OrgHeader>();
			Org.OH_FullName = "Some Enquiry Org";
			Org.MainAddress.OA_Address1 = "1 Street";
			Org.OH_RL_NKClosestPort = "AUSYD";

			Opportunity = Org.SalesOpportunities.AddNew();
			var pivots1 = Opportunity.RelatedChildActivityPivotCollection;

			Rating3 = Factory.NewWithValidTestData<Quote>();
			Rating3.TH_QuoteNumber = "003";
			Rating3.TH_OH = Org.PK;
			var pivot3 = pivots1.AddNew();
			pivot3.RAP_ParentActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			pivot3.RAP_ChildActivityTableCode = RatingHeaderSchema.Constants.Prefix;
			pivot3.RAP_ChildActivityID = Rating3.PK;

			Rating2 = Factory.NewWithValidTestData<Quote>();
			Rating2.TH_QuoteNumber = "002";
			Rating2.TH_OH = Org.PK;
			var pivot2 = pivots1.AddNew();
			pivot2.RAP_ParentActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			pivot2.RAP_ChildActivityTableCode = RatingHeaderSchema.Constants.Prefix;
			pivot2.RAP_ChildActivityID = Rating2.PK;

			Rating1 = Factory.NewWithValidTestData<Quote>();
			Rating1.TH_QuoteNumber = "001";
			Rating1.TH_OH = Org.PK;
			var pivot1 = pivots1.AddNew();
			pivot1.RAP_ParentActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			pivot1.RAP_ChildActivityTableCode = RatingHeaderSchema.Constants.Prefix;
			pivot1.RAP_ChildActivityID = Rating1.PK;

			Factory.Save();
		}
		protected override void SetUp()
		{
			InitialVariables();
			base.SetUp();
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			return new SalesRelationsWrapper(opportunity, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			return new SalesRelationsWrapper(opportunity, Factory);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
SalesRelations
======================================================================
Name                                    Type
----------------------------------------------------------------------

OneOffQuotes                            Freight Collection
Quotations                              Rating Information Collection
";
			}
		}

		#endregion
	}
}
