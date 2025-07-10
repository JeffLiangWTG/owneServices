using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public sealed class CommercialInvoiceDetailsControlBag : ControlBag
{
	public static CommercialInvoiceDetailsControlBag Instance => instance ?? (instance = new CommercialInvoiceDetailsControlBag());

	[ThreadStatic]
	static CommercialInvoiceDetailsControlBag instance;

	CommercialInvoiceDetailsControlBag()
	{
		ExporterContractNumberTextBox = RegisterControl(nameof(CommercialInvoiceDetailsLayoutUserControl.ExporterContractNumberTextBox));
		PaymentDaysCalcEdit = RegisterControl(nameof(CommercialInvoiceDetailsLayoutUserControl.PaymentDaysCalcEdit));
		AuthorizedEconomicOperatorGuidFindBox = RegisterControl(nameof(CommercialInvoiceDetailsLayoutUserControl.AuthorizedEconomicOperatorGuidFindBox));
		AuthorizedEconomicOperatorCountryTextBox = RegisterControl(nameof(CommercialInvoiceDetailsLayoutUserControl.AuthorizedEconomicOperatorCountryTextBox));
		AuthorizedEconomicOperatorRoleTextBox = RegisterControl(nameof(CommercialInvoiceDetailsLayoutUserControl.AuthorizedEconomicOperatorRoleTextBox));
		AuthorizedEconomicOperatorCodeTextBox = RegisterControl(nameof(CommercialInvoiceDetailsLayoutUserControl.AuthorizedEconomicOperatorCodeTextBox));
		IGSTPaymentStatusDropEdit = RegisterControl(nameof(CommercialInvoiceDetailsLayoutUserControl.IGSTPaymentStatusDropEdit));
	}

	protected override Control CreateTemplate() => new CommercialInvoiceDetailsLayoutUserControl();

	public ControlReference ExporterContractNumberTextBox { get; }
	public ControlReference PaymentDaysCalcEdit { get; }
	public ControlReference AuthorizedEconomicOperatorGuidFindBox { get; }
	public ControlReference AuthorizedEconomicOperatorCountryTextBox { get; }
	public ControlReference AuthorizedEconomicOperatorRoleTextBox { get; }
	public ControlReference AuthorizedEconomicOperatorCodeTextBox { get; }
	public ControlReference IGSTPaymentStatusDropEdit { get; }
}
