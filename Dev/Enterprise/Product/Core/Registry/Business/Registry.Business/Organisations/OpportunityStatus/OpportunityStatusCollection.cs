using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class OpportunityStatusCollection : CodeDescriptionBoolCollection
	{
		public OpportunityStatusCollection()
			: this(false, false)
		{
		}

		public OpportunityStatusCollection(bool defaultEffectiveAgreementForNewChild, bool defaultBoolForNewChild)
			: base(null, defaultBoolForNewChild, 3)
		{
			DefaultEffectiveAgreementForNewChild = defaultEffectiveAgreementForNewChild;
		}

		readonly bool DefaultEffectiveAgreementForNewChild;

		public new OpportunityStatus this[int i]
		{
			get { return (OpportunityStatus)base[i]; }
		}

		public new OpportunityStatus AddNew()
		{
			return (OpportunityStatus)base.AddNew();
		}

		protected override CargoWise.EntityFramework.BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OpportunityStatus();
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new OpportunityStatusCollection(DefaultEffectiveAgreementForNewChild, DefaultBoolForNewChild);
		}

		public OpportunityStatus Add(ZString code, MultilingualString description, bool effectiveAgreement, bool booleanValue, bool enabled, ZString tradeStatus)
		{
			var result = (OpportunityStatus)base.Add(code, description, booleanValue);
			result.EffectiveAgreement = effectiveAgreement;
			result.Enabled = enabled;
			result.TradeStatus = tradeStatus;
			return result;
		}

		public bool GetEffectiveAgreementFromCode(string code)
		{
			var element = (OpportunityStatus)FindByCode(code);
			return (element == null) ? ZBool.False : element.EffectiveAgreement;
		}

		public ZString GetTradeStatusFromCode(string code)
		{
			return ((OpportunityStatus)FindByCode(code))?.TradeStatus ?? ZString.Empty;
		}
	}
}
