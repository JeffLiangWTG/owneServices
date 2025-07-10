using CargoWise.EntityFramework;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace ZClientEDI.Test.Licencing;

[TestedType(typeof(LicenceDatabase))]
public class LicenceDatabaseTest : SecurityBusinessObjectTestCase
{
	[GuiTest]
	public void TestReadOnlySecurity_InnerObjects()
	{
		bool oldDatabaseDetailsSecurityCheckpoint = EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails.IsAllowed;
		bool oldConnectionDetailsCheckPointValue = EDISecurityCheckpoints.OrgLicenceModifyConnectionDetails.IsAllowed;
		bool oldSupportAndContractDetails = EDISecurityCheckpoints.OrgLicenceModifySupportAndContractDetails.IsAllowed;

		EDIOrgHeader organization = HeaderForTest;
		organization.CreateAndLoadLicenceForOrg();
		organization.GenerateNewLicenceCode();
		LicenceDatabase database = organization.LicCompany.LicDatabases.AddNew();

		try
		{
			EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails.IsAllowed = false;
			EDISecurityCheckpoints.OrgLicenceModifyConnectionDetails.IsAllowed = false;
			EDISecurityCheckpoints.OrgLicenceModifySupportAndContractDetails.IsAllowed = true;
			using (var form = new CargoWise.Windows.UI.KForm())
			{
				var control = new ZDateEdit();
				form.Controls.Add(control);
				CargoWise.Windows.UI.DataBoundControl.GetDefaultImplementation(control).SetDataBinding(database, "LicHeadersForAllCompanies.LA_SupportStartDate");
				form.Show();
				System.Windows.Forms.Application.DoEvents();
				AssertEquals("ReadOnly set once the form is shown.", false, control.ReadOnly);
			}
		}
		finally
		{
			EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails.IsAllowed = oldDatabaseDetailsSecurityCheckpoint;
			EDISecurityCheckpoints.OrgLicenceModifyConnectionDetails.IsAllowed = oldConnectionDetailsCheckPointValue;
			EDISecurityCheckpoints.OrgLicenceModifySupportAndContractDetails.IsAllowed = oldSupportAndContractDetails;
		}
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		EDIOrgHeader testHeader = HeaderForTest;
		testHeader.CreateAndLoadLicenceForOrg();
		LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
		return database;
	}

	EDIOrgHeader HeaderForTest
	{
		get
		{
			if (headerForTest == null)
			{
				headerForTest = Factory.NewWithValidTestData<EDIOrgHeader>();
				headerForTest.OH_RL_NKClosestPort = "AUBNE";
				headerForTest.OH_FullName = "My Organisation";
				headerForTest.OH_Code = "TGBLOG";

				OrgAddress newAddress = headerForTest.Addresses.AddNew();
				newAddress.OA_Address1 = "666 Test Address";
			}

			return headerForTest;
		}
	}
	EDIOrgHeader headerForTest;

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		EDIOrgHeader testHeader = HeaderForTest;
		testHeader.CreateAndLoadLicenceForOrg();
		LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
		return database;
	}
}
