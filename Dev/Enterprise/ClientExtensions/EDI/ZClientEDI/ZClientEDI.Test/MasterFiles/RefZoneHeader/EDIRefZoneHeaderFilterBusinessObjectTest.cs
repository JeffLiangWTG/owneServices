using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Module.Testing
{
	[TestedType(typeof(EDIRefZoneHeaderFilterBusinessObject))]
	public class EDIRefZoneHeaderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Implementation
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EDIRefZoneHeaderFilterBusinessObject();
		}

		#endregion
		public void ZoneTypeListFilter()
		{
			EDIRefZoneHeaderFilterBusinessObject filter = new EDIRefZoneHeaderFilterBusinessObject();
			Assert("Should contain EDI specific zone 'TRN' for training", ((CodeDescriptionPairList)(((ModuleTextFilter)filter["Zone Type"]).List)).ContainsCode(EDIRefZoneHeaderLookups.EDIZoneTypeCodes.Training));
		}
	}
}
