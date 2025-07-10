using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	public abstract class BranchInboundInterchangeProcessor : InboundInterchangeProcessor, ICustomsServiceTaskProcess
	{
		protected BranchInboundInterchangeProcessor(IEnumerable<string> applicationCodes)
		{
			Argument.NotNull(applicationCodes, nameof(applicationCodes));
			this.applicationCodes = applicationCodes.ToArray();
		}
		readonly string[] applicationCodes;

		public string[] GetSecurePrivateGrades()
		{
			return (string[])applicationCodes.Clone();
		}

		protected sealed override string[] ApplicationCodes => applicationCodes;
		protected sealed override bool IsNoBranchFilter => true;
		protected sealed override void AddTypeFilter(ZQuery query)
		{
			query.AddToFilter(EDIInterchangeSchema.EI_GB, GlbBranch.CurrentBranch.PK);
		}

		public sealed override bool IsInterchangeNotDeleted(EDIInterchange interchange) => !interchange.IsDeleted && interchange.EI_Status != EDIInterchange.Status.Error;
	}
}
