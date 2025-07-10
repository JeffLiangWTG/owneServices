using System.Windows.Forms;
using Enterprise.Accounting.Business.eNett;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.eNett.Testing
{
	[TestedType(typeof(ContainerStoragePaymentOrgSelectionForm))]
	internal sealed class ContainerStoragePaymentOrgSelectionFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			ComPayRegisteredOrganisationDataSource dataSource = new ComPayRegisteredOrganisationDataSource(Factory);
			return new ContainerStoragePaymentOrgSelectionForm(dataSource);
		}

		#endregion
	}
}
