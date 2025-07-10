using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class TaxRecognitionDefaultingRules : RegistryBusinessObjectTemplate
	{
		abstract class Schema
		{
			public const string APInputGoods = "APInputGoods";
			public const string APInputServices = "APInputServices";
			public const string APOrganizationOverride = "APOrganizationOverride";
			public const string AROutputGoods = "AROutputGoods";
			public const string AROutputServices = "AROutputServices";
			public const string AROrganizationOverride = "AROrganizationOverride";
		}

		public TaxRecognitionDefaultingRules(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public TaxRecognitionDefaultingRules()
		{
		}

		public bool IsCashBasisRecognition(string ledger, AccChargeCode chargeCode)
		{
			Argument.NotNullOrEmpty(ledger, "Ledger");

			string resultOption = null;
			var isGoods = chargeCode != null && chargeCode.AC_GoodsServiceType == GoodServiceTypes.Codes.GDS;

			if (ledger == LedgerTypes.AccountsReceivable)
			{
				resultOption = isGoods ? AROutputGoods : AROutputServices;
			}
			else if (ledger == LedgerTypes.AccountsPayable)
			{
				resultOption = isGoods ? APInputGoods : APInputServices;
			}

			return !string.IsNullOrEmpty(resultOption) && resultOption == RecognitionTypesCashCode;
		}

		public bool IsOrganisationOverridePermitted(string ledger)
		{
			Argument.NotNullOrEmpty(ledger, "Ledger");

			string resultOption = null;
			if (ledger == LedgerTypes.AccountsReceivable)
			{
				resultOption = AROrganizationOverride;
			}
			else if (ledger == LedgerTypes.AccountsPayable)
			{
				resultOption = APOrganizationOverride;
			}

			return !string.IsNullOrEmpty(resultOption) && resultOption == OrganisationOverrideTypesYesCode;
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			APInputGoods = RecognitionTypes[0].Code;
			APInputServices = RecognitionTypes[0].Code;
			APOrganizationOverride = OrganisationOverrideTypes[0].Code;
			AROutputGoods = RecognitionTypes[0].Code;
			AROutputServices = RecognitionTypes[0].Code;
			AROrganizationOverride = OrganisationOverrideTypes[0].Code;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TaxRecognitionDefaultingRules(fallbackLevel, factory)
				{
					APInputGoods = APInputGoods,
					APInputServices = APInputServices,
					APOrganizationOverride = APOrganizationOverride,
					AROutputGoods = AROutputGoods,
					AROutputServices = AROutputServices,
					AROrganizationOverride = AROrganizationOverride
				};
		}

		#region Properties

		#region APInputGoods

		[List("RecognitionTypes")]
		[MaxLength(3)]
		public ZString APInputGoods
		{
			get { return apInputGoods; }
			set
			{
				SetNonPersistentPropertyValue(APInputGoodsInfo, ref apInputGoods, value);

				if (!IsValidationSuspended)
				{
					ValidateListProperty(APInputGoodsInfo);
				}
			}
		}
		ZString apInputGoods;

		public ZPropertyInfo APInputGoodsInfo
		{
			get { return GetZPropertyInfo(Schema.APInputGoods); }
		}

		#endregion

		#region APInputServices

		[List("RecognitionTypes")]
		[MaxLength(3)]
		public ZString APInputServices
		{
			get { return apInputServices; }
			set
			{
				SetNonPersistentPropertyValue(APInputServicesInfo, ref apInputServices, value);

				if (!IsValidationSuspended)
				{
					ValidateListProperty(APInputServicesInfo);
				}
			}
		}
		ZString apInputServices;

		public ZPropertyInfo APInputServicesInfo
		{
			get { return GetZPropertyInfo(Schema.APInputServices); }
		}

		#endregion

		#region APOrganizationOverride

		[List("OrganisationOverrideTypes")]
		[MaxLength(3)]
		public ZString APOrganizationOverride
		{
			get { return apOrganizationOverride; }
			set
			{
				SetNonPersistentPropertyValue(APOrganizationOverrideInfo, ref apOrganizationOverride, value);

				if (!IsValidationSuspended)
				{
					ValidateListProperty(APOrganizationOverrideInfo);
				}
			}
		}
		ZString apOrganizationOverride;

		public ZPropertyInfo APOrganizationOverrideInfo
		{
			get { return GetZPropertyInfo(Schema.APOrganizationOverride); }
		}

		#endregion

		#region AROutputGoods

		[List("RecognitionTypes")]
		[MaxLength(3)]
		public ZString AROutputGoods
		{
			get { return arOutputGoods; }
			set
			{
				SetNonPersistentPropertyValue(AROutputGoodsInfo, ref arOutputGoods, value);

				if (!IsValidationSuspended)
				{
					ValidateListProperty(AROutputGoodsInfo);
				}
			}
		}
		ZString arOutputGoods;

		public ZPropertyInfo AROutputGoodsInfo
		{
			get { return GetZPropertyInfo(Schema.AROutputGoods); }
		}

		#endregion

		#region AROutputServices

		[List("RecognitionTypes")]
		[MaxLength(3)]
		public ZString AROutputServices
		{
			get { return arOutputServices; }
			set
			{
				SetNonPersistentPropertyValue(AROutputServicesInfo, ref arOutputServices, value);

				if (!IsValidationSuspended)
				{
					ValidateListProperty(AROutputServicesInfo);
				}
			}
		}
		ZString arOutputServices;

		public ZPropertyInfo AROutputServicesInfo
		{
			get { return GetZPropertyInfo(Schema.AROutputServices); }
		}

		#endregion

		#region AROrganizationOverride

		[List("OrganisationOverrideTypes")]
		[MaxLength(3)]
		public ZString AROrganizationOverride
		{
			get { return arOrganizationOverride; }
			set
			{
				SetNonPersistentPropertyValue(AROrganizationOverrideInfo, ref arOrganizationOverride, value);

				if (!IsValidationSuspended)
				{
					ValidateListProperty(AROrganizationOverrideInfo);
				}
			}
		}
		ZString arOrganizationOverride;

		public ZPropertyInfo AROrganizationOverrideInfo
		{
			get { return GetZPropertyInfo(Schema.AROrganizationOverride); }
		}

		#endregion

		#endregion

		#region Lists

		public CodeDescriptionPairList RecognitionTypes
		{
			get
			{
				if (recognitionTypes == null)
				{
					recognitionTypes = new CodeDescriptionPairList();
					recognitionTypes.AddPair(RecognitionTypesAccrualCode, ResString.GetMultilingualString("746438D3-66B4-4876-B259-634E7A2715D4", "Accrual Basis"));
					recognitionTypes.AddPair(RecognitionTypesCashCode, ResString.GetMultilingualString("2B9CA30D-63A3-4EBF-90CB-61F32B7D1EF4", "Cash Basis"));
				}

				return recognitionTypes;
			}
		}
		CodeDescriptionPairList recognitionTypes;

		public CodeDescriptionPairList OrganisationOverrideTypes
		{
			get
			{
				if (organisationOverrideTypes == null)
				{
					organisationOverrideTypes = new CodeDescriptionPairList();
					organisationOverrideTypes.AddPair(OrganisationOverrideTypesNoCode, ResString.GetMultilingualString("9E8C4909-346A-43B7-A45D-980DE8412971", "Organization Specific Rule not Permitted"));
					organisationOverrideTypes.AddPair(OrganisationOverrideTypesYesCode, ResString.GetMultilingualString("539DE111-1CD6-4AB1-AE40-40C047822AD0", "Organization can be assigned a Specific Override Behavior"));
				}

				return organisationOverrideTypes;
			}
		}
		CodeDescriptionPairList organisationOverrideTypes;

		public const string OrganisationOverrideTypesNoCode = "NO";
		public const string OrganisationOverrideTypesYesCode = "YES";
		public const string RecognitionTypesAccrualCode = "ACR";
		public const string RecognitionTypesCashCode = "CSH";

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateListProperty(APInputGoodsInfo);
			ValidateListProperty(APInputServicesInfo);
			ValidateListProperty(APOrganizationOverrideInfo);
			ValidateListProperty(AROutputGoodsInfo);
			ValidateListProperty(AROutputServicesInfo);
			ValidateListProperty(AROrganizationOverrideInfo);
		}

		void ValidateListProperty(ZPropertyInfo property)
		{
			property.ClearAllNotifications();
			MandatoryValidation.CheckEntered(property);
			ListValidation.ErrorIfInvalidCode(property);
		}

		#endregion

		#region Xml Serialisation

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			APInputGoods = reader.ReadElementString(Schema.APInputGoods);
			APInputServices = reader.ReadElementString(Schema.APInputServices);
			APOrganizationOverride = reader.ReadElementString(Schema.APOrganizationOverride);
			AROutputGoods = reader.ReadElementString(Schema.AROutputGoods);
			AROutputServices = reader.ReadElementString(Schema.AROutputServices);
			AROrganizationOverride = reader.ReadElementString(Schema.AROrganizationOverride);
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.APInputGoods, APInputGoods);
			writer.WriteElementString(Schema.APInputServices, APInputServices);
			writer.WriteElementString(Schema.APOrganizationOverride, APOrganizationOverride);
			writer.WriteElementString(Schema.AROutputGoods, AROutputGoods);
			writer.WriteElementString(Schema.AROutputServices, AROutputServices);
			writer.WriteElementString(Schema.AROrganizationOverride, AROrganizationOverride);
		}

		#endregion
	}
}
