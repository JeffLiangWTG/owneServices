using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class AddInfoJobDeclarationLookupsTest : CAAddInfoLookupsTest
	{
		protected override AddInfo GetNewAddInfo() => new AddInfoJobDeclaration(Factory.New<JobDeclaration>().JE_AddInfoInfo);

		public void TestLookups()
		{
			AssertType<PriorityIndicators>("PriorityIndicatorList type", addInfo.Lookups.PriorityIndicatorList);
			AssertType<ZZRefCarrierCombinedCollection>("CarrierCodes type", addInfo.Lookups.CarrierCodes);
			AssertType<ACROSSServiceOptions>("ServiceOptions type", addInfo.Lookups.ServiceOptions);
			AssertType<AssessmentOptions>("AssessmentOptions type", addInfo.Lookups.AssessmentOptions);
			AssertType<B3MergeByList>("CAMergeByList type", addInfo.Lookups.CAMergeByList);
			AssertType<B2TypeList>("B2TypeList type", addInfo.Lookups.B2TypeList);
			AssertType<OrgHeaderCollection>("OrgHeaderCollection type", addInfo.Lookups.MailToOrganisations);
			AssertType<OriginalDeclarationCollection>("LodgedB3JobDeclarationCollection type", addInfo.Lookups.LodgedB3Declarations);
		}

		public void TestB2TypeList()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("BK, SC", declaration.AddInfoLookups.B2TypeList.CodesAsString);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertSame(declaration.AddInfoLookups.B2TypeList, declaration2.AddInfoLookups.B2TypeList);

			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			AssertEquals("SC", declaration.AddInfoLookups.B2TypeList.CodesAsString);
		}

		public void TestLodgedB3Declarations()
		{
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var impDeclaration = collection.AddNew();
			impDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var b2Declaration = collection.AddNew();
			b2Declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var b3XDeclaration = collection.AddNew();
			b3XDeclaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			var lvsDeclaration = collection.AddNew();
			lvsDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			Factory.Save();

			var newB2Declaration = collection.AddNew();
			newB2Declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var b2AddInfo = new AddInfoJobDeclaration(newB2Declaration.JE_AddInfoInfo);

			var decColl = b2AddInfo.Lookups.LodgedB3Declarations;
			decColl.Load();
			Assert(decColl.Contains(impDeclaration));
			Assert(decColl.Contains(b2Declaration));
			Assert(decColl.Contains(b3XDeclaration));
			Assert(decColl.Contains(lvsDeclaration));

			var newB3XDeclaration = collection.AddNew();
			newB3XDeclaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			var b3XAddInfo = new AddInfoJobDeclaration(newB3XDeclaration.JE_AddInfoInfo);
			decColl = b3XAddInfo.Lookups.LodgedB3Declarations;
			decColl.Load();

			Assert(decColl.Contains(impDeclaration));
			Assert(!decColl.Contains(b2Declaration));
			Assert(decColl.Contains(b3XDeclaration));
			Assert(decColl.Contains(lvsDeclaration));

			var declaration = Factory.New<JobDeclaration>();
			declaration.CA_OriginalTransactionNo = "123456";
			var coll = declaration.AddInfoLookups.LodgedB3Declarations;
			AssertEquals("123456", coll.FilterBusinessObjectDefaults["Transaction #:Property"].Value);
		}

		public void TestServiceOptions()
		{
			Assert("Contains IID option", addInfo.Lookups.ServiceOptions.ContainsCode(ACROSSServiceOptions.Codes.IID));
			Assert("Contains CSA option", addInfo.Lookups.ServiceOptions.ContainsCode(ACROSSServiceOptions.Codes.CSA));
			Assert("option 463 removed", !addInfo.Lookups.ServiceOptions.ContainsCode("463"));
			Assert("option 471 removed", !addInfo.Lookups.ServiceOptions.ContainsCode("471"));
			Assert("option 257 removed", !addInfo.Lookups.ServiceOptions.ContainsCode("257"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			addInfo = (AddInfoJobDeclaration)GetNewAddInfo();
		}

		JobDeclaration declaration;
		AddInfoJobDeclaration addInfo;
	}
}
