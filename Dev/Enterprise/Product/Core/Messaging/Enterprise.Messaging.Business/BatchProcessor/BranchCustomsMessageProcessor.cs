using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	public abstract class BranchCustomsMessageProcessor : BranchMessageProcessor, ICustomsServiceTaskProcess
	{
		protected BranchCustomsMessageProcessor(IEnumerable<ZString> applicationCodes, IEnumerable<ZString> messageTypes)
		{
			this.applicationCodes = Argument.NotNull(applicationCodes, nameof(applicationCodes)).ToArray();
			this.messageTypes = messageTypes?.ToArray() ?? System.Array.Empty<ZString>();
		}
		readonly ZString[] applicationCodes;
		readonly ZString[] messageTypes;

		protected sealed override ZQuery GetQueuedQuery() => base.GetQueuedQuery();
		protected sealed override ZQuery ValidBranchesForMessageFilter
		{
			get
			{
				var result = new ZQuery(EDIMessageSchema.EM_GB, GlbBranch.CurrentBranch.PK);
				result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, applicationCodes);
				if (messageTypes.Length > 0)
				{
					result.AddToFilter(EDIMessageSchema.EM_MessageType, messageTypes);
				}
				return result;
			}
		}

		protected sealed override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			return GetMessageProcessorsCore().ToList();
		}

		protected virtual List<ApplicationTypeMessageProcessor> GetMessageProcessorsCore()
		{
			return new List<ApplicationTypeMessageProcessor>();
		}

		protected override bool HasAnyMessageAnticipated()
		{
			return applicationCodes.Length > 0 || base.HasAnyMessageAnticipated();
		}
	}
}
