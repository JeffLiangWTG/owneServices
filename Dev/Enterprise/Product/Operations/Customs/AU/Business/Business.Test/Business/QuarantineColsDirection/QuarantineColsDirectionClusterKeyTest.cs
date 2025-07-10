using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineColsDirection))]
	sealed class QuarantineColsDirectionClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var colsHeader = (QuarantineColsHeader)NewParentObject();
			var colsDirection = colsHeader.Directions.AddNew();
			return colsDirection;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			return colsHeader;
		}
	}
}
