using System.Linq;
using Enterprise.Core;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public class ExitControlAdditionalInfoValidation : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoValidation
	{
		public ExitControlAdditionalInfoValidation(ExitControlAdditionalInfo parent)
			: base(parent)
		{
		}

		protected new ExitControlAdditionalInfo Parent => (ExitControlAdditionalInfo)base.Parent;

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();

			if (!Parent.CSI_Code.IsEmpty)
			{
				ValidateCSI_Code_Unique();
				ValidateConsignmentMRNForCodeX1002();
			}
		}

		void ValidateCSI_Code_Unique()
		{
			var parent = Parent;
			var currentCode = parent.CSI_Code;
			var additionalInfos = parent.ExitConsignment.AdditionalInfos;
			if (additionalInfos.Cast<ExitControlAdditionalInfo>().Any(x => x.CSI_Code == currentCode && x.PK != parent.PK))
			{
				parent.CSI_CodeInfo.AddMessageError(Res.GetString("5FEE6E5D-14A7-4C92-8D2F-E543E4DFB586", "An Additional Information of Type {0} has already been entered.", currentCode));
			}
		}

		void ValidateConsignmentMRNForCodeX1002()
		{
			var parent = Parent;
			var consignmentMRN = parent.ExitConsignment.CXC_MovementReference;
			if (!consignmentMRN.IsEmpty && parent.CSI_Code == UniversalReferenceConstants.RefCusCodeList.Codes.X1002 && consignmentMRN.SubstringSafe(2, 2) != Constants.CountryCodes.Germany)
			{
				parent.CSI_CodeInfo.AddMessageError(Res.GetString("495C871A-15AE-4B1C-9EA5-7056F645B0FB", "The selected Full Type may not be entered, if the MRN does not contain 'DE' as digit 3 and 4."));
			}
		}
		protected override bool ShouldCodeBeInTheList => true;
	}
}
