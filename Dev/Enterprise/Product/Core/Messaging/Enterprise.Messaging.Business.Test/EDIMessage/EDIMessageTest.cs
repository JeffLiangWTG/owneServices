using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework.TestHelper;

namespace Enterprise.Messaging.Testing
{
	public abstract class EDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		protected EDIMessageTest() { }

		#region eNett Related Properties

		public void TestEM_Ledger()
		{
			AssertEquals(ExpectedEM_Ledger, EDIMessage.EM_Ledger);
		}

		public void TestEM_TransactionType()
		{
			AssertEquals(ExpectedEM_TransactionType, EDIMessage.EM_TransactionType);
		}

		public void TestEM_TransactionNumber()
		{
			AssertEquals(ExpectedEM_TransactionNumber, EDIMessage.EM_TransactionNumber);
		}

		public void TestEM_JobInvoiceNo()
		{
			AssertEquals(ExpectedEM_JobInvoiceNo, EDIMessage.EM_JobInvoiceNo);
		}

		public void TestEM_ChequeOrReference()
		{
			AssertEquals(ExpectedEM_ChequeOrReference, EDIMessage.EM_ChequeOrReference);
		}

		public void TestEM_InvoiceDate()
		{
			AssertEquals(ExpectedEM_InvoiceDate, EDIMessage.EM_InvoiceDate);
		}

		public void TestEM_PostDate()
		{
			AssertEquals(ExpectedEM_PostDate, EDIMessage.EM_PostDate);
		}

		public void TestEM_LocalInvoiceAmtInclTax()
		{
			AssertEquals(ExpectedEM_LocalInvoiceAmtInclTax, EDIMessage.EM_LocalInvoiceAmtInclTax);
		}

		public void TestEM_OSInvoiceAmtInclTax()
		{
			AssertEquals(ExpectedEM_OSInvoiceAmtInclTax, EDIMessage.EM_OSInvoiceAmtInclTax);
		}

		public void TestEM_Currency()
		{
			AssertEquals(ExpectedEM_Currency, EDIMessage.EM_Currency);
		}

		public void TestEM_OrganisationCode()
		{
			AssertEquals(ExpectedEM_OrganisationCode, EDIMessage.EM_OrganisationCode);
		}

		public void TestEM_OrganisationName()
		{
			AssertEquals(ExpectedEM_OrganisationName, EDIMessage.EM_OrganisationName);
		}

		#endregion

		#region Implementation

		protected virtual ZString ExpectedEM_Ledger { get { return ZString.Empty; } }
		protected virtual ZString ExpectedEM_TransactionType { get { return ZString.Empty; } }
		protected virtual ZString ExpectedEM_TransactionNumber { get { return ZString.Empty; } }
		protected virtual ZString ExpectedEM_JobInvoiceNo { get { return ZString.Empty; } }
		protected virtual ZString ExpectedEM_ChequeOrReference { get { return ZString.Empty; } }
		protected virtual ZString ExpectedEM_InvoiceDate { get { return ZString.Empty; } }
		protected virtual ZString ExpectedEM_PostDate { get { return ZString.Empty; } }
		protected virtual ZString ExpectedEM_LocalInvoiceAmtInclTax { get { return ZString.Empty; } }
		protected virtual ZString ExpectedEM_OSInvoiceAmtInclTax { get { return ZString.Empty; } }
		protected virtual ZString ExpectedEM_Currency { get { return ZString.Empty; } }
		protected virtual ZString ExpectedEM_OrganisationCode { get { return ZString.Empty; } }
		protected virtual ZString ExpectedEM_OrganisationName { get { return ZString.Empty; } }

		EDIMessage fEDIMessage;
		protected EDIMessage EDIMessage
		{
			get { return fEDIMessage ?? (fEDIMessage = GetNewMessage()); }
		}

		protected virtual EDIMessage GetNewMessage()
		{
			return (EDIMessage)Factory.New(TestedTypeHelper.GetTestedType(GetType()));
		}

