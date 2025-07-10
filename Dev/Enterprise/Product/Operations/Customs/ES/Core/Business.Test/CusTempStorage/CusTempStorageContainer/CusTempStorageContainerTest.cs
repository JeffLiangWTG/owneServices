using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageContainer))]
	class CusTempStorageContainerTest : CusCodeDataTest<CusTempStorageContainer>
	{
		public void TestSetDefaultValues()
		{
			AssertEquals("TSC", tempStorageContainer.CY_Type);
		}

		#region Implementation

		protected override IEnumerable<CusTempStorageContainer> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return GetNewTempStorageContainer(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewTempStorageContainer(factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewTempStorageContainer();
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			tempStorageContainer = GetNewTempStorageContainer();
		}
		CusTempStorageContainer tempStorageContainer;

		CusTempStorageContainer GetNewTempStorageContainer(BusinessObjectFactory factory = null)
		{
			var currentFactory = factory ?? Factory;
			var storageHeader = CusTempStorageJobHeader.New(currentFactory);
			storageHeader.SJH_OH_Customer = currentFactory.NewWithValidTestData<OrgHeader>().PK;
			return storageHeader.CusTempStorageDec.CusTempStorageContainers.AddNew();
		}

		public new void TestCorrectlyTypeDecided()
		{
			CombineAssertions(() =>
			{
				var factory = new BusinessObjectFactory();
				var bizObjs = GetBizObjsForCorrectlyTypeDecideTest(factory).ToArray();
				factory.Save();
				var supporterFetchStrategyType = typeof(Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy);
				var parentsNotSetupCorrectly = new List<string>();
				foreach (var bizObj in bizObjs)
				{
					string parentType = "Unknown";
					var parent = bizObj.Parent;
					if (parent != null)
					{
						parentType = parent.GetType().FullName;
						var fetchStrategy = (parent as IAdditionalBusinessObjectFetchStrategyProvider)?.GetFetchStrategies().FirstOrDefault(x => supporterFetchStrategyType.IsAssignableFrom(x.GetType()));
						if (fetchStrategy == null && !parentsNotSetupCorrectly.Contains(parentType))
						{
							parentsNotSetupCorrectly.Add(parentType);
						}
					}
					Assert($"{bizObj.GetType().FullName} (Parent: {parentType}) was deleted", !bizObj.IsDeleted);
					var newFactory = new BusinessObjectFactory();
					LoadParentIfNeeded(newFactory, bizObj);
					CusTempStorageContainer bizObjInDiffFactory = null;
					AssertNoExceptionThrown($"Loading {bizObj.GetType().FullName} (Parent: {parentType})", () => bizObjInDiffFactory = newFactory.Load<CusTempStorageContainer>(bizObj.PK));
					AssertEquals(typeof(CusTempStorageContainer), bizObjInDiffFactory.GetType());
				}
				if (parentsNotSetupCorrectly.Count > 0)
				{
					Fail($"The following BizObjs needs to implement {typeof(IAdditionalBusinessObjectFetchStrategyProvider).FullName} and return either {supporterFetchStrategyType.FullName} or a subclass of it.\r\n{new ZStringBuilder(parentsNotSetupCorrectly).ToStringWithNewLineBetweenAppends()}");
				}
			});
		}
	}
}
