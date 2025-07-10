using System.Windows.Forms;

namespace Enterprise.MasterFiles.Integration
{
	public interface ISendEmailActionMenuStrategy
	{
		void AddSendEmailActionMenuIfApplicable(Form form);
	}
}
