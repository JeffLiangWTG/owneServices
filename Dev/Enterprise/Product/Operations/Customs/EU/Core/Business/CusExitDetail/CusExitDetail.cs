using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.EU.Business
{
	[CodeProperty(nameof(CusExitDetail.CED_MovementReferenceNumber)), DescriptionProperty(nameof(CusExitDetail.CED_MovementReferenceNumber))]
	[DependentBusinessObject(typeof(CusExitControlHeader), nameof(CusExitControlHeader.CusExitDetails))]
	public class CusExitDetail : AutoCusExitDetail, Integration.Customs.EU.ICusExitDetail, IDocManagerSupport
	{
		public CusExitDetail(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static readonly CusExitDetailTypeDecider TypeDecider = new CusExitDetailTypeDecider();

		public CusExitControlHeader Header => Factory.Load<CusExitControlHeader>(CED_CEH);

		[MaxLength(50)]
		public override ZString CED_ArrivalNotificationPlace { get => base.CED_ArrivalNotificationPlace; set => base.CED_ArrivalNotificationPlace = value; }

		[MaxLength(50)]
		public override ZString CED_TransportID { get => base.CED_TransportID; set => base.CED_TransportID = value; }

		[List(nameof(Lookups) + "." + nameof(CusExitDetailLookups.StatusList))]
		public override ZString CED_Status { get => base.CED_Status; set => base.CED_Status = value; }

		[List(nameof(Lookups) + "." + nameof(CusExitDetailLookups.CustomsOffices))]
		public override ZString CED_CustomsOffice { get => base.CED_CustomsOffice; set => base.CED_CustomsOffice = value; }

		[List(nameof(Lookups) + "." + nameof(CusExitDetailLookups.OrganizationsFindBoxList))]
		[ResourceStringData("Enterprise.Customs.EU.Business.CusExitDetail|CED_OA_Carrier", Caption = "Carrier")]
		public override ZGuid CED_OA_Carrier
		{
			get => base.CED_OA_Carrier;
			set
			{
				base.CED_OA_Carrier = value;
				CED_OA_CarrierInfo.RefreshBinding();
			}
		}

		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(this, Factory);
					fMessages.Load();
					fMessages.IsManagedForDataRefresh = true;
				}
				return fMessages;
			}
		}
		EDIMessageCollection fMessages;

		[ChildEditable(true)]
		public CusExitItemCollection CusExitItems
		{
			get
			{
				if (cusExitItems == null)
				{
					cusExitItems = GetNewCusExitItemsCollectionCore();
					cusExitItems.Load();
					RegisterEditableChildObject(cusExitItems);
				}
				return cusExitItems;
			}
		}
		CusExitItemCollection cusExitItems;

		[ChildEditable(true)]
		public AdditionalInfoCollection AdditionalInfos
		{
			get
			{
				if (additionalInfos == null)
				{
					additionalInfos = GetNewAdditionalInfosCollectionCore();
					additionalInfos.Load();
					RegisterEditableChildObject(additionalInfos);
				}
				return additionalInfos;
			}
		}
		AdditionalInfoCollection additionalInfos;

		protected virtual AdditionalInfoCollection GetNewAdditionalInfosCollectionCore() => new AdditionalInfoCollection(this);

		protected virtual CusExitItemCollection GetNewCusExitItemsCollectionCore() => new CusExitItemCollection(this);

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.CustomsExitDetail));
		DocManagerInfo docManagerInfo;

		#endregion
	}
}
