using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	[CodeProperty(nameof(CusInBondMoveHeader.HumanReadableName))]
	public class CusInBondMoveHeader : Customs.Business.BaseCusInBondMoveHeader, Integration.Customs.AsycudaCustoms.ICusInBondMoveHeader, IDocumentSupportable, IDocManagerSupport
	{
		public CusInBondMoveHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : Customs.Business.AutoCusInBondMoveHeader.Schema
		{
			public const string BM_Calc_PermitNumber = "BM_Calc_PermitNumber";
			public const string BM_Calc_IssueDate = "BM_Calc_IssueDate";
			public const string BM_Calc_ValidityDate = "BM_Calc_ValidityDate";
		}

		public new CusInBondHeader Header => (CusInBondHeader)base.Header;

		public CusEntryInstruction EntryInstruction => Factory.Load<CusEntryInstruction>(new ZGuid(Header?.BH_ParentID));

		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		[ResourceStringData("Enterprise.Customs.AsycudaCustoms.Business|BM_Calc_PermitNumber", Caption = "Permit #")]
		public ZString BM_Calc_PermitNumber
		{
			get => PermitNumberBizObj.CE_EntryNum;
			set
			{
				var oldValue = PermitNumberBizObj.CE_EntryNum;
				PermitNumberBizObj.CE_EntryNum = value;
				if (oldValue != value)
				{
					BM_Calc_PermitNumberInfo.RefreshBinding(oldValue);
					ValueSetStrategy.ValueSet(BM_Calc_PermitNumberInfo, oldValue);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateBM_Calc_PermitNumber();
				}
			}
		}

		public ZPropertyInfo BM_Calc_PermitNumberInfo => GetZPropertyInfo(Schema.BM_Calc_PermitNumber);

		[ResourceStringData("Enterprise.Customs.AsycudaCustoms.Business|BM_Calc_IssueDate", Caption = "Issue Date")]
		public ZDateTime BM_Calc_IssueDate
		{
			get => PermitNumberBizObj.CE_IssueDate;
			set
			{
				var oldValue = PermitNumberBizObj.CE_IssueDate;
				PermitNumberBizObj.CE_IssueDate = value;
				if (oldValue != value)
				{
					BM_Calc_IssueDateInfo.RefreshBinding(oldValue);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateBM_Calc_IssueDate();
				}
			}
		}

		public ZPropertyInfo BM_Calc_IssueDateInfo => GetZPropertyInfo(Schema.BM_Calc_IssueDate);

		[ResourceStringData("Enterprise.Customs.AsycudaCustoms.Business|BM_ArrivalDate", Caption = "Arrival Date")]
		public override ZDateTime BM_ArrivalDate
		{
			get => base.BM_ArrivalDate;
			set => base.BM_ArrivalDate = value;
		}

		[ResourceStringData("Enterprise.Customs.AsycudaCustoms.Business|BM_Calc_ValidityDate", Caption = "Validity Date")]
		public ZDateTime BM_Calc_ValidityDate
		{
			get => PermitNumberBizObj.CE_ExpiryDate;
			set
			{
				var oldValue = PermitNumberBizObj.CE_ExpiryDate;
				PermitNumberBizObj.CE_ExpiryDate = value;
				if (oldValue != value)
				{
					BM_Calc_ValidityDateInfo.RefreshBinding(oldValue);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateBM_Calc_ValidityDate();
				}
			}
		}

		[DecimalPlaces(2)]
		[ResourceStringData("Enterprise.Customs.AsycudaCustoms.Business|BM_MonetaryValue", Caption = "Customs Value")]
		public override ZDecimal BM_MonetaryValue
		{
			get => base.BM_MonetaryValue;
			set => base.BM_MonetaryValue = value;
		}

		[DecimalPlaces(2)]
		[ResourceStringData("Enterprise.Customs.AsycudaCustoms.Business|BM_NetWeight", Caption = "Net Weight")]
		public override ZDecimal BM_NetWeight
		{
			get => base.BM_NetWeight;
			set => base.BM_NetWeight = value;
		}

		[DecimalPlaces(5)]
		[ResourceStringData("Enterprise.Customs.AsycudaCustoms.Business|BM_CustomsQuantity", Caption = "Customs Quantity")]
		public override ZDecimal BM_CustomsQuantity
		{
			get => base.BM_CustomsQuantity;
			set => base.BM_CustomsQuantity = value;
		}

		public ZPropertyInfo BM_Calc_ValidityDateInfo => GetZPropertyInfo(Schema.BM_Calc_ValidityDate);

		[ResourceStringData("Enterprise.Customs.AsycudaCustoms.Business|BM_AdditionalText", Caption = "Comments")]
		public override ZString BM_AdditionalText
		{
			get => base.BM_AdditionalText;
			set => base.BM_AdditionalText = value;
		}

		public CusEntryNumber PermitNumberBizObj
		{
			get
			{
				if (permitNumberBizObj == null)
				{
					var headerBranch = HeaderBranch ?? GlbBranch.CurrentBranch;
					permitNumberBizObj = CusEntryNumber.LoadOrCreate(this, PermitNumberType, headerBranch.Company.GC_RN_NKCountryCode);
					RegisterEditableChildObject(permitNumberBizObj);
				}

				return permitNumberBizObj;
			}
		}
		CusEntryNumber permitNumberBizObj;

		public const string PermitNumberType = "PMT";

		public override void Delete()
		{
			this.DeleteChildren<CusEntryNumber>(CusEntryNumSchema.CE_ParentID);
			movementDetails?.DeleteAll();
			base.Delete();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BM_SubApplicationCode = PermitNumberType;
		}

		public new CusInBondMoveHeaderValidation Validation => (CusInBondMoveHeaderValidation)base.Validation;
		protected override Customs.Business.CusInBondMoveHeaderValidation GetNewValidation() => new CusInBondMoveHeaderValidation(this);

		public DocumentSupporter DocumentSupporter => new CusInBondMoveHeaderDocumentSupporter(this);

		public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.AsycudaCusInBondMoveHeader));
		DocManagerInfo docManagerInfo;

		public override MultilingualString GetWarningBeforeBeingDeleted()
		{
			var result = base.GetWarningBeforeBeingDeleted();
			if (result.IsEmpty && DocManagerInfo.AllEDocs.Count > 0)
			{
				result = ResString.GetMultilingualString("268927f3-8f79-444a-8fc7-f3d7d948a0b4", "This transit permit has related eDoc(s).");
			}

			return result;
		}

		protected override IValueSetStrategy GetValueSetStrategy()
		{
			return new CusInBondMoveHeaderValueSetStrategy(this);
		}

		CusInBondMoveHeaderValueSetStrategy ValueSetStrategy => (CusInBondMoveHeaderValueSetStrategy)GetValueSetStrategy();

		[ChildEditable(false)]
		public CusInBondMoveDetailCollection MovementDetails
		{
			get
			{
				if (movementDetails == null)
				{
					movementDetails = new CusInBondMoveDetailCollection(this);
					RegisterEditableChildObject(movementDetails);
				}
				return movementDetails;
			}
		}
		CusInBondMoveDetailCollection movementDetails;

		public CusInBondMoveDetail FirstMoveDetail => CachedValueHelper.GetValue(Factory, ref firstMoveDetailCached, () => MovementDetails.FirstOrDefault() ?? MovementDetails.AddNew());
		CachedProperty<CusInBondMoveDetail> firstMoveDetailCached;

		public void AddAllContainers()
		{
			var firstMoveDetail = FirstMoveDetail;
			var addedContainers = firstMoveDetail.Containers.Select(x => x.BC_ContainerNum).ToHashSet();

			foreach (var containerNumber in EntryInstruction?.JobDeclaration?.CusContainers.Cast<CusContainer>().Select(x => x.CO_ContainerNumber))
			{
				if (!containerNumber.IsEmpty && !addedContainers.Contains(containerNumber))
				{
					var cusInBondContainer = firstMoveDetail.Containers.AddNew();
					cusInBondContainer.BC_ContainerNum = containerNumber;
				}
			}
		}

		protected override ZString HumanReadableNameCore => Factory.GetValue(ref humanReadableNameCached, () =>
		{
			var result = new ZStringBuilder();
			result.AppendIfNotEmpty(BM_Calc_PermitNumber);
			if (EntryInstruction is CusEntryInstruction entryInstruction)
			{
				result.AppendIfNotEmpty(entryInstruction.CEI_Style);
				result.AppendIfNotEmpty(entryInstruction.CEI_Description);
			}
			return result.ToStringWithDelimiterBetweenAppends("-");
		});
		CachedProperty<ZString> humanReadableNameCached;

		protected override Type MovementDetailTypeCore => typeof(CusInBondMoveDetail);

		#region Suspend Change Validation For Permit Dates   

		public IDisposable SuspendChangeValidationForPermitDates() => new ChangeValidationForPermitDatesSuspender(this);

		public bool IsPermitDateChangeValidationSuspended => suspendChangeValidationForPermitDatesIndex > 0;

		int suspendChangeValidationForPermitDatesIndex;

		sealed class ChangeValidationForPermitDatesSuspender : IDisposable
		{
			public ChangeValidationForPermitDatesSuspender(CusInBondMoveHeader moveHeader, Action additionalAction = null)
			{
				this.moveHeader = moveHeader;
				this.moveHeader.suspendChangeValidationForPermitDatesIndex++;
				this.additionalAction = additionalAction;
			}

			public void Dispose()
			{
				if (!isDisposed)
				{
					moveHeader.suspendChangeValidationForPermitDatesIndex--;
					additionalAction?.Invoke();

					isDisposed = true;
				}
			}

			readonly CusInBondMoveHeader moveHeader;
			readonly Action additionalAction;
			bool isDisposed;
		}
		#endregion
	}
}
