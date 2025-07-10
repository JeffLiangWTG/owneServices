using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	class CcsukEmailSenderTest : TestCaseWithFactory
	{
		public void TestCalculateStaffFromJobWhenNotExpresslyPassedOver()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "DJC";
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var messageInAnotherFactoryJustToAvoidGetMessageReferenceNumber = new BusinessObjectFactory().New<GbEDIMessage>(); // 
			messageInAnotherFactoryJustToAvoidGetMessageReferenceNumber.EM_SystemCreateUser = staff.GS_Code;

			AssertEquals("Passed back verbatim", staff, new CcsukEmailSender(Factory, CcsukEmailSender.ToWhom.ItDepartmentAndCustomsGroup, null, staff).Staff);
			AssertEquals("Declaration", staff, new CcsukEmailSender(Factory, CcsukEmailSender.ToWhom.ItDepartmentAndCustomsGroup, declaration, null).Staff);
			AssertEquals("Message", staff.PK, new CcsukEmailSender(Factory, CcsukEmailSender.ToWhom.ItDepartmentAndCustomsGroup, messageInAnotherFactoryJustToAvoidGetMessageReferenceNumber, null).Staff.PK);
			AssertEquals("Entry", staff, new CcsukEmailSender(Factory, CcsukEmailSender.ToWhom.ItDepartmentAndCustomsGroup, entry, null).Staff);

			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKFFW98000DJC";
			declaration = ((ICcsukCusAwb)basic).CreateNewStandaloneCDSDeclaration();
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("Basic", staff, new CcsukEmailSender(Factory, CcsukEmailSender.ToWhom.ItDepartmentAndCustomsGroup, basic, null).Staff);

			var master = Factory.New<CusMAWB>();
			master.Profile = "CUKFFW98000DJC";
			ICcsukCusAwb house = master.ChildBills.AddNew();
			declaration = house.CreateNewStandaloneCDSDeclaration();
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("House", staff, new CcsukEmailSender(Factory, CcsukEmailSender.ToWhom.ItDepartmentAndCustomsGroup, (BusinessObject)house, null).Staff);

			basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKFFW98000DJC";
			var splitBasic = basic.Splits.AddNew();
			declaration = ((ICcsukCusAwb)splitBasic).CreateNewStandaloneCDSDeclaration();
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("Split Basic", staff, new CcsukEmailSender(Factory, CcsukEmailSender.ToWhom.ItDepartmentAndCustomsGroup, splitBasic, null).Staff);

			house = master.ChildBills.AddNew();
			var splitHouse = house.Splits.AddNew();
			declaration = ((ICcsukCusAwb)splitHouse).CreateNewStandaloneCDSDeclaration();
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("Split House", staff, new CcsukEmailSender(Factory, CcsukEmailSender.ToWhom.ItDepartmentAndCustomsGroup, splitHouse, null).Staff);
		}
	}
}
