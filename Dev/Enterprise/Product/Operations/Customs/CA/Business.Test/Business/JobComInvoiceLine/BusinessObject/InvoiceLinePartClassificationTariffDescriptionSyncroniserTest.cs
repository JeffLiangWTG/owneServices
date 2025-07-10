using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class InvoiceLinePartClassificationTariffDescriptionSyncroniserTest : InvoiceLinePartClassificationTariffDescriptionSyncroniserAbstractTest
	{
		#region TariffCode

		protected override ZString TariffCode
		{
			get { return "0000000000"; }
		}

		protected override ZString TariffCode2
		{
			get { return "0000000001"; }
		}

		#endregion

		#region TariffDescription

		protected override ZString TariffDescription
		{
			get { return TariffDescriptionCore; }
		}

		internal const string TariffDescriptionCore = "";

		protected override ZString TariffDescription2
		{
			get { return TariffDescriptionCore2; }
		}

		internal const string TariffDescriptionCore2 = "";

		#endregion

		#region DeclarationTypeForTest

		protected override Type DeclarationTypeForTest
		{
			get { return typeof(JobDeclaration); }
		}

		protected override BaseCusClassification GetNewLookup()
		{
			BaseCusClassification result = Factory.New<BaseCusClassification>();
			result.CC_LookupCode = LookupCode;
			result.CC_ClassificationType = BaseCusClassification.ClassificationType.EXP;
			result.CC_TariffNum = TariffCode;
			result.CC_Description = LookupDescription;
			return result;
		}

		protected override void AddLookupToPart(Customs.Business.OrgSupplierPart part, BaseCusClassification lookup)
		{
			CusClassPartPivot exportPivot = ((OrgSupplierPart)part).PivotsForBinding.AddNew();
			exportPivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			exportPivot.CI_CC = lookup.PK;
		}

		protected override void ChangeLookupOnPartInAnotherFactory(ZGuid partPK, ZGuid newClassificationPK)
		{
			BusinessObjectFactory anotherNewFactory = new BusinessObjectFactory();
			OrgSupplierPart loadedPart = anotherNewFactory.Load<OrgSupplierPart>(partPK);
			CusClassification newClassInAnotherFactory = anotherNewFactory.Load<CusClassification>(newClassificationPK);
			CusClassPartPivot exportPivot = null;
			if (loadedPart.PivotsForBinding.Count > 0)
			{
				exportPivot = loadedPart.PivotsForBinding[0];
			}
			if (exportPivot == null)
			{
				exportPivot = loadedPart.PivotsForBinding.AddNew();
				exportPivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			}
			exportPivot.CI_CC = newClassInAnotherFactory.PK;
			foreach (var pivot in loadedPart.PivotsForBinding)
			{
				if (pivot != exportPivot)
				{
					pivot.Delete();
				}
			}
			anotherNewFactory.Save();
		}

		#endregion
	}
}
