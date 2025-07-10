using System;
using System.Linq;
using CargoWise.IO;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	[TestedType(typeof(ContainerTypeValueObjectDataAdapter))]
	sealed class ContainerTypeXmlDataAdapterTest : ValueObjectDataAdapterTest<RefContainer, Xsd.ContainerType>
	{
		public void TestContainer_Export_ContainerMode()
		{
			RefContainer populatedContainer = Factory.New<RefContainer>();
			populatedContainer.RC_Code = "40GP";
			populatedContainer.RC_ISOType = "ISO";

			var cnCodeMap = populatedContainer.CodeMapCollection.AddNew();
			cnCodeMap.RCM_RC_Container = populatedContainer.PK;
			cnCodeMap.RCM_RN_NKCountry = "CN";
			cnCodeMap.RCM_Code = "CN1";

			var usCodeMap = populatedContainer.CodeMapCollection.AddNew();
			usCodeMap.RCM_RC_Container = populatedContainer.PK;
			usCodeMap.RCM_RN_NKCountry = "US";
			usCodeMap.RCM_Code = "US1";

			NotificationBuffer notify = new NotificationBuffer();

			Xsd.ContainerType type = new ContainerTypeValueObjectDataAdapter().ExportToValueObject(populatedContainer, new ValueObjectExportContext(notify));

			AssertEquals("Container Mode", "40GP", type.ContainerCode);
			AssertEquals("US Container Code", "US1", type.USContainerCode);
		}

		public void TestContainer_Import_ContainerMode()
		{
			Xsd.ContainerType type = new Xsd.ContainerType();
			type.ISOCode = "ISO";
			type.ContainerCode = "40GP";
			type.USContainerCode = "US1";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			RefContainer container = Factory.New<RefContainer>();
			new ContainerTypeValueObjectDataAdapter().ImportFromValueObject(container, type, context);

			AssertEquals("Container Mode should be same as ISO Code when Import", "ISO", container.RC_Code);
			AssertEquals("Should have one code map", 1, container.CodeMapCollection.Count);
			AssertEquals("RefContainerCodeMap value", "US", ((RefContainerCodeMap)container.CodeMapCollection.FirstOrDefault()).RCM_RN_NKCountry);
			AssertEquals("RefContainerCodeMap value", "US1", ((RefContainerCodeMap)container.CodeMapCollection.FirstOrDefault()).RCM_Code);
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		protected override ValueObjectDataAdapter<RefContainer, Xsd.ContainerType> GetNewBizObjXmlDataAdapter() => new ContainerTypeValueObjectDataAdapter();

		protected override string ExpectedRootCollectionElementName => "ContainerTypes";

		protected override string ExpectedRootElementName => "ContainerType";

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var containerType = Factory.New<RefContainer>();
			containerType.RC_ISOType = "xxx";
			return new BusinessObjectAndExpectedOutputFileName(containerType, null, ValidationKind.None, "Empty container type");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample() => GetFullyPopulatedBizObjSample();

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var populatedContainer = Factory.New<RefContainer>();
			populatedContainer.RC_Code = "40GP";
			populatedContainer.RC_ISOType = "40GP";
			populatedContainer.RC_Length = 1;
			populatedContainer.RC_Width = 2;
			populatedContainer.RC_Height = 3;
			populatedContainer.RC_USContainerCode = "U1";
			populatedContainer.SetCountrySpecificContainerCode("U2", Core.Constants.CountryCodes.UnitedStates);
			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.DataTransfer.Test.DataAdapters.Shared.Testing.PopulatedRefContainer.xml");
			return new BusinessObjectAndExpectedOutputFileName(populatedContainer, expectedOutputFilename, ValidationKind.Xsd, "Populated container type");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects() => Array.Empty<BusinessObjectAndExpectedOutputFileName>();
	}
}
