using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.Module
{
	public class EnquiryModule : UPEAirCargoModule
	{
		public override ModuleIdentifier ID
		{
			get { return ClientModuleRegistration.Enquiry; }
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CalloutCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new EnquiryFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new EnquiryFilterControl(GridCollection, (EnquiryFilterBusinessObject)FilterBusinessObject);
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ClientControllerRegistration.Enquiry);
		}

		protected override MenuItem NewBulkStatusUpdatingMenuItem()
		{
			MenuItem result = base.NewBulkStatusUpdatingMenuItem();
			result.Text = "Force to Finance";
			return result;
		}
	}
}
