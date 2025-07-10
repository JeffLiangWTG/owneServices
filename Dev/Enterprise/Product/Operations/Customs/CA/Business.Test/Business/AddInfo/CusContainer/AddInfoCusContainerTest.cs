using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(AddInfoCusContainer))]
	sealed class AddInfoCusContainerTest : AddInfoBOTest
	{
		protected override Type GetExpectedLookupsType()
		{
			return typeof(AddInfoCusContainerLookups);
		}

		protected override Type GetExpectedValidationType()
		{
			return typeof(AddInfoCusContainerValidation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CusContainer container = declaration.CusContainers.AddNew();

			return new AddInfoCusContainer(container.CO_AddInfoInfo);
		}
	}
}
