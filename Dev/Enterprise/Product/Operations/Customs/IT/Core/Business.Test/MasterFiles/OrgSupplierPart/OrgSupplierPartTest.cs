using System;
using Enterprise.Customs.EU.Business.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(OrgSupplierPart))]
sealed class OrgSupplierPartTest : Customs.Business.Testing.OrgSupplierPartTest
{
	public void TestCustomsCountryCodeIsCorrect()
	{
		using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			AssertEquals(Core.Constants.CountryCodes.Italy, pivot.CI_RN_NKCountry);
		}
	}
	protected override Type ExpectedClassificationCollectionType => typeof(ClassificationCollection<CusClassification>);

	public void TestSupportingDocumentsCorrectType()
	{
		AssertType<Customs.Business.CusClassPartPivotCollection<CusClassPartPivot>>(Part.PivotsForBinding);
		AssertEquals(Core.Constants.CountryCodes.Italy, Part.PivotsForBinding.countryCode);
	}

	new OrgSupplierPart Part => (OrgSupplierPart)base.Part;

	protected override void SetUp()
	{
		base.SetUp();
		countrySetter = Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy);
	}
	IDisposable countrySetter;

	protected override void TearDown()
	{
		if (countrySetter != null)
		{
			countrySetter.Dispose();
			countrySetter = null;
		}
		base.TearDown();
	}
}
