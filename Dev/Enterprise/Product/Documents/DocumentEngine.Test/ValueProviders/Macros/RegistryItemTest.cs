using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(RegistryItem))]
	sealed class RegistryItemTest : ValueProviderTest
	{
		public void TestEndToEnd()
		{
			var factory = new BusinessObjectFactory();
			var template = DocumentEngineTestHelper.CreateTemplateFromString(
				factory,
				"Test",
@"{A}-[#Config]
{A}-[#SectionBody]
{B}-[<Upper(""Method: <RegistryItem(OrganisationsDataRegistry.Instance.APPaymentMethod)>"")>]
{A}-[#EndOfReport]");
			template.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.UnitTest);

			var documentCommand = factory.New<DocumentCommand>();

			var pivot = factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = documentCommand.PK;

			var dummy = factory.New<DummyBODocSupportable>();

			using (var setter = new TemporaryValueSetter<string>(value => OrganisationsDataRegistry.Instance.APPaymentMethod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value), OrganisationsDataRegistry.Instance.APPaymentMethod.Value))
			{
				setter.Set(ZArchitecture.Core.ReceiptTypes.Cheque);

				AssertMultilineASCIIEquals("End to end test for <RegistryItem> macro.",
	@"{B}-[METHOD: CHQ]",
					DocumentEngineTestHelper.ExecuteDocumentCommand(documentCommand, dummy));
			}
		}

		[ExpectNoExceptions]
		public void TestNewRegistryLocationItem()
		{
			ValueProviderToTest.GetReplacement("<RegistryItem(Enterprise.Customs.CA.Registry.CACustomsDataRegistry.Instance.InBondTermsAndConditions,Enterprise.Customs.CA.Business)>", Report);
		}

		public void TestIsRegistryValueOfBoolType()
		{
			AssertEquals("New style; BooleanRegistryItem", true, RegistryItem.IsRegistryValueOfBoolType("<RegistryItem(Enterprise.Accounting.Registry.Business.AccountingConfigurationRegistry.Instance.EnableInvoicePaymentWebService,Enterprise.Accounting.Business)>"));
			AssertEquals("New style; StringRegistryItem", false, RegistryItem.IsRegistryValueOfBoolType("<RegistryItem(Enterprise.Customs.CA.Registry.CACustomsDataRegistry.Instance.InBondTermsAndConditions,Enterprise.Customs.CA.Business)>"));
			AssertEquals("New style; AllowBackPostingSubLedgerTransactionRegistryItem : BooleanRegistryItem", true, RegistryItem.IsRegistryValueOfBoolType("<RegistryItem(Enterprise.Accounting.Registry.Business.AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction,Enterprise.Accounting.Business)>"));
			AssertEquals("Old style; DocumentOpenCloseTextRegistryItem", false, RegistryItem.IsRegistryValueOfBoolType("<RegistryItem(Env.Registry.AWBSecurityDeclaration.OpeningText)>"));
			AssertEquals("Old style; BooleanRegistryItem", true, RegistryItem.IsRegistryValueOfBoolType("<RegistryItem(Env.Registry.Rating.ExcludeHolidaysInTimeRating)>"));
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("Should match", ValueProviderToTest.IsResponsibleForReplacing("<RegistryItem(aaa)>", Passes.FirstPass));
			Assert("Should match", ValueProviderToTest.IsResponsibleForReplacing("<registryitem(aaa)>", Passes.FirstPass));
			Assert("Should match", ValueProviderToTest.IsResponsibleForReplacing("< Registry Item (aaa) >", Passes.FirstPass));

			Assert("Should not match", !ValueProviderToTest.IsResponsibleForReplacing("<reg istryitem(aaa)>", Passes.FirstPass));
			Assert("Should not match", !ValueProviderToTest.IsResponsibleForReplacing("<RegistryItem>", Passes.FirstPass));
			Assert("Should not match", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
		}

		[TestDate(2008, 8, 8)]
		public void TestReplacement()
		{
			OrganisationsDataRegistry.Instance.APPaymentMethod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZArchitecture.Core.ReceiptTypes.Cheque);
			AssertEquals("Should be Cheque", ZArchitecture.Core.ReceiptTypes.Cheque, ValueProviderToTest.GetReplacement("<RegistryItem(OrganisationsDataRegistry.Instance.APPaymentMethod)>", Report));
			OrganisationsDataRegistry.Instance.APPaymentMethod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZArchitecture.Core.ReceiptTypes.DirectDebit);
			AssertEquals("Should be DirectDebit", ZArchitecture.Core.ReceiptTypes.DirectDebit, ValueProviderToTest.GetReplacement("<RegistryItem(OrganisationsDataRegistry.Instance.APPaymentMethod)>", Report));

			DocumentsDataRegistry.Instance.CompanyDisplayName.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "LALA Land");
			AssertIsReplacedWith("LALA Land", "<RegistryItem(Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.CompanyDisplayName,Enterprise.DocumentEngineCore)>");

			var activeIncoTerms = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms);

			AssertEquals("CarriageAndInsurancePaidTo should be an active inco term.", true, activeIncoTerms.ContainsCode(Enterprise.Core.Constants.IncoTerms.CarriageAndInsurancePaidTo));
			OrganisationsDataRegistry.Instance.ConsigneeIncoTerm.SetValue(Enterprise.Core.Constants.IncoTerms.CarriageAndInsurancePaidTo);
			AssertEquals("Should be CarriageAndInsurancePaidTo", Enterprise.Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, ValueProviderToTest.GetReplacement("<RegistryItem(OrganisationsDataRegistry.Instance.ConsigneeIncoTerm)>", Report));

			AssertEquals("DeliveredAtFrontier should be an active inco term.", true, activeIncoTerms.ContainsCode(Enterprise.Core.Constants.IncoTerms.DeliveredAtFrontier));
			OrganisationsDataRegistry.Instance.ConsigneeIncoTerm.SetValue(Enterprise.Core.Constants.IncoTerms.DeliveredAtFrontier);
			AssertEquals("Should be DeliveredAtFrontier", Enterprise.Core.Constants.IncoTerms.DeliveredAtFrontier, ValueProviderToTest.GetReplacement("<RegistryItem(OrganisationsDataRegistry.Instance.ConsigneeIncoTerm)>", Report));

			AssertEquals("Report.Errors", ReportErrorManager.HasNoErrors, Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			AssertEquals("Not Found Test", "", ValueProviderToTest.GetReplacement("<RegistryItem(Env.Registry.Eat.My.Shorts)>", Report));
			AssertEquals("Report.Errors", "Severity: [Warning (without error report)] Message: [Error in RegistryItem Macro: Could not find Registry Item indicated by <RegistryItem(Env.Registry.Eat.My.Shorts)>.]", Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
		}

		public void TestMultilingualReplacement()
		{
			DocumentsDataRegistry.Instance.CustomsDelayAlertAlertText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Some English Text");
			using (var mockGrm = Res.GetLanguageInstance(Core.SharedConstants.Languages.German).UseMockData())
			{
				string key = ((ResourceString)DocumentsDataRegistry.Instance.CustomsDelayAlertAlertText.Value).ResourceKey;
				mockGrm.Put(key, new ResourceStringData(key, "Some German Text"));

				AssertIsReplacedWith("Some English Text", "<RegistryItem(DocumentsDataRegistry.Instance.CustomsDelayAlertAlertText)>");
				Report.Parent.Language = Core.SharedConstants.Languages.German;
				AssertIsReplacedWith("Some German Text", "<RegistryItem(DocumentsDataRegistry.Instance.CustomsDelayAlertAlertText)>");
				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
				{
					Report.Parent.Language = Core.SharedConstants.Languages.EnglishAmerican;
					AssertIsReplacedWith("Some English Text", "<RegistryItem(DocumentsDataRegistry.Instance.CustomsDelayAlertAlertText)>");
				}
			}
		}

		public void TestMaskTheReturnValueWhenUsePasswordRegistryMacros()
		{
			RawDataRegistry.Instance.SMTPPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestPassWord");
			AssertIsReplacedWith("EditorType is Password in RegistryItemImpl return ***", "***", "<RegistryItem(RawRegistry.Instance.SMTPPassword)>");
			RawDataRegistry.Instance.AUCCompanyCertificatePassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestPassWord");
			AssertIsReplacedWith("EditorType is Password in RegistryItemWrapper return ***", "***", "<RegistryItem(RawRegistry.Instance.AUCCompanyCertificatePassword)>");
		}

		public void TestReplacement_Field()
		{
			Env.Registry.AWBSecurityDeclaration = new DocumentOpenCloseText("Some Opening", "");
			AssertEquals("Correct value", "Some Opening", ValueProviderToTest.GetReplacement("<RegistryItem(Env.Registry.AWBSecurityDeclaration.OpeningText)>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new RegistryItem();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Env.Registry.AWBSecurityDeclaration = new DocumentOpenCloseText("Some Opening", "");
		}
	}
}
