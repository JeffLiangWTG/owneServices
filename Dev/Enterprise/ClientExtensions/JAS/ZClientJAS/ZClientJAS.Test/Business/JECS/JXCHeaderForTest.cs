using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Testing
{
	internal class JXCHeaderForTest : NonPersistentBusinessObject, IJXCExportHeader, IJXCImportHeader
	{
		public JXCHeaderForTest() : base(new BusinessObjectFactory())
		{
		}

		public JXCHeaderForTest(ZString sendingOfficeCode, ZString destOfficeCode, ZString freightDest, ZString destNettingCode, ZString sendingNettingCode) : this()
		{
			SetDestinationForwarder(destOfficeCode, destNettingCode);
			SetSendingForwarder(sendingOfficeCode, sendingNettingCode);
			fFreightDest = freightDest;
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return "MEHMEH";
			}
		}

		#region IJXCExportHeader & IJXCImportHeader Members
		public JASOrgHeader SendingForwarder
		{
			get
			{
				return fSendingForwarder;
			}

			set
			{
				fSendingForwarder = value;
			}
		}

		public JASOrgHeader ReceivingForwarder
		{
			get
			{
				return fReceivingForwarder;
			}

			set
			{
				fReceivingForwarder = value;
			}
		}

		public ZString FreightDest
		{
			get
			{
				return fFreightDest;
			}

			set
			{
				fFreightDest = value;
			}
		}

		public void SetDestinationForwarder(ZString destOfficeCode, ZString destNettingCode)
		{
			fReceivingForwarder = Factory.New<JASOrgHeader>();
			fReceivingForwarder.OfficeCode = destOfficeCode;
			fReceivingForwarder.NettingCode = destNettingCode;
		}

		public void SetSendingForwarder(ZString sendingOfficeCode, ZString sendingNettingCode)
		{
			fSendingForwarder = Factory.New<JASOrgHeader>();
			fSendingForwarder.OfficeCode = sendingOfficeCode;
			fSendingForwarder.NettingCode = sendingNettingCode;
		}

		public void SetFreightDestination(ZString freightDestination)
		{
			FreightDest = freightDestination;
		}

		public bool HasJXCWarnings()
		{
			return ExpectedHasJXCWarnings;
		}

		public bool ExpectedHasJXCWarnings;
		JASOrgHeader fSendingForwarder;
		JASOrgHeader fReceivingForwarder;
		ZString fFreightDest;
		#endregion
	}
}
