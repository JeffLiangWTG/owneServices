using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class GlowOpportunityStatus : CodeDescriptionBool
	{
		#region Schema

		new abstract class Schema : CodeDescriptionBool.Schema
		{
			public const string TradeStatus = "TradeStatus";
		}

		#endregion

		#region Properties

		#region Trade Status

		[List(nameof(TradeStatusList))]
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
		ZString tradeStatus = OpportunityTradeStatus.Codes.Active;

		public CodeDescriptionPairList TradeStatusList => new OpportunityTradeStatus();

		public ZPropertyInfo TradeStatusInfo => GetZPropertyInfo(Schema.TradeStatus);

		void ValidateTradeStatus()
		{
			TradeStatusInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(TradeStatusInfo);
			MandatoryValidation.CheckEntered(TradeStatusInfo);
		}

		#endregion

		#endregion

		protected override bool IsDescriptionMandatory => true;

		protected override RegistryBusinessObjectTemplate GetClone(ZArchitecture.Environment.FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new GlowOpportunityStatus();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var oppStatus = clone as GlowOpportunityStatus;
			oppStatus.TradeStatus = TradeStatus;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateTradeStatus();
		}

		#region XML Serialisation

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			writer.WriteElementString(Schema.TradeStatus, TradeStatus);
		}

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			TradeStatus = reader.ReadElementString(Schema.TradeStatus);
		}

		#endregion
	}
}
