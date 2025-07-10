using CargoWise.Types;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionImportWizardSettings : DataImportWizardSettings
	{
		public CommissionImportWizardSettings()
		{
			ImportType = CommissionImportTypeList.Codes.Add;
		}

		public ZString ImportType { get; set; }
	}
}
