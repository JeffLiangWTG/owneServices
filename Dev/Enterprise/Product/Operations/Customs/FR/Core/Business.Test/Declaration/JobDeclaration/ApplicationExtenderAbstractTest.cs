using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(ApplicationExtender))]
	public abstract class ApplicationExtenderAbstractTest : TestCaseWithFactory
	{
		public abstract void TestGetJobDeclarationValueSetStrategy();

		public abstract void TestGetCusEntryInstructionValueSetStrategy();

		public abstract void TestGetCusAuthorizationUsageValueSetStrategy();

		public abstract void TestIsUCC6();

		public abstract void TestGetVATDeferStrategy();

		public abstract void TestAmendmentSnapshotMessageType();

		public abstract void TestGetEffectiveCountryOfOrigin();

		public abstract void TestGetDataGroupingForCusProcedure();

		public abstract void TestGetDataGroupingForAdditionalDocumentCodes();

		public abstract void TestGetDefinedDeclarationTypeList();

		public abstract void TestGetNewValidation();

		public abstract void TestGetNewLookups();

		public abstract void TestGetAdditionalInfoValidation();

		public abstract void TestGetVATNumberSupporter();

		public abstract void TestGetCustomsProfileRelatedAccount();

		public abstract void TestGetJobComInvoiceHeaderValidation();

		public abstract void TestGetJobComInvoiceHeaderLookups();

		public abstract void TestGetJobComInvoiceLineValidation();

		public abstract void TestGetJobComInvoiceLineLookups();

		public abstract void TestGetJobComInvoiceLineValueSetStrategy();

		public abstract void TestGetCusEntryInstructionValidation();

		public abstract void TestGetCusEntryInstructionLookups();

		public abstract void TestGetCusAuthorizationUsageLookups();

		public abstract void TestGetJobComInvoiceHeaderValueSetStrategy();

		public abstract void TestGetAddInfoCusEntryInstructionValidation();

		public abstract void TestGetJobComInvoiceHeaderValuePostProcessingStrategy();

		public abstract void TestCanBeRevertedToLastBAE();

		public abstract void TestIsEntryInstructionOutOfInward();

		public abstract void TestGetCorrelationIDPrefix();

		public abstract void TestIsEntryStatusCleared();

		public abstract void TestGetDeltaAccounts();

		public abstract void TestGetCusEntryLineFeeLookups();

		protected ApplicationExtender applicationExtender;

		protected override void SetUp()
		{
			base.SetUp();

			applicationExtender = (ApplicationExtender)Activator.CreateInstance(TestedTypeHelper.GetTestedType(GetType()));
		}
	}
}
