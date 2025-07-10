using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.CDS.Declaration;

namespace Enterprise.Customs.GB.CDS
{
	public class EntryStyleValidator
	{
		protected CDSJobDeclarationValidation validationObject;
		public EntryStyleValidator(CDSJobDeclarationValidation validationObject)
		{
			this.validationObject = validationObject;
		}

		public virtual void Validate()
		{
			if (DeclarationTypeEntryStyleCombosNotAllowed.Any(x => x.DeclarationType.EqualsIgnoringCase(validationObject.Parent.JE_DeclarationType) && x.EntryStyle.EqualsIgnoringCase(validationObject.Parent.JE_EntryStyle)))
			{
				validationObject.Parent.JE_EntryStyleInfo.AddMessageError(string.Format(CultureInfo.CurrentCulture, "{0} is not allowed for declaration type {1}", validationObject.Parent.JE_EntryStyle, validationObject.Parent.JE_DeclarationType));
			}
		}

		DeclarationTypeEntryStyleCombo[] DeclarationTypeEntryStyleCombosNotAllowed
		{
			get
			{
				return new DeclarationTypeEntryStyleCombo[]
				{
					new DeclarationTypeEntryStyleCombo { DeclarationType = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse, EntryStyle = EntryStyleListImport.Codes.ImportFromSpecialTerritory },
					new DeclarationTypeEntryStyleCombo { DeclarationType = ImportDeclarationTypeList.Codes.DeclarationForGoodsFromTheSpecialFiscalTerritories, EntryStyle = EntryStyleListImport.Codes.ImportNormal }
				};
			}
		}
	}

	class DeclarationTypeEntryStyleCombo
	{
		public ZString DeclarationType { get; set; }
		public ZString EntryStyle { get; set; }
	}
}
