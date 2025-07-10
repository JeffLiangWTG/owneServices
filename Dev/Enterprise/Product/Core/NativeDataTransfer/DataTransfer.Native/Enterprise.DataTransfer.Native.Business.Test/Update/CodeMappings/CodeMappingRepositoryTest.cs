using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.DataTransfer.Native.Common.CodeMappings;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.CodeMappings
{
	public class CodeMappingRepositoryTest : TransactionedTestCase
	{
		public void TestMapLocalCode()
		{
			const string localCode = "LL";
			const string foreignCode = "TT";

			var country = factory.New<RefCountry>();
			country.RN_Code = localCode;
			factory.Save();

			var query = new ZQuery();
			var ownerPK = factory.LoadTop1<OrgHeader>(query).PK.ToGuid();

			// Setup OrgPatternMatch
			// Weird, if I assign OO_LocalGuid first, it will become Guid.Empty when I save factory
			var pattern = repository.New();
			pattern.OO_OH = ownerPK;
			pattern.OO_Relationship = relationship;
			pattern.OO_ForeignCode = foreignCode;
			pattern.OO_LocalGuid = country.PK;
			factory.Save();

			// Setup Entity
			var result = repository.MapLocalCode(relationship, foreignCode, ownerPK);

			AssertEquals("Should be able to find local code when there is Code Mapping existed", localCode, result);
		}

		public void TestMapLocalCode_Input_Is_Null()
		{
			var result = repository.MapLocalCode(null);
			AssertEquals("Should return Empty String", string.Empty, result);
		}

		public void TestFindOwnerOrgPK_CodeMapping_Existed()
		{
			var defaultOrgPK = CodeMappingRepository.DefaultOrgPK(factory);
			var testOrg = factory.NewWithValidTestData<OrgHeader>();
			factory.Save();

			var codeMapping = repository.New();
			//In OO_Relationship
			//It will set OO_LocalGuid and OO_LocalCode to Empty
			codeMapping.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Organisation;
			codeMapping.OO_LocalGuid = testOrg.PK;
			codeMapping.OO_LocalCode = testOrg.OH_Code;

			codeMapping.OO_ForeignCode = "ZZZ_ABC";
			codeMapping.OO_OH = defaultOrgPK;

			var result = repository.FindOwnerOrgPK("ZZZ_ABC");
			AssertEquals(testOrg.PK, result);
		}

		public void TestFindOwnerOrgPK_CodeMapping_Not_Existed()
		{
			var testOrg = factory.NewWithValidTestData<OrgHeader>();
			factory.Save();

			var result = repository.FindOwnerOrgPK(testOrg.OH_Code);
			AssertEquals(testOrg.PK, result);
		}

		public void TestFindOwnerOrgPK_Input_Is_Empty()
		{
			var result = repository.FindOwnerOrgPK(string.Empty);
			AssertEquals(CodeMappingRepository.DefaultOrgPK(factory), result);
		}

		public void TestCreateCodeMapping()
		{
			var ownerOrg = factory.NewWithValidTestData<OrgHeader>();
			factory.Save();

			var codeMapping = new CodeMapping();
			codeMapping.ForeignCode = "ABCDEFG";
			codeMapping.Relationship = relationship;
			codeMapping.LocalCode = "GFEDCBA";
			codeMapping.LocalGuid = Guid.NewGuid();
			codeMapping.OwnerCode = ownerOrg.OH_Code;

			repository.CreateCodeMapping(codeMapping);

			var patternMatch = repository.Load(relationship, "ABCDEFG", ownerOrg.PK.ToGuid());
			AssertNotNull(patternMatch);
		}

		protected override void SetUp()
		{
			base.SetUp();
			factory = new BusinessObjectFactory();
			repository = new CodeMappingRepository(factory);
			relationship = Constants.OrgPatternMatchOverrideRelationships.Country;
		}
		BusinessObjectFactory factory;
		CodeMappingRepository repository;
		string relationship;
	}
}
