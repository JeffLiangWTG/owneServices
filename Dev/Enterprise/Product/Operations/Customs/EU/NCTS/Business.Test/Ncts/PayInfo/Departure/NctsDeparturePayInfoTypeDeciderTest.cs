using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class NctsDeparturePayInfoTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBinding()
		{
			var typeDecider = new NctsDeparturePayInfoTypeDecider();
			AssertNull("GetTypeForBinding", typeDecider.GetTypeForBinding());
		}

		public void TestGetTypeForLoad()
		{
			var moveHeader = Factory.New<NctsDepartureMovementHeader>();
			var payInfo = moveHeader.PayInfoCollection.AddNew();
			var typeDecider = new NctsDeparturePayInfoTypeDecider();
			var typeForLoad = typeDecider.GetTypeForLoad(((INeedRow)payInfo).Row, Factory);
			AssertEquals("GetTypeForLoad returned object", typeof(NctsDeparturePayInfo), typeForLoad);
		}

		public void TestGetTypeForLoadReportsError()
		{
			var payInfo = Factory.New<NctsDeparturePayInfo>();
			var typeDecider = new NctsDeparturePayInfoTypeDecider();
			var typeForLoad = typeDecider.GetTypeForLoad(((INeedRow)payInfo).Row, Factory);
			CombineAssertions(() =>
			{
				AssertNull("GetTypeForLoad returned object", typeForLoad);
				AssertEquals("ErrorReporter.LastMessageReported", "Cannot determine the NctsDeparturePayInfo object, because parent business object is unknown", ErrorReporter.LastMessageReported);
			});
			ErrorReporter.Clear();
		}

		public void TestGetTypeForNew()
		{
			var typeDecider = new NctsDeparturePayInfoTypeDecider();
			AssertNull("GetTypeForNew", typeDecider.GetTypeForNew());
		}
	}
}
