using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public class AddInfoJobComInvoiceLine : AddInfo
	{
		public AddInfoJobComInvoiceLine(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		#region CA_ImportReasonCode

		public override ZString CA_ImportReasonCode
		{
			get { return base.CA_ImportReasonCode; }
			set
			{
				base.CA_ImportReasonCode = value;
				InvoiceLine.MarkAsNeedingValidation();
			}
		}

		#endregion

		#region CA_IsCasualImport

		public override ZBool CA_IsCasualImport
		{
			get { return base.CA_IsCasualImport; }
			set
			{
				var oldValue = CA_IsCasualImport;
				base.CA_IsCasualImport = value;
				if (!IsCopying && oldValue != CA_IsCasualImport)
				{
					Declaration?.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region CA_TreatmentCode

		public override ZString CA_TreatmentCode
		{
			get { return base.CA_TreatmentCode; }
			set
			{
				base.CA_TreatmentCode = value;
				InvoiceLine.MarkAsNeedingValidation();
			}
		}

		#endregion

		#region CA_CalculationMethod

		public override ZString CA_CalculationMethod
		{
			get { return base.CA_CalculationMethod; }
			set
			{
				base.CA_CalculationMethod = value;
				InvoiceLine.MarkAsNeedingValidation();
				if (InvoiceHeader != null)
				{
					InvoiceHeader.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region CA_ConveyanceIdentificationNumber

		public override ZString CA_ConveyanceIdentificationNumber
		{
			get { return base.CA_ConveyanceIdentificationNumber; }
			set
			{
				base.CA_ConveyanceIdentificationNumber = value;
				InvoiceLine.MarkAsNeedingValidation();
			}
		}

		#endregion

		#region CA_CustomsValue

		public override ZDecimal CA_CustomsValue
		{
			get { return base.CA_CustomsValue; }
			set
			{
				base.CA_CustomsValue = value;
				if (InvoiceHeader != null)
				{
					InvoiceHeader.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region CA_RN_NKExport

		public override ZString CA_RN_NKExport
		{
			get { return base.CA_RN_NKExport; }
			set
			{
				var oldValue = CA_RN_NKExport;
				base.CA_RN_NKExport = value;
				if (!IsCopying && oldValue != CA_RN_NKExport)
				{
					InvoiceLine.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region CA_CFIAInd

		public override ZString CA_CFIAInd
		{
			get => base.CA_CFIAInd;
			set
			{
				var oldValue = CA_CFIAInd;
				base.CA_CFIAInd = value;
				if (!IsCopying && oldValue != CA_CFIAInd)
				{
					Declaration?.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region CA_GACInd

		public override ZString CA_GACInd
		{
			get => base.CA_GACInd;
			set
			{
				var oldValue = base.CA_GACInd;

				if (oldValue != value)
				{
					base.CA_GACInd = value;
					InvoiceHeader?.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region CA_HCInd

		public override ZString CA_HCInd
		{
			get => base.CA_HCInd;
			set
			{
				var oldValue = CA_HCInd;
				base.CA_HCInd = value;
				if (!IsCopying && oldValue != CA_HCInd)
				{
					Declaration?.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region CA_PHACInd

		public override ZString CA_PHACInd
		{
			get => base.CA_PHACInd;
			set
			{
				var oldValue = CA_PHACInd;
				base.CA_PHACInd = value;
				if (!IsCopying && oldValue != CA_PHACInd)
				{
					Declaration?.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region CA_TCInd
		public override ZString CA_TCInd
		{
			get => base.CA_TCInd;
			set
			{
				var oldValue = CA_TCInd;
				base.CA_TCInd = value;
				if (!IsCopying && oldValue != CA_TCInd)
				{
					Declaration?.MarkAsNeedingValidation();
				}
			}
		}
		#endregion

		#region Implementation

		public new JobComInvoiceLine Parent
		{
			get { return (JobComInvoiceLine)base.Parent; }
			protected set { base.Parent = value; }
		}

		public JobComInvoiceLine InvoiceLine
		{
			get { return Parent; }
		}

		public JobComInvoiceHeader InvoiceHeader
		{
			get { return Parent.InvoiceHeader; }
		}

		public JobDeclaration Declaration
		{
			get { return Parent.Declaration; }
		}

		public new AddInfoJobComInvoiceLineLookups Lookups
		{
			get { return (AddInfoJobComInvoiceLineLookups)base.Lookups; }
		}

		public new AddInfoJobComInvoiceLineValidation Validation
		{
			get { return (AddInfoJobComInvoiceLineValidation)base.Validation; }
		}

		protected override CAAddInfoLookups GetNewLookups()
		{
			return new AddInfoJobComInvoiceLineLookups(this);
		}

		protected override CAAddInfoValidation GetNewValidation()
		{
			CAAddInfoValidation result;
			switch (Declaration?.JE_MessageType ?? InvoiceHeader?.JZ_StandAloneInvoiceDirection ?? ZString.Empty)
			{
				case JobMessageTypeList.Codes.Export:
					result = new ExportAddInfoJobComInvoiceLineValidation(this);
					break;
				case JobMessageTypeList.Codes.Import:
				case JobMessageTypeList.Codes.ImportCopyforB2:
				case JobMessageTypeList.Codes.LowValueShipments:
				case JobMessageTypeList.Codes.LVSForConsolidation:
					result = new ImportAddInfoJobComInvoiceLineValidation(this);
					break;
				default:
					result = new AddInfoJobComInvoiceLineValidation(this);
					break;
			}
			return result;
		}

		#endregion
	}
}
