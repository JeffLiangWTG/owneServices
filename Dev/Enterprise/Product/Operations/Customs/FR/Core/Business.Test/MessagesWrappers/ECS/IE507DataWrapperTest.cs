using System;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.ECS.Testing
{
	public class IE507DataWrapperTest : ECSDataWrapperTest
	{
		protected override Type GetMessageWrapperType() => typeof(IE507EnveloppeMessageWrapper);

		protected override ECSDataWrapper GetNewECSDataWrapper(CusExitDetail exitDetail) => new IE507DataWrapper(exitDetail);
	}
}
