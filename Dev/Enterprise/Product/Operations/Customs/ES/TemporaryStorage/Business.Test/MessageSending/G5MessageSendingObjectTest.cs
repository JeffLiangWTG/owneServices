using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.Testing;

[TestedType(typeof(G5MessageSendingObject))]
public class G5MessageSendingObjectTest : AbstractGenericMessageSendingObjectTest<G5MessageSendingObject>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Should be exception when header param is null", () => new G5MessageSendingObject(null, staff));
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
		header = Factory.New<TemporaryStorageHeader>();
		header.AMA_GS_NKCustomsAgent = staff.GS_Code;
		header.AMA_CustomsProfile = brokerCertificate;
	}
	TemporaryStorageHeader header;

	protected override G5MessageSendingObject GetNewMessageSendingObject(GlbStaff staff, ZString brokerCertificate) => new G5MessageSendingObject(header, staff);
}
