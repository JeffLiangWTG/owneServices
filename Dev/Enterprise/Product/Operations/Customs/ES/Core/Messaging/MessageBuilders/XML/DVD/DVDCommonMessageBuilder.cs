using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DVDT;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.Outgoing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public abstract class DVDCommonMessageBuilder<TProvider, TObject> : XMLMessageBuilder<TProvider, TObject>
		where TProvider : IDVDCommonDataProvider
	{
		protected DVDCommonMessageBuilder(TProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		public T GetPopulatedCommonMessage<T>()
			where T : IDVDMessageCommon, new()
		{
			return new T()
			{
				MRN_Operacion = provider.MRN,
				SegmentosDeServicio = GetPopulatedServiceSegment()
			};
		}

		TdSegmDeServicio GetPopulatedServiceSegment()
		{
			var serviceSegment = GetPopulatedCommonServiceSegment<TdSegmDeServicio>();
			if (serviceSegment != null)
			{
				serviceSegment.IndicadorTestValueSpecified = false;
			}
			return serviceSegment;
		}

		protected T GetPopulatedCommonServiceSegment<T>()
			where T : IDVDServiceSegmentTDCommon, new()
		{
			return new T()
			{
				Id = TransactionId,
				FechaPreparacion = DateOfCET,
				HoraPreparacion = TimeOfCET,
			};
		}

		protected T GetPopulatedCommonPackage<T>(IDVDCommonPackage packageProvider)
			where T : IDVDPackageCommon, new()
		{
			var package = GetPopulatedPackage<T>(packageProvider);
			if (package != null)
			{
				package.NumberOfPackages = packageProvider.NumberOfPackages;
				package.NumberOfPackagesSpecified = !packageProvider.NumberOfPackages.IsEmpty;
			}
			return package;
		}
	}
}
