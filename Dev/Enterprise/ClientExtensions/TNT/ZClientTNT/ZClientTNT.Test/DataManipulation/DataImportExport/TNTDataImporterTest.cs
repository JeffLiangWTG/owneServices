using System.Collections;
using System.IO;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Billing.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Client.TNT.Testing
{
	public class TNTDataImporterTest : TestCaseWithFactory, INotifications
	{
		public void TestNoDeclarationsAreCreatedForExemptionShipments()
		{
			string exit1TempTestFilePath = TestUtils.CopyResourceToFile(TNTTestUtils.Exit1TestFile1);
			string exit2TempTestFilePath = TestUtils.CopyResourceToFile(TNTTestUtils.Exit2TestFile1);
			TNTDataImporter testImporter = new TNTDataImporter();
			using (StreamReader reader = new StreamReader(exit1TempTestFilePath, Encoding.ASCII))
			{
				testImporter.ImportData(reader, TNTTestUtils.Exit1TestFile1, this, SourceInfo.EmptySourceInfo);
			}

			using (StreamReader reader = new StreamReader(exit2TempTestFilePath, Encoding.ASCII))
			{
				testImporter.ImportData(reader, TNTTestUtils.Exit2TestFile1, this, SourceInfo.EmptySourceInfo);
			}

			ForwardingShipment testShipment1 = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, TestHouseBill1));
			Assert("Declaration on Exemption Code Shipment", testShipment1.Declarations.Length == 0);
			ForwardingShipment testShipment2 = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, TestHouseBill2));
			Assert("Declaration on Exemption Code Shipment", testShipment2.Declarations.Length == 0);
			ForwardingShipment testShipment3 = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, TestHouseBill3));
			Assert("Declaration on Exemption Code Shipment", testShipment3.Declarations.Length == 0);
		}

		public void TestExit2CustomerProblemFile1()
		{
			string tempExit2CustomerProblemFile1Path = TestUtils.CopyResourceToFile(TNTTestUtils.Exit2CustomerProblemFile1);
			using (StreamReader exit2CustomerProblemFile1FullNameStream = new StreamReader(tempExit2CustomerProblemFile1Path, Encoding.ASCII))
			{
				TNTDataImporter testImporter = new TNTDataImporter();
				testImporter.ImportData(exit2CustomerProblemFile1FullNameStream, TNTTestUtils.Exit2CustomerProblemFile1, this, SourceInfo.EmptySourceInfo);
				const string LastHouseBillInMessage = "942852984";
				const string ExemptionHouseBill = "941091513";
				ForwardingShipment lastShipmentImported = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, LastHouseBillInMessage));
				AssertNotNull("Failed to import last shipment", lastShipmentImported);
				AssertEquals("Cus Entry Num Type", "CAN", lastShipmentImported.CustomsEntryNumberType);
				AssertEquals("Cus Entry Num", "3B0425916", lastShipmentImported.CustomsEntryNumber);
				ForwardingShipment exemptionShipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, ExemptionHouseBill));
				AssertEquals("Cus Entry Num Type", "EX2", exemptionShipment.CustomsEntryNumberType);
				AssertEquals("Cus Entry Num", "", exemptionShipment.CustomsEntryNumber);
			}
		}

		public void TestExit2CustomerFileEntryTypeProblem()
		{
			string tempExit2CustomerFileEntryTypeProblemPath = TestUtils.CopyResourceToFile(TNTTestUtils.Exit2CustomerFileEntryTypeProblem);
			using (StreamReader exit2CustomerFileEntryTypeProblemFullNameStream = new StreamReader(tempExit2CustomerFileEntryTypeProblemPath, Encoding.ASCII))
			{
				TNTDataImporter testImporter = new TNTDataImporter();
				testImporter.ImportData(exit2CustomerFileEntryTypeProblemFullNameStream, TNTTestUtils.Exit2CustomerFileEntryTypeProblem, this, SourceInfo.EmptySourceInfo);
				const string EXLVHouseBillInMessage = "465627815";
				ForwardingShipment eXLVShipmentImported = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, EXLVHouseBillInMessage));
				AssertNotNull("Failed to import EXLV shipment", eXLVShipmentImported);
				AssertEquals("Cus Entry Num Type", "EXLV", eXLVShipmentImported.CustomsEntryNumberType);
				AssertEquals("Cus Entry Num", "", eXLVShipmentImported.CustomsEntryNumber);
			}
		}

		public void TestImportConsolTwiceFile1()
		{
			string tempX2TestImportTwicePath = TestUtils.CopyResourceToFile(TNTTestUtils.X2TestImportTwice);
			TNTDataImporter testImporter = new TNTDataImporter();
			using (StreamReader x2TestImportTwiceFullNameStream = new StreamReader(tempX2TestImportTwicePath, Encoding.ASCII))
			{
				testImporter.ImportData(x2TestImportTwiceFullNameStream, TNTTestUtils.X2TestImportTwice, this, SourceInfo.EmptySourceInfo);
			}

			tempX2TestImportTwicePath = TestUtils.CopyResourceToFile(TNTTestUtils.X2TestImportTwice);
			using (StreamReader x2TestImportTwiceFullNameStream2 = new StreamReader(tempX2TestImportTwicePath, Encoding.ASCII))
			{
				testImporter.ImportData(x2TestImportTwiceFullNameStream2, TNTTestUtils.X2TestImportTwice, this, SourceInfo.EmptySourceInfo);
			}

			const string HouseBillInMessage = "940353181";
			ForwardingShipment lastShipmentImported = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, HouseBillInMessage));
			AssertNotNull("Failed to import last shipment", lastShipmentImported);
		}

		public void TestImportConsolTwiceFile2()
		{
			string tempX2TestImportTwice2Path = TestUtils.CopyResourceToFile(TNTTestUtils.X2TestImportTwice2);
			TNTDataImporter testImporter = new TNTDataImporter();
			using (StreamReader x2TestImportTwice2FullNameStream = new StreamReader(tempX2TestImportTwice2Path, Encoding.ASCII))
			{
				testImporter.ImportData(x2TestImportTwice2FullNameStream, TNTTestUtils.X2TestImportTwice2, this, SourceInfo.EmptySourceInfo);
			}

			tempX2TestImportTwice2Path = TestUtils.CopyResourceToFile(TNTTestUtils.X2TestImportTwice2);
			using (StreamReader x2TestImportTwice2FullNameStream2 = new StreamReader(tempX2TestImportTwice2Path, Encoding.ASCII))
			{
				testImporter.ImportData(x2TestImportTwice2FullNameStream2, TNTTestUtils.X2TestImportTwice2, this, SourceInfo.EmptySourceInfo);
			}

			const string HouseBillInMessage = "905620969";
			ForwardingShipment lastShipmentImported = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, HouseBillInMessage));
			AssertNotNull("Failed to import last shipment", lastShipmentImported);
		}

		public void TestImportConsolTwiceFile3()
		{
			string tempX2TestImportTwice3Path = TestUtils.CopyResourceToFile(TNTTestUtils.X2TestImportTwice3);
			TNTDataImporter testImporter = new TNTDataImporter();
			using (StreamReader x2TestImportTwice3FullNameReader = new StreamReader(tempX2TestImportTwice3Path, Encoding.ASCII))
			{
				testImporter.ImportData(x2TestImportTwice3FullNameReader, TNTTestUtils.X2TestImportTwice3, this, SourceInfo.EmptySourceInfo);
			}

			const string HouseBillInMessage = "934311307";
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			ForwardingShipment lastShipmentImported = secondFactory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, HouseBillInMessage));
			AssertNotNull("Failed to import last shipment", lastShipmentImported);
			AssertEquals("Cus Entry Num Type", "EX2", lastShipmentImported.CustomsEntryNumberType);
		}

		public void TestANewFactoryIsUsedEachTimeTheExportIsRun()
		{
			TNTDataImporter importer = new TNTDataImporter();
			ITransactionParticipant[] additionalTransactionActions;
			NotificationBuffer buffer = new NotificationBuffer();
			string tinyTempFile = TestUtils.CopyResourceToFile(TNTTestUtils.TinyFileName);
			ArrayList factories = new ArrayList();
			BusinessObjectFactory currentFactory = importer.FactoryProviderForTest.Current;
			using (StreamReader reader = new StreamReader(tinyTempFile, Encoding.ASCII))
			{
				importer.ImportDataToFactory(reader, TNTTestUtils.TinyFileName, buffer, SourceInfo.EmptySourceInfo, out additionalTransactionActions);
				AssertEquals("A new factory should have been created", true, currentFactory != importer.FactoryProviderForTest.Current);
				factories.AddRange(additionalTransactionActions);
				AssertEquals("AdditionalTransactionActions should contain previous Factory", true, factories.Contains(currentFactory));
			}

			tinyTempFile = TestUtils.CopyResourceToFile(TNTTestUtils.TinyFileName);
			using (StreamReader reader = new StreamReader(tinyTempFile, Encoding.ASCII))
			{
				currentFactory = importer.FactoryProviderForTest.Current;
				importer.ImportDataToFactory(reader, "", buffer, SourceInfo.EmptySourceInfo, out additionalTransactionActions);
				AssertEquals("A new factory should have been created", true, currentFactory != importer.FactoryProviderForTest.Current);
				factories.AddRange(additionalTransactionActions);
				AssertEquals("AdditionalTransactionActions should contain previous Factory", true, factories.Contains(currentFactory));
			}
		}

		#region INotifications Members
		void INotifications.Add(INotification @event)
		{
			if (@event.Message.StartsWith("Error: Could not find record from Natural"))
			{
			}
			else if (@event.Message.StartsWith("Error: You must set up the Email Notification Group in order for"))
			{
			}
			else if (@event.Message.IndexOf("source directory") != -1 || @event.Message.IndexOf("Invalid file name") != -1 || @event.Message.IndexOf("Error") != -1)
			{
				Fail(@event.Message);
			}
			else if (@event is ErrorNotification)
			{
				Fail("Unexpected Error Notify Event Sent: " + @event.Message);
			}
		}

		#endregion
		#region Implementation
		const string TestHouseBill1 = "929399270";
		const string TestHouseBill2 = "936367286";
		const string TestHouseBill3 = "937382537";
		protected TNTTestUtils TestUtils = new TNTTestUtils();
		protected override void SetUp()
		{
			base.SetUp();
			TNTDataRegistry.Instance.QuantumFileSourceDirectory = TestUtils.TempDirectory;
			TNTDataRegistry.Instance.QuantumFileProcessedDirectory = TestUtils.TempDirectory;
			OrgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			CreatePatternMatchOverridesForSetUp();
			Factory.Save();
		}

		OrgHeader OrgProxy;
		protected override void TearDown()
		{
			TestUtils.DeleteTempDirectoryFiles();
			base.TearDown();
		}

		protected void CreatePatternMatchOverridesForSetUp()
		{
			CreatePatternMatchOverride("SYD", "AUSYD");
			CreatePatternMatchOverride("ADL", "AUADL");
			CreatePatternMatchOverride("MEL", "AUMEL");
			CreatePatternMatchOverride("BNE", "AUBNE");
			CreatePatternMatchOverride("MCY", "AUMCY");
			CreatePatternMatchOverride("CBR", "AUCBR");
			CreatePatternMatchOverride("HM3", "AUHMG");
			CreatePatternMatchOverride("ASP", "AUASP");
			CreatePatternMatchOverride("PER", "AUPER");
			CreatePatternMatchOverride("NTL", "AUNTL");
			CreatePatternMatchOverride("TWB", "AUTWB");
			CreatePatternMatchOverride("MKY", "AUMKY");
			CreatePatternMatchOverride("OOL", "AUOOL");
			CreatePatternMatchOverride("WGA", "AUWGA");
			CreatePatternMatchOverride("WOL", "AUWOL");
			CreatePatternMatchOverride("GOS", "AUGOS");
			CreatePatternMatchOverride("LSY", "AULSY");
			CreatePatternMatchOverride("SIN", "SGSIN");
			CreatePatternMatchOverride("SFO", "USSFO");
			CreatePatternMatchOverride("LAX", "USLAX");
			CreatePatternMatchOverride("AMS", "NLAMS");
			CreatePatternMatchOverride("BA3", "USBAG");
			CreatePatternMatchOverride("USO", "PGUSO");
			CreatePatternMatchOverride("BDL", "USBDL");
			CreatePatternMatchOverride("BQ4", "USBQW");
			CreatePatternMatchOverride("NYC", "USNYC");
			CreatePatternMatchOverride("SUV", "FJSUV");
			CreatePatternMatchOverride("NAN", "FJNAN");
			CreatePatternMatchOverride("AAL", "DKAAL");
			CreatePatternMatchOverride("WSV", "USWSV");
			CreatePatternMatchOverride("CMB", "USCMB");
			CreatePatternMatchOverride("BTH", "USBTH");
			CreatePatternMatchOverride("HKG", "HKHKG");
			CreatePatternMatchOverride("PHL", "USPHL");
			CreatePatternMatchOverride("CA4", "USCAQ");
			CreatePatternMatchOverride("CB3", "AUCBN");
			CreatePatternMatchOverride("BOS", "USBOS");
			CreatePatternMatchOverride("ORD", "USORD");
			CreatePatternMatchOverride("QUE", "CAQUE");
			CreatePatternMatchOverride("EWR", "USEWR");
			CreatePatternMatchOverride("BWI", "USBWI");
			CreatePatternMatchOverride("POM", "PGPOM");
			CreatePatternMatchOverride("PNO", "MXPNO");
			CreatePatternMatchOverride("NBI", "USNBI");
			CreatePatternMatchOverride("AKL", "NZAKL");
			CreatePatternMatchOverride("WLG", "NZWLG");
			CreatePatternMatchOverride("HLZ", "NZHLZ");
			CreatePatternMatchOverride("NPE", "NZNPE");
			CreatePatternMatchOverride("CHC", "NZCHC");
			CreatePatternMatchOverride("SEL", "KRSEL");
			CreatePatternMatchOverride("YUL", "CAYUL");
			CreatePatternMatchOverride("PPT", "PFPPT");
			CreatePatternMatchOverride("TRG", "NZTRG");
			CreatePatternMatchOverride("NZO", "KENZO");
			CreatePatternMatchOverride("DUD", "NZDUD");
			CreatePatternMatchOverride("PMR", "NZPMR");
			CreatePatternMatchOverride("TBU", "TOTBU");
			CreatePatternMatchOverride("NPL", "NZNPL");
			CreatePatternMatchOverride("BKK", "THBKK");
			CreatePatternMatchOverride("KOR", "PGKOR");
			CreatePatternMatchOverride("YYZ", "AUNUB");
			CreatePatternMatchOverride("PEN", "MYPEN");
			CreatePatternMatchOverride("XMN", "CNXMN");
			CreatePatternMatchOverride("GNO", "USGNO");
			CreatePatternMatchOverride("NGO", "JPNGO");
			CreatePatternMatchOverride("JXO", "JPAJX");
			CreatePatternMatchOverride("APW", "WSAPW");
			CreatePatternMatchOverride("POA", "BRPOA");
			CreatePatternMatchOverride("ADC", "USADC");
			CreatePatternMatchOverride("LHR", "GBLHR");
			CreatePatternMatchOverride("JNB", "ZAJNB");
			CreatePatternMatchOverride("RTM", "NLRTM");
		}

		protected void CreatePatternMatchOverride(string code, string uNLOCO)
		{
			OrgPatternMatchOverride[] patterns = (OrgPatternMatchOverride[])OrgProxy.PatternMatchOverrides_ForBinding.Find(new ZQuery(OrgPatternMatchOverrideSchema.OO_ForeignCode, code));
			if (patterns.Length == 0)
			{
				OrgPatternMatchOverride pattern1 = OrgProxy.CreatePatternMatchOverrideForTest();
				pattern1.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Port;
				pattern1.OO_ForeignCode = code;
				pattern1.OO_LocalCode = uNLOCO;
				pattern1.OO_LocalGuid = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, uNLOCO).PK;
			}
		}
		#endregion
	}
}
