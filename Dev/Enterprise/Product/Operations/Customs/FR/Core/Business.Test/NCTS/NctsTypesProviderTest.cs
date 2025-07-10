using System;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.NCTS.Testing;

[TestedType(typeof(NctsTypesProvider))]
sealed class NctsTypesProviderTest : EU.NCTS.Business.Testing.NctsTypesProviderAbstractTest<NctsTypesProvider>
{
	protected override Type ExpectedNctsAdditionalInfoType => typeof(EU.NCTS.Business.NctsAdditionalInfo);
	protected override Type ExpectedNctsArrivalCargoDescType => typeof(EU.NCTS.Business.NctsArrivalCargoDesc);
	protected override Type ExpectedNctsDepartureCargoDescType => typeof(NctsDepartureCargoDesc);
}
