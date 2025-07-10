using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(LocalProcessing))]
	class LocalProcessingTest : DataObjectTestCase<LocalProcessing>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ "ArrivalCartageRef",  DtbBookingSchema.KM_TransportReference.MaxLength }
			};
		}
	}
}
