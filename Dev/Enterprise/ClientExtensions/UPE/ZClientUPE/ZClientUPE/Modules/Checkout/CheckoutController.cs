using System;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.Module
{
	public class CheckoutController : ZSingletonController
	{
		public override ControllerID ID
		{
			get { return ClientControllerRegistration.Checkout; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(Checkout); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CheckoutForm((Checkout)businessEntity);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new Checkout(new BusinessObjectFactory());
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		internal SecurityCheckpoint InternalCheckPointForNew => CheckPointForNew;
	}
}
