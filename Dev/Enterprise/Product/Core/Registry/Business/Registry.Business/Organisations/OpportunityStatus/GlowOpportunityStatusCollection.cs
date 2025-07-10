using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class GlowOpportunityStatusCollection : CodeDescriptionBoolCollection
	{
		public GlowOpportunityStatusCollection()
			: base(3)
		{
		}

		public new GlowOpportunityStatus this[int i]
		{
			get { return (GlowOpportunityStatus)base[i]; }
		}

		public new GlowOpportunityStatus AddNew()
		{
			return (GlowOpportunityStatus)base.AddNew();
		}

		protected override CargoWise.EntityFramework.BusinessObject CreateNonPersistentBusinessObject()
		{
			return new GlowOpportunityStatus();
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new GlowOpportunityStatusCollection();
		}

		public GlowOpportunityStatus Add(ZString code, MultilingualString description, bool enabled, ZString tradeStatus)
		{
			var result = (GlowOpportunityStatus)base.Add(code, description, enabled);
			result.TradeStatus = tradeStatus;

			return result;
		}
	}
}
