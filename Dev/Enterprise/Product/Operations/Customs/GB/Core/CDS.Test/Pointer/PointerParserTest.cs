using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.Customs.Shared.WorldCustomsOrganisation.MessageDefinitions;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;
using PointerParser = CargoWise.Customs.GB.MessageDefinitions.CDS.PointerParser;

namespace Enterprise.Customs.GB.CDS.Messaging.Testing
{
	[TestedType(typeof(FriendlyCodeWithPointers))]
	sealed class PointerParserTest : PointerParserAbstractTest<FriendlyCodeWithPointers, MetaData>
	{
		protected override MetaData GetMetaData => new FriendlyCodeWithPointerTest().GetRejectionMessageXml();

		protected override IPointerParser Parser => new PointerParser();

		protected override FriendlyCodeWithPointers GetFriendlyCodeWithPointers(ICodeWithPointers c) => new FriendlyCodeWithPointers((PointerParser)Parser, c, null);

		public override IEnumerable<ICodeWithPointers> GetResponses(MetaData metaData) => metaData.GetResponses().SelectMany(x => x.GetCodeWithPointers(res => res.Error));
	}
}
