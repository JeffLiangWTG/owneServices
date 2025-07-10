using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.MessageBuilders;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.GB.Business
{
	public class GBJobDeclarationMessageManager : JobDeclarationMessageManager
	{
		public GBJobDeclarationMessageManager(JobDeclaration declaration, IMessageGenerator<EU.Business.Declaration.CusEntryHeader> generator)
			: base(declaration, generator)
		{
			this.declaration = (Declaration.JobDeclaration)declaration;
		}

		protected override bool CheckDeniedParty(BusinessObject master)
		{
			return true;
		}

		protected override void SaveFactoryAfterSendingMessages(ISendsMessagesToCustoms sender, CancellationToken token)
		{
			base.SaveFactoryAfterSendingMessages(sender, token);

			if (!declaration.IsUCCCompliant)
			{
				if (declaration.IsImport)
				{
					CountOfImportDeclarationsThisSession++;
				}

				if (declaration.IsExport)
				{
					CountOfExportDeclarationsThisSession++;
				}
			}
		}

		protected Declaration.JobDeclaration declaration;

		[ThreadSafe]
		public static long CountOfImportDeclarationsThisSession = 0;

		[ThreadSafe]
		public static long CountOfExportDeclarationsThisSession = 0;
	}
}
