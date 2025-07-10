using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Declaration.Business;

public class CusUnderbondDocumentSupporter : DocumentSupporter
{
	public CusUnderbondDocumentSupporter(CusUnderbond cusUnderbond)
		: base(cusUnderbond)
	{
	}

	protected CusUnderbond Underbond
	{
		get { return (CusUnderbond)BusinessObject; }
	}

	public override BusinessContext BusinessContext
	{
		get { return BusinessContext.CFSAirCargoOutturn; }  // need to create a new BusinessContext??
	}

	public override ISecurityCheckpoint CustomisationSecurityCheckpoint
	{
		get { return Env.Security.None; }
	}

	protected override Core.Constants.DataContext[] GetSupportedDataContexts()
	{
		return new Core.Constants.DataContext[] { Core.Constants.DataContext.GenericFreightJob };
	}

	protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
	{
		var genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Underbond);
		if (genericWrappers != null)
		{
			return genericWrappers;
		}
		return null;
	}

	public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
	{
		return null;
	}

	public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
	{
		if (dataContextValue.DataContext == Core.Constants.DataContext.GenericFreightJob)
		{
			return Res.GetString("84B1097E-31A7-4A6B-9959-A991CF68DDCE", "This shipment does not have any underbond.");
		}
		return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
	}

	public override ZBool ShowReasonForNotPrinting(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
	{
		return dataContext == Core.Constants.DataContext.GenericFreightJob && base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
	}
}
