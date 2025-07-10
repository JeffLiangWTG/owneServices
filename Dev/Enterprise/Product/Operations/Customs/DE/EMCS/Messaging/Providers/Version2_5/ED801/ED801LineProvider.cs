using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5
{
	public class ED801LineProvider : IED801Line
	{
		public ED801LineProvider(ED801EBodyEadContainerBodyEadEsad line)
		{
			this.line = Argument.NotNull(line, nameof(line));
		}
		readonly ED801EBodyEadContainerBodyEadEsad line;

		public ZInt BodyRecordUniqueReference => ZInt.Parse(line.BodyRecordUniqueReference);
		public ZString ExciseProductCode => line.ExciseProductCode;
		public ZString CnCode => line.CnCode;
		public ZBool FiscalMarkUsedFlag => line.FiscalMarkUsedFlagSpecified && line.FiscalMarkUsedFlag == ED801EBodyEadContainerBodyEadEsadFiscalMarkUsedFlag.Item1;
		public ZString FiscalMark => line.FiscalMark;
		public ZString DesignationOfOrigin => line.DesignationOfOrigin;
		public ZString CommercialDescription => line.CommercialDescription;
		public ZString BrandNameOfProducts => line.BrandNameOfProducts;
		public ZDecimal Quantity => line.Quantity;
		public ZDecimal GrossWeight => line.GrossMass;
		public ZDecimal NetWeight => line.NetMass;
		public ZDecimal AlcoholicStrength => line.AlcoholicStrengthSpecified ? line.AlcoholicStrength : 0m;
		public ZDecimal DegreePlato => line.DegreePlatoSpecified ? line.DegreePlato : 0m;
		public ZDecimal SizeOfProducer => ZDecimal.ParseSafe(line.SizeOfProducer, 0m);
		public ZDecimal Density => line.DensitySpecified ? line.Density : 0m;
		public IReadOnlyCollection<IEMCSPackageInComing> Packages => packages ?? (packages = line.Package.Select(x => new ED801PackageProvider(x)).ToArray());
		IReadOnlyCollection<IEMCSPackageInComing> packages;
		public ZString WineGrowingZoneCode => line.WineProduct?.WineGrowingZoneCode ?? ZString.Empty;
		public ZString WineProductCategory => line.WineProduct?.WineProductCategory.XmlEnumToString() ?? ZString.Empty;
		public ZString WineProductThirdCountryOfOrigin => line.WineProduct?.ThirdCountryOfOrigin ?? ZString.Empty;
		public ZString WineProductOtherInfo => line.WineProduct?.OtherInformation ?? ZString.Empty;
		public IReadOnlyCollection<ZString> WineOperationCodes => wineOperationCodes ?? (wineOperationCodes = line.WineProduct?.WineOperation?.Select(x => new ZString(x.WineOperationCode)).ToArray() ?? Array.Empty<ZString>());
		IReadOnlyCollection<ZString> wineOperationCodes;
		public ZString MaturationPeriodOrAgeOfProducts => line.MaturationPeriodOrAgeOfProducts;
	}
}
