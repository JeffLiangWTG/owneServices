using System;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class CDSEntryLocalReferenceNumberGeneratorTargetTests : NumberGeneratorTargetTest
	{
		public void TestOverrides()
		{
			var customisation = new CDSEntryLocalReferenceNumberCustomisation();
			GBCustomsDataRegistry.Instance.CdsEntryLocalReferenceNumberCustomisationFromCW1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);

			var target = new CDSEntryLocalReferenceNumberGeneratorTarget { Context = new NumberGeneratorContext() };
			AssertCustomisation("Should find the customisation", "", target.NumberCustomisation);
			AssertLocation(GBCustomsDataRegistry.Instance.CdsEntryLocalReferenceNumberCustomisationFromCW1, target.NumberCustomisationLocation);
			AssertEquals(CusEntryHeader.Schema.LRNMaxLength, target.MaxLength);
			AssertEquals("CDS Local Reference Number", target.Name);
		}
	}
}
