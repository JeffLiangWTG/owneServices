using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(NonPersistentCusDV1DetailPivot))]
	sealed class NonPersistentCusDV1DetailPivotTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructorArgumentNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new NonPersistentCusDV1DetailPivot(null));
		}

		public void TestIsForEntryInstruction()
		{
			AssertEquals(false, dv1DetailPivot.IsForEntryInstruction);
		}

		public void TestIsForEntryInstruction_PivotCreated()
		{
			CombineAssertions(() =>
			{
				AssertNull("Pivot deleted", LoadGenPivot(dv1Detail));
				dv1DetailPivot.IsForEntryInstruction = true;
				AssertNotNull("EntryInstructionPivot created", LoadGenPivot(dv1Detail));
			});
		}

		public void TestIsForEntryInstruction_PivotDeleted()
		{
			CombineAssertions(() =>
			{
				dv1DetailPivot.IsForEntryInstruction = true;
				AssertNotNull("EntryInstructionPivot created", LoadGenPivot(dv1Detail));
				dv1DetailPivot.IsForEntryInstruction = false;
				AssertNull("Pivot deleted", LoadGenPivot(dv1Detail));
			});
		}

		public void TestSequence()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Sequence Number", (ZShort)1, dv1DetailPivot.Sequence);
				dv1Detail.Delete();
				AssertEquals("DV1Detail deleted (null)", (ZShort)0, dv1DetailPivot.Sequence);
			});
		}

		public void TestRelationship()
		{
			CombineAssertions(() =>
			{
				dv1Detail.DV1_Relationship = YesNoList.Codes.Yes;
				AssertEquals("Relationship", YesNoList.Codes.Yes, dv1DetailPivot.Relationship);
				dv1Detail.Delete();
				AssertEquals("DV1Detail deleted (null)", ZString.Empty, dv1DetailPivot.Relationship);
			});
		}

		public void TestPriceInfluence()
		{
			CombineAssertions(() =>
			{
				dv1Detail.DV1_PriceInfluence = YesNoList.Codes.No;
				AssertEquals("Price Influence", YesNoList.Codes.No, dv1DetailPivot.PriceInfluence);
				dv1Detail.Delete();
				AssertEquals("DV1Detail deleted (null)", ZString.Empty, dv1DetailPivot.PriceInfluence);
			});
		}

		public void TestRelationDetails()
		{
			CombineAssertions(() =>
			{
				dv1Detail.DV1_RelationDetails = "SUPPLIER";
				AssertEquals("Relation Details", "SUPPLIER", dv1DetailPivot.RelationDetails);
				dv1Detail.Delete();
				AssertEquals("DV1Detail deleted (null)", ZString.Empty, dv1DetailPivot.RelationDetails);
			});
		}

		public void TestRestrictions()
		{
			CombineAssertions(() =>
			{
				dv1Detail.DV1_Restrictions = YesNoList.Codes.Yes;
				AssertEquals("Restrictions", YesNoList.Codes.Yes, dv1DetailPivot.Restrictions);
				dv1Detail.Delete();
				AssertEquals("DV1Detail deleted (null)", ZString.Empty, dv1DetailPivot.Restrictions);
			});
		}

		public void TestConsideration()
		{
			CombineAssertions(() =>
			{
				dv1Detail.DV1_Consideration = YesNoList.Codes.No;
				AssertEquals("Consideration", YesNoList.Codes.No, dv1DetailPivot.Consideration);
				dv1Detail.Delete();
				AssertEquals("DV1Detail deleted (null)", ZString.Empty, dv1DetailPivot.Consideration);
			});
		}

		public void TestRestrictionConsiderationDetails()
		{
			CombineAssertions(() =>
			{
				dv1Detail.DV1_RestrictionConsiderationDetails = "CONSIDER THIS NOT RESTRICTED";
				AssertEquals("Restriction Consideration Details", "CONSIDER THIS NOT RESTRICTED", dv1DetailPivot.RestrictionConsiderationDetails);
				dv1Detail.Delete();
				AssertEquals("DV1Detail deleted (null)", ZString.Empty, dv1DetailPivot.RestrictionConsiderationDetails);
			});
		}

		public void TestRoyaltiesLicence()
		{
			CombineAssertions(() =>
			{
				dv1Detail.DV1_RoyaltiesLicence = YesNoList.Codes.Yes;
				AssertEquals("Royalties Licence", YesNoList.Codes.Yes, dv1DetailPivot.RoyaltiesLicence);
				dv1Detail.Delete();
				AssertEquals("DV1Detail deleted (null)", ZString.Empty, dv1DetailPivot.RoyaltiesLicence);
			});
		}

		public void TestRoyaltiesLicenceDetails()
		{
			CombineAssertions(() =>
			{
				dv1Detail.DV1_RoyaltiesLicenceDetails = "ROYALTY LICENSE DETAILS";
				AssertEquals("Royalties Licence Details", "ROYALTY LICENSE DETAILS", dv1DetailPivot.RoyaltiesLicenceDetails);
				dv1Detail.Delete();
				AssertEquals("DV1Detail deleted (null)", ZString.Empty, dv1DetailPivot.RoyaltiesLicenceDetails);
			});
		}

		public void TestResale()
		{
			CombineAssertions(() =>
			{
				dv1Detail.DV1_Resale = YesNoList.Codes.No;
				AssertEquals("Resale", YesNoList.Codes.No, dv1DetailPivot.Resale);
				dv1Detail.Delete();
				AssertEquals("DV1Detail deleted (null)", ZString.Empty, dv1DetailPivot.Resale);
			});
		}

		public void TestResaleDetails()
		{
			CombineAssertions(() =>
			{
				dv1Detail.DV1_ResaleDetails = "RESALE DETAILS";
				AssertEquals("Resale Details", "RESALE DETAILS", dv1DetailPivot.ResaleDetails);
				dv1Detail.Delete();
				AssertEquals("DV1Detail deleted (null)", ZString.Empty, dv1DetailPivot.ResaleDetails);
			});
		}

		public void TestCustomsDecisionNumber()
		{
			CombineAssertions(() =>
			{
				dv1Detail.DV1_CustomsDecisionNumber = "DESC12345678";
				AssertEquals("Customs Decision Number", "DESC12345678", dv1DetailPivot.CustomsDecisionNumber);
				dv1Detail.Delete();
				AssertEquals("DV1Detail deleted (null)", ZString.Empty, dv1DetailPivot.CustomsDecisionNumber);
			});
		}

		public void TestPivotDeleted()
		{
			CombineAssertions(() =>
			{
				dv1DetailPivot.IsForEntryInstruction = true;
				var entryInstructionAndDeclarationPivot = LoadGenPivot(dv1Detail);
				AssertNotNull("EntryInstructionPivot created", entryInstructionAndDeclarationPivot);
				dv1DetailPivot.Delete();
				AssertEquals("GenPivot Is Deleted", true, entryInstructionAndDeclarationPivot.IsDeleted);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			dv1DetailPivot.IsForEntryInstruction = true;
			return dv1DetailPivot;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			dv1Detail = declaration.DV1Details.AddNew();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			dv1DetailPivot = entryInstruction.DV1DetailsPivots[0];
		}
		CusDV1Detail dv1Detail;
		NonPersistentCusDV1DetailPivot dv1DetailPivot;
		CusEntryInstruction entryInstruction;

		GenPivot LoadGenPivot(CusDV1Detail dv1Detail)
		{
			var pivotQuery = new ZQuery(GenPivotSchema.XX_RelationType, Core.Constants.GenPivotTypes.CusDV1Detail);
			pivotQuery.AddToFilter(GenPivotSchema.XX_Relation1ID, entryInstruction.PK);
			pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, dv1Detail.PK);
			pivotQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, CusEntryInstructionSchema.Constants.Prefix);
			pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, CusDV1DetailSchema.Constants.Prefix);

			return entryInstruction.Factory.LoadTop1<GenPivot>(pivotQuery);
		}
	}
}
