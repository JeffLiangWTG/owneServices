using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.OperationalActions.Testing
{
#if !WINZOR
	[TestedType(typeof(Business.OperationalActions.DeclarationUpdatePreviousDocumentsApplicator))]
	class DeclarationUpdatePreviousDocumentsApplicatorTest : Services.OperationalActions.Support.Testing.OperationalActionMethodApplicatorTest
	{
		public void TestDocumentCodeList()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection, "DC40I", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, "DC40E", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			helper.CreateCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection, "TST1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, "TST2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();

			var applicator = new Business.OperationalActions.DeclarationUpdatePreviousDocumentsApplicator(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var expectedDocumentCodeList = applicator.DocumentCodeList.GetAllCodes();
			var baseDocumentCodeList = new string[] { "TST1", "TST2" };
			var previousDocumentCodeListCDS = new PreviousDocumentCodeListCDS().GetAllCodes();
			CombineAssertions("Document code list population", () =>
			{
				Assert("Document code list should partially be populated with previous docs codes from RefDB.", expectedDocumentCodeList.Any(x => baseDocumentCodeList.Contains(x)));
				Assert("Document code list should partially be populated with previous docs codes from PreviousDocumentCodeListCDS.", expectedDocumentCodeList.Any(x => previousDocumentCodeListCDS.Contains(x)));
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new Business.OperationalActions.DeclarationUpdatePreviousDocumentsApplicator(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}
	}
#endif
}
