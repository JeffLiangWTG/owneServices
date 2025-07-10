using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(LPCOMessageSendingObjectParent))]
	public class LPCOMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTopLevelBusinessObject()
		{
			var permitObjParent = (LPCOMessageSendingObjectParent)GetNewBusinessObject();
			AssertSame("TopLevelBusinessObject", header, permitObjParent.TopLevelBusinessObject);
		}

		public void TestSecurityCheckpointToSendWithMessageError()
		{
			var permitObjParent = (LPCOMessageSendingObjectParent)GetNewBusinessObject();
			AssertEquals("SecurityCheckpointToSendWithMessageError", Env.Security.None, permitObjParent.SecurityCheckpointToSendWithMessageError);
		}

		public void TestSendingObjectsCollection()
		{
			var permitObjParent = (LPCOMessageSendingObjectParent)GetNewBusinessObject();
			AssertEquals(1, permitObjParent.SendingObjectsCollection.Count);
		}

		public void TestMessageSendingObjectProperties()
		{
			var permitObjParent = (LPCOMessageSendingObjectParent)GetNewBusinessObject();

			var properties = permitObjParent.MessageSendingObjectProperties;

			CombineAssertions(() =>
			{
				AssertEquals("Properties count", 9, properties.Count());

				AssertEquals(nameof(LPCOMessageSendingObject.MessageType), properties.ElementAt(0).PropertyName);
				AssertEquals(nameof(LPCOMessageSendingObject.SubmittedDate), properties.ElementAt(1).PropertyName);
				AssertEquals(nameof(LPCOMessageSendingObject.CustomsStatus), properties.ElementAt(2).PropertyName);
				AssertEquals(nameof(LPCOMessageSendingObject.Reason), properties.ElementAt(3).PropertyName);
				AssertEquals(nameof(LPCOMessageSendingObject.Requirement), properties.ElementAt(4).PropertyName);
				AssertEquals(nameof(LPCOMessageSendingObject.PermitNumber), properties.ElementAt(5).PropertyName);
				AssertEquals(nameof(LPCOMessageSendingObject.MessageStatusDescription), properties.ElementAt(6).PropertyName);
				AssertEquals(nameof(LPCOMessageSendingObject.NewEffectiveDate), properties.ElementAt(7).PropertyName);
				AssertEquals(nameof(LPCOMessageSendingObject.Message), properties.ElementAt(8).PropertyName);
			});
		}

		public void TestBrokerCode()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_WorkPhone = "0412345678";
			staff.GS_FullName = "USERPERMIT";
			staff.GS_Code = "PRT";
			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var permitObjParent = (LPCOMessageSendingObjectParent)GetNewBusinessObject();
				AssertEquals("Broker Code should be", "PRT", permitObjParent.BrokerCode);

				permitObjParent.BrokerCode = ZString.Empty;
				AssertEquals("Broker Code should be", ZString.Empty, permitObjParent.BrokerCode);
			}
		}

		public void TestBrokerCertificate()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_WorkPhone = "0412345678";
			staff.GS_FullName = "USERPERMIT";
			staff.GS_Code = "PRT";

			var password = BRGlbStaffWrapper.Get(staff).CCTPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(10);
			password.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var permitObjParent = (LPCOMessageSendingObjectParent)GetNewBusinessObject();
				AssertEquals("BrokerCertificate should be", password, permitObjParent.BrokerCertificate);

				permitObjParent.BrokerCode = ZString.Empty;
				AssertEquals("BrokerCertificate should be", null, permitObjParent.BrokerCertificate);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			header = Factory.NewWithValidTestData<CusLPCOHeader>();
			return new LPCOMessageSendingObjectParent(header);
		}

		CusLPCOHeader header;
	}
}
