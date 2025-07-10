using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Module
{
	public class LPCOEntryHeaderFilterBusinessObject : EntryHeaderFilterBusinessObject
	{
		public override ZQuery Filter => base.Filter.AddToFilter(CusEntryHeaderSchema.CH_MessageType, BRJobMessageTypeList.Codes.LPCO);

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var collection = base.GetModuleFiltersCore();

			foreach (var filter in GetFilterToRemove())
			{
				collection.RemoveFilter(collection[filter]);
			}

			collection[Constants.EntryNumber].MultilingualDescription = LPCODeclarationFilterStripBusinessObject.LPCONumberText;
			collection[Constants.EntryStatus].MultilingualDescription = LPCODeclarationFilterStripBusinessObject.LPCOStatusText;

			return collection;
		}

		IEnumerable<ZString> GetFilterToRemove()
		{
			yield return Constants.OriginDestination;
			yield return Constants.LoadingDischarge;
			yield return Constants.ExportDate;
			yield return Constants.ReleaseDate;
			yield return Constants.FinalDestinationETA;
			yield return Constants.OriginETD;
			yield return Customs.Module.DeclarationFilterConstants.NumberFilterTypes.HouseBill;
			yield return Customs.Module.DeclarationFilterConstants.NumberFilterTypes.MasterBill;
			yield return Constants.ReferenceNumber;
			yield return Constants.WarehouseTransactionStatus;
			yield return Constants.CustomsAgent;
			yield return Constants.Declarant;
			yield return Constants.DeclarationType;
			yield return Customs.Module.DeclarationFilterConstants.FlightVoyageVessel;
		}

		protected override EntryHeaderFilterLookups GetNewLookups() => new LPCOEntryHeaderFilterLookups(this);
		public new LPCOEntryHeaderFilterLookups Lookups => (LPCOEntryHeaderFilterLookups)base.Lookups;
	}
}
