using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(AddInfoCusEntryHeader))]
	sealed class AddInfoCusEntryHeaderBOTest : AddInfoBOTest
	{
		protected override Type GetExpectedLookupsType()
		{
			return typeof(AddInfoCusEntryHeaderLookups);
		}

		protected override Type GetExpectedValidationType()
		{
			return typeof(AddInfoCusEntryHeaderValidation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return new AddInfoCusEntryHeader(entryHeader.CH_AddInfoInfo);
		}
	}
}
