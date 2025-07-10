using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business
{
	public class DeclarationDmExtPreviousDocumentWrapper : IDeclarationDmExtPreviousDocument
	{
		DeclarationDmExtPreviousDocumentWrapper(CusSupportingInfo previousDocument)
		{
			this.previousDocument = previousDocument;
		}
		readonly CusSupportingInfo previousDocument;

		public static DeclarationDmExtPreviousDocumentWrapper NewOrNull(CusSupportingInfo previousDocument) => previousDocument == null ? null : new DeclarationDmExtPreviousDocumentWrapper(previousDocument);

		public IIDType ID => IDTypeWrapper.NewOrNull(previousDocument.CSI_ReferenceNumber);

		public ICodeType TypeCode => CodeTypeWrapper.NewOrNull(previousDocument.CSI_Code);
	}
}
