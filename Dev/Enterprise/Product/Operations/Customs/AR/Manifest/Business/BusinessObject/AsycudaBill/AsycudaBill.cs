using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class AsycudaBill : ASYCUDA.Business.AsycudaBill, Integration.Customs.ASYCUDA.ARManifest.IAsycudaBill, IMessageAttachee
	{
		public AsycudaBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : ASYCUDA.Business.AsycudaBill.Schema
		{
			public const string ABL_IsInformedToRenar = "ABL_IsInformedToRenar";
			public const string ABL_IsMonitoredTransit = "ABL_IsMonitoredTransit";
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;
		protected override ZString GetCountryCode() => Core.Constants.CountryCodes.Argentina;
		protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);
		protected override ManifestBase.AsycudaBillValidation GetNewValidationForMasterChild() => new AsycudaBillValidationForMasterChild(this);
		protected override Type GetPackTypeCore() => typeof(AsycudaPack);
		public new AsycudaPackCollection Packs => (AsycudaPackCollection)base.Packs;
		protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new AsycudaPackCollection(this);

		#region ABL_IsInformedToRenar

		[ResourceStringData("AsycudaBill.ABL_IsInformedToRenar", Caption = "RENAR")]
		public ZBool ABL_IsInformedToRenar
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.ABL_IsInformedToRenar);
			set
			{
				var oldValue = ABL_IsInformedToRenar;
				this.SetSystemDefinedValue(Schema.ABL_IsInformedToRenar, value);
				ABL_IsInformedToRenarInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo ABL_IsInformedToRenarInfo => GetZPropertyInfo(Schema.ABL_IsInformedToRenar);

		#endregion

		#region ABL_IsMonitoredTransit

		[ResourceStringData("AsycudaBill.ABL_IsMonitoredTransit", Caption = "Monitored Transit")]
		public ZBool ABL_IsMonitoredTransit
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.ABL_IsMonitoredTransit);
			set
			{
				var oldValue = ABL_IsMonitoredTransit;
				this.SetSystemDefinedValue(Schema.ABL_IsMonitoredTransit, value);
				ABL_IsMonitoredTransitInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo ABL_IsMonitoredTransitInfo => GetZPropertyInfo(Schema.ABL_IsMonitoredTransit);

		#endregion

		#region Shipper

		public override ZString[] ShipperRegNoTypes() => RegNoTypes();

		#endregion

		#region Consignee

		public override ZString[] ConsigneeRegNoTypes() => RegNoTypes();

		#endregion

		#region Overrided properties

		protected override bool ABL_BillStatus_ReadOnly => true;

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				switch (ABL_BillStatus)
				{
					case MessageStatusCodeList.Codes.Accepted:
						return ResString.GetMultilingualString("F2663D7B-F5EF-44B0-A607-869D33BEC756", "This Bill is already sent and accepted. If you need to cancel it from Customs, you must use the Cancel option from Manifest menu.");
					case MessageStatusCodeList.Codes.Sent:
						return ResString.GetMultilingualString("7138519B-51EC-427D-8952-D6EF0565323B", "This Bill is already sent and it is awaiting for a response.");
					case MessageStatusCodeList.Codes.Cancel:
						return ResString.GetMultilingualString("0D4DA801-7B96-4571-AD38-3739E63754B9", "This Bill is canceled. Must be kept for history purposes.");
					default:
						return base.ReasonForNotAbleToDelete;
				}
			}
		}

		public override bool CanDelete
		{
			get
			{
				var baseDelete = base.CanDelete;
				return baseDelete && ABL_BillStatus != MessageStatusCodeList.Codes.Accepted && ABL_BillStatus != MessageStatusCodeList.Codes.Cancel && ABL_BillStatus != MessageStatusCodeList.Codes.Sent;
			}
		}

		#endregion

		#region IManifestMessageAttachee

		ZGuid IMessageAttachee.GlobalBranchPK => Header.AMA_GB;
		IBusinessObjectCollection IMessageAttachee.Messages => Messages;
		ZString IMessageAttachee.TableName => AsycudaBillSchema.Constants.TableName;
		ZString IMessageAttachee.JobReference
		{
			get
			{
				var messageText = new ZStringBuilder();
				messageText.Append(ZString.Format("{0}_{1}", Header.AMA_JobReference, ABL_BillNumber));
				return messageText.ToString();
			}
		}

		#endregion

		#region Message Sending Conditions

		public bool CanSendOriginalMessage() => ABL_BillStatus == ZString.Empty;

		public bool CanSendCancellationMessage() => ABL_BillStatus == CustomsStatusList.Codes.ACP;

		public bool CanSendModificationMessage() => ABL_BillStatus == CustomsStatusList.Codes.ACP;

		#endregion

		ZString[] RegNoTypes()
		{
			return new ZString[] { ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIL, ArgentinaOrgCusCodeInfo.OrgCusCodes.DNI, OrgCusCode.CodeTypes.PassportID };
		}

		protected override ZBool ShouldSynchronisePaymentType() => ZBool.True;
	}
}
