using System;
using NUnit.Framework;

namespace Enterprise.Licensing
{
	sealed class SystemUpdatePacketTest : TestCase
	{
		public void TestConstructWithXMLKey()
		{
			SystemUpdatePacket packet = new SystemUpdatePacket(ValidTestXMLKey);
			AssertEquals(TestRegKey, packet.EncryptedSysRegKey);
			AssertEquals(TestLicenceKey, packet.EncryptedLicenceKey);
		}

		public void TestToEncryptedKeyString()
		{
			SystemUpdatePacket packet1 = new SystemUpdatePacket();
			packet1.EncryptedLicenceKey = TestLicenceKey;
			packet1.EncryptedSysRegKey = TestRegKey;

			SystemUpdatePacket packet2 = new SystemUpdatePacket(ValidTestXMLKey);

			AssertEquals(packet1.ToXMLString(), packet2.ToXMLString());
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), SystemUpdatePacket.PacketWasCorrupt)]
		public void TestCorruptSystemPacket()
		{
			SystemUpdatePacket packet = new SystemUpdatePacket("blah");
		}

		public void TestInvalidPacketHash()
		{
			try
			{
				SystemUpdatePacket packet = new SystemUpdatePacket(InvalidHashTestXMLKey);
			}
			catch (Exception e)
			{
				Assert("Outer Exception is Invalid Operation", e is InvalidOperationException);
				AssertEquals("Outer Exception message", "The System Information Packet Was Corrupt", e.Message);
				Assert("Inner Exception is Invalid Operation", e.InnerException is InvalidOperationException);
				AssertEquals("Inner Exception message", "The packet hash does not match its original contents", e.InnerException.Message);
			}
		}

		#region Set up

		string ValidTestXMLKey
		{
			get
			{
				string template = new SystemUpdatePacket().XMLTemplate;
				string packetForHashing = String.Format(template, TestRegKey, TestLicenceKey, String.Empty);
				return String.Format(template, TestRegKey, TestLicenceKey, new SystemUpdatePacket().GenerateHash(packetForHashing));
			}
		}

		string InvalidHashTestXMLKey
		{
			get { return String.Format(new SystemUpdatePacket().XMLTemplate, TestRegKey, TestLicenceKey, "ABCDEFG"); }
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestRegKey = "ABCDEFG";
			TestLicenceKey = "HIJKLMNOP";
		}

		string TestRegKey;
		string TestLicenceKey;

		#endregion
	}
}
