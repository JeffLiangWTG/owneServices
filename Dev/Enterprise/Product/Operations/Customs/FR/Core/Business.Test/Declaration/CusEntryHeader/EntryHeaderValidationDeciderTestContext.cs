using System;
using Enterprise.Customs.EU.Business.Testing;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class EntryHeaderValidationDeciderTestContext : DeclarationValidationDeciderTestContext<IEntryHeaderValidationDecider>
	{
		public EntryHeaderValidationDeciderTestContext(CusEntryHeader cusEntryHeader, params Type[] ruleDeciderInterfaceTypes) : base(cusEntryHeader.Declaration, false, ruleDeciderInterfaceTypes)
		{
			InitializeConfiguration();
		}

		protected override void SetupConfiguration(Mock<EU.Business.DeclarationConfiguration> configurationMock, IEntryHeaderValidationDecider validationDecider)
		{
			base.SetupConfiguration(configurationMock, validationDecider);

			var entryHeaderConfigurationMock = new Mock<EntryHeaderConfiguration>();

			_ = entryHeaderConfigurationMock.Protected()
				.Setup<EU.Business.Declaration.IEntryHeaderValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<CusEntryHeader>())
				.Returns(validationDecider);

			_ = configurationMock.Protected()
				.Setup<EU.Business.EntryHeaderConfiguration>("GetNewEntryHeaderConfiguration")
				.Returns(entryHeaderConfigurationMock.Object);
		}
	}
}
