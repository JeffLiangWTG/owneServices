using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(GuidFindBoxBusinessObject))]
	sealed class GuidFindBoxBusinessObjectTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		#region TestDebtorFindBoxBusinessObject

		public void TestDebtorFindBoxBusinessObject()
		{
			GuidFindBoxRegistryEditorInfo editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.Debtor);
			FallbackLevel fallback = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
			GuidFindBoxBusinessObject bizO = new GuidFindBoxBusinessObject(fallback, editorInfo);
			((BusinessObjectCollection)bizO.ChoicesCollection).Load();

			Assert("Failed to return a collection with any organisations.", bizO.ChoicesCollection.Count > 0);

			int i = 0;
			foreach (OrgHeader debtor in bizO.ChoicesCollection)
			{
				AssertEquals("Members of collection should all be debtors.", true, debtor.CompanyData.OB_IsDebtor);
				if (i++ > 20)
				{
					break;
				}
			}
		}

		#endregion

		#region TestARInvoiceMenuItemFilter

		public void TestARInvoiceMenuItemFilter()
		{
			StmMenuItem unpublishedARInvoiceMenuItem = Factory.NewWithValidTestData<StmMenuItem>();
			unpublishedARInvoiceMenuItem.SU_BusinessContext = "ARInvoice";
			unpublishedARInvoiceMenuItem.SU_IsPublished = false;

			StmMenuItem publishedARInvoiceMenuItem = Factory.NewWithValidTestData<StmMenuItem>();
			publishedARInvoiceMenuItem.SU_BusinessContext = "ARInvoice";
			publishedARInvoiceMenuItem.SU_IsPublished = true;

			StmMenuItem publisedNonARInvoiceMenuItem = Factory.NewWithValidTestData<StmMenuItem>();
			publisedNonARInvoiceMenuItem.SU_BusinessContext = "ABC123";
			publisedNonARInvoiceMenuItem.SU_IsPublished = true;
			Factory.Save();

			GuidFindBoxRegistryEditorInfo editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.ARInvoiceMenuItem);
			GuidFindBoxBusinessObject bizO = new GuidFindBoxBusinessObject(null, editorInfo);

			((BusinessObjectCollection)bizO.ChoicesCollection).Load();
			Assert("Should not contain unpublished AR Invoice item", !bizO.ChoicesCollection.Contains(unpublishedARInvoiceMenuItem.PK));
			Assert("Should contain published AR Invoice item", bizO.ChoicesCollection.Contains(publishedARInvoiceMenuItem.PK));
			Assert("Should not contain non AR Invoice item", !bizO.ChoicesCollection.Contains(publisedNonARInvoiceMenuItem.PK));
		}

		#endregion

		#region TestRegistryFindBoxFilterTTL

		public void TestRegistryFindBoxFilterTTL()
		{
			var gLHeader1 = Factory.NewWithValidTestData<AccGLHeader>();
			var gLHeader2 = Factory.NewWithValidTestData<AccGLHeader>();

			gLHeader1.AG_AccountType = "TTL";
			gLHeader2.AG_AccountType = "HDR";

			Factory.Save();

			GuidFindBoxRegistryEditorInfo editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.TTL);
			GuidFindBoxBusinessObject bizO = new GuidFindBoxBusinessObject(null, editorInfo);

			((BusinessObjectCollection)bizO.ChoicesCollection).Load();
			AssertEquals("Collection should contain AccGLHeader with AG_AccountType of 'TTL'.", true, bizO.ChoicesCollection.Contains(gLHeader1.PK));
			AssertEquals("Collection should not contain AccGLHeader with AG_AccountType of 'HDR'.", false, bizO.ChoicesCollection.Contains(gLHeader2.PK));
		}

		#endregion

		#region TestRegistryFindBoxFilterBSH

		public void TestRegistryFindBoxFilterBSH()
		{
			var header1 = Factory.NewWithValidTestData<AccGLHeader>();
			var header2 = Factory.NewWithValidTestData<AccGLHeader>();
			var header3 = Factory.NewWithValidTestData<AccGLHeader>();

			header1.AG_AccountType = "BSH";
			header1.AG_ControlAccount = true;

			header2.AG_AccountType = "HDR";

			header3.AG_AccountType = "BSH";
			header3.AG_ControlAccount = false;

			Factory.Save();

			GuidFindBoxRegistryEditorInfo editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSH);
			GuidFindBoxBusinessObject bizO = new GuidFindBoxBusinessObject(null, editorInfo);

			((BusinessObjectCollection)bizO.ChoicesCollection).Load();
			AssertEquals(true, bizO.ChoicesCollection.Contains(header1.PK));
			AssertEquals(true, bizO.ChoicesCollection.Contains(header3.PK));
			AssertEquals(false, bizO.ChoicesCollection.Contains(header2.PK));
		}

		public void TestRegistryFindBoxFilter_BSHAndNonControl_AllowDirectPost()
		{
			var header1 = Factory.NewWithValidTestData<AccGLHeader>();
			var header2 = Factory.NewWithValidTestData<AccGLHeader>();
			var header3 = Factory.NewWithValidTestData<AccGLHeader>();
			var header4 = Factory.NewWithValidTestData<AccGLHeader>();
			var header5 = Factory.NewWithValidTestData<AccGLHeader>();

			header1.AG_AccountType = "BSH";
			header1.AG_ControlAccount = true;
			header1.AG_DisallowDirectPosting = false;

			header2.AG_AccountType = "HDR";
			header2.AG_ControlAccount = false;
			header2.AG_DisallowDirectPosting = false;

			header3.AG_AccountType = "P&L";
			header3.AG_ControlAccount = false;
			header3.AG_DisallowDirectPosting = false;

			header4.AG_AccountType = "BSH";
			header4.AG_ControlAccount = false;
			header4.AG_DisallowDirectPosting = true;

			header5.AG_AccountType = "BSH";
			header5.AG_ControlAccount = false;
			header5.AG_DisallowDirectPosting = false;

			Factory.Save();

			GuidFindBoxRegistryEditorInfo editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndNonControl_AllowDirectPost);
			GuidFindBoxBusinessObject bizO = new GuidFindBoxBusinessObject(null, editorInfo);
			((BusinessObjectCollection)bizO.ChoicesCollection).Load();

			Assert(!bizO.ChoicesCollection.Contains(header1.PK));
			Assert(!bizO.ChoicesCollection.Contains(header2.PK));
			Assert(!bizO.ChoicesCollection.Contains(header3.PK));
			Assert(!bizO.ChoicesCollection.Contains(header4.PK));
			Assert(bizO.ChoicesCollection.Contains(header5.PK));
		}

		public void TestRegistryFindBoxFilter_PandLOrBSHandNonControl()
		{
			var header1 = Factory.NewWithValidTestData<AccGLHeader>();
			header1.AG_AccountType = Core.Constants.AccountType.Total;
			header1.AG_ControlAccount = false;

			var header2 = Factory.NewWithValidTestData<AccGLHeader>();
			header2.AG_AccountType = Core.Constants.AccountType.Header;

			var header3 = Factory.NewWithValidTestData<AccGLHeader>();
			header3.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			header3.AG_ControlAccount = true;

			var header4 = Factory.NewWithValidTestData<AccGLHeader>();
			header4.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			header4.AG_ControlAccount = false;
			header4.AG_IsGlobal = true;
			header4.AG_DisallowDirectPosting = true;

			var header5 = Factory.NewWithValidTestData<AccGLHeader>();
			header5.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			header5.AG_ControlAccount = false;
			header5.AG_IsGlobal = false;

			var header6 = Factory.NewWithValidTestData<AccGLHeader>();
			header6.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			header6.AG_ControlAccount = false;
			header6.AG_IsGlobal = true;
			header6.AG_DisallowDirectPosting = false;

			var header7 = Factory.NewWithValidTestData<AccGLHeader>();
			header7.AG_AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			header7.AG_ControlAccount = true;

			var header8 = Factory.NewWithValidTestData<AccGLHeader>();
			header8.AG_AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			header8.AG_ControlAccount = false;
			header8.AG_IsGlobal = true;
			header8.AG_DisallowDirectPosting = true;

			var header9 = Factory.NewWithValidTestData<AccGLHeader>();
			header9.AG_AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			header9.AG_ControlAccount = false;
			header9.AG_IsGlobal = false;

			var header10 = Factory.NewWithValidTestData<AccGLHeader>();
			header10.AG_AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			header10.AG_ControlAccount = false;
			header10.AG_IsGlobal = true;
			header10.AG_DisallowDirectPosting = false;

			Factory.Save();

			GuidFindBoxRegistryEditorInfo editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.PandLOrBSHandNonControl);
			GuidFindBoxBusinessObject bizO = new GuidFindBoxBusinessObject(null, editorInfo);
			((BusinessObjectCollection)bizO.ChoicesCollection).Load();

			Assert(!bizO.ChoicesCollection.Contains(header1.PK));
			Assert(!bizO.ChoicesCollection.Contains(header2.PK));
			Assert(!bizO.ChoicesCollection.Contains(header3.PK));
			Assert(bizO.ChoicesCollection.Contains(header4.PK));
			Assert(!bizO.ChoicesCollection.Contains(header5.PK));
			Assert(bizO.ChoicesCollection.Contains(header6.PK));
			Assert(!bizO.ChoicesCollection.Contains(header7.PK));
			Assert(bizO.ChoicesCollection.Contains(header8.PK));
			Assert(!bizO.ChoicesCollection.Contains(header9.PK));
			Assert(bizO.ChoicesCollection.Contains(header10.PK));
		}

		public void TestRegistryFindBoxFilter_BSHAndNonControlDisallowDirectPost()
		{
			var header1 = Factory.NewWithValidTestData<AccGLHeader>();
			var header2 = Factory.NewWithValidTestData<AccGLHeader>();
			var header3 = Factory.NewWithValidTestData<AccGLHeader>();
			var header4 = Factory.NewWithValidTestData<AccGLHeader>();
			var header5 = Factory.NewWithValidTestData<AccGLHeader>();

			header1.AG_AccountType = "BSH";
			header1.AG_ControlAccount = true;
			header1.AG_DisallowDirectPosting = false;

			header2.AG_AccountType = "HDR";
			header2.AG_ControlAccount = false;
			header2.AG_DisallowDirectPosting = false;

			header3.AG_AccountType = "P&L";
			header3.AG_ControlAccount = false;
			header3.AG_DisallowDirectPosting = false;

			header4.AG_AccountType = "BSH";
			header4.AG_ControlAccount = false;
			header4.AG_DisallowDirectPosting = false;

			header5.AG_AccountType = "BSH";
			header5.AG_ControlAccount = false;
			header5.AG_DisallowDirectPosting = true;

			Factory.Save();

			GuidFindBoxRegistryEditorInfo editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndNonControlDisallowDirectPost);
			GuidFindBoxBusinessObject bizO = new GuidFindBoxBusinessObject(null, editorInfo);
			((BusinessObjectCollection)bizO.ChoicesCollection).Load();

			Assert(!bizO.ChoicesCollection.Contains(header1.PK));
			Assert(!bizO.ChoicesCollection.Contains(header2.PK));
			Assert(!bizO.ChoicesCollection.Contains(header3.PK));
			Assert(!bizO.ChoicesCollection.Contains(header4.PK));
			Assert(bizO.ChoicesCollection.Contains(header5.PK));
		}

		public void TestRegistryFindBoxFilter_PandLOrBSHandNonControl_DisallowDirectPost()
		{
			var header1 = Factory.NewWithValidTestData<AccGLHeader>();
			header1.AG_AccountType = Core.Constants.AccountType.Total;
			header1.AG_ControlAccount = false;

			var header2 = Factory.NewWithValidTestData<AccGLHeader>();
			header2.AG_AccountType = Core.Constants.AccountType.Header;

			var header3 = Factory.NewWithValidTestData<AccGLHeader>();
			header3.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			header3.AG_ControlAccount = true;

			var header4 = Factory.NewWithValidTestData<AccGLHeader>();
			header4.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			header4.AG_ControlAccount = false;
			header4.AG_IsGlobal = true;
			header4.AG_DisallowDirectPosting = true;

			var header5 = Factory.NewWithValidTestData<AccGLHeader>();
			header5.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			header5.AG_ControlAccount = false;
			header5.AG_IsGlobal = false;

			var header6 = Factory.NewWithValidTestData<AccGLHeader>();
			header6.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			header6.AG_ControlAccount = false;
			header6.AG_IsGlobal = true;
			header6.AG_DisallowDirectPosting = false;

			var header7 = Factory.NewWithValidTestData<AccGLHeader>();
			header7.AG_AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			header7.AG_ControlAccount = true;

			var header8 = Factory.NewWithValidTestData<AccGLHeader>();
			header8.AG_AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			header8.AG_ControlAccount = false;
			header8.AG_IsGlobal = true;
			header8.AG_DisallowDirectPosting = true;

			var header9 = Factory.NewWithValidTestData<AccGLHeader>();
			header9.AG_AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			header9.AG_ControlAccount = false;
			header9.AG_IsGlobal = false;

			var header10 = Factory.NewWithValidTestData<AccGLHeader>();
			header10.AG_AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			header10.AG_ControlAccount = false;
			header10.AG_IsGlobal = true;
			header10.AG_DisallowDirectPosting = false;

			Factory.Save();

			GuidFindBoxRegistryEditorInfo editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.PandLOrBSHAndNonControl_DisallowDirectPost);
			GuidFindBoxBusinessObject bizO = new GuidFindBoxBusinessObject(null, editorInfo);
			((BusinessObjectCollection)bizO.ChoicesCollection).Load();

			Assert(!bizO.ChoicesCollection.Contains(header1.PK));
			Assert(!bizO.ChoicesCollection.Contains(header2.PK));
			Assert(!bizO.ChoicesCollection.Contains(header3.PK));
			Assert(bizO.ChoicesCollection.Contains(header4.PK));
			Assert(!bizO.ChoicesCollection.Contains(header5.PK));
			Assert(!bizO.ChoicesCollection.Contains(header6.PK));
			Assert(!bizO.ChoicesCollection.Contains(header7.PK));
			Assert(bizO.ChoicesCollection.Contains(header8.PK));
			Assert(!bizO.ChoicesCollection.Contains(header9.PK));
			Assert(!bizO.ChoicesCollection.Contains(header10.PK));
		}

		#endregion

		#region TestRegistryFindBoxFilterAccTaxRate

		public void TestRegistryFindBoxFilterAccTaxRate()
		{
			AccTaxRate capitalRated = Factory.LoadTop1<AccTaxRate>(GetTaxRateFilterByType(AccTaxRate.Types.CapitalRated)) ?? CreateTaxRateOfType(AccTaxRate.Types.CapitalRated);

			AccTaxRate exempt = Factory.LoadTop1<AccTaxRate>(GetTaxRateFilterByType(AccTaxRate.Types.Exempt)) ?? CreateTaxRateOfType(AccTaxRate.Types.Exempt);

			AccTaxRate notReportable = Factory.LoadTop1<AccTaxRate>(GetTaxRateFilterByType(AccTaxRate.Types.NotReportable)) ?? CreateTaxRateOfType(AccTaxRate.Types.NotReportable);

			AccTaxRate rated = Factory.LoadTop1<AccTaxRate>(GetTaxRateFilterByType(AccTaxRate.Types.Rated)) ?? CreateTaxRateOfType(AccTaxRate.Types.Rated);

			AccTaxRate reverseRated = Factory.LoadTop1<AccTaxRate>(GetTaxRateFilterByType(AccTaxRate.Types.ReverseRated)) ?? CreateTaxRateOfType(AccTaxRate.Types.ReverseRated);

			Factory.Save();

			FallbackLevel companyLevel = new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

			GuidFindBoxRegistryEditorInfo editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccTaxRate, RegistryFindBoxFilter.AccTaxRateTypeCapitalRated);
			GuidFindBoxBusinessObject bizO = new GuidFindBoxBusinessObject(companyLevel, editorInfo);
			((BusinessObjectCollection)bizO.ChoicesCollection).Load();
			AssertEquals("Collection should contain CapitalRated TaxRate", true, bizO.ChoicesCollection.Contains(capitalRated.PK));
			AssertEquals("Collection should not contain Exempt TaxRate", false, bizO.ChoicesCollection.Contains(exempt.PK));
			AssertEquals("Collection should not contain NotReportable TaxRate", false, bizO.ChoicesCollection.Contains(notReportable.PK));
			AssertEquals("Collection should not contain Rated TaxRate", false, bizO.ChoicesCollection.Contains(rated.PK));
			AssertEquals("Collection should not contain ReverseRated TaxRate", false, bizO.ChoicesCollection.Contains(reverseRated.PK));

			editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccTaxRate, RegistryFindBoxFilter.AccTaxRateTypeExempt);
			bizO = new GuidFindBoxBusinessObject(companyLevel, editorInfo);
			((BusinessObjectCollection)bizO.ChoicesCollection).Load();
			AssertEquals("Collection should not contain CapitalRated TaxRate", false, bizO.ChoicesCollection.Contains(capitalRated.PK));
			AssertEquals("Collection should not contain Exempt TaxRate", true, bizO.ChoicesCollection.Contains(exempt.PK));
			AssertEquals("Collection should not contain NotReportable TaxRate", false, bizO.ChoicesCollection.Contains(notReportable.PK));
			AssertEquals("Collection should not contain Rated TaxRate", false, bizO.ChoicesCollection.Contains(rated.PK));
			AssertEquals("Collection should not contain ReverseRated TaxRate", false, bizO.ChoicesCollection.Contains(reverseRated.PK));

			editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccTaxRate, RegistryFindBoxFilter.AccTaxRateTypeNotReportable);
			bizO = new GuidFindBoxBusinessObject(companyLevel, editorInfo);
			((BusinessObjectCollection)bizO.ChoicesCollection).Load();
			AssertEquals("Collection should not contain CapitalRated TaxRate", false, bizO.ChoicesCollection.Contains(capitalRated.PK));
			AssertEquals("Collection should not contain Exempt TaxRate", false, bizO.ChoicesCollection.Contains(exempt.PK));
			AssertEquals("Collection should contain NotReportable TaxRate", true, bizO.ChoicesCollection.Contains(notReportable.PK));
			AssertEquals("Collection should not contain Rated TaxRate", false, bizO.ChoicesCollection.Contains(rated.PK));
			AssertEquals("Collection should not contain ReverseRated TaxRate", false, bizO.ChoicesCollection.Contains(reverseRated.PK));

			editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccTaxRate, RegistryFindBoxFilter.AccTaxRateTypeRated);
			bizO = new GuidFindBoxBusinessObject(companyLevel, editorInfo);
			((BusinessObjectCollection)bizO.ChoicesCollection).Load();
			AssertEquals("Collection should not contain CapitalRated TaxRate", false, bizO.ChoicesCollection.Contains(capitalRated.PK));
			AssertEquals("Collection should not contain Exempt TaxRate", false, bizO.ChoicesCollection.Contains(exempt.PK));
			AssertEquals("Collection should not contain NotReportable TaxRate", false, bizO.ChoicesCollection.Contains(notReportable.PK));
			AssertEquals("Collection should contain Rated TaxRate", true, bizO.ChoicesCollection.Contains(rated.PK));
			AssertEquals("Collection should not contain ReverseRated TaxRate", false, bizO.ChoicesCollection.Contains(reverseRated.PK));

			editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccTaxRate, RegistryFindBoxFilter.AccTaxRateTypeReverseRated);
			bizO = new GuidFindBoxBusinessObject(companyLevel, editorInfo);
			((BusinessObjectCollection)bizO.ChoicesCollection).Load();
			AssertEquals("Collection should not contain CapitalRated TaxRate", false, bizO.ChoicesCollection.Contains(capitalRated.PK));
			AssertEquals("Collection should not contain Exempt TaxRate", false, bizO.ChoicesCollection.Contains(exempt.PK));
			AssertEquals("Collection should not contain NotReportable TaxRate", false, bizO.ChoicesCollection.Contains(notReportable.PK));
			AssertEquals("Collection should not contain Rated TaxRate", false, bizO.ChoicesCollection.Contains(rated.PK));
			AssertEquals("Collection should contain ReverseRated TaxRate", true, bizO.ChoicesCollection.Contains(reverseRated.PK));
		}

		ZQuery GetTaxRateFilterByType(ZString rateType)
		{
			ZQuery result = new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			result.AddToFilter(AccTaxRateSchema.AT_IsActive, ZBool.True);
			result.AddToFilter(AccTaxRateSchema.AT_Type, rateType);
			return result;
		}

		AccTaxRate CreateTaxRateOfType(ZString rateType)
		{
			AccTaxRate result = Factory.NewWithValidTestData<AccTaxRate>();
			result.AT_Type = rateType;
			result.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			return result;
		}

		#endregion

		#region TestNonMiscDepartment

		public void TestNonMiscDepartment()
		{
			var parent1 = Factory.NewWithValidTestData<GlbDepartment>();
			var parent2 = Factory.NewWithValidTestData<GlbDepartment>();
			var parent3 = Factory.NewWithValidTestData<GlbDepartment>();
			var child1 = Factory.NewWithValidTestData<GlbDepartment>();
			var child2 = Factory.NewWithValidTestData<GlbDepartment>();
			var child3 = Factory.NewWithValidTestData<GlbDepartment>();

			child1.GE_GE = parent1.PK;
			child2.GE_GE = parent2.PK;
			child3.GE_GE = parent3.PK;

			parent1.GE_Misc = true;
			parent2.GE_Misc = false;
			parent3.GE_Misc = false;
			child1.GE_Misc = false;
			child2.GE_Misc = true;
			child3.GE_Misc = false;

			Factory.Save();

			GuidFindBoxRegistryEditorInfo editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
			GuidFindBoxBusinessObject bizO = new GuidFindBoxBusinessObject(null, editorInfo);

			if (bizO.ChoicesCollection is BusinessObjectCollection)
			{
				((BusinessObjectCollection)bizO.ChoicesCollection).Load();
			}

			AssertEquals("ChoicesCollection.Contains(Parent1)", false, bizO.ChoicesCollection.Contains(parent1.PK));
			AssertEquals("ChoicesCollection.Contains(Parent2)", true, bizO.ChoicesCollection.Contains(parent2.PK));
			AssertEquals("ChoicesCollection.Contains(Parent3)", true, bizO.ChoicesCollection.Contains(parent3.PK));
			AssertEquals("ChoicesCollection.Contains(Child1)", false, bizO.ChoicesCollection.Contains(child1.PK));
			AssertEquals("ChoicesCollection.Contains(Child2)", false, bizO.ChoicesCollection.Contains(child2.PK));
			AssertEquals("ChoicesCollection.Contains(Child3)", true, bizO.ChoicesCollection.Contains(child3.PK));
		}

		#endregion

		#region TestStmPrintQueueOnlyContainsActivePrinters

		public void TestStmPrintQueueOnlyContainsActivePrinters()
		{
			IStmPrintQueue printer1 = (IStmPrintQueue)Factory.NewWithValidTestData(ObjectFactory.GetType<IStmPrintQueue>());
			IStmPrintQueue printer2 = (IStmPrintQueue)Factory.NewWithValidTestData(ObjectFactory.GetType<IStmPrintQueue>());

			printer1.SQ_AllowPrinting = true;
			printer2.SQ_AllowPrinting = false;

			Factory.Save();

			GuidFindBoxRegistryEditorInfo editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.StmPrintQueue);
			GuidFindBoxBusinessObject bizO = new GuidFindBoxBusinessObject(null, editorInfo);

			((BusinessObjectCollection)bizO.ChoicesCollection).Load();
			AssertEquals("ChoicesCollection.Contains(Printer1.PK)", true, bizO.ChoicesCollection.Contains(printer1.PK));
			AssertEquals("ChoicesCollection.Contains(Printer2.PK)", false, bizO.ChoicesCollection.Contains(printer2.PK));
		}

		#endregion

		#region TestTagMagnitude

		public void TestTagMagnitude()
		{
			var editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.TagMagnitude);
			var bizO = new GuidFindBoxBusinessObject(null, editorInfo);

			AssertEquals(ModuleIDs.BMTagMagnitude, bizO.ModuleID);
			Assert(typeof(ITagMagnitudeCollection).IsAssignableFrom(bizO.ChoicesCollection.GetType()));
		}

		#endregion

		#region TestPackingDocumentFilter

		public void TestPackingDocumentFilter()
		{
			var validCustomPackingDocument = Factory.NewWithValidTestData<StmMenuItem>();
			validCustomPackingDocument.SU_BusinessContext = nameof(BusinessContext.Package);

			var nonPackingDocument = Factory.NewWithValidTestData<StmMenuItem>();
			nonPackingDocument.SU_BusinessContext = nameof(BusinessContext.Order);

			var customPackingDocumentWithPreventAutoDeliveryOn = Factory.NewWithValidTestData<StmMenuItem>();
			customPackingDocumentWithPreventAutoDeliveryOn.SU_BusinessContext = nameof(BusinessContext.Package);
			customPackingDocumentWithPreventAutoDeliveryOn.SU_PreventAutoDelivery = true;

			var customPackingDocumentWithIncorrectDataContext = Factory.NewWithValidTestData<StmMenuItem>();
			customPackingDocumentWithIncorrectDataContext.SU_BusinessContext = nameof(BusinessContext.Package);

			var pivot1 = Factory.NewWithValidTestData<StmMenuTemplatePivot>();
			pivot1.SI_SU = validCustomPackingDocument.PK;

			var pivot2 = Factory.NewWithValidTestData<StmMenuTemplatePivot>();
			pivot2.SI_SU = nonPackingDocument.PK;

			var pivot3 = Factory.NewWithValidTestData<StmMenuTemplatePivot>();
			pivot3.SI_SU = customPackingDocumentWithPreventAutoDeliveryOn.PK;

			var pivot4 = Factory.NewWithValidTestData<StmMenuTemplatePivot>();
			pivot4.SI_SU = customPackingDocumentWithIncorrectDataContext.PK;

			Factory.Save();

			TestConnection.ExecuteNonQuery(string.Format(@"
INSERT INTO dbo.StmMenuDocumentConfig (S3_PK, S3_SI, S3_OverrideDataContext, S3_IsSystem, S3_Description)
VALUES (NewID(), '{0}', '{1}', 1, 'Test')", pivot1.PK.ToString(), nameof(Constants.DataContext.GenericBasicLabel)));

			TestConnection.ExecuteNonQuery(string.Format(@"
INSERT INTO dbo.StmMenuDocumentConfig (S3_PK, S3_SI, S3_OverrideDataContext, S3_IsSystem, S3_Description)
VALUES (NewID(), '{0}', '{1}', 1, 'Test')", pivot2.PK.ToString(), nameof(Constants.DataContext.GenericBasicLabel)));

			TestConnection.ExecuteNonQuery(string.Format(@"
INSERT INTO dbo.StmMenuDocumentConfig (S3_PK, S3_SI, S3_OverrideDataContext, S3_IsSystem, S3_Description)
VALUES (NewID(), '{0}', '{1}', 1, 'Test')", pivot3.PK.ToString(), nameof(Constants.DataContext.GenericBasicLabel)));

			TestConnection.ExecuteNonQuery(string.Format(@"
INSERT INTO dbo.StmMenuDocumentConfig (S3_PK, S3_SI, S3_OverrideDataContext, S3_IsSystem, S3_Description)
VALUES (NewID(), '{0}', '{1}', 1, 'Test')", pivot4.PK.ToString(), nameof(Constants.DataContext.GenericBasicLabelAll)));

			Factory.Save();

			var editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.PackingDocument);
			var bizO = new GuidFindBoxBusinessObject(null, editorInfo);

			var collection = (BusinessObjectCollection)bizO.ChoicesCollection;
			collection.Load();
			AssertEquals(ModuleIDs.StmMenuItem, bizO.ModuleID);
			AssertEquals(true, bizO.ChoicesCollection.Contains(validCustomPackingDocument.PK));
			AssertEquals(false, bizO.ChoicesCollection.Contains(nonPackingDocument.PK));
			AssertEquals(false, bizO.ChoicesCollection.Contains(customPackingDocumentWithPreventAutoDeliveryOn.PK));
			AssertEquals(false, bizO.ChoicesCollection.Contains(customPackingDocumentWithIncorrectDataContext.PK));

			AssertEquals("This Document cannot be chosen because 'Prevent Auto Delivery' is checked.",
				collection.GetAllNotificationsWhenAdditionalFilterNotMet(customPackingDocumentWithPreventAutoDeliveryOn));

			AssertEquals("This Document cannot be chosen because it prints multiple labels.",
				collection.GetAllNotificationsWhenAdditionalFilterNotMet(customPackingDocumentWithIncorrectDataContext));

			var businessContextDefault = collection.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().First(); // should blow up if null
			AssertEquals("Business Context", businessContextDefault.FilterName);
			AssertEquals(nameof(BusinessContext.Package), businessContextDefault.Value);
			AssertEquals(false, businessContextDefault.IsRemovable);
		}

		#endregion

		#region TestRefDocTypeForCommunicationParsedEmailFilter

		public void TestRefDocTypeForCommunicationParsedEmailFilter()
		{
			var docType1 = Factory.LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.MiscellaneousDocument));
			var docType2 = Factory.LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.DemandLetter));
			var docType3 = Factory.LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.FoodControlCertificate));

			Factory.Save();

			var editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.RefDocType, RegistryFindBoxFilter.RefDocTypeForCommunicationParsedEmail);
			var bizO = new GuidFindBoxBusinessObject(null, editorInfo);

			Assert("ChoicesCollection should contain doctype1", bizO.ChoicesCollection.Contains(docType1));
			Assert("ChoicesCollection should contain docType2", bizO.ChoicesCollection.Contains(docType2));
			Assert("ChoicesCollection should not contain docType3", !bizO.ChoicesCollection.Contains(docType3));
		}

		#endregion

		#region TestWhsWarehouseOnlyContainsActiveWhsWarehouse

		public void TestWhsWarehouseOnlyContainsActiveWhsWarehouse()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whsWarehouse1 = helper.CreateWarehouse("WHS1", "A");
			var whsWarehouse2 = helper.CreateWarehouse("WHS2", "B");

			whsWarehouse1[WhsWarehouseSchema.WW_IsActive] = true;
			whsWarehouse2[WhsWarehouseSchema.WW_IsActive] = false;

			Factory.Save();

			var editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.WhsWarehouse);
			var bizO = new GuidFindBoxBusinessObject(null, editorInfo);

			((BusinessObjectCollection)bizO.ChoicesCollection).Load();
			AssertEquals("ChoicesCollection.Contains(whsWarehouse1.PK)", true, bizO.ChoicesCollection.Contains(whsWarehouse1.PK));
			AssertEquals("ChoicesCollection.Contains(whsWarehouse2.PK)", false, bizO.ChoicesCollection.Contains(whsWarehouse2.PK));
		}

		#endregion

		#region Account Descriptors

		public void TestChineseSimplifiedAccountDescriptor()
		{
			GuidFindBoxRegistryEditorInfo editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccountDescriptors);
			editorInfo.FindBoxFilter = RegistryFindBoxFilter.ChineseSimplifiedAccountDescriptor;

			var accountDescriptor1 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			var accountDescriptor2 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			var accountDescriptor3 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();

			accountDescriptor1.AJ_ReportCategory = "TTL";
			accountDescriptor1.AJ_Language = Core.SharedConstants.Languages.ChineseTraditional;

			accountDescriptor2.AJ_ReportCategory = "TTL";
			accountDescriptor2.AJ_Language = Core.SharedConstants.Languages.ChineseSimplified;

			accountDescriptor3.AJ_ReportCategory = "HDR";
			accountDescriptor3.AJ_Language = Core.SharedConstants.Languages.ChineseSimplified;

			var accountDescriptor4 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			accountDescriptor4.AJ_ReportCategory = "HDR";
			accountDescriptor4.AJ_Language = Core.SharedConstants.Languages.ChineseSimplified;
			accountDescriptor4.AJ_ReportType = "ABC";

			Factory.Save();

			AssertEquals(AccGLAccountDescriptor.ReportTypeCOA, accountDescriptor1.AJ_ReportType);
			AssertEquals(AccGLAccountDescriptor.ReportTypeCOA, accountDescriptor2.AJ_ReportType);
			AssertEquals(AccGLAccountDescriptor.ReportTypeCOA, accountDescriptor3.AJ_ReportType);
			AssertEquals("ABC", accountDescriptor4.AJ_ReportType);

			FallbackLevel fallback = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
			GuidFindBoxBusinessObject guidBusinessObject = new GuidFindBoxBusinessObject(fallback, editorInfo);
			AccGLAccountDescriptorCollection collection = (AccGLAccountDescriptorCollection)guidBusinessObject.ChoicesCollection;
			collection.Load();

			Assert("ChoicesCollection should not contain AccountDescriptor1", !collection.Contains(accountDescriptor1));
			Assert("ChoicesCollection should not contain AccountDescriptor2", !collection.Contains(accountDescriptor2));
			Assert("ChoicesCollection should contain AccountDescriptor3", collection.Contains(accountDescriptor3));
			Assert("ChoicesCollection should not contain AccountDescriptor4", !collection.Contains(accountDescriptor4));
		}

		public void TestChineseTraditionalAccountDescriptor()
		{
			GuidFindBoxRegistryEditorInfo editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccountDescriptors);
			editorInfo.FindBoxFilter = RegistryFindBoxFilter.ChineseTraditionalAccountDescriptor;

			var accountDescriptor1 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			var accountDescriptor2 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			var accountDescriptor3 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();

			accountDescriptor1.AJ_ReportCategory = "TTL";
			accountDescriptor1.AJ_Language = Core.SharedConstants.Languages.ChineseSimplified;

			accountDescriptor2.AJ_ReportCategory = "TTL";
			accountDescriptor2.AJ_Language = Core.SharedConstants.Languages.ChineseTraditional;

			accountDescriptor3.AJ_ReportCategory = "HDR";
			accountDescriptor3.AJ_Language = Core.SharedConstants.Languages.ChineseTraditional;

			var accountDescriptor4 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			accountDescriptor4.AJ_ReportCategory = "HDR";
			accountDescriptor4.AJ_Language = Core.SharedConstants.Languages.ChineseTraditional;
			accountDescriptor4.AJ_ReportType = "ABC";
			Factory.Save();

			AssertEquals(AccGLAccountDescriptor.ReportTypeCOA, accountDescriptor1.AJ_ReportType);
			AssertEquals(AccGLAccountDescriptor.ReportTypeCOA, accountDescriptor2.AJ_ReportType);
			AssertEquals(AccGLAccountDescriptor.ReportTypeCOA, accountDescriptor3.AJ_ReportType);
			AssertEquals("ABC", accountDescriptor4.AJ_ReportType);

			FallbackLevel fallback = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
			GuidFindBoxBusinessObject guidBusinessObject = new GuidFindBoxBusinessObject(fallback, editorInfo);
			AccGLAccountDescriptorCollection collection = (AccGLAccountDescriptorCollection)guidBusinessObject.ChoicesCollection;
			collection.Load();

			Assert("ChoicesCollection should not contain AccountDescriptor1", !collection.Contains(accountDescriptor1));
			Assert("ChoicesCollection should not contain AccountDescriptor2", !collection.Contains(accountDescriptor2));
			Assert("ChoicesCollection should contain AccountDescriptor3", collection.Contains(accountDescriptor3));
			Assert("ChoicesCollection should not contain AccountDescriptor4", !collection.Contains(accountDescriptor4));
		}

		public void TestVietnameseAccountDescriptor()
		{
			GuidFindBoxRegistryEditorInfo editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccountDescriptors);
			editorInfo.FindBoxFilter = RegistryFindBoxFilter.VietnameseAccountDescriptor;

			var accountDescriptor1 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			var accountDescriptor2 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			var accountDescriptor3 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();

			accountDescriptor1.AJ_ReportCategory = "TTL";
			accountDescriptor1.AJ_Language = Constants.Languages.ChineseSimplified;

			accountDescriptor2.AJ_ReportCategory = "TTL";
			accountDescriptor2.AJ_Language = Constants.Languages.Vietnamese;

			accountDescriptor3.AJ_ReportCategory = "HDR";
			accountDescriptor3.AJ_Language = Constants.Languages.Vietnamese;

			var accountDescriptor4 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			accountDescriptor4.AJ_ReportCategory = "HDR";
			accountDescriptor4.AJ_Language = Constants.Languages.Vietnamese;
			accountDescriptor4.AJ_ReportType = "ABC";
			Factory.Save();

			AssertEquals(AccGLAccountDescriptor.ReportTypeCOA, accountDescriptor1.AJ_ReportType);
			AssertEquals(AccGLAccountDescriptor.ReportTypeCOA, accountDescriptor2.AJ_ReportType);
			AssertEquals(AccGLAccountDescriptor.ReportTypeCOA, accountDescriptor3.AJ_ReportType);
			AssertEquals("ABC", accountDescriptor4.AJ_ReportType);

			FallbackLevel fallback = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
			GuidFindBoxBusinessObject guidBusinessObject = new GuidFindBoxBusinessObject(fallback, editorInfo);
			AccGLAccountDescriptorCollection collection = (AccGLAccountDescriptorCollection)guidBusinessObject.ChoicesCollection;
			collection.Load();

			Assert("ChoicesCollection should not contain AccountDescriptor1", !collection.Contains(accountDescriptor1));
			Assert("ChoicesCollection should not contain AccountDescriptor2", !collection.Contains(accountDescriptor2));
			Assert("ChoicesCollection should contain AccountDescriptor3", collection.Contains(accountDescriptor3));
			Assert("ChoicesCollection should not contain AccountDescriptor4", !collection.Contains(accountDescriptor4));
		}

		#endregion

		#region Charge Codes

		public void TestDisbursementChargeCode()
		{
			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();

			chargeCode1.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode2.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode1.AC_ChargeType = "ABC";
			chargeCode2.AC_ChargeType = "DSB";

			Factory.Save();

			GuidFindBoxRegistryEditorInfo editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.AUCustomsQuarantineChargeCode);
			GuidFindBoxBusinessObject bizO = new GuidFindBoxBusinessObject(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), editorInfo);

			var collection = bizO.ChoicesCollection as AccChargeCodeCollection;
			collection.Load();
			AssertEquals("Collection should contain AccChargeCode with AC_ChargeType of 'DSB'.", true, collection.Contains(chargeCode2.PK));
			AssertEquals("Collection should not contain AccChargeCode with AC_ChargeType of 'ABC'.", false, collection.Contains(chargeCode1.PK));

			var businessContextDefault = collection.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().FirstOrDefault();
			AssertEquals("Charge Type", businessContextDefault.FilterName);
			AssertEquals("DSB", businessContextDefault.Value);
			Assert(!businessContextDefault.IsRemovable);
		}

		public void TestGlobalDSBOrMRGChargeCode()
		{
			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode4 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode5 = Factory.NewWithValidTestData<AccChargeCode>();

			chargeCode1.AC_GC = Guid.Empty;
			chargeCode1.AC_Desc = "Global DSB Charge Code";
			chargeCode1.AC_ChargeType = "DSB";

			chargeCode2.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode2.AC_ChargeType = "DSB";

			chargeCode3.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode3.AC_ChargeType = "ABC";

			chargeCode4.AC_GC = Guid.Empty;
			chargeCode4.AC_Desc = "Global MRG Charge Code";
			chargeCode4.AC_ChargeType = "MRG";

			chargeCode5.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode5.AC_ChargeType = "MRG";

			Factory.Save();

			var editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.GlobalDSBOrMRGChargeCode);
			var bizO = new GuidFindBoxBusinessObject(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), editorInfo);

			var collection = bizO.ChoicesCollection as AccChargeCodeCollection;
			collection.Load();
			AssertEquals("Collection should contain AccChargeCode with empty company and AC_ChargeType of 'DSB'.", true, collection.Contains(chargeCode1.PK));
			AssertEquals("Collection should contain AccChargeCode with empty company and AC_ChargeType of 'MRG'.", true, collection.Contains(chargeCode4.PK));
			AssertEquals("Collection should not contain AccChargeCode with AC_ChargeType of 'ABC'.", false, collection.Contains(chargeCode2.PK));
			AssertEquals("Collection should not contain AccChargeCode with specific company.", false, collection.Contains(chargeCode3.PK));
		}

		public void TestCustomDeferredChargeCode()
		{
			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();

			chargeCode1.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode2.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode1.AC_ChargeType = "ABC";
			chargeCode2.AC_ChargeType = "CMT";

			Factory.Save();

			GuidFindBoxRegistryEditorInfo editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.CustomDeferredChargeCode);
			GuidFindBoxBusinessObject bizO = new GuidFindBoxBusinessObject(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), editorInfo);

			((BusinessObjectCollection)bizO.ChoicesCollection).Load();
			AssertEquals("Collection should contain AccChargeCode with AC_ChargeType of 'CMT'.", true, bizO.ChoicesCollection.Contains(chargeCode2.PK));
			AssertEquals("Collection should not contain AccChargeCode with AC_ChargeType of 'ABC'.", false, bizO.ChoicesCollection.Contains(chargeCode1.PK));
		}

		public void TestMsgOrDsbChargeCode()
		{
			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode4 = Factory.NewWithValidTestData<AccChargeCode>();

			chargeCode1.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode2.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode3.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode4.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode1.AC_ChargeType = "DSB";
			chargeCode2.AC_ChargeType = "MRG";
			chargeCode3.AC_ChargeType = "CMT";
			chargeCode4.AC_ChargeType = "MJA";

			Factory.Save();

			GuidFindBoxRegistryEditorInfo editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.MrgDsbOrMjaChargeCode);
			GuidFindBoxBusinessObject bizO = new GuidFindBoxBusinessObject(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), editorInfo);

			((BusinessObjectCollection)bizO.ChoicesCollection).Load();
			AssertEquals("Collection should contain AccChargeCode with AC_ChargeType of 'DSB'.", true, bizO.ChoicesCollection.Contains(chargeCode1));
			AssertEquals("Collection should contain AccChargeCode with AC_ChargeType of 'MRG'.", true, bizO.ChoicesCollection.Contains(chargeCode2));
			AssertEquals("Collection should not contain AccChargeCode with AC_ChargeType of 'CMT'.", false, bizO.ChoicesCollection.Contains(chargeCode3));
			AssertEquals("Collection should contain AccChargeCode with AC_ChargeType of 'MJA'.", true, bizO.ChoicesCollection.Contains(chargeCode4));
		}

		public void TestFreightChargeCode()
		{
			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();

			chargeCode1.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode2.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode1.AC_ChargeGroup = "NGC";
			chargeCode2.AC_ChargeGroup = "FRT";

			Factory.Save();

			GuidFindBoxRegistryEditorInfo editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.FreightChargeCode);
			GuidFindBoxBusinessObject bizO = new GuidFindBoxBusinessObject(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), editorInfo);

			((BusinessObjectCollection)bizO.ChoicesCollection).Load();
			AssertEquals("Collection should contain AccChargeCode with AC_ChargeGroup of 'FRT'.", true, bizO.ChoicesCollection.Contains(chargeCode2.PK));
			AssertEquals("Collection should not contain AccChargeCode with AC_ChargeGroup of 'NGC'.", false, bizO.ChoicesCollection.Contains(chargeCode1.PK));
		}

		public void TestNonJobRelatedChargeCode()
		{
			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();

			chargeCode1.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode2.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode3.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode1.AC_ChargeType = "OVR";
			chargeCode2.AC_ChargeType = "MRG";
			chargeCode3.AC_ChargeType = "NON";

			Factory.Save();

			var editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.NonJobRelatedChargeCode);
			var bizO = new GuidFindBoxBusinessObject(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), editorInfo);

			((BusinessObjectCollection)bizO.ChoicesCollection).Load();
			AssertEquals("Collection should contain AccChargeCode with AC_ChargeType of 'OVR'.", true, bizO.ChoicesCollection.Contains(chargeCode1.PK));
			AssertEquals("Collection should not contain AccChargeCode with AC_ChargeType of 'MRG'.", false, bizO.ChoicesCollection.Contains(chargeCode2.PK));
			AssertEquals("Collection should contain AccChargeCode with AC_ChargeType of 'NON'.", true, bizO.ChoicesCollection.Contains(chargeCode3.PK));
		}

		public void TestRevenueOrNonJobRelatedChargeCode()
		{
			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();

			chargeCode1.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode2.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode3.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode1.AC_ChargeType = "REV";
			chargeCode2.AC_ChargeType = "NON";
			chargeCode3.AC_ChargeType = "MRG";

			Factory.Save();

			var editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.RevenueOrNonJobRelatedChargeCode);
			var bizO = new GuidFindBoxBusinessObject(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), editorInfo);

			((BusinessObjectCollection)bizO.ChoicesCollection).Load();
			AssertEquals("Collection should contain AccChargeCode with AC_ChargeType of 'REV'.", true, bizO.ChoicesCollection.Contains(chargeCode1.PK));
			AssertEquals("Collection should contain AccChargeCode with AC_ChargeType of 'NON'.", true, bizO.ChoicesCollection.Contains(chargeCode2.PK));
			AssertEquals("Collection should not contain AccChargeCode with AC_ChargeType of 'MRG'.", false, bizO.ChoicesCollection.Contains(chargeCode3.PK));
		}

		public void TestRevenueChargeCode()
		{
			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();

			chargeCode1.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode2.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode3.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode1.AC_ChargeType = "REV";
			chargeCode2.AC_ChargeType = "NON";
			chargeCode3.AC_ChargeType = "MRG";

			Factory.Save();

			var editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.RevenueChargeCode);
			var bizO = new GuidFindBoxBusinessObject(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), editorInfo);

			((BusinessObjectCollection)bizO.ChoicesCollection).Load();
			AssertEquals("Does Collection contain AccChargeCode with AC_ChargeType of 'REV'?", true, bizO.ChoicesCollection.Contains(chargeCode1.PK));
			AssertEquals("Does Collection contain AccChargeCode with AC_ChargeType of 'NON'?", false, bizO.ChoicesCollection.Contains(chargeCode2.PK));
			AssertEquals("Does Collection contain AccChargeCode with AC_ChargeType of 'MRG'?", false, bizO.ChoicesCollection.Contains(chargeCode3.PK));
		}

		public void TestAccountFeeDefaultTaxID()
		{
			var editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccTaxRate, RegistryFindBoxFilter.VATTaxSystem);
			var bizO = new GuidFindBoxBusinessObject(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), editorInfo);

			((BusinessObjectCollection)bizO.ChoicesCollection).Load();
			Assert("Collection should have VAT Tax System", bizO.ChoicesCollection.Cast<AccTaxRate>().All(item => item.AT_TaxSystemCode.IsEmpty));
		}

		public void TestAccChargeCodeAddidtionalFilterContainsGCAndIsActiveFilter()
		{
			var editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.MrgDsbOrMjaChargeCode);
			var bizO = new GuidFindBoxBusinessObject(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), editorInfo);
			var collection = bizO.ChoicesCollection as BusinessObjectCollection;
			var additionalFilter = ((ILegacyBusinessObjectCollectionInternals)collection).AdditionalFilter;
			AssertEquals("ChargeCode AdditionalFilter contains AC_GC filter - AccChargeCode-MrgDsbOrMjaChargeCode", true, additionalFilter.Params.Any(param => param.SchemaColumn == AccChargeCodeSchema.AC_GC && (Guid)param.Value == Env.CurrentCompany.PK));
			AssertEquals("ChargeCode AdditionalFilter contains AC_IsActive filter - AccChargeCode-MrgDsbOrMjaChargeCode", true, additionalFilter.Params.Any(param => param.SchemaColumn == AccChargeCodeSchema.AC_IsActive && (int)param.Value == 1));

			editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.None);
			bizO = new GuidFindBoxBusinessObject(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), editorInfo);
			collection = bizO.ChoicesCollection as BusinessObjectCollection;
			additionalFilter = ((ILegacyBusinessObjectCollectionInternals)collection).AdditionalFilter;
			AssertEquals("ChargeCode AdditionalFilter contains AC_GC filter - AccChargeCode-None", true, additionalFilter.Params.Any(param => param.SchemaColumn == AccChargeCodeSchema.AC_GC && (Guid)param.Value == Env.CurrentCompany.PK));
			AssertEquals("ChargeCode AdditionalFilter contains AC_IsActive filter - AccChargeCode-None", true, additionalFilter.Params.Any(param => param.SchemaColumn == AccChargeCodeSchema.AC_IsActive && (int)param.Value == 1));
		}

		#endregion

		#region GLBBranch

		public void TestGlbBranchWithCompanyFallbackHasFilterBussinessObjectDefaultForCompany()
		{
			var companyLevel = new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbBranch);
			var obj = new GuidFindBoxBusinessObject(companyLevel, editorInfo);
			var choicesCollection = ((BusinessObjectCollection)obj.ChoicesCollection);
			AssertHasDefault(choicesCollection, "Company", "Property", GlbCompany.CurrentCompany.PK);
		}

		public void TestGlbBranchAtSystemLevel()
		{
			var systemLevel = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
			var editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbBranch);
			var obj = new GuidFindBoxBusinessObject(systemLevel, editorInfo);
			var choicesCollection = ((BusinessObjectCollection)obj.ChoicesCollection);
			AssertHasDefault(choicesCollection, "Company", "Property", ZGuid.Empty);
		}

		#endregion

		#region Collection and Module ID

		public void TestAccBankAccount()
		{
			TestCollectionAndModuleID(RegistryFindBoxCollection.AccBankAccount, RegistryFindBoxFilter.None, typeof(AccBankAccountCollection), ModuleIDs.AccBankAccount);
		}

		public void TestAccChargeCode()
		{
			TestCollectionAndModuleID(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.None, typeof(AccChargeCodeCollection), ModuleIDs.AccChargeCodeForRegistry);
		}

		public void TestAccGLHeader()
		{
			TestCollectionAndModuleID(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.None, typeof(AccGLHeaderCollection), ModuleIDs.AccGLHeader);
		}

		public void TestAccountDescriptors()
		{
			TestCollectionAndModuleID(RegistryFindBoxCollection.AccountDescriptors, RegistryFindBoxFilter.None, typeof(AccGLAccountDescriptorCollection), ModuleIDs.AccGLAccountDescriptor);
		}

		public void TestBroker()
		{
			TestCollectionAndModuleID(RegistryFindBoxCollection.Broker, RegistryFindBoxFilter.None, typeof(BrokerCollection), ModuleIDs.Organisation);
		}

		public void TestDebtor()
		{
			TestCollectionAndModuleID(RegistryFindBoxCollection.Debtor, RegistryFindBoxFilter.None, typeof(DebtorCollection), ModuleIDs.Organisation);
		}

		public void TestFumigationContractors()
		{
			TestCollectionAndModuleID(RegistryFindBoxCollection.FumigationContractors, RegistryFindBoxFilter.None, typeof(FumigationContractorCollection), ModuleIDs.Organisation);
		}

		public void TestGlbBranchNotCurrentCompanyRelated()
		{
			TestCollectionAndModuleID(RegistryFindBoxCollection.GlbBranchNotCurrentCompanyRelated, RegistryFindBoxFilter.None, typeof(GlbBranchNotCurrentCompanyRelatedCollection), ModuleIDs.GlbBranchNotCurrentCompanyRelated);
		}

		public void TestGlbBranch()
		{
			TestCollectionAndModuleID(RegistryFindBoxCollection.GlbBranch, RegistryFindBoxFilter.None, typeof(GlbBranchCollection), ModuleIDs.GlbBranch);
		}

		public void TestGlbCompany()
		{
			TestCollectionAndModuleID(RegistryFindBoxCollection.GlbCompany, RegistryFindBoxFilter.None, typeof(GlbCompanyCollection), ModuleIDs.GlbCompany);
		}

		public void TestGlbDepartment()
		{
			TestCollectionAndModuleID(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.None, typeof(GlbDepartmentCollection), ModuleIDs.GlbDepartment);
		}

		public void TestGlbGroup()
		{
			TestCollectionAndModuleID(RegistryFindBoxCollection.GlbGroup, RegistryFindBoxFilter.None, typeof(GlbGroupCollection), ModuleIDs.GlbGroup);
		}

		public void TestGlbStaff()
		{
			TestCollectionAndModuleID(RegistryFindBoxCollection.GlbStaff, RegistryFindBoxFilter.None, typeof(GlbStaffCollection), ModuleIDs.GlbStaff);
		}

		public void TestOrgCreditorGroup()
		{
			TestCollectionAndModuleID(RegistryFindBoxCollection.OrgCreditorGroup, RegistryFindBoxFilter.None, typeof(OrgCreditorGroupCollection), ModuleIDs.OrgCreditorGroup);
		}

		public void TestOrgDebtorGroup()
		{
			TestCollectionAndModuleID(RegistryFindBoxCollection.OrgDebtorGroup, RegistryFindBoxFilter.None, typeof(OrgDebtorGroupCollection), ModuleIDs.OrgDebtorGroup);
		}

		public void TestOrgHeader()
		{
			TestCollectionAndModuleID(RegistryFindBoxCollection.OrgHeader, RegistryFindBoxFilter.None, typeof(OrgHeaderCollection), ModuleIDs.Organisation);
		}

		public void TestRefCommodityCode()
		{
			TestCollectionAndModuleID(RegistryFindBoxCollection.RefCommodityCode, RegistryFindBoxFilter.None, typeof(RefCommodityCodeCollection), ModuleIDs.RefCommodityCode);
		}

		public void TestRefCurrency()
		{
			TestCollectionAndModuleID(RegistryFindBoxCollection.RefCurrency, RegistryFindBoxFilter.None, typeof(RefCurrencyCollection), ModuleIDs.RefCurrency);
		}

		public void TestRefServiceLevel()
		{
			TestCollectionAndModuleID(RegistryFindBoxCollection.RefServiceLevel, RegistryFindBoxFilter.None, typeof(RefServiceLevelCollection), ModuleIDs.ServiceLevel);
		}

		public void TestShippingProvider()
		{
			TestCollectionAndModuleID(RegistryFindBoxCollection.ShippingProvider, RegistryFindBoxFilter.None, typeof(ShippingProviderCollection), ModuleIDs.Organisation);
		}

		public void TestStmPrintQueue()
		{
			TestCollectionAndModuleID(RegistryFindBoxCollection.StmPrintQueue, RegistryFindBoxFilter.None, ObjectFactory.GetType<IStmPrintQueueCollection>(), ModuleIDs.PrintQueue);
		}

		public void TestWhsWareouse()
		{
			TestCollectionAndModuleID(RegistryFindBoxCollection.WhsWarehouse, RegistryFindBoxFilter.None, ObjectFactory.GetType<IWhsWarehouseCollection>(), ModuleIDs.WhsConfigWarehouse);
		}

		public void TestOrgContact()
		{
			TestCollectionAndModuleID(RegistryFindBoxCollection.OrgContact, RegistryFindBoxFilter.None, typeof(OrgContactCollection), ModuleIDs.OrgContacts);
		}

		public void TestZACustomsOffice()
		{
			TestCollectionAndModuleID(RegistryFindBoxCollection.ZACustomsOffice, RegistryFindBoxFilter.None,
				ObjectFactory.GetType<Enterprise.Integration.Customs.ZA.IZARefCusCodeListCollection>(), ModuleIDs.Customs.Universal.ZZRefCusCodeList);
		}

		public void TestRefDocType()
		{
			TestCollectionAndModuleID(RegistryFindBoxCollection.RefDocType, RegistryFindBoxFilter.None, typeof(RefDocTypeCollection), ModuleIDs.RefDocType);
		}

		public void TestAccInvMsg()
		{
			TestCollectionAndModuleID(RegistryFindBoxCollection.AccInvMsg, RegistryFindBoxFilter.None, typeof(AccInvMsgCollection), ModuleIDs.AccInvMsg);

			var editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccInvMsg);
			var fallback = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
			var bizO = new GuidFindBoxBusinessObject(fallback, editorInfo);

			AssertEquals("Collection Type", typeof(AccInvMsgCollection), bizO.ChoicesCollection.GetType());
			AssertEquals("Module ID", ModuleIDs.AccInvMsg, bizO.ModuleID);
		}

		public void TestAccAlternateChart()
		{
			var editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccAlternateChart, RegistryFindBoxFilter.None);
			var nonCurrentCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			var fallback = new FallbackLevel(((ICompany)nonCurrentCompany).PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var bizO = new GuidFindBoxBusinessObject(fallback, editorInfo);

			AssertEquals("Collection Type", typeof(AccAlternateChartCollection), bizO.ChoicesCollection.GetType());
			AssertEquals("Module ID", ModuleIDs.AlternateChartofAccounts, bizO.ModuleID);

			var filter = ((IBusinessObjectCollectionTestingMembers)bizO.ChoicesCollection).GetAdditionalFilter();
			var query = new ZQuery(AccAlternateChartSchema.AAC_GC_Company, fallback.CompanyPK(false));
			query.AddToFilter(JoinCondition.Or, AccAlternateChartSchema.AAC_GC_Company, null);
			AssertEquals(query.FilterString, filter.FilterString);
		}

		void TestCollectionAndModuleID(RegistryFindBoxCollection findBoxCollection, RegistryFindBoxFilter findBoxFilter, Type expectedCollectionType, ModuleIdentifier expectedModuleID)
		{
			GuidFindBoxRegistryEditorInfo editorInfo = new GuidFindBoxRegistryEditorInfo(findBoxCollection, findBoxFilter);
			GuidFindBoxBusinessObject bizO = new GuidFindBoxBusinessObject(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), editorInfo);

			AssertEquals("Collection Type", expectedCollectionType, bizO.ChoicesCollection.GetType());
			AssertEquals("Module ID", expectedModuleID, bizO.ModuleID);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			GuidFindBoxRegistryEditorInfo editorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.OrgHeader);
			FallbackLevel fallback = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
			return new GuidFindBoxBusinessObject(fallback, editorInfo);
		}

		#endregion
	}
}
