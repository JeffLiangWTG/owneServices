using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class MismatchedParentTest : TestCaseWithFactory
	{
		public void TestFailureDoesNotReportDeveloperException()
		{
			TestUtil.AddDummyBizoToGlobalDefinitions();

			var dummy = Factory.New<DummyWithDependentsBusinessObject>();
			dummy.Z0_Code = "XXX";
			var dummyChild = dummy.Dependents.AddNew();
			dummyChild.ZD1_Code = "YYY";

			Factory.Save();

			string xmlWithSubversiveChild = string.Format(XML_TryingToSubvertTheNewlyCreatedChild, dummyChild.PK.ToString());

			var message = Factory.New<IEDIMessage>();
			message.EM_MessageText = xmlWithSubversiveChild;

			var logger = new TestErrorLogger();
			var processor = new NativeDataMessageProcessor(logger);

			var result = processor.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("processor.Process(message)", MessageStatus.Rejected, result);

				string expectedLog = string.Format(@"
Error - Database parent does not match entity parent.
PK '{0}' already exists on a row in the database, and is linked to a different parent row.
Entity: DummyDependentBizo (DummyDependentBizo):[Property [PK, {0}]Property [Code, YYY]]
Parent: DummyBizo (DummyBizo):[Property [Code, AAA]]
DB Parent Key: {1}
				".Trim(), dummyChild.PK.ToString(), dummy.PK.ToString());

				AssertMultilineASCIIEquals("Log should contain human readable description of problem with no call stack", expectedLog, logger.Logs);
			});
		}

		#region XML_TryingToSubvertTheNewlyCreatedChild
		const string XML_TryingToSubvertTheNewlyCreatedChild = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Dummy>
			<DummyBizo Action=""INSERT"">
        <Code>AAA</Code>
        <DummyDependentBizoCollection>
          <DummyDependentBizo Action=""MERGE"">
						<PK>{0}</PK>
            <Code>YYY</Code>
          </DummyDependentBizo>
        </DummyDependentBizoCollection>
      </DummyBizo>
    </Dummy>
  </Body>
</Native>
";
		#endregion
	}
}

