using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class DocScanningHelperTest : TestCaseWithFactory
	{
		public void TestDocumentTypeSecurity()
		{
			Env.Security.GetDocumentTypeUploadCheckPoint("ACV").IsAllowed = false;
			var orgDocTypes = DocScanningHelper.GetCategoryDocTypesFromJobType(Core.Constants.DocManagerCodes.Organisation, new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()), true);
			Assert(!orgDocTypes.GetAllCodes().Contains("ACV"));

			Env.Security.GetDocumentTypeUploadCheckPoint("ACV").IsAllowed = true;
			orgDocTypes = DocScanningHelper.GetCategoryDocTypesFromJobType(Core.Constants.DocManagerCodes.Organisation, new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()), true);
			Assert(orgDocTypes.GetAllCodes().Contains("ACV"));
		}

		public void TestGetDocTypesFromJobTypeSecurity()
		{
			Env.Security.GetDocumentTypeUploadCheckPoint("ACV").IsAllowed = false;
			var orgDocTypes = new DocScanningHelper().GetDocTypesFromJobType(Core.Constants.DocManagerCodes.Organisation, new BusinessObjectFactory(), true);
			Assert(!orgDocTypes.Cast<RefDocType>().Any(x => x.RT_DocType.EqualsIgnoringCase("ACV")));

			Env.Security.GetDocumentTypeUploadCheckPoint("ACV").IsAllowed = true;
			orgDocTypes = new DocScanningHelper().GetDocTypesFromJobType(Core.Constants.DocManagerCodes.Organisation, new BusinessObjectFactory(), true);
			Assert(orgDocTypes.Cast<RefDocType>().Any(x => x.RT_DocType.EqualsIgnoringCase("ACV")));
		}

		public void TestGetCategoryDocTypesFromJobType()
		{
			var orgDocTypes = DocScanningHelper.GetCategoryDocTypesFromJobType(Core.Constants.DocManagerCodes.Organisation, new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()), false);

			ZQuery query = new ZQuery(RefDocTypeSchema.RT_ReferenceType, "ALL");
			query.AddToFilter(new ZQuery(RefDocTypeSchema.RT_ReferenceType, "CSR"), JoinCondition.Or);
			var types = Factory.Load<RefDocType>(query);

			AssertEquals(types.Length, orgDocTypes.Count);
			foreach (var type in types)
			{
				Assert(orgDocTypes.ContainsCode(type.RT_DocType));
			}
		}

		public void TestDocScanning_DoesNotIncludeFreightDocTypesWhenProductivityWiseModeEnabled()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			var orgDocTypes = DocScanningHelper.GetCategoryDocTypesFromJobType(Core.Constants.DocManagerCodes.Organisation, new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()), true);
			var orgCodes = orgDocTypes.GetAllCodes();
			AssertCollectionNotContains("We should load no doctypes with Freight reference types, and yet...",
				new ZString[] {
					Enterprise.Core.Constants.ReferenceTypes.ClientSupplierRelationship,
					Enterprise.Core.Constants.ReferenceTypes.SupplyChainLogistics
				},
				orgCodes);
		}

		public void TestGetAvailableDocumentTypes()
		{
			var docTypes = new DocScanningHelper().GetAvailableDocumentTypes(ZString.Empty, Factory);
			AssertGreaterThan(docTypes.Count, 0);
			foreach (RefDocType docType in docTypes)
			{
				AssertEquals(true, docType.RT_IsActive);
				AssertEquals("ALL", docType.RT_ReferenceType);
			}

			docTypes = new DocScanningHelper().GetAvailableDocumentTypes("SHP", Factory);
			Assert(docTypes.Count > 0);
			int sclDocTypes = 0;
			foreach (RefDocType docType in docTypes)
			{
				AssertEquals(true, docType.RT_IsActive);
				AssertEquals(true, docType.RT_ReferenceType == "ALL" || docType.RT_ReferenceType == "SCL");
				if (docType.RT_ReferenceType == "SCL")
				{
					sclDocTypes++;
				}
			}
			AssertGreaterThan(sclDocTypes, 0);
		}
	}
}
