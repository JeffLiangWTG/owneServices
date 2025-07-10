using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs.EU;
using static Enterprise.Integration.Customs.EUExitControl;
using CusEntryInstruction = Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(CusAuthorizationUsage))]
	sealed class CusAuthorizationUsageTest : EnterpriseBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestCustomsCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
			helper.CreateCusCodeType("AUTH", "Authorisation");
			helper.CreateCusCodeList("EUN", "AUTH", "SAS", "Self-Assessment", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusMapType("EUNAU", "OUT", "Authorisation Codes to Document Type Code", true);
			helper.CreateCusMap("EUNAU", "DPO", "C506", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "EUN");
			helper.CreateCusMap("EUNAU", "SAS", "C515", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "EUN");
			helper.CreateCusMap("EUNAU", "SAS", "CSAS", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, CountryCodes.Spain);
			helper.CreateCusMap("EUNAU", "OTE", "CES", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, CountryCodes.Spain);
			helper.CreateCusMap("EUNAU", "OTI", "CIT", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, CountryCodes.Italy);
			Factory.Save();

			var authorizationUsage = Factory.New<CusAuthorizationUsage>();

			CombineAssertions("Temporary authorisation properties should propagate to authorisationUsage", () =>
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Latvia))
				{
					authorizationUsage.AGC_Code = "SAS";
					NUnit.Framework.Assert.That(authorizationUsage.CustomsCode, Is.EqualTo("C515").Using(CustomComparers.TypeComparison), "Value for SAS when country is Latvia");

					authorizationUsage.AGC_Code = "DPO";
					NUnit.Framework.Assert.That(authorizationUsage.CustomsCode, Is.EqualTo("C506").Using(CustomComparers.TypeComparison), "Cache Refreshed, value for DPO when country is Latvia");

					authorizationUsage.AGC_Code = "OTE";
					NUnit.Framework.Assert.That(authorizationUsage.CustomsCode, Is.EqualTo(ZString.Empty), "Cache Refreshed, value for OTE when country is Latvia");

					authorizationUsage.AGC_Code = "OTI";
					NUnit.Framework.Assert.That(authorizationUsage.CustomsCode, Is.EqualTo(ZString.Empty), "Cache Refreshed, value for OTI when country is Latvia");
				}
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Spain))
				{
					authorizationUsage.AGC_Code = "SAS";
					NUnit.Framework.Assert.That(authorizationUsage.CustomsCode, Is.EqualTo("CSAS").Using(CustomComparers.TypeComparison), "Value for SAS when country is Spain");

					authorizationUsage.AGC_Code = "DPO";
					NUnit.Framework.Assert.That(authorizationUsage.CustomsCode, Is.EqualTo("C506").Using(CustomComparers.TypeComparison), "Cache Refreshed, value for DPO when country is Spain");

					authorizationUsage.AGC_Code = "OTE";
					NUnit.Framework.Assert.That(authorizationUsage.CustomsCode, Is.EqualTo("CES").Using(CustomComparers.TypeComparison), "Cache Refreshed, value for OTE when country is Spain");

					authorizationUsage.AGC_Code = "OTI";
					NUnit.Framework.Assert.That(authorizationUsage.CustomsCode, Is.EqualTo(ZString.Empty), "Cache Refreshed, value for OTI when country is Spain");
				}
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
				{
					authorizationUsage.AGC_Code = "SAS";
					NUnit.Framework.Assert.That(authorizationUsage.CustomsCode, Is.EqualTo("C515").Using(CustomComparers.TypeComparison), "Value for SAS when country is Italy");

					authorizationUsage.AGC_Code = "DPO";
					NUnit.Framework.Assert.That(authorizationUsage.CustomsCode, Is.EqualTo("C506").Using(CustomComparers.TypeComparison), "Cache Refreshed, value for DPO when country is Italy");

					authorizationUsage.AGC_Code = "OTE";
					NUnit.Framework.Assert.That(authorizationUsage.CustomsCode, Is.EqualTo(ZString.Empty), "Cache Refreshed, value for OTE when country is Italy");

					authorizationUsage.AGC_Code = "OTI";
					NUnit.Framework.Assert.That(authorizationUsage.CustomsCode, Is.EqualTo("CIT").Using(CustomComparers.TypeComparison), "Cache Refreshed, value for OTI when country is Italy");
				}
			});
		}

		[ExpectNoExceptions]
		public void TestRelatedAuthorisationHeader() => CombineAssertions(() =>
		{
			var cusAuthorizationUsage = Factory.New<CusAuthorizationUsage>();
			var owner = Factory.NewWithValidTestData<OrgHeader>();

			cusAuthorizationUsage.AGC_Number = "12345";
			cusAuthorizationUsage.AGC_OH_Owner = owner.PK;
			cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
			cusAuthorizationUsage.AGC_ParentTableCode = "BH";
			NUnit.Framework.Assert.That(cusAuthorizationUsage.RelatedAuthorisationHeader, Is.EqualTo(default(CusAuthorisationHeader)), "No related authorisation should be found. - should be [null]");

			var authorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			authorisationHeader.CPH_Number = "12345";
			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
			authorisationHeader.CPH_OH_PermitHolder = owner.PK;
			Factory.Save();

			NUnit.Framework.Assert.That(cusAuthorizationUsage.RelatedAuthorisationHeader, Is.EqualTo(authorisationHeader), "A related authorisation should be found.");

			cusAuthorizationUsage.AGC_Number = "12345";
			cusAuthorizationUsage.AGC_OH_Owner = ZGuid.Empty;
			cusAuthorizationUsage.AGC_ParentTableCode = "BH";
			NUnit.Framework.Assert.That(cusAuthorizationUsage.RelatedAuthorisationHeader, Is.EqualTo(authorisationHeader), "A related authorisation should be found if Owner is empty.");

			cusAuthorizationUsage.AGC_Number = "12345";
			cusAuthorizationUsage.AGC_OH_Owner = owner.PK;
			cusAuthorizationUsage.AGC_Code = ZString.Empty;
			cusAuthorizationUsage.AGC_ParentTableCode = "BH";
			NUnit.Framework.Assert.That(cusAuthorizationUsage.RelatedAuthorisationHeader, Is.EqualTo(authorisationHeader), "A related authorisation should be found if Code is empty.");
		});

		[ExpectNoExceptions]
		public void TestRelatedAuthorisationHeaderIgnoringReferenceNumber()
		{
			var cusAuthorizationUsage = Factory.New<CusAuthorizationUsage>();
			var owner = Factory.NewWithValidTestData<OrgHeader>();

			cusAuthorizationUsage.AGC_Number = "12345";
			cusAuthorizationUsage.AGC_OH_Owner = owner.PK;
			cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
			cusAuthorizationUsage.AGC_ParentTableCode = "BH";
			NUnit.Framework.Assert.That(cusAuthorizationUsage.RelatedAuthorisationHeaderIgnoringReferenceNumber, Is.EqualTo(default(CusAuthorisationHeader)), "No related authorisation  ignoring reference number should not be found. - should be [null]");

			var authorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			authorisationHeader.CPH_Number = "67890";
			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
			authorisationHeader.CPH_OH_PermitHolder = owner.PK;
			Factory.Save();

			NUnit.Framework.Assert.That(cusAuthorizationUsage.RelatedAuthorisationHeader, Is.EqualTo(default(CusAuthorisationHeader)), "A related authorisation should not be found.");
			NUnit.Framework.Assert.That(cusAuthorizationUsage.RelatedAuthorisationHeaderIgnoringReferenceNumber, Is.EqualTo(authorisationHeader), "A related authorisation ignoring reference number should be found.");
		}

		[ExpectNoExceptions]
		public void TestCopyFromTemporaryAuthorisation()
		{
			var authorisationUsage = Factory.New<CusAuthorizationUsage>();
			authorisationUsage.AGC_Number = "11111";
			authorisationUsage.AGC_OH_Owner = ZGuid.Empty;
			authorisationUsage.AGC_Code = "BLA";

			var temporaryAuthorisation = authorisationUsage.CreateTemporaryAuthorisationFromUsage();
			temporaryAuthorisation.CPH_Number = "99999";
			temporaryAuthorisation.CPH_OH_PermitHolder = ZGuid.BrettsGuid;
			temporaryAuthorisation.CPH_Type = "ZZZ";

			authorisationUsage.CopyFromTemporaryAuthorization(temporaryAuthorisation);
			CombineAssertions("Temporary authorisation properties should propagate to authorisationUsage", () =>
			{
				NUnit.Framework.Assert.That(authorisationUsage.AGC_OH_Owner, Is.EqualTo(ZGuid.BrettsGuid), "AGC_OH_Owner");
				NUnit.Framework.Assert.That(authorisationUsage.AGC_Code, Is.EqualTo("ZZZ").Using(CustomComparers.TypeComparison), "AGC_Code");
				NUnit.Framework.Assert.That(authorisationUsage.AGC_CPH_Authorization, Is.EqualTo(temporaryAuthorisation.PK), "AGC_CPH_Authorisation");
			});
		}

		[ExpectNoExceptions]
		public void TestAGCNumberChangeWhenAGCCPHAuthorizationIsSet()
		{
			var authorisationHeader = Factory.New<CusAuthorisationHeader>();
			authorisationHeader.CPH_Number = "999";

			var authorisationUsage = Factory.New<CusAuthorizationUsage>();
			authorisationUsage.AGC_Number = "1234";
			authorisationUsage.AGC_CPH_Authorization = authorisationHeader.PK;

			NUnit.Framework.Assert.That(authorisationUsage.AGC_Number, Is.EqualTo(ZString.Empty), "AGC_Number should have been set to empty as AGC_CPH_Authorization has a value.");
		}

		[ExpectNoExceptions]
		public void TestAGCCPHAuthorizationChangeWhenAGCNulberIsSet()
		{
			var authorisationHeader = Factory.New<CusAuthorisationHeader>();
			authorisationHeader.CPH_Number = "999";

			var authorisationUsage = Factory.New<CusAuthorizationUsage>();
			authorisationUsage.AGC_CPH_Authorization = authorisationHeader.PK;
			authorisationUsage.AGC_Number = "1234";

			NUnit.Framework.Assert.That(authorisationUsage.AGC_CPH_Authorization, Is.EqualTo(ZGuid.Empty), "AGC_CPH_Authorization should have been set to empty as AGC_Number has a value.");
		}

		[ExpectNoExceptions]
		public void TestCreateTemporaryAuthorisationFromUsage()
		{
			var authorisationUsage = Factory.New<CusAuthorizationUsage>();
			authorisationUsage.AGC_Number = "12345";
			authorisationUsage.AGC_OH_Owner = ZGuid.BrettsGuid;
			authorisationUsage.AGC_Code = "BLA";
			var temporaryAuthorisation = authorisationUsage.CreateTemporaryAuthorisationFromUsage();
			CombineAssertions("Temporary authorisation created form usage properties", () =>
			{
				NUnit.Framework.Assert.That(temporaryAuthorisation.CPH_IsAdHoc, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "CPH_IsAdHoc");
				NUnit.Framework.Assert.That(temporaryAuthorisation.CPH_Number, Is.EqualTo("12345").Using(CustomComparers.TypeComparison), "CPH_Number");
				NUnit.Framework.Assert.That(temporaryAuthorisation.CPH_OH_PermitHolder, Is.EqualTo(ZGuid.BrettsGuid), "CPH_OH_PermitHolder");
				NUnit.Framework.Assert.That(temporaryAuthorisation.CPH_Type, Is.EqualTo("BLA").Using(CustomComparers.TypeComparison), "CPH_Type");
			});
			NUnit.Framework.Assert.That(authorisationUsage.Factory, Is.Not.EqualTo(temporaryAuthorisation.Factory), "Temporary Authorisation should be created by a dedicated Factory.");
		}

		[ExpectNoExceptions]
		public void TestParent() => CombineAssertions(() =>
		{
			var cusAuthorizationUsage = Factory.New<CusAuthorizationUsage>();
			cusAuthorizationUsage.Parent = Factory.New<CusEntryInstruction>();
			NUnit.Framework.Assert.That(cusAuthorizationUsage.Parent is CusEntryInstruction, Is.EqualTo(true), "CusEntryInstruction");

			cusAuthorizationUsage.Parent = Factory.New<JobComInvoiceLine>();
			NUnit.Framework.Assert.That(cusAuthorizationUsage.Parent is JobComInvoiceLine, Is.EqualTo(true), "JobComInvoiceLine");

			cusAuthorizationUsage.Parent = (BusinessObject)Factory.New<ICusTempStorageRegPremises>();
			NUnit.Framework.Assert.That(cusAuthorizationUsage.Parent is ICusTempStorageRegPremises, Is.EqualTo(true), "CusTempStorageRegPremises");

			cusAuthorizationUsage.Parent = (CusInBondHeader)Factory.New<NCTS.ICusInBondHeader>();
			NUnit.Framework.Assert.That(cusAuthorizationUsage.Parent is CusInBondHeader, Is.EqualTo(true), "CusInBondHeader");

			var cusInBondHeader = (CusInBondHeader)Factory.New<NCTS.ICusInBondHeader>();
			cusInBondHeader.BH_HeaderType = "D";
			cusAuthorizationUsage.Parent = cusInBondHeader.MovementHeader;
			NUnit.Framework.Assert.That(cusAuthorizationUsage.Parent is CusInBondMoveHeader, Is.EqualTo(true), "CusInBondMoveHeader");

			cusAuthorizationUsage.Parent = Factory.New<ICusExitReport>() as BusinessObject;
			NUnit.Framework.Assert.That(cusAuthorizationUsage.Parent is ICusExitReport, Is.EqualTo(true), "CusExitReport");

			cusAuthorizationUsage.Parent = Factory.New<ICusExitReportItem>() as BusinessObject;
			NUnit.Framework.Assert.That(cusAuthorizationUsage.Parent is ICusExitReportItem, Is.EqualTo(true), "CusExitReportItem");

			cusAuthorizationUsage.Parent = Factory.New<DummyEnterpriseBusinessObject>();
			NUnit.Framework.Assert.That(cusAuthorizationUsage.Parent, Is.EqualTo(default(BusinessObject)), "Unknown - should be [null]");
		});

		[ExpectNoExceptions]
		public void TestUseClusterKey()
		{
			var authorizationWithCusInBondHeaderParent = Factory.New<CusAuthorizationUsage>();
			authorizationWithCusInBondHeaderParent.AGC_ParentTableCode = CusInBondHeaderSchema.Constants.Prefix;

			var authorizationWithCusInBondMoveHeaderParent = Factory.New<CusAuthorizationUsage>();
			authorizationWithCusInBondMoveHeaderParent.AGC_ParentTableCode = CusInBondMoveHeaderSchema.Constants.Prefix;

			var authorizationWithCusTempStorageRegPremisesParent = Factory.New<CusAuthorizationUsage>();
			authorizationWithCusTempStorageRegPremisesParent.AGC_ParentTableCode = CusTempStorageRegPremisesSchema.Constants.Prefix;

			var authorizationWithJobComInvoiceLineParent = Factory.New<CusAuthorizationUsage>();
			authorizationWithJobComInvoiceLineParent.AGC_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;

			var authorizationWithCusEntryInstructionParent = Factory.New<CusAuthorizationUsage>();
			authorizationWithCusEntryInstructionParent.AGC_ParentTableCode = CusEntryInstructionSchema.Constants.Prefix;

			var authorizationWithAsycudaManifestHeaderParent = Factory.New<CusAuthorizationUsage>();
			authorizationWithAsycudaManifestHeaderParent.AGC_ParentTableCode = AsycudaManifestHeaderSchema.Constants.Prefix;

			var authorizationWithCusExitReportParent = Factory.New<CusAuthorizationUsage>();
			authorizationWithCusExitReportParent.AGC_ParentTableCode = CusExitReportSchema.Constants.Prefix;

			var authorizationWithCusExitReportItemParent = Factory.New<CusAuthorizationUsage>();
			authorizationWithCusExitReportItemParent.AGC_ParentTableCode = CusExitReportItemSchema.Constants.Prefix;

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(((IOptionalClusterKeyEntity)authorizationWithCusInBondHeaderParent).UseClusterKey, Is.EqualTo(false), "ClusterKey should NOT be used for NctsHeader parent");
				NUnit.Framework.Assert.That(((IOptionalClusterKeyEntity)authorizationWithCusInBondMoveHeaderParent).UseClusterKey, Is.EqualTo(false), "ClusterKey should NOT be used for NctsMoveHeader parent");
				NUnit.Framework.Assert.That(((IOptionalClusterKeyEntity)authorizationWithCusTempStorageRegPremisesParent).UseClusterKey, Is.EqualTo(false), "ClusterKey should NOT be used for CusTempStorageRegPremises parent");

				NUnit.Framework.Assert.That(((IOptionalClusterKeyEntity)authorizationWithJobComInvoiceLineParent).UseClusterKey, Is.EqualTo(true), "ClusterKey should be used for JobComInvoiceLine parent");
				NUnit.Framework.Assert.That(((IOptionalClusterKeyEntity)authorizationWithCusEntryInstructionParent).UseClusterKey, Is.EqualTo(true), "ClusterKey should be used for CusEntryInstruction parent");
				NUnit.Framework.Assert.That(((IOptionalClusterKeyEntity)authorizationWithAsycudaManifestHeaderParent).UseClusterKey, Is.EqualTo(true), "ClusterKey should be used for AsycudaManifestHeader parent");
				NUnit.Framework.Assert.That(((IOptionalClusterKeyEntity)authorizationWithCusExitReportParent).UseClusterKey, Is.EqualTo(true), "ClusterKey should be used for CusExitReport parent");
				NUnit.Framework.Assert.That(((IOptionalClusterKeyEntity)authorizationWithCusExitReportItemParent).UseClusterKey, Is.EqualTo(true), "ClusterKey should be used for CusExitReportItem parent");
			});
		}

		[ExpectNoExceptions]
		public void TestClusterKey()
		{
			var temporaryStorage = Factory.New<TemporaryStorageHeader>();
			var authorizationUsage = temporaryStorage.AuthorizationUsageOrNew;

			NUnit.Framework.Assert.That(authorizationUsage.AGC_ClusterKey, Is.EqualTo(temporaryStorage.AMA_ClusterKey), "AGC_CLusterKey should be equal to AMA_ClusterKey.");
		}

		[ExpectNoExceptions]
		public void TestAGC_Code_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			var info = authorizationUsage.AGC_CodeInfo;

			CombineAssertions("ExportUCC6", () =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					AssertCaption(info, authorizationUsage.MultipleKeysToUse, "Type", "Type", "[12 12 001 000] Type");
				}
			});

			CombineAssertions("Not ExportUCC6", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
				{
					AssertCaption(info, authorizationUsage.MultipleKeysToUse, "Code", "Code", ZString.Empty);
				}
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsCode_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			var info = authorizationUsage.CustomsCodeInfo;

			CombineAssertions("ImportUCC6", () =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					AssertCaption(info, authorizationUsage.MultipleKeysToUse, "Customs Code", "Customs Code", "[12 12 002 000] Authorization < Type");
				}
			});

			CombineAssertions("Not UCC6", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
				{
					AssertCaption(info, authorizationUsage.MultipleKeysToUse, "Customs Code", "Customs Code", ZString.Empty);
				}
			});
		}

		[ExpectNoExceptions]
		public void TestAGC_Number_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			var info = authorizationUsage.AGC_NumberInfo;

			CombineAssertions("ExportUCC6", () =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					AssertCaption(info, authorizationUsage.MultipleKeysToUse, "Reference", "Reference", "[12 12 080 000] Reference Number");
				}
			});

			CombineAssertions("ImportUCC6", () =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					AssertCaption(info, authorizationUsage.MultipleKeysToUse, "Number", "Number", "[12 12 001 000] Authorization < Reference number");
				}
			});

			CombineAssertions("Not UCC6", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
				{
					AssertCaption(info, authorizationUsage.MultipleKeysToUse, "Number", "Number", ZString.Empty);
				}
			});
		}

		[ExpectNoExceptions]
		public void TestAGC_OH_Owner_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			var info = authorizationUsage.AGC_OH_OwnerInfo;

			CombineAssertions("ImportUCC6", () =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					AssertCaption(info, authorizationUsage.MultipleKeysToUse, "Owner", "Owner", "[12 12 080 000] Authorization < Holder of the authorization");
				}
			});

			CombineAssertions("Not UCC6", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
				{
					AssertCaption(info, authorizationUsage.MultipleKeysToUse, "Owner", "Owner", ZString.Empty);
				}
			});
		}

		[ExpectNoExceptions]
		void AssertCaption(ZPropertyInfo info, IReadOnlyList<string> keys, string expectedHumanReadableName, string expectedCaption, string expectedFullDescription)
		{
			NUnit.Framework.Assert.That(info.HumanReadableName, Is.EqualTo(expectedHumanReadableName).Using(CustomComparers.TypeComparison), "HumanReadableName");
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, keys);
			NUnit.Framework.Assert.That(captionResourceString.Caption, Is.EqualTo(expectedCaption), "Caption");
			NUnit.Framework.Assert.That(captionResourceString.FullDescription, Is.EqualTo(expectedFullDescription), "FullDescription");
		}

		[ExpectNoExceptions]
		public void TestInstruction()
		{
			var testDec = Factory.New<JobDeclaration>();
			var entryInstruction = testDec.CustomsEntryInstructions.AddNew();
			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(cusAuthorizationUsage.Instruction.PK, Is.EqualTo(entryInstruction.PK));
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_ParentTableCode, Is.EqualTo(CusEntryInstructionSchema.Constants.Prefix).Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestHumanReadableName()
		{
			var cusAuthorizationUsage = CreateAuthorizationUsage();
			cusAuthorizationUsage.AGC_Number = "AAA";
			NUnit.Framework.Assert.That(cusAuthorizationUsage.HumanReadableName, Is.EqualTo("Customs Authorization AAA").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAGC_NumberDefaults()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();

			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Number = "123";
			authorizationHeader.CPH_Type = "abc";
			authorizationHeader.CPH_OH_PermitHolder = orgHeader.PK;

			cusAuthorizationUsage.AGC_Code = "abc";
			NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Number.IsEmpty, Is.EqualTo(true));
			cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
			NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Number.IsEmpty, Is.EqualTo(false));
			NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Number, Is.EqualTo("123").Using(CustomComparers.TypeComparison));

			var cusAuthorizationUsage2 = entryInstruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage2.AGC_OH_Owner = orgHeader.PK;
			NUnit.Framework.Assert.That(cusAuthorizationUsage2.AGC_Number.IsEmpty, Is.EqualTo(true));
			cusAuthorizationUsage2.AGC_Code = "abc";
			NUnit.Framework.Assert.That(cusAuthorizationUsage2.AGC_Number.IsEmpty, Is.EqualTo(false));
			NUnit.Framework.Assert.That(cusAuthorizationUsage2.AGC_Number, Is.EqualTo("123").Using(CustomComparers.TypeComparison));

			var cusAuthorizationUsage3 = entryInstruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage3.AGC_Number = "999";
			cusAuthorizationUsage3.AGC_OH_Owner = orgHeader.PK;
			NUnit.Framework.Assert.That(cusAuthorizationUsage3.AGC_Number, Is.EqualTo("999").Using(CustomComparers.TypeComparison));
			cusAuthorizationUsage3.AGC_Code = "abc";
			NUnit.Framework.Assert.That(cusAuthorizationUsage3.AGC_Number, Is.EqualTo("999").Using(CustomComparers.TypeComparison));

			var authorizationHeader2 = Factory.New<CusAuthorisationHeader>();
			authorizationHeader2.CPH_Number = "456";
			authorizationHeader2.CPH_Type = "abc";
			authorizationHeader2.CPH_OH_PermitHolder = orgHeader.PK;

			var cusAuthorizationUsage4 = entryInstruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage4.AGC_OH_Owner = orgHeader.PK;
			NUnit.Framework.Assert.That(cusAuthorizationUsage4.AGC_Number.IsEmpty, Is.EqualTo(true));
			cusAuthorizationUsage4.AGC_Code = "abc";
			NUnit.Framework.Assert.That(cusAuthorizationUsage4.AGC_Number.IsEmpty, Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestAGC_OH_OwnerDefaults()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();

			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Number = "123";
			authorizationHeader.CPH_Type = "abc";
			authorizationHeader.CPH_OH_PermitHolder = orgHeader.PK;

			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_Code = "abc";
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_OH_Owner.IsEmpty, Is.EqualTo(true), "When only code is set, owner is empty");
				cusAuthorizationUsage.AGC_Number = "123";
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_OH_Owner.IsEmpty, Is.EqualTo(false), "When number is set after code, owner is not empty");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_OH_Owner, Is.EqualTo(orgHeader.PK), "When number is set after code, owner is defaulted");

				var cusAuthorizationUsage3 = entryInstruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage3.AGC_Number = "999";
				cusAuthorizationUsage3.AGC_OH_Owner = orgHeader.PK;
				NUnit.Framework.Assert.That(cusAuthorizationUsage3.AGC_OH_Owner, Is.EqualTo(orgHeader.PK), "When owner is not empty and number is set, owner is not changed");
				cusAuthorizationUsage3.AGC_Code = "abc";
				NUnit.Framework.Assert.That(cusAuthorizationUsage3.AGC_OH_Owner, Is.EqualTo(orgHeader.PK), "When owner and number are not empty and code is set, owner is not changed");

				var authorizationHeader2 = Factory.New<CusAuthorisationHeader>();
				authorizationHeader2.CPH_Number = "123";
				authorizationHeader2.CPH_Type = "abc";
				authorizationHeader2.CPH_OH_PermitHolder = Factory.NewWithValidTestData<OrgHeader>().PK;

				var cusAuthorizationUsage4 = entryInstruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage4.AGC_Number = "123";
				NUnit.Framework.Assert.That(cusAuthorizationUsage4.AGC_OH_Owner.IsEmpty, Is.EqualTo(true), "When only number is set, owner is empty");
				cusAuthorizationUsage4.AGC_Code = "abc";
				NUnit.Framework.Assert.That(cusAuthorizationUsage4.AGC_OH_Owner.IsEmpty, Is.EqualTo(true), "When code is set after number but there are multiple authorizations with those values, owner is empty");
			});
		}

		[ExpectNoExceptions]
		public void TestAGC_CodeDefaults()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();

			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Number = "123";
			authorizationHeader.CPH_Type = "abc";
			authorizationHeader.CPH_OH_PermitHolder = orgHeader.PK;

			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Code.IsEmpty, Is.EqualTo(true), "When only owner is set, code is empty");
				cusAuthorizationUsage.AGC_Number = "123";
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Code.IsEmpty, Is.EqualTo(false), "When number is set after owner, code is not empty");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Code, Is.EqualTo("abc").Using(CustomComparers.TypeComparison), "When number is set after owner, code is defaulted");

				var cusAuthorizationUsage3 = entryInstruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage3.AGC_Number = "999";
				cusAuthorizationUsage3.AGC_Code = "abc";
				NUnit.Framework.Assert.That(cusAuthorizationUsage3.AGC_Code, Is.EqualTo("abc").Using(CustomComparers.TypeComparison), "When code is not empty and number is set, code is not changed");
				cusAuthorizationUsage3.AGC_OH_Owner = orgHeader.PK;
				NUnit.Framework.Assert.That(cusAuthorizationUsage3.AGC_Code, Is.EqualTo("abc").Using(CustomComparers.TypeComparison), "When owner and number are not empty and code is set, code is not changed");

				var authorizationHeader2 = Factory.New<CusAuthorisationHeader>();
				authorizationHeader2.CPH_Number = "123";
				authorizationHeader2.CPH_Type = "def";
				authorizationHeader2.CPH_OH_PermitHolder = orgHeader.PK;

				var cusAuthorizationUsage4 = entryInstruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage4.AGC_Number = "123";
				NUnit.Framework.Assert.That(cusAuthorizationUsage4.AGC_Code.IsEmpty, Is.EqualTo(true), "When only number is set, code is empty");
				cusAuthorizationUsage4.AGC_OH_Owner = orgHeader.PK;
				NUnit.Framework.Assert.That(cusAuthorizationUsage4.AGC_Code.IsEmpty, Is.EqualTo(true), "When owner is set after number but there are multiple authorizations with those values, code is empty");
			});
		}

		[ExpectNoExceptions]
		public void TestAGC_NumberFromEori()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var uk = Factory.Load<RefCountry>(CountryGuids.UnitedKingdom);
			org.SetCustomsCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, uk, "999999999888");

			var invoiceLineConfigurationMock = new Mock<InstructionConfiguration>();
			invoiceLineConfigurationMock.Protected()
				.Setup<ZBool>("UseEoriForAuthorisationReferenceCore")
				.Returns(ZBool.True);

			using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(Factory, "GetNewInstructionConfiguration", invoiceLineConfigurationMock.Object, null))
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();

				cusAuthorizationUsage.AGC_Code = "1";
				cusAuthorizationUsage.AGC_OH_Owner = org.PK;
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Number, Is.EqualTo("GB999999999888").Using(CustomComparers.TypeComparison), "AGC_Number");
			}
		}

		[ExpectNoExceptions]
		public void TestSetAuthorisationProperties()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();

			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Number = "123";
			authorizationHeader.CPH_Type = "abc";
			authorizationHeader.CPH_OH_PermitHolder = orgHeader.PK;

			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_Number = "123";
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Number, Is.EqualTo("123").Using(CustomComparers.TypeComparison), "Number is set");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_OH_Owner, Is.EqualTo(orgHeader.PK), "Owner is set");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Code, Is.EqualTo("abc").Using(CustomComparers.TypeComparison), "Code is set");

				cusAuthorizationUsage.AGC_Number = ZString.Empty;
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Number, Is.EqualTo(ZString.Empty), "Number is AGC_Number");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_OH_Owner, Is.EqualTo(orgHeader.PK), "Owner is left as is when AGC_Number is empty");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Code, Is.EqualTo("abc").Using(CustomComparers.TypeComparison), "Code is left as is when AGC_Number is empty");

				var authorizationHeader2 = Factory.New<CusAuthorisationHeader>();
				authorizationHeader2.CPH_Number = "456";
				authorizationHeader2.CPH_Type = "defgh";
				authorizationHeader2.CPH_OH_PermitHolder = orgHeader.PK;

				cusAuthorizationUsage.AGC_Number = ZString.Empty;
				cusAuthorizationUsage.AGC_OH_Owner = ZGuid.Empty;
				cusAuthorizationUsage.AGC_Code = ZString.Empty;

				cusAuthorizationUsage.AGC_Number = "456";
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Number, Is.EqualTo("456").Using(CustomComparers.TypeComparison), "Number is set for authorization2");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_OH_Owner, Is.EqualTo(orgHeader.PK), "Owner is set for authorization2");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Code, Is.EqualTo("defg").Using(CustomComparers.TypeComparison), "Code is set for authorization2 (only first 4 characters)");

				cusAuthorizationUsage.AGC_Number = "789";
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Number, Is.EqualTo("789").Using(CustomComparers.TypeComparison), "Number is set for given code (no authorization exists)");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_OH_Owner, Is.EqualTo(orgHeader.PK), "Owner is left as is when AGC_Number is not in the authorization list");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Code, Is.EqualTo("defg").Using(CustomComparers.TypeComparison), "Code is left as is when AGC_Number is not in the authorization list");

				var authorizationHeader3 = Factory.New<CusAuthorisationHeader>();
				authorizationHeader3.CPH_Number = "456";
				authorizationHeader3.CPH_Type = "ijk";
				authorizationHeader3.CPH_OH_PermitHolder = orgHeader.PK;

				cusAuthorizationUsage.AGC_Number = ZString.Empty;
				cusAuthorizationUsage.AGC_OH_Owner = ZGuid.Empty;
				cusAuthorizationUsage.AGC_Code = ZString.Empty;

				cusAuthorizationUsage.AGC_Number = "456";
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Number, Is.EqualTo("456").Using(CustomComparers.TypeComparison), "Number is set for given code (it is repeated in 2 authorizations)");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_OH_Owner, Is.EqualTo(ZGuid.Empty), "Owner is left empty when there are more than one authorization with the selected number");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Code, Is.EqualTo(ZString.Empty), "Code is left empty when there are more than one authorization with the selected number");
			});
		}

		[ExpectNoExceptions]
		public void TestLoadParent()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var testDec = Factory.New<JobDeclaration>();
			var entryInstruction = testDec.CustomsEntryInstructions.AddNew();
			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_Code = "abc";
			cusAuthorizationUsage.AGC_Number = "123";
			cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;

			var cusAuthorizationUsageLoaded = CusAuthorizationUsage.Load(entryInstruction);

			NUnit.Framework.Assert.That(cusAuthorizationUsage.PK, Is.EqualTo(cusAuthorizationUsageLoaded.PK), "The cusAuthorisationUsageLoaded should be equals the cusAuthorizationUsage of the entryInstruction.");

			var entryInstruction2 = testDec.CustomsEntryInstructions.AddNew();
			var cusAuthorizationUsageLoaded2 = CusAuthorizationUsage.Load(entryInstruction2);

			NUnit.Framework.Assert.That(cusAuthorizationUsageLoaded2, Is.EqualTo(default(CusAuthorizationUsage)), "The cusAuthorisationUsageLoaded2 should be null  as entryInstruction2 has no cusAuthorizationUsage. - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestNewFromParent()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var testDec = Factory.New<JobDeclaration>();
			var entryInstruction = testDec.CustomsEntryInstructions.AddNew();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Count, Is.EqualTo(0), "No CusAuthorizationUsages.");
			var cusAuthorizationUsageCreated = CusAuthorizationUsage.New(entryInstruction);
			entryInstruction.CusAuthorizationUsages.Load();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Count, Is.EqualTo(1), "1 CusAuthorizationUsage has been created.");
		}
		[ExpectNoExceptions]
		public void TestAGCNumberFieldType_EnableAdHoc()
		{
			var cusAuthorisationHeaderProviderMock = new Mock<CusAuthorisationHeaderProvider>(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			cusAuthorisationHeaderProviderMock.CallBase = true;

			cusAuthorisationHeaderProviderMock.Protected()
				.Setup<bool>("EnableAdHocCore")
				.Returns(true);

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(m => m.GetObject(It.IsAny<object>())).Returns(cusAuthorisationHeaderProviderMock.Object);

			var config = new Hashtable();
			config.Add(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, objectHandleMock.Object);

			using (ObjectFactory.Substitute("CusAuthorisationHeaderProviders", config))
			{
				CombineAssertions(() =>
				{
					var cusAuthorizationUsage = Factory.New<CusAuthorizationUsage>();
					NUnit.Framework.Assert.That(cusAuthorizationUsage.EnableAdHoc, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Prerequisite: ");
					NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_NumberFieldType, Is.EqualTo(nameof(FieldType.Text)).Using(CustomComparers.TypeComparison), "FieldType from AGCNUmber if EnableAdHoc is true should be text.");
				});
			}
		}

		[ExpectNoExceptions]
		public void TestAGC_NumberFieldType_IsAgcNumberFieldALookup_True()
		{
			var cusAuthorisationHeaderProviderMock = new Mock<CusAuthorisationHeaderProvider>(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			cusAuthorisationHeaderProviderMock.CallBase = true;

			cusAuthorisationHeaderProviderMock.Protected()
				.Setup<bool>("IsAgcNumberFieldALookupCore")
				.Returns(true);

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(m => m.GetObject(It.IsAny<object>())).Returns(cusAuthorisationHeaderProviderMock.Object);

			var config = new Hashtable();
			config.Add(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, objectHandleMock.Object);

			using (ObjectFactory.Substitute("CusAuthorisationHeaderProviders", config))
			{
				CombineAssertions(() =>
				{
					var cusAuthorizationUsage = Factory.New<CusAuthorizationUsage>();
					NUnit.Framework.Assert.That(cusAuthorizationUsage.IsAgcNumberFieldALookup, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Prerequisite: ");
					NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_NumberFieldType, Is.EqualTo(nameof(FieldType.TextCodeFindBox)).Using(CustomComparers.TypeComparison), "FieldType from AGC_Number if IsAgcNumberFieldALookup is true should be a lookup.");
				});
			}
		}

		[ExpectNoExceptions]
		public void TestAGC_NumberFieldType_IsAgcNumberFieldALookup_False()
		{
			var cusAuthorisationHeaderProviderMock = new Mock<CusAuthorisationHeaderProvider>(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			cusAuthorisationHeaderProviderMock.CallBase = true;

			cusAuthorisationHeaderProviderMock.Protected()
				.Setup<bool>("IsAgcNumberFieldALookupCore")
				.Returns(false);

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(m => m.GetObject(It.IsAny<object>())).Returns(cusAuthorisationHeaderProviderMock.Object);

			var config = new Hashtable();
			config.Add(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, objectHandleMock.Object);

			using (ObjectFactory.Substitute("CusAuthorisationHeaderProviders", config))
			{
				CombineAssertions(() =>
				{
					var cusAuthorizationUsage = Factory.New<CusAuthorizationUsage>();
					NUnit.Framework.Assert.That(cusAuthorizationUsage.IsAgcNumberFieldALookup, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Prerequisite: ");
					NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_NumberFieldType, Is.EqualTo(nameof(FieldType.Text)).Using(CustomComparers.TypeComparison), "FieldType from AGC_Number if IsAgcNumberFieldALookup is false should be a text field.");
				});
			}
		}

		[ExpectNoExceptions]
		public void TestDefaultAGC_Number_IgnoreAdhocAuths()
		{
			var cusAuthorisationHeaderProviderMock = new Mock<CusAuthorisationHeaderProvider>(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			cusAuthorisationHeaderProviderMock.CallBase = true;

			cusAuthorisationHeaderProviderMock.Protected()
				.Setup<bool>("EnableAdHocCore")
				.Returns(true);

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(m => m.GetObject(It.IsAny<object>())).Returns(cusAuthorisationHeaderProviderMock.Object);

			var config = new Hashtable();
			config.Add(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, objectHandleMock.Object);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Number = "123";
			authorizationHeader.CPH_Type = "abc";
			authorizationHeader.CPH_IsAdHoc = true;
			authorizationHeader.CPH_OH_PermitHolder = orgHeader.PK;

			using (ObjectFactory.Substitute("CusAuthorisationHeaderProviders", config))
			{
				CombineAssertions(() =>
				{
					var cusAuthorizationUsage = Factory.New<CusAuthorizationUsage>();
					cusAuthorizationUsage.AGC_Code = "abc";
					cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
					NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Number.IsEmpty, Is.EqualTo(true), "Adhoc authorisation should be ignored when EnableAdHoc is true");
				});
			}
		}

		[ExpectNoExceptions]
		public void TestAGCNumberFieldType_NotEnableAdHoc()
		{
			CombineAssertions(() =>
			{
				var cusAuthorizationUsage = Factory.New<CusAuthorizationUsage>();
				NUnit.Framework.Assert.That(cusAuthorizationUsage.EnableAdHoc, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Prerequisite: ");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_NumberFieldType, Is.EqualTo(nameof(FieldType.TextCodeFindBox)).Using(CustomComparers.TypeComparison), "FieldType from AGCNUmber if EnableAdHoc is false should be TextCodeFindBox.");
			});
		}

		[ExpectNoExceptions]
		public void TestAGCNumberReadOnly_EnableAdHoc()
		{
			var cusAuthorisationHeaderProviderMock = new Mock<CusAuthorisationHeaderProvider>(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			cusAuthorisationHeaderProviderMock.CallBase = true;

			cusAuthorisationHeaderProviderMock.Protected()
				.Setup<bool>("EnableAdHocCore")
				.Returns(true);

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(m => m.GetObject(It.IsAny<object>())).Returns(cusAuthorisationHeaderProviderMock.Object);

			var config = new Hashtable();
			config.Add(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, objectHandleMock.Object);

			using (ObjectFactory.Substitute("CusAuthorisationHeaderProviders", config))
			{
				CombineAssertions(() =>
				{
					var cusAuthorizationUsage = Factory.New<CusAuthorizationUsage>();
					NUnit.Framework.Assert.That(cusAuthorizationUsage.EnableAdHoc, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Prerequisite: ");
					NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_NumberInfo.ReadOnly, Is.True, "AGC_Number should be read only.");
				});
			}
		}

		[ExpectNoExceptions]
		public void TestAGCNumberReadOnly_NotEnableAdHoc()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			var authorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			NUnit.Framework.Assert.That(cusAuthorizationUsage.EnableAdHoc, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Prerequisite: ");
			NUnit.Framework.Assert.That(!cusAuthorizationUsage.AGC_NumberInfo.ReadOnly, Is.True, "AGC_Number should not be read only.");
		}

		[ExpectNoExceptions]
		public void TestValueSetStrategy()
		{
			var cusAuthorizationUsage = Factory.New<CusAuthorizationUsageForTest>();
			NUnit.Framework.Assert.That(cusAuthorizationUsage.GetValueSetStrategyCore(), Is.TypeOf<CusAuthorizationUsageValueSetStrategy>());
		}

		[ExpectNoExceptions]
		public void TestCusAuthorisationHeader()
		{
			var cusAuthorizationUsage = Factory.New<CusAuthorizationUsage>();
			var authorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			NUnit.Framework.Assert.That(cusAuthorizationUsage.AuthorisationHeader, Is.EqualTo(default(Integration.Customs.ICusAuthorisationHeader)), "CusAuthorisationHeader should be null as AGC_CPH_Authorization is empty. - should be [null]");

			cusAuthorizationUsage.AGC_CPH_Authorization = authorisationHeader.PK;
			NUnit.Framework.Assert.That(cusAuthorizationUsage.AuthorisationHeader.PK, Is.EqualTo(authorisationHeader.PK), "CusAuthorisationHeader should be equal authorisationHeader.");

			var authorisationHeader2 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			cusAuthorizationUsage.AGC_CPH_Authorization = authorisationHeader2.PK;
			NUnit.Framework.Assert.That(cusAuthorizationUsage.AuthorisationHeader.PK, Is.EqualTo(authorisationHeader2.PK), "CusAuthorisationHeader should be equal authorisationHeader2.");

			cusAuthorizationUsage.AGC_CPH_Authorization = ZGuid.Empty;
			NUnit.Framework.Assert.That(cusAuthorizationUsage.AuthorisationHeader, Is.EqualTo(default(Integration.Customs.ICusAuthorisationHeader)), "CusAuthorisationHeader should be null again as AGC_CPH_Authorization is again empty. - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestProvider()
		{
			var cusAuthorizationUsage = Factory.New<CusAuthorizationUsage>();
			var owner = Factory.NewWithValidTestData<OrgHeader>();

			NUnit.Framework.Assert.That(Customs.Business.CusAuthorisationHeaderProvider.GetByCountryCode(GlbCompany.CurrentCompany.GC_RN_NKCountryCode).CountryCode, Is.EqualTo(cusAuthorizationUsage.AuthorisationHeaderProvider.CountryCode), "CusAuthorisationHeader is null => provider should come from current company country code.");

			var authorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			var authorisationHeader2 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			authorisationHeader2.CPH_RN_NKCountryCode = CountryCodes.France;

			cusAuthorizationUsage.AGC_CPH_Authorization = authorisationHeader.PK;
			NUnit.Framework.Assert.That(Customs.Business.CusAuthorisationHeaderProvider.GetByCountryCode(GlbCompany.CurrentCompany.GC_RN_NKCountryCode).CountryCode, Is.EqualTo(cusAuthorizationUsage.AuthorisationHeaderProvider.CountryCode), "CPH_RN_NKCountryCode is empty => provider should come from current company country code.");

			cusAuthorizationUsage.AGC_CPH_Authorization = authorisationHeader2.PK;
			NUnit.Framework.Assert.That(Customs.Business.CusAuthorisationHeaderProvider.GetByCountryCode(CountryCodes.France).CountryCode, Is.EqualTo(cusAuthorizationUsage.AuthorisationHeaderProvider.CountryCode), "CPH_RN_NKCountryCode is FR => provider should come from FR.");
		}

		[ExpectNoExceptions]
		public void TestEffectiveReferenceNumber_UseEffectiveReferenceNumber()
		{
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Number = "123";
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
			authorizationHeader.CPH_IsAdHoc = true;
			authorizationHeader.CPH_OH_PermitHolder = owner.PK;

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusAuthorizationUsage = Factory.New<CusAuthorizationUsageForTest>();
			cusAuthorizationUsage.AGC_ParentID = entryInstruction.PK;
			cusAuthorizationUsage.AGC_ParentTableCode = entryInstruction.TablePrefix;
			cusAuthorizationUsage.AGC_OH_Owner = owner.PK;
			cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
			cusAuthorizationUsage.UseEffectiveReferenceNumberOverriden = true;

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(cusAuthorizationUsage.EffectiveReferenceNumberInfo.MaxLength, Is.EqualTo(CusAuthorizationUsage.Schema.AGC_NumberMaxLength), "MaxLength");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.EffectiveReferenceNumber, Is.EqualTo(ZString.Empty), "Default");

				cusAuthorizationUsage.EffectiveReferenceNumber = "987";
				NUnit.Framework.Assert.That(cusAuthorizationUsage.EffectiveReferenceNumber, Is.EqualTo("987").Using(CustomComparers.TypeComparison), "non existing authorizationHeader");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.EffectiveReferenceNumber, Is.EqualTo(cusAuthorizationUsage.AGC_Number), "CalculatedAuthorizationNumber should be the same as AGC_Number");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AuthorisationHeader, Is.EqualTo(default(Integration.Customs.ICusAuthorisationHeader)), "When AGC_Number exists AuthorisationHeader should be empty - should be [null]");

				cusAuthorizationUsage.EffectiveReferenceNumber = "123";
				NUnit.Framework.Assert.That(cusAuthorizationUsage.EffectiveReferenceNumber, Is.EqualTo("123").Using(CustomComparers.TypeComparison), "existing authorizationHeader");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.EffectiveReferenceNumber, Is.EqualTo(cusAuthorizationUsage.AuthorisationHeader.CPH_Number), "CalculatedAuthorizationNumber should be the same as AuthorisationHeader");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Number, Is.EqualTo(ZString.Empty), "When AuthorisationHeader exists AGC_Number should be empty");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_CPH_Authorization, Is.EqualTo(authorizationHeader.PK), "123 AuthorizationHeader exists, AGC_CPH_Authorization = authorizationHeader.PK");

				cusAuthorizationUsage.EffectiveReferenceNumber = "987";
				cusAuthorizationUsage.AGC_Code = default;
				cusAuthorizationUsage.EffectiveReferenceNumber = "123";
				NUnit.Framework.Assert.That(cusAuthorizationUsage.EffectiveReferenceNumber, Is.EqualTo("123").Using(CustomComparers.TypeComparison), "Empty AGC_Code: existing authorizationHeader");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.EffectiveReferenceNumber, Is.EqualTo(cusAuthorizationUsage.AuthorisationHeader.CPH_Number), "Empty AGC_Code: CalculatedAuthorizationNumber should be the same as AuthorisationHeader");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Number, Is.EqualTo(ZString.Empty), "Empty AGC_Code: When AuthorisationHeader exists AGC_Number should be empty");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Code, Is.EqualTo(authorizationHeader.CPH_Type), "Empty AGC_Code: AGC_Code must load value from authorizationHeader.CPH_Type");

				cusAuthorizationUsage.EffectiveReferenceNumber = "987";
				cusAuthorizationUsage.AGC_OH_Owner = default;
				cusAuthorizationUsage.EffectiveReferenceNumber = "123";
				NUnit.Framework.Assert.That(cusAuthorizationUsage.EffectiveReferenceNumber, Is.EqualTo("123").Using(CustomComparers.TypeComparison), "Empty AGC_OH_Owner: existing authorizationHeader");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.EffectiveReferenceNumber, Is.EqualTo(cusAuthorizationUsage.AuthorisationHeader.CPH_Number), "Empty AGC_OH_Owner: CalculatedAuthorizationNumber should be the same as AuthorisationHeader");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Number, Is.EqualTo(ZString.Empty), "Empty AGC_OH_Owner: When AuthorisationHeader exists AGC_Number should be empty");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_OH_Owner, Is.EqualTo(authorizationHeader.CPH_OH_PermitHolder), "Empty AGC_OH_Owner: AGC_OH_Owner must load value from authorizationHeader.CPH_OH_PermitHolder");
			});
		}

		[ExpectNoExceptions]
		public void TestEffectiveReferenceNumber_NotUseEffectiveReferenceNumber()
		{
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Number = "123";
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
			authorizationHeader.CPH_IsAdHoc = true;
			authorizationHeader.CPH_OH_PermitHolder = owner.PK;

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusAuthorizationUsage = Factory.New<CusAuthorizationUsageForTest>();
			cusAuthorizationUsage.AGC_ParentID = entryInstruction.PK;
			cusAuthorizationUsage.AGC_ParentTableCode = entryInstruction.TablePrefix;
			cusAuthorizationUsage.AGC_OH_Owner = owner.PK;
			cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
			cusAuthorizationUsage.UseEffectiveReferenceNumberOverriden = false;

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(cusAuthorizationUsage.EffectiveReferenceNumberInfo.MaxLength, Is.EqualTo(CusAuthorizationUsage.Schema.AGC_NumberMaxLength), "MaxLength");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.EffectiveReferenceNumber, Is.EqualTo(ZString.Empty), "Default");

				cusAuthorizationUsage.EffectiveReferenceNumber = "987";
				NUnit.Framework.Assert.That(cusAuthorizationUsage.EffectiveReferenceNumber, Is.EqualTo("987").Using(CustomComparers.TypeComparison), "non existing authorizationHeader");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.EffectiveReferenceNumber, Is.EqualTo(cusAuthorizationUsage.AGC_Number), "CalculatedAuthorizationNumber should be the same as AGC_Number");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AuthorisationHeader, Is.EqualTo(default(Integration.Customs.ICusAuthorisationHeader)), "When AGC_Number exists AuthorisationHeader should be empty - should be [null]");

				cusAuthorizationUsage.EffectiveReferenceNumber = "123";
				NUnit.Framework.Assert.That(cusAuthorizationUsage.EffectiveReferenceNumber, Is.EqualTo("123").Using(CustomComparers.TypeComparison), "123 AuthorizationHeader exists");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.EffectiveReferenceNumber, Is.EqualTo(cusAuthorizationUsage.AGC_Number), "123 AuthorizationHeader exists, EffectiveReferenceNumber = AGC_Number");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AuthorisationHeader, Is.EqualTo(default(Integration.Customs.ICusAuthorisationHeader)), "123 AuthorizationHeader exists, AuthorisationHeader is null - should be [null]");

				cusAuthorizationUsage.EffectiveReferenceNumber = "987";
				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedIssuer;
				cusAuthorizationUsage.EffectiveReferenceNumber = "123";
				NUnit.Framework.Assert.That(cusAuthorizationUsage.EffectiveReferenceNumber, Is.EqualTo("123").Using(CustomComparers.TypeComparison), "Empty AGC_Code: 123 AuthorizationHeader exists");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.EffectiveReferenceNumber, Is.EqualTo(cusAuthorizationUsage.AGC_Number), "Empty AGC_Code: 123 AuthorizationHeader exists, EffectiveReferenceNumber = AGC_Number");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AuthorisationHeader, Is.EqualTo(default(Integration.Customs.ICusAuthorisationHeader)), "Empty AGC_Code: 123 AuthorizationHeader exists, AuthorisationHeader is null - should be [null]");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Code, Is.EqualTo(CusAuthorizationHeaderTypeList.Codes.AuthorizedIssuer).Using(CustomComparers.TypeComparison), "Empty AGC_Code: AGC_Code still unchanged");

				cusAuthorizationUsage.EffectiveReferenceNumber = "987";
				cusAuthorizationUsage.AGC_OH_Owner = ZGuid.BrettsGuid;
				cusAuthorizationUsage.EffectiveReferenceNumber = "123";
				NUnit.Framework.Assert.That(cusAuthorizationUsage.EffectiveReferenceNumber, Is.EqualTo("123").Using(CustomComparers.TypeComparison), "Empty AGC_OH_Owner: 123 AuthorizationHeader exists");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.EffectiveReferenceNumber, Is.EqualTo(cusAuthorizationUsage.AGC_Number), "Empty AGC_OH_Owner: 123 AuthorizationHeader exists, EffectiveReferenceNumber = AGC_Number");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AuthorisationHeader, Is.EqualTo(default(Integration.Customs.ICusAuthorisationHeader)), "Empty AGC_OH_Owner: 123 AuthorizationHeader exists, AuthorisationHeader is null - should be [null]");
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_OH_Owner, Is.EqualTo(ZGuid.BrettsGuid), "Empty AGC_OH_Owner: AGC_OH_Owner still unchanged");
			});
		}

		[ExpectNoExceptions]
		public void TestUseEffectiveReferenceNumber() => NUnit.Framework.Assert.That(CreateAuthorizationUsage().UseEffectiveReferenceNumber, Is.EqualTo(false), "This should be set to false as default for EU");

		[ExpectNoExceptions]
		public void TestPopulateCustomsOfficesForCentralizedClearance() => CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.CustomsOffices.RemoveAndDeleteAll();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();

			using (var testContext = new CusAuthorizationUsageValidationDeciderTestContext(declaration, isUCC6: true, isPopulateAuthorisationsForOfficeOfPresentationEnabled: true))
			{
				AssertOfficeOfPresentationPopulated(mustHaveOfficeOfPresentation: false);

				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				AssertOfficeOfPresentationPopulated(mustHaveOfficeOfPresentation: false);

				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CentralizedClearance;
				AssertOfficeOfPresentationPopulated(mustHaveOfficeOfPresentation: true);
				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;

				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CentralizedClearance;
				NUnit.Framework.Assert.That(declaration.CustomsOffices.Count, Is.EqualTo(1), "Offices count not changes");
				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;

				declaration.CustomsOffices[0].CY_Code = EuOfficeCodesTypes.Codes.CustomsOfficeForTemporaryStorage;
				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CentralizedClearance;
				NUnit.Framework.Assert.That(declaration.CustomsOffices.Count, Is.EqualTo(2), "Offices count changes");
				AssertOfficeOfPresentationPopulated(mustHaveOfficeOfPresentation: true);
			}

			using (var testContext = new CusAuthorizationUsageValidationDeciderTestContext(declaration, isUCC6: true, isPopulateAuthorisationsForOfficeOfPresentationEnabled: false))
			{
				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				declaration.CustomsOffices.RemoveAndDeleteAll();
				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CentralizedClearance;
				AssertOfficeOfPresentationPopulated(mustHaveOfficeOfPresentation: false);
			}

			using (var testContext = new CusAuthorizationUsageValidationDeciderTestContext(declaration, isUCC6: true, isPopulateAuthorisationsForOfficeOfPresentationEnabled: true))
			{
				declaration.CustomsOffices.RemoveAndDeleteAll();

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CentralizedClearance;
				AssertOfficeOfPresentationPopulated(mustHaveOfficeOfPresentation: false);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
			declaration.CustomsOffices.RemoveAndDeleteAll();
			using (var testContext = new CusAuthorizationUsageValidationDeciderTestContext(declaration, isUCC6: false))
			{
				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CentralizedClearance;
				AssertOfficeOfPresentationPopulated(mustHaveOfficeOfPresentation: false);
			}

			void AssertOfficeOfPresentationPopulated(bool mustHaveOfficeOfPresentation)
			{
				var isPopulateAuthorisationsForOfficeOfPresentationEnabled = declaration.Configuration.IsPopulateAuthorisationsForOfficeOfPresentationEnabled(declaration);
				var officesCodes = ZString.Join(", ", declaration.CustomsOffices.Select(x => x.CY_Code).ToArray());
				var description =
					$"IsUCC6: {declaration.IsUCC6}\r\n" +
					$"IsExport: {declaration.IsExport}\r\n" +
					$"populate authorisations enabled: {isPopulateAuthorisationsForOfficeOfPresentationEnabled}\r\n" +
					$"AGC_Code: {cusAuthorizationUsage.AGC_Code}\r\n" +
					$"Office codes: {officesCodes}";
				if (mustHaveOfficeOfPresentation)
				{
					NUnit.Framework.Assert.That(declaration.CustomsOffices.ContainsCode(EuOfficeCodesTypes.Codes.OfficeOfPresentation), Is.True, description);
				}
				else
				{
					NUnit.Framework.Assert.That(declaration.CustomsOffices.ContainsCode(EuOfficeCodesTypes.Codes.OfficeOfPresentation), Is.EqualTo(false), description);
				}
			}
		});

		protected override BusinessObject GetNewBusinessObject()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var cusAuthorizationUsage = CreateAuthorizationUsage();
			cusAuthorizationUsage.AGC_Code = "abc";
			cusAuthorizationUsage.AGC_Number = "123";
			cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
			return cusAuthorizationUsage;
		}

		CusAuthorizationUsage CreateAuthorizationUsage()
		{
			var testDec = Factory.New<JobDeclaration>();
			var entryInstruction = testDec.CustomsEntryInstructions.AddNew();
			return entryInstruction.CusAuthorizationUsages.AddNew();
		}
	}

	[TestedType(typeof(CusAuthorizationUsage))]
	class CusAuthorizationClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		/// <summary>
		/// CusAuthorizationUsage must have a parent (CusEntryInstruction or JobComInvoiceLine) as enforced by Constraint_AGC_InstructionOrInvoiceLine.
		/// </summary>
		protected override bool IsFkToParentMandatory() => true;

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var entryInstruction = (CusEntryInstruction)NewParentObject();
			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_Code = "abc";
			cusAuthorizationUsage.AGC_Number = "123";
			cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
			return cusAuthorizationUsage;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var testDec = Factory.New<JobDeclaration>();
			var entryInstruction = testDec.CustomsEntryInstructions.AddNew();
			return entryInstruction;
		}
	}

	[TestedType(typeof(CusAuthorizationUsage))]
	class CusAuthorizationClusterKeyInvoiceLineTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		/// <summary>
		/// CusAuthorizationUsage must have a parent (CusEntryInstruction or JobComInvoiceLine) as enforced by Constraint_AGC_InstructionOrInvoiceLine.
		/// </summary>
		protected override bool IsFkToParentMandatory() => true;

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var invoiceLine = (JobComInvoiceLine)NewParentObject();
			var cusAuthorizationUsage = invoiceLine.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_Code = "abc";
			cusAuthorizationUsage.AGC_Number = "123";
			cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
			return cusAuthorizationUsage;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var testDec = Factory.New<JobDeclaration>();
			var invoiceLine = testDec.Invoices.AddNew().InvoiceLines.AddNew();
			return invoiceLine;
		}

		[ExpectNoExceptions]
		public void TestInvoiceLine()
		{
			var testDec = Factory.New<JobDeclaration>();
			var invoiceLine = testDec.Invoices.AddNew().InvoiceLines.AddNew();
			var authorizationsUsage = invoiceLine.CusAuthorizationUsages.AddNew();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(authorizationsUsage.InvoiceLine, Is.EqualTo(invoiceLine), "InvoiceLine on CusAuthorizationsUsage");
				NUnit.Framework.Assert.That(authorizationsUsage.AGC_ParentTableCode, Is.EqualTo(JobComInvoiceLineSchema.Constants.Prefix).Using(CustomComparers.TypeComparison));
			});
		}
	}

	class CusAuthorizationUsageForTest : CusAuthorizationUsage
	{
		public CusAuthorizationUsageForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public IValueSetStrategy GetValueSetStrategyCore() => base.GetValueSetStrategy();

		public bool UseEffectiveReferenceNumberOverriden { get; set; }
		protected override bool UseEffectiveReferenceNumberCore => UseEffectiveReferenceNumberOverriden;
	}
}
