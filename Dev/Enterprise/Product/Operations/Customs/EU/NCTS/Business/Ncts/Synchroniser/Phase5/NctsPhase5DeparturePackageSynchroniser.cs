using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	class NctsPhase5DeparturePackageSynchroniser : BusinessObjectSynchroniser
	{
		public NctsPhase5DeparturePackageSynchroniser(NctsPackage destPack, ForwardingPackLine sourcePack)
			: base(destPack, sourcePack)
		{
		}

		public new NctsPackage Destination => (NctsPackage)base.Destination;

		public new ForwardingPackLine Source => (ForwardingPackLine)base.Source;

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.B5_UnitTypeInfo, GetConvertedSourcePackageType, GetSourcePackageTypeInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.B5_UnitCountInfo, GetConvertedSourceUnitCount, GetSourceUnitCountInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.B5_MarksAndNumbersInfo, Source.JL_MarksAndNumbersInfo));
			Synchronisers.Add(new NctsPhase5DeparturePackageContainerPivotCollectionSynchroniser(Source, Destination));
		}

		IEnumerable<ZPropertyInfo> GetSourcePackageTypeInfo()
		{
			yield return Source.JL_F3_NKPackTypeInfo;
		}

		IZType GetConvertedSourcePackageType() => Converter.GetTwoCharacterUnitType(Source.JL_F3_NKPackType).MappedCode;

		InvoiceLineFromOrderLineSynchroniser converter;
		InvoiceLineFromOrderLineSynchroniser Converter => converter ?? (converter = new InvoiceLineFromOrderLineSynchroniser());

		IZType GetConvertedSourceUnitCount() => new ZLong(Source.JL_PackageCount);

		IEnumerable<ZPropertyInfo> GetSourceUnitCountInfo()
		{
			yield return Source.JL_PackageCountInfo;
		}
	}
}
