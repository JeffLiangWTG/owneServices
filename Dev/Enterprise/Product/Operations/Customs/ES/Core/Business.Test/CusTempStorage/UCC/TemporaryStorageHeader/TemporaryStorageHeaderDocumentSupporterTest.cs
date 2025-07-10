using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

[TestedType(typeof(TemporaryStorageHeaderDocumentSupporter))]
public class TemporaryStorageHeaderDocumentSupporterTest : DocumentSupporterTest
{
	public void TestBusinessContext()
	{
		AssertEquals("BusinessContext", BusinessContext.EuPnts, DocumentSupporter.BusinessContext);
	}

	public void TestCustomisationSecurityCheckpoint()
	{
		AssertEquals("CustomisationSecurityCheckpoint", Env.Security.None, DocumentSupporter.CustomisationSecurityCheckpoint);
	}

	public void TestDefaultDataContext()
	{
		AssertEquals("DefaultDataContext", DataContext.EuPnts, DocumentSupporter.DefaultDataContext);
	}

	public void TestGetDocumentWrappers()
	{
		var wrappers = DocumentSupporter.GetDocumentWrappers(DataContext.EuPnts, null);
		CombineAssertions(() =>
		{
			AssertEquals("Wrappers count", 1, wrappers.Length);
			AssertEquals("Wrapper type", ESConstants.DocumentWrapperConstants.TemporaryStorageHeaderDocumentWrapperType, wrappers[0].GetType().FullName);
		});
	}

	public void TestGetFilterValue()
	{
		AssertEquals("Filter CTY=ES", Core.Constants.CountryCodes.Spain, DocumentSupporter.GetFilterValue(DocumentFilters.CTY));
	}

	protected override IDocumentSupportable GetDocumentSupportableBusinessObject() => Header;

	TemporaryStorageHeader Header => header ??= Factory.New<TemporaryStorageHeader>();
	TemporaryStorageHeader header;

	DocumentSupporter DocumentSupporter => documentSupporter ??= ((IDocumentSupportable)Header).DocumentSupporter;
	DocumentSupporter documentSupporter;
}
