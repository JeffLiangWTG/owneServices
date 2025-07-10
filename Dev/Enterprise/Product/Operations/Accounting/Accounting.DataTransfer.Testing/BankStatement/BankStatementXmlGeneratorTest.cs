using System.IO;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.BankStatement.Testing
{
	sealed class BankStatementXmlGeneratorTest : TestCase
	{
		#region TestGeneratXmlFromNab

		public void TestGeneratXmlFromNab()
		{
			string xmlFile = "";
			try
			{
				var file = embeddedResourceRetriever.SaveResourceToFile(NabFile);
				xmlFile = BankStatementXmlGenerator.GeneratXmlFromNab(file);
				AssertASCIIFileSameAsString(xmlFile, ExpectedXmlNab);
			}
			finally
			{
				if (File.Exists(xmlFile))
				{
					File.Delete(xmlFile);
				}
			}
		}

		readonly string NabFile = "NabBankStatement.csv";

		#region ExpectedXmlNab

		readonly string ExpectedXmlNab =
@"<BankStatement>
  <BankStatementLines>
    <BankStatementLine>
      <TransactionType>RCB</TransactionType>
      <ChequeOrReferenceNumber>REF</ChequeOrReferenceNumber>
      <TransactionDescription>GOLDY FASHION HOUSE PTY LTD</TransactionDescription>
      <PayeeName></PayeeName>
      <TransactionDate>2006-11-06</TransactionDate>
      <StatementDate>2006-11-06</StatementDate>
      <TransactionAmount>-1601.00</TransactionAmount>
      <CurrencyCode>AUD</CurrencyCode>
    </BankStatementLine>
    <BankStatementLine>
      <TransactionType>RCB</TransactionType>
      <ChequeOrReferenceNumber>REF</ChequeOrReferenceNumber>
      <TransactionDescription>CASH AND/OR CHEQUES</TransactionDescription>
      <PayeeName></PayeeName>
      <TransactionDate>2006-11-06</TransactionDate>
      <StatementDate>2006-11-06</StatementDate>
      <TransactionAmount>-342.54</TransactionAmount>
      <CurrencyCode>AUD</CurrencyCode>
    </BankStatementLine>
    <BankStatementLine>
      <TransactionType>RCB</TransactionType>
      <ChequeOrReferenceNumber>REF</ChequeOrReferenceNumber>
      <TransactionDescription>ELECTRONIC TRANSFERCLARION AUSTRALIA P/REF: 6648-2393.     VCLAAUS CLARION</TransactionDescription>
      <PayeeName></PayeeName>
      <TransactionDate>2006-11-06</TransactionDate>
      <StatementDate>2006-11-06</StatementDate>
      <TransactionAmount>-13790.26</TransactionAmount>
      <CurrencyCode>AUD</CurrencyCode>
    </BankStatementLine>
    <BankStatementLine>
      <TransactionType>RCB</TransactionType>
      <ChequeOrReferenceNumber>41</ChequeOrReferenceNumber>
      <TransactionDescription>AGENTS CREDITS</TransactionDescription>
      <PayeeName></PayeeName>
      <TransactionDate>2006-11-06</TransactionDate>
      <StatementDate>2006-11-06</StatementDate>
      <TransactionAmount>-28779.85</TransactionAmount>
      <CurrencyCode>AUD</CurrencyCode>
    </BankStatementLine>
    <BankStatementLine>
      <TransactionType>RCB</TransactionType>
      <ChequeOrReferenceNumber>REF</ChequeOrReferenceNumber>
      <TransactionDescription>ALCENG-CREDITOR020ALCAN ENGINEERIN 158824</TransactionDescription>
      <PayeeName></PayeeName>
      <TransactionDate>2006-11-06</TransactionDate>
      <StatementDate>2006-11-06</StatementDate>
      <TransactionAmount>-2848.71</TransactionAmount>
      <CurrencyCode>AUD</CurrencyCode>
    </BankStatementLine>
    <BankStatementLine>
      <TransactionType>RCB</TransactionType>
      <ChequeOrReferenceNumber>REF</ChequeOrReferenceNumber>
      <TransactionDescription>BOTANY</TransactionDescription>
      <PayeeName></PayeeName>
      <TransactionDate>2006-11-06</TransactionDate>
      <StatementDate>2006-11-06</StatementDate>
      <TransactionAmount>-2541.30</TransactionAmount>
      <CurrencyCode>AUD</CurrencyCode>
    </BankStatementLine>
    <BankStatementLine>
      <TransactionType>RCB</TransactionType>
      <ChequeOrReferenceNumber>REF</ChequeOrReferenceNumber>
      <TransactionDescription>EFT 244086        Patrick Finance  144168</TransactionDescription>
      <PayeeName></PayeeName>
      <TransactionDate>2006-11-06</TransactionDate>
      <StatementDate>2006-11-06</StatementDate>
      <TransactionAmount>-130660.27</TransactionAmount>
      <CurrencyCode>AUD</CurrencyCode>
    </BankStatementLine>
    <BankStatementLine>
      <TransactionType>CHQ</TransactionType>
      <ChequeOrReferenceNumber>123463</ChequeOrReferenceNumber>
      <TransactionDescription>CHEQUE</TransactionDescription>
      <PayeeName></PayeeName>
      <TransactionDate>2006-11-06</TransactionDate>
      <StatementDate>2006-11-06</StatementDate>
      <TransactionAmount>10336.70</TransactionAmount>
      <CurrencyCode>AUD</CurrencyCode>
    </BankStatementLine>
    <BankStatementLine>
      <TransactionType>EFT</TransactionType>
      <ChequeOrReferenceNumber>REF</ChequeOrReferenceNumber>
      <TransactionDescription>360               TOLL INTERNATION 119812</TransactionDescription>
      <PayeeName></PayeeName>
      <TransactionDate>2006-11-06</TransactionDate>
      <StatementDate>2006-11-06</StatementDate>
      <TransactionAmount>306005.94</TransactionAmount>
      <CurrencyCode>AUD</CurrencyCode>
    </BankStatementLine>
  </BankStatementLines>
</BankStatement>";

		#endregion

		#endregion

		#region TestGeneratXmlFromANZ

		public void TestGeneratXmlFromANZ()
		{
			string xmlFile = "";
			try
			{
				var file = embeddedResourceRetriever.SaveResourceToFile(ANZFile);
				xmlFile = BankStatementXmlGenerator.GeneratXmlFromANZ(file);
				AssertASCIIFileSameAsString(xmlFile, ExpectedXmlANZ);
			}
			finally
			{
				if (File.Exists(xmlFile))
				{
					File.Delete(xmlFile);
				}
			}
		}

		readonly string ANZFile = "ANZ_NZ_BankStatement_ForTest.csv";

		#region ExpectedXmlANZ

		readonly string ExpectedXmlANZ =
@"<BankStatement>
  <BankStatementLines>
    <BankStatementLine>
      <TransactionType>DCR</TransactionType>
      <ChequeOrReferenceNumber>STRANDBAGS P</ChequeOrReferenceNumber>
      <TransactionAmount>-54922.02</TransactionAmount>
    </BankStatementLine>
    <BankStatementLine>
      <TransactionType>CHQ</TransactionType>
      <ChequeOrReferenceNumber>301125</ChequeOrReferenceNumber>
      <TransactionAmount>100.79</TransactionAmount>
    </BankStatementLine>
    <BankStatementLine>
      <TransactionType>DCR</TransactionType>
      <ChequeOrReferenceNumber>00023</ChequeOrReferenceNumber>
      <TransactionAmount>-7007.83</TransactionAmount>
    </BankStatementLine>
    <BankStatementLine>
      <TransactionType>RCB</TransactionType>
      <ChequeOrReferenceNumber>000000000000</ChequeOrReferenceNumber>
      <TransactionAmount>-437103.22</TransactionAmount>
    </BankStatementLine>
    <BankStatementLine>
      <TransactionType>CHQ</TransactionType>
      <ChequeOrReferenceNumber>1260</ChequeOrReferenceNumber>
      <TransactionAmount>10.00</TransactionAmount>
    </BankStatementLine>
    <BankStatementLine>
      <TransactionType>EFT</TransactionType>
      <ChequeOrReferenceNumber>INCL GST</ChequeOrReferenceNumber>
      <TransactionAmount>3038.73</TransactionAmount>
    </BankStatementLine>
    <BankStatementLine>
      <TransactionType>CHQ</TransactionType>
      <ChequeOrReferenceNumber></ChequeOrReferenceNumber>
      <TransactionAmount>500000.00</TransactionAmount>
    </BankStatementLine>
    <BankStatementLine>
      <TransactionType>CHQ</TransactionType>
      <ChequeOrReferenceNumber></ChequeOrReferenceNumber>
      <TransactionAmount>266.99</TransactionAmount>
    </BankStatementLine>
  </BankStatementLines>
</BankStatement>";

		#endregion

		#endregion

		#region TestGeneratXmlFromANZ

		public void TestGeneratXmlFromWestpac()
		{
			string xmlFile = "";
			try
			{
				var file = embeddedResourceRetriever.SaveResourceToFile(WestpacFile);
				xmlFile = BankStatementXmlGenerator.GeneratXmlFromWestpac(file);
				AssertASCIIFileSameAsString(xmlFile, ExpectedXmlWestpac);
			}
			finally
			{
				if (File.Exists(xmlFile))
				{
					File.Delete(xmlFile);
				}
			}
		}

		readonly string WestpacFile = "WestpacNZBankStatement.csv";

		#region ExpectedXmlWestpac

		readonly string ExpectedXmlWestpac =
@"<BankStatement>
  <BankStatementLines>
    <BankStatementLine>
      <TransactionType>DCR</TransactionType>
      <ChequeOrReferenceNumber>N PEN /RFB/0</ChequeOrReferenceNumber>
      <TransactionAmount>-2747.90</TransactionAmount>
    </BankStatementLine>
    <BankStatementLine>
      <TransactionType>DCR</TransactionType>
      <ChequeOrReferenceNumber></ChequeOrReferenceNumber>
      <TransactionAmount>-6836.33</TransactionAmount>
    </BankStatementLine>
    <BankStatementLine>
      <TransactionType>DCR</TransactionType>
      <ChequeOrReferenceNumber></ChequeOrReferenceNumber>
      <TransactionAmount>-12566.76</TransactionAmount>
    </BankStatementLine>
    <BankStatementLine>
      <TransactionType>EFT</TransactionType>
      <ChequeOrReferenceNumber></ChequeOrReferenceNumber>
      <TransactionAmount>112.50</TransactionAmount>
    </BankStatementLine>
    <BankStatementLine>
      <TransactionType>EFT</TransactionType>
      <ChequeOrReferenceNumber></ChequeOrReferenceNumber>
      <TransactionAmount>219.38</TransactionAmount>
    </BankStatementLine>
    <BankStatementLine>
      <TransactionType>EFT</TransactionType>
      <ChequeOrReferenceNumber></ChequeOrReferenceNumber>
      <TransactionAmount>225.00</TransactionAmount>
    </BankStatementLine>
    <BankStatementLine>
      <TransactionType>EFT</TransactionType>
      <ChequeOrReferenceNumber></ChequeOrReferenceNumber>
      <TransactionAmount>236.25</TransactionAmount>
    </BankStatementLine>
    <BankStatementLine>
      <TransactionType>CHQ</TransactionType>
      <ChequeOrReferenceNumber>112510</ChequeOrReferenceNumber>
      <TransactionAmount>150.00</TransactionAmount>
    </BankStatementLine>
    <BankStatementLine>
      <TransactionType>CHQ</TransactionType>
      <ChequeOrReferenceNumber>112511</ChequeOrReferenceNumber>
      <TransactionAmount>150.00</TransactionAmount>
    </BankStatementLine>
    <BankStatementLine>
      <TransactionType>CHQ</TransactionType>
      <ChequeOrReferenceNumber>112550</ChequeOrReferenceNumber>
      <TransactionAmount>32835.57</TransactionAmount>
    </BankStatementLine>
  </BankStatementLines>
</BankStatement>"
		;
		#endregion
		#endregion

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();

			embeddedResourceRetriever = new ();
		}

		protected override void TearDown()
		{
			base.TearDown();

			embeddedResourceRetriever?.Dispose();
			embeddedResourceRetriever = null;
		}

		EmbeddedResourceRetriever embeddedResourceRetriever;
		#endregion
	}
}
