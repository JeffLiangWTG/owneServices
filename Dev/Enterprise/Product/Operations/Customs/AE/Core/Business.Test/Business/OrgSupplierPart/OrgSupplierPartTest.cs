using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(OrgSupplierPart))]
sealed class OrgSupplierPartTest : Customs.Business.Testing.OrgSupplierPartTest
{
	public void TestCustomsCountryCodeIsCorrect()
	{
		using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			AssertEquals(Core.Constants.CountryCodes.UnitedArabEmirates, pivot.CI_RN_NKCountry);
		}
	}

	public void TestCusClassPartPivots()
	{
		var part = Factory.New<OrgSupplierPart>();
		AssertType<CusClassPartPivotCollection<CusClassPartPivot>>("Part.CusClassPartPivots", part.PivotsForBinding);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return OrgSupplierPart.New(Factory);
	}
	protected override Type ExpectedClassificationCollectionType => typeof(ClassificationCollection<CusClassification>);
}
