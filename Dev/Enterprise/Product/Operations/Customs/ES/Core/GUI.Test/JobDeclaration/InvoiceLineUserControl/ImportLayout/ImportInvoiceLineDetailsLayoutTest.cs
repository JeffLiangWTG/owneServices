using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing;

[TestedType(typeof(ImportInvoiceLineDetailsLayout))]
sealed class ImportInvoiceLineDetailsLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 3;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
			yield return ThirdColumnControls;
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CommonInvoiceLineDetailsControlBag.Instance.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CommodityCodeFindBox, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CountryOfOriginDropEdit, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.FormattedWithDescriptionTariffFindBox, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.DescriptionLongTextControl, ControlWidthClass.Long);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.SupplementaryCode1DropEdit, ControlWidthClass.Medium);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.SupplementaryCode2DropEdit, ControlWidthClass.Medium);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.AdditionalSupplementaryCodesAndGDMUserControl, ControlWidthClass.Long);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.PreferenceCodeDropEdit, ControlWidthClass.Long);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.FormattedProcedureCodeFindBox, ControlWidthClass.Long);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.AdditionalProcedureCodesUserControl, ControlWidthClass.Long);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.QuotaWithCheckLinkUserControl, ControlWidthClass.Auto);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (InvoiceLineDetailsControlBag.Instance.T2LItemNumberCalcEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.WeightCalcDropEdit, ControlWidthClass.Medium);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Medium);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.LinePriceCurrencyCalcFindBox, ControlWidthClass.Medium);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Medium);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Medium);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsQuantityCalcDropEdit, ControlWidthClass.Medium);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsFourthQuantityCalcDropEdit, ControlWidthClass.Medium);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.InvoiceQuantityCalcDropEdit, ControlWidthClass.Medium);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Medium);
			yield return (InvoiceLineDetailsControlBag.Instance.CommercialReferenceTextBox, ControlWidthClass.Long);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.CountryOfSupplyCodeFindBox, ControlWidthClass.Long);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.DispatchCodeFindBox, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.CountryOfDestinationCodeFindBox, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.RegionOfDestinationCodeFindBox, ControlWidthClass.Long);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.RegionOfDestinationDropEdit, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
	{
		get
		{
			yield return (InvoiceLineDetailsControlBag.Instance.MethodOfPaymentDropEdit, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.MethodOfPayment2DropEdit, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.VATIGICTypeDropEdit, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.AIEMTypeDropEdit, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.ExciseExemptionDropEdit, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.ExciseCodeDropEdit, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.GlobalWarmingPotentialCalcEdit, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.PVPCalcFindBox, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.REAProductCodeDropEdit, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.READirectConsumptionCheckBox, ControlWidthClass.Auto);
			yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.CusNumberCodeFindBox, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.HasNonRecycledPlasticsCheckBox, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.ValuationCodeDropEdit, ControlWidthClass.Long);
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new ExportInvoiceLineDetailsLayoutBuilder();

	public void TestRegionOfDestinationDropEditVisibility() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.ZG_CountryOfDestination = Core.Constants.CountryCodes.Spain;

		using (Business.Testing.CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
		{
			AssertFieldVisibility<ZDropEdit>(true, "RegionOfDestinationDropEdit Visible for IMP, H1 active and ZG_CountryOfDestination = ES", declaration, "RegionOfDestinationDropEdit");
			invoiceLine.ZG_CountryOfDestination = Core.Constants.CountryCodes.Germany;
			AssertFieldVisibility<ZDropEdit>(false, "RegionOfDestinationDropEdit not Visible for IMP, H1 active and ZG_CountryOfDestination = DE", declaration, "RegionOfDestinationDropEdit");
			invoiceLine.ZG_CountryOfDestination = Core.Constants.NonStandardCountryCodes.Codes.XC;
			AssertFieldVisibility<ZDropEdit>(true, "RegionOfDestinationDropEdit Visible for IMP, H1 active and ZG_CountryOfDestination = XC", declaration, "RegionOfDestinationDropEdit");
			invoiceLine.ZG_CountryOfDestination = Core.Constants.CountryCodes.Italy;
			AssertFieldVisibility<ZDropEdit>(false, "RegionOfDestinationDropEdit not Visible for IMP, H1 active and ZG_CountryOfDestination = IT", declaration, "RegionOfDestinationDropEdit");
			invoiceLine.ZG_CountryOfDestination = Core.Constants.NonStandardCountryCodes.Codes.XL;
			AssertFieldVisibility<ZDropEdit>(true, "RegionOfDestinationDropEdit Visible for IMP, H1 active and ZG_CountryOfDestination = XL", declaration, "RegionOfDestinationDropEdit");
			invoiceLine.ZG_CountryOfDestination = Core.Constants.CountryCodes.France;
			AssertFieldVisibility<ZDropEdit>(false, "RegionOfDestinationDropEdit not Visible for IMP, H1 active and ZG_CountryOfDestination = FR", declaration, "RegionOfDestinationDropEdit");
			invoiceLine.ZG_CountryOfDestination = ZString.Empty;
			AssertFieldVisibility<ZDropEdit>(true, "RegionOfDestinationDropEdit Visible for IMP, H1 active and ZG_CountryOfDestination = Empty", declaration, "RegionOfDestinationDropEdit");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertFieldVisibility<ZDropEdit>(false, "RegionOfDestinationDropEdit not Visible for EXP and H1 active", declaration, "RegionOfDestinationDropEdit");
		}

		using (Business.Testing.CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(false))
		{
			AssertFieldVisibility<ZDropEdit>(false, "RegionOfDestinationDropEdit not Visible for EXP and H1 not active", declaration, "RegionOfDestinationDropEdit");
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertFieldVisibility<ZDropEdit>(false, "RegionOfDestinationDropEdit not Visible for IMP and H1 not active", declaration, "RegionOfDestinationDropEdit");
		}
	});

	public void TestRegionOfDestinationCodeFindBoxVisibility() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.ZG_CountryOfDestination = Core.Constants.CountryCodes.Spain;

		using (Business.Testing.CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
		{
			AssertFieldVisibility<ZCodeFindBox>(false, "RegionOfDestinationCodeFindBox not Visible for IMP, H1 active and ZG_CountryOfDestination = ES", declaration, "RegionOfDestinationCodeFindBox");
			invoiceLine.ZG_CountryOfDestination = Core.Constants.CountryCodes.Germany;
			AssertFieldVisibility<ZCodeFindBox>(true, "RegionOfDestinationCodeFindBox Visible for IMP, H1 active and ZG_CountryOfDestination = DE", declaration, "RegionOfDestinationCodeFindBox");
			invoiceLine.ZG_CountryOfDestination = Core.Constants.NonStandardCountryCodes.Codes.XC;
			AssertFieldVisibility<ZCodeFindBox>(false, "RegionOfDestinationCodeFindBox not Visible for IMP, H1 active and ZG_CountryOfDestination = XC", declaration, "RegionOfDestinationCodeFindBox");
			invoiceLine.ZG_CountryOfDestination = Core.Constants.CountryCodes.Italy;
			AssertFieldVisibility<ZCodeFindBox>(true, "RegionOfDestinationCodeFindBox Visible for IMP, H1 active and ZG_CountryOfDestination = IT", declaration, "RegionOfDestinationCodeFindBox");
			invoiceLine.ZG_CountryOfDestination = Core.Constants.NonStandardCountryCodes.Codes.XL;
			AssertFieldVisibility<ZCodeFindBox>(false, "RegionOfDestinationCodeFindBox not Visible for IMP, H1 active and ZG_CountryOfDestination = XL", declaration, "RegionOfDestinationCodeFindBox");
			invoiceLine.ZG_CountryOfDestination = Core.Constants.CountryCodes.France;
			AssertFieldVisibility<ZCodeFindBox>(true, "RegionOfDestinationCodeFindBox Visible for IMP, H1 active and ZG_CountryOfDestination = FR", declaration, "RegionOfDestinationCodeFindBox");
			invoiceLine.ZG_CountryOfDestination = ZString.Empty;
			AssertFieldVisibility<ZCodeFindBox>(false, "RegionOfDestinationCodeFindBox not Visible for IMP, H1 active and ZG_CountryOfDestination = Empty", declaration, "RegionOfDestinationCodeFindBox");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertFieldVisibility<ZCodeFindBox>(false, "RegionOfDestinationCodeFindBox not Visible for EXP and H1 active", declaration, "RegionOfDestinationCodeFindBox");
		}
		using (Business.Testing.CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(false))
		{
			AssertFieldVisibility<ZCodeFindBox>(false, "RegionOfDestinationCodeFindBox not Visible for EXP and H1 not active", declaration, "RegionOfDestinationCodeFindBox");
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertFieldVisibility<ZCodeFindBox>(false, "RegionOfDestinationCodeFindBox not Visible for IMP and H1 not active", declaration, "RegionOfDestinationCodeFindBox");
		}
	});

	public void TestREAFieldsVisibility() => CombineAssertions(() =>
	{
		var helper = new Business.Testing.ESUniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, "61", "Test 61");

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		declaration.ZG_DestinationState = "zz";
		var invoiceLine = invoice.InvoiceLines.AddNew();

		using (Business.Testing.CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(false))
		{
			AssertFieldVisibility<ZDropEdit>(false, "REA is not visible when wrong canary islands code in ZG_DestinationState", declaration, "REAProductCodeDropEdit");
			AssertFieldVisibility<ZCheckBox>(false, "READirectConsumption is not visible when import and canary islands code in ZG_DestinationState", declaration, "READirectConsumptionCheckBox");

			declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
			AssertFieldVisibility<ZDropEdit>(true, "REA is visible when import and canary islands code in ZG_DestinationState", declaration, "REAProductCodeDropEdit");
			AssertFieldVisibility<ZCheckBox>(true, "READirectConsumption is visible when import and canary islands code in ZG_DestinationState", declaration, "READirectConsumptionCheckBox");

			declaration.ZG_DestinationState = "zz";
			declaration.JE_CustomsOffice = "ES003861";
			AssertFieldVisibility<ZDropEdit>(false, "REA is not visible when JE_CustomsOffice = ES003861, Import and not UCC6", declaration, "REAProductCodeDropEdit");
			AssertFieldVisibility<ZCheckBox>(false, "READirectConsumption is not visible when JE_CustomsOffice = ES003861, Import and not UCC6", declaration, "READirectConsumptionCheckBox");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertFieldVisibility<ZDropEdit>(false, "REA is not visible when ZG_DestinationState no IsCanaryIsland, Export and not UCC6", declaration, "REAProductCodeDropEdit");
			AssertFieldVisibility<ZCheckBox>(false, "READirectConsumption is not visible when ZG_DestinationState no IsCanaryIsland, Export and not UCC6", declaration, "READirectConsumptionCheckBox");

			AssertFieldVisibility<ZDropEdit>(false, "REA is not visible when JE_CustomsOffice = ES003861, Export and not UCC6", declaration, "REAProductCodeDropEdit");
			AssertFieldVisibility<ZCheckBox>(false, "READirectConsumption is not visible when JE_CustomsOffice = ES003861, Export and not UCC6", declaration, "READirectConsumptionCheckBox");
		}

		using (Business.Testing.CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
		{
			declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
			AssertFieldVisibility<ZDropEdit>(false, "REA is not visible when ZG_DestinationState IsCanaryIsland, Export and UCC6", declaration, "REAProductCodeDropEdit");
			AssertFieldVisibility<ZCheckBox>(false, "READirectConsumption is not visible when ZG_DestinationState IsCanaryIsland, Export and UCC6", declaration, "READirectConsumptionCheckBox");

			AssertFieldVisibility<ZDropEdit>(false, "REA is not visible when JE_CustomsOffice = ES003861, Export and UCC6", declaration, "REAProductCodeDropEdit");
			AssertFieldVisibility<ZCheckBox>(false, "READirectConsumption is not visible when JE_CustomsOffice = ES003861, Export and UCC6", declaration, "READirectConsumptionCheckBox");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertFieldVisibility<ZDropEdit>(true, "REA is visible when JE_CustomsOffice = ES003861, Import and UCC6", declaration, "REAProductCodeDropEdit");
			AssertFieldVisibility<ZCheckBox>(true, "READirectConsumption is visible when JE_CustomsOffice = ES003861, Import and UCC6", declaration, "READirectConsumptionCheckBox");

			declaration.JE_CustomsOffice = ZString.Empty;
			AssertFieldVisibility<ZDropEdit>(false, "REA is not visible when JE_CustomsOffice = Empty, Import and UCC6", declaration, "REAProductCodeDropEdit");
			AssertFieldVisibility<ZCheckBox>(false, "READirectConsumption is not visible when JE_CustomsOffice = Empty, Import and UCC6", declaration, "READirectConsumptionCheckBox");

			AssertFieldVisibility<ZDropEdit>(false, "REA is not visible when ZG_DestinationState IsCanaryIsland, Import and UCC6", declaration, "REAProductCodeDropEdit");
			AssertFieldVisibility<ZCheckBox>(false, "READirectConsumption is not visible when ZG_DestinationState IsCanaryIsland, Import and UCC6", declaration, "READirectConsumptionCheckBox");

			declaration.JE_CustomsOffice = "ES003541";
			AssertFieldVisibility<ZDropEdit>(true, "REA is visible when JE_CustomsOffice = ES003541, Import and UCC6", declaration, "REAProductCodeDropEdit");
			AssertFieldVisibility<ZCheckBox>(true, "READirectConsumption is visible when JE_CustomsOffice = ES003541, Import and UCC6", declaration, "READirectConsumptionCheckBox");

			declaration.JE_CustomsOffice = "ES003712";
			AssertFieldVisibility<ZDropEdit>(false, "REA is not visible when JE_CustomsOffice = ES003712, Import and UCC6", declaration, "REAProductCodeDropEdit");
			AssertFieldVisibility<ZCheckBox>(false, "READirectConsumption is not visible when JE_CustomsOffice = ES003712, Import and UCC6", declaration, "READirectConsumptionCheckBox");

			declaration.JE_CustomsOffice = "ES009998";
			AssertFieldVisibility<ZDropEdit>(true, "REA is visible when JE_CustomsOffice = ES009998, Import and UCC6", declaration, "REAProductCodeDropEdit");
			AssertFieldVisibility<ZCheckBox>(true, "READirectConsumption is visible when JE_CustomsOffice = ES009998, Import and UCC6", declaration, "READirectConsumptionCheckBox");
		}

		invoiceLine.JI_JZ = ZGuid.Missing;
		AssertFieldVisibility<ZDropEdit>(false, "REA is not visible when no invoice Header", declaration, "REAProductCodeDropEdit");
		AssertFieldVisibility<ZCheckBox>(false, "READirectConsumption is not visible when no invoice Header", declaration, "READirectConsumptionCheckBox");
	});

	public void TestAIEMTypeDropEdit() => CombineAssertions(() =>
	{
		var helper = new Business.Testing.ESUniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, "61", "Test 61");

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		declaration.ZG_DestinationState = "zz";
		var invoiceLine = invoice.InvoiceLines.AddNew();

		using (Business.Testing.CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(false))
		{
			AssertFieldVisibility<ZDropEdit>(false, "AIEMType is not visible when ZG_DestinationState no IsCanaryIsland, Import and not UCC6", declaration, "AIEMTypeDropEdit");

			declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
			AssertFieldVisibility<ZDropEdit>(true, "AIEMType is visible when ZG_DestinationState IsCanaryIsland, Import and not UCC6", declaration, "AIEMTypeDropEdit");

			declaration.ZG_DestinationState = "zz";
			declaration.JE_CustomsOffice = "ES003861";
			AssertFieldVisibility<ZDropEdit>(false, "AIEMType is not visible when JE_CustomsOffice = ES003861, Import and not UCC6", declaration, "AIEMTypeDropEdit");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertFieldVisibility<ZDropEdit>(false, "AIEMType is not visible when ZG_DestinationState no IsCanaryIsland, Export and not UCC6", declaration, "AIEMTypeDropEdit");

			AssertFieldVisibility<ZDropEdit>(false, "AIEMType is not visible when JE_CustomsOffice = ES003861, Export and not UCC6", declaration, "AIEMTypeDropEdit");
		}

		using (Business.Testing.CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
		{
			declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
			AssertFieldVisibility<ZDropEdit>(false, "AIEMType is not visible when ZG_DestinationState IsCanaryIsland, Export and UCC6", declaration, "AIEMTypeDropEdit");

			AssertFieldVisibility<ZDropEdit>(false, "AIEMType is not visible when JE_CustomsOffice = ES003861, Export and UCC6", declaration, "AIEMTypeDropEdit");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertFieldVisibility<ZDropEdit>(true, "AIEMType is visible when JE_CustomsOffice = ES003861, Import and UCC6", declaration, "AIEMTypeDropEdit");

			declaration.JE_CustomsOffice = ZString.Empty;
			AssertFieldVisibility<ZDropEdit>(false, "AIEMType is not visible when JE_CustomsOffice = Empty, Import and UCC6", declaration, "AIEMTypeDropEdit");

			AssertFieldVisibility<ZDropEdit>(false, "AIEMType is not visible when ZG_DestinationState IsCanaryIsland, Import and UCC6", declaration, "AIEMTypeDropEdit");

			declaration.JE_CustomsOffice = "ES003541";
			AssertFieldVisibility<ZDropEdit>(true, "AIEMType is visible when JE_CustomsOffice = ES003541, Import and UCC6", declaration, "AIEMTypeDropEdit");

			declaration.JE_CustomsOffice = "ES003712";
			AssertFieldVisibility<ZDropEdit>(false, "AIEMType is not visible when JE_CustomsOffice = ES003712, Import and UCC6", declaration, "AIEMTypeDropEdit");

			declaration.JE_CustomsOffice = "ES009998";
			AssertFieldVisibility<ZDropEdit>(true, "AIEMType is visible when JE_CustomsOffice = ES009998, Import and UCC6", declaration, "AIEMTypeDropEdit");
		}

		invoiceLine.JI_JZ = ZGuid.Missing;
		AssertFieldVisibility<ZDropEdit>(false, "AIEMType is not visible when no Invoice Header", declaration, "AIEMTypeDropEdit");
	});

	public void TestMethodOfPayment2DropEdit() => CombineAssertions(() =>
	{
		var helper = new Business.Testing.ESUniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, "61", "Test 61");

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		declaration.ZG_DestinationState = "zz";
		var invoiceLine = invoice.InvoiceLines.AddNew();

		using (Business.Testing.CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(false))
		{
			AssertFieldVisibility<ZDropEdit>(false, "MethodOfPayment2 is not visible when ZG_DestinationState no IsCanaryIsland, Import and not UCC6", declaration, "MethodOfPayment2DropEdit");

			declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
			AssertFieldVisibility<ZDropEdit>(true, "MethodOfPayment2 is visible when ZG_DestinationState IsCanaryIsland, Import and not UCC6", declaration, "MethodOfPayment2DropEdit");

			declaration.ZG_DestinationState = "zz";
			declaration.JE_CustomsOffice = "ES003861";
			AssertFieldVisibility<ZDropEdit>(false, "MethodOfPayment2 is not visible when JE_CustomsOffice = ES003861, Import and not UCC6", declaration, "MethodOfPayment2DropEdit");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertFieldVisibility<ZDropEdit>(false, "MethodOfPayment2 is not visible when ZG_DestinationState no IsCanaryIsland, Export and not UCC6", declaration, "MethodOfPayment2DropEdit");

			AssertFieldVisibility<ZDropEdit>(false, "MethodOfPayment2 is not visible when JE_CustomsOffice = ES003861, Export and not UCC6", declaration, "MethodOfPayment2DropEdit");
		}

		using (Business.Testing.CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
		{
			declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
			AssertFieldVisibility<ZDropEdit>(false, "MethodOfPayment2 is not visible when ZG_DestinationState IsCanaryIsland, Export and UCC6", declaration, "MethodOfPayment2DropEdit");

			AssertFieldVisibility<ZDropEdit>(false, "MethodOfPayment2 is not visible when JE_CustomsOffice = ES003861, Export and UCC6", declaration, "MethodOfPayment2DropEdit");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertFieldVisibility<ZDropEdit>(true, "MethodOfPayment2 is visible when JE_CustomsOffice = ES003861, Import and UCC6", declaration, "MethodOfPayment2DropEdit");

			declaration.JE_CustomsOffice = ZString.Empty;
			AssertFieldVisibility<ZDropEdit>(false, "MethodOfPayment2 is not visible when JE_CustomsOffice = Empty, Import and UCC6", declaration, "MethodOfPayment2DropEdit");

			AssertFieldVisibility<ZDropEdit>(false, "MethodOfPayment2 is not visible when ZG_DestinationState IsCanaryIsland, Import and UCC6", declaration, "MethodOfPayment2DropEdit");

			declaration.JE_CustomsOffice = "ES003541";
			AssertFieldVisibility<ZDropEdit>(true, "MethodOfPayment2 is visible when JE_CustomsOffice = ES003541, Import and UCC6", declaration, "MethodOfPayment2DropEdit");

			declaration.JE_CustomsOffice = "ES003712";
			AssertFieldVisibility<ZDropEdit>(false, "MethodOfPayment2 is not visible when JE_CustomsOffice = ES003712, Import and UCC6", declaration, "MethodOfPayment2DropEdit");

			declaration.JE_CustomsOffice = "ES009998";
			AssertFieldVisibility<ZDropEdit>(true, "MethodOfPayment2 is visible when JE_CustomsOffice = ES009998, Import and UCC6", declaration, "MethodOfPayment2DropEdit");
		}

		invoiceLine.JI_JZ = ZGuid.Missing;
		AssertFieldVisibility<ZDropEdit>(false, "MethodofPayment2 is not visible when no Invoice Header", declaration, "MethodOfPayment2DropEdit");
	});

	void AssertFieldVisibility<T>(ZBool expectedResult, ZString testMessage, JobDeclaration declaration, ZString nameField) where T : Control
	{
		using (var form = new ZForm(declaration))
		using (var control = new ImportInvoiceLineUserControl())
		{
			control.JobDeclaration = declaration;
			control.InitializeGridLayout();

			form.Controls.Add(control);
			form.Show();

			var field = control.FindSingleOrDefault<T>(nameField);
			AssertEquals(testMessage, expectedResult, field.Visible);
		}
	}

	public void TestT2LItemNumberVisible()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		var entryLine = entryHeader.MergedLines.AddNew();
		var invoiceHeader = jobDeclaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_CL = entryLine.PK;

		using (var form = new ZForm(jobDeclaration))
		using (var control = new ImportInvoiceLineUserControl())
		{
			control.JobDeclaration = jobDeclaration;
			control.InitializeGridLayout();

			form.Controls.Add(control);
			form.Show();

			CombineAssertions("T2l Item Number", () =>
			{
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				var t2lItemNumber = control.FindSingleOrDefault<ZTextBox>("T2LItemNumberCalcEdit");
				AssertEquals("T2L Item Number should be visible", true, t2lItemNumber.Visible);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				AssertEquals("T2L Item Number should not be visible", false, t2lItemNumber.Visible);
			});
		}
	}

	public void TestPVPCalcFindBox()
	{
		#region Setup

		var helper = new Business.Testing.ESUniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, "61", "Test 61");

		var countryCode = Core.Constants.CountryCodes.Spain;
		var impTariffType = helper.CreateTariffType(countryCode, "IMP");
		var canexcTariffType = helper.CreateTariffType(countryCode, "CANEX");
		var rateType = helper.CreateCusRateType(countryCode, "EXC");
		Factory.Save();

		var tariff = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

		var tariffExcise = helper.LoadOrCreateNewTariff(countryCode, canexcTariffType.PK, "0A0", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc1");
		var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "0A0", rateType.PK);
		helper.CreateRate(tariffExcise, rateCode.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "41.5*[MIL]");

		var tariffExciseWithPvP = helper.LoadOrCreateNewTariff(countryCode, canexcTariffType.PK, "0A7", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc2");
		var rateCodePVP = helper.LoadOrCreateNewCusRateCode(Factory, "0A7", rateType.PK);
		helper.CreateRate(tariffExciseWithPvP, rateCodePVP.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "0.158*PVP");

		helper.CreateTariffRelationship(tariffExcise.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
		helper.CreateTariffRelationship(tariffExciseWithPvP.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
		Factory.Save();

		#endregion

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		using (var form = new ZForm(declaration))
		using (var control = new ImportInvoiceLineUserControl())
		{
			control.JobDeclaration = declaration;
			control.InitializeGridLayout();

			form.Controls.Add(control);
			form.Show();

			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;

			var exciseExemption = control.FindSingleOrDefault<ZCalcFindBox>("PVPCalcFindBox");
			AssertEquals("PVP is not Visible, there is no ZG_ExciseCode", false, exciseExemption.Visible);

			invoiceLine.ZG_ExciseCode = "0A0";
			AssertEquals("PVP is not Visible, the excise isn't PVP", false, exciseExemption.Visible);

			invoiceLine.ZG_ExciseCode = "0A7";
			AssertEquals("PVP is Visible, 0A7 is PVP", true, exciseExemption.Visible);

			invoiceLine.ZG_ExciseCode = "0A6";
			AssertEquals("PVP is not Visible, the code is not valid", false, exciseExemption.Visible);
		}
	}

	public void TestCommercialReferenceVisibility()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = JobMessageTypeList.Codes.Import;
		dec.Invoices.AddNew().InvoiceLines.AddNew();
		using (var form = new ZForm())
		using (var control = new ImportInvoiceLineUserControl())
		{
			form.SetDataBinding(dec, ZString.Empty);
			form.Controls.Add(control);
			control.JobDeclaration = dec;
			form.Show();
			var commercialReferenceBox = control.FindSingleOrDefault<ZTextBox>("CommercialReferenceTextBox");
			AssertNotNull("For Import Declaration, Commercial Reference must be available on the screen", commercialReferenceBox);
			AssertEquals("Commercial Reference should be visible for Import declarations in ES", true, commercialReferenceBox.Visible);
		}
	}

	public void TestExciseCodeDropEdit()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.Invoices.AddNew().InvoiceLines.AddNew();
		using (var form = new ZForm(declaration))
		using (var control = new ImportInvoiceLineUserControl())
		{
			control.JobDeclaration = declaration;
			control.InitializeGridLayout();

			form.Controls.Add(control);
			form.Show();

			var exciseExemption = control.FindSingleOrDefault<ZDropEdit>("ExciseCodeDropEdit");
			AssertEquals("Excise Code is Visible", true, exciseExemption.Visible);
		}
	}

	public void TestExciseExemptionField()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.Invoices.AddNew().InvoiceLines.AddNew();
		using (var form = new ZForm(declaration))
		using (var control = new ImportInvoiceLineUserControl())
		{
			control.JobDeclaration = declaration;
			control.InitializeGridLayout();

			form.Controls.Add(control);
			form.Show();

			var exciseExemption = control.FindSingleOrDefault<ZDropEdit>("ExciseExemptionDropEdit");
			AssertEquals("Excise Exemption is Visible", true, exciseExemption.Visible);
		}
	}

	public void TestHasNonRecycledPlasticsCheckBox()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.Invoices.AddNew().InvoiceLines.AddNew();
		using (var form = new ZForm(declaration))
		using (var control = new ImportInvoiceLineUserControl())
		{
			control.JobDeclaration = declaration;
			control.InitializeGridLayout();

			form.Controls.Add(control);
			form.Show();

			var hasNonRecycledPlasticsCheckBox = control.FindSingleOrDefault<ZCheckBox>("HasNonRecycledPlasticsCheckBox");
			AssertEquals("HasNonRecycledPlasticsCheckBox is Visible", true, hasNonRecycledPlasticsCheckBox.Visible);
		}
	}

	public void TestGlobalWarmingPotentialCalcEdit()
	{
		#region Setup

		var helper = new Business.Testing.ESUniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, "61", "Test 61");

		var countryCode = Core.Constants.CountryCodes.Spain;
		var impTariffType = helper.CreateTariffType(countryCode, "IMP");
		var canexcTariffType = helper.CreateTariffType(countryCode, "CANEX");
		var rateType = helper.CreateCusRateType(countryCode, "EXC");
		Factory.Save();

		var tariff = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

		var tariffExcise = helper.LoadOrCreateNewTariff(countryCode, canexcTariffType.PK, "0A0", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc1");
		var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "0A0", rateType.PK);
		helper.CreateRate(tariffExcise, rateCode.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "41.5*[MIL]");

		var tariffExciseWithPCA = helper.LoadOrCreateNewTariff(countryCode, canexcTariffType.PK, "0A7", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc2");
		var rateCodePCA = helper.LoadOrCreateNewCusRateCode(Factory, "0A7", rateType.PK);
		helper.CreateRate(tariffExciseWithPCA, rateCodePCA.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "0.158*PCA");

		helper.CreateTariffRelationship(tariffExcise.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
		helper.CreateTariffRelationship(tariffExciseWithPCA.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
		Factory.Save();

		#endregion

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		using (var form = new ZForm(declaration))
		using (var control = new ImportInvoiceLineUserControl())
		{
			control.JobDeclaration = declaration;
			control.InitializeGridLayout();

			form.Controls.Add(control);
			form.Show();

			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;

			var globalWarmingPotentialCalcEdit = control.FindSingleOrDefault<ZCalcEdit>("GlobalWarmingPotentialCalcEdit");
			AssertEquals("GlobalWarmingPotentialCalcEdit is not Visible, there is no ZG_ExciseCode", false, globalWarmingPotentialCalcEdit.Visible);

			invoiceLine.ZG_ExciseCode = "0A0";
			AssertEquals("GlobalWarmingPotentialCalcEdit is not Visible, the excise isn't PCA", false, globalWarmingPotentialCalcEdit.Visible);

			invoiceLine.ZG_ExciseCode = "0A7";
			AssertEquals("GlobalWarmingPotentialCalcEdit is Visible, 0A7 is PCA", true, globalWarmingPotentialCalcEdit.Visible);

			invoiceLine.ZG_ExciseCode = "0A6";
			AssertEquals("GlobalWarmingPotentialCalcEdit is not Visible, the code is not valid", false, globalWarmingPotentialCalcEdit.Visible);
		}
	}

	public void TestUCC6Fields()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			using (Business.Testing.CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(false))
			{
				AssertControlsVisibility("Controls not visible for import with no UCC6 FUNCS", expectedVisible: false);
			}

			using (Business.Testing.CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
			{
				AssertControlsVisibility("Controls not visible for import with UCC6 FUNCS", expectedVisible: true);
			}

			void AssertControlsVisibility(string testCase, bool expectedVisible)
			{
				using var form = new ZForm(declaration);
				using var control = new ImportInvoiceLineUserControl();
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();

				form.Controls.Add(control);
				form.Show();

				var dispatchCodeFindBox = control.FindSingleOrDefault<ZCodeFindBox>("DispatchCodeFindBox");
				var valuationCodeDropEdit = control.FindSingleOrDefault<ZDropEdit>("ValuationCodeDropEdit");

				AssertEquals($"{testCase}: DispatchCodeFindBox.Visible", expectedVisible, dispatchCodeFindBox.Visible);
				AssertEquals($"{testCase}: ValuationCodeDropEdit.Visible", expectedVisible, valuationCodeDropEdit.Visible);
			}
		});
	}
}
