using System.Collections.Generic;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE426;
using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument.Testing;

[TestedType(typeof(IDDDutiesAndTaxWrapperCollection<IDD426DutiesAndTaxWrapper>))]
sealed class IDD426DutiesAndTaxWrapperCollectionTest : DocBaseWrapperCollectionTest<IDDDutiesAndTaxWrapperCollection<IDD426DutiesAndTaxWrapper>>
{
	protected override IDDDutiesAndTaxWrapperCollection<IDD426DutiesAndTaxWrapper> GetNewDocumentWrapperCollection()
	{
		return new IDDDutiesAndTaxWrapperCollection<IDD426DutiesAndTaxWrapper>(Factory);
	}

	protected override object GetNewObjectToWrap() => null;

	protected override DocumentEngineCore.DocWrappers.DocumentWrapper AddNewDocumentWrapperToCollection(DocumentEngineCore.DocWrappers.DocumentWrapperCollection collection)
	{
		var dutiesAndTaxes = new DutiesAndTaxesType();
		var dutiesAndTaxesSummaries = new List<DutiesAndTaxesSummariesType>();
		var result = IDD426DutiesAndTaxWrapper.New(dutiesAndTaxes, dutiesAndTaxesSummaries, Factory);
		collection.Add(result);
		return result;
	}
}
