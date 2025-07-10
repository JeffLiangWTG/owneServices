using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(MessageSendingObject))]
	public class MessageSendingObjectTest : AbstractGenericMessageSendingObjectTest<MessageSendingObject>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Should be exception when declaration param is null", () => new MessageSendingObject(null, staff));
		}

		public void TestDeclaration()
		{
			CombineAssertions(() =>
			{
				var messageSending = GetNewMessageSendingObject(staff, brokerCertificate);
				AssertNotNull("Declaration", messageSending.Declaration);
				AssertEquals("Declaration PK", declaration.PK, messageSending.Declaration.PK);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			declaration.JE_CustomsProfile = brokerCertificate;
		}
		JobDeclaration declaration;

		protected override MessageSendingObject GetNewMessageSendingObject(GlbStaff staff, ZString brokerCertificate) => new MessageSendingObject(declaration, staff);
	}
}
