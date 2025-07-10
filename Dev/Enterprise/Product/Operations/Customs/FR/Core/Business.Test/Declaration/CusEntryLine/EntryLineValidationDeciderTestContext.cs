using System;
using Enterprise.Customs.EU.Business.Testing;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class EntryLineValidationDeciderTestContext : DeclarationValidationDeciderTestContext<IEntryLineValidationDecider>
	{
		public EntryLineValidationDeciderTestContext(CusEntryLine cusEntryLine, params Type[] ruleDeciderInterfaceTypes) : base(cusEntryLine.Declaration, false, ruleDeciderInterfaceTypes)
		{
			InitializeConfiguration();
		}

		protected override void SetupConfiguration(Mock<EU.Business.DeclarationConfiguration> configurationMock, IEntryLineValidationDecider validationDecider)
		{
			base.SetupConfiguration(configurationMock, validationDecider);

			var entryHeaderConfigurationMock = new Mock<EntryLineConfiguration>();

			_ = entryHeaderConfigurationMock.Protected()
				.Setup<EU.Business.Declaration.IEntryLineValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<CusEntryLine>())
				.Returns(validationDecider);

			_ = configurationMock.Protected()
				.Setup<EU.Business.EntryLineConfiguration>("GetNewEntryLineConfiguration")
				.Returns(entryHeaderConfigurationMock.Object);
		}
	}
}
