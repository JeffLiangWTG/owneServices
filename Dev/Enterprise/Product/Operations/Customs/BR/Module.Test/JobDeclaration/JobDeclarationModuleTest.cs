using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(JobDeclarationModule))]
	sealed class JobDeclarationModuleTest : Customs.Module.Testing.JobDeclarationModuleAbstractTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Brazil;

		protected override Type GetExpectedJobDeclarationType() => typeof(JobDeclaration);

		protected override Type GetExpectedInvoiceHeaderType() => typeof(JobComInvoiceHeader);

		protected override Type GetExpectedInvoiceLineType() => typeof(JobComInvoiceLine);

		protected override BaseJobDeclaration CreateDeclarationForFetchHintTest(BusinessObjectFactory factory, string messageType, int i)
		{
			var result = (JobDeclaration)base.CreateDeclarationForFetchHintTest(factory, messageType, i);
			var entryHeader = result.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CargoStatus = BRCargoStatusList.Codes.InTransit;
			entryHeader.CH_EntryReleaseDate = ZDateTime.Today;
			entryHeader.CH_AdministrativeStatus = BRAdministrativeStatusList.Codes.InProcess;
			entryHeader.CH_RiskChannel = RiskChannelList.Codes.Red;

			return result;
		}
	}
}
