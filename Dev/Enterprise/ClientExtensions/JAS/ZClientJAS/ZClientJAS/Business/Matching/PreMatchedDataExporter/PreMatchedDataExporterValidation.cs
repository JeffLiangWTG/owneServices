using System;

using CargoWise.EntityFramework;

namespace Enterprise.Client.JAS.Business.Matching
{
	public class PreMatchedDataExporterValidation : JASDataExporterBizOValidation
	{
		public PreMatchedDataExporterValidation(PreMatchedDataExporter parent)
			: base(parent)
		{
		}

		public override Type AutoValidationType
		{
			get { return typeof(PreMatchedDataExporterValidation); }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateNettingCycle();
		}

		public void ValidateNettingCycle()
		{
			ValidateCalculatedProperty(Parent.NettingCycleInfo);
		}

		protected void CheckNettingCycle()
		{
			MandatoryValidation.CheckEntered(Parent.NettingCycleInfo);
			ListValidation.ErrorIfInvalidCode(Parent.NettingCycleInfo, Parent.NettingCycleList);
		}

		public new PreMatchedDataExporter Parent
		{
			get { return (PreMatchedDataExporter)base.Parent; }
		}
	}
}
