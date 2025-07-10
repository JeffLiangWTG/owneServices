namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class NZ
		{
			public interface ICusClassPartPivot : Shared.ICusClassPartPivot
			{
				ICodeDataPairCollection PermitCodes { get; }

				ICodeDataPairCollection ProhibitedCodes { get; }

				ICodeDataPairCollection OtherInfos { get; }
			}
		}
	}
}
