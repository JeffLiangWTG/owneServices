using System.Collections.Generic;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE428;
using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument.Testing;

[TestedType(typeof(IDDDutiesAndTaxWrapperCollection<IDD428DutiesAndTaxWrapper>))]
sealed class IDD428DutiesAndTaxWrapperCollectionTest : DocBaseWrapperCollectionTest<IDDDutiesAndTaxWrapperCollection<IDD428DutiesAndTaxWrapper>>
{
	protected override IDDDutiesAndTaxWrapperCollection<IDD428DutiesAndTaxWrapper> GetNewDocumentWrapperCollection()
	{
		return new IDDDutiesAndTaxWrapperCollection<IDD428DutiesAndTaxWrapper>(Factory);
	}

	protected override object GetNewObjectToWrap() => null;

	protected override DocumentEngineCore.DocWrappers.DocumentWrapper AddNewDocumentWrapperToCollection(DocumentEngineCore.DocWrappers.DocumentWrapperCollection collection)
	{
		var dutiesAndTaxes = new DutiesAndTaxesType();
		var dutiesAndTaxesSummaries = new List<DutiesAndTaxesSummariesType>();
		var result = IDD428DutiesAndTaxWrapper.New(dutiesAndTaxes, dutiesAndTaxesSummaries, Factory);
		collection.Add(result);
		return result;
	}
}
