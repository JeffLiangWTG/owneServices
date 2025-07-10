
using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations
{
	public class JXCExportAWBRateLineValidation : Freight.Forwarding.AWB.Business.AutoExportAWBRateLineValidation
	{
		public JXCExportAWBRateLineValidation(ExportAWBRateLine aWBRateLine)
			: base(aWBRateLine)
		{
		}

		#region Overrides

		protected override void CheckER_NoOfPiecesOrRCP()
		{
			if (!Parent.IsRateDescriptionEmpty)
			{
				ValidationHelper.AddJXCWarningIfNotEntered(Parent.ER_NoOfPiecesOrRCPInfo);
			}
		}

		protected override void CheckER_WeightInLBsOrKGs()
		{
			if (!Parent.IsRateDescriptionEmpty)
			{
				ValidationHelper.ValidateRegexField(Parent.ER_WeightInLBsOrKGsInfo, JXCConstants.FBDNFieldBoundaries.WeightUnit, "Invalid Weight Unit");
			}
		}

		protected override void CheckER_RateClass()
		{
			if (!Parent.IsRateDescriptionEmpty)
			{
				string warningMessage = string.Concat("Invalid ", Parent.ER_RateClassInfo.HumanReadableName);
				ValidationHelper.ValidateRegexField(Parent.ER_RateClassInfo, JXCConstants.FBDNFieldBoundaries.RateClassCode, warningMessage);
			}
		}

		#endregion

		protected new ExportAWBRateLine Parent
		{
			get { return (ExportAWBRateLine)base.Parent; }
		}

		ValidationHelper ValidationHelper
		{
			get
			{
				if (fValidationHelper == null)
				{
					fValidationHelper = new ValidationHelper();
				}
				return fValidationHelper;
			}
		}

		ValidationHelper fValidationHelper;
	}
}
