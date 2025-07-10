using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Netting
{
	public class NettingCalculation : AutoNettingCalculation
	{
		public NettingCalculation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("MatchingPivot")]
		public override ZGuid NPC_NMP_Pivot
		{
			get { return base.NPC_NMP_Pivot; }
			set { base.NPC_NMP_Pivot = value; }
		}

		public NettingMatchPivot MatchingPivot
		{
			get { return Factory.Load<NettingMatchPivot>(NPC_NMP_Pivot); }
		}

		[RelatedBusinessObject("Organization")]
		public override ZGuid NPC_NSO_Organisation
		{
			get { return base.NPC_NSO_Organisation; }
			set { base.NPC_NSO_Organisation = value; }
		}

		public NettingOrganisation Organization
		{
			get { return Factory.Load<NettingOrganisation>(NPC_NSO_Organisation); }
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			NPC_NettingOrganisationAmount = 1M;
			NPC_NettingRate = 1M;
			NPC_NettingSystemAmount = 1M;
		}
#endif
	}
}
