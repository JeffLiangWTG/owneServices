using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(CusExitDetailCollection))]
	public class CusExitDetailCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType() => typeof(CusExitDetailCollection);

		protected override BusinessObjectCollection GetCollectionToTest() => new CusExitDetailCollection(Factory.New<CusExitControlHeader>());

		[ExpectNoExceptions]
		public void TestNewChild()
		{
			var guid = new CargoWise.Types.ZGuid();

			var cusExitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			cusExitHeader.CEH_CustomsOffice = "testoffice";
			cusExitHeader.CEH_ArrivalNotificationDate = new CargoWise.Types.ZDateTime(2020, 8, 20, 14, 36, 00);
			cusExitHeader.CEH_ArrivalNotificationPlace = "testArrivalNotificationPlace";
			cusExitHeader.CEH_ExitDate = new CargoWise.Types.ZDateTime(2020, 8, 21, 18, 25, 00);
			cusExitHeader.CEH_TransportID = "testTransportID";
			cusExitHeader.CEH_OA_Carrier = guid;

			var detail = cusExitHeader.CusExitDetails.AddNew();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(detail.CED_CustomsOffice, NUnit.Framework.Is.EqualTo("testoffice").Using(CustomComparers.TypeComparison), "CED_CustomsOffice");
				NUnit.Framework.Assert.That(detail.CED_ArrivalNotificationDate, NUnit.Framework.Is.EqualTo(new CargoWise.Types.ZDateTime(2020, 8, 20, 14, 36, 00)), "CED_ArrivalNotificationDate");
				NUnit.Framework.Assert.That(detail.CED_ArrivalNotificationPlace, NUnit.Framework.Is.EqualTo("testArrivalNotificationPlace").Using(CustomComparers.TypeComparison), "CED_ArrivalNotificationPlace");
				NUnit.Framework.Assert.That(detail.CED_ExitDate, NUnit.Framework.Is.EqualTo(new CargoWise.Types.ZDateTime(2020, 8, 21, 18, 25, 00)), "CED_ExitDate");
				NUnit.Framework.Assert.That(detail.CED_TransportID, NUnit.Framework.Is.EqualTo("testTransportID").Using(CustomComparers.TypeComparison), "CED_TransportID");
				NUnit.Framework.Assert.That(detail.CED_OA_Carrier, NUnit.Framework.Is.EqualTo(guid), "CED_OA_Carrier");
			});
		}
	}
}
