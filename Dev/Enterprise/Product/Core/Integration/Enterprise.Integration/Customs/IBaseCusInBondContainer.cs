using CargoWise.Types;
namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface IBaseCusInBondContainer
		{
			ZGuid PK { get; }
			ZString BC_ContainerNum { get; set; }
			ZGuid BC_ParentID { get; set; }
			ZString BC_ParentTableCode { get; set; }
			ZGuid BC_RC { get; set; }
			ZString BC_Seal1 { get; set; }
			ZString BC_Seal2 { get; set; }
			ZString BC_TypeOfService { get; set; }
		}
	}
}
