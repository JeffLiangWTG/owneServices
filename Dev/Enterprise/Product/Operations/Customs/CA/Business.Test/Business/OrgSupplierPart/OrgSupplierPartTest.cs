using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(OrgSupplierPart))]
	sealed class OrgSupplierPartTest : Customs.Business.Testing.OrgSupplierPartTest
	{
		public void TestCustomsCountryCodeIsCorrect()
		{
			using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var part = Factory.New<OrgSupplierPart>();
				var pivot = part.PivotsForBinding.AddNew();
				AssertEquals(Core.Constants.CountryCodes.Canada, pivot.CI_RN_NKCountry);
			}
		}

		#region ExpectedClassificationCollectionType
		protected override Type ExpectedClassificationCollectionType
		{
			get { return typeof(ClassificationCollection<CusClassification>); }
		}
		#endregion

		#region GetNewBusinessObject
		protected override BusinessObject GetNewBusinessObject()
		{
			return OrgSupplierPart.New(Factory);
		}
		#endregion

		#region Copy
		public void TestProductTemplateCopy_Pivots()
		{
			var header = Factory.New(typeof(MasterFiles.Business.OrgHeader));
			var header2 = Factory.New(typeof(MasterFiles.Business.OrgHeader));
			var product = Factory.New<OrgSupplierPart>();
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();

			var partRelation0 = product.RelatedOrganisations.AddOrganisationIfNotExist(header.PK, Enterprise.MasterFiles.Business.OrgPartRelation.RelationshipTypes.Owner);

			var classification = Factory.New<CusClassification>();
			classification.CC_TariffNum = "00000000";
			pivot.CI_OP = product.PK;
			pivot.CI_TariffNum = "1111.11.11";

			product.ResumeValidation();

			var productClone = (OrgSupplierPart)((ITemplateCopyable)product).TemplateCopy();
			AssertEquals(pivot.CI_TariffNum, productClone.PivotsForBinding[0].CI_TariffNum);
		}
		#endregion
	}
}
