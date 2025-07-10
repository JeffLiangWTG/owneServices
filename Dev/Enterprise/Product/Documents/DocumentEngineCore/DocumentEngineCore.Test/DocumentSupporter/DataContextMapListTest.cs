using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngineCore.DocumentSupport.Testing
{
	sealed class DataContextMapListTest : TestCaseWithFactory
	{
		public void TestGetTypesAvailableForMapping()
		{
			var dummyBO = Factory.New<DummyEnterpriseBusinessObject>();
			var documentSupporter = new DocumentSupporterForBOTestingWithGenericWrapperTypes(dummyBO);
			var typesAvailable = new DataContextMapList(documentSupporter);
			ZStringBuilder typeNames = new ZStringBuilder();
			foreach (var mapType in typesAvailable)
			{
				typeNames.Append(string.Format("{0} ({1})", mapType.DataContextIdentifier, mapType.TopLevelDataSourceType.Name));
			}

			AssertMultilineASCIIEquals("documentSupporter.GetTypesAvailableForMapping()", @"
GenericCommercialInvoice (CommercialInvoiceWrapper)
GenericFreightJob (FreightWrapper)
.DummyChildBusinessObject (DummyChildBusinessObject)
.DummyEnterpriseBusinessObject (DummyEnterpriseBusinessObject)
".Trim(), typeNames.ToStringWithNewLineBetweenAppends());
		}

		#region Implementation

		class DocumentSupporterForBOTestingWithGenericWrapperTypes : DocumentSupporterWithBOOverridesTest.DocumentSupporterForBOTesting
		{
			public DocumentSupporterForBOTestingWithGenericWrapperTypes(DummyEnterpriseBusinessObject parentBusinessObject)
				: base(parentBusinessObject)
			{
			}

			protected override DataContext[] GetSupportedDataContexts()
			{
				List<DataContext> result = new List<DataContext>(base.GetSupportedDataContexts());
				result.Add(DataContext.GenericFreightJob);
				result.Add(DataContext.GenericCommercialInvoice);
				return result.ToArray();
			}
		}

		#endregion
	}
}
