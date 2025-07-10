using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(LegalActInfoCollection))]
	class LegalActInfoCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<LegalActInfo>
	{
		protected override Customs.Business.CusSupportingInfoCollection<LegalActInfo> GetCusSupportingInfoCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var cusLineTariff = invoiceLine.AdditionalTariffs.AddNew();
			cusLineTariff.TariffType = ChildTariffTypeList.Codes.LETEC;
			cusLineTariff.ExNumber = "001";
			return new LegalActInfoCollection(cusLineTariff);
		}

		public void TestAddNewWithSubject()
		{
			var collection = GetCusSupportingInfoCollection() as LegalActInfoCollection;
			AssertEquals("CSI_SubType", AdditionalTaxTypeList.Codes.ExDutyTariff, collection.AddNew(AdditionalTaxTypeList.Codes.ExDutyTariff).CSI_SubType);
			AssertEquals("LegalAct Has Changes should be False", false, collection.HasChanges);
		}

		public void TestFindBySubject()
		{
			var collection = GetCusSupportingInfoCollection() as LegalActInfoCollection;
			var legalAct = collection.AddNew(AdditionalTaxTypeList.Codes.ExDutyTariff);
			AssertEquals(legalAct, collection.FindBySubject(AdditionalTaxTypeList.Codes.ExDutyTariff));
			AssertNull(collection.FindBySubject(AdditionalTaxTypeList.Codes.ExIPITariff));
		}
	}
}
