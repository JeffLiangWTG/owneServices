using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsDocketContainer : DocBaseWrapper, IDocSimpleContainer
	{
		#region Constructors

		protected DocWhsDocketContainer(WhsDocketContainer whsDocketContainer, BusinessObjectFactory factoryToWrap)
			: base(whsDocketContainer, factoryToWrap)
		{
		}

		#endregion

		#region Static

		public static DocWhsDocketContainer New(WhsDocketContainer whsDocketContainer, BusinessObjectFactory factoryToWrap)
		{
			return (whsDocketContainer == null) ? null : new DocWhsDocketContainer(whsDocketContainer, factoryToWrap);
		}

		#endregion

		#region Properties

		#region ZString Fields

		public ZString IsChargeable
		{
			get { return WhsDocketContainer.WC_IsChargeable ? (ZString)Res.GetString("6684fdab-c75b-49df-a57b-23d92449d564", "Yes") : (ZString)Res.GetString("21a0ca85-8d49-4c32-9ef3-eaf921948907", "No"); }
		}

		#endregion

		#region ZInt Fields

		public ZInt ItemCount
		{
			get { return WhsDocketContainer.WC_ItemCount; }
		}

		public ZInt PalletCount
		{
			get { return WhsDocketContainer.WC_PalletCount; }
		}

		#endregion

		#endregion

		#region IDocSimpleContainer members

		public ZString ContainerNumber
		{
			get { return WhsDocketContainer.WC_ContainerNum; }
		}

		public ZString SealNumber
		{
			get { return WhsDocketContainer.WC_SealNum; }
		}

		public ZInt TotalAllocatedJobPackages
		{
			get { return 0; }
		}

		public ZDecimal TotalAllocatedJobWeight
		{
			get { return 0; }
		}

		public ZDecimal TotalAllocatedJobVolume
		{
			get { return 0; }
		}

		public DocRefContainer Container
		{
			get { return DocRefContainer.New(WhsDocketContainer.Container, Factory); }
		}

		public ZString DeliveryMode
		{
			get { return ZString.Empty; }
		}

		public ZString Type
		{
			get
			{
				RefContainer docRefContainer = Factory.Load<RefContainer>(WhsDocketContainer.WC_RC);
				return docRefContainer != null ? docRefContainer.RC_Code : ZString.Empty;
			}
		}

		public ZShort ContainerCount
		{
			get { return 0; }
		}

		public ZString ClientRef
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region Implementation

		WhsDocketContainer WhsDocketContainer
		{
			get { return (WhsDocketContainer)WrappedObject; }
		}

		#endregion
	}
}
