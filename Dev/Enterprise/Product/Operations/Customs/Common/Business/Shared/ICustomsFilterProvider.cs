using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Common
{
	/// <summary>
	/// Query helpers that may be used for filters/queries etc.
	/// </summary>
	public interface ICustomsFilterProvider
	{
		/// <summary>
		/// Returns a DBOnlySubQuery filter starting at JobDeclaration attached to JobShipment
		/// Allows additional filtering to be added at the invoice line level
		/// </summary>
		/// <param name="additionalInvoiceLineFilter"></param>
		/// <returns></returns>
		ZDBOnlySubQuery GetJobDeclarationFromCommercialInvoiceQueryWithLineFilterAttachedToJobShipment(ZQuery additionalInvoiceLineFilter);

		/// <summary>
		/// Returns a DBOnlySubQuery filter starting at JobDeclaration attached to JobDeclaration
		/// Allows additional filtering to be added at the invoice line level
		/// </summary>
		/// <param name="additionalInvoiceLineFilter"></param>
		/// <returns></returns>
		ZDBOnlySubQuery GetJobDeclarationFromCommercialInvoiceQueryWithLineFilterAttachedToJobDeclaration(ZQuery additionalInvoiceLineFilter);

		/// <summary>
		/// Returns a DBOnlySubQuery filter starting at JobComInvoiceHeader attached to JobDeclaration.
		/// Allows additional filtering to be added at the invoice line level.
		/// </summary>
		/// <param name="additionalInvoiceLineFilter"></param>
		/// <returns></returns>
		ZDBOnlySubQuery GetJobComInvoiceHeaderFromCommercialInvoiceQueryWithLineFilterAttachedToJobDeclaration(ZQuery additionalInvoiceLineFilter);

		/// <summary>
		/// Returns a DBOnlySubQuery filter starting at JobComInvoiceLine attached to JobComInvoiceHeader.
		/// Allows additional filtering to be added at the invoice line level.
		/// </summary>
		/// <param name="additionalInvoiceLineFilter"></param>
		/// <returns></returns>
		ZDBOnlySubQuery GetJobCommercialInvoiceQueryWithLineFilterAttachedToJobComInvoiceHeader(ZQuery additionalInvoiceLineFilter);

		/// <summary>
		/// Returns a ZDBOnlyQuery filter starting at JobComInvoiceLine.
		/// Allows additional filtering on ProductCode at the invoice line level.
		/// </summary>
		/// <param name="comparisonOperator"></param>
		/// <param name="lineProductCode"></param>
		/// <returns></returns>
		ZDBOnlyQuery GetInvoiceLineProductCodeQuery(SQLComparisonOperator comparisonOperator, ZString lineProductCode);

		ZDBOnlyQuery GetJobDeclarationFromEntryNumber(SQLComparisonOperator comparisonOperator, ZString value);
	}
}
