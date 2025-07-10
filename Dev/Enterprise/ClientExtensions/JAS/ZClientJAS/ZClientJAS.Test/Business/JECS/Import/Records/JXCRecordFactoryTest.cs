using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class JXCRecordFactoryTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestNewRecord_PassingInNull()
		{
			JXCRecord record = RecordFactory.NewRecord(null);
			AssertNull("Should return null if null is passed in", record);
		}

		public void TestNewRecord()
		{
			AssertNewRecord(JXCConstants.LineTypes.HEAD, typeof(HEADRecord));
			AssertNewRecord(JXCConstants.LineTypes.MAWB, typeof(MAWBRecord));
			AssertNewRecord(JXCConstants.LineTypes.DAWB, typeof(DAWBRecord));
			AssertNewRecord(JXCConstants.LineTypes.REFR, typeof(REFRRecord));
			AssertNewRecord(JXCConstants.LineTypes.FBDN, typeof(FBDNRecord));
			AssertNewRecord(JXCConstants.LineTypes.OTHR, typeof(OTHRRecord));
			AssertNewRecord(JXCConstants.LineTypes.DHAB, typeof(DummyHouseRecord));
			AssertNewRecord(JXCConstants.LineTypes.DOHB, typeof(DummyHouseRecord));
			AssertNewRecord(JXCConstants.LineTypes.HAWB, typeof(HAWBRecord));
			AssertNewRecord(JXCConstants.LineTypes.CHAB, typeof(HAWBRecord));
			AssertNewRecord(JXCConstants.LineTypes.PSAB, typeof(HAWBRecord));
			AssertNewRecord(JXCConstants.LineTypes.TRLR, typeof(TRLRRecord));
			AssertNewRecord(JXCConstants.LineTypes.SHMK, typeof(SHMKRecord));
			AssertNewRecord(JXCConstants.LineTypes.OMAN, typeof(OMANRecord));
			AssertNewRecord(JXCConstants.LineTypes.OHBL, typeof(OHBLRecord));
			AssertNewRecord(JXCConstants.LineTypes.COHB, typeof(OHBLRecord));
			AssertNewRecord(JXCConstants.LineTypes.PSBL, typeof(OHBLRecord));
			AssertNewRecord(JXCConstants.LineTypes.CONT, typeof(CONTRecord));
			AssertNewRecord(JXCConstants.LineTypes.CHGS, typeof(CHGSRecord));
		}

		#region Implementation
		void AssertNewRecord(ZString lineType, Type expectedJXCRecordType)
		{
			ZString recordLine = lineType + JXCConstants.Version + ";BLAH123";
			JXCRecord record = RecordFactory.NewRecord(recordLine);
			AssertEquals("Wrong type being returned", expectedJXCRecordType, record.GetType());
			AssertEquals("BLAH123", record.LineContent);
		}

		JXCRecordFactory RecordFactory
		{
			get
			{
				if (fRecordFactory == null)
				{
					fRecordFactory = new JXCRecordFactory();
				}

				return fRecordFactory;
			}
		}

		JXCRecordFactory fRecordFactory;
		#endregion
	}
}
