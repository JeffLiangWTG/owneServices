using CargoWise.EntityFramework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	class FormForUPECurrentQueueUserControl : FormForCurrentQueueUserControl
	{
		public FormForUPECurrentQueueUserControl(IBusiness businessEntity) : base(businessEntity)
		{
		}

		public override string BindToPrefix
		{
			get
			{
				return "ActiveProcessQueueForBinding.";
			}
		}
	}
}
