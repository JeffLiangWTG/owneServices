using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	public class ImportDeclarationMessageSendingActionParentForTest : ImportDeclarationMessageSendingActionParent
	{
		public ImportDeclarationMessageSendingActionParentForTest(JobDeclaration declaration) : base(declaration)
		{
		}

		public string WarningToAdd;

		public string MessageErrorToAdd;

		protected override ZString GetAdditionalWarningsCore() => WarningToAdd ?? base.GetAdditionalWarnings();

		protected override ZString GetBizObjValidationMessageErrors() => MessageErrorToAdd ?? base.GetBizObjValidationMessageErrors();
	}
}
