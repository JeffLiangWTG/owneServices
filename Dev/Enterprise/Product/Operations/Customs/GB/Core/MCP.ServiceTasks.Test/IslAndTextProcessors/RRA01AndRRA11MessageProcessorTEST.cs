using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.MailManager.MailFilters.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using MailManager;
using Moq.Protected;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.GB.MCP.ServiceTasks.RRA01AndRRA11.Testing
{
	[TestedType(typeof(McpStatusRetrieverServiceProvider))]
	public sealed class MCPMessageProcessorTest : ServiceTaskTestCase<McpStatusRetrieverServiceProvider>
	{
		public void TestRRAVersion2013WithEntryDetails_ExactEntryNumberMatch()
		{
			SetupAndRunRRATest("RRA11", "RRA11 FEY-FEYI [ISL] Release/Removal Authority (Nominated Agent) = 341560045",
				"=RRA11~TTY~00000000000000~TRLU9435845 ~         ~         ~GEORGE VENDOR NAME: VENDOR ADDRESS: DEPA~01022~00009399~0809021718~HK1038936 ~D02~MDB                 ~FullBillOfLadingUpTo35CharactersDan~AgentReference~071~001006L~130131~2359~XXX~YYY~A85}",
				expectedLogEvents("RRA11"),
				"Release/removal advice for  B000069 / 000000000 / TRLU9435845 / FullBillOfLadingUpTo35CharactersDan",
				true, new string[] { "This RRA matched the entry by CHIEF entry number", "071-001006L", "Dest=YYY, Code=A85 (Transit to Linked Approved Destination)" });
		}

		public void TestRRAVersion2013WithEntryDetails_ExactEntryNumberMatchForCDS()
		{
			const string expectedEntryNumber = "23GB5EMKWSQO6A4AR9";
			SetupAndRunRRATest("RRA11", "RRA11 FEY-FEYI [ISL] Release/Removal Authority (Nominated Agent) = 341560045",
				"=RRA11~NTI~77339014900000~DFDA8000450 ~         ~         ~14601411                                ~00061~00012401~0809021718~64654297  ~D02~MDB                 ~64654297                           ~3GB249915424000-B00320467          ~   ~23GB5EMKWSQO6A4AR9                 ~130131~2359~   ~YYY~03 }",
				expectedLogEvents("RRA11", expectedEntryNumber),
				"Release/removal advice for  B000069 / 773390149 / DFDA8000450 / 64654297",
				true, new string[] { "This RRA matched the entry by CDS entry number", expectedEntryNumber, "Dest=YYY, Code=03 ()" },
				entryNumber: expectedEntryNumber);
		}

		public void TestRRAVersion2013WithEntryDetails_NoEntryNumberMatchTryUcn()
		{
			SetupAndRunRRATest("RRA11", "RRA11 FEY-FEYI [ISL] Release/Removal Authority (Nominated Agent) = 341560045",
				"=RRA11~TTY~34156004500000~TRLU9435845 ~         ~         ~GEORGE VENDOR NAME: VENDOR ADDRESS: DEPA~01022~00009399~0809021718~HK1038936 ~D02~MDB                 ~FullBillOfLadingUpTo35CharactersDan~AgentReference~071~XXXXXX~130131~2359~XXX~YYY~ZZZ}",
				expectedLogEvents("RRA11"),
				"Release/removal advice for  B000069 / 341560045 / TRLU9435845 / FullBillOfLadingUpTo35CharactersDan",
				true, "This RRA matched the entry by MUCR");
		}

		public void TestRRA11ProcessingForIndividualPostHoldRelease_LastRemoved()
		{
			// Hold exists on amalgamation
			SetupAndRunRRATest("RRA11", "RRA11 FEY-FEYI [ISL] Release/Removal Authority (Nominated Agent) = 341560045",
				"=RRA11~MTE~84430045400000~TRLU7298888 ~         ~         ~149200692118                            ~00890~00008992~0809021718~1492006921~LHX~HMQ7099307          ~149200692118                       }",
				expectedLogEvents("RRA11"),
				"Release/removal advice for  B000069 / 844300454 / TRLU7298888 / 149200692118",
				true,
				"There are no remaining holds on this entry",
				delegate(CusEntryHeader ceh)
				{
					var oneUcn = ceh.MaritimeUcnsThatAreHeld.AddNew().Data;
					oneUcn.NW_UCN = "84430045400000";
					oneUcn.NW_HoldTypeComments = "Scanner hold exists";
				});
		}

		public void TestRRA11ProcessingForIndividualPostHoldRelease_StillOneRemaining()
		{
			// Hold exists on amalgamation
			SetupAndRunRRATest("RRA11", "RRA11 FEY-FEYI [ISL] Release/Removal Authority (Nominated Agent) = 341560045",
				"=RRA11~MTE~84430045400000~TRLU7298888 ~         ~         ~149200692118                            ~00890~00008992~0809021718~1492006921~LHX~HMQ7099307          ~149200692118                       }",
				expectedLogEvents("RRA11"),
				"Release/removal advice for  B000069 / 844300454 / TRLU7298888 / 149200692118",
				false,
				"Holds still exist for the following UCNs</h5> <p>1234567890124 (Port health hold exists)",
				delegate(CusEntryHeader ceh)
				{
					var oneUcn = ceh.MaritimeUcnsThatAreHeld.AddNew().Data;
					oneUcn.NW_UCN = "84430045400000";
					oneUcn.NW_HoldTypeComments = "Scanner hold exists";
					var remainingUcn = ceh.MaritimeUcnsThatAreHeld.AddNew().Data;
					remainingUcn.NW_UCN = "1234567890124";
					remainingUcn.NW_HoldTypeComments = "Port health hold exists";
				});
			AssertEquals("Entry status unaffected", "ABC", cusEntryHeader.CH_EntryStatus);
		}

		public void TestRRA11Processing()
		{
			SetupAndRunRRATest("RRA11", "RRA11 FEY-FEYI [ISL] Release/Removal Authority (Nominated Agent) = 341560045",
				"=RRA11~TTY~34156004500000~TRLU9435845 ~         ~         ~GEORGE VENDOR NAME: VENDOR ADDRESS: DEPA~01022~00009399~0809021718~HK1038936 ~D02~MDB                 }",
				expectedLogEvents("RRA11"),
				"Release/removal advice for  B000069 / 341560045 / TRLU9435845 / HK1038936",
				true, "This RRA matched the entry by MUCR");
		}

		public void TestRRA11ProcessingWithFourteenElementMessage()
		{
			SetupAndRunRRATest("RRA11", "RRA11 FEY-FEYI [ISL] Release/Removal Authority (Nominated Agent) = 341560045",
				"=RRA11~TTY~34156004500000~TRLU9435845 ~         ~         ~GEORGE VENDOR NAME: VENDOR ADDRESS: DEPA~01022~00009399~0809021718~HK1038936 ~D02~MDB                 ~FullBillOfLadingUpTo35CharactersDan}",
				expectedLogEvents("RRA11"),
				"Release/removal advice for  B000069 / 341560045 / TRLU9435845 / FullBillOfLadingUpTo35CharactersDan",
				true, "This RRA matched the entry by MUCR");
		}

		public void TestRRA01Processing()
		{
			SetupAndRunRRATest("RRA01", "RRA01 FRF-FRFP [ISL] Release/Removal Authority = 348130347",
				"=RRA01~TTY~34156004500000~CSLU1137752 ~         ~         ~                                        ~01216~00014751~0809021718~YTNF057108~D02~MDB                 ~YTNF057108                         }",
				expectedLogEvents("RRA01"),
				"Release/removal advice for  B000069 / 341560045 / CSLU1137752 / YTNF057108",
				true, "This RRA matched the entry by MUCR");
		}

		public void TestRRA06Processing_ExactEntryNumberMatch()
		{
			var messageList = new RRA06TestMessage[]
			{
				new RRA06TestMessage
				{
					RRAType = "RRA06",
					Subject = "RRA06 FEY-FEYI [ISL] Customs Clearance Notification UCN = 36818062000003",
					Message = "=RRA06~LNG~34156004500000~EITU1351575 ~         ~SLDSSI51460                             ~146800003271                       ~00341~00004945~N~NNNNNNNNNN~0809021718~SLDSSI51460          ~071~001006L~130131~2359~   ~   ~   ~INV~MDB                 }",
					ExpectedLogEvents = expectedLogEvents("RRA06"),
					ExpectedSubject = "Release/removal advice for  B000069 / 341560045 / EITU1351575 / 146800003271",
					ExpectEntryStatusToBeClear = true,
					ExpectedBodyTextRegardingHolds = new string[] { "This RRA matched the entry by CHIEF entry number" }
				}
			};

			SetupAndRunRRA06Test(messageList, true);
		}

		public void TestRRA06Processing_ExactEntryNumberMatchForCDS()
		{
			const string expectedEntryNumber = "23GB5EMKWSQO6A4AR9";
			var messageList = new RRA06TestMessage[]
			{
				new RRA06TestMessage
				{
					RRAType = "RRA06",
					Subject = "RRA06 FEY-FEYI [ISL] Customs Clearance Notification UCN = 36818062000003",
					Message = "=RRA06~LNG~34156004500000~EITU1351575 ~         ~SLDSSI51460                             ~146800003271                       ~00341~00004945~N~NNNNNNNNNN~0809021718~SLDSSI51460          ~   ~23GB5EMKWSQO6A4AR9~130131~2359~   ~   ~   ~INV~MDB                 }",
					ExpectedLogEvents = expectedLogEvents("RRA06", expectedEntryNumber),
					ExpectedSubject = "Release/removal advice for  B000069 / 341560045 / EITU1351575 / 146800003271",
					ExpectEntryStatusToBeClear = true,
					ExpectedBodyTextRegardingHolds = new string[] { "This RRA matched the entry by CDS entry number" }
				}
			};

			SetupAndRunRRA06Test(messageList, true, expectedEntryNumber);
		}

		public void TestRRA06Processing_NoEntryNumberMatchTryUcn()
		{
			var messageList = new RRA06TestMessage[]
			{
				new RRA06TestMessage
				{
					RRAType = "RRA06",
					Subject = "RRA06 FEY-FEYI [ISL] Customs Clearance Notification UCN = 36818062000003",
					Message = "=RRA06~LNG~34156004500000~EITU1351575 ~         ~SLDSSI51460                             ~146800003271                       ~00341~00004945~N~NNNNNNNNNN~0809021718~SLDSSI51460          ~071~XXXXXXX~130131~2359~   ~   ~   ~INV~MDB                 }",
					ExpectedLogEvents = expectedLogEvents("RRA06"),
					ExpectedSubject = "Release/removal advice for  B000069 / 341560045 / EITU1351575 / 146800003271",
					ExpectEntryStatusToBeClear = true,
					ExpectedBodyTextRegardingHolds = new string[] { "This RRA matched the entry by MUCR" }
				}
			};

			SetupAndRunRRA06Test(messageList, true);
		}

		public void TestRRA06Processing_WithPortHealthHold()
		{
			var messageList = new RRA06TestMessage[]
			{
				new RRA06TestMessage
				{
					RRAType = "RRA06",
					Subject = "RRA06 FEY-FEYI [ISL] Customs Clearance Notification UCN = 36818062000003",
					Message = "=RRA06~LNG~34156004500000~EITU1351575 ~         ~SLDSSI51460                             ~146800003271                       ~00341~00004945~Y~NNNNNNNNNN~0809021718~SLDSSI51460          ~071~001006L~130131~2359~   ~   ~   ~INV~MDB                 }",
					ExpectedLogEvents = expectedLogEvents("RRA06"),
					ExpectedSubject = "Release/removal advice for  B000069 / 341560045 / EITU1351575 / 146800003271",
					ExpectEntryStatusToBeClear = true,
					ExpectedBodyTextRegardingHolds = new string[] { "This RRA matched the entry by CHIEF entry number" },
					ExpectedHeldLog = new string[] { "Hold-PortHealthDetention" }
				}
			};

			SetupAndRunRRA06Test(messageList, false);
		}

		public void TestRRA06Processing_WithLocalCustomsHold()
		{
			var messageList = new RRA06TestMessage[]
			{
				new RRA06TestMessage
				{
					RRAType = "RRA06",
					Subject = "RRA06 FEY-FEYI [ISL] Customs Clearance Notification UCN = 36818062000003",
					Message = "=RRA06~LNG~34156004500000~EITU1351575 ~         ~SLDSSI51460                             ~146800003271                       ~00341~00004945~N~YNNNNNNNNN~0809021718~SLDSSI51460          ~071~001006L~130131~2359~   ~   ~   ~INV~MDB                 }",
					ExpectedLogEvents = expectedLogEvents("RRA06"),
					ExpectedSubject = "Release/removal advice for  B000069 / 341560045 / EITU1351575 / 146800003271",
					ExpectEntryStatusToBeClear = true,
					ExpectedBodyTextRegardingHolds = new string[] { "This RRA matched the entry by CHIEF entry number" },
					ExpectedHeldLog = new string[] { "Hold-LocalCustoms" }
				}
			};

			SetupAndRunRRA06Test(messageList, false);
		}

		public void TestRRA06Processing_WithTradingStandardsHold()
		{
			var messageList = new RRA06TestMessage[]
			{
				new RRA06TestMessage
				{
					RRAType = "RRA06",
					Subject = "RRA06 FEY-FEYI [ISL] Customs Clearance Notification UCN = 36818062000003",
					Message = "=RRA06~LNG~34156004500000~EITU1351575 ~         ~SLDSSI51460                             ~146800003271                       ~00341~00004945~N~NYNNNNNNNN~0809021718~SLDSSI51460          ~071~001006L~130131~2359~   ~   ~   ~INV~MDB                 }",
					ExpectedLogEvents = expectedLogEvents("RRA06"),
					ExpectedSubject = "Release/removal advice for  B000069 / 341560045 / EITU1351575 / 146800003271",
					ExpectEntryStatusToBeClear = true,
					ExpectedBodyTextRegardingHolds = new string[] { "This RRA matched the entry by CHIEF entry number" },
					ExpectedHeldLog = new string[] { "Hold-TradingStandards" }
				}
			};

			SetupAndRunRRA06Test(messageList, false);
		}

		public void TestRRA06Processing_WithDefraHold()
		{
			var messageList = new RRA06TestMessage[]
			{
				new RRA06TestMessage
				{
					RRAType = "RRA06",
					Subject = "RRA06 FEY-FEYI [ISL] Customs Clearance Notification UCN = 36818062000003",
					Message = "=RRA06~LNG~34156004500000~EITU1351575 ~         ~SLDSSI51460                             ~146800003271                       ~00341~00004945~N~NNYNNNNNNNN~0809021718~SLDSSI51460          ~071~001006L~130131~2359~   ~   ~   ~INV~MDB                 }",
					ExpectedLogEvents = expectedLogEvents("RRA06"),
					ExpectedSubject = "Release/removal advice for  B000069 / 341560045 / EITU1351575 / 146800003271",
					ExpectEntryStatusToBeClear = true,
					ExpectedBodyTextRegardingHolds = new string[] { "This RRA matched the entry by CHIEF entry number" },
					ExpectedHeldLog = new string[] { "Hold-Defra" }
				}
			};

			SetupAndRunRRA06Test(messageList, false);
		}

		public void TestRRA06Processing_WithScannerHold()
		{
			var messageList = new RRA06TestMessage[]
			{
				new RRA06TestMessage
				{
					RRAType = "RRA06",
					Subject = "RRA06 FEY-FEYI [ISL] Customs Clearance Notification UCN = 36818062000003",
					Message = "=RRA06~LNG~34156004500000~EITU1351575 ~         ~SLDSSI51460                             ~146800003271                       ~00341~00004945~N~NNNYNNNNNNN~0809021718~SLDSSI51460          ~071~001006L~130131~2359~   ~   ~   ~INV~MDB                 }",
					ExpectedLogEvents = expectedLogEvents("RRA06"),
					ExpectedSubject = "Release/removal advice for  B000069 / 341560045 / EITU1351575 / 146800003271",
					ExpectEntryStatusToBeClear = true,
					ExpectedBodyTextRegardingHolds = new string[] { "This RRA matched the entry by CHIEF entry number" },
					ExpectedHeldLog = new string[] { "Hold-Scanner" }
				}
			};

			SetupAndRunRRA06Test(messageList, false);
		}

		public void TestRRA06Processing_WithPoliceSpecialBranchHold()
		{
			var messageList = new RRA06TestMessage[]
			{
				new RRA06TestMessage
				{
					RRAType = "RRA06",
					Subject = "RRA06 FEY-FEYI [ISL] Customs Clearance Notification UCN = 36818062000003",
					Message = "=RRA06~LNG~34156004500000~EITU1351575 ~         ~SLDSSI51460                             ~146800003271                       ~00341~00004945~N~NNNNYNNNNNN~0809021718~SLDSSI51460          ~071~001006L~130131~2359~   ~   ~   ~INV~MDB                 }",
					ExpectedLogEvents = expectedLogEvents("RRA06"),
					ExpectedSubject = "Release/removal advice for  B000069 / 341560045 / EITU1351575 / 146800003271",
					ExpectEntryStatusToBeClear = true,
					ExpectedBodyTextRegardingHolds = new string[] { "This RRA matched the entry by CHIEF entry number" },
					ExpectedHeldLog = new string[] { "Hold-PoliceSpecialBranch" }
				}
			};

			SetupAndRunRRA06Test(messageList, false);
		}

		public void TestRRA06Processing_WithRuralHold()
		{
			var messageList = new RRA06TestMessage[]
			{
				new RRA06TestMessage
				{
					RRAType = "RRA06",
					Subject = "RRA06 FEY-FEYI [ISL] Customs Clearance Notification UCN = 36818062000003",
					Message = "=RRA06~LNG~34156004500000~EITU1351575 ~         ~SLDSSI51460                             ~146800003271                       ~00341~00004945~N~NNNNNYNNNNN~0809021718~SLDSSI51460          ~071~001006L~130131~2359~   ~   ~   ~INV~MDB                 }",
					ExpectedLogEvents = expectedLogEvents("RRA06"),
					ExpectedSubject = "Release/removal advice for  B000069 / 341560045 / EITU1351575 / 146800003271",
					ExpectEntryStatusToBeClear = true,
					ExpectedBodyTextRegardingHolds = new string[] { "This RRA matched the entry by CHIEF entry number" },
					ExpectedHeldLog = new string[] { "Hold-Rural" }
				}
			};

			SetupAndRunRRA06Test(messageList, false);
		}

		public void TestRRA06Processing_WithEnvironmentAgencyHold()
		{
			var messageList = new RRA06TestMessage[]
			{
				new RRA06TestMessage
				{
					RRAType = "RRA06",
					Subject = "RRA06 FEY-FEYI [ISL] Customs Clearance Notification UCN = 36818062000003",
					Message = "=RRA06~LNG~34156004500000~EITU1351575 ~         ~SLDSSI51460                             ~146800003271                       ~00341~00004945~N~NNNNNNYNNNN~0809021718~SLDSSI51460          ~071~001006L~130131~2359~   ~   ~   ~INV~MDB                 }",
					ExpectedLogEvents = expectedLogEvents("RRA06"),
					ExpectedSubject = "Release/removal advice for  B000069 / 341560045 / EITU1351575 / 146800003271",
					ExpectEntryStatusToBeClear = true,
					ExpectedBodyTextRegardingHolds = new string[] { "This RRA matched the entry by CHIEF entry number" },
					ExpectedHeldLog = new string[] { "Hold-EnvironmentAgency" }
				}
			};

			SetupAndRunRRA06Test(messageList, false);
		}

		public void TestRRA06Processing_WithForestryCommissionHold()
		{
			var messageList = new RRA06TestMessage[]
			{
				new RRA06TestMessage
				{
					RRAType = "RRA06",
					Subject = "RRA06 FEY-FEYI [ISL] Customs Clearance Notification UCN = 36818062000003",
					Message = "=RRA06~LNG~34156004500000~EITU1351575 ~         ~SLDSSI51460                             ~146800003271                       ~00341~00004945~N~NNNNNNNYNNN~0809021718~SLDSSI51460          ~071~001006L~130131~2359~   ~   ~   ~INV~MDB                 }",
					ExpectedLogEvents = expectedLogEvents("RRA06"),
					ExpectedSubject = "Release/removal advice for  B000069 / 341560045 / EITU1351575 / 146800003271",
					ExpectEntryStatusToBeClear = true,
					ExpectedBodyTextRegardingHolds = new string[] { "This RRA matched the entry by CHIEF entry number" },
					ExpectedHeldLog = new string[] { "Hold-ForestryCommission" }
				}
			};

			SetupAndRunRRA06Test(messageList, false);
		}

		public void TestRRA06Processing_WithMultipleHolds()
		{
			var messageList = new RRA06TestMessage[]
			{
				new RRA06TestMessage
				{
					RRAType = "RRA06",
					Subject = "RRA06 FEY-FEYI [ISL] Customs Clearance Notification UCN = 36818062000003",
					Message = "=RRA06~LNG~34156004500000~EITU1351575 ~         ~SLDSSI51460                             ~146800003271                       ~00341~00004945~Y~YNNNNNNNNN~0809021718~SLDSSI51460          ~071~001006L~130131~2359~   ~   ~   ~INV~MDB                 }",
					ExpectedLogEvents = expectedLogEvents("RRA06"),
					ExpectedSubject = "Release/removal advice for  B000069 / 341560045 / EITU1351575 / 146800003271",
					ExpectEntryStatusToBeClear = true,
					ExpectedBodyTextRegardingHolds = new string[] { "This RRA matched the entry by CHIEF entry number" },
					ExpectedHeldLog = new string[] { "Hold-Multiple" }
				}
			};

			SetupAndRunRRA06Test(messageList, false);
		}

		public void TestRRA06MultipleMessagesForClearedStatus()
		{
			var messageList = new RRA06TestMessage[]
			{
				new RRA06TestMessage
				{
					RRAType = "RRA06",
					Subject = "RRA06 FEY-FEYI [ISL] Customs Clearance Notification UCN = 36818062000003",
					Message = "=RRA06~LNG~34156004500000~EITU1351575 ~         ~SLDSSI51460                             ~146800003271                       ~00341~00004945~N~NNNNNNNNNN~0809021718~SLDSSI51460          ~071~001006L~130131~2359~   ~   ~   ~INV~MDB                 }",
					ExpectedLogEvents = expectedLogEvents("RRA06"),
					ExpectedSubject = "Release/removal advice for  B000069 / 341560045 / EITU1351575 / 146800003271",
					ExpectEntryStatusToBeClear = true,
					ExpectedBodyTextRegardingHolds = new string[] { "This RRA matched the entry by CHIEF entry number" }
				},
				new RRA06TestMessage
				{
					RRAType = "RRA06",
					Subject = "RRA06 FEY-FEYI [ISL] Customs Clearance Notification UCN = 36818062000003",
					Message = "=RRA06~LNG~34156004500000~WORKERCON001 ~         ~SLDSSI51460                             ~146800003271                       ~00341~00004945~N~NNNNNNNNNN~0809021718~SLDSSI51460          ~071~001006L~130131~2359~   ~   ~   ~INV~MDB                 }",
					ExpectedLogEvents = expectedLogEvents("RRA06"),
					ExpectedSubject = "Release/removal advice for  B000069 / 341560045 / WORKERCON001 / 146800003271",
					ExpectEntryStatusToBeClear = true,
					ExpectedBodyTextRegardingHolds = new string[] { "This RRA matched the entry by CHIEF entry number" }
				},
				new RRA06TestMessage
				{
					RRAType = "RRA06",
					Subject = "RRA06 FEY-FEYI [ISL] Customs Clearance Notification UCN = 36818062000003",
					Message = "=RRA06~LNG~34156004500000~WORKERCON002 ~         ~SLDSSI51460                             ~146800003271                       ~00341~00004945~N~NNNNNNNNNN~0809021718~SLDSSI51460          ~071~001006L~130131~2359~   ~   ~   ~INV~MDB                 }",
					ExpectedLogEvents = expectedLogEvents("RRA06"),
					ExpectedSubject = "Release/removal advice for  B000069 / 341560045 / WORKERCON002 / 146800003271",
					ExpectEntryStatusToBeClear = true,
					ExpectedBodyTextRegardingHolds = new string[] { "This RRA matched the entry by CHIEF entry number" }
				}
			};

			SetupAndRunRRA06Test(messageList, true);
		}

		public void TestRRA06MultipleMessagesForHeldStatus()
		{
			var messageList = new RRA06TestMessage[]
			{
				new RRA06TestMessage
				{
					RRAType = "RRA06",
					Subject = "RRA06 FEY-FEYI [ISL] Customs Clearance Notification UCN = 36818062000003",
					Message = "=RRA06~LNG~34156004500000~EITU1351575 ~         ~SLDSSI51460                             ~146800003271                       ~00341~00004945~Y~NNNNNNNNNN~0809021718~SLDSSI51460          ~071~001006L~130131~2359~   ~   ~   ~INV~MDB                 }",
					ExpectedLogEvents = expectedLogEvents("RRA06"),
					ExpectedSubject = "Release/removal advice for  B000069 / 341560045 / EITU1351575 / 146800003271",
					ExpectEntryStatusToBeClear = true,
					ExpectedBodyTextRegardingHolds = new string[] { "This RRA matched the entry by CHIEF entry number" },
					ExpectedHeldLog = new string[] { "Hold-PortHealthDetention" }
				},
				new RRA06TestMessage
				{
					RRAType = "RRA06",
					Subject = "RRA06 FEY-FEYI [ISL] Customs Clearance Notification UCN = 36818062000003",
					Message = "=RRA06~LNG~34156004500000~WORKERCON001 ~         ~SLDSSI51460                             ~146800003271                       ~00341~00004945~N~YNNNNNNNNN~0809021718~SLDSSI51460          ~071~001006L~130131~2359~   ~   ~   ~INV~MDB                 }",
					ExpectedLogEvents = expectedLogEvents("RRA06"),
					ExpectedSubject = "Release/removal advice for  B000069 / 341560045 / WORKERCON001 / 146800003271",
					ExpectEntryStatusToBeClear = true,
					ExpectedBodyTextRegardingHolds = new string[] { "This RRA matched the entry by CHIEF entry number" },
					ExpectedHeldLog = new string[] { "Hold-LocalCustoms" }
				},
				new RRA06TestMessage
				{
					RRAType = "RRA06",
					Subject = "RRA06 FEY-FEYI [ISL] Customs Clearance Notification UCN = 36818062000003",
					Message = "=RRA06~LNG~34156004500000~WORKERCON002 ~         ~SLDSSI51460                             ~146800003271                       ~00341~00004945~N~NYNNNNNNNN~0809021718~SLDSSI51460          ~071~001006L~130131~2359~   ~   ~   ~INV~MDB                 }",
					ExpectedLogEvents = expectedLogEvents("RRA06"),
					ExpectedSubject = "Release/removal advice for  B000069 / 341560045 / WORKERCON002 / 146800003271",
					ExpectEntryStatusToBeClear = true,
					ExpectedBodyTextRegardingHolds = new string[] { "This RRA matched the entry by CHIEF entry number" },
					ExpectedHeldLog = new string[] { "Hold-TradingStandards" }
				}
			};

			SetupAndRunRRA06Test(messageList, false);
		}

		string[] expectedLogEvents(string rraType, string entryNumber = "071-001006L")
		{
			return new string[] { "Processing RRA message: =" + rraType + "~", "Updated entry from " + rraType, entryNumber, "Finished processing " + rraType + " and sent email to: foo@bar.com" };
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			branchEnvironment = Enterprise.Environment.DisposableEnvironment.ForBranch(Enterprise.Customs.GB.Business.Testing.DeclarationTestHelper.CreateGbCompanyAndBranchAndSave(Factory).PK.ToGuid());
		}

		IDisposable branchEnvironment;

		protected override void TearDownCore()
		{
			base.TearDownCore();
			branchEnvironment.Dispose();
		}

		void SetupAndRunRRATest(string rraType, string rraSourceSubject, string rraSourceMessageText, string[] expectedLogEvents, string expectedSubject, bool expectEntryStatusToBeClear, string expectedBodyTextRegardingHolds,
			Action<CusEntryHeader> furtherInitialisationOfEntry = null)
		{
			SetupAndRunRRATest(rraType, rraSourceSubject, rraSourceMessageText, expectedLogEvents, expectedSubject, expectEntryStatusToBeClear, new string[] { expectedBodyTextRegardingHolds }, furtherInitialisationOfEntry);
		}

		void SetupAndRunRRATest(string rraType, string rraSourceSubject, string rraSourceMessageText, string[] expectedLogEvents, string expectedSubject, bool expectEntryStatusToBeClear, string[] expectedBodyTextRegardingHolds,
			Action<CusEntryHeader> furtherInitialisationOfEntry = null, string entryNumber = "071-001006L")
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "DJC";
			staff.GS_IsSystemAccount = false;
			staff.GS_EmailAddress = "foo@bar.com";
			staff.Groups.Add(Factory.Load<GlbGroup>(GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroup));
			Factory.Save();

			var staffSystemAccount = Factory.New<GlbStaff>();
			staffSystemAccount.GS_Code = "AAA";
			staffSystemAccount.GS_IsSystemAccount = true;
			staffSystemAccount.GS_LoginName = "AAA";
			staffSystemAccount.GS_EmailAddress = "AAA@bar.com";
			staffSystemAccount.Groups.Add(Factory.Load<GlbGroup>(GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroup));
			Factory.Save();

			var mailItem = Factory.New<MailItem>();
			mailItem.MI_From = "Destin8PRD@destin8.co.uk";
			mailItem.MI_Subject = rraSourceSubject;
			mailItem.MI_Status = MailStatus.Queued;
			mailItem.MI_Direction = MailDirection.Receive;
			mailItem.MI_SendDateTime = ZDateTime.Now.AddMinutes(-5);
			mailItem.MI_ReceivedDateTime = ZDateTime.Now;
			var attachment = mailItem.MailAttachments.AddNew();
			attachment.MA_Data = ZBlob.FromAscii(rraSourceMessageText);
			attachment.MA_FileName = "56607033_FEY.isl";
			MailFilterLocatorTestHelper.SetApplication(mailItem, MailFilterCodes.GbMcpRra11AndRra06AndRra01);

			var aaaBranch = Factory.New<GlbBranch>();
			aaaBranch.GB_Code = "AAA";
			aaaBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var bbbBranch = Factory.New<GlbBranch>();
			bbbBranch.GB_Code = "BBB";
			bbbBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			JobDeclaration declaration;
			EDIMessage outboundMessage;

			using (DisposableEnvironment.ForBranch(bbbBranch.PK.ToGuid()))
			{
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = "IMP";
				declaration.JE_MasterUCR = "341560045";
				declaration.JE_DeclarationReference = "B000069";
				cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
				var mockMessage = Factory.NewMoq<EDIMessageDummyForTest_123>();
				mockMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("123");
				outboundMessage = mockMessage.Object;
				cusEntryHeader.Messages.Add(outboundMessage);
				cusEntryHeader.EntryNumber = entryNumber;
				cusEntryHeader.CusEntryNumber.CE_IssueDate = new ZDateTime(2013, 1, 31, 23, 59, 00);
				cusEntryHeader.CH_EntryStatus = "ABC";
				outboundMessage.EM_SystemCreateUser = "DJC";
				outboundMessage.EM_MessageText = " I hate this silly insistence on message number <<MSGNO PLACEHOLDER>>";
				outboundMessage.EM_ReceiveTransmit = "TRX";
				Factory.Save();

				Thread.Sleep(100);
				var mockMessage2 = Factory.NewMoq<EDIMessageDummyForTest_456>();
				mockMessage2.Protected().Setup<string>("GetMessageReferenceNumber").Returns("456");
				EDIMessage outboundMessageFromSystemAccount = mockMessage2.Object;
				cusEntryHeader.Messages.Add(outboundMessageFromSystemAccount);
				outboundMessageFromSystemAccount.EM_SystemCreateUser = "AAA";
				outboundMessageFromSystemAccount.EM_MessageText = "I too hate this silly insistence on message number <<MSGNO PLACEHOLDER>>";
				outboundMessageFromSystemAccount.EM_ReceiveTransmit = "TRX";

				declaration.JE_UCR = "348130347";
				cusEntryHeader.CH_BGMReference = "BGMREF";
				cusEntryHeader.CH_EntrySubmittedDate = ZDateTime.Now.AddMinutes(-60);
				if (furtherInitialisationOfEntry != null)
				{
					furtherInitialisationOfEntry(cusEntryHeader);
				}
				Factory.Save();
				mockMessage.VerifyAll();
			}
			AssertEquals("Pre-req: message should have been sent by DJC, not by a built-in account", "DJC", outboundMessage.UserWhoQueuedThisRecord.GS_Code);

			var serviceTask = new McpStatusRetrieverServiceProvider();
			var log = InitialiseAndRunTaskSchedule(serviceTask);
			var allLogEvents = log.ToString();
			foreach (var s in expectedLogEvents)
			{
				AssertContains(s, allLogEvents);
			}

			mailItem.Reload();
			AssertEquals("Received Email Status", MailStatus.Processed, mailItem.MI_Status);

			var emails = Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals("The number of emails created by the Mcp RRA processor in the test should be 2.  That is, we should send one email notification to the user ('s group) for the RRA that we received, and one for ERouter.", 1, emails.Count);
			AssertEquals("Subject", expectedSubject, emails[0].Subject);
			AssertContains("B000069", emails[0].Body);
			foreach (var expected in expectedBodyTextRegardingHolds)
			{
				AssertContains(expected, emails[0].Body);
			}
			AssertContains("Release/removal", emails[0].Body);
			AssertEquals("foo@bar.com", emails[0].Recipients[0]);

			cusEntryHeader.Reload();
			((ILogsInternals)cusEntryHeader.Logs).ReloadFromDB();
			if (expectEntryStatusToBeClear)
			{
				AssertEquals("Checking the CH_EntryStatus flag to see if the status of the entry is now 'cleared'.", "CLR", cusEntryHeader.CH_EntryStatus);
				AssertEquals(new ZDateTime(2008, 9, 2, 17, 18, 0), cusEntryHeader.CH_EntryReleaseDate);
				AssertEquals(new ZDateTime(2008, 9, 2, 17, 18, 0), cusEntryHeader.ClearanceDate);
				declaration.Reload();
				cusEntryHeader.Reload();
				AssertEquals("CLR", declaration.JE_EntryStatus);
				AssertEquals(EntryStatusList.Codes.Clear, cusEntryHeader.CH_EntryStatus);
				ZQuery query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code);
				query.AddToFilter(StmALogSchema.SL_Parent, declaration.PK);
				AssertEquals("Cleared logs on declaration", 1, Factory.Load<StmALog>(query).Length);
			}

			ZQuery queryEntry = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code);
			queryEntry.AddToFilter(StmALogSchema.SL_Parent, cusEntryHeader.PK);
			AssertEquals("Cleared logs on entry, always", 1, Factory.Load<StmALog>(queryEntry).Length);

			// The service task takes a peek inside the Messages collection during processing, which caches the collection.  We need to load it to refresh the cached collection otherwise the following assertion fails.
			cusEntryHeader.Messages.Load();
			EDIMessage inboundEdiMessageThatWeMade = cusEntryHeader.Messages.LastIncomingMessage;
			AssertEquals(EDIMessage.Status.Received, inboundEdiMessageThatWeMade.EM_Status);
			AssertEquals(EDIInterchange.Status.Received, inboundEdiMessageThatWeMade.Interchange.EI_Status);

			AssertEquals(bbbBranch.PK, inboundEdiMessageThatWeMade.EM_GB);

			//Check RRA goes to eDocs:
			var docManagerInfo = ((IDocManagerSupport)declaration).DocManagerInfo;
			AssertEquals("Attached Files in eDocs against Declaration", 1, docManagerInfo.Files.Count);
			var eDoc = docManagerInfo.Files[0];
			AssertEquals("eDoc.Description", "Release/removal advice (" + rraType + ") from Destin8", eDoc.Description);
			AssertEquals("eDoc.DocType", RefDocTypes.ReleaseRemovalAdvice, eDoc.DocType);
			AssertEquals("eDoc.FileName", $"Release-removal advice for {entryNumber}.{rraType}.htm", eDoc.FileName);
			AssertContains("eDoc.ImageData", @"<h3>Release/removal advice (" + rraType + ") from Destin8</h3>", eDoc.ImageData.ToAscii());
			AssertContains("eDoc.ImageData", @"<td><b>B000069</b></td>", eDoc.ImageData.ToAscii());

			CombineAssertions(() =>
			{
				Assert("Log for DDA added", cusEntryHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.DocumentAllocated.Code).Any());
				Assert("Event for DDA added", cusEntryHeader.Logs.Find(x => x.Event.SE_Code == Events.DocumentAllocated.Code).Any());
			});
		}

		void SetupAndRunRRA06Test(RRA06TestMessage[] messages, bool expectToBeClear, string entryNumber = "071-001006L")
		{
			CreateStaff();

			var mailItems = CreateMailItems(messages);
			var consol = CreateForwardingConsol();
			var primaryShipment = CreatePrimaryShipment(consol);

			var relatedShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			relatedShipment.JS_JS_ColoadMasterShipment = primaryShipment.PK;

			consol.Shipments.Add(primaryShipment);
			consol.Shipments.Add(relatedShipment);

			var gbBranch = CreateBranch("CPT", consol.JK_JX_JB_RL_NKPortOfDischarge, Core.Constants.CountryCodes.UnitedKingdom);
			var auBranch = CreateBranch("AAA", "AUBNE", Core.Constants.CountryCodes.Australia);
			var declaration = CreateDeclarationForShipment(gbBranch.PK, primaryShipment.PK, "IMP", "341560045");
			declaration.JE_DeclarationReference = "B000069";
			declaration.JE_UCR = "348130347";

			var secondaryDeclaration1 = CreateDeclarationForShipment(gbBranch.PK, relatedShipment.PK, "IMP", string.Empty);
			var secondaryContainer1 = CreateContainerForDeclaration(secondaryDeclaration1, "WORKERCON001");
			var secondaryContainer2 = CreateContainerForDeclaration(secondaryDeclaration1, "WORKERCON002");
			Factory.Save();

			var secondaryDeclaration2 = CreateDeclarationForShipment(auBranch.PK, relatedShipment.PK, "IMP", string.Empty);

			var container1 = CreateContainerForDeclaration(declaration, "EITU1351575");
			var container2 = CreateContainerForDeclaration(declaration, "Container02");

			var entryHeader = CreateEntryHeader(declaration, entryNumber);
			var outboundMessage = CreateOutboundMessage();
			cusEntryHeader.Messages.Add(outboundMessage);

			Factory.Save();
			AssertEquals("Pre-req: message should have been sent by DJC, not by a built-in account", "DJC", outboundMessage.UserWhoQueuedThisRecord.GS_Code);

			var serviceTask = new McpStatusRetrieverServiceProvider();
			var log = InitialiseAndRunTaskSchedule(serviceTask);
			var allLogEvents = log.ToString();
			foreach (var s in messages.SelectMany(m => m.ExpectedLogEvents).Distinct())
			{
				AssertContains(s, allLogEvents);
			}

			var emails = Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals("The number of emails created by the Mcp RRA processor in the test should be 3.", messages.Length, emails.Count);

			foreach (var mailItem in mailItems)
			{
				mailItem.Reload();

				var email = emails.FirstOrDefault(x => x.Subject == mailItem.Message.ExpectedSubject);
				AssertNotNull("Could not find email from subject", email);

				AssertEquals("Received Email Status", MailStatus.Processed, mailItem.MI_Status);

				AssertContains("B000069", email.Body);
				foreach (var expected in mailItem.Message.ExpectedBodyTextRegardingHolds)
				{
					AssertContains(expected, email.Body);
				}
				AssertContains("Release/removal", email.Body);
				AssertEquals("foo@bar.com", email.Recipients[0]);
			}

			cusEntryHeader.Reload();
			((ILogsInternals)cusEntryHeader.Logs).ReloadFromDB();
			if (expectToBeClear)
			{
				AssertEquals("Checking the CH_EntryStatus flag to see if the status of the entry is now 'cleared'.", "CLR", cusEntryHeader.CH_EntryStatus);
				AssertEquals(new ZDateTime(2008, 9, 2, 17, 18, 0), cusEntryHeader.CH_EntryReleaseDate);
				AssertEquals(new ZDateTime(2008, 9, 2, 17, 18, 0), cusEntryHeader.ClearanceDate);
				declaration.Reload();
				cusEntryHeader.Reload();
				AssertEquals(Events.CustomsCleared.Code, declaration.JE_EntryStatus);
				AssertEquals(EntryStatusList.Codes.Clear, cusEntryHeader.CH_EntryStatus);
				ZQuery query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code);
				query.AddToFilter(StmALogSchema.SL_Parent, declaration.PK);
				query.AddToFilter(StmALogSchema.SL_Reference, Events.CustomsCleared.Code);
				AssertEquals("Cleared logs on declaration", 1, Factory.Load<StmALog>(query).Length);
				ZQuery queryEntry = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code);
				queryEntry.AddToFilter(StmALogSchema.SL_Parent, cusEntryHeader.PK);
				AssertEquals("Cleared logs on entry, always", 1, Factory.Load<StmALog>(queryEntry).Length);

				secondaryDeclaration1.Reload();
				AssertEquals(Events.CustomsCleared.Code, secondaryDeclaration1.JE_EntryStatus);
				query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code);
				query.AddToFilter(StmALogSchema.SL_Parent, secondaryDeclaration1.PK);
				query.AddToFilter(StmALogSchema.SL_Reference, Events.CustomsCleared.Code);
				AssertEquals("Cleared logs on GB declaration from related shipment", 1, Factory.Load<StmALog>(query).Length);

				RRA06TestMessage message = null;
				if ((message = messages.FirstOrDefault(x => x.Message.Contains(container1.CO_ContainerNumber))) != null)
				{
					container1.Reload();
					AssertEquals("Container should be released", EU.Business.Declaration.ContainerStatusCodesList.Codes.Released, container1.CO_MessageStatus);
				}
				if ((message = messages.FirstOrDefault(x => x.Message.Contains(secondaryContainer1.CO_ContainerNumber))) != null)
				{
					secondaryContainer1.Reload();
					AssertEquals("Container on secondary shipment should be released", EU.Business.Declaration.ContainerStatusCodesList.Codes.Released, secondaryContainer1.CO_MessageStatus);
				}
				if ((message = messages.FirstOrDefault(x => x.Message.Contains(secondaryContainer2.CO_ContainerNumber))) != null)
				{
					secondaryContainer2.Reload();
					AssertEquals("Container on secondary shipment should be released", EU.Business.Declaration.ContainerStatusCodesList.Codes.Released, secondaryContainer2.CO_MessageStatus);
				}
			}
			else
			{
				AssertEquals("Checking the CH_EntryStatus flag to see if the status of the entry is cleared.", "CLR", cusEntryHeader.CH_EntryStatus);
				declaration.Reload();
				cusEntryHeader.Reload();
				AssertEquals(EntryStatusList.Codes.Clear, declaration.JE_EntryStatus);
				AssertEquals(EntryStatusList.Codes.Clear, cusEntryHeader.CH_EntryStatus);
				ZQuery queryEntry = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code);
				queryEntry.AddToFilter(StmALogSchema.SL_Parent, cusEntryHeader.PK);
				AssertEquals("Cleared logs on entry, always", 1, Factory.Load<StmALog>(queryEntry).Length);

				secondaryDeclaration1.Reload();
				AssertEquals(EntryStatusList.Codes.Clear, secondaryDeclaration1.JE_EntryStatus);

				RRA06TestMessage message = null;
				if ((message = messages.FirstOrDefault(x => x.Message.Contains(container1.CO_ContainerNumber))) != null)
				{
					AssertContainerHolds(container1, message);
				}
				if ((message = messages.FirstOrDefault(x => x.Message.Contains(secondaryContainer1.CO_ContainerNumber))) != null)
				{
					AssertContainerHolds(secondaryContainer1, message);
				}
				if ((message = messages.FirstOrDefault(x => x.Message.Contains(secondaryContainer2.CO_ContainerNumber))) != null)
				{
					AssertContainerHolds(secondaryContainer2, message);
				}
			}

			// The service task takes a peek inside the Messages collection during processing, which caches the collection.  We need to load it to refresh the cached collection otherwise the following assertion fails.
			cusEntryHeader.Messages.Load();
			EDIMessage inboundEdiMessageThatWeMade = cusEntryHeader.Messages.LastIncomingMessage;
			AssertEquals(EDIMessage.Status.Received, inboundEdiMessageThatWeMade.EM_Status);
			AssertEquals(EDIInterchange.Status.Received, inboundEdiMessageThatWeMade.Interchange.EI_Status);

			//Check RRA goes to eDocs:
			var docManagerInfo = ((IDocManagerSupport)declaration).DocManagerInfo;
			AssertEquals("Attached Files in eDocs against Declaration", messages.Length, docManagerInfo.Files.Count);
			var eDoc = docManagerInfo.Files[0];
			AssertEquals("eDoc.Description", "Release/removal advice (" + messages[0].RRAType + ") from Destin8", eDoc.Description);
			AssertEquals("eDoc.DocType", RefDocTypes.ReleaseRemovalAdvice, eDoc.DocType);
			AssertEquals("eDoc.FileName", $"Release-removal advice for {entryNumber}.{messages[0].RRAType}.htm", eDoc.FileName);
			AssertContains("eDoc.ImageData", @"<h3>Release/removal advice (" + messages[0].RRAType + ") from Destin8</h3>", eDoc.ImageData.ToAscii());
			AssertContains("eDoc.ImageData", @"<td><b>B000069</b></td>", eDoc.ImageData.ToAscii());

			CombineAssertions(() =>
			{
				Assert("Log for DDA added", cusEntryHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.DocumentAllocated.Code).Any());
				Assert("Event for DDA added", cusEntryHeader.Logs.Find(x => x.Event.SE_Code == Events.DocumentAllocated.Code).Any());
			});
		}

		public void TestRRA11ProcessingForUnknownEntry()
		{
			string expectedErrorMessage = @"A response was received from Destin8 for an unknown entry. The RRA11's details are: <pre>
RRA11:
UCN = 341560045
Raw UCN = 34156004500000
Cont = TRLU9435845
BoL = HK1038936
M&N = GEORGE VENDOR NAME: VENDOR ADDRESS: DEPA

</pre>Since no related entry could be found, no job has been updated.";

			// We need to assign the user under which this appears to run, "Developer", an email address, otherwise we cannot send any emails
			var staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staff.GS_EmailAddress = "foo@bar.com";
			staff.Groups.Add(Factory.Load<GlbGroup>(GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroup));
			var postmasters = Factory.Load<GlbGroup>(Groups.PostMastersGroupPK);
			var postMaster = postmasters.Staff.AddNew();
			postMaster.GS_EmailAddress = "post@master.com";
			Factory.Save();

			var mailItem = Factory.New<MailItem>();
			mailItem.MI_From = "Destin8PRD@destin8.co.uk";
			mailItem.MI_Subject = "RRA11 FEY-FEYI [ISL] Release/Removal Authority (Nominated Agent) = 348130347";
			mailItem.MI_Status = MailStatus.Queued;
			mailItem.MI_Direction = MailDirection.Receive;
			mailItem.MI_SendDateTime = ZDateTime.Now.AddMinutes(-5);
			mailItem.MI_ReceivedDateTime = ZDateTime.Now;
			var attachment = mailItem.MailAttachments.AddNew();
			attachment.MA_Data = ZBlob.FromAscii("=RRA11~TTY~34156004500000~TRLU9435845 ~         ~         ~GEORGE VENDOR NAME: VENDOR ADDRESS: DEPA~01022~00009399~0809021718~HK1038936 ~D02~MDB                 }");
			attachment.MA_FileName = "56607033_FEY.isl";
			MailFilterLocatorTestHelper.SetApplication(mailItem, MailFilterCodes.GbMcpRra11AndRra06AndRra01);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_MasterUCR = "blah blah";
			declaration.JE_DeclarationReference = "B000069";
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_UCR = "348130347";
			cusEntryHeader.CH_BGMReference = "BGMREF";
			cusEntryHeader.CH_EntrySubmittedDate = ZDateTime.Now.AddMinutes(-60);
			cusEntryHeader.CH_MessageType = ApplicationCodeList.Codes.GbMcpRra01AndRra11;
			Factory.Save();

			var serviceTask = new McpStatusRetrieverServiceProvider();
			InitialiseAndRunTaskSchedule(serviceTask);

			mailItem.Reload();
			AssertEquals("Received Email Status", MailStatus.Processed, mailItem.MI_Status);
			var emails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals("The number of emails created by the Mcp RRA11 processor in the test should be 1: 1 to webmaster", 1, emails.Count);
			AssertEquals("Subject", "Release/removal advice for (Unknown job)", emails[0].Subject);
			AssertContains("Recipient", "post@master.com", emails[0].Recipients.RecipientsAsDelimitedString());
			AssertContains(expectedErrorMessage, emails[0].Body);
			var mailDbItemsInDb = Factory.GetDatabaseCount(typeof(AutoMailDBItems), new ZQuery(MailDBItemsSchema.MI_Direction, "TRX"));
			AssertEquals("Mail Item should have been saved to database", 1, mailDbItemsInDb);
			Assert(string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
			ErrorReporter.Clear();
		}

		public void TestRRA11ProcessingForReceiptOfTxtInsteadOfIslFiles()
		{
			var staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staff.GS_EmailAddress = "foo@bar.com";
			staff.Groups.Add(Factory.Load<GlbGroup>(GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroup));

			MailItem receivedEmail = Factory.New<MailItem>();
			using (Stream inStream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.GB.MCP.ServiceTasks.Test.IslAndTextProcessors.TestFiles.SampleBadRraWithTxtNotIslAttachment.txt"))
			{
				receivedEmail.RawMIMEString = new StreamReader(inStream).ReadToEnd();
			}
			receivedEmail.MI_Direction = DirectionList.Codes.Receive;
			MailFilterLocatorTestHelper.SetApplication(receivedEmail, MailFilterCodes.GbMcpRra11AndRra06AndRra01);
			Factory.Save();

			var serviceTask = new McpStatusRetrieverServiceProvider();
			serviceTask.RunTask();

			AssertContains("An email was received from MCP without a relevant attachment", OutgoingCustomsMailCreator.Instance.EmailsCreated[0].Body);
			receivedEmail.Reload();
			AssertEquals("FAL", receivedEmail.MI_Status);
		}

		CusEntryHeader cusEntryHeader;

		#region private methods

		ZQuery AssertContainerHolds(EU.Business.Declaration.CusContainer container, RRA06TestMessage message)
		{
			ZQuery queryEntry = new ZQuery(StmALogSchema.SL_SE_NKEvent, "CES");
			foreach (var heldLog in message.ExpectedHeldLog)
			{
				queryEntry.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, heldLog);
			}
			queryEntry.AddToFilter(StmALogSchema.SL_Parent, container.JobContainer.PK);
			AssertEquals("Container is held", 1, Factory.Load<StmALog>(queryEntry).Length);

			container.Reload();
			AssertEquals("Container should be held", EU.Business.Declaration.ContainerStatusCodesList.Codes.HoldIsAdded, container.CO_MessageStatus);
			return queryEntry;
		}

		EDIMessage CreateOutboundMessage()
		{
			var mockMessage = Factory.NewMoq<EDIMessageDummyForTest_123>();
			mockMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("123");
			EDIMessage outboundMessage = mockMessage.Object;

			outboundMessage.EM_SystemCreateUser = "DJC";
			outboundMessage.EM_MessageText = " I hate this silly insistence on message number <<MSGNO PLACEHOLDER>>";
			outboundMessage.EM_ReceiveTransmit = "TRX";
			return outboundMessage;
		}

		CusEntryHeader CreateEntryHeader(JobDeclaration declaration, ZString entryNumber)
		{
			cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.EntryNumber = entryNumber;
			cusEntryHeader.CusEntryNumber.CE_IssueDate = new ZDateTime(2013, 1, 31, 23, 59, 00);
			cusEntryHeader.CH_EntryStatus = "ABC";
			cusEntryHeader.CH_BGMReference = "BGMREF";
			cusEntryHeader.CH_EntrySubmittedDate = ZDateTime.Now.AddMinutes(-60);
			return cusEntryHeader;
		}

		JobDeclaration CreateDeclarationForShipment(ZGuid branchPK, ZGuid shipmentPK, ZString messageType, ZString masterUCR)
		{
			JobDeclaration declaration = null;
			using (DisposableEnvironment.ForBranch(branchPK.ToGuid()))
			{
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = messageType;
				declaration.JE_MasterUCR = !masterUCR.IsEmpty ? masterUCR : declaration.JE_MasterUCR;
				declaration.JE_JS = shipmentPK;
				declaration.JE_EntryStatus = "ABC";
			}
			Factory.Save();

			return declaration;
		}

		ForwardingConsol CreateForwardingConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "GBLHR";
			consol.JK_RL_NKDischargePort = "ZACPT";
			consol.JK_TransportMode = "AIR";
			return consol;
		}

		List<MailItemForTest> CreateMailItems(RRA06TestMessage[] messages)
		{
			List<MailItemForTest> mailItems = new List<MailItemForTest>();
			foreach (var message in messages)
			{
				mailItems.Add(CreateMailItem(message));
			}

			return mailItems;
		}

		void CreateStaff()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "DJC";
			staff.GS_EmailAddress = "foo@bar.com";
			staff.Groups.Add(Factory.Load<GlbGroup>(GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroup));
			Factory.Save();
		}

		ForwardingShipment CreatePrimaryShipment(ForwardingConsol consol)
		{
			ForwardingShipment primaryShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			primaryShipment.JS_UniqueConsignRef = "B000069";
			primaryShipment.JS_ShipmentType = Enterprise.Core.Constants.ShipmentTypes.CoLoadMaster;
			primaryShipment.JS_RL_NKOrigin = consol.JK_RL_NKLoadPort;
			primaryShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			return primaryShipment;
		}

		MailItemForTest CreateMailItem(RRA06TestMessage message)
		{
			MailItemForTest mailItem = Factory.New<MailItemForTest>();
			mailItem.Message = message;
			mailItem.MI_From = "Destin8PRD@destin8.co.uk";
			mailItem.MI_Subject = message.Subject;
			mailItem.MI_Status = MailStatus.Queued;
			mailItem.MI_Direction = MailDirection.Receive;
			mailItem.MI_SendDateTime = ZDateTime.Now.AddMinutes(-5);
			mailItem.MI_ReceivedDateTime = ZDateTime.Now;
			var attachment = mailItem.MailAttachments.AddNew();
			attachment.MA_Data = ZBlob.FromAscii(message.Message);
			attachment.MA_FileName = "56607033_FEY.isl";

			MailFilterLocatorTestHelper.SetApplication(mailItem, MailFilterCodes.GbMcpRra11AndRra06AndRra01);

			return mailItem;
		}

		GlbBranch CreateBranch(ZString code, ZString homePort, ZString country)
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = country;
			company.GC_Code = code;
			var branch = company.Branches.AddNew();
			branch.GB_Code = code;
			branch.GB_RL_NKHomePort = homePort;
			Factory.Save();

			return branch;
		}

		EU.Business.Declaration.CusContainer CreateContainerForDeclaration(JobDeclaration declaration, ZString containerNumber)
		{
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = containerNumber;
			return container;
		}

		#endregion
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
				new TaskNudgeInformationForTest(
					MailDBItemsSchema.Constants.TableName,
					"Destin8 RRA11/06/01 mail inbound",
					MailDBItemsSchema.Constants.MI_Application + "=" + MailFilterCodes.GbMcpRra11AndRra06AndRra01,
					MailDBItemsSchema.Constants.MI_Status + "=" + StatusCodeList.Codes.Queued,
					MailDBItemsSchema.Constants.MI_Direction + "=" + DirectionList.Codes.Receive),
				};
			}
		}
	}

	public class EDIMessageDummyForTest_123 : EDIMessage
	{
		public EDIMessageDummyForTest_123(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		protected override string GetMessageReferenceNumber()
		{
			return "123";
		}
	}

	public class EDIMessageDummyForTest_456 : EDIMessage
	{
		public EDIMessageDummyForTest_456(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		protected override string GetMessageReferenceNumber()
		{
			return "456";
		}
	}

	class MailItemForTest : MailItem
	{
		public MailItemForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public RRA06TestMessage Message { get; set; }
	}

	class RRA06TestMessage
	{
		public string RRAType { get; set; }

		public string Subject { get; set; }

		public string Message { get; set; }

		public string[] ExpectedLogEvents { get; set; }

		public string ExpectedSubject { get; set; }

		public bool ExpectEntryStatusToBeClear { get; set; }

		public string[] ExpectedBodyTextRegardingHolds { get; set; }

		public Action<CusEntryHeader> FurtherInitialisationOfEntry { get; set; }

		public string[] ExpectedHeldLog { get; set; } = Array.Empty<string>();
	}
}
