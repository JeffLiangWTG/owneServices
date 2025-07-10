using System.Collections.Specialized;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.MessageBuilders
{
	public class JobDeclarationMessageManager : Customs.Business.MessageManager
	{
		public JobDeclarationMessageManager(JobDeclaration declaration, IMessageGenerator<CusEntryHeader> generator)
			: base(true)
		{
			this.declaration = declaration;
			this.generator = generator;
		}
		readonly IMessageGenerator<CusEntryHeader> generator;

		protected override BusinessObject Master
		{
			get { return declaration; }
		}
		readonly JobDeclaration declaration;

		protected override SendAmendmentIfNeededDelegate[] SendAmendmentIfNeededDelegates
		{
			get { return null; }
		}

		public bool DeclareDeclaration(Customs.Business.ISendsMessagesToCustoms sender)
		{
			// Log first, then message.  Cos the messaging has a built-in save.  If we message (&save) and then log only afterwards, the user might reject the changes (the new event) and CCC event is not saved. 
			var replaceThisTokenEventuallyQuestionMarkExclamationMark = CancellationToken.None;
			declaration.LogCustomsCommencedIfNeeded();
			return SendMessage(sender,
				GetAnyReasonsWeCantDeclareTheDeclaration(),
				GetWarningsAboutDeclaringTheDeclaration(),
				new MessageBuilderDelegate[] { GetDeclarationMessageBuilder }, (NoResString)"Declaration",
				replaceThisTokenEventuallyQuestionMarkExclamationMark);
		}

		IMessageBuilder GetDeclarationMessageBuilder(BusinessObject master)
		{
			return new DeclarationMessageBuilder(master as JobDeclaration, generator);
		}

		StringCollection GetAnyReasonsWeCantDeclareTheDeclaration()
		{
			return new StringCollection();
		}

		StringCollection GetWarningsAboutDeclaringTheDeclaration()
		{
			return new StringCollection();
		}
	}
}
