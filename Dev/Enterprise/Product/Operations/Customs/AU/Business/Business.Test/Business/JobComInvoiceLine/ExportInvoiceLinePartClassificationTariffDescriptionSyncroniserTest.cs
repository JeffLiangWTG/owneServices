using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ExportInvoiceLinePartClassificationTariffDescriptionSyncroniserTest : Customs.Business.Testing.InvoiceLinePartClassificationTariffDescriptionSyncroniserAbstractTest
	{
		public override void TestDescriptionOnTarrifWhenMerged()
		{
			Assert
			(
				"AU export does not merge, for import there is another test",
				typeof(ImportInvoiceLinePartClassificationTariffDescriptionSyncroniserTest)
					.IsSubclassOf(typeof(Customs.Business.Testing.InvoiceLinePartClassificationTariffDescriptionSyncroniserAbstractTest))
			);
		}

		protected override ZString TariffCode
		{
			get { return new AUExportTariffUniversalFormatter().FormatDotted(Tariff.ZZ1_TariffCode); }
		}

		protected override ZString TariffCode2
		{
			get { return new AUExportTariffUniversalFormatter().FormatDotted(Tariff2.ZZ1_TariffCode); }
		}

		protected override ZString TariffDescription
		{
			get { return Tariff.ZZ1_Description; }
		}

		protected override ZString TariffDescription2
		{
			get { return Tariff2.ZZ1_Description; }
		}

		TariffView Tariff
		{
			get
			{
				if (fTariff == null)
				{
					var helper = new UniversalReferenceTestDataHelper(Factory);
					var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Export);

					fTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "00000000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
						, description: "Tariff_Description"
						, taxOrFeeCode: "GST");
				}
				return fTariff;
			}
		}
		TariffView fTariff;

		TariffView Tariff2
		{
			get
			{
				if (fTariff2 == null)
				{
					var helper = new UniversalReferenceTestDataHelper(Factory);
					var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Export);

					fTariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "00000200", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
						, description: "Tariff_Description2"
						, taxOrFeeCode: "GST");
				}
				return fTariff2;
			}
		}
		TariffView fTariff2;

		protected override Type DeclarationTypeForTest
		{
			get { return typeof(JobDeclaration); }
		}

		protected override ZString ExpectedDescriptionFromMergeOfOneLine
		{
			get { return "LINE"; }
		}

		protected override BaseCusClassification GetNewLookup()
		{
			BaseCusClassification result = base.GetNewLookup();
			result.CC_ClassificationType = BaseCusClassification.ClassificationType.EXP;
			return result;
		}

		protected override BaseJobDeclaration GetNewJobDeclaration()
		{
			BaseJobDeclaration result = base.GetNewJobDeclaration();
			result.JE_MessageType = JobMessageTypeList.Codes.Export;
			return result;
		}

		protected override void AddLookupToPart(OrgSupplierPart part, BaseCusClassification lookup)
		{
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_CC = lookup.PK;
			pivot.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTE;
		}

		protected override void SetUp()
		{
			base.SetUp();
			enableCWRefForAHECCDataRegItem = AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			enableCWRefForAHECCDataRegItem?.Dispose();
		}

		IDisposable enableCWRefForAHECCDataRegItem;
	}
}
