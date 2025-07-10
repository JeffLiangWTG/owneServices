using System.Collections.Generic;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE429;
using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument.Testing;

[TestedType(typeof(IDDDutiesAndTaxWrapperCollection<IDD429DutiesAndTaxWrapper>))]
sealed class IDD429DutiesAndTaxWrapperCollectionTest : DocBaseWrapperCollectionTest<IDDDutiesAndTaxWrapperCollection<IDD429DutiesAndTaxWrapper>>
{
	protected override IDDDutiesAndTaxWrapperCollection<IDD429DutiesAndTaxWrapper> GetNewDocumentWrapperCollection()
	{
		return new IDDDutiesAndTaxWrapperCollection<IDD429DutiesAndTaxWrapper>(Factory);
	}

	protected override object GetNewObjectToWrap() => null;

	protected override DocumentEngineCore.DocWrappers.DocumentWrapper AddNewDocumentWrapperToCollection(DocumentEngineCore.DocWrappers.DocumentWrapperCollection collection)
	{
		var dutiesAndTaxes = new DutiesAndTaxesType();
		var dutiesAndTaxesSummaries = new List<DutiesAndTaxesSummariesType>();
		var result = IDD429DutiesAndTaxWrapper.New(dutiesAndTaxes, dutiesAndTaxesSummaries, Factory);
		collection.Add(result);
		return result;
	}
}
