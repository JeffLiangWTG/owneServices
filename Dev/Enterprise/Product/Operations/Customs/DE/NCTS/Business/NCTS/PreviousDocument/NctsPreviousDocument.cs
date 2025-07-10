using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.NCTS.Business
{
	[SystemDefinedValues]
	public class NctsPreviousDocument : EU.NCTS.Business.NctsPreviousDocument, ITariffFormatProvider
	{
		public NctsPreviousDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : EU.NCTS.Business.NctsPreviousDocument.Schema
		{
			public const string Status = nameof(NctsPreviousDocument.Status);
			public const string AuthorizationNumber = nameof(NctsPreviousDocument.AuthorizationNumber);
			public const string SimplifiedGrantAuthorizationFlag = nameof(NctsPreviousDocument.SimplifiedGrantAuthorizationFlag);
			public const string UsualProcessingFlag = nameof(NctsPreviousDocument.UsualProcessingFlag);
			public const string FormattedTariff = nameof(NctsPreviousDocument.FormattedTariff);

			public const int AuthorizationNumberMaxLength = 35;
			public const int ReferenceNumber2MaxLength = 35;
		}

		protected override ZString HumanReadableNameCore => Res.GetString("DF3F1869-A483-4C40-8130-CE7EB6D57DFD", "Reference");

		public new NctsPreviousDocumentValidation Validation => (NctsPreviousDocumentValidation)base.Validation;

		protected override CusSupportingInfoLookups GetNewPhase5Lookups() => new NctsPreviousDocumentLookups(this);

		protected override CusSupportingInfoValidation GetNewPhase5Validation() => new NctsPreviousDocumentValidation(this);

		protected override CusSupportingInfoValidation GetNewPhase4Validation() => new NctsPreviousDocumentValidation(this);

		#region Properties

		[MaxLength(nameof(CSI_Quantity_MaxLength))]
		[DecimalPlaces(nameof(CSI_Quantity_Digit))]
		[ReadOnlyMember(nameof(MainQuantity_ReadOnly))]
		public override ZDecimal CSI_Quantity
		{
			get => base.CSI_Quantity;
			set => base.CSI_Quantity = value;
		}

		int CSI_Quantity_MaxLength => IsProcedureN337 ? 5 : 19;

		public int CSI_Quantity_Digit => IsProcedureN337 ? 0 : 3;

		[ReadOnlyMember(nameof(MainQuantity_ReadOnly))]
		public override ZString CSI_UnitOfQuantity
		{
			get => base.CSI_UnitOfQuantity;
			set => base.CSI_UnitOfQuantity = value;
		}

		[MaxLength(nameof(CSI_Description_MaxLength))]
		public override ZString CSI_Description
		{
			get => base.CSI_Description;
			set => base.CSI_Description = value;
		}

		int CSI_Description_MaxLength
		{
			get
			{
				var result = 26;
				if (IsProcedure9DEZ)
				{
					result = 100;
				}
				else if (IsProcedure9DEY)
				{
					result = 350;
				}
				return result;
			}
		}

		[MaxLength(nameof(CSI_ReferenceNumber_MaxLength))]
		[ReadOnlyMember(nameof(CSI_ReferenceNumberReadOnly))]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set
			{
				var oldValue = base.CSI_ReferenceNumber;
				if (!IsCopying && oldValue != value)
				{
					base.CSI_ReferenceNumber = value;
					CSI_ReferenceNumberInfo.RefreshBinding();
				}
			}
		}

		int CSI_ReferenceNumber_MaxLength
		{
			get
			{
				var result = 70;
				if (IsProcedureN337)
				{
					if (CSI_SubType.In(new ZString[] { PreviousDocSubTypeList.Codes.AWB, PreviousDocSubTypeList.Codes.ULD }))
					{
						result = 44;
					}
					else if (CSI_SubType == PreviousDocSubTypeList.Codes.REG)
					{
						result = 21;
					}
				}
				else if (IsForPreviousDocument && IsInPhase5TransitionPeriod)
				{
					result = 35;
				}
				return result;
			}
		}

		bool CSI_ReferenceNumberReadOnly => ReferenceNumbersShouldBeReadOnly(RefCusCodeListAttributes.Name.Reference);

		[MaxLength(nameof(CSI_ReferenceNumber2_MaxLength))]
		[ReadOnlyMember(nameof(CSI_ReferenceNumber2ReadOnly))]
		public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

		int CSI_ReferenceNumber2_MaxLength => IsForPreviousDocument && IsInPhase5TransitionPeriod ? 26 : Schema.ReferenceNumber2MaxLength;

		bool CSI_ReferenceNumber2ReadOnly => ReferenceNumbersShouldBeReadOnly(RefCusCodeListAttributes.Name.Complement);

		bool ReferenceNumbersShouldBeReadOnly(string attributeCode) => IsForPreviousDocument && (CSI_Code.IsEmpty || RefCusCode != null && RefCusCode.MissesAttribute(attributeCode, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item));

		[MaxLength(nameof(AutoCusSupportingInfo.Schema.CSI_AdditionalDescriptionMaxLength))]
		[ResourceStringData("Enterprise.Customs.DE.Business.NctsPreviousDocument|CSI_AdditionalDescription", Caption = "Complement")]
		public override ZString CSI_AdditionalDescription
		{
			get => base.CSI_AdditionalDescription;
			set => base.CSI_AdditionalDescription = value;
		}

		[ResourceStringData("Enterprise.Customs.DE.Business.NctsPreviousDocument|Status", Caption = "Entry via ATLAS?")]
		public ZBool Status
		{
			get => CSI_Status == YesNoList.Codes.Yes;
			set => CSI_Status = value ? YesNoList.Codes.Yes : YesNoList.Codes.No;
		}
		public ZPropertyInfo StatusInfo => GetWrappedZPropertyInfo(Schema.Status, x => CSI_StatusInfo);

		[BusinessObjectTestExclude]
		public override ZString CSI_Tariff
		{
			get => base.CSI_Tariff;
			set => base.CSI_Tariff = TariffFormatter.Format(value).Left(CSI_TariffInfo.MaxLength);
		}

		[ResourceStringData("Enterprise.Customs.DE.Business.NctsPreviousDocument|FormattedTariff", Caption = "Commodity Code")]
		public ZString FormattedTariff
		{
			get => TariffFormatter.DisplayFormat(CSI_Tariff);
			set => CSI_Tariff = value;
		}

		public ZPropertyInfo FormattedTariffInfo => GetWrappedZPropertyInfo(Schema.FormattedTariff, x => CSI_TariffInfo);

		Customs.Business.TariffFormatter TariffFormatter => tariffFormatter ?? (tariffFormatter = EU.Business.TariffFormatter.New(Core.Constants.CountryCodes.Germany));
		Customs.Business.TariffFormatter tariffFormatter;

		ITariffFormatter ITariffFormatProvider.TariffFormatter => TariffFormatter;

		[ResourceStringData("Enterprise.Customs.DE.Business.NctsPreviousDocument|UsualProcessingFlag", Caption = "Usual Processing Flag")]
		public ZBool UsualProcessingFlag
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.UsualProcessingFlag);
			set
			{
				var oldValue = UsualProcessingFlag;
				if (oldValue != value)
				{
					this.SetSystemDefinedValue(Schema.UsualProcessingFlag, value);
					UsualProcessingFlagInfo.RefreshBinding(oldValue);
					ClearQuantityIfReadOnly();
				}
			}
		}
		public ZPropertyInfo UsualProcessingFlagInfo => GetZPropertyInfo(Schema.UsualProcessingFlag);

		[MaxLength(Schema.AuthorizationNumberMaxLength)]
		public ZString AuthorizationNumber
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.AuthorizationNumber);
			set
			{
				var oldValue = AuthorizationNumber;
				if (oldValue != value)
				{
					CheckMaximumLength(AuthorizationNumberInfo, value);
					this.SetSystemDefinedValue(Schema.AuthorizationNumber, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateAuthorizationNumber();
					}
					AuthorizationNumberInfo.RefreshBinding(oldValue);
				}
			}
		}
		public ZPropertyInfo AuthorizationNumberInfo => GetZPropertyInfo(Schema.AuthorizationNumber);

		public ZBool SimplifiedGrantAuthorizationFlag
		{
			get => CSI_SubType == SimplifiedGrantAuthorizationList.Codes.J;
			set => CSI_SubType = value ? SimplifiedGrantAuthorizationList.Codes.J : SimplifiedGrantAuthorizationList.Codes.N;
		}
		public ZPropertyInfo SimplifiedGrantAuthorizationFlagInfo => GetWrappedZPropertyInfo(Schema.SimplifiedGrantAuthorizationFlag, x => CSI_SubTypeInfo);

		bool MainQuantity_ReadOnly => IsProcedure9DEZ && !UsualProcessingFlag;

		public bool IsProcedure9DEY => Factory.GetValue(ref isProcedure9DEYCached, () => CSI_Procedure == NctsPreviousProcedureList.Codes._9DEY);
		CachedProperty<bool> isProcedure9DEYCached;

		public bool IsProcedure9DEZ => Factory.GetValue(ref isProcedure9DEZCached, () => CSI_Procedure == NctsPreviousProcedureList.Codes._9DEZ);
		CachedProperty<bool> isProcedure9DEZCached;

		public bool IsProcedureN337 => Factory.GetValue(ref isProcedureN337Cached, () => CSI_Procedure == NctsPreviousProcedureList.Codes._N337);
		CachedProperty<bool> isProcedureN337Cached;

		public new NctsDepartureCargoDesc Parent => (NctsDepartureCargoDesc)base.Parent;

		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = base.CSI_Code;
				base.CSI_Code = value;

				if (!IsCopying
					&& oldValue != value
					&& (value == NctsConstants.NctsTypeOfPreviousDocument.Codes.N830 || oldValue == NctsConstants.NctsTypeOfPreviousDocument.Codes.N830)
					&& Parent != null
					&& IsForPreviousDocument)
				{
					CreateOrRemoveAdditionalDocumentInfForN830(Parent);
				}
			}
		}

		#endregion

		public override void Delete()
		{
			var parent = Parent;
			var shouldCreateOrRemoveAdditionalDocumentInfForN830 =
				parent != null && IsForPreviousDocument && CSI_Code == NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
			base.Delete();
			if (shouldCreateOrRemoveAdditionalDocumentInfForN830)
			{
				CreateOrRemoveAdditionalDocumentInfForN830(parent);
			}
		}

		static void CreateOrRemoveAdditionalDocumentInfForN830(NctsDepartureCargoDesc parent)
		{
			var n830Exists = parent.PreviousDocuments
				.Any(d => d.CSI_Code == NctsConstants.NctsTypeOfPreviousDocument.Codes.N830);
			var infExists = parent.AdditionalInfos.Any(d => d.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation && d.CSI_Code == AdditionalInfoCodes._20300);

			if (n830Exists && !infExists)
			{
				var infAddInfo = parent.AdditionalInfos.AddNew();
				infAddInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				infAddInfo.CSI_Code = AdditionalInfoCodes._20300;
			}
			else if (infExists && !n830Exists)
			{
				parent.AdditionalInfos.Where(d => d.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation && d.CSI_Code == AdditionalInfoCodes._20300).DeleteAll();
			}
		}

		void ClearQuantityIfReadOnly()
		{
			if (MainQuantity_ReadOnly)
			{
				CSI_Quantity = ZDecimal.Zero;
				CSI_UnitOfQuantity = ZString.Empty;
			}
		}

		internal bool IsForPreviousDocument => CSI_Procedure.IsEmpty;
	}
}
