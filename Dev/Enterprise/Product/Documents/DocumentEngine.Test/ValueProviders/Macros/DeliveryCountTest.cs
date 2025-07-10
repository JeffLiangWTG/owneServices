using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(DeliveryCount))]
	sealed class DeliveryCountTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.SecondPass));
			Assert("should not match <DeliveryCount>", !ValueProviderToTest.IsResponsibleForReplacing("<DeliveryCount>", Passes.FirstPass));
			Assert("should match <DeliveryCount>", ValueProviderToTest.IsResponsibleForReplacing("<DeliveryCount>", Passes.SecondPass));
			Assert("should not match <   Print  \t       Count   >", !ValueProviderToTest.IsResponsibleForReplacing("<   Print  \t       Count   >", Passes.FirstPass));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReplacement()
		{
			PrepareRenderer();
			AssertEquals(1, ValueProviderToTest.GetReplacement("<     DeliveryCount      >", Report));

			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyEnterpriseBusinessObject>();
			dummy.GetLogs().AddNew(Events.DocumentDelivered, "Test Menu/Test Report");
			dummy.GetLogs().AddNew(Events.DocumentDelivered, "Test Menu/Test Report");

			var menuItem = factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Test Menu";
			var pack = new DocumentPack(menuItem);
			var mock = new BODocDataMock();
			mock.ParentBizoForTest = dummy;
			var newReport = new Report(pack, ExcelTemplate, mock, "Test Report", new RuntimeOptions.UserControlProviderList(), DocumentDirection.ANY, false);
			newReport.DocumentDeliveredEventCode = Events.DocumentDelivered.Code;
			pack.Add(newReport);

			AssertEquals(3, ValueProviderToTest.GetReplacement("<DeliveryCount>", newReport));
		}

		protected override ValueProvider GetNewValueProvider() => new DeliveryCount();

		protected override void PrepareDataForExamplesEvaluate() => PrepareRenderer();

		sealed class BODocDataMock : IBODocDataProvider
		{
			internal BusinessObject ParentBizoForTest { get; set; }

			DocWrapperCopyInfo IBODocDataProvider.AdditionalCopyInfo => null;

			BusinessObject IBODocDataProvider.BusinessObjectToLogAgainst => ParentBizoForTest;

			ZString IBODocDataProvider.GetDocDataValue(ZString docDataIdentifier, ZString formatStringForFallbackValue) => "";

			string[] IBODocDataProvider.ImageNamesToRemove => null;

			BusinessObject IBODocDataProvider.ParentBusinessObject => ParentBizoForTest;

			void IBODocDataProvider.SetDocWrapperContext(Dictionary<string, object> constants)
			{
			}

			string IBODocDataProvider.ToString() => "";

			IZType IBODocDataProvider.GetCustomField(string fieldName, string typeName) => ZString.Empty;

			string IBODocDataProvider.GetCustomFieldCodeDescription(string fieldName, string typeName) => string.Empty;

			ZDateTime IBODocDataProvider.GetEventLastDateTime(string eventCode) => ZDateTime.Empty;
		}
	}
}
