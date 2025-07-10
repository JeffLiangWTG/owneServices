using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE429;
using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument.Testing;

[TestedType(typeof(IDDImputationSheet429WrapperCollection))]
sealed class IDDImputationSheet429WrapperCollectionTest : DocBaseWrapperCollectionTest<IDDImputationSheet429WrapperCollection>
{
	protected override IDDImputationSheet429WrapperCollection GetNewDocumentWrapperCollection()
	{
		return new IDDImputationSheet429WrapperCollection(Factory);
	}

	protected override object GetNewObjectToWrap() => null;

	protected override DocumentEngineCore.DocWrappers.DocumentWrapper AddNewDocumentWrapperToCollection(DocumentEngineCore.DocWrappers.DocumentWrapperCollection collection)
	{
		var supportingDocument = new MSupportingDocumentType01FR { };
		var result =  IDDImputationSheet429Wrapper.New(supportingDocument, "12", Factory);
		collection.Add(result);
		return result;
	}
}
