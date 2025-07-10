using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.eNett_Integration;
using Enterprise.Accounting.GUI.eNett;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ContainerStoragePaymentController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.ContainerStoragePayment; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(StorageFeeInvoicePayment); }
		}

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			IContainerStorageDataProvider container = sourceEntity as IContainerStorageDataProvider;
			StorageFeeInvoicePayment paymentbizObj = sourceEntity as StorageFeeInvoicePayment;

			if (paymentbizObj == null && container != null)
			{
				paymentbizObj = new StorageFeeInvoicePayment(container);
			}

			paymentbizObj.Initialise();

			return paymentbizObj;
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ContainerStoragePaymentForm(businessEntity as StorageFeeInvoicePayment);
		}

		public override IZForm ShowNewForm()
		{
			throw new ContainerStoragePaymentNotSupportedException("Do not use ShowNewForm. Use GetForm(businessEntity) instead.");
		}

		[Serializable]
		class ContainerStoragePaymentNotSupportedException : ModuleFeatureNotSupportedException
		{
			public ContainerStoragePaymentNotSupportedException(string message) : base(message) { }

#if NETFRAMEWORK
			protected ContainerStoragePaymentNotSupportedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewPayablesInvoice; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}
