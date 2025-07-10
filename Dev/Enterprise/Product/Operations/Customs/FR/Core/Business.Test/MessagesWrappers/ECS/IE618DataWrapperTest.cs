using System;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.ECS.Testing
{
	public class IE618DataWrapperTest : ECSDataWrapperTest
	{
		protected override Type GetMessageWrapperType() => typeof(IE618EnveloppeMessageWrapper);

		protected override ECSDataWrapper GetNewECSDataWrapper(CusExitDetail exitDetail) => new IE618DataWrapper(exitDetail);
	}
}
