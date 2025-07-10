using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface IBaseAutoSendingMessageSupporter
		{
			Guid RegistryBranchPK { get; }
			IProcessor CreateStmProcessQueueProcessor(BusinessObject parent, ZString triggerActionCode);
		}
	}
}
