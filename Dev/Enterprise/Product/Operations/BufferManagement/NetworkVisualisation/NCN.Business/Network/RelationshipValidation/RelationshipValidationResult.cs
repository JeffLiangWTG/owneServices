using CargoWise.Types;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	class RelationshipValidationResult
	{
		internal static RelationshipValidationResult Success => new RelationshipValidationResult(true, ZString.Empty);

		internal static RelationshipValidationResult Failure(ZString failureReason) => new RelationshipValidationResult(false, failureReason);

		RelationshipValidationResult(bool isValid, ZString failureReason)
		{
			IsValid = isValid;
			FailureReason = failureReason;
		}

		internal bool IsValid { get; }

		internal ZString FailureReason { get; }
	}
}
