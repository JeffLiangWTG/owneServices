using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class NZ
		{
			public interface ICusClassification : IBaseCusClassification
			{
				ZString CC_PartsOfClassification { get; set; }

				ZString CC_ConcessionCode { get; set; }

				ICodeDataPairCollection PermitCodes { get; }

				ICodeDataPairCollection ProhibitedCodes { get; }

				ICodeDataPairCollection OtherInfos { get; }
			}
		}
	}
}