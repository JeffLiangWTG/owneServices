using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public interface IFactLoader
	{
		IEnumerable<IInputFact> GetFacts(IJobInvoicingPlugIn parentPlugin, ICompany loginCompany, IBranch loginBranch, IDepartment loginDepartment);
	}

	public interface IShipmentJobFactLoader : IFactLoader
	{
	}

	public interface IQuickBookingJobFactLoader : IFactLoader
	{
	}

	public interface IConsolJobFactLoader : IFactLoader
	{
	}

	public interface IDeclarationJobFactLoader : IFactLoader
	{
	}

	public interface IWarehouseJobFactLoader : IFactLoader
	{
	}

	public interface IWorkItemJobFactLoader : IFactLoader
	{
	}

	public interface ILandTransportJobFactLoader : IFactLoader
	{
	}

	public interface ITransitReceiveConsignmentJobFactLoader : IFactLoader
	{
	}

	public interface ITransitDispatchConsignmentJobFactLoader : IFactLoader
	{
	}

	public interface ITransitReceiveTransportationUnitJobFactLoader : IFactLoader
	{
	}

	public interface ITransitDispatchTransportationUnitJobFactLoader : IFactLoader
	{
	}
}
