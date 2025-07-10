using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Messaging;

public static class CustomsFieldDescriptionsProvider
{
	[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
	public static ZString GetDescriptionByReferenceCode<T>(BusinessObjectFactory factory, ZString referenceCode)
		where T : CodeDescriptionPairList, new()
	{
		return factory?.GetCachedValue<T>().GetDescriptionFromCode(referenceCode) ?? ZString.Empty;
	}
}

public class CustomsFieldDescriptionsProvider<T>
	where T : CodeDescriptionPairList, new()
{
	public CustomsFieldDescriptionsProvider(BusinessObjectFactory factory)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
	}
	readonly protected BusinessObjectFactory factory;

	public ZString GetDescriptionByReferenceCode(ZString referenceCode) => CustomsFieldDescriptionsProvider.GetDescriptionByReferenceCode<T>(factory, referenceCode);
}
