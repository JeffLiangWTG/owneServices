using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocJobComInvoiceHeader))]
	sealed class DocJobComInvoiceHeaderTest : DocBaseJobComInvoiceHeaderAbstractTest<JobComInvoiceHeader, DocJobComInvoiceHeader>
	{
		#region Overrides

		public override void TestTariffHeading()
		{
			AssertEquals("Tariff Heading", "Tariff Stat / Trt / Concession Type and Number", InvoiceHeaderWrapper.TariffHeading);
		}

		public override void TestIncoTermDescription()  // CMR
		{
			Declaration.JE_ApplicationCode = "CMR";
			Declaration.JE_MessageType = "IMP";

			InvoiceHeader.JZ_IncoTerm = "FOB";
			AssertEquals("Inco term should be 'Free On Board'", "Free On Board", InvoiceHeaderWrapper.IncoTermDescription);

			InvoiceHeader.JZ_IncoTerm = "EXW";
			AssertEquals("Inco term should be 'Ex Works'", "Ex Works", InvoiceHeaderWrapper.IncoTermDescription);
		}

		public void TestIncoTermDescription_Legacy()
		{
			Declaration.JE_ApplicationCode = "LEG";
			Declaration.JE_MessageType = "IMP";

			InvoiceHeader.JZ_IncoTerm = "C&I";
			AssertEquals("Inco term should be 'Cost & Insurance Only'", "Cost and insurance", InvoiceHeaderWrapper.IncoTermDescription);

			InvoiceHeader.JZ_IncoTerm = "FOB";
			AssertEquals("Inco term should be 'Free On Board at Port'", "Packed free on board at port, airport or container yard", InvoiceHeaderWrapper.IncoTermDescription);

			InvoiceHeader.JZ_IncoTerm = "CIF";
			AssertEquals("Inco term should be 'Cost, Insurance & Freight'", "Cost, insurance and non-dutiable freight", InvoiceHeaderWrapper.IncoTermDescription);

			InvoiceHeader.JZ_IncoTerm = "PAF";
			AssertEquals("Inco term should be 'Packed At Factory'", "Packed at factory", InvoiceHeaderWrapper.IncoTermDescription);

			InvoiceHeader.JZ_IncoTerm = "UAF";
			AssertEquals("Inco term should be 'Unpacked At Factory'", "Unpacked at factory", InvoiceHeaderWrapper.IncoTermDescription);

			InvoiceHeader.JZ_IncoTerm = "UCF";
			AssertEquals("Inco term should be 'Unpacked Cost & Freight'", "Unpacked cost and non-dutiable freight", InvoiceHeaderWrapper.IncoTermDescription);

			InvoiceHeader.JZ_IncoTerm = "UCI";
			AssertEquals("Inco term should be 'Unpacked Cost, Insurance & Freight'", "Unpacked cost, insurance and non-dutiable freight", InvoiceHeaderWrapper.IncoTermDescription);

			InvoiceHeader.JZ_IncoTerm = "UFB";
			AssertEquals("Inco term should be 'Unpacked Free On Board'", "Unpacked free on board at port, airport or container yard", InvoiceHeaderWrapper.IncoTermDescription);
		}

		#endregion

		#region ZDecimal Fields
		public override void TestConversionFactorIsWrapped()
		{
			InvoiceHeaderInternal.JZ_InvoiceAmount = 1000m;
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals(1m, InvoiceHeaderWrapperInternal.ConversionFactor);
		}
		#endregion

		#region ZString Fields

		public void TestValuationBasis()
		{
			InvoiceHeader.JZ_ValuationBasis = "VVV";
			AssertEquals("ValuationBasis", InvoiceHeader.JZ_ValuationBasis, InvoiceHeaderWrapper.ValuationBasis);
		}

		public void TestNature10UnitPack()
		{
			AssertEquals("Nature10UnitPack", InvoiceHeader.JZ_Nature10UnitPack, InvoiceHeaderWrapper.Nature10UnitPack);
		}

		public void TestBondUnitPack()
		{
			AssertEquals("BondUnitPack", InvoiceHeader.JZ_BondUnitPack, InvoiceHeaderWrapper.BondUnitPack);
		}

		public void TestCommissionType()
		{
			InvoiceHeader.JZ_CommissionType = "C";
			AssertEquals("CommissionType", InvoiceHeader.JZ_CommissionType, InvoiceHeaderWrapper.CommissionType);
		}

		public void TestDrawbackEDN()
		{
			Declaration.JE_MessageType = Declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Drawback;
			Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			InvoiceHeader.AddInfo.ZA_EDN_Hidden = "HEDN";
			AssertEquals("DrawbackEDN", "HEDN", InvoiceHeaderWrapper.DrawbackEDN);
		}

		#endregion

		#region ZInt Fields

		public void TestNature10PackCount()
		{
			InvoiceHeader.JZ_Nature10PackCount = 12;
			AssertEquals("Nature10PackCount", InvoiceHeader.JZ_Nature10PackCount, InvoiceHeaderWrapper.Nature10PackCount);
		}

		public void TestPiecesForRelease()
		{
			InvoiceHeader.JZ_PiecesForRelease = 12;
			AssertEquals("PiecesForRelease", InvoiceHeader.JZ_PiecesForRelease, InvoiceHeaderWrapper.PiecesForRelease);
		}

		public void TestPiecesToBond()
		{
			InvoiceHeader.JZ_PiecesToBond = 12;
			AssertEquals("PiecesToBond", InvoiceHeader.JZ_PiecesToBond, InvoiceHeaderWrapper.PiecesToBond);
		}

		public void TestBondPackCount()
		{
			InvoiceHeader.JZ_BondPackCount = 12;
			AssertEquals("BondPackCount", InvoiceHeader.JZ_BondPackCount, InvoiceHeaderWrapper.BondPackCount);
		}

		#endregion

		#region Implementation

		JobDeclaration Declaration;
		JobComInvoiceHeader InvoiceHeader;
		DocJobComInvoiceHeader InvoiceHeaderWrapper;

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.Australia; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			InvoiceHeader = InvoiceHeaderInternal;
			InvoiceHeaderWrapper = InvoiceHeaderWrapperInternal;
			Declaration = InvoiceHeader.JobDeclaration;
		}

		protected override DocJobComInvoiceHeader CreateInvoiceHeaderWrapper(JobComInvoiceHeader invoiceHeaderInternal)
		{
			return DocJobComInvoiceHeader.New(invoiceHeaderInternal, Factory);
		}

		#endregion
	}
}
