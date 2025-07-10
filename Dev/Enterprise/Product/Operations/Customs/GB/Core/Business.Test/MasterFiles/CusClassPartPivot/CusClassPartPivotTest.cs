using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.MasterFiles.Testing
{
	[TestedType(typeof(CusClassPartPivot))]
	public class CusClassPartPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSupportingDocumentsCorrectType()
		{
			AssertType<SupportingDocumentCollection>(pivot.SupportingDocuments);
		}

		public void TestAdditionalInfoCorrectType()
		{
			AssertType<AdditionalInfoCollection>(pivot.AdditionalInfos);
		}

		public void TestLookupsCorrectType()
		{
			AssertType<CusClassPartPivotLookups>(pivot.Lookups);
		}

		public void TestGBPivotCPCPropertyAttributes()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			AssertEquals("Pivot type BTH can't set CPC, should be blank", "", pivot.CI_CPC);
			AssertEquals("Pivot type BTH can't edit CPC", true, pivot.CI_CPCInfo.ReadOnly);
			pivot.CI_CPC = "1234";
			AssertEquals("Pivot type BTH can edit existing CPC value", false, pivot.CI_CPCInfo.ReadOnly);
			pivot.CI_ChildType = "";
			AssertEquals("Pivot type BLANK can edit existing CPC value", false, pivot.CI_CPCInfo.ReadOnly);
			pivot.CI_CPC = "";
			AssertEquals("Pivot type BLANK can't select CPC", true, pivot.CI_CPCInfo.ReadOnly);
			pivot.CI_ChildType = EU.Business.MasterFiles.CusClassification.ClassificationType.IMP;
			AssertEquals("Pivot type IMP can edit existing CPC value", false, pivot.CI_CPCInfo.ReadOnly);
			pivot.CI_ChildType = EU.Business.MasterFiles.CusClassification.ClassificationType.EXP;
			AssertEquals("Pivot type EXP can edit existing CPC value", false, pivot.CI_CPCInfo.ReadOnly);
		}

		public void TestDataGroupingCode()
		{
			var pivo = (BaseCusClassPartPivot)pivot;

			pivo.CI_ChildType = Customs.Common.ClassificationType.IMP;
			AssertEquals("CDS", ((EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport)pivo).DataGroupingCode);

			pivo.CI_ChildType = Customs.Common.ClassificationType.EXP;
			using (GBCustomsDataRegistry.Instance.CDSEnabledForExports.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false))
			{
				AssertEquals("CDS", ((EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport)pivo).DataGroupingCode);
			}
			using (GBCustomsDataRegistry.Instance.CDSEnabledForExports.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true))
			{
				AssertEquals("CDS", ((EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport)pivo).DataGroupingCode);
			}
		}

		public void TestDataGroupingCodeForAdditionalProcedures()
		{
			pivot.CI_ChildType = Customs.Common.ClassificationType.IMP;
			AssertEquals("CDS", pivot.DataGroupingCodeForAdditionalProcedures);

			pivot.CI_ChildType = Customs.Common.ClassificationType.EXP;
			using (GBCustomsDataRegistry.Instance.CDSEnabledForExports.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false))
			{
				AssertEquals("CDS", pivot.DataGroupingCodeForAdditionalProcedures);
			}
			using (GBCustomsDataRegistry.Instance.CDSEnabledForExports.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true))
			{
				AssertEquals("CDS", pivot.DataGroupingCodeForAdditionalProcedures);
			}
		}

		public void TestICusClassPartPivotImplements()
		{
			AssertSame(pivot.Taxes, ((Integration.Customs.GB.ICusClassPartPivot)pivot).Taxes);
			AssertSame(pivot.SupportingDocuments, ((Integration.Customs.GB.ICusClassPartPivot)pivot).SupportingDocuments);
			AssertSame(pivot.PreviousDocuments, ((Integration.Customs.GB.ICusClassPartPivot)pivot).PreviousDocuments);
			AssertSame(pivot.AdditionalInfos, ((Integration.Customs.GB.ICusClassPartPivot)pivot).AdditionalInfos);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return pivot;
		}

		protected override void SetUp()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "POOPY";
			var relationship = product.RelatedOrganisations.AddNew();
			relationship.OU_Relationship = "BTH";
			relationship.OU_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = EU.Business.MasterFiles.CusClassification.ClassificationType.Both;
		}
		CusClassPartPivot pivot;
	}

	public class CusClassPartPivotAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
	}
}
