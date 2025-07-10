using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public class CC0044AWrapper : EU.NCTS.Business.CC044ADeclarationWrapper, ICC044ADeclaration
	{
		public CC0044AWrapper(NctsHeader nctsHeader)
			: base(nctsHeader)
		{
		}

		public ZString AgreementNumber => CachedValueHelper.GetValue(ref agreementNumber, () => DeclarationWrapperHelper.ArrivalAgreementNumber(NctsHeader));
		CachedValue<ZString> agreementNumber;

		public IReadOnlyCollection<IUnloadedGoodsItem> UnloadedGoodsItems => unloadedGoodsItems ?? (unloadedGoodsItems = NctsHeader.UnloadingMovementHeader.GoodsItems.Cast<EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc>().Select(UnloadedGoodsItemWrapper.New).OrderBy(x => x.ItemNumber).ToArray());
		IReadOnlyCollection<IUnloadedGoodsItem> unloadedGoodsItems;

		public IReadOnlyCollection<Tuple<string, string>> ListOfDifferenceInHeader
		{
			get
			{
				if (listOfDifferenceInHeader == null)
				{
					listOfDifferenceInHeader = new List<Tuple<string, string>>();
					if (NctsHeader.UnloadingRemark.G9_Conform == Customs.Business.YesNoList.Codes.No)
					{
						var unloadingMovementHeader = NctsHeader.UnloadingMovementHeader;
						var arrivalMovementHeader = NctsHeader.ArrivalMovementHeader;
						if (arrivalMovementHeader != null)
						{
							AddDifferenceResultsOfControl(listOfDifferenceInHeader, MeansOfTransportAtDepartureIdentityPointer, arrivalMovementHeader.BM_TransportAtDeparture, NctsHeader.UnloadedMeansOfTransportAtDepartureIdentity);
							AddDifferenceResultsOfControl(listOfDifferenceInHeader, MeansOfTransportAtDepartureNationalityPointer, arrivalMovementHeader.BM_RN_NKTransportAtDepartureCountry, NctsHeader.UnloadedMeansOfTransportAtDepartureNationality);
							AddDifferenceResultsOfControl(listOfDifferenceInHeader, TotalNumberOfItemsPointer, arrivalMovementHeader.TotalNumberOfItems.ToString(), unloadingMovementHeader.TotalNumberOfItems.ToString());
							AddDifferenceResultsOfControl(listOfDifferenceInHeader, TotalNumberOfPackagesPointer, arrivalMovementHeader.TotalNumberOfPackages.ToString(), unloadingMovementHeader.TotalNumberOfPackages.ToString());
							AddDifferenceResultsOfControl(listOfDifferenceInHeader, TotalGrossMassInKilogramsPointer, arrivalMovementHeader.TotalGrossMassInKilograms.ToString(), unloadingMovementHeader.TotalGrossMassInKilograms.ToString());
						}
					}
				}
				return listOfDifferenceInHeader;
			}
		}
		List<Tuple<string, string>> listOfDifferenceInHeader;

		NctsHeader NctsHeader => (NctsHeader)nctsHeader;

		void AddDifferenceResultsOfControl(List<Tuple<string, string>> plistOfDifferenceInHeader, ZString pointerToTheAttribute, ZString originalValue, ZString newValue)
		{
			if (!newValue.IsEmpty && originalValue != newValue)
			{
				plistOfDifferenceInHeader.Add(new Tuple<string, string>(pointerToTheAttribute, newValue));
			}
		}

		const string MeansOfTransportAtDepartureIdentityPointer = "18";
		const string MeansOfTransportAtDepartureNationalityPointer = "18#1";
		const string TotalNumberOfItemsPointer = "5";
		const string TotalNumberOfPackagesPointer = "6";
		const string TotalGrossMassInKilogramsPointer = "35";
	}
}
