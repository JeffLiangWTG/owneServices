using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.TNT.Testing
{
	public class IQDownRecordFactoryTest : TestCase
	{
		public void TestNewRecord()
		{
			IQDownRecordFactory factory = new IQDownRecordFactory();
			NotificationBuffer buffer = new NotificationBuffer();
			IQDownBaseRecord record = factory.NewRecord(FlightLine, buffer);
			AssertEquals("Buffer should not have error - Buffer contains:" + System.Environment.NewLine + buffer.AsString, false, buffer.HasErrors);
			AssertNotNull("Record should be not null", record);
			AssertEquals("Record type should be FlightRecord", typeof(FlightRecord), record.GetType());
			record = factory.NewRecord(TBagLine, buffer);
			AssertEquals("Buffer should not have error - Buffer contains:" + System.Environment.NewLine + buffer.AsString, false, buffer.HasErrors);
			AssertNotNull("Record should be not null", record);
			AssertEquals("Record type should be TBagRecord", typeof(TBagRecord), record.GetType());
			record = factory.NewRecord(ConsignmentLine, buffer);
			AssertEquals("Buffer should not have error - Buffer contains:" + System.Environment.NewLine + buffer.AsString, false, buffer.HasErrors);
			AssertNotNull("Record should be not null", record);
			AssertEquals("Record type should be ConsignmentRecord", typeof(ConsignmentRecord), record.GetType());
			record = factory.NewRecord(ConsignmentNoteLine, buffer);
			AssertEquals("Buffer should not have error - Buffer contains:" + System.Environment.NewLine + buffer.AsString, false, buffer.HasErrors);
			AssertNotNull("Record should be not null", record);
			AssertEquals("Record type should be ConsignmentNoteRecord", typeof(ConsignmentNoteRecord), record.GetType());
			record = factory.NewRecord("01", buffer);
			AssertNull("Record should be null", record);
			AssertEquals("Buffer should have error - Buffer contains:" + System.Environment.NewLine + buffer.AsString, true, buffer.HasErrors);
			AssertEquals("Buffer should have error type 'InvalidFileFormat' - Buffer contains:" + System.Environment.NewLine + buffer.AsString, true, buffer.ContainsNotificationType(TNTErrorType.InvalidFileFormat));
			string errorMessage = "Record line length too short";
			AssertEquals("Buffer should have error message '" + errorMessage + "' - Buffer contains:" + System.Environment.NewLine + buffer.AsString, true, buffer.AsString.IndexOf(errorMessage) >= 0);
			buffer.Clear();
			record = factory.NewRecord("BLAH BLAH", buffer);
			AssertNull("Record should be null", record);
			AssertEquals("Buffer should have error - Buffer contains:" + System.Environment.NewLine + buffer.AsString, true, buffer.HasErrors);
			AssertEquals("Buffer should have error type 'UnknownRecordType' - Buffer contains:" + System.Environment.NewLine + buffer.AsString, true, buffer.ContainsNotificationType(TNTErrorType.UnknownRecordType));
			errorMessage = "Not an IQDown record";
			AssertEquals("Buffer should have error message '" + errorMessage + "' - Buffer contains:" + System.Environment.NewLine + buffer.AsString, true, buffer.AsString.IndexOf(errorMessage) >= 0);
		}

		const string FlightLine = "01BA0151SINSYD150705A12527013486 M0000151  SINSYDTP   231.423                                                                                                                                                                                                                                                                                                                                                                                                                                            .";
		const string TBagLine = "02B1927029  TBAGNO    SINSYD                                                                                                                                                                                                                                                                                                                                                                                                                                                                             .";
		const string ConsignmentLine = "03940432180 ADLUSO20908767COVANCE P/L                    FL 3 4 RESEARCH PARK DR        MACQUARIE UNI                  NORTH RYDE                     NEW SOUTH WALES                AU 2113     0288792000  0288792000  BOB SMITH             " + "SLEEP LAB LEVEL 6 MCEWIN BLDG  ROYAL ADELAIDE HOSPITAL        NORTH TERRACE                  ADELAIDE                       SOUTH AUSTRALIA                AU 5000     08485648144 08468451155 GAIL KOSHOREK         " + "HENRY FORD HOSPITAL            SLEEP DISORDERS RESEARCH CENT  2799 W GRAND BLVD CFP 3NIK     DETROIT                        MI                             US 48202    68454578982 12345678901 GAIL KOSHOREK         " + "DELIVERY COMPANY NAME          DELIVERY ADDRESS 1             DELIVERY ADDRESS 2             DELIVERY CITY                  DELIVERY STATE                 CAN123456789012345678901234567890123DELIVERY CONTACT NAME " + "NS1233233234.34AUD123456123456.620T-DOC1234Delivery Instructions for the remark    12345678901212345678901               .";
		const string ConsignmentNoteLine = "04940432180 01123456789012345DOCUMENTS AND DOCS.IN FOLDER                                                  " + "DEscription 2                                                                 " + "Description 3                                                                 " + "MELIAHEX2456789012                                                                                                                                                                                                                .";
	}
}
