using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class CC014CHeaderProvider : NctsHeaderSharedDataProvider, ICC014C
	{
		public CC014CHeaderProvider(NctsHeader nctsHeader, ZString justification) : base(nctsHeader)
		{
			this.justification = justification;
		}

		public ITransitOperation TransitOperation => transitOperation ?? (transitOperation = new TransitOperationProvider(nctsHeader));
		ITransitOperation transitOperation;

		public IInvalidationType02 Invalidation => invalidation ?? (invalidation = new InvalidationType02Provider(nctsHeader, justification));
		IInvalidationType02 invalidation;

		public IHolderOfTheTransitProcedure HolderOfTheTransitProcedure => holderOfTheTransitProcedure ?? (holderOfTheTransitProcedure = nctsHeader.Principal != null ? new HolderOfTheTransitProcedureProvider(nctsHeader.Principal, false, nctsHeader.IsInPhase5TransitionPeriod) : null);
		IHolderOfTheTransitProcedure holderOfTheTransitProcedure;

		public override string MessageRecipient => string.Format(System.Globalization.CultureInfo.InvariantCulture, "NTA.{0}", nctsHeader.IsPhase5 ? nctsHeader.CommonMovementHeader.DepartureCustomsOfficeCodeCountry : nctsHeader.DepartureCustomsOfficeCodeCountry);

		public override string MessageType => Constants.MessageTypes.CC014C;

		readonly ZString justification;
	}
}
