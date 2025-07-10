using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public class CharteraOutputDocumentSearchSendingObjectValidation : AutoCharteraOutputDocumentSearchSendingObjectValidation
{
	public CharteraOutputDocumentSearchSendingObjectValidation(AutoCharteraOutputDocumentSearchSendingObject parent) : base(parent)
	{
	}

	protected override void CheckCreationTimeFrom()
	{
		base.CheckCreationTimeFrom();
		MandatoryValidation.CheckEntered(Parent.CreationTimeFromInfo);

		if (Parent.CreationTimeFrom.IsValid && Parent.CreationTimeTo.IsValid)
		{
			if (Parent.CreationTimeFrom >= Parent.CreationTimeTo)
			{
				Parent.CreationTimeFromInfo.AddError(Res.GetString("01276543-A3D0-4044-9C6D-BACBBCE49F70", "Creation Time From should be earlier than Creation Time To."));
			}
			else if (Parent.CreationTimeFrom.AddDays(1) < Parent.CreationTimeTo)
			{
				Parent.CreationTimeFromInfo.AddError(Res.GetString("3A04C52B-6382-41D2-9C72-4F6F63988CD2", "The time range cannot exceed 24 hours."));
			}
		}
	}

	protected override void CheckCreationTimeTo()
	{
		base.CheckCreationTimeTo();
		MandatoryValidation.CheckEntered(Parent.CreationTimeToInfo, Parent.CreationTimeFromInfo.HumanReadableName);
	}

	protected override void CheckIsHistoricalQuery()
	{
		base.CheckIsHistoricalQuery();

		var oneMonthAgo = ZDateTime.Now.AddMonths(-1);

		if (Parent.IsHistoricalQuery)
		{
			if (Parent.CreationTimeTo > oneMonthAgo)
			{
				Parent.IsHistoricalQueryInfo.AddError(Res.GetString("65C09467-45F4-498E-AE27-731E8E966ED1", "Creation Time To should be earlier than 1 month ago if Historical Query is ticked."));
			}
		}
		else
		{
			if (Parent.CreationTimeFrom <= oneMonthAgo)
			{
				Parent.IsHistoricalQueryInfo.AddError(Res.GetString("3BC54D4E-87BD-4953-9311-FE9EABE949F0", "Please tick Historical Query to request documents created earlier than 1 month ago."));
			}
		}
	}
}
