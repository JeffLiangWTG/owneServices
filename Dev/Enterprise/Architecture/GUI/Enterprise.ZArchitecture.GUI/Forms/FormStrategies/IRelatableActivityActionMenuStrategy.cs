using System.Windows.Forms;

namespace Enterprise.MasterFiles.Integration
{
	public interface IRelatableActivityActionMenuStrategy
	{
		void AddRelatableActivityMenuItemsIfApplicable(Form form);
	}
}
