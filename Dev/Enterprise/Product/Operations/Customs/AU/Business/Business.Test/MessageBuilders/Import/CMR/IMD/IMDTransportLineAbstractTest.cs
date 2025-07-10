using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Edifact;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class IMDTransportLineAbstractTest : TestCaseWithFactory
	{
		public abstract void TestPopulate();

		public void TestNewLineCharactersOfMarksAndNumbersAreTrimmed()
		{
			SetupDeclarationWithMarksAndNumbers();
			var message = GetPopulatedMessage().ToUpper();
			AssertEquals(-1, message.IndexOf(MarksAndNumbers.ToUpper()));
			AssertEquals(true, message.IndexOf(MarksAndNumbers.Replace("\r\n", "").ToUpper()) > -1);
		}

		protected abstract IMDTransportLine GetTransportLineToTest { get; }

		protected abstract string DeclarationTransportMode { get; }

		protected override void SetUp()
		{
			SetUpDeclaration();
			base.SetUp();
		}

		protected ZString GetPopulatedMessage()
		{
			var line = GetTransportLineToTest;
			line.Populate(1, LineAction.Insert);
			return line.group21.ToString(new UNOCCMRCharacterSet());
		}

		void SetUpDeclaration()
		{
			testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);
			testDec.JE_TransportMode = DeclarationTransportMode;
			testDec.JE_HouseBill = "AQT1HBL";
			testDec.JE_MasterBill = "AQT1";

			houseBill1 = testDec.PrimaryHouseBill;

			Customs.Business.Bill masterBill2 = testDec.Bills.AddNew();
			masterBill2.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill2.CU_BillNum = "AQT2";

			houseBill2 = testDec.Bills.AddNew();
			houseBill2.CU_MasterBill = "AQT2";
			houseBill2.CU_HouseBill = "AQT2HBL";

			var houseBill3 = testDec.Bills.AddNew();
			houseBill3.CU_HouseBill = "AQT3HBL";

			if (DeclarationTransportMode == Core.Constants.TransportModes.Sea)
			{
				container = testDec.CusContainers.AddNew();
				container.CO_ContainerNumber = "Container No";
			}

			package1 = testDec.Packages.AddNew();
			package1.CW_HouseBill = houseBill1.CU_BillUniqueCode;
			package1.CW_PackQty = 100;
			package1.CW_OuterPacks = 1;

			package2 = testDec.Packages.AddNew();
			package2.CW_HouseBill = houseBill2.CU_BillUniqueCode;
			package2.CW_PackQty = 200;
			package2.CW_OuterPacks = 2;

			package3 = testDec.Packages.AddNew();
			package3.CW_HouseBill = houseBill3.CU_BillUniqueCode;
			package3.CW_InBondPackQty = 2;
			package3.CW_PackQty = 2;

			invoiceHeader = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var sender = new SendsMessagesToCustomsShutterUpperer();
			testDec.MessageInitiator = sender;
			testDec.DoMerge();

			entryHeader = testDec.CustomsEntryHeaders[0];
			entryLine = entryHeader.MergedLines[0];
		}

		protected abstract void SetupDeclarationWithMarksAndNumbers();

		protected JobDeclaration testDec;
		protected JobComInvoiceHeader invoiceHeader;
		protected JobComInvoiceLine invoiceLine;
		protected CusEntryLine entryLine;
		protected CusEntryHeader entryHeader;
		protected CusContainer container;
		protected Package package1;
		protected Package package2;
		protected Package package3;
		protected Customs.Business.Bill houseBill1;
		protected Customs.Business.Bill houseBill2;

		protected string MarksAndNumbers => "Marks and Numbers" + "\r\n";
	}
}
