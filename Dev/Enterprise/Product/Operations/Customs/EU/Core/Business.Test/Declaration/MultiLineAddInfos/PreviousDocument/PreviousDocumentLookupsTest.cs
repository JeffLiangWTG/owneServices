using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	public class PreviousDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public virtual void TestReferenceList()
		{
			AssertType<CodeDescriptionPairList>(PreviousDocument.Lookups.ReferenceList);
		}

		public void TestSubTypeList()
		{
			AssertNotNull(PreviousDocument.Lookups.SubTypeList);
			AssertContainsExactElementsInAnyOrder(new PreviousDocumentClassList(), PreviousDocument.Lookups.SubTypeList);
		}

		public void TestCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var previoucDocumentType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection;
			helper.CreateNewOrGetExistingCusCodeType(previoucDocumentType, "Previous Documents Export (BOX40)");
			Create_CusCodeList_RefData(helper, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, previoucDocumentType, "T1");
			Create_CusCodeList_RefData(helper, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, previoucDocumentType, "T2");
			Create_CusCodeList_RefData(helper, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, previoucDocumentType, "T3");
			Create_CusCodeList_RefData(helper, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, previoucDocumentType, "T4");
			Create_CusCodeList_RefData(helper, Core.Constants.CountryCodes.Latvia, previoucDocumentType, "AA");
			Create_CusCodeList_RefData(helper, Core.Constants.CountryCodes.Latvia, previoucDocumentType, "BB");

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				var declaration = Factory.New<JobDeclaration>();
				var previousDocument = declaration.PreviousDocuments.AddNew();
				var lookups = new PreviousDocumentLookups(previousDocument);

				AssertEquals(typeof(CodeDescriptionPairList), lookups.CodeList.GetType());
				var codeListLookup = (CodeDescriptionPairList)lookups.CodeList;
				AssertContainsExactElementsInAnyOrder("Only gets Country's previous document types", new string[] { "AA", "BB" }, codeListLookup.GetAllCodes());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var declaration = Factory.New<JobDeclaration>();
				var previousDocument = declaration.PreviousDocuments.AddNew();
				var lookups = new PreviousDocumentLookups(previousDocument);

				AssertEquals(typeof(CodeDescriptionPairList), lookups.CodeList.GetType());
				var codeListLookup = (CodeDescriptionPairList)lookups.CodeList;
				AssertContainsExactElementsInAnyOrder("Gets Europe's previous document types", new string[] { "T1", "T2", "T3", "T4" }, codeListLookup.GetAllCodes());
			}
		}

		void Create_CusCodeList_RefData(UniversalReferenceTestDataHelper helper, string countryCode, string refDataType, string code)
		{
			var dateMIN = ZDateTime.Today.AddMonths(-1);
			var dateMAX = ZDateTime.Today.AddMonths(3);

			helper.CreateCusCodeList(countryCode, refDataType, code, dateMIN, dateMAX);
		}

		#region Implementation
		PreviousDocument PreviousDocument
		{
			get
			{
				if (previousDocument == null)
				{
					previousDocument = Factory.New<JobDeclaration>().Invoices.AddNew().PreviousDocuments.AddNew();
				}
				return previousDocument;
			}
		}
		PreviousDocument previousDocument;
		#endregion
	}
}
