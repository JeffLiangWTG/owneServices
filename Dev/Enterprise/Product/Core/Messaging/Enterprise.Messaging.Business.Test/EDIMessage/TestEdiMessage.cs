using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Messaging.Testing
{
	public class TestEdiMessage : EDIMessage
	{
		public TestEdiMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("M", "SENDER", "RECEIVER").GetNextFormatted(Factory);
		}

		protected override string GetContainedChecksum(string messageText)
		{
			return UniqueString;
		}

		protected override CodeDescriptionPairList MessageSubTypeList
		{
			get
			{
				var result = new CodeDescriptionPairList(base.MessageSubTypeList);
				result.AddPair("TTT", "TEST MESSAGE");
				return result;
			}
		}

		protected override string GetOwnerReference()
		{
			return "GetOwnerReferenceResult";
		}

		protected override string GetAgentReference()
		{
			return "Agent Reference";
		}

		protected override string GetEntryNumber()
		{
			return "GetEntryNumberResult";
		}

		protected override string GetBatchNumber()
		{
			return "GetBatchNumberResult";
		}

		protected override ZString GetPkForCommonAccessReference()
		{
			return "PrimaryKeyAsString";
		}

		protected override string GetDocumentMessageVersion()
		{
			return "GetDocumentMessageVersion";
		}

		protected override bool ShouldReplaceUniqueBatchNumber
		{
			get { return ShouldReplaceUniqueBatchNumberExposed; }
		}
		public bool ShouldReplaceUniqueBatchNumberExposed = true;

		public const string UniqueString = "ISureAsHellHopeThisStringIsUnique";
	}
}
