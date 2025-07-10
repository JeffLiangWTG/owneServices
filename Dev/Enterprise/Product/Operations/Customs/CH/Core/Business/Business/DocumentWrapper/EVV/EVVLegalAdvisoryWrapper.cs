using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.CH.Business;

public sealed class EVVLegalAdvisoryWrapper : DocumentWrapper
{
	public static EVVLegalAdvisoryWrapper New(IEvvLegalAdvisory legalAdvisory, BusinessObjectFactory factory)
		=> new EVVLegalAdvisoryWrapper(Argument.NotNull(legalAdvisory, nameof(legalAdvisory)), Argument.NotNull(factory, nameof(factory)));

	EVVLegalAdvisoryWrapper(IEvvLegalAdvisory legalAdvisory, BusinessObjectFactory factory) : base(legalAdvisory, factory)
	{
	}
	IEvvLegalAdvisory LegalAdvisory => (IEvvLegalAdvisory)WrappedObject;

	public ZInt SequenceNumber => LegalAdvisory.SequenceNumber;

	public ZString Title => LegalAdvisory.Title;

	public ZString Text => LegalAdvisory.Text;
}
