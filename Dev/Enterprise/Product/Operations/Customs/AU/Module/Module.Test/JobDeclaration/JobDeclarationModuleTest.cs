using System;
using System.Globalization;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU.CMR;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing;

[TestedType(typeof(JobDeclarationModule))]
sealed class JobDeclarationModuleTest : Customs.Module.Testing.JobDeclarationModuleAbstractTest
{
	protected override string CountryCode => Core.Constants.CountryCodes.Australia;

	protected override Business.BaseJobDeclaration CreateDeclarationForFetchHintTest(BusinessObjectFactory factory, string messageType, int i)
	{
		var declaration = base.CreateDeclarationForFetchHintTest(factory, messageType, i);
		var consolidatedCargoStatuses = factory.GetCachedValue<CMRConsolidatedCargoStatuses>();
		declaration.JE_ConsolidatedCargoStatus = consolidatedCargoStatuses[i % consolidatedCargoStatuses.Count].Code;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.EntryNumber = i.ToString("000", CultureInfo.InvariantCulture);
		entryHeader.CH_ClusterKey = declaration.JE_ClusterKey;
		var quarantineColsHeader = Factory.New<QuarantineColsHeader>();
		quarantineColsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
		quarantineColsHeader.QCH_ClusterKey = declaration.JE_ClusterKey;
		return declaration;
	}

	protected override Type GetExpectedJobDeclarationType() => typeof(JobDeclaration);

	protected override Type GetExpectedInvoiceHeaderType() => typeof(JobComInvoiceHeader);

	protected override Type GetExpectedInvoiceLineType() => typeof(JobComInvoiceLine);
}
