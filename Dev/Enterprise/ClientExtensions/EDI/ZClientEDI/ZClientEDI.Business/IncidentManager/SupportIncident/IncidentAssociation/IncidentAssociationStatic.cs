using System;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public static class IncidentAssociationStatic
	{
		public static int MaxDegreeOfParallelism { get; } = Math.Max(1, System.Environment.ProcessorCount > 4 ? System.Environment.ProcessorCount - 2 : System.Environment.ProcessorCount - 1);
	}
}
