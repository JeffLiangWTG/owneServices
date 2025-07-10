using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	public static class TestExtensions
	{
		public static void SetConsolidatedDocument(this EMCSJobDeclaration declaration) => declaration.ZG_DeferredSubmission = EmcsDeferredSubmissionList.Codes.JaZusammengefasstesEVd;

		public static EMCSJobDeclaration CreateDeclarationWithEadReference(this BusinessObjectFactory factory, ZString eadNumber, ZString sequenceNumber, ZString declarantType)
		{
			var declaration = factory.New<EMCSJobDeclaration>();
			declaration.JE_DeclarantType = declarantType;

			var entryNum = CusEntryNumber.New(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			entryNum.CE_EntryNum = eadNumber;
			entryNum.CE_EntryLineReference = sequenceNumber;

			return declaration;
		}
	}
}
