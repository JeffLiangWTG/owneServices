using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Core;
using Moq;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Panama.Testing
{
	public class PanamaEInvoicingAdditionalDataItemsProviderTest : TestCaseWithFactory
	{
		#region TransactionPivot

		public void TestTransactionPivotIsNull()
		{
			var batch = Factory.New<AccEInvoicingBatch>();

			var additionalDataItemsProvider = new PanamaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var additionalDataItems = additionalDataItemsProvider.GetAdditionalHeaderDataItems(batch, Factory.New<GlbBranch>(), new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), new Mock<ICountryEInvoicingObjectFactory>().Object, new Common.Logger());

			AssertEquals("Number Of Elements", 0, additionalDataItems.Count);
		}

		public void TestTransactionPivotHasOnePivot_WithDefaultValues()
		{
			var companyPK = Factory.New<GlbCompany>().PK;

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = companyPK;

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_GB = branch.PK;

			var batch = CreateInvoicePivotAndBatch(invoice.PK);

			AssertAllAdditionalDataItemsValues(batch, OrgConstants.Category.Business, "0", "0", OrgConstants.Category.Business);
		}

		public void TestTransactionPivotHasMoreThanOnePivot_MustTakeFirstOne()
		{
			var companyPK = Factory.New<GlbCompany>().PK;

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = companyPK;
			var branchProxy = CreateOrgHeader("YYY", ZGeography.CreatePoint(120.362445, 5.963247));
			branch.GB_OH_OrgProxy = branchProxy.PK;

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_GB = branch.PK;

			var debtor = CreateOrgHeader("ABC", null);
			invoice.AH_OH = debtor.PK;

			var batch = CreateInvoicePivotAndBatch(invoice.PK);

			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = companyPK;
			var branchProxy2 = CreateOrgHeader(null, ZGeography.CreatePoint(31.002504, -18.900253));
			branch2.GB_OH_OrgProxy = branchProxy2.PK;

			var invoice2 = Factory.New<ARInvoice>();
			invoice2.AH_GB = branch2.PK;

			var debtor2 = CreateOrgHeader("GHI", null);
			invoice2.AH_OH = debtor2.PK;

			AddPivotToBatch(batch, invoice2.PK);

			AssertAllAdditionalDataItemsValues(batch, "YYY", "120.362445", "5.963247", "ABC");
		}

		#endregion

		public void TestParentTransactionHeaderIsNull()
		{
			var companyPK = Factory.New<GlbCompany>().PK;

			var batch = Factory.New<AccEInvoicingBatch>();
			batch.AIB_GC = companyPK;

			var pivot = batch.TransactionPivots.AddNew();
			pivot.SetCompanyAndCountryCode(batch.Company);

			var additionalDataItemsProvider = new PanamaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var additionalDataItems = additionalDataItemsProvider.GetAdditionalHeaderDataItems(batch, Factory.New<GlbBranch>(), new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), new Mock<ICountryEInvoicingObjectFactory>().Object, new Common.Logger());

			AssertEquals("Number Of Elements", 0, additionalDataItems.Count);
		}

		#region OrgProxyData

		public void TestGetCorrectOrgProxyData_WhenOrgProxyFromCompanyAndBranchAreNull()
		{
			var company = Factory.New<GlbCompany>();

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_GB = branch.PK;

			var batch = CreateInvoicePivotAndBatch(invoice.PK);

			AssertNull("PreCondition: company.OrgProxy should be null", company.OrgProxy);
			AssertNull("PreCondition: branch.OrgProxy should be null", branch.OrgProxy);
			AssertOrgProxyData(batch, OrgConstants.Category.Business, "0", "0");
		}

		public void TestGetCorrectOrgProxyData()
		{
			var company = Factory.New<GlbCompany>();
			var companyProxy = CreateOrgHeader("XXX", ZGeography.CreatePoint(-79.534364, 8.953966));
			company.GC_OH_OrgProxy = companyProxy.PK;

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			var branchProxy = CreateOrgHeader("WXY", ZGeography.CreatePoint(9.0630214987512, 33.2569810012069));
			branch.GB_OH_OrgProxy = branchProxy.PK;

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_GB = branch.PK;

			var batch = CreateInvoicePivotAndBatch(invoice.PK);

			AssertOrgProxyData(batch, "WXY", "9.0630214987512", "33.2569810012069");

			branch.GB_OH_OrgProxy = ZGuid.Empty;

			AssertNull("PreCondition: branch.OrgProxy should be null", branch.OrgProxy);
			AssertOrgProxyData(batch, "XXX", "-79.534364", "8.953966");
		}

		#endregion

		#region dTipoRucEmisor

		public void TestdTipoRucEmisor_CategoryIsNull()
		{
			var companyPK = Factory.New<GlbCompany>().PK;

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = companyPK;
			var branchProxy = CreateOrgHeader(null, ZGeography.Empty);
			branch.GB_OH_OrgProxy = branchProxy.PK;

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_GB = branch.PK;

			var batch = CreateInvoicePivotAndBatch(invoice.PK);

			var additionalDataItemsProvider = new PanamaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var additionalDataItems = additionalDataItemsProvider.GetAdditionalHeaderDataItems(batch, Factory.New<GlbBranch>(), new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), new Mock<ICountryEInvoicingObjectFactory>().Object, new Common.Logger());

			AssertEquals("PreCondition: Number Of Elements", 4, additionalDataItems.Count);
			AssertEquals("dTipoRucEmisor Key", "dTipoRucEmisor", additionalDataItems[0].Key);
			AssertEquals("dTipoRucEmisor Value", OrgConstants.Category.Business, additionalDataItems[0].Value);
		}

		public void TestdTipoRucEmisor_CategoryIsEmpty()
		{
			var companyPK = Factory.New<GlbCompany>().PK;

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = companyPK;
			var branchProxy = CreateOrgHeader(ZString.Empty, ZGeography.Empty);
			branch.GB_OH_OrgProxy = branchProxy.PK;

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_GB = branch.PK;

			var batch = CreateInvoicePivotAndBatch(invoice.PK);

			var additionalDataItemsProvider = new PanamaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var additionalDataItems = additionalDataItemsProvider.GetAdditionalHeaderDataItems(batch, Factory.New<GlbBranch>(), new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), new Mock<ICountryEInvoicingObjectFactory>().Object, new Common.Logger());

			AssertEquals("PreCondition: Number Of Elements", 4, additionalDataItems.Count);
			AssertEquals("dTipoRucEmisor Key", "dTipoRucEmisor", additionalDataItems[0].Key);
			AssertEquals("dTipoRucEmisor Value", OrgConstants.Category.Business, additionalDataItems[0].Value);
		}

		public void TestdTipoRucEmisor_CategoryIsNotEmpty()
		{
			var companyPK = Factory.New<GlbCompany>().PK;

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = companyPK;
			var branchProxy = CreateOrgHeader("GOV", ZGeography.Empty);
			branch.GB_OH_OrgProxy = branchProxy.PK;

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_GB = branch.PK;

			var batch = CreateInvoicePivotAndBatch(invoice.PK);

			var additionalDataItemsProvider = new PanamaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var additionalDataItems = additionalDataItemsProvider.GetAdditionalHeaderDataItems(batch, Factory.New<GlbBranch>(), new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), new Mock<ICountryEInvoicingObjectFactory>().Object, new Common.Logger());

			AssertEquals("PreCondition: Number Of Elements", 4, additionalDataItems.Count);
			AssertEquals("dTipoRucEmisor Key", "dTipoRucEmisor", additionalDataItems[0].Key);
			AssertEquals("dTipoRucEmisor Value", "GOV", additionalDataItems[0].Value);
		}

		#endregion

		#region GPS Latitude and Longitude

		public void TestLatitudeAndLongitude_GeoLocationIsEmpty()
		{
			var companyPK = Factory.New<GlbCompany>().PK;

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = companyPK;
			var branchProxy = Factory.New<OrgHeader>();
			branch.GB_OH_OrgProxy = branchProxy.PK;

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_GB = branch.PK;

			var batch = CreateInvoicePivotAndBatch(invoice.PK);

			AssertGPSLongitudeAndLatitude(batch, branch, "0", "0");
		}

		public void TestLatitudeAndLongitude_GeoLocationIsNotEmpty()
		{
			var companyPK = Factory.New<GlbCompany>().PK;

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = companyPK;
			var branchProxy = CreateOrgHeader(null, ZGeography.CreatePoint(25.7852121354789, -56.6133241249795));
			branch.GB_OH_OrgProxy = branchProxy.PK;

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_GB = branch.PK;

			var batch = CreateInvoicePivotAndBatch(invoice.PK);

			AssertGPSLongitudeAndLatitude(batch, branch, "25.7852121354789", "-56.6133241249795");

			branchProxy.MainAddress.GeoLocation = ZGeography.CreatePoint(-151.18546, 86.29591);
			AssertGPSLongitudeAndLatitude(batch, branch, "-151.18546", "86.29591");
		}

		public void TestLatitudeAndLongitude_UseCorrectIFormatProvider()
		{
			AssertLatitudeAndLongitude_UseCorrectIFormatProvider("AU", ".");
			AssertLatitudeAndLongitude_UseCorrectIFormatProvider("FR", ",");

			void AssertLatitudeAndLongitude_UseCorrectIFormatProvider(string country, string decimalSeparator)
			{
				using (Culture.SetTemporarily(Culture.GetCulture(country)))
				{
					var companyPK = Factory.New<GlbCompany>().PK;

					var branch = Factory.New<GlbBranch>();
					branch.GB_GC = companyPK;
					var branchProxy = CreateOrgHeader(null, ZGeography.CreatePoint(48.858093, 2.294694));
					branch.GB_OH_OrgProxy = branchProxy.PK;

					var geoLocation = branchProxy.MainAddress.OA_GeoLocation;
					var latitude = geoLocation.Latitude.GetValueOrDefault();
					var longitude = geoLocation.Longitude.GetValueOrDefault();

					AssertEquals($"PreCondicion: GeoLocation latitude should be use '{decimalSeparator}' as decimal separator.", $"2{decimalSeparator}294694", latitude.ToString());
					AssertEquals($"PreCondicion: GeoLocation longitude should be use '{decimalSeparator}' as decimal separator.", $"48{decimalSeparator}858093", longitude.ToString());

					var invoice = Factory.New<ARInvoice>();
					invoice.AH_GB = branch.PK;

					var batch = CreateInvoicePivotAndBatch(invoice.PK);
					AssertGPSLongitudeAndLatitude(batch, branch, "48.858093", "2.294694");
				}
			}
		}

		#endregion

		#region dTipoRucReceptor

		public void TestdTipoRucReceptor_DebtorIsMissing()
		{
			var companyPK = Factory.New<GlbCompany>().PK;

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = companyPK;

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_GB = branch.PK;

			var batch = CreateInvoicePivotAndBatch(invoice.PK);

			var additionalDataItemsProvider = new PanamaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var additionalDataItems = additionalDataItemsProvider.GetAdditionalHeaderDataItems(batch, Factory.New<GlbBranch>(), new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), new Mock<ICountryEInvoicingObjectFactory>().Object, new Common.Logger());

			AssertEquals("PreCondition: Number Of Elements", 4, additionalDataItems.Count);
			AssertEquals("dTipoRucReceptor Key", "dTipoRucReceptor", additionalDataItems[3].Key);
			AssertEquals("dTipoRucReceptor Value", OrgConstants.Category.Business, additionalDataItems[3].Value);
		}

		public void TestdTipoRucReceptor_CategoryIsNull()
		{
			var companyPK = Factory.New<GlbCompany>().PK;

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = companyPK;

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_GB = branch.PK;
			invoice.AH_OH = Factory.New<OrgHeader>().PK;

			var batch = CreateInvoicePivotAndBatch(invoice.PK);

			var additionalDataItemsProvider = new PanamaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var additionalDataItems = additionalDataItemsProvider.GetAdditionalHeaderDataItems(batch, Factory.New<GlbBranch>(), new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), new Mock<ICountryEInvoicingObjectFactory>().Object, new Common.Logger());

			AssertEquals("PreCondition: Number Of Elements", 4, additionalDataItems.Count);
			AssertEquals("dTipoRucReceptor Key", "dTipoRucReceptor", additionalDataItems[3].Key);
			AssertEquals("dTipoRucReceptor Value", OrgConstants.Category.Business, additionalDataItems[3].Value);
		}

		public void TestdTipoRucReceptor_CategoryIsEmpty()
		{
			var companyPK = Factory.New<GlbCompany>().PK;

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = companyPK;

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_GB = branch.PK;

			var debtor = CreateOrgHeader(ZString.Empty, null);
			invoice.AH_OH = debtor.PK;

			var batch = CreateInvoicePivotAndBatch(invoice.PK);

			var additionalDataItemsProvider = new PanamaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var additionalDataItems = additionalDataItemsProvider.GetAdditionalHeaderDataItems(batch, Factory.New<GlbBranch>(), new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), new Mock<ICountryEInvoicingObjectFactory>().Object, new Common.Logger());

			AssertEquals("PreCondition: Number Of Elements", 4, additionalDataItems.Count);
			AssertEquals("dTipoRucReceptor Key", "dTipoRucReceptor", additionalDataItems[3].Key);
			AssertEquals("dTipoRucReceptor Value", OrgConstants.Category.Business, additionalDataItems[3].Value);
		}

		public void TestdTipoRucReceptor_CategoryIsNotNull()
		{
			var companyPK = Factory.New<GlbCompany>().PK;

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = companyPK;

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_GB = branch.PK;

			var debtor = CreateOrgHeader("GHI", null);
			invoice.AH_OH = debtor.PK;

			var batch = CreateInvoicePivotAndBatch(invoice.PK);

			var additionalDataItemsProvider = new PanamaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var additionalDataItems = additionalDataItemsProvider.GetAdditionalHeaderDataItems(batch, Factory.New<GlbBranch>(), new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), new Mock<ICountryEInvoicingObjectFactory>().Object, new Common.Logger());

			AssertEquals("PreCondition: Number Of Elements", 4, additionalDataItems.Count);
			AssertEquals("dTipoRucReceptor Key", "dTipoRucReceptor", additionalDataItems[3].Key);
			AssertEquals("dTipoRucReceptor Value", "GHI", additionalDataItems[3].Value);
		}

		#endregion

		#region dPtoFacDF and dNroDF

		public void TestdPtoFacDF_And_dNroDF_TransactionReference_IsEmpty()
		{
			var companyPK = Factory.New<GlbCompany>().PK;
			var invoice = Factory.New<ARInvoice>();
			var batch = CreateInvoicePivotAndBatch(invoice.PK);
			var additionalDataItemsProvider = new PanamaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var additionalDataItems = additionalDataItemsProvider.GetAdditionalHeaderDataItems(batch, Factory.New<GlbBranch>(), new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), new Mock<ICountryEInvoicingObjectFactory>().Object, new Common.Logger());

			AssertEquals("Does Not Exists dPtoFacDF key", expected: false, additionalDataItems.ToList<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem>().Any(s => s.Key == "dPtoFacDF"));
			AssertEquals("Does Not Exists dNroDF key", expected: false, additionalDataItems.ToList<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem>().Any(s => s.Key == "dNroDF"));
		}

		public void TestdPtoFacDF_And_dNroDF_TransactionReference_Prefix_LessThanThreeCharacters_ComplianceBook_Null()
		{
			var arInvoice = CreateARInvoice("66");
			var additionalDataItems = GetAdditionalHeaderDataItemsForInvoice(arInvoice.PK);

			AssertEquals("Does Not Exists dPtoFacDF key", expected: false, additionalDataItems.ToList<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem>().Any(s => s.Key == "dPtoFacDF"));
			AssertEquals("Does Not Exists dNroDF key", expected: false, additionalDataItems.ToList<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem>().Any(s => s.Key == "dNroDF"));
		}

		public void TestdPtoFacDF_TransactionReference_NotEmpty_ComplianceBook_NotNull()
		{
			var sequenceBook = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequenceBook.XD_Code = "ABC";
			sequenceBook.XD_Prefix = "AAA";

			var arInvoice = CreateARInvoice("AAA15159878", sequenceBook);
			var additionalDataItems = GetAdditionalHeaderDataItemsForInvoice(arInvoice.PK);

			AssertEquals("dPtoFacDF Key", "dPtoFacDF", additionalDataItems[4].Key);
			AssertEquals("dPtoFacDF Value", "AAA", additionalDataItems[4].Value);
		}

		public void TestdNroDF_TransactionReference_NotEmpty_ComplianceBook_NotNull()
		{
			var sequenceBook = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequenceBook.XD_Code = "ABC";
			sequenceBook.XD_Prefix = "AAA";

			var arInvoice = CreateARInvoice("AAA15159878", sequenceBook);
			var additionalDataItems = GetAdditionalHeaderDataItemsForInvoice(arInvoice.PK);

			AssertEquals("dNroDF Key", "dNroDF", additionalDataItems[5].Key);
			AssertEquals("dNroDF Value", "15159878", additionalDataItems[5].Value);
		}

		public void TestdPtoFacDF_TransactionReference_Prefix_MoreThanThreeCharacters_ComplianceBook_Null()
		{
			var arInvoice = CreateARInvoice("12234");
			var additionalDataItems = GetAdditionalHeaderDataItemsForInvoice(arInvoice.PK);

			AssertEquals("dPtoFacDF Key", "dPtoFacDF", additionalDataItems[4].Key);
			AssertEquals("dPtoFacDF Value", "122", additionalDataItems[4].Value);
		}

		public void TestdNroDF_TransactionReference_Prefix_MoreThanThreeCharacters_ComplianceBook_Null()
		{
			var arInvoice = CreateARInvoice("12234");
			var additionalDataItems = GetAdditionalHeaderDataItemsForInvoice(arInvoice.PK);

			AssertEquals("dNroDF Key", "dNroDF", additionalDataItems[5].Key);
			AssertEquals("dNroDF Value", "34", additionalDataItems[5].Value);
		}

		public void TestdPtoFacDF_TransactionReference_Prefix_EqualThreeCharacters_ComplianceBook_Null()
		{
			var arInvoice = CreateARInvoice("123");
			var additionalDataItems = GetAdditionalHeaderDataItemsForInvoice(arInvoice.PK);

			AssertEquals("dPtoFacDF Key", "dPtoFacDF", additionalDataItems[4].Key);
			AssertEquals("dPtoFacDF Value", "123", additionalDataItems[4].Value);
		}

		public void TestdNroDF_TransactionReference_Prefix_EqualThreeCharacters_ComplianceBook_Null()
		{
			var arInvoice = CreateARInvoice("123");
			var additionalDataItems = GetAdditionalHeaderDataItemsForInvoice(arInvoice.PK);

			AssertEquals("dNroDF Key", "dNroDF", additionalDataItems[5].Key);
			AssertEquals("dNroDF Value", "0", additionalDataItems[5].Value);
		}

		#endregion

		#region InvoiceTransactionReference

		public void TestInvoiceTransactionReference_HasCorrectKeyAndValue()
		{
			var (expectedKey, expectedValue) = ("InvoiceTransactionReference", "12345");
			var invoice = Factory.New<ARInvoice>();

			var transactionHeaderReference = Factory.New<AccTransactionHeaderReference>();
			transactionHeaderReference.AH1_Type = AccTransactionHeaderReferenceTypes.ITR;
			transactionHeaderReference.AH1_AH = invoice.PK;
			transactionHeaderReference.AH1_Reference = expectedValue;

			var batch = CreateInvoicePivotAndBatch(invoice.PK);
			var additionalDataItemsProvider = new PanamaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var additionalDataItems = additionalDataItemsProvider
				.GetAdditionalHeaderDataItems(batch, Factory.New<GlbBranch>(), new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), new Mock<ICountryEInvoicingObjectFactory>().Object, new Common.Logger())
				.ToList<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem>();

			AssertEquals("Precondition: InvoiceTransactionReference", expectedValue, invoice.InvoiceTransactionReference);

			var invoiceTransactionReferenceItems = additionalDataItems.Where(x => x.Key == expectedKey).ToArray();
			AssertEquals("Does InvoiceTransactionReference key exist?", 1, invoiceTransactionReferenceItems.Length);
			AssertEquals("InvoiceTransactionReference value", expectedValue, invoiceTransactionReferenceItems[0].Value);
		}

		#endregion InvoiceTransactionReference

		#region Implementation 

		void AssertAllAdditionalDataItemsValues(AccEInvoicingBatch batch, string expecteddTipoRucEmisor, string expecteddCoordEmLongitude, string expecteddCoordEmLatitude, string expecteddTipoRucReceptor)
		{
			var additionalDataItemsProvider = new PanamaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var additionalDataItems = additionalDataItemsProvider.GetAdditionalHeaderDataItems(batch, Factory.New<GlbBranch>(), new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), new Mock<ICountryEInvoicingObjectFactory>().Object, new Common.Logger());

			AssertEquals("PreCondicion: Number Of Elements", 4, additionalDataItems.Count);

			AssertEquals("dTipoRucEmisor Key", "dTipoRucEmisor", additionalDataItems[0].Key);
			AssertEquals("dTipoRucEmisor Value", expecteddTipoRucEmisor, additionalDataItems[0].Value);

			AssertEquals("dCoordEmLongitude Key", "dCoordEmLongitude", additionalDataItems[1].Key);
			AssertEquals("dCoordEmLongitude Value", expecteddCoordEmLongitude, additionalDataItems[1].Value);

			AssertEquals("dCoordEmLatitude Key", "dCoordEmLatitude", additionalDataItems[2].Key);
			AssertEquals("dCoordEmLatitude Value", expecteddCoordEmLatitude, additionalDataItems[2].Value);

			AssertEquals("dTipoRucReceptor Key", "dTipoRucReceptor", additionalDataItems[3].Key);
			AssertEquals("dTipoRucReceptor Value", expecteddTipoRucReceptor, additionalDataItems[3].Value);
		}

		GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection GetAdditionalHeaderDataItemsForInvoice(ZGuid invoicePK)
		{
			var batch = CreateInvoicePivotAndBatch(invoicePK);
			var additionalDataItemsProvider = new PanamaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var additionalDataItems = additionalDataItemsProvider.GetAdditionalHeaderDataItems(batch, Factory.New<GlbBranch>(), new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), new Mock<ICountryEInvoicingObjectFactory>().Object, new Common.Logger());

			return additionalDataItems;
		}

		ARInvoice CreateARInvoice(ZString transactionReference, AccComplianceSequence accComplianceSequence = null)
		{
			var invoice = Factory.New<ARInvoice>();

			invoice.AH_TransactionReference = transactionReference;

			if (accComplianceSequence != null)
			{
				invoice.AH_XD_ComplianceBook = accComplianceSequence.PK;
			}

			return invoice;
		}

		void AssertOrgProxyData(AccEInvoicingBatch batch, string expecteddTipoRucEmisor, string expecteddCoordEmLongitude, string expecteddCoordEmLatitude)
		{
			var additionalDataItemsProvider = new PanamaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var additionalDataItems = additionalDataItemsProvider.GetAdditionalHeaderDataItems(batch, Factory.New<GlbBranch>(), new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), new Mock<ICountryEInvoicingObjectFactory>().Object, new Common.Logger());

			AssertEquals("PreCondition: Number Of Elements", 4, additionalDataItems.Count);

			AssertEquals("dTipoRucEmisor Key", "dTipoRucEmisor", additionalDataItems[0].Key);
			AssertEquals("dTipoRucEmisor Value", expecteddTipoRucEmisor, additionalDataItems[0].Value);

			AssertEquals("dCoordEmLongitude Key", "dCoordEmLongitude", additionalDataItems[1].Key);
			AssertEquals("dCoordEmLongitude Value", expecteddCoordEmLongitude, additionalDataItems[1].Value);

			AssertEquals("dCoordEmLatitude Key", "dCoordEmLatitude", additionalDataItems[2].Key);
			AssertEquals("dCoordEmLatitude Value", expecteddCoordEmLatitude, additionalDataItems[2].Value);
		}

		void AssertGPSLongitudeAndLatitude(AccEInvoicingBatch batch, GlbBranch branch, string expectedLongitude, string expectedLatitude)
		{
			var additionalDataItemsProvider = new PanamaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var additionalDataItems = additionalDataItemsProvider.GetAdditionalHeaderDataItems(batch, Factory.New<GlbBranch>(), new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), new Mock<ICountryEInvoicingObjectFactory>().Object, new Common.Logger());

			AssertEquals("Number Of Elements", 4, additionalDataItems.Count);

			AssertEquals("dCoordEmLongitude Key", "dCoordEmLongitude", additionalDataItems[1].Key);
			AssertEquals("dCoordEmLongitude Value", expectedLongitude, additionalDataItems[1].Value);

			AssertEquals("dCoordEmLatitude Key", "dCoordEmLatitude", additionalDataItems[2].Key);
			AssertEquals("dCoordEmLatitude Value", expectedLatitude, additionalDataItems[2].Value);
		}

		OrgHeader CreateOrgHeader(ZString category, ZGeography? point)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Category = category;
			orgHeader.MainAddress.OA_GeoLocation = point ?? ZGeography.Empty;

			return orgHeader;
		}

		AccEInvoicingBatch CreateInvoicePivotAndBatch(ZGuid invoicePk)
		{
			var batch = Factory.New<AccEInvoicingBatch>();
			batch.AIB_GC = GlbCompany.CurrentCompany.PK;

			AddPivotToBatch(batch, invoicePk);

			return batch;
		}

		void AddPivotToBatch(AccEInvoicingBatch batch, ZGuid invoicePk)
		{
			var pivot = batch.TransactionPivots.AddNew();
			pivot.AIP_ParentID = invoicePk;
			pivot.SetCompanyAndCountryCode(batch.Company);
		}

		#endregion
	}
}
