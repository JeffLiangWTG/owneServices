using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.Common;

public class CusOtherLawReferenceValidation(CusOtherLawReference parent) : JPCusReferenceValidation(parent)
{
	public new CusOtherLawReference Parent => (CusOtherLawReference)base.Parent;

	protected override void CheckCFR_Reference()
	{
		base.CheckCFR_Reference();
		CheckCFR_Reference_Common();
		switch (Parent.CurrentOtherLawType)
		{
			case CusOtherLawReference.OtherLawType.RoadTranportVehicleLaw:
				CheckCFR_Reference_RoadTranportVehicleLaw();
				break;
		}
	}

	void CheckCFR_Reference_Common()
	{
		if (Parent.Parent is ICusOtherLawReferenceParent cusOtherLawReferenceParent && cusOtherLawReferenceParent.IsOtherLawReferenceRequired)
		{
			var propertyInfo = Parent.CFR_ReferenceInfo;

			ListValidation.MessageErrorIfInvalidCode(propertyInfo, ResString.GetMultilingualString("877056A2-13B0-4AFE-9180-05782D8E0F7A", "The selected value is invalid."));

			var parentCollections = ((IBusinessObjectInternals)Parent).ParentCollections.FirstOrDefault();
			if (parentCollections != null && parentCollections.Any(x => x is AutoCusReference info && info != Parent && info.CFR_Reference == Parent.CFR_Reference))
			{
				propertyInfo.AddMessageError(Res.GetString("6A4CD3FA-C014-4AB0-AD29-6DF4BA8F07BA", "The same information has been entered."));
			}
		}
	}

	protected virtual void CheckCFR_Reference_RoadTranportVehicleLaw()
	{
	}
}
