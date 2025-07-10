using System;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class ExportInvoiceLinePartClassificationTariffDescriptionSyncroniserTest_AHECC : Customs.Business.Testing.InvoiceLinePartClassificationTariffDescriptionSyncroniserAbstractTest
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
			get { return Tariff.UA_AHECC; }
		}

		protected override ZString TariffCode2
		{
			get { return Tariff2.UA_AHECC; }
		}

		protected override ZString TariffDescription
		{
			get { return Tariff.UA_LongDescription; }
		}

		protected override ZString TariffDescription2
		{
			get { return Tariff2.UA_LongDescription; }
		}

		protected AUCAHECC Tariff
		{
			get
			{
				if (fTariff == null)
				{
					fTariff = Factory.New<AUCAHECC>();
					fTariff.UA_AHECC = "0000.00";
					fTariff.UA_LongDescription = "Tariff_Description";
				}
				return fTariff;
			}
		}
		AUCAHECC fTariff;

		protected AUCAHECC Tariff2
		{
			get
			{
				if (fTariff2 == null)
				{
					fTariff2 = Factory.New<AUCAHECC>();
					fTariff2.UA_AHECC = "0000.02";
					fTariff2.UA_LongDescription = "Tariff_Description2";
				}
				return fTariff2;
			}
		}
		AUCAHECC fTariff2;

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
			useCustomsReferenceDataRegItem = AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		protected override void TearDown()
		{
			base.TearDown();
			useCustomsReferenceDataRegItem?.Dispose();
		}

		IDisposable useCustomsReferenceDataRegItem;
	}
}
