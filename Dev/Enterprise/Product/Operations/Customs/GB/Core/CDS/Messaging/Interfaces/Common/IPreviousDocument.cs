using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using CusEntryHeader = Enterprise.Customs.GB.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface IPreviousDocument
	{
		ZString CategoryCode { get; }
		ZString TypeCode { get; }
		ZString ID { get; }
		ZInt LineNumeric { get; }
		ZDateTime SystemCreateTime { get; }
	}

	public class PreviousDocumentWrapper : IPreviousDocument
	{
		PreviousDocumentWrapper(EU.Integration.SadH.IPreviousDocument previousDocument, ZInt lineNumeric, ZDateTime systemCreateTime)
		{
			this.previousDocument = previousDocument;
			this.lineNumeric = lineNumeric;
			this.systemCreateTime = systemCreateTime;
		}

		public static PreviousDocumentWrapper New(PreviousDocument previousDocument)
		{
			return new PreviousDocumentWrapper(new Business.Messaging.PreviousDocumentWrapper(previousDocument), previousDocument.CSI_LineNo, previousDocument.CSI_SystemCreateTimeUtc);
		}

		ZString IPreviousDocument.CategoryCode => previousDocument.Class;

		ZString IPreviousDocument.TypeCode => previousDocument.Type;

		ZString IPreviousDocument.ID => previousDocument.Reference.StripNewlineCharacters(CDSDataElementsLengths.PreviousDocumentIDMaxLength);

		ZInt IPreviousDocument.LineNumeric => lineNumeric;

		ZDateTime IPreviousDocument.SystemCreateTime => systemCreateTime;

		readonly EU.Integration.SadH.IPreviousDocument previousDocument;
		readonly ZInt lineNumeric;
		readonly ZDateTime systemCreateTime;
	}

	public abstract class CustomPreviousDocument : IPreviousDocument
	{
		public CustomPreviousDocument(ZInt lineNumeric, ZString id)
		{
			this.lineNumeric = lineNumeric;
			this.id = id;
			this.systemCreateTime = ZDateTime.UtcNow;
		}

		public virtual ZString CategoryCode => "Z";

		public virtual ZString TypeCode => string.Empty;

		public virtual ZString ID => id == CusEntryHeader.UCRReferencePlaceHolderXmlFriendly ? CusEntryHeader.UCRReferencePlaceHolderXmlFriendly : id.Trim().SubstringSafe(0, 55).ToString();

		public virtual ZInt LineNumeric => lineNumeric;

		public virtual ZDateTime SystemCreateTime => systemCreateTime;

		readonly ZInt lineNumeric;
		readonly ZString id;
		readonly ZDateTime systemCreateTime;
	}

	public class MUCRPreviousDocument : CustomPreviousDocument, IPreviousDocument
	{
		MUCRPreviousDocument(ZInt lineNumeric, ZString id) : base(lineNumeric, id)
		{
		}

		public static MUCRPreviousDocument New(ZInt lineNumeric, ZString id)
		{
			return new MUCRPreviousDocument(lineNumeric, id);
		}

		ZString IPreviousDocument.TypeCode => "MCR";
	}

	public class DCRPreviousDocument : CustomPreviousDocument, IPreviousDocument
	{
		DCRPreviousDocument(ZInt lineNumeric, ZString id) : base(lineNumeric, id)
		{
		}

		public static DCRPreviousDocument New(ZInt lineNumeric, ZString id)
		{
			return new DCRPreviousDocument(lineNumeric, id);
		}

		ZString IPreviousDocument.TypeCode => "DCR";
	}

	public class DCSPreviousDocument : CustomPreviousDocument, IPreviousDocument
	{
		DCSPreviousDocument(ZInt lineNumeric, ZString id) : base(lineNumeric, id)
		{
		}

		public static DCSPreviousDocument New(ZInt lineNumeric, ZString id)
		{
			return new DCSPreviousDocument(lineNumeric, id);
		}

		ZString IPreviousDocument.TypeCode => "DCS";
	}
}
