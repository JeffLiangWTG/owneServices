using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IContact
	{
		ZString FullName { get; set; }
		ZString Phone { get; set; }
		ZString Email { get; set; }
	}
}
