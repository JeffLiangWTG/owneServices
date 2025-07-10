using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Application;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	public class ApplicationGUIProviderForTesting : ApplicationGUIProvider, IDisposable
	{
		public static ApplicationGUIProviderForTesting GetTestProvider(AsycudaManifestHeader header)
		{
			var factory = header.Factory;
			var applicationGUIProvider = new ApplicationGUIProviderForTesting(header);
			var applicationProviderKey = header.GetApplicationProviderKey();
			var cacheKey = string.Format(CultureInfo.InvariantCulture, "ApplicationGuiProvider_{0}_{1}_{2}", applicationProviderKey.CountryOrGrouping, applicationProviderKey.ManfestTypeCode, applicationProviderKey.ApplicationCode); // CachedValueKey

			var globalManifestApplicationBusinessProvider = new List<ApplicationGUIProvider>
			{
				applicationGUIProvider
			};

			factory.ClearCachedValue<ApplicationGUIProvider>(cacheKey);
			factory.ClearCachedValue<ApplicationGUIProvider[]>("GlobalManifestApplicationGuiProviders");
			applicationGUIProvider.globalManifestApplicationGuiProviderSubstitute = ObjectFactory.Substitute("GlobalManifestApplicationGuiProvider", globalManifestApplicationBusinessProvider);
			applicationGUIProvider.cacheKey = cacheKey;

			return applicationGUIProvider;
		}

		public ApplicationGUIProviderForTesting(AsycudaManifestHeader header)
			: base()
		{
			this.header = header;
		}

		readonly AsycudaManifestHeader header;

		public Func<ZForm, MenuBuilder> GetMenuBuilderForTesting;

		public override MenuBuilder GetMenuBuilder(ZForm mainForm, AsycudaManifestHeader header) => GetMenuBuilderForTesting != null ? GetMenuBuilderForTesting(mainForm) : null;

		public Func<ContainerCountrySpecificUserControl> GetContainerCountrySpecificUserControlForTesting;
		protected override ContainerCountrySpecificUserControl GetContainerCountrySpecificUserControlCore() => GetContainerCountrySpecificUserControlForTesting != null ? GetContainerCountrySpecificUserControlForTesting() : null;

		public Func<IReadOnlyDictionary<bool, string[]>> GetContainersGridColumnAvailabilityForTesting;
		protected override IReadOnlyDictionary<bool, string[]> GetContainersGridColumnAvailabilityCore() => GetContainersGridColumnAvailabilityForTesting != null ? GetContainersGridColumnAvailabilityForTesting() : null;

		public Func<IEnumerable<string>> GetContainersGridColumnsOrderForTesting;
		protected override IEnumerable<string> GetContainersGridColumnsOrderCore() => GetContainersGridColumnsOrderForTesting != null ? GetContainersGridColumnsOrderForTesting() : null;

		public Func<IEnumerable<ZGridColumnInfo>> GetContainersGridExtraColumnInfosForTesting;
		protected override IEnumerable<ZGridColumnInfo> GetContainersGridExtraColumnInfosCore(AsycudaManifestHeader header) => GetContainersGridExtraColumnInfosForTesting != null ? GetContainersGridExtraColumnInfosForTesting() : null;

		public Func<IReadOnlyDictionary<bool, string[]>> GetArrivalHeadersGridColumnAvailabilityForTesting;
		protected override IReadOnlyDictionary<bool, string[]> GetArrivalHeadersGridColumnAvailabilityCore() => GetArrivalHeadersGridColumnAvailabilityForTesting != null ? GetArrivalHeadersGridColumnAvailabilityForTesting() : null;

		public Func<IReadOnlyDictionary<string, int>> GetContainersGridColumnsWidthForTesting;
		protected override IReadOnlyDictionary<string, int> GetContainersGridColumnsWidthCore() => GetContainersGridColumnsWidthForTesting != null ? GetContainersGridColumnsWidthForTesting() : null;

		public Func<AsycudaBill, string> GetBillFormCaptionForTesting;
		protected override string GetBillFormCaptionCore(AsycudaBill bill) => GetBillFormCaptionForTesting != null ? GetBillFormCaptionForTesting(bill) : base.GetBillFormCaptionCore(bill);

		protected override bool IsInEnforceOnlyValidTransportModeCore => true;

		protected override bool IsInEnforceOnlyValidSpecificCircumstanceIndicatorCore => true;

		public Func<IAdditionalTabPage> GetBillMessagesUserControlForTesting;

		protected override IAdditionalTabPage GetBillMessagesUserControlCore() => GetBillMessagesUserControlForTesting != null ? GetBillMessagesUserControlForTesting() : base.GetBillMessagesUserControlCore();

		public bool ShouldPositionMessagesTabAccordingToMessageLevelForTesting;

		public override bool ShouldPositionMessagesTabAccordingToMessageLevel => ShouldPositionMessagesTabAccordingToMessageLevelForTesting;

		protected override IEnumerable<ZGridColumnInfo> GetMessagesGridExtraColumnInfosCore()
		{
			var testTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			testTextBoxColumnStyleInfo.ColumnName = "TestColumn";
			yield return testTextBoxColumnStyleInfo;
		}

		string cacheKey;
		IDisposable globalManifestApplicationGuiProviderSubstitute;

		public void Dispose()
		{
			globalManifestApplicationGuiProviderSubstitute?.Dispose();
			if (cacheKey != null)
			{
				header.Factory.ClearCachedValue<ApplicationGUIProvider>(cacheKey);
			}
		}
	}
}
