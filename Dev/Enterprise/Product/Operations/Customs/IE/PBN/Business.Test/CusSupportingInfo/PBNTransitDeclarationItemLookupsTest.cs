using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing
{
	[TestedType(typeof(PBNTransitDeclarationItemLookups))]
	class PBNTransitDeclarationItemLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			var header = Factory.New<PBNTransitDeclarationItem>();
			var lookups = (PBNTransitDeclarationItemLookups)header.Lookups;
			var codelist = (CodeDescriptionPairList)lookups.CodeList;
			AssertEquals(1, codelist.Count);
			AssertContainsExactElementsInExactOrder(ExpectedCustomsReferenceCodes, codelist.GetAllCodes());
			AssertSame("CodeList is Cached", lookups.CodeList, codelist);
		}

		public void TestStatusList()
		{
			var header = Factory.New<PBNTransitDeclarationItem>();
			var lookups = (PBNTransitDeclarationItemLookups)header.Lookups;
			var statusList = lookups.StatusList;
			AssertEquals(3, statusList.Count);
			AssertContainsExactElementsInExactOrder(ExpectedStatusListCodes, statusList.GetAllCodes());
			AssertSame("StatusList is Cached", lookups.StatusList, statusList);
		}

		string[] ExpectedCustomsReferenceCodes =>
		[
			IEPBNDeclarationTypes.Codes.NCTS,
		];

		string[] ExpectedStatusListCodes =>
		[
			PBNDeclarationReferenceStatusList.Codes.HBA,
			PBNDeclarationReferenceStatusList.Codes.TBA,
			PBNDeclarationReferenceStatusList.Codes.TBD,
		];
	}
}
