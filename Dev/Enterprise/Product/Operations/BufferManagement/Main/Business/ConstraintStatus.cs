using System;

namespace Enterprise.BufferManagement.Business
{
	public enum ConstraintStatus
	{
		Unknown = 0,
		NonConstrained,
		PreConstraint,
		ReadyForConstraint,
		PostConstraint,
	}

	public static class ConstraintStatusExtensions
	{
		public static string ToCode(this ConstraintStatus status)
		{
			switch (status)
			{
				case ConstraintStatus.NonConstrained:
					return ConstraintStatusList.Codes.NonConstrained;

				case ConstraintStatus.PreConstraint:
					return ConstraintStatusList.Codes.PreConstraint;

				case ConstraintStatus.ReadyForConstraint:
					return ConstraintStatusList.Codes.ReadyforConstraint;

				case ConstraintStatus.PostConstraint:
					return ConstraintStatusList.Codes.PostConstraint;

				case ConstraintStatus.Unknown:
					return string.Empty;

				default:
					throw new ArgumentException("Invalid ConstraintStatus: " + status);
			}
		}
	}
}
