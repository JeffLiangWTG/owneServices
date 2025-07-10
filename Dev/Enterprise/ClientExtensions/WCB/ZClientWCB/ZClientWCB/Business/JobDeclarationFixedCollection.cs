using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;

namespace Enterprise.Client.WCB
{
	internal class JobDeclarationFixedCollection : JobDeclarationCollection
	{
		public JobDeclarationFixedCollection(BaseJobDeclarationCollection jobDecs)
			: base(jobDecs.Factory)
		{
			foreach (BaseJobDeclaration jobDec in jobDecs)
			{
				JobDeclarationWithFixedInvHeads jobDecWithFixedInvHeads = Factory.Load<JobDeclarationWithFixedInvHeads>(jobDec.PK);
				Add(jobDecWithFixedInvHeads);
			}
		}

		public new JobDeclarationWithFixedInvHeads this[int index]
		{
			get { return (JobDeclarationWithFixedInvHeads)base[index]; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
