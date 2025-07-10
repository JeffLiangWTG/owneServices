using System.Windows.Forms;

namespace Enterprise.DocumentScanning.Business
{
	public interface IEditResponse
	{
		event DocumentEventHandler CloseOfForm;
		DialogResult DialogResult { get; set; }
		bool ApplyToAll { get; }
	}
}
