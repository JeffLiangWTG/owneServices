using System;
using System.Collections.Generic;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(JobWorkGridColumnsLayout))]
sealed class JobWorkGridColumnsLayoutTest : GridColumnLayoutProviderAbstractTest<JobWorkGridColumnsLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		(JobWork.Schema.CSI_LineNo, typeof(ZCalcEditColumnStyleInfo), 50),
		(JobWork.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 100),
		(JobWork.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyleInfo), 100),
		(JobWork.Schema.CSI_CustomsOffice, typeof(ZTextBoxColumnStyleInfo), 100),
		(JobWork.Schema.CSI_ReferenceNumber2, typeof(ZTextBoxColumnStyleInfo), 100),
		(JobWork.Schema.CSI_ItemNumber, typeof(ZCalcEditColumnStyleInfo), 100),
		(JobWork.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyleInfo), 100),
		(JobWork.Schema.CSI_UnitOfQuantity, typeof(ZTextBoxColumnStyleInfo), 100)
	};

	protected override Type GridBoundEntityType => typeof(JobWork);
}

