using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(AddInfoCusEntryLine))]
	sealed class AddInfoCusEntryLineBOTest : AddInfoBOTest
	{
		protected override Type GetExpectedLookupsType()
		{
			return typeof(AddInfoCusEntryLineLookups);
		}

		protected override Type GetExpectedValidationType()
		{
			return typeof(AddInfoCusEntryLineValidation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			return new AddInfoCusEntryLine(entryLine.CL_AddInfoInfo);
		}
	}
}
