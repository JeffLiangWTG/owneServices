using System;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.FR.DocumentWrappers.TempStorageHeader;

public class DocTempStorageLine : DocBaseWrapper
{
	DocTempStorageLine(CusTempStorageLine line, BusinessObjectFactory factoryToWrap)
		: base(line, factoryToWrap)
	{
		Argument.NotNull(line, "line");
	}

	public static DocTempStorageLine New(CusTempStorageLine line, BusinessObjectFactory factoryToWrap)
	{
		return line == null ? null : new DocTempStorageLine(line, factoryToWrap);
	}

	#region Related Business Objects

	CusTempStorageLine Line => (CusTempStorageLine)WrappedObject;

	#endregion

	#region Properties

	public ZInt LineNumber => Line.TSL_LineNo;

	public ZString PackageType => Line.TSL_PackageType;

	public ZInt PackageNumber => Line.TSL_PackageQty;

	public ZString GoodsDescription => Line.TSL_GoodsDescription;

	public ZDecimal GrossWeight => Line.TSL_GrossWeight;

	public ZString Deadline => Line.Dec?.StorageHeader?.SJH_TempStorageEndDateUtc.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? "";

	public ZString SupportingDocument
	{
		get
		{
			var result = new ZStringBuilder();
			Line.Dec?.SupportingDocuments?.Cast<CusSupportingInfo>().ForEach(c => result.AppendLine(FormattableString.Invariant($"{c.CSI_Code} {c.CSI_DateOfIssue.ToString("yyyyMMdd", CultureInfo.InvariantCulture)} {c.CSI_ReferenceNumber}")));
			return result.ToString().Trim();
		}
	}

	#endregion
}
