using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(MailboxAndRemoteWebPrintClientCredentialsRegistryDataType))]
	sealed class MailboxAndRemoteWebPrintClientCredentialsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<MailboxAndRemoteWebPrintClientCredentialsRegistryDataType>
	{
		protected override MailboxAndRemoteWebPrintClientCredentialsRegistryDataType GetNewDataType() => new MailboxAndRemoteWebPrintClientCredentialsRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();

			var item1 = new MailboxAndRemoteWebPrintClientCredentials(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), factory);
			item1.LocalComputerAlias = "TYO";
			item1.DomainName = "JPCUS";

			var item2 = new MailboxAndRemoteWebPrintClientCredentials(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), factory);
			item2.LocalComputerAlias = "SHG";
			item2.DomainName = "CNCUS";

			return new[] { new ValidSampleAndBinaryValueInDB(item1, new MailboxAndRemoteWebPrintClientCredentialsRegistryDataType().Serialise(item1)), new ValidSampleAndBinaryValueInDB(item2, new MailboxAndRemoteWebPrintClientCredentialsRegistryDataType().Serialise(item2)) };
		}

		protected override string ExpectedEditorName => "MailboxAndRemoteWebPrintClientCredentialsRegistryItemEditor";
	}
}
