using System;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.IT.NCTS.GUI;

sealed class SupportingDocumentLayoutCaptions
{
	SupportingDocumentLayoutCaptions()
	{
	}

	public static SupportingDocumentLayoutCaptions Instance => instance ?? (instance = new SupportingDocumentLayoutCaptions());

	internal readonly ResourceStringData YearOfIssueCaption = Res.GetData("5279B7CA-C6E6-4506-8BCA-7F0DF1394899", "Year of Issue");

	[ThreadStatic]
	static SupportingDocumentLayoutCaptions instance;
}
