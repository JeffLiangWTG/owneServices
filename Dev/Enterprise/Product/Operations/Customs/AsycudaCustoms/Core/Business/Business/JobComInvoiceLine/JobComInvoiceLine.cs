using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public partial class JobComInvoiceLine : BaseJobComInvoiceLine
		, Integration.Customs.AsycudaCustoms.IJobComInvoiceLine
		, ISupportingDocumentsProvider
	{
		public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : BaseJobComInvoiceLine.Schema
		{
			public const string JI_CEI_Description = "JI_CEI_Description";
		}

		#region Implementation
		public BaseApplicationBusinessProvider ApplicationBusinessProvider
		{
			get
			{
				if (InvoiceHeader is JobComInvoiceHeader header)
				{
					return header.ApplicationBusinessProvider;
				}
				else
				{
					return BaseApplicationBusinessProvider.GetApplicationBusinessProvider(Factory, Declaration?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				}
			}
		}

		public override ZString UniversalTariffType => ApplicationBusinessProvider?.UniversalTariffType ?? base.UniversalTariffType;

		#region GetTariffDescription - to be overridden once the Tariff is setup for a new country
		protected override ZString GetTariffDescription(ZString tariffCode)
		{
			return ZString.Empty;
		}
		#endregion

		#endregion

		#region Override Properties

		[ResourceStringData("55F0A0AC-E1C1-4DA1-8F94-C54C5FA98666", Caption = "Previous Entry Number", ShortCaption = "Prev. Entry #")]
		public override ZString JI_PreviousEntryNumber
		{
			get { return base.JI_PreviousEntryNumber; }
			set { base.JI_PreviousEntryNumber = value; }
		}

		[ResourceStringData("17ABAA71-F605-4304-9CEB-A720C765CAB0", Caption = "Previous Entry Line Number", ShortCaption = "Prev. Entry Line #")]
		public override ZShort JI_PreviousEntryLineNumber
		{
			get { return base.JI_PreviousEntryLineNumber; }
			set
			{
				var oldValue = base.JI_PreviousEntryLineNumber;
				base.JI_PreviousEntryLineNumber = value;
				if (!IsCopying && oldValue != JI_PreviousEntryLineNumber)
				{
					Validation.ValidateJI_PreviousEntryNumber();
				}
			}
		}

		public override ZGuid JI_CEI
		{
			get => base.JI_CEI;
			set
			{
				base.JI_CEI = value;
				Declaration?.MarkAsNeedingValidation();
			}
		}

		public override ZString JI_Procedure
		{
			get => base.JI_Procedure;
			set
			{
				base.JI_Procedure = value;
				Declaration?.MarkAsNeedingValidation();
			}
		}

		protected override void SetDefaultTaxOrFeeCode()
		{
			var taxOrFeeCode = UniversalTariff?.ZZ1_ZZF_NKTaxOrFeeCode ?? ZString.Empty;
			if (!taxOrFeeCode.IsEmpty)
			{
				JI_ZZF_NKTaxType = taxOrFeeCode;
			}
			else
			{
				base.SetDefaultTaxOrFeeCode();
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		[ResourceStringData("6dc8ab5b-2726-44b1-b030-0f290b6d8d0d", Caption = "UQ")]
		public override ZString JI_CustomsSecondUnitQty
		{
			get => base.JI_CustomsSecondUnitQty;
			set => base.JI_CustomsSecondUnitQty = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		[ResourceStringData("f2814810-31cc-4a84-a168-bf9688fc50b7", Caption = "UQ")]
		public override ZString JI_CustomsThirdUnitQty
		{
			get => base.JI_CustomsThirdUnitQty;
			set => base.JI_CustomsThirdUnitQty = value;
		}

		#region JI_CEI_Description

		[ResourceStringData("2ab38ade-b37e-4feb-9583-1e3d71c62d4d", ShortCaption = "Entry Ins. Desc.", Caption = "Entry Ins. Description", FullDescription = "Entry Instruction Description")]
		public ZString JI_CEI_Description => EntryInstruction?.CEI_Description ?? ZString.Empty;

		public ZPropertyInfo JI_CEI_DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.JI_CEI_Description); }
		}

		#endregion

		protected override bool ShouldSetDescriptionWhenTariffChanges => false;

		public override ZDateTime EffectiveAssessmentDate
		{
			get
			{
				var dateForDuty = EntryInstruction?.CEI_DateForDuty ?? ZDateTime.Empty;
				return dateForDuty.IsValid ? dateForDuty : base.EffectiveAssessmentDate;
			}
		}

		#endregion

		#region CusSupportingInfo Related

		SupportingDocumentCollection fSupportingDocuments;

		[ChildEditable(true)]
		public SupportingDocumentCollection SupportingDocuments
		{
			get
			{
				if (fSupportingDocuments == null)
				{
					fSupportingDocuments = new SupportingDocumentCollection(this);
					fSupportingDocuments.Load();
					RegisterEditableChildObject(fSupportingDocuments);
				}
				return fSupportingDocuments;
			}
		}

		IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			return new Dictionary<ZString, Type> {
				{ Constants.CusSupportingInfoTypes.CusSupportingDocument, typeof(SupportingDocument) }
			};
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		ZGuid Integration.Customs.ICusSupportingInfoTypeSupporter.PK => PK;

		BusinessObjectFactory Integration.Customs.ICusSupportingInfoTypeSupporter.Factory => Factory;

		bool Integration.Customs.ICusSupportingInfoTypeSupporter.IsInDatabase => IsInDatabase;

		#endregion

		[ResourceStringData("40EBCA69-BE8E-4BD5-96C6-90614888EB47", Caption = "VIN")]
		[MaxLength(CusVehicle.Schema.CVH_VehicleIdentificationNumberMaxLength)]
		public ZString VehicleVIN
		{
			get => FirstVehicle.CVH_VehicleIdentificationNumber;
			set
			{
				var oldValue = VehicleVIN;
				CheckMaximumLength(VehicleVINInfo, value);
				FirstVehicle.CVH_VehicleIdentificationNumber = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidVehicleVIN();
				}
				VehicleVINInfo.RefreshBinding(oldValue);
			}
		}

		protected override ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategy()
		{
			return new UniversalRateCustomsUnitDefaultingStrategy<JobComInvoiceLine>();
		}

		public ZPropertyInfo VehicleVINInfo => GetZPropertyInfo(nameof(VehicleVIN));

		protected override ICusVehicleCollection<Customs.Business.CusVehicle, BaseJobComInvoiceLine> GetNewCusVehicleCollection() => new CusVehicleCollection<CusVehicle, JobComInvoiceLine>(this);

		public override VehicleRelationshipType VehicleRelationship => VehicleRelationshipType.One;

		protected override ZArchitecture.Business.EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobComInvoiceLineFetchStrategy(this);
		}
		protected override Type TypeOfPartUsedCore => typeof(OrgSupplierPart);
	}
}
