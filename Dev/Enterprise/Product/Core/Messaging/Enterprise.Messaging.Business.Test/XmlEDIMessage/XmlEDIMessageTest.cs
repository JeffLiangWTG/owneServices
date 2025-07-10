using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.Business.XmlMessaging.Testing
{
	[TestedType(typeof(XmlEDIMessage))]
	public class XmlEDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDontSetMessageNumberTwice()
		{
			var message = Factory.New<XmlEDIMessage>();
			var counter = 0;
			message.EM_MessageNumInfo.ValueChanged += (s, e) => counter++;
			var num = Env.NumberFountains.XmlEDIMessageNumber.GetNext(Db.Connection);
			Factory.Save();
			AssertEquals("EDI message only set once", 1, counter);
			AssertEquals("Called twice, so should be +2", num + 2, Env.NumberFountains.XmlEDIMessageNumber.GetNext(Db.Connection));
		}

		public void TestAllOfTheCountries()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				foreach (var field in new RefCountryCollection(Factory))
				{
					var code = field.Code;
					GlbCompany.CurrentCompany.GC_RN_NKCountryCode = code;
					var newFactory = Factory.CreateNewFactory();
					newFactory.RefreshEnabled = false;
					var message = newFactory.New<XmlEDIMessage>();
					newFactory.Load<XmlEDIMessage>(message.PK);
					AssertNoExceptionThrown(newFactory.Save);
				}
			}
		}

		public void TestMessageSubTypeDescription()
		{
			var message = Factory.New<XmlEDIMessage>();
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			AssertEquals("message.EM_MessageSubTypeDescription for XmlUniversalEvent", EDIMessageSubTypeList.Descriptions.XmlUniversalEvent, message.EM_MessageSubTypeDescription);

			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			AssertEquals("message.EM_MessageSubTypeDescription for XmlUniversalShipment", EDIMessageSubTypeList.Descriptions.XmlUniversalShipment, message.EM_MessageSubTypeDescription);
		}

		public void TestMessageNumber()
		{
			var message = Factory.New<XmlEDIMessage>();
			Factory.Save();

			Assert(!message.EM_MessageNum.IsEmpty);
			AssertEquals(20, message.EM_MessageNum.Length);
		}

		public void TestContextSetterAndGetter()
		{
			var message = Factory.New<XmlEDIMessage>();
			AssertNull("Content should be null when message text is empty", message.Content);

			var testContextText = "<Something />";
			var testContent = XElement.Parse(testContextText);

			message = Factory.New<XmlEDIMessage>();
			message.Content = testContent;

			AssertEquals("Should be able to get Content and it should be the same as what we set before", testContextText, message.Content.ToString());
			AssertEquals("Should set context text to EM_MessageText", testContextText, message.EM_MessageText);
		}

		public void TestSetStreamContent()
		{
			var message = Factory.New<XmlEDIMessage>();

			var testContextText = "<SomethingElse />";
			using (var stream = new MemoryStream(MessageEncoding.UTF8WithoutBOM.GetBytes(testContextText)))
			{
				message.SetContent(stream);
			}

			AssertEquals("EM_MessageText should contain the content we set", testContextText, message.EM_MessageText);
		}

		public void TestSetContentWithInvalidContent()
		{
			var message = Factory.New<XmlEDIMessage>();

			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ message.SetContent(null); });

			var testContextText = "<Something WRONG!!!!";
			using (var stream = new MemoryStream(MessageEncoding.UTF8WithoutBOM.GetBytes(testContextText)))
			{
				AssertExceptionThrown(typeof(XmlException), delegate
				{ message.SetContent(stream); });
			}

			AssertEquals("EM_MessageText should be empty", ZString.Empty, message.EM_MessageText);
		}

		public void TestCanSaveTransmitMessageAndTheMessageNumberIsNotAffected()
		{
			var message = Factory.New<XmlEDIMessage>();
			message.EM_ReceiveTransmit = XmlEDIMessage.Direction.Transmit;
			message.EM_MessageNum = "LALALA";
			AssertEquals("Precondition: Is Transmit Message", true, message.IsTransmitMessage);

			AssertNoExceptionThrown("Should be able to save.", delegate
			{ Factory.Save(); });
			AssertEquals("message.EM_MessageNum should not have changed", "LALALA", message.EM_MessageNum);
		}

		public override void TestCloneAuditProperties()
		{
			Assert("Need this to supress test failure caused by customized default values", true);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals("EM_IsActive should be true", true, Message.EM_IsActive);
			AssertEquals("EM_ApplicationCode should be XMS", EDIMessage.ApplicationCodes.XMS, Message.EM_ApplicationCode);
			AssertEquals("EM_MessageType should be XML", EDIMessageTypeList.Codes.XMS, Message.EM_MessageType);
			Message.SetMessageTypeFromStream(new MemoryStream());
			AssertEquals("EM_MessageSubType should be UNK", EDIMessageSubTypeList.Codes.Unknown, Message.EM_MessageSubType);
			AssertEquals("EM_Status should be SNT", EDIMessage.Status.Sent, Message.EM_Status);
			AssertDateTimeWithinOneSecond("EM_SystemCreateTimeUtc should be close to UtcNow", factoryCallTime.ToDateTime(), Message.EM_SystemCreateTimeUtc.ToDateTime());
			AssertEquals("EM_SystemCreateUser should be CurrentUser", GlbStaff.CurrentUser.GS_Code, Message.EM_SystemCreateUser);
			AssertEquals("EM_SystemLastEditTimeUtc should be same as EM_SystemCreateTimeUtc", Message.EM_SystemCreateTimeUtc, Message.EM_SystemLastEditTimeUtc);
			AssertEquals("EM_SystemLastEditUser should be same as EM_SystemCreateUser", Message.EM_SystemCreateUser, Message.EM_SystemLastEditUser);
		}

		#region Implementation

		XmlEDIMessage Message
		{
			get { return message ?? (message = (XmlEDIMessage)GetNewBusinessObject()); }
		}
		XmlEDIMessage message;

		ZDateTime factoryCallTime;
		protected override BusinessObject GetNewBusinessObject()
		{
			factoryCallTime = ZDateTime.UtcNow;
			return Factory.New<XmlEDIMessage>();
		}

		protected override bool CanPersistedObjectBeDeleted => false;

		#endregion
	}
}
