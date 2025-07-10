namespace Enterprise.Customs.IE.ExitControl.Business.AES.Testing
{
	class AdditionalInfosProviderTest : Customs.Business.Testing.DataProviderTestCase<AdditionalInfosProvider, CargoWise.Customs.IE.MessageContracts.Interfaces.IDocument>
	{
		public void TestType()
		{
			AssertEquals("Type", "N750", IProvider.Type);
		}

		public void TestReference()
		{
			AssertEquals("Reference", "TRANSDOC123", IProvider.Reference);
		}

		protected override AdditionalInfosProvider GetProvider() => new AdditionalInfosProvider(CreateData());

		AdditionalInfo CreateData()
		{
			var addinfo = Factory.New<AdditionalInfo>();
			addinfo.CSI_Type = "TRA";
			addinfo.CSI_Code = "N750";
			addinfo.CSI_ReferenceNumber = "TRANSDOC123";
			return addinfo;
		}
	}
}
