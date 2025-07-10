using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public class Document9015PreSaveDialogStrategy : PreSaveDialogStrategy
	{
		public Document9015PreSaveDialogStrategy(JobDeclaration declaration)
		{
			Declaration = Argument.NotNull(declaration, nameof(declaration));
		}
		JobDeclaration Declaration { get; }

		protected override ContinueWithSave RunPreSaveAction()
		{
			if (Declaration.IsImport)
			{
				var docManager = new DocumentManager9015(Declaration, new Document9015MessageBoxProvider());
				docManager.Remove9015DocumentsIfNeeded();
			}
			return ContinueWithSave.Yes;
		}

		protected override bool ShouldRunPreSaveAction() => true;
	}
}
