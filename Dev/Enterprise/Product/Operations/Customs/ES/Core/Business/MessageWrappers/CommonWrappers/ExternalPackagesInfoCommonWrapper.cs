using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ExternalPackagesInfoCommonWrapper : IExternalPackagesInfoCommon
	{
		public ExternalPackagesInfoCommonWrapper(IReadOnlyCollection<ZString> packagesTags)
		{
			tags = packagesTags;
		}
		readonly IReadOnlyCollection<ZString> tags;

		public ZLong NumberOfPackages => tags.Count;

		public ZString PackageType => GetPackageType(); //External packaging type code
		protected virtual ZString GetPackageType() => BusinessQuantityUnit.Container;

		public IReadOnlyCollection<ZString> Tags => tags;
	}
}
