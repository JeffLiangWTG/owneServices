using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class ImportInvoiceLinePartClassificationTariffDescriptionSyncroniserTest_AUCClass : Customs.Business.Testing.InvoiceLinePartClassificationTariffDescriptionSyncroniserAbstractTest
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
			get { return Tariff.UJ_Code; }
		}

		protected override ZString TariffCode2
		{
			get { return Tariff2.UJ_Code; }
		}

		protected override ZString TariffDescription
		{
			get { return Tariff.UJ_Txt; }
		}

		protected override ZString TariffDescription2
		{
			get { return Tariff2.UJ_Txt; }
		}

		AUCClass Tariff
		{
			get
			{
				if (fTariff == null)
				{
					fTariff = Factory.New<AUCClass>();
					fTariff.UJ_Code = "0000.00.00 00";
					fTariff.UJ_Txt = "Tariff_Description";
				}
				return fTariff;
			}
		}
		AUCClass fTariff;

		AUCClass Tariff2
		{
			get
			{
				if (fTariff2 == null)
				{
					fTariff2 = Factory.New<AUCClass>();
					fTariff2.UJ_Code = "0000.00.00 02";
					fTariff2.UJ_Txt = "Tariff_Description2";
				}
				return fTariff2;
			}
		}
		AUCClass fTariff2;

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
			useCustomsReferenceDataRegItem = AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);
		}

		protected override void TearDown()
		{
			base.TearDown();
			useCustomsReferenceDataRegItem?.Dispose();
		}

		IDisposable useCustomsReferenceDataRegItem;
	}
}
