using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public static class WrapperHelper
	{
		public static ZString GetLongDate(this ZDateTime datetimeValue) => datetimeValue.IsValid ? datetimeValue.ToString("yyyyMMdd", CultureInfo.InvariantCulture) : string.Empty;

		public static ZString GetLongDateAndTime(this ZDateTime datetimeValue) => datetimeValue.IsValid ? datetimeValue.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture) : string.Empty;

		public static ZString ArrivalDeclarantTIN(this NctsHeader nctsHeader)
		{
			var result = nctsHeader.Declarant?.Address.GetEORI() ?? ZString.Empty;
			if (result.IsEmpty)
			{
				result = nctsHeader.Consignee?.Address.GetEORI() ?? ZString.Empty;
			}
			return result;
		}

		public static ZString DepartureDeclarantTIN(this NctsHeader nctsHeader)
		{
			var result = nctsHeader.Declarant?.Address.GetEORI() ?? ZString.Empty;
			if (result.IsEmpty)
			{
				result = nctsHeader.Consignor?.Address.GetEORI() ?? ZString.Empty;
			}
			return result;
		}

		public static ZString DeparturePrincipalTIN(this NctsHeader nctsHeader)
		{
			var result = nctsHeader.Principal?.Address.GetEORI() ?? ZString.Empty;
			return result;
		}

		public static ZString AuthorisedConsigneeTIN(this NctsHeader nctsHeader)
		{
			var consignee = nctsHeader.Consignee;
			var result = ZString.Empty;
			if (consignee != null && consignee.Address != null && consignee.Organisation != null)
			{
				var authorisations = CusAuthorisationHeader.Loader.GetAuthorisationsForAddressesAndPermitHolder(nctsHeader.Factory, nctsHeader.CountryCode, new ZString[] { CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit }, ZDate.Today, new[] { consignee.Organisation.PK }, new[] { consignee.Address.PK });
				if (authorisations != null && authorisations.Any())
				{
					result = authorisations.FirstOrDefault().CPH_Number;
				}
			}
			return result;
		}

		public static ZString[] GetSealsSelected(this NctsHeader nctsHeader)
		{
			var sealList = new List<ZString>();
			if (nctsHeader != null)
			{
				var goodsItems = nctsHeader.IsPhase5 ? nctsHeader.Bills.SelectMany(x => x.GoodsItems) : nctsHeader.MovementHeader.GoodsItems;
				foreach (var item in goodsItems)
				{
					foreach (NonPersistentDepartureContainerPivot container in item.ContainersPivots)
					{
						if (container.ContainerSelected)
						{
							if (!container.Container.BC_Seal1.IsEmpty)
							{
								sealList.Add(container.Container.BC_Seal1);
							}

							if (!container.Container.BC_Seal2.IsEmpty)
							{
								sealList.Add(container.Container.BC_Seal2);
							}
						}
					}
				}
			}
			return sealList.Distinct().ToArray();
		}

		public static ZString GetCarrierEORI(this NctsHeader nctsHeader)
		{
			return nctsHeader.Carrier?.GetEORI() ?? ZString.Empty;
		}

		public static IReadOnlyCollection<ISealID> GetSealIDs(this NctsHeader nctsHeader, IReadOnlyList<ZString> selectedSeals)
		{
			var result = Array.Empty<ISealID>();
			switch (nctsHeader.MovementHeader.BM_SealType)
			{
				case SealTypeList.Codes.ContainerSeal:
					result = selectedSeals.Select(x => new SealWrapper(x)).Cast<ISealID>().ToArray();
					break;
				case SealTypeList.Codes.PackageSeal:
					result = nctsHeader.Seals.Cast<Seal>().Select(x => new SealWrapper(x.CY_Data)).Cast<ISealID>().ToArray();
					break;
			}
			return result;
		}
	}
}
