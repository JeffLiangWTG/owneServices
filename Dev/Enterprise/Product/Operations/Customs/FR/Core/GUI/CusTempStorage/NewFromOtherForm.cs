using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.FR.GUI.CusTempStorage
{
	public partial class NewFromOtherForm : ZChildForm
	{
		public NewFromOtherForm(TemporaryStorageWrapperFromParentHelper helper)
			: base(helper)
		{
			InitializeComponent();
			SetDataBinding(helper, string.Empty);
		}

		protected string FormCaptionCore => (NoResString)"New IST From Other Job";

		TemporaryStorageWrapperFromParentHelper Helper => (TemporaryStorageWrapperFromParentHelper)BusinessEntity;

		void CancelButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		protected void OkButton_Click(object sender, System.EventArgs e)
		{
			TemporaryStorageWrapperHeader temporaryStorageHeader = null;

			if (Helper.Shipment != null)
			{
				temporaryStorageHeader = TemporaryStorageWrapperHeader.NewFromShipment(Helper.Shipment);
			}
			else if (Helper.Declaration != null)
			{
				temporaryStorageHeader = TemporaryStorageWrapperHeader.NewFromDeclaration(Helper.Declaration);
			}
			else if (Helper.DeltaT != null)
			{
				temporaryStorageHeader = TemporaryStorageWrapperHeader.NewFromDeltaT(Helper.DeltaT);
			}

			if (temporaryStorageHeader != null)
			{
				var factory = new BusinessObjectFactory();
				var storageHeader = CusTempStorageJobHeader.New(factory);
				storageHeader.InitialisefromNPBO(temporaryStorageHeader);

				if (Helper.Shipment != null)
				{
					storageHeader.SetRelatedBusinessObject(Helper.Shipment, Core.Constants.GenPivotTypes.CusStorageHeaderShipment);
				}
				else if (Helper.Declaration != null)
				{
					storageHeader.SetRelatedBusinessObject(Helper.Declaration, Core.Constants.GenPivotTypes.CusStorageHeaderDeclaration);
				}
				else
				{
					storageHeader.SetRelatedBusinessObject(Helper.DeltaT, Core.Constants.GenPivotTypes.CusStorageHeaderNctsHeader);
				}

				factory.Save();

				this.Close();
				var newForm = new CusTempStorageForm(storageHeader);
				newForm.ControllerID = ControllerIDs.Customs.TemporaryStorage;
				ZFormModaliser.ShowDialogAndDispose(newForm);
			}
		}

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
		}
	}
}
