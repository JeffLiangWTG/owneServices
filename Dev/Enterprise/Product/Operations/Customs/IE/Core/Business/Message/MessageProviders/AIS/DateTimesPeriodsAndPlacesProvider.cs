using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class DateTimesPeriodsAndPlacesProvider : IDateTimesPeriodsAndPlaces
	{
		public DateTimesPeriodsAndPlacesProvider(CusEntryInstruction instruction)
		{
			this.instruction = Argument.NotNull(instruction, nameof(instruction));
			declaration = Argument.NotNull(instruction.JobDeclaration, nameof(instruction.JobDeclaration));
		}
		protected readonly CusEntryInstruction instruction;
		protected readonly JobDeclaration declaration;

		public IPlaceOfProcessing FirstPlaceOfProcessing
			=> CachedValueHelper.GetValue(ref firstPlaceOfProcessing, () => PlaceOfProcessingProvider.New(instruction.FirstPlaceOfUseOrProcessing));
		CachedValue<IPlaceOfProcessing> firstPlaceOfProcessing;

		public IReadOnlyCollection<IPlaceOfProcessing> PlaceOfUseOrProcessing
			=> placeOfUseOrProcessingCached ??= instruction.PlaceOfUseOrProcessingCollection.Select(x => PlaceOfProcessingProvider.New(x)).ToArray();
		IReadOnlyCollection<IPlaceOfProcessing> placeOfUseOrProcessingCached;

		public string CustomsOfficeOfDischarge => declaration.CustomsOffices.GetOfficeOfDischarge()?.CY_Data;

		public string SupervisingCustomsOffice => declaration.CustomsOffices.GetSupervisingOffice()?.CY_Data;

		public IPeriodForDischarge PeriodForDischarge => CachedValueHelper.GetValue(ref periodForDischarge, () => new PeriodForDischargeProvider(instruction));
		CachedValue<IPeriodForDischarge> periodForDischarge;

		public IBillOfDischarge BillOfDischarge => CachedValueHelper.GetValue(ref billOfDischarge, () => new BillOfDischargeProvider(instruction));
		CachedValue<IBillOfDischarge> billOfDischarge;
	}
}
