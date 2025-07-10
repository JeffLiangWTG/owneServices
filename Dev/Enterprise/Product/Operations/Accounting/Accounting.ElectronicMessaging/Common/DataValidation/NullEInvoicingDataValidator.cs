using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Common.DataValidation
{
	/// <summary>
	/// Validator without validation rules.
	/// </summary>
	/// <remarks>
	/// If you wish to introduce validation to your eInvoicing service task, follow the conventions in Spain and Taiwan.
	/// </remarks>
	public sealed class NullEInvoicingDataValidator : BaseEInvoicingDataValidator
	{
		public NullEInvoicingDataValidator(GlbCompany company) : base(company)
		{
		}

		protected override void RunCore(ILogger logger)
		{
			// No-op. This is the Null data validator, after all!!
		}
	}
}
