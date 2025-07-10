using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocJobRequiredDocument : DocBaseWrapper
	{
		protected DocJobRequiredDocument(JobRequiredDocument docRequired, BusinessObjectFactory factoryToWrap)
			: base(docRequired, factoryToWrap)
		{
		}

		public static DocJobRequiredDocument New(JobRequiredDocument docRequired, BusinessObjectFactory factoryToWrap)
		{
			return docRequired != null ? new DocJobRequiredDocument(docRequired, factoryToWrap) : null;
		}

		protected JobRequiredDocument DocRequired
		{
			get { return (JobRequiredDocument)WrappedObject; }
		}

		public ZString Type
		{
			get { return DocRequired.EQ_DocType; }
		}

		public ZString Description
		{
			get { return DocRequired.EQ_DocDescriptionMultilingual; }
		}

		public ZDateTimeOffset DateReceived
		{
			get { return DocRequired.EQ_DateReceived; }
		}

		public ZBool IsReceived
		{
			get { return !DocRequired.EQ_DateReceived.IsEmpty; }
		}

		public ZBool IsOriginalRequired
		{
			get { return DocRequired.EQ_OriginalDocRequired; }
		}
	}
}
