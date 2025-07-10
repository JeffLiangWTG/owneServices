using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(CusGoodsLocation))]
	sealed class CusGoodsLocationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultsForNew_TemporaryStorage()
		{
			var ts = Factory.New<TemporaryStorageHeader>();
			ts.AMA_ManifestType = ImportDeclarationApplicationCodeList.Codes.V1;
			var tsCusGoodsLocation = ts.GoodsLocation;
			AssertEquals("CGL_Qualifier default U when attached to a V1 TemporaryStorage", Customs.Business.CusGoodsLocationQualifierList.Codes.UnLocode, tsCusGoodsLocation.CGL_Qualifier);

			ts = Factory.New<TemporaryStorageHeader>();
			ts.AMA_ManifestType = ImportDeclarationApplicationCodeList.Codes.V2;
			tsCusGoodsLocation = ts.GoodsLocation;
			AssertEquals("CGL_Qualifier default empty when attached to a non-V1 TemporaryStorage", string.Empty, tsCusGoodsLocation.CGL_Qualifier);
		}

		public void TestSetDefaultsForNew_Declaration()
		{
			AssertEquals("CGL_Qualifier default empty", string.Empty, declarationCusGoodsLocation.CGL_Qualifier);

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, false))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				var cusGoodsLocation = declaration.CustomsEntryInstructions.AddNew().GoodsLocation;
				AssertEquals("CGL_Qualifier default U when attached to non-UCC5", string.Empty, cusGoodsLocation.CGL_Qualifier);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, true))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				var cusGoodsLocation = declaration.CustomsEntryInstructions.AddNew().GoodsLocation;
				AssertEquals("CGL_Qualifier default U when attached to IMPUCC5", Customs.Business.CusGoodsLocationQualifierList.Codes.UnLocode, cusGoodsLocation.CGL_Qualifier);
			}
		}

		public void TestBeginEdit()
		{
			var ts = Factory.New<TemporaryStorageHeader>();
			ts.AMA_ManifestType = ImportDeclarationApplicationCodeList.Codes.V1;
			var tsCusGoodsLocation = ts.GoodsLocation;

			tsCusGoodsLocation.CGL_Qualifier = string.Empty;
			tsCusGoodsLocation.BeginEdit();

			AssertEquals("CGL_Qualifier set when BeginEdit and associated with UCC5.", Customs.Business.CusGoodsLocationQualifierList.Codes.UnLocode, tsCusGoodsLocation.CGL_Qualifier);
		}

		public void TestCGL_QualifierReadonly_TemporaryStorage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, true))
			{
				var cusGoodsLocation = declaration.CustomsEntryInstructions.AddNew().GoodsLocation;
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("CGL_QualifierReadonly true when attached to IMPUCC5", true, cusGoodsLocation.CGL_QualifierReadonly);
				AssertEquals("CGL_QualifierInfo writable when attached to IMPUCC5", true, cusGoodsLocation.CGL_QualifierInfo.ReadOnly);
			}

			var ts = Factory.New<TemporaryStorageHeader>();
			ts.AMA_ManifestType = ImportDeclarationApplicationCodeList.Codes.V1;
			var tsCusGoodsLocation = ts.GoodsLocation;
			AssertEquals("CGL_QualifierReadonly true when attached to a V1 TemporaryStorage", true, tsCusGoodsLocation.CGL_QualifierReadonly);
			AssertEquals("CGL_QualifierInfo readonly when attached to a V1 TemporaryStorage", true, tsCusGoodsLocation.CGL_QualifierInfo.ReadOnly);

			ts = Factory.New<TemporaryStorageHeader>();
			ts.AMA_ManifestType = ImportDeclarationApplicationCodeList.Codes.V2;
			tsCusGoodsLocation = ts.GoodsLocation;
			AssertEquals("CGL_QualifierReadonly false when attached to a non-V1 TemporaryStorage", false, tsCusGoodsLocation.CGL_QualifierReadonly);
			AssertEquals("CGL_QualifierInfo writable when attached to a non-V1 TemporaryStorage", false, tsCusGoodsLocation.CGL_QualifierInfo.ReadOnly);
		}

		public void TestUCCVersionProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var cusGoodsLocation = instruction.GoodsLocation;
			AssertSame("UCCVersionProvider: The associated JobDeclaration.", instruction, cusGoodsLocation.UCCVersionProvider);

			var storateHeader = Factory.New<TemporaryStorageHeader>();
			var tsCusGoodsLocation = (CusGoodsLocation)storateHeader.GoodsLocation;
			AssertSame("UCCVersionProvider: The associated TemporaryStorageHeader.", storateHeader, tsCusGoodsLocation.UCCVersionProvider);
		}

		public void TestValidationType()
		{
			AssertType<CusGoodsLocationValidation>("CusGoodsLocationValidation", declarationCusGoodsLocation.Validation);
		}

		public void TestLookupsType()
		{
			AssertType<CusGoodsLocationLookups>("CusGoodsLocationValidation", declarationCusGoodsLocation.Lookups);
		}

		public void TestUnlocode()
		{
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.CGL_CustomsOffice = "IEDUB100";
			AssertEquals("Unlocode mapping", "IEDUB100", cusGoodsLocation.Unlocode);
		}

		protected override BusinessObject GetNewBusinessObject() => declarationCusGoodsLocation;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => declarationCusGoodsLocation;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => declarationCusGoodsLocation;

		CusGoodsLocation declarationCusGoodsLocationCache;
		CusGoodsLocation declarationCusGoodsLocation
		{
			get
			{
				if (declarationCusGoodsLocationCache == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declarationCusGoodsLocationCache = declaration.CustomsEntryInstructions.AddNew().GoodsLocation;
				}
				return declarationCusGoodsLocationCache;
			}
		}
		JobDeclaration declaration;
	}
}
