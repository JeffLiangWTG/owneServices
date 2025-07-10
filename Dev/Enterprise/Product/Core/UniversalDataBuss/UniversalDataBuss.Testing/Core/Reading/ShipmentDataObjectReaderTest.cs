using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.UniversalDataBuss.Management.ShipmentProcessing.Testing
{
	class ShipmentDataObjectReaderTest : TestCaseWithFactory
	{
		public void TestModuleHasReferenceAndPartyIDMatchingImplemented()
		{
			var manager = new DummyShipmentDataObjectReader(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance), new DummyLogger(), new UniversalObjectFactory());

			AssertEquals("ModuleHasReferenceAndPartyIDMatchingImplemented should be false when GetReferenceAndPartyIDMatcher method is not overridden."
				, false
				, manager.ModuleHasReferenceAndPartyIDMatchingImplemented);
		}

		class DummyShipmentDataObjectReader : ShipmentDataObjectReader<DummyBusinessObject>
		{
			public DummyShipmentDataObjectReader(UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
				: base(dataObject, logger, factory)
			{
			}

			protected override DummyBusinessObject GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
			{
				throw new NotImplementedException();
			}

			protected override IMatchingBusinessEntityFinder<DummyBusinessObject> GetCombinedReferenceMatcher()
			{
				throw new NotImplementedException();
			}

			public override DataContextType DataContextType
			{
				get { throw new NotImplementedException(); }
			}

			protected override void PopulateBusinessObject(DummyBusinessObject targetBO)
			{
				throw new NotImplementedException();
			}

			protected override IEnumerable<(string KeyValue, string KeySource)> ReadKeysForParallelismCore()
			{
				throw new NotImplementedException();
			}
		}
	}
}
