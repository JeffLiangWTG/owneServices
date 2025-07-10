using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Business.Testing
{
	[TestedType(typeof(StmMenuDocumentConfigDependentCollection))]
	sealed class StmMenuDocumentConfigDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		new StmMenuDocumentConfigDependentCollection Collection
		{
			get { return (StmMenuDocumentConfigDependentCollection)base.Collection; }
		}

		StmMenuDocumentConfig AddNew(bool isSystem, ZGuid companyPK, ZGuid clientPK)
		{
			StmMenuDocumentConfig result = Collection.AddNew();
			result.S3_IsSystem = isSystem;
			result.S3_GC = companyPK;
			result.S3_OH = clientPK;
			return result;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmMenuDocumentConfigDependentCollection(Factory.New<StmMenuTemplatePivotBase>());
		}

		public void TestAllowNew()
		{
			AssertEquals("AllowNew", false, Collection.AllowNew);
		}

		public void TestRelationship()
		{
			StmMenuTemplatePivotBase pivot = Factory.New<StmMenuTemplatePivotBase>();
			StmMenuDocumentConfig docConfig = Factory.New<StmMenuDocumentConfig>();
			docConfig.S3_SI = pivot.PK;

			StmMenuDocumentConfigDependentCollection docConfigs = new StmMenuDocumentConfigDependentCollection(pivot);
			docConfigs.Load();

			AssertEquals("Count", 1, docConfigs.Count);
			AssertEquals("[0].S3_SI", pivot.PK, docConfigs[0].S3_SI);
			AssertEquals("AddNew().S3_SI", pivot.PK, docConfigs.AddNew().S3_SI);
		}

		public void TestSortByFallback()
		{
			GlbCompany company1 = Factory.New<GlbCompany>();
			GlbCompany company2 = Factory.New<GlbCompany>();

			company1.GC_Code = "CO1";
			company2.GC_Code = "CO2";

			OrgHeader client1 = Factory.New<OrgHeader>();
			OrgHeader client2 = Factory.New<OrgHeader>();

			client1.OH_Code = "CL1";
			client2.OH_Code = "CL2";

			StmMenuDocumentConfig client1DocConfig = AddNew(false, ZGuid.Empty, client1.PK);
			StmMenuDocumentConfig enterpriseDocConfig1 = AddNew(false, ZGuid.Empty, ZGuid.Empty);
			StmMenuDocumentConfig company2DocConfig = AddNew(false, company2.PK, ZGuid.Empty);
			StmMenuDocumentConfig systemDocConfig = AddNew(true, ZGuid.Empty, ZGuid.Empty);
			StmMenuDocumentConfig client2DocConfig = AddNew(false, ZGuid.Empty, client2.PK);
			StmMenuDocumentConfig company1DocConfig = AddNew(false, company1.PK, ZGuid.Empty);
			StmMenuDocumentConfig company2Client2DocConfig = AddNew(false, company2.PK, client2.PK);
			StmMenuDocumentConfig company1Client2DocConfig = AddNew(false, company1.PK, client2.PK);
			StmMenuDocumentConfig company1Client1DocConfig = AddNew(false, company1.PK, client1.PK);
			StmMenuDocumentConfig enterpriseDocConfig2 = AddNew(false, ZGuid.Empty, ZGuid.Empty);

			Collection.SortByFallback();

			AssertEquals("[0]", systemDocConfig, Collection[0]);
			AssertEquals("[1]", enterpriseDocConfig1, Collection[1]);
			AssertEquals("[2]", enterpriseDocConfig2, Collection[2]);
			AssertEquals("[3]", company1DocConfig, Collection[3]);
			AssertEquals("[4]", company2DocConfig, Collection[4]);
			AssertEquals("[5]", client1DocConfig, Collection[5]);
			AssertEquals("[6]", client2DocConfig, Collection[6]);
			AssertEquals("[7]", company1Client1DocConfig, Collection[7]);
			AssertEquals("[8]", company1Client2DocConfig, Collection[8]);
			AssertEquals("[9]", company2Client2DocConfig, Collection[9]);
		}

		public void TestSupportsSorting()
		{
			AssertEquals("SupportsSorting", false, ((IBindingList)Collection).SupportsSorting);
		}
	}
}
