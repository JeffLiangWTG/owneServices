using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Customs.DK.Business.MasterFiles.Testing
{
	[TestedType(typeof(OrgSupplierPart))]
	class OrgSupplierPartTest : Customs.Business.Testing.OrgSupplierPartTest
	{
		public void TestCustomsCountryCodeIsCorrect()
		{
			using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var part = Factory.New<OrgSupplierPart>();
				var pivot = part.PivotsForBinding.AddNew();
				AssertEquals(Core.Constants.CountryCodes.Denmark, pivot.CI_RN_NKCountry);
			}
		}

		protected override BusinessObject GetNewBusinessObject() => OrgSupplierPart.New(Factory);

		protected override Type ExpectedClassificationCollectionType => typeof(ClassificationCollection<CusClassification>);
	}
}
