using System.Data;
using System.Diagnostics;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IL.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaTransportDocumentInfo : AsycudaBaseAdditionalInfo
	{
		public new partial class Schema : AutoCusSupportingInfo.Schema
		{
			public const string CSI_ConditionCode = "CSI_ConditionCode";
			public const string CSI_CodeUserInterface = "CSI_CodeUserInterface";
			public const string CSI_ReferenceNumberUserInterface = "CSI_ReferenceNumberUserInterface";
			public const int CSI_ConditionCodeMaxLength = 2;
		}

		public AsycudaTransportDocumentInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new AsycudaBill Parent => (AsycudaBill)base.Parent;

		[MaxLength(Schema.CSI_CodeMaxLength)]
		[ResourceStringData("IL.Manifest.Business.AsycudaTransportDocumentInfo|CSI_CodeUserInterface", Caption = "Type")]
		[List(nameof(Lookups) + "." + nameof(AsycudaTransportDocumentInfoLookups.CodeList))]
		public ZString CSI_CodeUserInterface
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = base.CSI_Code;
				BusinessObject.CheckMaximumLength(CSI_CodeUserInterfaceInfo, value);
				base.CSI_Code = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCSI_CodeUserInterface();
				}
				if (oldValue != CSI_CodeUserInterface)
				{
					UpdateCSIStatus();
				}
				CSI_CodeUserInterfaceInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CSI_CodeUserInterfaceInfo
		{
			[DebuggerStepThrough]
			get
			{
				return GetZPropertyInfo(Schema.CSI_CodeUserInterface);
			}
		}

		[MaxLength(Schema.CSI_ReferenceNumberMaxLength)]
		[ResourceStringData("IL.Manifest.Business.AsycudaTransportDocumentInfo|CSI_ReferenceNumberUserInterface", Caption = "Reference")]
		public ZString CSI_ReferenceNumberUserInterface
		{
			get => base.CSI_ReferenceNumber;
			set
			{
				var oldValue = base.CSI_ReferenceNumber;
				BusinessObject.CheckMaximumLength(CSI_ReferenceNumberUserInterfaceInfo, value);
				base.CSI_ReferenceNumber = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCSI_ReferenceNumberUserInterface();
				}
				if (oldValue != CSI_ReferenceNumberUserInterface)
				{
					UpdateCSIStatus();
				}
				CSI_ReferenceNumberUserInterfaceInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CSI_ReferenceNumberUserInterfaceInfo
		{
			[DebuggerStepThrough]
			get
			{
				return GetZPropertyInfo(Schema.CSI_ReferenceNumberUserInterface);
			}
		}

		protected override ZString HumanReadableNameCore => Res.GetString("F1565AA7-D44F-42D2-8C8D-D196E9FBFDB3", "Transport Document");

		public new AsycudaTransportDocumentInfoValidation Validation => (AsycudaTransportDocumentInfoValidation)base.Validation;

		public new AsycudaTransportDocumentInfoLookups Lookups => (AsycudaTransportDocumentInfoLookups)base.Lookups;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		}

		protected override CusSupportingInfoLookups GetNewLookups() => new AsycudaTransportDocumentInfoLookups(this);

		protected override CusSupportingInfoValidation GetNewValidation() => new AsycudaTransportDocumentInfoValidation(this);

		void UpdateCSIStatus()
		{
			base.CSI_Status =
				base.CSI_Code != TransportDocsTypeList.Codes.IL1 || base.CSI_ReferenceNumber.IsEmpty
				? ZString.Empty
				: IL.Business.Constants.TransportDocument.UserOverrideStatusCode;
		}
	}
}
