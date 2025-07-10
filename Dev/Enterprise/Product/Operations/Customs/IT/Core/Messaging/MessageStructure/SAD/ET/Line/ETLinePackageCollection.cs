using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETLinePackageCollection
{
	readonly IEnumerable<IPackage> iPackages;

	public ETLinePackageCollection(IEnumerable<IPackage> iPackages)
	{
		this.iPackages = Argument.NotNull(iPackages, "iPackages");
	}

	[MessageLayout(Order = 0)]
	[MessageFieldIntegerRepresentation(2, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZInt NumberOfOccurences => iPackages.Count();

	[MessageLayout(Order = 1)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public IEnumerable<ETLinePackage> Packages
	{
		get
		{
			foreach (var package in iPackages)
			{
				yield return new ETLinePackage(package);
			}
		}
	}
}
