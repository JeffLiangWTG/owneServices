using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using AddressType = Enterprise.ZArchitecture.Business.AddressType;

namespace Enterprise.Customs.CL.Manifest.Business
{
	[CodeProperty(AsycudaBill.Schema.ABL_BillNumber), DescriptionProperty(AsycudaBill.Schema.ABL_BillNumber)]
	public class AsycudaBill : ASYCUDA.Business.AsycudaBill, Integration.Customs.ASYCUDA.CLManifest.IAsycudaBill, IMessageAttachee
	{
		public AsycudaBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : ASYCUDA.Business.AsycudaBill.Schema
		{
			public const string ABL_RoRo = "ABL_RoRo";
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;
		public new AsycudaPackCollection<AsycudaPack, AsycudaBill> Packs => (AsycudaPackCollection<AsycudaPack, AsycudaBill>)base.Packs;
		protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new AsycudaPackCollection<AsycudaPack, AsycudaBill>(this);
		protected override ZString GetCountryCode() => Core.Constants.CountryCodes.Chile;
		protected override Type GetPackTypeCore() => typeof(AsycudaPack);
		protected override Type GetPackageContainerLinkTypeCore() => typeof(AsycudaContainerBillOrPackageLink);
		protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new AsycudaBillLookups(this);
		public new AsycudaBillLookups Lookups => (AsycudaBillLookups)base.Lookups;
		protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);

		#region ABL_RoRo

		[ResourceStringData("AsycudaBill.ABL_RoRo", Caption = "Ro-Ro")]
		public ZBool ABL_RoRo
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.ABL_RoRo);
			set
			{
				var oldValue = ABL_RoRo;
				this.SetSystemDefinedValue(Schema.ABL_RoRo, value);
				ABL_RoRoInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo ABL_RoRoInfo => GetZPropertyInfo(Schema.ABL_RoRo);

		#endregion

		#region GoodsLocation

		protected override ZAddress GetNewABL_OA_GoodsLocation_ZAddress()
		{
			var result = base.GetNewABL_OA_GoodsLocation_ZAddress();
			result.DefaultAddressType = AddressType.OFC;
			return result;
		}

		#endregion

		public override ZString[] ShipperRegNoTypes() => RegNoTypes();

		public override ZString[] ConsigneeRegNoTypes() => RegNoTypes();

		public override ZString[] NotifyPartyRegNoTypes() => RegNoTypes();

		#region Overrided properties

		protected override bool ABL_BillStatus_ReadOnly => true;

		protected override bool CustomsEntryNumber_ReadOnly => true;

		public override bool ABL_BillNumber_ReadOnly => ABL_BillStatus != CustomsStatusList.Codes.ERR && ABL_BillStatus != ZString.Empty;

		public override ZString ABL_BillStatus
		{
			get => base.ABL_BillStatus;
			set
			{
				var oldValue = ABL_BillStatus;
				base.ABL_BillStatus = value;
				if (!IsCopying && oldValue != ABL_BillStatus)
				{
					this.canSendOriginalMessageCached = null;
					CheckIfPacksShouldBeReadOnly();
				}
			}
		}

		public override ZString ABL_MessageStatus
		{
			get => base.ABL_MessageStatus;
			set
			{
				base.ABL_MessageStatus = value;
				CheckIfPacksShouldBeReadOnly();
			}
		}

		void CheckIfPacksShouldBeReadOnly()
		{
			if (ABL_BillStatus == MessageStatusCodeList.Codes.Accepted || (ABL_MessageStatus == MessageStatusCodeList.Codes.Awaiting && ABL_BillStatus == MessageStatusCodeList.Codes.Sent) || ABL_BillStatus == MessageStatusCodeList.Codes.Cancel)
			{
				Packs.SetReadOnlyIncludingChildren(true);
			}
			else
			{
				Packs.SetReadOnlyIncludingChildren(false);
			}
		}

		#endregion

		#region IMessageAttachee

		ZString IMessageAttachee.Number => base.ABL_BillNumber;
		ZGuid IMessageAttachee.GlobalBranchPK => base.Header.AMA_GB;
		ASYCUDA.Business.AsycudaManifestHeader IMessageAttachee.Header => base.Header;
		IBusinessObjectCollection IMessageAttachee.Messages => Messages;

		#endregion

		#region Message Sending Conditions

		public bool CanSendModificationMessage() => ABL_BillStatus == CustomsStatusList.Codes.ACP;

		public bool CanSendCancellationMessage() => ABL_BillStatus == CustomsStatusList.Codes.ACP;

		public bool CanSendOriginalMessage => Factory.GetValue(ref canSendOriginalMessageCached, () =>
		{
			if (ABL_BillStatus == CustomsStatusList.Codes.SNT)
			{
				return false;
			}
			else
			{
				if (Header.IsSea)
				{
					return ABL_BillStatus != CustomsStatusList.Codes.ACP;
				}
				else
				{
					var lastAcceptedMessage = CLMessageHelper.GetMASObjectOriginal(this);

					if (lastAcceptedMessage != null)
					{
						var number = lastAcceptedMessage.PartialCorrelative;
						if (number == null)
						{
							foreach (var header in from arrivalHeader in Header.ArrivalHeaders
												   from details in
													   from arrivalDetails in arrivalHeader.ArrivalDetails
													   where arrivalDetails.ATL_ABL_AsycudaBill == PK
													   select new { }
												   select new { })
							{
								return true;
							}
							return false;
						}
						else
						{
							ZShort sequence = 0;
							if (ZShort.TryParse(number, out sequence) && this.Header.ArrivalHeaders.Count > 0)
							{
								return sequence < this.Header.ArrivalHeaders.Max(x => x.ATH_ArrivalSequence);
							}
						}
					}
					return true;
				}
			}
		});

		CachedProperty<bool> canSendOriginalMessageCached;

		public override bool CanDelete
		{
			get
			{
				var baseDelete = base.CanDelete;
				return baseDelete && this.ABL_BillStatus != CustomsStatusList.Codes.ACP && this.ABL_BillStatus != CustomsStatusList.Codes.CAN && this.ABL_BillStatus != CustomsStatusList.Codes.SNT;
			}
		}

		public void SetCachedNullForSendOriginalMessage()
		{
			this.canSendOriginalMessageCached = null;
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				switch (ABL_BillStatus)
				{
					case MessageStatusCodeList.Codes.Accepted:
						return ResString.GetMultilingualString("9CFDFDAF-0050-42D9-B7BC-4B39B90257D7", "This Bill is already sent and accepted.\r\nIf you need to cancel it from Customs, you must use the Cancel option from Manifest menu.");
					case MessageStatusCodeList.Codes.Sent when ABL_MessageStatus == MessageStatusCodeList.Codes.Awaiting:
						return ResString.GetMultilingualString("D7F4ACD9-AB2C-4429-A1C2-A354FD1285AB", "This Bill is already sent and it is awaiting for a response.");
					case MessageStatusCodeList.Codes.Cancel:
						return ResString.GetMultilingualString("521F7930-7A52-4267-A688-3CFD1D869687", "This bill is canceled. Must be kept for history purposes.");
					default:
						return base.ReasonForNotAbleToDelete;
				}
			}
		}

		#endregion

		ZString[] RegNoTypes() => new ZString[] { ChileOrgCusCodeInfo.OrgCusCodes.RUT, OrgCusCode.CodeTypes.PassportID };

		protected override bool IsManifestUQNeedToConvertCore => false;
	}
}
