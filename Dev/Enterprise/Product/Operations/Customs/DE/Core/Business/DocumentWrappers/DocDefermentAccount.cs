using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.DE.Business.DocumentWrappers
{
	public class DocDefermentAccount : DocBaseWrapper
	{
		DocDefermentAccount(IDefermentAccount objectToWrap, BusinessObjectFactory factory) : base(objectToWrap, factory)
		{
		}

		public static DocDefermentAccount New(IDefermentAccount defermentAccount, BusinessObjectFactory factoryToWrap) => defermentAccount == null ? null : new DocDefermentAccount(defermentAccount, factoryToWrap);

		public ZString Type => DefermentAccount.Type;
		public ZString AccountNumber => DefermentAccount.AccountNumber;
		public ZString Eori => DefermentAccount.Applicant;
		public ZString AccountHolder => DefermentAccount.AccountHolder;

		IDefermentAccount DefermentAccount => defermentAccount ??= (IDefermentAccount)WrappedObject;
		IDefermentAccount defermentAccount;
	}
}
