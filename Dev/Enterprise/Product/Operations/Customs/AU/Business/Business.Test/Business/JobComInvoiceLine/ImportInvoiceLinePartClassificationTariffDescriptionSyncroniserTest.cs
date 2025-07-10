using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class ImportInvoiceLinePartClassificationTariffDescriptionSyncroniserTest : Customs.Business.Testing.InvoiceLinePartClassificationTariffDescriptionSyncroniserAbstractTest
	{
		protected override ZString MessageType
		{
			get
			{
				return Customs.Business.JobMessageTypeList.Codes.Import;
			}
		}

		protected override ZString TariffCode
		{
			get { return new AUImportTariffUniversalFormatter().FormatDotted(Tariff.ZZ1_TariffCode); }
		}

		protected override ZString TariffCode2
		{
			get { return new AUImportTariffUniversalFormatter().FormatDotted(Tariff2.ZZ1_TariffCode); }
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
					var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);

					fTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "0000000000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
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
					var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);

					fTariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "0000000002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
						, description: "Tariff_Description2"
						, taxOrFeeCode: "GST");
				}
				return fTariff2;
			}
		}
		TariffView fTariff2;

		protected override ZString ExpectedDescriptionFromMergeOfMultipleLines
		{
			get { return TariffDescription.ToUpper(); }
		}

		protected override Type DeclarationTypeForTest
		{
			get { return typeof(JobDeclaration); }
		}

		protected override BaseCusClassification GetNewLookup()
		{
			BaseCusClassification result = base.GetNewLookup();
			result.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			return result;
		}

		protected override BaseJobDeclaration GetNewJobDeclaration()
		{
			BaseJobDeclaration result = base.GetNewJobDeclaration();
			result.JE_MessageType = JobMessageTypeList.Codes.Import;
			return result;
		}

		protected override void AddLookupToPart(OrgSupplierPart part, BaseCusClassification lookup)
		{
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_CC = lookup.PK;
			pivot.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTI;
		}

		protected override void SetUp()
		{
			base.SetUp();
			useCustomsReferenceDataRegItem = AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			useCustomsReferenceDataRegItem?.Dispose();
		}

		IDisposable useCustomsReferenceDataRegItem;
	}
}
