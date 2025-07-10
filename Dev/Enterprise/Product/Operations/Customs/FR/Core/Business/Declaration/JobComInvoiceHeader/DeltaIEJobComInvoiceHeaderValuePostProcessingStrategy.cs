using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaIEJobComInvoiceHeaderValuePostProcessingStrategy : JobComInvoiceHeaderValuePostProcessingStrategy, IValuePostProcessingStrategy
	{
		public DeltaIEJobComInvoiceHeaderValuePostProcessingStrategy(JobComInvoiceHeader jobComInvoiceHeader) : base(jobComInvoiceHeader)
		{
		}

		public void ValuePostProcess(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			base.ValuePostProcessCore(valueThatHasChanged, oldValue);

			if (valueThatHasChanged.Name == JobComInvoiceHeader.Schema.JZ_IncoTerm)
			{
				base.UpdateGroupCharges();
			}
		}
	}
}
