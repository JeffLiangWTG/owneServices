using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class StampDutyRecharge : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string StampDutyRechargeOrganizationType = "StampDutyRechargeOrganizationType";
			public const string StampDutyRechargeTransactionType = "StampDutyRechargeTransactionType";
		}

		#endregion

		public StampDutyRecharge()
			: base()
		{
		}

		public StampDutyRecharge(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new StampDutyRecharge(fallbackLevel, null);
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			StampDutyRechargeOrganizationType = Constants.StampDutyRechargeOrganizationType.All;
			StampDutyRechargeTransactionType = Constants.StampDutyRechargeTransactionType.All;
		}

		#region Bound Properties

		#region Stamp Duty Recharge Organization Type

		ZString fStampDutyRechargeOrganizationType;

		[MaxLength(3)]
		[List("StampDutyRechargeOrganizationTypeList")]
		public ZString StampDutyRechargeOrganizationType
		{
			get { return fStampDutyRechargeOrganizationType; }
			set
			{
				SetNonPersistentPropertyValue(StampDutyRechargeOrganizationTypeInfo, ref fStampDutyRechargeOrganizationType, value);
				if (!IsValidationSuspended)
				{
					ValidateStampDutyRechargeOrganizationType();
				}
			}
		}

		public ZPropertyInfo StampDutyRechargeOrganizationTypeInfo
		{
			get { return GetZPropertyInfo(Schema.StampDutyRechargeOrganizationType, "Variance Calculation Style"); }
		}

		#endregion

		#region Stamp Duty Recharge Transaction Type

		ZString fStampDutyRechargeTransactionType;

		[MaxLength(3)]
		[List("StampDutyRechargeTransactionTypeList")]
		public ZString StampDutyRechargeTransactionType
		{
			get { return fStampDutyRechargeTransactionType; }
			set
			{
				SetNonPersistentPropertyValue(StampDutyRechargeTransactionTypeInfo, ref fStampDutyRechargeTransactionType, value);
				if (!IsValidationSuspended)
				{
					ValidateStampDutyRechargeTransactionType();
				}
			}
		}

		public ZPropertyInfo StampDutyRechargeTransactionTypeInfo
		{
			get { return GetZPropertyInfo(Schema.StampDutyRechargeTransactionType, "Stamp Duty Recharge Transaction Type"); }
		}

		#endregion

		#endregion

		#region Lists

		CodeDescriptionPairList fStampDutyRechargeOrganizationTypeList;
		public CodeDescriptionPairList StampDutyRechargeOrganizationTypeList
		{
			get
			{
				if (fStampDutyRechargeOrganizationTypeList == null)
				{
					fStampDutyRechargeOrganizationTypeList = new CodeDescriptionPairList();
					fStampDutyRechargeOrganizationTypeList.AddPair(Constants.StampDutyRechargeOrganizationType.All, Res.GetString("2e3171a0-ce1c-4380-aac4-a3312b16a41f", "AR Organizations in Any Country/Region"));
					fStampDutyRechargeOrganizationTypeList.AddPair(Constants.StampDutyRechargeOrganizationType.LocalOrganizations, Res.GetString("80c52490-ecc0-4260-8e40-066d2c599f19", "Local AR Organizations Only / Login Company’s Country/Region Only"));
					fStampDutyRechargeOrganizationTypeList.AddPair(Constants.StampDutyRechargeOrganizationType.NotRecharging, Res.GetString("d4c1f2e0-38b1-477c-b944-61ea0a4e79c0", "DO NOT Recharge Stamp Duty to AR Organizations"));
				}
				return fStampDutyRechargeOrganizationTypeList;
			}
		}

		CodeDescriptionPairList fStampDutyRechargeTransactionTypeList;
		public CodeDescriptionPairList StampDutyRechargeTransactionTypeList
		{
			get
			{
				if (fStampDutyRechargeTransactionTypeList == null)
				{
					fStampDutyRechargeTransactionTypeList = new CodeDescriptionPairList();
					fStampDutyRechargeTransactionTypeList.AddPair(Constants.StampDutyRechargeTransactionType.All, Res.GetString("bc962cf9-de35-429e-a805-33149212f11a", "Both AR Invoice and AR Credit Note Transactions"));
					fStampDutyRechargeTransactionTypeList.AddPair(Constants.StampDutyRechargeTransactionType.ARInvoice, Res.GetString("83995445-ca92-4cb4-aec2-908e5e83c9c6", "AR Invoice Only"));
				}
				return fStampDutyRechargeTransactionTypeList;
			}
		}

		#endregion

		#region Validation

		public void ValidateStampDutyRechargeOrganizationType()
		{
			StampDutyRechargeOrganizationTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(StampDutyRechargeOrganizationTypeInfo);
			ListValidation.ErrorIfInvalidCode(StampDutyRechargeOrganizationTypeInfo, StampDutyRechargeOrganizationTypeList);
		}

		public void ValidateStampDutyRechargeTransactionType()
		{
			StampDutyRechargeTransactionTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(StampDutyRechargeTransactionTypeInfo);
			ListValidation.ErrorIfInvalidCode(StampDutyRechargeTransactionTypeInfo, StampDutyRechargeTransactionTypeList);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateStampDutyRechargeOrganizationType();
			ValidateStampDutyRechargeTransactionType();
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.StampDutyRechargeOrganizationType, StampDutyRechargeOrganizationType);
			writer.WriteElementString(Schema.StampDutyRechargeTransactionType, StampDutyRechargeTransactionType);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			StampDutyRechargeOrganizationType = reader.ReadElementString(Schema.StampDutyRechargeOrganizationType);
			StampDutyRechargeTransactionType = reader.ReadElementString(Schema.StampDutyRechargeTransactionType);
		}

		#endregion
	}
}