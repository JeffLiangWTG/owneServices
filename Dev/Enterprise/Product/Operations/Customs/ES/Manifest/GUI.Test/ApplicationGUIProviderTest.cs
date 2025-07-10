using System;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.ES.Manifest.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.GUI.Testing
{
	[TestedType(typeof(ApplicationGUIProvider))]
	sealed class ApplicationGUIProviderTest : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<ApplicationGUIProvider, AsycudaManifestHeader>
	{
		public void TestGetPackedItemColumns()
		{
			var header = CreateNewManifest();
			var applicationGUIProvider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			var packItems = applicationGUIProvider.GetPackedItemColumns();
			AssertEquals(10, packItems.Length);
		}

		public void TestGetPacksGridMandatoryOrderedColumns()
		{
			var header = CreateNewManifest();
			var applicationGUIProvider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			var packsGrid = applicationGUIProvider.GetPacksGridMandatoryColumns();
			AssertEquals(13, packsGrid.Length);
		}

		protected override void AssertGetPacksGridColumnsOrder(string[] columnsOrder)
		{
			AssertEquals(13, columnsOrder.Length);
		}

		protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[] { typeof(AsycudaPackUserControl), typeof(SupportingDocumentsUserControl) };

		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

		protected override AsycudaManifestHeader CreateNewManifest()
		{
			var header = base.CreateNewManifest();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Spain;
			header.AMA_ManifestType = ESManifestTypes.Codes.ICS;
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;
			return header;
		}
	}
}
