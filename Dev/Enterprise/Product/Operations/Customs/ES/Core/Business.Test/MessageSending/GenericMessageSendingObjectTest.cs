using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(GenericMessageSendingObject))]
	public class GenericMessageSendingObjectTest : AbstractGenericMessageSendingObjectTest<GenericMessageSendingObject>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Should be exception when broker param is null", () => new GenericMessageSendingObject(null, ZString.Empty));
		}

		protected override GenericMessageSendingObject GetNewMessageSendingObject(GlbStaff staff, ZString brokerCertificate) => new GenericMessageSendingObject(staff, brokerCertificate);
	}

	public abstract class AbstractGenericMessageSendingObjectTest<T> : NonPersistentBusinessObjectTestCase
		where T : GenericMessageSendingObject
	{
		protected override BusinessObject GetNewBusinessObject() => GetNewMessageSendingObject(staff, brokerCertificate);

		public void TestBroker()
		{
			CombineAssertions(() =>
			{
				var messageSending = GetNewMessageSendingObject(staff, brokerCertificate);
				AssertNotNull("Broker", messageSending.Broker);
				AssertEquals("Broker PK", staff.PK, messageSending.Broker.PK);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AH";
			staff.GS_LoginName = "ahtest";
			staff.StaffPlainTextPassword = "1234";
			wrapper = GlbStaffWrapper.Get(staff);
			var cert = wrapper.ESBPasswordCollection.AddNew();
			cert.GP_Name = brokerCertificate;
			cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			cert.GP_MailBoxID = certificateNif;
			Factory.Save();
		}

		protected abstract T GetNewMessageSendingObject(GlbStaff staff, ZString brokerCertificate);

		GlbStaffWrapper wrapper;
		protected GlbStaff staff;
		protected ZString brokerCertificate = "TestCert1";
		protected ZString certificateNif = "12345678X";
	}
}
