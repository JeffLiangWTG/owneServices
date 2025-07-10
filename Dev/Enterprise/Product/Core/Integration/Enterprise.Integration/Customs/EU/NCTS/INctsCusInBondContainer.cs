using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EU
		{
			public static partial class NCTS
			{
				public interface INctsCusInBondContainer : ICusInBondContainer
				{
					bool IsDeleted { get; }
					string TablePrefix { get; }
					SchemaGuidColumn PKSchemaColumn { get; }
					void MarkAsNeedingValidation();
					ZPropertyInfo BC_ContainerNumInfo { get; }
					ZPropertyInfo BC_RCInfo { get; }
					ZString BC_Mode { get; set; }
					ZPropertyInfo BC_ModeInfo { get; }
					ZString BC_UnloadedState { get; }
					ZPropertyInfo BC_UnloadedStateInfo { get; }
					ZShort BC_SequenceNumber { get; set; }
				}
			}
		}
	}
}

