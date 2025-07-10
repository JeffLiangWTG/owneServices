using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Core.Forms
{
	public interface ICodeDescriptionListControl
	{
		object Data { get; set; }
		ZUserControl Control { get; }
		ZGrid Grid { get; }
		bool ReadOnly { get; set; }
	}
}