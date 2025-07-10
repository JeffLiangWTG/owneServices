using System.Windows.Forms;
using Enterprise.Client.ZClientCCP.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.ZClientCCP.GUI.Testing
{
	[TestedType(typeof(KawasakiDataTransferForm))]
	public class KawasakiDataTransferFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();
			KawasakiDataTransferSupplySupplier transferBusinessObject = new KawasakiDataTransferSupplySupplier(jobDec, "Comma delimited files (*.csv)|*.csv|Text files (*.txt)|*.txt|All files (*.*)|*.*", "Kawasaki");
			return new KawasakiDataTransferForm(transferBusinessObject);
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			if (control.Name == "LogListBox")
			{
				return true;
			}

			return base.ShouldIgnoreMissingBindingMember(control);
		}
	}
}
