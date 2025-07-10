using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS
{
	public class JobDeclarationMessageSendingObjectParent : Customs.Business.JobDeclarationMessageSendingObjectParent<JobDeclarationMessageSendingObject>
	{
		public JobDeclarationMessageSendingObjectParent(JobDeclaration declaration) : base(declaration)
		{
		}

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		public bool LockDeclarationUntilResponseReceived { get; set; }

		protected override Customs.Business.JobDeclarationMessageSendingObject CreateNewJobDeclarationMessageSendingObject(Customs.Business.CusEntryHeader header)
		{
			return new JobDeclarationMessageSendingObject((CusEntryHeader)header);
		}

		public IEnumerable<JobDeclarationMessageSendingObject> ObjectsToSend
		{
			get => SendingObjectsCollection.OfType<JobDeclarationMessageSendingObject>().Where(x => x.ShouldSend);
		}
	}
}
