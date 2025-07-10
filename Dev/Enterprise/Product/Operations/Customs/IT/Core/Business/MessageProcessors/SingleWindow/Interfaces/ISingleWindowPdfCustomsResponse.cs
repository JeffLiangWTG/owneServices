using System;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface ISingleWindowPdfCustomsResponse
{
	ZString DocumentType { get; }
	ZString Filename { get; }
	ReadOnlyMemory<byte> ImageData { get; }
}
