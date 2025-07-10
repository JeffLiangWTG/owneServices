using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	sealed class EManifestResponseWrapperTest : TestCaseWithFactory
	{
		public void TestGetLinkedObject()
		{
			var newFactory = new BusinessObjectFactory();
			var master = newFactory.NewWithValidTestData<CusCAeMHMaster>();
			master.BP_MessageReference = "AX123456";
			var house = master.HouseBills.AddNew();
			house.BW_MessageReference = "803636474747";

			var dec = newFactory.NewWithValidTestData<JobDeclaration>();
			dec.JE_GC = GlbCompany.CurrentCompany.PK;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.TransactionNumber.AccountSecurityCode = "12345";
			dec.TransactionNumber.SequentialNumber = "00006789";
			dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var invoice = dec.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();
			var number = dec.AdditionalReferenceNumbers.AddNew();
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = "A01X1234567";
			dec.DoMerge();
			newFactory.Save();

			var message = Factory.New<ACIForwarderMessage>();
			message.EM_MessageType = "XYZ";
			message.EM_MessageText = @"UNH+27+GOVCBR:D:11B:UN:ACIHCM
BGM+87+081-14081201+9
RFF+AGO:CLS-AX123456
NAD+FW+8036
DOC+85+8036CAH1000001
UNS+D
HYN+3
UNS+S
UNT+9+27".Replace("'\r\n", "'").Replace("\r\n", "'");
			var wrapper = new EManifestResponseWrapper(message);
			var masterBO = wrapper.LinkedObject as CusCAeMHMaster;
			AssertNotNull(masterBO);
			AssertEquals("Matched to MasterBill", master.PK, masterBO.PK);

			message = Factory.New<ACIForwarderMessage>();
			message.EM_MessageType = "XYZ";
			message.EM_MessageText = @"UNH+27+GOVCBR:D:11B:UN:ACIHCM
BGM+87+081-14081201+9
RFF+AGO:HBL-803636474747
NAD+FW+8036
DOC+85+8036CAH1000001
UNS+D
HYN+3
UNS+S
UNT+9+27".Replace("'\r\n", "'").Replace("\r\n", "'");
			wrapper = new EManifestResponseWrapper(message);
			var houseBO = wrapper.LinkedObject as CusCAeMHHouse;
			AssertNotNull(houseBO);
			AssertEquals("Matched to HouseBill", house.PK, houseBO.PK);

			message = Factory.New<ACIForwarderMessage>();
			message.EM_MessageType = "XYZ";
			message.EM_MessageText = @"UNH+1+GOVCBR:D:11B:UN
BGM+23+12345000067897
DTM+9:201510270833:203
RFF+AGO:123029118RM0003
STS++2:::0001
UNS+D
HYN+3
UNS+S
UNT+9+1".Replace("'\r\n", "'").Replace("\r\n", "'");
			wrapper = new EManifestResponseWrapper(message);
			var entryBO = wrapper.LinkedObject as CusEntryHeader;
			AssertNotNull(entryBO);
			AssertEquals("Matched to declaration by entry reference", dec.ReleaseEntryHeader.PK, entryBO.PK);

			message = Factory.New<ACIForwarderMessage>();
			message.EM_MessageType = "XYZ";
			message.EM_MessageText = @"UNH+1+GOVCBR:D:11B:UN
BGM+23+A01X1234567
DTM+9:201510270833:203
RFF+AGO:123029118RM0003
STS++2:::0001
UNS+D
HYN+3
UNS+S
UNT+9+1".Replace("'\r\n", "'").Replace("\r\n", "'");
			wrapper = new EManifestResponseWrapper(message);
			entryBO = wrapper.LinkedObject as CusEntryHeader;
			AssertNotNull(entryBO);
			AssertEquals("Matched to declaration by CCN Number", dec.ReleaseEntryHeader.PK, entryBO.PK);
		}
	}
}
