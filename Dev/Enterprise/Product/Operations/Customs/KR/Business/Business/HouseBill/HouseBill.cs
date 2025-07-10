using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.KR.Business
{
	public partial class Bill : AutoKRHouseBill,
		Integration.Customs.KR.IBill,
		ICusCodeDataTypeSupporter
	{
		public Bill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoKRHouseBill.Schema
		{
			public const string HBSplitDecReasonRemark = "HBSplitDecReasonRemark";
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new CusDecHouseBillFetchStrategy(this);

		[ChildEditable(true)]
		public CargoManagementNumberBOCollection CargoManagementNumbers
		{
			get
			{
				if (cargoManagementNumbers == null)
				{
					cargoManagementNumbers = new CargoManagementNumberBOCollection(this);
					cargoManagementNumbers.Load();
					RegisterEditableChildObject(cargoManagementNumbers);
					cargoManagementNumbers.CountChanged += CargoManagementNumbers_CountChanged;
				}
				return cargoManagementNumbers;
			}
		}

		void CargoManagementNumbers_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (sender is CargoManagementNumberBOCollection collection && collection.Count > 1)
			{
				CU_HBSplitDecInd = HouseBillSplitDeclarationIndicatorCodeList.Codes.Y;
			}
		}

		CargoManagementNumberBOCollection cargoManagementNumbers;

		public ZString CargoManagementNumber
		{
			get => CargoManagementNumberBO?.CY_Data ?? ZString.Empty;
			set
			{
				if (CargoManagementNumberBO == null)
				{
					var cargoManageNum = CargoManagementNumbers.AddNew();
					cargoManageNum.CY_Data = value;
				}
			}
		}
		CargoManagementNumberBO CargoManagementNumberBO
		{
			get
			{
				if ((cargoManagementNumberBO == null || cargoManagementNumberBO.IsDeleted || cargoManagementNumberBO.CY_ParentID.IsEmpty) && CargoManagementNumbers.Count > 0)
				{
					cargoManagementNumberBO = CargoManagementNumbers.Cast<CargoManagementNumberBO>().OrderBy(x => x.CY_Order).ThenBy(x => x.CY_Data).First();
				}
				return cargoManagementNumberBO;
			}
		}
		CargoManagementNumberBO cargoManagementNumberBO;

		[ReadOnlyMember(nameof(CU_HBSplitDecInd_ReadOnly))]
		[List(nameof(Lookups) + "." + nameof(CusDecHouseBillLookups.HouseBillSplitDeclarationIndicatorCodeList))]
		[ResourceStringData("FA8A80CE-A9A5-4CAF-8612-B0F689B08674", Caption = "Bill Split YN")]
		public override ZString CU_HBSplitDecInd
		{
			get => base.CU_HBSplitDecInd;
			set => base.CU_HBSplitDecInd = value;
		}
		bool CU_HBSplitDecInd_ReadOnly => CargoManagementNumbers.Count > 1;

		[List(nameof(Lookups) + "." + nameof(CusDecHouseBillLookups.HouseBillSplitDeclarationReasonCodeList))]
		[ResourceStringData("D0F86B84-DD99-4658-B611-9BB770BFDBE0", Caption = "Bill Split Reason Code")]
		public override ZString CU_HBSplitDecReasonCode
		{
			get => base.CU_HBSplitDecReasonCode;
			set => base.CU_HBSplitDecReasonCode = value;
		}

		[MaxLength(50)]
		[ResourceStringData("F1615718-EE6D-4338-B92F-A7DCE9BBBE0D", Caption = "Bill Split Reason Description")]
		public ZString HBSplitDecReasonRemark
		{
			get => AdditionalInformationNote?.ST_NoteText ?? ZString.Empty;
			set
			{
				CheckMaximumLength(HBSplitDecReasonRemarkInfo, value);
				var note = AdditionalInformationNote;
				if (note == null)
				{
					Notes.AddNew(false, PredefinedNoteTypes.Instance.AdditionalInformation.Description, value);
				}
				else
				{
					note.ST_NoteText = value;
				}
				HBSplitDecReasonRemarkInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateHBSplitDecReasonRemark();
				}
			}
		}

		public ZPropertyInfo HBSplitDecReasonRemarkInfo
		{
			get { return GetZPropertyInfo(nameof(HBSplitDecReasonRemark)); }
		}

		StmNote AdditionalInformationNote
		{
			get
			{
				if (additionalInformationNoteCached == null || additionalInformationNoteCached.IsDeleted)
				{
					additionalInformationNoteCached = Notes.FindByDescription(PredefinedNoteTypes.Instance.AdditionalInformation.Description).SingleOrDefault();
				}
				return additionalInformationNoteCached;
			}
		}
		StmNote additionalInformationNoteCached;

		public bool IsD87 => Declaration?.IsD87 ?? false;

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			return new Dictionary<ZString, Type>
			{
				{ CusCodeDataTypeList.Codes.CargoManagementNumber, typeof(CargoManagementNumberBO) }
			};
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}
	}
}
