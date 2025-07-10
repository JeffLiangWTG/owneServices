using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

[TestedType(typeof(CusEntryNumberForCC043CMessage))]
sealed class CusEntryNumberForCC043CMessageTest : EnterpriseBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "ORG001";
		orgHeader.OH_FullName = "INTRISNV";

		var listOfValues = new CustomsRegistryCollection();
		var customsRegistryEntry = new CustomsRegistry();
		customsRegistryEntry.Organization = orgHeader.PK;
		customsRegistryEntry.DeclarationType = "TA";
		customsRegistryEntry.StartingDate = new DateTime(2023, 07, 20);
		customsRegistryEntry.StartingNo = 21;

		listOfValues.Add(customsRegistryEntry);

		var registryNumber = Factory.New<CusEntryNumberForCC043CMessage>();
		registryNumber.CE_ParentTable = "CusInBondMoveHeader";
		return registryNumber;
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
}
