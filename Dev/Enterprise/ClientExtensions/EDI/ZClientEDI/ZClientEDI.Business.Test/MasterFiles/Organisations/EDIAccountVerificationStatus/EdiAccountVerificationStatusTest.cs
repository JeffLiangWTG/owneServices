using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Organisations.EDIAccountVerificationStatus;
using Enterprise.Client.EDI.UserManagement.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Organisation.Business.Test
{
	[TestedType(typeof(EdiAccountVerificationStatus))]
	public class EdiAccountVerificationStatusTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAllPropertiesAreCorrect()
		{
			var status = new EdiAccountVerificationStatus(Factory);

			status.ContactRelationshipStatus = ContactRelationshipStatusList.Codes.EmailChanged;
			status.Product = "CW1";
			status.LicenceType = "Full";
			status.ServerCode = "SC1";
			status.IsActive = ZBool.True;
			status.UserID = "150";
			status.FullName = "Jason Foo";
			status.Email = "jf@mail.com";

			AssertEquals("EdiAccountVerificationStatus.ContactRelationshipStatus", status.ContactRelationshipStatus, ContactRelationshipStatusList.Codes.EmailChanged);
			AssertEquals("EdiAccountVerificationStatus.Product", status.Product, "CW1");
			AssertEquals("EdiAccountVerificationStatus.LicenceType", status.LicenceType, "Full");
			AssertEquals("EdiAccountVerificationStatus.ServerCode", status.ServerCode, "SC1");
			AssertEquals("EdiAccountVerificationStatus.IsActive", status.IsActive, ZBool.True);
			AssertEquals("EdiAccountVerificationStatus.UserID", status.UserID, "150");
			AssertEquals("EdiAccountVerificationStatus.FullName", status.FullName, "Jason Foo");
			AssertEquals("EdiAccountVerificationStatus.Email", status.Email, "jf@mail.com");
		}
	}
}
