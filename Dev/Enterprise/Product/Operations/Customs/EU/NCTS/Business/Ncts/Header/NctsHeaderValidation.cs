using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsHeaderValidation : Customs.Business.CusInBondHeaderValidation
	{
		public NctsHeaderValidation(NctsHeader parent)
			: base(parent)
		{
		}

		public new NctsHeader Parent => (NctsHeader)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateLocalReferenceNumber();
			ValidateHeaderUnloadingNotes();
			ValidateExplanation();
			ValidateArrivalMrnFromUser();
			ValidateUnloadedMeansOfTransportAtDepartureNationality();
			ValidateDestinationCustomsOfficeCodeForDeparture();
			ValidateDestinationCustomsOfficeCodeForArrival();
			ValidateMaxGoodsItemsForAllBills();
		}

		public void ValidateDestinationCustomsOfficeCodeForDeparture()
		{
			ValidateCalculatedProperty(Parent.DestinationCustomsOfficeCodeForDepartureInfo);
		}

		protected virtual void CheckDestinationCustomsOfficeCodeForDeparture()
		{
			if (Parent.IsDepartureMovement)
			{
				ValidateCustomsOfficeCode(Parent.DestinationCustomsOfficeForDeparture, Parent.DestinationCustomsOfficeCodeForDepartureInfo);
			}
		}

		public void ValidateDestinationCustomsOfficeCodeForArrival()
		{
			ValidateCalculatedProperty(Parent.DestinationCustomsOfficeCodeForArrivalInfo);
		}

		protected void CheckDestinationCustomsOfficeCodeForArrival()
		{
			if (Parent.IsArrivalMovement && ValidationEnabled)
			{
				CheckDestinationCustomsOfficeCodeForArrivalCore();
			}
		}

		protected virtual void CheckDestinationCustomsOfficeCodeForArrivalCore()
		{
			ValidateCustomsOfficeCode(Parent.DestinationCustomsOfficeForArrival, Parent.DestinationCustomsOfficeCodeForArrivalInfo);
		}

		void ValidateCustomsOfficeCode(ICustomsOffice customsOffice, ZPropertyInfo customsOfficeInfo)
		{
			if (customsOffice is EuOfficeCode office)
			{
				office.Validation.ValidateCY_Data();
				customsOfficeInfo.AddAllNotificationsFrom(office.CY_DataInfo);
			}
		}

		public void ValidateUnloadedMeansOfTransportAtDepartureNationality()
		{
			ValidateCalculatedProperty(Parent.UnloadedMeansOfTransportAtDepartureNationalityInfo);
		}

		public void ValidateMaxGoodsItemsForAllBills()
		{
			var parent = Parent;
			if (parent.IsPhase5)
			{
				var error = MaxGoodsItemsForAllBillsError;
				var billsToAddError = new List<NctsBill>();
				var totalGoodsItems = 0;

				foreach (var bill in parent.Bills)
				{
					bill.ClearRowNotificationsContaining(error);
					var goodsItemsForBillCount = bill.GoodsItems.Count(x => !x.IsDeleted);

					if (goodsItemsForBillCount > 0)
					{
						billsToAddError.Add(bill);
						totalGoodsItems += goodsItemsForBillCount;
					}
				}

				if (totalGoodsItems > MaxGoodsItemsAllowedForAllBills)
				{
					foreach (var bill in billsToAddError)
					{
						bill.AddRowError(error);
					}
				}
			}
		}

		const int MaxGoodsItemsAllowedForAllBills = 1999;

		static string MaxGoodsItemsForAllBillsError => Res.GetString("d2713a60-2745-406e-a6c6-d5887b12b930", "You are only allowed to enter a maximum of {0} items per declaration.", MaxGoodsItemsAllowedForAllBills);

		protected void CheckUnloadedMeansOfTransportAtDepartureNationality()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.UnloadedMeansOfTransportAtDepartureNationalityInfo, Parent.Lookups.Countries);
		}

		#region HeaderUnloadingNotes

		public void ValidateHeaderUnloadingNotes()
		{
			ValidateCalculatedProperty(Parent.HeaderUnloadingNotesInfo);
		}

		protected virtual void CheckHeaderUnloadingNotes()
		{
		}

		#endregion

		#region Explanation

		public void ValidateExplanation()
		{
			ValidateCalculatedProperty(Parent.ExplanationInfo);
		}

		protected virtual void CheckExplanation()
		{
		}

		#endregion

		#region Arrival Mrn From User

		public void ValidateArrivalMrnFromUser()
		{
			ValidateCalculatedProperty(Parent.ArrivalMrnFromUserInfo);
		}

		protected virtual void CheckArrivalMrnFromUser()
		{
			var parent = Parent;

			if (parent.IsArrivalMovement)
			{
				var otherNctsHeader = parent.Factory.LoadTop1<NctsHeader>(NctsHeaderValidationHelper.GetDuplicateMRNQuery(parent));
				if (otherNctsHeader != null)
				{
					parent.ArrivalMrnFromUserInfo.AddWarning(Res.GetString("3A676672-E0D7-4610-9D9A-CFCB6F2843B3", "Another Entry already contains the same MRN number (Entry: '{0}', Company: '{1}', Branch: '{2}').", otherNctsHeader.BH_JobReference, otherNctsHeader.Company.GC_Name, otherNctsHeader.Branch != null ? otherNctsHeader.Branch.GB_BranchName : ZString.Empty));
				}
			}
		}

		#endregion

		public void ValidateLocalReferenceNumber()
		{
			ValidateCalculatedProperty(Parent.LocalReferenceNumberInfo);
		}

		protected override void CheckBH_RL_NKImportLoadPort()
		{
			base.CheckBH_RL_NKImportLoadPort();

			var parent = Parent;
			var isFullLoadPortSupport = parent.Configuration?.FullLoadPortSupport ?? false;
			var propertyInfo = parent.BH_RL_NKImportLoadPortInfo;

			var countryIsEmpty = Res.GetString("35713D56-6A75-494F-B6C8-8D841A82C730", "Either Dispatch Country in Goods tab or Country of Dispatch in Declaration must be filled.");
			var countryIsPresentInDecAndGoods = Res.GetString("35713D56-6A75-494F-B6C8-8D841A82C731", "Dispatch Country must be filled in Goods tab or Country of Dispatch must be filled in Declaration tab but not both.");

			if (isFullLoadPortSupport)
			{
				ListValidation.MessageErrorIfInvalidCode(propertyInfo, parent.Lookups.PortOfDispatchList);
				countryIsEmpty = Res.GetString("92D0799C-95CA-48E2-A2C4-C5FC0E11CE7F", "Either Dispatch Country in Goods tab or Port of Dispatch in Declaration must be filled.");
				countryIsPresentInDecAndGoods = Res.GetString("3CC980EF-355C-490E-9CC0-1EC39D44F44A", "Dispatch Country must be filled in Goods tab or Port of Dispatch must be filled in Declaration tab but not both.");
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(propertyInfo, parent.Lookups.CountryOfDispatchList);
			}

			if (parent.IsDepartureMovement)
			{
				var departureMovementHeader = parent.MovementHeader;
				if (departureMovementHeader != null)
				{
					var dispatchCountry = Parent.BH_RL_NKImportLoadPort;
					if (dispatchCountry.IsEmpty)
					{
						if (!parent.IsPhase5
							&& EitherDispatchCountryOnGoodsItemOrOnHeaderMustBeFilled
							&& !departureMovementHeader.HasGoodsItemsWithCountryOfDispatch)
						{
							propertyInfo.AddMessageError(countryIsEmpty);
						}
					}
					else
					{
						UniversalValidationHelper.CheckCountryOfDispatchOrDestinationIsAtLeastOneC0009Code(parent.Factory, departureMovementHeader.BM_InBondEntryType, parent.BH_RL_NKImportLoadPort.Split(2)[0], dispatchCountry, parent.DefaultDataGroupingCode, propertyInfo);

						if (departureMovementHeader.HasGoodsItemsWithCountryOfDispatch)
						{
							propertyInfo.AddMessageError(countryIsPresentInDecAndGoods);
						}
					}
				}
			}
		}

		protected virtual bool EitherDispatchCountryOnGoodsItemOrOnHeaderMustBeFilled => true;

		protected override void CheckBH_FTZMove()
		{
			base.CheckBH_FTZMove();
			if (!Parent.IsNull && Parent.IsDepartureMovement)
			{
				Parent.SecurityConsignor.Validation.ValidateOrganisationPK();
			}
		}

		protected override void CheckBH_CommunicationLanguage()
		{
			base.CheckBH_CommunicationLanguage();
			ListValidation.MessageErrorIfInvalidCode(Parent.BH_CommunicationLanguageInfo);
		}

		protected void CheckLocalReferenceNumber()
		{
			if (ValidationEnabled)
			{
				CheckLocalReferenceNumberCore();
			}
		}

		protected virtual void CheckLocalReferenceNumberCore()
		{
			var parent = Parent;
			CheckLocalReferenceNumber_Length();
			if (!parent.LocalReferenceNumber.IsWesternEuropeanOrEmpty)
			{
				EnglishCharactersValidation.ErrorIfNotWesternEuropean(parent.LocalReferenceNumberInfo);
			}
		}

		protected virtual void CheckLocalReferenceNumber_Length()
		{
			if (Parent.LocalReferenceNumber.Length <= 4)
			{
				Parent.LocalReferenceNumberInfo.AddMessageError(Res.GetString("89FEDCF7-7FD8-414B-8F00-F5332BFD5B82", "This field must have a value and the value must be longer than 4 characters."));
			}
		}

		public void CheckDepartureCustomsOfficeAgainstConsignorAuthorisation(ZPropertyInfo info) => CheckDepartureCustomsOfficeAgainstConsignorAuthorisationCore(info);

		protected virtual void CheckDepartureCustomsOfficeAgainstConsignorAuthorisationCore(ZPropertyInfo info)
		{
		}

		protected virtual bool ValidationEnabled => !Parent.IsArrivalDetailsReadOnly;
	}
}
