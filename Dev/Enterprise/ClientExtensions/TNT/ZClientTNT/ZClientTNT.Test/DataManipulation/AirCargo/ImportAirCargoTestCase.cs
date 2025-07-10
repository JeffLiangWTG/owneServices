using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TNT.AirCargo.Testing
{
	public abstract class ImportAirCargoTestCase : NonPersistentBusinessObjectTestCase
	{
		public void TestIsValid()
		{
			ImportAirCargo airCargo = GetNewBusinessObject() as ImportAirCargo;
			AssertNotNull(airCargo);
			airCargo.Buffer.Clear();
			AssertEquals("AirCargo.IsValid should be true", true, airCargo.IsValid);
			AssertEquals("Buffer should not contain any errors: Buffer contains:" + System.Environment.NewLine + airCargo.Buffer.AsString, false, airCargo.Buffer.HasErrors);
			UnitTestUserNotification userNotification = Globals.Message as UnitTestUserNotification;
			AssertNotNull("UserNotification should not be null", userNotification);
			userNotification.ClearMessagesAndAnswers();
			AssertEquals("Initial Length (UserNotification.None)", 1, userNotification.PreviousMessages.Length);
			string expectedMessage = "TEST ERROR NOTIFY WITH YES RESPONSE";
			airCargo.Buffer.Notify(new ErrorNotification(ErrorType.Error, expectedMessage));
			userNotification.AddAnswer(DialogResult.Yes);
			AssertEquals("AirCargo.IsValid should be true as there are errors with response YES", true, airCargo.IsValid);
			AssertEquals("Buffer should contain errors: Buffer contains:" + System.Environment.NewLine + airCargo.Buffer.AsString, true, airCargo.Buffer.HasErrors);
			var lastMessage = userNotification.LastMessage.Text ?? string.Empty;
			AssertEquals("Expected 1 new error message", 2, userNotification.PreviousMessages.Length);
			AssertEquals("Last Error Message:" + System.Environment.NewLine + lastMessage, true, lastMessage.IndexOf(expectedMessage) >= 0);
			userNotification.ClearMessagesAndAnswers();
			airCargo.Buffer.Clear();
			AssertEquals("Initial Length (UserNotification.None)", 1, userNotification.PreviousMessages.Length);
			expectedMessage = "TEST ERROR NOTIFY WITH NO RESPONSE";
			airCargo.Buffer.Notify(new ErrorNotification(ErrorType.Error, expectedMessage));
			userNotification.AddAnswer(DialogResult.No);
			AssertEquals("AirCargo.IsValid should be false as there are errors with response NO", false, airCargo.IsValid);
			AssertEquals("Buffer should contain errors: Buffer contains:" + System.Environment.NewLine + airCargo.Buffer.AsString, true, airCargo.Buffer.HasErrors);
			lastMessage = userNotification.LastMessage.Text ?? string.Empty;
			AssertEquals("Expected 1 new error message", 2, userNotification.PreviousMessages.Length);
			AssertEquals("Last Error Message:" + System.Environment.NewLine + lastMessage, true, lastMessage.IndexOf(expectedMessage) >= 0);
		}

		#region Implementation
		protected CusMAWB CreateDummyCusMAWB(ZString masterBill, ZString flightNo, ZDateTime arrivalDate, ZString portOfLoading, ZString portOfDischarge)
		{
			CusMAWB result = Factory.New<CusMAWB>();
			result.CM_MAWB = masterBill;
			result.CM_FlightNo = flightNo;
			result.CM_ArrivalDate = arrivalDate;
			result.CM_RL_NKLoadPort = portOfLoading;
			result.CM_RL_NKDischargePort = portOfDischarge;
			return result;
		}

		#region Assertions
		protected void AssertPopulateCusMAWB(CusMAWB masterBill, ImportAirCargo airCargo)
		{
			AssertNotNull("MasterBill is not null", masterBill);
			AssertNotNull("AirCargo is not null", airCargo);
			AssertEqualsIgnoreTrailingSpaces("MasterBill no", airCargo.MasterBill, masterBill.CM_MAWB);
			AssertEqualsIgnoreTrailingSpaces("FlightNo", airCargo.FlightNo, masterBill.CM_FlightNo);
			AssertEquals("ArrivalDate", airCargo.ArrivalDate, masterBill.CM_ArrivalDate);
			AssertEqualsIgnoreTrailingSpaces("PortOfLoading", airCargo.PortOfLoading, masterBill.CM_RL_NKLoadPort);
			AssertEqualsIgnoreTrailingSpaces("PortOfDischarge", airCargo.PortOfDischarge, masterBill.CM_RL_NKDischargePort);
			AssertEquals("Masterbill Branch", SydBranch.PK, masterBill.CM_GB);
			AssertEquals("Flight Date is also departure date", airCargo.DepartureDate, masterBill.CM_DepartureDate);
		}

		protected void AssertEqualsIgnoreTrailingSpaces(string message, string expected, string actual)
		{
			AssertEquals(message, expected.TrimEnd(), actual.TrimEnd());
		}

		#endregion
		#region SydBranch
		protected GlbBranch SydBranch
		{
			get
			{
				if (fSydBranch == null)
				{
					fSydBranch = GetSydBranch();
				}

				return fSydBranch;
			}
		}

		GlbBranch fSydBranch;
		GlbBranch GetSydBranch()
		{
			string branchCode = "SYD";
			GlbBranch result = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, branchCode);
			if (result == null)
			{
				result = Factory.New<GlbBranch>();
				result.GB_Code = branchCode;
				Factory.Save();
			}

			return result;
		}

		#endregion ;
		#region FlightRec
		protected FlightRecord FlightRec
		{
			get
			{
				if (fFlightRec == null)
				{
					fFlightRec = new FlightRecord(FlightDetailLine);
				}

				return fFlightRec;
			}
		}

		FlightRecord fFlightRec;
		#endregion
		#region Consignment
		protected ConsignmentRecord Consignment
		{
			get
			{
				if (fConsignment == null)
				{
					fConsignment = new ConsignmentRecord(ConsignmentLine);
				}

				return fConsignment;
			}
		}

		ConsignmentRecord fConsignment;
		#endregion
		#region ConsignmentNote
		protected ConsignmentNoteRecord ConsignmentNote
		{
			get
			{
				if (fConsignmentNote == null)
				{
					fConsignmentNote = new ConsignmentNoteRecord(ConsignmentNoteLine);
				}

				return fConsignmentNote;
			}
		}

		ConsignmentNoteRecord fConsignmentNote;
		#endregion
		#region Buffer
		protected NotificationBuffer Buffer
		{
			get
			{
				if (fBuffer == null)
				{
					fBuffer = new NotificationBuffer();
				}

				return fBuffer;
			}
		}

		NotificationBuffer fBuffer;
		#endregion
		protected const string FlightDetailLine = "01BA0151SINWHR150705A12527013486 M0000151  SINWHRTP   231.423                                                                                                                                                                                                                                                                                                                                                                                                                                            .";
		protected const string ConsignmentLine = "03940432180 SINWHR20908767HENRY FORD HOSPITAL            SLEEP DISORDERS RESEARCH CENT  2799 W GRAND BLVD CFP 3NIK     DETROIT                        MI                             SG 48202    68454578982 12345678901 GAIL KOSHOREK         " + "DELIVERY COMPANY NAME          DELIVERY ADDRESS 1             DELIVERY ADDRESS 2             DELIVERY CITY                  DELIVERY STATE                 CAN123456789012345678901234567890123DELIVERY CONTACT NAME " + "COVANCE P/L                    FL 3 4 RESEARCH PARK DR        MACQUARIE UNI                  NORTH RYDE                     WHERE IS THIS STATE            AU 2113     0288792000  0288792000  BOB SMITH             " + "SLEEP LAB LEVEL 6 MCEWIN BLDG  ROYAL ADELAIDE HOSPITAL        NORTH TERRACE                  ADELAIDE                       SA                             AU 5000     08485648144 08468451155 GAIL KOSHOREK         " + "NS1233233234.34AUD12345 123456.620T-DOC1234Delivery Instructions for the remark    12345678901212345678901               .";
		protected const string ConsignmentNoteLine = "04940432180 01123456789012345USED CLOTHES TOILET BAG                                                                                                                                                                                                                   SINWHR                                                                                                                                                                                                                            .";
		#endregion
	}
}
