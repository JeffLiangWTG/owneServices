using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineColsHeader))]
	sealed class QuarantineColsHeaderClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren()
		{
			var colsHeader = ClusterKeyEntityToTest;
			var colsDirection = ((QuarantineColsHeader)colsHeader).Directions.AddNew();

			return new IClusterKeyWorker[] { colsDirection };
		}

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var entryHeader = (CusEntryHeader)NewParentObject();
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			return colsHeader;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			return Factory.NewWithValidTestData<CusEntryHeader>();
		}
	}
}
