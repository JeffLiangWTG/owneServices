using CargoWise.Types;

namespace Enterprise.ZArchitecture.Integration
{
	public interface IGenPivot
	{
		ZGuid XX_Relation1ID { get; set; }
		ZString XX_Relation1TableCode { get; set; }
		ZGuid XX_Relation2ID { get; set; }
		ZString XX_Relation2TableCode { get; set; }
		ZString XX_RelationType { get; set; }
	}
}