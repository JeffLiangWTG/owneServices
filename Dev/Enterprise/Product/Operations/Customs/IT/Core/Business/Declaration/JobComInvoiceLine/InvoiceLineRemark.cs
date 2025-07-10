using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public class InvoiceLineRemark
{
	public InvoiceLineRemark(JobComInvoiceLine parent)
	{
		this.parent = Argument.NotNull(parent, nameof(parent));
	}
	readonly JobComInvoiceLine parent;

	public ZString Remarks
	{
		get => RemarksInfoCollection.Any() ? RemarksInfoCollection[0].CSI_Description : ZString.Empty;
		set => RemoveIfEmptyOrSetDescription(value);
	}

	#region Implementation

	RemarksInfoCollection RemarksInfoCollection => remarksInfoCollection ?? (remarksInfoCollection = LoadInvoiceLineRemarksCollection());
	RemarksInfoCollection remarksInfoCollection;

	RemarksInfoCollection LoadInvoiceLineRemarksCollection()
	{
		var result = new RemarksInfoCollection(parent);
		result.Load();
		parent.RegisterEditableChildObject(result);
		return result;
	}

	void RemoveIfEmptyOrSetDescription(ZString value)
	{
		if (value.IsEmpty)
		{
			RemarksInfoCollection.RemoveAndDeleteAll();
		}
		else
		{
			var remarksSupportingInfo = RemarksInfoCollection.Any()
				? RemarksInfoCollection[0]
				: RemarksInfoCollection.AddNew();

			remarksSupportingInfo.CSI_Description = value;
		}
	}

	#endregion
}
