using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Internal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.GUI
{
	interface IExTariffFindBox : IFindBox
	{
		string TariffCodeProperty { get; }
		string TariffTypeProperty { get; }
		string DataPropertyName { get; }
		object CurrentItem { get; }
	}

	class ExTariffGridFindBox : ZGridFindBox, IExTariffFindBox, ICustomizableFindBoxPopup
	{
		protected override IModuleDecisionProvider GetModuleDecisionProvider(ZFilterModule module)
		{
			return new ExTariffPopupModuleDecisionProvider(this);
		}

		[Browsable(true), DefaultValue("")]
		public string TariffCodeProperty { get; set; }

		[Browsable(true), DefaultValue("")]
		public string TariffTypeProperty { get; set; }

		string ICustomizableFindBoxPopup.CodeForPopup => (CurrentItem as BusinessObject)?[TariffCodeProperty].ToString() ?? Code;

		string ICustomizableFindBoxPopup.PropertyNameForPopup => TariffViewSchema.Constants.ZZ1_TariffCode;

		string IExTariffFindBox.DataPropertyName => DataPropertyName;

		object IExTariffFindBox.CurrentItem => CurrentItem;

		public override ModuleIdentifier ModuleID => Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.RefCusTariff;
	}

	class ExTariffPopupModuleDecisionProvider : PopupModuleDecisionProvider
	{
		public ExTariffPopupModuleDecisionProvider(IExTariffFindBox findBox) : base(findBox)
		{
		}

		public override void SetFindBoxCodeDescription(BusinessObject bizo)
		{
			var findBox = FindBox as IExTariffFindBox;

			if (findBox.CurrentItem is BusinessObject obj && bizo is TariffView tariffView)
			{
				if (!string.IsNullOrEmpty(findBox.TariffCodeProperty))
				{
					obj[findBox.TariffCodeProperty] = tariffView.ZZ1_TariffCode;
				}
				if (!string.IsNullOrEmpty(findBox.TariffTypeProperty))
				{
					obj[findBox.TariffTypeProperty] = tariffView.ZZ1_ZZI_NKTariffType;
				}

				FindBox.Code = obj[findBox.DataPropertyName].ToString();
				FindBox.Description = tariffView.ZZ1_Description.ToString();
			}
		}
	}
}
