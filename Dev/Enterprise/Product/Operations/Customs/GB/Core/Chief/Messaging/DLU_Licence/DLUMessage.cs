using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Chief.Messaging.DLU
{
	public class DLUMessage : CusPermitEnquiryMessage, IResetToQueuedStatusSupporter
	{
		public new class Schema : EDIMessage.Schema
		{
			public const string OrgPK = "OrgPK";
		}

		public DLUMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void OnSaving()
		{
			base.OnSaving();
			EM_MessageText = GbTransmissionMessageGenerator.PutBizoPkIntoSysCarPlaceholder(EM_MessageText, this);
		}

		public override void ResetToQueuedStatus()
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = ChiefConstants.CusDecTypeDLU;
		}

		public override void Delete()
		{
			GenAddOn.Delete();
			base.Delete();
		}

		public ZString OrgCode
		{
			get
			{
				if (orgCode.IsEmpty)
				{
					var org = Factory.Load<OrgHeader>(OrgPK);
					if (org != null)
					{
						orgCode = org.OH_Code;
					}
				}
				return orgCode;
			}
		}
		ZString orgCode;

		public ZGuid OrgPK
		{
			get { return new ZGuid(GenAddOn.XA_Data); }
			set
			{
				if (value.IsEmpty)
				{
					if (cachedAddOn != null && cachedAddOn.Value != null)
					{
						GenAddOn.Delete();
					}
				}
				else
				{
					GenAddOn.XA_Data = value.ToString();
				}
				OrgPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo OrgPKInfo
		{
			get { return GetZPropertyInfo(Schema.OrgPK); }
		}

		GenAddOnColumn GenAddOn => Factory.GetValue(ref cachedAddOn, () => FindOrMakeNewAddOn());

		CachedProperty<GenAddOnColumn> cachedAddOn;

		GenAddOnColumn FindOrMakeNewAddOn()
		{
			var addOnStatusQuery = new ZQuery(GenAddOnColumnSchema.XA_ParentID, PK);
			addOnStatusQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, EDIMessageSchema.Constants.Prefix);
			addOnStatusQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, ChiefConstants.CusDecTypeDLU);
			var result = Factory.LoadTop1<GenAddOnColumn>(addOnStatusQuery);
			if (result == null)
			{
				result = Factory.New<GenAddOnColumn>();
				result.XA_Name = ChiefConstants.CusDecTypeDLU;
				result.XA_ParentTableCode = EDIMessageSchema.Constants.Prefix;
				result.XA_ParentID = PK;
			}
			return result;
		}
	}
}
