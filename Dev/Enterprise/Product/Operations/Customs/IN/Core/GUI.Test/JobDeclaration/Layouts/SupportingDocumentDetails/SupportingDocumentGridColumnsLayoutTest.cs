using System;
using System.Collections.Generic;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(SupportingDocumentGridColumnsLayout))]
sealed class SupportingDocumentGridColumnsLayoutTest : GridColumnLayoutProviderAbstractTest<SupportingDocumentGridColumnsLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		(SupportingDocument.Schema.CSI_LineNo, typeof(ZTextBoxColumnStyleInfo), 50),
		(SupportingDocument.Schema.CSI_ReferenceNumber2, typeof(ZTextBoxColumnStyleInfo), 100),
		(SupportingDocument.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyleInfo), 100),
		(SupportingDocument.Schema.OrganizationPK, typeof(ZGuidFindBoxColumnStyleInfo), 100),
		(SupportingDocument.Schema.CSI_IssuerType, typeof(ZDropEditColumnStyleInfo), 100),
	};

	protected override Type GridBoundEntityType => typeof(SupportingDocument);
}