		protected const string TestCharacterSet = @"UNH+36700+CUSRES:002:912:UN'BGM+961++9:200401211556:203+9+ZZ:802*9130145*24'LOC+12:AUSYD'RFF+ACW:1000'UNT+5+36700'UNH+36800+CUSRES:002:912:UN'BGM+961++9:200401211556:203+9+ZZ:802*7526493*187'LOC+12:AUSYD'RFF+ACW:1100'UNT+5+36800'";
		protected const string TestMessageNumberPlaceHolderIsRestoredExampleText = @"UNH+<<MSGNO PLACEHOLDER>>+CUSEXP:D:95A:UN+HMF68974393X160'BGM+85:::EXPRESS CONSIGNMENT MANIFEST+16068974393/C00003134+5'LOC+5+BNE'LOC+8+HKG'CNT+7:94.0:KGM'CNT+8:3'NAD+PK+1331929'TDT+13+CX102'DTM+132:001223:101'RFF+MWB:16068974393'CNT+10:1'CNI+1+3134-001'CNT+8:3'MEA+WT++KGM:94.0'LOC+5+BNE'LOC+8+HKG'NAD+CN+++ACO SEWINGART LTD+RM 833 METRO CENTER 2, 21 LAM HING:ST., KOWLOON BAY+HONG KONG+++HK'NAD+CZ+++J. LEUTENEGGER PTY LTD+PO BOX 6241:FAIRFIELD GARDENS+QLD+++AU'GDS+12'FTX+AAA+++3 CARTONS STC HOMETIME SAMPL'PAC'PCI+28+HERE ARE SOME TEST MARKS AND NUMBER:S BY SCOTT AS I NEED THEM FOR THE T:EST MESSAGE. THIS IS THE GREATEST D:ISCUSSION IN THE WORLD ABOUT THE ST:ATE OF NANOPARTICULES IN A MAGNETIC: FIELD AS DESCRIBED BY MAXWELLS EQU:ATIONS. A ROUND OF APPLAUSE FOR SCO:TT R WRIGHT THE WORLD LEADER IN THI:S AREA. NOW THAT I HAVE ENOUGH MARK:S AND NUMBERS'MOA+43:1710.02:AUD'MOA+44:12.34:AUD'DOC+811:::3B003571077FDC'UNT+26+<<MSGNO PLACEHOLDER>>'";
		protected const string TestContainedChecksumPlaceHolderExampleText = @"UNH+<<MSGNO PLACEHOLDER>>+CUSEXP:D:95A:UN+HMF68974393X160'BGM+85:::EXPRESS CONSIGNMENT MANIFEST+16068974393/C00003134+5'LOC+5+BNE'LOC+8+HKG'CNT+7:94.0:KGM'CNT+8:3'NAD+PK+1331929'TDT+13+CX102'DTM+132:001223:101'RFF+MWB:16068974393'CNT+10:1'CNI+1+3134-001'CNT+8:3'MEA+WT++KGM:94.0'LOC+5+BNE'LOC+8+HKG'NAD+CN+++ACO SEWINGART LTD+RM 833 METRO CENTER 2, 21 LAM HING:ST., KOWLOON BAY+HONG KONG+++HK'NAD+CZ+++J. LEUTENEGGER PTY LTD+PO BOX 6241:FAIRFIELD GARDENS+QLD+++AU'GDS+12'FTX+AAA+++3 CARTONS STC HOMETIME SAMPL'PAC'PCI+28+HERE ARE SOME TEST MARKS AND NUMBER:S BY SCOTT AS I NEED THEM FOR THE T:EST MESSAGE. THIS IS THE GREATEST D:ISCUSSION IN THE WORLD ABOUT THE ST:ATE OF NANOPARTICULES IN A MAGNETIC: FIELD AS DESCRIBED BY MAXWELLS EQU:ATIONS. A ROUND OF APPLAUSE FOR SCO:TT R WRIGHT THE WORLD LEADER IN THI:S AREA. NOW THAT I HAVE ENOUGH MARK:S AND NUMBERS'MOA+43:1710.02:AUD'MOA+44:12.34:AUD'DOC+811:<<CONTAINED CHECKSUM PLACE HOLDER>>::3B003571077FDC'UNT+26+<<MSGNO PLACEHOLDER>>'";
		#endregion
	}
}
