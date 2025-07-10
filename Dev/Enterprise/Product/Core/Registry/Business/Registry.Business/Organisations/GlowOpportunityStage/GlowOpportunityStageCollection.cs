using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class GlowOpportunityStageCollection : CodeDescriptionBoolCollection
	{
		public GlowOpportunityStageCollection()
			: base(3)
		{
		}

		public new GlowOpportunityStage this[int i]
		{
			get { return (GlowOpportunityStage)base[i]; }
		}

		public new GlowOpportunityStage AddNew()
		{
			return (GlowOpportunityStage)base.AddNew();
		}

		protected override CargoWise.EntityFramework.BusinessObject CreateNonPersistentBusinessObject()
		{
			return new GlowOpportunityStage();
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new GlowOpportunityStageCollection();
		}

		public GlowOpportunityStage Add(ZString code, MultilingualString description, bool enabled, ZInt winProbability)
		{
			var result = (GlowOpportunityStage)base.Add(code, description, enabled);
			result.WinProbability = winProbability;

			return result;
		}
	}
}
