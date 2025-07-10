using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class OpportunityClosedReasonsCollection : CodeDescriptionBoolCollection
	{
		public OpportunityClosedReasonsCollection()
		{
			CodeMaxLength = OrgOpportunitySchema.P8_LostReason.MaxLength;
		}

		public CodeDescriptionPairList GetActiveCodeDescriptionPairList(ZString statusCode)
		{
			var list = new CodeDescriptionPairList();
			list.AddRange(this.OfType<OpportunityClosedReasons>().Where(x => x.Bool && ((ICodeDescriptionPairList)x.StatusRules).ContainsCode(statusCode)).ToList());
			return list;
		}

		public new OpportunityClosedReasons this[int i] => (OpportunityClosedReasons)base[i];

		public new OpportunityClosedReasons AddNew() => (OpportunityClosedReasons)base.AddNew();

		public new OpportunityClosedReasons Add(ZString code, MultilingualString description) => (OpportunityClosedReasons)Add(code, description, true);

		protected override BusinessObject CreateNonPersistentBusinessObject() => new OpportunityClosedReasons();

		protected override CodeDescriptionBoolCollection GetNewCollection() => new OpportunityClosedReasonsCollection();

		protected override FallbackLevel CurrentFallbackLevelCore
		{
			get { return base.CurrentFallbackLevelCore; }
			set
			{
				base.CurrentFallbackLevelCore = value;
				foreach (OpportunityClosedReasons item in this)
				{
					item.CurrentFallbackLevel = value;
				}
			}
		}
	}
}
