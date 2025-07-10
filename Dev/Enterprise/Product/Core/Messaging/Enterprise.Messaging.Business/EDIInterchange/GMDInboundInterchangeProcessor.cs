using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	public abstract class GMDInboundInterchangeProcessor : InboundInterchangeProcessor, ICustomsServiceTaskProcess
	{
		protected GMDInboundInterchangeProcessor(IEnumerable<ZString> interchangeTypes)
		{
			this.interchangeTypes = Argument.NotNull(interchangeTypes, nameof(interchangeTypes));
		}
		readonly protected IEnumerable<ZString> interchangeTypes;

		protected sealed override string[] ApplicationCodes => new string[] { EDIInterchange.ApplicationCodes.GenericMessageDelivery };
		protected sealed override bool IsNoBranchFilter => true;
		protected sealed override void AddTypeFilter(ZQuery query)
		{
			query.AddToFilter(EDIInterchangeSchema.EI_GB, GlbBranch.CurrentBranch.PK);
			query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, interchangeTypes);
		}

		public sealed override bool IsInterchangeNotDeleted(EDIInterchange interchange) => !interchange.IsDeleted && interchange.EI_Status != EDIInterchange.Status.Error;
	}
}
