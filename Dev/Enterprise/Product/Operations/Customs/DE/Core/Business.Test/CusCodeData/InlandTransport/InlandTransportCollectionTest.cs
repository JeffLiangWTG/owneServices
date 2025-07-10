using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(InlandTransportCollection))]
	sealed class InlandTransportCollectionTest : BusinessObjectCollectionTestCase
	{
		[ExpectNoExceptions]
		public void TestMaxCount_Rail_NotInTransitionPeriodAES30()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Rail;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				NUnit.Framework.Assert.That(declaration.InlandTransports.MaxCount, NUnit.Framework.Is.EqualTo(999), "MaxCount");
			}
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = Factory.New<JobDeclaration>();
			return new InlandTransportCollection(parent);
		}
	}
}
