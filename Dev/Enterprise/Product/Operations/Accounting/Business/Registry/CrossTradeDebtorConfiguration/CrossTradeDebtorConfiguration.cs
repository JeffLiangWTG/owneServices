using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public partial class CrossTradeDebtorConfiguration : ChargeGroupSetting, IJobConfigurationSelector
	{
		#region Schema

		public new abstract class Schema : ChargeGroupSetting.Schema
		{
			public const string ChargePaymentType = "ChargePaymentType";
			public const string Debtor = "Debtor";
		}

		#endregion

		public override IJobConfigurationSelector[] ParentCollectionForValidation
		{
			get
			{
				var parentCollection = GetParentCollection(this, typeof(CrossTradeDebtorConfigurationCollection));
				return parentCollection != null ? parentCollection.Cast<CrossTradeDebtorConfiguration>().ToArray() : Array.Empty<CrossTradeDebtorConfiguration>();
			}
		}

		public CrossTradeDebtorConfiguration[] CollectionForValidation
		{
			get
			{
				return (CrossTradeDebtorConfiguration[])ParentCollectionForValidation;
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CrossTradeDebtorConfiguration();
		}

		#region Default Values

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			DirectionCode = Constants.FreightShipmentDirection.Code.Other;
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateChargePaymentType();
			ValidateDebtor();
		}

		public new CrossTradeDebtorConfigurationValidation Validation
		{
			get { return (CrossTradeDebtorConfigurationValidation)base.Validation; }
		}

		protected override JobConfigurationSelectorValidation GetNewValidation()
		{
			return new CrossTradeDebtorConfigurationValidation(this);
		}

		#endregion

		#region JobType

		[MaxLength(3)]
		[List("JobTypeList")]
		public override ZString JobType
		{
			get
			{
				return jobType;
			}
			set
			{
				CheckMaximumLength(JobTypeInfo, value);
				SetNonPersistentPropertyValue(JobTypeInfo, ref jobType, value);
				if (!IsValidationSuspended)
				{
					ValidateJobType();
				}
			}
		}

		ZString jobType;

		#endregion

		#region DirectionCode

		public override bool DirectionCode_ReadOnly => true;

		#endregion

		#region Mode

		public override bool Mode_ReadOnly => false;

		#endregion

		#region Debtor

		[List(nameof(DebtorOptionList))]
		public ZString Debtor
		{
			get { return debtor; }
			set
			{
				CheckMaximumLength(DebtorInfo, value);
				SetNonPersistentPropertyValue(DebtorInfo, ref debtor, value);
				if (!IsValidationSuspended)
				{
					ValidateDebtor();
				}
			}
		}
		public ZPropertyInfo DebtorInfo
		{
			get { return GetZPropertyInfo(Schema.Debtor); }
		}

		public void ValidateDebtor()
		{
			DebtorInfo.ClearAllNotifications();

			Validation.ValidateDebtor();
		}

		public CodeDescriptionPairList DebtorOptionList => CrossTradeConfigurationLookUp.DebtorOptionList;

		ZString debtor;

		#endregion

		#region Charge Payment Type

		[MaxLength(3)]
		[List(nameof(ChargePaymentTypeList))]
		public ZString ChargePaymentType
		{
			get { return chargePaymentType; }
			set
			{
				CheckMaximumLength(ChargePaymentTypeInfo, value);
				SetNonPersistentPropertyValue(ChargePaymentTypeInfo, ref chargePaymentType, value);
				if (!IsValidationSuspended)
				{
					ValidateChargePaymentType();
				}
			}
		}

		public ZPropertyInfo ChargePaymentTypeInfo
		{
			get { return GetZPropertyInfo(Schema.ChargePaymentType); }
		}

		public void ValidateChargePaymentType()
		{
			ChargePaymentTypeInfo.ClearAllNotifications();

			Validation.ValidateChargePaymentType();
		}

		public CodeDescriptionPairList ChargePaymentTypeList => CrossTradeConfigurationLookUp.ChargePaymentTypeList;

		ZString chargePaymentType;

		#endregion

		#region CrossTradeDebtorConfigurationLookup

		public CrossTradeDebtorConfigurationLookups CrossTradeConfigurationLookUp
		{
			get { return (CrossTradeDebtorConfigurationLookups)ChargeGroupSettingLookups; }
		}

		protected override JobConfigurationSelectorLookups GetNewLookups()
		{
			return new CrossTradeDebtorConfigurationLookups(this);
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.ChargePaymentType, ChargePaymentType);
			writer.WriteElementString(Schema.Debtor, Debtor);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);

			ChargePaymentType = reader.ReadElementString(Schema.ChargePaymentType);
			Debtor = reader.ReadElementString(Schema.Debtor);
		}

		#endregion
	}
}
