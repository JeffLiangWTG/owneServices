using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class OpportunityStatus : CodeDescriptionBool
	{
		#region Schema

		new abstract class Schema : CodeDescriptionBool.Schema
		{
			public const string EffectiveAgreement = "EffectiveAgreement";
			public const string Enabled = "Enabled";
			public const string TradeStatus = "TradeStatus";
		}

		#endregion

		#region Properties

		#region Bool

		[ResourceStringData("OpportunityStatus|Bool", Caption = "Auto Close")]
		public override ZBool Bool
		{
			get { return base.Bool; }
			set
			{
				base.Bool = value;
				if (!IsValidationSuspended)
				{
					ValidateBool();
				}
			}
		}

		void ValidateBool()
		{
			BoolInfo.ClearAllNotifications();
			if (!EffectiveAgreementInfo.HasNotifications() && EffectiveAgreement && !Bool)
			{
				BoolInfo.AddError(Res.GetString("efd41eda-6059-479e-bd23-4b00e83f91c4", "Status flagged as Effective Agreement must also be flagged as Auto Close."));
			}
		}

		#endregion

		#region Effective Agreement

		public ZBool EffectiveAgreement
		{
			get { return effectiveAgreement; }
			set
			{
				SetNonPersistentPropertyValue(EffectiveAgreementInfo, ref effectiveAgreement, value);
				if (!IsValidationSuspended)
				{
					ValidateEffectiveAgreement();
				}
			}
		}
		ZBool effectiveAgreement;

		public ZPropertyInfo EffectiveAgreementInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveAgreement); }
		}

		void ValidateEffectiveAgreement()
		{
			EffectiveAgreementInfo.ClearAllNotifications();
			if (!BoolInfo.HasNotifications() && !Bool && EffectiveAgreement)
			{
				EffectiveAgreementInfo.AddError(Res.GetString("efd41eda-6059-479e-bd23-4b00e83f91c4", "Status flagged as Effective Agreement must also be flagged as Auto Close."));
			}

			if (!EffectiveAgreement && EffectiveAgreementInfo.HasChanges)
			{
				EffectiveAgreementInfo.AddWarning(Res.GetString("60DACED2-17AB-47FD-9881-99254B8B0008", "Warning: Disabling an Effective Status will have an impact on Commission Management configurations"));
			}
		}

		#endregion

		#region Enabled

		public ZBool Enabled
		{
			get => enabled;
			set => SetNonPersistentPropertyValue(EnabledInfo, ref enabled, value);
		}
		ZBool enabled;

		public ZPropertyInfo EnabledInfo => GetZPropertyInfo(Schema.Enabled);

		#endregion

		#region Trade Status

		[List("TradeStatusList")]
		public ZString TradeStatus
		{
			get => tradeStatus;
			set
			{
				SetNonPersistentPropertyValue(TradeStatusInfo, ref tradeStatus, value);
				if (!IsValidationSuspended)
				{
					ValidateTradeStatus();
				}
			}
		}
		ZString tradeStatus;

		public CodeDescriptionPairList TradeStatusList => new OpportunityTradeStatus();

		public ZPropertyInfo TradeStatusInfo => GetZPropertyInfo(Schema.TradeStatus);

		void ValidateTradeStatus()
		{
			TradeStatusInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(TradeStatusInfo);
		}

		#endregion

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(ZArchitecture.Environment.FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OpportunityStatus();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var oppStatus = clone as OpportunityStatus;
			oppStatus.EffectiveAgreement = EffectiveAgreement;
			oppStatus.Enabled = Enabled;
			oppStatus.TradeStatus = TradeStatus;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateBool();
			ValidateEffectiveAgreement();
			ValidateTradeStatus();
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			Enabled = true;
		}

		#region XML Serialisation

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			writer.WriteElementString(Schema.EffectiveAgreement, EffectiveAgreement.ToString());
			writer.WriteElementString(Schema.Enabled, Enabled.ToString());
			writer.WriteElementString(Schema.TradeStatus, TradeStatus);
		}

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			EffectiveAgreement = new ZBool(reader.ReadElementString(Schema.EffectiveAgreement));
			Enabled = reader.IsStartElement(Schema.Enabled) ? new ZBool(reader.ReadElementString(Schema.Enabled)) : ZBool.True;
			TradeStatus = reader.ReadElementString(Schema.TradeStatus);
		}

		#endregion
	}
}
