using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocARComplianceDocumentLine : DocBaseWrapper
	{
		#region Constructor

		protected DocARComplianceDocumentLine(AccComplianceDocumentLine line, BusinessObjectFactory factory)
			: base(line, factory)
		{
		}

		public static DocARComplianceDocumentLine New(AccComplianceDocumentLine line, BusinessObjectFactory factory)
		{
			if (line == null)
			{
				return null;
			}

			return new DocARComplianceDocumentLine(line, factory);
		}

		#endregion

		public AccComplianceDocumentLine AccComplianceDocumentLine
		{
			get { return (AccComplianceDocumentLine)WrappedObject; }
		}

		public ZString Description => AccComplianceDocumentLine.ADL_Description;

		public ZDecimal Amount => AccComplianceDocumentLine.Amount;

		public ZString INVDocumentDateYearForCRD => AccComplianceDocumentLine.ComplianceDocumentHeader?.INVComplianceDocumentHeaderForCRD?.ADH_DocumentDate.Year.ToString(CultureInfo.InvariantCulture) ?? ZString.Empty;

		public ZString INVDocumentDateMonthForCRD => AccComplianceDocumentLine.ComplianceDocumentHeader?.INVComplianceDocumentHeaderForCRD?.ADH_DocumentDate.Month.ToString(CultureInfo.InvariantCulture) ?? ZString.Empty;

		public ZString INVDocumentDateDayForCRD => AccComplianceDocumentLine.ComplianceDocumentHeader?.INVComplianceDocumentHeaderForCRD?.ADH_DocumentDate.Day.ToString(CultureInfo.InvariantCulture) ?? ZString.Empty;

		public ZString DocumentNumberPrefix => AccComplianceDocumentLine.ComplianceDocumentHeader?.ADH_DocumentNumber.Left(2) ?? ZString.Empty;

		public ZString DocumentNumberSuffix => AccComplianceDocumentLine.ComplianceDocumentHeader?.ADH_DocumentNumber.Right(8) ?? ZString.Empty;

		public ZDecimal LocalAmount => AccComplianceDocumentLine.LocalAmount;

		public ZDecimal LocalTaxAmount => AccComplianceDocumentLine.LocalTaxAmount;

		public ZBool Taxable => AccComplianceDocumentLine.TaxRate != null && (AccComplianceDocumentLine.TaxRate.AT_Code == "VAT" || AccComplianceDocumentLine.TaxRate.AT_Code == "CAPVAT");

		public ZBool ZeroRated => AccComplianceDocumentLine.TaxRate != null && AccComplianceDocumentLine.TaxRate.AT_Code == "FREEVAT";

		public ZBool TaxExempt => AccComplianceDocumentLine.TaxRate != null && AccComplianceDocumentLine.TaxRate.AT_Code == "EXEMPT";

		public ZInt Sequence => AccComplianceDocumentLine.ADL_Sequence;
	}
}
