using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Generators
{
	public interface IG2gDeclaration
	{
		ZString DeclarationUcr { get; }
		ZString DeclarationUcrPart { get; }
		ZString DeclarationEpu { get; }
		ZString DeclarationENo { get; }
		ZDateTime DeclarationDoe { get; }
		ZString DeclarationSoe { get; }
		ZString CustomsAuthorisationReference { get; }
		ZString AirportCode { get; }
		ZString ShedOpId { get; }
		ZString CargoWiseNumber { get; }
		ZString DeclarationRoute { get; }
	}
}
