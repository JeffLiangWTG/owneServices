using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	sealed class ApplicationGUIProviderBaseOnlyTest : TestCaseWithFactory
	{
		public void TestGetApplicationGuiProvider()
		{
			var enableIcsManifestRegItem = (BooleanRegistryItem)ObjectFactory.Get<Integration.Customs.GB.IGBCustomsDataRegistry>().EnableIcsManifest;
			using (enableIcsManifestRegItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
				helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				Factory.Save();

				CombineAssertions(() =>
				{
					AssertSubscriberMapping(Core.Constants.CountryCodes.Eritrea, ZString.Empty, "NVC", "ASYCUDA manifest", "ASYCUDAManifest.GUI", "ASYCUDAManifest.GUI");
					AssertSubscriberMapping(Core.Constants.CountryCodes.Singapore, "MGE", "NVC", "SG Access", "SG.Access.GUI", "SG.Access.GUI");
					AssertSubscriberMapping(Core.Constants.CountryCodes.Singapore, "MGI", "NVC", "SG Access", "SG.Access.GUI", "SG.Access.GUI");
					AssertSubscriberMapping(Core.Constants.CountryCodes.SouthAfrica, "HAB", "NVC", "ZA Manifest", "ZA.Manifest.GUI", "ZA.Manifest.GUI");
					AssertSubscriberMapping(Core.Constants.CountryCodes.UnitedStates, "IAM", "NVC", "US ACE", "ACEManifest.GUI", "ACEManifest.GUI");
					AssertSubscriberMapping(Core.Constants.CountryCodes.UnitedKingdom, "ICS", "VOC", "GB ICS", "GB.ICS.GUI", "GB.ICS.GUI");
				});

				var trProviders = ApplicationBusinessProvider.GetApplicationBusinessProviders(Factory, Core.Constants.CountryCodes.Turkey).ToArray().Select(x => x.GetType().ToString());
				AssertEquals(2, trProviders.Count());
				Assert(trProviders.Any(x => x == "Enterprise.Customs.TR.Manifest.Business.ApplicationBusinessProvider"));
				Assert(trProviders.Any(x => x == "Enterprise.Customs.TR.ETrade.Business.ApplicationBusinessProvider"));
			}
		}

		void AssertSubscriberMapping(string country, string manifestType, string applicationCode, string applicationName, string nameSpace, string assembly)
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, country, manifestType, applicationCode);
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			AssertNotNull($"{applicationName} provider must exist", provider);
			AssertContains($"{applicationName} provider must be in {nameSpace} namespace", nameSpace, provider.GetType().FullName);
			AssertContains($"{applicationName} provider must be in {assembly} assembly", assembly, provider.GetType().AssemblyQualifiedName);
		}

		public void TestSupportsAsycudaPacksShowAsycudaPackUserControl()
		{
			CombineAssertions(() =>
			{
				foreach (var businessProvider in ApplicationBusinessProvider.GetAllApplicationBusinessProviders(Factory))
				{
					var featureProvider = businessProvider.FeatureProvider;
					var guiProvider = ApplicationGUIProvider.GetApplicationGuiProvider(Factory, businessProvider);
					if (guiProvider != null && featureProvider != null)
					{
						var businessProviderType = businessProvider.GetType();
						AssertEquals($"{businessProviderType} | {guiProvider.GetType()}", true, !featureProvider.SupportsAsycudaPacks
							|| providersUsesAsycudaPacksWithoutTheBaseUI.Contains(businessProviderType.ToString()) || guiProvider.GetBillAdditionalTabPageUserControl().Any(x =>
							{
								using (x)
								{
									return x is AsycudaPackUserControl;
								}
							}));
					}
				}
			});
		}

		public void TestIsMessageGridUserFullNameVisible()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Eritrea, ZString.Empty, "NVC");
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			AssertEquals(false, provider.IsMessageGridUserFullNameVisible());
		}

		public void TestIsInEnforceOnlyValidTransportMode()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Eritrea, ZString.Empty, "NVC");
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			AssertEquals(false, provider.IsInEnforceOnlyValidTransportMode());
		}

		public void TestIsInEnforceOnlyValidSpecificCircumstanceIndicator()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Eritrea, ZString.Empty, "NVC");
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			AssertEquals(false, provider.IsInEnforceOnlyValidSpecificCircumstanceIndicator());
		}

		public void TestCheckPacksGridColumnAvailability()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Eritrea, ZString.Empty, "NVC");
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			AssertEquals("ShouldCheckPacksGridColumnAvailability is false by default", false, provider.ShouldCheckPacksGridColumnAvailability());
		}

		readonly List<string> providersUsesAsycudaPacksWithoutTheBaseUI = new List<string>
		{
			"Enterprise.Customs.TW.Manifest.Business.ApplicationBusinessProvider",
			"Enterprise.Customs.TW.BriefCustomsDeclaration.Business.ApplicationBusinessProvider",
			"Enterprise.Customs.IL.Manifest.Business.ApplicationBusinessProvider",
			"Enterprise.Customs.TW.BriefCustomsDeclaration.Business.ApplicationBusinessProvider",
			"Enterprise.Customs.EU.H7.Business.H7ApplicationBusinessProvider",
			"Enterprise.Customs.IE.H7.Business.H7ApplicationBusinessProvider",
			"Enterprise.Customs.GB.H7.Business.ApplicationBusinessProvider",
			"Enterprise.Customs.FR.H7.Business.H7ApplicationBusinessProvider",
			"Enterprise.Customs.US.ACEManifest.Business.ApplicationBusinessProvider",
			"Enterprise.Customs.JP.Manifest.Business.ApplicationBusinessProvider",
			"Enterprise.Customs.ES.Manifest.H7.Business.H7ApplicationBusinessProvider",
			"Enterprise.Customs.IT.H7.Business.H7ApplicationBusinessProvider",
		};
	}
}
