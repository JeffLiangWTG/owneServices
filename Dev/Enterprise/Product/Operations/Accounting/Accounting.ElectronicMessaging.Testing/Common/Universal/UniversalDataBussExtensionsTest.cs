using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Universal.Testing
{
	public class UniversalDataBussExtensionsTest : TestCaseWithFactory
	{
		public void TestToXmlFragment_NullParameter()
		{
			TransactionInfo nullValue = null;
			AssertExceptionThrown<ArgumentNullException>(() =>
				nullValue.ToXmlFragment());
		}

		public void TestToXmlFragment_WithEmptyTransactionInfo()
		{
			var transactionInfo = new TransactionInfo();
			var output = transactionInfo.ToXmlFragment();

			var expectedXmlResult = @"<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo/>
</UniversalTransaction>";
			this.AssertXMLEqualsIgnoreChildOrder("XML not expected", expectedXmlResult, output);
		}

		public void TestToXmlFragment_WithTransactionInfo()
		{
			var transactionInfo = GetTestTransactionInfo();
			var output = transactionInfo.ToXmlFragment();

			var expectedXmlResult = @"<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <Branch>
      <Code>BNE</Code>
    </Branch>
    <Description>AAAAAA</Description>
    <LocalTotal>200.00</LocalTotal>
    <TransactionDate>2022-03-04T00:00:00</TransactionDate>
    <TransactionType>AJL</TransactionType>
  </TransactionInfo>
</UniversalTransaction>";
			this.AssertXMLEqualsIgnoreChildOrder("XML not expected", expectedXmlResult, output);
		}

		public void TestToBase64EncodedXmlFragment_NullParameter()
		{
			TransactionInfo nullValue = null;
			AssertExceptionThrown<ArgumentNullException>(() =>
				nullValue.ToBase64EncodedXmlFragment());
		}

		public void TestToBase64EncodedXmlFragment_Encoding()
		{
			var transactionInfo = new TransactionInfo();
			var encodedOutput = transactionInfo.ToBase64EncodedXmlFragment();
			var byteArray = Convert.FromBase64String(encodedOutput);
			var outputXml = MessageEncoding.UTF8WithoutBOM.GetString(byteArray);

			var expectedXmlResult = @"<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo/>
</UniversalTransaction>";
			this.AssertXMLEqualsIgnoreChildOrder("XML not expected", expectedXmlResult, outputXml);
		}

		static TransactionInfo GetTestTransactionInfo()
		{
			return new TransactionInfo
			{
				LocalTotal = 200.00m,
				TransactionType = TransactionType.AJL,
				TransactionDate = new ZDateTime(2022, 03, 04),
				Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code },
				Description = "AAAAAA",
			};
		}
	}
}
