using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CusDV1Detail))]
	class CusDV1DetailTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSequence()
		{
			var dv1Detail2 = declaration.DV1Details.AddNew();
			var dv1Detail3 = declaration.DV1Details.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("Sequence 1", (ZShort)1, dv1Detail.Sequence);
				AssertEquals("Sequence 2", (ZShort)2, dv1Detail2.Sequence);
				AssertEquals("Sequence 3", (ZShort)3, dv1Detail3.Sequence);

				declaration.DV1Details.Remove(dv1Detail2);
				AssertEquals("Sequence 1 stay same", (ZShort)1, dv1Detail.Sequence);
				AssertEquals("Sequence 3 change to 2", (ZShort)2, dv1Detail3.Sequence);
			});
		}

		public void TestISequenceNumberLine()
		{
			CombineAssertions(() =>
			{
				var sequenceLine = (IShortSequenceNumberLine)dv1Detail;
				AssertEquals("FKToHeader", declaration.PK, sequenceLine.FKToHeader);
				AssertEquals("SequenceNumber", (ZShort)1, sequenceLine.SequenceNumber);
			});
		}

		public void TestDV1_PriceInfluence_SetBlank()
		{
			dv1Detail.DV1_Relationship = YesNoList.Codes.No;
			AssertEquals(ZString.Empty, dv1Detail.DV1_PriceInfluence);
		}

		public void TestDV1_PriceInfluence_ReadOnly()
		{
			dv1Detail.DV1_Relationship = ZString.Empty;
			Assert(dv1Detail.DV1_PriceInfluenceInfo.ReadOnly);
		}

		public void TestDV1_CloseApproximation_SetBlank()
		{
			dv1Detail.DV1_Relationship = YesNoList.Codes.No;
			AssertEquals(ZString.Empty, dv1Detail.DV1_CloseApproximation);
		}

		public void TestDV1_CloseApproximation_ReadOnly()
		{
			dv1Detail.DV1_Relationship = ZString.Empty;
			Assert(dv1Detail.DV1_CloseApproximationInfo.ReadOnly);
		}

		public void TestDV1_RelationDetails_ReadOnly()
		{
			dv1Detail.DV1_Relationship = ZString.Empty;
			Assert(dv1Detail.DV1_RelationDetailsInfo.ReadOnly);
		}

		public void TestDV1_RestrictionConsiderationDetails_SetBlank()
		{
			dv1Detail.DV1_Restrictions = YesNoList.Codes.No;
			AssertEquals(ZString.Empty, dv1Detail.DV1_RestrictionConsiderationDetails);
		}

		public void TestDV1_RestrictionConsiderationDetails_ReadOnly()
		{
			dv1Detail.DV1_Restrictions = YesNoList.Codes.No;
			Assert(dv1Detail.DV1_RestrictionConsiderationDetailsInfo.ReadOnly);
		}

		public void TestDV1_RoyaltiesLicenceDetails_SetBlank()
		{
			dv1Detail.DV1_RoyaltiesLicence = YesNoList.Codes.No;
			AssertEquals(ZString.Empty, dv1Detail.DV1_RoyaltiesLicenceDetails);
		}

		public void TestDV1_RoyaltiesLicenceDetails_ReadOnly()
		{
			dv1Detail.DV1_RoyaltiesLicence = ZString.Empty;
			Assert(dv1Detail.DV1_RoyaltiesLicenceDetailsInfo.ReadOnly);
		}

		public void TestDV1_ResaleDetails_SetBlank()
		{
			dv1Detail.DV1_Resale = YesNoList.Codes.No;
			AssertEquals(ZString.Empty, dv1Detail.DV1_ResaleDetails);
		}

		public void TestDV1_ResaleDetails_ReadOnly()
		{
			dv1Detail.DV1_Resale = ZString.Empty;
			Assert(dv1Detail.DV1_ResaleDetailsInfo.ReadOnly);
		}

		public void TestDelete()
		{
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var dv1Pivot1 = entryInstruction1.DV1DetailsPivots[0];
			dv1Pivot1.IsForEntryInstruction = true;
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var dv1Pivot2 = entryInstruction2.DV1DetailsPivots[0];
			dv1Pivot2.IsForEntryInstruction = true;
			CombineAssertions(() =>
			{
				AssertEquals("Multiple Gen Pivot records", 2, LoadGenPivot(dv1Detail).Length);
				dv1Detail.Delete();
				AssertEquals("Pivots deleted", 0, LoadGenPivot(dv1Detail).Length);
			});

			GenPivot[] LoadGenPivot(CusDV1Detail dv1Detail)
			{
				var pivotQuery = new ZQuery(GenPivotSchema.XX_RelationType, Core.Constants.GenPivotTypes.CusDV1Detail);
				pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, dv1Detail.PK);
				pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, CusDV1DetailSchema.Constants.Prefix);
				return Factory.Load<GenPivot>(pivotQuery);
			}
		}

		public void TestSetDefaultValues()
		{
			CombineAssertions(() =>
			{
				AssertEquals("DV1_Relationship Default", YesNoList.Codes.No, dv1Detail.DV1_Relationship);
				AssertEquals("DV1_Restrictions Default", YesNoList.Codes.No, dv1Detail.DV1_Restrictions);
				AssertEquals("DV1_Consideration Default", YesNoList.Codes.No, dv1Detail.DV1_Consideration);
				AssertEquals("DV1_RoyaltiesLicence Default", YesNoList.Codes.No, dv1Detail.DV1_RoyaltiesLicence);
				AssertEquals("DV1_Resale Default", YesNoList.Codes.No, dv1Detail.DV1_Resale);
			});
		}

		public void TestDV1_RelationDetailsCleanedIfNecessary()
		{
			dv1Detail.DV1_Relationship = YesNoList.Codes.Yes;
			dv1Detail.DV1_RelationDetails = "Details Info";

			dv1Detail.DV1_Relationship = YesNoList.Codes.No;
			AssertEquals("Value cleaned", ZString.Empty, dv1Detail.DV1_RelationDetails);
		}

		public void TestDV1_RestrictionConsiderationDetailsCleanedIfNecessary()
		{
			CombineAssertions(() =>
			{
				dv1Detail.DV1_Restrictions = YesNoList.Codes.Yes;
				dv1Detail.DV1_Consideration = YesNoList.Codes.Yes;
				dv1Detail.DV1_RestrictionConsiderationDetails = "Restriction Details";

				dv1Detail.DV1_Consideration = YesNoList.Codes.No;
				AssertEquals("Value not cleaned", "Restriction Details", dv1Detail.DV1_RestrictionConsiderationDetails);

				dv1Detail.DV1_Restrictions = YesNoList.Codes.No;
				AssertEquals("Value cleaned", ZString.Empty, dv1Detail.DV1_RestrictionConsiderationDetails);
			});
		}

		public void TestDV1_RoyaltiesLicenceDetailsCleanedIfNecessary()
		{
			dv1Detail.DV1_RoyaltiesLicence = YesNoList.Codes.Yes;
			dv1Detail.DV1_RoyaltiesLicenceDetails = "Licence Details";

			dv1Detail.DV1_RoyaltiesLicence = YesNoList.Codes.No;
			AssertEquals("Value cleaned", ZString.Empty, dv1Detail.DV1_RoyaltiesLicenceDetails);
		}

		public void TestDV1_ResaleDetailsCleanedIfNecessary()
		{
			dv1Detail.DV1_Resale = YesNoList.Codes.Yes;
			dv1Detail.DV1_ResaleDetails = "Resale Details";

			dv1Detail.DV1_Resale = YesNoList.Codes.No;
			AssertEquals("Value cleaned", ZString.Empty, dv1Detail.DV1_ResaleDetails);
		}

		public void TestDV1_CloseApproximationCleanedIfNecessary()
		{
			CombineAssertions(() =>
			{
				dv1Detail.DV1_Relationship = YesNoList.Codes.Yes;
				AssertEquals("DV1_Relationship is Yes", YesNoList.Codes.No, dv1Detail.DV1_CloseApproximation);
				dv1Detail.DV1_CloseApproximation = YesNoList.Codes.Yes;

				dv1Detail.DV1_Relationship = YesNoList.Codes.No;
				AssertEquals("Value cleaned", ZString.Empty, dv1Detail.DV1_CloseApproximation);
			});
		}

		public void TestCaptions()
		{
			CombineAssertions(() =>
			{
				AssertEquals("DV1_Relationship Caption", "Relationship", dv1Detail.DV1_RelationshipInfo.Description);
				AssertEquals("DV1_PriceInfluence Caption", "Price influenced?", dv1Detail.DV1_PriceInfluenceInfo.Description);
				AssertEquals("DV1_RelationDetails Caption", "Details", dv1Detail.DV1_RelationDetailsInfo.Description);
				AssertEquals("DV1_Restrictions Caption", "Restrictions", dv1Detail.DV1_RestrictionsInfo.Description);
				AssertEquals("DV1_Consideration Caption", "Conditions", dv1Detail.DV1_ConsiderationInfo.Description);
				AssertEquals("DV1_RestrictionConsiderationDetails Caption", "Details", dv1Detail.DV1_RestrictionConsiderationDetailsInfo.Description);
				AssertEquals("DV1_RoyaltiesLicence Caption", "License Fees", dv1Detail.DV1_RoyaltiesLicenceInfo.Description);
				AssertEquals("DV1_RoyaltiesLicenceDetails Caption", "Details", dv1Detail.DV1_RoyaltiesLicenceDetailsInfo.Description);
				AssertEquals("DV1_Resale Caption", "Resale", dv1Detail.DV1_ResaleInfo.Description);
				AssertEquals("DV1_ResaleDetails Caption", "Details", dv1Detail.DV1_ResaleDetailsInfo.Description);
				AssertEquals("DV1_CustomsDecisionNumber Caption", "Former Decisions", dv1Detail.DV1_CustomsDecisionNumberInfo.Description);
				AssertEquals("DV1_CloseApproximation Caption", "Consistent Price?", dv1Detail.DV1_CloseApproximationInfo.Description);
				AssertEquals("DV1_ContractNumber Caption", "Contract Number", dv1Detail.DV1_ContractNumberInfo.Description);
				AssertEquals("DV1_ContractDate Caption", "Contract Date", dv1Detail.DV1_ContractDateInfo.Description);
			});
		}

		public void TestSupportsClone()
		{
			AssertEquals(true, dv1Detail.SupportsClone());
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

		protected BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			dv1Detail = declaration.DV1Details.AddNew();
			dv1Detail.DV1_ContractDate = new ZDate(2020, 10, 19);
			dv1Detail.DV1_CustomsDecisionDate = new ZDate(2020, 10, 11);
			return dv1Detail;
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			dv1Detail = declaration.DV1Details.AddNew();
		}
		CusDV1Detail dv1Detail;
		JobDeclaration declaration;
	}

	[TestedType(typeof(CusDV1Detail))]
	class CusDV1DetailClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IEnumerable<SchemaGuidColumn> ExemptedCandidateChildrenRepresentedByActualParentClusterKeyFk => new SchemaGuidColumn[] { CusEntryInstructionSchema.CEI_JE };

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var declaration = (JobDeclaration)NewParentObject();
			var dv1Details = Factory.New<CusDV1Detail>();
			dv1Details.DV1_JE = declaration.PK;
			dv1Details.DV1_ContractDate = new ZDate(2020, 10, 19);
			dv1Details.DV1_CustomsDecisionDate = new ZDate(2020, 11, 20);
			return dv1Details;
		}
		protected override EnterpriseBusinessObject NewParentObject() => Factory.New<JobDeclaration>();
	}
}
