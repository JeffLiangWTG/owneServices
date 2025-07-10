using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business;

public class PreviousDocumentProcedureValidator
{
	public PreviousDocumentProcedureValidator(PreviousDocument previousDocument)
	{
		this.previousDocument = Argument.NotNull(previousDocument, nameof(previousDocument));
		parent = (EU.Business.Declaration.MultiLineAddInfos.IPreviousDocumentsProvider)this.previousDocument.Parent;
	}

	readonly EU.Business.Declaration.MultiLineAddInfos.IPreviousDocumentsProvider parent;
	readonly PreviousDocument previousDocument;

	public void CheckCSI_Procedure()
	{
		if (parent != null)
		{
			var moreThanOnePaDoc = parent.PreviousDocuments.Cast<PreviousDocument>().Where(x => x.IsSummaryDeclarationDocument).Skip(1).Any();
			var anyRpDoc = parent.PreviousDocuments.Cast<PreviousDocument>().Any(x => x.IsPreviousProcedureDocument);

			if (moreThanOnePaDoc && anyRpDoc)
			{
				previousDocument.CSI_ProcedureInfo.AddWarning(ValidationCaptions.PreviousDocument.MoreThanOnePaAndOneOrMoreRpDocuments);
			}
		}
	}
}
