using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Internal;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI;

interface IManualLocationOfGoodsCodeFindBox : IFindBox
{
	public object DataSource { get; }
}
public sealed class ManualLocationOfGoodsCodeFindBox : ZCodeFindBox, IManualLocationOfGoodsCodeFindBox
{
	object IManualLocationOfGoodsCodeFindBox.DataSource  => base.DataSource;

	[Browsable(true), DefaultValue("")]
	public string PremisesLocationProperty { get; set; }

	protected override string GetDescription()
	{
		var retrunValue = base.Description;
		if (DataSource is TemporaryStorageHeader temporaryStorageHeader && !temporaryStorageHeader.ManualDestinationCustomsOffice.IsEmpty)
		{
			retrunValue = IntoTemporaryStorageHelper.GetFormattedManualLocationOfGoodsDescription(base.Code);
		}

		return retrunValue;
	}

	protected override IModuleDecisionProvider GetModuleDecisionProvider(ZFilterModule module)
	{
		return new ManualLocationOfGoodsPopupModuleDecisionProvider(this);
	}
}

class ManualLocationOfGoodsPopupModuleDecisionProvider : PopupModuleDecisionProvider
{
	public ManualLocationOfGoodsPopupModuleDecisionProvider(IFindBox findBox) : base(findBox)
	{
	}

	public override void SetFindBoxCodeDescription(BusinessObject bizo)
	{
		var findBox = FindBox as IManualLocationOfGoodsCodeFindBox;

		if (findBox.DataSource is TemporaryStorageHeader temporaryStorageHeader && bizo is EU.TemporaryStorage.Business.CusTempStorageRegPremises premises)
		{
			FindBox.Code = premises.SRP_CustomsLocation;
			FindBox.Description = IntoTemporaryStorageHelper.GetFormattedManualLocationOfGoodsDescription(premises.SRP_CustomsLocation);
		}
	}
}
