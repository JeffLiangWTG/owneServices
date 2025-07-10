using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Forwarding.Business.Testing
{
	class LatestAMSDispositionProviderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLatestAMSDispositionProvider()
		{
			var helper = ObjectFactory.Get<Integration.Customs.Shared.Universal.IUniversalReferenceTestDataHelper>("Universal.IUniversalReferenceTestDataHelper", Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode, "AMSDD", "US");
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList("US", "AMSDD", "3U", "3U DESC", startDate, endDate);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var consol = newFactory.New<ForwardingConsol>();
			var header = newFactory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header.BH_ParentID = consol.PK;
			var bill = newFactory.New<Integration.Customs.US.USAMS.ICusInBondBill>();
			bill.B0_BH = header.PK;
			var dispositionCode = newFactory.New<Integration.Customs.US.IDispositionData>();
			dispositionCode.B7_ParentID = bill.PK;
			dispositionCode.B7_ParentTableCode = "B0";
			dispositionCode.US_Code = "3U";
			dispositionCode.US_DispositionDate = ZDateTime.Today;
			var provider = new LatestAMSDispositionProvider(consol);
			NUnit.Framework.Assert.That(provider.LatestAMSDispositionCode, Is.EqualTo("3U").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(provider.LatestAMSDispositionCode, Is.EqualTo(header.BH_LatestDispositionCode));
			NUnit.Framework.Assert.That(provider.LatestAMSDispositionDesc, Is.EqualTo("3U DESC").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(provider.LatestAMSDispositionDesc, Is.EqualTo(header.BH_LatestDispositionCodeDescription));
		}
	}
}
