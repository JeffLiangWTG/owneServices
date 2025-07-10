using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("Description")]
	public class RequiredDocumentsWrapper : GenericWrapper
	{
		public RequiredDocumentsWrapper(JobRequiredDocument docRequired, BusinessObjectFactory factoryToWrap)
			: base(docRequired, factoryToWrap)
		{
		}

		public static RequiredDocumentsWrapper New(BusinessObject docRequired, BusinessObjectFactory factoryToWrap)
		{
			return docRequired != null ? new RequiredDocumentsWrapper((JobRequiredDocument)docRequired, factoryToWrap) : null;
		}

		protected JobRequiredDocument DocRequired
		{
			get { return (JobRequiredDocument)WrappedObject; }
		}

		public ZString Type
		{
			get { return DocRequired != null ? DocRequired.EQ_DocType : ZString.Empty; }
		}

		public ZString Description
		{
			get { return DocRequired != null ? DocRequired.EQ_DocDescriptionMultilingual : ZString.Empty; }
		}

		public ZDateTimeOffset DateReceived
		{
			get { return DocRequired != null ? DocRequired.EQ_DateReceived : ZDateTimeOffset.Empty; }
		}

		public ZBool IsReceived
		{
			get { return DocRequired != null && !DocRequired.EQ_DateReceived.IsEmpty; }
		}

		public ZBool IsOriginalRequired
		{
			get { return DocRequired != null ? DocRequired.EQ_OriginalDocRequired : ZBool.False; }
		}

		internal bool IsCorrectUsage
		{
			get { return IsAnyDocument || (IsExportDocument && IsExportUsage) || (IsImportDocument && IsImportUsage); }
		}

		ZString Usage
		{
			get { return DocRequired != null ? DocRequired.EQ_DocUsage : ZString.Empty; }
		}

		bool IsExportUsage
		{
			get
			{
				return Usage == JobRequiredDocument.DocUsage.Export
					|| Usage == JobRequiredDocument.DocUsage.Both
					|| Usage == JobRequiredDocument.DocUsage.Domestic;
			}
		}

		bool IsImportUsage
		{
			get
			{
				return Usage == JobRequiredDocument.DocUsage.Import
					|| Usage == JobRequiredDocument.DocUsage.Both
					|| Usage == JobRequiredDocument.DocUsage.Domestic;
			}
		}
	}
}
