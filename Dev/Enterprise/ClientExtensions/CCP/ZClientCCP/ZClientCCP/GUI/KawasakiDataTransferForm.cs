using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.ZClientCCP.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.ZClientCCP.GUI
{
	public partial class KawasakiDataTransferForm : ZArchitecture.GUI.DataTransferForm
	{
		public KawasakiDataTransferForm(KawasakiDataTransferSupplySupplier dataTransferBusinessObject) : base(dataTransferBusinessObject)
		{
			InitializeComponent();
			this.DataTransferBusinessObject = dataTransferBusinessObject;
			this.DialogFilter = dataTransferBusinessObject.DialogFilter;
			this.fFormHeading = dataTransferBusinessObject.FormHeading;
			this.FileNameTextBox.CaptionResourceString = Res.GetData("KawasakiDataTransferForm|7b4997dc-65d5-460c-9575-c491c0fbe1b1", "Filename");
		}

		protected KawasakiDataTransferSupplySupplier DataTransferBusinessObject;

		protected override bool ProcessButtonClick()
		{
			bool result = false;
			if (!DataTransferBusinessObject.SupplierForDeclarationInfo.HasErrors())
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();
				OrgHeader supplier = (OrgHeader)factory.LoadFromNaturalKey(typeof(OrgHeader), OrgHeaderSchema.OH_Code, SupplierFindBox.CurrentCode);
				if (supplier != null)
				{
					DataTransferBusinessObject.JobDeclaration.JE_OH_Supplier = supplier.PK;
					result = true;
				}
				else
				{
					Globals.Message.ShowError("Please select a supplier", "Error");
				}
			}
			return result;
		}
	}
}
