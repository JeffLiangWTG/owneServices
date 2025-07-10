using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class AddInfoJobDeclaration : AddInfo
	{
		public AddInfoJobDeclaration(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		public new JobDeclaration Parent
		{
			get { return (JobDeclaration)base.Parent; }
			protected set { base.Parent = value; }
		}

		public override ZGuid CA_RX_DeclaredCurr
		{
			get { return base.CA_RX_DeclaredCurr; }
			set
			{
				var oldValue = CA_RX_DeclaredCurr;
				base.CA_RX_DeclaredCurr = value;
				if (oldValue != CA_RX_DeclaredCurr && isInitialised)
				{
					Parent.Invoices.MarkAsNeedingValidation();
				}
			}
		}

		#region CA_AssesmentOption

		public override ZString CA_AssesmentOption
		{
			get { return base.CA_AssesmentOption; }
			set
			{
				var oldValue = CA_AssesmentOption;
				base.CA_AssesmentOption = value;
				if (!IsCopying && oldValue != CA_AssesmentOption && isInitialised)
				{
					Parent.Invoices.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region CA_EstReleaseDate

		public override ZDateTime CA_EstReleaseDate
		{
			get { return base.CA_EstReleaseDate; }
			set
			{
				var oldValue = CA_EstReleaseDate;
				base.CA_EstReleaseDate = value;
				if (!IsCopying && oldValue != CA_EstReleaseDate && isInitialised)
				{
					Parent.InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region CA_OGDCFIA

		public override ZBool CA_OGDCFIA
		{
			get { return base.CA_OGDCFIA; }
			set
			{
				var oldValue = CA_OGDCFIA;
				base.CA_OGDCFIA = value;
				if (!IsCopying && oldValue != CA_OGDCFIA && isInitialised)
				{
					Parent.InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region CA_OGDNR

		public override ZBool CA_OGDNR
		{
			get { return base.CA_OGDNR; }
			set
			{
				var oldValue = CA_OGDNR;
				base.CA_OGDNR = value;
				if (!IsCopying && oldValue != CA_OGDNR && isInitialised)
				{
					Parent.InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region CA_OGDTC

		public override ZBool CA_OGDTC
		{
			get { return base.CA_OGDTC; }
			set
			{
				var oldValue = CA_OGDTC;
				base.CA_OGDTC = value;
				if (!IsCopying && oldValue != CA_OGDTC && isInitialised)
				{
					Parent.InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		public override ZString CA_ServiceOption
		{
			get { return base.CA_ServiceOption; }
			set
			{
				var oldValue = CA_ServiceOption;
				base.CA_ServiceOption = value;
				if (oldValue != CA_ServiceOption && isInitialised)
				{
					Parent.MarkAsNeedingValidation();
					Parent.Packages.MarkAsNeedingValidation();
					Parent.Invoices.MarkAsNeedingValidation();
					Parent.InvoiceLines.MarkAsNeedingValidation();

					foreach (JobComInvoiceLine invoiceLine in Parent.InvoiceLines)
					{
						var header = invoiceLine.CFIAPGAHeader;
						header?.AIRSRegistrationNumbers.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZString CA_RN_NKExport
		{
			get { return base.CA_RN_NKExport; }
			set
			{
				var oldValue = CA_RN_NKExport;
				base.CA_RN_NKExport = value;
				if (oldValue != CA_RN_NKExport && isInitialised)
				{
					Parent.MarkAsNeedingValidation();
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
				if (oldValue != CA_TradeZone && isInitialised)
				{
					Parent.MarkAsNeedingValidation();
				}
			}
		}

		public new AddInfoJobDeclarationLookups Lookups
		{
			get { return (AddInfoJobDeclarationLookups)base.Lookups; }
		}

		public new AddInfoJobDeclarationValidation Validation
		{
			get { return (AddInfoJobDeclarationValidation)base.Validation; }
		}

		#region Implementation

		protected override SchemaColumn[] ColumnsForFastSearch
		{
			get
			{
				return new SchemaColumn[] {
					CAAddInfoSchema.CA_LVSCloseDate,
					CAAddInfoSchema.CA_UnladingOffice,
					CAAddInfoSchema.CA_CarrierCode,
					CAAddInfoSchema.CA_ServiceOption,
					CAAddInfoSchema.CA_B2Type,
					CAAddInfoSchema.CA_OriginalTransactionNo,
					CAAddInfoSchema.CA_OGDStatus,
					CAAddInfoSchema.CA_EstimatedPaymentDueDate,
					CAAddInfoSchema.CA_IsOurFault,
					CAAddInfoSchema.CA_InitiatedBy,
					CAAddInfoSchema.CA_ChequeNo,
					CAAddInfoSchema.CA_ChequeDate,
					CAAddInfoSchema.CA_B2Total,
					CAAddInfoSchema.CA_DeclarationException,
					CAAddInfoSchema.CA_B2SubmissionDate,
					CAAddInfoSchema.CA_ConfirmedDate,
					CAAddInfoSchema.CA_B2AcceptedDate,
					CAAddInfoSchema.CA_CSAEntry
				};
			}
		}

		protected override CAAddInfoLookups GetNewLookups()
		{
			return new AddInfoJobDeclarationLookups(this);
		}

		protected override CAAddInfoValidation GetNewValidation()
		{
			CAAddInfoValidation result;
			switch (Parent.JE_MessageType)
			{
				case JobMessageTypeList.Codes.Export:
					result = new ExportAddInfoJobDeclarationValidation(this);
					break;
				case JobMessageTypeList.Codes.Import:
				case JobMessageTypeList.Codes.LowValueShipments:
				case JobMessageTypeList.Codes.LVSForConsolidation:
					result = new ImportAddInfoJobDeclarationValidation(this);
					break;
				case JobMessageTypeList.Codes.B2Adjustments:
				case JobMessageTypeList.Codes.ImportCopyforB2:
				case JobMessageTypeList.Codes.XTypeEntry:
					result = new B2CommonAddInfoJobDeclarationValidation(this);
					break;
				default:
					result = new AddInfoJobDeclarationValidation(this);
					break;
			}
			return result;
		}
		#endregion
	}
}
