using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Integration
{
	public interface IPivotBusinessObject
	{
		ZGuid Relation1ID { get; set; }
		BusinessObject Relation1Object { get; }
		ZGuid Relation2ID { get; set; }
		BusinessObject Relation2Object { get; }

		void Delete();
	}
}
