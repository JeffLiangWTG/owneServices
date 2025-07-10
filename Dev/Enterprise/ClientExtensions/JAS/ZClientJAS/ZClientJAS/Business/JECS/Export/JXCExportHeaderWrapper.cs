
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public abstract class JXCExportHeaderWrapper : NonPersistentBusinessObject, IJXCExportHeader
	{
		public JXCExportHeaderWrapper(BusinessObject wrappedBizO)
			: base(wrappedBizO.Factory)
		{
			this.WrappedBizO = wrappedBizO;
		}

		#region IJXCExportHeader Members

		public JASOrgHeader SendingForwarder
		{
			get { return SendingForwarderCore; }
		}

		public JASOrgHeader ReceivingForwarder
		{
			get { return ReceivingForwarderCore; }
		}

		public ZString FreightDest
		{
			get { return FreightDestCore; }
		}

		protected abstract JASOrgHeader SendingForwarderCore { get; }
		protected abstract JASOrgHeader ReceivingForwarderCore { get; }
		protected abstract ZString FreightDestCore { get; }

		#endregion

		protected override ZString HumanReadableNameCore
		{
			get { return WrappedBizO.HumanReadableName; }
		}

		protected readonly BusinessObject WrappedBizO;
	}
}
