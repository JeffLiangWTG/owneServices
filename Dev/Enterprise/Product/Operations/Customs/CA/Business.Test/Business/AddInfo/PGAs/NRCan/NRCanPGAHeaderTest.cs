using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(NRCanPGAHeader))]
	sealed class NRCanPGAHeaderTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<NRCanPGAHeader>
	{
		public void TestDangerousGoodsDGSubs()
		{
			var undg = Factory.New<UNDGSubstance>();
			undg.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undg.DG_Code = "9999";
			var undgItem = Factory.New<UNDGDataItem>();
			undgItem.DI_DG = undg.PK;

			var invoiceLine = header.InvoiceLine;
			AssertNull(invoiceLine.DangerousGoods.UNDGSubstance);
			AssertEquals(ZGuid.Empty, header.DangerousGoodsDGSubs);
			header.DangerousGoodsDGSubs = undg.PK;
			AssertNotNull(invoiceLine.DangerousGoods);
		}

		public void TestLPCODefaulter()
		{
			var header = Factory.New<NRCanPGAHeader>();
			var lpcoDefaulter = header as ILPCODefaulter;
			AssertNotNull(lpcoDefaulter);
			Assert(!lpcoDefaulter.ShouldDefaultLPCOFields);
			header.CA_RDAProgramInd = "Y";
			Assert(lpcoDefaulter.ShouldDefaultLPCOFields);
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_Type));
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_RefNo));
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_EndDate));
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_IssueDate));
		}

		public void TestAvailableLPCOFields()
		{
			AssertEquals(13, NRCanPGAHeader.AvailableLPCOFields.Count);
			Assert("CLP_RN_NKIssuanceCountryCode", NRCanPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_RN_NKIssuanceCountryCode));
			Assert("CLP_RN_NKOriginCountryCode", NRCanPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_RN_NKOriginCountryCode));
			Assert("CLP_DIFRefNumberOrLocation", NRCanPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation));
			Assert("CLP_IsMixedCountryOfOrigin", NRCanPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_IsMixedCountryOfOrigin));
			Assert("CLP_ApplicantType", NRCanPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_ApplicantType));
			Assert("LPCOApplicantOrgPK", NRCanPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.LPCOApplicantOrgPK));
			Assert("LPCOApplicantAddressPK", NRCanPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_OA_Applicant));
			Assert("CLP_ApplicantName", NRCanPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_ApplicantName));
			Assert("CLP_EndDate", NRCanPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_EndDate));
			Assert("CLP_IssueDate", NRCanPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_IssueDate));
			Assert("CLP_RefNo", NRCanPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_RefNo));
			Assert("CLP_Type", NRCanPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_Type));
			Assert("CLP_IsApplicantOverridden", NRCanPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_IsApplicantOverridden));
		}

		public void TestDoNotCreateLpcoOnEmptyHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();

			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			invoice.JZ_InvoiceNumber = "111";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.FillWithValidTestData();
			invoiceLine.JI_Weight = 2;
			invoiceLine.CA_NRCanInd = YesNoList.Codes.Yes;

			var pgaHeader = invoiceLine.NRCanPGAHeader;
			pgaHeader.CA_EEFProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_EXPProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_RDAProgramInd = YesNoList.Codes.Yes;

			Factory.Save();

			Assert("Should be saved.", pgaHeader.IsInDatabase);
			Assert("Should not have any empty Lpco data on a empty pga header.", !pgaHeader.LPCOViews.Any());
		}

		public void TestPurgeValues()
		{
			header.CA_EEFProgramInd = YesNoList.Codes.Yes;
			header.CA_EXPProgramInd = YesNoList.Codes.No;
			header.CA_RDAProgramInd = YesNoList.Codes.No;
			header.CA_IntendedUseCode = NRCanIntendedUseCodes.Codes.NR01;
			header.CA_IsNotRegulatedByOEE = true;
			AssertEquals(NRCanCategoryCodes.Codes.NR01, header.CA_Category);

			header.CA_EXPProgramInd = YesNoList.Codes.Yes;
			header.CA_EEFProgramInd = YesNoList.Codes.No;
			AssertEquals("Not purged", NRCanIntendedUseCodes.Codes.NR01, header.CA_IntendedUseCode);
			AssertEquals("Purged", ZString.Empty, header.CA_Category);

			header.CA_IsNotRegulatedByExplosives = true;
			AssertEquals(NRCanIntendedUseCodes.Codes.NR04, header.CA_IntendedUseCode);
			header.CA_AuthorizedProductID = "123";
			header.CA_AuthorizedParty = LPCOHolderPartyTypeCodes.Codes.Importer;

			header.CA_RDAProgramInd = YesNoList.Codes.Yes;
			header.CA_EXPProgramInd = YesNoList.Codes.No;
			CombineAssertions(() =>
			{
				AssertEquals("Purged", ZString.Empty, header.CA_IntendedUseCode);
				AssertEquals("Purged", ZString.Empty, header.CA_AuthorizedProductID);
				AssertEquals("Purged", ZString.Empty, header.CA_AuthorizedParty);
			});

			header.CA_CaratWeight = 1m;
			header.CA_PackQty1 = 1m;
			header.CA_PackUQ1 = "CLT";
			header.CA_PackQty2 = 1m;
			header.CA_PackUQ2 = "CLT";
			header.CA_PackQty3 = 1m;
			header.CA_PackUQ3 = "CLT";
			header.CA_EEFProgramInd = YesNoList.Codes.Yes;
			header.CA_RDAProgramInd = YesNoList.Codes.No;
			CombineAssertions(() =>
			{
				AssertEquals("Purged", 0m, header.CA_CaratWeight);
				AssertEquals("Purged", 0m, header.CA_PackQty1);
				AssertEquals("Purged", ZString.Empty, header.CA_PackUQ1);
				AssertEquals("Purged", 0m, header.CA_PackQty2);
				AssertEquals("Purged", ZString.Empty, header.CA_PackUQ2);
				AssertEquals("Purged", 0m, header.CA_PackQty3);
				AssertEquals("Purged", ZString.Empty, header.CA_PackUQ3);
			});
		}

		public void TestSupportsNotes()
		{
			var bo = (NRCanPGAHeader)GetNewBusinessObject();

			AssertEquals(false, bo.SupportsNotes);
		}

		public void TestCheckCA_PackagingQuantity()
		{
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"AE", "Aerosol", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			newFactory.Save();

			var message = "The code you have selected is not in the list.";

			var declaration = newFactory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_NRCanInd = Customs.Business.YesNoList.Codes.Yes;

			var nRCanHeader = invoiceLine.NRCanPGAHeader;

			nRCanHeader.CA_PackUQ1 = "AE";
			nRCanHeader.CA_PackUQ2 = "AE";
			nRCanHeader.CA_PackUQ3 = "AE";
			AssertNoMessageError(nRCanHeader.CA_PackUQ1Info, message);
			AssertNoMessageError(nRCanHeader.CA_PackUQ2Info, message);
			AssertNoMessageError(nRCanHeader.CA_PackUQ3Info, message);

			nRCanHeader.CA_PackUQ1 = "AA";
			nRCanHeader.CA_PackUQ2 = "AA";
			nRCanHeader.CA_PackUQ3 = "AA";
			AssertHasMessageErrorContaining(nRCanHeader.CA_PackUQ1Info, message);
			AssertHasMessageErrorContaining(nRCanHeader.CA_PackUQ2Info, message);
			AssertHasMessageErrorContaining(nRCanHeader.CA_PackUQ3Info, message);
		}

		public void TestLPCOViewCollection()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "3004", "3004 DESC", startDate, endDate);
			var attributeNamePGA = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypePGAType, "PGA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributePGA = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.NRCan);
			newFactory.Save();

			AssertEquals("LpcoViews on CNSC", 0, header.LPCOViews.Count);
			AssertEquals(typeof(LPCOViewCollection), header.LPCOViews.GetType());

			var headerLPCO = declaration.LPCOViews.AddNew();
			headerLPCO.CLP_Type = "3004";
			header.LPCOViews.AddNew();
			AssertEquals("LpcoViews on NRCan", 2, header.LPCOViews.Count);

			var headerLPCO2 = declaration.LPCOViews.AddNew();
			headerLPCO2.CLP_Type = "3004";
			AssertEquals("LpcoViews on NRCan", 3, header.LPCOViews.Count);
		}

		public void TestThereIsNoExceptionWhenRequirementsParentIsNULL()
		{
			AssertNoExceptionThrown(() =>
			{
				var pgaHeader = Factory.New<NRCanPGAHeader>();
				_ = pgaHeader.RN_NKCountryOfOrigin;
				pgaHeader.RN_NKCountryOfOrigin = ZString.Empty;
				_ = pgaHeader.RN_NKCountryOfOriginInfo.SupportsMaxLength;
				_ = pgaHeader.RW_NKOriginState;
				pgaHeader.RW_NKOriginState = ZString.Empty;
				_ = pgaHeader.RW_NKOriginStateInfo.SupportsMaxLength;
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return header;
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "111";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Weight = 2;
			invoiceLine.CA_NRCanInd = YesNoList.Codes.Yes;

			header = invoiceLine.NRCanPGAHeader;
			header.CA_EEFProgramInd = YesNoList.Codes.Yes;
			header.CA_EXPProgramInd = YesNoList.Codes.Yes;
			header.CA_RDAProgramInd = YesNoList.Codes.Yes;
		}
		NRCanPGAHeader header;
		JobDeclaration declaration;
		protected override IEnumerable<NRCanPGAHeader> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_NRCanInd = "Y";
			yield return invoiceLine.NRCanPGAHeader;

			var product = factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = factory.New<CusClassPartPivot>();
			pivot.CI_OP = product.PK;
			pivot.CI_TariffNum = "1111.11.11";
			pivot.CCA_NRCanIndicator = Customs.Business.YesNoList.Codes.Yes;
			yield return pivot.NRCanPGAHeader;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_NRCanInd = "Y";
			return invoiceLine.NRCanPGAHeader;
		}
	}
}
