using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataConverters.Testing.Base
{
	sealed internal class PartMatcherTest : TestCaseWithFactory
	{
		public void TestImporterCodeCouldNotBeMatched()
		{
			var matcher = new PartMatcher(PartNumberMatch, SupplierMatch, ImporterNoMatch, Factory);
			AssertEquals("matcher.ReasonPartCannotBeImported", "Importer Code: IMPORTEN Could Not Be Matched", matcher.ReasonPartCannotBeImported);
			AssertEquals("matcher.MatchedPart", null, matcher.MatchedPart);
		}

		public void TestSupplierCodeCouldNotBeMatched()
		{
			var matcher = new PartMatcher(PartNumberMatch, SupplierNoMatch, ImporterMatch, Factory);
			AssertEquals("matcher.ReasonPartCannotBeImported", "Supplier Code: SUPPLIEN Could Not Be Matched", matcher.ReasonPartCannotBeImported);
			AssertEquals("matcher.MatchedPart", null, matcher.MatchedPart);
		}

		public void TestPartCodeNotFound()
		{
			var matcher = new PartMatcher(PartNumberNoMatch, SupplierMatch, ImporterMatch, Factory);
			AssertEquals("matcher.ReasonPartCannotBeImported", "", matcher.ReasonPartCannotBeImported);
			AssertEquals("matcher.MatchedPart", null, matcher.MatchedPart);
		}

		public void TestCannotPossiblyBeAMatchBySupplier()
		{
			AddRelationshipToPart(OrgSupplierWrong, OrgPartRelation.RelationshipTypes.Supplier);
			var matcher = new PartMatcher(PartNumberMatch, SupplierMatch, ImporterMatch, Factory);
			AssertEquals("matcher.ReasonPartCannotBeImported", "", matcher.ReasonPartCannotBeImported);
			AssertEquals("matcher.MatchedPart", null, matcher.MatchedPart);
		}

		public void TestCannotPossiblyBeAMatchByImporter()
		{
			AddRelationshipToPart(OrgImporterWrong, OrgPartRelation.RelationshipTypes.Owner);
			var matcher = new PartMatcher(PartNumberMatch, SupplierMatch, ImporterMatch, Factory);
			AssertEquals("matcher.ReasonPartCannotBeImported", "", matcher.ReasonPartCannotBeImported);
			AssertEquals("matcher.MatchedPart", null, matcher.MatchedPart);
		}

		public void TestHasRightSupplierAndNoImporterGetsAMatch()
		{
			AddRelationshipToPart(OrgSupplierMatch, OrgPartRelation.RelationshipTypes.Supplier);
			var matcher = new PartMatcher(PartNumberMatch, SupplierMatch, ImporterEmpty, Factory);
			AssertEquals("matcher.ReasonPartCannotBeImported", "", matcher.ReasonPartCannotBeImported);
			AssertEquals("matcher.MatchedPart", Part, matcher.MatchedPart);
		}

		public void TestHasRightImporterAndNoSupplierGetsAMatch()
		{
			AddRelationshipToPart(OrgImporterMatch, OrgPartRelation.RelationshipTypes.Owner);
			var matcher = new PartMatcher(PartNumberMatch, SupplierEmpty, ImporterMatch, Factory);
			AssertEquals("matcher.ReasonPartCannotBeImported", "", matcher.ReasonPartCannotBeImported);
			AssertEquals("matcher.MatchedPart", Part, matcher.MatchedPart);
		}

		public void TestHasRightImporterAndSupplierGetsAMatch()
		{
			AddRelationshipToPart(OrgSupplierMatch, OrgPartRelation.RelationshipTypes.Supplier);
			AddRelationshipToPart(OrgImporterMatch, OrgPartRelation.RelationshipTypes.Owner);
			var matcher = new PartMatcher(PartNumberMatch, SupplierMatch, ImporterMatch, Factory);
			AssertEquals("matcher.ReasonPartCannotBeImported", "", matcher.ReasonPartCannotBeImported);
			AssertEquals("matcher.MatchedPart", Part, matcher.MatchedPart);
		}

		public void TestHasNoImporterOrSupplierGetsAMatch()
		{
			var matcher = new PartMatcher(PartNumberMatch, SupplierEmpty, ImporterEmpty, Factory);
			AssertEquals("matcher.ReasonPartCannotBeImported", "", matcher.ReasonPartCannotBeImported);
			AssertEquals("matcher.MatchedPart", Part, matcher.MatchedPart);
		}

		public void TestAdditionalSupplierBlowsMatch()
		{
			AddRelationshipToPart(OrgSupplierMatch, OrgPartRelation.RelationshipTypes.Supplier);
			AddRelationshipToPart(OrgSupplierWrong, OrgPartRelation.RelationshipTypes.Supplier);
			var matcher = new PartMatcher(PartNumberMatch, SupplierMatch, ImporterEmpty, Factory);
			AssertEquals("matcher.ReasonPartCannotBeImported", "Part Matched but is linked to more Suppliers than: SUPPLIEM", matcher.ReasonPartCannotBeImported);
			AssertEquals("matcher.MatchedPart", null, matcher.MatchedPart);
		}

		public void TestAdditionalImporterBlowsMatch()
		{
			AddRelationshipToPart(OrgImporterWrong, OrgPartRelation.RelationshipTypes.Owner);
			AddRelationshipToPart(OrgImporterMatch, OrgPartRelation.RelationshipTypes.Owner);
			var matcher = new PartMatcher(PartNumberMatch, SupplierEmpty, ImporterMatch, Factory);
			AssertEquals("matcher.ReasonPartCannotBeImported", "Part Matched but is linked to more Importers than: IMPORTEM", matcher.ReasonPartCannotBeImported);
			AssertEquals("matcher.MatchedPart", null, matcher.MatchedPart);
		}

		public void TestWrongSupplierGetsNoMatchButNoConflict()
		{
			AddRelationshipToPart(OrgImporterWrong, OrgPartRelation.RelationshipTypes.Owner);
			AddRelationshipToPart(OrgImporterMatch, OrgPartRelation.RelationshipTypes.Owner);
			AddRelationshipToPart(OrgSupplierWrong, OrgPartRelation.RelationshipTypes.Supplier);
			var matcher = new PartMatcher(PartNumberMatch, SupplierEmpty, ImporterMatch, Factory);
			AssertEquals("matcher.ReasonPartCannotBeImported", "", matcher.ReasonPartCannotBeImported);
			AssertEquals("matcher.MatchedPart", null, matcher.MatchedPart);
		}

		public void TestWrongImporterGetsNoMatchButNoConflict()
		{
			AddRelationshipToPart(OrgSupplierMatch, OrgPartRelation.RelationshipTypes.Supplier);
			AddRelationshipToPart(OrgSupplierWrong, OrgPartRelation.RelationshipTypes.Supplier);
			AddRelationshipToPart(OrgImporterWrong, OrgPartRelation.RelationshipTypes.Owner);
			var matcher = new PartMatcher(PartNumberMatch, SupplierMatch, ImporterEmpty, Factory);
			AssertEquals("matcher.ReasonPartCannotBeImported", "", matcher.ReasonPartCannotBeImported);
			AssertEquals("matcher.MatchedPart", null, matcher.MatchedPart);
		}

		#region SupplierMatch = "SUPPLIEM"
		OrgMatcher fSupplierMatch;
		OrgMatcher SupplierMatch
		{
			get
			{
				if (fSupplierMatch == null)
				{
					fSupplierMatch = new OrgMatcher(OrgSupplierMatch.OH_Code, "", Factory);
				}
				return fSupplierMatch;
			}
		}
		#endregion
		#region SupplierNoMatch = "SUPPLIEN"
		OrgMatcher fSupplierNoMatch;
		OrgMatcher SupplierNoMatch
		{
			get
			{
				if (fSupplierNoMatch == null)
				{
					fSupplierNoMatch = new OrgMatcher(SupplierCodeNoMatch, "", Factory);
				}
				return fSupplierNoMatch;
			}
		}
		#endregion
		#region ImporterMatch = "IMPORTEM"
		OrgMatcher fImporterMatch;
		OrgMatcher ImporterMatch
		{
			get
			{
				if (fImporterMatch == null)
				{
					fImporterMatch = new OrgMatcher(OrgImporterMatch.OH_Code, "", Factory);
				}
				return fImporterMatch;
			}
		}
		#endregion
		#region ImporterNoMatch = "IMPORTEN"
		OrgMatcher fImporterNoMatch;
		OrgMatcher ImporterNoMatch
		{
			get
			{
				if (fImporterNoMatch == null)
				{
					fImporterNoMatch = new OrgMatcher(ImporterCodeNoMatch, "", Factory);
				}
				return fImporterNoMatch;
			}
		}
		#endregion

		OrgSupplierPart Part;
		OrgHeader OrgSupplierMatch;
		OrgHeader OrgSupplierWrong;
		OrgHeader OrgImporterMatch;
		OrgHeader OrgImporterWrong;

		OrgMatcher SupplierEmpty;
		OrgMatcher ImporterEmpty;

		protected override void SetUp()
		{
			base.SetUp();
			OrgSupplierMatch = OrgHeader.New(Factory);
			OrgSupplierMatch.OH_Code = SupplierCodeMatch;
			OrgSupplierWrong = OrgHeader.New(Factory);
			OrgSupplierWrong.OH_Code = SupplierCodeWrong;

			OrgImporterMatch = OrgHeader.New(Factory);
			OrgImporterMatch.OH_Code = ImporterCodeMatch;
			OrgImporterWrong = OrgHeader.New(Factory);
			OrgImporterWrong.OH_Code = ImporterCodeWrong;

			SupplierEmpty = new OrgMatcher("", "", Factory);
			ImporterEmpty = new OrgMatcher("", "", Factory);

			Part = Factory.New<OrgSupplierPart>();
			Part.OP_PartNum = PartNumberMatch;
		}

		const string SupplierCodeMatch = "SUPPLIEM";
		const string ImporterCodeMatch = "IMPORTEM";
		const string PartNumberMatch = "PARTCODEM";

		const string SupplierCodeNoMatch = "SUPPLIEN";
		const string ImporterCodeNoMatch = "IMPORTEN";
		const string PartNumberNoMatch = "PARTCODEZ";

		const string SupplierCodeWrong = "SUPPLIEW";
		const string ImporterCodeWrong = "IMPORTEW";

		void AddRelationshipToPart(OrgHeader relatedParty, ZString relationshipType)
		{
			var relation = Part.RelatedOrganisations.AddNew();
			relation.OU_OH = relatedParty.PK;
			relation.OU_Relationship = relationshipType;
		}
	}
}
