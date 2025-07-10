using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class ChargeCodeGuidFindBoxRegistryItemEditor : GuidFindBoxRegistryItemEditor
	{
		public ChargeCodeGuidFindBoxRegistryItemEditor(IRegistryDataType dataType, IRegistryEditorInfo editorInfo, FallbackLevel fallback) : base(dataType, editorInfo, fallback)
		{
		}

		public override string GetCustomValidation(Control editorPane)
		{
			string errorMessage = base.GetCustomValidation(editorPane);
			if (string.IsNullOrEmpty(errorMessage))
			{
				var companyPK = Fallback.CompanyPK(false);

				var customsQuarantineChargeCode = Business.RatingDataRegistry.Instance.CustomsQuarantineChargeCode.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);

				if (customsQuarantineChargeCode.ChargeCode.IsValid)
				{
					var currentData = (Guid)base.GetValueFromEditorPaneCore(editorPane);

					if (customsQuarantineChargeCode.ChargeCode == currentData)
					{
						errorMessage = Res.GetString("13C68D69-0DDE-4582-9F93-A6825DF713BF", "The value should not be same to \"Default Quarantine Charge Code\".");
					}
				}
			}

			return errorMessage;
		}
	}
}
