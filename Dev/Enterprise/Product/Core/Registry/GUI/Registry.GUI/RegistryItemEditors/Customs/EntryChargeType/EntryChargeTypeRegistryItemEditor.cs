using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI.Customs
{
	public class EntryChargeTypeRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public EntryChargeTypeRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		public override string GetCustomValidation(Control editorPane)
		{
			string errorMessage = base.GetCustomValidation(editorPane);

			if (string.IsNullOrEmpty(errorMessage))
			{
				var companyPK = Level.CompanyPK(false);

				if (string.IsNullOrEmpty(errorMessage))
				{
					var customsQuarantineChargeCode = Business.RatingDataRegistry.Instance.CustomsQuarantineChargeCode.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
					if (customsQuarantineChargeCode.ChargeCode.IsValid)
					{
						var currentData = (EntryChargeTypeSettingCollection)base.GetValueFromEditorPaneCore(editorPane);

						if (currentData != null && currentData.Cast<Business.Customs.EntryChargeTypeSetting>().Any(x => x.AC_ChargeCode == customsQuarantineChargeCode.ChargeCode))
						{
							errorMessage = Res.GetString("13C68D69-0DDE-4582-9F93-A6825DF713BF", "The value should not be same to \"Default Quarantine Charge Code\".");
						}
					}
				}
			}

			return errorMessage;
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new EntryChargeTypeControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
