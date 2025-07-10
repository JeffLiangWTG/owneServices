using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Intrastat.Business.Testing
{
	[TestedType(typeof(CusIntrastatMergedLine))]
	sealed class CusIntrastatMergedLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGroup()
		{
			AssertEquals(group, mergedLine.Group);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) =>
			IntrastatTestDataHelper.New(Factory).NewCusIntrastatMergedLineWithValidData();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => mergedLine;

		protected override void SetUp()
		{
			helper = IntrastatTestDataHelper.New(Factory);
			group = IntrastatTestDataHelper.New(Factory).NewCusIntrastatGroupWithValidData();
			mergedLine = helper.NewCusIntrastatMergedLineWithValidData(group);
		}

		IntrastatTestDataHelper helper;
		CusIntrastatGroup group;
		CusIntrastatMergedLine mergedLine;
	}

	[TestedType(typeof(CusIntrastatMergedLine))]
	sealed class CusIntrastatMergedLineClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var report = (CusIntrastatGroup)NewParentObject();

			var line = IntrastatTestDataHelper.New(Factory).NewCusIntrastatMergedLineWithValidData(report);

			return line;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var report = IntrastatTestDataHelper.New(Factory).NewCusIntrastatGroupWithValidData();
			return report;
		}

		protected override IEnumerable<SchemaGuidColumn> ExemptedCandidateChildrenRepresentedByActualParentClusterKeyFk => new[]
		{
			CusIntrastatLineSchema.CIL_CIH_Header,
		};
	}
}
