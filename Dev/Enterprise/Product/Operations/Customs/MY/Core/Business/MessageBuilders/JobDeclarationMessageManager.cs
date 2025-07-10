using System.Collections.Specialized;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.MY.Business
{
	public class JobDeclarationMessageManager : Customs.Business.MessageManager
	{
		public JobDeclarationMessageManager(JobDeclaration declaration)
			: base(false)
		{
			this.declaration = declaration;
		}

		#region Implementation

		protected override BusinessObject Master
		{
			get { return declaration; }
		}
		readonly JobDeclaration declaration;

		protected override SendAmendmentIfNeededDelegate[] SendAmendmentIfNeededDelegates
		{
			get { return null; }
		}

		public void DeclareDeclaration(Customs.Business.ISendsMessagesToCustoms sender)
		{
			var replaceThisTokenEventuallyQuestionMarkExclamationMark = CancellationToken.None;
			SendMessage(sender,
				GetAnyReasonsWeCantDeclareTheDeclaration(),
				GetWarningsAboutDeclaringTheDeclaration(),
				new MessageBuilderDelegate[] { new MessageBuilderDelegate(GetDeclarationMessageBuilder) }, "Declaration",
				replaceThisTokenEventuallyQuestionMarkExclamationMark);
		}

		IMessageBuilder GetDeclarationMessageBuilder(BusinessObject master)
		{
			return new DeclarationMessageBuilder(master as JobDeclaration);
		}

		StringCollection GetAnyReasonsWeCantDeclareTheDeclaration()
		{
			return new StringCollection();
		}

		StringCollection GetWarningsAboutDeclaringTheDeclaration()
		{
			return new StringCollection();
		}

		#endregion
	}
}
