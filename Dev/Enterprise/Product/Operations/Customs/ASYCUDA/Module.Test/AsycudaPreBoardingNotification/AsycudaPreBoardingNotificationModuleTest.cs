using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Module.Testing
{
	[TestedType(typeof(AsycudaPreBoardingNotificationModule))]
	sealed class AsycudaPreBoardingNotificationModuleTest : ZModuleBasherTest
	{
		public void TestFilterBusinessObject()
		{
			using (var module = new AsycudaPreBoardingNotificationModule())
			{
				AssertType<AsycudaPreBoardingNotificationFilterStripBusinessObject>(module.FilterBusinessObject);
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = new AsycudaPreBoardingNotificationModule())
			{
				AssertType<AsycudaPreBoardingNotificationCollection>(module.GridCollection);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.ASYCUDA.PreBoardingNotification;

		protected override bool HasController() => true;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var result = (AsycudaManifestHeader)base.GetNewBusinessObjectForHelperFilterTests(factory, businessObjectType);
			result.AMA_JobReference = ZString.Empty;
			return result;
		}
	}
}
