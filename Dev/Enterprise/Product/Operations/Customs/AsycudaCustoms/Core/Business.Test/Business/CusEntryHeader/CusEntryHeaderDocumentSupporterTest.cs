using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderDocumentSupporter))]
	class CusEntryHeaderDocumentSupporterTest : Customs.Business.Testing.CusEntryHeaderDocumentSupportTest
	{
		public void TestGetBODocDataProviders()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.ActiveEntryHeaders.AddNew();
			var providers = header.DocumentSupporter.GetBODocDataProviders(new DataContextValue(SummaryCusDecDocument), null);
			AssertEquals("Provider for SummaryCusDecDocument", 1, providers.Length);
		}

		public override void TestGetBODocDataProvidersNotFoundMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "BGM";
			AssertEquals("Entry Header BGM cannot be found.", entryHeader.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue(SummaryCusDecDocument), null));
		}

		protected override ZString GetMainNameSpace() => "Enterprise.Customs.AsycudaCustoms.Business.";

		internal const string SummaryCusDecDocument = ".SummaryCusDecDocument";
	}
}
