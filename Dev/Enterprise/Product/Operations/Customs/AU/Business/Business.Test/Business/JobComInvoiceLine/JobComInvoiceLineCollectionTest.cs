using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLineViewCollection))]
	public class JobComInvoiceLineCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionViewTestCase<JobComInvoiceLineViewCollection>
	{
		public void TestCreateContainersForLines()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLines = (JobComInvoiceLineViewCollection)invoice.InvoiceLines;

			var line1 = invoiceLines.AddNew();
			line1.AddInfo.ZA_AQISTempContainerNumber_Hidden = "MAEU9304711";
			var line2 = invoiceLines.AddNew();
			line2.AddInfo.ZA_AQISTempContainerNumber_Hidden = "MAEU9304712";
			invoiceLines.CreateContainersForLines();

			AssertEquals(2, declaration.AUCusContainers.Count);
			AssertNotNull("MAEU9304711", declaration.AUCusContainers.Find("MAEU9304711"));
			AssertNotNull("MAEU9304712", declaration.AUCusContainers.Find("MAEU9304712"));
		}

		public void TestSynchroniseQuarantineLineProcesses()
		{
			var aqisEstablishment = Factory.NewWithValidTestData<OrgHeader>();
			aqisEstablishment.OH_Code = "TAQS";
			aqisEstablishment.OH_FullName = "AQIS SYDNEY";
			var mainAddress = aqisEstablishment.MainAddress;
			mainAddress.Address1 = "185 O'RIORDAN ST";
			mainAddress.City = "MASCOT";
			mainAddress.State = "NSW";
			mainAddress.OA_RN_NKCountryCode = "AU";
			mainAddress.Postcode = "2020";
			var esnCusCode = mainAddress.CustomsCodes.AddNew();
			esnCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			esnCusCode.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber;
			esnCusCode.OK_CustomsRegNo = "77";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLines = (JobComInvoiceLineViewCollection)invoice.InvoiceLines;
			var line1 = invoiceLines.AddNew();
			AssertNull(line1.QuarantineExDocLine);
			AssertNoExceptionThrown("Does not blow up when dec is not quarantine", () => invoiceLines.SynchroniseQuarantineLineProcesses());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var quarantineLine = line1.QuarantineExDocLine;
			var process = Factory.New<QuarantineExDocEstablishmentAndTime>();
			process.EE_AuthorisationEstablishmentID = "77";
			quarantineLine.Processes.Add(process);
			AssertEquals(ZGuid.Empty, process.EE_E2_Address);
			AssertNoExceptionThrown(() => invoiceLines.SynchroniseQuarantineLineProcesses());
			AssertNotEquals(ZGuid.Empty, process.EE_E2_Address);
			AssertEquals("Creates a JobDocAddress that maps to the AQIS Establishment for code 77", aqisEstablishment.PK, process.Address.OrganisationPK);
		}

		public void TestUpdateInvoiceUQ_ListWhenSupplierChanged()
		{
			string uQ = "@#";
			Assert("Precondition : " + uQ + " is not in the list", !line1.JI_UQ_List.ContainsCode(uQ));
			var packConversion = Factory.New<CusRefPacks>();
			packConversion.RP_CommercialPack = "M2";
			packConversion.RP_CustomsPack = uQ;
			packConversion.RP_ConversionFactor = 1m;
			packConversion.RP_CustomsCountry = "AU";
			packConversion.RP_OH_Supplier = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			Factory.Save();

			Assert(uQ + " is not in the list as it is specific", !line1.JI_UQ_List.ContainsCode(uQ));

			header.JZ_OH_Supplier = packConversion.RP_OH_Supplier;
			Assert(uQ + " is in the list", line1.JI_UQ_List.ContainsCode(uQ));
		}

		public void TestLineNosDontUpdateWhenLoaded()
		{
			JobDeclaration jobDec = Factory.New<JobDeclaration>();

			JobComInvoiceHeader header = jobDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header.JZ_InvoiceNumber = "INV1";
			JobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 100m;
			JobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 50m;
			JobComInvoiceLine line3 = header.JobComInvoiceLines.AddNew();
			line3.JI_LinePrice = 200m;

			header.JobComInvoiceLines.Sort(JobComInvoiceLine.Schema.JI_LinePrice, ListSortDirection.Descending);

			AssertEquals("Line 1", (short)1, line1.JI_LineNo);
			AssertEquals("Line 2", (short)2, line2.JI_LineNo);
			AssertEquals("Line 3", (short)3, line3.JI_LineNo);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobComInvoiceHeader loadedHeader = newFactory.Load<JobComInvoiceHeader>(header.PK);
			foreach (JobComInvoiceLine invoiceLine in loadedHeader.JobComInvoiceLines)
			{
				if (invoiceLine.PK == line1.PK)
				{
					AssertEquals("Line1", (short)1, invoiceLine.JI_LineNo);
				}
				else if (invoiceLine.PK == line2.PK)
				{
					AssertEquals("Line2", (short)2, invoiceLine.JI_LineNo);
				}
				else if (invoiceLine.PK == line3.PK)
				{
					AssertEquals("Line3", (short)3, invoiceLine.JI_LineNo);
				}
			}
		}

		public void TestAddedInvoiceLineAddedToJobDeclarationInvoiceLines()
		{
			JobComInvoiceLine line4 = header.JobComInvoiceLines.AddNew();
			AssertEquals("JobDeclaration InvoiceLine should contain added invoice line", true, header.JobDeclaration.FilteredInvoiceLines.Contains(line4));
			AssertEquals("JobDeclaration InvoiceLine should contain added invoice line", true, header.JobDeclaration.InvoiceLines.Contains(line4));
		}

		public void TestAddNewAndAdjustLinePriceWithInvoiceHeader()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 201.34m;
			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNewAndAdjustLinePriceWithInvoiceHeader();

			AssertEquals("No InvoiceLine is added or more than are added.", 1, invoice.JobComInvoiceLines.Count);
			AssertEquals(201.34m, line.JI_LinePrice);
		}

		#region Implementation

		protected override JobComInvoiceLineViewCollection GetCollectionToTest()
		{
			return new JobComInvoiceLineViewCollection(InvoiceHeader, new InvoiceLineCompleteCollection(Declaration));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BaseJobComInvoiceLine result = Factory.New<BaseJobComInvoiceLine>();
			result.JI_JZ = InvoiceHeader.PK;
			return result;
		}

		JobDeclaration fDeclaration;
		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = JobDeclaration.New(Factory);
				}
				return fDeclaration;
			}
		}

		JobComInvoiceHeader fInvoiceHeader;
		JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				if (fInvoiceHeader == null)
				{
					fInvoiceHeader = Declaration.Invoices.AddNew();
				}
				return fInvoiceHeader;
			}
		}

		protected JobComInvoiceHeader header;
		protected JobComInvoiceLine line1;
		protected JobComInvoiceLine line2;
		protected JobComInvoiceLine line3;

		protected override void SetUp()
		{
			base.SetUp();
			JobDeclaration jobDec = Factory.New<JobDeclaration>();

			header = jobDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			line1 = header.JobComInvoiceLines.AddNew();
			line2 = header.JobComInvoiceLines.AddNew();
			line3 = header.JobComInvoiceLines.AddNew();
		}

		#endregion
	}
}
