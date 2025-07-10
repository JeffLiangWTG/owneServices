using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(ECSMessageSendingObject))]
	public class ECSMessageSendingObjectTest : AbstractGenericMessageSendingObjectTest<ECSMessageSendingObject>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Should be exception when exitHeader param is null", () => new ECSMessageSendingObject(null, staff));
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

			exitHeader = Factory.New<CusExitControlHeader>();
			exitHeader.CEH_GS_NKCustomsAgent = staff.GS_Code;
			exitHeader.CEH_CustomsProfile = brokerCertificate;
		}
		CusExitControlHeader exitHeader;

		protected override ECSMessageSendingObject GetNewMessageSendingObject(GlbStaff staff, ZString brokerCertificate) => new ECSMessageSendingObject(exitHeader, staff);
	}
}
