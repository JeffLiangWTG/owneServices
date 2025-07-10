using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	/// <summary>
	/// When implemented, this interface specifies that a column of an EnterpriseBusinessObject ought to be set by a number fountain.
	/// This ought not be used for other purposes.
	/// </summary>
	public interface INumberFountainEntityWithID : IBusiness
	{
		ZString ID { get; set; }
	}
}
