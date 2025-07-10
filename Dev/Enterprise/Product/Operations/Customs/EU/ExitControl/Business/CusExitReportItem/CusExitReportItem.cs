using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitReportItem : ExitControlBase.Business.CusExitReportItem
		, EUExitControl.ICusExitReportItem
		, ICusSupportingInfoTypeSupporter
		, IUcc6ValueProvider
		, ICusAuthorizationUsageMaster
	{
		public CusExitReportItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly CusExitReportItemTypeDecider TypeDecider = new CusExitReportItemTypeDecider();

		public new CusExitReport Report => Factory.Load<CusExitReport>(ERI_CER_Report);

		public new CusExitConsignmentItem ConsignmentItem => Factory.Load<CusExitConsignmentItem>(ERI_CCI_ConsignmentItem);

		public new CusExitConsignmentPackage Package => Factory.Load<CusExitConsignmentPackage>(ERI_CXP_Package);

		public new CusExitReportItemLookups Lookups => (CusExitReportItemLookups)base.Lookups;

		public new CusExitReportItemValidation Validation => (CusExitReportItemValidation)base.Validation;

		protected override ExitControlBase.Business.CusExitReportItemLookups GetNewLookups() => new CusExitReportItemLookups(this);

		protected override ExitControlBase.Business.CusExitReportItemValidation GetNewValidation() => new CusExitReportItemValidation(this);

		[ResourceStringData("F0557C29-430F-4848-98D7-1A02592A5132", Caption = "Gross Mass KG")]
		public override ZDecimal ERI_GrossMass
		{
			get => base.ERI_GrossMass;
			set => base.ERI_GrossMass = value;
		}

		[ResourceStringData("11B6C9F4-7DCA-4231-B7F0-ED54F4FC59FE", Caption = "Net Mass KG")]
		public override ZDecimal ERI_NetMass
		{
			get => base.ERI_NetMass;
			set => base.ERI_NetMass = value;
		}

		[ResourceStringData("A39E92EE-03A7-4580-9D2A-B2B1D5A12E24", Caption = "Pack Qty")]
		public override ZInt ERI_Quantity
		{
			get => base.ERI_Quantity;
			set => base.ERI_Quantity = value;
		}

		[ResourceStringData("5ABF789B-D047-41F2-B4A8-BB69F4E01336", Caption = "Sequence Number", ShortCaption = "Seq. No.", MediumCaption = "Seq. Number", FullDescription = "Package Sequence Number")]
		public ZShort SeqNo => Package?.CXP_Sequence ?? ZShort.Zero;

		[ResourceStringData("8A0D9EA2-81E1-4FC2-9E94-2E5350FFCB4F", Caption = "Pack Type")]
		public ZString PackageType => Package?.CXP_PackageType ?? ZString.Empty;

		[ResourceStringData("C1F9EDD1-B3AB-4C13-BAB5-9321CF5BA492", Caption = "Marks & Numbers")]
		public ZString PackageMarksAndNumbers => Package?.CXP_MarksAndNumbers ?? ZString.Empty;

		[ResourceStringData("05BB54FA-CA15-4FD2-9C23-09A88E478266", Caption = "Container/Equipment")]
		public ZString PackageContainerOrEquipment => Package?.ConsignmentPivot?.Container?.CXN_ContainerNumber ?? ZString.Empty;

		#region public AdditionalInfoCollection AdditionalInfos
		[ChildEditable(true)]
		public IAdditionalInfoCollection<AdditionalInfo> AdditionalInfos => fAdditionalInfos ?? (fAdditionalInfos = GetAdditionalInfos());
		IAdditionalInfoCollection<AdditionalInfo> fAdditionalInfos;

		IAdditionalInfoCollection<AdditionalInfo> GetAdditionalInfos()
		{
			var result = CreateNewAdditionalInfoCollection();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		protected virtual IAdditionalInfoCollection<AdditionalInfo> CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection<AdditionalInfo>(this);
		#endregion

		[ChildEditable(true)]
		public ICusAuthorizationUsageCollection<CusAuthorizationUsage, CusExitReportItem> CusAuthorizationUsages
		{
			get
			{
				if (cusAuthorizationUsages is null)
				{
					cusAuthorizationUsages = GetCusAuthorizationUsages();
					cusAuthorizationUsages.Load();
					cusAuthorizationUsages.IsManagedForDataRefresh = true;
					RegisterEditableChildObject(cusAuthorizationUsages);
				}
				return cusAuthorizationUsages;
			}
		}
		ICusAuthorizationUsageCollection<CusAuthorizationUsage, CusExitReportItem> cusAuthorizationUsages;

		protected virtual ICusAuthorizationUsageCollection<CusAuthorizationUsage, CusExitReportItem> GetCusAuthorizationUsages() => new CusAuthorizationUsageCollection<CusAuthorizationUsage, CusExitReportItem>(this, Factory);

		SchemaGuidColumn ICusAuthorizationUsageMaster.FKSchemaColumnInDependent => CusAuthorizationUsageSchema.AGC_ParentID;

		[ResourceStringData("A66EE0E8-9F7D-4102-9678-B09CA437FC0C", Caption = "Item No.")]
		public ZShort ConsignmentItemLineNumber => ConsignmentItem?.CCI_LineNumber ?? ZShort.Zero;

		[ResourceStringData("F7A2835C-0607-426F-BFFC-E4C9D32CABB7", Caption = "Reference Number UCR")]
		public ZString ConsignmentItemUniqueConsignmentReference => ConsignmentItem?.CCI_UniqueConsignmentReference ?? ZString.Empty;

		public override void OnSaving()
		{
			base.OnSaving();

			if (Report is CusExitReport report && report.CusExitReportItemsForBinding.Contains(this))
			{
				var otherReportItemsLinkedToTheSameConsignmentItem = report.CusExitReportItems.Where(x => x.ERI_CCI_ConsignmentItem == ERI_CCI_ConsignmentItem && x.PK != PK);
				foreach (var otherReportItem in otherReportItemsLinkedToTheSameConsignmentItem)
				{
					if (otherReportItem.ERI_GrossMass != ERI_GrossMass)
					{
						otherReportItem.ERI_GrossMass = ERI_GrossMass;
					}
					if (otherReportItem.ERI_NetMass != ERI_NetMass)
					{
						otherReportItem.ERI_NetMass = ERI_NetMass;
					}
				}
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			base.Delete();
		}

		#region ICusSupportingInfoTypeSupporter Implementation

		IDictionary<ZString, Type> ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes() => GetCusSupportingInfoTypes();

		protected virtual IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo, typeof(AdditionalInfo));
			return result;
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies() => GetAdditionalBusinessObjectFetchStrategies();

		protected virtual IEnumerable<IBusinessObjectFetchStrategy> GetAdditionalBusinessObjectFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		#endregion

		public bool IsUCC6 => IsUCC6Core;
		protected virtual bool IsUCC6Core => Report?.IsUCC6 ?? false;
	}
}
