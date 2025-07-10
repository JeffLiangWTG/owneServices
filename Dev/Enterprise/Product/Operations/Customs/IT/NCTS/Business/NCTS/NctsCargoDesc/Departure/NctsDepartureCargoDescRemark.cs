using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsDepartureCargoDescRemark
{
	public NctsDepartureCargoDescRemark(NctsDepartureCargoDesc goodsItem)
	{
		this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
	}
	readonly NctsDepartureCargoDesc goodsItem;

	public ZString Remarks
	{
		get => NctsDepartureCargoDescRemarksCollection.Any() ? NctsDepartureCargoDescRemarksCollection[0].CSI_Description : ZString.Empty;
		set => RemoveIfEmptyOrSetDescription(value);
	}

	#region Implementation

	NctsDepartureCargoDescRemarksCollection NctsDepartureCargoDescRemarksCollection => nctsDepartureCargoDescRemarksCollection ?? (nctsDepartureCargoDescRemarksCollection = LoadNctsDepartureCargoDescRemarksCollection());
	NctsDepartureCargoDescRemarksCollection nctsDepartureCargoDescRemarksCollection;

	NctsDepartureCargoDescRemarksCollection LoadNctsDepartureCargoDescRemarksCollection()
	{
		var result = new NctsDepartureCargoDescRemarksCollection(goodsItem);
		result.Load();
		goodsItem.RegisterEditableChildObject(result);
		return result;
	}

	void RemoveIfEmptyOrSetDescription(ZString value)
	{
		if (value.IsEmpty)
		{
			NctsDepartureCargoDescRemarksCollection.RemoveAndDeleteAll();
		}
		else
		{
			var remarksSupportingInfo = NctsDepartureCargoDescRemarksCollection.Any()
				? NctsDepartureCargoDescRemarksCollection[0]
				: NctsDepartureCargoDescRemarksCollection.AddNew();

			remarksSupportingInfo.CSI_Description = value;
		}
	}

	#endregion
}
