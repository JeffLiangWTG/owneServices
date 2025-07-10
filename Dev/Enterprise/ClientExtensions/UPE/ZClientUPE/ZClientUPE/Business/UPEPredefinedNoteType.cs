using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Business
{
	public class UPEPredefinedNoteType : PredefinedNoteType
	{
		public UPEPredefinedNoteType(ZString description, StmNoteVisibility defaultVisibility, bool isOnlyOneAllowed, bool isReadOnlyAfterAdd, bool isTextOnly)
			: base((NoResString)description, defaultVisibility, isOnlyOneAllowed, isReadOnlyAfterAdd, isTextOnly, false)
		{
		}

		public UPEPredefinedNoteType(ZString description, StmNoteVisibility defaultVisibility, bool isOnlyOneAllowed, bool isReadOnlyAfterAdd, bool isTextOnly, int textOnlyMaxLength)
			: base((NoResString)description, defaultVisibility, isOnlyOneAllowed, isReadOnlyAfterAdd, isTextOnly, textOnlyMaxLength, false)
		{
		}
	}
}
