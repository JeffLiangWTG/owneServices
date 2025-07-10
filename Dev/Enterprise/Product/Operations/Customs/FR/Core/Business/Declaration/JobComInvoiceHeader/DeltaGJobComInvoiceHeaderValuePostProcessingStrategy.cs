using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaGJobComInvoiceHeaderValuePostProcessingStrategy : JobComInvoiceHeaderValuePostProcessingStrategy, IValuePostProcessingStrategy
	{
		readonly JobComInvoiceHeader invoiceHeader;

		public DeltaGJobComInvoiceHeaderValuePostProcessingStrategy(JobComInvoiceHeader jobComInvoiceHeader) : base(jobComInvoiceHeader)
		{
			invoiceHeader = jobComInvoiceHeader;
		}

		public void ValuePostProcess(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			base.ValuePostProcessCore(valueThatHasChanged, oldValue);

			if (valueThatHasChanged.Name == JobComInvoiceHeader.Schema.JZ_IncoTerm)
			{
				invoiceHeader.ShouldClearIncoTermPlacesIfNeeded = true;
				invoiceHeader.JZ_IncoTermPlace = ZString.Empty;

				var defaultAgreedPlaceCode = invoiceHeader.JobDeclaration?.GetNumericIncoTermModeCodeIncoTermAndFlux(invoiceHeader.JZ_IncoTerm) ?? ZString.Empty;
				if (defaultAgreedPlaceCode != invoiceHeader.ZG_AgreedPlaceCode)
				{
					// Changes to ZG_AgreedPlaceCode will update the group charges
					invoiceHeader.ZG_AgreedPlaceCode = defaultAgreedPlaceCode;
				}
				else
				{
					base.UpdateGroupCharges();
				}
			}
		}
	}
}
