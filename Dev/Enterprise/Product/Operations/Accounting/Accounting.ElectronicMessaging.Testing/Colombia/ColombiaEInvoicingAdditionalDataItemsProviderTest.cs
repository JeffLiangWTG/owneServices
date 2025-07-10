using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Moq;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Colombia.Testing
{
	public class ColombiaEInvoicingAdditionalDataItemsProviderTest : TestCaseWithFactory
	{
		#region TipoEmisor

		public void TestTipoEmisor_WhenCompanyOrgProxy_And_BranchOrgProxy_AreNull()
		{
			var batch = CreateInvoicePivotAndBatch();
			var company = Factory.New<GlbCompany>();
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;

			AssertNull("PreCondition: branch.OrgProxy should be null", branch.OrgProxy);
			AssertNull("PreCondition: company.OrgProxy should be null", company.OrgProxy);

			var additionalDataItems = CreateAdditionalDataItems(batch, branch);

			AssertCollectionContains(additionalDataItems, x => x.Key == "TipoEmisor" && x.Value == OrgConstants.Category.Business);
		}

		public void TestTipoEmisor_WhenCompanyOrgProxyCategory_IsEmpty()
		{
			var batch = CreateInvoicePivotAndBatch();

			var company = Factory.New<GlbCompany>();
			company.GC_OH_OrgProxy = CreateOrgHeader(ZString.Empty).PK;

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_OH_OrgProxy = ZGuid.Empty;

			AssertNull("PreCondition: branch.OrgProxy should be null", branch.OrgProxy);

			var additionalDataItems = CreateAdditionalDataItems(batch, branch);

			AssertCollectionContains(additionalDataItems, x => x.Key == "TipoEmisor" && x.Value == OrgConstants.Category.Business);
		}

		public void TestTipoEmisor_WhenCompanyOrgProxyCategory_HasValue()
		{
			var batch = CreateInvoicePivotAndBatch();
			var company = Factory.New<GlbCompany>();
			company.GC_OH_OrgProxy = CreateOrgHeader("SSD").PK;

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_OH_OrgProxy = ZGuid.Empty;

			AssertNull("PreCondition: branch.OrgProxy should be null", branch.OrgProxy);

			var additionalDataItems = CreateAdditionalDataItems(batch, branch);

			AssertCollectionContains(additionalDataItems, x => x.Key == "TipoEmisor" && x.Value == "SSD");
		}

		public void TestTipoEmisor_WhenBranchOrgProxyCategory_IsEmpty()
		{
			var batch = CreateInvoicePivotAndBatch();
			var company = Factory.New<GlbCompany>();
			var branch = Factory.New<GlbBranch>();
			branch.GB_OH_OrgProxy = CreateOrgHeader(ZString.Empty).PK;
			branch.GB_GC = company.PK;

			AssertNull("PreCondition: company.OrgProxy should be null", company.OrgProxy);

			var additionalDataItems = CreateAdditionalDataItems(batch, branch);

			AssertCollectionContains(additionalDataItems, x => x.Key == "TipoEmisor" && x.Value == OrgConstants.Category.Business);
		}

		public void TestTipoEmisor_WhenBranchOrgProxyCategory_HasValue()
		{
			var batch = CreateInvoicePivotAndBatch();
			var company = Factory.New<GlbCompany>();
			var branch = Factory.New<GlbBranch>();
			branch.GB_OH_OrgProxy = CreateOrgHeader("AAA").PK;
			branch.GB_GC = company.PK;

			AssertNull("PreCondition: company.OrgProxy should be null", company.OrgProxy);

			var additionalDataItems = CreateAdditionalDataItems(batch, branch);

			AssertCollectionContains(additionalDataItems, x => x.Key == "TipoEmisor" && x.Value == "AAA");
		}

		#endregion

		#region TipoAdquirente

		public void TestTipoAdquirente_WhenTransactionHeaderOrgProxy_IsNull()
		{
			var batch = CreateInvoicePivotAndBatch();
			var additionalDataItems = CreateAdditionalDataItems(batch, CreateBranch());

			AssertCollectionContains(additionalDataItems, x => x.Key == "TipoAdquirente" && x.Value == OrgConstants.Category.Business);
		}

		public void TestTipoAdquirente_WhenTransactionHeaderOrgProxyCategory_IsEmpty()
		{
			var orgHeader = CreateOrgHeader(ZString.Empty).PK;

			var batch = CreateInvoicePivotAndBatch(orgHeaderPK: orgHeader);
			var additionalDataItems = CreateAdditionalDataItems(batch, CreateBranch());

			AssertCollectionContains(additionalDataItems, x => x.Key == "TipoAdquirente" && x.Value == OrgConstants.Category.Business);
		}

		public void TestTipoAdquirente_WhenTransactionHeaderOrgProxyCategory_HasValue()
		{
			var orgHeader = CreateOrgHeader("CCF").PK;

			var batch = CreateInvoicePivotAndBatch(orgHeaderPK: orgHeader);
			var additionalDataItems = CreateAdditionalDataItems(batch, CreateBranch());

			AssertCollectionContains(additionalDataItems, x => x.Key == "TipoAdquirente" && x.Value == "CCF");
		}

		#endregion

		#region Prefijo and Correlativo

		public void TestPrefijo_WhenTransactionReference_NotEmpty_ComplianceBook_NotNull()
		{
			var sequenceBook = Factory.New<AccComplianceSequence>();
			sequenceBook.XD_Code = "ABC";
			sequenceBook.XD_Prefix = "AAA";

			var batch = CreateInvoicePivotAndBatch(transactionReference: "AAA15159878", accComplianceSequence: sequenceBook);
			var additionalDataItems = CreateAdditionalDataItems(batch, CreateBranch());

			AssertCollectionContains(additionalDataItems, x => x.Key == "Prefijo" && x.Value == "AAA");
		}

		public void TestCorrelativo_WhenTransactionReference_NotEmpty_ComplianceBook_NotNull()
		{
			var sequenceBook = Factory.New<AccComplianceSequence>();
			sequenceBook.XD_Code = "ABC";
			sequenceBook.XD_Prefix = "AAA";

			var batch = CreateInvoicePivotAndBatch(transactionReference: "AAA15159878", accComplianceSequence: sequenceBook);
			var additionalDataItems = CreateAdditionalDataItems(batch, CreateBranch());

			AssertCollectionContains(additionalDataItems, x => x.Key == "Correlativo" && x.Value == "15159878");
		}

		public void TestCorrelativo_WhenTransactionReference_NotEmpty_ComplianceBook_Prefix_IsEmpty()
		{
			var sequenceBook = Factory.New<AccComplianceSequence>();
			sequenceBook.XD_Code = "ABC";
			sequenceBook.XD_Prefix = string.Empty;

			var batch = CreateInvoicePivotAndBatch(transactionReference: "15159878", accComplianceSequence: sequenceBook);
			var additionalDataItems = CreateAdditionalDataItems(batch, CreateBranch());

			AssertCollectionContains(additionalDataItems, x => x.Key == "Correlativo" && x.Value == "15159878");
		}

		public void TestPrefijo_WhenTransactionReference_NotEmpty_ComplianceBook_Prefix_IsEmpty()
		{
			var sequenceBook = Factory.New<AccComplianceSequence>();
			sequenceBook.XD_Code = "ABC";
			sequenceBook.XD_Prefix = string.Empty;

			var batch = CreateInvoicePivotAndBatch(transactionReference: "15159878", accComplianceSequence: sequenceBook);
			var additionalDataItems = CreateAdditionalDataItems(batch, CreateBranch());

			AssertCollectionContains(additionalDataItems, x => x.Key == "Prefijo" && x.Value == string.Empty);
		}

		public void TestPrefijo_WhenComplianceBook_Null()
		{
			var batch = CreateInvoicePivotAndBatch();
			var additionalDataItems = CreateAdditionalDataItems(batch, CreateBranch());

			AssertCollectionContains(additionalDataItems, x => x.Key == "Prefijo" && x.Value == "0");
		}

		public void TestCorrelativo_WhenComplianceBook_Null()
		{
			var batch = CreateInvoicePivotAndBatch();
			var additionalDataItems = CreateAdditionalDataItems(batch, CreateBranch());

			AssertCollectionContains(additionalDataItems, x => x.Key == "Correlativo" && x.Value == "0");
		}

		#endregion

		#region FechaInicio, FechaFin, ConsecutivoInicial, ConsecutivoFinal, NumeroResolucion

		public void TestAddAdditionalItemsFromComplianceSequence_ComplianceBook_IsNull()
		{
			var batch = CreateInvoicePivotAndBatch(transactionReference: "AAA15159878", accComplianceSequence: null);
			var additionalDataItems = CreateAdditionalDataItems(batch, CreateBranch());

			AssertAdditionalDataItemsIsNotAdded(additionalDataItems);
		}

		public void TestAddAdditionalItemsFromComplianceSequence_TransactionReference_IsNull()
		{
			var complianceSequence = CreateAccComplianceSequence();
			var batch = CreateInvoicePivotAndBatch(transactionReference: null, accComplianceSequence: complianceSequence);
			var additionalDataItems = CreateAdditionalDataItems(batch, CreateBranch());

			AssertAdditionalDataItemsIsNotAdded(additionalDataItems);
		}

		public void TestAddAdditionalItemsFromComplianceSequence_TransaccionReference_IsEmpty()
		{
			var complianceSequence = CreateAccComplianceSequence();
			var batch = CreateInvoicePivotAndBatch(transactionReference: "", accComplianceSequence: complianceSequence);
			var additionalDataItems = CreateAdditionalDataItems(batch, CreateBranch());

			AssertAdditionalDataItemsIsNotAdded(additionalDataItems);
		}

		void AssertAdditionalDataItemsIsNotAdded(List<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem> additionalDataItems)
		{
			AssertEquals("PreCondition: Number Of Elements", 4, additionalDataItems.Count);
			AssertCollectionNotContains("FechaInicio", additionalDataItems);
			AssertCollectionNotContains("FechaFin", additionalDataItems);
			AssertCollectionNotContains("ConsecutivoInicial", additionalDataItems);
			AssertCollectionNotContains("ConsecutivoFinal", additionalDataItems);
			AssertCollectionNotContains("NumeroResolucion", additionalDataItems);
		}

		#region FechaInicio

		public void TestFechaInicio_ComplianceSequence_XD_StartDate_IsEmpty_Or_Invalid()
		{
			FechaInicioIsMissingInAdditionalDataItems(new ZDate(""));
			FechaInicioIsMissingInAdditionalDataItems(ZDate.Invalid);

			void FechaInicioIsMissingInAdditionalDataItems(ZDate date)
			{
				AssertCollectionNotContains_ComplianceSequenceInformation_InAdditionalDataItems(
					setPropertiesAccComplianceSequence: x => x.XD_StartDate = date,
					expectedKey: "FechaInicio");
			}
		}

		public void TestFechaInicio_ComplianceSequence_XD_StartDate_Valid()
		{
			AssertCollectionContains_ComplianceSequenceInformation_InAdditionalDataItems(
				setPropertiesAccComplianceSequence: x => x.XD_StartDate = new ZDate(2024, 10, 25),
				expectedKey: "FechaInicio",
				expectedValue: "2024-10-25");
		}

		#endregion

		#region FechaFin

		public void TestFechaFin_ComplianceSequence_XD_ExpiryDate_IsEmpty_Or_Invalid()
		{
			FechaFinIsMissingInAdditionalDataItems(new ZDateTime(""));
			FechaFinIsMissingInAdditionalDataItems(ZDateTime.Invalid);

			void FechaFinIsMissingInAdditionalDataItems(ZDateTime date)
			{
				AssertCollectionNotContains_ComplianceSequenceInformation_InAdditionalDataItems(
					setPropertiesAccComplianceSequence: x => x.XD_ExpiryDate = date,
					expectedKey: "FechaFin");
			}
		}

		public void TestFechaFin_ComplianceSequence_XD_ExpiryDate_Valid()
		{
			AssertCollectionContains_ComplianceSequenceInformation_InAdditionalDataItems(
				setPropertiesAccComplianceSequence: x => x.XD_ExpiryDate = new ZDateTime(2024, 10, 25, 10, 10, 10),
				expectedKey: "FechaFin",
				expectedValue: "2024-10-25");
		}

		#endregion

		#region Consecutivo Inicial

		public void TestConsecutivoInicial_ComplianceSequence_XD_StartNumber()
		{
			AssertCollectionContains_ComplianceSequenceInformation_InAdditionalDataItems(
					setPropertiesAccComplianceSequence: x => x.XD_StartNumber = 1,
					expectedKey: "ConsecutivoInicial",
					expectedValue: "1");
		}

		#endregion

		#region Consecutivo Final

		public void TestConsecutivoFinal_ComplianceSequence_XD_EndNumber()
		{
			AssertCollectionContains_ComplianceSequenceInformation_InAdditionalDataItems(
				setPropertiesAccComplianceSequence: x => x.XD_EndNumber = 10,
				expectedKey: "ConsecutivoFinal",
				expectedValue: "10");
		}

		#endregion

		#region Numero Resolucion

		public void TestNumeroResolucion_ComplianceSequence_XD_PrintingAuthorizationNumber_IsNull_Or_IsEmpty()
		{
			NumeroResolucionIsMissingInAdditionalDataItems(null);
			NumeroResolucionIsMissingInAdditionalDataItems("");

			void NumeroResolucionIsMissingInAdditionalDataItems(ZString printingAuthorizationNumber)
			{
				AssertCollectionNotContains_ComplianceSequenceInformation_InAdditionalDataItems(
					setPropertiesAccComplianceSequence: x => x.XD_PrintingAuthorizationNumber = printingAuthorizationNumber,
					"NumeroResolucion");
			}
		}

		public void TestNumeroResolucion_ComplianceSequence_XD_PrintingAuthorizationNumber_HasValue()
		{
			AssertCollectionContains_ComplianceSequenceInformation_InAdditionalDataItems(
				setPropertiesAccComplianceSequence: x => x.XD_PrintingAuthorizationNumber = "111111",
				expectedKey: "NumeroResolucion",
				expectedValue: "111111");
		}

		#endregion

		#endregion

		#region Implementation

		List<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem> CreateAdditionalDataItems(AccEInvoicingBatch batch, GlbBranch branch)
		{
			var additionalDataItemsProvider = new ColombiaEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var additionalDataItems = additionalDataItemsProvider.GetAdditionalHeaderDataItems(batch, branch, new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), new Mock<ICountryEInvoicingObjectFactory>().Object, new Mock<Common.Logger>().Object);

			return additionalDataItems.ToList<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem>();
		}

		OrgHeader CreateOrgHeader(ZString category)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Category = category;

			return orgHeader;
		}

		AccEInvoicingBatch CreateInvoicePivotAndBatch(string transactionReference = null, AccComplianceSequence accComplianceSequence = null, string complianceSubType = null, ZGuid orgHeaderPK = default)
		{
			var batch = Factory.New<AccEInvoicingBatch>();
			batch.AIB_GC = Factory.New<GlbCompany>().PK;

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_TransactionReference = transactionReference;
			invoice.AH_ComplianceSubType = complianceSubType;
			invoice.AH_OH = orgHeaderPK;

			if (accComplianceSequence != null)
			{
				invoice.AH_XD_ComplianceBook = accComplianceSequence.PK;
			}

			var pivot = batch.TransactionPivots.AddNew();
			pivot.AIP_ParentID = invoice.PK;
			pivot.SetCompanyAndCountryCode(batch.Company);

			return batch;
		}

		GlbBranch CreateBranch()
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = Factory.New<GlbCompany>().PK;

			return branch;
		}

		AccComplianceSequence CreateAccComplianceSequence(Action<AccComplianceSequence> setProperties = null)
		{
			var complianceSequence = Factory.New<AccComplianceSequence>();
			setProperties?.Invoke(complianceSequence);

			return complianceSequence;
		}

		#region Assert for Compliance Sequence Information in AdditionalDataItems

		void AssertCollectionNotContains_ComplianceSequenceInformation_InAdditionalDataItems(Action<AccComplianceSequence> setPropertiesAccComplianceSequence, string expectedKey)
		{
			foreach (var complianceSubType in ColombiaComplianceInfo.GetEInvoicingEligibleComplianceSubTypeList())
			{
				var batch = CreateInvoicePivotAndBatch(
					transactionReference: "123654789",
					accComplianceSequence: CreateAccComplianceSequence(setPropertiesAccComplianceSequence),
					complianceSubType: complianceSubType);

				var additionalDataItems = CreateAdditionalDataItems(batch, CreateBranch());

				AssertCollectionNotContains($"{expectedKey} must not exist", expectedKey, additionalDataItems);
			}
		}

		void AssertCollectionContains_ComplianceSequenceInformation_InAdditionalDataItems(Action<AccComplianceSequence> setPropertiesAccComplianceSequence, string expectedKey, string expectedValue)
		{
			foreach (var complianceSubType in ColombiaComplianceInfo.GetEInvoicingEligibleComplianceSubTypeList())
			{
				var batch = CreateInvoicePivotAndBatch(
					transactionReference: "123654789",
					accComplianceSequence: CreateAccComplianceSequence(setPropertiesAccComplianceSequence),
					complianceSubType: complianceSubType);

				var additionalDataItems = CreateAdditionalDataItems(batch, CreateBranch());

				AssertCollectionContains(additionalDataItems, x => x.Key == expectedKey && x.Value == expectedValue);
			}
		}

		#endregion

		#endregion
	}
}
