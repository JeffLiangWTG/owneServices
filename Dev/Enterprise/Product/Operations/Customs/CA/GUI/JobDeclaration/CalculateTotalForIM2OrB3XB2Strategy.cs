using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.CA.GUI
{
	class CalculateTotalForIM2OrB3XB2Strategy : PreSaveDialogStrategy
	{
		public CalculateTotalForIM2OrB3XB2Strategy(JobDeclaration source)
		{
			this.source = source;
		}
		readonly JobDeclaration source;

		#region Override

		protected override bool ShouldRunPreSaveAction()
		{
			return source != null
				&& (source.IsIM2 && source.PreviousJob != null || source.IsB2Adjustments || source.IsB3X);
		}

		protected override ContinueWithSave RunPreSaveAction()
		{
			source.AddFetchsHintForPopulateDutiesAndTaxesIfNeeded();

			if (source.IsIM2)
			{
				source.CalculateIM2Total();
			}
			else if (source.IsB2Adjustments || source.IsB3X)
			{
				source.CalculateB2Total();
			}
			return ContinueWithSave.Yes;
		}

		#endregion
	}
}
