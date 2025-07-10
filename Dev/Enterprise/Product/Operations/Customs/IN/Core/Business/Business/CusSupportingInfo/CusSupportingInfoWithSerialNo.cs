using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IN.Business;

public abstract class CusSupportingInfoWithSerialNo : AutoINCusSupportingInfo, IHugeSequenceNumberLine
{
	public CusSupportingInfoWithSerialNo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	HugeSequenceNumberGenerator GetSequenceNumberGenerator(BusinessObject parent) => (parent as ICusSupportingInfoWithSerialNoParent)?.GetSequenceNumberGenerator(CSI_Type);

	HugeSequenceNumberGenerator SequenceNumberGenerator => GetSequenceNumberGenerator(Parent);

	[ResourceStringData("Enterprise.Customs.IN.Business.DfiaImportItemDetail|CSI_LineNo", Caption = "Serial Number", MediumCaption = "Serial No.", ShortCaption = "Sr. No.")]
	public override ZInt CSI_LineNo
	{
		get { return base.CSI_LineNo; }
		set
		{
			if (value > 0)
			{
				var oldValue = CSI_LineNo;
				base.CSI_LineNo = value;
				if (oldValue != value && !IsCopying)
				{
					SequenceNumberGenerator?.RecalculateWhenRenumbered(this, oldValue);
				}
			}
		}
	}

	ZGuid ISequenceNumberLine.FKToHeader => CSI_ParentID;

	ZInt ISequenceNumberLine<ZInt>.SequenceNumber { get => CSI_LineNo; set => CSI_LineNo = value; }

	public override ZGuid CSI_ParentID
	{
		get => base.CSI_ParentID;
		set
		{
			var oldParent = Parent;
			var oldValue = CSI_ParentID;
			base.CSI_ParentID = value;
			if (!IsCopying && oldValue != value)
			{
				SetLineNoOnSettingParent(oldParent);
			}
		}
	}

	public override ZString CSI_ParentTableCode
	{
		get => base.CSI_ParentTableCode;
		set
		{
			var oldParent = Parent;
			var oldValue = CSI_ParentTableCode;
			base.CSI_ParentTableCode = value;
			if (!IsCopying && oldValue != value)
			{
				SetLineNoOnSettingParent(oldParent);
			}
		}
	}

	void SetLineNoOnSettingParent(BusinessObject oldParent)
	{
		GetSequenceNumberGenerator(oldParent)?.RecalculateWhenAboutToBeDetachedOrDeleted(this);
		SequenceNumberGenerator?.RecalculateWhenAdded(this);
	}

	public override void Delete()
	{
		SequenceNumberGenerator?.RecalculateWhenAboutToBeDetachedOrDeleted(this);
		base.Delete();
	}
}
