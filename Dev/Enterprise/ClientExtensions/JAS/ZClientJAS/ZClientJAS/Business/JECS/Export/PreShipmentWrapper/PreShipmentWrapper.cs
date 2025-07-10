using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class PreShipmentWrapper : JXCExportHeaderWrapper
	{
		public PreShipmentWrapper(JASForwardingShipment shipment)
			: base(shipment)
		{
			RegisterEditableChildObject(shipment);
		}

		#region Properties

		public ZGuid SendingForwarderPK
		{
			get { return fSendingForwarderPK; }
			set
			{
				if (fSendingForwarderPK != value)
				{
					SetNonPersistentPropertyValue(SendingForwarderPKInfo, ref fSendingForwarderPK, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateSendingForwarderPK();
					}
				}
			}
		}

		public ZPropertyInfo SendingForwarderPKInfo
		{
			get { return GetZPropertyInfo(nameof(SendingForwarderPK)); }
		}

		public ForwarderCollection SendingForwarders
		{
			get
			{
				if (fSendingForwarders == null)
				{
					fSendingForwarders = new ForwarderCollection(Factory);
				}
				return fSendingForwarders;
			}
		}

		public ZGuid ReceivingForwarderPK
		{
			get { return fReceivingForwarderPK; }
			set
			{
				if (fReceivingForwarderPK != value)
				{
					SetNonPersistentPropertyValue(ReceivingForwarderPKInfo, ref fReceivingForwarderPK, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateReceivingForwarderPK();
					}
				}
			}
		}

		public ZPropertyInfo ReceivingForwarderPKInfo
		{
			get { return GetZPropertyInfo(nameof(ReceivingForwarderPK)); }
		}

		public ForwarderCollection ReceivingForwarders
		{
			get
			{
				if (fReceivingForwarders == null)
				{
					fReceivingForwarders = new ForwarderCollection(Factory);
				}
				return fReceivingForwarders;
			}
		}

		ZGuid fSendingForwarderPK;
		ForwarderCollection fSendingForwarders;
		ZGuid fReceivingForwarderPK;
		ForwarderCollection fReceivingForwarders;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		public PreShipmentWrapperValidation Validation
		{
			get { return new PreShipmentWrapperValidation(this); }
		}

		#endregion

		#region IJXCExportHeader Overrides

		protected override JASOrgHeader SendingForwarderCore
		{
			get { return (JASOrgHeader)Factory.Load(typeof(JASOrgHeader), SendingForwarderPK); }
		}

		protected override JASOrgHeader ReceivingForwarderCore
		{
			get { return (JASOrgHeader)Factory.Load(typeof(JASOrgHeader), ReceivingForwarderPK); }
		}

		protected override ZString FreightDestCore
		{
			get { return Shipment.JS_RL_NKDestination; }
		}

		#endregion

		public JASForwardingShipment Shipment
		{
			get { return (JASForwardingShipment)WrappedBizO; }
		}
	}
}

#region Properties
#endregion
#region IJXCExportHeader
#endregion
#region Implementation
#endregion
