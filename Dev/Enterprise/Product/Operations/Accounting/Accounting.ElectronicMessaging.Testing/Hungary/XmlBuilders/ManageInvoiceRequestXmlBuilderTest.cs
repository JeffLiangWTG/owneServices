using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using WTG.TestHelpers.Xml;

namespace Enterprise.Accounting.ElectronicMessaging.Hungary.Testing
{
	public class ManageInvoiceRequestXmlBuilderTest : TestCaseWithFactory
	{
		[TestDate(2020, 5, 7, 15, 26, 32, 253)]
		public void TestHeaderXmlFragment()
		{
			var model = CreateModelForTest();

			var actualXml = GetRequestXmlBuilder(model).BuildHeaderXml().ToString();
			var expectedXml = HeaderXmlFragment_ExpectedXml.Replace("'", "\"");

			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string HeaderXmlFragment_ExpectedXml => FormattableString.Invariant($@"<header xmlns='{HeaderNamespace}'>
  <requestId>$$requestIdReplaceMe$$</requestId>
  <timestamp>$$timestampReplaceMe$$</timestamp>
  <requestVersion>3.0</requestVersion>
  <headerVersion>1.0</headerVersion>
</header>
");

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestUserXmlFragment()
		{
			var model = CreateModelForTest();

			var actualXml = GetRequestXmlBuilder(model).BuildUserXml().ToString();
			var expectedXml = UserXmlFragment_ExpectXml.Replace("'", "\"");

			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string UserXmlFragment_ExpectXml => FormattableString.Invariant($@"<user xmlns='{HeaderNamespace}'>
  <login>h9nupbpmgi8yhet</login>
  <passwordHash cryptoType='SHA-512'>3581511529344BF49B6B93A44BDEDBE8AED53942D04F45D6BA0702E2E5F7A928667FB0C11CDEBA39A45D010FBD89C1F951CE9FF1B6789412780C70613CDAB56B</passwordHash>
  <taxNumber>12036024</taxNumber>
  <requestSignature cryptoType='SHA3-512'>$$requestSignatureReplaceMe$$</requestSignature>
</user>
");

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestSoftwareXmlFragment()
		{
			var model = CreateModelForTest();

			var actualXml = GetRequestXmlBuilder(model).BuildSoftwareXml().ToString();
			var expectedXml = FormattableString.Invariant($@"<software xmlns='{Namespace}'>
  <softwareId>AU658947CARGOWISE1</softwareId>
  <softwareName>CargoWise</softwareName>
  <softwareOperation>ONLINE_SERVICE</softwareOperation>
  <softwareMainVersion>20.4.30.9</softwareMainVersion>
  <softwareDevName>Wisetech Global Limited</softwareDevName>
  <softwareDevContact>eInvoice.Hungary@WisetechGlobal.com</softwareDevContact>
  <softwareDevCountryCode>AU</softwareDevCountryCode>
  <softwareDevTaxNumber>41065894724</softwareDevTaxNumber>
</software>
").Replace("'", "\"");

			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestExchangeTokenXmlFragment()
		{
			var model = CreateModelForTest();

			var expectedXml = FormattableString.Invariant($"<exchangeToken xmlns='{Namespace}'>$$exchangeTokenReplaceMe$$</exchangeToken>").Replace("'", "\"");
			var actualXml = GetRequestXmlBuilder(model).BuildExchangeTokenXml().ToString();
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestInvoiceOperationsXmlFragment_NormalInvoice()
		{
			var model = CreateModelForTest();
			var actualXml = GetRequestXmlBuilder(model).BuildInvoiceOperationsXml().ToString();
			var expectedXml = InvoiceOperationsXmlFragment_NormalInvoice_ExpectedXml.Replace("'", "\"");

			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string InvoiceOperationsXmlFragment_NormalInvoice_ExpectedXml => FormattableString.Invariant($@"<invoiceOperations xmlns='{Namespace}'>
  <compressedContent>false</compressedContent>
  <invoiceOperation>
    <index>1</index>
    <invoiceOperation>CREATE</invoiceOperation>
    <invoiceData>PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz48SW52b2ljZURhdGEgeG1sbnM9Imh0dHA6Ly9zY2hlbWFzLm5hdi5nb3YuaHUvT1NBLzMuMC9kYXRhIiB4bWxuczpuczI9Imh0dHA6Ly9zY2hlbWFzLm5hdi5nb3YuaHUvT1NBLzMuMC9iYXNlIj48aW52b2ljZUlzc3VlRGF0ZT48L2ludm9pY2VJc3N1ZURhdGU+PGNvbXBsZXRlbmVzc0luZGljYXRvcj5mYWxzZTwvY29tcGxldGVuZXNzSW5kaWNhdG9yPjxpbnZvaWNlTWFpbj48aW52b2ljZT48aW52b2ljZUhlYWQ+PHN1cHBsaWVySW5mbz48c3VwcGxpZXJUYXhOdW1iZXI+PG5zMjp0YXhwYXllcklkPjc0MzI4OTc0PC9uczI6dGF4cGF5ZXJJZD48L3N1cHBsaWVyVGF4TnVtYmVyPjxncm91cE1lbWJlclRheE51bWJlcj48bnMyOnRheHBheWVySWQ+MTIwMzYwMjQ8L25zMjp0YXhwYXllcklkPjwvZ3JvdXBNZW1iZXJUYXhOdW1iZXI+PHN1cHBsaWVyQWRkcmVzcz48bnMyOnNpbXBsZUFkZHJlc3M+PG5zMjpjb3VudHJ5Q29kZSAvPjxuczI6cG9zdGFsQ29kZT4wMDAwPC9uczI6cG9zdGFsQ29kZT48bnMyOmFkZGl0aW9uYWxBZGRyZXNzRGV0YWlsPjwvbnMyOmFkZGl0aW9uYWxBZGRyZXNzRGV0YWlsPjwvbnMyOnNpbXBsZUFkZHJlc3M+PC9zdXBwbGllckFkZHJlc3M+PC9zdXBwbGllckluZm8+PGN1c3RvbWVySW5mbz48Y3VzdG9tZXJWYXRTdGF0dXM+RE9NRVNUSUM8L2N1c3RvbWVyVmF0U3RhdHVzPjxjdXN0b21lclZhdERhdGE+PGN1c3RvbWVyVGF4TnVtYmVyPjxuczI6dGF4cGF5ZXJJZD4xMjU2NTk4PC9uczI6dGF4cGF5ZXJJZD48Z3JvdXBNZW1iZXJUYXhOdW1iZXI+PG5zMjp0YXhwYXllcklkPjU3MjkzODc0PC9uczI6dGF4cGF5ZXJJZD48L2dyb3VwTWVtYmVyVGF4TnVtYmVyPjwvY3VzdG9tZXJUYXhOdW1iZXI+PC9jdXN0b21lclZhdERhdGE+PGN1c3RvbWVyQWRkcmVzcz48bnMyOnNpbXBsZUFkZHJlc3M+PG5zMjpwb3N0YWxDb2RlPjAwMDA8L25zMjpwb3N0YWxDb2RlPjxuczI6YWRkaXRpb25hbEFkZHJlc3NEZXRhaWw+PC9uczI6YWRkaXRpb25hbEFkZHJlc3NEZXRhaWw+PC9uczI6c2ltcGxlQWRkcmVzcz48L2N1c3RvbWVyQWRkcmVzcz48L2N1c3RvbWVySW5mbz48aW52b2ljZURldGFpbD48aW52b2ljZUNhdGVnb3J5PjwvaW52b2ljZUNhdGVnb3J5PjxpbnZvaWNlRGVsaXZlcnlEYXRlPjwvaW52b2ljZURlbGl2ZXJ5RGF0ZT48Y3VycmVuY3lDb2RlPjwvY3VycmVuY3lDb2RlPjxleGNoYW5nZVJhdGU+PC9leGNoYW5nZVJhdGU+PHBheW1lbnREYXRlPjwvcGF5bWVudERhdGU+PGludm9pY2VBcHBlYXJhbmNlPkVMRUNUUk9OSUM8L2ludm9pY2VBcHBlYXJhbmNlPjwvaW52b2ljZURldGFpbD48L2ludm9pY2VIZWFkPjxpbnZvaWNlTGluZXM+PG1lcmdlZEl0ZW1JbmRpY2F0b3I+ZmFsc2U8L21lcmdlZEl0ZW1JbmRpY2F0b3I+PC9pbnZvaWNlTGluZXM+PGludm9pY2VTdW1tYXJ5PjxzdW1tYXJ5Tm9ybWFsPjxpbnZvaWNlTmV0QW1vdW50PjAuMDA8L2ludm9pY2VOZXRBbW91bnQ+PGludm9pY2VOZXRBbW91bnRIVUY+MC4wMDwvaW52b2ljZU5ldEFtb3VudEhVRj48aW52b2ljZVZhdEFtb3VudD4wLjAwPC9pbnZvaWNlVmF0QW1vdW50PjxpbnZvaWNlVmF0QW1vdW50SFVGPjAuMDA8L2ludm9pY2VWYXRBbW91bnRIVUY+PC9zdW1tYXJ5Tm9ybWFsPjxzdW1tYXJ5R3Jvc3NEYXRhPjxpbnZvaWNlR3Jvc3NBbW91bnQ+PC9pbnZvaWNlR3Jvc3NBbW91bnQ+PGludm9pY2VHcm9zc0Ftb3VudEhVRj48L2ludm9pY2VHcm9zc0Ftb3VudEhVRj48L3N1bW1hcnlHcm9zc0RhdGE+PC9pbnZvaWNlU3VtbWFyeT48L2ludm9pY2U+PC9pbnZvaWNlTWFpbj48L0ludm9pY2VEYXRhPg==</invoiceData>
    <electronicInvoiceHash cryptoType='SHA3-512'>$$invoiceHashReplaceMe$$</electronicInvoiceHash>
  </invoiceOperation>
</invoiceOperations>");

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestInvoiceOperationsXmlFragment_CreditNote()
		{
			var model = CreateModelForTest(transactionType: TransactionType.CRD);

			var actualXml = GetRequestXmlBuilder(model).BuildInvoiceOperationsXml().ToString();
			var expectedXml = InvoiceOperationsXmlFragment_CreditNote_InvoiceData_ExpectedXml.Replace("'", "\"");

			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string InvoiceOperationsXmlFragment_CreditNote_InvoiceData_ExpectedXml => FormattableString.Invariant($@"<invoiceOperations xmlns='{Namespace}'>
  <compressedContent>false</compressedContent>
  <invoiceOperation>
    <index>1</index>
    <invoiceOperation>MODIFY</invoiceOperation>
    <invoiceData>PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz48SW52b2ljZURhdGEgeG1sbnM9Imh0dHA6Ly9zY2hlbWFzLm5hdi5nb3YuaHUvT1NBLzMuMC9kYXRhIiB4bWxuczpuczI9Imh0dHA6Ly9zY2hlbWFzLm5hdi5nb3YuaHUvT1NBLzMuMC9iYXNlIj48aW52b2ljZUlzc3VlRGF0ZT48L2ludm9pY2VJc3N1ZURhdGU+PGNvbXBsZXRlbmVzc0luZGljYXRvcj5mYWxzZTwvY29tcGxldGVuZXNzSW5kaWNhdG9yPjxpbnZvaWNlTWFpbj48aW52b2ljZT48aW52b2ljZUhlYWQ+PHN1cHBsaWVySW5mbz48c3VwcGxpZXJUYXhOdW1iZXI+PG5zMjp0YXhwYXllcklkPjc0MzI4OTc0PC9uczI6dGF4cGF5ZXJJZD48L3N1cHBsaWVyVGF4TnVtYmVyPjxncm91cE1lbWJlclRheE51bWJlcj48bnMyOnRheHBheWVySWQ+MTIwMzYwMjQ8L25zMjp0YXhwYXllcklkPjwvZ3JvdXBNZW1iZXJUYXhOdW1iZXI+PHN1cHBsaWVyQWRkcmVzcz48bnMyOnNpbXBsZUFkZHJlc3M+PG5zMjpjb3VudHJ5Q29kZSAvPjxuczI6cG9zdGFsQ29kZT4wMDAwPC9uczI6cG9zdGFsQ29kZT48bnMyOmFkZGl0aW9uYWxBZGRyZXNzRGV0YWlsPjwvbnMyOmFkZGl0aW9uYWxBZGRyZXNzRGV0YWlsPjwvbnMyOnNpbXBsZUFkZHJlc3M+PC9zdXBwbGllckFkZHJlc3M+PC9zdXBwbGllckluZm8+PGN1c3RvbWVySW5mbz48Y3VzdG9tZXJWYXRTdGF0dXM+RE9NRVNUSUM8L2N1c3RvbWVyVmF0U3RhdHVzPjxjdXN0b21lclZhdERhdGE+PGN1c3RvbWVyVGF4TnVtYmVyPjxuczI6dGF4cGF5ZXJJZD4xMjU2NTk4PC9uczI6dGF4cGF5ZXJJZD48Z3JvdXBNZW1iZXJUYXhOdW1iZXI+PG5zMjp0YXhwYXllcklkPjU3MjkzODc0PC9uczI6dGF4cGF5ZXJJZD48L2dyb3VwTWVtYmVyVGF4TnVtYmVyPjwvY3VzdG9tZXJUYXhOdW1iZXI+PC9jdXN0b21lclZhdERhdGE+PGN1c3RvbWVyQWRkcmVzcz48bnMyOnNpbXBsZUFkZHJlc3M+PG5zMjpwb3N0YWxDb2RlPjAwMDA8L25zMjpwb3N0YWxDb2RlPjxuczI6YWRkaXRpb25hbEFkZHJlc3NEZXRhaWw+PC9uczI6YWRkaXRpb25hbEFkZHJlc3NEZXRhaWw+PC9uczI6c2ltcGxlQWRkcmVzcz48L2N1c3RvbWVyQWRkcmVzcz48L2N1c3RvbWVySW5mbz48aW52b2ljZURldGFpbD48aW52b2ljZUNhdGVnb3J5PjwvaW52b2ljZUNhdGVnb3J5PjxpbnZvaWNlRGVsaXZlcnlEYXRlPjwvaW52b2ljZURlbGl2ZXJ5RGF0ZT48Y3VycmVuY3lDb2RlPjwvY3VycmVuY3lDb2RlPjxleGNoYW5nZVJhdGU+PC9leGNoYW5nZVJhdGU+PHBheW1lbnREYXRlPjwvcGF5bWVudERhdGU+PGludm9pY2VBcHBlYXJhbmNlPkVMRUNUUk9OSUM8L2ludm9pY2VBcHBlYXJhbmNlPjwvaW52b2ljZURldGFpbD48L2ludm9pY2VIZWFkPjxpbnZvaWNlTGluZXM+PG1lcmdlZEl0ZW1JbmRpY2F0b3I+ZmFsc2U8L21lcmdlZEl0ZW1JbmRpY2F0b3I+PC9pbnZvaWNlTGluZXM+PGludm9pY2VTdW1tYXJ5PjxzdW1tYXJ5Tm9ybWFsPjxpbnZvaWNlTmV0QW1vdW50PjAuMDA8L2ludm9pY2VOZXRBbW91bnQ+PGludm9pY2VOZXRBbW91bnRIVUY+MC4wMDwvaW52b2ljZU5ldEFtb3VudEhVRj48aW52b2ljZVZhdEFtb3VudD4wLjAwPC9pbnZvaWNlVmF0QW1vdW50PjxpbnZvaWNlVmF0QW1vdW50SFVGPjAuMDA8L2ludm9pY2VWYXRBbW91bnRIVUY+PC9zdW1tYXJ5Tm9ybWFsPjxzdW1tYXJ5R3Jvc3NEYXRhPjxpbnZvaWNlR3Jvc3NBbW91bnQ+PC9pbnZvaWNlR3Jvc3NBbW91bnQ+PGludm9pY2VHcm9zc0Ftb3VudEhVRj48L2ludm9pY2VHcm9zc0Ftb3VudEhVRj48L3N1bW1hcnlHcm9zc0RhdGE+PC9pbnZvaWNlU3VtbWFyeT48L2ludm9pY2U+PC9pbnZvaWNlTWFpbj48L0ludm9pY2VEYXRhPg==</invoiceData>
    <electronicInvoiceHash cryptoType='SHA3-512'>$$invoiceHashReplaceMe$$</electronicInvoiceHash>
  </invoiceOperation>
</invoiceOperations>");

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestInvoiceOperationsXmlFragment_AmendInvoice()
		{
			var previousInvoice = new[]
			{
				new PreviousInvoice() { AH_TransactionNum = "100", AH_TransactionType = "INV", WasSubmittedSuccessfully = true, MaximumLineSequence = 12 }
			};
			var model = CreateModelForTest(previousInvoices: previousInvoice);

			var actualXml = GetRequestXmlBuilder(model).BuildInvoiceOperationsXml().ToString();
			var expectedXml = InvoiceOperationsXmlFragment_AmendInvoice_InvoiceData_ExpectedXml.Replace("'", "\"");

			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string InvoiceOperationsXmlFragment_AmendInvoice_InvoiceData_ExpectedXml => FormattableString.Invariant($@"<invoiceOperations xmlns='{Namespace}'>
  <compressedContent>false</compressedContent>
  <invoiceOperation>
    <index>1</index>
    <invoiceOperation>MODIFY</invoiceOperation>
    <invoiceData>PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz48SW52b2ljZURhdGEgeG1sbnM9Imh0dHA6Ly9zY2hlbWFzLm5hdi5nb3YuaHUvT1NBLzMuMC9kYXRhIiB4bWxuczpuczI9Imh0dHA6Ly9zY2hlbWFzLm5hdi5nb3YuaHUvT1NBLzMuMC9iYXNlIj48aW52b2ljZUlzc3VlRGF0ZT48L2ludm9pY2VJc3N1ZURhdGU+PGNvbXBsZXRlbmVzc0luZGljYXRvcj5mYWxzZTwvY29tcGxldGVuZXNzSW5kaWNhdG9yPjxpbnZvaWNlTWFpbj48aW52b2ljZT48aW52b2ljZVJlZmVyZW5jZT48b3JpZ2luYWxJbnZvaWNlTnVtYmVyPjEwMDwvb3JpZ2luYWxJbnZvaWNlTnVtYmVyPjxtb2RpZnlXaXRob3V0TWFzdGVyPmZhbHNlPC9tb2RpZnlXaXRob3V0TWFzdGVyPjxtb2RpZmljYXRpb25JbmRleD4xPC9tb2RpZmljYXRpb25JbmRleD48L2ludm9pY2VSZWZlcmVuY2U+PGludm9pY2VIZWFkPjxzdXBwbGllckluZm8+PHN1cHBsaWVyVGF4TnVtYmVyPjxuczI6dGF4cGF5ZXJJZD43NDMyODk3NDwvbnMyOnRheHBheWVySWQ+PC9zdXBwbGllclRheE51bWJlcj48Z3JvdXBNZW1iZXJUYXhOdW1iZXI+PG5zMjp0YXhwYXllcklkPjEyMDM2MDI0PC9uczI6dGF4cGF5ZXJJZD48L2dyb3VwTWVtYmVyVGF4TnVtYmVyPjxzdXBwbGllckFkZHJlc3M+PG5zMjpzaW1wbGVBZGRyZXNzPjxuczI6Y291bnRyeUNvZGUgLz48bnMyOnBvc3RhbENvZGU+MDAwMDwvbnMyOnBvc3RhbENvZGU+PG5zMjphZGRpdGlvbmFsQWRkcmVzc0RldGFpbD48L25zMjphZGRpdGlvbmFsQWRkcmVzc0RldGFpbD48L25zMjpzaW1wbGVBZGRyZXNzPjwvc3VwcGxpZXJBZGRyZXNzPjwvc3VwcGxpZXJJbmZvPjxjdXN0b21lckluZm8+PGN1c3RvbWVyVmF0U3RhdHVzPkRPTUVTVElDPC9jdXN0b21lclZhdFN0YXR1cz48Y3VzdG9tZXJWYXREYXRhPjxjdXN0b21lclRheE51bWJlcj48bnMyOnRheHBheWVySWQ+MTI1NjU5ODwvbnMyOnRheHBheWVySWQ+PGdyb3VwTWVtYmVyVGF4TnVtYmVyPjxuczI6dGF4cGF5ZXJJZD41NzI5Mzg3NDwvbnMyOnRheHBheWVySWQ+PC9ncm91cE1lbWJlclRheE51bWJlcj48L2N1c3RvbWVyVGF4TnVtYmVyPjwvY3VzdG9tZXJWYXREYXRhPjxjdXN0b21lckFkZHJlc3M+PG5zMjpzaW1wbGVBZGRyZXNzPjxuczI6cG9zdGFsQ29kZT4wMDAwPC9uczI6cG9zdGFsQ29kZT48bnMyOmFkZGl0aW9uYWxBZGRyZXNzRGV0YWlsPjwvbnMyOmFkZGl0aW9uYWxBZGRyZXNzRGV0YWlsPjwvbnMyOnNpbXBsZUFkZHJlc3M+PC9jdXN0b21lckFkZHJlc3M+PC9jdXN0b21lckluZm8+PGludm9pY2VEZXRhaWw+PGludm9pY2VDYXRlZ29yeT48L2ludm9pY2VDYXRlZ29yeT48aW52b2ljZURlbGl2ZXJ5RGF0ZT48L2ludm9pY2VEZWxpdmVyeURhdGU+PGN1cnJlbmN5Q29kZT48L2N1cnJlbmN5Q29kZT48ZXhjaGFuZ2VSYXRlPjwvZXhjaGFuZ2VSYXRlPjxwYXltZW50RGF0ZT48L3BheW1lbnREYXRlPjxpbnZvaWNlQXBwZWFyYW5jZT5FTEVDVFJPTklDPC9pbnZvaWNlQXBwZWFyYW5jZT48L2ludm9pY2VEZXRhaWw+PC9pbnZvaWNlSGVhZD48aW52b2ljZUxpbmVzPjxtZXJnZWRJdGVtSW5kaWNhdG9yPmZhbHNlPC9tZXJnZWRJdGVtSW5kaWNhdG9yPjwvaW52b2ljZUxpbmVzPjxpbnZvaWNlU3VtbWFyeT48c3VtbWFyeU5vcm1hbD48aW52b2ljZU5ldEFtb3VudD4wLjAwPC9pbnZvaWNlTmV0QW1vdW50PjxpbnZvaWNlTmV0QW1vdW50SFVGPjAuMDA8L2ludm9pY2VOZXRBbW91bnRIVUY+PGludm9pY2VWYXRBbW91bnQ+MC4wMDwvaW52b2ljZVZhdEFtb3VudD48aW52b2ljZVZhdEFtb3VudEhVRj4wLjAwPC9pbnZvaWNlVmF0QW1vdW50SFVGPjwvc3VtbWFyeU5vcm1hbD48c3VtbWFyeUdyb3NzRGF0YT48aW52b2ljZUdyb3NzQW1vdW50PjwvaW52b2ljZUdyb3NzQW1vdW50PjxpbnZvaWNlR3Jvc3NBbW91bnRIVUY+PC9pbnZvaWNlR3Jvc3NBbW91bnRIVUY+PC9zdW1tYXJ5R3Jvc3NEYXRhPjwvaW52b2ljZVN1bW1hcnk+PC9pbnZvaWNlPjwvaW52b2ljZU1haW4+PC9JbnZvaWNlRGF0YT4=</invoiceData>
    <electronicInvoiceHash cryptoType='SHA3-512'>$$invoiceHashReplaceMe$$</electronicInvoiceHash>
  </invoiceOperation>
</invoiceOperations>");

		public void TestConstructorThrows()
		{
			var model = CreateModelForTest();
			AssertExceptionThrown<ArgumentNullException>("Exception should be thrown when model is null.", () => GetRequestXmlBuilder(null));
		}

		string Namespace => "http://schemas.nav.gov.hu/OSA/3.0/api";

		string HeaderNamespace => "http://schemas.nav.gov.hu/NTCA/1.0/common";

		ManageInvoiceRequestModel CreateModelForTest(string batchNumber = "0001",
			string companyCode = "DHU",
			string branchCode = "BHU",
			string loginId = "h9nupbpmgi8yhet",
			string passwordHash = "3581511529344BF49B6B93A44BDEDBE8AED53942D04F45D6BA0702E2E5F7A928667FB0C11CDEBA39A45D010FBD89C1F951CE9FF1B6789412780C70613CDAB56B",
			string signatureKey = "fb-8530-0ead7916e5432DA4ZSW1UKWX",
			string replacementKey = "8a042DA4ZSW17HY6",
			string softwareVersion = "20.4.30.9",
			string taxRegistrationNumber = "12036024",
			TransactionType transactionType = TransactionType.INV,
			IReadOnlyCollection<PreviousInvoice> previousInvoices = null)
		{
			var model = CreateAndPopulateTheModel(companyCode
				, branchCode
				, loginId
				, passwordHash
				, signatureKey
				, replacementKey
				, softwareVersion
				, taxRegistrationNumber
				, transactionType
				, previousInvoices
				, batchNumber);
			return model;
		}

		ManageInvoiceRequestXmlBuilder GetRequestXmlBuilder(ManageInvoiceRequestModel model)
		{
			return new ManageInvoiceRequestXmlBuilder(model);
		}

		ManageInvoiceRequestModel GetModel(TransactionInfo transactionInfo, HungaryTransactionExtraInfo transactionExtraInfo, string batchNumber)
		{
			return new ManageInvoiceRequestModel(transactionInfo, transactionExtraInfo, batchNumber, new NotificationBuffer());
		}

		ManageInvoiceRequestModel CreateAndPopulateTheModel(
			string companyCode,
			string branchCode,
			string loginId,
			string passwordHash,
			string signatureKey,
			string replacementKey,
			string softwareVersion,
			string taxRegistrationNumber,
			TransactionType transactionType,
			IReadOnlyCollection<PreviousInvoice> previousInvoices,
			string batchNumber)
		{
			var address1 = new UniversalDataBuss.DataObjects.Universal.OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			address1.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>()
			{
				new UniversalDataBuss.DataObjects.Universal.RegistrationNumber()
				{
					CountryOfIssue = new UniversalDataBuss.DataObjects.Universal.Country() { Code = "HU" },
					Type = new UniversalDataBuss.DataObjects.Universal.RegistrationNumberType() { Code = "VAT" },
					Value = taxRegistrationNumber
				},
				new UniversalDataBuss.DataObjects.Universal.RegistrationNumber()
				{
					CountryOfIssue = new UniversalDataBuss.DataObjects.Universal.Country() { Code = "HU" },
					Type = new UniversalDataBuss.DataObjects.Universal.RegistrationNumberType() { Code = "GBR" },
					Value = "743289745"
				}
			});
			var address2 = new UniversalDataBuss.DataObjects.Universal.OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			address2.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>()
			{
				new UniversalDataBuss.DataObjects.Universal.RegistrationNumber()
				{
					CountryOfIssue = new UniversalDataBuss.DataObjects.Universal.Country() { Code = "HU" },
					Type = new UniversalDataBuss.DataObjects.Universal.RegistrationNumberType() { Code = "VAT" },
					Value = "572938749",
				},
				new UniversalDataBuss.DataObjects.Universal.RegistrationNumber()
				{
					CountryOfIssue = new UniversalDataBuss.DataObjects.Universal.Country() { Code = "HU" },
					Type = new UniversalDataBuss.DataObjects.Universal.RegistrationNumberType() { Code = "GBR" },
					Value = "1256598"
				}
			});

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionType = transactionType,
				Branch = new Branch() { Code = branchCode, Name = branchCode + " Name" },
				BranchAddress = address1,
				OrganizationAddress = address2
			};

			var transactionExtraInfo = new HungaryTransactionExtraInfo();

			var model = GetModel(transactionInfo, transactionExtraInfo, batchNumber);

			model.SetData_ForTestOnly(
				companyCode: companyCode,
				branchCode: branchCode,
				loginId: loginId,
				passwordHash: passwordHash,
				signatureKey: signatureKey,
				replacementKey: replacementKey,
				softwareVersion: softwareVersion,
				previousInvoices: previousInvoices);

			return model;
		}
	}
}
