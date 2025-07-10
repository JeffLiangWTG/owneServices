
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public class JXCRecordFactory
	{
		public JXCRecord NewRecord(ZString recordLine)
		{
			JXCRecord result = null;

			ZString lineType = recordLine.Left(JXCConstants.LineTypeLength);
			int lineIdentifierLength = JXCConstants.LineTypeLength + JXCConstants.Version.Length + 1;
			ZString lineContent = recordLine.SubstringSafe(lineIdentifierLength);

			switch (lineType)
			{
				case JXCConstants.LineTypes.HEAD:
					result = new HEADRecord(lineType, lineContent);
					break;

				case JXCConstants.LineTypes.MAWB:
					result = new MAWBRecord(lineType, lineContent);
					break;

				case JXCConstants.LineTypes.DAWB:
					result = new DAWBRecord(lineType, lineContent);
					break;

				case JXCConstants.LineTypes.REFR:
					result = new REFRRecord(lineType, lineContent);
					break;

				case JXCConstants.LineTypes.FBDN:
					result = new FBDNRecord(lineType, lineContent);
					break;

				case JXCConstants.LineTypes.OTHR:
					result = new OTHRRecord(lineType, lineContent);
					break;

				case JXCConstants.LineTypes.SHMK:
					result = new SHMKRecord(lineType, lineContent);
					break;

				case JXCConstants.LineTypes.DHAB:
				case JXCConstants.LineTypes.DOHB:
					result = new DummyHouseRecord(lineType, lineContent);
					break;

				case JXCConstants.LineTypes.CHAB:
				case JXCConstants.LineTypes.HAWB:
				case JXCConstants.LineTypes.PSAB:
					result = new HAWBRecord(lineType, lineContent);
					break;

				case JXCConstants.LineTypes.OMAN:
					result = new OMANRecord(lineType, lineContent);
					break;

				case JXCConstants.LineTypes.OHBL:
				case JXCConstants.LineTypes.PSBL:
				case JXCConstants.LineTypes.COHB:
					result = new OHBLRecord(lineType, lineContent);
					break;

				case JXCConstants.LineTypes.CONT:
					result = new CONTRecord(lineType, lineContent);
					break;

				case JXCConstants.LineTypes.CHGS:
					result = new CHGSRecord(lineType, lineContent);
					break;

				case JXCConstants.LineTypes.TRLR:
					result = new TRLRRecord(lineType, lineContent);
					break;
			}

			return result;
		}
	}
}

#region Implementation
#endregion
