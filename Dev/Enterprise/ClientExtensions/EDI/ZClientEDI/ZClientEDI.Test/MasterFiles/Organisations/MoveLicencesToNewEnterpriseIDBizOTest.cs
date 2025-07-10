using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Client.EDI.MasterFiles.Business.Test.MoveLicencesToNewEnterpriseIDBizOTest;

namespace Enterprise.Client.EDI.MasterFiles.Testing;

[TestedType(typeof(MoveLicencesToNewEnterpriseIDBizO))]
public class MoveLicencesToNewEnterpriseIDBizOTest : NonPersistentBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject()
	{
		LicenceEnterprise licEntForBizOPropertyTest = this.licEntForBizOPropertyTest;
		EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();
		Factory.Save();
		MoveLicencesToNewEnterpriseIDBizO newBizO = new MoveLicencesToNewEnterpriseIDBizO(LicEntToPassToCtor, org, null);
		return newBizO;
	}

	public void TestLicenceEnterpriseID()
	{
		EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();
		MoveLicencesToNewEnterpriseIDBizOForTest moveBizO = new MoveLicencesToNewEnterpriseIDBizOForTest(LicEntToPassToCtor, org, new MoveLicencesToNewEnterpriseIDBizOCallbacks());
		LicenceEnterprise licEnt1 = Factory.NewWithValidTestData<LicenceEnterprise>();
		licEnt1.LE_EnterpriseID = "EC1";
		LicenceEnterprise licEnt2 = Factory.NewWithValidTestData<LicenceEnterprise>();
		licEnt2.LE_EnterpriseID = "EC2";
		Factory.Save();

		AssertEquals("LicenceEnterpriseID should return empty GUID if licEntToCopyTo is empty", ZString.Empty, moveBizO.LicenceEnterpriseID);
		moveBizO.fLicEnterpriseAccessor = licEnt1;
		AssertEquals("LicenceEnterpriseID should return licEntToCopyTo.LE_EnterpriseID if licEntToCopyTo is not empty", licEnt1.LE_EnterpriseID, moveBizO.LicenceEnterpriseID);

		moveBizO.LicenceEnterpriseID = licEnt2.LE_EnterpriseID;
		AssertEquals("LicenceEnterpriseID setter should set licEntToCopyTo according to code passed to setter", licEnt2, moveBizO.fLicEnterpriseAccessor);

		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
		moveBizO.LicenceEnterpriseID = "ERR";
		AssertEquals("LicenceEnterpriseID setter should show error message if user enters enterprise ID that does not exist in the system", "Enterprise ID '" + "ERR" + "' does not exist in the system", UnitTestUserNotification.Instance.LastMessage.Text);
		AssertEquals("LicenceEnterpriseID setter should set MoveLicencesToNewEnterpriseIDBizO.licEntToCopyTo to null if user enters enterprise ID that does not exist in the system", null, moveBizO.fLicEnterpriseAccessor);

		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
		moveBizO.LicenceEnterpriseID = LicEntToPassToCtor.LE_EnterpriseID;
		AssertEquals("LicenceEnterpriseID setter should show error message if user enters enterprise ID that is source for copying licences", "This enterprise ID is chosen as source to copy from - choose another enterprise ID to copy to", UnitTestUserNotification.Instance.LastMessage.Text);
		AssertEquals("LicenceEnterpriseID setter should set MoveLicencesToNewEnterpriseIDBizO.licEntToCopyTo to null if user enters enterprise ID that is source for copying licences", null, moveBizO.fLicEnterpriseAccessor);
	}

	public void TestGetAutoAdjustedLD_ServerCode()
	{
		LicenceEnterprise licEntforBizO = Factory.NewWithValidTestData<LicenceEnterprise>();
		licEntforBizO.LE_EnterpriseID = "AAA";
		EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();
		MoveLicencesToNewEnterpriseIDBizOForTest moveBizO = new MoveLicencesToNewEnterpriseIDBizOForTest(licEntforBizO, org);
		LicenceEnterprise licEnt = Factory.NewWithValidTestData<LicenceEnterprise>();
		licEnt.LE_EnterpriseID = "BBB";

		licEnt.Databases.AddNew().LD_ServerCode = "SYD";
		Factory.Save();
		AssertEquals("Should return '**1' if passed LicenceEnterprise already contains LicenceDatabase with LD_ServerCode=='***'", "SY1", moveBizO.Call_GetAutoAdjustedLD_ServerCode(licEnt, "SYD"));

		for (int i = 1; i < 9; i++)
		{
			licEnt.Databases.AddNew().LD_ServerCode = "SY" + i.ToString();
			Factory.Save();
			AssertEquals("Should return '**" + (i + 1).ToString() + "' if passed LicenceEnterprise already contains LicenceDatabase with LD_ServerCode=='**" + i.ToString() + "' and all similar variations with lesser numbers", "SY" + (i + 1).ToString(), moveBizO.Call_GetAutoAdjustedLD_ServerCode(licEnt, "SY" + i.ToString()));
		}

		licEnt.Databases.AddNew().LD_ServerCode = "SY9";
		Factory.Save();
		AssertEquals("Should return '*10' if passed LicenceEnterprise already contains LicenceDatabase with LD_ServerCode=='**9' and all similar variations with lesser numbers", "S10", moveBizO.Call_GetAutoAdjustedLD_ServerCode(licEnt, "SY9"));

		for (int i = 10; i < 99; i++)
		{
			licEnt.Databases.AddNew().LD_ServerCode = "S" + i.ToString();
			Factory.Save();
			AssertEquals("Should return '*" + (i + 1).ToString() + "' if passed LicenceEnterprise already contains LicenceDatabase with LD_ServerCode=='*" + i.ToString() + "' and all similar variations with lesser numbers", "S" + (i + 1).ToString(), moveBizO.Call_GetAutoAdjustedLD_ServerCode(licEnt, "S" + i.ToString()));
		}

		licEnt.Databases.AddNew().LD_ServerCode = "S99";
		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
		Factory.Save();
		Exception exception = null;
		try
		{
			moveBizO.Call_GetAutoAdjustedLD_ServerCode(licEnt, "SY9");
		}
		catch (Exception ex)
		{
			exception = ex;
		}
		AssertNotNull("Should throw exception if passed LicenceEnterprise already contains LicenceDatabase with LD_ServerCode=='*99' and all similar variations with lesser numbers", exception);
		AssertEquals("Should throw exception if passed LicenceEnterprise already contains LicenceDatabase with LD_ServerCode=='*99' and all similar variations with lesser numbers", "cannot adjust code", exception.Message);
	}

	public void TestMoveLicencesToNewEnterpriseID_DatabaseLinkedByMultipleOrgs()
	{
		var lic1 = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "DB1", true);
		var sourceLicEnt = lic1.Database.LicEnterprise;
		var sourceLicDb1 = lic1.Database;
		sourceLicDb1.LD_TenantID = "TID001";
		var sourceOrgHeader = lic1.Company.Header;

		var lic2 = BillingTestHelper.CreateLicence(Factory, "EN2", "CO2", "DB2", true);
		var lic3 = Factory.New<LicenceHeader>();
		lic3.CopyPersistentValuesFrom(lic2);
		lic3.LA_LD = sourceLicDb1.PK;

		LicenceEnterprise targetLicEnt = Factory.NewWithValidTestData<LicenceEnterprise>();
		targetLicEnt.LE_EnterpriseID = "TLE";

		Factory.Save();

		MoveLicencesToNewEnterpriseIDBizOForTest moveBizO = new MoveLicencesToNewEnterpriseIDBizOForTest(sourceLicEnt, sourceOrgHeader, new MoveLicencesToNewEnterpriseIDBizOCallbacks());
		moveBizO.LicenceEnterpriseID = "TLE";
		moveBizO.MoveLicencesToNewEnterpriseID();
		AssertEquals(@"Licence Database with the server code 'DB1' cannot be copied because it is linked by multiple organizations and it has a unique Tenant ID.
No Licences will be copied.
Please call the administrator to resolve this problem.", UnitTestUserNotification.Instance.LastMessage.Text);
		AssertEquals(false, lic1.HasChanges);
		AssertEquals(false, lic2.HasChanges);
		AssertEquals(false, lic3.HasChanges);
		AssertEquals(false, sourceOrgHeader.HasChanges);
		AssertEquals(false, sourceLicEnt.HasChanges);
		AssertEquals(false, sourceLicDb1.HasChanges);
	}

	LicenceEnterprise LicEntToPassToCtor
	{
		get
		{
			if (fLicEntToPassToCtor == null)
			{
				fLicEntToPassToCtor = Factory.NewWithValidTestData<LicenceEnterprise>();
				fLicEntToPassToCtor.LE_EnterpriseID = "YYY";
			}
			return fLicEntToPassToCtor;
		}
	}
	LicenceEnterprise fLicEntToPassToCtor;

	LicenceEnterprise licEntForBizOPropertyTest
	{
		get
		{
			if (fLicEntToPassToCtor == null)
			{
				flicEntForBizOPropertyTest = Factory.NewWithValidTestData<LicenceEnterprise>();
				flicEntForBizOPropertyTest.LE_EnterpriseID = "AAA";
			}
			return flicEntForBizOPropertyTest;
		}
	}
	LicenceEnterprise flicEntForBizOPropertyTest;
}
