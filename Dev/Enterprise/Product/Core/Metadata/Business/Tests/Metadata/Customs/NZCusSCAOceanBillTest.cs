using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business.Tests
{
	class NZCusSCAOceanBillTest : EnterpriseBusinessObjectTest
	{
		protected internal override NoteTypeCollection ExpectedNoteTypes()
		{
			var noteTypes = base.ExpectedNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.CustomsDeliveryInstructions);
			return noteTypes;
		}

		protected override IMetadata NewMetadata => new NZCusSCAOceanBill();
	}
}
