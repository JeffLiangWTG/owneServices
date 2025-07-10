using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusOutturnValidation : BaseCusOutturnValidation
	{
		public CusOutturnValidation(CusOutturn outturn)
			: base(outturn)
		{
		}

		protected override void CheckC5_HouseBill()
		{
			base.CheckC5_HouseBill();
			if (Outturn.Underbond != null && Outturn.Parent == null)
			{
				ZQuery filter = new ZQuery(CusOutturnSchema.C5_HouseBill, Outturn.C5_HouseBill);
				foreach (CusOutturn otherOutturn in Outturn.Underbond.Outturns.Find(filter))
				{
					if (otherOutturn.PK != Outturn.PK)
					{
						Outturn.C5_HouseBillInfo.AddMessageError("Outturn Line with this Housebill value already exists.");
					}
				}
			}
		}

		protected override void CheckC5_LastMessageDate()
		{
			base.CheckC5_LastMessageDate();
			if (Outturn.C5_LastMessageDate.IsEmpty)
			{
				var underbond = Outturn.Underbond;
				if (underbond != null && underbond.IsLastMessageDateSupported &&
							(underbond.OutturnStatus.Code == CMRBaseStatuses.Codes.OriginalAccepted ||
							 underbond.OutturnStatus.Code == CMRBaseStatuses.Codes.AmendmentAccepted ||
							 underbond.OutturnStatus.Code == CMRBaseStatuses.Codes.AmendmentRejected))
				{
					Parent.C5_LastMessageDateInfo.AddWarning("This outturn line has not been sent, or was rejected by Customs");
				}
			}
		}

		protected override void CheckC5_OutturnResultType()
		{
			base.CheckC5_OutturnResultType();
			if (Parent.C5_OutturnResultType.IsEmpty)
			{
				Parent.C5_OutturnResultTypeInfo.AddError("Outturn result type is mandatory.");
			}

			ListValidation.ErrorIfInvalidCode(Parent.C5_OutturnResultTypeInfo, Parent.Lookups.OutturnResultTypeList);
			ValidateC5_GoodsDescription();
		}

		protected override void CheckC5_PackagesOutturned()
		{
			base.CheckC5_PackagesOutturned();
			if (Parent.C5_PackagesOutturned < 0)
			{
				Parent.C5_PackagesOutturnedInfo.AddMessageError("Amount may not be less than zero");
			}
		}

		protected override void CheckParentStringRepresentation()
		{
			base.CheckParentStringRepresentation();
			IOutturnableLine parent = Outturn.Parent;
			if (parent == null && Outturn.ParentStringRepresentation != Outturn.C5_HouseBill && !Outturn.C5_HouseBill.IsEmpty)
			{
				Outturn.ParentStringRepresentationInfo.AddError("Outturn Line Parent can't be empty");
			}
			else if (Outturn.Underbond != null)
			{
				if (parent != null)
				{
					ZQuery filter = new ZQuery(CusOutturnSchema.C5_ParentID, parent.LinkPK);
					foreach (CusOutturn otherOutturn in Outturn.Underbond.Outturns.Find(filter))
					{
						if (otherOutturn != Outturn)
						{
							Outturn.ParentStringRepresentationInfo.AddError("Outturn Line Parent already exists");
							break;
						}
					}
				}
				if (Outturn.Underbond.LinkedObject != null && Array.IndexOf(Outturn.Underbond.LinkedObject.OutturnableLines, Outturn.Parent) == -1)
				{
					Outturn.ParentStringRepresentationInfo.AddError("This line is not valid for outturning");
				}
			}
		}
	}
}
