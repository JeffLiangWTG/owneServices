using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IServiceRequestController
	{
		void SetParentForm(Form parentForm);
		bool ShouldSendERequestDocument { get; set; }
		string GetModuleId(Form form);
		string GetCustomerServiceMenuSectionCode(string moduleTreeId, Form form);
	}
}
