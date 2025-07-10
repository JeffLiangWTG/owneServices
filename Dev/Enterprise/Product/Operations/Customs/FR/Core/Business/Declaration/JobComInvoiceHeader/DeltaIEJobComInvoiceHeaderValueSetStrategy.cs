using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaIEJobComInvoiceHeaderValueSetStrategy : IValueSetStrategy
	{
		bool incotermLogicInProcess;

		public DeltaIEJobComInvoiceHeaderValueSetStrategy(JobComInvoiceHeader invoiceHeader)
		{
			InvoiceHeader = invoiceHeader;
		}

		public JobComInvoiceHeader InvoiceHeader { get; }

		public void ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			ProcessIncotermLogic(valueThatHasChanged);
		}

		void ProcessIncotermLogic(ZPropertyInfo valueThatHasChanged)
		{
			if (incotermLogicInProcess)
			{
				return;
			}

			if (valueThatHasChanged.Name == JobComInvoiceHeader.Schema.JZ_IncoTerm
				|| valueThatHasChanged.Name == JobComInvoiceHeader.Schema.ZG_AgreedPlaceCode
				|| valueThatHasChanged.Name == JobComInvoiceHeader.Schema.JZ_IncoTermPlace
				|| valueThatHasChanged.Name == JobComInvoiceHeader.Schema.ZG_IncotermCountry)
			{
				incotermLogicInProcess = true;

				try
				{
					switch (valueThatHasChanged.Name)
					{
						case JobComInvoiceHeader.Schema.JZ_IncoTerm:
							InvoiceHeader.ShouldClearIncoTermPlacesIfNeeded = false;
							SetIncotermTextState();
							SetUNLOCOState();
							SetLocationAndCountryState();
							break;
						case JobComInvoiceHeader.Schema.ZG_AgreedPlaceCode:
							SetLocationAndCountryState();
							break;
						case JobComInvoiceHeader.Schema.JZ_IncoTermPlace:
							SetUNLOCOState();
							break;
						case JobComInvoiceHeader.Schema.ZG_IncotermCountry:
							SetUNLOCOState();
							break;
					}
				}
				finally
				{
					incotermLogicInProcess = false;
				}
			}
		}

		void SetLocationAndCountryState()
		{
			if (InvoiceHeader.JZ_IncoTerm.Equals(Core.Constants.IncoTerms.Other) || !InvoiceHeader.ZG_AgreedPlaceCode.IsEmpty)
			{
				InvoiceHeader.JZ_IncoTermPlaceInfo.ClearValue();
				InvoiceHeader.ZG_IncotermCountryInfo.ClearValue();
			}
		}

		void SetUNLOCOState()
		{
			if (InvoiceHeader.JZ_IncoTerm.Equals(Core.Constants.IncoTerms.Other)
				|| !InvoiceHeader.JZ_IncoTermPlace.IsEmpty
				|| !InvoiceHeader.ZG_IncotermCountry.IsEmpty)
			{
				InvoiceHeader.ZG_AgreedPlaceCodeInfo.ClearValue();
			}
		}

		void SetIncotermTextState()
		{
			if (!InvoiceHeader.JZ_IncoTerm.Equals(Core.Constants.IncoTerms.Other))
			{
				InvoiceHeader.JZ_AdditionalTermsInfo.ClearValue();
			}
		}
	}
}
