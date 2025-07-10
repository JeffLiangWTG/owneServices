using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	partial class InvoicingLineBase
	{
		[ResourceStringData("InvoicingLineBase|JobConsolXMLData", Caption = "Job XML Data")]
		public ZString JobConsolXMLData
		{
			get
			{
				var result = ZString.Empty;
				var universalLine = GetUniversalTransactionLine();
				if (universalLine != null)
				{
					result = universalLine.JobConsolXMLData;
				}

				return result;
			}
		}

		public ZString UXml_JobErrorMessages
		{
			get { return uXml_JobErrorMessages; }
			set
			{
				uXml_JobErrorMessages = value;

				Validation.ValidateAL_JH();
			}
		}
		ZString uXml_JobErrorMessages;

		public ZString UXml_ConsolErrorMessages
		{
			get { return uXml_ConsolErrorMessages; }
			set
			{
				uXml_ConsolErrorMessages = value;

				var lineValidation = Validation as InvoicingLineBaseValidation;
				if (lineValidation != null)
				{
					lineValidation.ValidateConsolIDFromApportionedCharge();
				}
			}
		}
		ZString uXml_ConsolErrorMessages;

		[ResourceStringData("InvoicingLineBase|ImportedChargeCode", Caption = "Default Imported Charge Code", ShortCaption = "Default Charge", FullDescription = "Charge code mapped to local system code by default (local code).")]
		public ZString ImportedChargeCode
		{
			get
			{
				var result = ZString.Empty;
				var universalLine = GetUniversalTransactionLine();
				if (universalLine != null)
				{
					result = universalLine.ChargeCode;
				}

				return result;
			}
		}

		public ZPropertyInfo ImportedChargeCodeInfo => GetZPropertyInfo(nameof(ImportedChargeCode));

		[ResourceStringData("InvoicingLineBase|ImportedChargeCodeXmlCode", Caption = "Imported XML Charge Code", ShortCaption = "XML Charge", FullDescription = "Charge code in imported XML (foreign code).")]
		public ZString ImportedChargeCodeXmlCode
		{
			get
			{
				var result = ZString.Empty;
				var universalLine = GetUniversalTransactionLine();
				if (universalLine != null)
				{
					result = universalLine.ChargeCodeSource;
				}

				return result;
			}
		}

		public ZPropertyInfo ImportedChargeCodeXmlCodeInfo => GetZPropertyInfo(nameof(ImportedChargeCodeXmlCode));

		public int IndexOfImportedUniversalTransactionLine
		{
			get { return indexOfImportedUniversalTransactionLine; }
			set { indexOfImportedUniversalTransactionLine = value; }
		}
		int indexOfImportedUniversalTransactionLine = -1;

		internal bool ShouldCreateChargeCodeMatchingRule => !ImportedChargeCodeXmlCode.IsEmpty && ChargeCode != null && ChargeCode.AC_Code != ImportedChargeCode && AccountingConfigurationRegistry.Instance.EnableAutomaticChargeCodeMappingForUnallocatedInvoices.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

		UniversalTransactionLineWrapper GetUniversalTransactionLine()
		{
			UniversalTransactionLineWrapper result = null;
			if (IndexOfImportedUniversalTransactionLine != -1)
			{
				var universalTransaction = InvoiceBase?.GetUniversalTransaction();
				if (universalTransaction != null)
				{
					result = universalTransaction.Lines[IndexOfImportedUniversalTransactionLine];
					result.UpdateMappedCodes(this);
				}
			}

			return result;
		}
	}
}
