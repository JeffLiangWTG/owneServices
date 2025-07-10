using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.Testing
{
	[TestedType(typeof(BillScreening))]
	sealed class BillScreeningTest : DataObjectTestCase<BillScreening>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>
			{
				{ nameof(BillScreening.Result), AsycudaBillScreeningSchema.ASR_Result.MaxLength },
				{ nameof(BillScreening.AuthorisedPerson), AsycudaBillScreeningSchema.ASR_AuthorizedPersonName.MaxLength },
				{ nameof(BillScreening.PersonType), AsycudaBillScreeningSchema.ASR_AuthorizedPersonType.MaxLength },
				{ nameof(BillScreening.PersonIdentifier), AsycudaBillScreeningSchema.ASR_AuthorizedPersonIdentifier.MaxLength },
				{ nameof(BillScreening.FacilityPlaceSubDivision), 35 },
				{ nameof(BillScreening.FacilityPlaceNumber), 35 },
				{ nameof(BillScreening.FacilityPlacePOBox), 70 }
			};
		}
	}
}
