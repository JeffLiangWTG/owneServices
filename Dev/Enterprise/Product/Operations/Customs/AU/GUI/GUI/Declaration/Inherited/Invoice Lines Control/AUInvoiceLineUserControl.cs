using System;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUInvoiceLineUserControl : DeclarationInvoiceLineUserControl
	{
		public AUInvoiceLineUserControl()
		{
			InitializeComponent();
		}

		protected override bool SupportsExtraPhysicalQuantitiesOnC2Pivot => true;

		protected override InvoiceLineFilterBusinessObject CreateFilterBusinessObject(Func<Customs.Business.IInvoicesProvider> getInvoicesProvider, Func<ZString, ZBool> isColumnAvailable)
		{
			return DesignModeFinder.IsDesigning ? null : new AUInvoiceLineFilterBusinessObject(getInvoicesProvider, isColumnAvailable);
		}

		protected override string GetCustomsCountryCode() => Core.Constants.CountryCodes.Australia;
		protected override ZString GetDataGroupingForUniversalTariff() => Core.Constants.CountryCodes.Australia;
	}
}
