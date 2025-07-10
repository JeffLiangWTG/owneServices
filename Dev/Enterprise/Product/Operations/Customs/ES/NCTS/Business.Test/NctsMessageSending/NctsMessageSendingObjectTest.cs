using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(NctsMessageSendingObject))]
	public class NctsMessageSendingObjectTest : AbstractGenericMessageSendingObjectTest<NctsMessageSendingObject>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Should be exception when header param is null", () => new NctsMessageSendingObject(null, staff));
		}

		public void TestHeader()
		{
			CombineAssertions(() =>
			{
				var messageSending = GetNewMessageSendingObject(staff, brokerCertificate);
				AssertNotNull("Header should be not null", messageSending.Header);
				AssertEquals("Header PK", header.PK, messageSending.Header.PK);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.MovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
			header.BH_CustomsProfile = brokerCertificate;
		}
		NctsHeader header;

		protected override NctsMessageSendingObject GetNewMessageSendingObject(GlbStaff staff, ZString brokerCertificate) => new NctsMessageSendingObject(header, staff);
	}
}
