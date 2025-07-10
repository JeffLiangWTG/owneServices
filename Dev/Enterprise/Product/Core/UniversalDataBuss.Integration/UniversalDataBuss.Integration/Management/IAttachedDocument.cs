using System;
using CargoWise.IO;
using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IAttachedDocument : IDisposable
	{
		ZString? FileName { get; set; }
		SubStreamableStream ImageData { get; set; }
		ZBool? IsPublished { get; set; }
		ZBool IsDisposedByParent { get; set; }
		ICodeDescriptionDataObject Type { get; set; }
		ICodeDescriptionDataObject Source { get; set; }
		ZString? VisibleCompanyCode { get; set; }
		ZString? VisibleBranchCode { get; set; }
		ZString? VisibleDepartmentCode { get; set; }
		ZDateTime? SaveDateUTC { get; set; }
		ICodeNameDataObject SavedBy { get; set; }
	}
}
