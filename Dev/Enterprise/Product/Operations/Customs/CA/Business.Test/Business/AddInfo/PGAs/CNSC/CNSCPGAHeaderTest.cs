using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CNSCPGAHeader))]
	sealed class CNSCPGAHeaderTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<CNSCPGAHeader>
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

		public void TestSetterSuspenderForProgramInds()
		{
			var header = Factory.New<CNSCPGAHeader>();
			var suspender = header.SetterSuspender;
			suspender.SuspendSetting(CNSCPGAHeader.Schema.CA_AllProgramInd);
			header.CA_AllProgramInd = "Y";
			AssertEquals("AllProgramInd", ZString.Empty, header.CA_AllProgramInd);
			suspender.ResumeSetting(CNSCPGAHeader.Schema.CA_AllProgramInd);
			header.CA_AllProgramInd = "Y";
			AssertEquals("AllProgramInd", "Y", header.CA_AllProgramInd);
		}

		public void TestLPCODefaulter()
		{
			var header = Factory.New<CNSCPGAHeader>();
			var lpcoDefaulter = header as ILPCODefaulter;
			AssertNotNull(lpcoDefaulter);
			Assert(lpcoDefaulter.ShouldDefaultLPCOFields);
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_Type));
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_RefNo));
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_HolderType));
		}

		#region PurgeValues

		public void TestPurgeValues()
		{
			var header = (CNSCPGAHeader)GetNewBusinessObject();
			AssertPurgeValue(header, header.CA_CategoryInfo, (ZString)"RD");
			AssertPurgeValue(header, header.CA_PackQtyInfo, (ZDecimal)2m);
			AssertPurgeValue(header, header.CA_PackUQInfo, (ZString)"AE");
			AssertPurgeValue(header, header.CA_PackMarksInfo, (ZString)"Marks");
			AssertPurgeValue(header, header.CA_NNIECRSchePartNoInfo, (ZString)"111");
			AssertPurgeValue(header, header.CA_UnitQtyInfo, (ZInt)22);

			header.CA_AllProgramInd = YesNoList.Codes.Yes;
			var lpco = header.LPCOViews.AddNew();
			lpco.CLP_Type = "10";
			lpco.CLP_DIFRefNumberOrLocation = "Test";
			lpco.CLP_RefNo = "Test";
			lpco.CLP_HolderType = "IMP";

			var component = header.Components.AddNew();
			component.CA_Name = "test";
			component.CA_Qty = 1m;
			component.CA_UQ = "CC";

			header.CA_AllProgramInd = YesNoList.Codes.No;

			AssertEquals("LPCOs are purged.", true, lpco.IsDeleted);
			AssertEquals("Components are purged.", true, component.IsDeleted);
		}

		void AssertPurgeValue(CNSCPGAHeader header, ZPropertyInfo info, IZType value)
		{
			header.CA_AllProgramInd = YesNoList.Codes.Yes;
			info.Value = value;
			header.CA_AllProgramInd = YesNoList.Codes.No;
			AssertEquals(info.Name + "is purged.", info.DefaultValue, info.Value);
		}

		#endregion

		public void TestAvailableLPCOFields()
		{
			AssertEquals(8, CNSCPGAHeader.AvailableLPCOFields.Count);
			Assert("CLP_DIFRefNumberOrLocation", CNSCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation));
			Assert("CLP_HolderType", CNSCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_HolderType));
			Assert("LPCOHolderOrgPK", CNSCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.LPCOHolderOrgPK));
			Assert("CLP_OA_Holder", CNSCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_OA_Holder));
			Assert("CLP_HolderName", CNSCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_HolderName));
			Assert("CLP_RefNo", CNSCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_RefNo));
			Assert("CLP_Type", CNSCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_Type));
			Assert("CLP_IsHolderOverridden", CNSCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_IsHolderOverridden));
		}

		public void TestSupportsNotes()
		{
			var bo = (CNSCPGAHeader)GetNewBusinessObject();

			AssertEquals(false, bo.SupportsNotes);
		}

		public void TestGetCusAddInfoType()
		{
			var pgaHeader = Factory.New<CNSCPGAHeader>();
			var iCusAddInfoTypeSupporter = (ICusAddInfoTypeSupporter)pgaHeader;
			iCusAddInfoTypeSupporter.AssertType(typeof(Component), CusAddInfoTypeAttribute.Codes.CAComponent);
		}

		public void TestLPCOViewCollection()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "7001", "7001 DESC", startDate, endDate);
			var attributeNamePGA = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypePGAType, "PGA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributePGA = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.CNSC);
			newFactory.Save();

			AssertEquals("LpcoViews on CNSC", 0, header.LPCOViews.Count);
			AssertEquals(typeof(LPCOViewCollection), header.LPCOViews.GetType());

			var headerLPCO = declaration.LPCOViews.AddNew();
			headerLPCO.CLP_Type = "7001";
			header.LPCOViews.AddNew();
			AssertEquals("LpcoViews on CNSC", 2, header.LPCOViews.Count);

			var headerLPCO2 = declaration.LPCOViews.AddNew();
			headerLPCO2.CLP_Type = "7001";
			AssertEquals("LpcoViews on CNSC", 3, header.LPCOViews.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CNSCInd = "Y";
			header = invoiceLine.CNSCPGAHeader;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return header;
		}
		CNSCPGAHeader header;
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		protected override IEnumerable<CNSCPGAHeader> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CNSCInd = "Y";
			yield return invoiceLine.CNSCPGAHeader;

			var product = factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = factory.New<CusClassPartPivot>();
			pivot.CI_OP = product.PK;
			pivot.CI_TariffNum = "1111.11.11";
			pivot.CCA_CNSCIndicator = Customs.Business.YesNoList.Codes.Yes;
			yield return pivot.CNSCPGAHeader;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CNSCInd = "Y";
			return invoiceLine.CNSCPGAHeader;
		}
	}
}
