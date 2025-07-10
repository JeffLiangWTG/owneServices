using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.MessageBuilders;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	sealed class IIDMessageWrapperForTest : IIDMessageWrapper, IEDIFACTMessageAttachee
	{
		public IIDMessageWrapperForTest(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
			Argument.NotNull(entryHeader, nameof(entryHeader));
		}

		BusinessObject IEDIFACTMessageAttachee.TopLevelBusinessObject => null;
	}
}
