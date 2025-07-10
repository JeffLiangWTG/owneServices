namespace CargoWise.EntityFramework
{
	public class TypeValidationLimits
	{
		public static readonly TypeValidationLimits Default = new TypeValidationLimits();

		public TypeValidationLimits()
		{
			FutureYearsBeforeError = 5;
			PastYearsBeforeError = 10;
			FutureYearsBeforeWarning = 1;
			PastYearsBeforeWarning = 1;
		}

		public int FutureYearsBeforeError { get; set; }
		public int PastYearsBeforeError { get; set; }
		public int FutureYearsBeforeWarning { get; set; }
		public int PastYearsBeforeWarning { get; set; }
	}
}
