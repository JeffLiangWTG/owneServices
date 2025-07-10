using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CFIAPGAHeader))]
	sealed class CFIAPGAHeaderTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<CFIAPGAHeader>
	{
		public void TestGetCusAddInfoTypes()
		{
			var header = Factory.New<CFIAPGAHeader>();
			var supporter = header as ICusAddInfoTypeSupporter;

			AssertEquals(0, supporter.GetCusAddInfoTypes().Count);
		}

		public void TestCopyAIRSToCFIA()
		{
			var navigator = new AIRSWebpageNavigator(Factory, ZString.Empty);
			navigator.AG_EndUseCode = "08";
			navigator.AG_ExtensionCode = "400601";
			navigator.AG_Miscellaneous = "384";

			var lpcoList = navigator.LPCOList;
			var lpco = new AIRSLPCOSelection(Factory, false);
			lpco.MaterializedLPCOs.AddPair("400", "400 Desc");
			lpco.MaterializedLPCOs.AddPair("4", "4 Desc");
			lpco.AIRSRegistrations.AddPair("65", "65 Desc");
			lpco.AIRSRegistrations.AddPair("893", "893 Desc");
			lpco.AL_DataSetSelected = true;
			lpcoList.Add(lpco);

			var header = Factory.New<CFIAPGAHeader>();
			header.CopyAIRSToCFIA(navigator);
			AssertEquals("08", header.CA_AIRSEndUse);
			AssertEquals("400601", header.CA_AIRSExtensionCode);
			AssertEquals("384", header.CA_AIRSMiscellaneous);
			AssertEquals(2, header.LPCOViews.Count);
			AssertEquals(2, header.AIRSRegistrationNumbers.Count);

			var navigator2 = new AIRSWebpageNavigator(Factory, "308031");
			navigator2.AG_ExtensionCode = "500601";

			var lpcoList2 = navigator2.LPCOList;
			var lpco2 = new AIRSLPCOSelection(Factory, false);
			lpco2.MaterializedLPCOs.AddPair("500", "500 Desc");
			lpco2.MaterializedLPCOs.AddPair("4", "4 Desc");
			lpco2.AIRSRegistrations.AddPair("895", "895 Desc");
			lpco2.AIRSRegistrations.AddPair("893", "893 Desc");
			lpco2.AL_DataSetSelected = true;
			lpcoList2.Add(lpco2);

			header.CopyAIRSToCFIA(navigator2);
			AssertEquals("Will not override", "08", header.CA_AIRSEndUse);
			AssertEquals("override", "500601", header.CA_AIRSExtensionCode);
			AssertEquals("Will not override", "384", header.CA_AIRSMiscellaneous);
			AssertEquals(3, header.LPCOViews.Count);
			AssertEquals(3, header.AIRSRegistrationNumbers.Count);

			AssertContainsExactElementsInAnyOrder(new[] { "400", "4", "500" }, header.LPCOViews.Cast<LPCOView>().Select(x => x.CLP_Type));
			AssertContainsExactElementsInAnyOrder(new[] { "893", "65", "895" }, header.AIRSRegistrationNumbers.Cast<AIRSRegistrationNumber>().Select(x => x.CY_Code));
		}

		public void TestIsBlank()
		{
			var header1 = Factory.New<CFIAPGAHeader>();
			AssertEquals(true, header1.IsBlank);

			header1.CA_AIRSEndUse = "XX";
			AssertEquals(false, header1.IsBlank);

			var header2 = Factory.New<CFIAPGAHeader>();
			AssertEquals(true, header2.IsBlank);

			header2.LPCOViews.AddNew();
			AssertEquals(false, header2.IsBlank);

			var header3 = Factory.New<CFIAPGAHeader>();
			AssertEquals(true, header3.IsBlank);

			header3.AIRSRegistrationNumbers.AddNew();
			AssertEquals(false, header3.IsBlank);
		}

		public void TestLPCODefaulter()
		{
			var header = Factory.New<CFIAPGAHeader>();
			var lpcoDefaulter = header as ILPCODefaulter;
			AssertNotNull(lpcoDefaulter);
			Assert(lpcoDefaulter.ShouldDefaultLPCOFields);
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_Type));
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_RefNo));
		}

		#region PurgeValues

		public void TestPurgeValues()
		{
			var header = (CFIAPGAHeader)GetNewBusinessObject();
			AssertPurgeValue(header, header.CA_AIRSExtensionCodeInfo, (ZString)"Test");
			AssertPurgeValue(header, header.CA_AIRSEndUseInfo, (ZString)"01");
			AssertPurgeValue(header, header.CA_AIRSMiscellaneousInfo, (ZString)"01");

			header.CA_AllProgramInd = YesNoList.Codes.Yes;
			var lpco = header.LPCOViews.AddNew();
			lpco.CLP_Type = "10";
			lpco.CLP_RefNo = "Test";
			lpco.CLP_DIFRefNumberOrLocation = "Test";

			var airs = header.AIRSRegistrationNumbers.AddNew();
			airs.CY_Code = "01";
			airs.CY_Data = "TTG TEST";
			header.CA_AllProgramInd = YesNoList.Codes.No;

			AssertEquals("LPCOs are purged.", true, lpco.IsDeleted);
			AssertEquals("AIRSRegistrationNumbers are purged.", true, airs.IsDeleted);
		}

		void AssertPurgeValue(CFIAPGAHeader header, ZPropertyInfo info, IZType value)
		{
			header.CA_AllProgramInd = YesNoList.Codes.Yes;
			info.Value = value;
			header.CA_AllProgramInd = YesNoList.Codes.No;
			AssertEquals(info.Name + "is purged.", info.DefaultValue, info.Value);
		}

		#endregion

		public void TestAvailableLPCOFields()
		{
			AssertEquals(3, CFIAPGAHeader.AvailableLPCOFields.Count);
			Assert("CLP_DIFRefNumberOrLocation", CFIAPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation));
			Assert("CLP_RefNo", CFIAPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_RefNo));
			Assert("CLP_Type", CFIAPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_Type));
		}

		public void TestSupportsNotes()
		{
			var bo = (CFIAPGAHeader)GetNewBusinessObject();

			AssertEquals(false, bo.SupportsNotes);
		}

		public void TestLPCOViewCollection()
		{
			RegistrationNumberHelperTest.PrepareGlobalData(Factory);

			AssertEquals("LpcoViews on CFIA", 0, header.LPCOViews.Count);
			AssertEquals(typeof(LPCOViewCollection), header.LPCOViews.GetType());

			var headerLPCO = declaration.LPCOViews.AddNew();
			headerLPCO.CLP_Type = "C01";
			header.LPCOViews.AddNew();
			AssertEquals("LpcoViews on CFIA", 2, header.LPCOViews.Count);

			var headerLPCO2 = declaration.LPCOViews.AddNew();
			headerLPCO2.CLP_Type = "C02";
			AssertEquals("LpcoViews on CFIA", 3, header.LPCOViews.Count);
		}

		public void TestDefaultLPCOViewCollection()
		{
			var cfia = Factory.New<CFIAPGAHeader>();
			AssertNotNull("Default LPCOViews on CFIA", cfia.LPCOViews);
			AssertEquals(typeof(LPCOViewCollection), cfia.LPCOViews.GetType());
		}

		public void TestCountryAndState()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			var cfia = Factory.New<CFIAPGAHeader>();
			cfia.B7_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			cfia.B7_ParentID = invoiceLine.PK;
			AssertEquals(ZString.Empty, cfia.RN_NKCountryOfSource);
			AssertEquals(ZString.Empty, cfia.RW_NKSourceState);

			invoiceLine.CA_RN_NKSource = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.CA_StateOfSource = USStatesList.Codes.Ohio;
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, cfia.RN_NKCountryOfSource);
			AssertEquals(USStatesList.Codes.Ohio, cfia.RW_NKSourceState);

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "123";
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_OP = part.PK;

			var cfiaOnPivot = Factory.New<CFIAPGAHeader>();
			cfiaOnPivot.B7_ParentTableCode = CusClassPartPivotSchema.Constants.Prefix;
			cfiaOnPivot.B7_ParentID = pivot.PK;
			AssertEquals(ZString.Empty, cfiaOnPivot.RN_NKCountryOfSource);
			AssertEquals(ZString.Empty, cfiaOnPivot.RW_NKSourceState);

			pivot.CCA_RN_NKSource = Core.Constants.CountryCodes.UnitedStates;
			pivot.CCA_StateOfSource = USStatesList.Codes.Alabama;
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, cfiaOnPivot.RN_NKCountryOfSource);
			AssertEquals(USStatesList.Codes.Alabama, cfiaOnPivot.RW_NKSourceState);
		}

		public void TestThereIsNoExceptionWhenRequirementsParentIsNULL()
		{
			AssertNoExceptionThrown(() =>
			{
				var pgaHeader = Factory.New<CFIAPGAHeader>();
				_ = pgaHeader.RN_NKCountryOfSource;
				pgaHeader.RN_NKCountryOfSource = ZString.Empty;
				_ = pgaHeader.RN_NKCountryOfSourceInfo.SupportsMaxLength;
				_ = pgaHeader.RW_NKSourceState;
				pgaHeader.RW_NKSourceState = ZString.Empty;
				_ = pgaHeader.RW_NKSourceStateInfo.SupportsMaxLength;
			});
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = "Y";
			header = invoiceLine.CFIAPGAHeader;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return header;
		}
		CFIAPGAHeader header;
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		protected override IEnumerable<CFIAPGAHeader> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = "Y";
			yield return invoiceLine.CFIAPGAHeader;

			var product = factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = factory.New<CusClassPartPivot>();
			pivot.CI_OP = product.PK;
			pivot.CI_TariffNum = "1111.11.11";
			pivot.CCA_CFIAIndicator = Customs.Business.YesNoList.Codes.Yes;
			yield return pivot.CFIAPGAHeader;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = "Y";
			return invoiceLine.CFIAPGAHeader;
		}

		#endregion
	}
}
