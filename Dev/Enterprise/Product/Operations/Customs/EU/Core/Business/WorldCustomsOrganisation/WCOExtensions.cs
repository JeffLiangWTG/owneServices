using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Customs.Shared.WorldCustomsOrganisation.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation
{
	public static class WCOExtensions
	{
		public static EDIMessage GetLatestMessageForComparison(this CusEntryHeader entry)
		{
			return entry.Messages.OfType<EDIMessage>().Where(x => !x.EM_Status.EqualsAny(new ZString[] { EDIMessageStatusList.Codes.Discarded, EDIMessageStatusList.Codes.Rejected }) && x.EM_MessageType.EqualsAny(new ZString[] { WCOEDIMessageTypeList.Codes.NewDeclaration, WCOEDIMessageTypeList.Codes.DMSNewDeclaration, WCOEDIMessageTypeList.Codes.NewAmendment })).OrderBy(x => x.EM_SystemCreateTimeUtc).LastOrDefault();
		}

		public static XElement RemoveAllNamespaces(this XElement xml)
		{
			foreach (var xe in xml.DescendantsAndSelf())
			{
				xe.Name = xe.Name.LocalName;
				xe.ReplaceAttributes((from xattrib in xe.Attributes().Where(xa => !xa.IsNamespaceDeclaration) select new XAttribute(xattrib.Name.LocalName, xattrib.Value)));
			}

			return xml;
		}

		public static int Sequence(this XElement node)
		{
			var currentNodeName = node.Name.LocalName;
			var nodesBefore = node.NodesBeforeSelf().Where(x => ((XElement)x).Name.LocalName == currentNodeName);
			var sequence = (nodesBefore?.Count() ?? 0) + 1;
			return sequence;
		}

		public static ZString JoinAsString(this IEnumerable<ZString> values, string split = "")
		{
			return ZString.Join(split, values.ToArray());
		}

		public static IPointer Find(this ICodeWithPointers provider, ZString documentSectionCode)
		{
			return provider?.Pointers?.FirstOrDefault(c => c.DocumentSectionCode == documentSectionCode);
		}

		public static ZString GetPointers(this ICodeWithPointers provider)
		{
			var pointers = provider?.Pointers ?? Enumerable.Empty<IPointer>();

			return pointers.Select(x => x.ToFriendlyString()).Where(x => !x.IsEmpty).JoinAsString("/");
		}

		static ZString ToFriendlyString(this IPointer pointer)
		{
			return pointer == null
				? string.Empty
				: string.IsNullOrEmpty(pointer.TagId)
					? pointer.SequenceNumeric > ZDecimal.Zero
						? string.Format("{0}[{1}]", pointer.DocumentSectionCode, pointer.SequenceNumeric)
						: pointer.DocumentSectionCode
					: pointer.SequenceNumeric > ZDecimal.Zero
						? string.Format("{0}[{1}]/{2}[1]", pointer.DocumentSectionCode, pointer.SequenceNumeric, pointer.TagId)
						: string.Format("{0}/{1}[1]", pointer.DocumentSectionCode, pointer.TagId);
		}
	}
}
