using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AQISEntityIdLookupsTest : TestCaseWithFactory
	{
		public void TestEntityIdListSortedOrder()
		{
			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var entity1 = CMRAqisEntity.New(Factory);
				entity1.QE_AQISEntityIdentifier = "12345";
				entity1.QE_AQISEntityName = "Test Entity";

				var entity2 = CMRAqisEntity.New(Factory);
				entity2.QE_AQISEntityIdentifier = "56789";
				entity2.QE_AQISEntityName = "An Entity";

				Factory.Save();

				var entityId = new AQISEntityId(Factory);
				var entityList = entityId.Lookups.AQISEntityIdList;
				AssertEquals("Elements are sorted by description", "56789, 12345", entityList.CodesAsString);
			}

			Factory.ClearCachedValue<CodeDescriptionPairList>("AQISEntityIdLookups.AQISEntityIDList");

			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				universalDataHelper.CreateNewOrGetExistingCusCodeType(AUConstants.RefCusCodeTypeCodes.CMRAE, "AQIS Entity ID", Core.Constants.CountryCodes.Australia);
				universalDataHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRAE, "REFEntityId1", "REFEntityDescription1", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
				universalDataHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRAE, "REFEntityId2", "A REFEntityDescription2", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
				universalDataHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRAE, "REFEntityId3", "Description3", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
				Factory.Save();

				var aqisEntityIdList = new AQISEntityId(Factory).Lookups.AQISEntityIdList;
				AssertEquals("Elements are sorted by description", "REFEntityId2, REFEntityId3, REFEntityId1", aqisEntityIdList.CodesAsString);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(CMRAqisEntity.Schema.TableName);
			universalDataHelper = new UniversalReferenceTestDataHelper(Factory);
		}
		UniversalReferenceTestDataHelper universalDataHelper;
	}
}
