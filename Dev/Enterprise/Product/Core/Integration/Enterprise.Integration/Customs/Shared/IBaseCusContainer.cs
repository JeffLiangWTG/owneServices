using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface IBaseCusContainer
			{
				ZGuid PK { get; }
				ZString CO_AddInfo { get; set; }
				ZString CO_ContainerNumber { get; set; }
				ZString CO_ContainerSize { get; set; }
				ZString CO_ContainerUQ { get; set; }
				ZString CO_CustomAttrib1 { get; set; }
				ZDateTime CO_CustomDate1 { get; set; }
				ZDecimal CO_CustomDecimal1 { get; set; }
				ZBool CO_CustomFlag1 { get; set; }
				ZString CO_FCL_LCL_AIR { get; set; }
				ZGuid CO_JC { get; set; }
				ZGuid CO_JE { get; set; }
				ZString CO_MessageStatus { get; set; }
				ZGuid CO_RC { get; set; }
				ZString CO_Seal { get; set; }
				ZString CO_SecondSeal { get; set; }
				ZDecimal CO_Weight { get; set; }
				ZString CO_WeightUQ { get; set; }

				ZString GetContainerModeFromFreight(ZString containerModeInFreight);
				bool IsDeletingJobContainer { get; }
				bool IsDeleted { get; }
			}
		}
	}
}
