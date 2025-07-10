using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.Testing;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers.Testing
{
	public class ExpeditionG5SendMessageWrapperTest : WrapperHelperTest<ExpeditionG5SendMessageWrapper>
	{
		protected override ExpeditionG5SendMessageWrapper GetProvider() => new ExpeditionG5SendMessageWrapper(Factory.New<TemporaryStorageHeader>(), Certificate);
	}
}
