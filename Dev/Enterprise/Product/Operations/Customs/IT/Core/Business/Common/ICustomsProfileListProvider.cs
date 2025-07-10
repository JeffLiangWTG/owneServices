using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;
public interface ICustomsProfileListProvider
{
	CodeDescriptionPairList GetAccountDetails(bool fetchAllIfNoneFound = false);
}
