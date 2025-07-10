using System.Collections;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.DeviceManagement.GUI
{
	class ClientTelRimRegistrationSchemeDropEdit : ZDropEdit
	{
		protected override IList GetFilteredListForDropDown()
		{
			return EDIDataRegistry.Instance.TcaRimEnrollmentSchemeDescriptions.Value;
		}
	}
}
