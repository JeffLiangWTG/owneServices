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
	[TestedType(typeof(PHACPGAHeader))]
	sealed class PHACPGAHeaderTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<PHACPGAHeader>
	{
		public void TestDangerousGoodsDGSubs()
		{
			var undg = Factory.New<UNDGSubstance>();
			undg.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undg.DG_Code = "9999";
			var undgItem = Factory.New<UNDGDataItem>();
			undgItem.DI_DG = undg.PK;

			AssertNull(invoiceLine.DangerousGoods.UNDGSubstance);
			AssertEquals(ZGuid.Empty, header.DangerousGoodsDGSubs);
			header.DangerousGoodsDGSubs = undg.PK;
			AssertNotNull(invoiceLine.DangerousGoods);
		}
		public void TestLPCODefaulter()
		{
			var header = Factory.New<PHACPGAHeader>();
			var lpcoDefaulter = header as ILPCODefaulter;
			AssertNotNull(lpcoDefaulter);
			Assert(lpcoDefaulter.ShouldDefaultLPCOFields);
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_Type));
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_RefNo));
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_EndDate));
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_StartDate));
		}

		#region PurgeValues

		public void TestPurgeValues()
		{
			var header = (PHACPGAHeader)GetNewBusinessObject();
			AssertPurgeValue(header, header.CA_CategoryInfo, (ZString)"PH01");
			AssertPurgeValue(header, header.CA_IntendedUseCodeInfo, (ZString)"PH01");
			AssertPurgeValue(header, header.CA_ExceptPathogenToxinInfo, ZBool.True);

			header.CA_HAPProgramInd = YesNoList.Codes.Yes;
			var lpco = header.LPCOViews.AddNew();
			lpco.CLP_Type = "10";
			lpco.CLP_DIFRefNumberOrLocation = "Test";
			lpco.CLP_RefNo = "Test";
			lpco.CLP_HolderType = "IMP";
			header.CA_HAPProgramInd = YesNoList.Codes.No;

			AssertEquals("LPCOs are purged.", true, lpco.IsDeleted);
		}

		void AssertPurgeValue(PHACPGAHeader header, ZPropertyInfo info, IZType value)
		{
			header.CA_HAPProgramInd = YesNoList.Codes.Yes;
			info.Value = value;
			header.CA_HAPProgramInd = YesNoList.Codes.No;
			AssertEquals(info.Name + "is purged.", info.DefaultValue, info.Value);
		}

		#endregion

		public void TestAvailableLPCOFields()
		{
			AssertEquals(5, PHACPGAHeader.AvailableLPCOFields.Count);
			Assert("CLP_DIFRefNumberOrLocation", PHACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation));
			Assert("CLP_EndDate", PHACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_EndDate));
			Assert("CLP_StartDate", PHACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_StartDate));
			Assert("CLP_RefNo", PHACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_RefNo));
			Assert("CLP_Type", PHACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_Type));
		}
		public void TestSupportsNotes()
		{
			var bo = (PHACPGAHeader)GetNewBusinessObject();

			AssertEquals(false, bo.SupportsNotes);
		}

		public void TestLPCOViewCollection()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "5503", "5503 DESC", startDate, endDate);
			var attributeNamePGA = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypePGAType, "PGA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributePGA = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.PHAC);
			newFactory.Save();

			AssertEquals("LpcoViews on PHAC", 0, header.LPCOViews.Count);
			AssertEquals(typeof(LPCOViewCollection), header.LPCOViews.GetType());

			var headerLPCO = declaration.LPCOViews.AddNew();
			headerLPCO.CLP_Type = "5503";
			header.LPCOViews.AddNew();
			AssertEquals("LpcoViews on PHAC", 2, header.LPCOViews.Count);

			var headerLPCO2 = declaration.LPCOViews.AddNew();
			headerLPCO2.CLP_Type = "5503";
			AssertEquals("LpcoViews on PHAC", 3, header.LPCOViews.Count);
		}

		protected override IEnumerable<PHACPGAHeader> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_PHACInd = "Y";
			yield return invoiceLine.PHACPGAHeader;

			var product = factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = factory.New<CusClassPartPivot>();
			pivot.CI_OP = product.PK;
			pivot.CI_TariffNum = "1111.11.11";
			pivot.CCA_PHACIndicator = Customs.Business.YesNoList.Codes.Yes;
			yield return pivot.PHACPGAHeader;
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_PHACInd = "Y";
			header = invoiceLine.PHACPGAHeader;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return header;
		}
		PHACPGAHeader header;
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_PHACInd = "Y";
			return invoiceLine.PHACPGAHeader;
		}
	}
}
