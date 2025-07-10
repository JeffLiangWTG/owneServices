using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Core.Testing
{
	[TestedType(typeof(MessageNumber))]
	class MessageNumberTest : DataObjectTestCase<MessageNumber>
	{
		protected override bool ShouldBeFlattenedIntoAttributes => true;
	}
}
