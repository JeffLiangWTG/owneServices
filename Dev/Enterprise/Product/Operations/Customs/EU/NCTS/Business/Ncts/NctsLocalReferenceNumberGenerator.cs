using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public partial class NctsHeader : IBillGenerationSupport
	{
		// Local Reference Number Generator implementation

		ZString GetNewLrnReference(BusinessObjectFactory factory)
		{
			var declarationTarget = new NctsLocalReferenceNumberGeneratorTarget();
			var generator = new NumberGenerator();
			generator.Factory = factory;
			generator.Context = new NumberGeneratorContext();
			generator.BaseFountain = Env.NumberFountains.NctsLocalReferenceNumber;
			generator.FountainGetter = Env.NumberFountains.GetNctsLocalReferenceNumberFountain;
			generator.PrimaryTarget = declarationTarget;
			generator.ValueProviders.AddRange(new StandardValueSource());
			generator.ValueProviders.AddRange(new FreightValueSource(this));
			generator.Generate();
			generator.EnforceMaxLengths();
			return declarationTarget.Value.ToUpper();
		}

		#region Number Fountain plumbing

		OrgHeader IBillGenerationSupport.CarrierPrincipal => Principal.Organisation;

		RefUNLOCO IBillGenerationSupport.Destination => new RefUNLOCO.Loader(Factory).Load(PlaceOfUnloadingCode);

		RefUNLOCO IBillGenerationSupport.Discharge => null;

		BusinessObjectFactory IBillGenerationSupport.Factory => Factory;

		RefUNLOCO IBillGenerationSupport.Load => null;

		RefUNLOCO IBillGenerationSupport.Origin => MovementHeader is NctsDepartureMovementHeader movementHeader ? new RefUNLOCO.Loader(Factory).Load(movementHeader.BM_RL_NKForeignDestPort) : null;

		ZString IBillGenerationSupport.ServiceLevel => ZString.Empty;

		ZString IBillGenerationSupport.TranshipmentIndicator => ZString.Empty;

		ZString IBillGenerationSupport.TransportMode => MovementHeader is NctsDepartureMovementHeader movementHeader ? TransportModeTranslator.TranslateToCargoWiseCode(movementHeader.BM_ExportTransportMode) : null;

		#endregion
	}
}
