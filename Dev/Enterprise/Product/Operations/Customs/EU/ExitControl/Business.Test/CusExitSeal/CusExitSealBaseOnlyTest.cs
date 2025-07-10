using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitSeal))]
	sealed class CusExitSealTest : CusExitSealAbstractTest<CusExitSeal, CusExitContainer, CusExitHeader>
	{
		public void TestGetZQuery()
		{
			(_, var container, _) = GetNewBusinessObject(Factory);
			var query = CusExitSeal.GetZQuery(container.PK);
			var seal1 = Factory.New<CusExitSeal>();
			seal1.BK_ParentID = container.PK;
			seal1.BK_ParentTableCode = "!@";
			var seal2 = Factory.New<CusExitSeal>();
			seal2.BK_ParentID = ZGuid.Invalid;
			seal2.BK_ParentTableCode = CusExitContainerSchema.Constants.Prefix;
			var seal3 = Factory.New<CusExitSeal>();
			seal3.BK_ParentID = container.PK;
			seal3.BK_ParentTableCode = CusExitContainerSchema.Constants.Prefix;
			CombineAssertions(() =>
			{
				AssertEquals("seal1.MatchesFilter(query)", false, seal1.MatchesFilter(query));
				AssertEquals("seal2.MatchesFilter(query)", false, seal2.MatchesFilter(query));
				AssertEquals("seal3.MatchesFilter(query)", true, seal3.MatchesFilter(query));
			});
		}

		public void TestLoad()
		{
			var (seal1, container, header) = GetNewBusinessObject(Factory);
			seal1.BK_SealNumber = "SL1";

			var seal2 = container.AllSealNumbers.AddNew();
			seal2.BK_SealNumber = "SL2";

			CombineAssertions(() =>
			{
				AssertSame("Seal1", seal1, CusExitSeal.Load(container, 1));
				AssertSame("Seal2", seal2, CusExitSeal.Load(container, 2));
				AssertNull("No one with sequence number = 3", CusExitSeal.Load(container, 3));
			});
		}

		public void TestLoadOrCreate()
		{
			var (seal1, container, header) = GetNewBusinessObject(Factory);
			seal1.BK_SealNumber = "SL1";

			var seal2 = container.AllSealNumbers.AddNew();
			seal2.BK_SealNumber = "SL2";

			CombineAssertions(() =>
			{
				AssertSame("Seal1", seal1, CusExitSeal.LoadOrCreate(container, 1));
				AssertSame("Seal2", seal2, CusExitSeal.LoadOrCreate(container, 2));
				var seal3 = CusExitSeal.LoadOrCreate(container, 2);
				AssertSeal(seal3, "seal3", container.PK, 2);
			});
		}

		public void TestCreate()
		{
			var header = Factory.New<CusExitHeader>();
			header.CXH_JobReference = header.PK.ToString().Substring(0, 35);
			var container = header.CusExitContainers.AddNew();
			CombineAssertions(() =>
			{
				AssertSeal(CusExitSeal.Create(container, 0), "seal1", container.PK, 0);
				AssertSeal(CusExitSeal.Create(container, 2), "seal2", container.PK, 2);
			});
		}

		public void TestBK_SequenceNumber_Tags()
		{
			var (seal, _, _) = GetNewBusinessObject(Factory);
			var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(seal.BK_SequenceNumberInfo, null);
			AssertNotNull(resourceStringData);

			CombineAssertions(() =>
			{
				AssertEquals("Caption","Sequence Number", resourceStringData.Caption);
				AssertEquals("Medium Caption","Seq Number", resourceStringData.MediumCaption);
				AssertEquals("Short Caption","Seq Num.", resourceStringData.ShortCaption);
				AssertEquals("Full Description", "Seal Sequence Number", resourceStringData.FullDescription);
			});
		}

		public void TestBK_UnloadingStateCaptions()
		{
			var (seal, _, _) = GetNewBusinessObject(Factory);
			var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(seal.BK_UnloadingStateInfo, null);

			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Status", resourceStringData.Caption);
				AssertEquals("Short Caption", "Status", resourceStringData.ShortCaption);
				AssertEquals("Medium Caption", "Status", resourceStringData.MediumCaption);
				AssertEquals("Description", "Seal Status", resourceStringData.FullDescription);
			});
		}

		public void TestTypeDecider()
		{
			AssertType<CusSealTypeDecider>(CusExitSeal.TypeDecider);
		}

		public void TestIsUCC6() => CombineAssertions(() =>
		{
			var (objectForTest, _, _) = GetNewBusinessObject(Factory);
			AssertEquals("Not UCC6", objectForTest.IsUCC6, false);

			AssertEquals("No Parent = not UCC6", Factory.New<CusExitSeal>().IsUCC6, false);

			var ucc6ObjectForTest = Factory.GetUcc6ExitHeader().CusExitContainers.AddNew().AllSealNumbers.AddNew();
			AssertEquals("UCC6 taken from parent", ucc6ObjectForTest.IsUCC6, true);
		});

		public void TestLookups() => CombineAssertions(() =>
		{
			var (objectForTest, _, _) = GetNewBusinessObject(Factory);
			AssertType<CusExitSealLookups>("not ucc6", objectForTest.Lookups);

			var ucc6ObjectForTest = Factory.GetUcc6ExitHeader().CusExitContainers.AddNew().AllSealNumbers.AddNew();
			AssertType<CusExitSealUcc6Lookups>("ucc6", ucc6ObjectForTest.Lookups);
		});

		public void TestValidation() => CombineAssertions(() =>
		{
			var (objectForTest, _, _) = GetNewBusinessObject(Factory);
			AssertType<CusExitSealValidation>("not ucc6", objectForTest.Validation);

			var ucc6ObjectForTest = Factory.GetUcc6ExitHeader().CusExitContainers.AddNew().AllSealNumbers.AddNew();
			AssertType<CusExitSealValidation>("ucc6 - Validation Decider should be different, not validation class", ucc6ObjectForTest.Validation);
		});

		public void TestValidationDecider() => CombineAssertions(() =>
		{
			var (objectForTest, _, _) = GetNewBusinessObject(Factory);
			AssertNull("not ucc6", objectForTest.ValidationDecider);

			var ucc6ObjectForTest = Factory.GetUcc6ExitHeader().CusExitContainers.AddNew().AllSealNumbers.AddNew();
			AssertType<CusExitSealUcc6ValidationDecider>("ucc6", ucc6ObjectForTest.ValidationDecider);
		});

		void AssertSeal(CusExitSeal seal, string desc, ZGuid parentID, ZShort sequenceNumber)
		{
			AssertEquals(desc + ".BK_ParentID", parentID, seal.BK_ParentID);
			AssertEquals(desc + ".BK_ParentTableCode", CusExitContainerSchema.Constants.Prefix, seal.BK_ParentTableCode);
			AssertEquals(desc + ".BK_SequenceNumber", sequenceNumber, seal.BK_SequenceNumber);
		}
	}
}
