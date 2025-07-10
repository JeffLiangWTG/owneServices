using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	[TestedType(typeof(MessageSendingObject))]
	class MessageSendingObjectTest : AbstractGenericMessageSendingObjectTest<MessageSendingObject>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Should be exception when cusExitHeader param is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "cusExitHeader"), () => new MessageSendingObject(null, staff));
		}

		public void TestExitHeader()
		{
			CombineAssertions(() =>
			{
				var messageSending = GetNewMessageSendingObject(staff, brokerCertificate);
				AssertNotNull("ExitHeader", messageSending.ExitHeader);
				AssertEquals("ExitHeader PK", exitHeader.PK, messageSending.ExitHeader.PK);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			exitHeader = Factory.New<CusExitHeader>();
			exitHeader.CXH_GS_NKCustomsAgent = staff.GS_Code;
			exitHeader.CXH_CustomsProfile = brokerCertificate;
		}
		CusExitHeader exitHeader;

		protected override MessageSendingObject GetNewMessageSendingObject(GlbStaff staff, ZString brokerCertificate) => new MessageSendingObject(exitHeader, staff);
	}
}
