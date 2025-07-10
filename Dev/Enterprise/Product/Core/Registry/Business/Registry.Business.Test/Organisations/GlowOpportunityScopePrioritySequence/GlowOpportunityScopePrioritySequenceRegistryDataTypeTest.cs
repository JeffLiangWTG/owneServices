using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(GlowOpportunityScopePrioritySequenceRegistryDataType))]
	public class GlowOpportunityScopePrioritySequenceRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<GlowOpportunityScopePrioritySequenceRegistryDataType>
	{
		protected override GlowOpportunityScopePrioritySequenceRegistryDataType GetNewDataType()
		{
			return new GlowOpportunityScopePrioritySequenceRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "GlowOpportunityScopePrioritySequenceRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var defaultValues1 = new GlowOpportunityScopePrioritySequenceCollection();
			defaultValues1.AddNew((NoResString)"Commodity");
			defaultValues1.AddNew((NoResString)"Incoterm");

			var xml1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfGlowOpportunityScopePrioritySequence xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><GlowOpportunityScopePrioritySequence><Description>Commodity</Description></GlowOpportunityScopePrioritySequence><GlowOpportunityScopePrioritySequence><Description>Incoterm</Description></GlowOpportunityScopePrioritySequence></ArrayOfGlowOpportunityScopePrioritySequence>";

			var defaultValues2 = new GlowOpportunityScopePrioritySequenceCollection();
			defaultValues2.AddNew((NoResString)"Service Level");
			defaultValues2.AddNew((NoResString)"Container / ULD");

			var xml2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfGlowOpportunityScopePrioritySequence xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><GlowOpportunityScopePrioritySequence><Description>Service Level</Description></GlowOpportunityScopePrioritySequence><GlowOpportunityScopePrioritySequence><Description>Container / ULD</Description></GlowOpportunityScopePrioritySequence></ArrayOfGlowOpportunityScopePrioritySequence>";

			return
				[
						new ValidSampleAndBinaryValueInDB(defaultValues1, xml1),
						new ValidSampleAndBinaryValueInDB(defaultValues2, xml2),
				];
		}
	}
}
