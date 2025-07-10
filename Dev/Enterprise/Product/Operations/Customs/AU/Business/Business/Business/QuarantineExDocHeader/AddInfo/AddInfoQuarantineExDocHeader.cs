using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AddInfoQuarantineExDocHeader : AutoAddInfoQuarantineExDocHeader
	{
		public AddInfoQuarantineExDocHeader(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new QuarantineExDocHeader Parent => (QuarantineExDocHeader)base.Parent;

		public override ZString ZH_ForwardLocation
		{
			get { return base.ZH_ForwardLocation; }
			set
			{
				var oldValue = ZH_ForwardLocation;
				base.ZH_ForwardLocation = value;
				if (oldValue != ZH_ForwardLocation && !IsMarkingAsNeedingValidationSuspended)
				{
					MarkRequiredObjectsAsNeedingValidation();
				}
			}
		}

		public override ZString ZH_ApprovedCertifier
		{
			get { return base.ZH_ApprovedCertifier; }
			set
			{
				var oldValue = ZH_ApprovedCertifier;
				base.ZH_ApprovedCertifier = value;
				if (oldValue != ZH_ApprovedCertifier && !IsMarkingAsNeedingValidationSuspended)
				{
					MarkRequiredObjectsAsNeedingValidation();
				}
			}
		}

		public override ZBool ZH_AuthorisationFlag
		{
			get { return base.ZH_AuthorisationFlag; }
			set
			{
				var oldValue = ZH_AuthorisationFlag;
				base.ZH_AuthorisationFlag = value;
				if (oldValue != ZH_AuthorisationFlag && !IsMarkingAsNeedingValidationSuspended)
				{
					MarkRequiredObjectsAsNeedingValidation();
				}
			}
		}

		public override ZString ZH_AuthorisationLocation
		{
			get { return base.ZH_AuthorisationLocation; }
			set
			{
				var oldValue = ZH_AuthorisationLocation;
				base.ZH_AuthorisationLocation = value;
				if (oldValue != ZH_AuthorisationLocation && !IsMarkingAsNeedingValidationSuspended)
				{
					MarkRequiredObjectsAsNeedingValidation();
					Parent.InvoiceHeader?.QuarantineExDocHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString ZH_AvAnimalAge
		{
			get { return base.ZH_AvAnimalAge; }
			set
			{
				var oldValue = ZH_AvAnimalAge;
				base.ZH_AvAnimalAge = value;
				if (oldValue != ZH_AvAnimalAge && !IsMarkingAsNeedingValidationSuspended)
				{
					MarkRequiredObjectsAsNeedingValidation();
				}
			}
		}

		public override ZString ZH_TransferExporterLocation
		{
			get { return base.ZH_TransferExporterLocation; }
			set
			{
				var oldValue = ZH_TransferExporterLocation;
				base.ZH_TransferExporterLocation = value;
				if (oldValue != ZH_TransferExporterLocation && !IsMarkingAsNeedingValidationSuspended)
				{
					MarkRequiredObjectsAsNeedingValidation();
				}
			}
		}

		public override ZString ZH_TransferEDIUserLocation
		{
			get { return base.ZH_TransferEDIUserLocation; }
			set
			{
				var oldValue = ZH_TransferEDIUserLocation;
				base.ZH_TransferEDIUserLocation = value;
				if (oldValue != ZH_TransferEDIUserLocation && !IsMarkingAsNeedingValidationSuspended)
				{
					MarkRequiredObjectsAsNeedingValidation();
				}
			}
		}

		public override ZString ZH_StorageLocation
		{
			get { return base.ZH_StorageLocation; }
			set
			{
				var oldValue = ZH_StorageLocation;
				base.ZH_StorageLocation = value;
				if (oldValue != ZH_StorageLocation && !IsMarkingAsNeedingValidationSuspended)
				{
					MarkRequiredObjectsAsNeedingValidation();
				}
			}
		}

		public override ZString ZH_ProductUseIndicator
		{
			get { return base.ZH_ProductUseIndicator; }
			set
			{
				var oldValue = ZH_ProductUseIndicator;
				base.ZH_ProductUseIndicator = value;
				if (oldValue != ZH_ProductUseIndicator && !IsMarkingAsNeedingValidationSuspended)
				{
					MarkRequiredObjectsAsNeedingValidation();
				}
			}
		}

		public override ZString ZH_PrintLocation
		{
			get { return base.ZH_PrintLocation; }
			set
			{
				var oldValue = ZH_PrintLocation;
				base.ZH_PrintLocation = value;
				if (oldValue != ZH_PrintLocation && !IsMarkingAsNeedingValidationSuspended)
				{
					Parent.MarkAsNeedingValidation();
					MarkRequiredObjectsAsNeedingValidation();
				}
			}
		}

		public override ZString ZH_TrueAndCompleteIndicator
		{
			get { return base.ZH_TrueAndCompleteIndicator; }
			set
			{
				var oldValue = ZH_TrueAndCompleteIndicator;
				base.ZH_TrueAndCompleteIndicator = value;
				if (oldValue != ZH_TrueAndCompleteIndicator && !IsMarkingAsNeedingValidationSuspended)
				{
					Parent.MarkAsNeedingValidation();
					MarkRequiredObjectsAsNeedingValidation();
				}
			}
		}

		public override ZString ZH_ApprovalNumber
		{
			get { return base.ZH_ApprovalNumber; }
			set
			{
				var oldValue = ZH_ApprovalNumber;
				base.ZH_ApprovalNumber = value;
				if (oldValue != ZH_ApprovalNumber && !IsMarkingAsNeedingValidationSuspended)
				{
					Parent.MarkAsNeedingValidation();
					MarkRequiredObjectsAsNeedingValidation();
				}
			}
		}

		public override ZString ZH_TransitLocationType
		{
			get { return base.ZH_TransitLocationType; }
			set
			{
				var oldValue = ZH_TransitLocationType;
				base.ZH_TransitLocationType = value;
				if (oldValue != ZH_TransitLocationType && !IsMarkingAsNeedingValidationSuspended)
				{
					Parent.MarkAsNeedingValidation();
					MarkRequiredObjectsAsNeedingValidation();
				}
			}
		}

		public override ZBool ZH_ForwardRequiresAcceptance
		{
			get { return base.ZH_ForwardRequiresAcceptance; }
			set
			{
				var oldValue = ZH_ForwardRequiresAcceptance;
				base.ZH_ForwardRequiresAcceptance = value;
				if (oldValue != ZH_ForwardRequiresAcceptance && !IsMarkingAsNeedingValidationSuspended)
				{
					Parent.MarkAsNeedingValidation();
					MarkRequiredObjectsAsNeedingValidation();
				}
			}
		}

		public override ZString ZH_AmendmentResponseStatus
		{
			get { return base.ZH_AmendmentResponseStatus; }
			set
			{
				var oldValue = ZH_AmendmentResponseStatus;
				base.ZH_AmendmentResponseStatus = value;
				if (oldValue != ZH_AmendmentResponseStatus && !IsMarkingAsNeedingValidationSuspended)
				{
					Parent.MarkAsNeedingValidation();
					MarkRequiredObjectsAsNeedingValidation();
				}
			}
		}

		void MarkRequiredObjectsAsNeedingValidation()
		{
			Parent.Declaration?.MarkAsNeedingValidation();
			Parent.InvoiceHeader?.JobComInvoiceLines.MarkAsNeedingValidation();
		}
	}
}
