using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.PBN.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.IE.PBN.Module.Testing
{
	[TestedType(typeof(PBNModule))]
	sealed class PBNModuleTest : ZModuleBasherTest
	{
		public void TestFilterBusinessObject()
		{
			using (var module = new PBNModuleForTest())
			{
				AssertType<PBNFilterStripBusinessObject>(module.FilterBusinessObject);
			}
		}

		public void TestGetNewFilterControl()
		{
			using (var module = new PBNModuleForTest())
			using (var filterControl = module.GetNewFilterControlExposedForTest())
			{
				AssertType<PBNFilterStripControl>(filterControl);
			}
		}

		protected override string CountryCode => Constants.CountryCodes.Ireland;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.IE.PreBoardingNotification;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var result = (AsycudaManifestHeader)base.GetNewBusinessObjectForHelperFilterTests(factory, businessObjectType);
			result.AMA_JobReference = ZString.Empty;
			result.AMA_ManifestType = PBNManifestTypes.Codes.PBN;
			result.AMA_RN_NKCountry = Constants.CountryCodes.Ireland;
			return result;
		}

		class PBNModuleForTest : PBNModule
		{
			public PBNModuleForTest()
			{ }

			public IFilterControl GetNewFilterControlExposedForTest()
			{
				return GetNewFilterControl();
			}

			public FilterBusinessObject GetNewFilterBusinessObjectExposedForTest()
			{
				return GetNewFilterBusinessObject();
			}
		}
	}
}
