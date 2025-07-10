using System;
using CargoWise.EntityFramework;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class CusGoodsLocationValidationDeciderTestContext<T> : NctsValidationDeciderTestContext<T>
		where T : class, ICusGoodsLocationValidationDecider
	{
		public CusGoodsLocationValidationDeciderTestContext(BusinessObjectFactory factory, params Type[] ruleDeciderInterfaceTypes)
			: base(factory, ruleDeciderInterfaceTypes)
		{
		}

		protected override void SetupConfiguration(Mock<NctsConfiguration> configuration, T validationDecider)
		{
			var movementHeaderConfiguration = new Mock<MovementHeaderConfiguration> { CallBase = true };
			movementHeaderConfiguration.Protected()
				.Setup<ICusGoodsLocationValidationDecider>("GetGoodsLocationValidationDeciderCore", ItExpr.IsAny<NctsCommonMovementHeader>())
				.Returns(validationDecider);

			configuration.Protected()
				.Setup<MovementHeaderConfiguration>("GetNewMovementHeaderConfiguration")
				.Returns(movementHeaderConfiguration.Object);
		}
	}
}
