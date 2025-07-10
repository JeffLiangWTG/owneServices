using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class AsycudaBill : ASYCUDA.Business.AsycudaBill, Integration.Customs.ASYCUDA.MXManifest.IAsycudaBill, IManifestMessageAttachee
	{
		public AsycudaBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;
		public new AsycudaPackCollection Packs => (AsycudaPackCollection)base.Packs;
		protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new AsycudaPackCollection(this);
		protected override ZString GetCountryCode() => Core.Constants.CountryCodes.Mexico;
		protected override Type GetPackTypeCore() => typeof(AsycudaPack);
		protected override Type GetPackageContainerLinkTypeCore() => typeof(AsycudaContainerBillOrPackageLink);
		protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);
		protected override ManifestBase.AsycudaBillValidation GetNewValidationForMasterChild() => new AsycudaBillValidationForMasterChild(this);

		public override bool CanDelete
		{
			get
			{
				return base.CanDelete
					&& this.ABL_BillStatus != MessageStatusCodeList.Codes.Accepted
					&& this.ABL_BillStatus != MessageStatusCodeList.Codes.Cancel
					&& this.ABL_BillStatus != MessageStatusCodeList.Codes.Sent;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				switch (ABL_BillStatus)
				{
					case MessageStatusCodeList.Codes.Accepted:
						return ResString.GetMultilingualString("B6CC5A7F-1B42-413B-8098-E37288AC908D", @"This Bill is already sent and accepted.
If you need to cancel it from Customs, you must use the Cancel option from Manifest menu.");
					case MessageStatusCodeList.Codes.Cancel:
						return ResString.GetMultilingualString("B470EE8D-BECD-4597-945E-7585E36126D7", "This Bill is canceled. Must be kept for history purposes.");
					case MessageStatusCodeList.Codes.Sent:
						return ResString.GetMultilingualString("807CD876-0BEB-4FF3-A312-74A4156AAAD4", "This Bill is already sent and it is awaiting for a response.");
					default:
						return base.ReasonForNotAbleToDelete;
				}
			}
		}

		#region Overrided properties

		protected override bool ABL_BillStatus_ReadOnly => true;

		protected override bool CustomsEntryNumber_ReadOnly => true;

		[DecimalPlaces(2)]
		public override ZDecimal ABL_CustomsValue
		{
			get => base.ABL_CustomsValue;
			set => base.ABL_CustomsValue = value;
		}

		[DecimalPlaces(2)]
		public override ZDecimal ABL_FreightValue
		{
			get => base.ABL_FreightValue;
			set => base.ABL_FreightValue = value;
		}

		[DecimalPlaces(2)]
		public override ZDecimal ABL_InsuranceValue
		{
			get => base.ABL_InsuranceValue;
			set => base.ABL_InsuranceValue = value;
		}

		[DecimalPlaces(2)]
		public override ZDecimal ABL_TransportValue
		{
			get => base.ABL_TransportValue;
			set => base.ABL_TransportValue = value;
		}

		#endregion

		#region IManifestMessageAttachee

		ZString IManifestMessageAttachee.BillNumber => ABL_BillNumber;
		ZGuid IMessageAttachee.GlobalBranchPK => Header.AMA_GB;
		IBusinessObjectCollection IMessageAttachee.Messages => Messages;
		ZString IMessageAttachee.TableName => AsycudaBillSchema.Constants.TableName;
		ZString IMessageAttachee.JobReference => Header.AMA_JobReference;

		#region Shipper

		public override ZString[] ShipperRegNoTypes() => RegNoTypes();

		#endregion

		#region Consignee

		public override ZString[] ConsigneeRegNoTypes() => RegNoTypes();

		#endregion

		#region NotifyParty

		public override ZString[] NotifyPartyRegNoTypes() => RegNoTypes();

		#endregion

		#endregion

		#region Message Sending Conditions

		public bool CanSendOriginalMessage() => ABL_BillStatus == ZString.Empty || ABL_BillStatus == CustomsStatusList.Codes.ERR;

		public bool CanSendCancellationMessage() => ABL_BillStatus == CustomsStatusList.Codes.ACP && ABL_MessageStatus != CustomsStatusList.Codes.AWA;

		public bool CanSendModificationMessage() => ABL_BillStatus == CustomsStatusList.Codes.ACP && ABL_MessageStatus != CustomsStatusList.Codes.AWA;

		#endregion

		ZString[] RegNoTypes() => new ZString[] { MexicoOrgCusCodeInfo.OrgCusCodes.RFC };

		protected override ZBool ShouldSynchronisePaymentType() => ZBool.True;
	}
}
