using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing
{
	[TestedType(typeof(PBNCustomsDeclarationItemLookups))]
	class PBNCustomsDeclarationItemLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			var header = Factory.New<PBNCustomsDeclarationItem>();
			var lookups = (PBNCustomsDeclarationItemLookups)header.Lookups;
			var codelist = (IEPBNDeclarationTypes)lookups.CodeList;
			AssertEquals(3, codelist.Count);
			AssertContainsExactElementsInExactOrder(ExpectedCustomsReferenceCodes, codelist.GetAllCodes());
			AssertSame("CodeList is Cached", lookups.CodeList, codelist);
		}

		public void TestStatusList()
		{
			var header = Factory.New<PBNCustomsDeclarationItem>();
			var lookups = (PBNCustomsDeclarationItemLookups)header.Lookups;
			var statusList = lookups.StatusList;
			AssertEquals(3, statusList.Count);
			AssertContainsExactElementsInExactOrder(ExpectedStatusListCodes, statusList.GetAllCodes());
			AssertSame("StatusList is Cached", lookups.StatusList, statusList);
		}

		string[] ExpectedCustomsReferenceCodes =>
		[
			IEPBNDeclarationTypes.Codes.AES,
			IEPBNDeclarationTypes.Codes.AIS,
			IEPBNDeclarationTypes.Codes.ICS,
		];

		string[] ExpectedStatusListCodes =>
		[
			PBNDeclarationReferenceStatusList.Codes.HBA,
			PBNDeclarationReferenceStatusList.Codes.TBA,
			PBNDeclarationReferenceStatusList.Codes.TBD,
		];
	}
}
