using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("SignedFor")]
	public class PickupDeliveryConfirmationsWrapper : GenericWrapper
	{
		public PickupDeliveryConfirmationsWrapper(CommonPickupDeliveryConfirm confirmation, BusinessObjectFactory factory)
			: base(confirmation, factory)
		{
			fConfirmation = confirmation ?? factory.GetNull<CommonPickupDeliveryConfirm>();
		}

		public CommonPickupDeliveryConfirm Confirmation
		{
			get { return fConfirmation; }
		}

		readonly CommonPickupDeliveryConfirm fConfirmation;

		public static PickupDeliveryConfirmationsWrapper New(BusinessObject confirmation, BusinessObjectFactory factoryToWrap)
		{
			return confirmation != null ? new PickupDeliveryConfirmationsWrapper((CommonPickupDeliveryConfirm)confirmation, factoryToWrap) : null;
		}

		internal FreightWrapper ParentWrapper { get; set; }

		public DocConfirmDivotCollection Divots
		{
			get
			{
				if (fDivots == null)
				{
					fDivots = new DocConfirmDivotCollection(Factory);

					foreach (CommonConfirmDivot divot in Confirmation.Divots)
					{
						if (divot.J8_PackagesDelivered != 0)
						{
							fDivots.Add(DocConfirmDivot.New(divot, Factory));
						}
					}
				}
				return fDivots;
			}
		}
		DocConfirmDivotCollection fDivots;

		public PackageWrapperCollection Packages
		{
			get { return packages ?? (packages = GetPackages()); }
		}
		PackageWrapperCollection packages;

		protected PackageWrapperCollection GetPackages()
		{
			return new PackageWrapperCollection(Confirmation, Factory);
		}

		public ContainerWrapperCollection Containers
		{
			get { return containers ?? (containers = GetContainers()); }
		}
		ContainerWrapperCollection containers;

		protected ContainerWrapperCollection GetContainers()
		{
			return new ContainerWrapperCollection(Confirmation, Factory);
		}

		#region Properties

		public ZInt PackagesConfirmed
		{
			get { return Confirmation.TotalDeliveredPackages; }
		}

		public ZString SignedFor
		{
			get { return Confirmation.EU_GoodsSignForBy; }
		}

		public ZDateTime PickupDeliveryTime
		{
			get { return Confirmation.EU_PickupDeliveryTime; }
		}

		public ZString ConfirmationType
		{
			get { return Confirmation.EU_PickupDeliveryType; }
		}

		public ZString Notes
		{
			get { return Confirmation.EU_PickupDeliveryInstruction; }
		}

		public ZString GatePassID
		{
			get { return Confirmation.FullGatePass; }
		}

		public ZString DriversName
		{
			get { return Confirmation.EU_DriversName; }
		}

		public ZString VehicleReg
		{
			get { return Confirmation.EU_VehicleRegistration; }
		}

		public ZString TransportCoName
		{
			get { return Confirmation.EU_TransportCoName; }
		}

		public ZDecimal DeliveredWeight
		{
			get { return Confirmation.TotalDeliveredWeight; }
		}

		public ZDecimal DeliveredVolume
		{
			get { return Confirmation.TotalDeliveredVolume; }
		}

		#endregion
	}
}
