using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class OrgSupBuyLinkTrnModeAddInfoBizObj : NonPersistentBusinessObject
	{
		public OrgSupBuyLinkTrnModeAddInfoBizObj(BusinessObjectFactory factory) : base(factory) { }

		public OrgSupBuyLinkTrnModeAddInfoBizObj(OrgSupBuyLinkTrnModeAddInfo addInfo) : base(addInfo.Factory)
		{
			AttachedAddInfo = Argument.NotNull(addInfo, nameof(addInfo));
		}

		OrgSupBuyLinkTrnModeAddInfo AttachedAddInfo { get; }

		public override bool IsDeleted =>
			AttachedAddInfo?.Parent is not BusinessObject parentBizObj
			|| parentBizObj.IsDeleted
			|| base.IsDeleted;

		#region Fields & Infos

		[ResourceStringData("Enterprise.Customs.CN.Business.OrgSupBuyLinkTrnModeAddInfoBizObj|ZO_CustomsOffice", Caption = "Customs Office")]
		[MaxLength(4)]
		[List(nameof(Lookups) + "." + nameof(OrgSupBuyLinkTrnModeAddInfoBizObjLookups.CustomsOfficeList))]
		public ZString ZO_CustomsOffice
		{
			get => AttachedAddInfo.ZO_CustomsOffice;
			set
			{
				AttachedAddInfo.ZO_CustomsOffice = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateZO_CustomsOffice();
				}
				ZO_CustomsOfficeInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo ZO_CustomsOfficeInfo => GetZPropertyInfo(OrgSupBuyLinkTrnModeAddInfo.Schema.ZO_CustomsOffice);

		[ResourceStringData("Enterprise.Customs.CN.Business.OrgSupBuyLinkTrnModeAddInfoBizObj|ZO_OfficeOfEntryExit", Caption = "Office of Entry/Exit")]
		[MaxLength(4)]
		[List(nameof(Lookups) + "." + nameof(OrgSupBuyLinkTrnModeAddInfoBizObjLookups.OfficeOfEntryExitList))]
		public ZString ZO_OfficeOfEntryExit
		{
			get => AttachedAddInfo.ZO_OfficeOfEntryExit;
			set
			{
				AttachedAddInfo.ZO_OfficeOfEntryExit = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateZO_OfficeOfEntryExit();
				}
				ZO_OfficeOfEntryExitInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo ZO_OfficeOfEntryExitInfo => GetZPropertyInfo(OrgSupBuyLinkTrnModeAddInfo.Schema.ZO_OfficeOfEntryExit);

		[ResourceStringData("Enterprise.Customs.CN.Business.OrgSupBuyLinkTrnModeAddInfoBizObj|ZO_CIQOfficeOfEntryExit", Caption = "CIQ Customs Office")]
		[MaxLength(6)]
		[List(nameof(Lookups) + "." + nameof(OrgSupBuyLinkTrnModeAddInfoBizObjLookups.CIQPortList))]
		public ZString ZO_CIQOfficeOfEntryExit
		{
			get => AttachedAddInfo.ZO_CIQOfficeOfEntryExit;
			set
			{
				AttachedAddInfo.ZO_CIQOfficeOfEntryExit = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateZO_CIQOfficeOfEntryExit();
				}
				ZO_CIQOfficeOfEntryExitInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo ZO_CIQOfficeOfEntryExitInfo => GetZPropertyInfo(OrgSupBuyLinkTrnModeAddInfo.Schema.ZO_CIQOfficeOfEntryExit);

		#endregion

		OrgSupBuyLinkTrnModeAddInfoBizObjValidation fValidation;
		OrgSupBuyLinkTrnModeAddInfoBizObjValidation Validation => fValidation ?? (fValidation = new OrgSupBuyLinkTrnModeAddInfoBizObjValidation(this));

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		OrgSupBuyLinkTrnModeAddInfoBizObjLookups fLookups;
		public OrgSupBuyLinkTrnModeAddInfoBizObjLookups Lookups => fLookups ?? (fLookups = new OrgSupBuyLinkTrnModeAddInfoBizObjLookups(this));

		[ChildEditable(true)]
		public CusCodeDataCollection<CustomsOffice> CustomsOffices
		{
			get
			{
				if (customsOffices == null && AttachedAddInfo.Parent != null)
				{
					customsOffices = new CusCodeDataCollection<CustomsOffice>(AttachedAddInfo.Parent, Constants.CusCodeDataTypes.Codes.CustomsOffice);
					customsOffices.Load();
					RegisterEditableChildObject(customsOffices);
				}
				return customsOffices;
			}
		}
		CusCodeDataCollection<CustomsOffice> customsOffices;

		[MaxLength(4)]
		[List(nameof(Lookups) + "." + nameof(OrgSupBuyLinkTrnModeAddInfoBizObjLookups.CustomsOfficeList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobDeclaration|OfficeOfDestination", Caption = "Office of Destination")]
		public ZString OfficeOfDestination
		{
			get => officeOfDestination.CY_Data;
			set
			{
				officeOfDestination.CY_Data = value;
				OfficeOfDestinationInfo.RefreshBinding();
			}
		}

		CustomsOffice officeOfDestination => CustomsOffices.GetFirstElementHaving(CustomsOfficeTypeList.Codes.DES) ?? CustomsOffices.AddNew(CustomsOfficeTypeList.Codes.DES);

		public ZPropertyInfo OfficeOfDestinationInfo => GetWrappedZPropertyInfo(nameof(OfficeOfDestination), x => officeOfDestination.CY_DataInfo);
	}
}
