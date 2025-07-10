using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Generators
{
	class G2gDeclarationFromDeclarationNoEntry : G2gDeclarationFromCusEntryHeader, IG2gDeclaration
	{
		public G2gDeclarationFromDeclarationNoEntry(JobDeclaration declaration)
			: base(declaration, null)
		{
		}

		ZString IG2gDeclaration.CustomsAuthorisationReference
		{
			get { return G2gUtilities.GetFirstNumber(G2gUtilities.CodeType.CAR, declaration.AdditionalReferenceNumbers); }
		}

		ZString IG2gDeclaration.DeclarationUcr
		{
			get { return (declaration.JE_UCR + "/").Split('/')[0]; }
		}

		ZString IG2gDeclaration.DeclarationUcrPart
		{
			get { return (declaration.JE_UCR + "/").Split('/')[1]; }
		}

		ZString IG2gDeclaration.DeclarationEpu
		{
			get { return ZString.Empty; }
		}

		ZString IG2gDeclaration.DeclarationENo
		{
			get { return ZString.Empty; }
		}

		ZDateTime IG2gDeclaration.DeclarationDoe
		{
			get { return ZDateTime.Empty; }
		}

		ZString IG2gDeclaration.CargoWiseNumber
		{
			get { return string.Format("Declaration without entry {0} on {1}", declaration.JE_UCR, declaration.JE_DeclarationReference); }
		}

		ZString IG2gDeclaration.DeclarationRoute
		{
			get { return ZString.Empty; }
		}
	}
}
