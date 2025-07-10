using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public class AddInfoJobComInvoiceHeader : AddInfo
	{
		public AddInfoJobComInvoiceHeader(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

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
				var oldValue = CA_TreatmentCode;
				base.CA_TreatmentCode = value;
				if (!IsCopying && oldValue != CA_TreatmentCode)
				{
					Parent.InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		public override ZString CA_RN_NKExport
		{
			get { return base.CA_RN_NKExport; }
			set
			{
				var oldValue = CA_RN_NKExport;
				base.CA_RN_NKExport = value;
				if (!IsCopying && oldValue != CA_RN_NKExport)
				{
					Declaration?.MarkAsNeedingValidation();
					Parent.InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString CA_TradeZone
		{
			get { return base.CA_TradeZone; }
			set
			{
				var oldValue = CA_TradeZone;
				base.CA_TradeZone = value;
				if (!IsCopying && oldValue != CA_TradeZone)
				{
					Declaration?.MarkAsNeedingValidation();
					Parent.InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		public new JobComInvoiceHeader Parent
		{
			get { return (JobComInvoiceHeader)base.Parent; }
			protected set { base.Parent = value; }
		}

		public JobDeclaration Declaration
		{
			get { return Parent.JobDeclaration; }
		}

		public new AddInfoJobComInvoiceHeaderLookups Lookups
		{
			get { return (AddInfoJobComInvoiceHeaderLookups)base.Lookups; }
		}

		public new AddInfoJobComInvoiceHeaderValidation Validation
		{
			get { return (AddInfoJobComInvoiceHeaderValidation)base.Validation; }
		}

		protected override CAAddInfoLookups GetNewLookups()
		{
			return new AddInfoJobComInvoiceHeaderLookups(this);
		}

		protected override CAAddInfoValidation GetNewValidation()
		{
			CAAddInfoValidation result;
			switch (Parent.JZ_MessageType)
			{
				case JobMessageTypeList.Codes.Export:
					result = new ExportAddInfoJobComInvoiceHeaderValidation(this);
					break;
				case JobMessageTypeList.Codes.Import:
				case JobMessageTypeList.Codes.LowValueShipments:
				case JobMessageTypeList.Codes.LVSForConsolidation:
					result = new ImportAddInfoJobComInvoiceHeaderValidation(this);
					break;
				case JobMessageTypeList.Codes.B2Adjustments:
				case JobMessageTypeList.Codes.XTypeEntry:
					result = new B2AddInfoJobComInvoiceHeaderValidation(this);
					break;
				default:
					result = new AddInfoJobComInvoiceHeaderValidation(this);
					break;
			}
			return result;
		}
	}
}
