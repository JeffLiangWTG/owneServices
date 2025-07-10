using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class DeltaGJobDeclarationMessageSendingObjectParent : Customs.Business.JobDeclarationMessageSendingObjectParent<DeltaGJobDeclarationMessageSendingObject>
	{
		public DeltaGJobDeclarationMessageSendingObjectParent(JobDeclaration declaration) : base(declaration)
		{
		}

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		protected override Customs.Business.JobDeclarationMessageSendingObject CreateNewJobDeclarationMessageSendingObject(Customs.Business.CusEntryHeader header)
		{
			return new DeltaGJobDeclarationMessageSendingObject((CusEntryHeader)header);
		}

		public IEnumerable<DeltaGJobDeclarationMessageSendingObject> ObjectsToSend
		{
			get => SendingObjectsCollection.OfType<DeltaGJobDeclarationMessageSendingObject>().Where(x => x.ShouldSend);
		}
	}
}
