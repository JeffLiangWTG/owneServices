using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public class AEODocumentPreSaveDialogStrategy : PreSaveDialogStrategy
	{
		public AEODocumentPreSaveDialogStrategy(JobDeclaration declaration)
		{
			Declaration = Argument.NotNull(declaration, nameof(declaration));
		}
		JobDeclaration Declaration { get; }

		protected override ContinueWithSave RunPreSaveAction()
		{
			var docManager = new AEODocumentManager(Declaration, new AEODocumentMessageBoxProvider());
			docManager.AddUpdateOrRemoveAEODocumentInEntryInstructions();
			return ContinueWithSave.Yes;
		}

		protected override bool ShouldRunPreSaveAction() => true;
	}
}
