using CargoWise.Types;

namespace Enterprise.Integration.Accounting
{
	public interface IJobInvoicingDefaultDepartments
	{
		ZString ConsolType { get; set; }
		ZGuid Department { get; set; }
	}
}
