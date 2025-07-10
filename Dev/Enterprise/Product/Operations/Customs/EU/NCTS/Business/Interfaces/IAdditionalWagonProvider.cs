using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business.Interfaces
{
	public interface IAdditionalWagonProvider
	{
		ZString WagonNumber { get; set; }

		[BusinessObjectTestExclude] // The [List] attribute must be applied to the property with a valid list property name
		ZString WagonNationality { get; set; }
	}
}
