using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.CH.Business.CusEntryHeader;

namespace Enterprise.Customs.CH.Module.Testing;

[TestedType(typeof(JobDeclarationModule))]
sealed class JobDeclarationModuleTest : Customs.Module.Testing.JobDeclarationModuleAbstractTest
{
	protected override string CountryCode => Core.Constants.CountryCodes.Switzerland;

	protected override Type GetExpectedJobDeclarationType() => typeof(JobDeclaration);

	protected override Type GetExpectedInvoiceHeaderType() => typeof(JobComInvoiceHeader);

	protected override Type GetExpectedInvoiceLineType() => typeof(JobComInvoiceLine);

	protected override BaseJobDeclaration CreateDeclarationForFetchHintTest(BusinessObjectFactory factory, string messageType, int i)
	{
		var createdBaseDeclaration = base.CreateDeclarationForFetchHintTest(factory, messageType, i);

		var entryHeader = (CusEntryHeader)createdBaseDeclaration.ActiveEntryHeaders.FirstOrDefault();
		entryHeader.MovementReferenceNumberSetter(entryHeader.MovementReferenceNumber, entryStatus: "1");

		return createdBaseDeclaration;
	}

	protected override bool HasFailedFetchHint(TableHitCount tableSelect)
	{
		bool result = base.HasFailedFetchHint(tableSelect);
		if (tableSelect.TableName == "CusEntryNum" && tableSelect.Value <= 42)
		{
			result = false;
		}

		return result;
	}

	protected override List<string> FetchHintIgnoreField
	{
		get
		{
			var baseIgnoreFields = base.FetchHintIgnoreField;
			baseIgnoreFields.Add("SelectionResultDescription");
			return baseIgnoreFields;
		}
	}
}
