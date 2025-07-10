using System;
using Enterprise.RemotePrinting.Types;

namespace Enterprise.RemotePrinting.Client
{
	public struct JobDetails
	{
		public JobDetails(Guid pk, string failureReason, ProcessedStatus status)
		{
			PK = pk;
			FailureReason = failureReason;
			Status = status;
		}

		public Guid PK { get; }
		public string FailureReason { get; }
		public ProcessedStatus Status { get; }

		public override bool Equals(object obj)
		{
			if (!(obj is JobDetails details))
			{
				return false;
			}

			return PK == details.PK &&
				   FailureReason == details.FailureReason &&
				   Status == details.Status;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = -861393856;
				hashCode = hashCode * -1521134295 + PK.GetHashCode();
				hashCode = hashCode * -1521134295 + (FailureReason?.GetHashCode() ?? 0);
				hashCode = hashCode * -1521134295 + Status.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(JobDetails details1, JobDetails details2)
			=> details1.Equals(details2);

		public static bool operator !=(JobDetails details1, JobDetails details2)
			=> !(details1 == details2);
	}
}
