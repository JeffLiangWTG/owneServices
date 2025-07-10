using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DUAExportInternalPackagesInfoWrapper : IInternalPackagesInfoCommon
	{
		public DUAExportInternalPackagesInfoWrapper(CusEntryLine cusEntryLine)
		{
			entryLine = Argument.NotNull(cusEntryLine, "CusEntryLine cannot be null");
			Argument.GreaterThan(entryLine.InvoiceLines.Count, 0, nameof(entryLine.InvoiceLines));
		}

		readonly CusEntryLine entryLine;

		readonly ZString fixedMarks = "BASTIDORES";

		public IReadOnlyCollection<IInternalPackageIdentificationCommon> Packages
		{
			get
			{
				if (packages == null)
				{
					packages = new List<InternalPackageIdentificationCommonWrapper>();

					var vehiclePacksNumber = entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.Vehicles.Count);

					if (vehiclePacksNumber > 0)
					{
						packages.Add(new InternalPackageIdentificationCommonWrapper(fixedMarks, RefCusCodeList.PackageType.Frame, vehiclePacksNumber));
					}

					var packagingDetails = entryLine.PackagingDetails.GroupBy(x => new { x.Package.CW_PackType, x.Package.CW_MarksAndNos });
					foreach (var pack in packagingDetails)
					{
						var packqty = ZLong.Zero;
						foreach (var p in pack.ToArray())
						{
							packqty += p.CHC_NumberOfPacks;
						}
						packages.Add(new InternalPackageIdentificationCommonWrapper(pack.Key.CW_MarksAndNos, pack.Key.CW_PackType, packqty));
					}
				}
				return packages;
			}
		}
		List<InternalPackageIdentificationCommonWrapper> packages;
	}
}
