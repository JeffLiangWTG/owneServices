using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;
using UniversalReferenceConstants = Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class SupportingDocument : EU.Business.Declaration.MultiLineAddInfos.SupportingDocument
		, Integration.Customs.GB.ISupportingDocument
	{
		public SupportingDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			SetupDataFromCSI_Status();
		}

		public new class Schema : EU.Business.Declaration.MultiLineAddInfos.SupportingDocument.Schema
		{
			public const string CSI_Actions = "CSI_Actions";
			public const string CSI_Availability = "CSI_Availability";

			public const int CSI_ActionsMaxLength = 1;
			public const int CSI_AvailabilityMaxLength = 1;
			public const int DescriptionMaxLength = 250;

			public new const int CSI_ReferenceNumber2MaxLength = 70;
		}

		[ResourceStringData("GBSupportingDocument|G1_Part", Caption = "Part")]
		public override ZString CSI_SubType { get => base.CSI_SubType; set => base.CSI_SubType = value; }

		[ResourceStringData("GBSupportingDocument|CSI_Description", Caption = "Reason")]
		[MaxLength(Schema.DescriptionMaxLength)]
		public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value.Left(Schema.DescriptionMaxLength); }

		public const string CSI_Availability_J = "J";
		public const string CSI_Action_E = "E";
		public const string CSI_Action_P = "P";

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (SupportingDocument)base.CloneInternal(args);
			SetupDataFromCSI_Status();
			using (result.GetValidationSuspender())
			{
				result.CSI_Actions = csi_Actions;
				result.CSI_Availability = csi_Availability;
			}
			return result;
		}

		public override ZString CSI_Code
		{
			get { return base.CSI_Code; }
			set
			{
				value = value.Left(Schema.CodeMaxLength);
				if (base.CSI_Code != value)
				{
					base.CSI_Code = value;

					var cusCode = this.RefCusCode;
					if (cusCode != null)
					{
						var availabilityList = Lookups.AvailabilityList;
						var actionList = Lookups.ActionList;

						if ((availabilityList.Count == 1 && actionList.Count > 0)
							|| (availabilityList.Count > 0 && actionList.Count == 1))
						{
							CSI_Availability = availabilityList[0].Code;
							CSI_Actions = actionList[0].Code;
						}
						else if (Validation.IsAvailabilityAndActionCombinationValidAttribute(CSI_Availability_J, CSI_Action_P))
						{
							CSI_Availability = CSI_Availability_J;
							CSI_Actions = CSI_Action_P;
						}
						else if (Validation.IsAvailabilityAndActionCombinationValidAttribute(CSI_Availability_J, CSI_Action_E))
						{
							CSI_Availability = CSI_Availability_J;
							CSI_Actions = CSI_Action_E;
						}
						else
						{
							CSI_Availability = ZString.Empty;
							CSI_Actions = ZString.Empty;
						}

						if (!IsValidationSuspended)
						{
							Validation.ValidateCSI_Actions();
							Validation.ValidateCSI_Availability();
						}
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.ActionList))]
		[MaxLength(Schema.CSI_ActionsMaxLength)]
		[ResourceStringData("GBSupportingDocument|G1_Actions", Caption = "Actions")]
		public ZString CSI_Actions
		{
			get { return csi_Actions; }
			set
			{
				if (SetNonPersistentPropertyValue(CSI_ActionsInfo, ref csi_Actions, value))
				{
					PersistToCSI_Status(csi_Availability, value);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateCSI_Actions();
				}
			}
		}
		ZString csi_Actions;

		public ZPropertyInfo CSI_ActionsInfo => GetZPropertyInfo(Schema.CSI_Actions);

		[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.AvailabilityList))]
		[MaxLength(Schema.CSI_AvailabilityMaxLength)]
		[ResourceStringData("GBSupportingDocument|G1_Availability", ShortCaption = "Avail.", Caption = "Availability")]
		public ZString CSI_Availability
		{
			get { return csi_Availability; }
			set
			{
				if (SetNonPersistentPropertyValue(CSI_AvailabilityInfo, ref csi_Availability, value))
				{
					PersistToCSI_Status(value, csi_Actions);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateCSI_Availability();
				}
			}
		}
		ZString csi_Availability;

		public ZPropertyInfo CSI_AvailabilityInfo => GetZPropertyInfo(Schema.CSI_Availability);

		public override ZInt QuantityDecimalPlaces => 3;

		[ReadOnlyMember(nameof(IsNotItemLevelCode))]
		[ResourceStringData("GBSupportingDocument|IssuingAuthority", ShortCaption = "Authority", Caption = "Issuing Authority")]
		[MaxLength(Schema.CSI_ReferenceNumber2MaxLength)]
		public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

		[ReadOnlyMember(nameof(IsNotItemLevelCode))]
		[ResourceStringData("GBSupportingDocument|DateOfValidity", ShortCaption = "Valid", Caption = "Date of Validity")]
		public override ZDateTime CSI_DateOfIssue { get => base.CSI_DateOfIssue; set => base.CSI_DateOfIssue = value; }

		[ReadOnlyMember(nameof(IsNotItemLevelCode))]
		[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.UnitOfQuantityList))]
		[MaxLength(Schema.CSI_UnitOfQuantityMaxLength)]
		[ResourceStringData("GBSupportingDocument|UnitOfQuantity", ShortCaption = "UQ", Caption = "Unit of Quantity", MediumCaption = "Unit")]
		public override ZString CSI_UnitOfQuantity { get => base.CSI_UnitOfQuantity; set => base.CSI_UnitOfQuantity = value; }

		public override ZString UnitOfQuantityFieldType => nameof(FieldType.TextDropEdit);

		[ReadOnlyMember(nameof(IsNotItemLevelCode))]
		[ResourceStringData("GBSupportingDocument|MeasurementQualifier", ShortCaption = "Qualifier", Caption = "Measurement Qualifier")]
		[MaxLength(Schema.CSI_UnitOfQuantity2MaxLength)]
		public override ZString CSI_UnitOfQuantity2 { get => base.CSI_UnitOfQuantity2; set => base.CSI_UnitOfQuantity2 = value; }

		protected bool IsNotItemLevelCode
		{
			get
			{
				JobDeclaration dec = null;
				if (Parent is JobComInvoiceLine invLine)
				{
					dec = invLine.Declaration;
				}
				else if (Parent is JobComInvoiceHeader invHeader)
				{
					dec = invHeader.JobDeclaration;
				}

				if (dec?.IsUCCCompliant ?? false)
				{
					if (RefCusCode?.Attributes?.HasAttribute(RefCusCodeListAttributeTypes.Codes.Level, UniversalReferenceConstants.RefCusCodeListAttributes.Values.Item) ?? false)
					{
						return false;
					}
				}
				return true;
			}
		}

		public new SupportingDocumentValidation Validation => (SupportingDocumentValidation)base.Validation;

		public new SupportingDocumentLookups Lookups => (SupportingDocumentLookups)base.Lookups;

		protected override IValueSetStrategy GetValueSetStrategy() => new SupportingDocumentValueSetStrategy(this);

		protected override void SetPropertiesFromCusAuthorisationHeaderCore(CusAuthorisationHeader header)
		{
			var prefix = header.CusAuthorisationRules.FirstOrDefault(r => r.CPR_RuleCode == GBCusAuthorisationRuleTypeBaseList.Codes.CTY)?.CPR_ValueFrom ?? ZString.Empty;
			CSI_ReferenceNumber = $"{(prefix.IsEmpty ? header.CPH_RN_NKCountryCode : prefix)}{header.CPH_Type}{header.CPH_Number}";
		}

		#region Implementation

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return new SupportingDocumentValidation(this);
		}

		protected override CusSupportingInfoLookups GetNewLookups()
		{
			return new SupportingDocumentLookups(this);
		}

		void SetupDataFromCSI_Status()
		{
			csi_Availability = CSI_Status.Left(1);
			csi_Actions = CSI_Status.SubstringSafe(1, 1);
		}

		void PersistToCSI_Status(ZString availability, ZString actions)
		{
			CSI_Status = availability.Left(1).PadRight(1) + actions.Left(1).PadRight(1);
		}

		public bool IsBeingCreatedByItemDefaulter { get; set; }
		#endregion
	}
}
