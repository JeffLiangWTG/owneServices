using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Generators
{
	class G2gDeclarationFromCusEntryHeader : IG2gDeclaration
	{
		public G2gDeclarationFromCusEntryHeader(JobDeclaration declaration, CusEntryHeader ceh)
		{
			this.declaration = declaration;
			this.ceh = ceh;
		}

		ZString IG2gDeclaration.DeclarationUcr
		{
			get { return ceh.DeclarationUCR; }
		}

		ZString IG2gDeclaration.DeclarationUcrPart
		{
			get { return ceh.DeclarationUCRPartSuffix; }
		}

		ZString IG2gDeclaration.DeclarationEpu
		{
			get { return ceh.EntryNumber.Left(3); }
		}

		ZString IG2gDeclaration.DeclarationENo
		{
			get { return ceh.EntryNumber.SubstringSafe(4); }
		}

		ZDateTime IG2gDeclaration.DeclarationDoe
		{
			get { return ceh.CusEntryNumber != null ? ceh.CusEntryNumber.CE_IssueDate : ZDateTime.Empty; }
		}

		ZString IG2gDeclaration.DeclarationSoe
		{
			get { return declaration.ZG_StyleOfEntrySOE; }
		}

		ZString IG2gDeclaration.CustomsAuthorisationReference
		{
			get
			{
				var carOnWholeDeclaration = G2gUtilities.GetFirstNumber(G2gUtilities.CodeType.CAR, declaration.AdditionalReferenceNumbers);
				var carOnOneEntry = ceh.RandomHeader.ZG_CustomsAuthorisationReferenceForExportFallback;
				return !carOnOneEntry.IsEmpty ? carOnOneEntry : carOnWholeDeclaration;
			}
		}

		ZString IG2gDeclaration.AirportCode
		{
			get { return declaration.JE_LocationOfGoods; }
		}

		ZString IG2gDeclaration.ShedOpId
		{
			get { return declaration.SubLocation; }
		}

		ZString IG2gDeclaration.CargoWiseNumber
		{
			get { return string.Format("Entry {0} on declaration {1}", ceh.CH_BGMReference, declaration.JE_DeclarationReference); }
		}

		ZString IG2gDeclaration.DeclarationRoute
		{
			get { return declaration.JE_GBRouteOfEntry; }
		}

		protected JobDeclaration declaration;
		readonly CusEntryHeader ceh;
	}
}
