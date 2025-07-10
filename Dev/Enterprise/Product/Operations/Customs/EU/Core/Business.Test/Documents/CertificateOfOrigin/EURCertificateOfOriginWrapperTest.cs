using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin.Testing
{
	public class EURCertificateOfOriginWrapperTest : CertificateOfOriginWrapperTest
	{
		[ExpectNoExceptions]
		public void TestOriginCountry()
		{
			var declaration = Factory.New<JobDeclaration>();
			var wrapper = GetNewWrapperFromDeclaration(declaration);
			NUnit.Framework.Assert.That(wrapper.OriginCountry, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "OriginCountry");

			declaration.JE_RL_NKOrigin = "AUSYD";
			wrapper = GetNewWrapperFromDeclaration(declaration);
			NUnit.Framework.Assert.That(wrapper.OriginCountry, NUnit.Framework.Is.EqualTo("Australia").Using(CustomComparers.TypeComparison), "OriginCountry");
		}

		[ExpectNoExceptions]
		public void TestOriginGroup()
		{
			var declaration = Factory.New<JobDeclaration>();
			var wrapper = GetNewWrapperFromDeclaration(declaration);
			NUnit.Framework.Assert.That(wrapper.OriginGroup, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "OriginGroup");

			declaration.JE_RL_NKOrigin = "AUSYD";
			wrapper = GetNewWrapperFromDeclaration(declaration);
			NUnit.Framework.Assert.That(wrapper.OriginGroup, NUnit.Framework.Is.EqualTo("Australia").Using(CustomComparers.TypeComparison), "OriginGroup");
		}

		[ExpectNoExceptions]
		public void TestDestinationCountry()
		{
			var declaration = Factory.New<JobDeclaration>();
			var wrapper = GetNewWrapperFromDeclaration(declaration);
			NUnit.Framework.Assert.That(wrapper.DestinationCountry, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "DestinationCountry");

			declaration.JE_RL_NKFinalDestination = "DEBER";
			wrapper = GetNewWrapperFromDeclaration(declaration);
			NUnit.Framework.Assert.That(wrapper.DestinationCountry, NUnit.Framework.Is.EqualTo("Germany").Using(CustomComparers.TypeComparison), "DestinationCountry");
		}

		[ExpectNoExceptions]
		public void TestDestinationGroup()
		{
			var zoneEUR1 = Factory.New<RefZoneHeader>();
			zoneEUR1.FZ_Code = "EUR1";
			zoneEUR1.FZ_Description = "Europe EUR1";

			var declaration = Factory.New<JobDeclaration>();
			var wrapper = GetNewWrapperFromDeclaration(declaration);
			NUnit.Framework.Assert.That(wrapper.DestinationGroup, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "DestinationGroup");

			declaration.JE_RL_NKFinalDestination = "DEBER";
			wrapper = GetNewWrapperFromDeclaration(declaration);
			NUnit.Framework.Assert.That(wrapper.DestinationGroup, NUnit.Framework.Is.EqualTo("Europe EUR1").Using(CustomComparers.TypeComparison), "DestinationGroup");
		}

		[ExpectNoExceptions]
		public void TestTransportDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			var wrapper = GetNewWrapperFromDeclaration(declaration);
			NUnit.Framework.Assert.That(wrapper.TransportDetail, NUnit.Framework.Is.TypeOf<TransportDetailWrapper>(), "TransportDetail Type");
		}

		[ExpectNoExceptions]
		public void TestGoodsSummary()
		{
			var declaration = Factory.New<JobDeclaration>();
			var wrapper = GetNewWrapperFromDeclaration(declaration);
			NUnit.Framework.Assert.That(wrapper.GoodsSummary, NUnit.Framework.Is.TypeOf<EURGoodsSummaryWrapper>(), "GoodsSummary Type");
		}

		[ExpectNoExceptions]
		public void TestCustomsEndorsement()
		{
			var declaration = Factory.New<JobDeclaration>();
			var wrapper = GetNewWrapperFromDeclaration(declaration);
			NUnit.Framework.Assert.That(wrapper.CustomsEndorsement, NUnit.Framework.Is.TypeOf<CustomsEndorsementWrapper>(), "CustomsEndorsement Type");
		}

		[ExpectNoExceptions]
		public void TestExporterDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var wrapper = GetNewWrapperFromDeclaration(declaration);
			NUnit.Framework.Assert.That(wrapper.ExporterDeclaration, NUnit.Framework.Is.TypeOf<ExporterDeclarationWrapper>(), "ExporterDeclaration Type");
		}

		[ExpectNoExceptions]
		public void TestInvoiceLinesWhenProviderIsCreatedFromJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var wrapper = new EURCertificateOfOriginWrapperForTest(declaration);
			NUnit.Framework.Assert.That(wrapper.InvoiceLinesExposed.Count(), NUnit.Framework.Is.EqualTo(0), "InvoiceLines Count");

			var invoice = declaration.Invoices.AddNew();
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader1.MergedLines.AddNew();
			invoice.InvoiceLines.AddNew().JI_CL = entryLine1.PK;
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine2 = entryHeader2.MergedLines.AddNew();
			invoice.InvoiceLines.AddNew().JI_CL = entryLine2.PK;
			invoice.InvoiceLines.AddNew().JI_CL = entryLine2.PK;
			NUnit.Framework.Assert.That(wrapper.InvoiceLinesExposed.Count(), NUnit.Framework.Is.EqualTo(3), "InvoiceLines Count");
		}

		[ExpectNoExceptions]
		public void TestInvoiceLinesWhenProviderIsCreatedFromCusEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader1.MergedLines.AddNew();
			invoice.InvoiceLines.AddNew().JI_CL = entryLine1.PK;
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = new EURCertificateOfOriginWrapperForTest(entryHeader2);
			NUnit.Framework.Assert.That(wrapper.InvoiceLinesExposed.Count(), NUnit.Framework.Is.EqualTo(0), "When entryHeader has no lines, InvoiceLines Count");

			var entryLine2 = entryHeader2.MergedLines.AddNew();
			invoice.InvoiceLines.AddNew().JI_CL = entryLine2.PK;
			invoice.InvoiceLines.AddNew().JI_CL = entryLine2.PK;
			entryHeader2.ResetInvoiceHeadersAndLines();
			NUnit.Framework.Assert.That(wrapper.InvoiceLinesExposed.Count(), NUnit.Framework.Is.EqualTo(2), "When entryHeader has lines, InvoiceLines Count");
		}

		[ExpectNoExceptions]
		public void TestUrl()
		{
			var declaration = Factory.New<JobDeclaration>();
			var wrapper = GetNewWrapperFromDeclaration(declaration);
			NUnit.Framework.Assert.That(wrapper.Url, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Url");
		}

		#region EURCertificateOfOriginWrapperForTest

		class EURCertificateOfOriginWrapperForTest : EURCertificateOfOriginWrapper
		{
			public EURCertificateOfOriginWrapperForTest(JobDeclaration declaration) : base(declaration)
			{
			}

			public EURCertificateOfOriginWrapperForTest(CusEntryHeader entryHeader) : base(entryHeader)
			{
			}

			public IEnumerable<JobComInvoiceLine> InvoiceLinesExposed => InvoiceLines;
		}

		#endregion
	}
}
