using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	sealed public partial class AttachedDocument : IDataObject, IAttachedDocument
	{
		[Mandatory]
		public DocumentType Type { get; set; }
		public CodeDescriptionPair Source { get; set; }
		[Mandatory, MaxLength(277)]
		public ZString? FileName { get; set; }
		public ZBool? IsPublished { get; set; }
		public ZBool IsDisposedByParent { get; set; }
		[Mandatory]
		public SubStreamableStream ImageData { get; set; }
		[MaxLength(3)]
		public ZString? VisibleCompanyCode { get; set; }
		[MaxLength(3)]
		public ZString? VisibleBranchCode { get; set; }
		[MaxLength(3)]
		public ZString? VisibleDepartmentCode { get; set; }
		public ZDateTime? SaveDateUTC { get; set; }
		public Staff SavedBy { get; set; }
		[MaxLength(36)]
		public ZString? DocumentID { get; set; }
		public ZInt? FileSizeInBytes { get; set; }

		public List<Context> ContextCollection { get; set; }

		ICodeDescriptionDataObject IAttachedDocument.Type
		{
			get { return Type; }
			set { Type = (DocumentType)value; }
		}

		ICodeDescriptionDataObject IAttachedDocument.Source
		{
			get { return Source; }
			set { Source = (CodeDescriptionPair)value; }
		}

		ICodeNameDataObject IAttachedDocument.SavedBy
		{
			get { return SavedBy; }
			set { SavedBy = (Staff)value; }
		}

		public void Dispose()
		{
			if (!IsDisposedByParent)
			{
				ImageData?.Dispose();
			}
		}

		public enum ContextTypes
		{
			CustomsDocumentType,
			ImporterBusinessNumber,
			EffectiveDate,
			ExpiryDate
		}
	}
}
