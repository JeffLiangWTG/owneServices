using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.GB.Chief
{
	public class StatusChecker : IStatusChecker
	{
		#region IStatusChecker Members

		public string GetStatusCodeFromRouteOfEntry(string routeCode)
		{
			return StatusChecker.GetStatusCodeFromRouteOfEntryStatic(routeCode);
		}

		public static string GetStatusCodeFromRouteOfEntryStatic(string routeCode)
		{
			switch (routeCode)
			{
				case "0":
					return EntryStatusList.Codes.Route0;
				case "0H":
					return EntryStatusList.Codes.Route0H;
				case "0P":
					return EntryStatusList.Codes.Route0P;
				case "0X":
					return EntryStatusList.Codes.Route0X;
				case "1":
					return EntryStatusList.Codes.Route1;
				case "1 ":
					return EntryStatusList.Codes.Route1Space;
				case "15":
					return EntryStatusList.Codes.Route1_5;
				case "1F":
					return EntryStatusList.Codes.Route1F;
				case "1H":
					return EntryStatusList.Codes.Route1H;
				case "1P":
					return EntryStatusList.Codes.Route1P;
				case "1X":
					return EntryStatusList.Codes.Route1X;
				case "1Y":
					return EntryStatusList.Codes.Route1Y;
				case "1Z":
					return EntryStatusList.Codes.Route1Z;
				case "2":
					return EntryStatusList.Codes.Route2;
				case "2 ":
					return EntryStatusList.Codes.Route2Space;
				case "25":
					return EntryStatusList.Codes.Route2_5;
				case "2F":
					return EntryStatusList.Codes.Route2F;
				case "2H":
					return EntryStatusList.Codes.Route2H;
				case "2P":
					return EntryStatusList.Codes.Route2P;
				case "2X":
					return EntryStatusList.Codes.Route2X;
				case "2Y":
					return EntryStatusList.Codes.Route2Y;
				case "2Z":
					return EntryStatusList.Codes.Route2Z;
				case "3":
					return EntryStatusList.Codes.Route3;
				case "5":
					return EntryStatusList.Codes.Route5;
				case "6":
					return EntryStatusList.Codes.Route6;
				case "E":
					return EntryStatusList.Codes.RouteE;
				case "F":
					return EntryStatusList.Codes.RouteF;
				case "H":
					return EntryStatusList.Codes.RouteH;
				case "H1":
					return EntryStatusList.Codes.ProbableRoute1;
				case "H2":
					return EntryStatusList.Codes.ProbableRoute2;
				case "H3":
					return EntryStatusList.Codes.ProbableRoute3;
				case "H5":
					return EntryStatusList.Codes.ProbableRoute5;
				case "H6":
					return EntryStatusList.Codes.ProbableRoute6;
				case "":
					return EntryStatusList.Codes.NotSent;
				default:
					return routeCode;
			}
		}

		public static string GetDTIDescriptionFromCode(string code)
		{
			switch (code)
			{
				// Pre-lodgement/Entry Acceptance Advice (H2/E2, P2/X2)
				case "DTI-E2":
				case "DTI-E2-AMD":
				case "DTI-E2-XH":
				case "DTI-E2-R":
				case "DTI-E2-AMD-R":
				case "DTI-E2-XH-R":
					return "IMPORT ENTRY ACCEPTANCE ADVICE";
				case "DTI-X2":
				case "DTI-X2-AMD":
				case "DTI-X2-XH":
				case "DTI-X2-R":
					return "EXPORT ENTRY ACCEPTANCE ADVICE";
				case "DTI-H2":
				case "DTI-H2-AMD":
					return "IMPORT PRE-LODGEMENT ADVICE";
				case "DTI-P2":
				case "DTI-P2-AMD":
					return "EXPORT PRE-LODGEMENT ADVICE";

				// Pre-lodged/Accepted Entry Amendment Advice (H7/E7, P7/X7)
				case "DTI-E7":
					return "IMPORT ENTRY AMENDMENT ADVICE";
				case "DTI-X7":
					return "EXPORT ENTRY AMENDMENT ADVICE";
				case "DTI-H7":
					return "IMPORT PRE-LODGEMENT AMENDMENT ADVICE";
				case "DTI-P7":
					return "EXPORT PRE-LODGEMENT AMENDMENT ADVICE";

				// Supervising Office Report (E8, X8)
				case "DTI-E8":
				case "DTI-X8":
					return "SUPERVISING OFFICE REPORT";

				// Accounting Rejection Advice (E9, X9)
				case "DTI-E9":
					return "IMPORT ACCOUNTING REJECTION ADVICE";
				case "DTI-X9":
					return "EXPORT ACCOUNTING REJECTION ADVICE";

				// Entry Exception Report (E0, X0, P3)
				case "DTI-E0":
					return "IMPORT ENTRY EXCEPTION REPORT";
				case "DTI-X0":
					return "EXPORT ENTRY EXCEPTION REPORT";
				case "DTI-P3":
					return "EXPORT ARRIVAL REPROCESSING ERROR REPORT";

				// Customs Action or Movement Advice (E1/5, N3/4/6, X1/5/6, S3/4/5/6/8/9)
				case "DTI-E1":
					return "IMPORT EXAMINATION ADVICE";
				case "DTI-E5":
					return "IMPORT ENTRY RESELECTION ADVICE";
				case "DTI-N3":
					return "IMPORT CANCELLATION REFUSAL ADVICE";
				case "DTI-N4":
					return "IMPORT CANCELLATION APPROVAL ADVICE";
				case "DTI-N6":
					return "QUERIED IMPORT ENTRY ADVICE";
				case "DTI-X1":
					return "EXPORT EXAMINATION ADVICE";
				case "DTI-X5":
					return "EXPORT ENTRY RESELECTION ADVICE";
				case "DTI-X6":
					return "EXPORT ENTRY PROGRESS ADVICE";
				case "DTI-S3":
					return "EXPORT CANCELLATION REFUSAL ADVICE";
				case "DTI-S4":
					return "EXPORT CANCELLATION APPROVAL ADVICE";
				case "DTI-S5":
					return "EXPORT MOVEMENT ARRIVAL ADVICE";
				case "DTI-S6":
					return "QUERIED EXPORT ENTRY ADVICE";
				case "DTI-S8":
					return "EXPORT MOVEMENT DEPARTURE ADVICE";
				case "DTI-S9":
					return "EXPORT GOODS DISPOSAL ADVICE";
				case "DTI-S0":
					return "EXIT FOLLOW-UP ADVICE";  // ECS, w00t!

				// Deleted Stored Entry/Pre-lodgement Advice (H5, N1, P5, P9, S1)
				case "DTI-H5":
					return "SYSTEM DELETED IMPORT PRE-LODGEMENT ADVICE";
				case "DTI-N1":
					return "SYSTEM DELETED STORED IMPORT ENTRY ADVICE";
				case "DTI-P5":
					return "SYSTEM DELETED EXPORT PRE-LODGEMENT ADVICE";
				case "DTI-P9":
					return "EXPORT PRE-LODGEMENT DELETION WARNING";
				case "DTI-S1":
					return "SYSTEM DELETED STORED EXPORT ENTRY ADVICE";

				// Import Arrival Reprocessing Error Report (H3)
				case "DTI-H3":
					return "IMPORT ARRIVAL REPROCESSING ERROR REPORT";

				default:
					return string.Empty;
			}
		}
		#endregion
	}
}
