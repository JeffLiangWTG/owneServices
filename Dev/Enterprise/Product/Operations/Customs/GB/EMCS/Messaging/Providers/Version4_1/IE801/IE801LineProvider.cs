using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie801;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tcl;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE801LineProvider : IIE801Line
	{
		public IE801LineProvider(BodyEadEsadType line)
		{
			this.line = Argument.NotNull(line, nameof(line));
		}
		readonly BodyEadEsadType line;

		public ZInt BodyRecordUniqueReference => ZInt.Parse(line.BodyRecordUniqueReference);

		public ZString ExciseProductCode => line.ExciseProductCode;

		public ZString CnCode => line.CnCode;

		public ZBool FiscalMarkUsedFlag => line.FiscalMarkUsedFlagValueSpecified && line.FiscalMarkUsedFlag == Flag.Item1;

		public ZString FiscalMark => line.FiscalMark?.Value;

		public ZString DesignationOfOrigin => line.DesignationOfOrigin?.Value;

		public ZString CommercialDescription => line.CommercialDescription?.Value;

		public ZString BrandNameOfProducts => line.BrandNameOfProducts?.Value;

		public ZString MaturationPeriodOrAgeOfProducts => line.MaturationPeriodOrAgeOfProducts?.Value;

		public ZString IndependentSmallProducersDeclaration => line.IndependentSmallProducersDeclaration?.Value;

		public ZDecimal Quantity => line.Quantity;

		public ZDecimal GrossWeight => line.GrossMass;

		public ZDecimal NetWeight => line.NetMass;

		public ZDecimal AlcoholicStrength => line.AlcoholicStrengthByVolumeInPercentageValueSpecified ? line.AlcoholicStrengthByVolumeInPercentage.Value : 0m;

		public ZDecimal DegreePlato => line.DegreePlatoValueSpecified ? line.DegreePlato.Value : 0m;

		public ZDecimal SizeOfProducer => ZDecimal.ParseSafe(line.SizeOfProducer, 0m);

		public ZDecimal Density => line.DensityValueSpecified ? line.Density.Value : 0m;

		public IReadOnlyCollection<IEMCSPackageInComing> Packages => packages ?? (packages = line.Package.Select(x => new IE801PackageProvider(x)).ToArray());
		IReadOnlyCollection<IEMCSPackageInComing> packages;

		public ZString WineGrowingZoneCode => line.WineProduct?.WineGrowingZoneCode;

		public ZString WineProductCategory => line.WineProduct?.WineProductCategory.XmlEnumToString() ?? ZString.Empty;

		public ZString WineProductThirdCountryOfOrigin => line.WineProduct?.ThirdCountryOfOrigin;

		public ZString WineProductOtherInfo => line.WineProduct?.OtherInformation?.Value;

		public IReadOnlyCollection<ZString> WineOperationCodes => wineOperationCodes ?? (wineOperationCodes = line.WineProduct?.WineOperation?.Select(x => new ZString(x.WineOperationCode)).ToArray() ?? Array.Empty<ZString>());
		IReadOnlyCollection<ZString> wineOperationCodes;
	}
}
