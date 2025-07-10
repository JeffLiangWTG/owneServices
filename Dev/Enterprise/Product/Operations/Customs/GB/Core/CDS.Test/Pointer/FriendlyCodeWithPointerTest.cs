using System.Linq;
using System.Xml.Linq;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Messaging.Testing
{
	[TestedType(typeof(FriendlyCodeWithPointers))]
	sealed class FriendlyCodeWithPointerTest : FriendlyCodeWithPointerAbstractTest<FriendlyCodeWithPointers, MetaData>
	{
		protected override string RejectionMessageXml => @"Enterprise.Customs.GB.CDS.Testing.Pointer.WCORejectionMessage.xml";

		protected override string RequestMessageXml => @"Enterprise.Customs.GB.CDS.Testing.Pointer.WCORequestMessage.xml";

		public override FriendlyCodeWithPointers[] GetResponses(MetaData metaData, XElement requestXml) => metaData.GetResponses().SelectMany(x => x.GetAllFriendlyErrorsForRequestAndRejection(requestXml))
				.ToArray();
	}
}
