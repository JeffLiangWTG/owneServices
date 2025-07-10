using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.Testing
{
	[TestedType(typeof(InBondMoveDetail))]
	sealed class InBondMoveDetailTest : DataObjectTestCase<InBondMoveDetail>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>
			{
				{ nameof(InBondMoveDetail.SequenceNumber), 5 },
				{ nameof(InBondMoveDetail.ForeignDestPortScheduleK), 5 },
				{ nameof(InBondMoveDetail.ExportVesselName), 35 },
			};
		}
	}
}
